using System;
using System.Collections;
using Steamworks;
using UnityEngine;

public class SteamGetFriendsProcess : ISteamFriends
{
	private static SteamGetFriendsProcess _instance;

	private byte[] _imageData = new byte[4096];

	private byte[] _imageTurnData = new byte[4096];

	private int _FriendsCount;

	internal static SteamGetFriendsProcess Instance => _instance;

	private void Awake()
	{
		_instance = this;
	}

	internal IEnumerator GetFriends()
	{
		if (ISteamFriends._funGetFriends == null)
		{
			yield break;
		}
		ISteamFriends._FriendsList.Clear();
		AppId_t[] appList = new AppId_t[1];
		SteamAppList.GetInstalledApps(appList, 1u);
		SteamAppList.GetNumInstalledApps();
		_FriendsCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate | EFriendFlags.k_EFriendFlagIgnoredFriend);
		for (int i = 0; i < _FriendsCount; i++)
		{
			try
			{
				SteamFriendsData data = new SteamFriendsData
				{
					_SteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate | EFriendFlags.k_EFriendFlagIgnoredFriend)
				};
				FriendGameInfo_t fgi = default(FriendGameInfo_t);
				if (SteamFriends.GetFriendGamePlayed(data._SteamID, out fgi))
				{
					if (fgi.m_gameID.AppID() == SteamUtils.GetAppID())
					{
						data._PlayedGameName = "Planet Explorers";
					}
					else
					{
						data._PlayedGameName = "Another game";
					}
				}
				data._PlayerState = SteamFriends.GetFriendPersonaState(data._SteamID);
				data._PlayerName = SteamFriends.GetFriendPersonaName(data._SteamID);
				int FriendAvatar = SteamFriends.GetSmallFriendAvatar(data._SteamID);
				if (SteamUtils.GetImageSize(FriendAvatar, out var ImageWidth, out var ImageHeight) && ImageWidth != 0 && ImageHeight != 0)
				{
					bool ret = SteamUtils.GetImageRGBA(FriendAvatar, _imageData, 4096);
					for (int n = 0; n < _imageData.Length; n += 4)
					{
						int x = n / 4 % 32;
						int y = n / 4 / 32;
						int tag = (32 * (32 - y - 1) + x) * 4;
						_imageTurnData[n] = _imageData[tag];
						_imageTurnData[n + 1] = _imageData[tag + 1];
						_imageTurnData[n + 2] = _imageData[tag + 2];
						_imageTurnData[n + 3] = _imageData[tag + 3];
					}
					data._avatar.LoadRawTextureData(_imageTurnData);
					data._avatar.Apply();
				}
				ISteamFriends._FriendsList[i] = data;
			}
			catch (Exception ex)
			{
				Debug.Log("SteamGetFriendsProcess GetFriends " + ex.ToString());
				ISteamFriends._funGetFriends(null, bOK: false);
			}
			yield return 0;
		}
		ISteamFriends._funGetFriends(ISteamFriends._FriendsList, bOK: true);
	}

	internal SteamFriendsData GetFriendInfo(CSteamID ID)
	{
		SteamFriendsData steamFriendsData = new SteamFriendsData();
		steamFriendsData._SteamID = ID;
		FriendGameInfo_t pFriendGameInfo = default(FriendGameInfo_t);
		if (SteamFriends.GetFriendGamePlayed(steamFriendsData._SteamID, out pFriendGameInfo))
		{
			if (pFriendGameInfo.m_gameID.AppID() == SteamUtils.GetAppID())
			{
				steamFriendsData._PlayedGameName = "Planet Explorers";
			}
			else
			{
				steamFriendsData._PlayedGameName = "Another game";
			}
		}
		else
		{
			steamFriendsData._PlayedGameName = string.Empty;
		}
		steamFriendsData._PlayerState = SteamFriends.GetFriendPersonaState(steamFriendsData._SteamID);
		steamFriendsData._PlayerName = SteamFriends.GetFriendPersonaName(steamFriendsData._SteamID);
		int smallFriendAvatar = SteamFriends.GetSmallFriendAvatar(steamFriendsData._SteamID);
		if (SteamUtils.GetImageSize(smallFriendAvatar, out var pnWidth, out var pnHeight) && pnWidth != 0 && pnHeight != 0)
		{
			bool imageRGBA = SteamUtils.GetImageRGBA(smallFriendAvatar, _imageData, 4096);
			for (int i = 0; i < _imageData.Length; i += 4)
			{
				int num = i / 4 % 32;
				int num2 = i / 4 / 32;
				int num3 = (32 * (32 - num2 - 1) + num) * 4;
				_imageTurnData[i] = _imageData[num3];
				_imageTurnData[i + 1] = _imageData[num3 + 1];
				_imageTurnData[i + 2] = _imageData[num3 + 2];
				_imageTurnData[i + 3] = _imageData[num3 + 3];
			}
			steamFriendsData._avatar.LoadRawTextureData(_imageTurnData);
			steamFriendsData._avatar.Apply();
			return steamFriendsData;
		}
		return null;
	}
}
