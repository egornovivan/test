using System;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class SingleGameStory : SingleGameOfficial
{
	public enum StoryScene
	{
		MainLand,
		L1Ship,
		DienShip0,
		TrainingShip,
		PajaShip,
		LaunchCenter,
		DienShip1,
		DienShip2,
		DienShip3,
		DienShip4,
		DienShip5
	}

	public static StoryScene curType;

	protected override void Load()
	{
		if (base.yirdName == "main")
		{
			LoadStory(base.newGame);
			curType = StoryScene.MainLand;
		}
		else if (base.yirdName == "DienShip0")
		{
			LoadYird(base.newGame, StoryScene.DienShip0);
			curType = StoryScene.DienShip0;
		}
		else if (base.yirdName == "DienShip1")
		{
			LoadYird(base.newGame, StoryScene.DienShip1);
			curType = StoryScene.DienShip1;
		}
		else if (base.yirdName == "DienShip2")
		{
			LoadYird(base.newGame, StoryScene.DienShip2);
			curType = StoryScene.DienShip2;
		}
		else if (base.yirdName == "DienShip3")
		{
			LoadYird(base.newGame, StoryScene.DienShip3);
			curType = StoryScene.DienShip3;
		}
		else if (base.yirdName == "DienShip4")
		{
			LoadYird(base.newGame, StoryScene.DienShip4);
			curType = StoryScene.DienShip4;
		}
		else if (base.yirdName == "DienShip5")
		{
			LoadYird(base.newGame, StoryScene.DienShip5);
			curType = StoryScene.DienShip5;
		}
		else if (base.yirdName == "L1Ship")
		{
			LoadYird(base.newGame, StoryScene.L1Ship);
			curType = StoryScene.L1Ship;
		}
		else if (base.yirdName == "PajaShip")
		{
			LoadYird(base.newGame, StoryScene.PajaShip);
			curType = StoryScene.PajaShip;
		}
		else if (base.yirdName == "LaunchCenter")
		{
			LoadYird(base.newGame, StoryScene.LaunchCenter);
			curType = StoryScene.LaunchCenter;
		}
	}

	private void LoadYird(bool bNewGame, StoryScene type = StoryScene.MainLand)
	{
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] " + (bNewGame ? "new" : "saved") + " story " + type);
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadReputation(bNewGame));
		PeLauncher.Instance.Add(new LoadSpecialScene(type));
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, string.Empty));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
		PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
		PeLauncher.Instance.Add(new LoadWaveSystem());
		if (type == StoryScene.PajaShip || type == StoryScene.LaunchCenter)
		{
			PeLauncher.Instance.Add(new LoadEnvironment());
		}
		PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
		PeLauncher.Instance.Add(new LoadGameGraphInfo(new Vector2(18432f, 18432f)));
		PeLauncher.Instance.Add(new LoadStoryMap(bNewGame));
		PeLauncher.Instance.Add(new LoadCreature(bNewGame));
		PeLauncher.Instance.Add(new LoadInGameAid(bNewGame));
		PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadCSData(bNewGame));
		PeLauncher.Instance.Add(new LoadFarm());
		PeLauncher.Instance.Add(new LoadColony());
		PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));
		PeLauncher.Instance.Add(new LoadItemBox(bNewGame));
		PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
		PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
		PeLauncher.Instance.Add(new LoadStory(bNewGame));
		PeLauncher.Instance.Add(new LoadMusicStroy());
		PeLauncher.Instance.Add(new LoadSingleStoryInitData(bNewGame));
		PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
		PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));
		PeLauncher.Instance.Add(new LoadDoodadShow());
		PlayerPackageCmpt.LockStackCount = false;
	}

	private static void LoadStory(bool bNewGame)
	{
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] " + (bNewGame ? "new" : "saved") + " story****************");
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadReputation(bNewGame));
		PeLauncher.Instance.Add(new LoadStoryPlayerSpawnPos(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new LoadCamera());
		string path = GameConfig.PEDataPath + "VoxelData/zData/";
		PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, path));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
		PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
		PeLauncher.Instance.Add(new LoadWaveSystem());
		string dataDir = GameConfig.PEDataPath + "VoxelData/SubTerrains";
		PeLauncher.Instance.Add(new LoadEditedGrass(bNewGame, dataDir));
		PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
		string dir = GameConfig.PEDataPath + "VoxelData/SubTerrains/";
		PeLauncher.Instance.Add(new LoadEditedTree(bNewGame, dir));
		PeLauncher.Instance.Add(new LoadEnvironment());
		PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
		PeLauncher.Instance.Add(new LoadGameGraphInfo(new Vector2(18432f, 18432f)));
		PeLauncher.Instance.Add(new LoadStoryMap(bNewGame));
		PeLauncher.Instance.Add(new LoadRailway(bNewGame));
		PeLauncher.Instance.Add(new LoadCreature(bNewGame));
		PeLauncher.Instance.Add(new LoadInGameAid(bNewGame));
		PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadCSData(bNewGame));
		PeLauncher.Instance.Add(new LoadFarm());
		PeLauncher.Instance.Add(new LoadColony());
		PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));
		PeLauncher.Instance.Add(new LoadItemBox(bNewGame));
		PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
		PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
		PeLauncher.Instance.Add(new LoadStory(bNewGame));
		PeLauncher.Instance.Add(new LoadMusicStroy());
		PeLauncher.Instance.Add(new LoadSingleStoryInitData(bNewGame));
		PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PeLauncher.Instance.Add(new LoadMonsterSiegeCamp());
		PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));
		PeLauncher.Instance.Add(new LoadMountsMonsterData(bNewGame));
		PlayerPackageCmpt.LockStackCount = false;
	}
}
