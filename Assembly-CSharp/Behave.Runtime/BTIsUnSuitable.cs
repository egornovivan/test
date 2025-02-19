using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTIsUnSuitable), "IsUnSuitable")]
public class BTIsUnSuitable : BTNormal
{
	private class Data
	{
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.selectattackEnemy.entityTarget.Field == MovementField.Sky)
		{
			if (base.entity.NpcCmpt.EqSelect.BetterAtkObj == null)
			{
				return BehaveResult.Success;
			}
			AttackMode attackMode = base.entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
			if (attackMode.type != AttackType.Ranged)
			{
				return BehaveResult.Success;
			}
		}
		return BehaveResult.Failure;
	}
}
