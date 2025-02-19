using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class StorageManager
{
	private static Dictionary<int, ItemPackage> _packageList = new Dictionary<int, ItemPackage>();

	public static bool ExistsPlayer(int playerID)
	{
		return _packageList.ContainsKey(playerID);
	}

	public static ItemPackage GetPlayerPackage(int playerID)
	{
		if (!_packageList.ContainsKey(playerID))
		{
			_packageList[playerID] = new ItemPackage(84, 84, 84, 84);
		}
		return _packageList[playerID];
	}

	public static IEnumerable<int> GetPlayerItemIDs(int playerID, int tabIndex)
	{
		ItemPackage playerPackage = GetPlayerPackage(playerID);
		return playerPackage.GetItemObjIDs(tabIndex);
	}

	public static void Store(Player player, int objID, int dstIndex)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null || null == player)
		{
			return;
		}
		if (!player.Package.ExistID(itemByID))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		ItemPackage playerPackage = GetPlayerPackage(player.Id);
		if (dstIndex != -1)
		{
			if (playerPackage.GetItemByIndex(dstIndex, itemByID.protoData.tabIndex) != null)
			{
				dstIndex = playerPackage.GetEmptyIndex(itemByID.protoData.tabIndex);
				if (dstIndex == -1)
				{
					if (LogFilter.logDebug)
					{
						Debug.LogWarning("Not enough space.");
					}
					return;
				}
			}
		}
		else
		{
			dstIndex = playerPackage.GetEmptyIndex(itemByID.protoData.tabIndex);
		}
		player.Package.RemoveItem(itemByID);
		playerPackage.AddItem(itemByID, dstIndex);
		player.SyncPackageIndex();
		player.RPCOwner(EPacketType.PT_InGame_PersonalStorageStore, itemByID.instanceId, dstIndex);
	}

	public static void Delete(Player player, int objID)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null || null == player || !_packageList.ContainsKey(player.Id))
		{
			return;
		}
		ItemPackage itemPackage = _packageList[player.Id];
		if (!itemPackage.ExistID(itemByID))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
		}
		else
		{
			itemPackage.RemoveItem(itemByID);
			ItemManager.RemoveItem(itemByID.instanceId);
			player.RPCOwner(EPacketType.PT_InGame_PersonalStroageDelete, itemByID.instanceId);
		}
	}

	public static void Fetch(Player player, int objID, int dstIndex)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null || null == player || !_packageList.ContainsKey(player.Id))
		{
			return;
		}
		ItemPackage itemPackage = _packageList[player.Id];
		if (!itemPackage.ExistID(itemByID))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		if (dstIndex != -1)
		{
			if (player.Package.GetItemByIndex(dstIndex, itemByID.protoData) != null)
			{
				dstIndex = player.Package.GetEmptyIndex(itemByID.protoData);
				if (dstIndex == -1)
				{
					if (LogFilter.logDebug)
					{
						Debug.LogWarning("Not enough space.");
					}
					return;
				}
			}
		}
		else
		{
			dstIndex = player.Package.GetEmptyIndex(itemByID.protoData);
			if (dstIndex == -1)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("Not enough space.");
				}
				return;
			}
		}
		itemPackage.RemoveItem(itemByID);
		player.Package.AddItem(itemByID, dstIndex);
		player.RPCOwner(EPacketType.PT_InGame_PersonalStorageFetch, itemByID.instanceId, dstIndex);
		player.SyncPackageIndex();
	}

	public static void Exchange(Player player, int objID, int originIndex, int destIndex)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null || null == player || !_packageList.ContainsKey(player.Id))
		{
			return;
		}
		ItemPackage itemPackage = _packageList[player.Id];
		if (!itemPackage.ExistID(itemByID))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		itemPackage.RemoveItem(itemByID);
		ItemObject itemByIndex = itemPackage.GetItemByIndex(destIndex, itemByID.protoData.tabIndex);
		if (itemByIndex != null)
		{
			itemPackage.RemoveItem(itemByIndex);
			itemPackage.AddItem(itemByIndex, originIndex);
		}
		itemPackage.AddItem(itemByID, destIndex);
		player.RPCOwner(EPacketType.PT_InGame_PersonalStorageExchange, objID, destIndex, itemByIndex?.instanceId ?? (-1), originIndex);
	}

	public static void Split(Player player, int objID, int num)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null || null == player || num >= itemByID.stackCount || !_packageList.ContainsKey(player.Id))
		{
			return;
		}
		ItemPackage itemPackage = _packageList[player.Id];
		if (!itemPackage.ExistID(itemByID))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		int emptyIndex = itemPackage.GetEmptyIndex(itemByID.protoData.tabIndex);
		if (emptyIndex == -1)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough space.");
			}
			return;
		}
		ItemObject itemObject = ItemManager.CreateFromItem(itemByID.protoId, num, itemByID);
		itemByID.CountDown(num);
		itemPackage.AddItem(itemObject, emptyIndex);
		player.SyncItemList(new ItemObject[2] { itemByID, itemObject });
		player.RPCOwner(EPacketType.PT_InGame_PersonalStorageSplit, itemObject.instanceId, emptyIndex);
	}

	public static void Sort(Player player, int tabIndex)
	{
		if (_packageList.ContainsKey(player.Id))
		{
			ItemPackage itemPackage = _packageList[player.Id];
			ItemObject[] items = itemPackage.Sort(tabIndex);
			player.SyncItemList(items);
			IEnumerable<int> itemObjIDs = itemPackage.GetItemObjIDs(tabIndex);
			player.RPCOwner(EPacketType.PT_InGame_PersonalStorageSort, tabIndex, itemObjIDs.ToArray());
		}
	}
}
