using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace SkillAsset;

public class EffCastList
{
	public int m_id;

	public int m_preEffId;

	public int m_castingEffId;

	public int m_targetEffId;

	public int m_buffEffId;

	public bool m_isMissile;

	public int m_missileEffId;

	private static Dictionary<int, EffCastList> m_data = new Dictionary<int, EffCastList>();

	public static EffCastList GetEffCastListData(int pID)
	{
		return (!m_data.ContainsKey(pID)) ? null : m_data[pID];
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("spellVisual");
		while (sqliteDataReader.Read())
		{
			EffCastList effCastList = new EffCastList();
			effCastList.m_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			effCastList.m_preEffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("precast_eff")));
			effCastList.m_castingEffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("casting_eff")));
			effCastList.m_targetEffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("target_eff")));
			effCastList.m_buffEffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("buff_eff")));
			effCastList.m_isMissile = ((Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("has_missile"))) != 0) ? true : false);
			effCastList.m_missileEffId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("missile_eff")));
			m_data.Add(effCastList.m_id, effCastList);
		}
	}
}
