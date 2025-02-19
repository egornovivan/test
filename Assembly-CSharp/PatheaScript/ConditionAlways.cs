namespace PatheaScript;

public class ConditionAlways : Condition
{
	public override bool Parse()
	{
		return true;
	}

	public override bool Do()
	{
		return true;
	}

	public override string ToString()
	{
		return $"CAlways";
	}
}
