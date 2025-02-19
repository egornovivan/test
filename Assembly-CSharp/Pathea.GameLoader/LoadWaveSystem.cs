using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadWaveSystem : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		LoadPrefab();
	}

	private void LoadPrefab()
	{
		Object.Instantiate(Resources.Load("Prefab/Wave Scene"));
	}
}
