using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

public class WorkShopMgr
{
	public delegate void _GetItemID(int ItemCount, int allItemCount);

	public delegate void _GetItemDetail(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail);

	public delegate void _VoteDetailCallBack(PublishedFileId_t p_id, bool bFor);

	public delegate void _DeleteFile(PublishedFileId_t p_id, bool bOk);

	public bool isActve;

	public EWorkshopEnumerationType mIdEnumType;

	public uint mGetIdStarIndex;

	public uint mGetIdDays;

	public string mQuereText;

	public int mTatileCount;

	public List<string> mTagList = new List<string>();

	public Dictionary<int, PublishedFileId_t> mIndexMap;

	public Dictionary<PublishedFileId_t, SteamPreFileAndVoteDetail> mItemsMap;

	public Dictionary<PublishedFileId_t, int> mGetCountMap;

	public Dictionary<PublishedFileId_t, byte> mVoteMap;

	public Dictionary<PublishedFileId_t, int> mDownMap;

	private bool mIsPrivate;

	public uint mGetIdCount => UIWorkShopCtrl.GetCurRequestCount();

	public event _GetItemID e_GetItemID;

	public event _GetItemDetail e_GetItemDetail;

	public event _VoteDetailCallBack e_VoteDetailCallBack;

	public event _DeleteFile e_DeleteFile;

	public WorkShopMgr()
	{
		mIndexMap = new Dictionary<int, PublishedFileId_t>();
		mItemsMap = new Dictionary<PublishedFileId_t, SteamPreFileAndVoteDetail>();
		mGetCountMap = new Dictionary<PublishedFileId_t, int>();
		mVoteMap = new Dictionary<PublishedFileId_t, byte>();
		mDownMap = new Dictionary<PublishedFileId_t, int>();
		mIdEnumType = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote;
		mGetIdStarIndex = 0u;
		mGetIdDays = 0u;
		mTagList.Add("Creation");
		mQuereText = string.Empty;
		isActve = true;
	}

	public void ClearIsoMap()
	{
		mIndexMap.Clear();
		mItemsMap.Clear();
		mGetCountMap.Clear();
	}

	public void InitItemList(bool isPrevate)
	{
		mIndexMap.Clear();
		mItemsMap.Clear();
		mGetCountMap.Clear();
		mGetIdStarIndex = 0u;
		mIsPrivate = isPrevate;
		GetNewItemList();
	}

	public void GetNewItemList()
	{
		if (mIsPrivate)
		{
			SteamProcessMgr.Instance.GetMyPreFileList(GetItemCallBack, mGetIdStarIndex, mTagList.ToArray(), mGetIdDays, mGetIdCount, mIdEnumType, mQuereText);
		}
		else
		{
			SteamProcessMgr.Instance.GetPreFileList(GetItemCallBack, mGetIdStarIndex, mTagList.ToArray(), mGetIdDays, mGetIdCount, mIdEnumType, mQuereText);
		}
	}

	private void GetItemCallBack(List<PublishedFileId_t> mIdLsit, int totalResults, uint starIndex, bool bOK)
	{
		if (mIdLsit != null && bOK && isActve)
		{
			for (int i = 0; i < mIdLsit.Count; i++)
			{
				mIndexMap[Convert.ToInt32(starIndex + i)] = mIdLsit[i];
			}
			if (this.e_GetItemID != null)
			{
				this.e_GetItemID(mIdLsit.Count, totalResults);
			}
			for (int j = 0; j < mIdLsit.Count; j++)
			{
				mGetCountMap[mIdLsit[j]] = 0;
				GetItemDetail(mIdLsit[j]);
			}
		}
	}

	public void GetItemDetail(PublishedFileId_t p_id)
	{
		Dictionary<PublishedFileId_t, int> dictionary;
		Dictionary<PublishedFileId_t, int> dictionary2 = (dictionary = mGetCountMap);
		PublishedFileId_t key;
		PublishedFileId_t key2 = (key = p_id);
		int num = dictionary[key];
		dictionary2[key2] = num + 1;
		SteamProcessMgr.Instance.GetPreFileDetail(GetItemDetailCallBack, p_id);
	}

	private void GetItemDetailCallBack(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail, bool bOK)
	{
		if (!isActve)
		{
			return;
		}
		if (bOK)
		{
			mItemsMap[p_id] = detail;
		}
		else
		{
			if (!mGetCountMap.ContainsKey(p_id))
			{
				mGetCountMap[p_id] = 0;
			}
			if (mGetCountMap[p_id] <= 3)
			{
				GetItemDetail(p_id);
				return;
			}
			mItemsMap[p_id] = null;
		}
		if (this.e_GetItemDetail != null)
		{
			this.e_GetItemDetail(p_id, detail);
		}
	}

	public void Vote(PublishedFileId_t p_id, bool bFor)
	{
		if (!mVoteMap.ContainsKey(p_id) || ((!bFor || mVoteMap[p_id] != 1) && (bFor || mVoteMap[p_id] != 2)))
		{
			SteamProcessMgr.Instance.Vote(OnVoteCallBack, p_id, bFor);
		}
	}

	private void OnVoteCallBack(PublishedFileId_t p_id, bool bFor, bool bOK)
	{
		if (!isActve)
		{
			return;
		}
		if (bOK)
		{
			if (bFor)
			{
				mVoteMap[p_id] = 1;
			}
			else
			{
				mVoteMap[p_id] = 2;
			}
			if (this.e_VoteDetailCallBack != null)
			{
				this.e_VoteDetailCallBack(p_id, bFor);
			}
		}
		else
		{
			mVoteMap[p_id] = 0;
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000491));
		}
	}

	public bool DownLoadFile(SteamPreFileAndVoteDetail detail)
	{
		if (mDownMap.ContainsKey(detail.m_nPublishedFileId) && mDownMap[detail.m_nPublishedFileId] >= 0)
		{
			return false;
		}
		mDownMap[detail.m_nPublishedFileId] = 0;
		SteamProcessMgr.Instance.GetPrimaryFile(OnDownLoadFileCallBack, detail.m_hFile, detail.m_nPublishedFileId);
		return true;
	}

	private void OnDownLoadFileCallBack(byte[] fileData, PublishedFileId_t p_id, bool bOK, int index = -1, int dungeonId = -1)
	{
		if (!isActve)
		{
			return;
		}
		if (bOK)
		{
			if (mItemsMap.ContainsKey(p_id) && mItemsMap[p_id] != null)
			{
				string filePath = VCConfig.s_IsoPath + "/Download/";
				string s_CreationNetCachePath = VCConfig.s_CreationNetCachePath;
				string rgchTitle = mItemsMap[p_id].m_rgchTitle;
				string fileName = CRC64.Compute(fileData).ToString();
				if (SaveToFile(fileData, rgchTitle, filePath, VCConfig.s_IsoFileExt))
				{
					UIWorkShopCtrl.AddDownloadFileName(mItemsMap[p_id].m_rgchTitle + VCConfig.s_IsoFileExt, mIsPrivate);
					mDownMap[p_id] = 100;
				}
				SaveToFile(fileData, fileName, s_CreationNetCachePath, VCConfig.s_CreationNetCacheFileExt);
			}
		}
		else
		{
			mDownMap[p_id] = -1;
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000493));
		}
	}

	public bool DeleteMyIsoFile(SteamPreFileAndVoteDetail detail)
	{
		if (detail == null || !mItemsMap.ContainsKey(detail.m_nPublishedFileId))
		{
			return false;
		}
		SteamProcessMgr.Instance.DeleteFile(DeleteMyIsoFileCallBack, detail.m_pchFileName, detail.m_nPublishedFileId);
		return true;
	}

	public void DeleteMyIsoFileCallBack(string fileName, PublishedFileId_t p_id, bool bOK)
	{
		if (isActve && this.e_DeleteFile != null && bOK)
		{
			this.e_DeleteFile(p_id, bOK);
		}
	}

	private bool SaveToFile(byte[] fileData, string fileName, string filePath, string fileExt)
	{
		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}
		fileName = UIWorkShopCtrl.GetValidFileName(fileName);
		string text = filePath;
		filePath = filePath + fileName + fileExt;
		int num = 0;
		while (File.Exists(filePath))
		{
			num++;
			filePath = text + fileName + num + fileExt;
		}
		try
		{
			using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				binaryWriter.Write(fileData);
				binaryWriter.Close();
				fileStream.Close();
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError($"Save ISO to filepath:{filePath} Error:{ex.ToString()}");
			return false;
		}
	}
}
