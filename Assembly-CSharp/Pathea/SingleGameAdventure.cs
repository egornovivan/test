using System;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class SingleGameAdventure : SingleGameOfficial
{
	public static AdventureScene curType;

	protected override string GetDefaultYirdName()
	{
		return AdventureScene.MainAdventure.ToString();
	}

	protected override void Load()
	{
		if (base.yirdName == AdventureScene.MainAdventure.ToString())
		{
			LoadAdventure(base.newGame);
			curType = AdventureScene.MainAdventure;
		}
		else if (base.yirdName == AdventureScene.Dungen.ToString())
		{
			LoadYird(base.newGame, AdventureScene.Dungen);
			curType = AdventureScene.Dungen;
		}
	}

	private void LoadYird(bool bNewGame, AdventureScene type = AdventureScene.MainAdventure)
	{
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] " + (bNewGame ? "new" : "saved") + " adventure " + type);
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadReputation(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomTerrainParam(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomTown());
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadRandomTerrainWithTown(bNewGame));
		PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
		PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
		PeLauncher.Instance.Add(new LoadEnvironment());
		PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
		PeLauncher.Instance.Add(new LoadCreature(bNewGame));
		PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadCSData(bNewGame));
		PeLauncher.Instance.Add(new LoadFarm());
		PeLauncher.Instance.Add(new LoadColony());
		PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));
		PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomMap(bNewGame));
		PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomStory(bNewGame));
		PeLauncher.Instance.Add(new LoadMusicAdventure());
		PeLauncher.Instance.Add(new LoadSingleAdventureInitData(bNewGame));
		PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PeLauncher.Instance.Add(new LoadRandomDungeon());
		PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));
		PlayerPackageCmpt.LockStackCount = false;
	}

	private static void LoadAdventure(bool bNewGame)
	{
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] " + (bNewGame ? "new" : "saved") + " adventure****************");
		PeLauncher.Instance.Add(new RandomMission(bNewGame));
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadReputation(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new LoadRandomTerrainParam(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomTown());
		PeLauncher.Instance.Add(new LoadAdventurePlayerSpawnPos(bNewGame));
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadRandomTerrainWithTown(bNewGame));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
		PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
		PeLauncher.Instance.Add(new LoadWaveSystem());
		PeLauncher.Instance.Add(new LoadGrassRandom(bNewGame));
		PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomTree(bNewGame));
		PeLauncher.Instance.Add(new LoadEnvironment());
		PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
		PeLauncher.Instance.Add(new LoadRailway(bNewGame));
		PeLauncher.Instance.Add(new LoadCreature(bNewGame));
		PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadCSData(bNewGame));
		PeLauncher.Instance.Add(new LoadFarm());
		PeLauncher.Instance.Add(new LoadColony());
		PeLauncher.Instance.Add(new LoadEntityCreator(bNewGame));
		PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomMap(bNewGame));
		PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomStory(bNewGame));
		PeLauncher.Instance.Add(new LoadMusicAdventure());
		PeLauncher.Instance.Add(new LoadSingleAdventureInitData(bNewGame));
		PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PeLauncher.Instance.Add(new LoadSingleGameLevel(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomDungeon());
		PeLauncher.Instance.Add(new LoadMountsMonsterData(bNewGame));
		PlayerPackageCmpt.LockStackCount = false;
	}
}
