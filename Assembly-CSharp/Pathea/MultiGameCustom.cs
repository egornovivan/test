using System;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class MultiGameCustom : MultiGameOfficial
{
	protected override void Load()
	{
		LoadCustom();
	}

	private static void LoadCustom(bool bNewGame = true)
	{
		CustomGameData customData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(PeGameMgr.mapUID);
		if (customData != null)
		{
			PeSingleton<CustomGameData.Mgr>.Instance.curGameData = customData;
		}
		YirdData curYirdData = customData.curYirdData;
		if (curYirdData == null)
		{
			Debug.LogError("custom game data is null");
			return;
		}
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] multi custom client");
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadMultiPlayerSpawnPos());
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, curYirdData.terrainPath));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadWaveSystem());
		PeLauncher.Instance.Add(new LoadEditedGrass(bNewGame, curYirdData.grassPath));
		PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
		PeLauncher.Instance.Add(new LoadEditedTree(bNewGame, curYirdData.treePath));
		PeLauncher.Instance.Add(new LoadEnvironment());
		PeLauncher.Instance.Add(new LoadFarm());
		PeLauncher.Instance.Add(new LoadColony());
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
		PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
		PeLauncher.Instance.Add(new LoadMultiCustomCreature());
		PeLauncher.Instance.Add(new LoadCustomEntityCreator(bNewGame, curYirdData));
		PeLauncher.Instance.Add(new LoadMultiCustom(bNewGame, customData));
		PlayerPackageCmpt.LockStackCount = false;
	}
}
