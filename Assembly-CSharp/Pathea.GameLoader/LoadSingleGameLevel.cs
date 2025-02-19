namespace Pathea.GameLoader;

internal class LoadSingleGameLevel : ModuleLoader
{
	public LoadSingleGameLevel(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
	}

	protected override void Restore()
	{
		PeGameSummary peGameSummary = PeSingleton<PeGameSummary.Mgr>.Instance.Get();
		if (peGameSummary != null)
		{
			PeGameMgr.gameLevel = peGameSummary.gameLevel;
		}
	}
}
