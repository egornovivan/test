using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class ISteamFriends : MonoBehaviour
{
	public delegate void GetFriendsEventHandler(Dictionary<int, SteamFriendsData> friendsList, bool bOK);

	public delegate void RecvMsgEventHandler(int index, string text);

	public delegate void PersonStateChangeHandler(int index);

	public delegate void GetFriendInfoEventHandler();

	public static Dictionary<int, SteamFriendsData> _FriendsList = new Dictionary<int, SteamFriendsData>();

	internal static PersonStateChangeHandler _funPersonStateChange = null;

	internal static RecvMsgEventHandler _funRecvChatMsg = null;

	internal static GetFriendsEventHandler _funGetFriends = null;

	public SteamFriendsData GetData(CSteamID steamID)
	{
		foreach (KeyValuePair<int, SteamFriendsData> friends in _FriendsList)
		{
			if (friends.Value != null && friends.Value._SteamID == steamID)
			{
				return friends.Value;
			}
		}
		return null;
	}

	public SteamFriendsData GetData(int index)
	{
		if (_FriendsList.ContainsKey(index))
		{
			return _FriendsList[index];
		}
		return null;
	}

	public int GetIndex(CSteamID steamID)
	{
		foreach (KeyValuePair<int, SteamFriendsData> friends in _FriendsList)
		{
			if (friends.Value != null && friends.Value._SteamID == steamID)
			{
				return friends.Key;
			}
		}
		return -1;
	}
}
