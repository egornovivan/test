using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadEntityCreator : ModuleLoader
{
	public LoadEntityCreator(bool bNew)
		: base(bNew)
	{
	}

	private void LoadPrefab()
	{
		Object.Instantiate(Resources.Load("Prefab/Mission/EntityCreateMgr"));
	}

	protected override void New()
	{
		LoadPrefab();
		EntityCreateMgr.Instance.New();
		SceneEntityCreator.self.New();
	}

	protected override void Restore()
	{
		LoadPrefab();
		SceneEntityCreator.self.Restore();
	}
}
