using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using NaturalResAsset;
using UnityEngine;

public class PlantInfo
{
	public int mTypeID;

	public int mItemId;

	public float[] mGrowTime;

	public string[] mModelPath;

	public string[] mDeadModePath;

	public float mDefaultWater;

	public float mDefaultClean;

	public float mWaterDS;

	public float mCleanDS;

	public float[] mWaterLevel;

	public float[] mCleanLevel;

	public float mSize;

	public float mHeight;

	public int mItemGetNum;

	public List<ResItemGot> mItemGetPro;

	private static Dictionary<int, PlantInfo> stbInfoDic;

	public static PlantInfo GetInfo(int ID)
	{
		if (stbInfoDic.ContainsKey(ID))
		{
			return stbInfoDic[ID];
		}
		return null;
	}

	public static PlantInfo GetPlantInfoByItemId(int itemId)
	{
		foreach (PlantInfo value in stbInfoDic.Values)
		{
			if (value.mItemId == itemId)
			{
				return value;
			}
		}
		return null;
	}

	public static Bounds GetPlantBounds(int itemId, Vector3 pos)
	{
		Bounds result = default(Bounds);
		PlantInfo plantInfoByItemId = GetPlantInfoByItemId(itemId);
		if (plantInfoByItemId == null)
		{
			return result;
		}
		result.SetMinMax(new Vector3(-0.5f * plantInfoByItemId.mSize, 0f, -0.5f * plantInfoByItemId.mSize) + pos, new Vector3(0.5f * plantInfoByItemId.mSize, plantInfoByItemId.mHeight, 0.5f * plantInfoByItemId.mSize) + pos);
		return result;
	}

	public static void LoadData()
	{
		stbInfoDic = new Dictionary<int, PlantInfo>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("plant");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			PlantInfo plantInfo = new PlantInfo();
			plantInfo.mTypeID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			plantInfo.mItemId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("seedid")));
			plantInfo.mGrowTime = new float[3];
			for (int i = 0; i < 3; i++)
			{
				plantInfo.mGrowTime[i] = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("time" + (i + 1))));
			}
			plantInfo.mModelPath = new string[3];
			for (int j = 0; j < 3; j++)
			{
				plantInfo.mModelPath[j] = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("model" + (j + 1)));
			}
			plantInfo.mDeadModePath = new string[3];
			for (int k = 0; k < 3; k++)
			{
				plantInfo.mDeadModePath[k] = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("dmodel" + (k + 1)));
			}
			plantInfo.mDefaultWater = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("water0")));
			plantInfo.mWaterDS = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("waterDS")));
			plantInfo.mWaterLevel = new float[2];
			plantInfo.mWaterLevel[0] = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("water1")));
			plantInfo.mWaterLevel[1] = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("water2")));
			plantInfo.mDefaultClean = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("clean0")));
			plantInfo.mCleanDS = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("cleanDS")));
			plantInfo.mCleanLevel = new float[2];
			plantInfo.mCleanLevel[0] = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("clean1")));
			plantInfo.mCleanLevel[1] = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("clean2")));
			plantInfo.mSize = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("size")));
			plantInfo.mHeight = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("height")));
			plantInfo.mItemGetNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("maxgetnum")));
			plantInfo.mItemGetPro = new List<ResItemGot>();
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemget"));
			string[] array = @string.Split(';');
			for (int l = 0; l < array.Length; l++)
			{
				string[] array2 = array[l].Split(',');
				ResItemGot resItemGot = new ResItemGot();
				resItemGot.m_id = Convert.ToInt32(array2[0]);
				resItemGot.m_probablity = Convert.ToSingle(array2[1]);
				plantInfo.mItemGetPro.Add(resItemGot);
			}
			stbInfoDic[plantInfo.mTypeID] = plantInfo;
		}
	}
}
