namespace Pathea.GameLoader;

internal class LoadRandomTerrainParam : ModuleLoader
{
	public LoadRandomTerrainParam(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<RandomMapConfigArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<RandomMapConfigArchiveMgr>.Instance.Restore();
	}
}
