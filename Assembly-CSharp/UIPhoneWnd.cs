using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class UIPhoneWnd : UIBaseWnd
{
	public enum PageSelect
	{
		Null = -1,
		Page_Help,
		Page_Scan,
		Page_Rail,
		Page_Diplomacy,
		Page_Message,
		Page_MonsterHandbook,
		Page_Radio,
		Max
	}

	private const string SELECTPAGEKEY = "UIPhoneSelectPage";

	private const int m_OpenRadioMissionID = 10047;

	private const int m_ChenZhenID = 9007;

	[SerializeField]
	private UICheckbox mCkHelp;

	[SerializeField]
	private UICheckbox mCkScan;

	[SerializeField]
	private UICheckbox mCkRail;

	[SerializeField]
	private UICheckbox mCkCamp;

	[SerializeField]
	private UICheckbox mCkMsg;

	[SerializeField]
	private UICheckbox mCkMh;

	[SerializeField]
	private UICheckbox mCkRadio;

	[SerializeField]
	private UILabel mCkHelpText;

	[SerializeField]
	private UILabel mCkScanText;

	[SerializeField]
	private UILabel mCkRailText;

	[SerializeField]
	private UILabel mCkCampText;

	[SerializeField]
	private UILabel mCkMsgText;

	[SerializeField]
	private UILabel mCkMhText;

	[SerializeField]
	private UILabel mCkRadioText;

	[SerializeField]
	private UIGrid mCksGrid;

	[SerializeField]
	private float mCheckIntervalTime = 0.3f;

	public bool OpenRadio;

	public UIHelpCtrl mUIHelp;

	public UIScanCtrl mUIScan;

	public UIMonoRailCtrl mUIRail;

	public UICampCtrl mUICamp;

	public UIAllianceCtrl mUIAlliance;

	public UIMessageCtrl mUIMessage;

	public UIMonsterHandbookCtrl mUIMonsterHandbook;

	public UIRadioCtrl mUIRadioCtrl;

	[HideInInspector]
	public PageSelect CurSelectPage;

	private UIBaseWidget selectPage;

	private UIBaseWidget curDiplomacyWnd;

	private float m_StartTime;

	private NpcMissionData m_ChenZhenMissionData;

	public bool InitRadio;

	private void Update()
	{
		mCkHelpText.color = ((!mCkHelp.isChecked) ? Color.black : Color.white);
		mCkScanText.color = ((!mCkScan.isChecked) ? Color.black : Color.white);
		mCkRailText.color = ((!mCkRail.isChecked) ? Color.black : Color.white);
		mCkCampText.color = ((!mCkCamp.isChecked) ? Color.black : Color.white);
		mCkMsgText.color = ((!mCkMsg.isChecked) ? Color.black : Color.white);
		mCkMhText.color = ((!mCkMh.isChecked) ? Color.black : Color.white);
		mCkRadioText.color = ((!mCkRadio.isChecked) ? Color.black : Color.white);
		if (Time.realtimeSinceStartup - m_StartTime > mCheckIntervalTime)
		{
			UpdateCksActiveState();
			m_StartTime = Time.realtimeSinceStartup;
		}
	}

	public void OnGUI()
	{
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		InitRadioData();
		CurSelectPage = PageSelect.Page_Help;
		if (null != UIRecentDataMgr.Instance)
		{
			int intValue = UIRecentDataMgr.Instance.GetIntValue("UIPhoneSelectPage", 0);
			if (intValue > -1 && intValue < 7)
			{
				CurSelectPage = (PageSelect)intValue;
				if (!CheckUnlockByPageSelect(CurSelectPage))
				{
					CurSelectPage = PageSelect.Page_Help;
				}
			}
		}
		curDiplomacyWnd = ((!PeGameMgr.IsStory) ? ((UIBaseWidget)mUIAlliance) : ((UIBaseWidget)mUICamp));
		CancelAllCkStartsChecked();
		switch (CurSelectPage)
		{
		case PageSelect.Page_Help:
			mCkHelp.startsChecked = true;
			selectPage = mUIHelp;
			break;
		case PageSelect.Page_Scan:
			mCkScan.startsChecked = true;
			selectPage = mUIScan;
			break;
		case PageSelect.Page_Rail:
			mCkRail.startsChecked = true;
			selectPage = mUIRail;
			break;
		case PageSelect.Page_Diplomacy:
			mCkCamp.startsChecked = true;
			selectPage = mUICamp;
			break;
		case PageSelect.Page_Message:
			mCkMsg.startsChecked = true;
			selectPage = mUIMessage;
			break;
		case PageSelect.Page_MonsterHandbook:
			mCkMh.startsChecked = true;
			selectPage = mUIMonsterHandbook;
			break;
		case PageSelect.Page_Radio:
			mCkRadio.startsChecked = true;
			selectPage = mUIRadioCtrl;
			break;
		}
		m_StartTime = Time.realtimeSinceStartup;
	}

	private void CancelAllCkStartsChecked()
	{
		mCkHelp.startsChecked = false;
		mCkScan.startsChecked = false;
		mCkRail.startsChecked = false;
		mCkCamp.startsChecked = false;
		mCkMsg.startsChecked = false;
		mCkMh.startsChecked = false;
		mCkRadio.startsChecked = false;
	}

	private bool CheckUnlockByPageSelect(PageSelect pageSelect)
	{
		switch (pageSelect)
		{
		case PageSelect.Page_Help:
			return true;
		case PageSelect.Page_Scan:
			return true;
		case PageSelect.Page_Rail:
			return true;
		case PageSelect.Page_Diplomacy:
			if (PeGameMgr.IsAdventure)
			{
				return true;
			}
			if (PeSingleton<ReputationSystem>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null && PeGameMgr.IsStory)
			{
				return PeSingleton<ReputationSystem>.Instance.GetActiveState((int)PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
			}
			return false;
		case PageSelect.Page_Message:
			return true;
		case PageSelect.Page_MonsterHandbook:
			if (PeGameMgr.IsAdventure || (PeGameMgr.IsStory && null != StroyManager.Instance && StroyManager.Instance.enableBook))
			{
				return true;
			}
			return false;
		case PageSelect.Page_Radio:
			return CheckOpenRadio();
		case PageSelect.Max:
			return false;
		default:
			return false;
		}
	}

	public void InitRadioData()
	{
		if (!InitRadio)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9007);
			if ((bool)peEntity)
			{
				m_ChenZhenMissionData = peEntity.GetUserData() as NpcMissionData;
			}
			InitRadio = true;
		}
	}

	public bool CheckOpenRadio()
	{
		if (PeGameMgr.IsStory)
		{
			if ((bool)MissionManager.Instance && MissionManager.Instance.m_PlayerMission.HadCompleteMission(10047))
			{
				OpenRadio = true;
			}
			else if (m_ChenZhenMissionData == null || m_ChenZhenMissionData.m_MissionList.Contains(10047))
			{
				OpenRadio = false;
			}
			else
			{
				OpenRadio = true;
			}
		}
		else
		{
			OpenRadio = true;
		}
		return OpenRadio;
	}

	private void UpdateCksActiveState()
	{
		mCkCamp.gameObject.SetActive(CheckUnlockByPageSelect(PageSelect.Page_Diplomacy));
		mCkMh.gameObject.SetActive(CheckUnlockByPageSelect(PageSelect.Page_MonsterHandbook));
		mCkRadio.gameObject.SetActive(CheckUnlockByPageSelect(PageSelect.Page_Radio));
		UpdateCksPos();
	}

	private void UpdateCksPos()
	{
		mCksGrid.Reposition();
		UICheckbox[] componentsInChildren = mCksGrid.GetComponentsInChildren<UICheckbox>(includeInactive: false);
		int num = ((componentsInChildren != null) ? componentsInChildren.Length : 0);
		mCksGrid.transform.localPosition = new Vector3((0f - mCksGrid.cellWidth) * (float)(num - 1) * 0.5f, mCksGrid.transform.localPosition.y, mCksGrid.transform.localPosition.z);
	}

	private void DelayShow()
	{
		Show();
	}

	private void ChangePage(PageSelect page)
	{
		if (!CheckUnlockByPageSelect(page))
		{
			return;
		}
		if (selectPage != null)
		{
			selectPage.Hide();
		}
		CurSelectPage = page;
		if (null != UIRecentDataMgr.Instance)
		{
			UIRecentDataMgr.Instance.SetIntValue("UIPhoneSelectPage", (int)page);
		}
		switch (page)
		{
		case PageSelect.Page_Help:
			mUIHelp.Show();
			selectPage = mUIHelp;
			if (!mCkHelp.isChecked)
			{
				mCkHelp.isChecked = true;
			}
			break;
		case PageSelect.Page_Scan:
			mUIScan.Show();
			selectPage = mUIScan;
			if (!mCkScan.isChecked)
			{
				mCkScan.isChecked = true;
			}
			break;
		case PageSelect.Page_Rail:
			mUIRail.Show();
			selectPage = mUIRail;
			if (!mCkRail.isChecked)
			{
				mCkRail.isChecked = true;
			}
			break;
		case PageSelect.Page_Diplomacy:
			curDiplomacyWnd.Show();
			selectPage = curDiplomacyWnd;
			if (!mCkCamp.isChecked)
			{
				mCkCamp.isChecked = true;
			}
			break;
		case PageSelect.Page_Message:
			mUIMessage.Show();
			selectPage = mUIMessage;
			mCkMsg.isChecked = true;
			break;
		case PageSelect.Page_MonsterHandbook:
			mUIMonsterHandbook.Show();
			selectPage = mUIMonsterHandbook;
			mCkMh.isChecked = true;
			break;
		case PageSelect.Page_Radio:
			mUIRadioCtrl.Show();
			selectPage = mUIRadioCtrl;
			mCkRadio.isChecked = true;
			break;
		}
	}

	private void OnHelpBtn()
	{
		ChangePage(PageSelect.Page_Help);
	}

	private void OnScanBtn()
	{
		ChangePage(PageSelect.Page_Scan);
	}

	private void OnRailBtn()
	{
		ChangePage(PageSelect.Page_Rail);
	}

	private void OnCampBtn()
	{
		ChangePage(PageSelect.Page_Diplomacy);
	}

	private void OnMsgBtn()
	{
		ChangePage(PageSelect.Page_Message);
	}

	private void OnMonsterHandbookBtn()
	{
		ChangePage(PageSelect.Page_MonsterHandbook);
	}

	private void OnRadioBtn()
	{
		ChangePage(PageSelect.Page_Radio);
	}

	public void Show(PageSelect page)
	{
		base.Show();
		UpdateCksActiveState();
		ChangePage(page);
	}

	public override void Show()
	{
		base.Show();
		Show(CurSelectPage);
	}
}
