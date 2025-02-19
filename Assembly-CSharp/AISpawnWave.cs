using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AISpawnWave
{
	private int mID;

	private bool mIsPath;

	private string mDataString;

	private List<AISpawnData> mData;

	private static Dictionary<int, AISpawnWave> mDataTable = new Dictionary<int, AISpawnWave>();

	public List<AISpawnData> data => mData;

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MonsterSiege_SpawnTypeId");
		while (sqliteDataReader.Read())
		{
			AISpawnWave aISpawnWave = new AISpawnWave();
			aISpawnWave.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SpType_id")));
			aISpawnWave.mIsPath = Convert.ToBoolean(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("IsPath_Id")));
			aISpawnWave.mDataString = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Sp_type"));
			aISpawnWave.InitData();
			if (mDataTable.ContainsKey(aISpawnWave.mID))
			{
				Debug.LogError("AISpawnWave data id is error!");
			}
			else
			{
				mDataTable.Add(aISpawnWave.mID, aISpawnWave);
			}
		}
	}

	public static AISpawnWave GetWaveData(int id)
	{
		return (!mDataTable.ContainsKey(id)) ? null : mDataTable[id];
	}

	private void InitData()
	{
		mData = new List<AISpawnData>();
		string[] array = AiUtil.Split(mDataString, ',');
		string[] array2 = array;
		foreach (string value in array2)
		{
			if (!string.IsNullOrEmpty(value))
			{
				mData.Add(AISpawnData.ToSpawnData(value, mIsPath));
			}
		}
	}
}
