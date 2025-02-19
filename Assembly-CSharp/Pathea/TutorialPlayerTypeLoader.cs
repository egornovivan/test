using System;
using System.IO;
using Pathea.GameLoader;
using UnityEngine;

namespace Pathea;

public class TutorialPlayerTypeLoader : PlayerTypeLoader
{
	private PeGameMgr.ETutorialMode mTutorialMode = PeGameMgr.ETutorialMode.Max;

	public PeGameMgr.ETutorialMode tutorialMode
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

	private string GetDataPath()
	{
		return Path.Combine(GameConfig.PEDataPath, "TutorialMode");
	}

	public override void Load()
	{
		TutorialGameData tutorialGameData = new TutorialGameData();
		if (tutorialGameData.Load(GetDataPath()))
		{
			TutorialLoader(tutorialGameData.yirdData, tutorialMode);
		}
	}

	private static void TutorialLoader(YirdData yirdData, PeGameMgr.ETutorialMode tutorialMode)
	{
		Debug.Log("Now Load Tutorial Scene, Mode is: " + tutorialMode);
		Debug.Log(DateTime.Now.ToString("G") + "[Start Game Mode] tutorial****************");
		bool flag = true;
		PeLauncher.Instance.Add(new LoadTutorialPlayerSpawnPos(flag));
		PeLauncher.Instance.Add(new ResetGlobalData());
		PeLauncher.Instance.Add(new LoadCamera());
		PeLauncher.Instance.Add(new LoadEditedTerrain(flag, string.Empty));
		PeLauncher.Instance.Add(new LoadItemAsset(flag));
		PeLauncher.Instance.Add(new LoadWaveSystem());
		PeLauncher.Instance.Add(new LoadCustomDoodad(flag, yirdData.GetDoodads()));
		PeLauncher.Instance.Add(new LoadCreature(flag));
		PeLauncher.Instance.Add(new LoadEntityCreator(flag));
		PeLauncher.Instance.Add(new LoadGUI());
		PeLauncher.Instance.Add(new LoadCustomMap(flag));
		PeLauncher.Instance.Add(new InitBuildManager(flag));
		PeLauncher.Instance.Add(new LoadTutorial());
		PeLauncher.Instance.Add(new LoadTutorialInitData(flag));
	}
}
