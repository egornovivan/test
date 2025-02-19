namespace Pathea.GameLoader;

internal class LoadWorldInfo : ModuleLoader
{
	public LoadWorldInfo(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<WorldInfoMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<WorldInfoMgr>.Instance.Restore();
	}
}
