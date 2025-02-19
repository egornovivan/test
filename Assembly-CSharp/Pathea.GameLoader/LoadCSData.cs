namespace Pathea.GameLoader;

internal class LoadCSData : ModuleLoader
{
	public LoadCSData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<CSDataMgrArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<CSDataMgrArchiveMgr>.Instance.Restore();
	}
}
