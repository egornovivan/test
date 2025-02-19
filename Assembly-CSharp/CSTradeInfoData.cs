using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

[Obsolete]
public class CSTradeInfoData
{
	public const int RANDOM_TOWN_ID = 1;

	public int id;

	public List<TradeObj> needItemList = new List<TradeObj>();

	public int needTypeAmountMin;

	public int needTypeAmountMax;

	public float needRandomVariate;

	public List<TradeObj> rewardItemList = new List<TradeObj>();

	public int rewardTypeAmountMin;

	public int rewardTypeAmountMax;

	public float refreshTime;

	public string icon;

	public static Dictionary<int, CSTradeInfoData> mTradeInfo = new Dictionary<int, CSTradeInfoData>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("tradePost");
		while (sqliteDataReader.Read())
		{
			CSTradeInfoData cSTradeInfoData = new CSTradeInfoData();
			cSTradeInfoData.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("tradeID")));
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("need")).Split(';');
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(',');
				TradeObj item = new TradeObj(Convert.ToInt32(array3[0]), Convert.ToInt32(array3[1]));
				cSTradeInfoData.needItemList.Add(item);
			}
			string[] array4 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("typeAmount1")).Split(',');
			cSTradeInfoData.needTypeAmountMin = Convert.ToInt32(array4[0]);
			cSTradeInfoData.needTypeAmountMax = Convert.ToInt32(array4[1]);
			cSTradeInfoData.needRandomVariate = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("randomMax"));
			string[] array5 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("reward")).Split(';');
			string[] array6 = array5;
			foreach (string text2 in array6)
			{
				string[] array7 = text2.Split(',');
				TradeObj item2 = new TradeObj(Convert.ToInt32(array7[0]), Convert.ToInt32(array7[1]));
				cSTradeInfoData.rewardItemList.Add(item2);
			}
			string[] array8 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("typeAmount2")).Split(',');
			cSTradeInfoData.rewardTypeAmountMin = Convert.ToInt32(array8[0]);
			cSTradeInfoData.rewardTypeAmountMax = Convert.ToInt32(array8[1]);
			cSTradeInfoData.refreshTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("refreshTime")));
			if (Application.isEditor)
			{
				cSTradeInfoData.refreshTime = 40f;
			}
			cSTradeInfoData.icon = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("icon"));
			cSTradeInfoData.CheckData();
			mTradeInfo.Add(cSTradeInfoData.id, cSTradeInfoData);
		}
	}

	public void CheckData()
	{
		if (needTypeAmountMax > needItemList.Count)
		{
			Debug.LogError(id + ":typeAmount1 too large!");
		}
		if (rewardTypeAmountMax > rewardItemList.Count)
		{
			Debug.LogError(id + ":typeAmount2 too large!");
		}
	}

	public static CSTradeInfoData GetData(int id)
	{
		if (mTradeInfo.ContainsKey(id))
		{
			return mTradeInfo[id];
		}
		return null;
	}
}
