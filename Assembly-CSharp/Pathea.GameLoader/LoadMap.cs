using PeMap;
using UnityEngine;

namespace Pathea.GameLoader;

internal abstract class LoadMap : ModuleLoader
{
	public LoadMap(bool bNew)
		: base(bNew)
	{
	}

	private void LoadMapRunner()
	{
		new GameObject("MapRunner", typeof(MapRunner));
	}

	protected override void New()
	{
		LoadMapRunner();
		PeSingleton<StaticPoint.Mgr>.Instance.New();
		PeSingleton<UserLabel.Mgr>.Instance.New();
		PeSingleton<MaskTile.Mgr>.Instance.New();
		PeSingleton<TowerMark.Mgr>.Instance.New();
	}

	protected override void Restore()
	{
		LoadMapRunner();
		PeSingleton<StaticPoint.Mgr>.Instance.Restore();
		PeSingleton<UserLabel.Mgr>.Instance.Restore();
		PeSingleton<MaskTile.Mgr>.Instance.Restore();
		PeSingleton<ReviveLabel.Mgr>.Instance.Restore();
		PeSingleton<TowerMark.Mgr>.Instance.Restore();
	}
}
