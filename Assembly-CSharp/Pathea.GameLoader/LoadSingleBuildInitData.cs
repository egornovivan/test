namespace Pathea.GameLoader;

internal class LoadSingleBuildInitData : ModuleLoader
{
	public LoadSingleBuildInitData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SingleGameInitData.AddBuildInitData();
	}

	protected override void Restore()
	{
	}
}
