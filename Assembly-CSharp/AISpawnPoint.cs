using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AISpawnPoint
{
	public static List<SPPoint> points = new List<SPPoint>();

	public static Dictionary<int, AISpawnPoint> s_spawnPointData = new Dictionary<int, AISpawnPoint>();

	public int id;

	public int resId;

	public int count;

	public bool active;

	public bool fixPosition;

	public bool isGroup;

	public bool isTower;

	public float radius;

	public float refreshtime;

	private Vector3 position;

	public Vector3 euler;

	private bool mActive;

	private SPPoint mPoint;

	public Vector3 Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public SPPoint spPoint
	{
		get
		{
			return mPoint;
		}
		set
		{
			if (mPoint != null)
			{
				if (points.Contains(mPoint))
				{
					points.Remove(mPoint);
				}
				UnityEngine.Object.Destroy(mPoint.gameObject);
				Debug.LogError("Static SPPoint has existed!!");
			}
			mPoint = value;
			if (mPoint != null && !points.Contains(mPoint))
			{
				points.Add(mPoint);
			}
		}
	}

	public bool isActive => mActive;

	private void OnDeath(AiObject aiObj)
	{
		mActive = false;
		if (spPoint != null)
		{
			spPoint.SetActive(isActive: false);
		}
	}

	public void OnSpawned(GameObject obj)
	{
		if (!(obj == null))
		{
			AiObject component = obj.GetComponent<AiObject>();
			if (component != null)
			{
				component.DeathHandlerEvent += OnDeath;
			}
		}
	}

	public static void Reset()
	{
		foreach (KeyValuePair<int, AISpawnPoint> s_spawnPointDatum in s_spawnPointData)
		{
			s_spawnPointDatum.Value.ResetActive();
		}
	}

	public static void Activate(int pointID, bool isActive)
	{
		AISpawnPoint aISpawnPoint = Find(pointID);
		if (aISpawnPoint != null && aISpawnPoint.spPoint != null)
		{
			aISpawnPoint.mActive = isActive;
			aISpawnPoint.spPoint.SetActive(isActive);
		}
	}

	public static void SpawnImmediately(int pointID)
	{
		AISpawnPoint aISpawnPoint = Find(pointID);
		if (aISpawnPoint != null && aISpawnPoint.spPoint != null)
		{
			aISpawnPoint.spPoint.SetActive(isActive: true);
			aISpawnPoint.spPoint.InstantiateImmediately();
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
			Vector3 vector = s_spawnPointDatum.Value.Position;
			if (vector.x > minx && vector.x <= maxx && vector.z > minz && vector.z <= maxz)
			{
				list.Add(s_spawnPointDatum.Key);
			}
		}
		return list;
	}

	public static List<SPPoint> GetPoints(IntVector4 node)
	{
		return points.FindAll((SPPoint ret) => Match(ret, node));
	}

	private static bool Match(SPPoint point, IntVector4 node)
	{
		if (point == null)
		{
			return false;
		}
		float num = point.position.x - (float)node.x;
		float num2 = point.position.z - (float)node.z;
		return num >= float.Epsilon && num <= (float)(32 << node.w) && num2 >= float.Epsilon && num2 <= (float)(32 << node.w);
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("aispawn_fix");
		while (sqliteDataReader.Read())
		{
			AISpawnPoint aISpawnPoint = new AISpawnPoint();
			aISpawnPoint.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("position"));
			if (@string.Split(',').Length == 3)
			{
				aISpawnPoint.position = AiUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("position")));
			}
			aISpawnPoint.resId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("pathid")));
			aISpawnPoint.radius = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("radius")));
			aISpawnPoint.refreshtime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("refresh")));
			aISpawnPoint.count = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("maxnum")));
			aISpawnPoint.active = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("produce")));
			aISpawnPoint.fixPosition = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("isfixpoint")));
			aISpawnPoint.euler = AiUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rotation")));
			aISpawnPoint.isGroup = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("isgroup")));
			aISpawnPoint.isTower = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("istower")));
			s_spawnPointData.Add(aISpawnPoint.id, aISpawnPoint);
		}
	}

	public static void RegisterSPPoint(SPPoint point)
	{
		if (!(point == null) && !points.Contains(point))
		{
			points.Add(point);
		}
	}

	public void ResetActive()
	{
		mActive = active;
	}
}
