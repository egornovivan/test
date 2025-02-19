using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("SLEEPING")]
public class SleepingCondition : Condition
{
	private OBJECT player;

	private bool state;

	protected override void OnCreate()
	{
		player = Utility.ToObject(base.parameters["player"]);
		state = Utility.ToBool(base.missionVars, base.parameters["state"]);
	}

	public override bool? Check()
	{
		if (player.type == OBJECT.OBJECTTYPE.Player)
		{
			PeEntity entity = PeScenarioUtility.GetEntity(player);
			if (entity != null && entity.motionMgr != null && entity.motionMgr.IsActionRunning(PEActionType.Sleep) == state)
			{
				return true;
			}
		}
		return false;
	}
}
