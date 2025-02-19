using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MonsterHandbookData
{
	private int m_ID;

	private int[] m_MonsterIDs;

	private int m_ModelID;

	private string m_Name;

	private string m_Description;

	public static Dictionary<int, MonsterHandbookData> AllMonsterHandbookDataDic = new Dictionary<int, MonsterHandbookData>();

	public static Dictionary<int, int[]> AllMonsterIDDic = new Dictionary<int, int[]>();

	public static List<int> ActiveMhDataID = new List<int>();

	public static Action<int> AddMhEvent;

	public static Action GetAllMonsterEvent;

	public int ID => m_ID;

	public int[] MonsterIDs => m_MonsterIDs;

	public int ModelID => m_ModelID;

	public string Name => m_Name;

	public string Description => m_Description;

	public MonsterHandbookData(int id, int[] monsterIDs, int modelID, string name, string description)
	{
		m_ID = id;
		m_MonsterIDs = monsterIDs;
		m_ModelID = modelID;
		m_Name = name;
		m_Description = description;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("book");
		MonsterHandbookData monsterHandbookData = null;
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsterID"));
			int modelID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ModelID")));
			int stringId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Name")));
			int stringId2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Content")));
			string[] array = @string.Split(',');
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Convert.ToInt32(array[i]);
			}
			monsterHandbookData = new MonsterHandbookData(num, array2, modelID, PELocalization.GetString(stringId), PELocalization.GetString(stringId2));
			AllMonsterHandbookDataDic.Add(num, monsterHandbookData);
			AllMonsterIDDic.Add(num, array2);
		}
	}

	public static void AddMhByKilledMonsterID(int monsterID)
	{
		if (AllMonsterHandbookDataDic == null || AllMonsterHandbookDataDic.Count <= 0)
		{
			return;
		}
		int[] array = AllMonsterIDDic.Where((KeyValuePair<int, int[]> a) => a.Value.Contains(monsterID)).ToDictionary((KeyValuePair<int, int[]> k) => k.Key, (KeyValuePair<int, int[]> v) => v.Value).Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!ActiveMhDataID.Contains(array[i]))
			{
				PublicData.Self.bChanged = true;
				ActiveMhDataID.Add(array[i]);
				if (AddMhEvent != null)
				{
					AddMhEvent(array[i]);
				}
				if (ActiveMhDataID.Count == AllMonsterIDDic.Count && GetAllMonsterEvent != null)
				{
					GetAllMonsterEvent();
				}
			}
		}
	}

	public static bool Deserialize(byte[] data)
	{
		ActiveMhDataID.Clear();
		try
		{
			MemoryStream input = new MemoryStream(data, writable: false);
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				int num = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					ActiveMhDataID.Add(binaryReader.ReadInt32());
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogWarning("MonsterHandbookData deserialize error: " + ex);
			}
			return false;
		}
	}

	public static byte[] Serialize()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(200);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(272);
				binaryWriter.Write(ActiveMhDataID.Count);
				for (int i = 0; i < ActiveMhDataID.Count; i++)
				{
					binaryWriter.Write(ActiveMhDataID[i]);
				}
			}
			return memoryStream.ToArray();
		}
		catch (Exception ex)
		{
			if (LogFilter.logError)
			{
				Debug.LogWarning("MonsterHandbookData serialize error: " + ex);
			}
			return null;
		}
	}

	public static void Clear()
	{
		ActiveMhDataID.Clear();
	}
}
