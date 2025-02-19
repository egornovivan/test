namespace Pathea.GameLoader;

internal class LoadMountsMonsterData : ModuleLoader
{
	public LoadMountsMonsterData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<MountsArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<MountsArchiveMgr>.Instance.Restore();
	}
}
