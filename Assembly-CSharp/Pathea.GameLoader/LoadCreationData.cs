namespace Pathea.GameLoader;

internal class LoadCreationData : ModuleLoader
{
	public LoadCreationData(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<CreationDataArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<CreationDataArchiveMgr>.Instance.Restore();
	}
}
