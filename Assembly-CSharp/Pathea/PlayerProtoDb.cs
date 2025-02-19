using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;

namespace Pathea;

public static class PlayerProtoDb
{
	public class Item
	{
		[DbReader.DbField("InitBuffList", false)]
		public int[] initBuff;

		public DbAttr dbAttr;

		public int[] InFeildBuff = new int[2] { 30200053, 30200046 };

		public int[] RecruitBuff = new int[2] { 30200049, 30200050 };
	}

	private static List<Item> items;

	public static Item Get()
	{
		return (items.Count <= 0) ? null : items[0];
	}

	public static Item GetRandomNpc()
	{
		return (items.Count <= 1) ? null : items[1];
	}

	public static void Load()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("initprop");
		items = new List<Item>();
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item = DbReader.ReadItem<Item>(sqliteDataReader);
			item.dbAttr = new DbAttr();
			item.dbAttr.ReadFromDb(sqliteDataReader);
			items.Add(item);
		}
	}

	public static void Release()
	{
		items = null;
	}
}
