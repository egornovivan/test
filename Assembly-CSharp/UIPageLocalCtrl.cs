using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UIPageLocalCtrl : MonoBehaviour
{
	public UIInput mQueryInput;

	public UIPageGridCtrl mLocalGridCtrl;

	public UILabel mIcoPath;

	public BoxCollider mBxBtnGoBack;

	public VCEUIStatisticsPanel mRinghtPanelCtrl;

	public GameObject mVCERightPanel;

	public UILabel mLbInfoMsg;

	public List<VCEAssetMgr.IsoFileInfo> mLocalIsoInfo;

	private string mLocalIsoPath;

	private string mDefoutIcoPath;

	private List<ulong> m_UploadingIsoHashList;

	private bool m_ImportIsoing;

	public int mSelectedIsoIndex = -1;

	public Dictionary<int, string> mUpLoadMap;

	public Dictionary<string, int> mUpLoadStateMap;

	private int mUpLoadIndex = -1;

	private int starIndex;

	private void Awake()
	{
		mLocalGridCtrl.mUpdateGrid += UpdateLocalGrid;
		mLocalGridCtrl.DoubleClickLocalFloder += OnDoubleClickFolder;
		mLocalGridCtrl.ClickLocalIso += OnClickIso;
		mLocalGridCtrl.ClickLocalFloder += OnClickFolder;
		mDefoutIcoPath = VCConfig.s_IsoPath.Replace('\\', '/');
		mUpLoadMap = new Dictionary<int, string>();
		mUpLoadStateMap = new Dictionary<string, int>();
		m_UploadingIsoHashList = new List<ulong>();
		m_ImportIsoing = false;
	}

	private void Start()
	{
		GetLocalItem(mDefoutIcoPath);
	}

	private void Update()
	{
	}

	private void GetLocalItem(string _isoPath)
	{
		mLocalIsoPath = _isoPath;
		mLocalIsoPath = mLocalIsoPath.Replace('\\', '/');
		mIcoPath.text = mLocalIsoPath.Replace(mDefoutIcoPath, "[ISO]/");
		if (mLocalIsoPath.Length <= mDefoutIcoPath.Length)
		{
			mBxBtnGoBack.enabled = false;
		}
		else
		{
			mBxBtnGoBack.enabled = true;
		}
		if (mLocalIsoInfo != null)
		{
			mLocalIsoInfo.Clear();
			mLocalIsoInfo = null;
		}
		string keyword = mQueryInput.text.Trim();
		mLocalIsoInfo = VCEAssetMgr.SearchIso(mLocalIsoPath, keyword);
		mLocalGridCtrl.ReSetGrid(mLocalIsoInfo.Count);
		mLocalGridCtrl._UpdatePagText();
		mSelectedIsoIndex = -1;
		mVCERightPanel.SetActive(value: false);
	}

	private void UpdateLocalGrid(int index_0)
	{
		starIndex = index_0;
		for (int i = 0; i < mLocalGridCtrl.mUIItems.Count; i++)
		{
			if (index_0 + i < mLocalIsoInfo.Count)
			{
				SetLocalGridItemInfo(mLocalIsoInfo[index_0 + i], mLocalGridCtrl.mUIItems[i]);
				mLocalGridCtrl.mUIItems[i].gameObject.SetActive(value: true);
			}
			else
			{
				mLocalGridCtrl.mUIItems[i].gameObject.SetActive(value: false);
			}
			mLocalGridCtrl.mUIItems[i].SetSelected(Selected: false);
		}
		mLocalGridCtrl.mGrid.repositionNow = true;
		UpdateDownLoadInfo();
	}

	private void SetLocalGridItemInfo(VCEAssetMgr.IsoFileInfo _isoFileInfo, UIWorkShopGridItem item)
	{
		string isoName = _isoFileInfo.m_Name;
		if (_isoFileInfo.m_IsFolder)
		{
			item.InitItem(WorkGridItemType.mLocalFloder, isoName);
			item.SetIco(null);
			return;
		}
		VCIsoData.ExtractHeader(_isoFileInfo.m_Path, out var iso_header);
		item.InitItem(WorkGridItemType.mLocalIcon, isoName);
		Texture2D texture2D = new Texture2D(256, 256);
		texture2D.LoadImage(iso_header.IconTex);
		item.SetIco(texture2D);
		item.SetAuthor(iso_header.Author);
	}

	private void OnDoubleClickFolder(int index)
	{
		if (index < 0 || index > mLocalIsoInfo.Count)
		{
			Debug.LogError("Update Work shop local folder error!");
		}
		else
		{
			GetLocalItem(mLocalIsoInfo[index].m_Path + "/");
		}
	}

	private void OnClickFolder(int index)
	{
		mVCERightPanel.SetActive(value: false);
		mSelectedIsoIndex = -1;
	}

	private void OnClickIso(int index)
	{
		if (index >= 0 || index < mLocalIsoInfo.Count)
		{
			if (VCIsoData.ExtractHeader(mLocalIsoInfo[index].m_Path, out var iso_header) > 0)
			{
				mVCERightPanel.SetActive(value: true);
				mRinghtPanelCtrl.m_NonEditorIcon = iso_header.IconTex;
				mRinghtPanelCtrl.m_NonEditorISODesc = iso_header.Desc;
				mRinghtPanelCtrl.m_NonEditorISOName = iso_header.Name;
				mRinghtPanelCtrl.m_NonEditorRemark = iso_header.Remarks;
				string nonEditorISOVersion = ((iso_header.Version >> 24) & 0xFF) + "." + ((iso_header.Version >> 16) & 0xFF);
				mRinghtPanelCtrl.m_NonEditorISOVersion = nonEditorISOVersion;
				mRinghtPanelCtrl.SetIsoIcon();
				mRinghtPanelCtrl.OnCreationInfoRefresh();
				mSelectedIsoIndex = index;
			}
			else
			{
				mVCERightPanel.SetActive(value: false);
				mSelectedIsoIndex = -1;
			}
		}
	}

	private void BtnGoBackOnClick()
	{
		if (mLocalIsoPath.Length > 0)
		{
			mLocalIsoPath = new DirectoryInfo(mLocalIsoPath).Parent.FullName + "/";
		}
		GetLocalItem(mLocalIsoPath);
	}

	private void BtnUploadOnClick()
	{
		if (m_ImportIsoing)
		{
			return;
		}
		m_ImportIsoing = true;
		if (mSelectedIsoIndex == -1 || mSelectedIsoIndex >= mLocalIsoInfo.Count)
		{
			SetInfoMsg(UIMsgBoxInfo.mCZ_WorkShopUpNeedSeletedIso.GetString());
			m_ImportIsoing = false;
			return;
		}
		int num = mSelectedIsoIndex % mLocalGridCtrl.mMaxGridCount;
		if (num < 0 || num >= mLocalGridCtrl.mMaxGridCount)
		{
			m_ImportIsoing = false;
			return;
		}
		VCEAssetMgr.IsoFileInfo isoFileInfo = mLocalIsoInfo[mSelectedIsoIndex];
		VCIsoData vCIsoData = new VCIsoData();
		try
		{
			string path = isoFileInfo.m_Path;
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			byte[] array = new byte[(int)fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			vCIsoData.Import(array, new VCIsoOption(editor: true));
			if (vCIsoData.m_HeadInfo.Version < 33751041)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000487));
				m_ImportIsoing = false;
				return;
			}
			string text = path.Replace('\\', '/');
			text = text.Replace(mDefoutIcoPath, "[ISO]/");
			mUpLoadIndex++;
			ulong item = CRC64.Compute(array);
			if (m_UploadingIsoHashList != null && m_UploadingIsoHashList.Contains(item))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000488));
				m_ImportIsoing = false;
				return;
			}
			m_UploadingIsoHashList.Add(item);
			mUpLoadMap[mUpLoadIndex] = text;
			mUpLoadStateMap[mUpLoadMap[mUpLoadIndex]] = 0;
			mLocalGridCtrl.mUIItems[num].ActiveUpDown(isActive: true);
			mLocalGridCtrl.mUIItems[num].UpdteUpDownInfo(PELocalization.GetString(8000908));
			SteamWorkShop.SendFile(UpLoadFileCallBack, vCIsoData.m_HeadInfo.Name, vCIsoData.m_HeadInfo.SteamDesc, vCIsoData.m_HeadInfo.SteamPreview, array, SteamWorkShop.AddNewVersionTag(vCIsoData.m_HeadInfo.ScenePaths()), sendToServer: false, mUpLoadIndex);
		}
		catch (Exception ex)
		{
			mLocalGridCtrl.mUIItems[num].ActiveUpDown(isActive: false);
			mLocalGridCtrl.mUIItems[num].UpdteUpDownInfo(string.Empty);
			Debug.Log(" WorkShop Loading ISO Error : " + ex.ToString());
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000489));
		}
		m_ImportIsoing = false;
	}

	private void UpLoadFileCallBack(int _upLoadindex, bool bOK, ulong hash)
	{
		string text = string.Empty;
		if (mUpLoadMap.ContainsKey(_upLoadindex))
		{
			text = mUpLoadMap[_upLoadindex];
			text = text.Replace('\\', '/');
			text = text.Replace(mDefoutIcoPath, "[ISO]/");
		}
		string infoMsg = string.Empty;
		if (text.Length > 0)
		{
			infoMsg = "'" + text + "' " + UIMsgBoxInfo.mCZ_WorkShopUpLoadIsoFailed.GetString();
		}
		if (bOK)
		{
			mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = 100;
		}
		else
		{
			mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = -1;
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000490));
		}
		if (m_UploadingIsoHashList != null && m_UploadingIsoHashList.Contains(hash))
		{
			m_UploadingIsoHashList.Remove(hash);
		}
		UpdateDownLoadInfo();
		SetInfoMsg(infoMsg);
	}

	private void UpdateDownLoadInfo()
	{
		int count = mLocalIsoInfo.Count;
		int num = starIndex;
		int num2 = 0;
		while (num < count && num2 < mLocalGridCtrl.mMaxGridCount)
		{
			string path = mLocalIsoInfo[num].m_Path;
			path = path.Replace('\\', '/');
			path = path.Replace(mDefoutIcoPath, "[ISO]/");
			if (!mUpLoadStateMap.ContainsKey(path))
			{
				mLocalGridCtrl.mUIItems[num2].ActiveUpDown(isActive: false);
				mLocalGridCtrl.mUIItems[num2].UpdteUpDownInfo(string.Empty);
			}
			else if (mUpLoadStateMap[path] == 100)
			{
				mLocalGridCtrl.mUIItems[num2].ActiveUpDown(isActive: true);
				mLocalGridCtrl.mUIItems[num2].UpdteUpDownInfo(PELocalization.GetString(8000909));
			}
			else if (mUpLoadStateMap[path] == 101)
			{
				mLocalGridCtrl.mUIItems[num2].ActiveUpDown(isActive: true);
				mLocalGridCtrl.mUIItems[num2].UpdteUpDownInfo(PELocalization.GetString(8000910));
			}
			else if (mUpLoadStateMap[path] == -1)
			{
				mLocalGridCtrl.mUIItems[num2].ActiveUpDown(isActive: true);
				mLocalGridCtrl.mUIItems[num2].UpdteUpDownInfo(PELocalization.GetString(8000911));
			}
			else
			{
				mLocalGridCtrl.mUIItems[num2].ActiveUpDown(isActive: true);
				mLocalGridCtrl.mUIItems[num2].UpdteUpDownInfo(PELocalization.GetString(8000908));
			}
			num++;
			num2++;
		}
	}

	private void SetInfoMsg(string msg)
	{
		mLbInfoMsg.text = msg;
	}

	private void BtnQueryOnClick()
	{
		GetLocalItem(mLocalIsoPath);
	}

	private void BtnQueryClearOnClick()
	{
		mQueryInput.text = string.Empty;
		GetLocalItem(mLocalIsoPath);
	}

	public void PublishFinishCellBack(int _upLoadindex, ulong publishID, ulong hash)
	{
		string text = string.Empty;
		if (mUpLoadMap.ContainsKey(_upLoadindex))
		{
			text = mUpLoadMap[_upLoadindex];
			text = text.Replace('\\', '/');
			text = text.Replace(mDefoutIcoPath, "[ISO]/");
		}
		string empty = string.Empty;
		if (text.Length > 0 && publishID != 0L)
		{
			mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = 101;
			empty = "'" + text + "' " + UIMsgBoxInfo.mCZ_WorkShopUpLoadIso.GetString();
		}
		else
		{
			mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = -1;
			empty = "'" + text + "' " + UIMsgBoxInfo.mCZ_WorkShopUpLoadIsoFailed.GetString();
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000491));
		}
		if (m_UploadingIsoHashList != null && m_UploadingIsoHashList.Contains(hash))
		{
			m_UploadingIsoHashList.Remove(hash);
		}
		UpdateDownLoadInfo();
		SetInfoMsg(empty);
	}
}
