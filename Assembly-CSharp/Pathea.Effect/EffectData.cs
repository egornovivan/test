using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea.Effect;

public class EffectData
{
	public int m_id;

	public string m_path;

	public float m_liveTime;

	public int m_direction;

	public string m_posStr;

	public bool m_bind;

	public bool m_Rot;

	public Vector3 m_Axis;

	private static Dictionary<int, EffectData> m_data = new Dictionary<int, EffectData>();

	public static EffectData GetEffCastData(int pID)
	{
		return (!m_data.ContainsKey(pID)) ? null : m_data[pID];
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("particle");
		while (sqliteDataReader.Read())
		{
			EffectData effectData = new EffectData();
			effectData.m_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			effectData.m_path = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("path"));
			effectData.m_liveTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("live")));
			effectData.m_posStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bone"));
			effectData.m_bind = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("bind")));
			effectData.m_Rot = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("rot")));
			effectData.m_Axis = PEUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("axis")), ',');
			m_data.Add(effectData.m_id, effectData);
		}
	}
}
