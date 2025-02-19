using System.Collections.Generic;
using Steamworks;

public class SteamFriendPrcMgr : ISteamFriends
{
	private static SteamFriendPrcMgr _instance;

	protected static Callback<PersonaStateChange_t> m_PersonaStateChange;

	protected static Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	protected static Callback<GameConnectedFriendChatMsg_t> m_GameConnectedFriendChatMsg;

	public static SteamFriendPrcMgr Instance => _instance;

	private void Awake()
	{
		_instance = this;
		m_PersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
		m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		m_GameConnectedFriendChatMsg = Callback<GameConnectedFriendChatMsg_t>.Create(OnGameConnectedFriendChatMsg);
		base.gameObject.AddComponent<SteamGetFriendsProcess>();
		base.gameObject.AddComponent<SteamChatProcess>();
	}

	public void Init(GetFriendsEventHandler eventHandler, RecvMsgEventHandler handler, PersonStateChangeHandler perHandler)
	{
		ISteamFriends._funGetFriends = eventHandler;
		ISteamFriends._funRecvChatMsg = handler;
		ISteamFriends._funPersonStateChange = perHandler;
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
	}

	private void OnPersonaStateChange(PersonaStateChange_t pCallback)
	{
		SteamFriendsData friendInfo = SteamGetFriendsProcess.Instance.GetFriendInfo(new CSteamID(pCallback.m_ulSteamID));
		if (ISteamFriends._funPersonStateChange == null || friendInfo == null)
		{
			return;
		}
		foreach (KeyValuePair<int, SteamFriendsData> friends in ISteamFriends._FriendsList)
		{
			if (friends.Value._SteamID == friendInfo._SteamID)
			{
				ISteamFriends._FriendsList[friends.Key] = friendInfo;
				ISteamFriends._funPersonStateChange(friends.Key);
				break;
			}
		}
	}

	public void SendChat(int index, string text)
	{
		if (ISteamFriends._FriendsList.ContainsKey(index) && ISteamFriends._FriendsList[index] != null)
		{
			ChatTo(ISteamFriends._FriendsList[index]._SteamID.m_SteamID);
			SteamChatProcess.SendMsg(index, text);
		}
	}

	public void SendChat(ulong steamID, string text)
	{
		CSteamID steamID2 = new CSteamID(steamID);
		int index = GetIndex(steamID2);
		ChatTo(steamID);
		SteamChatProcess.SendMsg(index, text);
	}

	public void ChatTo(ulong steamID)
	{
		CSteamID steamID2 = new CSteamID(steamID);
		SteamFriends.ActivateGameOverlayToUser("chat", steamID2);
	}

	public void FriendAdd(ulong steamID)
	{
		CSteamID steamID2 = new CSteamID(steamID);
		SteamFriends.ActivateGameOverlayToUser("friendadd", steamID2);
	}

	public void FriendRequestIgnore(ulong steamID)
	{
		CSteamID steamID2 = new CSteamID(steamID);
		SteamFriends.ActivateGameOverlayToUser("friendrequestignore", steamID2);
	}

	public void FriendRemove(ulong steamID)
	{
		CSteamID steamID2 = new CSteamID(steamID);
		SteamFriends.ActivateGameOverlayToUser("friendremove", steamID2);
	}

	public void GetFriends()
	{
		StartCoroutine(SteamGetFriendsProcess.Instance.GetFriends());
	}

	public SteamFriendsData GetMyInfo()
	{
		return SteamGetFriendsProcess.Instance.GetFriendInfo(SteamMgr.steamId);
	}

	private void OnGameConnectedFriendChatMsg(GameConnectedFriendChatMsg_t pCallback)
	{
		SteamFriends.GetFriendMessage(pCallback.m_steamIDUser, pCallback.m_iMessageID, out var pvData, 2048, out var _);
		if (pvData.Length > 0 && ISteamFriends._funRecvChatMsg != null)
		{
			int index = GetIndex(pCallback.m_steamIDUser);
			if (index > -1)
			{
				ISteamFriends._funRecvChatMsg(index, pvData);
			}
		}
	}

	public void Invite(ulong steamID, long serverUID)
	{
		foreach (KeyValuePair<int, SteamFriendsData> friends in ISteamFriends._FriendsList)
		{
			if (friends.Value._SteamID.m_SteamID == steamID)
			{
				CSteamID steamId = SteamMgr.steamId;
				LobbyInterface.LobbyRPC(ELobbyMsgType.SteamInvite, steamId.m_SteamID, steamID, serverUID);
				if (friends.Value._PlayedGameName != "Planet Explorers")
				{
					InviteToGame(steamID, serverUID);
				}
				break;
			}
		}
	}

	private bool InviteToGame(ulong steamID, long serverUID)
	{
		return SteamFriends.InviteUserToGame(new CSteamID(steamID), string.Empty);
	}
}
