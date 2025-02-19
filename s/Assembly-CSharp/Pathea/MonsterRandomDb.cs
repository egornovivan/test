using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace Pathea;

public class MonsterRandomDb
{
	public class Item
	{
		public int player;

		public int[] equipments;

		public int[] weapons;
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdventureCampRelated");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item.player = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("color_type"));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("npc_equipment"));
			if (!string.IsNullOrEmpty(@string) && @string != " ")
			{
				string[] array = @string.Split(',');
				item.equipments = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					item.equipments[i] = int.Parse(array[i]);
				}
			}
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("npc_weapon"));
			if (!string.IsNullOrEmpty(string2) && string2 != " ")
			{
				string[] array2 = string2.Split(',');
				item.weapons = new int[array2.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					item.weapons[j] = int.Parse(array2[j]);
				}
			}
			sList.Add(item);
		}
	}

	public static int[] GetEquipments(int player)
	{
		return sList.Find((Item ret) => ret.player == player)?.equipments;
	}

	public static int GetWeapon(int player)
	{
		Item item = sList.Find((Item ret) => ret.player == player);
		if (item != null)
		{
			return item.weapons[Random.Range(0, item.weapons.Length)];
		}
		return -1;
	}
}
