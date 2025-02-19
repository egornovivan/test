using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class CampPatrolData
{
	public int mId;

	public string mDescription;

	public Vector3 mPosition;

	public float mRadius;

	public List<Vector3> mPatrolList = new List<Vector3>();

	public List<string> mPatrolNpc = new List<string>();

	public int mPatrolNum;

	public int mPatrolTimeMin;

	public int mPatrolTimeMax;

	public List<float> mEatTimeMin = new List<float>();

	public List<float> mEatTimeMax = new List<float>();

	public List<float> mTalkTimeMin = new List<float>();

	public List<float> mTalkTimeMax = new List<float>();

	public int mSleepTime;

	public int mWakeupTime;

	public List<int> m_PreLimit = new List<int>();

	public List<int> m_TalkList = new List<int>();

	public static List<CampPatrolData> m_MapCampList;

	public static CampPatrolData GetCampData(int id)
	{
		for (int i = 0; i < m_MapCampList.Count; i++)
		{
			CampPatrolData campPatrolData = m_MapCampList[i];
			if (campPatrolData.mId == id)
			{
				return campPatrolData;
			}
		}
		return null;
	}

	public static void LoadDate()
	{
		m_MapCampList = new List<CampPatrolData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("CampPatrol");
		while (sqliteDataReader.Read())
		{
			CampPatrolData campPatrolData = new CampPatrolData();
			campPatrolData.mId = Convert.ToInt32(sqliteDataReader.GetString(0));
			campPatrolData.mDescription = sqliteDataReader.GetString(1);
			string[] array = sqliteDataReader.GetString(2).Split(',');
			campPatrolData.mPosition = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			campPatrolData.mRadius = Convert.ToSingle(sqliteDataReader.GetString(3));
			string @string = sqliteDataReader.GetString(4);
			string[] array2 = @string.Split(';');
			for (int i = 0; i < array2.Length; i++)
			{
				array = array2[i].Split(',');
				if (array.Length == 3)
				{
					campPatrolData.mPatrolList.Add(new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2])));
				}
			}
			@string = sqliteDataReader.GetString(5);
			array2 = @string.Split(',');
			for (int j = 0; j < array2.Length; j++)
			{
				if (!(array2[j] == "0"))
				{
					campPatrolData.mPatrolNpc.Add(array2[j]);
				}
			}
			campPatrolData.mPatrolNum = Convert.ToInt32(sqliteDataReader.GetString(6));
			@string = sqliteDataReader.GetString(8);
			array2 = @string.Split('_');
			if (array2.Length == 2)
			{
				campPatrolData.mPatrolTimeMin = Convert.ToInt32(array2[0]) * 3600;
				campPatrolData.mPatrolTimeMax = Convert.ToInt32(array2[1]) * 3600;
			}
			@string = sqliteDataReader.GetString(9);
			array2 = @string.Split('_');
			if (array2.Length == 2)
			{
				campPatrolData.mSleepTime = Convert.ToInt32(array2[0]);
				campPatrolData.mWakeupTime = Convert.ToInt32(array2[1]);
			}
			@string = sqliteDataReader.GetString(10);
			array2 = @string.Split(',');
			for (int k = 0; k < array2.Length; k++)
			{
				if (!(array2[k] == "0"))
				{
					string[] array3 = array2[k].Split('_');
					if (array3.Length == 2)
					{
						campPatrolData.mTalkTimeMin.Add(Convert.ToSingle(array3[0]));
						campPatrolData.mTalkTimeMax.Add(Convert.ToSingle(array3[1]));
					}
				}
			}
			@string = sqliteDataReader.GetString(11);
			array2 = @string.Split(',');
			for (int l = 0; l < array2.Length; l++)
			{
				if (!(array2[l] == "0"))
				{
					string[] array3 = array2[l].Split('_');
					if (array3.Length == 2)
					{
						campPatrolData.mEatTimeMin.Add(Convert.ToSingle(array3[0]));
						campPatrolData.mEatTimeMax.Add(Convert.ToSingle(array3[1]));
					}
				}
			}
			@string = sqliteDataReader.GetString(12);
			array2 = @string.Split(';');
			for (int m = 0; m < array2.Length; m++)
			{
				if (array2[m] == "0")
				{
					continue;
				}
				string[] array3 = array2[m].Split('_');
				if (array3.Length == 2)
				{
					string[] array4 = array3[0].Split(',');
					for (int n = 0; n < array4.Length; n++)
					{
						campPatrolData.m_PreLimit.Add(Convert.ToInt32(array4[n]));
					}
					array4 = array3[1].Split(',');
					for (int num = 0; num < array4.Length; num++)
					{
						int item = Convert.ToInt32(array4[num]);
						campPatrolData.m_TalkList.Add(item);
					}
				}
			}
			m_MapCampList.Add(campPatrolData);
		}
	}
}
