using System;
using System.Collections.Generic;
using AiAsset;
using ItemAsset;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MutiPlayRandRespository
{
	public static int nextRandomMissionID = 0;

	public static Dictionary<int, MutiPlayMissionRand> m_MissionRand = new Dictionary<int, MutiPlayMissionRand>();

	public static Dictionary<string, RandIdx> m_RandInfo = new Dictionary<string, RandIdx>();

	public static MutiPlayMissionRand GetMissRand(int id)
	{
		return (!m_MissionRand.ContainsKey(id)) ? null : m_MissionRand[id];
	}

	public static void CreateRandomMission()
	{
		MutiPlayMissionRand missRand = GetMissRand(996);
		if (missRand == null)
		{
			return;
		}
		int num = UnityEngine.Random.Range(0, 2);
		string[] array = new string[2] { "Monster Hunter", "Gather" };
		int num2 = 0;
		MissionCommonData missionCommonData = new MissionCommonData();
		missionCommonData.m_ID = 996;
		missionCommonData.m_MissionName = array[num];
		missionCommonData.m_iNpc = 0;
		missionCommonData.m_iReplyNpc = 0;
		missionCommonData.m_Type = MissionType.MissionType_Main;
		missionCommonData.m_MaxNum = 1;
		int num3 = 0;
		if (num == 0)
		{
			TypeMonsterData typeMonsterData = new TypeMonsterData();
			List<int> list = new List<int>();
			num2 = UnityEngine.Random.Range(0, list.Count);
			if (list.Count == 0)
			{
				Debug.Log("idx = " + num2);
			}
			typeMonsterData.m_TargetID = 1100;
			typeMonsterData.m_MonsterID = list[num2];
			typeMonsterData.m_Desc = "KillMonster : " + AiDataBlock.GetAIDataName(typeMonsterData.m_MonsterID);
			num2 = UnityEngine.Random.Range(0, missRand.m_MulKillNum.Count);
			typeMonsterData.m_MonsterNum = missRand.m_MulKillNum[num2];
			num3 = 901;
			MissionRepository.AddTypeMonsterData(typeMonsterData.m_TargetID, typeMonsterData);
			missionCommonData.m_TargetIDList.Add(typeMonsterData.m_TargetID);
		}
		else
		{
			TypeCollectData typeCollectData = new TypeCollectData();
			num2 = UnityEngine.Random.Range(0, missRand.m_MulCollectID.Count);
			typeCollectData.m_TargetID = 2100;
			typeCollectData.ItemID = missRand.m_MulCollectID[num2];
			num2 = UnityEngine.Random.Range(0, missRand.m_MulCollectNum.Count);
			typeCollectData.ItemNum = missRand.m_MulCollectNum[num2];
			typeCollectData.m_Desc = "Gather : " + ItemProto.GetName(typeCollectData.ItemID);
			MissionIDNum item = default(MissionIDNum);
			item.id = typeCollectData.ItemID;
			item.num = typeCollectData.ItemNum;
			missionCommonData.m_Com_RemoveItem.Add(item);
			num3 = 906;
			MissionRepository.AddTypeCollectData(typeCollectData.m_TargetID, typeCollectData);
			missionCommonData.m_TargetIDList.Add(typeCollectData.m_TargetID);
		}
		TalkData talkData = TalkRespository.GetTalkData(num3);
		if (talkData != null)
		{
			missionCommonData.m_Description = talkData.m_Content;
		}
		num2 = UnityEngine.Random.Range(0, missRand.m_MulRD.Count);
		MissionIDNum item2 = default(MissionIDNum);
		item2.id = 30000000;
		item2.num = missRand.m_MulRD[num2];
		missionCommonData.m_Com_RewardItem.Add(item2);
		if (!MissionRepository.m_MissionCommonMap.ContainsKey(missionCommonData.m_ID))
		{
			MissionRepository.m_MissionCommonMap.Add(missionCommonData.m_ID, missionCommonData);
		}
		nextRandomMissionID = missionCommonData.m_ID;
	}

	public static void CreateRandomMission(int id)
	{
		int num = 1;
		if (MissionRepository.m_MissionCommonMap.ContainsKey(id))
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(id);
			if (missionCommonData != null)
			{
				return;
			}
		}
		MutiPlayMissionRand missRand = GetMissRand(id);
		if (missRand == null)
		{
			return;
		}
		if (GameConfig.IsMultiServer)
		{
			num = UnityEngine.Random.Range(1, 3);
		}
		MissionType type = MissionType.MissionType_Main;
		TypeMonsterData typeMonsterData = null;
		TypeCollectData typeCollectData = null;
		TypeMessengerData typeMessengerData = null;
		TypeFollowData typeFollowData = null;
		TypeSearchData typeSearchData = null;
		TypeTowerDefendsData typeTowerDefendsData = null;
		int num2 = 0;
		MissionCommonData missionCommonData2 = new MissionCommonData();
		missionCommonData2.m_ID = id;
		missionCommonData2.m_iNpc = 0;
		missionCommonData2.m_iReplyNpc = 0;
		missionCommonData2.m_Type = type;
		missionCommonData2.m_MaxNum = 1;
		TalkData talkData = TalkRespository.GetTalkData(911);
		if (talkData != null)
		{
			missionCommonData2.m_Description = talkData.m_Content;
		}
		switch (num)
		{
		case 0:
		case 1:
			if (GameConfig.IsMultiServer)
			{
				num2 = 0;
			}
			typeMonsterData.m_TargetID = 1100;
			typeMonsterData.m_MonsterID = missRand.m_MulKillID[num2];
			typeMonsterData.m_MonsterNum = missRand.m_MulKillNum[num2];
			typeMonsterData.m_Desc = "KillMonster : " + AiDataBlock.GetAIDataName(typeMonsterData.m_MonsterID);
			missionCommonData2.m_TargetIDList.Add(typeMonsterData.m_TargetID);
			break;
		case 2:
			if (GameConfig.IsMultiServer)
			{
				num2 = UnityEngine.Random.Range(0, missRand.m_MulCollectID.Count);
			}
			typeCollectData.m_TargetID = 2100;
			typeCollectData.ItemID = missRand.m_MulCollectID[num2];
			num2 = UnityEngine.Random.Range(0, missRand.m_MulCollectNum.Count);
			typeCollectData.ItemNum = missRand.m_MulCollectNum[num2];
			typeCollectData.m_Desc = "Gather : " + ItemProto.GetName(typeCollectData.ItemID);
			missionCommonData2.m_TargetIDList.Add(typeCollectData.m_TargetID);
			break;
		}
		if (num == 5)
		{
			typeTowerDefendsData.m_TargetID = 7100;
			typeTowerDefendsData.m_Time = 5;
			typeTowerDefendsData.m_Desc = missionCommonData2.m_MissionName;
			switch (num2)
			{
			case 1:
				typeTowerDefendsData.m_Count = UnityEngine.Random.Range(3, 6);
				break;
			case 2:
				typeTowerDefendsData.m_Count = UnityEngine.Random.Range(5, 9);
				break;
			default:
				typeTowerDefendsData.m_Count = UnityEngine.Random.Range(8, 13);
				break;
			}
			missionCommonData2.m_TargetIDList.Add(typeTowerDefendsData.m_TargetID);
		}
		MissionIDNum item = default(MissionIDNum);
		switch (num)
		{
		case 0:
		case 1:
		case 2:
			if (GameConfig.IsMultiServer)
			{
				num2 = UnityEngine.Random.Range(0, missRand.m_MulRD.Count);
			}
			item.id = 30000000;
			item.num = missRand.m_MulRD[num2];
			missionCommonData2.m_Com_RewardItem.Add(item);
			break;
		case 3:
			if (GameConfig.IsMultiServer)
			{
				num2 = UnityEngine.Random.Range(0, missRand.m_MulEscort.Count);
			}
			item.id = 30000000;
			item.num = missRand.m_MulEscort[num2];
			missionCommonData2.m_Com_RewardItem.Add(item);
			break;
		case 4:
			if (GameConfig.IsMultiServer)
			{
				num2 = UnityEngine.Random.Range(0, missRand.m_MulExplore.Count);
			}
			item.id = 30000000;
			item.num = missRand.m_MulExplore[num2];
			missionCommonData2.m_Com_RewardItem.Add(item);
			break;
		case 5:
			if (GameConfig.IsMultiServer)
			{
				num2 = UnityEngine.Random.Range(0, missRand.m_MulTD.Count);
			}
			item.id = 30000000;
			item.num = missRand.m_MulTD[num2];
			missionCommonData2.m_Com_RewardItem.Add(item);
			break;
		}
		if (typeMonsterData != null)
		{
			MissionRepository.AddTypeMonsterData(typeMonsterData.m_TargetID, typeMonsterData);
		}
		else if (typeCollectData != null)
		{
			MissionRepository.AddTypeCollectData(typeCollectData.m_TargetID, typeCollectData);
		}
		else if (typeFollowData != null)
		{
			MissionRepository.AddTypeFollowData(typeFollowData.m_TargetID, typeFollowData);
		}
		else if (typeSearchData != null)
		{
			MissionRepository.AddTypeSearchData(typeSearchData.m_TargetID, typeSearchData);
		}
		else if (typeMessengerData != null)
		{
			MissionRepository.AddTypeMessengerData(typeMessengerData.m_TargetID, typeMessengerData);
		}
		else if (typeTowerDefendsData != null)
		{
			MissionRepository.AddTypeTowerDefendsData(typeTowerDefendsData.m_TargetID, typeTowerDefendsData);
		}
		if (!MissionRepository.m_MissionCommonMap.ContainsKey(missionCommonData2.m_ID))
		{
			MissionRepository.m_MissionCommonMap.Add(missionCommonData2.m_ID, missionCommonData2);
		}
	}

	private static void LoadMissRand()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("RmultiMission");
		while (sqliteDataReader.Read())
		{
			MutiPlayMissionRand mutiPlayMissionRand = new MutiPlayMissionRand();
			mutiPlayMissionRand.m_TaskPerson = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("player_num")));
			mutiPlayMissionRand.m_Level = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("tasklevel")));
			mutiPlayMissionRand.MapID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_mapID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_monsterID"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				mutiPlayMissionRand.m_MulKillID.Add(Convert.ToInt32(array[i]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_killNUM"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				mutiPlayMissionRand.m_MulKillNum.Add(Convert.ToInt32(array[j]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_bossID"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				mutiPlayMissionRand.m_MulBossID.Add(Convert.ToInt32(array[k]));
			}
			mutiPlayMissionRand.m_MulBossNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_bossNUM")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_collectID"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				mutiPlayMissionRand.m_MulCollectID.Add(Convert.ToInt32(array[l]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_collectNUM"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				mutiPlayMissionRand.m_MulCollectNum.Add(Convert.ToInt32(array[m]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_escort"));
			array = @string.Split(',');
			for (int n = 0; n < array.Length; n++)
			{
				mutiPlayMissionRand.m_MulEscort.Add(Convert.ToInt32(array[n]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_explore"));
			array = @string.Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				mutiPlayMissionRand.m_MulExplore.Add(Convert.ToInt32(array[num]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_td"));
			array = @string.Split(',');
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				mutiPlayMissionRand.m_MulTD.Add(Convert.ToInt32(array[num2]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("multiplayer_reward"));
			array = @string.Split(',');
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				mutiPlayMissionRand.m_MulRD.Add(Convert.ToInt32(array[num3]));
			}
			m_MissionRand.Add(mutiPlayMissionRand.m_Level, mutiPlayMissionRand);
		}
	}

	public static void LoadData()
	{
		LoadMissRand();
	}
}
