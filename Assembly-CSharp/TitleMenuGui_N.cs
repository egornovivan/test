using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class TitleMenuGui_N : UIStaticWnd
{
	private static TitleMenuGui_N mInstance;

	public GameObject mMainMenu;

	public GameObject mSingleMenu;

	public GameObject mGraphWaringWnd;

	public UILabel mGraphWaringLbl;

	public UIButton mContinueBtn;

	public UIButton mLoadBtn;

	public Transform mWndTran;

	public GameObject mControlWnd;

	public GameObject mMask;

	public GameObject mFixeBlurryWnd;

	public UICheckbox mFixBlurryCB;

	public UILabel mVersionLabel;

	public GameObject mPEModel;

	public UICustomGameSelectWnd mCustomSelectWnd;

	public List<Transform> menuMain;

	public List<Transform> menuSingle;

	public static TitleMenuGui_N Instance => mInstance;

	private void Start()
	{
		mInstance = this;
		PeGameMgr.playerType = PeGameMgr.EPlayerType.Multiple;
		PeGameMgr.gameLevel = PeGameMgr.EGameLevel.Normal;
		if (!string.IsNullOrEmpty(GlobalBehaviour.BadGfxDeviceName))
		{
			mGraphWaringLbl.text = "  " + PELocalization.GetString(8000691).Replace("$A$", GlobalBehaviour.BadGfxDeviceName);
			mGraphWaringWnd.SetActive(value: true);
		}
		else
		{
			mGraphWaringWnd.SetActive(value: false);
			CheckControlWndNeedShow();
		}
		InitVersion();
		mCustomSelectWnd.onOpen += OnCustomWndOpen;
		mCustomSelectWnd.onClose += OnCustomWndClose;
	}

	private void Update()
	{
		if (SystemSettingData.Instance == null || !mControlWnd.activeSelf || SystemSettingData.Instance.FixBlurryFont == mFixBlurryCB.isChecked)
		{
			return;
		}
		SystemSettingData.Instance.FixBlurryFont = mFixBlurryCB.isChecked;
		if (UIFontMgr.Instance != null)
		{
			UILabel[] componentsInChildren = UIFontMgr.Instance.gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].MakePixelPerfect();
			}
		}
	}

	private void InitVersion()
	{
		mVersionLabel.text = GameConfig.GameVersion;
	}

	private void OnSingleplayerBtn()
	{
		PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
	}

	public void OnMultiplayerBtn()
	{
		PeGameMgr.playerType = PeGameMgr.EPlayerType.Multiple;
		RandomMapConfig.useSkillTree = false;
		GameClientLobby.ConnectToLobby();
		MessageBox_N.ShowMaskBox(MsgInfoType.LobbyLoginMask, PELocalization.GetString(8000118), 30f);
	}

	private void OnCreateBtn()
	{
		VCEditor.Open();
	}

	private void OnTutorialBtn()
	{
		UIPlayerBuildCtrl.MainmenuToTutorial();
	}

	private void OnOptionsBtn()
	{
		Hide();
		UIOption.Instance.mParentWnd = this;
		UIOption.Instance.Show();
		UIOption.Instance.OnVideoBtn();
	}

	private void OnCreditsBtn()
	{
		Application.LoadLevel("GameCredits");
	}

	private void OnBoardBtn()
	{
		Application.OpenURL("http://board.pathea.net/");
	}

	private void OnQuitBtn()
	{
		Application.Quit();
	}

	private void OnStoryBtn()
	{
		PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
		PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Story;
	}

	private void OnAdventureBtn()
	{
		PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
		PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Adventure;
	}

	private void OnEasyModeBtn()
	{
		PeGameMgr.gameLevel = PeGameMgr.EGameLevel.Easy;
		LoadRoleScene();
	}

	private void OnNormalModeBtn()
	{
		PeGameMgr.gameLevel = PeGameMgr.EGameLevel.Normal;
		LoadRoleScene();
	}

	private void LoadRoleScene()
	{
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.RoleScene);
	}

	private void OnBuildBtn()
	{
		PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
		PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Build;
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.RoleScene);
	}

	private void OnContinueBtn()
	{
		ArchiveMgr.ESave eSave = ArchiveMgr.ESave.Min;
		string value = PeSingleton<ArchiveMgr>.Instance.Load(eSave);
		PeGameSummary peGameSummary = null;
		if (!string.IsNullOrEmpty(value))
		{
			peGameSummary = PeSingleton<PeGameSummary.Mgr>.Instance.Get();
		}
		if (peGameSummary == null)
		{
			Debug.Log(string.Concat("<color=aqua>Failed continue archive:", eSave, "</color>"));
			return;
		}
		PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
		PeGameMgr.loadArchive = eSave;
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene, save: false);
	}

	private void OnCoustomBtn()
	{
		mCustomSelectWnd.Open();
	}

	private void OnLoadBtn()
	{
		Hide();
		UISaveLoad.Instance.Show();
		UISaveLoad.Instance.ToLoadWnd();
		UISaveLoad.Instance.HideSaveTabBtn();
	}

	private void OnCustomWndOpen()
	{
		if (mPEModel != null)
		{
			mPEModel.gameObject.SetActive(value: false);
		}
	}

	private void OnCustomWndClose()
	{
		if (mPEModel != null)
		{
			mPEModel.gameObject.SetActive(value: true);
		}
	}

	private void OnFixBlurryClose()
	{
		mFixeBlurryWnd.SetActive(value: false);
		SystemSettingData.Instance.mMMOControlType = true;
		SystemSettingData.Instance.ApplyKeySetting();
		mControlWnd.SetActive(value: false);
	}

	private void CheckControlWndNeedShow()
	{
		if (!SystemSettingData.Instance.mHasData)
		{
			mControlWnd.SetActive(value: true);
			SystemSettingData.Instance.mHasData = true;
			mFixBlurryCB.isChecked = SystemSettingData.Instance.FixBlurryFont;
		}
	}

	private void OnGraphWaringWndClose()
	{
		mGraphWaringWnd.SetActive(value: false);
		CheckControlWndNeedShow();
	}
}
