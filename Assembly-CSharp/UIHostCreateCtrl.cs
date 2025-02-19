using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UIHostCreateCtrl : MonoBehaviour
{
	public UIServerCtrl mServerCtrl;

	public UIMultiCustomCtrl mMultiCustomCtrl;

	public UIInput mServerName;

	public UIInput mPassword;

	public UILabel mMapName;

	public UICheckbox mPrivateServer;

	public UICheckbox mInfiniteResoureces;

	public UICheckbox mProxyServer;

	public UICheckbox mSkillTreeSystem;

	public UICheckbox mAllScriptsAvailable;

	public UIPopupList mBiomePop;

	public UIPopupList mWeatherPop;

	public UIPopupList mGameModePop;

	public UIPopupList mGameTypePop;

	public UILabel mTeamNum;

	public UILabel mPlayerNum;

	public UILabel mDropRateNum;

	public UILabel mSeed;

	public UILabel mMonsterText;

	public UISlicedSprite mMonsterLeftBtnBg;

	public UISlicedSprite mMonsterRightBtnBg;

	public UISlicedSprite mTeamLeftBtnBg;

	public UISlicedSprite mTeamRightBtnBg;

	public UIScrollBar mSbTerrainHeight;

	public UIScrollBar mSbMapSzie;

	public UIScrollBar mSbHostilityAllyCount;

	public UIScrollBar mSbRiverDensity;

	public UIScrollBar mSbRiverWidth;

	public UIScrollBar mSbPlainHeight;

	public UIScrollBar mSbFlatness;

	public UIScrollBar mSbBridgeMaxHeight;

	public UILabel mLbTerrainHeightInfo;

	public UILabel mLbMapSizeInfo;

	public UILabel mLbHostilityAllyCount;

	public UILabel mLbRiverDensityInfo;

	public UILabel mLbRiverWidthInfo;

	public UILabel mLbPlainHeight;

	public UILabel mLbFlatness;

	public UILabel mLbBridgeMaxHeight;

	private bool mMonsterYes;

	private string mGameTypeText = string.Empty;

	private string mGameModeText = string.Empty;

	private string mUID = string.Empty;

	public List<string> mDropRateSelections;

	private UIAlphaGroup[] mAlphaGroupsForCustom;

	public UIAlphaGroup[] mAlphaGroupsForStory;

	public BoxCollider[] mCollidersForCustom;

	public BoxCollider[] mCollidersForStory;

	public TweenPosition[] mTPs;

	private List<MapConfig> mMapList = new List<MapConfig>();

	private int mMapIndex;

	private int teamNum;

	private int playerNum;

	private int mDropRateIndex;

	private int mTerrainHeight = 128;

	private int mMapSize;

	private int mHostilityAllyCount = 3;

	private int mRiverDensity;

	private int mRiverWidth;

	private int plainHeight = 30;

	private int flatness = 50;

	private int bridgeMaxHeight;

	public bool UseAnimation = true;

	private int mAllyCount => mHostilityAllyCount + 1;

	public string UID
	{
		get
		{
			return mUID;
		}
		set
		{
			mUID = value;
		}
	}

	private void Awake()
	{
		mAlphaGroupsForCustom = GetComponentsInChildren<UIAlphaGroup>();
	}

	private void Start()
	{
		mSbTerrainHeight.scrollValue = 1f;
		mSbMapSzie.scrollValue = 0.5f;
		mSbHostilityAllyCount.scrollValue = 1f;
		mSbRiverDensity.scrollValue = 0.25f;
		mSbRiverWidth.scrollValue = 0.1f;
		mSbPlainHeight.scrollValue = 0.15f;
		mSbFlatness.scrollValue = 0.25f;
		mSbBridgeMaxHeight.scrollValue = 0f;
		UpdateScrollValueChage();
	}

	public void InitMapInfo()
	{
		mMapList.Clear();
		mMapList.AddRange(MapsConfig.Self.PatheaMapConfig);
		mMapIndex = 0;
		mDropRateIndex = 0;
		teamNum = 1;
		playerNum = 4;
		mGameModePop.items.Clear();
		mGameTypePop.items.Clear();
		mBiomePop.items.Clear();
		mWeatherPop.items.Clear();
		if (mMapList.Count > 0)
		{
			mMapName.text = mMapList[mMapIndex].MapName;
			mGameModePop.items.AddRange(mMapList[mMapIndex].GameMode);
			mGameTypePop.items.AddRange(mMapList[mMapIndex].GameType);
			mBiomePop.items.AddRange(mMapList[mMapIndex].MapTerrainType);
			mWeatherPop.items.AddRange(mMapList[mMapIndex].MapWeatherType);
			mBiomePop.selection = mMapList[mMapIndex].MapTerrainType[0];
			mWeatherPop.selection = mMapList[mMapIndex].MapWeatherType[0];
			mGameModePop.selection = mMapList[mMapIndex].GameMode[0];
			mGameTypePop.selection = mMapList[mMapIndex].GameType[0];
			ResetMapList();
			mMonsterYes = true;
			mMonsterText.text = "YES";
		}
		else
		{
			mMapName.text = string.Empty;
			mGameModePop.items.Clear();
			mGameTypePop.items.Clear();
			mBiomePop.items.Clear();
			mWeatherPop.items.Clear();
			mTeamNum.text = string.Empty;
			mPlayerNum.text = string.Empty;
			mDropRateNum.text = string.Empty;
		}
	}

	private void UpdateMonsterState()
	{
		float num = 0.3f;
		if (mGameTypeText == "VS" && mGameModeText == "Adventure")
		{
			mMonsterLeftBtnBg.color = new Color(1f, 1f, 1f, 1f);
			mMonsterRightBtnBg.color = new Color(1f, 1f, 1f, 1f);
			BoxCollider component = mMonsterLeftBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
			component.enabled = true;
			BoxCollider component2 = mMonsterRightBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
			component2.enabled = true;
			return;
		}
		mMonsterLeftBtnBg.color = new Color(num, num, num, 1f);
		mMonsterRightBtnBg.color = new Color(num, num, num, 1f);
		BoxCollider component3 = mMonsterLeftBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
		component3.enabled = false;
		BoxCollider component4 = mMonsterRightBtnBg.transform.parent.gameObject.GetComponent<BoxCollider>();
		component4.enabled = false;
		if (mGameTypeText == "Cooperation" && mGameModeText == "Adventure")
		{
			mMonsterYes = true;
			mMonsterText.text = "YES";
		}
		else
		{
			mMonsterYes = false;
			mMonsterText.text = "NO";
		}
	}

	private void ResetMapList()
	{
		switch (PeGameMgr.gameType)
		{
		case PeGameMgr.EGameType.VS:
			teamNum = 2;
			playerNum = 4;
			mDropRateIndex = 0;
			break;
		case PeGameMgr.EGameType.Survive:
			teamNum = 0;
			playerNum = 4;
			mDropRateIndex = 0;
			break;
		case PeGameMgr.EGameType.Cooperation:
			teamNum = 1;
			playerNum = 4;
			mDropRateIndex = 0;
			break;
		default:
			teamNum = 1;
			playerNum = 32;
			mDropRateIndex = 0;
			break;
		}
		mTeamNum.text = teamNum.ToString();
		mPlayerNum.text = playerNum.ToString();
		mDropRateNum.text = mDropRateSelections[mDropRateIndex] + "%";
	}

	private void OnCancelHostBtn()
	{
		if (Input.GetMouseButtonUp(0))
		{
			mServerCtrl.gameObject.SetActive(value: true);
			mMultiCustomCtrl.Close();
			StartCoroutine(CloseThisObj(DurTime()));
		}
	}

	private float DurTime()
	{
		float num = 0f;
		TweenPosition[] array = mTPs;
		foreach (TweenPosition tweenPosition in array)
		{
			if (num < tweenPosition.duration)
			{
				num = tweenPosition.duration;
			}
		}
		return num;
	}

	private IEnumerator CloseThisObj(float _wait)
	{
		yield return new WaitForSeconds(_wait + 0.1f);
		base.gameObject.SetActive(value: false);
	}

	private void OnCreateHostBtn()
	{
		if (string.IsNullOrEmpty(mServerName.text))
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000059));
		}
		else if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Custom)
		{
			if (mMultiCustomCtrl.HasSelectMap())
			{
				if (LoadServer.Exist(mServerName.text))
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000485));
					return;
				}
				mMultiCustomCtrl.OnWndStartClick();
				StartCoroutine(WhetherMapCheckHasFinished());
			}
		}
		else if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
		{
			string text = mServerName.text;
			string text2 = mPassword.text;
			string text3 = mMapName.text;
			bool flag = !mPrivateServer.isChecked;
			switch (mBiomePop.selection)
			{
			case "Grassland":
				RandomMapConfig.RandomMapID = RandomMapType.GrassLand;
				RandomMapConfig.vegetationId = RandomMapType.GrassLand;
				break;
			case "Forest":
				RandomMapConfig.RandomMapID = RandomMapType.Forest;
				RandomMapConfig.vegetationId = RandomMapType.Forest;
				break;
			case "Desert":
				RandomMapConfig.RandomMapID = RandomMapType.Desert;
				RandomMapConfig.vegetationId = RandomMapType.Desert;
				break;
			case "Redstone":
				RandomMapConfig.RandomMapID = RandomMapType.Redstone;
				RandomMapConfig.vegetationId = RandomMapType.Redstone;
				break;
			case "Rainforest":
				RandomMapConfig.RandomMapID = RandomMapType.Rainforest;
				RandomMapConfig.vegetationId = RandomMapType.Rainforest;
				break;
			case "Mountain":
				RandomMapConfig.RandomMapID = RandomMapType.Mountain;
				RandomMapConfig.vegetationId = RandomMapType.Mountain;
				break;
			case "Swamp":
				RandomMapConfig.RandomMapID = RandomMapType.Swamp;
				RandomMapConfig.vegetationId = RandomMapType.Swamp;
				break;
			case "Crater":
				RandomMapConfig.RandomMapID = RandomMapType.Crater;
				RandomMapConfig.vegetationId = RandomMapType.Crater;
				break;
			default:
				RandomMapConfig.RandomMapID = RandomMapType.GrassLand;
				RandomMapConfig.vegetationId = RandomMapType.GrassLand;
				break;
			}
			switch (mWeatherPop.selection)
			{
			case "Dry":
				RandomMapConfig.ScenceClimate = ClimateType.CT_Dry;
				break;
			case "Temperate":
				RandomMapConfig.ScenceClimate = ClimateType.CT_Temperate;
				break;
			case "Wet":
				RandomMapConfig.ScenceClimate = ClimateType.CT_Wet;
				break;
			case "Random":
				RandomMapConfig.ScenceClimate = ClimateType.CT_Random;
				break;
			}
			PeGameMgr.monsterYes = mMonsterYes;
			string text4 = mSeed.text;
			text = text.Trim();
			if (string.IsNullOrEmpty(text))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000060));
				return;
			}
			if (text.Length < 4 || text.Length > 19)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000486));
				return;
			}
			if (LoadServer.Exist(text))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000485));
				return;
			}
			bool isChecked = mInfiniteResoureces.isChecked;
			bool isChecked2 = mProxyServer.isChecked;
			bool isChecked3 = mSkillTreeSystem.isChecked;
			bool isChecked4 = mAllScriptsAvailable.isChecked;
			RandomMapConfig.useSkillTree = isChecked3;
			MyServer myServer = new MyServer();
			myServer.gameName = text;
			myServer.gamePassword = text2;
			myServer.mapName = text3;
			myServer.gameMode = (int)PeGameMgr.sceneMode;
			myServer.gameType = (int)PeGameMgr.gameType;
			myServer.seedStr = text4;
			myServer.teamNum = teamNum;
			myServer.numPerTeam = playerNum;
			myServer.dropDeadPercent = Convert.ToInt32(mDropRateSelections[mDropRateIndex]);
			myServer.terrainType = (int)RandomMapConfig.RandomMapID;
			myServer.vegetationId = (int)RandomMapConfig.vegetationId;
			myServer.sceneClimate = (int)RandomMapConfig.ScenceClimate;
			myServer.monsterYes = PeGameMgr.monsterYes;
			myServer.unlimitedRes = isChecked;
			myServer.terrainHeight = mTerrainHeight;
			myServer.mapSize = mMapSize;
			myServer.riverDensity = mRiverDensity;
			myServer.riverWidth = mRiverWidth;
			myServer.plainHeight = plainHeight;
			myServer.flatness = flatness;
			myServer.bridgeMaxHeight = bridgeMaxHeight;
			myServer.allyCount = mAllyCount;
			myServer.scriptsAvailable = isChecked4;
			myServer.proxyServer = isChecked2;
			myServer.isPrivate = !flag;
			myServer.masterRoleName = GameClientLobby.role.name;
			myServer.useSkillTree = isChecked3;
			myServer.uid = string.Empty;
			MyServerManager.CreateNewServer(myServer);
		}
	}

	private void OnGameModeChange(string item)
	{
		mGameModeText = item;
		if (item.Equals("Adventure"))
		{
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Adventure;
			mSkillTreeSystem.isChecked = false;
			mSkillTreeSystem.gameObject.SetActive(value: true);
			mInfiniteResoureces.isChecked = false;
			mInfiniteResoureces.gameObject.SetActive(value: true);
			OnExitStoryMode();
			mMultiCustomCtrl.Close();
			OnGameTypeChangs(mGameTypeText);
		}
		else if (item.Equals("Build"))
		{
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Build;
			mSkillTreeSystem.isChecked = false;
			mSkillTreeSystem.gameObject.SetActive(value: false);
			mInfiniteResoureces.isChecked = false;
			mInfiniteResoureces.gameObject.SetActive(value: true);
			OnExitStoryMode();
			mMultiCustomCtrl.Close();
			OnGameTypeChangs(mGameTypeText);
		}
		else if (item.Equals("Story"))
		{
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Story;
			mSkillTreeSystem.isChecked = false;
			mSkillTreeSystem.gameObject.SetActive(value: false);
			mInfiniteResoureces.isChecked = false;
			mInfiniteResoureces.gameObject.SetActive(value: false);
			mMonsterYes = true;
			mMonsterText.text = "YES";
			mMultiCustomCtrl.Close();
			string text = mMapList[mMapIndex].GameType[0];
			mGameTypePop.selection = text;
			OnGameTypeChangs(text);
			OnStoryMode();
		}
		else if (item.Equals("Custom"))
		{
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Custom;
			mSkillTreeSystem.isChecked = false;
			mSkillTreeSystem.gameObject.SetActive(value: false);
			mInfiniteResoureces.isChecked = false;
			mInfiniteResoureces.gameObject.SetActive(value: false);
			string text2 = mMapList[mMapIndex].GameType[2];
			mGameTypePop.selection = text2;
			OnGameTypeChangs(text2);
			OnExitStoryMode();
			mMultiCustomCtrl.Open();
			return;
		}
		ResetMapList();
	}

	private void OnGameTypeChangs(string item)
	{
		mGameTypeText = item;
		if (item == "VS")
		{
			PeGameMgr.gameType = PeGameMgr.EGameType.VS;
			mTeamLeftBtnBg.color = new Color(1f, 1f, 1f, 1f);
			mTeamRightBtnBg.color = new Color(1f, 1f, 1f, 1f);
			mAllScriptsAvailable.isChecked = true;
			mAllScriptsAvailable.gameObject.SetActive(value: true);
		}
		else if (item == "Survival")
		{
			PeGameMgr.gameType = PeGameMgr.EGameType.Survive;
			float num = 0.3f;
			mTeamLeftBtnBg.color = new Color(num, num, num, 1f);
			mTeamRightBtnBg.color = new Color(num, num, num, 1f);
			mAllScriptsAvailable.isChecked = true;
			mAllScriptsAvailable.gameObject.SetActive(value: true);
		}
		else
		{
			PeGameMgr.gameType = PeGameMgr.EGameType.Cooperation;
			float num2 = 0.3f;
			mTeamLeftBtnBg.color = new Color(num2, num2, num2, 1f);
			mTeamRightBtnBg.color = new Color(num2, num2, num2, 1f);
			if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Adventure)
			{
				if (!mAllScriptsAvailable.gameObject.activeInHierarchy)
				{
					mAllScriptsAvailable.isChecked = true;
					mAllScriptsAvailable.gameObject.SetActive(value: true);
				}
			}
			else
			{
				mAllScriptsAvailable.isChecked = false;
				mAllScriptsAvailable.gameObject.SetActive(value: false);
			}
		}
		ResetMapList();
	}

	private void OnMapSelectLeft()
	{
		if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
		{
			mMapIndex--;
			if (mMapIndex < 0)
			{
				mMapIndex = mMapList.Count - 1;
			}
			mGameModePop.items.Clear();
			mGameTypePop.items.Clear();
			mBiomePop.items.Clear();
			mWeatherPop.items.Clear();
			mMapName.text = mMapList[mMapIndex].MapName;
			mGameModePop.items.AddRange(mMapList[mMapIndex].GameMode);
			mGameTypePop.items.AddRange(mMapList[mMapIndex].GameType);
			mBiomePop.items.AddRange(mMapList[mMapIndex].MapTerrainType);
			mWeatherPop.items.AddRange(mMapList[mMapIndex].MapWeatherType);
			mMonsterYes = false;
			mMonsterText.text = "YES";
		}
	}

	private void OnMapSelectRight()
	{
		if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
		{
			mMapIndex++;
			if (mMapIndex >= mMapList.Count)
			{
				mMapIndex = 0;
			}
			mGameModePop.items.Clear();
			mGameTypePop.items.Clear();
			mBiomePop.items.Clear();
			mWeatherPop.items.Clear();
			mMapName.text = mMapList[mMapIndex].MapName;
			mGameModePop.items.AddRange(mMapList[mMapIndex].GameMode);
			mGameTypePop.items.AddRange(mMapList[mMapIndex].GameType);
			mBiomePop.items.AddRange(mMapList[mMapIndex].MapTerrainType);
			mWeatherPop.items.AddRange(mMapList[mMapIndex].MapWeatherType);
			mMonsterYes = false;
			mMonsterText.text = "YES";
		}
	}

	private void OnMonsterSelectLeft()
	{
		if (!(mGameModeText == "Story") && Input.GetMouseButtonUp(0))
		{
			if (mMonsterYes)
			{
				mMonsterYes = false;
				mMonsterText.text = "NO";
			}
			else
			{
				mMonsterYes = true;
				mMonsterText.text = "YES";
			}
		}
	}

	private void OnPlayerNumSelectLeft()
	{
		if (!Input.GetMouseButtonUp(0) || mMapList.Count <= 0)
		{
			return;
		}
		if (!PeGameMgr.IsSurvive)
		{
			if (teamNum * (playerNum / 2) >= 1)
			{
				playerNum /= 2;
				mPlayerNum.text = playerNum.ToString();
			}
		}
		else
		{
			playerNum = Mathf.Max(1, playerNum / 2);
			mPlayerNum.text = playerNum.ToString();
		}
	}

	private void OnPlayerNumSelectRight()
	{
		if (!Input.GetMouseButtonUp(0) || mMapList.Count <= 0)
		{
			return;
		}
		if (!PeGameMgr.IsSurvive)
		{
			if (teamNum * (playerNum * 2) <= 32)
			{
				playerNum *= 2;
				mPlayerNum.text = playerNum.ToString();
			}
		}
		else
		{
			playerNum = Mathf.Min(32, playerNum * 2);
			mPlayerNum.text = playerNum.ToString();
		}
	}

	private void OnTeamNumSelectLeft()
	{
		if (Input.GetMouseButtonUp(0) && mMapList.Count > 0 && PeGameMgr.IsVS && teamNum >= 3)
		{
			teamNum--;
			playerNum = 4;
			mTeamNum.text = teamNum.ToString();
			mPlayerNum.text = playerNum.ToString();
		}
	}

	private void OnTeamNumSelectRight()
	{
		if (Input.GetMouseButtonUp(0) && mMapList.Count > 0 && PeGameMgr.IsVS && teamNum <= 3)
		{
			teamNum++;
			playerNum = 4;
			mTeamNum.text = teamNum.ToString();
			mPlayerNum.text = playerNum.ToString();
		}
	}

	private void OnDropRateNumSelectLeft()
	{
		if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
		{
			if (--mDropRateIndex < 0)
			{
				mDropRateIndex = mDropRateSelections.Count - 1;
			}
			mDropRateNum.text = mDropRateSelections[mDropRateIndex] + "%";
		}
	}

	private void OnDropRateNumSelectRight()
	{
		if (Input.GetMouseButtonUp(0) && mMapList.Count > 0)
		{
			if (++mDropRateIndex > mDropRateSelections.Count - 1)
			{
				mDropRateIndex = 0;
			}
			mDropRateNum.text = mDropRateSelections[mDropRateIndex] + "%";
		}
	}

	private void OnPrivateActivate()
	{
		if (mPrivateServer.isChecked && mProxyServer.isChecked)
		{
			mProxyServer.isChecked = false;
		}
	}

	private void OnProxyActivate()
	{
		if (mProxyServer.isChecked && mPrivateServer.isChecked)
		{
			mPrivateServer.isChecked = false;
		}
	}

	private void Update()
	{
		UpdateScrollValueChage();
	}

	private void UpdateScrollValueChage()
	{
		float scrollValue = mSbTerrainHeight.scrollValue;
		if (scrollValue >= 0f && (double)scrollValue < 0.33)
		{
			mSbTerrainHeight.scrollValue = 0f;
			mTerrainHeight = 128;
			mLbTerrainHeightInfo.text = "128m";
		}
		else if ((double)scrollValue >= 0.33 && (double)scrollValue < 0.66)
		{
			mSbTerrainHeight.scrollValue = 0.5f;
			mTerrainHeight = 256;
			mLbTerrainHeightInfo.text = "256m";
		}
		else if ((double)scrollValue >= 0.66 && scrollValue <= 1f)
		{
			mSbTerrainHeight.scrollValue = 1f;
			mTerrainHeight = 512;
			mLbTerrainHeightInfo.text = "512m";
		}
		scrollValue = mSbMapSzie.scrollValue;
		if (scrollValue >= 0f && (double)scrollValue < 0.125)
		{
			mSbMapSzie.scrollValue = 0f;
			mMapSize = 4;
			mLbMapSizeInfo.text = "2km * 2km";
			UpdateHostilityAllyCountRangByMapSize(2);
		}
		else if ((double)scrollValue >= 0.125 && (double)scrollValue < 0.375)
		{
			mSbMapSzie.scrollValue = 0.25f;
			mMapSize = 3;
			mLbMapSizeInfo.text = "4km * 4km";
			UpdateHostilityAllyCountRangByMapSize(4);
		}
		else if ((double)scrollValue >= 0.375 && (double)scrollValue < 0.625)
		{
			mSbMapSzie.scrollValue = 0.5f;
			mMapSize = 2;
			mLbMapSizeInfo.text = "8km * 8km";
			UpdateHostilityAllyCountRangByMapSize(8);
		}
		else if ((double)scrollValue >= 0.625 && (double)scrollValue < 0.875)
		{
			mSbMapSzie.scrollValue = 0.75f;
			mMapSize = 1;
			mLbMapSizeInfo.text = "20km * 20km";
			UpdateHostilityAllyCountRangByMapSize(20);
		}
		else if ((double)scrollValue >= 0.875 && scrollValue <= 1f)
		{
			mSbMapSzie.scrollValue = 1f;
			mMapSize = 0;
			mLbMapSizeInfo.text = "40km * 40km";
			UpdateHostilityAllyCountRangByMapSize(40);
		}
		int num = Convert.ToInt32(mSbRiverDensity.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbRiverDensityInfo.text = num.ToString();
		mRiverDensity = num;
		num = Convert.ToInt32(mSbRiverWidth.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbRiverWidthInfo.text = num.ToString();
		mRiverWidth = num;
		num = Convert.ToInt32(mSbPlainHeight.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbPlainHeight.text = num.ToString();
		plainHeight = num;
		num = Convert.ToInt32(mSbFlatness.scrollValue * 100f);
		if (num < 1)
		{
			num = 1;
		}
		mLbFlatness.text = num.ToString();
		flatness = num;
		num = Convert.ToInt32(mSbBridgeMaxHeight.scrollValue * 100f);
		mLbBridgeMaxHeight.text = num.ToString();
		bridgeMaxHeight = num;
	}

	public void OnCustomWndOpen()
	{
		UIAlphaGroup[] array = mAlphaGroupsForCustom;
		foreach (UIAlphaGroup uIAlphaGroup in array)
		{
			uIAlphaGroup.State = 1;
		}
		BoxCollider[] array2 = mCollidersForCustom;
		foreach (BoxCollider boxCollider in array2)
		{
			boxCollider.enabled = false;
		}
		if (UseAnimation)
		{
			HorizontalMove(_direction: true);
		}
	}

	public void OnCustomWndClose()
	{
		UIAlphaGroup[] array = mAlphaGroupsForCustom;
		foreach (UIAlphaGroup uIAlphaGroup in array)
		{
			uIAlphaGroup.State = 0;
		}
		BoxCollider[] array2 = mCollidersForCustom;
		foreach (BoxCollider boxCollider in array2)
		{
			boxCollider.enabled = true;
		}
		if (UseAnimation)
		{
			HorizontalMove(_direction: false);
		}
	}

	private void OnStoryMode()
	{
		UIAlphaGroup[] array = mAlphaGroupsForStory;
		foreach (UIAlphaGroup uIAlphaGroup in array)
		{
			uIAlphaGroup.State = 1;
		}
		BoxCollider[] array2 = mCollidersForStory;
		foreach (BoxCollider boxCollider in array2)
		{
			boxCollider.enabled = false;
		}
	}

	private void OnExitStoryMode()
	{
		UIAlphaGroup[] array = mAlphaGroupsForStory;
		foreach (UIAlphaGroup uIAlphaGroup in array)
		{
			uIAlphaGroup.State = 0;
		}
		BoxCollider[] array2 = mCollidersForStory;
		foreach (BoxCollider boxCollider in array2)
		{
			boxCollider.enabled = true;
		}
	}

	private IEnumerator WhetherMapCheckHasFinished()
	{
		do
		{
			yield return 0;
			if (mMultiCustomCtrl.Integrity == true)
			{
				CreateCustomServer();
				break;
			}
		}
		while (mMultiCustomCtrl.Integrity != false);
	}

	private void HorizontalMove(bool _direction)
	{
		TweenPosition[] array = mTPs;
		foreach (TweenPosition tweenPosition in array)
		{
			tweenPosition.Play(_direction);
		}
	}

	private void CreateCustomServer()
	{
		PeSingleton<CustomGameData.Mgr>.Instance.curGameData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(UID);
		if (PeSingleton<CustomGameData.Mgr>.Instance.curGameData != null)
		{
			string text = mServerName.text;
			if (!LoadServer.Exist(text))
			{
				PeGameMgr.gameName = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.name;
				PeGameMgr.monsterYes = mMonsterYes;
				PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
				MyServer myServer = new MyServer();
				myServer.gameName = text;
				myServer.gamePassword = mPassword.text;
				myServer.gameMode = (int)PeGameMgr.sceneMode;
				myServer.masterRoleName = GameClientLobby.role.name;
				myServer.mapName = PeGameMgr.gameName;
				myServer.seedStr = mSeed.text;
				myServer.uid = UID;
				myServer.unlimitedRes = mInfiniteResoureces.isChecked;
				myServer.proxyServer = mProxyServer.isChecked;
				myServer.isPrivate = mPrivateServer.isChecked;
				myServer.useSkillTree = (RandomMapConfig.useSkillTree = mSkillTreeSystem.isChecked);
				myServer.scriptsAvailable = mAllScriptsAvailable.isChecked;
				MyServerManager.StartCustomServer(myServer);
			}
		}
	}

	private void UpdateHostilityAllyCountRangByMapSize(int mapSize)
	{
		mSbHostilityAllyCount.enabled = true;
		float scrollValue = mSbHostilityAllyCount.scrollValue;
		switch (mapSize)
		{
		case 2:
			mHostilityAllyCount = 3;
			mSbHostilityAllyCount.scrollValue = 1f;
			break;
		case 4:
			if (scrollValue <= 0.1f)
			{
				mHostilityAllyCount = 3;
				mSbHostilityAllyCount.scrollValue = 0f;
			}
			else if (scrollValue <= 0.51f)
			{
				mHostilityAllyCount = 4;
				mSbHostilityAllyCount.scrollValue = 0.5f;
			}
			else
			{
				mHostilityAllyCount = 5;
				mSbHostilityAllyCount.scrollValue = 1f;
			}
			break;
		case 8:
			if (scrollValue <= 0.1f)
			{
				mHostilityAllyCount = 3;
				mSbHostilityAllyCount.scrollValue = 0f;
			}
			else if (scrollValue <= 0.34f)
			{
				mHostilityAllyCount = 4;
				mSbHostilityAllyCount.scrollValue = 0.33f;
			}
			else if (scrollValue <= 0.67f)
			{
				mHostilityAllyCount = 5;
				mSbHostilityAllyCount.scrollValue = 0.66f;
			}
			else
			{
				mHostilityAllyCount = 6;
				mSbHostilityAllyCount.scrollValue = 1f;
			}
			break;
		case 20:
		case 40:
			if (scrollValue <= 0.1f)
			{
				mHostilityAllyCount = 3;
				mSbHostilityAllyCount.scrollValue = 0f;
			}
			else if (scrollValue <= 0.26f)
			{
				mHostilityAllyCount = 4;
				mSbHostilityAllyCount.scrollValue = 0.25f;
			}
			else if (scrollValue <= 0.51f)
			{
				mHostilityAllyCount = 5;
				mSbHostilityAllyCount.scrollValue = 0.5f;
			}
			else if (scrollValue <= 0.76f)
			{
				mHostilityAllyCount = 6;
				mSbHostilityAllyCount.scrollValue = 0.75f;
			}
			else
			{
				mHostilityAllyCount = 7;
				mSbHostilityAllyCount.scrollValue = 1f;
			}
			break;
		}
		mLbHostilityAllyCount.text = mHostilityAllyCount.ToString();
	}
}
