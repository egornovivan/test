using System;
using Steamworks;
using UnityEngine;

public class SteamRefreshVoteDetailProcess : ISteamGetFile
{
	private CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t> RemoteStorageGetPublishedItemVoteDetailsResult;

	private SteamVoteDetailEventHandler CallBackRefreshVoteDetail;

	private SteamVoteDetail _VoteDetail;

	private PublishedFileId_t _PublishID;

	public SteamRefreshVoteDetailProcess(SteamVoteDetailEventHandler callBackRefreshVoteDetail, PublishedFileId_t publishID)
	{
		RemoteStorageGetPublishedItemVoteDetailsResult = CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t>.Create(OnRemoteStorageGetPublishedItemVoteDetailsResult);
		CallBackRefreshVoteDetail = callBackRefreshVoteDetail;
		RefreshVoteDetail(publishID);
		_VoteDetail = new SteamVoteDetail();
		_PublishID = publishID;
	}

	private void Finish(PublishedFileId_t publishedFileId, SteamVoteDetail detail, bool bOK)
	{
		if (CallBackRefreshVoteDetail != null)
		{
			CallBackRefreshVoteDetail(publishedFileId, detail, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void RefreshVoteDetail(PublishedFileId_t publishID)
	{
		try
		{
			SteamAPICall_t publishedItemVoteDetails = SteamRemoteStorage.GetPublishedItemVoteDetails(publishID);
			RemoteStorageGetPublishedItemVoteDetailsResult.Set(publishedItemVoteDetails);
		}
		catch (Exception ex)
		{
			Finish(_PublishID, null, bOK: false);
			Debug.Log("SteamRefreshVoteDetailProcess RefreshVoteDetail " + ex.ToString());
		}
	}

	private void OnRemoteStorageGetPublishedItemVoteDetailsResult(RemoteStorageGetPublishedItemVoteDetailsResult_t pCallback, bool bIOFailure)
	{
		if (CallBackRefreshVoteDetail == null)
		{
			return;
		}
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			if (_VoteDetail != null)
			{
				_VoteDetail.m_fScore = pCallback.m_fScore;
				_VoteDetail.m_nVotesFor = pCallback.m_nVotesFor;
				_VoteDetail.m_nVotesAgainst = pCallback.m_nVotesAgainst;
				_VoteDetail.m_nReports = pCallback.m_nReports;
			}
			Finish(_PublishID, _VoteDetail, bOK: true);
		}
		else
		{
			Finish(_PublishID, null, bOK: false);
		}
	}
}
