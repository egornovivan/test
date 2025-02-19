using System;
using System.Collections.Generic;
using CustomData;
using Pathea;
using uLink;
using uLobby;
using UnityEngine;

public class GameClientLobby : LobbyInterface
{
	private enum EAccountType
	{
		Normal,
		Steam,
		Max
	}

	public bool actionOk = true;

	public List<RoleInfo> myRoles = new List<RoleInfo>();

	public List<RoleInfo> myRolesExisted = new List<RoleInfo>();

	public List<RoleInfo> myRolesDeleted = new List<RoleInfo>();

	public List<RoleInfoProxy> m_RolesInLobby = new List<RoleInfoProxy>();

	public List<ServerInfo> m_ServerList = new List<ServerInfo>();

	private static GameClientLobby self;

	public static RoleInfo role { get; private set; }

	public static GameClientLobby Self => self;

	public static event Action OnLobbyDisconnectedEvent;

	private void Awake()
	{
		self = this;
	}

	private void Start()
	{
		Lobby.AddListener(this);
		Lobby.OnConnected += Lobby_OnConnected;
		Lobby.OnDisconnected += Lobby_OnDisconnected;
		Lobby.OnFailedToConnect += Lobby_OnFailedToConnect;
		RegisterMsgHandlers();
		uLink.Network.config.timeoutDelay = 180f;
		GameClientLobby.OnLobbyDisconnectedEvent = (Action)Delegate.Combine(GameClientLobby.OnLobbyDisconnectedEvent, new Action(OnLobbyDisconnected));
		MyServerManager.OnServerHostEvent += OnServerHost;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		Lobby.OnConnected -= Lobby_OnConnected;
		Lobby.OnDisconnected -= Lobby_OnDisconnected;
		Lobby.OnFailedToConnect -= Lobby_OnFailedToConnect;
		GameClientLobby.OnLobbyDisconnectedEvent = (Action)Delegate.Remove(GameClientLobby.OnLobbyDisconnectedEvent, new Action(OnLobbyDisconnected));
		MyServerManager.OnServerHostEvent -= OnServerHost;
	}

	private void Lobby_OnFailedToConnect(LobbyConnectionError error)
	{
		MessageBox_N.CancelMask(MsgInfoType.LobbyLoginMask);
		switch (error)
		{
		case LobbyConnectionError.RSAPublicKeyMismatch:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000034));
			break;
		case LobbyConnectionError.CreateSocketOrThreadFailure:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000860));
			break;
		case LobbyConnectionError.ConnectionTimeout:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000861));
			break;
		case LobbyConnectionError.ConnectionFailed:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000862));
			break;
		default:
			MessageBox_N.ShowOkBox(error.ToString());
			break;
		}
		LogManager.Error("Failed to connect lobby server:", error);
	}

	private void Lobby_OnDisconnected()
	{
		MessageBox_N.CancelMask(MsgInfoType.LobbyLoginMask);
		if (LogFilter.logDebug)
		{
			Debug.Log("Disconnected from lobby server");
		}
		if (GameClientLobby.OnLobbyDisconnectedEvent != null)
		{
			GameClientLobby.OnLobbyDisconnectedEvent();
		}
		Cursor.lockState = (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None);
		Cursor.visible = true;
		PeCamera.SetVar("ForceShowCursor", true);
		NetworkInterface.Disconnect();
		if (PeSingleton<PeFlowMgr>.Instance != null && PeSingleton<PeFlowMgr>.Instance.curScene != PeFlowMgr.EPeScene.MainMenuScene)
		{
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.MainMenuScene, save: false);
		}
	}

	private static void OnLobbyDisconnected()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000035), PeSceneCtrl.Instance.GotoMainMenuScene);
	}

	private void OnServerHost()
	{
		MessageBox_N.ShowMaskBox(MsgInfoType.NoticeOnly, PELocalization.GetString(8000756), 20f);
	}

	private void Lobby_OnConnected()
	{
		if (LogFilter.logDebug)
		{
			Debug.Log("Connected to lobby server");
		}
	}

	public static void ConnectToLobby()
	{
		if (Lobby.connectionStatus == LobbyConnectionStatus.Disconnected)
		{
			Lobby.publicKey = new uLobby.PublicKey("<RSAKeyValue><Modulus>iX4oKfF4o4FCWskfUl6//kxZjP3oMRFQW4xkpnWDaACPKdmIXwaTMBWJpSl/ooDgZfbJHKtHzp43nDx9Cpm0cFfz40N+6LiBL+YSO1d+VJkF/WMsl9C7NZtYRlBTvsQKh0unIESCsuauJotbg2hrapfasUr7KxgVxKM9P+tdn00=</Modulus><Exponent>EQ==</Exponent></RSAKeyValue>");
			Lobby.ConnectAsClient(ClientConfig.LobbyIP, ClientConfig.LobbyPort);
		}
	}

	public static void Disconnect()
	{
		GameClientLobby.OnLobbyDisconnectedEvent = (Action)Delegate.Remove(GameClientLobby.OnLobbyDisconnectedEvent, new Action(OnLobbyDisconnected));
		Lobby.Disconnect();
	}

	private void RegisterMsgHandlers()
	{
		MessageHandlers messageHandlers = new MessageHandlers();
		messageHandlers.RegisterHandler(ELobbyMsgType.SendMsg, RPC_L2C_SendMsg);
		messageHandlers.RegisterHandler(ELobbyMsgType.RoleInfoAllGot, RPC_L2C_RoleInfoAllGot);
		messageHandlers.RegisterHandler(ELobbyMsgType.RoleInfoNone, RPC_L2C_RoleInfoNone);
		messageHandlers.RegisterHandler(ELobbyMsgType.SteamLogin, RPC_L2C_SteamLogin);
		messageHandlers.RegisterHandler(ELobbyMsgType.RolesInLobby, RPC_L2C_RolesInLobby);
		messageHandlers.RegisterHandler(ELobbyMsgType.RepeatLogin, RPC_L2C_RepeatLogin);
		messageHandlers.RegisterHandler(ELobbyMsgType.AccountLogout, RPC_L2C_AccountLogOut);
		messageHandlers.RegisterHandler(ELobbyMsgType.RoleLogin, RPC_L2C_RoleLoggedIn);
		messageHandlers.RegisterHandler(ELobbyMsgType.CloseServer, RPC_L2C_CloseServer);
		messageHandlers.RegisterHandler(ELobbyMsgType.RoleCreateSuccess, RPC_L2C_CreateRoleSuccess);
		messageHandlers.RegisterHandler(ELobbyMsgType.RoleCreateFailed, RPC_L2C_CreateRoleFailed);
		messageHandlers.RegisterHandler(ELobbyMsgType.EnterLobbySuccess, RPC_L2C_EnterLobbySuccuss);
		messageHandlers.RegisterHandler(ELobbyMsgType.EnterLobbyFailed, RPC_L2C_EnterLobbyFailed);
		messageHandlers.RegisterHandler(ELobbyMsgType.DeleteRoleSuccess, RPC_L2C_TryDeleteRoleSuccess);
		messageHandlers.RegisterHandler(ELobbyMsgType.DeleteRoleFailed, RPC_L2C_TryDeleteRoleFailed);
		messageHandlers.RegisterHandler(ELobbyMsgType.SteamInvite, RPC_L2C_Invite);
		messageHandlers.RegisterHandler(ELobbyMsgType.SteamInviteData, RPC_L2C_SyncInviteData);
		messageHandlers.RegisterHandler(ELobbyMsgType.ShopData, RPC_L2C_ShopData);
		messageHandlers.RegisterHandler(ELobbyMsgType.ShopDataAll, RPC_L2C_ShopDataAll);
		messageHandlers.RegisterHandler(ELobbyMsgType.BuyItems, RPC_L2C_BuyItems);
		messageHandlers.RegisterHandler(ELobbyMsgType.QueryLobbyExp, RPC_L2C_QueryLobbyExp);
		messageHandlers.RegisterHandler(ELobbyMsgType.UploadISO, RPC_L2C_UploadIso);
		SetHandlers(messageHandlers);
	}

	internal static void AccountLoginSteamWorks(byte[] tokenByteArray, uint tokenLen, ulong steamId)
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.AccountLogin, tokenByteArray, tokenLen, steamId);
	}

	public void ResetActionOK()
	{
		actionOk = true;
	}

	public void TryCreateRole()
	{
		if (actionOk)
		{
			PeSceneCtrl.Instance.GotoRoleScene();
			actionOk = false;
			Invoke("ResetActionOK", 3f);
		}
	}

	public void TryEnterLobby(int roleId)
	{
		if (actionOk)
		{
			LobbyInterface.LobbyRPC(ELobbyMsgType.EnterLobby, roleId);
			actionOk = false;
			Invoke("ResetActionOK", 3f);
		}
	}

	public void TryDeleteRole(int roleId)
	{
		if (actionOk)
		{
			LobbyInterface.LobbyRPC(ELobbyMsgType.DeleteRole, roleId);
			actionOk = false;
			Invoke("ResetActionOK", 3f);
		}
	}

	public void QueryLobbyExp()
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.QueryLobbyExp);
	}

	public void BackToRole()
	{
		PeSceneCtrl.Instance.GotoMultiRoleScene();
	}

	public void GetShopData(int index = 0, int count = 20, int tabIndex = 0)
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.ShopData, index, count, tabIndex);
	}

	public void GetShopDataAll()
	{
		LobbyInterface.LobbyRPC(ELobbyMsgType.ShopDataAll);
	}

	private void RPC_L2C_SendMsg(uLink.BitStream stream, LobbyMessageInfo info)
	{
		stream.Read<EMsgType>(new object[0]);
		stream.Read<ulong>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		string content = stream.Read<string>(new object[0]);
		if (UILobbyMainWndCtrl.Instance != null && UILobbyMainWndCtrl.Instance.isShow)
		{
			UILobbyMainWndCtrl.Instance.AddTalk(text, content);
		}
	}

	private void RPC_L2C_RoleInfoAllGot(uLink.BitStream stream, LobbyMessageInfo info)
	{
		stream.TryRead<RoleInfo[]>(out var value);
		myRoles = new List<RoleInfo>();
		myRolesExisted = new List<RoleInfo>();
		myRolesDeleted = new List<RoleInfo>();
		AccountItems.self.balance = stream.Read<float>(new object[0]);
		AccountItems.self.ImportData(stream.Read<byte[]>(new object[0]));
		myRoles.AddRange(value);
		for (int i = 0; i < myRoles.Count; i++)
		{
			if (myRoles[i].deletedFlag != 1 && myRolesExisted.Count <= 3)
			{
				myRolesExisted.Add(myRoles[i]);
			}
			else
			{
				myRolesDeleted.Add(myRoles[i]);
			}
		}
		PeSceneCtrl.Instance.GotoMultiRoleScene();
		if ((bool)TitleMenuGui_N.Instance)
		{
			TitleMenuGui_N.Instance.Hide();
		}
		LogManager.Info("RoleInfoAllGot");
	}

	private void RPC_L2C_RoleInfoNone(uLink.BitStream stream, LobbyMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		if (value == 1)
		{
			myRoles = new List<RoleInfo>();
			myRolesExisted = new List<RoleInfo>();
			myRolesDeleted = new List<RoleInfo>();
			Debug.Log("RoleInfoNone");
			PeSceneCtrl.Instance.GotoMultiRoleScene();
			AccountItems.self.balance = stream.Read<float>(new object[0]);
			AccountItems.self.ImportData(stream.Read<byte[]>(new object[0]));
		}
		else
		{
			Debug.LogError("DataBase query failed!!!");
		}
	}

	private void RPC_L2C_SteamLogin(uLink.BitStream stream, LobbyMessageInfo info)
	{
		MessageBox_N.CancelMask(MsgInfoType.LobbyLoginMask);
		Debug.Log("Steam validate ticket.");
	}

	private void RPC_L2C_RolesInLobby(uLink.BitStream stream, LobbyMessageInfo info)
	{
		RoleInfoProxy[] array = stream.Read<RoleInfoProxy[]>(new object[0]);
		m_RolesInLobby.Clear();
		m_RolesInLobby.AddRange(array);
		Debug.Log("Roles In Lobby. Num:" + array.Length);
	}

	private void RPC_L2C_RepeatLogin(uLink.BitStream stream, LobbyMessageInfo info)
	{
		Disconnect();
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000041));
	}

	private void RPC_L2C_AccountLogOut(uLink.BitStream stream, LobbyMessageInfo info)
	{
		ulong steamId = stream.Read<ulong>(new object[0]);
		m_RolesInLobby.RemoveAll((RoleInfoProxy iter) => iter.steamId == steamId);
	}

	private void RPC_L2C_RoleLoggedIn(uLink.BitStream stream, LobbyMessageInfo info)
	{
		RoleInfoProxy role = stream.Read<RoleInfoProxy>(new object[0]);
		m_RolesInLobby.RemoveAll((RoleInfoProxy iter) => iter.steamId == role.steamId);
		m_RolesInLobby.Add(role);
	}

	private void RPC_L2C_CloseServer(uLink.BitStream stream, LobbyMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		MessageBox_N.CancelMask(MsgInfoType.ServerDeleteMask);
		MessageBox_N.ShowOkBox(text);
	}

	private void RPC_L2C_CreateRoleSuccess(uLink.BitStream stream, LobbyMessageInfo info)
	{
		stream.TryRead<RoleInfo>(out var value);
		myRoles.Add(value);
		myRolesExisted.Add(value);
		MLPlayerInfo.Instance.SetSelectedRole(value.name);
		PeSceneCtrl.Instance.GotoMultiRoleScene();
		Debug.Log("Create Role Success");
	}

	private void RPC_L2C_CreateRoleFailed(uLink.BitStream stream, LobbyMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		switch (value)
		{
		case 1:
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000040));
			break;
		case 2:
		{
			stream.TryRead<RoleInfo[]>(out var value2);
			myRoles = new List<RoleInfo>();
			myRolesExisted = new List<RoleInfo>();
			myRolesDeleted = new List<RoleInfo>();
			myRoles.AddRange(value2);
			for (int i = 0; i < myRoles.Count; i++)
			{
				if (myRoles[i].deletedFlag != 1 && myRolesExisted.Count <= 3)
				{
					myRolesExisted.Add(myRoles[i]);
				}
				else
				{
					myRolesDeleted.Add(myRoles[i]);
				}
			}
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000496), PeSceneCtrl.Instance.GotoMultiRoleScene);
			break;
		}
		}
		Debug.Log("Create Role Failed");
	}

	private void RPC_L2C_EnterLobbySuccuss(uLink.BitStream stream, LobbyMessageInfo info)
	{
		role = stream.Read<RoleInfo>(new object[0]);
		PeSceneCtrl.Instance.GotoLobbyScene();
		Debug.Log("Enter Lobby Succuss!");
	}

	private void RPC_L2C_EnterLobbyFailed(uLink.BitStream stream, LobbyMessageInfo info)
	{
		stream.TryRead<RoleInfo[]>(out var value);
		myRoles = new List<RoleInfo>();
		myRolesExisted = new List<RoleInfo>();
		myRolesDeleted = new List<RoleInfo>();
		myRoles.AddRange(value);
		for (int i = 0; i < myRoles.Count; i++)
		{
			if (myRoles[i].deletedFlag != 1 && myRolesExisted.Count <= 3)
			{
				myRolesExisted.Add(myRoles[i]);
			}
			else
			{
				myRolesDeleted.Add(myRoles[i]);
			}
		}
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000496), MLPlayerInfo.Instance.UpdateScene);
		Debug.Log("Enter Lobby Failed");
	}

	private void RPC_L2C_TryDeleteRoleSuccess(uLink.BitStream stream, LobbyMessageInfo info)
	{
		int roleId = stream.Read<int>(new object[0]);
		RoleInfo roleInfo = myRoles.Find((RoleInfo it) => it.roleID == roleId);
		roleInfo.deletedFlag = 1;
		myRolesExisted.Remove(roleInfo);
		myRolesDeleted.Clear();
		myRolesDeleted.Add(roleInfo);
		MLPlayerInfo.Instance.DeleteRole(roleId);
		Debug.Log("Try Delete Role Success");
	}

	private void RPC_L2C_TryDeleteRoleFailed(uLink.BitStream stream, LobbyMessageInfo info)
	{
		stream.TryRead<RoleInfo[]>(out var value);
		myRoles = new List<RoleInfo>();
		myRolesExisted = new List<RoleInfo>();
		myRolesDeleted = new List<RoleInfo>();
		myRoles.AddRange(value);
		for (int i = 0; i < myRoles.Count; i++)
		{
			if (myRoles[i].deletedFlag != 1 && myRolesExisted.Count <= 3)
			{
				myRolesExisted.Add(myRoles[i]);
			}
			else
			{
				myRolesDeleted.Add(myRoles[i]);
			}
		}
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000496), MLPlayerInfo.Instance.UpdateScene);
		Debug.Log("Try Delete Role Failed");
	}

	private void RPC_L2C_Invite(uLink.BitStream stream, LobbyMessageInfo info)
	{
		ulong inviteSteamId = stream.Read<ulong>(new object[0]);
		long serverUID = stream.Read<long>(new object[0]);
		PeSteamFriendMgr.Instance.ReciveInvite(inviteSteamId, serverUID);
	}

	private void RPC_L2C_SyncInviteData(uLink.BitStream stream, LobbyMessageInfo info)
	{
		long[] array = stream.Read<long[]>(new object[0]);
		ulong[] array2 = stream.Read<ulong[]>(new object[0]);
		if (array.Length > 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				PeSteamFriendMgr.Instance.ReciveInvite(array2[i], array[i]);
			}
		}
	}

	private void RPC_L2C_ShopData(uLink.BitStream stream, LobbyMessageInfo info)
	{
		LobbyShopData[] data = stream.Read<LobbyShopData[]>(new object[0]);
		int startIndex = stream.Read<int>(new object[0]);
		int tabIndex = stream.Read<int>(new object[0]);
		LobbyShopMgr.AddRange(data, startIndex, tabIndex);
	}

	private void RPC_L2C_ShopDataAll(uLink.BitStream stream, LobbyMessageInfo info)
	{
		LobbyShopData[] data = stream.Read<LobbyShopData[]>(new object[0]);
		LobbyShopMgr.AddAll(data);
	}

	private void RPC_L2C_BuyItems(uLink.BitStream stream, LobbyMessageInfo info)
	{
		int itemType = stream.Read<int>(new object[0]);
		int amount = stream.Read<int>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		if (AccountItems.self != null)
		{
			AccountItems.self.AddItems(itemType, amount);
			AccountItems.self.balance = num;
			UIMallWnd.Instance.SetMyBalance((int)num);
			UILobbyShopItemMgr._self.MallItemEvent(0, UIMallWnd.Instance.mCurrentTab);
		}
	}

	private void RPC_L2C_QueryLobbyExp(uLink.BitStream stream, LobbyMessageInfo info)
	{
		float lobbyExp = stream.Read<float>(new object[0]);
		float balance = stream.Read<float>(new object[0]);
		role.lobbyExp = lobbyExp;
		AccountItems.self.balance = balance;
	}

	private void RPC_L2C_UploadIso(uLink.BitStream stream, LobbyMessageInfo info)
	{
		ulong hash = stream.Read<ulong>(new object[0]);
		if (stream.Read<bool>(new object[0]))
		{
			SteamWorkShop.SendCacheIso(hash);
			return;
		}
		SendIsoCache cacheIso = SteamWorkShop.GetCacheIso(hash);
		if (cacheIso != null && cacheIso.callBackSteamUploadResult != null)
		{
			cacheIso.callBackSteamUploadResult(cacheIso.id, bOK: false, hash);
		}
	}
}
