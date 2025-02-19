using System.Collections.Generic;
using ItemAsset;
using Pathea.PeEntityExtNpcPackage;

namespace Pathea;

public class StorageSupply_Ammo : StrorageSupply
{
	private AmmoExpend[] m_atkExpends;

	public override ESupplyType Type => ESupplyType.Ammo;

	public StorageSupply_Ammo()
	{
		m_atkExpends = new AmmoExpend[3];
		m_atkExpends[0] = new AmmoExpend_Bullet();
		m_atkExpends[1] = new AmmoExpend_Arrow();
		m_atkExpends[2] = new AmmoExpend_Battery();
	}

	public override bool DoSupply(PeEntity entity, CSAssembly csAssembly)
	{
		if (csAssembly == null || csAssembly.Storages == null)
		{
			return false;
		}
		if (entity.packageCmpt == null)
		{
			return false;
		}
		List<ItemObject> atkEquipObjs = entity.GetAtkEquipObjs(AttackType.Ranged);
		if (atkEquipObjs == null || atkEquipObjs.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < atkEquipObjs.Count; i++)
		{
			MatchExpend(atkEquipObjs[i])?.SupplySth(entity, csAssembly);
		}
		return true;
	}

	public AmmoExpend MatchExpend(ItemObject obj)
	{
		if (obj.protoData.weaponInfo == null)
		{
			return null;
		}
		int costItem = obj.protoData.weaponInfo.costItem;
		if (costItem != 0)
		{
			for (int i = 0; i < m_atkExpends.Length; i++)
			{
				if (m_atkExpends[i] != null && m_atkExpends[i].MatchExpend(costItem))
				{
					return m_atkExpends[i];
				}
			}
		}
		if (obj.protoData.weaponInfo.useEnergry)
		{
			for (int j = 0; j < m_atkExpends.Length; j++)
			{
				if (m_atkExpends[j] != null && m_atkExpends[j].MatchExpend(228))
				{
					return m_atkExpends[j];
				}
			}
		}
		return null;
	}
}
