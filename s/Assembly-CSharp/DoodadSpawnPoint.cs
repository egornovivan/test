public class DoodadSpawnPoint : SpawnPoint
{
	public ISceneObject m_Agent;

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
