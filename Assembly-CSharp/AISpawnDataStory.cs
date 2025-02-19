using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AISpawnDataStory
{
	private struct EffectData
	{
		public int type1;

		public int type2;

		public int[] effectIds;
	}

	private int id;

	private Color mColor;

	private AISpeciesData[] mapDataLand;

	private AISpeciesData[] mapDataWater;

	private AISpeciesData[] mapDataCave;

	private AISpeciesData[] mapDataNight;

	private AISpeciesData[] mapDataSky;

	private int mBgSound;

	private List<int> mEnvSounds;

	private List<EffectData> mEffects;

	private static List<AISpawnDataStory> m_data = new List<AISpawnDataStory>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("aispawn_story");
		while (sqliteDataReader.Read())
		{
			AISpawnDataStory aISpawnDataStory = new AISpawnDataStory();
			aISpawnDataStory.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			aISpawnDataStory.mColor.r = (float)(int)sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("R")) / 255f;
			aISpawnDataStory.mColor.g = (float)(int)sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("G")) / 255f;
			aISpawnDataStory.mColor.b = (float)(int)sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("B")) / 255f;
			aISpawnDataStory.mColor.a = (float)(int)sqliteDataReader.GetByte(sqliteDataReader.GetOrdinal("A")) / 255f;
			aISpawnDataStory.mapDataLand = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterLandInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupLandInfo")));
			aISpawnDataStory.mapDataWater = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterWaterInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupWaterInfo")));
			aISpawnDataStory.mapDataCave = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterCaveInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupCaveInfo")));
			aISpawnDataStory.mapDataNight = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterNightInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupNightInfo")));
			aISpawnDataStory.mapDataSky = AISpeciesData.AnalysisSpeciesString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("monsterSkyInfo")), sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("groupSkyInfo")));
			aISpawnDataStory.mBgSound = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Music")));
			aISpawnDataStory.mEnvSounds = AnalysisSoundString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("surroundMusic")));
			aISpawnDataStory.mEffects = AnalysisEffectString(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Effect")));
			m_data.Add(aISpawnDataStory);
		}
	}

	public static AISpeciesData[] GetAiSpawnData(int mapid, int typeid)
	{
		AISpawnDataStory aISpawnDataStory = m_data.Find((AISpawnDataStory it) => it.id == mapid);
		if (aISpawnDataStory == null)
		{
			Debug.LogError("[AiSpawnData]Spawn not found:" + mapid);
			return null;
		}
		switch (typeid)
		{
		default:
			Debug.LogError("[AiSpawnData]Type is error:" + typeid);
			return null;
		case 1:
			if (GameConfig.IsNight)
			{
				return aISpawnDataStory.mapDataNight;
			}
			return aISpawnDataStory.mapDataLand;
		case 2:
			return aISpawnDataStory.mapDataWater;
		case 3:
			return aISpawnDataStory.mapDataCave;
		case 4:
			return aISpawnDataStory.mapDataSky;
		}
	}

	public static int GetBgMusicID(int mapid)
	{
		return m_data.Find((AISpawnDataStory it) => it.id == mapid)?.mBgSound ?? 0;
	}

	public static int GetEnvMusicID(int mapid)
	{
		AISpawnDataStory aISpawnDataStory = m_data.Find((AISpawnDataStory it) => it.id == mapid);
		if (aISpawnDataStory != null && aISpawnDataStory.mEnvSounds != null && aISpawnDataStory.mEnvSounds.Count > 0)
		{
			return aISpawnDataStory.mEnvSounds[UnityEngine.Random.Range(0, aISpawnDataStory.mEnvSounds.Count)];
		}
		return 0;
	}

	public static int GetEnvEffectID(int mapid, int type1, int type2)
	{
		AISpawnDataStory aISpawnDataStory = m_data.Find((AISpawnDataStory it) => it.id == mapid);
		if (aISpawnDataStory != null)
		{
			EffectData effectData = aISpawnDataStory.mEffects.Find((EffectData ret) => ret.type1 == type1 && ret.type2 == type2);
			if (effectData.effectIds != null && effectData.effectIds.Length > 0)
			{
				return effectData.effectIds[UnityEngine.Random.Range(0, effectData.effectIds.Length)];
			}
		}
		return 0;
	}

	public static int GetEnvironmentEffect(int type1, int type2, Color color)
	{
		AISpawnDataStory aISpawnDataStory = m_data.Find((AISpawnDataStory ret) => MatchMap(ret, color));
		if (aISpawnDataStory != null)
		{
			EffectData effectData = aISpawnDataStory.mEffects.Find((EffectData ret) => ret.type1 == type1 && ret.type2 == type2);
			if (effectData.effectIds != null && effectData.effectIds.Length > 0)
			{
				return effectData.effectIds[UnityEngine.Random.Range(0, effectData.effectIds.Length)];
			}
		}
		return 0;
	}

	private static List<int> AnalysisSoundString(string str)
	{
		List<int> list = new List<int>();
		string[] array = AiUtil.Split(str, ';');
		string[] array2 = array;
		foreach (string value in array2)
		{
			list.Add(Convert.ToInt32(value));
		}
		return list;
	}

	private static List<EffectData> AnalysisEffectString(string str)
	{
		List<EffectData> list = new List<EffectData>();
		string[] array = AiUtil.Split(str, ';');
		string[] array2 = array;
		foreach (string value in array2)
		{
			string[] array3 = AiUtil.Split(value, '_');
			if (array3.Length == 3)
			{
				EffectData item = default(EffectData);
				item.type1 = Convert.ToInt32(array3[0]);
				item.type2 = Convert.ToInt32(array3[1]);
				List<int> list2 = new List<int>();
				string[] array4 = AiUtil.Split(array3[2], ',');
				string[] array5 = array4;
				foreach (string value2 in array5)
				{
					list2.Add(Convert.ToInt32(value2));
				}
				item.effectIds = list2.ToArray();
				list.Add(item);
			}
		}
		return list;
	}

	public static int GetBackGroundMusic(Color color)
	{
		return m_data.Find((AISpawnDataStory ret) => MatchMap(ret, color))?.mBgSound ?? 0;
	}

	public static int GetRandomPathIDFromType(int type, Vector3 position)
	{
		Debug.LogError("[Error]Specis texture not exists now.");
		return -1;
	}

	private static bool MatchMap(AISpawnDataStory map, Color color)
	{
		if (map == null)
		{
			return false;
		}
		return Mathf.Abs(map.mColor.r - color.r) * 255f < 5f && Mathf.Abs(map.mColor.g - color.g) * 255f < 5f && Mathf.Abs(map.mColor.b - color.b) * 255f < 5f && Mathf.Abs(map.mColor.a - color.a) * 255f < 5f;
	}
}
