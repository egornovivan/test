using UnityEngine;

namespace Pathea.GameLoader;

internal abstract class LoadGrass : ModuleLoader
{
	public LoadGrass(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<GrassDataSLArchiveMgr>.Instance.New();
		Load();
	}

	protected override void Restore()
	{
		PeSingleton<GrassDataSLArchiveMgr>.Instance.Restore();
		Load();
	}

	private void Load()
	{
		string grassPrefabName = GetGrassPrefabName();
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(grassPrefabName));
		if (null == gameObject)
		{
			Debug.LogError("can't find grass prefab:" + grassPrefabName);
		}
	}

	protected abstract string GetGrassPrefabName();
}
