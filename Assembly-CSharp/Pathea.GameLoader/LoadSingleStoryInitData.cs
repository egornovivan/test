namespace Pathea.GameLoader;

internal class LoadSingleStoryInitData : ModuleLoader
{
	public LoadSingleStoryInitData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SingleGameInitData.AddStoryInitData();
	}

	protected override void Restore()
	{
	}
}
