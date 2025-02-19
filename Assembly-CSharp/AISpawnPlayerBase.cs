using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AISpawnPlayerBase
{
	private int mTimerShaft;

	private int mDifficulty;

	private int mArea;

	private int mSpawnID;

	private float mDelayTime;

	private static List<AISpawnPlayerBase> mDataTable = new List<AISpawnPlayerBase>();

	public float delayTime => mDelayTime;

	public int spawnID => mSpawnID;

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MonsterSiege");
		while (sqliteDataReader.Read())
		{
			AISpawnPlayerBase aISpawnPlayerBase = new AISpawnPlayerBase();
			aISpawnPlayerBase.mTimerShaft = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("timeid_upon")));
			aISpawnPlayerBase.mDifficulty = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("dif_id")));
			aISpawnPlayerBase.mArea = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Area_type")));
			aISpawnPlayerBase.mSpawnID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sp_id")));
			aISpawnPlayerBase.mDelayTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("start_time")));
			mDataTable.Add(aISpawnPlayerBase);
		}
	}

	public static AISpawnPlayerBase GetRandomPlayerBase(int timeId, int areaId, int difficultyId)
	{
		AISpawnPlayerBase[] array = mDataTable.FindAll((AISpawnPlayerBase ret) => ret.mTimerShaft == timeId && ret.mArea == areaId && ret.mDifficulty == difficultyId).ToArray();
		if (array.Length > 0)
		{
			return array[UnityEngine.Random.Range(0, array.Length)];
		}
		Debug.LogError("Can't find data : [time = " + timeId + "area = " + areaId + "difficulty" + difficultyId + "]");
		return new AISpawnPlayerBase();
	}
}
