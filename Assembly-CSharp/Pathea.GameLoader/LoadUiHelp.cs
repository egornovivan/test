namespace Pathea.GameLoader;

internal class LoadUiHelp : ModuleLoader
{
	public LoadUiHelp(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<UiHelpArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<UiHelpArchiveMgr>.Instance.Restore();
	}
}
