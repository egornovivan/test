using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadRandomDungeon : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		Object original = Resources.Load("Prefab/RandomDunGen/RandomDungenMgr");
		Object.Instantiate(original);
		RandomDungenMgr.Instance.Init();
		if (PeGameMgr.yirdName == AdventureScene.Dungen.ToString())
		{
			while (!RandomDungenMgr.Instance.GenDungeon())
			{
			}
			RandomDungenMgr.Instance.LoadPathFinding();
		}
		else
		{
			RandomDungenMgr.Instance.CreateInitTaskEntrance();
		}
		RandomDungenMgrData.initTaskEntrance.Clear();
	}
}
