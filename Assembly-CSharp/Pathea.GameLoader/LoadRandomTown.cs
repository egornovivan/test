using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadRandomTown : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		GameObject gameObject = new GameObject("VArtifactTownManager");
		gameObject.AddComponent<VArtifactTownManager>();
		VArtifactTownManager.Instance.InitISO();
		if (PeGameMgr.IsSingleAdventure && PeGameMgr.yirdName == AdventureScene.MainAdventure.ToString())
		{
			Object.Instantiate(Resources.Load("Prefab/MonsterSiege_Town"));
		}
		VArtifactTownManager.Instance.GenTown();
	}
}
