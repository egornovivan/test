using System;
using Mono.Data.SqliteClient;

namespace AiAsset;

public class AiGlobal
{
	public static float PatrolTimeMin = 5f;

	public static float PatrolTimeMax = 7f;

	public static float AISpawnAreaDist;

	public static float AISpawnGroupDist;

	public static int FrameTimeOut;

	public static int SatiationLimit;

	public static int DrinkingLimit;

	public static int FatigueLimit;

	public static int SatiationFallMin;

	public static int SatiationFallMax;

	public static int DrinkingFallMin;

	public static int DrinkingFallMax;

	public static int FatigueFallMin;

	public static int FatigueFallMax;

	public static float FeedPercentMin;

	public static float FeedPercentMax;

	public static float DrinkPercent;

	public static float SleepPercent;

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("ai_global");
		while (sqliteDataReader.Read())
		{
			AISpawnAreaDist = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_groupDist")));
			AISpawnGroupDist = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_areaDist")));
			FrameTimeOut = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_propertyDecInt")));
			SatiationLimit = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_satiationLimit")));
			DrinkingLimit = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_drinkingLimit")));
			FatigueLimit = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_fatigueLimit")));
			SatiationFallMin = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_satiationDecMin")));
			SatiationFallMax = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_satiationDecMax")));
			DrinkingFallMin = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_drinkingDecMin")));
			DrinkingFallMax = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_drinkingDecMax")));
			FatigueFallMin = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_fatigueDecMin")));
			FatigueFallMax = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_fatigueDecMax")));
			FeedPercentMin = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_feedingCriMin")));
			FeedPercentMax = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_feedingCriMax")));
			SleepPercent = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_sleepCritical")));
		}
	}
}
