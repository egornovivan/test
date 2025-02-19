using System;
using System.Collections.Generic;

public class PeEventMgr
{
	public class EventArg
	{
		public object mSender;
	}

	public enum EEventType
	{
		Conversation,
		MouseClicked,
		NpcDead,
		StatsChanged,
		Max
	}

	public class MouseEvent : EventArg
	{
		public enum Type
		{
			LeftClicked,
			RightClicked,
			MiddleClicked,
			Max
		}

		public Type mType;

		public MouseEvent(Type type)
		{
			mType = type;
		}
	}

	private class Event
	{
		private event EventHandler mEventDispatch;

		public void SubscribeEvent(EventHandler handler)
		{
			this.mEventDispatch = (EventHandler)Delegate.Combine(this.mEventDispatch, handler);
		}

		public void UnsubscribeEvent(EventHandler handler)
		{
			this.mEventDispatch = (EventHandler)Delegate.Remove(this.mEventDispatch, handler);
		}

		public void PublishEvent(EventArg arg)
		{
			if (this.mEventDispatch != null)
			{
				this.mEventDispatch(arg);
			}
		}
	}

	public delegate void EventHandler(EventArg arg);

	private static PeEventMgr instance;

	private Dictionary<EEventType, Event> mDicEvent;

	public static PeEventMgr Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PeEventMgr();
			}
			return instance;
		}
	}

	public void EmitEvent(EEventType eventType, EventArg arg)
	{
		if (mDicEvent != null && mDicEvent.ContainsKey(eventType))
		{
			mDicEvent[eventType].PublishEvent(arg);
		}
	}

	public void SubscribeEvent(EEventType eventType, EventHandler eventHandler)
	{
		if (mDicEvent == null)
		{
			mDicEvent = new Dictionary<EEventType, Event>(20);
		}
		if (!mDicEvent.ContainsKey(eventType))
		{
			mDicEvent[eventType] = new Event();
		}
		mDicEvent[eventType].SubscribeEvent(eventHandler);
	}

	public void UnsubscribeEvent(EEventType eventType, EventHandler eventHandler)
	{
		if (mDicEvent != null && mDicEvent.ContainsKey(eventType))
		{
			mDicEvent[eventType].UnsubscribeEvent(eventHandler);
		}
	}
}
