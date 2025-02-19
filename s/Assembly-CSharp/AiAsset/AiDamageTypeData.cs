using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace AiAsset;

public class AiDamageTypeData
{
	public int m_damageTypeId;

	public float[] m_damageData;

	public static int DamageTypeCount;

	public static List<AiDamageTypeData> s_tblDamageData;

	public static void LoadData()
	{
		s_tblDamageData = new List<AiDamageTypeData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("adtype");
		DamageTypeCount = sqliteDataReader.FieldCount - 1;
		while (sqliteDataReader.Read())
		{
			AiDamageTypeData aiDamageTypeData = new AiDamageTypeData();
			aiDamageTypeData.m_damageData = new float[DamageTypeCount];
			aiDamageTypeData.m_damageTypeId = Convert.ToInt32(sqliteDataReader.GetString(0));
			aiDamageTypeData.m_damageData[0] = 1f;
			for (int i = 1; i < DamageTypeCount; i++)
			{
				aiDamageTypeData.m_damageData[i] = Convert.ToSingle(sqliteDataReader.GetString(i + 1));
			}
			s_tblDamageData.Add(aiDamageTypeData);
		}
	}

	public static AiDamageTypeData GetDamageData(int damageId)
	{
		return s_tblDamageData.Find((AiDamageTypeData ret) => ret.m_damageTypeId == damageId);
	}

	public static float GetDamageScale(int damageType, int defenceType)
	{
		AiDamageTypeData aiDamageTypeData = s_tblDamageData.Find((AiDamageTypeData ret) => ret.m_damageTypeId == damageType);
		if (aiDamageTypeData == null)
		{
			return 1f;
		}
		if (defenceType < 0 || defenceType >= DamageTypeCount)
		{
			return 1f;
		}
		return aiDamageTypeData.m_damageData[defenceType];
	}
}
