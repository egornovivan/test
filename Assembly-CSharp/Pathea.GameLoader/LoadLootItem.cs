namespace Pathea.GameLoader;

internal class LoadLootItem : ModuleLoader
{
	public LoadLootItem(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<LootItemMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<LootItemMgr>.Instance.Restore();
	}
}
