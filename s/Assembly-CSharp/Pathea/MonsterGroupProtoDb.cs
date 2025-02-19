using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace Pathea;

public class MonsterGroupProtoDb
{
	public class Item
	{
		public int id;

		public int limitNum;

		public string name;

		public float hOffset;
	}

	private static List<Item> sList = new List<Item>(50);

	public static void Load()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeMonsterGroup");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item.id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("id"));
			item.name = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ENG_name"));
			item.hOffset = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("SpawnHeight"));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Spawn_Num"));
			string[] array = @string.Split(',');
			if (array.Length == 2)
			{
				int.TryParse(array[0], out var result);
				int.TryParse(array[1], out var result2);
				item.limitNum = Random.Range(result, result2);
			}
			else
			{
				item.limitNum = 2;
			}
			sList.Add(item);
		}
	}

	public static Item Get(int id)
	{
		return sList.Find((Item item) => item.id == id);
	}
}
