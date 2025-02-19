namespace PeCustom;

public class DoodadSpawnPoint : SpawnPoint
{
	public SceneStaticAgent agent;

	public DoodadSpawnPoint()
	{
	}

	public DoodadSpawnPoint(WEDoodad sp)
		: base(sp)
	{
	}

	public DoodadSpawnPoint(DoodadSpawnPoint sp)
		: base(sp)
	{
	}
}
