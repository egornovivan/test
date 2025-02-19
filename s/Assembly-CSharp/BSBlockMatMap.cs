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
		catch (Exception exception)
		{
			if (LogFilter.logFatal)
			{
				Debug.LogException(exception);
			}
		}
	}

	public static bool BlockIsZero(BSVoxel voxel, float volume)
	{
		return voxel.value0 >> 2 == 0;
	}

	public static bool VoxelIsZero(BSVoxel voxel, float volume)
	{
		return (float)(int)voxel.value0 < volume;
	}

	public static int GetBlockItemProtoID(byte matIndex)
	{
		if (s_MatToItem.ContainsKey(matIndex))
		{
			return s_MatToItem[matIndex];
		}
		return -1;
	}

	public static int GetBlockMaterialType(int proto_id)
	{
		if (s_ItemToMat.ContainsKey(proto_id))
		{
			return s_ItemToMat[proto_id];
		}
		return -1;
	}
}
