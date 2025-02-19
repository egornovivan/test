namespace Pathea.GameLoader;

internal class LoadItemAsset : ModuleLoader
{
	public LoadItemAsset(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<ItemAssetArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<ItemAssetArchiveMgr>.Instance.Restore();
	}
}
