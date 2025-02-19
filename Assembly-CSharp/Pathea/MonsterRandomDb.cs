using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea;

public class MonsterRandomDb
{
	public class Item
	{
		[DbReader.DbField("color_type", false)]
		public int player;

		[DbReader.DbField("color_setting", false)]
		public Color color;

		[DbReader.DbField("npc_equipment", false)]
		public int[] equipments;

		[DbReader.DbField("npc_weapon", false)]
		public int[] weapons;

		public Dictionary<string, Material[]> materialDic = new Dictionary<string, Material[]>();

		public void RegisterMaterials(string modelName, Material[] materials)
		{
			if (!materialDic.ContainsKey(modelName))
			{
				Material[] array = new Material[materials.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Object.Instantiate(materials[i]);
					array[i].SetColor("_SkinColor", color);
					array[i].SetFloat("_SkinCoef", 1f);
				}
				materialDic.Add(modelName, array);
			}
		}
	}

	private static List<Item> sList;

	public static void Load()
	{
		sList = new List<Item>(50);
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdventureCampRelated");
		while (sqliteDataReader.Read())
		{
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
			sList.Add(item);
		}
	}

	public static void RegisterMaterials(int player, string modelName, Material[] materials)
	{
		sList.Find((Item ret) => ret.player == player)?.RegisterMaterials(modelName, materials);
	}

	public static bool ContainsMaterials(int player, string modelName)
	{
		return sList.Find((Item ret) => ret.player == player)?.materialDic.ContainsKey(modelName) ?? false;
	}

	public static Material[] GetMaterials(int player, string modelName)
	{
		Item item = sList.Find((Item ret) => ret.player == player);
		if (item != null && item.materialDic.ContainsKey(modelName))
		{
			return item.materialDic[modelName];
		}
		return null;
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
