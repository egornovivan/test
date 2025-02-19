using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadEditedTree : ModuleLoader
{
	public LoadEditedTree(bool bNew, string dir)
		: base(bNew)
	{
		LSubTerrIO.OriginalSubTerrainDir = dir;
	}

	private void Load()
	{
		string text = "Layered SubTerrain Group";
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>(text));
		if (null == gameObject)
		{
			Debug.LogError("can't find tree prefab:" + text);
			return;
		}
		LSubTerrainMgr.Instance.CameraTransform = Camera.main.transform;
		LSubTerrainMgr.Instance.VEditor = VoxelEditor.Get();
		LSubTerrSL.Init();
	}

	protected override void New()
	{
		Load();
		PeSingleton<LSubTerrSLArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		Load();
		PeSingleton<LSubTerrSLArchiveMgr>.Instance.Restore();
	}
}
