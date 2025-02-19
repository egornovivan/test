using System;
using Steamworks;
using UnityEngine;

public class SteamVoteProcess : ISteamGetFile
{
	private CallResult<RemoteStorageUpdateUserPublishedItemVoteResult_t> RemoteStorageUpdateUserPublishedItemVoteResult;

	private VoteResultEventHandler CallBackVoteResult;

	private PublishedFileId_t _PublishID;

	private bool _BFor;

	public SteamVoteProcess(VoteResultEventHandler callBackVoteResult, PublishedFileId_t publishID, bool bFor)
	{
		CallBackVoteResult = callBackVoteResult;
		RemoteStorageUpdateUserPublishedItemVoteResult = CallResult<RemoteStorageUpdateUserPublishedItemVoteResult_t>.Create(OnRemoteStorageUpdateUserPublishedItemVoteResultVote);
		Vote(publishID, bFor);
		_PublishID = publishID;
		_BFor = bFor;
	}

	private void Finish(PublishedFileId_t publishID, bool bFor, bool bOK)
	{
		if (CallBackVoteResult != null)
		{
			CallBackVoteResult(publishID, bFor, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void Vote(PublishedFileId_t publishID, bool bFor)
	{
		try
		{
			if (publishID.m_PublishedFileId == 0L)
			{
				ISteamGetFile.ProcessList.Remove(this);
				return;
			}
			SteamAPICall_t hAPICall = SteamRemoteStorage.UpdateUserPublishedItemVote(publishID, bVoteUp: true);
			RemoteStorageUpdateUserPublishedItemVoteResult.Set(hAPICall);
		}
		catch (Exception ex)
		{
			Finish(_PublishID, _BFor, bOK: false);
			Debug.Log("SteamVoteProcess Vote " + ex.ToString());
		}
	}

	private void OnRemoteStorageUpdateUserPublishedItemVoteResultVote(RemoteStorageUpdateUserPublishedItemVoteResult_t pCallback, bool bIOFailure)
	{
		if (CallBackVoteResult != null)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Finish(_PublishID, _BFor, bOK: true);
			}
			else
			{
				Finish(_PublishID, _BFor, bOK: false);
			}
		}
	}
}
