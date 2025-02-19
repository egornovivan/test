using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace Pathea;

public class MonsterProtoDb
{
	public class Item
	{
		public int id;

		public int dropItemId;

		public string name;

		public int[] initEquip;

		public float maxScale;

		public float minScale;

		public int npcProtoID;

		public DbAttr dbAttr = new DbAttr();
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeMonster");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item.id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("id"));
			item.dropItemId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("loot"));
			item.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ENG_name"));
			item.npcProtoID = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("Npc_id"));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EquipID"));
			if (!string.IsNullOrEmpty(@string) && @string.Equals("0"))
			{
				string[] array = @string.Split(',');
				item.initEquip = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					item.initEquip[i] = int.Parse(array[i]);
				}
			}
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Scale"));
			if (!string.IsNullOrEmpty(string2) && !string2.Equals("0"))
			{
				string[] array2 = string2.Split(',');
				item.maxScale = Convert.ToSingle(array2[0]);
				item.minScale = Convert.ToSingle(array2[1]);
			}
			item.dbAttr.ReadFromDb(sqliteDataReader);
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList = null;
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => (item.id == id) ? true : false);
	}
}
