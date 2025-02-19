using ItemAsset;
using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTEquip), "Equip")]
public class BTEquip : BTNormal
{
	private class Data
	{
		[Behave]
		public int EqCombat;

		[Behave]
		public int EqRange;

		[Behave]
		public int EqMelee;

		[Behave]
		public int EqSheild;

		[Behave]
		public int EqTool;

		[Behave]
		public int EqEnergySheild;

		[Behave]
		public int EqEnergy;
	}

	private Data m_Data;

	private void TryEquipSheild()
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
		if (base.entity.motionEquipment.sheild == null && itemObject != null)
		{
			SelectItem.EquipByObj(base.entity, itemObject);
		}
		else if (base.entity.motionEquipment.sheild != null && itemObject != null && base.entity.motionEquipment.sheild.m_ItemObj != itemObject)
		{
			SelectItem.EquipByObj(base.entity, itemObject);
		}
	}

	private void TryEquipEnergy()
	{
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsEnergy(base.entity, EeqSelect.energy))
		{
			SelectItem.EquipByObj(base.entity, base.entity.NpcCmpt.EqSelect.GetBetterEnergyObj());
		}
	}

	private void TryEquipEnergySheild()
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
		if (m_Data.EqCombat > 0)
		{
			base.entity.NpcCmpt.EqSelect.ClearSelect();
			base.entity.NpcCmpt.EqSelect.ClearAtkSelects();
			if (m_Data.EqRange > 0)
			{
				base.entity.NpcCmpt.EqSelect.SetSelectObjsAtk(base.entity, AttackType.Ranged);
			}
			if (m_Data.EqMelee > 0)
			{
				base.entity.NpcCmpt.EqSelect.SetSelectObjsAtk(base.entity, AttackType.Melee);
			}
			base.entity.NpcCmpt.EqSelect.GetBetterObj(base.entity, base.selectattackEnemy, EeqSelect.combat);
			ItemObject betterAtkObj = base.entity.NpcCmpt.EqSelect.BetterAtkObj;
			if (betterAtkObj != null && base.entity.motionEquipment.ActiveableEquipment == null)
			{
				SelectItem.EquipByObj(base.entity, betterAtkObj);
			}
			else if (betterAtkObj != null && base.entity.motionEquipment.ActiveableEquipment != null && base.entity.motionEquipment.ActiveableEquipment.m_ItemObj != betterAtkObj)
			{
				SelectItem.TakeOffEquip(base.entity);
				SelectItem.EquipByObj(base.entity, betterAtkObj);
			}
		}
		if (m_Data.EqSheild > 0)
		{
			TryEquipSheild();
		}
		if (m_Data.EqEnergySheild > 0)
		{
			TryEquipEnergySheild();
		}
		if (m_Data.EqEnergy > 0)
		{
			TryEquipEnergy();
		}
		return BehaveResult.Success;
	}
}
