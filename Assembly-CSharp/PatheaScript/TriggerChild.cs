namespace PatheaScript;

public abstract class TriggerChild : ParseObj
{
	protected Trigger mTrigger;

	public void SetTrigger(Trigger trigger)
	{
		mTrigger = trigger;
	}
}
