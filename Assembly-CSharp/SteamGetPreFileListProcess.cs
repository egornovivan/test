using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamGetPreFileListProcess : ISteamGetFile
{
	private CallResult<RemoteStorageEnumerateWorkshopFilesResult_t> RemoteStorageEnumerateWorkshopFilesResult;

	private GetPreListCallBackEventHandler CallBackGetPreListResult;

	public List<PublishedFileId_t> _FileIDLsit = new List<PublishedFileId_t>();

	private uint _StartIndex;

	public SteamGetPreFileListProcess(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex, string[] tags, uint days = 0, uint count = 9, EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote)
	{
		RemoteStorageEnumerateWorkshopFilesResult = CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>.Create(OnRemoteStorageEnumerateWorkshopFilesResultAllUser);
		CallBackGetPreListResult = callBackGetPreListResult;
		tags = SteamWorkShop.AddNewVersionTag(tags);
		GetPreFileList(startIndex, tags, days, count, orderBy);
		_StartIndex = startIndex;
	}

	private void Finish(List<PublishedFileId_t> publishIDList, int totalResults, uint startIndex, bool bOK)
	{
		if (CallBackGetPreListResult != null)
		{
			CallBackGetPreListResult(publishIDList, totalResults, startIndex, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void GetPreFileList(uint startIndex, string[] tags, uint days = 0, uint count = 9, EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent)
	{
		try
		{
			SteamAPICall_t hAPICall = SteamRemoteStorage.EnumeratePublishedWorkshopFiles(orderBy, startIndex, count, days, tags, null);
			RemoteStorageEnumerateWorkshopFilesResult.Set(hAPICall);
		}
		catch (Exception ex)
		{
			Finish(_FileIDLsit, 0, _StartIndex, bOK: false);
			Debug.Log("SteamGetPreFileListProcess GetMyPreFileList " + ex.ToString());
		}
	}

	private void OnRemoteStorageEnumerateWorkshopFilesResultAllUser(RemoteStorageEnumerateWorkshopFilesResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			for (int i = 0; i < pCallback.m_nResultsReturned; i++)
			{
				_FileIDLsit.Add(pCallback.m_rgPublishedFileId[i]);
			}
			Finish(_FileIDLsit, pCallback.m_nTotalResultCount, _StartIndex, bOK: true);
		}
		else
		{
			Finish(_FileIDLsit, 0, _StartIndex, bOK: false);
		}
	}
}
