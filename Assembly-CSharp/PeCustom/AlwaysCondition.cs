using ScenarioRTL;

namespace PeCustom;

[Statement("ALWAYS")]
public class AlwaysCondition : Condition
{
	protected override void OnCreate()
	{
	}

	public override bool? Check()
	{
		return true;
	}
}
