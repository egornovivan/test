using System.Collections.Generic;
using System.Text;
using CustomData;
using Railway;
using uLink;
using UnityEngine;

public class uLinkNetwork : UnityEngine.MonoBehaviour
{
	private static EServerStatus serverStatus = EServerStatus.None;

	private static Dictionary<ulong, uLink.NetworkPlayer> LoginIds = new Dictionary<ulong, uLink.NetworkPlayer>();

	private static Dictionary<uLink.NetworkPlayer, ulong> LoginPeers = new Dictionary<uLink.NetworkPlayer, ulong>();

	private static Dictionary<uLink.NetworkPlayer, int> ColorIndex = new Dictionary<uLink.NetworkPlayer, int>();

	public static EServerStatus ServerStatus => serverStatus;

	public static bool HasServerPrepared => (serverStatus & EServerStatus.Prepared) == EServerStatus.Prepared;

	public static bool HasServerInitialized => (serverStatus & EServerStatus.Initialized) == EServerStatus.Initialized;

	public static bool HasServerStarted => (serverStatus & EServerStatus.Gameing) == EServerStatus.Gameing;

	private void Start()
	{
		if (HasServerPrepared)
		{
			uLink.Network.isAuthoritativeServer = true;
			uLink.Network.config.timeoutDelay = 180f;
			uLink.Network.maxManualViewIDs = 10;
			uLink.Network.minimumAllocatableViewIDs = 50000;
			uLink.Network.minimumUsedViewIDs = 10;
			uLink.Network.sendRate = 10f;
			uLink.Network.privateKey = new PrivateKey("<RSAKeyValue><Modulus>oipCMkge/+BwcenDk3XdJMeBXwW+V6WVEtP/U7YKoFfJokNbqffWW65zUCSCUCJyalnqtKen5fbQiOtFyNwsuxdksUTiRDTSwW/gMOtyZ84YAED+W8OOmLRCWtnt/YBqxIVnKUVX2oT3aQ/pGOxmtZS7krThKyuO2RwDAWoETDM=</Modulus><Exponent>EQ==</Exponent><P>3gJwo5guBEA+Ed3p/NjnHsRJhK/O3cJFOO0sOBFB7YQk3Xk4DiBr7w3DYnbsynmgyQ1gVuTG301EvF8osQZhDQ==</P><Q>uv4+MaoSMGuJTNzRwGj0nHh7y/GqHUpRL76ezPtl7opzpxM/Ulz4ywxydLWJYVvEs3NNa0THtKmQW1Mt7j2SPw==</Q><DP>ttTVO25EA4AzHcXPwSsYr+zxQBhQAetIEMNRl5W90qkPTPpqZfyVHziCyY8dW3M5HgsEC1L+IU6xE5mpCkF9GQ==</DP><DQ>Qf9hPrR+4+m3/QKkYgbs6+5Jz4J4RpKzPgcK3u9RJwOwWRXaHRHBOJr7OEASXpjcAxmi2pC+9HgU1PAuVBW7JQ==</DQ><InverseQ>VGV2xXk5hNJyKaEJvdscLGb/rGdjz/GhtePY2wg35qCPC/VLZlU6MLGOldgmkauiihEtu85Qrz6P/ma8kp0uSA==</InverseQ><D>CYoD5NcQ4eAGnUn8chX98xrLX/FHbpFFARuHfWUPrxQ5CYt93NJX50Z/QPMWuWtwJF+kZPrNpB2Ty9GasXZc+9pCHkLNLWlrtFtcjmwg+S5JIZWrHGYlw+ZUJpQNQUXnTeHuvyuUYUgrISF0ggtT14j0Z9IKzPnbxABA+vzeI1k=</D></RSAKeyValue>");
			uLink.Network.incomingPassword = ServerConfig.Password;
			NetInterface.InitializeServer(ServerConfig.MaxConnections, ServerConfig.ServerPort, ServerConfig.UseProxy);
		}
	}

	public static void SetServerState(EServerStatus state)
	{
		serverStatus |= state;
	}

	public static void Clear()
	{
		RailwayManager.Instance.DoRemoveAllPassengers();
		foreach (KeyValuePair<uLink.NetworkPlayer, ulong> loginPeer in LoginPeers)
		{
			NetInterface.CloseConnection(loginPeer.Key);
		}
		LoginIds.Clear();
		LoginPeers.Clear();
		ColorIndex.Clear();
	}

	private void uLink_OnServerInitialized()
	{
		Debug.Log("Server initialized on port:" + uLink.Network.listenPort);
		uLink.Network.InitializeSecurity();
		GameWorld.InitWorld();
		LoadManager.Load();
		LoadManager.Init();
		SetServerState(EServerStatus.Initialized);
		if (ServerConfig.IsNewServer)
		{
			ServerConfig.InitTownAreaPara();
			ServerConfig.SyncSave();
		}
		Debug.Log($"ServerInfo--ServerName:{ServerConfig.ServerName}, MasterRole:{ServerConfig.MasterRoleName}, MapSeed:{ServerConfig.MapSeed}, GameMode:{ServerConfig.SceneMode}, GameType:{ServerConfig.GameType}, Monster:{ServerConfig.MonsterYes}, Major Bioma:{ServerConfig.TerrainType}, Climate:{ServerConfig.ClimateType}, TeamNum:{ServerConfig.TeamNum}, NumPerTeam:{ServerConfig.NumPerTeam}, UnlimitedRes:{ServerConfig.UnlimitedRes},TerrainHeight:{ServerConfig.TerrainHeight}, MapSize:{ServerConfig.MapSize}, RiverDensity:{ServerConfig.RiverDensity}, RiverWidth:{ServerConfig.RiverWidth},PlainHeight:{ServerConfig.PlainHeight},Flatness:{ServerConfig.Flatness},BridgeMaxHeight:{ServerConfig.BridgeMaxHeight},AllyCount:{ServerConfig.AllyCount}, Version:{ServerConfig.ServerVersion}");
	}

	private void uLink_OnServerUninitialized()
	{
		Debug.Log("Server uninitialized.");
		uLink.MasterServer.UnregisterHost();
		Clear();
	}

	private void uLink_OnPlayerApproval(NetworkPlayerApproval approval)
	{
		uLink.BitStream remainingBitStream = approval.loginData.GetRemainingBitStream();
		ulong num = remainingBitStream.Read<ulong>(new object[0]);
		int num2 = remainingBitStream.Read<int>(new object[0]);
		if (!TestLogin(num))
		{
			approval.Deny(uLink.NetworkConnectionError.ApprovalDenied);
			return;
		}
		if (!HasServerInitialized)
		{
			approval.Deny((uLink.NetworkConnectionError)201);
			return;
		}
		if (!ServerConfig.isCompatible)
		{
			approval.Deny((uLink.NetworkConnectionError)200);
			return;
		}
		if (ServerConfig.IsCustom && BaseNetwork.IsFull())
		{
			approval.Deny((uLink.NetworkConnectionError)202);
			return;
		}
		num2 |= 0x2DC6C0;
		if (!ServerAdministrator.IsAdmin(num2))
		{
			if (!ServerAdministrator.AllowJoin)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("Server owner banned the login.");
				}
				approval.Deny(uLink.NetworkConnectionError.ConnectionBanned);
				return;
			}
			if (ServerAdministrator.IsBlack(num2))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogFormat("User in the banned list:{0}", num);
				}
				approval.Deny(uLink.NetworkConnectionError.ConnectionBanned);
				return;
			}
		}
		approval.Approve();
	}

	private void uLink_OnPlayerConnected(uLink.NetworkPlayer peer)
	{
		ulong steamId = peer.loginData.Read<ulong>(new object[0]);
		int num = peer.loginData.Read<int>(new object[0]);
		RoleInfo roleInfo = peer.loginData.Read<RoleInfo>(new object[0]);
		uLink.Network.UninitializeSecurity(peer);
		if (TestLogin(steamId))
		{
			num |= 0x2DC6C0;
			ServerConfig.SetMasterId(roleInfo.name, num);
			AddLoginPeer(steamId, peer);
			SyncServerInfo(peer);
			SyncForceInfo(peer);
			AllocateColorIndex(peer);
			NetInterface.Instantiate(peer, PrefabManager.Self.BaseProxy, PrefabManager.Self.BaseOwner, PrefabManager.Self.BaseCreator, Vector3.zero, Quaternion.identity, 100, roleInfo);
			uLobbyNetwork.UpdateServerInfo();
			LobbyInterface.LobbyRPC(ELobbyMsgType.PlayerEnterGS, (int)ServerConfig.GameType, (int)ServerConfig.SceneMode, ServerConfig.ServerID);
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("Role[{0}] has connected", roleInfo.name);
			}
		}
	}

	private void uLink_OnPlayerDisconnected(uLink.NetworkPlayer peer)
	{
		uLink.Network.RemoveRPCs(peer);
		uLink.Network.DestroyPlayerObjects(peer);
		uLink.Network.RemoveInstantiates(peer);
		ulong num = RemoveLoginPeer(peer);
		DeallocateColorIndex(peer);
		uLobbyNetwork.UpdateServerInfo();
		if (LogFilter.logDebug)
		{
			Debug.LogFormat("{0} disconnected from server", num);
		}
	}

	private bool TestLogin(ulong steamId)
	{
		if (LoginIds.ContainsKey(steamId))
		{
			NetInterface.CloseConnection(LoginIds[steamId]);
			RemoveLoginId(steamId);
			return false;
		}
		return true;
	}

	private void AddLoginPeer(ulong steamId, uLink.NetworkPlayer peer)
	{
		LoginIds.Add(steamId, peer);
		LoginPeers.Add(peer, steamId);
	}

	private ulong GetLoginId(uLink.NetworkPlayer peer)
	{
		if (LoginPeers.ContainsKey(peer))
		{
			return LoginPeers[peer];
		}
		return 0uL;
	}

	private uLink.NetworkPlayer GetLoginPeer(ulong id)
	{
		if (LoginIds.ContainsKey(id))
		{
			return LoginIds[id];
		}
		return uLink.NetworkPlayer.unassigned;
	}

	private ulong RemoveLoginPeer(uLink.NetworkPlayer peer)
	{
		if (LoginPeers.ContainsKey(peer))
		{
			ulong num = LoginPeers[peer];
			LoginIds.Remove(num);
			LoginPeers.Remove(peer);
			return num;
		}
		return 0uL;
	}

	private void RemoveLoginId(ulong steamId)
	{
		if (LoginIds.ContainsKey(steamId))
		{
			LoginPeers.Remove(LoginIds[steamId]);
			LoginIds.Remove(steamId);
		}
	}

	private static bool AllocateColorIndex(uLink.NetworkPlayer peer)
	{
		int num = -1;
		for (int i = 0; i < 32; i++)
		{
			num = i;
			foreach (KeyValuePair<uLink.NetworkPlayer, int> item in ColorIndex)
			{
				if (item.Value == i)
				{
					num = -1;
					break;
				}
			}
		}
		ColorIndex[peer] = num;
		return -1 == num;
	}

	private static void DeallocateColorIndex(uLink.NetworkPlayer peer)
	{
		ColorIndex.Remove(peer);
	}

	public static int GetColorIndex(uLink.NetworkPlayer peer)
	{
		return (!ColorIndex.ContainsKey(peer)) ? (-1) : ColorIndex[peer];
	}

	private void SyncForceInfo(uLink.NetworkPlayer peer)
	{
	}

	public string StringToHex(string str)
	{
		byte[] bytes = Encoding.Default.GetBytes(str);
		string text = string.Empty;
		byte[] array = bytes;
		foreach (byte b in array)
		{
			text += b.ToString("X");
		}
		return text;
	}

	private void SyncServerInfo(uLink.NetworkPlayer peer)
	{
		int num = ServerConfig.GenerateMapSeed(ServerConfig.MapSeed);
		NetworkManager.SyncPeer(peer, EPacketType.PT_Common_ServerInfo, ServerConfig.ServerName, num, (int)ServerConfig.SceneMode, (int)ServerConfig.GameType, ServerConfig.MonsterYes, (int)ServerConfig.TerrainType, (int)ServerConfig.VegetationType, (int)ServerConfig.ClimateType, ServerConfig.TeamNum, ServerConfig.NumPerTeam, ServerConfig.UnlimitedRes, ServerConfig.UseSkillTree, ServerConfig.TerrainHeight, ServerConfig.MapSize, ServerConfig.RiverDensity, ServerConfig.RiverWidth, ServerConfig.PlainHeight, ServerConfig.Flatness, ServerConfig.BridgeMaxHeight, ServerConfig.AllyCount, ServerConfig.mirror, ServerConfig.rotation, ServerConfig.pickedLineIndex, ServerConfig.pickedLevelIndex, ServerConfig.MasterId);
	}
}
