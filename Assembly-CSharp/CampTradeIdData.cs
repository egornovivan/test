using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

[Obsolete]
public class CampTradeIdData
{
	public const int StoryDetectTrade = 1;

	public const int RandomDetectTrade = 2;

	public const int StoryMissionTrade = 3;

	public int id;

	public List<int> campId = new List<int>();

	public int mode;

	public int tradePostId;

	public static Dictionary<int, CampTradeIdData> campTradeIdInfo = new Dictionary<int, CampTradeIdData>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("trademanager");
		while (sqliteDataReader.Read())
		{
			CampTradeIdData campTradeIdData = new CampTradeIdData();
			campTradeIdData.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("campid")).Split(',');
			string[] array2 = array;
			foreach (string value in array2)
			{
				campTradeIdData.campId.Add(Convert.ToInt32(value));
			}
			campTradeIdData.mode = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("mode")));
			campTradeIdData.tradePostId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("tradepostid")));
			campTradeIdInfo.Add(campTradeIdData.id, campTradeIdData);
		}
	}

	public static bool IsStoryDetectTradeCamp(int campId)
	{
		foreach (KeyValuePair<int, CampTradeIdData> item in campTradeIdInfo)
		{
			if (item.Value.mode == 1 && item.Value.campId.Contains(campId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsStoryMissionTradeCamp(int campId)
	{
		foreach (KeyValuePair<int, CampTradeIdData> item in campTradeIdInfo)
		{
			if (item.Value.mode == 3 && item.Value.campId.Contains(campId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsDetectTradeCamp(int campId)
	{
		return campTradeIdInfo.ContainsKey(campId) && (campTradeIdInfo[campId].mode == 1 || campTradeIdInfo[campId].mode == 2);
	}
}
