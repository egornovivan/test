using System.Collections.Generic;
using Steamworks;

public class ISteamGetFile
{
	public delegate void SteamGetPreFileCallBackEventHandler(PublishedFileId_t publishedFileId, SteamPreFileAndVoteDetail detail, bool bOK);

	public delegate void GetPrimaryFileResultEventHandler(byte[] fileData, PublishedFileId_t publishedFileId, bool bOK, int index, int dungeonId = -1);

	public delegate void VoteResultEventHandler(PublishedFileId_t publishID, bool bFor, bool bOK);

	public delegate void DeleteFileResultEventHandler(string fileName, PublishedFileId_t publishID, bool bOK);

	public delegate void GetPreListCallBackEventHandler(List<PublishedFileId_t> publishIDList, int totalResults, uint startIndex, bool bOK);

	public delegate void GetRandomIsoListCallBackEventHandler(List<ulong> fileIDsList, List<ulong> publishIds, int dungeonId, bool bOK);

	public delegate void PublishedFileDetailsResultEventHandler(PublishedFileId_t fileIDList, UGCHandle_t fileHandle, UGCHandle_t preFileHandle, bool bOK);

	public delegate void DownloadPreUGCResultEventHandler(byte[] fileByte, string fileName, bool bOK);

	public delegate void SteamPreFileAndVoteDetailEventHandler(SteamPreFileAndVoteDetail detail, bool bOK);

	public delegate void SteamVoteDetailEventHandler(PublishedFileId_t publishedFileId, SteamVoteDetail detail, bool bOK);

	public static List<ISteamGetFile> ProcessList = new List<ISteamGetFile>();
}
