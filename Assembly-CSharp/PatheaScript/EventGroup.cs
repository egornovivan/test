using System.Collections.Generic;
using System.Xml;

namespace PatheaScript;

public class EventGroup : TriggerChild
{
	private List<Event> mList;

	private void Add(Event e)
	{
		mList.Add(e);
	}

	public override bool Parse()
	{
		mList = new List<Event>(mInfo.ChildNodes.Count);
		foreach (XmlNode childNode in mInfo.ChildNodes)
		{
			string stmtName = Util.GetStmtName(childNode);
			Event @event = mFactory.CreateEvent(stmtName);
			if (@event != null)
			{
				@event.SetInfo(mFactory, childNode);
				@event.SetTrigger(mTrigger);
				@event.Name = stmtName;
				if (@event.Parse())
				{
					Add(@event);
				}
			}
		}
		return true;
	}

	public void SetMsgHndler(Event.IMsgHandler msgHandler)
	{
		foreach (Event m in mList)
		{
			m.SetMsgHndler(msgHandler);
		}
	}

	public bool Init()
	{
		foreach (Event m in mList)
		{
			if (!m.Init())
			{
				return false;
			}
		}
		return true;
	}

	public void Reset()
	{
		mList.ForEach(delegate(Event e)
		{
			e.Reset();
		});
	}
}
