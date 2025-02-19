using PeEvent;

namespace Pathea;

public class EntityEventCmpt : PeCmpt, IPeMsg
{
	public class EventArg : PeEvent.EventArg
	{
		public EMsg msg;

		public object[] args;

		public override string ToString()
		{
			return "Msg:" + msg;
		}
	}

	private Event<EventArg> mEventor = new Event<EventArg>();

	public Event<EventArg> eventor => mEventor;

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		EventArg eventArg = new EventArg();
		eventArg.msg = msg;
		eventArg.args = args;
		eventor.Dispatch(eventArg, base.Entity);
	}
}
