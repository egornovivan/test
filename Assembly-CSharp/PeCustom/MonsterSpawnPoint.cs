using System.IO;
using UnityEngine;

namespace PeCustom;

public class MonsterSpawnPoint : SpawnPoint
{
	public int MaxRespawnCount;

	public float RespawnTime;

	public Bounds bound;

	public SceneEntityAgent agent;

	private float mCurTime;

	public MonsterSpawnPoint()
	{
	}

	public MonsterSpawnPoint(WEMonster mst)
		: base(mst)
	{
		MaxRespawnCount = mst.MaxRespawnCount;
		RespawnTime = mst.RespawnTime;
	}

	public MonsterSpawnPoint(MonsterSpawnPoint sp)
		: base(sp)
	{
		MaxRespawnCount = sp.MaxRespawnCount;
		RespawnTime = sp.RespawnTime;
	}

	public override void Serialize(BinaryWriter bw)
	{
		base.Serialize(bw);
		bw.Write(MaxRespawnCount);
		bw.Write(RespawnTime);
		bw.Write(bound.center.x);
		bw.Write(bound.center.y);
		bw.Write(bound.center.z);
		bw.Write(bound.size.x);
		bw.Write(bound.size.y);
		bw.Write(bound.size.z);
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
			MaxRespawnCount = br.ReadInt32();
			RespawnTime = br.ReadSingle();
			bound = new Bounds(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()), new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
			break;
		}
	}

	public bool UpdateRespawnTime(float time_delta)
	{
		if (isDead)
		{
			mCurTime += time_delta;
			if (mCurTime >= RespawnTime)
			{
				mCurTime = 0f;
				return true;
			}
		}
		return false;
	}
}
