using System.IO;
using UnityEngine;

namespace PeCustom;

public class SpawnData
{
	public int ID;

	public int EntityID = -1;

	public SpawnType Type;

	public Vector3 Position;

	public Quaternion Rotation;

	public Vector3 Scale;

	public string Name = "No Name";

	public int Prototype;

	public int PlayerIndex;

	public bool IsTarget;

	public bool Visible;

	public bool isDead;

	public int MaxRespawnCount;

	public float RespawnTime;

	public Bounds bound;

	public void Serialize(BinaryWriter bw)
	{
	}

	public void Deserialize(int version, BinaryReader br)
	{
	}
}
