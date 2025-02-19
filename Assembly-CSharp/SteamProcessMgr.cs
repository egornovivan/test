using Steamworks;

public class SteamProcessMgr : ISteamGetFile
{
	private static SteamProcessMgr _instance = new SteamProcessMgr();

	internal static SteamProcessMgr Instance => _instance;

	private SteamProcessMgr()
	{
		_instance = this;
	}

	private void Awake()
	{
		_instance = this;
	}

	public void GetMyPreFileList(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex, string[] tags, uint days = 0, uint count = 9, EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote, string searchText = "")
	{
		if (searchText.CompareTo(string.Empty) == 0)
		{
			SteamGetMyPreListProcess item = new SteamGetMyPreListProcess(callBackGetPreListResult, startIndex, tags, count);
			ISteamGetFile.ProcessList.Add(item);
		}
		else
		{
			SteamSearchMyProcess item2 = new SteamSearchMyProcess(callBackGetPreListResult, startIndex, tags, count, searchText);
			ISteamGetFile.ProcessList.Add(item2);
		}
	}

	public void GetPreFileList(GetPreListCallBackEventHandler callBackGetPreList, uint startIndex, string[] tags, uint days = 0, uint count = 9, EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote, string searchText = "")
	{
		if (searchText.CompareTo(string.Empty) == 0)
		{
			SteamGetPreFileListProcess item = new SteamGetPreFileListProcess(callBackGetPreList, startIndex, tags, days, count, orderBy);
			ISteamGetFile.ProcessList.Add(item);
		}
		else
		{
			SteamSearchProcess item2 = new SteamSearchProcess(callBackGetPreList, startIndex, tags, count, searchText);
			ISteamGetFile.ProcessList.Add(item2);
		}
	}

	public void GetPreFileDetail(SteamGetPreFileCallBackEventHandler callBackGetPreFileResult, PublishedFileId_t publishID)
	{
		SteamGetPreFileDetailProcess item = new SteamGetPreFileDetailProcess(callBackGetPreFileResult, publishID);
		ISteamGetFile.ProcessList.Add(item);
	}

	public void GetPrimaryFile(GetPrimaryFileResultEventHandler callBackDownloadFileUGCResult, UGCHandle_t file, PublishedFileId_t publishID, int index = -1, int dungeonId = -1)
	{
		SteamGetPrimaryFileProcess item = new SteamGetPrimaryFileProcess(callBackDownloadFileUGCResult, file, publishID, index, dungeonId);
		ISteamGetFile.ProcessList.Add(item);
	}

	public void Vote(VoteResultEventHandler callBackVoteResult, PublishedFileId_t publishID, bool bFor)
	{
		SteamVoteProcess item = new SteamVoteProcess(callBackVoteResult, publishID, bFor);
		ISteamGetFile.ProcessList.Add(item);
	}

	public void DeleteFile(DeleteFileResultEventHandler callBackDeleteFileResult, string fileName, PublishedFileId_t publishID)
	{
		SteamDeleteProcess item = new SteamDeleteProcess(callBackDeleteFileResult, fileName, publishID);
		ISteamGetFile.ProcessList.Add(item);
	}

	public void RefreshVoteDetail(SteamVoteDetailEventHandler callBackRefreshVoteDetail, PublishedFileId_t publishID)
	{
		SteamRefreshVoteDetailProcess item = new SteamRefreshVoteDetailProcess(callBackRefreshVoteDetail, publishID);
		ISteamGetFile.ProcessList.Add(item);
	}

	public bool GetDownProgress(UGCHandle_t file, out int downloadedBytes, out int totalBytes)
	{
		return SteamRemoteStorage.GetUGCDownloadProgress(file, out downloadedBytes, out totalBytes);
	}

	public void RandomGetIsosFromWorkShop(GetRandomIsoListCallBackEventHandler callback, int amount, int dungeonId, string tag)
	{
		SteamRandomGetIsoProcess item = new SteamRandomGetIsoProcess(callback, amount, dungeonId, tag);
		ISteamGetFile.ProcessList.Add(item);
	}
}
