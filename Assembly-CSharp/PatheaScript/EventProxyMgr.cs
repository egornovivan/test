using System.Collections.Generic;

namespace PatheaScript;

public class EventProxyMgr
{
	private PsScriptMgr mScriptMgr;

	private List<EventProxy> mList;

	public EventProxyMgr(PsScriptMgr scriptMgr)
	{
		mScriptMgr = scriptMgr;
	}

	public void Tick()
	{
		if (mList != null)
		{
			mList.ForEach(delegate(EventProxy e)
			{
				e.Tick();
			});
		}
	}

	private EventProxy GetProxy(string name)
	{
		return mList.Find((EventProxy ep) => string.Equals(ep.Name, name) ? true : false);
	}

	public bool SubEvent(Event e)
	{
		if (e == null)
		{
			return false;
		}
		if (mList == null)
		{
			mList = new List<EventProxy>(5);
		}
		EventProxy eventProxy = GetProxy(e.Name);
		if (eventProxy == null)
		{
			eventProxy = mScriptMgr.Factory.CreateEventProxy(e.Name);
			if (eventProxy == null)
			{
				return false;
			}
			eventProxy.Name = e.Name;
			if (!eventProxy.Subscribe())
			{
				return false;
			}
			mList.Add(eventProxy);
		}
		eventProxy.Add(e);
		return true;
	}

	public bool UnsubEvent(Event e)
	{
		if (e == null)
		{
			return false;
		}
		if (mList == null)
		{
			return false;
		}
		EventProxy proxy = GetProxy(e.Name);
		if (proxy == null)
		{
			return false;
		}
		if (!proxy.Remove(e))
		{
			return false;
		}
		if (proxy.Count <= 0)
		{
			proxy.Unsubscribe();
			mList.Remove(proxy);
		}
		return true;
	}
}
