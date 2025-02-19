using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class StoreRepository
{
	private static Dictionary<int, StoreData> dicShop = new Dictionary<int, StoreData>();

	public static StoreData GetStoreData(int id)
	{
		if (dicShop.ContainsKey(id))
		{
			return dicShop[id];
		}
		return null;
	}

	public static StoreData GetNpcStoreData(int npcid)
	{
		foreach (KeyValuePair<int, StoreData> item in dicShop)
		{
			StoreData value = item.Value;
			if (value.npcid == npcid)
			{
				return value;
			}
		}
		return null;
	}

	public static string GetStoreNpcIcon(int npcid)
	{
		StoreData npcStoreData = GetNpcStoreData(npcid);
		if (npcStoreData == null || npcStoreData.itemList.Count <= 0)
		{
			return "0";
		}
		return npcStoreData.iconname;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPCstore");
		while (sqliteDataReader.Read())
		{
			StoreData storeData = new StoreData();
			int key = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("store_id")));
			storeData.npcid = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("storenpc_id")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("story"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					storeData.itemListstory.Add(Convert.ToInt32(array[i]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("adven_single"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (!(array[j] == "0"))
				{
					storeData.itemListadvensingle.Add(Convert.ToInt32(array[j]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("adven_coop"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					storeData.itemListadvencoop.Add(Convert.ToInt32(array[k]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("adven_vs"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					storeData.itemListadvenvs.Add(Convert.ToInt32(array[l]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("adven_freemode"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				if (!(array[m] == "0"))
				{
					storeData.itemListadvenfree.Add(Convert.ToInt32(array[m]));
				}
			}
			storeData.iconname = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("icon"));
			dicShop.Add(key, storeData);
		}
	}
}
