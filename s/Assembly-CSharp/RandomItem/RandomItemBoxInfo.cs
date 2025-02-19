using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.SqliteClient;

namespace RandomItem;

public class RandomItemBoxInfo
{
	public int boxNo;

	public int boxId;

	public string boxName;

	public int boxAmount;

	public float boxDepth;

	public int boxRange;

	public List<int> boxMapType;

	public int boxItemAmountMin;

	public int boxItemAmountMax;

	public int rulesId;

	public string boxModelPath;

	public static Dictionary<int, RandomItemBoxInfo> mDataDic = new Dictionary<int, RandomItemBoxInfo>();

	public bool MatchCondition(List<int> conditions)
	{
		foreach (int item in boxMapType)
		{
			if (!conditions.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public static RandomItemBoxInfo GetBoxInfoById(int id)
	{
		if (mDataDic.ContainsKey(id))
		{
			return mDataDic[id];
		}
		return null;
	}

	public static List<RandomItemBoxInfo> RandomBoxMatchCondition(List<int> conditions, int height)
	{
		List<RandomItemBoxInfo> list = new List<RandomItemBoxInfo>();
		foreach (RandomItemBoxInfo value in mDataDic.Values)
		{
			if (value.MatchCondition(conditions) && value.boxDepth <= (float)height)
			{
				list.Add(value);
			}
		}
		return list;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("randombox");
		while (sqliteDataReader.Read())
		{
			RandomItemBoxInfo randomItemBoxInfo = new RandomItemBoxInfo();
			randomItemBoxInfo.boxNo = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxno")));
			randomItemBoxInfo.boxId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxid")));
			randomItemBoxInfo.boxName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxname"));
			randomItemBoxInfo.boxAmount = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxamount")));
			randomItemBoxInfo.boxDepth = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxdepth")));
			randomItemBoxInfo.boxRange = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxrange")));
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxmaptype")).Split(',');
			randomItemBoxInfo.boxMapType = new List<int>();
			string[] array2 = array;
			foreach (string value in array2)
			{
				randomItemBoxInfo.boxMapType.Add(Convert.ToInt32(value));
			}
			string[] array3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxitemamount")).Split(',');
			randomItemBoxInfo.boxItemAmountMin = Convert.ToInt32(array3[0]);
			randomItemBoxInfo.boxItemAmountMax = randomItemBoxInfo.boxItemAmountMin;
			if (array3.Count() > 1)
			{
				randomItemBoxInfo.boxItemAmountMax = Convert.ToInt32(array3[1]);
			}
			randomItemBoxInfo.rulesId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rulesid")));
			randomItemBoxInfo.boxModelPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxmodelpath"));
			mDataDic.Add(randomItemBoxInfo.boxId, randomItemBoxInfo);
		}
	}
}
