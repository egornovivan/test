using System.IO;
using System.Xml;

namespace PatheaScript;

public class ActionQueue : ActionGroup
{
	private int mId;

	private int mCurRunningActionIndex;

	public int id => mId;

	public ActionQueue()
	{
		mCurRunningActionIndex = -1;
	}

	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		if (mInfo.Name != "GROUP")
		{
			return false;
		}
		mId = Util.GetInt(mInfo, "index");
		foreach (XmlNode childNode in mInfo.ChildNodes)
		{
			Action action = mFactory.CreateAction(Util.GetStmtName(childNode));
			if (action != null)
			{
				action.SetInfo(mFactory, childNode);
				action.SetTrigger(mTrigger);
				if (action.Parse())
				{
					Add(action);
				}
			}
		}
		return true;
	}

	protected override bool OnInit()
	{
		if (!base.OnInit())
		{
			return false;
		}
		mCurRunningActionIndex = 0;
		return true;
	}

	protected override TickResult OnTick()
	{
		if (base.OnTick() == TickResult.Finished)
		{
			return TickResult.Finished;
		}
		while (mList.Count != mCurRunningActionIndex && mList[mCurRunningActionIndex].Tick() == TickResult.Finished)
		{
			mCurRunningActionIndex++;
		}
		if (mList.Count == mCurRunningActionIndex)
		{
			return TickResult.Finished;
		}
		return TickResult.Running;
	}

	protected override void OnReset()
	{
		base.OnReset();
		mCurRunningActionIndex = -1;
	}

	public override void Store(BinaryWriter w)
	{
		base.Store(w);
		w.Write(mCurRunningActionIndex);
	}

	public override void Restore(BinaryReader r)
	{
		base.Restore(r);
		mCurRunningActionIndex = r.ReadInt32();
	}
}
