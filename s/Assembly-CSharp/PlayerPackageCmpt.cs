using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomData;
using ItemAsset;
using PETools;

public class PlayerPackageCmpt : DataCmpt
{
	private ItemPackage _playerPak;

	private ItemPackage m_MissionPackage;

	private Player _player;

	public ItemPackage missionPackage
	{
		get
		{
			if (m_MissionPackage == null)
			{
				m_MissionPackage = MissionPackageMgr.GetPak(_player.TeamId);
			}
			return m_MissionPackage;
		}
	}

	public int MaxItemNum => _playerPak.MaxItemNum;

	public int MaxEquipNum => _playerPak.MaxEquipNum;

	public int MaxResourceNum => _playerPak.MaxResourceNum;

	public int MaxArmorNum => _playerPak.MaxArmorNum;

	public ItemPackage ItemPack => _playerPak;

	public PlayerPackageCmpt()
	{
		mType = ECmptType.PlayerPackage;
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, MaxItemNum);
		BufferHelper.Serialize(w, MaxEquipNum);
		BufferHelper.Serialize(w, MaxResourceNum);
		BufferHelper.Serialize(w, MaxArmorNum);
		int[] array = GetItemObjIDs(0).ToArray();
		BufferHelper.Serialize(w, array.Length);
		int[] array2 = array;
		foreach (int value in array2)
		{
			BufferHelper.Serialize(w, value);
		}
		int[] array3 = GetItemObjIDs(1).ToArray();
		BufferHelper.Serialize(w, array3.Length);
		int[] array4 = array3;
		foreach (int value2 in array4)
		{
			BufferHelper.Serialize(w, value2);
		}
		int[] array5 = GetItemObjIDs(2).ToArray();
		BufferHelper.Serialize(w, array5.Length);
		int[] array6 = array5;
		foreach (int value3 in array6)
		{
			BufferHelper.Serialize(w, value3);
		}
		int[] array7 = GetItemObjIDs(3).ToArray();
		BufferHelper.Serialize(w, array7.Length);
		int[] array8 = array7;
		foreach (int value4 in array8)
		{
			BufferHelper.Serialize(w, value4);
		}
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		int num = BufferHelper.ReadInt32(r);
		int num2 = BufferHelper.ReadInt32(r);
		int num3 = BufferHelper.ReadInt32(r);
		int armorMax = BufferHelper.ReadInt32(r);
		ExtendPackage(num, num, num, armorMax);
		int num4 = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num4; i++)
		{
			int num5 = BufferHelper.ReadInt32(r);
			if (num5 != -1)
			{
				ItemObject itemByID = ItemManager.GetItemByID(num5);
				SetItem(itemByID, i, 0, ItemCategory.IC_None);
			}
			else
			{
				SetItem(null, i, 0, ItemCategory.IC_None);
			}
		}
		num4 = BufferHelper.ReadInt32(r);
		for (int j = 0; j < num4; j++)
		{
			int num6 = BufferHelper.ReadInt32(r);
			if (num6 != -1)
			{
				ItemObject itemByID2 = ItemManager.GetItemByID(num6);
				SetItem(itemByID2, j, 1, ItemCategory.IC_None);
			}
			else
			{
				SetItem(null, j, 1, ItemCategory.IC_None);
			}
		}
		num4 = BufferHelper.ReadInt32(r);
		for (int k = 0; k < num4; k++)
		{
			int num7 = BufferHelper.ReadInt32(r);
			if (num7 != -1)
			{
				ItemObject itemByID3 = ItemManager.GetItemByID(num7);
				SetItem(itemByID3, k, 2, ItemCategory.IC_None);
			}
			else
			{
				SetItem(null, k, 2, ItemCategory.IC_None);
			}
		}
		num4 = BufferHelper.ReadInt32(r);
		for (int l = 0; l < num4; l++)
		{
			int num8 = BufferHelper.ReadInt32(r);
			if (num8 != -1)
			{
				ItemObject itemByID4 = ItemManager.GetItemByID(num8);
				SetItem(itemByID4, l, 3, ItemCategory.IC_None);
			}
			else
			{
				SetItem(null, l, 3, ItemCategory.IC_None);
			}
		}
	}

	public void InitPackage(int itemMax, int equipMax, int resourceMax, int armorMax)
	{
		_player = base.Net as Player;
		_playerPak = new ItemPackage(itemMax, equipMax, resourceMax, armorMax);
	}

	public ItemObject GetItemByItemID(int itemId)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return null;
		}
		if (itemData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.GetItemByItemID(itemId);
		}
		return _playerPak.GetItemByItemID(itemId);
	}

	public void DeleteItem(ItemObject itemObj)
	{
		if (itemObj != null)
		{
			if (itemObj.protoData.category == ItemCategory.IC_QuestItem)
			{
				missionPackage.RemoveItem(itemObj);
				ItemManager.RemoveItem(itemObj.instanceId);
			}
			else
			{
				_playerPak.RemoveItem(itemObj);
				ItemManager.RemoveItem(itemObj.instanceId);
			}
		}
	}

	internal void RemoveItem(int itemID, int count, ref List<ItemObject> effItems)
	{
		ItemProto itemData = ItemProto.GetItemData(itemID);
		if (itemData != null)
		{
			if (itemData.category == ItemCategory.IC_QuestItem)
			{
				missionPackage.RemoveItem(itemID, count, ref effItems);
			}
			else
			{
				_playerPak.RemoveItem(itemID, count, ref effItems);
			}
		}
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

	internal void AddSameItems(int itemID, int count, ref List<ItemObject> effItems)
	{
		ItemProto itemData = ItemProto.GetItemData(itemID);
		if (itemData != null)
		{
			if (itemData.category == ItemCategory.IC_QuestItem)
			{
				missionPackage.AddSameItems(itemID, count, ref effItems);
			}
			else
			{
				_playerPak.AddSameItems(itemID, count, ref effItems);
			}
		}
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
			if (item.protoData.category == ItemCategory.IC_QuestItem)
			{
				missionPackage.AddSameItems(item, ref effItems);
			}
			else
			{
				_playerPak.AddSameItems(item, ref effItems);
			}
		}
		return effItems.ToArray();
	}

	internal void AddSameItems(ItemSample item, ref List<ItemObject> effItems)
	{
		AddSameItems(item.protoId, item.stackCount, ref effItems);
	}

	public ItemObject GetItemById(int objId)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return null;
		}
		if (itemByID.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.GetItemById(objId);
		}
		return _playerPak.GetItemById(objId);
	}

	public int RemoveItem(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return -1;
		}
		if (itemObj.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.RemoveItem(itemObj);
		}
		return _playerPak.RemoveItem(itemObj);
	}

	public List<ItemObject> GetValidItemList(int type)
	{
		return _playerPak.GetValidItemList(type);
	}

	public byte[] GetChangedIndex()
	{
		return _playerPak.GetChangedIndex();
	}

	internal IEnumerable<int> GetItemObjIDs(int tab)
	{
		return _playerPak.GetItemObjIDs(tab);
	}

	public void ExtendPackage(int itemMax, int equipmentMax, int resourceMax, int armorMax)
	{
		_playerPak.ExtendPackage(itemMax, equipmentMax, resourceMax, armorMax);
	}

	public void SetItem(ItemObject item, int index, int tabIndex, ItemCategory category)
	{
		if (category == ItemCategory.IC_QuestItem)
		{
			missionPackage.SetItem(item, index, tabIndex);
		}
		else
		{
			_playerPak.SetItem(item, index, tabIndex);
		}
	}

	public int AddItem(ItemObject item)
	{
		if (item == null)
		{
			return -1;
		}
		if (item.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.AddItem(item);
		}
		return _playerPak.AddItem(item);
	}

	public int AddItem(ItemObject item, int index)
	{
		if (!CanAdd(item))
		{
			return -1;
		}
		if (item.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.AddItem(item, index);
		}
		return _playerPak.AddItem(item, index);
	}

	public bool CanAdd(IEnumerable<ItemSample> items)
	{
		foreach (ItemSample item in items)
		{
			if (item.protoData.category == ItemCategory.IC_QuestItem)
			{
				if (!missionPackage.CanAdd(item))
				{
					return false;
				}
			}
			else if (!_playerPak.CanAdd(item))
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
		if (item.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.CanAdd(item);
		}
		return _playerPak.CanAdd(item);
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
		if (itemData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.CanAdd(itemId, num);
		}
		return _playerPak.CanAdd(itemId, num);
	}

	public void AddItemList(IEnumerable<ItemObject> items)
	{
		foreach (ItemObject item in items)
		{
			if (item.protoData.category == ItemCategory.IC_QuestItem)
			{
				missionPackage.AddItem(item, -1);
			}
			else
			{
				_playerPak.AddItem(item, -1);
			}
		}
	}

	public int GetEmptyGridCount(ItemProto protoData)
	{
		if (protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.GetEmptyGridCount(protoData.tabIndex);
		}
		return _playerPak.GetEmptyGridCount(protoData.tabIndex);
	}

	public ItemObject[] Sort(int type)
	{
		return _playerPak.Sort(type);
	}

	public ItemObject[] MissionPackageSort(int type)
	{
		return missionPackage.Sort(type);
	}

	public bool HasEnoughItems(IEnumerable<ItemSample> items)
	{
		foreach (ItemSample item in items)
		{
			if (item != null && item.protoId != -1 && item.protoData.category == ItemCategory.IC_QuestItem)
			{
				if (!missionPackage.HasEnoughItems(items))
				{
					return false;
				}
				if (!_playerPak.HasEnoughItems(items))
				{
					return false;
				}
			}
		}
		return true;
	}

	public ItemObject GetItemByIndex(int idx, ItemProto protoData)
	{
		if (protoData != null)
		{
			if (protoData.category == ItemCategory.IC_QuestItem)
			{
				return missionPackage.GetItemByIndex(idx, protoData.tabIndex);
			}
			return _playerPak.GetItemByIndex(idx, protoData.tabIndex);
		}
		return null;
	}

	internal bool ExistID(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.ExistID(item);
		}
		return _playerPak.ExistID(item);
	}

	internal bool ExistID(int objID)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null)
		{
			return false;
		}
		if (itemByID.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.ExistID(objID);
		}
		return _playerPak.ExistID(objID);
	}

	public int GetEmptyIndex(ItemProto protoData)
	{
		if (protoData == null)
		{
			return -1;
		}
		if (protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.GetEmptyIndex(protoData.tabIndex);
		}
		return _playerPak.GetEmptyIndex(protoData.tabIndex);
	}

	public int ItemNotBindCount()
	{
		return _playerPak.ItemNotBindCount();
	}

	public List<ItemObject> GetValidItemListNotBind(int type)
	{
		return _playerPak.GetValidItemListNotBind(type);
	}

	public int GetItemCount(int itemId)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return -1;
		}
		if (itemData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.GetItemCount(itemId);
		}
		return _playerPak.GetItemCount(itemId);
	}

	public int GetCountByEditorType(int editorType)
	{
		ItemProto itemDataByEditorType = ItemProto.GetItemDataByEditorType(editorType);
		if (itemDataByEditorType.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.GetCountByEditorType(editorType);
		}
		return _playerPak.GetCountByEditorType(editorType);
	}

	public int GetAllItemsCount()
	{
		return missionPackage.GetAllItemsCount() + _playerPak.GetAllItemsCount();
	}

	public bool HasEnoughSpace(List<MissionIDNum> missionItems)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int emptyGridCount = _playerPak.GetEmptyGridCount(0);
		int emptyGridCount2 = _playerPak.GetEmptyGridCount(1);
		int emptyGridCount3 = _playerPak.GetEmptyGridCount(2);
		int emptyGridCount4 = _playerPak.GetEmptyGridCount(3);
		int emptyGridCount5 = missionPackage.GetEmptyGridCount(0);
		int emptyGridCount6 = missionPackage.GetEmptyGridCount(1);
		int emptyGridCount7 = missionPackage.GetEmptyGridCount(2);
		int emptyGridCount8 = missionPackage.GetEmptyGridCount(3);
		for (int i = 0; i < missionItems.Count; i++)
		{
			ItemProto itemData = ItemProto.GetItemData(missionItems[i].id);
			if (itemData == null)
			{
				continue;
			}
			if (itemData.category == ItemCategory.IC_QuestItem)
			{
				if (itemData.tabIndex == 0)
				{
					if (itemData.maxStackNum > 0)
					{
						num5 += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
					}
				}
				else if (itemData.tabIndex == 1)
				{
					if (itemData.maxStackNum > 0)
					{
						num6 += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
					}
				}
				else if (itemData.tabIndex == 2)
				{
					if (itemData.maxStackNum > 0)
					{
						num7 += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
					}
				}
				else if (itemData.tabIndex == 3 && itemData.maxStackNum > 0)
				{
					num8 += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
				}
			}
			else if (itemData.tabIndex == 0)
			{
				if (itemData.maxStackNum > 0)
				{
					num += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
				}
			}
			else if (itemData.tabIndex == 1)
			{
				if (itemData.maxStackNum > 0)
				{
					num2 += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
				}
			}
			else if (itemData.tabIndex == 2)
			{
				if (itemData.maxStackNum > 0)
				{
					num3 += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
				}
			}
			else if (itemData.tabIndex == 3 && itemData.maxStackNum > 0)
			{
				num4 += (missionItems[i].num - 1) / itemData.maxStackNum + 1;
			}
		}
		if (num > emptyGridCount || num2 > emptyGridCount2 || num3 > emptyGridCount3 || num4 > emptyGridCount4)
		{
			return false;
		}
		if (num5 > emptyGridCount5 || num6 > emptyGridCount6 || num7 > emptyGridCount7 || num8 > emptyGridCount8)
		{
			return false;
		}
		return true;
	}

	public int GetItemIndex(ItemObject item)
	{
		if (item == null)
		{
			return -1;
		}
		if (item.protoData.category == ItemCategory.IC_QuestItem)
		{
			return missionPackage.GetItemIndex(item);
		}
		return _playerPak.GetItemIndex(item);
	}

	public void Clear(bool isPlayer = true)
	{
		if (isPlayer)
		{
			_playerPak.Clear();
		}
		else
		{
			missionPackage.Clear();
		}
	}

	public int GetCreationItemCount(ECreation type)
	{
		return _playerPak.GetCreationCount(_playerPak, type);
	}

	public void SyncMissionPackageIndex()
	{
		byte[] changedIndex = missionPackage.GetChangedIndex();
		if (changedIndex != null)
		{
			_player.SyncGroupData(EPacketType.PT_InGame_MissionPackageIndex, changedIndex);
		}
	}

	public void SyncPackageIndex(int tab)
	{
		byte[] array2 = Serialize.Export(delegate(BinaryWriter w)
		{
			int[] array = missionPackage.GetItemObjIDs(tab).ToArray();
			BufferHelper.Serialize(w, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				int num = tab;
				num <<= 16;
				num += i;
				BufferHelper.Serialize(w, num);
				BufferHelper.Serialize(w, array[i]);
			}
		});
		_player.RPCOwner(EPacketType.PT_InGame_MissionPackageIndex, array2);
	}

	public void SyncPackageItems(int tab)
	{
		List<ItemObject> validItemList = missionPackage.GetValidItemList(tab);
		_player.SyncItemList(validItemList.ToArray());
	}
}
