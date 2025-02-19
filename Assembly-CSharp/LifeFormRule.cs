using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class LifeFormRule
{
	private static Dictionary<int, LifeFormRule> s_tblRules;

	public int mID;

	public float mUpdateInterval;

	public int mPropertyType;

	public float mPropertyValueMax;

	public int mConditionType;

	public float mConditionMin;

	public float mConditionMax;

	public int mCostSkillID;

	public static void LoadData()
	{
		s_tblRules = new Dictionary<int, LifeFormRule>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("LifeFormRule");
		while (sqliteDataReader.Read())
		{
			LifeFormRule lifeFormRule = new LifeFormRule();
			lifeFormRule.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			lifeFormRule.mUpdateInterval = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UpdateInterval")));
			lifeFormRule.mPropertyType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PropertyType")));
			lifeFormRule.mPropertyValueMax = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PropertyValueMax")));
			lifeFormRule.mConditionType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ConditionType")));
			lifeFormRule.mConditionMin = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ConditionMin")));
			lifeFormRule.mConditionMax = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ConditionMax")));
			lifeFormRule.mCostSkillID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CostSkillID")));
			s_tblRules[lifeFormRule.mID] = lifeFormRule;
		}
	}

	public static LifeFormRule GetRule(int id)
	{
		if (s_tblRules.ContainsKey(id))
		{
			return s_tblRules[id];
		}
		Debug.LogError("Can't find rule : " + id);
		return null;
	}
}
