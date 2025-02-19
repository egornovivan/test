using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace Pathea;

public class DamageData
{
	private int m_ID;

	private int[] m_Data;

	private static Dictionary<int, DamageData> s_CampData;

	public static void LoadData()
	{
		s_CampData = new Dictionary<int, DamageData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("isaddhatred");
		int num = sqliteDataReader.FieldCount - 3;
		while (sqliteDataReader.Read())
		{
			DamageData damageData = new DamageData();
			damageData.m_Data = new int[num];
			damageData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(0));
			sqliteDataReader.GetString(1);
			for (int i = 0; i < num; i++)
			{
				damageData.m_Data[i] = Convert.ToInt32(sqliteDataReader.GetString(i + 3));
			}
			s_CampData.Add(damageData.m_ID, damageData);
		}
	}

	public static int GetValue(int src, int dst)
	{
		try
		{
			if (PeGameMgr.IsMulti)
			{
				if (src >= 10000 && src <= 19999 && dst >= 10000 && dst <= 19999)
				{
					return 5;
				}
				if (src >= 10000 && src <= 19999)
				{
					src = 1;
				}
				else if (dst >= 10000 && dst <= 19999)
				{
					dst = 1;
				}
			}
			if (s_CampData.ContainsKey(src))
			{
				return s_CampData[src].m_Data[dst];
			}
		}
		catch
		{
			Debug.LogError("src id = " + src + " --> dst id = " + dst);
		}
		return 0;
	}
}
