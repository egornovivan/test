using System;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class SingleGameBuild : SingleGameOfficial
{
	protected override void Load()
	{
		LoadBuild(base.newGame);
	}

	private static void LoadBuild(bool bNewGame)
	{
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] " + (bNewGame ? "new" : "saved") + " Build****************");
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadReputation(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new LoadRandomTerrainParam(bNewGame));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
		PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
		PeLauncher.Instance.Add(new LoadBuildPlayerSpawnPos(bNewGame));
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadRandomTerrainWithoutTown(bNewGame));
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
		PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
		PeLauncher.Instance.Add(new LoadMusicBuild());
		PeLauncher.Instance.Add(new LoadRandomMap(bNewGame));
		PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
		PeLauncher.Instance.Add(new LoadSingleBuildInitData(bNewGame));
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PeLauncher.Instance.Add(new LoadForceSetting(bNewGame));
		PlayerPackageCmpt.LockStackCount = true;
	}
}
