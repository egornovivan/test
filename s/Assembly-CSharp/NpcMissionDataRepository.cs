using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class NpcMissionDataRepository
{
	public static Dictionary<int, NpcMissionData> dicMissionData = new Dictionary<int, NpcMissionData>();

	public static Dictionary<int, AdNpcData> m_AdRandMisNpcData = new Dictionary<int, AdNpcData>();

	public static NpcMissionData GetMissionData(int npcId)
	{
		if (dicMissionData.ContainsKey(npcId))
		{
			return dicMissionData[npcId];
		}
		return null;
	}

	public static void AddMissionData(int npcId, NpcMissionData data)
	{
		if (!dicMissionData.ContainsKey(npcId))
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
			StoryNpc npc = StoryNpcMgr.GetNpc(npcId);
			if (npc != null)
			{
				npcMissionData.m_Rnpc_ID = npc._prototypeNpc;
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
			AddMissionData(npcId, npcMissionData);
		}
	}

	public static void LoadAdRandMisNpcData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdRandNPCMission");
		m_AdRandMisNpcData.Clear();
		GroupInfo item = default(GroupInfo);
		while (sqliteDataReader.Read())
		{
			AdNpcData adNpcData = new AdNpcData();
			adNpcData.mID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			if (m_AdRandMisNpcData.ContainsKey(adNpcData.mID))
			{
				continue;
			}
			adNpcData.mRnpc_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RNPC_ID")));
			adNpcData.mRecruitQC_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RecruitQC_ID")));
			adNpcData.mArea = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Area")));
			adNpcData.mWild = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Wild")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QC_ID"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				if (array2.Length == 2)
				{
					item.id = Convert.ToInt32(array2[0]);
					item.radius = Convert.ToInt32(array2[1]);
					adNpcData.mQC_IDList.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Colony_RecruitMissionChainID"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != "0")
				{
					adNpcData.m_CSRecruitMissionList.Add(Convert.ToInt32(array[j]));
				}
			}
			m_AdRandMisNpcData[adNpcData.mID] = adNpcData;
		}
	}

	public static void AddReplyMission(int npcid, int id)
	{
		GetMissionData(npcid)?.AddMissionListReply(id);
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

	public static AdNpcData GetAdNpcData(int key)
	{
		if (m_AdRandMisNpcData.ContainsKey(key))
		{
			return m_AdRandMisNpcData[key];
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

	public static void CreateStoryNpc()
	{
		foreach (KeyValuePair<int, NpcMissionData> dicMissionDatum in dicMissionData)
		{
			if (dicMissionDatum.Value == null)
			{
				continue;
			}
			if (dicMissionDatum.Key < 9200 && dicMissionDatum.Key >= 9000)
			{
				StoryNpc npc = StoryNpcMgr.GetNpc(dicMissionDatum.Key);
				if (npc == null)
				{
					if (LogFilter.logDebug)
					{
						Debug.LogError("story npc error npcid = " + dicMissionDatum.Key);
					}
				}
				else
				{
					float scale = UnityEngine.Random.Range(SPTerrainEvent.ModelScaleMin, SPTerrainEvent.ModelScaleMax);
					SPTerrainEvent.CreateNpc(-1, 200, npc._startPoint, npc._prototypeNpc, 5, scale, dicMissionDatum.Key, isStand: false, 0f);
				}
			}
			else if (dicMissionDatum.Key >= 9200 && dicMissionDatum.Key < 10000)
			{
				float scale2 = UnityEngine.Random.Range(SPTerrainEvent.ModelScaleMin, SPTerrainEvent.ModelScaleMax);
				SPTerrainEvent.CreateNpc(-1, 200, dicMissionDatum.Value.m_Pos, dicMissionDatum.Value.m_Rnpc_ID, 4, scale2, dicMissionDatum.Key, isStand: false, 0f);
			}
			else if (LogFilter.logDebug)
			{
				Debug.LogError("story mode had some invaild npc npcid = " + dicMissionDatum.Key);
			}
		}
	}
}
