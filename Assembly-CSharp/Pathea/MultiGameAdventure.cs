using System;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class MultiGameAdventure : MultiGameOfficial
{
	protected override void Load()
	{
		LoadAdventure();
	}

	private static void LoadAdventure(bool bNewGame = true)
	{
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] multi adventure client");
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadRandomTown());
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new LoadMultiPlayerSpawnPos());
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadRandomTerrainWithTown(bNew: true));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadWaveSystem());
		PeLauncher.Instance.Add(new LoadGrassRandom(bNew: true));
		PeLauncher.Instance.Add(new LoadVETreeProtos(bNew: true));
		PeLauncher.Instance.Add(new LoadRandomTree(bNew: true));
		PeLauncher.Instance.Add(new LoadEnvironment());
		PeLauncher.Instance.Add(new LoadRailway(bNew: true));
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadCSData(bNew: true));
		PeLauncher.Instance.Add(new LoadFarm());
		PeLauncher.Instance.Add(new LoadColony());
		PeLauncher.Instance.Add(new LoadEntityCreator(bNew: true));
		PeLauncher.Instance.Add(new LoadUiHelp(bNew: true));
		PeLauncher.Instance.Add(new LoadItemBox(bNew: true));
		PeLauncher.Instance.Add(new LoadRandomMap(bNew: true));
		PeLauncher.Instance.Add(new InitBuildManager(newGame: true));
		PeLauncher.Instance.Add(new LoadMultiStory());
		PeLauncher.Instance.Add(new LoadMultiCreature());
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PeLauncher.Instance.Add(new LoadRandomDungeon());
		PeLauncher.Instance.Add(new LoadMusicAdventure());
		PlayerPackageCmpt.LockStackCount = false;
	}
}
