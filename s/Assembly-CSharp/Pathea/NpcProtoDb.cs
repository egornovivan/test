using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace Pathea;

public class NpcProtoDb
{
	public class Item
	{
		public int id;

		public int sort;

		public string defaultName;

		public string defaultShowName;

		public PeSex sex;

		public DbAttr dbAttr = new DbAttr();
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeNPC");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item.id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("id"));
			item.sort = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("sort"));
			item.defaultName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("default_name"));
			item.defaultShowName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("default_showname"));
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
