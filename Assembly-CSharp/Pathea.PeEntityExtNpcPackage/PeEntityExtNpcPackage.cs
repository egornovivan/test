using System.Collections.Generic;
using ItemAsset;

namespace Pathea.PeEntityExtNpcPackage;

public static class PeEntityExtNpcPackage
{
	public static int GetBagItemCount(this PeEntity entity)
	{
		return 0;
	}

	public static int GetItemCount(this PeEntity entity, int protoId)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return 0;
		}
		return npcPackageCmpt.GetItemCount(protoId);
	}

	public static ItemObject GetBagItemObj(this PeEntity entity, int prototypeId)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return null;
		}
		SlotList slotList = npcPackageCmpt.GetSlotList();
		if (slotList.FindItemByProtoId(prototypeId) != null)
		{
			return slotList.FindItemByProtoId(prototypeId);
		}
		slotList = npcPackageCmpt.GetHandinList();
		return slotList.FindItemByProtoId(prototypeId);
	}

	public static ItemObject GetBagItem(this PeEntity entity, int index)
	{
		return null;
	}

	public static bool AddToBag(this PeEntity entity, ItemObject item, bool isnew = false)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return false;
		}
		return npcPackageCmpt.Add(item, isnew);
	}

	public static bool RemoveFromBag(this PeEntity entity, ItemObject item)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return false;
		}
		return npcPackageCmpt.Remove(item);
	}

	public static List<ItemObject> GetAtkEquipObjs(this PeEntity entity, AttackType atkType)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return null;
		}
		return npcPackageCmpt.GetAtkEquipItemObjs(atkType);
	}

	public static List<ItemObject> GetEquipObjs(this PeEntity entity, EeqSelect selcet)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return null;
		}
		return npcPackageCmpt.GetEquipItemObjs(selcet);
	}

	public static bool IsInPackage(this PeEntity entity, ItemObject obj)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return false;
		}
		return npcPackageCmpt.Contain(obj);
	}

	public static bool HasEquip(this PeEntity entity, EeqSelect sle)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return false;
		}
		return npcPackageCmpt.HasEq(sle);
	}

	public static bool HasCanAttackEquip(this PeEntity entity, EeqSelect sle)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return false;
		}
		List<ItemObject> equipItemObjs = npcPackageCmpt.GetEquipItemObjs(sle);
		if (equipItemObjs == null)
		{
			return false;
		}
		for (int i = 0; i < equipItemObjs.Count; i++)
		{
			if (SelectItem.EquipCanAttack(entity, equipItemObjs[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasAtkEquips(this PeEntity entity, AttackType type)
	{
		NpcPackageCmpt npcPackageCmpt = entity.packageCmpt as NpcPackageCmpt;
		if (null == npcPackageCmpt)
		{
			return false;
		}
		return npcPackageCmpt.HasAtkEquip(type);
	}
}
