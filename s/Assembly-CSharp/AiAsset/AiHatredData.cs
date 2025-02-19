using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace AiAsset;

public class AiHatredData
{
	public static int PlayerCamp = 1;

	public string m_camName;

	public int m_campID;

	public int[] m_campData;

	public static int CampCount;

	public static List<AiHatredData> s_tblCampData;

	public static void LoadData()
	{
		s_tblCampData = new List<AiHatredData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("ai_campnew");
		CampCount = sqliteDataReader.FieldCount - 3;
		while (sqliteDataReader.Read())
		{
			AiHatredData aiHatredData = new AiHatredData();
			aiHatredData.m_campData = new int[CampCount];
			aiHatredData.m_campID = Convert.ToInt32(sqliteDataReader.GetString(0));
			aiHatredData.m_camName = sqliteDataReader.GetString(1);
			for (int i = 0; i < CampCount; i++)
			{
				aiHatredData.m_campData[i] = Convert.ToInt32(sqliteDataReader.GetString(i + 3));
			}
			s_tblCampData.Add(aiHatredData);
		}
	}

	public static AiHatredData GetHatredData(int campID)
	{
		foreach (AiHatredData s_tblCampDatum in s_tblCampData)
		{
			if (s_tblCampDatum.m_campID == campID)
			{
				AiHatredData aiHatredData = new AiHatredData();
				return s_tblCampDatum;
			}
		}
		return null;
	}
}
