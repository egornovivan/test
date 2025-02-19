using System.Collections.Generic;
using Pathea;
using UnityEngine;
using WhiteCat;

namespace ItemAsset.SlotListHelper;

public static class SlotListAccessor
{
	public static bool ConatinsItem(this SlotList slotList, int prototypeId)
	{
		foreach (ItemObject item in (IEnumerable<ItemObject>)slotList)
		{
			if (item != null && item.protoId == prototypeId)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetCount(this SlotList slotList, int prototypeId)
	{
		int num = 0;
		if (slotList == null)
		{
			return num;
		}
		foreach (ItemObject item in (IEnumerable<ItemObject>)slotList)
		{
			if (item != null && item.protoId == prototypeId)
			{
				num += item.stackCount;
			}
		}
		return num;
	}

	public static int GetCountByEditorType(this SlotList slotList, int editorType)
	{
		int num = 0;
		foreach (ItemObject item in (IEnumerable<ItemObject>)slotList)
		{
			if (item != null)
			{
				ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(item.protoId);
				if (itemProto != null && itemProto.editorTypeId == editorType)
				{
					num += item.stackCount;
				}
			}
		}
		return num;
	}

	public static int GetAllItemsCount(this SlotList slotList)
	{
		int num = 0;
		foreach (ItemObject item in (IEnumerable<ItemObject>)slotList)
		{
			if (item != null)
			{
				num += item.stackCount;
			}
		}
		return num;
	}

	public static ItemObject GetItemByProtoID(this SlotList slotList, int protoId)
	{
		for (int i = 0; i < slotList.Count; i++)
		{
			if (slotList[i] != null && slotList[i].protoId == protoId)
			{
				return slotList[i];
			}
		}
		return null;
	}

	public static List<ItemObject> GetAllItemByProtoID(this SlotList slotList, int protoId)
	{
		List<ItemObject> list = new List<ItemObject>();
		for (int i = 0; i < slotList.Count; i++)
		{
			if (slotList[i] != null && slotList[i].protoId == protoId)
			{
				list.Add(slotList[i]);
			}
		}
		return list;
	}

	public static int GetCreationCount(this SlotList slotList, ECreation type)
	{
		int num = 0;
		foreach (ItemObject item in (IEnumerable<ItemObject>)slotList)
		{
			if (item != null && CreationHelper.GetCreationItemClass(item) == CreationHelper.ECreationToItemClass(type))
			{
				num += item.stackCount;
			}
		}
		return num;
	}

	public static List<int> GetCreationInstanceId(this SlotList slotList, ECreation type)
	{
		List<int> list = new List<int>();
		foreach (ItemObject item in (IEnumerable<ItemObject>)slotList)
		{
			if (item != null && CreationHelper.GetCreationItemClass(item) == CreationHelper.ECreationToItemClass(type))
			{
				list.Add(item.instanceId);
			}
		}
		return list;
	}

	public static int GetNeedSlotCount(int prototypeId, int count)
	{
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(prototypeId);
		if (itemProto == null)
		{
			return 0;
		}
		return (count - 1) / itemProto.maxStackNum + 1;
	}

	public static bool CanAdd(this SlotList slotList, int prototypeId, int count)
	{
		return slotList.GetVacancyCount() >= GetNeedSlotCount(prototypeId, count);
	}

	public static bool CanAdd(this SlotList slotList, ItemSample itemSample)
	{
		return slotList.CanAdd(itemSample.protoId, itemSample.stackCount);
	}

	public static bool Add(this SlotList slotList, int prototypeId, int count, bool isNew = false)
	{
		if (count <= 0)
		{
			return false;
		}
		int count2 = slotList.GetCount(prototypeId);
		if (count2 > 0)
		{
			slotList.Destroy(prototypeId, count2);
		}
		count += count2;
		return slotList.AddAsNew(prototypeId, count, isNew, count2);
	}

	public static bool Set(this SlotList slotList, int prototypeId, int count)
	{
		int count2 = slotList.GetCount(prototypeId);
		if (count == count2)
		{
			return true;
		}
		if (count > count2)
		{
			return slotList.Add(prototypeId, count - count2);
		}
		return slotList.Destroy(prototypeId, count2 - count);
	}

	public static bool AddAsNew(this SlotList slotList, int prototypeId, int count, bool isNew = false, int existCount = 0)
	{
		if (count <= 0)
		{
			return false;
		}
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(prototypeId);
		if (itemProto == null)
		{
			Debug.LogError("cant find item proto by id:" + prototypeId);
			return false;
		}
		int num = count / itemProto.maxStackNum;
		for (int i = 0; i < num; i++)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(prototypeId);
			if (itemObject != null)
			{
				itemObject.SetStackCount(itemProto.maxStackNum);
				slotList.Add(itemObject, isNew);
			}
		}
		int num2 = count % itemProto.maxStackNum;
		if (num2 > 0)
		{
			ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.CreateItem(prototypeId);
			itemObject2.SetStackCount(num2);
			slotList.Add(itemObject2, isNew);
		}
		return true;
	}

	public static bool Destroy(this SlotList slotList, int prototypeId, int count)
	{
		if (slotList.GetCount(prototypeId) < count)
		{
			return false;
		}
		for (int num = slotList.Length - 1; num >= 0; num--)
		{
			if (slotList[num] != null && slotList[num].protoId == prototypeId)
			{
				slotList.newFlagMgr.Remove(num);
				if (count < slotList[num].stackCount)
				{
					slotList[num].stackCount -= count;
					count = 0;
					break;
				}
				count -= slotList[num].stackCount;
				PeSingleton<ItemMgr>.Instance.DestroyItem(slotList[num].instanceId);
				slotList[num] = null;
			}
		}
		slotList.SendUpdateEvent();
		return true;
	}

	public static bool DestroyItem(this SlotList slotList, int instanceId, int count)
	{
		int num = slotList.FindItemIndexById(instanceId);
		if (num < 0)
		{
			return false;
		}
		slotList.newFlagMgr.Remove(num);
		if (slotList[num].stackCount == count)
		{
			PeSingleton<ItemMgr>.Instance.DestroyItem(instanceId);
			slotList[num] = null;
		}
		else
		{
			slotList[num].stackCount -= count;
		}
		slotList.SendUpdateEvent();
		return true;
	}

	public static void Reduce(this SlotList slotList)
	{
		List<ItemObject> list = new List<ItemObject>(slotList);
		slotList.Clear();
		foreach (ItemObject item in list)
		{
			if (item != null)
			{
				if (item.GetStackMax() > 1)
				{
					slotList.Add(item.protoId, item.stackCount);
					PeSingleton<ItemMgr>.Instance.DestroyItem(item.instanceId);
				}
				else
				{
					slotList.Add(item);
				}
			}
		}
	}
}
