using System.Collections.Generic;
using ItemAsset;

public class PackageCmpt : DataCmpt
{
	private ItemPackage mPackage;

	public int MaxItemNum => mPackage.MaxItemNum;

	public int MaxEquipNum => mPackage.MaxEquipNum;

	public int MaxResourceNum => mPackage.MaxResourceNum;

	public int MaxArmorNum => mPackage.MaxArmorNum;

	public PackageCmpt()
	{
		mType = ECmptType.Package;
	}

	public void InitPackage(int itemMax, int equipMax, int resourceMax, int armorMax)
	{
		mPackage = new ItemPackage(itemMax, equipMax, resourceMax, armorMax);
	}

	public ItemObject GetItemByItemID(int itemId)
	{
		return mPackage.GetItemByItemID(itemId);
	}

	public ItemObject GetItemById(int objId)
	{
		return mPackage.GetItemById(objId);
	}

	public void DeleteItem(ItemObject itemObj)
	{
		if (itemObj != null && RemoveItem(itemObj) != -1)
		{
			ItemManager.RemoveItem(itemObj.instanceId);
		}
	}

	public int RemoveItem(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return -1;
		}
		return mPackage.RemoveItem(itemObj);
	}

	public void RemoveItem(int itemID, int count, ref List<ItemObject> effItems)
	{
		ItemProto itemData = ItemProto.GetItemData(itemID);
		if (itemData != null)
		{
			mPackage.RemoveItem(itemID, count, ref effItems);
		}
	}

	public ItemObject[] RemoveItem(IEnumerable<ItemSample> items)
	{
		List<ItemObject> effItems = new List<ItemObject>(10);
		foreach (ItemSample item in items)
		{
			RemoveItem(item.protoId, item.stackCount, ref effItems);
		}
		return effItems.ToArray();
	}

	public void AddSameItems(int itemID, int count, ref List<ItemObject> effItems)
	{
		mPackage.AddSameItems(itemID, count, ref effItems);
	}

	public ItemObject[] AddSameItems(IEnumerable<ItemSample> items)
	{
		if (!CanAdd(items))
		{
			return null;
		}
		List<ItemObject> effItems = new List<ItemObject>(10);
		foreach (ItemSample item in items)
		{
			mPackage.AddSameItems(item, ref effItems);
		}
		return effItems.ToArray();
	}

	public void AddSameItems(ItemSample item, ref List<ItemObject> effItems)
	{
		AddSameItems(item.protoId, item.stackCount, ref effItems);
	}

	public List<ItemObject> GetValidItemList(int type)
	{
		return mPackage.GetValidItemList(type);
	}

	public byte[] GetChangedIndex()
	{
		return mPackage.GetChangedIndex();
	}

	internal IEnumerable<int> GetItemObjIDs(int tab)
	{
		return mPackage.GetItemObjIDs(tab);
	}

	public void ExtendPackage(int itemMax, int equipmentMax, int resourceMax, int armorMax)
	{
		mPackage.ExtendPackage(itemMax, equipmentMax, resourceMax, armorMax);
	}

	public void SetItem(ItemObject item, int index, int tabIndex, ItemCategory category)
	{
		mPackage.SetItem(item, index, tabIndex);
	}

	public int AddItem(ItemObject item)
	{
		if (item == null)
		{
			return -1;
		}
		return mPackage.AddItem(item);
	}

	public int AddItem(ItemObject item, int index)
	{
		if (!CanAdd(item))
		{
			return -1;
		}
		return mPackage.AddItem(item, index);
	}

	public bool CanAdd(IEnumerable<ItemSample> items)
	{
		foreach (ItemSample item in items)
		{
			if (!mPackage.CanAdd(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanAdd(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		return mPackage.CanAdd(item);
	}

	public bool CanAdd(ItemSample item)
	{
		return CanAdd(item.protoId, item.stackCount);
	}

	public bool CanAdd(int itemId, int num)
	{
		return mPackage.CanAdd(itemId, num);
	}

	public void AddItemList(IEnumerable<ItemObject> items)
	{
		foreach (ItemObject item in items)
		{
			mPackage.AddItem(item, -1);
		}
	}

	public int GetEmptyGridCount(ItemProto protoData)
	{
		return mPackage.GetEmptyGridCount(protoData.tabIndex);
	}

	public ItemObject[] Sort(int type)
	{
		return mPackage.Sort(type);
	}

	public bool HasEnoughItems(IEnumerable<ItemSample> items)
	{
		foreach (ItemSample item in items)
		{
			if (item == null || item.protoId == -1 || mPackage.HasEnoughItems(items))
			{
				continue;
			}
			return false;
		}
		return true;
	}

	public ItemObject GetItemByIndex(int idx, ItemProto protoData)
	{
		if (protoData != null)
		{
			return mPackage.GetItemByIndex(idx, protoData.tabIndex);
		}
		return null;
	}

	internal bool ExistID(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		return mPackage.ExistID(item);
	}

	internal bool ExistID(int objID)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null)
		{
			return false;
		}
		return mPackage.ExistID(objID);
	}

	public int GetEmptyIndex(ItemProto protoData)
	{
		if (protoData == null)
		{
			return -1;
		}
		return mPackage.GetEmptyIndex(protoData.tabIndex);
	}

	public int ItemNotBindCount()
	{
		return mPackage.ItemNotBindCount();
	}

	public List<ItemObject> GetValidItemListNotBind(int type)
	{
		return mPackage.GetValidItemListNotBind(type);
	}

	public int GetItemCount(int itemId)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return -1;
		}
		return mPackage.GetItemCount(itemId);
	}

	public int GetItemIndex(ItemObject item)
	{
		if (item == null)
		{
			return -1;
		}
		return mPackage.GetItemIndex(item);
	}

	public void Clear()
	{
		mPackage.Clear();
	}
}
