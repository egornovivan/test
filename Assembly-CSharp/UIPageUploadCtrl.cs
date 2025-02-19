using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using WhiteCat;

public class UIPageUploadCtrl : MonoBehaviour
{
	public GameObject mLeftListItem;

	public UIGrid mleftListGrid;

	public UIPageGridCtrl mUploadGridCtrl;

	public VCEUIStatisticsPanel mRinghtPanelCtrl;

	public GameObject mVCERightPanel;

	public UILabel mLbInfoMsg;

	public UIInput mInput;

	private List<UIWorkShopListItem> mLeftList;

	private int mLeftListSelectedIndex = -1;

	private SteamPreFileAndVoteDetail mSelectedDetail;

	public WorkShopMgr mMyWorkShopMgr;

	[SerializeField]
	private N_ImageButton m_DeleteBtn;

	[SerializeField]
	private N_ImageButton m_DownloadBtn;

	[SerializeField]
	private UIPopupList m_PopupList0;

	private int tempFrame;

	private void Awake()
	{
		InitLeftList();
		mUploadGridCtrl.mUpdateGrid += UpdateUpLoadGrid;
		mUploadGridCtrl.ClickUpload += OnClickWorkShopItem;
		mUploadGridCtrl.ClickUpLoadBtnReLoad += OnClickItemBtnReLoad;
		mMyWorkShopMgr = new WorkShopMgr();
		mMyWorkShopMgr.e_GetItemID += OnGetItemIDList;
		mMyWorkShopMgr.e_GetItemDetail += GetGridItemDetail;
		mMyWorkShopMgr.e_DeleteFile += OnDeleteFile;
		m_DeleteBtn.isEnabled = false;
		m_PopupList0.items.Clear();
		m_PopupList0.items.Add(PELocalization.GetString(8000698));
		m_PopupList0.items.Add(PELocalization.GetString(8000697));
		m_PopupList0.selection = m_PopupList0.items[0];
	}

	private void InitLeftList()
	{
		mLeftList = new List<UIWorkShopListItem>();
		for (int i = 0; i < PEVCConfig.isoNames.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mLeftListItem);
			gameObject.transform.parent = mleftListGrid.gameObject.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			UIWorkShopListItem component = gameObject.GetComponent<UIWorkShopListItem>();
			component.SetText(PEVCConfig.isoNames[i].GetNameByID());
			component.SetIndex(i);
			component.SetSelected(isSelected: false);
			component.ItemClick += OnCheckLeftList;
			mLeftList.Add(component);
		}
		mleftListGrid.repositionNow = true;
	}

	private void Start()
	{
		mUploadGridCtrl.mPagIndex = 0;
		mUploadGridCtrl.mMaxPagIndex = 0;
		mMyWorkShopMgr.InitItemList(isPrevate: true);
		mLeftList[0].SetSelected(isSelected: true);
		mLeftListSelectedIndex = 0;
	}

	private void Update()
	{
		tempFrame++;
		if (tempFrame % 10 == 0)
		{
			UpdateDownLoadState();
			tempFrame = 0;
		}
	}

	private void GetPageGrid()
	{
		mMyWorkShopMgr.GetNewItemList();
	}

	private void OnCheckLeftList(int index)
	{
		if (index >= 0 && index < mLeftList.Count)
		{
			mMyWorkShopMgr.ClearIsoMap();
			if (mLeftListSelectedIndex != -1)
			{
				mLeftList[mLeftListSelectedIndex].SetSelected(isSelected: false);
			}
			mLeftList[index].SetSelected(isSelected: true);
			mLeftListSelectedIndex = index;
			mMyWorkShopMgr.mGetIdStarIndex = 0u;
			mUploadGridCtrl.mPagIndex = 0;
			mUploadGridCtrl.mMaxPagIndex = 0;
			if (mMyWorkShopMgr.mTagList.Count == 1)
			{
				mMyWorkShopMgr.mTagList.Add(PEVCConfig.isoNames[index].Tag);
			}
			else
			{
				mMyWorkShopMgr.mTagList[1] = PEVCConfig.isoNames[index].Tag;
			}
			GetPageGrid();
		}
	}

	private void UpdateDownLoadState()
	{
		int num = Convert.ToInt32(mMyWorkShopMgr.mGetIdStarIndex);
		for (int i = 0; i < mUploadGridCtrl.mUIItems.Count; i++)
		{
			if (!mUploadGridCtrl.mUIItems[i].gameObject.activeSelf || !mMyWorkShopMgr.mIndexMap.ContainsKey(num + i))
			{
				continue;
			}
			PublishedFileId_t key = mMyWorkShopMgr.mIndexMap[num + i];
			if (!mMyWorkShopMgr.mItemsMap.ContainsKey(key))
			{
				continue;
			}
			SteamPreFileAndVoteDetail steamPreFileAndVoteDetail = mMyWorkShopMgr.mItemsMap[key];
			if (steamPreFileAndVoteDetail == null || !mUploadGridCtrl.mUIItems[i].gameObject.activeSelf)
			{
				continue;
			}
			if (SteamProcessMgr.Instance.GetDownProgress(steamPreFileAndVoteDetail.m_hFile, out var downloadedBytes, out var totalBytes))
			{
				int num2 = 0;
				if (totalBytes != 0)
				{
					num2 = Convert.ToInt32(downloadedBytes * 100 / totalBytes);
				}
				if (num2 > 0)
				{
					mUploadGridCtrl.mUIItems[i].UpdteUpDownInfo(num2 + "%");
					mMyWorkShopMgr.mDownMap[key] = num2;
				}
			}
			else if (mMyWorkShopMgr.mDownMap.ContainsKey(key))
			{
				if (mMyWorkShopMgr.mDownMap[key] <= 0)
				{
					mUploadGridCtrl.mUIItems[i].UpdteUpDownInfo(PELocalization.GetString(8000911));
				}
				else if (mMyWorkShopMgr.mDownMap[key] >= 100)
				{
					mUploadGridCtrl.mUIItems[i].SetDownloaded(download: true);
				}
				else
				{
					mUploadGridCtrl.mUIItems[i].UpdteUpDownInfo(mMyWorkShopMgr.mDownMap[key] + "%");
				}
				UpdateDownloadBtnState();
			}
		}
	}

	private void UpdateUpLoadGrid(int index_0)
	{
		if (index_0 < 0)
		{
			index_0 = 0;
		}
		mSelectedDetail = null;
		mVCERightPanel.SetActive(value: false);
		mMyWorkShopMgr.mGetIdStarIndex = Convert.ToUInt32(index_0);
		if (!mMyWorkShopMgr.mIndexMap.ContainsKey(index_0))
		{
			for (int i = 0; i < mUploadGridCtrl.mUIItems.Count; i++)
			{
				mUploadGridCtrl.mUIItems[i].gameObject.SetActive(value: false);
			}
			GetPageGrid();
			return;
		}
		for (int j = 0; j < mUploadGridCtrl.mUIItems.Count; j++)
		{
			if (mMyWorkShopMgr.mIndexMap.ContainsKey(index_0 + j))
			{
				PublishedFileId_t key = mMyWorkShopMgr.mIndexMap[index_0 + j];
				mUploadGridCtrl.mUIItems[j].gameObject.SetActive(value: true);
				if (!mMyWorkShopMgr.mItemsMap.ContainsKey(key))
				{
					SetWorkShopGridItemInfo(null, mUploadGridCtrl.mUIItems[j]);
					continue;
				}
				SteamPreFileAndVoteDetail del = mMyWorkShopMgr.mItemsMap[key];
				if (mMyWorkShopMgr.mDownMap.ContainsKey(key))
				{
					if (mMyWorkShopMgr.mDownMap[key] > 0)
					{
						mUploadGridCtrl.mUIItems[j].ActiveUpDown(isActive: true);
					}
					else
					{
						mUploadGridCtrl.mUIItems[j].ActiveUpDown(isActive: false);
					}
				}
				else
				{
					mUploadGridCtrl.mUIItems[j].ActiveUpDown(isActive: false);
				}
				SetWorkShopGridItemInfo(del, mUploadGridCtrl.mUIItems[j]);
			}
			else
			{
				mUploadGridCtrl.mUIItems[j].gameObject.SetActive(value: false);
			}
			mUploadGridCtrl.mUIItems[j].SetSelected(Selected: false);
		}
	}

	private void SetWorkShopGridItemInfo(SteamPreFileAndVoteDetail del, UIWorkShopGridItem item)
	{
		if (item == null)
		{
			return;
		}
		item.gameObject.SetActive(value: true);
		if (del != null)
		{
			item.SetIsoName(del.m_rgchTitle);
			item.SetDownloaded(UIWorkShopCtrl.CheckDownloadExist(del.m_rgchTitle + VCConfig.s_IsoFileExt));
			if (del.m_aPreFileData != null)
			{
				VCIsoHeadData vCIsoHeadData = default(VCIsoHeadData);
				vCIsoHeadData.SteamPreview = del.m_aPreFileData;
				item.SetAuthor(PELocalization.GetString(8000696));
				Texture2D texture2D = new Texture2D(4, 4);
				texture2D.LoadImage(vCIsoHeadData.IconTex);
				item.SetIco(texture2D);
			}
			else
			{
				item.SetIco(null);
			}
		}
		else
		{
			item.SetIco(null);
			item.SetIsoName(PELocalization.GetString(8000695));
			item.SetAuthor(PELocalization.GetString(8000695));
		}
		item.ActiveLoadingItem(isStar: false);
	}

	private void OnClickWorkShopItem(int index)
	{
		mSelectedDetail = null;
		if (!mMyWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			mVCERightPanel.SetActive(value: false);
			return;
		}
		PublishedFileId_t key = mMyWorkShopMgr.mIndexMap[index];
		if (key.m_PublishedFileId == 0L)
		{
			mVCERightPanel.SetActive(value: false);
			return;
		}
		SteamPreFileAndVoteDetail steamPreFileAndVoteDetail = (mSelectedDetail = ((!mMyWorkShopMgr.mItemsMap.ContainsKey(key)) ? null : mMyWorkShopMgr.mItemsMap[key]));
		if (steamPreFileAndVoteDetail == null)
		{
			mVCERightPanel.SetActive(value: false);
			return;
		}
		mVCERightPanel.SetActive(value: true);
		VCIsoHeadData vCIsoHeadData = default(VCIsoHeadData);
		vCIsoHeadData.SteamPreview = steamPreFileAndVoteDetail.m_aPreFileData;
		Texture2D texture2D = new Texture2D(4, 4);
		texture2D.LoadImage(vCIsoHeadData.IconTex);
		mRinghtPanelCtrl.m_NonEditorIcon = vCIsoHeadData.IconTex;
		mRinghtPanelCtrl.m_NonEditorISODesc = steamPreFileAndVoteDetail.m_rgchDescription;
		mRinghtPanelCtrl.m_NonEditorISOName = steamPreFileAndVoteDetail.m_rgchTitle;
		mRinghtPanelCtrl.m_NonEditorRemark = vCIsoHeadData.Remarks;
		mRinghtPanelCtrl.m_NonEditorISOVersion = steamPreFileAndVoteDetail.m_rgchTags;
		mRinghtPanelCtrl.SetIsoIcon();
		mRinghtPanelCtrl.OnCreationInfoRefresh();
		UpdateDeleteBtnState();
		UpdateDownloadBtnState();
	}

	private void OnClickItemBtnReLoad(int index)
	{
		if (!mMyWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			return;
		}
		PublishedFileId_t p_id = mMyWorkShopMgr.mIndexMap[index];
		if (p_id.m_PublishedFileId != 0L)
		{
			int num = index % mUploadGridCtrl.mMaxGridCount;
			if (num <= mUploadGridCtrl.mMaxGridCount && num >= 0)
			{
				mMyWorkShopMgr.GetItemDetail(p_id);
				mUploadGridCtrl.mUIItems[num].ActiveLoadingItem(isStar: true);
			}
		}
	}

	private void OnGetItemIDList(int count, int allCount)
	{
		mUploadGridCtrl.ReSetGrid(allCount);
		mUploadGridCtrl._UpdatePagText();
		for (int i = 0; i < mUploadGridCtrl.mUIItems.Count; i++)
		{
			if (i < count)
			{
				mUploadGridCtrl.mUIItems[i].ActiveLoadingItem(isStar: true);
				mUploadGridCtrl.mUIItems[i].InitItem(WorkGridItemType.mUpLoad, PELocalization.GetString(8000699));
				mUploadGridCtrl.mUIItems[i].gameObject.SetActive(value: true);
			}
			else
			{
				mUploadGridCtrl.mUIItems[i].gameObject.SetActive(value: false);
			}
			mUploadGridCtrl.mUIItems[i].SetSelected(Selected: false);
		}
		mUploadGridCtrl.mGrid.repositionNow = true;
	}

	private void OnDeleteFile(PublishedFileId_t p_id, bool bOk)
	{
		int num = FindIndexInGrid(p_id);
		if (num != -1)
		{
			int num2 = num % mUploadGridCtrl.mMaxGridCount;
			if (num2 <= mUploadGridCtrl.mMaxGridCount && num2 >= 0)
			{
				SetWorkShopGridItemInfo(null, mUploadGridCtrl.mUIItems[num2]);
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000030));
			}
		}
	}

	private void GetGridItemDetail(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail)
	{
		int num = FindIndexInGrid(p_id);
		if (num != -1)
		{
			int num2 = num % mUploadGridCtrl.mMaxGridCount;
			if (num2 <= mUploadGridCtrl.mMaxGridCount && num2 >= 0)
			{
				SetWorkShopGridItemInfo(detail, mUploadGridCtrl.mUIItems[num2]);
			}
		}
	}

	private int FindIndexInGrid(PublishedFileId_t p_id)
	{
		int result = -1;
		int num = (mUploadGridCtrl.mPagIndex - 1) * mUploadGridCtrl.mMaxGridCount;
		int num2 = mUploadGridCtrl.mPagIndex * mUploadGridCtrl.mMaxGridCount;
		foreach (KeyValuePair<int, PublishedFileId_t> item in mMyWorkShopMgr.mIndexMap)
		{
			if (item.Value == p_id && item.Key >= num && item.Key < num2)
			{
				result = item.Key;
				break;
			}
		}
		return result;
	}

	private void BtnDownloadOnClick()
	{
		if (mSelectedDetail == null || !mMyWorkShopMgr.DownLoadFile(mSelectedDetail))
		{
			return;
		}
		int num = FindIndexInGrid(mSelectedDetail.m_nPublishedFileId);
		if (num != -1)
		{
			int num2 = num % mUploadGridCtrl.mMaxGridCount;
			if (num2 <= mUploadGridCtrl.mMaxGridCount && num2 >= 0)
			{
				mUploadGridCtrl.mUIItems[num2].ActiveUpDown(isActive: true);
				mUploadGridCtrl.mUIItems[num2].UpdteUpDownInfo("0%");
			}
		}
	}

	private void BtnQueryOnClick()
	{
		mMyWorkShopMgr.mQuereText = mInput.text;
		GetPageGrid();
	}

	private void BtnQueryClearOnClick()
	{
		mInput.text = string.Empty;
		if (mMyWorkShopMgr.mQuereText.Length != 0)
		{
			mMyWorkShopMgr.mQuereText = string.Empty;
			GetPageGrid();
		}
	}

	private void OnPop1SelectionChange(string strText)
	{
		EWorkshopEnumerationType eWorkshopEnumerationType;
		if (strText == PELocalization.GetString(8000698))
		{
			eWorkshopEnumerationType = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote;
		}
		else
		{
			if (!(strText == PELocalization.GetString(8000697)))
			{
				return;
			}
			eWorkshopEnumerationType = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent;
		}
		if (eWorkshopEnumerationType != mMyWorkShopMgr.mIdEnumType)
		{
			mMyWorkShopMgr.mIdEnumType = eWorkshopEnumerationType;
			GetPageGrid();
		}
	}

	private void OnPop2SelectionChange(string strText)
	{
		uint num = 0u;
		if (strText == "All time")
		{
			num = 0u;
		}
		switch (strText)
		{
		default:
			return;
		case "Today":
			num = 1u;
			break;
		case "This week":
			num = 7u;
			break;
		case "This month":
			num = 31u;
			break;
		}
		if (mMyWorkShopMgr.mGetIdDays != num)
		{
			mMyWorkShopMgr.mGetIdDays = num;
			GetPageGrid();
		}
	}

	private void BtnDeleteOnClick()
	{
		if (mSelectedDetail == null)
		{
			return;
		}
		float result = 0f;
		float num = float.Parse(SteamWorkShop.NewVersionTag);
		if (float.TryParse(mSelectedDetail.m_rgchTags, out result) && result >= num)
		{
			MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000492), num));
			return;
		}
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000015), delegate
		{
			mMyWorkShopMgr.DeleteMyIsoFile(mSelectedDetail);
		});
	}

	private void UpdateDeleteBtnState()
	{
		if (mSelectedDetail != null)
		{
			float result = 0f;
			float num = float.Parse(SteamWorkShop.NewVersionTag);
			if (float.TryParse(mSelectedDetail.m_rgchTags, out result) && result >= num)
			{
				m_DeleteBtn.isEnabled = false;
				return;
			}
		}
		m_DeleteBtn.isEnabled = true;
	}

	private void UpdateDownloadBtnState()
	{
		if (mSelectedDetail != null)
		{
			m_DownloadBtn.isEnabled = !UIWorkShopCtrl.CheckDownloadExist(mSelectedDetail.m_rgchTitle + VCConfig.s_IsoFileExt);
		}
		else
		{
			m_DownloadBtn.isEnabled = true;
		}
	}

	public void SetItemIsDownloadedByFileName(string fileName)
	{
		UIWorkShopGridItem workShopItemByFileName = mUploadGridCtrl.GetWorkShopItemByFileName(fileName);
		if (null != workShopItemByFileName)
		{
			workShopItemByFileName.SetDownloaded(download: true);
		}
	}
}
