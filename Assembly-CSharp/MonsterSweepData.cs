using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MonsterSweepData
{
	public struct MonsterData
	{
		public int typeId;

		public int num;
	}

	public int id;

	public int cdTime;

	public int preTime;

	public int minAngle;

	public int maxAngle;

	public Vector3 spawnPos;

	public int deviation;

	public List<MonsterData> monsData;

	public List<int> plotId;

	public static Dictionary<int, MonsterSweepData> monsterSweepData = new Dictionary<int, MonsterSweepData>();

	public MonsterSweepData()
	{
		monsData = new List<MonsterData>();
		plotId = new List<int>();
	}

	public static AISpawnTDWavesData.TDWaveSpData GetWaveSpData(List<int> tmp, Vector3 endPoint)
	{
		AISpawnTDWavesData.TDWaveSpData tDWaveSpData = new AISpawnTDWavesData.TDWaveSpData();
		foreach (int item in tmp)
		{
			MonsterSweepData monsterSweepData = MonsterSweepData.monsterSweepData[item];
			if (monsterSweepData == null)
			{
				continue;
			}
			tDWaveSpData._dif = 0;
			List<int> list = new List<int>(Array.ConvertAll(tmp.ToArray(), (int ite) => MonsterSweepData.monsterSweepData[ite].cdTime));
			list.Sort();
			tDWaveSpData._timeToCool = list[0];
			tDWaveSpData._timeToStart = 0;
			tDWaveSpData._weight = 0f;
			AISpawnTDWavesData.TDWaveData tDWaveData = new AISpawnTDWavesData.TDWaveData();
			tDWaveData._delayTime = monsterSweepData.preTime;
			tDWaveData._plotID = monsterSweepData.plotId;
			foreach (MonsterData monsDatum in monsterSweepData.monsData)
			{
				if (monsterSweepData.minAngle != 0 || monsterSweepData.maxAngle != 0)
				{
					tDWaveData._maxDegs.Add(monsterSweepData.maxAngle);
					tDWaveData._minDegs.Add(monsterSweepData.minAngle);
				}
				else
				{
					Vector3 vector = monsterSweepData.spawnPos - endPoint;
					int num = (int)(Math.Atan(vector.x / vector.z) / Math.PI * 180.0);
					if (vector.z < 0f)
					{
						num += 180;
					}
					tDWaveData._maxDegs.Add(num + monsterSweepData.deviation);
					tDWaveData._minDegs.Add(num - monsterSweepData.deviation);
				}
				tDWaveData._maxNums.Add(monsDatum.num);
				tDWaveData._minNums.Add(monsDatum.num);
				tDWaveData._monsterTypes.Add(monsDatum.typeId);
			}
			tDWaveSpData._waveDatas.Add(tDWaveData);
		}
		return tDWaveSpData;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MonsterSweep");
		MonsterData item = default(MonsterData);
		while (sqliteDataReader.Read())
		{
			MonsterSweepData monsterSweepData = new MonsterSweepData();
			monsterSweepData.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			monsterSweepData.cdTime = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("cd_time")));
			monsterSweepData.preTime = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PreTime")));
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Angle")).Split('_');
			if (array.Length == 2)
			{
				monsterSweepData.minAngle = Convert.ToInt32(array[0]);
				monsterSweepData.maxAngle = Convert.ToInt32(array[1]);
			}
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsPos"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				monsterSweepData.deviation = Convert.ToInt32(array[1]);
				string[] array2 = array[0].Split(',');
				if (array2.Length == 3)
				{
					float x = Convert.ToSingle(array2[0]);
					float y = Convert.ToSingle(array2[1]);
					float z = Convert.ToSingle(array2[2]);
					monsterSweepData.spawnPos = new Vector3(x, y, z);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Monslist"));
			array = @string.Split(';');
			string[] array3 = array;
			foreach (string text in array3)
			{
				string[] array2 = text.Split('_');
				if (array2.Length == 2)
				{
					item.typeId = Convert.ToInt32(array2[0]);
					item.num = Convert.ToInt32(array2[1]);
					monsterSweepData.monsData.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("plot"));
			array = @string.Split(',');
			string[] array4 = array;
			foreach (string text2 in array4)
			{
				if (!(text2 == "0"))
				{
					monsterSweepData.plotId.Add(Convert.ToInt32(text2));
				}
			}
			MonsterSweepData.monsterSweepData.Add(monsterSweepData.id, monsterSweepData);
		}
	}
}
