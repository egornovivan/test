using System.Collections.Generic;
using ItemAsset.SlotListHelper;
using Pathea;

namespace ItemAsset.PackageHelper;

public static class PackageAccessor
{
	public static bool ContainsItem(this ItemPackage package, int prototypeId)
	{
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			if (slotList.ConatinsItem(prototypeId))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetCount(this ItemPackage package, int prototypeId)
	{
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(prototypeId);
		if (itemProto != null)
		{
			return package.GetSlotList((ItemPackage.ESlotType)itemProto.tabIndex)?.GetCount(prototypeId) ?? 0;
		}
		return 0;
	}

	public static int GetCountByEditorType(this ItemPackage package, int editorType)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			num += slotList.GetCountByEditorType(editorType);
		}
		return num;
	}

	public static int GetAllItemsCount(this ItemPackage package)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			num += slotList.GetAllItemsCount();
		}
		return num;
	}

	public static ItemObject GetItemByProtoID(this ItemPackage package, int protoID)
	{
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			ItemObject itemByProtoID = slotList.GetItemByProtoID(protoID);
			if (itemByProtoID != null)
			{
				return itemByProtoID;
			}
		}
		return null;
	}

	public static List<ItemObject> GetAllItemByProtoId(this ItemPackage package, int protoID)
	{
		List<ItemObject> list = new List<ItemObject>();
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			list.AddRange(slotList.GetAllItemByProtoID(protoID));
		}
		return list;
	}

	public static int GetCreationCount(this ItemPackage package, ECreation type)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			num += slotList.GetCreationCount(type);
		}
		return num;
	}

	public static List<int> GetCreationInstanceId(this ItemPackage package, ECreation type)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			foreach (int item in slotList.GetCreationInstanceId(type))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static bool CanAdd(this ItemPackage package, int prototypeId, int count)
	{
		return package.GetSlotList(ItemPackage.GetSlotType(prototypeId))?.CanAdd(prototypeId, count) ?? false;
	}

	public static bool CanAdd(this ItemPackage package, ItemSample itemSample)
	{
		return package.CanAdd(itemSample.protoId, itemSample.stackCount);
	}

	public static bool Split(this ItemPackage package, int instanceId, int count)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject == null)
		{
			return false;
		}
		if (!package.HasItemObj(itemObject))
		{
			return false;
		}
		if (itemObject.stackCount <= count)
		{
			return false;
		}
		itemObject.stackCount -= count;
		return package.AddAsOneItem(itemObject.protoId, count);
	}

	public static bool AddAsOneItem(this ItemPackage package, int prototypeId, int count, bool newFlag = false)
	{
		if (count <= 0)
		{
			return false;
		}
		return package.GetSlotList(ItemPackage.GetSlotType(prototypeId))?.AddAsNew(prototypeId, count, newFlag) ?? false;
	}

	public static bool Add(this ItemPackage package, int prototypeId, int count, bool newFlag = false)
	{
		if (count <= 0)
		{
			return false;
		}
		SlotList slotList = package.GetSlotList(ItemPackage.GetSlotType(prototypeId));
		if (slotList == null)
		{
			return false;
		}
		if (PeSingleton<ItemProto.Mgr>.Instance.Get(prototypeId).maxStackNum > 1)
		{
			return slotList.Add(prototypeId, count, newFlag);
		}
		bool flag = true;
		for (int i = 0; i < count; i++)
		{
			if (flag)
			{
				flag = slotList.Add(PeSingleton<ItemMgr>.Instance.CreateItem(prototypeId), newFlag);
			}
			else
			{
				slotList.Add(PeSingleton<ItemMgr>.Instance.CreateItem(prototypeId), newFlag);
			}
		}
		return flag;
	}

	public static bool Set(this ItemPackage package, int prototypeId, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		return package.GetSlotList(ItemPackage.GetSlotType(prototypeId))?.Set(prototypeId, count) ?? false;
	}

	public static bool Destroy(this ItemPackage package, int prototypeId, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		if (package.GetCount(prototypeId) < count)
		{
			return false;
		}
		return package.GetSlotList(ItemPackage.GetSlotType(prototypeId))?.Destroy(prototypeId, count) ?? false;
	}

	public static bool DestroyItem(this ItemPackage package, int instanceId, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject.stackCount < count)
		{
			return false;
		}
		SlotList slotList = package.GetSlotList(ItemPackage.GetSlotType(itemObject.protoId));
		return slotList.DestroyItem(instanceId, count);
	}

	public static bool DestroyItem(this ItemPackage package, ItemObject item, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		if (item.GetCount() < count)
		{
			return false;
		}
		SlotList slotList = package.GetSlotList(ItemPackage.GetSlotType(item.protoId));
		return slotList.DestroyItem(item.instanceId, count);
	}

	public static bool CanAdd(this ItemPackage package, IEnumerable<MaterialItem> list)
	{
		int[] array = new int[4];
		foreach (MaterialItem item in list)
		{
			int slotType = (int)ItemPackage.GetSlotType(item.protoId);
			if (slotType < array.Length)
			{
				array[slotType] += SlotListAccessor.GetNeedSlotCount(item.protoId, item.count);
			}
		}
		for (int i = 0; i < array.Length; i++)
		{
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)i);
			if (slotList.GetVacancyCount() < array[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool Add(this ItemPackage package, IEnumerable<MaterialItem> list)
	{
		foreach (MaterialItem item in list)
		{
			if (!package.Add(item.protoId, item.count))
			{
				return false;
			}
		}
		return true;
	}
}
