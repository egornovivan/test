using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class NpcMissionDataRepository
{
	public static Dictionary<int, NpcMissionData> dicMissionData = new Dictionary<int, NpcMissionData>(10);

	public static Dictionary<int, AdNpcData> m_AdRandMisNpcData = new Dictionary<int, AdNpcData>();

	public static void Reset()
	{
		dicMissionData.Clear();
		m_AdRandMisNpcData.Clear();
		LoadData();
	}

	public static NpcMissionData GetMissionData(int npcId)
	{
		if (dicMissionData.ContainsKey(npcId))
		{
			return dicMissionData[npcId];
		}
		return null;
	}

	public static AdNpcData GetAdNpcDataByIdx(int idx)
	{
		int num = 0;
		foreach (KeyValuePair<int, AdNpcData> adRandMisNpcDatum in m_AdRandMisNpcData)
		{
			if (idx == num)
			{
				return adRandMisNpcDatum.Value;
			}
			num++;
		}
		return null;
	}

	public static AdNpcData GetAdNpcData(int adid)
	{
		if (m_AdRandMisNpcData.ContainsKey(adid))
		{
			return m_AdRandMisNpcData[adid];
		}
		return null;
	}

	public static AdNpcData GetAdNpcDataByNpcID(int npcid)
	{
		foreach (KeyValuePair<int, AdNpcData> adRandMisNpcDatum in m_AdRandMisNpcData)
		{
			if (adRandMisNpcDatum.Value.mRnpc_ID == npcid)
			{
				return adRandMisNpcDatum.Value;
			}
		}
		return null;
	}

	public static void AddMissionData(int npcId, NpcMissionData data)
	{
		if (dicMissionData.ContainsKey(npcId))
		{
			dicMissionData[npcId] = data;
		}
		else
		{
			dicMissionData.Add(npcId, data);
		}
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPCMission");
		while (sqliteDataReader.Read())
		{
			NpcMissionData npcMissionData = new NpcMissionData();
			int npcId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("missionlist"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != "0")
				{
					npcMissionData.m_MissionList.Add(Convert.ToInt32(array[i]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("missionlistreply"));
			string[] array2 = @string.Split(',');
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] != "0")
				{
					npcMissionData.AddMissionListReply(Convert.ToInt32(array2[j]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Colony_RecruitMissionID"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] != "0")
				{
					npcMissionData.m_CSRecruitMissionList.Add(Convert.ToInt32(array[k]));
				}
			}
			AddMissionData(npcId, npcMissionData);
		}
		LoadNpcRandomMissionData();
		LoadAdRandMisNpcData();
	}

	private static void LoadNpcRandomMissionData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("RandNPCMission");
		while (sqliteDataReader.Read())
		{
			NpcMissionData npcMissionData = new NpcMissionData();
			npcMissionData.mCurComMisNum = 0;
			int npcId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			npcMissionData.m_Rnpc_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RNPC_ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("natalplace"));
			string[] array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				npcMissionData.m_Pos = new Vector3(x, y, z);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Rmissionlist"));
			if (@string != "0")
			{
				npcMissionData.m_RandomMission = Convert.ToInt32(@string);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("missionlist"));
			array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != "0")
				{
					npcMissionData.m_MissionList.Add(Convert.ToInt32(array[i]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RecruitMissionID"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != "0")
				{
					npcMissionData.m_RecruitMissionList.Add(Convert.ToInt32(array[j]));
				}
			}
			npcMissionData.mCompletedMissionCount = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RecruitMissionNum")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Colony_RecruitMissionID"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] != "0")
				{
					npcMissionData.m_CSRecruitMissionList.Add(Convert.ToInt32(array[k]));
				}
			}
			npcMissionData.m_bRandomNpc = true;
			AddMissionData(npcId, npcMissionData);
		}
	}

	public static void LoadAdRandMisNpcData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdRandNPCMission");
		GroupInfo item = default(GroupInfo);
		while (sqliteDataReader.Read())
		{
			AdNpcData adNpcData = new AdNpcData();
			adNpcData.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			adNpcData.mRnpc_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RNPC_ID")));
			adNpcData.mRecruitQC_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RecruitQC_ID")));
			adNpcData.mArea = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Area")));
			adNpcData.mWild = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Wild")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QC_ID"));
			string[] array = @string.Split('_');
			string[] array2 = array[0].Split(';');
			for (int i = 0; i < array2.Length; i++)
			{
				string[] array3 = array2[i].Split(',');
				if (array3.Length == 2)
				{
					item.id = Convert.ToInt32(array3[0]);
					item.radius = Convert.ToInt32(array3[1]);
					adNpcData.mQC_IDList.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Colony_RecruitMissionChainID"));
			array2 = @string.Split(',');
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] != "0")
				{
					adNpcData.m_CSRecruitMissionList.Add(Convert.ToInt32(array2[j]));
				}
			}
			m_AdRandMisNpcData.Add(adNpcData.mID, adNpcData);
		}
	}

	public static void AddReplyMission(int npcid, int id)
	{
		GetMissionData(npcid)?.AddMissionListReply(id);
	}

	public static List<int> GetAdRandListByWild(int wild)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, AdNpcData> adRandMisNpcDatum in m_AdRandMisNpcData)
		{
			if (adRandMisNpcDatum.Value.mWild == wild)
			{
				list.Add(adRandMisNpcDatum.Key);
			}
		}
		return list;
	}

	public static int GetRNpcId(int id)
	{
		if (m_AdRandMisNpcData.ContainsKey(id))
		{
			return m_AdRandMisNpcData[id].mRnpc_ID;
		}
		return -1;
	}
}
