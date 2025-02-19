using ScenarioRTL;

namespace PeCustom;

[Statement("COMPARE")]
public class CompareCondition : Condition
{
	private Var lhs;

	private Var rhs;

	private ECompare comp;

	protected override void OnCreate()
	{
		lhs = Utility.ToVar(base.missionVars, base.parameters["lhs"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		rhs = Utility.ToVar(base.missionVars, base.parameters["rhs"]);
	}

	public override bool? Check()
	{
		return Utility.CompareVar(lhs, rhs, comp);
	}
}
