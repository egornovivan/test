namespace Pathea.GameLoader;

internal class LoadReputation : ModuleLoader
{
	public LoadReputation(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<ReputationSystem>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<ReputationSystem>.Instance.Restore();
	}
}
