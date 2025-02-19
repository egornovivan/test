using System;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class SingleGameCustom : SingleGame
{
	private string mUID;

	public SingleGameCustom(string UID, string gameName)
	{
		mUID = UID;
	}

	protected override string GetDefaultYirdName()
	{
		return PeSingleton<CustomGameData.Mgr>.Instance.GetYirdData(mUID)?.name;
	}

	protected override void Load()
	{
		ScenarioMapDesc mapByUID = ScenarioMapUtils.GetMapByUID(mUID, GameConfig.CustomDataDir);
		CustomGameData customData = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomData(mUID, mapByUID.Path);
		if (customData != null)
		{
			PeSingleton<CustomGameData.Mgr>.Instance.curGameData = customData;
			LoadCustom(base.newGame, customData);
		}
		else
		{
			Debug.LogError("Error");
		}
	}

	private static void LoadCustom(bool bNewGame, CustomGameData customData)
	{
		YirdData curYirdData = customData.curYirdData;
		if (curYirdData == null)
		{
			Debug.LogError("custom game data is null");
			return;
		}
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] " + (bNewGame ? "new" : "saved") + " Custom, path:" + curYirdData.terrainPath + "****************");
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadReputation(bNewGame));
		PeLauncher.Instance.Add(new LoadRandomItemMgr());
		PeLauncher.Instance.Add(new LoadCustomPlayerSpawnPos(bNewGame, customData.curPlayer.StartLocation));
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadEditedTerrain(bNewGame, curYirdData.terrainPath));
		PeLauncher.Instance.Add(new LoadPathFinding());
		PeLauncher.Instance.Add(new LoadPathFindingEx());
		PeLauncher.Instance.Add(new LoadCreationData(bNewGame));
		PeLauncher.Instance.Add(new LoadItemAsset(bNewGame));
		PeLauncher.Instance.Add(new LoadWaveSystem());
		PeLauncher.Instance.Add(new LoadEditedGrass(bNewGame, curYirdData.grassPath));
		PeLauncher.Instance.Add(new LoadVETreeProtos(bNewGame));
		PeLauncher.Instance.Add(new LoadEditedTree(bNewGame, curYirdData.treePath));
		PeLauncher.Instance.Add(new LoadEnvironment());
		PeLauncher.Instance.Add(new LoadWorldInfo(bNewGame));
		PeLauncher.Instance.Add(new LoadCustomDoodad(bNewGame, curYirdData.GetDoodads()));
		PeLauncher.Instance.Add(new LoadCustomDragItem(curYirdData.GetItems()));
		PeLauncher.Instance.Add(new LoadCustomSceneEffect(curYirdData.GetEffects()));
		PeLauncher.Instance.Add(new LoadCustomCreature(bNewGame));
		PeLauncher.Instance.Add(new LoadCustomEntityCreator(bNewGame, curYirdData));
		PeLauncher.Instance.Add(new LoadNPCTalkHistory(bNewGame));
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PeLauncher.Instance.Add(new LoadUiHelp(bNewGame));
		PeLauncher.Instance.Add(new InitBuildManager(bNewGame));
		PeLauncher.Instance.Add(new LoadCustomStory(bNewGame, customData));
		PeLauncher.Instance.Add(new LoadSingleCustomInitData(bNewGame));
		PeLauncher.Instance.Add(new LoadLootItem(bNewGame));
		PeLauncher.Instance.Add(new LoadWorldCollider());
		PlayerPackageCmpt.LockStackCount = false;
	}
}
