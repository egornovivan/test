using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using UnityEngine;

public class TutorialData
{
	public delegate void OnAddActiveIDEvent(int id);

	public const int BuildingId = 1;

	public const int Building_1Id = 16;

	public const int PlantSolarId = 9;

	public const int RepairMachineId = 8;

	public const int GetOnVehicle = 15;

	public const int ColonyID0 = 20;

	public const int ColonyID1 = 4;

	public const int ColonyID2 = 5;

	public const int ColonyID3 = 6;

	public const int ColonyID4 = 7;

	public const int ColonyID5 = 10;

	public const int ColonyID6 = 17;

	public const int ColonyID7 = 18;

	public const int ColonyID8 = 19;

	public const string HelpTexChinesePath = "Texture2d/HelpTex_Chinese/";

	public const string HelpTexEnglishPath = "Texture2d/HelpTex_English/";

	public static int[] SkillIDs = new int[5] { 21, 22, 23, 24, 25 };

	public int mID;

	public int mType;

	public string mTexName;

	private int m_ContentID;

	public static Dictionary<int, TutorialData> s_tblTutorialData;

	public static List<int> m_ActiveIDList = new List<int>();

	public string mContent => PELocalization.GetString(m_ContentID);

	public static event OnAddActiveIDEvent e_OnAddActiveID;

	static TutorialData()
	{
		TutorialData.e_OnAddActiveID = null;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("handphone");
		s_tblTutorialData = new Dictionary<int, TutorialData>();
		while (sqliteDataReader.Read())
		{
			TutorialData tutorialData = new TutorialData();
			tutorialData.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			tutorialData.mType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			tutorialData.m_ContentID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Name")));
			tutorialData.mTexName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Picture"));
			s_tblTutorialData[tutorialData.mID] = tutorialData;
		}
	}

	public static bool AddActiveTutorialID(int _id, bool execEvent = true)
	{
		if (!m_ActiveIDList.Contains(_id) && s_tblTutorialData.ContainsKey(_id))
		{
			m_ActiveIDList.Add(_id);
			if (execEvent && TutorialData.e_OnAddActiveID != null)
			{
				TutorialData.e_OnAddActiveID(_id);
			}
			return true;
		}
		return false;
	}

	public static byte[] Serialize()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(200);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(m_ActiveIDList.Count);
				for (int i = 0; i < m_ActiveIDList.Count; i++)
				{
					binaryWriter.Write(m_ActiveIDList[i]);
				}
			}
			return memoryStream.ToArray();
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return null;
		}
	}

	public static bool Deserialize(byte[] buf)
	{
		m_ActiveIDList.Clear();
		try
		{
			MemoryStream input = new MemoryStream(buf, writable: false);
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int num2 = binaryReader.ReadInt32();
				if (s_tblTutorialData.ContainsKey(num2))
				{
					m_ActiveIDList.Add(num2);
				}
			}
			if (m_ActiveIDList.Contains(1) && !m_ActiveIDList.Contains(16))
			{
				if (s_tblTutorialData.ContainsKey(16))
				{
					m_ActiveIDList.Add(16);
				}
			}
			else if (!m_ActiveIDList.Contains(1) && m_ActiveIDList.Contains(16) && s_tblTutorialData.ContainsKey(1))
			{
				m_ActiveIDList.Add(1);
			}
			if (m_ActiveIDList.Contains(10))
			{
				for (int j = 17; j < 20; j++)
				{
					if (!m_ActiveIDList.Contains(j) && s_tblTutorialData.ContainsKey(j))
					{
						m_ActiveIDList.Add(j);
					}
				}
			}
			if (m_ActiveIDList.Contains(4) && !m_ActiveIDList.Contains(20) && s_tblTutorialData.ContainsKey(20))
			{
				m_ActiveIDList.Add(20);
			}
			return true;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
	}

	public static void Clear()
	{
		m_ActiveIDList.Clear();
	}
}
