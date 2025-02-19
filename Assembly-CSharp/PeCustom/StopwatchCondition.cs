using ScenarioRTL;

namespace PeCustom;

[Statement("STOPWATCH")]
public class StopwatchCondition : Condition
{
	private int id;

	private ECompare comp;

	private double amt;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		amt = Utility.ToDouble(base.missionVars, base.parameters["amount"]);
	}

	public override bool? Check()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.stopwatchMgr != null)
		{
			return PeCustomScene.Self.scenario.stopwatchMgr.CompareStopwatch(id, comp, amt);
		}
		return Utility.Compare(0.0, amt, comp);
	}
}
