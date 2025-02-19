using Railway;
using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadRailway : ModuleLoader
{
	public LoadRailway(bool bNew)
		: base(bNew)
	{
	}

	private void LoadPrefab()
	{
		new GameObject("RailwayManager", typeof(RailwayRunner));
	}

	protected override void New()
	{
		LoadPrefab();
		PeSingleton<Manager>.Instance.New();
	}

	protected override void Restore()
	{
		LoadPrefab();
		PeSingleton<Manager>.Instance.Restore();
	}
}
