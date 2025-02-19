using System.Collections;
using uLink;
using uLobby;
using UnityEngine;

public class uLobbyNetwork : LobbyInterface
{
	private void Start()
	{
		Lobby.AddListener(this);
		Lobby.OnConnected += Lobby_OnConnected;
		Lobby.OnDisconnected += Lobby_OnDisconnected;
		Lobby.OnFailedToConnect += Lobby_OnFailedToConnect;
		Lobby.OnSecurityUninitialized += Lobby_OnSecurityUninitialized;
		Lobby.OnSecurityInitialized += Lobby_OnSecurityInitialized;
		RegisterMsgHandlers();
		uLink.Network.config.timeoutDelay = 180f;
	}

	private void uLink_OnServerInitialized()
	{
		ConnectLobby();
		uLink.MasterServer.password = "patheahaha";
		uLink.MasterServer.gameType = "PatheaGame";
		uLink.MasterServer.gameName = ServerConfig.ServerName;
		uLink.MasterServer.dedicatedServer = true;
		uLink.MasterServer.updateRate = 2f;
		Update2MasterServer();
	}

	private void RegisterMsgHandlers()
	{
		MessageHandlers messageHandlers = new MessageHandlers();
		messageHandlers.RegisterHandler(ELobbyMsgType.CloseServer, RPC_L2S_CloseServer);
		messageHandlers.RegisterHandler(ELobbyMsgType.ServerRegister, RPC_L2S_ServerRegistered);
		messageHandlers.RegisterHandler(ELobbyMsgType.CreateItem, RPC_L2S_CreateItems);
		messageHandlers.RegisterHandler(ELobbyMsgType.Statistics, RPC_L2S_Statistics);
		SetHandlers(messageHandlers);
	}

	private IEnumerator TestConnectLobby()
	{
		while (true)
		{
			if (Lobby.connectionStatus == LobbyConnectionStatus.Disconnected)
			{
				Lobby.publicKey = new uLobby.PublicKey("<RSAKeyValue><Modulus>njj4wBQW593lzN1CMkd/soo6yiz4Q1pOzGjGqq0GwR1S/PKdKiNxdyWFING69FGf6V6Almf5oVHXmoN0LNfIDUOw1Lfsq3hORXkUuz2L2dMp98RkkfKprQ+S4w0Y/HRVmp9kEO2PxSqxTwoCcaq/g65XcXs1lhGF26PQRv//pAk=</Modulus><Exponent>EQ==</Exponent></RSAKeyValue>");
				Lobby.ConnectAsServer(ServerConfig.LobbyIP, ServerConfig.LobbyPort);
			}
			yield return new WaitForSeconds(3f);
		}
	}

	private IEnumerator CheckMasterServer()
	{
		while (true)
		{
			if (!uLink.MasterServer.isRegistered)
			{
				uLink.MasterServer.ipAddress = ServerConfig.ProxyIP;
				uLink.MasterServer.port = ServerConfig.ProxyPort;
				uLink.MasterServer.RegisterHost();
			}
			yield return new WaitForSeconds(3f);
		}
	}

	private static void Register2Lobby()
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.MasterRegister, ServerConfig.ServerPort, ServerConfig.MaxConnections, uLink.Network.connections.Length, (int)uLinkNetwork.ServerStatus, (int)ServerConfig.SceneMode, (int)ServerConfig.GameType, ServerConfig.ServerName, ServerConfig.MasterRoleName, ServerConfig.ServerVersion, ServerConfig.PassWordState, ServerConfig.ServerID, ServerConfig.ServerUID, ServerConfig.UID, ServerConfig.MapName);
	}

	private static void Update2Lobby()
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.MasterUpdate, ServerConfig.MaxConnections, uLink.Network.connections.Length, (int)uLinkNetwork.ServerStatus, (int)ServerConfig.SceneMode, (int)ServerConfig.GameType, ServerConfig.ServerName, ServerConfig.MasterRoleName, ServerConfig.ServerVersion, ServerConfig.PassWordState, ServerConfig.ServerID, ServerConfig.ServerUID, ServerConfig.UID, ServerConfig.MapName);
	}

	private static void Update2MasterServer()
	{
		string masterRoleName = ServerConfig.MasterRoleName;
		if (masterRoleName.Contains(","))
		{
			masterRoleName = masterRoleName.Replace(",", "@--@");
		}
		string comment = $"{(int)uLinkNetwork.ServerStatus},{(int)ServerConfig.SceneMode},{(int)ServerConfig.GameType},{ServerConfig.MasterRoleName},{ServerConfig.ServerVersion},{ServerConfig.PassWordState},{ServerConfig.ServerID},{ServerConfig.ServerUID},{ServerConfig.UID},{ServerConfig.MapName}";
		uLink.MasterServer.comment = comment;
	}

	private void ConnectLobby()
	{
		StartCoroutine(TestConnectLobby());
	}

	public static void UpdateServerInfo()
	{
		if (ServerConfig.PublicServer && !ServerConfig.UseProxy)
		{
			Update2Lobby();
		}
		Update2MasterServer();
	}

	public static void ServerRegister()
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.ServerRegister, ServerConfig.OwerSteamId, (int)ServerConfig.SceneMode, (int)ServerConfig.GameType);
	}

	private void Lobby_OnConnected()
	{
		ServerRegister();
		if (LogFilter.logDebug)
		{
			Debug.Log("Connected to lobby server");
		}
	}

	private void Lobby_OnDisconnected()
	{
		if (LogFilter.logDebug)
		{
			Debug.Log("Disconnected from lobby server");
		}
	}

	private void Lobby_OnFailedToConnect(LobbyConnectionError error)
	{
		if (LogFilter.logDebug)
		{
			Debug.LogError(error);
		}
	}

	private void Lobby_OnSecurityUninitialized(LobbyPeer peer)
	{
		if (LogFilter.logDebug)
		{
			Debug.Log(string.Concat(peer, " unencrypted"));
		}
	}

	private void Lobby_OnSecurityInitialized(LobbyPeer peer)
	{
		if (LogFilter.logDebug)
		{
			Debug.Log(string.Concat(peer, " encrypted"));
		}
	}

	public static void CreateAccountItems(int itemType, int amount, ulong steamId)
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.CreateItem, itemType, amount, steamId);
	}

	public static void AddAccountLobbyExp(ulong steamId, float exp)
	{
		if (exp > Mathf.Epsilon)
		{
			LobbyInterface.LobbyRPC(ELobbyMsgType.AddLobbyExp, steamId, exp);
		}
	}

	private void RPC_L2S_CloseServer(uLink.BitStream stream, LobbyMessageInfo info)
	{
		GameServer.Quit();
	}

	private void RPC_L2S_ServerRegistered(uLink.BitStream stream, LobbyMessageInfo info)
	{
		ServerConfig.ServerID = stream.ReadInt32();
		Update2MasterServer();
		if (ServerConfig.PublicServer)
		{
			if (ServerConfig.UseProxy)
			{
				StartCoroutine(CheckMasterServer());
			}
			else
			{
				Register2Lobby();
			}
		}
	}

	private void RPC_L2S_CreateItems(uLink.BitStream stream, LobbyMessageInfo info)
	{
		int itemType = stream.ReadInt32();
		int amount = stream.ReadInt32();
		ulong steamId = stream.ReadUInt64();
		int baseIdByAccount = BaseNetwork.GetBaseIdByAccount(steamId);
		if (baseIdByAccount != -1)
		{
			Player player = Player.GetPlayer(baseIdByAccount);
			if (player != null)
			{
				player.CreateAccountItem(itemType, amount);
			}
		}
	}

	private void RPC_L2S_Statistics(uLink.BitStream stream, LobbyMessageInfo info)
	{
		string str = stream.ReadString();
		GMCommand.Self.SendStatisticsToGM(str);
	}
}
