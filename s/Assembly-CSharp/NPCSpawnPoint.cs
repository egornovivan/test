public class NPCSpawnPoint : SpawnPoint
{
	private bool mSpawned;

	public ISceneObject m_Agent;

	public bool IsSpawned => mSpawned;

	public NPCSpawnPoint()
	{
	}

	public NPCSpawnPoint(WENPC npc)
		: base(npc)
	{
	}

	public NPCSpawnPoint(NPCSpawnPoint sp)
		: base(sp)
	{
		mSpawned = sp.mSpawned;
	}
}
