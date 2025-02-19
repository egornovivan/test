using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AISpawnPoint
{
	public static Dictionary<int, AISpawnPoint> s_spawnPointData = new Dictionary<int, AISpawnPoint>();

	public static Dictionary<int, int> s_spawnPoints = new Dictionary<int, int>();

	public int id;

	public int resId;

	public int count;

	public bool active;

	public bool fixPosition;

	public bool isGroup;

	public float radius;

	public float refreshtime;

	public Vector3 position;

	public Vector3 euler;

	private bool mActive;

	public bool isActive => mActive;

	public static void Reset()
	{
		foreach (KeyValuePair<int, AISpawnPoint> s_spawnPointDatum in s_spawnPointData)
		{
			s_spawnPointDatum.Value.ResetActive();
		}
	}

	public static AISpawnPoint Find(int pointID)
	{
		AISpawnPoint value;
		return (!s_spawnPointData.TryGetValue(pointID, out value)) ? null : value;
	}

	public static List<int> Find(float minx, float minz, float maxx, float maxz)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, AISpawnPoint> s_spawnPointDatum in s_spawnPointData)
		{
			Vector3 vector = s_spawnPointDatum.Value.position;
			if (vector.x > minx && vector.x <= maxx && vector.z > minz && vector.z <= maxz)
			{
				list.Add(s_spawnPointDatum.Key);
			}
		}
		return list;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("aispawn_fix");
		while (sqliteDataReader.Read())
		{
			AISpawnPoint aISpawnPoint = new AISpawnPoint();
			aISpawnPoint.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			aISpawnPoint.position = AiUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("position")));
			aISpawnPoint.resId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("pathid")));
			aISpawnPoint.radius = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("radius")));
			aISpawnPoint.refreshtime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("refresh")));
			aISpawnPoint.count = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("maxnum")));
			aISpawnPoint.active = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("produce")));
			aISpawnPoint.fixPosition = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("isfixpoint")));
			aISpawnPoint.euler = AiUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rotation")));
			aISpawnPoint.isGroup = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("isgroup")));
			s_spawnPointData.Add(aISpawnPoint.id, aISpawnPoint);
		}
	}

	public static void CreateKillMonsterFix()
	{
		foreach (KeyValuePair<int, AISpawnPoint> s_spawnPointDatum in s_spawnPointData)
		{
			if (s_spawnPointDatum.Value.active)
			{
				int aiId = ((!s_spawnPointDatum.Value.isGroup) ? s_spawnPointDatum.Value.resId : (s_spawnPointDatum.Value.resId | 0x40000000));
				SPTerrainEvent.CreateKillMonster(-1, 200, s_spawnPointDatum.Value.position, aiId, -1, -1, -1, -1, -1, s_spawnPointDatum.Key);
			}
		}
	}

	public static void CreateKillMonsterFix(int fixId)
	{
		foreach (KeyValuePair<int, AISpawnPoint> s_spawnPointDatum in s_spawnPointData)
		{
			if (s_spawnPointDatum.Key == fixId)
			{
				int aiId = ((!s_spawnPointDatum.Value.isGroup) ? s_spawnPointDatum.Value.resId : (s_spawnPointDatum.Value.resId | 0x40000000));
				SPTerrainEvent.CreateKillMonster(-1, 200, s_spawnPointDatum.Value.position, aiId, -1, -1, -1, -1, -1, s_spawnPointDatum.Key, 0, canride: false);
			}
		}
	}

	public static void KillKillMonsterFix(int fixId)
	{
		foreach (KeyValuePair<int, int> s_spawnPoint in s_spawnPoints)
		{
			if (s_spawnPoint.Key == fixId)
			{
				(ObjNetInterface.Get(s_spawnPoint.Value) as AiObject).OnDeath();
				break;
			}
		}
	}

	public static bool CheckSpawn(int id)
	{
		if (id == -1 || s_spawnPoints.ContainsKey(id))
		{
			return false;
		}
		return true;
	}

	public static bool AddSpawnPoint(int id, int obj)
	{
		if (!CheckSpawn(id))
		{
			if (id != -1)
			{
				AiMonsterNetwork aiMonsterNetwork = ObjNetInterface.Get<AiMonsterNetwork>(s_spawnPoints[id]);
				if (aiMonsterNetwork != null && aiMonsterNetwork.LordPlayer != null)
				{
					DeleteSpawnPoint(id);
					s_spawnPoints.Add(id, obj);
					return true;
				}
			}
			return false;
		}
		s_spawnPoints.Add(id, obj);
		return true;
	}

	public static void DeleteSpawnPoint(int id)
	{
		s_spawnPoints.Remove(id);
	}

	public void ResetActive()
	{
		mActive = active;
	}
}
