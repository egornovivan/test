namespace PatheaScript;

public abstract class Event : TriggerChild
{
	public interface IMsgHandler
	{
		void OnEventTriggered();
	}

	private IMsgHandler mMsgHandler;

	public string Name { get; set; }

	public int Priority { get; private set; }

	public abstract bool Filter(params object[] param);

	public void Emit()
	{
		if (mMsgHandler != null)
		{
			mMsgHandler.OnEventTriggered();
		}
	}

	public void SetMsgHndler(IMsgHandler msgHandler)
	{
		mMsgHandler = msgHandler;
	}

	public override bool Parse()
	{
		Priority = Util.GetEventPriority(mInfo);
		return true;
	}

	public virtual bool Init()
	{
		mTrigger.Parent.Parent.EventProxyMgr.SubEvent(this);
		return true;
	}

	public virtual void Reset()
	{
		mTrigger.Parent.Parent.EventProxyMgr.UnsubEvent(this);
		SetMsgHndler(null);
	}
}
