using System.IO;

namespace PeCustom;

public class NPCSpawnPoint : SpawnPoint
{
	public SceneEntityAgent agent;

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
	}

	public override void Serialize(BinaryWriter bw)
	{
		base.Serialize(bw);
	}

	public override void Deserialize(int version, BinaryReader br)
	{
		base.Deserialize(version, br);
		switch (version)
		{
		case 1:
		case 2:
		case 3:
		case 4:
			break;
		}
	}
}
