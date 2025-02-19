using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class AISpawnDataAdvMulti
{
	public int mapType;

	public int mapIndex;

	public float minScale = 1f;

	public float maxScale = 1f;

	private AISpeciesData[] mapDataLand;

	private AISpeciesData[] mapDataWater;

	private AISpeciesData[] mapDataNight;

	private AISpeciesData[] mapDataCave;

	private AISpeciesData[] mapDataSky;

	private AISpeciesData[] mapDataBossLand;

	private AISpeciesData[] mapDataBossWater;

	private AISpeciesData[] mapDataBossCave;

	private AISpeciesData[] mapDataBossSky;

	private static List<AISpawnDataAdvMulti> m_data = new List<AISpawnDataAdvMulti>();

	public static int GetPathID(int mapID, int areaID, int pointTypeID)
	{
		return m_data.Find((AISpawnDataAdvMulti ret) => ret.mapType == mapID && ret.mapIndex == areaID)?.GetRanomAI(pointTypeID) ?? 0;
	}

	public static int GetPathID(int mapID, int areaID, int pointTypeID, float value)
	{
		return m_data.Find((AISpawnDataAdvMulti ret) => ret.mapType == mapID && ret.mapIndex == areaID)?.GetRanomAI(pointTypeID, value) ?? 0;
	}

	public static int GetBossPathID(int mapID, int areaID, int pointTypeID)
	{
		return m_data.Find((AISpawnDataAdvMulti ret) => ret.mapType == mapID && ret.mapIndex == areaID)?.GetRandomAIBoss(pointTypeID) ?? 0;
	}

	public static int GetBossPathID(int mapID, int areaID, int pointTypeID, float value)
	{
		return m_data.Find((AISpawnDataAdvMulti ret) => ret.mapType == mapID && ret.mapIndex == areaID)?.GetRanomAIBoss(pointTypeID, value) ?? 0;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("aispawn_multiAVG");
		while (sqliteDataReader.Read())
		{
			AISpawnDataAdvMulti aISpawnDataAdvMulti = new AISpawnDataAdvMulti();
			aISpawnDataAdvMulti.mapType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("area_type")));
			aISpawnDataAdvMulti.mapIndex = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("area_Value")));
			aISpawnDataAdvMulti.GetScaleValue(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("zoomValue")));
			aISpawnDataAdvMulti.mapDataLand = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterLandInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupLandInfo")));
			aISpawnDataAdvMulti.mapDataWater = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterWaterInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupWaterInfo")));
			aISpawnDataAdvMulti.mapDataCave = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterCaveInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupCaveInfo")));
			aISpawnDataAdvMulti.mapDataNight = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterNightInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupNightInfo")));
			aISpawnDataAdvMulti.mapDataSky = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterSkyInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupSkyInfo")));
			aISpawnDataAdvMulti.mapDataBossLand = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bossLandInfo")));
			aISpawnDataAdvMulti.mapDataBossWater = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bossWaterInfo")));
			aISpawnDataAdvMulti.mapDataBossCave = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bossCaveInfo")));
			aISpawnDataAdvMulti.mapDataBossSky = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bossSkyInfo")));
			m_data.Add(aISpawnDataAdvMulti);
		}
	}

	private void GetScaleValue(string str)
	{
		string[] array = str.Split(',');
		if (array.Length >= 2)
		{
			minScale = Convert.ToSingle(array[0]);
			maxScale = Convert.ToSingle(array[1]);
		}
	}

	private int GetRanomAI(int pointType)
	{
		switch (pointType)
		{
		case 1:
			if (GameConfig.IsNight)
			{
				return AISpeciesData.GetRandomAI(mapDataNight);
			}
			return AISpeciesData.GetRandomAI(mapDataLand);
		case 2:
			return AISpeciesData.GetRandomAI(mapDataWater);
		case 3:
			return AISpeciesData.GetRandomAI(mapDataCave);
		case 4:
			return AISpeciesData.GetRandomAI(mapDataSky);
		default:
			return 0;
		}
	}

	private int GetRanomAI(int pointType, float value)
	{
		switch (pointType)
		{
		case 1:
			if (GameConfig.IsNight)
			{
				return AISpeciesData.GetRandomAI(mapDataNight, value);
			}
			return AISpeciesData.GetRandomAI(mapDataLand, value);
		case 2:
			return AISpeciesData.GetRandomAI(mapDataWater, value);
		case 3:
			return AISpeciesData.GetRandomAI(mapDataCave, value);
		case 4:
			return AISpeciesData.GetRandomAI(mapDataSky, value);
		default:
			return 0;
		}
	}

	private int GetRandomAIBoss(int pointType)
	{
		return pointType switch
		{
			1 => AISpeciesData.GetRandomAI(mapDataBossLand), 
			2 => AISpeciesData.GetRandomAI(mapDataBossWater), 
			3 => AISpeciesData.GetRandomAI(mapDataBossCave), 
			4 => AISpeciesData.GetRandomAI(mapDataBossSky), 
			_ => 0, 
		};
	}

	private int GetRanomAIBoss(int pointType, float value)
	{
		return pointType switch
		{
			1 => AISpeciesData.GetRandomAI(mapDataBossLand, value), 
			2 => AISpeciesData.GetRandomAI(mapDataBossWater, value), 
			3 => AISpeciesData.GetRandomAI(mapDataBossCave, value), 
			4 => AISpeciesData.GetRandomAI(mapDataBossSky, value), 
			_ => 0, 
		};
	}
}
