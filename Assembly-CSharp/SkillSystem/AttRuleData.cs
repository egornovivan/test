using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace SkillSystem;

public class AttRuleData
{
	public int mID;

	public string mFilter;

	public string mCond;

	public string mAction;

	private static Dictionary<int, AttRuleData> _RuleData;

	public static AttRuleData GetRuleData(int id)
	{
		if (_RuleData.ContainsKey(id))
		{
			return _RuleData[id];
		}
		return null;
	}

	public static void LoadData()
	{
		_RuleData = new Dictionary<int, AttRuleData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AttRule");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			AttRuleData attRuleData = new AttRuleData();
			attRuleData.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			attRuleData.mFilter = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("filter"));
			attRuleData.mCond = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("condition"));
			attRuleData.mAction = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("action"));
		}
	}
}
