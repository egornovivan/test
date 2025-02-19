using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AISpawnAutomatic
{
	private int mID;

	private string mDataString;

	private List<AISpawnWaveData> mData;

	private static Dictionary<int, AISpawnAutomatic> mDataTable = new Dictionary<int, AISpawnAutomatic>();

	public List<AISpawnWaveData> data => mData;

	public static AISpawnAutomatic GetAutomatic(int id)
	{
		return (!mDataTable.ContainsKey(id)) ? null : mDataTable[id];
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MonsterSiege_SpawnId");
		while (sqliteDataReader.Read())
		{
			AISpawnAutomatic aISpawnAutomatic = new AISpawnAutomatic();
			aISpawnAutomatic.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Sp_id")));
			aISpawnAutomatic.mDataString = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Sp_type"));
			aISpawnAutomatic.InitData();
			if (mDataTable.ContainsKey(aISpawnAutomatic.mID))
			{
				Debug.LogError("AISpawnAutomatic data id is error!");
			}
			else
			{
				mDataTable.Add(aISpawnAutomatic.mID, aISpawnAutomatic);
			}
		}
	}

	private void InitData()
	{
		mData = new List<AISpawnWaveData>();
		string[] array = AiUtil.Split(mDataString, ',');
		string[] array2 = array;
		foreach (string value in array2)
		{
			if (!string.IsNullOrEmpty(value))
			{
				mData.Add(AISpawnWaveData.ToWaveData(value, mData.Count));
			}
		}
	}
}
