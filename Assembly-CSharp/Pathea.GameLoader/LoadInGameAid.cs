namespace Pathea.GameLoader;

internal class LoadInGameAid : ModuleLoader
{
	public LoadInGameAid(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<InGamAidArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<InGamAidArchiveMgr>.Instance.Restore();
	}
}
