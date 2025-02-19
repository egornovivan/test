namespace PatheaScript;

public abstract class Condition : TriggerChild
{
	public virtual bool Do()
	{
		return true;
	}
}
