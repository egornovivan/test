using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;

namespace Pathea;

public class AttPlusBuffDb
{
	public class Item
	{
		[DbReader.DbField("AttPlusID", false)]
		public int _type;

		[DbReader.DbField("skBuff_id", false)]
		public int _buffId;
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AttPlusBuff");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
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
