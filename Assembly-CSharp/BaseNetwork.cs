using System.Collections.Generic;
using System.IO;
using CustomData;
using Pathea;
using PETools;
using Steamworks;
using uLink;
using UnityEngine;

public class BaseNetwork : NetworkInterface
{
	private static Dictionary<int, RoomPlayerInfo> RolesInRoom = new Dictionary<int, RoomPlayerInfo>();

	private static Dictionary<int, BaseNetwork> BasePeers = new Dictionary<int, BaseNetwork>();

	private static BaseNetwork _mainPlayer;

	private static List<Vector3> delTreePos = new List<Vector3>();

	private static List<Vector3> delGrassPos = new List<Vector3>();

	private RoleInfo _role;

	private ENetworkState _networkState;

	private CSteamID _steamID;

	private int _colorIndex;

	private int _descId;

	private bool _useNewPos;

	public static PlayerDesc curPlayerDesc { get; protected set; }

	public static BaseNetwork MainPlayer => _mainPlayer;

	public string RoleName => Role.name;

	public byte Sex => Role.sex;

	public RoleInfo Role => _role;

	public ENetworkState NetworkState => _networkState;

	public CSteamID SteamID => _steamID;

	public int ColorIndex => _colorIndex;

	public int DescId => _descId;

	public bool UseNewPos => _useNewPos;

	public static BaseNetwork GetBaseNetwork(int id)
	{
		return (!BasePeers.ContainsKey(id)) ? null : BasePeers[id];
	}

	public static bool HasBaseNetwork(int id)
	{
		return BasePeers.ContainsKey(id);
	}

	public static Dictionary<int, BaseNetwork> GetBaseNetworkList()
	{
		return BasePeers;
	}

	private void AddBaseNetwork()
	{
		if (BasePeers.ContainsKey(base.Id))
		{
			BasePeers.Remove(base.Id);
		}
		BasePeers.Add(base.Id, this);
	}

	private void DelBaseNetwork()
	{
		if (BasePeers.ContainsKey(base.Id))
		{
			BasePeers.Remove(base.Id);
		}
	}

	public static bool IsInRoom()
	{
		if (RolesInRoom != null && RolesInRoom.Count > 0)
		{
			return true;
		}
		return false;
	}

	public static CSteamID? GetSteamID(string roleName)
	{
		foreach (KeyValuePair<int, BaseNetwork> basePeer in BasePeers)
		{
			if (basePeer.Value.RoleName == roleName)
			{
				return basePeer.Value.SteamID;
			}
		}
		return null;
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_role = info.networkView.initialData.Read<RoleInfo>(new object[0]);
		_networkState = ENetworkState.Null;
		_id = Role.roleID + 3000000;
		base.name = Role.name;
		AddBaseNetwork();
	}

	protected override void OnPEStart()
	{
		BindAction(EPacketType.PT_InRoom_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_InRoom_TeamChange, RPC_S2C_TeamChange);
		BindAction(EPacketType.PT_InRoom_StatusChange, RPC_S2C_RoomStatusChanged);
		BindAction(EPacketType.PT_InRoom_StartLogin, RPC_S2C_StartLogin);
		BindAction(EPacketType.PT_InRoom_Message, RPC_Message);
		BindAction(EPacketType.PT_InRoom_Ping, RPC_Ping);
		BindAction(EPacketType.PT_InRoom_KickPlayer, RPC_S2C_KickPlayer);
		BindAction(EPacketType.PT_Common_RandomTownData, RPC_S2C_RandomTownData);
		BindAction(EPacketType.PT_InGame_GrassInfo, RPC_S2C_GrassInfo);
		BindAction(EPacketType.PT_InGame_TreeInfo, RPC_S2C_TreeInfo);
		if (base.IsOwner)
		{
			if (PeGameMgr.IsMultiCustom || PeGameMgr.IsMultiStory)
			{
				LSubTerrSL.OnLSubTerrSLInitEvent += CacheDelTree;
			}
			else
			{
				RSubTerrSL.OnRSubTerrSLInitEvent += CacheDelTree;
			}
			GrassDataSL.OnGrassDataInitEvent += CacheDelGrass;
			_mainPlayer = this;
			RequestUGC();
		}
		RequestInitData();
	}

	protected override void OnPEDestroy()
	{
		DelBaseNetwork();
		if (object.ReferenceEquals(this, MainPlayer))
		{
			if (PeGameMgr.IsMultiCustom || PeGameMgr.IsMultiStory)
			{
				LSubTerrSL.OnLSubTerrSLInitEvent -= CacheDelTree;
			}
			else
			{
				RSubTerrSL.OnRSubTerrSLInitEvent -= CacheDelTree;
			}
			GrassDataSL.OnGrassDataInitEvent -= CacheDelGrass;
		}
		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
		{
			RoomGui_N.RemoveRoomPlayerByNet(base.Id);
		}
	}

	public bool UseNewPosition(bool hasRecord, int recordTeamId)
	{
		if (!PeGameMgr.IsCustom)
		{
			if (PeGameMgr.IsVS)
			{
				return !hasRecord || recordTeamId != base.TeamId;
			}
			return !hasRecord;
		}
		return false;
	}

	private void CacheDelTree()
	{
		if (delTreePos.Count != 0)
		{
			DigTerrainManager.CacheDeleteTree(delTreePos);
		}
	}

	private void CacheDelGrass()
	{
		if (delGrassPos.Count != 0)
		{
			DigTerrainManager.CacheDeleteGrass(delGrassPos);
		}
	}

	public void RequestPlayerLogin(Vector3 pos)
	{
		RPCServer(EPacketType.PT_InGame_PlayerLogin, pos);
		PlayerNetwork.ResetTerrainState();
	}

	public void RequestUGC()
	{
		NetworkManager.SyncServer(EPacketType.PT_Common_RequestUGC);
	}

	public void RequestInitData()
	{
		RPCServer(EPacketType.PT_InRoom_InitData, base.IsOwner);
	}

	public void RequestChangeTeam(int forceId, int playerId)
	{
		RPCServer(EPacketType.PT_InRoom_TeamChange, forceId, playerId);
	}

	public void RequestChangeStatus(ENetworkState state)
	{
		RPCServer(EPacketType.PT_InRoom_StatusChange, (int)state);
	}

	public void SendMsg(string msg)
	{
		RPCServer(EPacketType.PT_InRoom_Message, msg);
	}

	public void KickPlayer(int playerId)
	{
		RPCServer(EPacketType.PT_InRoom_KickPlayer, playerId);
	}

	private void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_networkState = stream.Read<ENetworkState>(new object[0]);
		_teamId = stream.Read<int>(new object[0]);
		_colorIndex = stream.Read<int>(new object[0]);
		_descId = stream.Read<int>(new object[0]);
		RoomPlayerInfo roomPlayerInfo = new RoomPlayerInfo();
		roomPlayerInfo.mId = base.Id;
		roomPlayerInfo.mPlayerInfo.mName = Role.name;
		roomPlayerInfo.mPlayerInfo.mSex = Role.sex;
		roomPlayerInfo.mPlayerInfo.mLevel = Role.level;
		roomPlayerInfo.mPlayerInfo.mWinnRate = Role.winrate;
		roomPlayerInfo.mFocreID = ((!PeGameMgr.IsSurvive || PeGameMgr.IsCustom) ? base.TeamId : (-1));
		roomPlayerInfo.mRoleID = _descId;
		roomPlayerInfo.mState = (int)NetworkState;
		RoomGui_N.InitRoomPlayerByNet(roomPlayerInfo);
	}

	private void RPC_S2C_StartLogin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		base.transform.position = vector;
		base._pos = vector;
		int recordTeamId = stream.Read<int>(new object[0]);
		bool hasRecord = stream.Read<bool>(new object[0]);
		_networkState = stream.Read<ENetworkState>(new object[0]);
		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
		{
			RoomGui_N.ChangePlayerStateByNet(base.Id, (int)NetworkState);
		}
		_useNewPos = UseNewPosition(hasRecord, recordTeamId);
		ChunkManager.Clear();
		if (null != PeSceneCtrl.Instance)
		{
			PeSceneCtrl.Instance.GotoGameSence();
		}
		if (PeGameMgr.IsCustom)
		{
			int id = ((DescId != -1) ? DescId : base.Id);
			curPlayerDesc = ForceSetting.AddPlayer(id, base.TeamId, EPlayerType.Human, RoleName);
		}
	}

	private void RPC_S2C_TeamChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int descId = stream.Read<int>(new object[0]);
		if (PeGameMgr.IsMultiCustom || base.TeamId != num)
		{
			_teamId = num;
			_descId = descId;
			RoomPlayerInfo roomPlayerInfo;
			if (!RolesInRoom.ContainsKey(base.Id))
			{
				roomPlayerInfo = new RoomPlayerInfo();
				roomPlayerInfo.mId = base.Id;
				roomPlayerInfo.mPlayerInfo.mName = Role.name;
				roomPlayerInfo.mPlayerInfo.mSex = Role.sex;
				RolesInRoom[base.Id] = roomPlayerInfo;
			}
			else
			{
				roomPlayerInfo = RolesInRoom[base.Id];
			}
			roomPlayerInfo.mPlayerInfo.mLevel = Role.level;
			roomPlayerInfo.mPlayerInfo.mWinnRate = Role.winrate;
			roomPlayerInfo.mFocreID = ((!PeGameMgr.IsSurvive || PeGameMgr.IsCustom) ? base.TeamId : (-1));
			roomPlayerInfo.mRoleID = _descId;
			roomPlayerInfo.mState = (int)NetworkState;
			_steamID = SteamMgr.steamId;
			if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
			{
				RoomGui_N.ChangeRoomPlayerByNet(roomPlayerInfo);
			}
		}
	}

	private void RPC_S2C_RoomStatusChanged(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_networkState = stream.Read<ENetworkState>(new object[0]);
		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
		{
			RoomGui_N.ChangePlayerStateByNet(base.Id, (int)NetworkState);
		}
		if (base.IsOwner)
		{
			delGrassPos.Clear();
			delTreePos.Clear();
		}
	}

	private void RPC_Message(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string msg = stream.Read<string>(new object[0]);
		RoomGui_N.GetNewMsgByNet(RoleName, msg);
	}

	private void RPC_Ping(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int delay = stream.Read<int>(new object[0]);
		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
		{
			RoomGui_N.ChangePlayerDelayByNet(base.Id, delay);
		}
	}

	private void RPC_S2C_KickPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int playerInstanceId = stream.Read<int>(new object[0]);
		RoomGui_N.KickPlayerByNet(playerInstanceId);
	}

	private void RPC_S2C_RandomTownData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int[]>(out var value);
		StartCoroutine(VArtifactTownManager.WaitForArtifactTown(value));
	}

	private void RPC_S2C_GrassInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int count = stream.Read<int>(new object[0]);
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			for (int i = 0; i < count; i++)
			{
				BufferHelper.ReadVector3(r, out var _value);
				delGrassPos.Add(_value);
			}
		});
	}

	private void RPC_S2C_TreeInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int count = stream.Read<int>(new object[0]);
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			for (int i = 0; i < count; i++)
			{
				BufferHelper.ReadVector3(r, out var _value);
				delTreePos.Add(_value);
			}
		});
	}
}
