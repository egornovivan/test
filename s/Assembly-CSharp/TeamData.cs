using System;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using uLink;
using UnityEngine;

public class TeamData
{
	private List<MemberData> _teamPeers;

	private ItemPackage _package;

	private int _teamID;

	private int _leaderId;

	private int _maxNum;

	public int TeamID => _teamID;

	public int LeaderID => _leaderId;

	public int MaxNum => _maxNum;

	public int MemberNum => _teamPeers.Count((MemberData iter) => null != iter);

	public bool IsFull => MemberNum == _maxNum;

	public TeamData(int teamId, int maxNum)
	{
		_teamID = teamId;
		_maxNum = Mathf.Clamp(maxNum, 1, 32);
		ForceSetting.AddForce(_teamID, _maxNum, ServerConfig.GameType);
	}

	public void InitTeamData()
	{
		_package = new ItemPackage(210, 210, 210, 210);
		_teamPeers = new List<MemberData>(_maxNum);
		for (int i = 0; i < _maxNum; i++)
		{
			_teamPeers.Add(null);
		}
		_leaderId = -1;
		uLink.Network.SetGroupFlags(TeamID, NetworkGroupFlags.HideGameObjects);
	}

	public void Reset()
	{
		_package.Clear();
		BattleManager.Reset(_teamID);
	}

	public void RemoveMember(int id)
	{
		int num = _teamPeers.FindIndex((MemberData iter) => iter != null && iter.Id == id);
		if (num != -1)
		{
			MemberData memberData = _teamPeers[num];
			NetInterface.RemovePlayerFromGroup(memberData.Peer, TeamID);
			ForceSetting.RemoveAllyPlayer(id, LeaderID);
			_teamPeers[num] = null;
		}
	}

	public bool AddMember(int id, uLink.NetworkPlayer peer)
	{
		int num = _teamPeers.FindIndex((MemberData iter) => null == iter);
		if (num != -1)
		{
			_teamPeers[num] = new MemberData(id, peer);
			NetInterface.AddPlayerToGroup(peer, TeamID);
			ForceSetting.AddAllyPlayer(id, LeaderID);
			return true;
		}
		return false;
	}

	public void GetMembers(List<uLink.NetworkPlayer> members)
	{
		for (int i = 0; i < _teamPeers.Count; i++)
		{
			if (_teamPeers[i] != null)
			{
				members.Add(_teamPeers[i].Peer);
			}
		}
	}

	public bool IsMember(int id)
	{
		return _teamPeers.Exists((MemberData iter) => iter != null && iter.Id == id);
	}

	public void MemberAction(Action<MemberData> handler)
	{
		for (int i = 0; i < _teamPeers.Count; i++)
		{
			handler(_teamPeers[i]);
		}
	}

	public void SetLeader(int leaderId)
	{
		_leaderId = leaderId;
	}

	public IEnumerable<int> GetItemIDs(int tab)
	{
		return _package.GetItemObjIDs(tab);
	}

	public void Store(Player player, ItemObject itemObj, int storageIndex)
	{
		if (itemObj == null || !player.Package.ExistID(itemObj))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		if (storageIndex != -1)
		{
			player.Package.RemoveItem(itemObj);
			ItemObject itemByIndex = _package.GetItemByIndex(storageIndex, itemObj.protoData.tabIndex);
			if (itemByIndex != null)
			{
				_package.RemoveItem(itemByIndex);
				player.Package.AddItem(itemByIndex);
			}
			_package.AddItem(itemObj, storageIndex);
		}
		else
		{
			if (_package.GetEmptyGridCount(itemObj.protoData.tabIndex) == 0)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("Not enough space");
				}
				return;
			}
			player.Package.RemoveItem(itemObj);
			_package.AddItem(itemObj, storageIndex);
		}
		player.SyncItem(itemObj);
		player.SyncPackageIndex();
		IEnumerable<int> itemObjIDs = _package.GetItemObjIDs(itemObj.protoData.tabIndex);
		player.SyncGroupData(EPacketType.PT_InGame_PublicStorageIndex, itemObj.protoData.tabIndex, itemObjIDs.ToArray());
		player.SyncGroupData(EPacketType.PT_InGame_PublicStorageStore, player.roleName, itemObj.instanceId);
	}

	public void Change(Player player, ItemObject itemSrc, int srcIndex, int destIndex)
	{
		ItemObject itemByIndex = _package.GetItemByIndex(destIndex, itemSrc.protoData.tabIndex);
		if (itemByIndex != null)
		{
			_package.RemoveItem(itemByIndex);
			_package.RemoveItem(itemSrc);
			_package.AddItem(itemSrc, destIndex);
			_package.AddItem(itemByIndex, srcIndex);
		}
		else
		{
			_package.RemoveItem(itemSrc);
			_package.AddItem(itemSrc, destIndex);
		}
		IEnumerable<int> itemObjIDs = _package.GetItemObjIDs(itemSrc.protoData.tabIndex);
		player.SyncGroupData(EPacketType.PT_InGame_PublicStorageIndex, itemSrc.protoData.tabIndex, itemObjIDs.ToArray());
	}

	public void Fetch(Player player, ItemObject itemObj, int packageIndex)
	{
		if (null == player)
		{
			return;
		}
		if (itemObj == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
		}
		else if (!_package.ExistID(itemObj))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("player[{0}] does not have this item[{1}].", player.roleName, itemObj.instanceId);
			}
		}
		else if (packageIndex != -1)
		{
			_package.RemoveItem(itemObj);
			ItemObject itemByIndex = player.Package.GetItemByIndex(packageIndex, itemObj.protoData);
			if (itemByIndex != null)
			{
				player.Package.RemoveItem(itemByIndex);
				_package.AddItem(itemByIndex);
			}
			player.Package.AddItem(itemObj, packageIndex);
			player.SyncPackageIndex();
			IEnumerable<int> itemObjIDs = _package.GetItemObjIDs(itemObj.protoData.tabIndex);
			player.SyncGroupData(EPacketType.PT_InGame_PublicStorageIndex, itemObj.protoData.tabIndex, itemObjIDs.ToArray());
			player.SyncGroupData(EPacketType.PT_InGame_PublicStorageFetch, player.roleName, itemObj.instanceId);
		}
		else if (player.Package.GetEmptyGridCount(itemObj.protoData) == 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("player[{0}] has not enough space.", player.roleName);
			}
		}
		else
		{
			player.Package.AddItem(itemObj);
			_package.RemoveItem(itemObj);
			player.SyncPackageIndex();
			IEnumerable<int> itemObjIDs2 = _package.GetItemObjIDs(itemObj.protoData.tabIndex);
			player.SyncGroupData(EPacketType.PT_InGame_PublicStorageIndex, itemObj.protoData.tabIndex, itemObjIDs2.ToArray());
			player.SyncGroupData(EPacketType.PT_InGame_PublicStorageFetch, player.roleName, itemObj.instanceId);
		}
	}

	public void Delete(Player player, ItemObject itemObj)
	{
		if (null == player)
		{
			return;
		}
		if (itemObj == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
			return;
		}
		if (!_package.ExistID(itemObj))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("player[{0}] does not have this item[{1}].", player.roleName, itemObj.instanceId);
			}
			return;
		}
		_package.DeleteItem(itemObj);
		IEnumerable<int> itemObjIDs = _package.GetItemObjIDs(itemObj.protoData.tabIndex);
		player.SyncGroupData(EPacketType.PT_InGame_PublicStorageIndex, itemObj.protoData.tabIndex, itemObjIDs.ToArray());
		player.SyncGroupData(EPacketType.PT_InGame_PublicStorageDelete, player.roleName, itemObj.instanceId);
	}

	public void Split(Player player, ItemObject itemObj, int num)
	{
		if (num == 0 || null == player)
		{
			return;
		}
		if (itemObj == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Invalid item.");
			}
		}
		else
		{
			if (itemObj.stackCount <= num)
			{
				return;
			}
			if (!_package.ExistID(itemObj))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("player[{0}] does not have this item[{1}].", player.roleName, itemObj.instanceId);
				}
				return;
			}
			int emptyIndex = _package.GetEmptyIndex(itemObj.protoData.tabIndex);
			if (emptyIndex == -1)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarningFormat("player[{0}] has not enough space.", player.roleName);
				}
				return;
			}
			ItemObject itemObject = ItemManager.CreateFromItem(itemObj.protoId, num, itemObj);
			_package.AddItem(itemObject);
			itemObj.CountDown(num);
			player.SyncItemList(new ItemObject[2] { itemObject, itemObj });
			IEnumerable<int> itemObjIDs = _package.GetItemObjIDs(itemObj.protoData.tabIndex);
			player.SyncGroupData(EPacketType.PT_InGame_PublicStorageIndex, itemObj.protoData.tabIndex, itemObjIDs.ToArray());
			player.SyncGroupData(EPacketType.PT_InGame_PublicStorageSplit, player.roleName);
		}
	}

	public void Sort(Player player, int tab)
	{
		ItemObject[] array = _package.Sort(tab);
		ChannelNetwork.SyncTeamData(player.WorldId, player.TeamId, EPacketType.PT_InGame_ItemObjectList, array, false);
		IEnumerable<int> itemObjIDs = _package.GetItemObjIDs(tab);
		player.SyncGroupData(EPacketType.PT_InGame_PublicStorageIndex, tab, itemObjIDs.ToArray());
	}
}
