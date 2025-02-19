using UnityEngine;

public class MonsterSpawnPoint : SpawnPoint
{
	public int MaxRespawnCount;

	public float RespawnTime;

	private float mCurTime;

	public Bounds bound;

	public ISceneObject m_Agent;

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

	public void ResetRespawn()
	{
		mCurTime = 0f;
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
