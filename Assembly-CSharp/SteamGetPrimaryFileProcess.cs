using System;
using Steamworks;
using UnityEngine;

public class SteamGetPrimaryFileProcess : ISteamGetFile
{
	private CallResult<RemoteStorageDownloadUGCResult_t> RemoteStorageDownloadUGCResult;

	private int _index;

	private int _dungeonId;

	private PublishedFileId_t _PublishedFileId;

	private GetPrimaryFileResultEventHandler CallBackGetPrimaryFileResult;

	public SteamGetPrimaryFileProcess(GetPrimaryFileResultEventHandler callBackGetPrimaryFileResult, UGCHandle_t file, PublishedFileId_t publishedFileId, int index = -1, int dungeonId = -1)
	{
		RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnRemoteStorageDownloadFileUGCResultPrimaryFile);
		CallBackGetPrimaryFileResult = callBackGetPrimaryFileResult;
		GetPrimaryFile(file);
		_PublishedFileId = publishedFileId;
		_index = index;
		_dungeonId = dungeonId;
	}

	private void Finish(byte[] fileData, PublishedFileId_t publishedFileId, bool bOK)
	{
		if (CallBackGetPrimaryFileResult != null)
		{
			CallBackGetPrimaryFileResult(fileData, publishedFileId, bOK, _index, _dungeonId);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void GetPrimaryFile(UGCHandle_t file)
	{
		try
		{
			if (file != UGCHandle_t.Invalid)
			{
				SteamAPICall_t hAPICall = SteamRemoteStorage.UGCDownload(file, 0u);
				RemoteStorageDownloadUGCResult.Set(hAPICall);
			}
			else
			{
				LogManager.Warning("GetFile error");
				Finish(null, _PublishedFileId, bOK: false);
			}
		}
		catch (Exception ex)
		{
			Finish(null, _PublishedFileId, bOK: false);
			Debug.Log("SteamGetPrimaryFileProcess GetPrimaryFile " + ex.ToString());
		}
	}

	private void OnRemoteStorageDownloadFileUGCResultPrimaryFile(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure)
	{
		if (CallBackGetPrimaryFileResult == null)
		{
			return;
		}
		try
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				byte[] array = new byte[pCallback.m_nSizeInBytes];
				SteamRemoteStorage.UGCRead(pCallback.m_hFile, array, pCallback.m_nSizeInBytes, 0u, EUGCReadAction.k_EUGCRead_Close);
				Finish(array, _PublishedFileId, bOK: true);
			}
			else
			{
				Finish(null, _PublishedFileId, bOK: false);
				LogManager.Warning("OnRemoteStorageDownloadFileUGCResult error");
			}
		}
		catch (Exception ex)
		{
			Finish(null, _PublishedFileId, bOK: false);
			Debug.Log("SteamGetPrimaryFileProcess OnRemoteStorageDownloadFileUGCResultPrimaryFile " + ex.ToString());
		}
	}
}
