using System;

namespace PeEvent;

public class Event<T> where T : EventArg
{
	public delegate void Handler(object sender, T arg);

	private object mSender;

	private event Handler mHandler;

	public Event(object sender)
	{
		mSender = sender;
	}

	public Event()
	{
		mSender = null;
	}

	public void Subscribe(Handler handler)
	{
		this.mHandler = (Handler)Delegate.Combine(this.mHandler, handler);
	}

	public void Unsubscribe(Handler handler)
	{
		this.mHandler = (Handler)Delegate.Remove(this.mHandler, handler);
	}

	public void Dispatch(T arg)
	{
		Dispatch(arg, null);
	}

	public void Dispatch(T arg, object sender)
	{
		if (this.mHandler != null)
		{
			if (sender != null)
			{
				this.mHandler(sender, arg);
			}
			else
			{
				this.mHandler(mSender, arg);
			}
		}
	}
}
