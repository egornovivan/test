using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PatheaScript;

public class TriggerGroup : ParseObj, Storeable
{
	private List<Trigger> mList;

	private PsScript mScript;

	public TriggerGroup(PsScript script)
	{
		mScript = script;
	}

	public void RequireStop()
	{
		foreach (Trigger m in mList)
		{
			m.RequireStop();
		}
	}

	public override bool Parse()
	{
		mList = new List<Trigger>(mInfo.ChildNodes.Count);
		foreach (XmlNode childNode in mInfo.ChildNodes)
		{
			Trigger trigger = new Trigger(mScript);
			trigger.SetInfo(mFactory, childNode);
			if (trigger.Parse())
			{
				mList.Add(trigger);
			}
		}
		return true;
	}

	public bool Init()
	{
		if (mList == null)
		{
			return false;
		}
		foreach (Trigger m in mList)
		{
			if (!m.Init())
			{
				return false;
			}
		}
		return true;
	}

	public TickResult Tick()
	{
		if (mList == null)
		{
			return TickResult.Finished;
		}
		TickResult result = TickResult.Finished;
		mList.ForEach(delegate(Trigger t)
		{
			if (t.Tick() == TickResult.Running)
			{
				result = TickResult.Running;
			}
		});
		return result;
	}

	public void Reset()
	{
		if (mList != null && mList.Count != 0)
		{
			mList.ForEach(delegate(Trigger t)
			{
				t.Reset();
			});
		}
	}

	public void Store(BinaryWriter w)
	{
		foreach (Trigger m in mList)
		{
			m.Store(w);
		}
	}

	public void Restore(BinaryReader r)
	{
		foreach (Trigger m in mList)
		{
			m.Restore(r);
		}
	}
}
