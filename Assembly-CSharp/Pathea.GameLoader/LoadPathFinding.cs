using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadPathFinding : PeLauncher.ILaunchable
{
	private const string dataPathStory = "Prefab/Pathfinder";

	private const string dataPathStd = "Prefab/PathfinderStd";

	void PeLauncher.ILaunchable.Launch()
	{
		DestoryOldPathFinder();
		string path = ((!PeGameMgr.IsStory) ? "Prefab/PathfinderStd" : "Prefab/Pathfinder");
		GameObject gameObject = Resources.Load(path) as GameObject;
		if (gameObject != null)
		{
			AstarPath.SkipOptScanOnStartUp = true;
			GameObject gameObject2 = Object.Instantiate(gameObject);
			AstarPath.SkipOptScanOnStartUp = false;
			if (AstarPath.active != null && AstarPath.active.graphs.Length > 1)
			{
				AstarPath.active.Scan(2);
			}
		}
	}

	private void DestoryOldPathFinder()
	{
		if (AstarPath.active != null)
		{
			if (AstarPath.active.transform.parent != null)
			{
				Object.Destroy(AstarPath.active.transform.parent.gameObject);
			}
			else
			{
				Object.Destroy(AstarPath.active.gameObject);
			}
		}
	}
}
