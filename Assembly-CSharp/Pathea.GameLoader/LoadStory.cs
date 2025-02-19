namespace Pathea.GameLoader;

internal class LoadStory : LoadScenario
{
	public LoadStory(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		base.New();
		PeSingleton<MisRepositoryArchiveMgr>.Instance.New();
		PeSingleton<RMRepositoryArchiveMgr>.Instance.New();
		PeSingleton<NpcUserDataArchiveMgr>.Instance.New();
		PeSingleton<EntityCreatedArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		base.Restore();
		PeSingleton<MisRepositoryArchiveMgr>.Instance.Restore();
		PeSingleton<RMRepositoryArchiveMgr>.Instance.Restore();
		PeSingleton<NpcUserDataArchiveMgr>.Instance.Restore();
		PeSingleton<EntityCreatedArchiveMgr>.Instance.Restore();
	}
}
