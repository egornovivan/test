using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadRandomTree : ModuleLoader
{
	public LoadRandomTree(bool bNew)
		: base(bNew)
	{
	}

	private void Load()
	{
		string text = "Layered Random SubTerrain Group";
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(text));
		if (null == gameObject)
		{
			Debug.LogError("can't find tree prefab:" + text);
			return;
		}
		RSubTerrainMgr.Instance.CameraTransform = Camera.main.transform;
		RSubTerrainMgr.Instance.VEditor = VoxelEditor.Get();
		RSubTerrSL.Init();
	}

	protected override void New()
	{
		Load();
		PeSingleton<RSubTerrSLArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		Load();
		PeSingleton<RSubTerrSLArchiveMgr>.Instance.Restore();
	}
}
