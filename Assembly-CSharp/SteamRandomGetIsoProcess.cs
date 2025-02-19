using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamRandomGetIsoProcess : ISteamGetFile
{
	private int _getAmount;

	private GetRandomIsoListCallBackEventHandler GetRandomIsoListCallBackEvent;

	private CallResult<RemoteStorageEnumerateWorkshopFilesResult_t> GetTotalCountResult;

	private List<CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>> GetFileInfoResult = new List<CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>>();

	private List<CallResult<RemoteStorageGetPublishedFileDetailsResult_t>> GetFileDetailsResult = new List<CallResult<RemoteStorageGetPublishedFileDetailsResult_t>>();

	private List<ulong> _fileIDs = new List<ulong>();

	private List<ulong> _publishIDs = new List<ulong>();

	private string[] _tags;

	private int _dungeonId;

	public SteamRandomGetIsoProcess(GetRandomIsoListCallBackEventHandler callback, int amount, int dungeonId, string tag)
	{
		GetRandomIsoListCallBackEvent = callback;
		_getAmount = amount;
		string[] tags = new string[2]
		{
			SteamWorkShop.NewVersionTag,
			tag
		};
		GetTotalCount(1u, tags, 0u, 1u);
		_tags = tags;
		_dungeonId = dungeonId;
	}

	private void AddToList(ulong fileId, ulong publishId)
	{
		_fileIDs.Add(fileId);
		_publishIDs.Add(publishId);
	}

	private void GetTotalCount(uint startIndex, string[] tags, uint days = 0, uint count = 1, EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent)
	{
		try
		{
			GetTotalCountResult = CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>.Create(OnGetTotalCountResult);
			SteamAPICall_t hAPICall = SteamRemoteStorage.EnumeratePublishedWorkshopFiles(orderBy, startIndex, count, days, tags, null);
			GetTotalCountResult.Set(hAPICall);
		}
		catch (Exception ex)
		{
			Debug.Log("SteamRandomGetIsoProcess GetPreFileList " + ex.ToString());
			Finish(_fileIDs, _publishIDs, bOK: false);
		}
	}

	private void OnGetTotalCountResult(RemoteStorageEnumerateWorkshopFilesResult_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				for (int i = 0; i < _getAmount; i++)
				{
					int unStartIndex = UnityEngine.Random.Range(1, pCallback.m_nTotalResultCount);
					GetFileInfoResult.Add(CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>.Create(OnGetFileInfoResult));
					SteamAPICall_t hAPICall = SteamRemoteStorage.EnumeratePublishedWorkshopFiles(EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent, (uint)unStartIndex, 1u, 0u, _tags, null);
					GetFileInfoResult[i].Set(hAPICall);
				}
			}
			else
			{
				Finish(_fileIDs, _publishIDs, bOK: false);
			}
		}
		catch (Exception ex)
		{
			Finish(_fileIDs, _publishIDs, bOK: false);
			Debug.LogError(ex.ToString());
		}
	}

	private void OnGetFileInfoResult(RemoteStorageEnumerateWorkshopFilesResult_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				GetFileDetailsResult.Add(CallResult<RemoteStorageGetPublishedFileDetailsResult_t>.Create(OnGetFinalInfo));
				SteamAPICall_t publishedFileDetails = SteamRemoteStorage.GetPublishedFileDetails(pCallback.m_rgPublishedFileId[0], 0u);
				GetFileDetailsResult[GetFileDetailsResult.Count - 1].Set(publishedFileDetails);
			}
			else
			{
				Finish(_fileIDs, _publishIDs, bOK: false);
			}
		}
		catch (Exception ex)
		{
			Finish(_fileIDs, _publishIDs, bOK: false);
			Debug.LogError(ex.ToString());
		}
	}

	private void OnGetFinalInfo(RemoteStorageGetPublishedFileDetailsResult_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult == EResult.k_EResultOK && pCallback.m_hFile.m_UGCHandle != 0L)
			{
				AddToList(pCallback.m_hFile.m_UGCHandle, pCallback.m_nPublishedFileId.m_PublishedFileId);
				if (_getAmount == _fileIDs.Count)
				{
					Finish(_fileIDs, _publishIDs, bOK: true);
				}
			}
			else
			{
				Finish(_fileIDs, _publishIDs, bOK: false);
			}
		}
		catch (Exception ex)
		{
			Finish(_fileIDs, _publishIDs, bOK: false);
			Debug.LogError(ex.ToString());
		}
	}

	private void Finish(List<ulong> fileIDsList, List<ulong> publishIds, bool bOK)
	{
		if (GetRandomIsoListCallBackEvent != null)
		{
			GetRandomIsoListCallBackEvent(fileIDsList, publishIds, _dungeonId, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}
}
