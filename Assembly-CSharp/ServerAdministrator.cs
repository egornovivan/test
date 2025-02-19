using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ServerAdministrator : MonoBehaviour
{
	public static bool updataFlag = false;

	private static ServerAdministrator _instance;

	public static Dictionary<string, ulong> RoleAdmin = new Dictionary<string, ulong>();

	public static List<UserAdmin> UserAdminList = new List<UserAdmin>();

	private static List<int> LockedArea = new List<int>();

	private static bool _allowJoin;

	private static bool _allowModify;

	public static Action<UserAdmin> PrivilegesChangedEvent;

	public static Action<int, bool> LockAreaChangedEvent;

	public static Action<bool> PlayerBanChangedEvent;

	public static Action<bool> BuildLockChangedEvent;

	public static ServerAdministrator Instance => _instance;

	public static bool AllowJoin
	{
		get
		{
			return _allowJoin;
		}
		set
		{
			_allowJoin = value;
		}
	}

	public static bool AllowModify
	{
		get
		{
			return _allowModify;
		}
		set
		{
			_allowModify = value;
		}
	}

	public IEnumerable<UserAdmin> BlackRoles => UserAdminList.Where((UserAdmin iter) => IsBlack(iter.Id));

	private void Awake()
	{
		_instance = this;
		init();
	}

	public void init()
	{
		UserAdminList.Clear();
		LockedArea.Clear();
		_allowJoin = true;
		_allowModify = true;
	}

	public static bool IsAdmin(int id)
	{
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.AdminRole));
	}

	public static bool IsAssistant(int id)
	{
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.AssistRole));
	}

	public static bool IsBlack(int id)
	{
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.BlackRole));
	}

	public static bool IsBuildLock(int id)
	{
		if (!AllowModify)
		{
			return true;
		}
		return UserAdminList.Exists((UserAdmin iter) => iter.Id == id && iter.HasPrivileges(AdminMask.BuildLock));
	}

	public static bool IsLockedArea(int areaIndex)
	{
		return LockedArea.Contains(areaIndex);
	}

	public static void AddBlacklist(int playerId)
	{
		UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == playerId);
		if (userAdmin != null)
		{
			userAdmin.RemovePrivileges(AdminMask.AssistRole);
			userAdmin.AddPrivileges(AdminMask.BlackRole);
			OnPrivilegesChanged(userAdmin);
		}
	}

	public static void DeleteBlacklist(int id)
	{
		if (IsBlack(id))
		{
			UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == id);
			if (userAdmin != null)
			{
				userAdmin.RemovePrivileges(AdminMask.BlackRole);
				OnPrivilegesChanged(userAdmin);
			}
		}
	}

	public static void ClearBlacklist()
	{
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			DeleteBlacklist(userAdmin.Id);
		}
	}

	public static void AddAssistant(int playerId)
	{
		UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == playerId);
		if (userAdmin != null)
		{
			userAdmin.RemovePrivileges(AdminMask.BlackRole);
			userAdmin.AddPrivileges(AdminMask.AssistRole);
			OnPrivilegesChanged(userAdmin);
		}
	}

	public static void DeleteAssistant(int id)
	{
		if (IsAssistant(id))
		{
			UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == id);
			if (userAdmin != null)
			{
				userAdmin.RemovePrivileges(AdminMask.AssistRole);
				OnPrivilegesChanged(userAdmin);
			}
		}
	}

	public static void ClearAssistant()
	{
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			DeleteAssistant(userAdmin.Id);
		}
	}

	public static void BuildLock(int playerId)
	{
		UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == playerId);
		if (userAdmin != null)
		{
			userAdmin.AddPrivileges(AdminMask.BuildLock);
			OnPrivilegesChanged(userAdmin);
		}
	}

	public static void BuildUnLock(int id)
	{
		if (IsBuildLock(id))
		{
			UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == id);
			if (userAdmin != null)
			{
				userAdmin.RemovePrivileges(AdminMask.BuildLock);
				OnPrivilegesChanged(userAdmin);
			}
		}
	}

	public static void ClearBuildLock()
	{
		foreach (UserAdmin userAdmin in UserAdminList)
		{
			BuildUnLock(userAdmin.Id);
		}
	}

	public static void LockArea(int index)
	{
		if (!LockedArea.Contains(index))
		{
			LockedArea.Add(index);
			OnLockAreaChanged(index, isLock: true);
		}
	}

	public static void UnLockArea(int index)
	{
		LockedArea.Remove(index);
		OnLockAreaChanged(index, isLock: false);
	}

	public static bool SetBuildChunk(bool flag)
	{
		_allowModify = flag;
		OnBuildLockChanged(flag);
		return true;
	}

	public static bool SetJoinGame(bool flag)
	{
		_allowJoin = flag;
		OnPlayerBanChanged(flag);
		return true;
	}

	private static void OnPrivilegesChanged(UserAdmin ua)
	{
		if (PrivilegesChangedEvent != null)
		{
			PrivilegesChangedEvent(ua);
		}
	}

	private static void OnLockAreaChanged(int index, bool isLock)
	{
		if (LockAreaChangedEvent != null)
		{
			LockAreaChangedEvent(index, isLock);
		}
	}

	private static void OnBuildLockChanged(bool flag)
	{
		if (BuildLockChangedEvent != null)
		{
			BuildLockChangedEvent(flag);
		}
	}

	private static void OnPlayerBanChanged(bool flag)
	{
		if (PlayerBanChangedEvent != null)
		{
			PlayerBanChangedEvent(flag);
		}
	}

	public static void DeserializeAdminData(byte[] data)
	{
		Instance.init();
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		_allowJoin = BufferHelper.ReadBoolean(reader);
		_allowModify = BufferHelper.ReadBoolean(reader);
		OnPlayerBanChanged(_allowJoin);
		OnBuildLockChanged(_allowModify);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			int roleId = BufferHelper.ReadInt32(reader);
			string roleName = BufferHelper.ReadString(reader);
			ulong privileges = BufferHelper.ReadUInt64(reader);
			UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == roleId);
			if (userAdmin == null)
			{
				userAdmin = new UserAdmin(roleId, roleName, privileges);
				UserAdminList.Add(userAdmin);
			}
			else
			{
				userAdmin.SetPrivileges(privileges);
			}
			OnPrivilegesChanged(userAdmin);
		}
		num = BufferHelper.ReadInt32(reader);
		for (int j = 0; j < num; j++)
		{
			int num2 = BufferHelper.ReadInt32(reader);
			if (!LockedArea.Contains(num2))
			{
				LockedArea.Add(num2);
				OnLockAreaChanged(num2, isLock: true);
			}
		}
	}

	public static void ProxyPlayerAdmin(PlayerNetwork player)
	{
		if (!(null == player))
		{
			UserAdmin userAdmin = UserAdminList.Find((UserAdmin iter) => iter.Id == player.Id);
			if (userAdmin == null)
			{
				userAdmin = new UserAdmin(player.Id, player.RoleName, 0uL);
				UserAdminList.Add(userAdmin);
				OnPrivilegesChanged(userAdmin);
			}
		}
	}

	public static void RequestAddBlackList(int id)
	{
		if (PlayerNetwork.mainPlayerId != id && (IsAdmin(PlayerNetwork.mainPlayerId) || (IsAssistant(PlayerNetwork.mainPlayerId) && !IsAssistant(id))))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AddBlackList, id);
		}
	}

	public static void RequestDeleteBlackList(int id)
	{
		if (PlayerNetwork.mainPlayerId != id && IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_DelBlackList, id);
		}
	}

	public static void RequestClearBlackList()
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearBlackList);
		}
	}

	public static void RequestAddAssistants(int id)
	{
		if (PlayerNetwork.mainPlayerId != id && IsAdmin(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AddAssistant, id);
		}
	}

	public static void RequestDeleteAssistants(int id)
	{
		if (PlayerNetwork.mainPlayerId != id && IsAdmin(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_DelAssistant, id);
		}
	}

	public static void RequestClearAssistants()
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearAssistant);
		}
	}

	public static void RequestBuildLock(int id)
	{
		if (PlayerNetwork.mainPlayerId != id && IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_BuildLock, id);
		}
	}

	public static void RequestBuildUnLock(int id)
	{
		if (PlayerNetwork.mainPlayerId != id && IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_BuildUnLock, id);
		}
	}

	public static void RequestClearBuildLock()
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearBuildLock);
		}
	}

	public static void RequestClearVoxelData(int index)
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearVoxel, index);
		}
	}

	public static void RequestClearAllVoxelData()
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearAllVoxel);
		}
	}

	public static void RequestLockArea(int index)
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AreaLock, index);
		}
	}

	public static void RequestUnLockArea(int index)
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AreaUnLock, index);
		}
	}

	public static void RequestSetBuildChunk(bool flag)
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_BlockLock, flag);
		}
	}

	public static void RequestSetJoinGame(bool flag)
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_LoginBan, flag);
		}
	}

	public static void RequestKickPlayer()
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Kick);
		}
	}
}
