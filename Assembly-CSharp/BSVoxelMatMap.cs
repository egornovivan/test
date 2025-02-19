using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class BSVoxelMatMap
{
	public class MapData
	{
		public int matID;

		public int itemProtoID;

		public int costNum;

		public string icon;
	}

	private static List<MapData> s_Datas;

	private static int[] s_MatToItemMap;

	private static Dictionary<int, List<int>> s_ItemToMatMap;

	public static List<int> GetAllProtoItems()
	{
		List<int> list = new List<int>();
		foreach (int key in s_ItemToMatMap.Keys)
		{
			list.Add(key);
		}
		return list;
	}

	public static void Load()
	{
		s_Datas = new List<MapData>();
		s_MatToItemMap = new int[256];
		for (int i = 0; i < s_MatToItemMap.Length; i++)
		{
			s_MatToItemMap[i] = -1;
		}
		s_ItemToMatMap = new Dictionary<int, List<int>>();
		try
		{
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("buildblock");
			while (sqliteDataReader.Read())
			{
				int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("matID")));
				int num2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemID")));
				int costNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("costNum")));
				string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("icon"));
				MapData mapData = new MapData();
				mapData.matID = num;
				mapData.itemProtoID = num2;
				mapData.costNum = costNum;
				mapData.icon = @string;
				s_Datas.Add(mapData);
				int item = s_Datas.Count - 1;
				s_MatToItemMap[num] = s_Datas.Count - 1;
				if (!s_ItemToMatMap.ContainsKey(num2))
				{
					s_ItemToMatMap.Add(num2, new List<int>());
				}
				s_ItemToMatMap[num2].Add(item);
			}
		}
		catch (Exception)
		{
			Debug.LogError("Load buildblock database table erro!");
		}
	}

	public static MapData GetMapData(int voxel_type)
	{
		if (s_MatToItemMap[voxel_type] != -1)
		{
			return s_Datas[s_MatToItemMap[voxel_type]];
		}
		return null;
	}

	public static int GetItemID(int voxel_type)
	{
		if (s_MatToItemMap[voxel_type] != -1)
		{
			return s_Datas[s_MatToItemMap[voxel_type]].itemProtoID;
		}
		return -1;
	}

	public static List<int> GetMaterialIDs(int item_id)
	{
		List<int> list = new List<int>();
		if (s_ItemToMatMap.ContainsKey(item_id))
		{
			List<int> list2 = s_ItemToMatMap[item_id];
			foreach (int item in list2)
			{
				list.Add(s_Datas[item].matID);
			}
		}
		return list;
	}
}
