using System.Collections.Generic;
using Behave.Runtime;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea;

public class MonsterGroupProtoDb
{
	public class Item
	{
		[DbReader.DbField("id", false)]
		public int id;

		[DbReader.DbField("ENG_name", false)]
		public string name;

		[DbReader.DbField("SpawnHeight", false)]
		public float hOffset;

		[DbReader.DbField("prefab_path", false)]
		public string prefabPath;

		[DbReader.DbField("behave_path", false)]
		public string behaveDataPath;

		[DbReader.DbField("SMonsterID", false)]
		public int protoID;

		[DbReader.DbField("Attack_Num", false)]
		public int[] atkMinMax;

		[DbReader.DbField("Spawn_Num", false)]
		public int[] cntMinMax;

		[DbReader.DbField("RadiusDesc", false)]
		public float[] radiusDesc;

		[DbReader.DbField("SubID", false)]
		public int[] subProtoID;

		[DbReader.DbField("SubPos", false)]
		public Vector3[] subPos;

		[DbReader.DbField("SubScl", false)]
		public Vector3[] subScl;

		[DbReader.DbField("SubRot", false)]
		public Vector3[] subRot;
	}

	private static List<Item> sList;

	private static Dictionary<int, string> sProtoIDMapBehave;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeMonsterGroup");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			BTResolver.RegisterToCache(item.behaveDataPath);
			sList.Add(item);
		}
		sProtoIDMapBehave = new Dictionary<int, string>(50);
		foreach (Item s in sList)
		{
			sProtoIDMapBehave[s.id] = s.behaveDataPath;
		}
	}

	public static void Release()
	{
		sList = null;
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => item.id == id);
	}

	public static string GetBehavePath(int protoId)
	{
		if (sProtoIDMapBehave.ContainsKey(protoId))
		{
			return sProtoIDMapBehave[protoId];
		}
		return string.Empty;
	}
}
