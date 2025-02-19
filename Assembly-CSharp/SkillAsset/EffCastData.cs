using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillAsset;

public class EffCastData
{
	public int m_id;

	public string m_path;

	public float m_delaytime;

	public float m_liveTime;

	public int m_soundid;

	public int m_direction;

	public string m_posStr;

	public bool m_bind;

	public Vector3 mOffsetPos;

	private static Dictionary<int, EffCastData> m_data = new Dictionary<int, EffCastData>();

	public static EffCastData GetEffCastData(int pID)
	{
		return (!m_data.ContainsKey(pID)) ? null : m_data[pID];
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("spellEffect");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			EffCastData effCastData = new EffCastData();
			effCastData.mOffsetPos = Vector3.zero;
			effCastData.m_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			effCastData.m_path = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("path"));
			effCastData.m_delaytime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("time_delay")));
			effCastData.m_liveTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("time_live")));
			effCastData.m_soundid = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sound")));
			effCastData.m_direction = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("direction")));
			effCastData.m_posStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("position"));
			effCastData.m_bind = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("bind")));
			effCastData.mOffsetPos.x = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("offset_right")));
			effCastData.mOffsetPos.y = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("offset_Up")));
			effCastData.mOffsetPos.z = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("offset_front")));
			m_data.Add(effCastData.m_id, effCastData);
		}
	}
}
