using ItemAsset;
using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTConfirmAttackMode), "ConfirmAttackMode")]
public class BTConfirmAttackMode : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.entity == null || base.entity.target == null || Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.motionEquipment == null)
		{
			return BehaveResult.Failure;
		}
		if (!SelectItem.MatchEnemyAttack(base.entity, base.selectattackEnemy.entityTarget))
		{
			base.selectattackEnemy.entityTarget.target.RemoveMelee(base.entity);
			return BehaveResult.Success;
		}
		ItemObject itemObject = ((!(base.entity.motionEquipment.ActiveableEquipment != null)) ? null : base.entity.motionEquipment.ActiveableEquipment.m_ItemObj);
		if (itemObject == null)
		{
			int n = ((base.selectattackEnemy.entityTarget.monsterProtoDb == null || base.selectattackEnemy.entityTarget.monsterProtoDb.AtkDb == null) ? 3 : base.selectattackEnemy.entityTarget.monsterProtoDb.AtkDb.mNumber);
			base.selectattackEnemy.entityTarget.target.AddMelee(base.entity, n);
			return BehaveResult.Success;
		}
		if (itemObject.protoData != null && itemObject.protoData.weaponInfo == null)
		{
			SelectItem.TakeOffEquip(base.entity);
		}
		if (base.selectattackEnemy.entityTarget.target == null)
		{
			return BehaveResult.Success;
		}
		AttackMode[] array = ((itemObject.protoData == null || itemObject.protoData.weaponInfo == null) ? null : itemObject.protoData.weaponInfo.attackModes);
		if (array == null || array.Length == 0)
		{
			return BehaveResult.Failure;
		}
		int n2 = ((base.selectattackEnemy.entityTarget.monsterProtoDb == null || base.selectattackEnemy.entityTarget.monsterProtoDb.AtkDb == null) ? 3 : base.selectattackEnemy.entityTarget.monsterProtoDb.AtkDb.mNumber);
		if (array[0].type == AttackType.Melee)
		{
			base.selectattackEnemy.entityTarget.target.AddMelee(base.entity, n2);
		}
		else
		{
			base.selectattackEnemy.entityTarget.target.RemoveMelee(base.entity);
		}
		return BehaveResult.Success;
	}
}
