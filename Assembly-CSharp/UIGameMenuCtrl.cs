using System;
using System.Collections.Generic;
using Pathea;
using PeUIEffect;
using UnityEngine;

public class UIGameMenuCtrl : UIStaticWnd
{
	private class MenuItemInfo
	{
		public string mItemText = string.Empty;

		public MenuItemFlag mFlag;

		public MenuItemFlag mParentFalg;

		public string mItemIcoStr = string.Empty;

		public int mKeyId = -1;

		public UIOption.KeyCategory mKeyCategory = UIOption.KeyCategory.Common;

		public MenuItemInfo(string itemText, MenuItemFlag flag, MenuItemFlag parentFlag, string icoStr, UIOption.KeyCategory category = UIOption.KeyCategory.Common, int id = -1)
		{
			mItemText = itemText;
			mFlag = flag;
			mParentFalg = parentFlag;
			mItemIcoStr = icoStr;
			mKeyCategory = category;
			mKeyId = id;
		}
	}

	public enum MenuItemFlag
	{
		Flag_Null,
		Flag_Storage,
		Flag_Admin,
		Flag_Workshop,
		Flag_Infomation,
		Flag_Friend,
		Flag_Mall,
		Flag_Online,
		Flag_Follower,
		Flag_Character,
		Flag_Mission,
		Flag_Phone,
		Flag_Colony,
		Flag_Replicatror,
		Flag_Creation,
		Flag_Skill,
		Flag_Inventory,
		Flag_Build,
		Flag_Options,
		Flag_Scan,
		Flag_Help,
		Flag_MonoRail,
		Flag_Diplomacy,
		Flag_Message,
		Flag_SpeciesWiki,
		Flag_Radio
	}

	private static UIGameMenuCtrl m_Instance;

	public UIMenuList mMenuList;

	public BoxCollider mBtnCollider;

	public GameMenuScaleEffect mTweenEffect;

	[SerializeField]
	private Transform m_TutorialParent;

	[SerializeField]
	private UIWndTutorialTip_N m_TutorialPrefab;

	private bool IsInitMenuList;

	private List<MenuItemInfo> mInfoList = new List<MenuItemInfo>();

	private Vector3 mMenuListPos = Vector3.zero;

	private string IcoStr_Admin = "listico_1_1";

	private string IcoStr_Workshop = "listico_4_1";

	private string IcoStr_Infomation = "listico_5_1";

	private string IcoStr_Friend = "listico_21_1";

	private string IcoStr_OnLine = "listico_19_1";

	private string IcoStr_Follower = "listico_3_1";

	private string IcoStr_Character = "listico_2_1";

	private string IcoStr_Mission = "listico_6_1";

	private string IcoStr_Phone = "listico_7_1";

	private string IcoStr_Colony = "listico_8_1";

	private string IcoStr_Replicatror = "listico_9_1";

	private string IcoStr_Creation = "listico_10_1";

	private string IcoStr_Inventory = "listico_11_1";

	private string IcoStr_Build = "listico_17_1";

	private string IcoStr_Options = "listico_13_1";

	private string IcoStr_Scan = "listico_16_1";

	private string IcoStr_Help = "listico_14_1";

	private string IcoStr_MonoRail = "listico_15_1";

	private string IcoStr_Diplomacy = "listico_22_1";

	private string IcoStr_Message = "listico_23_1";

	private string IcoStr_SpiciesWiki = "listico_24_1";

	private string IcoStr_Radio = "listico_25_1";

	private string IcoStr_Skill = "listico_1_1";

	public static UIGameMenuCtrl Instance => m_Instance;

	private void Awake()
	{
		m_Instance = this;
		Init();
	}

	private void Start()
	{
		mMenuList.Hide();
	}

	private void Init()
	{
		mBtnCollider.gameObject.AddComponent<ShowToolTipItem_N>().mTipContent = PELocalization.GetString(2000060) + "[4169e1][~][-]";
		mInfoList.Clear();
		if (PeGameMgr.IsMulti)
		{
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuOnline.GetString(), MenuItemFlag.Flag_Online, MenuItemFlag.Flag_Null, IcoStr_OnLine));
			if (!PeGameMgr.IsSurvive)
			{
			}
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuAdmin.GetString(), MenuItemFlag.Flag_Admin, MenuItemFlag.Flag_Online, IcoStr_Admin));
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuWorkshop.GetString(), MenuItemFlag.Flag_Workshop, MenuItemFlag.Flag_Online, IcoStr_Workshop));
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuInformation.GetString(), MenuItemFlag.Flag_Infomation, MenuItemFlag.Flag_Online, IcoStr_Infomation));
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuFriend.GetString(), MenuItemFlag.Flag_Friend, MenuItemFlag.Flag_Online, IcoStr_Friend, UIOption.KeyCategory.Common, 25));
		}
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuFollower.GetString(), MenuItemFlag.Flag_Follower, MenuItemFlag.Flag_Null, IcoStr_Follower, UIOption.KeyCategory.Common, 21));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuCharacter.GetString(), MenuItemFlag.Flag_Character, MenuItemFlag.Flag_Null, IcoStr_Character, UIOption.KeyCategory.Common, 17));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuMission.GetString(), MenuItemFlag.Flag_Mission, MenuItemFlag.Flag_Null, IcoStr_Mission, UIOption.KeyCategory.Common, 15));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuPhone.GetString(), MenuItemFlag.Flag_Phone, MenuItemFlag.Flag_Null, IcoStr_Phone, UIOption.KeyCategory.Common, 22));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuHelp.GetString(), MenuItemFlag.Flag_Help, MenuItemFlag.Flag_Phone, IcoStr_Help));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuScan.GetString(), MenuItemFlag.Flag_Scan, MenuItemFlag.Flag_Phone, IcoStr_Scan));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuMonoRail.GetString(), MenuItemFlag.Flag_MonoRail, MenuItemFlag.Flag_Phone, IcoStr_MonoRail));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuDiplomacy.GetString(), MenuItemFlag.Flag_Diplomacy, MenuItemFlag.Flag_Phone, IcoStr_Diplomacy));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuMessage.GetString(), MenuItemFlag.Flag_Message, MenuItemFlag.Flag_Phone, IcoStr_Message));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuSpeciesWiki.GetString(), MenuItemFlag.Flag_SpeciesWiki, MenuItemFlag.Flag_Phone, IcoStr_SpiciesWiki));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuRadio.GetString(), MenuItemFlag.Flag_Radio, MenuItemFlag.Flag_Phone, IcoStr_Radio));
		if (!PeGameMgr.IsTutorial && !PeGameMgr.IsCustom && !PeGameMgr.IsMultiCustom)
		{
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuColony.GetString(), MenuItemFlag.Flag_Colony, MenuItemFlag.Flag_Null, IcoStr_Colony, UIOption.KeyCategory.Common, 24));
		}
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuReplicator.GetString(), MenuItemFlag.Flag_Replicatror, MenuItemFlag.Flag_Null, IcoStr_Replicatror, UIOption.KeyCategory.Common, 19));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuCreation.GetString(), MenuItemFlag.Flag_Creation, MenuItemFlag.Flag_Null, IcoStr_Creation, UIOption.KeyCategory.Common, 23));
		if (PeGameMgr.IsAdventure && RandomMapConfig.useSkillTree)
		{
			mInfoList.Add(new MenuItemInfo(NewUIText.mMenuSkill.GetString(), MenuItemFlag.Flag_Skill, MenuItemFlag.Flag_Null, IcoStr_Skill, UIOption.KeyCategory.Common, 20));
		}
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuInventory.GetString(), MenuItemFlag.Flag_Inventory, MenuItemFlag.Flag_Null, IcoStr_Inventory, UIOption.KeyCategory.Common, 16));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuBuild.GetString(), MenuItemFlag.Flag_Build, MenuItemFlag.Flag_Null, IcoStr_Build, UIOption.KeyCategory.Construct, 0));
		mInfoList.Add(new MenuItemInfo(NewUIText.mMenuOptions.GetString(), MenuItemFlag.Flag_Options, MenuItemFlag.Flag_Null, IcoStr_Options));
		InitMenuList();
		mMenuList.RefreshHotKeyName();
	}

	private void InitMenuList()
	{
		mMenuList.Items.Clear();
		UIMenuListItem uIMenuListItem = null;
		int i;
		for (i = 0; i < mInfoList.Count; i++)
		{
			uIMenuListItem = ((mInfoList[i].mParentFalg != 0) ? mMenuList.Items.Find((UIMenuListItem li) => li.mMenuItemFlag == mInfoList[i].mParentFalg) : null);
			UIMenuListItem uIMenuListItem2 = mMenuList.AddItem(uIMenuListItem, mInfoList[i].mItemText, mInfoList[i].mFlag, mInfoList[i].mItemIcoStr);
			uIMenuListItem2.KeyId = mInfoList[i].mKeyId;
			uIMenuListItem2.mCategory = mInfoList[i].mKeyCategory;
		}
		int num = Convert.ToInt32(mMenuList.rootPanel.spBg.transform.localScale.y / 2f) + 26;
		mMenuListPos = new Vector3(-130f, num, 0f);
		mMenuList.transform.localPosition = mMenuListPos;
		TweenPosition component = mMenuList.GetComponent<TweenPosition>();
		component.from = mMenuListPos;
		IsInitMenuList = true;
	}

	public override void Show()
	{
		BtnMenuOnClick();
		mTweenEffect.Play(forward: true);
	}

	private void BtnMenuOnClick()
	{
		if (IsInitMenuList)
		{
			if (!mMenuList.IsShow || !mMenuList.gameObject.activeSelf)
			{
				base.PlayOpenSoundEffect();
			}
			if (!mMenuList.IsShow)
			{
				mMenuList.Show();
			}
			mTweenEffect.Play();
		}
	}

	private void HideMenuList()
	{
		if (null != UICamera.mainCamera)
		{
			Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
			if (!mBtnCollider.Raycast(ray, out var _, 300f) && !mMenuList.mouseMoveOn)
			{
				mTweenEffect.Play(forward: false);
			}
		}
	}

	private void Update()
	{
		if (mMenuList.gameObject.activeSelf && (Input.GetMouseButton(0) || Input.GetMouseButton(1)) && mMenuList.gameObject.transform.localScale == Vector3.one)
		{
			HideMenuList();
		}
	}

	private void MenuItemOnClick(object sender)
	{
		UIMenuListItem uIMenuListItem = sender as UIMenuListItem;
		if (uIMenuListItem == null || uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Null)
		{
			return;
		}
		if (uIMenuListItem.mMenuItemFlag != MenuItemFlag.Flag_Storage)
		{
			if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Admin)
			{
				GameUI.Instance.mAdminstratorWnd.Show();
			}
			else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Workshop)
			{
				GameUI.Instance.mWorkShopCtrl.Show();
			}
			else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Infomation)
			{
				GameUI.Instance.mTeamInfoMgr.Show();
			}
			else if (uIMenuListItem.mMenuItemFlag != MenuItemFlag.Flag_Friend)
			{
				if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Mall)
				{
					if (GameUI.Instance.mMallWnd != null)
					{
						GameUI.Instance.mMallWnd.Show();
					}
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Follower)
				{
					GameUI.Instance.mServantWndCtrl.Show();
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Character)
				{
					GameUI.Instance.mUIPlayerInfoCtrl.Show();
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Mission)
				{
					if (PeGameMgr.IsCustom)
					{
						GameUI.Instance.mMissionGoal.Show();
					}
					else
					{
						GameUI.Instance.mUIMissionWndCtrl.Show();
					}
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Phone)
				{
					GameUI.Instance.mPhoneWnd.Show();
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Colony)
				{
					GameUI.Instance.mCSUI_MainWndCtrl.Show();
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Replicatror)
				{
					GameUI.Instance.mCompoundWndCtrl.Show();
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Creation)
				{
					if (null != GameUI.Instance && !GameUI.Instance.SystemUIIsOpen)
					{
						VCEditor.Open();
					}
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Inventory)
				{
					GameUI.Instance.mItemPackageCtrl.Show();
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Build)
				{
					if (SingleGameStory.curType == SingleGameStory.StoryScene.MainLand || SingleGameStory.curType == SingleGameStory.StoryScene.TrainingShip)
					{
						GameUI.Instance.mBuildBlock.EnterBuildMode();
					}
					else
					{
						new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
					}
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Options)
				{
					GameUI.Instance.mSystemMenu.Show();
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Scan)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Scan);
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Help)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_MonoRail)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Rail);
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Diplomacy)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Diplomacy);
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Message)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Message);
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_SpeciesWiki)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_MonsterHandbook);
				}
				else if (uIMenuListItem.mMenuItemFlag == MenuItemFlag.Flag_Radio)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Radio);
				}
				else
				{
					if (uIMenuListItem.mMenuItemFlag != MenuItemFlag.Flag_Skill)
					{
						return;
					}
					GameUI.Instance.mSkillWndCtrl.Show();
				}
			}
		}
		GameUI.Instance.ShowGameWndAll();
		mTweenEffect.Play(forward: false);
	}

	private void OnHideWndBtn()
	{
	}

	public void ShowWndTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_TutorialPrefab.gameObject);
			gameObject.transform.parent = m_TutorialParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
		}
	}
}
