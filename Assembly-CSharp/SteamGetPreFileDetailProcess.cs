using System;
using System.Text;
using Steamworks;
using UnityEngine;

public class SteamGetPreFileDetailProcess : ISteamGetFile
{
	private CallResult<RemoteStorageGetPublishedFileDetailsResult_t> remoteStorageGetPublishedFileDetailsResult;

	private CallResult<RemoteStorageDownloadUGCResult_t> remoteStorageDownloadPreUGCResult;

	private CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t> RemoteStorageGetPublishedItemVoteDetailsResult;

	private SteamGetPreFileCallBackEventHandler CallBackGetPreFileResult;

	private SteamPreFileAndVoteDetail _PreFileDetail = new SteamPreFileAndVoteDetail();

	private PublishedFileId_t _PublishID;

	public SteamGetPreFileDetailProcess(SteamGetPreFileCallBackEventHandler callBackGetPreFileResult, PublishedFileId_t publishID)
	{
		remoteStorageGetPublishedFileDetailsResult = CallResult<RemoteStorageGetPublishedFileDetailsResult_t>.Create(OnRemoteStorageGetPublishedFileDetailsResult);
		remoteStorageDownloadPreUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnRemoteStorageDownloadPreUGCResult);
		RemoteStorageGetPublishedItemVoteDetailsResult = CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t>.Create(OnRemoteStorageGetPublishedItemVoteDetailsResult);
		CallBackGetPreFileResult = callBackGetPreFileResult;
		GetPreFileDetail(publishID);
		_PublishID = publishID;
	}

	private void Finish(PublishedFileId_t publishedFileId, SteamPreFileAndVoteDetail detail, bool bOK)
	{
		if (CallBackGetPreFileResult != null)
		{
			CallBackGetPreFileResult(publishedFileId, detail, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void GetPreFileDetail(PublishedFileId_t publishID)
	{
		try
		{
			if (publishID.m_PublishedFileId == 0L)
			{
				ISteamGetFile.ProcessList.Remove(this);
				return;
			}
			SteamAPICall_t publishedFileDetails = SteamRemoteStorage.GetPublishedFileDetails(publishID, 0u);
			remoteStorageGetPublishedFileDetailsResult.Set(publishedFileDetails);
		}
		catch (Exception ex)
		{
			Finish(_PublishID, null, bOK: false);
			Debug.Log("SteamGetPreFileDetailProcess GetPreFileDetail " + ex.ToString());
		}
	}

	private void OnRemoteStorageGetPublishedFileDetailsResult(RemoteStorageGetPublishedFileDetailsResult_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				_PreFileDetail.m_hFile = pCallback.m_hFile;
				_PreFileDetail.m_hPreviewFile = pCallback.m_hPreviewFile;
				_PreFileDetail.m_nPublishedFileId = pCallback.m_nPublishedFileId;
				_PreFileDetail.m_pchFileName = pCallback.m_pchFileName;
				_PreFileDetail.m_rgchTitle = pCallback.m_rgchTitle;
				_PreFileDetail.m_rgchDescription = pCallback.m_rgchDescription;
				string[] array = pCallback.m_rgchTags.Split(',');
				if (array.Length > 0)
				{
					_PreFileDetail.m_rgchTags = array[array.Length - 1];
				}
				SteamAPICall_t hAPICall = SteamRemoteStorage.UGCDownload(pCallback.m_hPreviewFile, 0u);
				remoteStorageDownloadPreUGCResult.Set(hAPICall);
			}
			else
			{
				Finish(_PublishID, null, bOK: false);
				LogManager.Warning("OnRemoteStorageGetPublishedFileDetailsResult error");
			}
		}
		catch (Exception ex)
		{
			Finish(_PublishID, null, bOK: false);
			Debug.Log("SteamGetPreFileDetailProcess OnRemoteStorageGetPublishedFileDetailsResultAllUser " + ex.ToString());
		}
	}

	private void OnRemoteStorageDownloadPreUGCResult(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				byte[] array = new byte[pCallback.m_nSizeInBytes];
				SteamRemoteStorage.UGCRead(pCallback.m_hFile, array, pCallback.m_nSizeInBytes, 0u, EUGCReadAction.k_EUGCRead_Close);
				_PreFileDetail.m_aPreFileData = ParsingName(array, out _PreFileDetail.m_sUploader);
				GetVoteDetail(_PreFileDetail.m_nPublishedFileId);
			}
			else
			{
				Finish(_PublishID, null, bOK: false);
				LogManager.Warning("OnRemoteStorageDownloadPreUGCResult ", pCallback.m_eResult);
			}
		}
		catch (Exception ex)
		{
			Finish(_PublishID, null, bOK: false);
			Debug.Log("SteamGetPreFileDetailProcess OnRemoteStorageDownloadPreUGCResultAllUser " + ex.ToString());
		}
	}

	public void GetVoteDetail(PublishedFileId_t publishID)
	{
		try
		{
			SteamAPICall_t publishedItemVoteDetails = SteamRemoteStorage.GetPublishedItemVoteDetails(publishID);
			RemoteStorageGetPublishedItemVoteDetailsResult.Set(publishedItemVoteDetails);
		}
		catch (Exception ex)
		{
			Finish(_PublishID, null, bOK: false);
			Debug.Log("SteamGetPreFileDetailProcess GetVoteDetailAllUser " + ex.ToString());
		}
	}

	private void OnRemoteStorageGetPublishedItemVoteDetailsResult(RemoteStorageGetPublishedItemVoteDetailsResult_t pCallback, bool bIOFailure)
	{
		if (CallBackGetPreFileResult == null)
		{
			return;
		}
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			if (_PreFileDetail != null)
			{
				_PreFileDetail.m_VoteDetail.m_fScore = pCallback.m_fScore;
				_PreFileDetail.m_VoteDetail.m_nVotesFor = pCallback.m_nVotesFor;
				_PreFileDetail.m_VoteDetail.m_nVotesAgainst = pCallback.m_nVotesAgainst;
				_PreFileDetail.m_VoteDetail.m_nReports = pCallback.m_nReports;
			}
			Finish(_PublishID, _PreFileDetail, bOK: true);
		}
		else
		{
			Finish(_PublishID, null, bOK: false);
		}
	}

	private byte[] ParsingName(byte[] data, out string uploader)
	{
		uploader = string.Empty;
		int num = data.Length;
		int num2 = -1;
		int num3 = 0;
		for (int num4 = num - 1; num4 > 0; num4--)
		{
			if (num4 < 7)
			{
				return null;
			}
			if (num3 > 256)
			{
				break;
			}
			if (data[num4] == 36 && data[num4 - 1] == 97 && data[num4 - 2] == 119 && data[num4 - 3] == 110 && data[num4 - 4] == 105 && data[num4 - 5] == 115 && data[num4 - 6] == 36)
			{
				num2 = num4 - 6;
			}
			num3++;
		}
		if (num2 > 0)
		{
			byte[] array = new byte[num - num2];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = data[num2 + i];
				data[num2 + i] = 0;
			}
			uploader = Encoding.UTF8.GetString(array);
			uploader = uploader.Replace("$sinwa$", string.Empty);
			byte[] array2 = new byte[num2];
			Array.Copy(data, array2, num2);
			return array2;
		}
		return null;
	}
}
