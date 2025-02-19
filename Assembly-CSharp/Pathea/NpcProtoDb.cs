using System.Collections.Generic;
using Behave.Runtime;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea;

public class NpcProtoDb
{
	public class Item
	{
		[DbReader.DbField("id", false)]
		public int id;

		[DbReader.DbField("sort", false)]
		public int sort;

		[DbReader.DbField("default_name", false)]
		public string name;

		[DbReader.DbField("default_showname", false)]
		public string showName;

		[DbReader.DbField("npc_icon", false)]
		public string icon;

		[DbReader.DbField("race", false)]
		public string race;

		[DbReader.DbField("npc_bigicon", false)]
		public string iconBig;

		[DbReader.DbField("path", false)]
		public string avatarModelPath;

		[DbReader.DbField("Bonepath_Editor", false)]
		public string modelPrefabPath;

		[DbReader.DbField("Bonepath", false)]
		public string modelBundlePath;

		[DbReader.DbField("behavior_path", false)]
		public string behaveDataPath;

		[DbReader.DbField("gender", true)]
		public PeSex sex;

		[DbReader.DbField("NPC_Chat1", false)]
		public int[] chart1;

		[DbReader.DbField("NPC_Chat2", false)]
		public int[] chart2;

		[DbReader.DbField("VoiceType", false)]
		public int voiceType;

		[DbReader.DbField("InitBuffList", false)]
		public int[] initBuff;

		public DbAttr dbAttr = new DbAttr();

		public int[] InFeildBuff = new int[2] { 30200053, 30200046 };

		public int[] RecruitBuff = new int[2] { 30200049, 30200050 };

		public string modelAssetPath = string.Empty;

		public Object modelObj;
	}

	private static List<Item> sList = new List<Item>(50);

	public static void Load()
	{
		sList.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeNPC");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			item.dbAttr.ReadFromDb(sqliteDataReader);
			BTResolver.RegisterToCache(item.behaveDataPath);
			if (!string.IsNullOrEmpty(item.modelBundlePath) && item.modelBundlePath != "0")
			{
				item.modelAssetPath = item.modelBundlePath;
			}
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList.Clear();
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => (item.id == id) ? true : false);
	}

	public static void CachePrefab()
	{
		int count = sList.Count;
		for (int i = 0; i < count; i++)
		{
			if (sList[i].modelAssetPath.StartsWith("Prefab"))
			{
				AssetsLoader.Instance.LoadPrefabImm(sList[i].modelAssetPath, bIntoCache: true);
			}
		}
	}
}
