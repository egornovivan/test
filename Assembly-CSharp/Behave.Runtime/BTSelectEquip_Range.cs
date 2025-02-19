namespace Behave.Runtime;

[BehaveAction(typeof(BTSelectEquip_Range), "IsSelectEquipRange")]
public class BTSelectEquip_Range : BTNormal
{
	private class Data
	{
	}

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.NpcCmpt.EqSelect.BetterAtkObj == null)
		{
			return BehaveResult.Failure;
		}
		AttackMode attackMode = base.entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
		if (attackMode.type == AttackType.Ranged)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
