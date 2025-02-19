using Steamworks;

public class SteamChatProcess : ISteamFriends
{
	private static SteamChatProcess _instance;

	internal static SteamChatProcess Instance => _instance;

	private void Awake()
	{
		_instance = this;
	}

	internal static bool SendMsg(int index, string text)
	{
		if (text.Length == 0)
		{
			return false;
		}
		if (ISteamFriends._FriendsList.ContainsKey(index) && ISteamFriends._FriendsList[index] != null)
		{
			return SteamFriends.ReplyToFriendMessage(ISteamFriends._FriendsList[index]._SteamID, text);
		}
		return false;
	}
}
