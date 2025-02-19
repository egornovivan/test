using System.IO;
using System.Xml;

namespace PatheaScript;

public class Trigger : ParseObj, Event.IMsgHandler, Storeable
{
	private class RepeatCount
	{
		private int mCount;

		public bool IsZero => mCount == 0;

		public int Value => mCount;

		public RepeatCount(int count)
		{
			mCount = count;
		}

		public void Manus()
		{
			if (mCount > 0)
			{
				mCount--;
			}
		}

		public override string ToString()
		{
			return mCount.ToString();
		}
	}

	private enum EStep
	{
		EventListenning,
		Comparing,
		ActionRunning,
		Finished,
		Max
	}

	private EStep mStep;

	private EventGroup mEvent;

	private ConditionOr mCondition;

	private ActionConcurrent mAction;

	private PsScript mScript;

	protected VariableMgr mVarMgr;

	public string Name { get; private set; }

	private RepeatCount Repeat { get; set; }

	public PsScript Parent => mScript;

	public Trigger(PsScript script)
	{
		mScript = script;
		mStep = EStep.Max;
		mVarMgr = new VariableMgr();
	}

	public void RequireStop()
	{
		mEvent.SetMsgHndler(null);
		if (mStep == EStep.ActionRunning)
		{
			Repeat = new RepeatCount(1);
		}
		else
		{
			Repeat = new RepeatCount(0);
		}
	}

	public Variable GetVar(string varName, bool bFindInParent = true)
	{
		Variable var = mVarMgr.GetVar(varName);
		if (var != null)
		{
			return var;
		}
		if (!bFindInParent)
		{
			return var;
		}
		return Parent.GetVar(varName);
	}

	public Variable AddVar(string varName, Variable.EScope eScope)
	{
		Variable variable = null;
		switch (eScope)
		{
		case Variable.EScope.Gloabel:
			variable = Parent.Parent.GetVar(varName);
			if (variable == null)
			{
				variable = new Variable();
				Parent.Parent.AddVar(varName, variable);
			}
			break;
		case Variable.EScope.Script:
			variable = Parent.GetVar(varName, bFindInParent: false);
			if (variable == null)
			{
				variable = new Variable();
				Parent.AddVar(varName, variable);
			}
			break;
		case Variable.EScope.Trigger:
			variable = GetVar(varName, bFindInParent: false);
			if (variable == null)
			{
				variable = new Variable();
				mVarMgr.AddVar(varName, variable);
			}
			break;
		}
		return variable;
	}

	public override bool Parse()
	{
		Name = Util.GetString(mInfo, "name");
		Repeat = new RepeatCount(Util.GetInt(mInfo, "repeat"));
		foreach (XmlNode childNode in mInfo.ChildNodes)
		{
			if (childNode.Name == "EVENTS")
			{
				mEvent = new EventGroup();
				mEvent.SetInfo(mFactory, childNode);
				mEvent.SetTrigger(this);
				if (!mEvent.Parse())
				{
					return false;
				}
			}
			else if (childNode.Name == "CONDITIONS")
			{
				mCondition = new ConditionOr();
				mCondition.SetInfo(mFactory, childNode);
				mCondition.SetTrigger(this);
				if (!mCondition.Parse())
				{
					return false;
				}
			}
			else if (childNode.Name == "ACTIONS")
			{
				mAction = new ActionConcurrent();
				mAction.SetInfo(mFactory, childNode);
				mAction.SetTrigger(this);
				if (!mAction.Parse())
				{
					return false;
				}
			}
		}
		return true;
	}

	public void OnEventTriggered()
	{
		if (mStep == EStep.EventListenning)
		{
			mStep = EStep.Comparing;
			Tick();
		}
	}

	public TickResult Tick()
	{
		if (mStep == EStep.Finished)
		{
			return TickResult.Finished;
		}
		if (mStep == EStep.Comparing)
		{
			if (mCondition.Do())
			{
				mAction.Init();
				mStep = EStep.ActionRunning;
			}
			else
			{
				mStep = EStep.EventListenning;
			}
		}
		if (mStep == EStep.ActionRunning && mAction.Tick() == TickResult.Finished)
		{
			Repeat.Manus();
			if (!Repeat.IsZero)
			{
				mStep = EStep.EventListenning;
			}
			else
			{
				mStep = EStep.Finished;
			}
		}
		if (mStep == EStep.Finished || Repeat.IsZero)
		{
			return TickResult.Finished;
		}
		return TickResult.Running;
	}

	public bool Init()
	{
		if (!mEvent.Init())
		{
			return false;
		}
		mEvent.SetMsgHndler(this);
		mStep = EStep.EventListenning;
		return true;
	}

	public void Reset()
	{
		mEvent.SetMsgHndler(null);
		mEvent.Reset();
	}

	public override string ToString()
	{
		return $"Trigger:Name={Name},Repeat={Repeat}";
	}

	public void Store(BinaryWriter w)
	{
		w.Write(Repeat.Value);
		w.Write((sbyte)mStep);
		byte[] array = VariableMgr.Export(mVarMgr);
		w.Write(array.Length);
		w.Write(array);
		mAction.Store(w);
	}

	public void Restore(BinaryReader r)
	{
		Repeat = new RepeatCount(r.ReadInt32());
		mStep = (EStep)r.ReadSByte();
		int count = r.ReadInt32();
		byte[] data = r.ReadBytes(count);
		mVarMgr = VariableMgr.Import(data);
		mAction.Restore(r);
	}
}
