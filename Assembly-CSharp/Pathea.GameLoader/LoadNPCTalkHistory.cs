namespace Pathea.GameLoader;

internal class LoadNPCTalkHistory : ModuleLoader
{
	public LoadNPCTalkHistory(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<NPCTalkHistroy>.Instance.New();
	}

	protected override void Restore()
	{
		PeSingleton<NPCTalkHistroy>.Instance.Restore();
	}
}
