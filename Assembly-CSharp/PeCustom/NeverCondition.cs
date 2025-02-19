using ScenarioRTL;

namespace PeCustom;

[Statement("NEVER")]
public class NeverCondition : Condition
{
	protected override void OnCreate()
	{
	}

	public override bool? Check()
	{
		return false;
	}
}
