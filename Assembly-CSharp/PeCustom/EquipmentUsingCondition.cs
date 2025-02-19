using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("EQUIPMENT USING STATE")]
public class EquipmentUsingCondition : Condition
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
			if (entity != null && entity.motionEquipment != null)
			{
				return entity.motionEquipment.Weapon != null == state;
			}
		}
		return !state;
	}
}
