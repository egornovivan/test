namespace Pathea.GameLoader;

internal class LoadRandomStory : LoadScenario
{
	public LoadRandomStory(bool bNew)
		: base(bNew)
	{
	}

	protected override void Init()
	{
		base.Init();
	}

	protected override void New()
	{
		base.New();
		PeSingleton<MisRepositoryArchiveMgr>.Instance.New();
		PeSingleton<NpcUserDataArchiveMgr>.Instance.New();
		PeSingleton<EntityCreatedArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		base.Restore();
		PeSingleton<MisRepositoryArchiveMgr>.Instance.Restore();
		PeSingleton<NpcUserDataArchiveMgr>.Instance.Restore();
		PeSingleton<EntityCreatedArchiveMgr>.Instance.Restore();
	}
}
