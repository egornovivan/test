using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea;

public class DoodadProtoDb
{
	public class Item
	{
		[DbReader.DbField("id", false)]
		public int id;

		[DbReader.DbField("name", false)]
		public string name;

		[DbReader.DbField("LOOT", false)]
		public int dropItemId;

		[DbReader.DbField("asset_path", false)]
		public string modelPath;

		[DbReader.DbField("radius", false)]
		public float[] rangeDesc;

		[DbReader.DbField("ReputationValueID", false)]
		public int repValId;

		public RadiusBound range;

		public DbAttr dbAttr = new DbAttr();
	}

	private static List<Item> sList = new List<Item>(50);

	public static void Load()
	{
		sList.Clear();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeDoodad");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			item.dbAttr.ReadFromDb(sqliteDataReader);
			if (item.rangeDesc == null || Mathf.RoundToInt(item.rangeDesc[0]) != 0)
			{
				item.range = new RadiusBound(32f, 0f, 0f, 0f);
			}
			else
			{
				item.range = new RadiusBound(item.rangeDesc[4], item.rangeDesc[1], item.rangeDesc[2], item.rangeDesc[3]);
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
}
