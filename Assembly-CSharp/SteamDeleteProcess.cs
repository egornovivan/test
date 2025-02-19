using System;
using Steamworks;
using UnityEngine;

public class SteamDeleteProcess : ISteamGetFile
{
	private DeleteFileResultEventHandler CallBackDeleteFileResult;

	public SteamDeleteProcess(DeleteFileResultEventHandler callBackDeleteFileResult, string fileName, PublishedFileId_t publishID)
	{
		CallBackDeleteFileResult = callBackDeleteFileResult;
		DeleteFile(fileName, publishID);
	}

	private void Finish(string fileName, PublishedFileId_t publishID, bool bOK)
	{
		if (CallBackDeleteFileResult != null)
		{
			CallBackDeleteFileResult(fileName, publishID, bOK);
		}
		ISteamGetFile.ProcessList.Remove(this);
	}

	public void DeleteFile(string fileName, PublishedFileId_t publishID)
	{
		try
		{
			if ((fileName == null || fileName.Length == 0) && publishID.m_PublishedFileId == 0L)
			{
				Finish(fileName, publishID, bOK: false);
				return;
			}
			if (fileName != null && fileName.Length != 0)
			{
				string pchFile = fileName + "_preview";
				bool flag = SteamRemoteStorage.FileDelete(fileName);
				Debug.Log("--------------------------------------------------------------------" + flag);
				flag = SteamRemoteStorage.FileDelete(pchFile);
				Debug.Log(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,," + flag);
			}
			if (publishID.m_PublishedFileId != 0L)
			{
				SteamRemoteStorage.DeletePublishedFile(publishID);
			}
			Finish(fileName, publishID, bOK: true);
		}
		catch (Exception ex)
		{
			Finish(fileName, publishID, bOK: false);
			Debug.Log("SteamDeleteProcess DeleteFile " + ex.ToString());
		}
	}
}
