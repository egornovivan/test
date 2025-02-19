using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

public class SkCastData
{
	public int m_id;

	public string m_path;

	public float m_delaytime;

	public float m_liveTime;

	public int m_soundid;

	public int m_direction;

	public string m_posStr;

	public bool m_bind;

	public Vector3 m_Pivot;

	private static Dictionary<int, SkCastData> m_data = new Dictionary<int, SkCastData>();

	public static SkCastData GetEffCastData(int pID)
	{
		return (!m_data.ContainsKey(pID)) ? null : m_data[pID];
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("spellEffect");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			SkCastData skCastData = new SkCastData();
			skCastData.m_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			skCastData.m_path = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("path"));
			skCastData.m_delaytime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("time_delay")));
			skCastData.m_liveTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("time_live")));
			skCastData.m_soundid = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sound")));
			skCastData.m_direction = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("direction")));
			skCastData.m_posStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("position"));
			skCastData.m_bind = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("bind")));
			skCastData.m_Pivot = PEUtil.ToVector3(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bind")), ',');
			m_data.Add(skCastData.m_id, skCastData);
		}
	}
}
