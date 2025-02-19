using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadFarm : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		new GameObject("FarmManager", typeof(FarmManager));
	}
}
