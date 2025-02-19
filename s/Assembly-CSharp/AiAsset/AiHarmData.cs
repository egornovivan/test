using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace AiAsset;

public class AiHarmData
{
	public static int PlayerHarm = 1;

	public string m_harmName;

	public int m_harmID;

	public int[] m_harmData;

	public static int HarmCount;

	public static List<AiHarmData> s_tblHarmData;

	public static void LoadData()
	{
		s_tblHarmData = new List<AiHarmData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("ai_harmdata");
		HarmCount = sqliteDataReader.FieldCount - 3;
		while (sqliteDataReader.Read())
		{
			AiHarmData aiHarmData = new AiHarmData();
			aiHarmData.m_harmData = new int[HarmCount];
			aiHarmData.m_harmID = Convert.ToInt32(sqliteDataReader.GetString(0));
			aiHarmData.m_harmName = sqliteDataReader.GetString(1);
			for (int i = 0; i < HarmCount; i++)
			{
				aiHarmData.m_harmData[i] = Convert.ToInt32(sqliteDataReader.GetString(i + 3));
			}
			s_tblHarmData.Add(aiHarmData);
		}
	}

	public static AiHarmData GetHarmData(int id)
	{
		return s_tblHarmData.Find((AiHarmData ret) => ret.m_harmID == id);
	}

	public static int GetHarmValue(int srcHarmID, int dstHarmID)
	{
		AiHarmData harmData = GetHarmData(srcHarmID);
		if (harmData == null)
		{
			return 0;
		}
		if (dstHarmID < 0 || dstHarmID >= harmData.m_harmData.Length)
		{
			return 0;
		}
		return harmData.m_harmData[dstHarmID];
	}
}
