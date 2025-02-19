using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("HATRED")]
public class HatredCondition : Condition
{
	private OBJECT obj1;

	private OBJECT obj2;

	private bool state;

	protected override void OnCreate()
	{
		obj1 = Utility.ToObject(base.parameters["object1"]);
		obj2 = Utility.ToObject(base.parameters["object2"]);
		state = Utility.ToBool(base.missionVars, base.parameters["state"]);
	}

	public override bool? Check()
	{
		PeEntity entity = PeScenarioUtility.GetEntity(obj1);
		PeEntity entity2 = PeScenarioUtility.GetEntity(obj2);
		if (entity == null || entity2 == null || entity2.target == null)
		{
			return false;
		}
		if (entity2.target.HasHatred(entity))
		{
			return state;
		}
		return !state;
	}
}
