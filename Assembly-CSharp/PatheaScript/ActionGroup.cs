using System.Collections.Generic;
using System.IO;

namespace PatheaScript;

public abstract class ActionGroup : Action
{
	protected List<Action> mList;

	public void Add(Action a)
	{
		mList.Add(a);
	}

	public override bool Parse()
	{
		int count = mInfo.ChildNodes.Count;
		mList = new List<Action>(count);
		return true;
	}

	protected override bool OnInit()
	{
		if (!base.OnInit())
		{
			return false;
		}
		if (mList.Count <= 0)
		{
			return false;
		}
		foreach (Action m in mList)
		{
			if (!m.Init())
			{
				return false;
			}
		}
		return true;
	}

	public override void Store(BinaryWriter w)
	{
		base.Store(w);
		foreach (Action m in mList)
		{
			m.Store(w);
		}
	}

	public override void Restore(BinaryReader r)
	{
		base.Restore(r);
		foreach (Action m in mList)
		{
			m.Restore(r);
		}
	}
}
