using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamSearchProcess : ISteamGetFile
{
	private CallResult<SteamUGCQueryCompleted_t> OnSteamUGCQueryCompletedCallResult;

	private UGCQueryHandle_t _UGCQueryHandle;

	private SteamAPICall_t _callbackHandle;

	private GetPreListCallBackEventHandler CallBackGetPreListResult;

	public List<PublishedFileId_t> _FileIDLsit = new List<PublishedFileId_t>();

	private uint _StartIndex;

	private uint _Count;

	private uint _Page;

	public SteamSearchProcess(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex, string[] tags, uint count = 9, string searchText = "")
	{
		OnSteamUGCQueryCompletedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(CallBackSendQuery);
		_Page = startIndex / 50 + 1;
		_StartIndex = startIndex;
		CallBackGetPreListResult = callBackGetPreListResult;
		_Count = count;
		tags = SteamWorkShop.AddNewVersionTag(tags);
		Search(tags, searchText);
	}

	private void Finish(List<PublishedFileId_t> publishIDList, int totalResults, uint startIndex, bool bOK)
	{
		if (CallBackGetPreListResult != null)
		{
			CallBackGetPreListResult(publishIDList, totalResults, startIndex, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void Search(string[] tags, string searchText)
	{
		try
		{
			if (!SteamUser.BLoggedOn())
			{
				return;
			}
			_UGCQueryHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByVote, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, SteamUtils.GetAppID(), SteamUtils.GetAppID(), _Page);
			SteamUGC.SetSearchText(_UGCQueryHandle, searchText);
			if (tags != null && tags.Length > 0)
			{
				for (int i = 0; i < tags.Length; i++)
				{
					SteamUGC.AddRequiredTag(_UGCQueryHandle, tags[i]);
				}
			}
			_callbackHandle = SteamUGC.SendQueryUGCRequest(_UGCQueryHandle);
			OnSteamUGCQueryCompletedCallResult.Set(_callbackHandle);
			ISteamGetFile.ProcessList.Remove(this);
		}
		catch (Exception)
		{
			Finish(null, 0, _StartIndex, bOK: false);
		}
	}

	public void CallBackSendQuery(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
	{
		try
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				uint num = pCallback.m_unNumResultsReturned;
				uint num2 = _StartIndex % 50;
				if (num - num2 > _Count)
				{
					num = _Count;
				}
				SteamUGCDetails_t pDetails = default(SteamUGCDetails_t);
				for (uint num3 = num2; num3 < num + num2; num3++)
				{
					SteamUGC.GetQueryUGCResult(_UGCQueryHandle, num3, out pDetails);
					_FileIDLsit.Add(pDetails.m_nPublishedFileId);
					Debug.LogWarning("CallBackSendQuery PublishedFileId_t " + num3.ToString() + " = " + pDetails.m_nPublishedFileId);
				}
				Finish(_FileIDLsit, (int)pCallback.m_unTotalMatchingResults, _StartIndex, bOK: true);
			}
			else
			{
				Finish(null, 0, _StartIndex, bOK: false);
			}
		}
		catch (Exception)
		{
			Finish(null, 0, _StartIndex, bOK: false);
		}
	}
}
