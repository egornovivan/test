using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AbnormalData
{
	public PEAbnormalType type;

	public string name;

	public string iconName;

	public string description;

	public int target;

	public bool deathRemove;

	public int[] buffID;

	private static Dictionary<PEAbnormalType, AbnormalData> g_DataDic;

	public static void LoadData()
	{
		g_DataDic = new Dictionary<PEAbnormalType, AbnormalData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AbnormalType");
		while (sqliteDataReader.Read())
		{
			AbnormalData abnormalData = new AbnormalData();
			abnormalData.type = (PEAbnormalType)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AbnormalId")));
			abnormalData.name = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TranslationNameId"))));
			abnormalData.iconName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Icon"));
			abnormalData.description = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TranslationDescribeId"))));
			abnormalData.target = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AbnormalTarget")));
			abnormalData.deathRemove = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("IsDeathRemove"))) == 1;
			g_DataDic.Add(abnormalData.type, abnormalData);
		}
	}

	public static AbnormalData GetData(PEAbnormalType type)
	{
		if (g_DataDic.ContainsKey(type))
		{
			return g_DataDic[type];
		}
		if (LogFilter.logDebug)
		{
			Debug.LogError("Can't find abnormaltype:" + type);
		}
		return null;
	}
}
