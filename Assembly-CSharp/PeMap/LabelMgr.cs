using PeEvent;

namespace PeMap;

public class LabelMgr : MyBaseListSingleton<LabelMgr, ILabel>
{
	public class Args : EventArg
	{
		public bool add;

		public ILabel label;
	}

	private Event<Args> mEventor = new Event<Args>();

	public Event<Args> eventor => mEventor;

	public override bool Add(ILabel item)
	{
		if (base.Add(item))
		{
			eventor.Dispatch(new Args
			{
				add = true,
				label = item
			});
			return true;
		}
		return false;
	}

	public override bool Remove(ILabel item)
	{
		if (base.Remove(item))
		{
			eventor.Dispatch(new Args
			{
				add = false,
				label = item
			});
			return true;
		}
		return false;
	}
}
