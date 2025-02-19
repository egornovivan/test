namespace Pathea.GameLoader;

internal class LoadSingleCustomInitData : ModuleLoader
{
	public LoadSingleCustomInitData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SingleGameInitData.AddCustomInitData();
	}

	protected override void Restore()
	{
	}
}
