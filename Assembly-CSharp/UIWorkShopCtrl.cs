using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

public class UIWorkShopCtrl : UIBaseWnd
{
	public GameObject mObjPageWorkShop_0;

	public GameObject mObjPageMyUpLoad_1;

	public GameObject mObjPageLocal_2;

	public GameObject mVCERightPanel;

	public UIPageWorkShopCtrl mPageWorkShopCtrl;

	public UIPageLocalCtrl mPageLocalCtrl;

	public UIPageUploadCtrl mPageUploadCtrl;

	public UILabel mLbInfoMsg;

	public int GridWidth;

	public UIWorkShopBgAdaptiveCtrl BgAdaptiveCtrl;

	public UIWorkShopPage0AdaptiveCtrl Page0AdaptiveCtrl;

	public UIWorkShopPage0AdaptiveCtrl Page1AdaptiveCtrl;

	public UIWorkShopPage2AdaptiveCtrl Page2AdaptiveCtrl;

	private static int m_CurColumnCount;

	private static int m_CurRowCount = 3;

	private List<string> m_LocalDownLoaded;

	private static UIWorkShopCtrl m_Instance;

	private int mTitleIndex = -1;

	public event OnGuiBtnClicked e_BtnClose;

	private void Awake()
	{
		m_Instance = this;
		InitAllDownLoadedByLocal();
		m_CurColumnCount = GetCurColumnCountByScreenSize();
		BgAdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
		Page0AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
	}

	private int GetCurColumnCountByScreenSize()
	{
		int width = Screen.width;
		if (width < 1366)
		{
			return 3;
		}
		if (width < 1440)
		{
			return 4;
		}
		if (width < 1920)
		{
			return 5;
		}
		return 6;
	}

	private void OnActivate_0(bool isActivate)
	{
		if (isActivate)
		{
			if (!mObjPageWorkShop_0.activeSelf)
			{
				Page0AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
				mObjPageWorkShop_0.SetActive(value: true);
				mObjPageMyUpLoad_1.SetActive(value: false);
				mObjPageLocal_2.SetActive(value: false);
				mVCERightPanel.SetActive(value: false);
			}
			mTitleIndex = 0;
		}
	}

	private void OnActivate_1(bool isActivate)
	{
		if (isActivate)
		{
			if (!mObjPageMyUpLoad_1.activeSelf)
			{
				Page1AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
				mObjPageWorkShop_0.SetActive(value: false);
				mObjPageMyUpLoad_1.SetActive(value: true);
				mObjPageLocal_2.SetActive(value: false);
				mVCERightPanel.SetActive(value: false);
			}
			mTitleIndex = 1;
		}
	}

	private void OnActivate_2(bool isActivate)
	{
		if (isActivate)
		{
			if (!mObjPageLocal_2.activeSelf)
			{
				Page2AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount + 1, GridWidth);
				mObjPageWorkShop_0.SetActive(value: false);
				mObjPageMyUpLoad_1.SetActive(value: false);
				mObjPageLocal_2.SetActive(value: true);
				mVCERightPanel.SetActive(value: false);
			}
			mTitleIndex = 2;
		}
	}

	private void BtnCloseOnClick()
	{
		base.gameObject.SetActive(value: false);
		if (mPageWorkShopCtrl.mWorkShopMgr != null)
		{
			mPageWorkShopCtrl.mWorkShopMgr.isActve = false;
		}
		if (mPageUploadCtrl.mMyWorkShopMgr != null)
		{
			mPageUploadCtrl.mMyWorkShopMgr.isActve = false;
		}
		if (this.e_BtnClose != null)
		{
			this.e_BtnClose();
		}
	}

	private void InitAllDownLoadedByLocal()
	{
		m_LocalDownLoaded = new List<string>();
		string path = VCConfig.s_IsoPath + "/Download/";
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] files = Directory.GetFiles(path, "*" + VCConfig.s_IsoFileExt);
		if (files != null && files.Length > 0)
		{
			for (int i = 0; i < files.Length; i++)
			{
				m_LocalDownLoaded.Add(Path.GetFileName(files[i]));
			}
		}
	}

	private static bool SaveToFile(byte[] fileData, string fileName, string filePath, string fileExt)
	{
		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}
		string path = filePath + fileName + fileExt;
		if (File.Exists(path))
		{
			return false;
		}
		try
		{
			using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
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

	public override void Show()
	{
		base.Show();
		if (mPageWorkShopCtrl.mWorkShopMgr != null)
		{
			mPageWorkShopCtrl.mWorkShopMgr.isActve = true;
		}
		if (mPageUploadCtrl.mMyWorkShopMgr != null)
		{
			mPageUploadCtrl.mMyWorkShopMgr.isActve = true;
		}
	}

	public static bool CheckDownloadExist(string fileName)
	{
		if (fileName == null || string.IsNullOrEmpty(fileName.Trim()) || null == m_Instance || m_Instance.m_LocalDownLoaded == null)
		{
			return false;
		}
		fileName = GetValidFileName(fileName);
		return m_Instance.m_LocalDownLoaded.Contains(fileName);
	}

	public static void AddDownloadFileName(string fileName, bool byUploadPage)
	{
		if (!(null == m_Instance) || m_Instance.m_LocalDownLoaded != null)
		{
			if (byUploadPage)
			{
				m_Instance.mPageWorkShopCtrl.SetItemIsDownloadedByFileName(fileName);
			}
			else
			{
				m_Instance.mPageUploadCtrl.SetItemIsDownloadedByFileName(fileName);
			}
			fileName = GetValidFileName(fileName);
			m_Instance.m_LocalDownLoaded.Add(fileName);
		}
	}

	public static void PublishFinishCellBack(int _upLoadindex, ulong publishID, ulong hash)
	{
		if (!(null == m_Instance) && !(null == m_Instance.mPageLocalCtrl))
		{
			m_Instance.mPageLocalCtrl.PublishFinishCellBack(_upLoadindex, publishID, hash);
		}
	}

	public static uint GetCurRequestCount()
	{
		return (uint)(m_CurColumnCount * m_CurRowCount);
	}

	public static uint GetCurLocalShowCount()
	{
		return (uint)((m_CurColumnCount + 1) * m_CurRowCount);
	}

	public static int GetCurColumnCount()
	{
		return m_CurColumnCount;
	}

	public static string GetValidFileName(string fileName)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		if (fileName.IndexOfAny(invalidFileNameChars) >= 0)
		{
			for (int i = 0; i < invalidFileNameChars.Length; i++)
			{
				fileName = fileName.Replace(invalidFileNameChars[i].ToString(), string.Empty);
			}
		}
		return fileName;
	}

	public static string DownloadFileCallBack(byte[] fileData, PublishedFileId_t p_id, bool bOK)
	{
		string s_CreationNetCachePath = VCConfig.s_CreationNetCachePath;
		string text = CRC64.Compute(fileData).ToString();
		if (bOK)
		{
			if (SaveToFile(fileData, text, s_CreationNetCachePath, VCConfig.s_CreationNetCacheFileExt))
			{
				Debug.Log("ISO save to netCache filepath succeed!");
			}
			else
			{
				Debug.Log("ISO exist or save failed!");
			}
			return s_CreationNetCachePath + text + VCConfig.s_CreationNetCacheFileExt;
		}
		Debug.Log("ISO download failed!");
		return string.Empty;
	}
}
