using UnityEngine;

namespace Pathea.GameLoader;

internal abstract class LoadTerrain : ModuleLoader
{
	public LoadTerrain(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		Load();
		PeSingleton<VoxelTerrainArchiveMgr>.Instance.New();
		SceneMan.self.New();
	}

	protected override void Restore()
	{
		Load();
		PeSingleton<VoxelTerrainArchiveMgr>.Instance.Restore();
		SceneMan.self.Restore();
	}

	protected virtual void Load()
	{
		Object.Instantiate(Resources.Load<GameObject>("Prefab/WindZone"));
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("SceneManager"));
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
	}
}
