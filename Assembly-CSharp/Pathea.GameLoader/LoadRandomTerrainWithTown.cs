using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadRandomTerrainWithTown : LoadRandomTerrain
{
	public LoadRandomTerrainWithTown(bool bNew)
		: base(bNew)
	{
	}

	protected override void Load()
	{
		VFDataRTGen.townAvailable = true;
		base.Load();
	}

	private void LoadSupport()
	{
		Object.Instantiate(Resources.Load<GameObject>("RandomTerrainSupport")).name = "RandomTerrainSupport";
	}

	private void NewSupport()
	{
		LoadSupport();
		PeSingleton<VArtifactTownArchiveMgr>.Instance.New();
		PeSingleton<TownNpcArchiveMgr>.Instance.New();
		PeSingleton<VABuildingArchiveMgr>.Instance.New();
	}

	private void RestoreSupport()
	{
		LoadSupport();
		PeSingleton<VArtifactTownArchiveMgr>.Instance.Restore();
		PeSingleton<TownNpcArchiveMgr>.Instance.Restore();
		PeSingleton<VABuildingArchiveMgr>.Instance.Restore();
	}

	protected override void New()
	{
		NewSupport();
		base.New();
	}

	protected override void Restore()
	{
		RestoreSupport();
		base.Restore();
	}
}
