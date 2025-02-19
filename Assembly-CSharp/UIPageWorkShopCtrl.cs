using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using WhiteCat;

public class UIPageWorkShopCtrl : MonoBehaviour
{
	public GameObject mLeftListItem;

	public UIGrid mleftListGrid;

	public UIPageGridCtrl mWorkShopGridCtrl;

	public WorkShopMgr mWorkShopMgr;

	public VCEUIStatisticsPanel mRinghtPanelCtrl;

	public GameObject mVCERightPanel;

	public UILabel mLbInfoMsg;

	public UIInput mInput;

	[SerializeField]
	private N_ImageButton m_DownloadBtn;

	[SerializeField]
	private UIPopupList m_PopupList0;

	private List<UIWorkShopListItem> mLeftList;

	private int mLeftListSelectedIndex = -1;

	private SteamPreFileAndVoteDetail mSelectedDetail;

	private List<int> m_DingList;

	private List<int> m_CaiList;

	private uint mGetIdCount;

	private int tempFrame;

	private void Awake()
	{
		InitLeftList();
		mWorkShopGridCtrl.mUpdateGrid += UpdateWorkShopGrid;
		mWorkShopGridCtrl.ClickWorkShop += OnClickWorkShopItem;
		mWorkShopGridCtrl.ClickWorkShopBtnReLoad += OnClickItemBtnReLoad;
		mWorkShopGridCtrl.ClickWorkShopBtnDing += OnClickItemBtnDing;
		mWorkShopGridCtrl.ClickWorkShopBtnCai += OnClickItemBtnCai;
		mWorkShopMgr = new WorkShopMgr();
		mWorkShopMgr.e_GetItemID += OnGetItemIDList;
		mWorkShopMgr.e_GetItemDetail += GetGridItemDetail;
		mWorkShopMgr.e_VoteDetailCallBack += OnVoteCallBack;
		m_DingList = new List<int>();
		m_CaiList = new List<int>();
		m_DownloadBtn.isEnabled = false;
		m_PopupList0.items.Clear();
		m_PopupList0.items.Add(PELocalization.GetString(8000698));
		m_PopupList0.items.Add(PELocalization.GetString(8000697));
		m_PopupList0.selection = m_PopupList0.items[0];
	}

	private void Start()
	{
		mWorkShopGridCtrl.mPagIndex = 0;
		mWorkShopGridCtrl.mMaxPagIndex = 0;
		mWorkShopMgr.InitItemList(isPrevate: false);
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
			component.ItemClick += OnChickLeftList;
			mLeftList.Add(component);
		}
		mleftListGrid.repositionNow = true;
	}

	private void GetPageGrid()
	{
		mWorkShopMgr.GetNewItemList();
	}

	private void UpdateDownLoadState()
	{
		int num = Convert.ToInt32(mWorkShopMgr.mGetIdStarIndex);
		for (int i = 0; i < mWorkShopGridCtrl.mUIItems.Count; i++)
		{
			if (!mWorkShopGridCtrl.mUIItems[i].gameObject.activeSelf || !mWorkShopMgr.mIndexMap.ContainsKey(num + i))
			{
				continue;
			}
			PublishedFileId_t key = mWorkShopMgr.mIndexMap[num + i];
			if (!mWorkShopMgr.mItemsMap.ContainsKey(key))
			{
				continue;
			}
			SteamPreFileAndVoteDetail steamPreFileAndVoteDetail = mWorkShopMgr.mItemsMap[key];
			if (steamPreFileAndVoteDetail == null || !mWorkShopGridCtrl.mUIItems[i].gameObject.activeSelf)
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
					mWorkShopGridCtrl.mUIItems[i].UpdteUpDownInfo(num2 + "%");
					mWorkShopMgr.mDownMap[key] = num2;
				}
			}
			else if (mWorkShopMgr.mDownMap.ContainsKey(key))
			{
				if (mWorkShopMgr.mDownMap[key] <= 0)
				{
					mWorkShopGridCtrl.mUIItems[i].UpdteUpDownInfo(PELocalization.GetString(8000911));
				}
				else if (mWorkShopMgr.mDownMap[key] >= 100)
				{
					mWorkShopGridCtrl.mUIItems[i].SetDownloaded(download: true);
				}
				else
				{
					mWorkShopGridCtrl.mUIItems[i].UpdteUpDownInfo(mWorkShopMgr.mDownMap[key] + "%");
				}
				UpdateDownloadBtnState();
			}
		}
	}

	private void UpdateWorkShopGrid(int index_0)
	{
		if (index_0 < 0)
		{
			index_0 = 0;
		}
		mSelectedDetail = null;
		mVCERightPanel.SetActive(value: false);
		mWorkShopMgr.mGetIdStarIndex = Convert.ToUInt32(index_0);
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index_0))
		{
			for (int i = 0; i < mWorkShopGridCtrl.mUIItems.Count; i++)
			{
				mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(value: false);
				mWorkShopGridCtrl.mUIItems[i].ActiveUpDown(isActive: false);
			}
			GetPageGrid();
			return;
		}
		for (int j = 0; j < mWorkShopGridCtrl.mUIItems.Count; j++)
		{
			if (mWorkShopMgr.mIndexMap.ContainsKey(index_0 + j))
			{
				PublishedFileId_t key = mWorkShopMgr.mIndexMap[index_0 + j];
				mWorkShopGridCtrl.mUIItems[j].gameObject.SetActive(value: true);
				if (!mWorkShopMgr.mItemsMap.ContainsKey(key))
				{
					SetWorkShopGridItemInfo(null, mWorkShopGridCtrl.mUIItems[j]);
					continue;
				}
				SteamPreFileAndVoteDetail del = mWorkShopMgr.mItemsMap[key];
				if (mWorkShopMgr.mDownMap.ContainsKey(key))
				{
					if (mWorkShopMgr.mDownMap[key] > 0)
					{
						mWorkShopGridCtrl.mUIItems[j].ActiveUpDown(isActive: true);
					}
					else
					{
						mWorkShopGridCtrl.mUIItems[j].ActiveUpDown(isActive: false);
					}
				}
				else
				{
					mWorkShopGridCtrl.mUIItems[j].ActiveUpDown(isActive: false);
				}
				SetWorkShopGridItemInfo(del, mWorkShopGridCtrl.mUIItems[j]);
			}
			else
			{
				mWorkShopGridCtrl.mUIItems[j].gameObject.SetActive(value: false);
			}
			mWorkShopGridCtrl.mUIItems[j].SetSelected(Selected: false);
		}
	}

	private void OnGetItemIDList(int count, int allCount)
	{
		mWorkShopGridCtrl.ReSetGrid(allCount);
		mWorkShopGridCtrl._UpdatePagText();
		for (int i = 0; i < mWorkShopGridCtrl.mUIItems.Count; i++)
		{
			if (i < count)
			{
				mWorkShopGridCtrl.mUIItems[i].ActiveLoadingItem(isStar: true);
				mWorkShopGridCtrl.mUIItems[i].InitItem(WorkGridItemType.mWorkShop, PELocalization.GetString(8000699));
				mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(value: true);
			}
			else
			{
				mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(value: false);
			}
			mWorkShopGridCtrl.mUIItems[i].SetSelected(Selected: false);
		}
		mWorkShopGridCtrl.mGrid.repositionNow = true;
	}

	private void GetGridItemDetail(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail)
	{
		int num = FindIndexInGrid(p_id);
		if (num != -1)
		{
			int num2 = num % mWorkShopGridCtrl.mMaxGridCount;
			if (num2 <= mWorkShopGridCtrl.mMaxGridCount && num2 >= 0)
			{
				SetWorkShopGridItemInfo(detail, mWorkShopGridCtrl.mUIItems[num2]);
			}
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
			item.SetAuthor(del.m_sUploader);
			item.ActiveVoteUI(isActive: true);
			item.SetDingText(del.m_VoteDetail.m_nVotesFor.ToString());
			item.SetCaiText(del.m_VoteDetail.m_nVotesAgainst.ToString());
			item.SetIsoName(del.m_rgchTitle);
			item.SetDownloaded(UIWorkShopCtrl.CheckDownloadExist(del.m_rgchTitle + VCConfig.s_IsoFileExt));
			if (del.m_aPreFileData != null)
			{
				VCIsoHeadData vCIsoHeadData = default(VCIsoHeadData);
				vCIsoHeadData.SteamPreview = del.m_aPreFileData;
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
			item.ActiveVoteUI(isActive: false);
			item.SetIco(null);
			item.SetIsoName(PELocalization.GetString(8000695));
			item.SetAuthor(PELocalization.GetString(8000695));
		}
		item.ActiveLoadingItem(isStar: false);
	}

	private void OnClickWorkShopItem(int index)
	{
		mSelectedDetail = null;
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			mVCERightPanel.SetActive(value: false);
			return;
		}
		PublishedFileId_t key = mWorkShopMgr.mIndexMap[index];
		if (key.m_PublishedFileId == 0L)
		{
			mVCERightPanel.SetActive(value: false);
			return;
		}
		SteamPreFileAndVoteDetail steamPreFileAndVoteDetail = (mSelectedDetail = ((!mWorkShopMgr.mItemsMap.ContainsKey(key)) ? null : mWorkShopMgr.mItemsMap[key]));
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
		UpdateDownloadBtnState();
	}

	private void OnChickLeftList(int index)
	{
		if (index >= 0 && index < mLeftList.Count)
		{
			mWorkShopMgr.ClearIsoMap();
			if (mLeftListSelectedIndex != -1)
			{
				mLeftList[mLeftListSelectedIndex].SetSelected(isSelected: false);
			}
			mLeftList[index].SetSelected(isSelected: true);
			mLeftListSelectedIndex = index;
			mWorkShopGridCtrl.mPagIndex = 0;
			mWorkShopGridCtrl.mMaxPagIndex = 0;
			mWorkShopMgr.mGetIdStarIndex = 0u;
			if (mWorkShopMgr.mTagList.Count == 1)
			{
				mWorkShopMgr.mTagList.Add(PEVCConfig.isoNames[index].Tag);
			}
			else
			{
				mWorkShopMgr.mTagList[1] = PEVCConfig.isoNames[index].Tag;
			}
			GetPageGrid();
		}
	}

	private void BtnQueryOnClick()
	{
		mWorkShopMgr.mQuereText = mInput.text;
		GetPageGrid();
	}

	private void BtnQueryClearOnClick()
	{
		mInput.text = string.Empty;
		if (mWorkShopMgr.mQuereText.Length != 0)
		{
			mWorkShopMgr.mQuereText = string.Empty;
			GetPageGrid();
		}
	}

	private void OnClickItemBtnReLoad(int index)
	{
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			return;
		}
		PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index];
		if (p_id.m_PublishedFileId != 0L)
		{
			int num = index % mWorkShopGridCtrl.mMaxGridCount;
			if (num <= mWorkShopGridCtrl.mMaxGridCount && num >= 0)
			{
				mWorkShopMgr.GetItemDetail(p_id);
				mWorkShopGridCtrl.mUIItems[num].ActiveLoadingItem(isStar: true);
			}
		}
	}

	private void OnClickItemBtnDing(int index)
	{
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			return;
		}
		if (m_DingList != null)
		{
			if (m_DingList.Contains(index))
			{
				return;
			}
			m_DingList.Add(index);
		}
		PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index];
		mWorkShopMgr.Vote(p_id, bFor: true);
	}

	private void OnClickItemBtnCai(int index)
	{
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			return;
		}
		if (m_CaiList != null)
		{
			if (m_CaiList.Contains(index))
			{
				return;
			}
			m_CaiList.Add(index);
		}
		PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index];
		mWorkShopMgr.Vote(p_id, bFor: false);
	}

	private void OnVoteCallBack(PublishedFileId_t p_id, bool bFor)
	{
		int num = FindIndexInGrid(p_id);
		if (num == -1)
		{
			return;
		}
		int num2 = num % mWorkShopGridCtrl.mMaxGridCount;
		if (num2 <= mWorkShopGridCtrl.mMaxGridCount && num2 >= 0 && mWorkShopMgr.mItemsMap.ContainsKey(p_id))
		{
			SteamPreFileAndVoteDetail steamPreFileAndVoteDetail = mWorkShopMgr.mItemsMap[p_id];
			if (bFor)
			{
				steamPreFileAndVoteDetail.m_VoteDetail.m_nVotesFor++;
			}
			else
			{
				steamPreFileAndVoteDetail.m_VoteDetail.m_nVotesAgainst++;
			}
			if (!(mWorkShopGridCtrl.mUIItems[num2] == null))
			{
				mWorkShopGridCtrl.mUIItems[num2].SetDingText(steamPreFileAndVoteDetail.m_VoteDetail.m_nVotesFor.ToString());
				mWorkShopGridCtrl.mUIItems[num2].SetCaiText(steamPreFileAndVoteDetail.m_VoteDetail.m_nVotesAgainst.ToString());
			}
		}
	}

	private int FindIndexInGrid(PublishedFileId_t p_id)
	{
		int result = -1;
		int num = (mWorkShopGridCtrl.mPagIndex - 1) * mWorkShopGridCtrl.mMaxGridCount;
		int num2 = mWorkShopGridCtrl.mPagIndex * mWorkShopGridCtrl.mMaxGridCount;
		foreach (KeyValuePair<int, PublishedFileId_t> item in mWorkShopMgr.mIndexMap)
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
		if (mSelectedDetail == null || !mWorkShopMgr.DownLoadFile(mSelectedDetail))
		{
			return;
		}
		int num = FindIndexInGrid(mSelectedDetail.m_nPublishedFileId);
		if (num != -1)
		{
			int num2 = num % mWorkShopGridCtrl.mMaxGridCount;
			if (num2 <= mWorkShopGridCtrl.mMaxGridCount && num2 >= 0)
			{
				mWorkShopGridCtrl.mUIItems[num2].ActiveUpDown(isActive: true);
				mWorkShopGridCtrl.mUIItems[num2].UpdteUpDownInfo("0%");
			}
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
		if (eWorkshopEnumerationType != mWorkShopMgr.mIdEnumType)
		{
			mWorkShopMgr.mIdEnumType = eWorkshopEnumerationType;
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
		if (mWorkShopMgr.mGetIdDays != num)
		{
			mWorkShopMgr.mGetIdDays = num;
			GetPageGrid();
		}
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
		UIWorkShopGridItem workShopItemByFileName = mWorkShopGridCtrl.GetWorkShopItemByFileName(fileName);
		if (null != workShopItemByFileName)
		{
			workShopItemByFileName.SetDownloaded(download: true);
		}
	}
}
