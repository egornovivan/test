using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadRandomItemMgr : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		GameObject gameObject = new GameObject("RandomItemMgr");
		gameObject.AddComponent<RandomItemMgr>();
	}
}
