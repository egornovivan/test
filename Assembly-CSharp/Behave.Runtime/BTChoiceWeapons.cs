using System;
using ItemAsset;
using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTChoiceWeapons), "ChoiceWeapons")]
public class BTChoiceWeapons : BTNormal
{
	private class Data
	{
		[Behave]
		public float maxEnemyDis;

		[Behave]
		public float minFollowerHpPer;

		[Behave]
		public float compareHpPer;
	}

	private Data m_Data;

	private void EquipSheild()
	{
		if (base.entity.NpcCmpt.EqSelect.BetterAtkObj != null)
		{
			AttackMode attackMode = base.entity.NpcCmpt.EqSelect.BetterAtkObj.protoData.weaponInfo.attackModes[0];
			if (attackMode.type == AttackType.Ranged)
			{
				return;
			}
		}
		ItemObject itemObject = null;
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsDef(base.entity, EeqSelect.protect))
		{
			itemObject = base.entity.NpcCmpt.EqSelect.GetBetterDefObj();
		}
		if (itemObject == null)
		{
			return;
		}
		Equip cmpt = itemObject.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < base.entity.equipmentCmpt._ItemList.Count; i++)
		{
			ItemObject itemObject2 = base.entity.equipmentCmpt._ItemList[i];
			Equip cmpt2 = itemObject2.GetCmpt<Equip>();
			if (cmpt2 != null && Convert.ToBoolean(cmpt.equipPos & cmpt2.equipPos))
			{
				flag = true;
				break;
			}
		}
		if (!flag || !(base.entity.motionEquipment.sheild == null))
		{
			if (!flag && itemObject != null)
			{
				SelectItem.EquipByObj(base.entity, itemObject);
			}
			else if (flag && itemObject != null && base.entity.motionEquipment.sheild.m_ItemObj != itemObject)
			{
				SelectItem.EquipByObj(base.entity, itemObject);
			}
		}
	}

	private void EquipEnergy()
	{
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsEnergy(base.entity, EeqSelect.energy))
		{
			SelectItem.EquipByObj(base.entity, base.entity.NpcCmpt.EqSelect.GetBetterEnergyObj());
		}
	}

	private void EquipEnergySheild()
	{
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsEnergySheild(base.entity, EeqSelect.energy_sheild))
		{
			SelectItem.EquipByObj(base.entity, base.entity.NpcCmpt.EqSelect.GetBetterEnergySheild());
		}
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.selectattackEnemy.entityTarget == null)
		{
			return BehaveResult.Failure;
		}
		base.entity.NpcCmpt.EqSelect.ClearSelect();
		base.entity.NpcCmpt.EqSelect.ClearAtkSelects();
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsAtk(base.entity, EeqSelect.combat))
		{
			base.entity.NpcCmpt.EqSelect.GetBetterAtkObj(base.entity, base.selectattackEnemy);
		}
		EquipEnergy();
		EquipSheild();
		EquipEnergySheild();
		return BehaveResult.Success;
	}
}
