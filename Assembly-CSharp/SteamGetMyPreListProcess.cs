using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamGetMyPreListProcess : ISteamGetFile
{
	private CallResult<RemoteStorageEnumerateUserSharedWorkshopFilesResult_t> remoteStorageEnumerateUserSharedWorkshopFilesResult;

	private GetPreListCallBackEventHandler CallBackGetPreListResult;

	public List<PublishedFileId_t> _FileIDLsit = new List<PublishedFileId_t>();

	private uint _StartIndex;

	private uint _Count;

	public SteamGetMyPreListProcess(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex, string[] tags, uint count = 9)
	{
		remoteStorageEnumerateUserSharedWorkshopFilesResult = CallResult<RemoteStorageEnumerateUserSharedWorkshopFilesResult_t>.Create(OnRemoteStorageEnumerateUserSharedWorkshopFilesResultMyPreFileList);
		CallBackGetPreListResult = callBackGetPreListResult;
		GetMyPreFileList(startIndex, tags);
		_StartIndex = startIndex;
		_Count = count;
	}

	private void Finish(List<PublishedFileId_t> publishIDList, int totalResults, uint startIndex, bool bOK)
	{
		if (CallBackGetPreListResult != null)
		{
			CallBackGetPreListResult(publishIDList, totalResults, startIndex, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void GetMyPreFileList(uint startIndex, string[] tags)
	{
		try
		{
			SteamAPICall_t hAPICall = SteamRemoteStorage.EnumerateUserSharedWorkshopFiles(SteamMgr.steamId, startIndex, tags, null);
			remoteStorageEnumerateUserSharedWorkshopFilesResult.Set(hAPICall);
		}
		catch (Exception ex)
		{
			Finish(_FileIDLsit, 0, _StartIndex, bOK: false);
			Debug.Log("SteamGetMyPreListProcess GetMyPreFileList " + ex.ToString());
		}
	}

	private void OnRemoteStorageEnumerateUserSharedWorkshopFilesResultMyPreFileList(RemoteStorageEnumerateUserSharedWorkshopFilesResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			int num = pCallback.m_nResultsReturned;
			if (num > _Count)
			{
				num = (int)_Count;
			}
			for (int i = 0; i < num; i++)
			{
				_FileIDLsit.Add(pCallback.m_rgPublishedFileId[i]);
			}
			Finish(_FileIDLsit, pCallback.m_nTotalResultCount, _StartIndex, bOK: true);
		}
		else
		{
			Finish(_FileIDLsit, 0, _StartIndex, bOK: false);
			LogManager.Warning(" OnRemoteStorageEnumerateUserSharedWorkshopFilesResult error ");
		}
	}
}
