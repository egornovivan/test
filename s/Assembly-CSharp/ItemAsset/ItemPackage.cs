using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomData;
using PETools;
using UnityEngine;

namespace ItemAsset;

public class ItemPackage
{
	public enum ESlotType
	{
		Item,
		Equipment,
		Resource,
		Armor,
		Max
	}

	private int _maxItemNum;

	private int _maxEquipNum;

	private int _maxResourceNum;

	private int _maxArmorNum;

	private List<ItemObject> _itemList;

	private List<ItemObject> _equipList;

	private List<ItemObject> _resourceList;

	private List<ItemObject> _armorList;

	private Dictionary<int, int> _changedIndex = new Dictionary<int, int>();

	public int MaxItemNum => _maxItemNum;

	public int MaxEquipNum => _maxEquipNum;

	public int MaxResourceNum => _maxResourceNum;

	public int MaxArmorNum => _maxArmorNum;

	public ItemPackage(int itemMax, int equipMax, int resourceMax, int armorMax)
	{
		_maxItemNum = itemMax;
		_maxEquipNum = equipMax;
		_maxResourceNum = resourceMax;
		_maxArmorNum = armorMax;
		_itemList = new List<ItemObject>(itemMax);
		_equipList = new List<ItemObject>(equipMax);
		_resourceList = new List<ItemObject>(resourceMax);
		_armorList = new List<ItemObject>(armorMax);
		ExtendPackage(itemMax, equipMax, resourceMax, armorMax);
	}

	private void OnChange(int tab, int index, int id)
	{
		int num = tab;
		num <<= 16;
		num += index;
		_changedIndex[num] = id;
	}

	public int ItemNotBindCount()
	{
		return _itemList.Count((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind) + _equipList.Count((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind) + _resourceList.Count((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind) + _armorList.Count((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind);
	}

	public void Clear()
	{
		_itemList.Clear();
		_equipList.Clear();
		_resourceList.Clear();
		_armorList.Clear();
	}

	public void ExtendPackage(int itemMax, int equipmentMax, int resourceMax, int armorMax)
	{
		if (itemMax > 0)
		{
			ExtendItemPackage(itemMax);
		}
		if (equipmentMax > 0)
		{
			ExtendEquipmentPackage(equipmentMax);
		}
		if (resourceMax > 0)
		{
			ExtendResourcesPackage(resourceMax);
		}
		if (armorMax > 0)
		{
			ExtendArmorPackage(armorMax);
		}
	}

	public void ExtendItemPackage(int nItemMax)
	{
		for (int i = _itemList.Count; i < nItemMax; i++)
		{
			_itemList.Add(null);
		}
	}

	public void ExtendEquipmentPackage(int nEquipmentMax)
	{
		for (int i = _equipList.Count; i < nEquipmentMax; i++)
		{
			_equipList.Add(null);
		}
	}

	public void ExtendResourcesPackage(int resourcesMax)
	{
		for (int i = _resourceList.Count; i < resourcesMax; i++)
		{
			_resourceList.Add(null);
		}
	}

	public void ExtendArmorPackage(int armorMax)
	{
		for (int i = _armorList.Count; i < armorMax; i++)
		{
			_armorList.Add(null);
		}
	}

	public IEnumerable<ItemObject> GetSameItems(int itemId)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			yield break;
		}
		List<ItemObject> items = GetItemList(itemData.tabIndex);
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] != null && items[i].protoId == itemId)
			{
				yield return items[i];
			}
		}
	}

	public IEnumerable<KeyValuePair<int, ItemObject>> GetSameItemsWithIndex(int itemId)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			yield break;
		}
		List<ItemObject> items = GetItemList(itemData.tabIndex);
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] != null && items[i].protoId == itemId)
			{
				yield return new KeyValuePair<int, ItemObject>(i, items[i]);
			}
		}
	}

	public List<ItemObject> GetItemList(int type)
	{
		return type switch
		{
			0 => _itemList, 
			1 => _equipList, 
			2 => _resourceList, 
			3 => _armorList, 
			_ => null, 
		};
	}

	public List<ItemObject> GetValidItemList(int type)
	{
		return type switch
		{
			0 => _itemList.FindAll((ItemObject iter) => null != iter), 
			1 => _equipList.FindAll((ItemObject iter) => null != iter), 
			2 => _resourceList.FindAll((ItemObject iter) => null != iter), 
			3 => _armorList.FindAll((ItemObject iter) => null != iter), 
			_ => null, 
		};
	}

	public List<ItemObject> GetValidItemListNotBind(int type)
	{
		return type switch
		{
			0 => _itemList.FindAll((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind), 
			1 => _equipList.FindAll((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind), 
			2 => _resourceList.FindAll((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind), 
			3 => _armorList.FindAll((ItemObject iter) => iter != null && iter.protoData != null && !iter.bind), 
			_ => null, 
		};
	}

	public byte[] GetChangedIndex()
	{
		if (_changedIndex.Count <= 0)
		{
			return null;
		}
		return Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, _changedIndex.Count);
			foreach (KeyValuePair<int, int> item in _changedIndex)
			{
				BufferHelper.Serialize(w, item.Key);
				BufferHelper.Serialize(w, item.Value);
			}
			_changedIndex.Clear();
		});
	}

	internal ItemObject[] AddSameItems(IEnumerable<ItemSample> items)
	{
		if (!CanAdd(items))
		{
			return null;
		}
		List<ItemObject> effItems = new List<ItemObject>(10);
		foreach (ItemSample item in items)
		{
			AddSameItems(item.protoId, item.stackCount, ref effItems);
		}
		return effItems.ToArray();
	}

	internal void AddSameItems(ItemSample item, ref List<ItemObject> effItems)
	{
		AddSameItems(item.protoId, item.stackCount, ref effItems);
	}

	internal void AddSameItems(int itemID, int count, ref List<ItemObject> effItems)
	{
		if (!CanAdd(itemID, count))
		{
			return;
		}
		ItemProto itemData = ItemProto.GetItemData(itemID);
		if (itemData == null)
		{
			return;
		}
		ItemObject[] array = GetSameItems(itemID).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (count <= 0)
			{
				break;
			}
			if (array[i].stackCount < itemData.maxStackNum)
			{
				int num = Mathf.Min(itemData.maxStackNum - array[i].stackCount, count);
				array[i].CountUp(num);
				count -= num;
				OnChange(array[i].protoData.tabIndex, GetItemIndex(ItemManager.GetItemByID(array[i].instanceId)), array[i].instanceId);
				if (!effItems.Contains(array[i]))
				{
					effItems.Add(array[i]);
				}
			}
		}
		int num2 = count / itemData.maxStackNum;
		for (int j = 0; j < num2; j++)
		{
			ItemObject item = ItemManager.CreateItem(itemID, itemData.maxStackNum);
			AddItem(item);
			count -= itemData.maxStackNum;
			if (!effItems.Contains(item))
			{
				effItems.Add(item);
			}
		}
		int num3 = count % itemData.maxStackNum;
		if (num3 != 0)
		{
			ItemObject item2 = ItemManager.CreateItem(itemID, num3);
			AddItem(item2);
			count -= num3;
			if (!effItems.Contains(item2))
			{
				effItems.Add(item2);
			}
		}
	}

	internal void RemoveItem(int itemID, int count, ref List<ItemObject> effItems)
	{
		ItemProto itemData = ItemProto.GetItemData(itemID);
		if (itemData == null)
		{
			return;
		}
		ItemObject[] array = GetSameItems(itemID).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (count <= 0)
			{
				break;
			}
			int num = Mathf.Min(count, array[i].stackCount);
			if (!array[i].CountDown(num))
			{
				DeleteItem(array[i]);
			}
			else if (!effItems.Contains(array[i]))
			{
				effItems.Add(array[i]);
			}
			count -= num;
		}
	}

	public bool HasEnoughItems(int protoId, int stackCount)
	{
		if (protoId == -1)
		{
			return false;
		}
		if (GetItemCount(protoId) <= stackCount)
		{
			return false;
		}
		return true;
	}

	public bool HasEnoughItems(ItemSample item)
	{
		if (item == null || item.protoId == -1)
		{
			return false;
		}
		if (GetItemCount(item.protoId) <= item.stackCount)
		{
			return false;
		}
		return true;
	}

	public bool HasEnoughItems(IEnumerable<ItemSample> items)
	{
		foreach (ItemSample item in items)
		{
			if (item == null || item.protoId == -1 || GetItemCount(item.protoId) > item.stackCount)
			{
				continue;
			}
			return false;
		}
		return true;
	}

	internal ItemObject[] RemoveItem(IEnumerable<ItemSample> items)
	{
		List<ItemObject> effItems = new List<ItemObject>(10);
		foreach (ItemSample item in items)
		{
			RemoveItem(item.protoId, item.stackCount, ref effItems);
		}
		return effItems.ToArray();
	}

	public ItemObject GetItemByIndex(int idx, int type)
	{
		if (idx < 0)
		{
			return null;
		}
		if (type == 1 && idx < _equipList.Count)
		{
			return _equipList[idx];
		}
		if (type == 2 && idx < _resourceList.Count)
		{
			return _resourceList[idx];
		}
		if (type == 0 && idx < _itemList.Count)
		{
			return _itemList[idx];
		}
		if (type == 3 && idx < _armorList.Count)
		{
			return _armorList[idx];
		}
		return null;
	}

	public ItemObject GetItemByIndex(ESlotType slotType, int index)
	{
		List<ItemObject> itemList = GetItemList((int)slotType);
		if (itemList != null && index >= 0 && index < itemList.Count)
		{
			return itemList[index];
		}
		return null;
	}

	public ItemObject GetItemById(int objId)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return null;
		}
		List<ItemObject> itemList = GetItemList(itemByID.GetTabIndex());
		return itemList.Find((ItemObject iter) => iter != null && iter.instanceId == objId);
	}

	internal bool ExistID(int objID)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null)
		{
			return false;
		}
		return ExistID(itemByID);
	}

	internal bool ExistID(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		List<ItemObject> itemList = GetItemList(item.GetTabIndex());
		return itemList.Exists((ItemObject iter) => iter != null && iter.instanceId == item.instanceId);
	}

	public int GetEmptyIndex(int type)
	{
		return type switch
		{
			1 => _equipList.FindIndex((ItemObject iter0) => iter0 == null), 
			2 => _resourceList.FindIndex((ItemObject iter0) => iter0 == null), 
			0 => _itemList.FindIndex((ItemObject iter0) => iter0 == null), 
			3 => _armorList.FindIndex((ItemObject iter0) => iter0 == null), 
			_ => -1, 
		};
	}

	internal IEnumerable<int> GetItemObjIDs(int tab)
	{
		return tab switch
		{
			1 => _equipList.Select((ItemObject iter) => iter?.instanceId ?? (-1)), 
			2 => _resourceList.Select((ItemObject iter) => iter?.instanceId ?? (-1)), 
			0 => _itemList.Select((ItemObject iter) => iter?.instanceId ?? (-1)), 
			3 => _armorList.Select((ItemObject iter) => iter?.instanceId ?? (-1)), 
			_ => null, 
		};
	}

	public int GetItemCount(int itemId)
	{
		IEnumerable<ItemObject> sameItems = GetSameItems(itemId);
		return sameItems.Sum((ItemObject iter) => iter.stackCount);
	}

	public int GetCountByEditorType(int editorType)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			List<ItemObject> itemList = GetItemList(i);
			num += itemList.Sum((ItemObject iter) => (iter != null && iter.protoData.editorTypeId == editorType) ? iter.stackCount : 0);
		}
		return num;
	}

	public int GetAllItemsCount()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			List<ItemObject> itemList = GetItemList(i);
			num += itemList.Sum((ItemObject iter) => iter?.stackCount ?? 0);
		}
		return num;
	}

	public ItemObject GetItemByItemID(int itemId)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return null;
		}
		List<ItemObject> itemList = GetItemList(itemData.tabIndex);
		return itemList.Find((ItemObject iter) => iter != null && iter.protoId == itemId);
	}

	public bool IsEmpty(int index, int type)
	{
		List<ItemObject> itemList = GetItemList(type);
		return itemList[index] == null;
	}

	public int GetEmptyGridCount(int type)
	{
		List<ItemObject> itemList = GetItemList(type);
		return itemList.Count((ItemObject iter) => null == iter);
	}

	public int GetItemIndex(ItemObject item)
	{
		List<ItemObject> itemList = GetItemList(item.protoData.tabIndex);
		return itemList.FindIndex((ItemObject iter) => iter != null && iter.instanceId == item.instanceId);
	}

	public int GetItemIndex(ESlotType slotType, int itemID)
	{
		List<ItemObject> itemList = GetItemList((int)slotType);
		return itemList.FindIndex((ItemObject item) => item != null && item.instanceId == itemID);
	}

	internal IEnumerable<ItemObject> MergeItemList(int type)
	{
		List<ItemObject> validItems = GetValidItemList(type);
		while (validItems.Count >= 1)
		{
			int itemId = validItems[0].protoId;
			ItemObject[] sameItems = validItems.Where((ItemObject iter) => iter.protoId == itemId && iter.stackCount != iter.MaxStackNum).ToArray();
			validItems.RemoveAll((ItemObject iter) => iter.protoId == itemId);
			if (sameItems.Length <= 1)
			{
				continue;
			}
			int totalNum = sameItems.Sum((ItemObject iter) => iter.stackCount);
			for (int i = 0; i < sameItems.Length; i++)
			{
				if (totalNum != 0)
				{
					sameItems[i].CountDown(sameItems[i].stackCount);
					int num = Mathf.Min(sameItems[i].MaxStackNum, totalNum);
					sameItems[i].SetStackCount(num);
					totalNum -= num;
					yield return sameItems[i];
				}
				else
				{
					DeleteItem(sameItems[i]);
				}
			}
		}
	}

	public ItemObject[] Sort(int type)
	{
		ItemObject[] result = MergeItemList(type).ToArray();
		List<ItemObject> validItemList = GetValidItemList(type);
		validItemList.Sort(delegate(ItemObject item1, ItemObject item2)
		{
			if (item1.protoData.sortLabel > item2.protoData.sortLabel)
			{
				return 1;
			}
			if (item1.protoData.sortLabel < item2.protoData.sortLabel)
			{
				return -1;
			}
			if (item1.protoId > item2.protoId)
			{
				return 1;
			}
			if (item1.protoId < item2.protoId)
			{
				return -1;
			}
			if (item1.stackCount > item2.stackCount)
			{
				return 1;
			}
			return (item1.stackCount < item2.stackCount) ? (-1) : 0;
		});
		ItemObject[] array = validItemList.ToArray();
		List<ItemObject> itemList = GetItemList(type);
		for (int i = 0; i < itemList.Count; i++)
		{
			if (i < array.Length)
			{
				SetItem(array[i], i, type);
				continue;
			}
			itemList[i] = null;
			SetItem(null, i, type);
		}
		return result;
	}

	public void DeleteItem(ItemObject itemObj)
	{
		RemoveItem(itemObj);
		ItemManager.RemoveItem(itemObj.instanceId);
	}

	public int RemoveItem(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return -1;
		}
		int tabIndex = itemObj.GetTabIndex();
		List<ItemObject> itemList = GetItemList(tabIndex);
		int num = itemList.FindIndex((ItemObject iter) => iter != null && iter.instanceId == itemObj.instanceId);
		if (num == -1)
		{
			return -1;
		}
		itemList[num] = null;
		OnChange(tabIndex, num, -1);
		SceneObjMgr.AddItem(itemObj);
		return num;
	}

	public ItemObject RemoveItemByIndex(ESlotType slotType, int index)
	{
		List<ItemObject> itemList = GetItemList((int)slotType);
		if (itemList != null && index >= 0 && index < itemList.Count)
		{
			ItemObject itemObject = itemList[index];
			itemList[index] = null;
			OnChange((int)slotType, index, -1);
			SceneObjMgr.AddItem(itemObject);
			return itemObject;
		}
		return null;
	}

	public void SetItem(ItemObject item, int index, int tabIndex)
	{
		switch (tabIndex)
		{
		default:
			return;
		case 0:
			_itemList[index] = item;
			break;
		case 1:
			_equipList[index] = item;
			break;
		case 2:
			_resourceList[index] = item;
			break;
		case 3:
			_armorList[index] = item;
			break;
		}
		int id = -1;
		if (item != null)
		{
			id = item.instanceId;
			SceneObjMgr.RemoveItem(item.instanceId);
		}
		OnChange(tabIndex, index, id);
	}

	public int AddItem(ItemObject item)
	{
		return AddItem(item, -1);
	}

	public int AddItem(ItemObject item, int index)
	{
		if (!CanAdd(item))
		{
			return -1;
		}
		int tabIndex = item.GetTabIndex();
		List<ItemObject> itemList = GetItemList(tabIndex);
		if (index == -1)
		{
			index = GetEmptyIndex(tabIndex);
		}
		else if (itemList[index] != null)
		{
			return -1;
		}
		itemList[index] = item;
		OnChange(tabIndex, index, item.instanceId);
		SceneObjMgr.RemoveItem(item.instanceId);
		return index;
	}

	public void AddItemList(IEnumerable<ItemObject> items)
	{
		foreach (ItemObject item in items)
		{
			AddItem(item, -1);
		}
	}

	public bool Contains(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		List<ItemObject> itemList = GetItemList(item.protoData.tabIndex);
		if (itemList.Contains(item))
		{
			return true;
		}
		return false;
	}

	public bool CanAdd(ItemObject item)
	{
		if (Contains(item))
		{
			return false;
		}
		return GetEmptyGridCount(item.protoData.tabIndex) >= 1;
	}

	public bool CanAdd(ItemSample item)
	{
		return CanAdd(item.protoId, item.stackCount);
	}

	public bool CanAdd(int itemId, int num)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return false;
		}
		IEnumerable<ItemObject> sameItems = GetSameItems(itemId);
		int num2 = sameItems.Sum((ItemObject iter) => iter.LeftStackCount);
		int num3 = ((num > num2) ? (num - num2) : 0);
		int num4 = (num3 - 1) / itemData.maxStackNum + 1;
		return num4 <= GetEmptyGridCount(itemData.tabIndex);
	}

	public bool CanAdd(ItemObject[] items)
	{
		if (items.Length <= 0)
		{
			return true;
		}
		int emptyGridCount = GetEmptyGridCount(0);
		int emptyGridCount2 = GetEmptyGridCount(1);
		int emptyGridCount3 = GetEmptyGridCount(2);
		int emptyGridCount4 = GetEmptyGridCount(3);
		int num = items.Count((ItemObject iter) => iter.GetTabIndex() == 0);
		int num2 = items.Count((ItemObject iter) => iter.GetTabIndex() == 1);
		int num3 = items.Count((ItemObject iter) => iter.GetTabIndex() == 2);
		int num4 = items.Count((ItemObject iter) => iter.GetTabIndex() == 3);
		return emptyGridCount >= num && emptyGridCount2 >= num2 && emptyGridCount3 >= num3 && emptyGridCount4 >= num4;
	}

	public bool CanAdd(IEnumerable<ItemSample> items)
	{
		int emptyGridCount = GetEmptyGridCount(0);
		int emptyGridCount2 = GetEmptyGridCount(1);
		int emptyGridCount3 = GetEmptyGridCount(2);
		int emptyGridCount4 = GetEmptyGridCount(3);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (ItemSample item in items)
		{
			if (item.protoData.tabIndex == 0)
			{
				num += (item.stackCount - 1) / item.MaxStackNum + 1;
			}
			else if (item.protoData.tabIndex == 1)
			{
				num2 += (item.stackCount - 1) / item.MaxStackNum + 1;
			}
			else if (item.protoData.tabIndex == 3)
			{
				num4 += (item.stackCount - 1) / item.MaxStackNum + 1;
			}
			else
			{
				num3 += (item.stackCount - 1) / item.MaxStackNum + 1;
			}
		}
		return num <= emptyGridCount && num2 <= emptyGridCount2 && num3 <= emptyGridCount3 && num4 <= emptyGridCount4;
	}

	public int GetCreationCount(ItemPackage package, ECreation type)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			List<ItemObject> itemList = package.GetItemList(i);
			foreach (ItemObject item in itemList)
			{
				if (item != null && item.instanceId >= 100000000 && CreationHelper.GetCreationItemClass(item) == CreationHelper.ECreationToItemClass(type))
				{
					num += item.stackCount;
				}
			}
		}
		return num;
	}
}
