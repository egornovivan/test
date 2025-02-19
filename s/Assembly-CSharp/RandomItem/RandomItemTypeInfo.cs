using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace RandomItem;

public class RandomItemTypeInfo
{
	public int itemNo;

	public int prototypeItem_id;

	public int itemAmount;

	public int itemWeight;

	public float itemFloating;

	public int itemLevel;

	public int itemType;

	public static Dictionary<int, Dictionary<int, List<RandomItemTypeInfo>>> mDataDic = new Dictionary<int, Dictionary<int, List<RandomItemTypeInfo>>>();

	public static List<RandomItemTypeInfo> RandomItemTypeInfoByLevel(int level, int type)
	{
		if (mDataDic.ContainsKey(type) && mDataDic[type].ContainsKey(level))
		{
			return mDataDic[type][level];
		}
		return null;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("randomitemtype");
		while (sqliteDataReader.Read())
		{
			RandomItemTypeInfo randomItemTypeInfo = new RandomItemTypeInfo();
			randomItemTypeInfo.itemNo = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemno")));
			randomItemTypeInfo.prototypeItem_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("prototypeItem_id")));
			randomItemTypeInfo.itemAmount = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemAmount")));
			randomItemTypeInfo.itemWeight = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemWeight")));
			randomItemTypeInfo.itemFloating = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemFloating")));
			randomItemTypeInfo.itemLevel = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemtypeLevel")));
			randomItemTypeInfo.itemType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemtype")));
			if (!mDataDic.ContainsKey(randomItemTypeInfo.itemType))
			{
				mDataDic[randomItemTypeInfo.itemType] = new Dictionary<int, List<RandomItemTypeInfo>>();
			}
			if (!mDataDic[randomItemTypeInfo.itemType].ContainsKey(randomItemTypeInfo.itemLevel))
			{
				mDataDic[randomItemTypeInfo.itemType][randomItemTypeInfo.itemLevel] = new List<RandomItemTypeInfo>();
			}
			mDataDic[randomItemTypeInfo.itemType][randomItemTypeInfo.itemLevel].Add(randomItemTypeInfo);
		}
	}
}
