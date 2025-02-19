namespace Pathea.GameLoader;

internal class LoadSingleAdventureInitData : ModuleLoader
{
	public LoadSingleAdventureInitData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SingleGameInitData.AddAdventureInitData();
	}

	protected override void Restore()
	{
	}
}
