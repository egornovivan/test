namespace Pathea.GameLoader;

internal class LoadTutorialInitData : ModuleLoader
{
	public LoadTutorialInitData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SingleGameInitData.AddTutorialInitData();
	}

	protected override void Restore()
	{
	}
}
