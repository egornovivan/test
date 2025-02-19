using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;
using WhiteCat;

public class GameUI : MonoBehaviour
{
	private const int m_PutOnEquipAudioID = 1683;

	private const int m_TakeOffEquipAudioID = 1684;

	private const int m_CompoundAudioID = 1685;

	private const int m_WndOpenAudioID = 914;

	private const int m_NpcTalkWndOpenAudioID = 4104;

	private const int m_BulidTalkID = 4076;

	private const int m_FullQuickBarTalkID = 4343;

	private const int m_GameMenuTalkID = 4344;

	private const int m_MinMapTalkID = 4345;

	private const int m_MissionTrackTalkID = 4346;

	private static GameUI mInstance;

	public bool testUI = true;

	public Camera mUICamera;

	public bool IsInput;

	[SerializeField]
	private Transform tsCenter;

	[SerializeField]
	private Transform tsCenterOther;

	[SerializeField]
	private Transform tsLeft;

	[SerializeField]
	private Transform tsRightTop;

	[SerializeField]
	private Transform tsRight;

	[SerializeField]
	private GameObject buildPrefab;

	[SerializeField]
	private GameObject tipsWmdPrefab;

	[SerializeField]
	private GameObject tipRecordsMgrPrefab;

	[SerializeField]
	private GameObject GameMainPrefab;

	[SerializeField]
	private GameObject playerInfoPrefab;

	[SerializeField]
	private GameObject ItemPackagePrefab;

	[SerializeField]
	private GameObject worldMapPrefab;

	[SerializeField]
	private GameObject limitWorldMapPrefab;

	[SerializeField]
	private GameObject npcStoragePrefab;

	[SerializeField]
	private GameObject SytemPrefab;

	[SerializeField]
	private GameObject compoundWndPrefab;

	[SerializeField]
	private GameObject servantWndPrefab;

	[SerializeField]
	private GameObject npcGuiPrefab;

	[SerializeField]
	private GameObject missionWndPrefab;

	[SerializeField]
	private GameObject missionTrackWndPrefab;

	[SerializeField]
	private GameObject itemGetWndPrefab;

	[SerializeField]
	private GameObject itemOpGuiPrefab;

	[SerializeField]
	private GameObject shopGuiPrefab;

	[SerializeField]
	private GameObject itemBoxGuiPrefab;

	[SerializeField]
	private GameObject repairWndGuiPrefab;

	[SerializeField]
	private GameObject powerPlantSolarPrefab;

	[SerializeField]
	private GameObject reviveGuiPrefab;

	[SerializeField]
	private GameObject wareHouseGuiPrefab;

	[SerializeField]
	private GameObject colonyWndPrefab;

	[SerializeField]
	private GameObject phoneWndPrefab;

	[SerializeField]
	private GameObject skillWndPrefab;

	[SerializeField]
	private GameObject workshopPrefb;

	[SerializeField]
	private GameObject informationPrefab;

	[SerializeField]
	private GameObject railwayPonitPrefab;

	[SerializeField]
	private GameObject mallWndPrefab;

	[SerializeField]
	private GameObject AdministratorPrefab;

	[SerializeField]
	private GameObject drivingPrefab;

	[SerializeField]
	private GameObject stopwatchPrefab;

	[SerializeField]
	private GameObject npcWndCustomPrefab;

	[SerializeField]
	private GameObject missionWndCustomPrefab;

	[SerializeField]
	private GameObject missionTrackCustomPrefab;

	[SerializeField]
	private GameObject mKickstarterCtrlPrefab;

	[SerializeField]
	private GameObject mNpcTalkHistoryPrefab;

	[SerializeField]
	private BoxCollider mSystemUIMaskCollider;

	[HideInInspector]
	public UIGameMenuCtrl mUIGameMenuCtrl;

	[HideInInspector]
	public UIMainMidCtrl mUIMainMidCtrl;

	[HideInInspector]
	public UIMinMapCtrl mUIMinMapCtrl;

	[HideInInspector]
	public UINPCTalk mNPCTalk;

	[HideInInspector]
	public UINpcSpeech mNPCSpeech;

	[HideInInspector]
	public UIServantTalk mServantTalk;

	[HideInInspector]
	public UIMissionTrackCtrl mMissionTrackWnd;

	[HideInInspector]
	public UIWorldMapCtrl mUIWorldMap;

	[HideInInspector]
	public UISystemMenu mSystemMenu;

	[HideInInspector]
	public UIOption mOption;

	[HideInInspector]
	public UISaveLoad mSaveLoad;

	[HideInInspector]
	public UIPlayerInfoCtrl mUIPlayerInfoCtrl;

	[HideInInspector]
	public UIItemPackageCtrl mItemPackageCtrl;

	[HideInInspector]
	public UINpcStorageCtrl mNpcStrageCtrl;

	[HideInInspector]
	public UICompoundWndControl mCompoundWndCtrl;

	[HideInInspector]
	public UIServantWnd mServantWndCtrl;

	[HideInInspector]
	public UINpcWnd mNpcWnd;

	[HideInInspector]
	public UIMissionWndCtrl mUIMissionWndCtrl;

	[HideInInspector]
	public UIItemGet mItemGet;

	[HideInInspector]
	public UIItemOp mItemOp;

	[HideInInspector]
	public UIShopWnd mShopWnd;

	[HideInInspector]
	public UIItemBox mItemBox;

	[HideInInspector]
	public UIRepairWnd mRepair;

	[HideInInspector]
	public UIPowerPlantSolar mPowerPlantSolar;

	[HideInInspector]
	public UIRevive mRevive;

	[HideInInspector]
	public UIWarehouse mWarehouse;

	[HideInInspector]
	public CSUI_MainWndCtrl mCSUI_MainWndCtrl;

	[HideInInspector]
	public UIBuildBlock mBuildBlock;

	[HideInInspector]
	public UIPhoneWnd mPhoneWnd;

	[HideInInspector]
	public UISkillWndCtrl mSkillWndCtrl;

	[HideInInspector]
	public UIWorkShopCtrl mWorkShopCtrl;

	[HideInInspector]
	public CSUI_TeamInfoMgr mTeamInfoMgr;

	[HideInInspector]
	public RailwayPointGui_N mRailwayPoint;

	[HideInInspector]
	public UIMallWnd mMallWnd;

	[HideInInspector]
	public UITips mTipsCtrl;

	[HideInInspector]
	public UITipRecordsMgr mTipRecordsMgr;

	[HideInInspector]
	public UIAdminstratorWnd mAdminstratorWnd;

	[HideInInspector]
	public UIDrivingCtrl mDrivingCtrl;

	[HideInInspector]
	public UIStopwatchList mStopwatchList;

	[HideInInspector]
	public UINpcDialog mNpcDialog;

	[HideInInspector]
	public UIMissionGoal mMissionGoal;

	[HideInInspector]
	public UIMissionTrack mCustomMissionTrack;

	[HideInInspector]
	public KickstarterCtrl mKickstarterCtrl;

	[HideInInspector]
	public NpcTalkHistoryWnd mNpcTalkHistoryWnd;

	private List<UIBaseWidget> mShowSystemWnds;

	private PackageCmpt cmpt;

	public bool refalhUI = true;

	private AudioController m_CompoundAudioCtrl;

	private AudioController wndOpenAudioCtrl;

	public List<UIBaseWidget> NeedPlayAudioWndList = new List<UIBaseWidget>();

	public static GameUI Instance => mInstance;

	public bool SystemUIIsOpen
	{
		get
		{
			if (mShowSystemWnds != null && mShowSystemWnds.Count > 0)
			{
				return true;
			}
			return false;
		}
	}

	[HideInInspector]
	public PeEntity mMainPlayer => PeSingleton<PeCreature>.Instance.mainPlayer;

	public bool bVoxelComplete => VFVoxelTerrain.TerrainVoxelComplete;

	public int playerMoney
	{
		get
		{
			if (mMainPlayer == null)
			{
				return 0;
			}
			if (cmpt == null)
			{
				cmpt = mMainPlayer.GetCmpt<PackageCmpt>();
			}
			if (cmpt != null)
			{
				return cmpt.money.current;
			}
			return 0;
		}
		set
		{
			if (cmpt == null)
			{
				cmpt = mMainPlayer.GetCmpt<PackageCmpt>();
			}
			cmpt.money.current = value;
		}
	}

	public bool bMainPlayerIsDead
	{
		get
		{
			if (PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer)
			{
				return PeSingleton<PeCreature>.Instance.mainPlayer.IsDeath();
			}
			return false;
		}
	}

	public bool bReflashUI => refalhUI;

	public bool bPlayerOnCarrier => false;

	public bool bPlayerOnTrain => false;

	private void Awake()
	{
		mInstance = this;
		mShowSystemWnds = new List<UIBaseWidget>();
		mSystemUIMaskCollider.enabled = false;
		InstantiateGameUI();
		OnCreateGameUI();
		InitNeedPlayOpenAudioList();
	}

	private void OnDestroy()
	{
		OnDestroyGameUI();
	}

	private void InstantiateGameUI()
	{
		GameObject gameObject = AddUIPrefab(GameMainPrefab, base.gameObject.transform);
		mUIGameMenuCtrl = gameObject.GetComponentsInChildren<UIGameMenuCtrl>(includeInactive: true)[0];
		mUIGameMenuCtrl.Show();
		mUIMainMidCtrl = gameObject.GetComponentsInChildren<UIMainMidCtrl>(includeInactive: true)[0];
		mUIMainMidCtrl.Show();
		mUIMinMapCtrl = gameObject.GetComponentsInChildren<UIMinMapCtrl>(includeInactive: true)[0];
		mUIMinMapCtrl.Show();
		mNPCTalk = gameObject.GetComponentsInChildren<UINPCTalk>(includeInactive: true)[0];
		mNPCTalk.Hide();
		mNPCSpeech = gameObject.GetComponentInChildren<UINpcSpeech>();
		mServantTalk = gameObject.GetComponentsInChildren<UIServantTalk>(includeInactive: true)[0];
		mServantTalk.Hide();
		gameObject.SetActive(value: true);
		gameObject = AddUIPrefab(SytemPrefab, base.gameObject.transform);
		mSystemMenu = gameObject.GetComponentInChildren<UISystemMenu>();
		UISystemMenu uISystemMenu = mSystemMenu;
		uISystemMenu.e_OnShow = (UIBaseWidget.WndEvent)Delegate.Combine(uISystemMenu.e_OnShow, new UIBaseWidget.WndEvent(OnSystemWndShow));
		UISystemMenu uISystemMenu2 = mSystemMenu;
		uISystemMenu2.e_OnHide = (UIBaseWidget.WndEvent)Delegate.Combine(uISystemMenu2.e_OnHide, new UIBaseWidget.WndEvent(OnSystemWndHide));
		mOption = gameObject.GetComponentInChildren<UIOption>();
		UIOption uIOption = mOption;
		uIOption.e_OnShow = (UIBaseWidget.WndEvent)Delegate.Combine(uIOption.e_OnShow, new UIBaseWidget.WndEvent(OnSystemWndShow));
		UIOption uIOption2 = mOption;
		uIOption2.e_OnHide = (UIBaseWidget.WndEvent)Delegate.Combine(uIOption2.e_OnHide, new UIBaseWidget.WndEvent(OnSystemWndHide));
		mSaveLoad = gameObject.GetComponentInChildren<UISaveLoad>();
		UISaveLoad uISaveLoad = mSaveLoad;
		uISaveLoad.e_OnShow = (UIBaseWidget.WndEvent)Delegate.Combine(uISaveLoad.e_OnShow, new UIBaseWidget.WndEvent(OnSystemWndShow));
		UISaveLoad uISaveLoad2 = mSaveLoad;
		uISaveLoad2.e_OnHide = (UIBaseWidget.WndEvent)Delegate.Combine(uISaveLoad2.e_OnHide, new UIBaseWidget.WndEvent(OnSystemWndHide));
		gameObject.SetActive(value: true);
		gameObject = AddUIPrefab(worldMapPrefab, base.gameObject.transform);
		mUIWorldMap = gameObject.GetComponent<UIWorldMapCtrl>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(buildPrefab, base.gameObject.transform);
		mBuildBlock = gameObject.GetComponent<UIBuildBlock>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(tipsWmdPrefab, base.gameObject.transform);
		mTipsCtrl = gameObject.GetComponent<UITips>();
		gameObject.SetActive(value: true);
		gameObject = AddUIPrefab(tipRecordsMgrPrefab, base.gameObject.transform);
		mTipRecordsMgr = gameObject.GetComponent<UITipRecordsMgr>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(playerInfoPrefab, tsCenter);
		mUIPlayerInfoCtrl = gameObject.GetComponent<UIPlayerInfoCtrl>();
		gameObject.transform.localPosition = UIDefaultPostion.Instance.pos_PlayerInfo;
		gameObject.SetActive(value: false);
		mUIPlayerInfoCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_PlayerInfo;
		gameObject = AddUIPrefab(ItemPackagePrefab, tsCenter);
		mItemPackageCtrl = gameObject.GetComponent<UIItemPackageCtrl>();
		gameObject.SetActive(value: false);
		mItemPackageCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_ItemPackge;
		gameObject = AddUIPrefab(npcStoragePrefab, tsCenter);
		mNpcStrageCtrl = gameObject.GetComponent<UINpcStorageCtrl>();
		gameObject.SetActive(value: false);
		mNpcStrageCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_NpcStorage;
		gameObject = AddUIPrefab(compoundWndPrefab, tsCenter);
		mCompoundWndCtrl = gameObject.GetComponent<UICompoundWndControl>();
		gameObject.SetActive(value: false);
		mCompoundWndCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_Compound;
		gameObject = AddUIPrefab(servantWndPrefab, tsCenter);
		mServantWndCtrl = gameObject.GetComponent<UIServantWnd>();
		gameObject.SetActive(value: false);
		mServantWndCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_Servant;
		gameObject = AddUIPrefab(npcGuiPrefab, tsCenter);
		mNpcWnd = gameObject.GetComponent<UINpcWnd>();
		gameObject.SetActive(value: false);
		mNpcWnd.transform.localPosition = UIDefaultPostion.Instance.pos_Npc;
		gameObject = AddUIPrefab(missionWndPrefab, tsCenter);
		mUIMissionWndCtrl = gameObject.GetComponent<UIMissionWndCtrl>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(missionTrackWndPrefab, tsCenterOther);
		mMissionTrackWnd = gameObject.GetComponent<UIMissionTrackCtrl>();
		mMissionTrackWnd.transform.localPosition = UIDefaultPostion.Instance.pos_MissionTruck;
		mMissionTrackWnd.transform.localPosition = new Vector3(Screen.width / 2 - 145, 35f, 0f);
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(itemGetWndPrefab, tsCenter);
		mItemGet = gameObject.GetComponent<UIItemGet>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(itemOpGuiPrefab, tsCenter);
		mItemOp = gameObject.GetComponent<UIItemOp>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(shopGuiPrefab, tsCenter);
		mShopWnd = gameObject.GetComponent<UIShopWnd>();
		gameObject.SetActive(value: false);
		mShopWnd.transform.localPosition = UIDefaultPostion.Instance.pos_Shop;
		gameObject = AddUIPrefab(itemBoxGuiPrefab, tsCenter);
		mItemBox = gameObject.GetComponent<UIItemBox>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(repairWndGuiPrefab, tsCenter);
		mRepair = gameObject.GetComponent<UIRepairWnd>();
		gameObject.SetActive(value: false);
		mRepair.transform.localPosition = UIDefaultPostion.Instance.pos_Repair;
		gameObject = AddUIPrefab(powerPlantSolarPrefab, tsCenter);
		mPowerPlantSolar = gameObject.GetComponent<UIPowerPlantSolar>();
		gameObject.SetActive(value: false);
		mPowerPlantSolar.transform.localPosition = UIDefaultPostion.Instance.pos_PowerPlant;
		gameObject = AddUIPrefab(reviveGuiPrefab, tsCenter);
		mRevive = gameObject.GetComponent<UIRevive>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(wareHouseGuiPrefab, tsCenter);
		mWarehouse = gameObject.GetComponent<UIWarehouse>();
		gameObject.SetActive(value: false);
		mWarehouse.transform.localPosition = UIDefaultPostion.Instance.pos_WareHouse;
		gameObject = AddUIPrefab(colonyWndPrefab, tsCenter);
		mCSUI_MainWndCtrl = gameObject.GetComponent<CSUI_MainWndCtrl>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(phoneWndPrefab, tsCenter);
		mPhoneWnd = gameObject.GetComponent<UIPhoneWnd>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(skillWndPrefab, tsCenter);
		mSkillWndCtrl = gameObject.GetComponent<UISkillWndCtrl>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(workshopPrefb, tsCenter);
		mWorkShopCtrl = gameObject.GetComponent<UIWorkShopCtrl>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(informationPrefab, tsCenter);
		mTeamInfoMgr = gameObject.GetComponent<CSUI_TeamInfoMgr>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(railwayPonitPrefab, tsCenter);
		mRailwayPoint = gameObject.GetComponent<RailwayPointGui_N>();
		gameObject.SetActive(value: false);
		if (PeGameMgr.IsMulti)
		{
			gameObject = AddUIPrefab(mallWndPrefab, tsCenter);
			mMallWnd = gameObject.GetComponent<UIMallWnd>();
			gameObject.SetActive(value: false);
		}
		gameObject = AddUIPrefab(AdministratorPrefab, tsCenter);
		mAdminstratorWnd = gameObject.GetComponent<UIAdminstratorWnd>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(drivingPrefab, tsCenterOther);
		mDrivingCtrl = gameObject.GetComponent<UIDrivingCtrl>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(stopwatchPrefab, base.transform);
		mStopwatchList = gameObject.GetComponent<UIStopwatchList>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(npcWndCustomPrefab, tsCenter);
		mNpcDialog = gameObject.GetComponent<UINpcDialog>();
		gameObject = AddUIPrefab(missionWndCustomPrefab, tsCenter);
		mMissionGoal = gameObject.GetComponent<UIMissionGoal>();
		gameObject = AddUIPrefab(missionTrackCustomPrefab, tsCenterOther);
		mCustomMissionTrack = gameObject.GetComponent<UIMissionTrack>();
		mCustomMissionTrack.transform.localPosition = UIDefaultPostion.Instance.pos_MissionTruck;
		mCustomMissionTrack.transform.localPosition = new Vector3(Screen.width / 2 - 145, 35f, 0f);
		gameObject = AddUIPrefab(mKickstarterCtrlPrefab, tsCenter);
		mKickstarterCtrl = gameObject.GetComponent<KickstarterCtrl>();
		gameObject.SetActive(value: false);
		gameObject = AddUIPrefab(mNpcTalkHistoryPrefab, tsCenter);
		mNpcTalkHistoryWnd = gameObject.GetComponent<NpcTalkHistoryWnd>();
		gameObject.SetActive(value: false);
	}

	private GameObject AddUIPrefab(GameObject prefab, Transform parentTs)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		gameObject.transform.parent = parentTs;
		gameObject.layer = parentTs.gameObject.layer;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		UIBaseWnd component = gameObject.GetComponent<UIBaseWnd>();
		if (component != null)
		{
			component.mAnchor = parentTs.gameObject.GetComponent<UIAnchor>();
		}
		UIAnchor[] componentsInChildren = gameObject.transform.GetComponentsInChildren<UIAnchor>(includeInactive: true);
		UIAnchor[] array = componentsInChildren;
		foreach (UIAnchor uIAnchor in array)
		{
			uIAnchor.uiCamera = mUICamera;
		}
		return gameObject;
	}

	private void OnCreateGameUI()
	{
		UIBaseWidget[] componentsInChildren = GetComponentsInChildren<UIBaseWidget>(includeInactive: true);
		UIBaseWidget[] array = componentsInChildren;
		foreach (UIBaseWidget uIBaseWidget in array)
		{
			uIBaseWidget.OnCreate();
		}
	}

	private void OnDestroyGameUI()
	{
		PeGameMgr.gamePause = false;
		UIBaseWidget[] componentsInChildren = GetComponentsInChildren<UIBaseWidget>(includeInactive: true);
		UIBaseWidget[] array = componentsInChildren;
		foreach (UIBaseWidget uIBaseWidget in array)
		{
			uIBaseWidget.OnDelete();
		}
	}

	private void OnSystemWndShow(UIBaseWidget widget = null)
	{
		if (!mShowSystemWnds.Contains(widget))
		{
			if (null != LockUI.instance)
			{
				LockUI.instance.HideWhenUIPopup();
			}
			if (null != mSystemUIMaskCollider)
			{
				mSystemUIMaskCollider.enabled = true;
			}
			mShowSystemWnds.Add(widget);
			PeGameMgr.gamePause = true;
		}
	}

	private void OnSystemWndHide(UIBaseWidget widget = null)
	{
		if (mShowSystemWnds.Remove(widget) && mShowSystemWnds.Count == 0)
		{
			if (null != LockUI.instance)
			{
				LockUI.instance.ShowWhenUIDisappear();
			}
			if (null != mSystemUIMaskCollider)
			{
				mSystemUIMaskCollider.enabled = false;
			}
			PeGameMgr.gamePause = false;
		}
	}

	public void ChangeGameWndShowState()
	{
		tsCenter.gameObject.SetActive(!tsCenter.gameObject.activeSelf);
		tsCenterOther.gameObject.SetActive(!tsCenterOther.gameObject.activeSelf);
	}

	public void ShowGameWndAll()
	{
		ShowGameWnd();
		ShowGameWndOther();
	}

	public void ShowGameWnd()
	{
		tsCenter.gameObject.SetActive(value: true);
	}

	public void ShowGameWndOther()
	{
		tsCenterOther.gameObject.SetActive(value: true);
	}

	public void HideGameWndAll()
	{
		HideGameWnd();
		HideGameWndOther();
	}

	public void HideGameWnd()
	{
		tsCenter.gameObject.SetActive(value: false);
	}

	public void HideGameWndOther()
	{
		tsCenterOther.gameObject.SetActive(value: false);
	}

	public void PlayPutOnEquipAudio()
	{
		if (null != AudioManager.instance)
		{
			AudioManager.instance.Create(Vector3.zero, 1683);
		}
	}

	public void PlayTakeOffEquipAudio()
	{
		if (null != AudioManager.instance)
		{
			AudioManager.instance.Create(Vector3.zero, 1684);
		}
	}

	public void PlayCompoundAudioEffect()
	{
		if (null == m_CompoundAudioCtrl)
		{
			m_CompoundAudioCtrl = AudioManager.instance.Create(Vector3.zero, 1685, null, isPlay: false, isDelete: false);
		}
		if (null != m_CompoundAudioCtrl && !m_CompoundAudioCtrl.isPlaying)
		{
			m_CompoundAudioCtrl.PlayAudio();
		}
	}

	public void StopCompoundAudioEffect()
	{
		if (null != m_CompoundAudioCtrl)
		{
			m_CompoundAudioCtrl.StopAudio();
		}
	}

	public void PlayWndOpenAudioEffect(UIBaseWidget widget)
	{
		if (null != widget && NeedPlayAudioWndList.Contains(widget))
		{
			PlayWndOpenAudioEffect();
		}
	}

	public void PlayWndOpenAudioEffect()
	{
		if (null == wndOpenAudioCtrl)
		{
			wndOpenAudioCtrl = AudioManager.instance.Create(Vector3.zero, 914, null, isPlay: false, isDelete: false);
		}
		if (null != wndOpenAudioCtrl && !wndOpenAudioCtrl.isPlaying)
		{
			wndOpenAudioCtrl.PlayAudio();
		}
	}

	public void PlayNpcTalkWndOpenAudioEffect()
	{
		AudioManager.instance.Create(Vector3.zero, 4104);
	}

	private void InitNeedPlayOpenAudioList()
	{
		NeedPlayAudioWndList.Add(mTipRecordsMgr);
		NeedPlayAudioWndList.Add(mSystemMenu);
		NeedPlayAudioWndList.Add(mOption);
		NeedPlayAudioWndList.Add(mSaveLoad);
		NeedPlayAudioWndList.Add(mServantWndCtrl);
		NeedPlayAudioWndList.Add(mUIPlayerInfoCtrl);
		NeedPlayAudioWndList.Add(mUIMissionWndCtrl);
		NeedPlayAudioWndList.Add(mCustomMissionTrack.GetBaseWnd());
		NeedPlayAudioWndList.Add(mPhoneWnd);
		NeedPlayAudioWndList.Add(mCSUI_MainWndCtrl);
		NeedPlayAudioWndList.Add(mCompoundWndCtrl);
		NeedPlayAudioWndList.Add(mItemPackageCtrl);
		NeedPlayAudioWndList.Add(mItemOp);
		NeedPlayAudioWndList.Add(mNpcWnd);
	}

	public void CheckTalkIDShowTutorial(int talkID)
	{
		if (!PeGameMgr.IsTutorial)
		{
			return;
		}
		switch (talkID)
		{
		case 4076:
			mBuildBlock.ShowAllBuildTutorial();
			break;
		case 4343:
			mUIMainMidCtrl.ShowFullQuickBarTutorial();
			break;
		case 4344:
			if (null != UIGameMenuCtrl.Instance)
			{
				UIGameMenuCtrl.Instance.ShowWndTutorial();
			}
			break;
		case 4345:
			if (null != UIMinMapCtrl.Instance)
			{
				UIMinMapCtrl.Instance.ShowMapTutorial();
			}
			break;
		case 4346:
			if (null != UIMinMapCtrl.Instance)
			{
				UIMinMapCtrl.Instance.ShowMissionTrackTutorial();
			}
			break;
		}
	}

	public void CheckMissionIDShowTutorial(int missionID)
	{
		if (PeGameMgr.IsTutorial)
		{
			switch (missionID)
			{
			case 739:
				mMissionTrackWnd.ShowWndTutorial();
				break;
			case 744:
				mItemPackageCtrl.AddTutorialItemProtoID(916);
				mUIMainMidCtrl.ShowQuickBarTutorial();
				break;
			case 745:
				mItemPackageCtrl.AddTutorialItemProtoID(33);
				break;
			case 748:
				mItemPackageCtrl.AddTutorialItemProtoID(1527);
				break;
			case 751:
				mItemPackageCtrl.AddTutorialItemProtoID(12);
				break;
			case 757:
				mItemPackageCtrl.AddTutorialItemProtoID(406);
				break;
			}
		}
	}
}
