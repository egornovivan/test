using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace Pathea;

public class AttPlusBuffDb
{
	public class Item
	{
		public int _type;

		public int _buffId;
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AttPlusBuff");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item._type = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("AttPlusID"));
			item._buffId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("skBuff_id"));
			sList.Add(item);
		}
	}

	public static void Release()
	{
		sList = null;
	}

	public static Item Get(int buffid)
	{
		return sList.Find((Item item) => (item._buffId == buffid) ? true : false);
	}

	public static Item Get(AttribType type)
	{
		return sList.Find((Item item) => (item._type == (int)type) ? true : false);
	}
}
