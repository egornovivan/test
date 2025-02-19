using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadGUI : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		Load();
	}

	private void Load()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("Prefab/GameUISystem"));
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
	}
}
