using System;
using PeCustom;
using PETools;
using UnityEngine;

namespace Pathea;

public static class PeGameMgr
{
	public enum EPlayerType
	{
		Single,
		Multiple,
		Creation,
		Tutorial,
		Max
	}

	public enum ESceneMode
	{
		Story,
		Adventure,
		Build,
		TowerDefense,
		Custom,
		Tutorial,
		Max
	}

	public enum EGameType
	{
		Cooperation,
		VS,
		Survive,
		Max
	}

	public enum ETutorialMode
	{
		GatherCut,
		Replicate,
		DigBuild,
		Max
	}

	public enum EGameLevel
	{
		Easy,
		Normal
	}

	public static GamePasue PasueEvent;

	private static EPlayerType mPlayerType;

	private static ESceneMode mSceneMode;

	private static EGameType mGameType;

	private static ETutorialMode mTutorialMode = ETutorialMode.Max;

	private static ArchiveMgr.ESave mLoadArchive = ArchiveMgr.ESave.MaxUser;

	private static bool mUnlimitedRes;

	private static bool mMonsterYes = true;

	private static bool mPause;

	private static EGameLevel mGameLevel = EGameLevel.Normal;

	private static YirdData mCustomGame;

	private static int cullMask;

	public static EPlayerType playerType
	{
		get
		{
			return mPlayerType;
		}
		set
		{
			mPlayerType = value;
		}
	}

	public static ESceneMode sceneMode
	{
		get
		{
			return mSceneMode;
		}
		set
		{
			mSceneMode = value;
		}
	}

	public static EGameLevel gameLevel
	{
		get
		{
			return mGameLevel;
		}
		set
		{
			mGameLevel = value;
		}
	}

	public static EGameType gameType
	{
		get
		{
			return mGameType;
		}
		set
		{
			mGameType = value;
		}
	}

	public static ETutorialMode tutorialMode
	{
		get
		{
			return mTutorialMode;
		}
		set
		{
			mTutorialMode = value;
		}
	}

	public static ArchiveMgr.ESave loadArchive
	{
		get
		{
			return mLoadArchive;
		}
		set
		{
			mLoadArchive = value;
		}
	}

	public static bool unlimitedRes
	{
		get
		{
			return mUnlimitedRes;
		}
		set
		{
			mUnlimitedRes = value;
		}
	}

	public static bool monsterYes
	{
		get
		{
			return mMonsterYes;
		}
		set
		{
			mMonsterYes = value;
		}
	}

	public static bool gamePause
	{
		get
		{
			return mPause;
		}
		set
		{
			if (mPause != value)
			{
				mPause = value;
				if (!IsMulti)
				{
					Time.timeScale = ((!mPause) ? 1f : 0.0001f);
				}
				if (PasueEvent != null)
				{
					PasueEvent(mPause);
				}
			}
		}
	}

	public static bool randomMap
	{
		get
		{
			if (playerType == EPlayerType.Multiple && sceneMode != 0 && sceneMode != ESceneMode.Custom)
			{
				return true;
			}
			if (playerType == EPlayerType.Single && (sceneMode == ESceneMode.Adventure || sceneMode == ESceneMode.Build) && yirdName != AdventureScene.Dungen.ToString())
			{
				return true;
			}
			return false;
		}
	}

	public static string gameName { get; set; }

	public static string targetYird { get; set; }

	public static string yirdName { get; set; }

	public static string mapUID { get; set; }

	public static bool IsSingle => mPlayerType == EPlayerType.Single;

	public static bool IsMulti => mPlayerType == EPlayerType.Multiple;

	public static bool IsStory => mSceneMode == ESceneMode.Story || mSceneMode == ESceneMode.Tutorial;

	public static bool IsAdventure => mSceneMode == ESceneMode.Adventure;

	public static bool IsTowerDefense => mSceneMode == ESceneMode.TowerDefense;

	public static bool IsBuild => mSceneMode == ESceneMode.Build;

	public static bool IsTutorial => mSceneMode == ESceneMode.Tutorial;

	public static bool IsCustom => mSceneMode == ESceneMode.Custom;

	public static bool IsCooperation => mGameType == EGameType.Cooperation;

	public static bool IsSurvive => mGameType == EGameType.Survive;

	public static bool IsVS => mGameType == EGameType.VS;

	public static bool IsSingleStory => IsSingle && IsStory;

	public static bool IsSingleAdventure => IsSingle && IsAdventure;

	public static bool IsSingleBuild => IsSingle && IsBuild;

	public static bool IsMultiAdventure => IsMulti && IsAdventure;

	public static bool IsMultiStory => IsMulti && IsStory;

	public static bool IsMultiCustom => IsMulti && IsCustom;

	public static bool IsMultiBuild => IsMulti && IsBuild;

	public static bool IsMultiTowerDefense => IsMulti && IsTowerDefense;

	public static bool IsMultiCoop => IsMulti && IsCooperation;

	public static bool IsMultiVS => IsMulti && IsVS;

	public static bool IsMultiSurvive => IsMulti && IsSurvive;

	public static bool IsMultiAdventureCoop => IsMulti && IsAdventure && IsCooperation;

	public static bool IsMultiAdventureVS => IsMulti && IsAdventure && IsVS;

	public static bool IsMultiAdventureSurvive => IsMulti && IsAdventure && IsSurvive;

	public static bool IsMultiBuildCoop => IsMulti && IsBuild && IsCooperation;

	public static bool IsMultiBuildVS => IsMulti && IsBuild && IsMultiVS;

	public static bool IsMultiBuildSurvive => IsMulti && IsBuild && IsSurvive;

	private static void BeforeLoad()
	{
		cullMask = Camera.main.cullingMask;
		Camera.main.cullingMask = 0;
		PeInput.enable = false;
	}

	private static void PostLoad()
	{
		Resources.UnloadUnusedAssets();
		ApplySystemSetting();
		PeSingletonRunner.Launch();
		if (mPlayerType == EPlayerType.Single)
		{
			AutoArchiveRunner.Init();
			if (yirdName != AdventureScene.Dungen.ToString())
			{
				AutoArchiveRunner.Start();
			}
		}
		Camera.main.cullingMask = cullMask;
		PeInput.enable = true;
		targetYird = string.Empty;
	}

	private static void ApplySystemSetting()
	{
		SystemSettingData.Instance.ApplySettings();
	}

	private static PlayerTypeLoader CreatePlayerTypeLoader()
	{
		if (mPlayerType == EPlayerType.Creation)
		{
			return new CreationPlayerTypeLoader();
		}
		if (mPlayerType == EPlayerType.Multiple)
		{
			if (PeSingleton<MultiPlayerTypeArchiveMgr>.Instance.multiScenario == null)
			{
				PeSingleton<MultiPlayerTypeArchiveMgr>.Instance.New();
			}
			MultiPlayerTypeLoader multiScenario = PeSingleton<MultiPlayerTypeArchiveMgr>.Instance.multiScenario;
			multiScenario.New(sceneMode, gameName);
			return multiScenario;
		}
		if (mPlayerType == EPlayerType.Single)
		{
			PeSingleton<ArchiveMgr>.Instance.LoadAndCleanSwap(loadArchive);
			if (loadArchive == ArchiveMgr.ESave.MaxUser)
			{
				PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.New();
				SinglePlayerTypeLoader singleScenario = PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.singleScenario;
				singleScenario.New(sceneMode, mapUID, gameName);
			}
			else
			{
				PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.Restore();
				SinglePlayerTypeLoader singleScenario2 = PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.singleScenario;
				if (!string.IsNullOrEmpty(targetYird))
				{
					singleScenario2.SetYirdName(targetYird);
				}
			}
			return PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.singleScenario;
		}
		if (mPlayerType == EPlayerType.Tutorial)
		{
			TutorialPlayerTypeLoader tutorialPlayerTypeLoader = new TutorialPlayerTypeLoader();
			tutorialPlayerTypeLoader.tutorialMode = tutorialMode;
			return tutorialPlayerTypeLoader;
		}
		return null;
	}

	public static void Run()
	{
		BeforeLoad();
		PeLauncher.Instance.endLaunch = delegate
		{
			if (IsMulti && !NetworkInterface.IsClient)
			{
				return true;
			}
			if (!VFVoxelTerrain.TerrainColliderComplete)
			{
				return false;
			}
			SceneMan.self.StartWork();
			PeEntity entity = PeSingleton<MainPlayer>.Instance.entity;
			if (null == entity)
			{
				return false;
			}
			MotionMgrCmpt cmpt = entity.GetCmpt<MotionMgrCmpt>();
			if (cmpt == null)
			{
				return false;
			}
			if (cmpt.freezePhyStateForSystem)
			{
				Vector3 safePos;
				if (IsMulti)
				{
					if (PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == 0)
					{
						if (PE.FindHumanSafePos(entity.position, out safePos))
						{
							entity.position = safePos;
						}
						else
						{
							entity.position += 10f * Vector3.up;
						}
					}
				}
				else if (PE.FindHumanSafePos(entity.position, out safePos))
				{
					entity.position = safePos;
				}
				else
				{
					entity.position += 10f * Vector3.up;
				}
				return false;
			}
			if (mSceneMode == ESceneMode.Custom)
			{
				PeCustomScene.Self.Notify(ESceneNoification.SceneBegin);
			}
			PostLoad();
			GC.Collect();
			return true;
		};
		PlayerTypeLoader playerTypeLoader = CreatePlayerTypeLoader();
		if (playerTypeLoader != null)
		{
			playerTypeLoader.Load();
			if (playerTypeLoader is SinglePlayerTypeLoader singlePlayerTypeLoader)
			{
				sceneMode = singlePlayerTypeLoader.sceneMode;
				yirdName = singlePlayerTypeLoader.yirdName;
				gameName = singlePlayerTypeLoader.gameName;
			}
			else if (playerTypeLoader is MultiPlayerTypeLoader multiPlayerTypeLoader)
			{
				sceneMode = multiPlayerTypeLoader.sceneMode;
				yirdName = multiPlayerTypeLoader.yirdName;
				gameName = multiPlayerTypeLoader.gameName;
			}
			VFVoxelTerrain.RandomMap = randomMap;
			PeLauncher.Instance.StartLoad();
		}
	}
}
