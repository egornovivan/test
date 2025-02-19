using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace Pathea;

public class ThreatData
{
	private int m_ID;

	private string m_Name;

	private int[] m_Init;

	private int[] m_Amount;

	private int[] m_InitChange;

	private Dictionary<int, int> m_InitCover;

	private static Dictionary<int, ThreatData> s_ThreatData;

	public static void LoadData()
	{
		s_ThreatData = new Dictionary<int, ThreatData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("initthreat");
		int num = sqliteDataReader.FieldCount - 3;
		while (sqliteDataReader.Read())
		{
			ThreatData threatData = new ThreatData();
			threatData.m_Init = new int[num];
			threatData.m_Amount = new int[num];
			threatData.m_InitCover = new Dictionary<int, int>();
			threatData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(0));
			threatData.m_Name = sqliteDataReader.GetString(1);
			for (int i = 0; i < num; i++)
			{
				threatData.m_Init[i] = Convert.ToInt32(sqliteDataReader.GetString(i + 3));
			}
			s_ThreatData.Add(threatData.m_ID, threatData);
		}
	}

	public static int GetInitData(int src, int dst)
	{
		if (PeGameMgr.IsMulti)
		{
			if (src >= 10000 && src <= 19999 && dst >= 10000 && dst <= 19999)
			{
				if (PeGameMgr.IsVS)
				{
					return 5;
				}
				return 0;
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
		if (s_ThreatData.ContainsKey(src))
		{
			if (s_ThreatData[src].m_InitCover.ContainsKey(dst))
			{
				return s_ThreatData[src].m_InitCover[dst];
			}
			if (dst >= 0 && dst < s_ThreatData[src].m_Init.Length)
			{
				return s_ThreatData[src].m_Init[dst];
			}
			Debug.LogError("Error camp id : " + dst);
			return 0;
		}
		Debug.LogError("Error camp id : " + src);
		return 0;
	}

	public static void SetThreatData(int src, int dst, int value)
	{
		if (s_ThreatData.ContainsKey(src))
		{
			if (s_ThreatData[src].m_InitCover.ContainsKey(dst))
			{
				s_ThreatData[src].m_InitCover[dst] = value;
			}
			else
			{
				s_ThreatData[src].m_InitCover.Add(dst, value);
			}
		}
	}

	public static void Clear(int src)
	{
		if (s_ThreatData.ContainsKey(src))
		{
			s_ThreatData[src].m_InitCover.Clear();
		}
	}

	public static void Clear(int src, int dst)
	{
		if (s_ThreatData.ContainsKey(src) && s_ThreatData[src].m_InitCover.ContainsKey(dst))
		{
			s_ThreatData[src].m_InitCover.Remove(dst);
		}
	}
}
