using System.Xml;

namespace PatheaScript;

public class ActionConcurrent : ActionGroup
{
	public override bool Parse()
	{
		if (!base.Parse())
		{
			return false;
		}
		if (mInfo.Name != "ACTIONS")
		{
			return false;
		}
		foreach (XmlNode childNode in mInfo.ChildNodes)
		{
			Action action = new ActionQueue();
			action.SetInfo(mFactory, childNode);
			action.SetTrigger(mTrigger);
			if (action.Parse())
			{
				Add(action);
			}
		}
		return true;
	}

	protected override TickResult OnTick()
	{
		if (base.OnTick() == TickResult.Finished)
		{
			return TickResult.Finished;
		}
		TickResult result = TickResult.Finished;
		foreach (Action m in mList)
		{
			if (m.Tick() == TickResult.Running)
			{
				result = TickResult.Running;
			}
		}
		return result;
	}
}
