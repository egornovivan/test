using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace Pathea;

public class DoodadProtoDb
{
	public class Item
	{
		public int id;

		public string name;

		public int dropItemId;

		public string modelPath;

		public DbAttr dbAttr = new DbAttr();
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeDoodad");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item.id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("id"));
			item.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("name"));
			item.dropItemId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("loot"));
			item.modelPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("asset_path"));
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
