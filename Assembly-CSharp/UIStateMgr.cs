using System.Collections.Generic;
using System.Linq;
using Pathea;
using UnityEngine;

public class UIStateMgr : MonoBehaviour
{
	[HideInInspector]
	public const float BaseWndTopDepth = 20f;

	[HideInInspector]
	public const float BaseWndDepth = 10f;

	private static UIStateMgr mInstance;

	[HideInInspector]
	public List<UIBaseWnd> mBaseWndList;

	[HideInInspector]
	public List<UIStaticWnd> mStaticWndList;

	[HideInInspector]
	private bool m_BackupShowGui;

	private bool m_CurShowGui;

	private List<UIBaseWnd> m_OpenHistoryList;

	private List<Rect> rectList = new List<Rect>();

	public static UIStateMgr Instance => mInstance;

	public bool CurShowGui => m_CurShowGui;

	public bool OpAnyGUI
	{
		get
		{
			if (GameUI.Instance.mNPCTalk.isShow)
			{
				return true;
			}
			return UIMouseEvent.opAnyGUI;
		}
	}

	public bool isTalking
	{
		get
		{
			if (null == GameUI.Instance || null == GameUI.Instance.mNPCTalk || null == GameUI.Instance.mNpcWnd || null == GameUI.Instance.mShopWnd)
			{
				return false;
			}
			return GameUI.Instance.mNPCTalk.isShow || GameUI.Instance.mNpcWnd.isShow || GameUI.Instance.mShopWnd.isShow;
		}
	}

	private void Awake()
	{
		mInstance = this;
		mBaseWndList = new List<UIBaseWnd>();
		m_OpenHistoryList = new List<UIBaseWnd>();
	}

	private void Start()
	{
		InitBaseWndList();
		mStaticWndList.AddRange(base.gameObject.GetComponentsInChildren<UIStaticWnd>(includeInactive: true));
	}

	private void InitBaseWndList()
	{
		mBaseWndList.AddRange(base.gameObject.GetComponentsInChildren<UIBaseWnd>(includeInactive: true));
		float num = 20f;
		for (int i = 0; i < mBaseWndList.Count; i++)
		{
			UIBaseWnd uIBaseWnd = mBaseWndList[i];
			num = (uIBaseWnd.mDepth = num + 10f);
			uIBaseWnd.transform.localPosition = new Vector3(uIBaseWnd.transform.localPosition.x, uIBaseWnd.transform.localPosition.y, uIBaseWnd.mDepth);
		}
	}

	private void UpdateWndActive()
	{
		for (int i = 0; i < mBaseWndList.Count; i++)
		{
			UIBaseWnd uIBaseWnd = mBaseWndList[i];
			if (!(null != uIBaseWnd) || !uIBaseWnd.isShow)
			{
				continue;
			}
			if (uIBaseWnd.IsCoverForTopsWnd(GetTopRects(uIBaseWnd)))
			{
				if (uIBaseWnd.Active)
				{
					uIBaseWnd.DeActiveWnd();
				}
			}
			else if (!uIBaseWnd.Active)
			{
				uIBaseWnd.ActiveWnd();
			}
		}
	}

	public void SaveUIPostion()
	{
		for (int i = 0; i < mBaseWndList.Count; i++)
		{
			mBaseWndList[i].SaveWndPostion();
		}
	}

	private List<Rect> GetTopRects(UIBaseWnd wnd)
	{
		rectList.Clear();
		for (int i = 0; i < mBaseWndList.Count; i++)
		{
			UIBaseWnd uIBaseWnd = mBaseWndList[i];
			if (null != uIBaseWnd && uIBaseWnd.isShow && uIBaseWnd.transform.localPosition.z < wnd.transform.localPosition.z)
			{
				rectList.Add(uIBaseWnd.rect);
			}
		}
		return rectList;
	}

	private bool GameUIIsShow()
	{
		if (GameConfig.IsInVCE)
		{
			return true;
		}
		if (null != UISightingTelescope.Instance && UISightingTelescope.Instance.CurType == UISightingTelescope.SightingType.Null && null != GameUI.Instance && GameUI.Instance.mNPCTalk.isShow && GameUI.Instance.mNPCTalk.type == UINPCTalk.NormalOrSp.Normal)
		{
			return true;
		}
		if (GameUI.Instance.mSystemMenu.IsOpen() || GameUI.Instance.mOption.isShow || GameUI.Instance.mSaveLoad.isShow || MessageBox_N.Instance.isShow)
		{
			return true;
		}
		if (GameUI.Instance.mUIWorldMap.isShow)
		{
			return true;
		}
		for (int i = 0; i < mBaseWndList.Count; i++)
		{
			UIBaseWnd uIBaseWnd = mBaseWndList[i];
			if (!(uIBaseWnd is UIMissionTrackCtrl) && null != uIBaseWnd && uIBaseWnd.gameObject.activeInHierarchy)
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		UpdateWndActive();
		m_CurShowGui = GameUIIsShow();
		if (m_CurShowGui != m_BackupShowGui)
		{
			m_BackupShowGui = m_CurShowGui;
			if (GameUI.Instance != null && GameUI.Instance.mMainPlayer != null)
			{
				GameUI.Instance.mMainPlayer.SendMsg(EMsg.UI_ShowChange, m_BackupShowGui);
			}
		}
	}

	public static void RecordOpenHistory(UIBaseWnd openWnd)
	{
		if (!(null == Instance))
		{
			RemoveOpenHistory(openWnd);
			Instance.m_OpenHistoryList.Add(openWnd);
		}
	}

	public static void RemoveOpenHistory(UIBaseWnd openWnd)
	{
		if (!(null == Instance) && Instance.m_OpenHistoryList.Contains(openWnd))
		{
			Instance.m_OpenHistoryList.RemoveAll((UIBaseWnd a) => a == openWnd);
		}
	}

	public UIBaseWnd GetTopWnd()
	{
		if (m_OpenHistoryList.Count > 0)
		{
			return m_OpenHistoryList.FirstOrDefault((UIBaseWnd a) => Mathf.Abs(a.transform.localPosition.z - 20f) < 0.0001f);
		}
		return null;
	}
}
