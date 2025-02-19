namespace Pathea.GameLoader;

internal class LoadAdventurePlayerSpawnPos : LoadPlayerSpawnPos
{
	public LoadAdventurePlayerSpawnPos(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SetPos(VArtifactTownManager.Instance.playerStartPos);
	}
}
