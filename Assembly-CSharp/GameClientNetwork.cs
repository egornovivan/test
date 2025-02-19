using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PeEvent;
using uLink;
using UnityEngine;

public class GameClientNetwork : UnityEngine.MonoBehaviour
{
	private static int m_MasterId;

	private static GameClientNetwork _instance;

	private static string m_ServerName;

	public List<RoomPlayerInfo> RolesInGame = new List<RoomPlayerInfo>();

	public List<uLink.HostData> LocalServers = new List<uLink.HostData>();

	public static GameClientNetwork Self => _instance;

	public static int MasterId => m_MasterId;

	public static string ServerName => m_ServerName;

	public static event Action OnDisconnectEvent;

	public static event Action OnFailedConnectEvent;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		uLink.Network.isAuthoritativeServer = true;
		uLink.Network.requireSecurityForConnecting = true;
		uLink.Network.config.timeoutDelay = 180f;
		uLink.Network.maxManualViewIDs = 10;
		uLink.Network.minimumAllocatableViewIDs = 5000;
		uLink.Network.minimumUsedViewIDs = 10;
		uLink.Network.sendRate = 10f;
		uLink.Network.publicKey = new PublicKey("<RSAKeyValue><Modulus>s9xPzYdYVTSz1K2JZUgcfCTBDcvGxKCsmkmKqum0P+m4iaF0zKfl/rl5Ub/HzR5R+LvUF/69uNQ68qS1FlMM+GAgoOD8C1x8ADESgW5qOdr5CUoW3H5wQREuhIFhWkCim0rHxeQFXYBPNUVUwVqXvPb9XiLbZVDXng6xuPMmrCM=</Modulus><Exponent>EQ==</Exponent></RSAKeyValue>");
		MyServerManager.OnServerHostEvent += OnServerHost;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		GameClientNetwork.OnDisconnectEvent = (Action)Delegate.Remove(GameClientNetwork.OnDisconnectEvent, new Action(OnDisconnectFromServer));
		P2PManager.OnServerRegisteredEvent -= OnServerRegistered;
		MyServerManager.OnServerHostEvent -= OnServerHost;
	}

	private void uLink_OnConnectedToServer()
	{
		if (LogFilter.logDebug)
		{
			Debug.Log("Server connected");
		}
		MessageBox_N.CancelMask(MsgInfoType.ServerLoginMask);
	}

	private void uLink_OnFailedToConnect(uLink.NetworkConnectionError error)
	{
		MessageBox_N.CancelMask(MsgInfoType.ServerLoginMask);
		if (LogFilter.logDebug)
		{
			Debug.LogErrorFormat("Failed to connect:{0}", error);
		}
		if (GameClientNetwork.OnFailedConnectEvent != null)
		{
			GameClientNetwork.OnFailedConnectEvent();
		}
		switch (error)
		{
		case uLink.NetworkConnectionError.InvalidPassword:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000037));
			break;
		case uLink.NetworkConnectionError.RSAPublicKeyMismatch:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000045));
			break;
		case uLink.NetworkConnectionError.ConnectionBanned:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000046));
			break;
		case uLink.NetworkConnectionError.LimitedPlayers:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000826));
			break;
		case (uLink.NetworkConnectionError)200:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000754));
			break;
		case (uLink.NetworkConnectionError)201:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000755));
			break;
		case (uLink.NetworkConnectionError)202:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000827));
			break;
		default:
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000502), Connect);
			break;
		}
	}

	private void uLink_OnDisconnectedFromServer(uLink.NetworkDisconnection mode)
	{
		MessageBox_N.CancelMask(MsgInfoType.ServerLoginMask);
		if (mode == uLink.NetworkDisconnection.LostConnection)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("Lost connection");
			}
			else if (LogFilter.logDebug)
			{
				Debug.Log("Disconnected. Mode:" + mode);
			}
		}
		RolesInGame.Clear();
		if (GameClientNetwork.OnDisconnectEvent != null)
		{
			GameClientNetwork.OnDisconnectEvent();
		}
		Cursor.lockState = (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None);
		Cursor.visible = true;
		PeCamera.SetVar("ForceShowCursor", true);
	}

	private void OnServerRegistered(string srvName, int srvMode, int srvPort)
	{
		if (object.Equals(MyServerManager.LocalName, srvName))
		{
			MessageBox_N.CancelMask(MsgInfoType.NoticeOnly);
			MyServerManager.LocalPort = srvPort;
			Connect();
			P2PManager.OnServerRegisteredEvent -= OnServerRegistered;
		}
	}

	private void OnServerHost()
	{
		P2PManager.OnServerRegisteredEvent += OnServerRegistered;
		StartCoroutine(OnServerHostTimeout());
	}

	private IEnumerator OnServerHostTimeout()
	{
		yield return new WaitForSeconds(20f);
		P2PManager.OnServerRegisteredEvent -= OnServerRegistered;
	}

	public static void OnDisconnectFromServer()
	{
		if (PeGameMgr.IsMultiStory)
		{
			PeGameMgr.yirdName = null;
		}
		if (PeLauncher.Instance.endLaunch != null)
		{
			PeLauncher.Instance.eventor.Subscribe(OnResponse);
		}
		else
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000048), PeSceneCtrl.Instance.GotoLobbyScene);
		}
	}

	private static void OnResponse(object sender, EventArg arg)
	{
		if (!NetworkInterface.IsClient && arg is PeLauncher.LoadFinishedArg)
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000048), PeSceneCtrl.Instance.GotoLobbyScene);
			PeLauncher.Instance.eventor.Unsubscribe(OnResponse);
		}
	}

	public static void Connect()
	{
		MessageBox_N.ShowMaskBox(MsgInfoType.ServerLoginMask, PELocalization.GetString(8000062));
		if (MyServerManager.LocalHost is ProxyServerRegistered { IsLan: false, UseProxy: not false } proxyServerRegistered)
		{
			NetworkInterface.Connect(proxyServerRegistered.ProxyServer, MyServerManager.LocalPwd, GameClientLobby.role.steamId, GameClientLobby.role.roleID, GameClientLobby.role);
		}
		else
		{
			NetworkInterface.Connect(MyServerManager.LocalIp, MyServerManager.LocalPort, MyServerManager.LocalPwd, GameClientLobby.role.steamId, GameClientLobby.role.roleID, GameClientLobby.role);
		}
		GameClientNetwork.OnDisconnectEvent = (Action)Delegate.Combine(GameClientNetwork.OnDisconnectEvent, new Action(OnDisconnectFromServer));
	}

	public static void Disconnect()
	{
		GameClientNetwork.OnDisconnectEvent = (Action)Delegate.Remove(GameClientNetwork.OnDisconnectEvent, new Action(OnDisconnectFromServer));
		NetworkInterface.Disconnect();
	}

	public static void RPC_S2C_ServerInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		m_ServerName = stream.Read<string>(new object[0]);
		RandomMapConfig.RandSeed = stream.Read<int>(new object[0]);
		PeGameMgr.sceneMode = (PeGameMgr.ESceneMode)stream.Read<int>(new object[0]);
		if (PeGameMgr.sceneMode != 0 && PeGameMgr.sceneMode != PeGameMgr.ESceneMode.Adventure && PeGameMgr.sceneMode != PeGameMgr.ESceneMode.Build && PeGameMgr.sceneMode != PeGameMgr.ESceneMode.Custom)
		{
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Adventure;
		}
		PeGameMgr.gameType = (PeGameMgr.EGameType)Mathf.Clamp(stream.Read<int>(new object[0]), 0, 2);
		PeGameMgr.monsterYes = stream.Read<bool>(new object[0]);
		RandomMapConfig.RandomMapID = (RandomMapType)Mathf.Clamp(stream.Read<int>(new object[0]), 1, 8);
		RandomMapConfig.vegetationId = (RandomMapType)Mathf.Clamp(stream.Read<int>(new object[0]), 1, 8);
		RandomMapConfig.ScenceClimate = (ClimateType)Mathf.Clamp(stream.Read<int>(new object[0]), 0, 3);
		if (RandomMapConfig.ScenceClimate == ClimateType.CT_Random)
		{
			RandomMapConfig.ScenceClimate = ClimateType.CT_Temperate;
		}
		int num = Mathf.Clamp(stream.Read<int>(new object[0]), 1, 32);
		int num2 = Mathf.Clamp(stream.Read<int>(new object[0]), 1, 32);
		PeGameMgr.unlimitedRes = stream.Read<bool>(new object[0]);
		RandomMapConfig.useSkillTree = stream.Read<bool>(new object[0]);
		RandomMapConfig.TerrainHeight = stream.Read<int>(new object[0]);
		if (RandomMapConfig.TerrainHeight <= 128)
		{
			RandomMapConfig.TerrainHeight = 128;
		}
		else if (RandomMapConfig.terrainHeight <= 256)
		{
			RandomMapConfig.TerrainHeight = 256;
		}
		else if (RandomMapConfig.TerrainHeight <= 512)
		{
			RandomMapConfig.TerrainHeight = 512;
		}
		else
		{
			RandomMapConfig.TerrainHeight = 512;
		}
		RandomMapConfig.mapSize = Mathf.Clamp(stream.Read<int>(new object[0]), 0, 4);
		RandomMapConfig.riverDensity = Mathf.Clamp(stream.Read<int>(new object[0]), 1, 100);
		RandomMapConfig.riverWidth = Mathf.Clamp(stream.Read<int>(new object[0]), 1, 100);
		RandomMapConfig.plainHeight = Mathf.Clamp(stream.Read<int>(new object[0]), 1, 100);
		RandomMapConfig.flatness = Mathf.Clamp(stream.Read<int>(new object[0]), 1, 100);
		RandomMapConfig.bridgeMaxHeight = Mathf.Clamp(stream.Read<int>(new object[0]), 0, 100);
		RandomMapConfig.allyCount = Mathf.Clamp(stream.Read<int>(new object[0]), 4, 8);
		RandomMapConfig.mirror = stream.Read<bool>(new object[0]);
		RandomMapConfig.rotation = stream.Read<int>(new object[0]);
		RandomMapConfig.pickedLineIndex = stream.Read<int>(new object[0]);
		RandomMapConfig.pickedLevelIndex = stream.Read<int>(new object[0]);
		m_MasterId = stream.Read<int>(new object[0]);
		PeGameMgr.gameName = ServerName;
		Debug.Log($"Game Mode With No Mask:{PeGameMgr.sceneMode}, Game Type:{PeGameMgr.gameType}, Team Num:{num}, Num Per Team:{num2},Monster:{PeGameMgr.monsterYes},MapSize:{RandomMapConfig.mapSize}, MapSeed:{RandomMapConfig.RandSeed}");
		BattleManager.InitBattleInfo(num, num2);
		Singleton<ForceSetting>.Instance.InitRoomForces(num, num2);
		if (null != UILobbyMainWndCtrl.Instance)
		{
			UILobbyMainWndCtrl.Instance.Hide();
		}
		if (null != RoomGui_N.Instance)
		{
			RoomGui_N.Instance.Show();
		}
		if (PeGameMgr.sceneMode != 0 && PeGameMgr.sceneMode != PeGameMgr.ESceneMode.Custom)
		{
			RandomMapConfig.Instance.SetMapParam();
		}
		if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story)
		{
			VFVoxelWater.c_fWaterLvl = 97f;
		}
	}
}
