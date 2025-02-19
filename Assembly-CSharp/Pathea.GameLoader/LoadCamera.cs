using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadCamera : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		Object.Instantiate(Resources.Load<GameObject>("Prefab/GameUI/MinmapCamera")).name = "MinmapCamera";
		Camera.main.gameObject.AddComponent<GlobalGLs>();
		Camera.main.transform.position = PeSingleton<PlayerSpawnPosProvider>.Instance.GetPos();
		PeCamera.SetGlobalVector("Default Anchor", PeSingleton<PlayerSpawnPosProvider>.Instance.GetPos());
		PeCamera.RecordHistory();
	}
}
