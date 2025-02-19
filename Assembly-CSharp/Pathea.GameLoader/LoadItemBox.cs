namespace Pathea.GameLoader;

internal class LoadItemBox : ModuleLoader
{
	public LoadItemBox(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		ItemBoxMgr.Instance.New();
	}

	protected override void Restore()
	{
		ItemBoxMgr.Instance.Restore();
	}
}
