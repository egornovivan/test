using System.Collections.Generic;

namespace PatheaScript;

public abstract class EventProxy
{
	private List<Event> mList;

	public string Name { get; set; }

	public int Count
	{
		get
		{
			if (mList == null)
			{
				return 0;
			}
			return mList.Count;
		}
	}

	public void Add(Event e)
	{
		if (mList == null)
		{
			mList = new List<Event>(5);
		}
		mList.Add(e);
	}

	public bool Remove(Event e)
	{
		return mList.Remove(e);
	}

	public virtual bool Subscribe()
	{
		return true;
	}

	public virtual void Tick()
	{
	}

	public virtual void Unsubscribe()
	{
	}

	protected void Emit(params object[] param)
	{
		if (mList == null || mList.Count == 0)
		{
			return;
		}
		List<Event> list = mList.FindAll((Event e) => e.Filter(param));
		list.Sort(delegate(Event x, Event y)
		{
			if (x.Priority > y.Priority)
			{
				return 1;
			}
			return (x.Priority != y.Priority) ? (-1) : 0;
		});
		list.ForEach(delegate(Event e)
		{
			e.Emit();
		});
	}
}
