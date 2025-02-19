using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadWorldCollider : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		Load();
	}

	private void Load()
	{
		Object.Instantiate(Resources.Load<GameObject>("Prefab/Other/WorldColliders"));
	}
}
