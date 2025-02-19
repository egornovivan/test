namespace Pathea.GameLoader;

internal class LoadRandomTerrainWithoutTown : LoadRandomTerrain
{
	public LoadRandomTerrainWithoutTown(bool bNew)
		: base(bNew)
	{
	}

	protected override void Load()
	{
		VFDataRTGen.townAvailable = false;
		base.Load();
	}
}
