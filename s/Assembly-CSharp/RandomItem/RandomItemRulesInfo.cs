using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace RandomItem;

public class RandomItemRulesInfo
{
	public int rulesNo;

	public int rulesId;

	public int equipmentLevel_1;

	public int equipmentWeightLevel_1;

	public int equipmentLevel_2;

	public int equipmentWeightLevel_2;

	public int equipmentLevel_3;

	public int equipmentWeightLevel_3;

	public int toolLevel_1;

	public int toolWeightLevel_1;

	public int toolLevel_2;

	public int toolWeightLevel_2;

	public int toolLevel_3;

	public int toolWeightLevel_3;

	public int scriptLevel_1;

	public int scriptWeightLevel_1;

	public int scriptLevel_2;

	public int scriptWeightLevel_2;

	public int scriptLevel_3;

	public int scriptWeightLevel_3;

	public int consumables;

	public int consumablesWeight;

	public int weightPool;

	public static Dictionary<int, RandomItemRulesInfo> mDataDic = new Dictionary<int, RandomItemRulesInfo>();

	public List<ItemIdCount> RandomItemDict(int count, System.Random rand = null)
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		if (rand == null)
		{
			rand = new System.Random((int)DateTime.UtcNow.Ticks);
		}
		for (int i = 0; i < count; i++)
		{
			int type;
			int level = RandomTypeLevel(out type, rand);
			List<RandomItemTypeInfo> list2 = RandomItemTypeInfo.RandomItemTypeInfoByLevel(level, type);
			if (list2 != null && list2.Count > 0)
			{
				RandomItemTypeInfo randomItemTypeInfo = new RandomItemTypeInfo();
				randomItemTypeInfo = list2[rand.Next(list2.Count)];
				int prototypeItem_id = randomItemTypeInfo.prototypeItem_id;
				double num = (double)randomItemTypeInfo.itemAmount * (1.0 + (double)randomItemTypeInfo.itemFloating * (rand.NextDouble() * 2.0 - 1.0));
				int num2 = Mathf.RoundToInt((float)num);
				if (num2 > 0)
				{
					ItemIdCount item = new ItemIdCount(prototypeItem_id, num2);
					list.Add(item);
				}
			}
		}
		return list;
	}

	public int CountWeightPool()
	{
		return equipmentWeightLevel_1 + equipmentWeightLevel_2 + equipmentWeightLevel_3 + toolWeightLevel_1 + toolWeightLevel_2 + toolWeightLevel_3 + scriptWeightLevel_1 + scriptWeightLevel_2 + scriptWeightLevel_3 + consumablesWeight;
	}

	private int RandomTypeLevel(out int type, System.Random rand = null)
	{
		if (rand == null)
		{
			rand = new System.Random((int)DateTime.UtcNow.Ticks);
		}
		int num = rand.Next(weightPool);
		if (num - equipmentWeightLevel_1 < 0)
		{
			type = 0;
			return equipmentLevel_1;
		}
		num -= equipmentWeightLevel_1;
		if (num - equipmentWeightLevel_2 < 0)
		{
			type = 0;
			return equipmentLevel_2;
		}
		num -= equipmentWeightLevel_2;
		if (num - equipmentWeightLevel_3 < 0)
		{
			type = 0;
			return equipmentLevel_3;
		}
		num -= equipmentWeightLevel_3;
		if (num - toolWeightLevel_1 < 0)
		{
			type = 1;
			return toolLevel_1;
		}
		num -= toolWeightLevel_1;
		if (num - toolWeightLevel_2 < 0)
		{
			type = 1;
			return toolLevel_2;
		}
		num -= toolWeightLevel_2;
		if (num - toolWeightLevel_3 < 0)
		{
			type = 1;
			return toolLevel_3;
		}
		num -= toolWeightLevel_3;
		if (num - scriptWeightLevel_1 < 0)
		{
			type = 2;
			return scriptLevel_1;
		}
		num -= scriptWeightLevel_1;
		if (num - scriptWeightLevel_2 < 0)
		{
			type = 2;
			return scriptLevel_2;
		}
		num -= scriptWeightLevel_2;
		if (num - scriptWeightLevel_3 < 0)
		{
			type = 2;
			return scriptLevel_3;
		}
		num -= scriptWeightLevel_3;
		if (num - consumablesWeight < 0)
		{
			type = 3;
			return consumables;
		}
		num -= consumablesWeight;
		type = -1;
		return -1;
	}

	public static RandomItemRulesInfo GetRuleInfoById(int id)
	{
		if (mDataDic.ContainsKey(id))
		{
			return mDataDic[id];
		}
		return null;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("randomItemrules");
		while (sqliteDataReader.Read())
		{
			RandomItemRulesInfo randomItemRulesInfo = new RandomItemRulesInfo();
			randomItemRulesInfo.rulesNo = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rulesno")));
			randomItemRulesInfo.rulesId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rulesid")));
			randomItemRulesInfo.equipmentLevel_1 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("equipmentLevel_1")));
			randomItemRulesInfo.equipmentWeightLevel_1 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("equipmentWeightLevel_1")));
			randomItemRulesInfo.equipmentLevel_2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("equipmentLevel_2")));
			randomItemRulesInfo.equipmentWeightLevel_2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("equipmentWeightLevel_2")));
			randomItemRulesInfo.equipmentLevel_3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("equipmentLevel_3")));
			randomItemRulesInfo.equipmentWeightLevel_3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("equipmentWeightLevel_3")));
			randomItemRulesInfo.toolLevel_1 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("toolLevel_1")));
			randomItemRulesInfo.toolWeightLevel_1 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("toolWeightLevel_1")));
			randomItemRulesInfo.toolLevel_2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("toolLevel_2")));
			randomItemRulesInfo.toolWeightLevel_2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("toolWeightLevel_2")));
			randomItemRulesInfo.toolLevel_3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("toolLevel_3")));
			randomItemRulesInfo.toolWeightLevel_3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("toolWeightLevel_3")));
			randomItemRulesInfo.scriptLevel_1 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptLevel_1")));
			randomItemRulesInfo.scriptWeightLevel_1 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptWeightLevel_1")));
			randomItemRulesInfo.scriptLevel_2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptLevel_2")));
			randomItemRulesInfo.scriptWeightLevel_2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptWeightLevel_2")));
			randomItemRulesInfo.scriptLevel_3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptLevel_3")));
			randomItemRulesInfo.scriptWeightLevel_3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptWeightLevel_3")));
			randomItemRulesInfo.consumables = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("consumables")));
			randomItemRulesInfo.consumablesWeight = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("consumablesWeight")));
			randomItemRulesInfo.weightPool = randomItemRulesInfo.CountWeightPool();
			mDataDic.Add(randomItemRulesInfo.rulesId, randomItemRulesInfo);
		}
	}
}
