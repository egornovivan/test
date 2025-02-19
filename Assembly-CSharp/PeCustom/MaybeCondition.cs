using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("MAYBE")]
public class MaybeCondition : Condition
{
	private float p;

	protected override void OnCreate()
	{
		p = Utility.ToSingle(base.missionVars, base.parameters["p"]);
	}

	public override bool? Check()
	{
		return Random.value * 100f < p;
	}
}
