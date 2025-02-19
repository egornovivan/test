using System;
using System.Text;
using Steamworks;
using uLobby;
using UnityEngine;

internal class SteamMgr : MonoBehaviour
{
	private static HAuthTicket hAuthTicket = HAuthTicket.Invalid;

	private static bool m_bInitialized = false;

	public static CSteamID steamId { get; private set; }

	public static event Action<ulong> OnSteamInitEvent;

	private void Awake()
	{
		steamId = CSteamID.Nil;
		if (SteamAPI.RestartAppIfNecessary(new AppId_t(237870u)))
		{
			Debug.LogError("Steamworks does not start from steam.");
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError("[Steamworks] SteamAPI_Init() failed.");
			Application.Quit();
			return;
		}
		SteamClient.SetWarningMessageHook(SteamAPIDebugTextHook);
		SteamUtils.SetOverlayNotificationPosition(ENotificationPosition.k_EPositionTopRight);
		SteamRemoteStorage.SetCloudEnabledForApp(bEnabled: true);
		steamId = SteamUser.GetSteamID();
		if (LogFilter.logDebug)
		{
			Debug.LogFormat("<color=red>Steam id:[{0}]</color>", steamId);
		}
		base.enabled = true;
	}

	private void Start()
	{
		Lobby.AddListener(this);
		Lobby.OnConnected += Lobby_OnConnected;
		Lobby.OnDisconnected += Lobby_OnDisconnected;
		Lobby.OnFailedToConnect += Lobby_OnFailedToConnect;
	}

	private void Lobby_OnConnected()
	{
		AccountLoginRequestSteamWorks();
	}

	private void Lobby_OnFailedToConnect(LobbyConnectionError error)
	{
		AccountLogoutSteamWorks();
	}

	private void Lobby_OnDisconnected()
	{
		AccountLogoutSteamWorks();
	}

	private void Shutdown()
	{
		if (m_bInitialized)
		{
			m_bInitialized = false;
			base.enabled = false;
			SteamAPI.Shutdown();
		}
	}

	private void OnDestroy()
	{
		Shutdown();
	}

	private void FixedUpdate()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public void AccountLoginRequestSteamWorks()
	{
		if (!m_bInitialized)
		{
			return;
		}
		try
		{
			if (hAuthTicket != HAuthTicket.Invalid)
			{
				SteamUser.CancelAuthTicket(hAuthTicket);
				hAuthTicket = HAuthTicket.Invalid;
				return;
			}
			byte[] array = new byte[1024];
			uint pcbTicket = 0u;
			hAuthTicket = SteamUser.GetAuthSessionTicket(array, array.Length, out pcbTicket);
			GameClientLobby.AccountLoginSteamWorks(array, pcbTicket, steamId.m_SteamID);
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
			AccountLogoutSteamWorks();
		}
	}

	internal static void AccountLogoutSteamWorks()
	{
		try
		{
			if (m_bInitialized)
			{
				SteamUser.CancelAuthTicket(hAuthTicket);
				hAuthTicket = HAuthTicket.Invalid;
			}
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogErrorFormat("{0}\r\n{1}", ex.Message, ex.StackTrace);
			}
		}
	}
}
