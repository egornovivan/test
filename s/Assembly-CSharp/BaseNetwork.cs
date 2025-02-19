using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CustomData;
using Mono.Data.SqliteClient;
using PETools;
using uLink;
using UnityEngine;

public class BaseNetwork : GroupNetInterface
{
	public delegate bool ActionBaseNetId(int id);

	protected static Dictionary<uLink.NetworkPlayer, int> BaseNetPeerDic = new Dictionary<uLink.NetworkPlayer, int>();

	protected static Dictionary<int, uLink.NetworkPlayer> BaseNetIdDic = new Dictionary<int, uLink.NetworkPlayer>();

	protected static Dictionary<int, BaseNetwork> BaseNetDic = new Dictionary<int, BaseNetwork>();

	protected static List<BaseNetwork> BaseNetList = new List<BaseNetwork>();

	protected static Dictionary<int, PlayerDesc> PlayerDescDic = new Dictionary<int, PlayerDesc>();

	protected RoleInfo _role;

	protected bool _hasRecord;

	public ENetworkState netState { get; private set; }

	public string account => _role.account;

	public string roleName => _role.name;

	public ulong steamId => _role.steamId;

	public byte sex => _role.sex;

	public int curTeamId { get; private set; }

	protected static event Action OnStartEventHandler;

	public static BaseNetwork GetBaseNetwork(int id)
	{
		if (BaseNetDic.ContainsKey(id))
		{
			return BaseNetDic[id];
		}
		return null;
	}

	public static BaseNetwork GetBaseNetwork(ulong steamId)
	{
		foreach (KeyValuePair<int, BaseNetwork> item in BaseNetDic)
		{
			if (item.Value.steamId == steamId)
			{
				return item.Value;
			}
		}
		return null;
	}

	public static int GetBaseIdByAccount(ulong steamId)
	{
		foreach (KeyValuePair<int, BaseNetwork> item in BaseNetDic)
		{
			if (item.Value.steamId == steamId)
			{
				return item.Value.Id;
			}
		}
		return -1;
	}

	private int AddPlayerDesc(int id)
	{
		List<PlayerDesc> humanPlayers = ForceSetting.humanPlayers;
		foreach (PlayerDesc item in humanPlayers)
		{
			if (!PlayerDescDic.ContainsValue(item))
			{
				PlayerDescDic[id] = item;
				return item.Force;
			}
		}
		return -1;
	}

	private int AddPlayerDesc(int id, int descId)
	{
		List<PlayerDesc> humanPlayers = ForceSetting.humanPlayers;
		foreach (PlayerDesc item in humanPlayers)
		{
			if (descId == item.ID)
			{
				if (PlayerDescDic.ContainsValue(item))
				{
					return -1;
				}
				PlayerDescDic[id] = item;
				return item.Force;
			}
		}
		return -1;
	}

	public static void DelPlayerDesc(int id)
	{
		if (PlayerDescDic.ContainsKey(id))
		{
			PlayerDescDic.Remove(id);
		}
	}

	public static bool IsFull()
	{
		if (!uLinkNetwork.HasServerStarted && ForceSetting.humanPlayers.Count != 0)
		{
			List<PlayerDesc> humanPlayers = ForceSetting.humanPlayers;
			foreach (PlayerDesc item in humanPlayers)
			{
				if (!PlayerDescDic.ContainsValue(item))
				{
					return false;
				}
			}
			return true;
		}
		return -1 == GroupNetwork.CanJoin();
	}

	public static bool ForeachActionAll(ActionBaseNetId handler)
	{
		foreach (BaseNetwork baseNet in BaseNetList)
		{
			if (!handler(baseNet.Id))
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("BaseNetMgr");
		_role = info.networkView.initialData.Read<RoleInfo>(new object[0]);
		_id = _role.roleID + 3000000;
		base.name = roleName + "_" + base.Id;
		int teamId = (curTeamId = -1);
		_worldId = (_teamId = teamId);
		netState = ENetworkState.Null;
		Load();
		if (ServerConfig.IsCustom)
		{
			BaseNetwork.OnStartEventHandler = (Action)Delegate.Combine(BaseNetwork.OnStartEventHandler, new Action(SyncStartEvent));
			int num2 = AddPlayerDesc(base.Id);
			if (num2 == -1)
			{
				curTeamId = GroupNetwork.AddToTeam(base.Id, base.OwnerView.owner);
			}
			else
			{
				curTeamId = GroupNetwork.AddToTeam(base.Id, base.OwnerView.owner, num2);
			}
		}
		else if (!ServerConfig.IsSurvive)
		{
			curTeamId = GroupNetwork.AddToTeam(base.Id, base.OwnerView.owner, base.TeamId);
			if (curTeamId == -1)
			{
				curTeamId = GroupNetwork.AddToTeam(base.Id, base.OwnerView.owner);
			}
		}
		else if (!_hasRecord)
		{
			curTeamId = GroupNetwork.NewTeam(base.Id, base.OwnerView.owner);
		}
		else
		{
			curTeamId = _teamId;
			GroupNetwork.NewSurviveTeam(curTeamId, base.Id, base.OwnerView.owner);
		}
		if (curTeamId == -1)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Invalid team id allocated.");
			}
			NetInterface.CloseConnection(base.OwnerView.owner);
		}
		_worldId = GameWorld.LoginWorld(base.WorldId);
	}

	protected override void OnPEStart()
	{
		if (!AddToNetMgr())
		{
			base.enabled = false;
			return;
		}
		base.OnPEStart();
		BindAction(EPacketType.PT_InRoom_InitData, RPC_C2S_InitData);
		BindAction(EPacketType.PT_InRoom_TeamChange, RPC_C2S_TeamChange);
		BindAction(EPacketType.PT_InRoom_StatusChange, RPC_C2S_StatusChanged);
		BindAction(EPacketType.PT_InGame_PlayerLogin, RPC_C2S_PlayerLogin);
		BindAction(EPacketType.PT_InRoom_Message, RPC_Message);
		BindAction(EPacketType.PT_InRoom_KickPlayer, RPC_C2S_KickPlayer);
		StartCoroutine(CheckPing());
	}

	protected override void OnPEDestroy()
	{
		if (ServerConfig.IsCustom)
		{
			BaseNetwork.OnStartEventHandler = (Action)Delegate.Remove(BaseNetwork.OnStartEventHandler, new Action(SyncStartEvent));
		}
		base.OnPEDestroy();
		GroupNetwork.RemoveFromTeam(curTeamId, base.Id);
		GroupNetwork.RemoveFromTeam(base.TeamId, base.Id);
		SteamWorks.RemoveRegisteredPlayer(base.Id);
		DelPlayerDesc(base.Id);
		DelFromNetMgr();
	}

	private bool AddToNetMgr()
	{
		if (BaseNetDic.ContainsKey(base.Id))
		{
			NetInterface.NetDestroy(BaseNetDic[base.Id]);
			NetInterface.CloseConnection(base.OwnerView.owner);
			return false;
		}
		BaseNetList.Add(this);
		BaseNetIdDic.Add(base.Id, base.OwnerView.owner);
		BaseNetPeerDic.Add(base.OwnerView.owner, base.Id);
		BaseNetDic.Add(base.Id, this);
		return true;
	}

	private void DelFromNetMgr()
	{
		if (BaseNetDic.ContainsKey(base.Id))
		{
			if (BaseNetIdDic.ContainsKey(base.Id))
			{
				BaseNetPeerDic.Remove(BaseNetIdDic[base.Id]);
				BaseNetIdDic.Remove(base.Id);
			}
			BaseNetList.Remove(this);
			BaseNetDic.Remove(base.Id);
		}
	}

	public static int GetBaseNetId(uLink.NetworkPlayer peer)
	{
		if (BaseNetPeerDic.ContainsKey(peer))
		{
			return BaseNetPeerDic[peer];
		}
		return -1;
	}

	public static uLink.NetworkPlayer GetBaseNetPeer(int id)
	{
		if (BaseNetIdDic.ContainsKey(id))
		{
			return BaseNetIdDic[id];
		}
		return uLink.NetworkPlayer.unassigned;
	}

	public static BaseNetwork GetBaseNet(int id)
	{
		if (!BaseNetDic.ContainsKey(id))
		{
			return null;
		}
		return BaseNetDic[id];
	}

	public static BaseNetwork GetBaseNet(uLink.NetworkPlayer peer)
	{
		int baseNetId = GetBaseNetId(peer);
		if (baseNetId == -1 || !BaseNetDic.ContainsKey(baseNetId))
		{
			return null;
		}
		return BaseNetDic[baseNetId];
	}

	private IEnumerator CheckPing()
	{
		yield return null;
		while (true)
		{
			yield return new WaitForSeconds(5f);
			RPCOthers(EPacketType.PT_InRoom_Ping, base.OwnerView.owner.averagePing);
		}
	}

	private void OnStartEvent()
	{
		if (BaseNetwork.OnStartEventHandler != null)
		{
			BaseNetwork.OnStartEventHandler();
		}
	}

	private void SyncStartEvent()
	{
		if (!_hasRecord)
		{
			base.transform.position = GetCustomModePos();
		}
		else if (base.WorldId != _worldId)
		{
			base.transform.position = GetCustomModePos();
		}
		SyncStartLogin();
		BaseNetwork.OnStartEventHandler = (Action)Delegate.Remove(BaseNetwork.OnStartEventHandler, new Action(SyncStartEvent));
	}

	private void SyncStartLogin()
	{
		SyncTree();
		SyncGrass();
		RPCOwner(EPacketType.PT_InRoom_StartLogin, base.transform.position, base.TeamId, _hasRecord, netState);
	}

	public void SyncGrass()
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			byte[] data;
			int num = gameWorld.GenGrassData(out data);
			if (num != 0)
			{
				RPCOwner(EPacketType.PT_InGame_GrassInfo, num, data);
			}
		}
	}

	public void SyncTree()
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			byte[] data;
			int num = gameWorld.GenTreeData(out data);
			if (num != 0)
			{
				RPCOwner(EPacketType.PT_InGame_TreeInfo, num, data);
			}
		}
	}

	private void AdjustPos(Vector3 pos)
	{
		if (!_hasRecord)
		{
			if (!ServerConfig.IsCustom)
			{
				base.transform.position = pos;
			}
		}
		else if (ServerConfig.IsVS && base.TeamId != curTeamId)
		{
			base.transform.position = pos;
		}
	}

	private Vector3 GetCustomModePos()
	{
		if (PlayerDescDic.ContainsKey(base.Id))
		{
			return PlayerDescDic[base.Id].StartLocation;
		}
		return ForceSetting.GetForcePos(curTeamId);
	}

	private void LoadComplete(SqliteDataReader r)
	{
		if (r.Read())
		{
			_worldId = r.GetInt32(r.GetOrdinal("worldid"));
			_teamId = r.GetInt32(r.GetOrdinal("teamid"));
			byte[] buff = (byte[])r.GetValue(r.GetOrdinal("pos"));
			Serialize.Import(buff, delegate(BinaryReader outr)
			{
				BufferHelper.ReadVector3(outr, out var _value);
				CommonHelper.AdjustPos(ref _value);
				base.transform.position = _value;
			});
			_hasRecord = true;
		}
	}

	public int GetPlayerDescId()
	{
		if (PlayerDescDic.ContainsKey(base.Id))
		{
			return PlayerDescDic[base.Id].ID;
		}
		return -1;
	}

	public void Load()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT worldid,teamid,pos FROM player WHERE roleid=@roleid AND steamid=@steamid;");
			pEDbOp.BindParam("@roleid", base.Id);
			pEDbOp.BindParam("@steamid", (long)steamId);
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private void RPC_C2S_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<bool>(new object[0]) && !ServerConfig.IsStory && !ServerConfig.IsCustom)
		{
			RandomTownManager.SyncRandomTown(info.sender, this);
		}
		int playerDescId = GetPlayerDescId();
		int colorIndex = uLinkNetwork.GetColorIndex(info.sender);
		RPCPeer(info.sender, EPacketType.PT_InRoom_InitData, netState, curTeamId, colorIndex, playerDescId);
	}

	private void RPC_C2S_TeamChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (num == -1 || curTeamId == num)
		{
			return;
		}
		if (ServerConfig.IsCustom)
		{
			if (num2 != -1)
			{
				int playerDescId = GetPlayerDescId();
				if (num2 == playerDescId)
				{
					return;
				}
				num = AddPlayerDesc(base.Id, num2);
				if (num == -1)
				{
					return;
				}
			}
			num = GroupNetwork.AddToTeam(base.Id, info.sender, num);
			if (num != -1)
			{
				GroupNetwork.RemoveFromTeam(curTeamId, base.Id);
				curTeamId = num;
				RPCOthers(EPacketType.PT_InRoom_TeamChange, num, num2);
			}
		}
		else if (ServerConfig.IsVS)
		{
			num = GroupNetwork.AddToTeam(base.Id, info.sender, num);
			if (num != -1)
			{
				GroupNetwork.RemoveFromTeam(curTeamId, base.Id);
				curTeamId = num;
				RPCOthers(EPacketType.PT_InRoom_TeamChange, num, -1);
			}
		}
	}

	private void RPC_C2S_StatusChanged(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ENetworkState eNetworkState = stream.Read<ENetworkState>(new object[0]);
		if (!SteamWorks.DispatchComplete(base.Id))
		{
			return;
		}
		if (ServerConfig.IsCustom)
		{
			if (uLinkNetwork.HasServerPrepared)
			{
				netState = eNetworkState;
				RPCOthers(EPacketType.PT_InRoom_StatusChange, eNetworkState);
				if (ForceSetting.humanPlayers.Count != PlayerDescDic.Count)
				{
					return;
				}
				foreach (KeyValuePair<int, PlayerDesc> item in PlayerDescDic)
				{
					BaseNetwork baseNetwork = GetBaseNetwork(item.Key);
					if (null != baseNetwork && baseNetwork.netState == ENetworkState.Null)
					{
						return;
					}
				}
				OnStartEvent();
				uLinkNetwork.SetServerState(EServerStatus.Gameing);
			}
			else
			{
				if (netState != 0)
				{
					return;
				}
				netState = eNetworkState;
				RPCOthers(EPacketType.PT_InRoom_StatusChange, eNetworkState);
				SyncStartLogin();
			}
		}
		else
		{
			if (netState != 0)
			{
				return;
			}
			netState = eNetworkState;
			RPCOthers(EPacketType.PT_InRoom_StatusChange, eNetworkState);
			SyncStartLogin();
			if (uLinkNetwork.HasServerPrepared)
			{
				uLinkNetwork.SetServerState(EServerStatus.Gameing);
			}
		}
		netState = ENetworkState.Loading;
		uLobbyNetwork.UpdateServerInfo();
	}

	private void RPC_C2S_PlayerLogin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		AdjustPos(pos);
		_teamId = curTeamId;
		netState = ENetworkState.Gameing;
		NetInterface.Instantiate(base.OwnerView.owner, PrefabManager.Self.PlayerProxy, PrefabManager.Self.PlayerOwner, PrefabManager.Self.PlayerCreator, base.transform.position, Quaternion.identity, 101, base.Id, base.WorldId, base.TeamId);
	}

	private void RPC_Message(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		RPCOthers(EPacketType.PT_InRoom_Message, text);
	}

	private void RPC_C2S_KickPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (ServerConfig.MasterId == base.Id && num != base.Id)
		{
			RPCOthers(EPacketType.PT_InRoom_KickPlayer, num);
		}
	}
}
