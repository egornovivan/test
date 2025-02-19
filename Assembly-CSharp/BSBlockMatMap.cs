using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class BSBlockMatMap
{
	public static Dictionary<int, int> s_MatToItem;

	public static Dictionary<int, int> s_ItemToMat;

	public static List<int> GetAllProtoItems()
	{
		List<int> list = new List<int>();
		foreach (int key in s_ItemToMat.Keys)
		{
			list.Add(key);
		}
		return list;
	}

	public static void Load()
	{
		s_MatToItem = new Dictionary<int, int>();
		s_ItemToMat = new Dictionary<int, int>();
		try
		{
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("blocktype");
			while (sqliteDataReader.Read())
			{
				int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("matid")));
				int num2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemid")));
				s_MatToItem[num] = num2;
				s_ItemToMat[num2] = num;
			}
		}
		catch (Exception)
		{
			Debug.LogError("Load buildtype database table erro!");
		}
	}
}
