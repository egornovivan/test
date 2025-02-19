using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using ItemAsset.PackageHelper;
using NetworkHelper;
using Pathea;

public class PlayerPackage
{
	public ItemPackage _playerPak;

	public static ItemPackage _missionPak;

	private string quest_item = "Quest Item";

	public PlayerPackage(int itemMax, bool createMisPkg = true)
	{
		if (createMisPkg)
		{
			_missionPak = new ItemPackage(itemMax);
		}
		_playerPak = new ItemPackage(itemMax);
	}

	public void ExtendPackage(int itemMax, int equipmentMax, int recourceMax, int armMax)
	{
		_playerPak.ExtendPackage(itemMax, equipmentMax, recourceMax, armMax);
		_missionPak.ExtendPackage(itemMax, equipmentMax, recourceMax, armMax);
	}

	public void Clear(ItemPackage.ESlotType type = ItemPackage.ESlotType.Max, bool isMissionPkg = false)
	{
		if (type == ItemPackage.ESlotType.Max)
		{
			GetSlotList(ItemPackage.ESlotType.Item).Clear();
			GetSlotList(ItemPackage.ESlotType.Equipment).Clear();
			GetSlotList(ItemPackage.ESlotType.Resource).Clear();
			GetSlotList(ItemPackage.ESlotType.Armor).Clear();
			GetSlotList(ItemPackage.ESlotType.Item, isMissionPkg: true).Clear();
		}
		else if (isMissionPkg)
		{
			_missionPak.GetSlotList(type).Clear();
		}
		else
		{
			_playerPak.GetSlotList(type).Clear();
		}
		if (isMissionPkg)
		{
			_missionPak.changeEventor.Dispatch(new ItemPackage.EventArg
			{
				op = ItemPackage.EventArg.Op.Clear,
				itemObj = null
			});
		}
		else
		{
			_playerPak.changeEventor.Dispatch(new ItemPackage.EventArg
			{
				op = ItemPackage.EventArg.Op.Clear,
				itemObj = null
			});
		}
	}

	public void UpdateNewFlag(float deltaTime, bool isMissionPkg = false)
	{
		if (isMissionPkg)
		{
			_missionPak.UpdateNewFlag(deltaTime);
		}
		else
		{
			_playerPak.UpdateNewFlag(deltaTime);
		}
	}

	public SlotList GetSlotList(int prototypeId)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData == null)
		{
			return null;
		}
		if (itemData.category == quest_item)
		{
			return _missionPak.GetSlotList(ItemPackage.GetSlotType(prototypeId));
		}
		return _playerPak.GetSlotList(ItemPackage.GetSlotType(prototypeId));
	}

	public SlotList GetSlotList(ItemPackage.ESlotType type, bool isMissionPkg = false)
	{
		if (isMissionPkg)
		{
			return _missionPak.GetSlotList(type);
		}
		return _playerPak.GetSlotList(type);
	}

	public bool CanAddItemList(IEnumerable<ItemObject> items)
	{
		if (items.Any((ItemObject o) => (o.protoData.category == quest_item) ? true : false))
		{
			return _missionPak.CanAddItemList(items);
		}
		return _playerPak.CanAddItemList(items);
	}

	public bool AddItemList(IEnumerable<ItemObject> items, bool isNew = false)
	{
		foreach (ItemObject item in items)
		{
			AddItem(item, isNew);
		}
		return true;
	}

	public int AddItem(ItemObject itemObject, bool isNew = false)
	{
		if (itemObject == null)
		{
			return ItemPackage.InvalidIndex;
		}
		if (itemObject.protoData.category == quest_item)
		{
			return _missionPak.AddItem(itemObject, isNew);
		}
		return _playerPak.AddItem(itemObject, isNew);
	}

	public ItemObject FindItemByProtoId(int itemId)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return null;
		}
		if (itemData.category == quest_item)
		{
			return _missionPak.FindItemByProtoId(itemId);
		}
		return _playerPak.FindItemByProtoId(itemId);
	}

	public int PutItem(ItemObject item, int slotIndex, ItemPackage.ESlotType slotType)
	{
		if (item.protoData.category == quest_item)
		{
			return _missionPak.PutItem(item, slotIndex, slotType);
		}
		return _playerPak.PutItem(item, slotIndex, slotType);
	}

	public void PutItem(ItemObject item, int codedIndex)
	{
		if (item.protoData.category == quest_item)
		{
			_missionPak.PutItem(item, codedIndex);
		}
		else
		{
			_playerPak.PutItem(item, codedIndex);
		}
	}

	public ItemObject GetItem(int codeIndex, bool isMissionPkg = false)
	{
		if (isMissionPkg)
		{
			return _missionPak.GetItem(codeIndex);
		}
		return _playerPak.GetItem(codeIndex);
	}

	public ItemObject GetItem(ItemPackage.ESlotType slotType, int index, bool isMissionPkg = false)
	{
		if (isMissionPkg)
		{
			return _missionPak.GetItem(slotType, index);
		}
		return _playerPak.GetItem(slotType, index);
	}

	public bool RemoveItem(int codedIndex, bool isMissionPkg = false)
	{
		if (isMissionPkg)
		{
			return _missionPak.RemoveItem(codedIndex);
		}
		return _playerPak.RemoveItem(codedIndex);
	}

	public int GetItemIndexById(int instanceId, out bool isMissionPkg)
	{
		if (_playerPak.GetItemIndexById(instanceId) != -1)
		{
			isMissionPkg = false;
			return _playerPak.GetItemIndexById(instanceId);
		}
		isMissionPkg = true;
		return _missionPak.GetItemIndexById(instanceId);
	}

	public bool RemoveItemById(int instanceId)
	{
		bool isMissionPkg;
		int itemIndexById = GetItemIndexById(instanceId, out isMissionPkg);
		if (isMissionPkg)
		{
			return _missionPak.RemoveItem(itemIndexById);
		}
		return RemoveItem(itemIndexById);
	}

	public bool RemoveItem(ItemObject item)
	{
		return RemoveItemById(item.instanceId);
	}

	public bool HasItemObj(ItemObject itemObject)
	{
		if (itemObject == null)
		{
			return false;
		}
		if (itemObject.protoData.category == quest_item)
		{
			return _missionPak.HasItemObj(itemObject);
		}
		return _playerPak.HasItemObj(itemObject);
	}

	public int GetVacancySlotIndex(ItemPackage.ESlotType slotType, bool isMissionPkg = false)
	{
		if (isMissionPkg)
		{
			_missionPak.GetVacancySlotIndex(slotType);
		}
		return _playerPak.GetVacancySlotIndex(slotType);
	}

	public void Sort(ItemPackage.ESlotType type)
	{
		_playerPak.Sort(type);
		_missionPak.Sort(type);
	}

	public void Export(BinaryWriter bw)
	{
		_playerPak.Export(bw);
	}

	public void Import(byte[] buffer)
	{
		_missionPak = new ItemPackage(420);
		_playerPak.Import(buffer);
		_playerPak.ExtendPackage(420, 420, 420, 420);
	}

	public int GetCount(int prototypeId)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData == null)
		{
			return 0;
		}
		if (itemData.category == quest_item)
		{
			return _missionPak.GetCount(prototypeId);
		}
		return _playerPak.GetCount(prototypeId);
	}

	public bool ContainsItem(int prototypeId)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData == null)
		{
			return false;
		}
		if (itemData.category == quest_item)
		{
			return _missionPak.ContainsItem(prototypeId);
		}
		return _playerPak.ContainsItem(prototypeId);
	}

	public int GetCountByEditorType(int editorType)
	{
		ItemProto itemDataByEditorType = ItemProto.GetItemDataByEditorType(editorType);
		if (itemDataByEditorType.category == quest_item)
		{
			return _missionPak.GetCountByEditorType(editorType);
		}
		return _playerPak.GetCountByEditorType(editorType);
	}

	public int GetAllItemsCount()
	{
		return _missionPak.GetAllItemsCount() + _playerPak.GetAllItemsCount();
	}

	public int GetCreationCount(ECreation type)
	{
		return _playerPak.GetCreationCount(type);
	}

	public List<int> GetCreationInstanceId(ECreation type)
	{
		return _playerPak.GetCreationInstanceId(type);
	}

	public bool CanAdd(int prototypeId, int count)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData.category == quest_item)
		{
			return _missionPak.CanAdd(prototypeId, count);
		}
		return _playerPak.CanAdd(prototypeId, count);
	}

	public bool CanAdd(ItemSample itemSample)
	{
		if (itemSample.protoData.category == quest_item)
		{
			return _missionPak.CanAdd(itemSample);
		}
		return _playerPak.CanAdd(itemSample);
	}

	public bool Split(int instanceId, int count)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject == null)
		{
			return false;
		}
		if (itemObject.protoData.category == quest_item)
		{
			return _missionPak.Split(instanceId, count);
		}
		return _playerPak.Split(instanceId, count);
	}

	public bool AddAsOneItem(int prototypeId, int count, bool newFlag = false)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData.category == quest_item)
		{
			return _missionPak.AddAsOneItem(prototypeId, count, newFlag);
		}
		return _playerPak.AddAsOneItem(prototypeId, count, newFlag);
	}

	public bool Add(int prototypeId, int count, bool newFlag = false)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData.category == quest_item)
		{
			return _missionPak.Add(prototypeId, count, newFlag);
		}
		return _playerPak.Add(prototypeId, count, newFlag);
	}

	public bool Set(int prototypeId, int count)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData.category == quest_item)
		{
			return _missionPak.Set(prototypeId, count);
		}
		return _playerPak.Set(prototypeId, count);
	}

	public bool Destroy(int prototypeId, int count)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData.category == quest_item)
		{
			return _missionPak.Destroy(prototypeId, count);
		}
		return _playerPak.Destroy(prototypeId, count);
	}

	public bool DestroyItem(int instanceId, int count)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject == null)
		{
			return false;
		}
		if (itemObject.protoData.category == quest_item)
		{
			return _missionPak.DestroyItem(instanceId, count);
		}
		return _playerPak.DestroyItem(instanceId, count);
	}

	public bool DestroyItem(ItemObject item, int count)
	{
		if (item.protoData.category == quest_item)
		{
			return _missionPak.DestroyItem(item, count);
		}
		return _playerPak.DestroyItem(item, count);
	}

	public bool CanAdd(IEnumerable<MaterialItem> list)
	{
		if (list.Any(delegate(MaterialItem tmp)
		{
			ItemProto itemData = ItemProto.GetItemData(tmp.protoId);
			return (itemData.category == quest_item) ? true : false;
		}))
		{
			return _missionPak.CanAdd(list);
		}
		return _playerPak.CanAdd(list);
	}

	public bool Add(IEnumerable<MaterialItem> list)
	{
		if (list.Any(delegate(MaterialItem tmp)
		{
			ItemProto itemData = ItemProto.GetItemData(tmp.protoId);
			return (itemData.category == quest_item) ? true : false;
		}))
		{
			return _missionPak.Add(list);
		}
		return _playerPak.Add(list);
	}

	public void ResetPackageItems(int tab, int index, int id, bool bMission)
	{
		if (bMission)
		{
			_missionPak.ResetPackageItems(tab, index, id);
		}
		else
		{
			_playerPak.ResetPackageItems(tab, index, id);
		}
	}

	public void ResetPackageItems(int tab, int[] ids)
	{
		if (ids.Any(delegate(int tmp)
		{
			ItemProto itemData = ItemProto.GetItemData(tmp);
			return (itemData.category == quest_item) ? true : false;
		}))
		{
			_missionPak.ResetPackageItems(tab, ids);
		}
		else
		{
			_playerPak.ResetPackageItems(tab, ids);
		}
	}
}
