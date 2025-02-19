namespace Pathea.GameLoader;

internal class RandomMission : ModuleLoader
{
	public RandomMission(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<AdRMRepositoryArchiveMgr>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<AdRMRepositoryArchiveMgr>.Instance.Restore();
	}
}
