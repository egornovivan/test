using Steamworks;
using UnityEngine;

public class SteamFriendsData
{
	public const int _imageHeight = 32;

	public const int _imageWidth = 32;

	public CSteamID _SteamID;

	public string _PlayerName;

	public string _PlayedGameName;

	public EPersonaState _PlayerState;

	public Texture2D _avatar;

	public SteamFriendsData()
	{
		_SteamID = default(CSteamID);
		_avatar = new Texture2D(32, 32, TextureFormat.RGBA32, mipmap: false, linear: true);
	}
}
