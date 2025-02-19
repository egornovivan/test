using System.Collections.Generic;
using System.IO;

namespace PeCustom;

public class SpawnAreaData
{
	public List<int> SpawnIds;

	public int MaxRespawnCount;

	public float RespawnTime;

	public int SpawnAmount;

	public int AmountPerSocial;

	public bool IsSocial;

	public void Serialize(BinaryWriter bw)
	{
	}

	public void Deserialize(int version, BinaryReader br)
	{
	}
}
