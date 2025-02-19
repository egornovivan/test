using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class AISpawnPath
{
	private int mID;

	private float mDamage;

	private float mMaxHp;

	private string mLandStr;

	private string mWaterStr;

	private string mSkyStr;

	private string mCaveStr;

	private int[] mLand;

	private int[] mWater;

	private int[] mSky;

	private int[] mCave;

	private static Dictionary<int, AISpawnPath> mDataTable = new Dictionary<int, AISpawnPath>();

	public float damage => mDamage;

	public float maxHp => mMaxHp;

	public static int GetPathID(int spid, int pointType)
	{
		if (mDataTable.ContainsKey(spid))
		{
			return mDataTable[spid].GetRandomPathID(pointType);
		}
		Debug.LogError("Can't find AISpawnPath from ID : " + spid);
		return 0;
	}

	public static AISpawnPath GetSpawnPath(int spid)
	{
		if (mDataTable.ContainsKey(spid))
		{
			return mDataTable[spid];
		}
		Debug.LogError("Can't find AISpawnPath from ID : " + spid);
		return null;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MonsterSiegeSpawnSimple");
		while (sqliteDataReader.Read())
		{
			AISpawnPath aISpawnPath = new AISpawnPath();
			aISpawnPath.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sp_id")));
			aISpawnPath.mLandStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("land"));
			aISpawnPath.mWaterStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("water"));
			aISpawnPath.mSkyStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sky"));
			aISpawnPath.mCaveStr = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("hole"));
			aISpawnPath.mDamage = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("dps"));
			aISpawnPath.mMaxHp = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("rhp"));
			aISpawnPath.InitData();
			if (mDataTable.ContainsKey(aISpawnPath.mID))
			{
				Debug.LogError("AISpawnAutomatic data id is error!");
			}
			else
			{
				mDataTable.Add(aISpawnPath.mID, aISpawnPath);
			}
		}
	}

	private void InitData()
	{
		mLand = ToData(mLandStr);
		mWater = ToData(mWaterStr);
		mSky = ToData(mSkyStr);
		mCave = ToData(mCaveStr);
	}

	private int[] ToData(string value)
	{
		List<int> list = new List<int>();
		string[] array = AiUtil.Split(value, ',');
		string[] array2 = array;
		foreach (string value2 in array2)
		{
			list.Add(Convert.ToInt32(value2));
		}
		return list.ToArray();
	}

	private int GetRandomPathID(int pointType)
	{
		return pointType switch
		{
			1 => mLand[UnityEngine.Random.Range(0, mLand.Length)], 
			2 => mWater[UnityEngine.Random.Range(0, mWater.Length)], 
			3 => mCave[UnityEngine.Random.Range(0, mCave.Length)], 
			4 => mSky[UnityEngine.Random.Range(0, mSky.Length)], 
			_ => 0, 
		};
	}
}
