namespace ScenarioRTL;

public abstract class EventListener : StatementObject
{
	public delegate void PostNotify(EventListener evtl);

	public event PostNotify OnPost;

	public abstract void Listen();

	public abstract void Close();

	protected void Post()
	{
		if (this.OnPost != null)
		{
			this.OnPost(this);
		}
		if (base.trigger != null && base.trigger.enabled && base.trigger.isAlive)
		{
			base.trigger.InitConditions();
			base.trigger.StartProcessCondition();
		}
	}
}
