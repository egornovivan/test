using System;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class MultiGameStory : MultiGameOfficial
{
	public static SingleGameStory.StoryScene curType;

	protected override void Load()
	{
		if (base.yirdName == "main")
		{
			LoadStory();
			curType = SingleGameStory.StoryScene.MainLand;
		}
		else if (base.yirdName == "DienShip0")
		{
			LoadYird(bNewGame: true, SingleGameStory.StoryScene.DienShip0);
			curType = SingleGameStory.StoryScene.DienShip0;
		}
		else if (base.yirdName == "L1Ship")
		{
			LoadYird(bNewGame: true, SingleGameStory.StoryScene.L1Ship);
			curType = SingleGameStory.StoryScene.L1Ship;
		}
	}

	private void LoadYird(bool bNewGame = true, SingleGameStory.StoryScene type = SingleGameStory.StoryScene.MainLand)
	{
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadSpecialScene(type));
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, string.Empty));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
		PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
		PeLauncher.Instance.Add(new LoadWaveSystem());
		PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
		PeLauncher.Instance.Add(new LoadGameGraphInfo(new Vector2(18432f, 18432f)));
		PeLauncher.Instance.Add(new LoadStoryMap(bNewGame));
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
		PeLauncher.Instance.Add(new LoadMultiCreature());
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PlayerPackageCmpt.LockStackCount = false;
	}

	private static void LoadStory(bool bNewGame = true)
	{
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] multi story client");
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new ResetGlobalData());
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
		PeLauncher.Instance.Add(new LoadMultiCreature());
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PlayerPackageCmpt.LockStackCount = false;
	}
}
