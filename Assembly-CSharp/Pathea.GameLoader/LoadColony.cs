using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadColony : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		Object.Instantiate(Resources.Load<GameObject>("Prefabs/CSMain")).name = "CSMain";
	}
}
