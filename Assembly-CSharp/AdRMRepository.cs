using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

public class AdRMRepository
{
	public static Dictionary<int, TypeMonsterData> m_AdTypeMonster = new Dictionary<int, TypeMonsterData>();

	public static Dictionary<int, TypeCollectData> m_AdTypeCollect = new Dictionary<int, TypeCollectData>();

	public static Dictionary<int, TypeFollowData> m_AdTypeFollow = new Dictionary<int, TypeFollowData>();

	public static Dictionary<int, TypeSearchData> m_AdTypeSearch = new Dictionary<int, TypeSearchData>();

	public static Dictionary<int, TypeUseItemData> m_AdTypeUseItem = new Dictionary<int, TypeUseItemData>();

	public static Dictionary<int, TypeMessengerData> m_AdTypeMessenger = new Dictionary<int, TypeMessengerData>();

	public static Dictionary<int, TowerDefendsInfoData> m_AdTDInfoMap = new Dictionary<int, TowerDefendsInfoData>();

	public static Dictionary<int, TypeTowerDefendsData> m_AdTypeTowerDefends = new Dictionary<int, TypeTowerDefendsData>();

	public static Dictionary<int, MissionCommonData> m_AdRandMisMap = new Dictionary<int, MissionCommonData>();

	public static Dictionary<int, RandomField> m_AdRandomFieldMap = new Dictionary<int, RandomField>();

	public static Dictionary<int, AdRandomGroup> m_AdRandomGroup = new Dictionary<int, AdRandomGroup>();

	public static AdRandomGroup GetAdRandomGroup(int id)
	{
		if (m_AdRandomGroup.ContainsKey(id))
		{
			return m_AdRandomGroup[id];
		}
		return null;
	}

	public static MissionCommonData GetAdRandomMission(int id)
	{
		if (m_AdRandMisMap.ContainsKey(id))
		{
			return m_AdRandMisMap[id];
		}
		return null;
	}

	public static bool HasAdRandomMission(int misid)
	{
		return m_AdRandMisMap.ContainsKey(misid);
	}

	public static void AddAdTypeMonsterData(int id, TypeMonsterData data)
	{
		if (!m_AdTypeMonster.ContainsKey(id))
		{
			m_AdTypeMonster.Add(id, data);
		}
	}

	public static TypeMonsterData GetAdTypeMonsterData(int MissionID)
	{
		if (!m_AdTypeMonster.ContainsKey(MissionID))
		{
			return null;
		}
		return m_AdTypeMonster[MissionID];
	}

	public static void AddAdTypeCollectData(int id, TypeCollectData data)
	{
		if (!m_AdTypeCollect.ContainsKey(id))
		{
			m_AdTypeCollect.Add(id, data);
		}
	}

	public static TypeCollectData GetAdTypeCollectData(int MissionID)
	{
		if (!m_AdTypeCollect.ContainsKey(MissionID))
		{
			return null;
		}
		return m_AdTypeCollect[MissionID];
	}

	public static void AddAdTypeFollowData(int id, TypeFollowData data)
	{
		if (!m_AdTypeFollow.ContainsKey(id))
		{
			m_AdTypeFollow.Add(id, data);
		}
	}

	public static TypeFollowData GetAdTypeFollowData(int MissionID)
	{
		if (!m_AdTypeFollow.ContainsKey(MissionID))
		{
			return null;
		}
		return m_AdTypeFollow[MissionID];
	}

	public static void AddAdTypeSearchData(int id, TypeSearchData data)
	{
		if (!m_AdTypeSearch.ContainsKey(id))
		{
			m_AdTypeSearch.Add(id, data);
		}
	}

	public static TypeSearchData GetAdTypeSearchData(int MissionID)
	{
		if (!m_AdTypeSearch.ContainsKey(MissionID))
		{
			return null;
		}
		return m_AdTypeSearch[MissionID];
	}

	public static void AddAdTypeUseItemData(int id, TypeUseItemData data)
	{
		if (!m_AdTypeUseItem.ContainsKey(id))
		{
			m_AdTypeUseItem.Add(id, data);
		}
	}

	public static TypeUseItemData GetAdTypeUseItemData(int MissionID)
	{
		if (!m_AdTypeUseItem.ContainsKey(MissionID))
		{
			return null;
		}
		return m_AdTypeUseItem[MissionID];
	}

	public static void AddAdTypeMessengerData(int id, TypeMessengerData data)
	{
		if (!m_AdTypeMessenger.ContainsKey(id))
		{
			m_AdTypeMessenger.Add(id, data);
		}
	}

	public static TypeMessengerData GetAdTypeMessengerData(int MissionID)
	{
		if (!m_AdTypeMessenger.ContainsKey(MissionID))
		{
			return null;
		}
		return m_AdTypeMessenger[MissionID];
	}

	public static void AddAdTypeTowerDefendsData(int id, TypeTowerDefendsData data)
	{
		if (!m_AdTypeTowerDefends.ContainsKey(id))
		{
			m_AdTypeTowerDefends.Add(id, data);
		}
	}

	public static TypeTowerDefendsData GetAdTypeTowerDefendsData(int MissionID)
	{
		if (!m_AdTypeTowerDefends.ContainsKey(MissionID))
		{
			return null;
		}
		return m_AdTypeTowerDefends[MissionID];
	}

	public static int GetRandomMission(int qcid, int groupidx)
	{
		AdRandomGroup adRandomGroup = GetAdRandomGroup(qcid);
		if (adRandomGroup == null)
		{
			return 0;
		}
		if (!adRandomGroup.m_GroupList.ContainsKey(groupidx))
		{
			return 0;
		}
		List<GroupInfo> list = adRandomGroup.m_GroupList[groupidx];
		if (list.Count == 0)
		{
			return 0;
		}
		int radius = list[list.Count - 1].radius;
		int num = UnityEngine.Random.Range(0, radius);
		for (int i = 0; i < list.Count; i++)
		{
			GroupInfo groupInfo = list[i];
			if (num < groupInfo.radius)
			{
				return groupInfo.id;
			}
		}
		return 0;
	}

	public static int GetRandomMission(NpcMissionData missionData)
	{
		AdRandomGroup adRandomGroup = GetAdRandomGroup(missionData.m_QCID);
		if (adRandomGroup == null)
		{
			return 0;
		}
		if (missionData.m_CurMissionGroup > 11)
		{
			missionData.m_CurGroupTimes++;
			if (!adRandomGroup.m_GroupList.ContainsKey(missionData.m_CurMissionGroup))
			{
				missionData.m_CurMissionGroup = -1;
			}
			if (adRandomGroup.m_FinishTimes == 0)
			{
				missionData.m_CurMissionGroup = -1;
			}
			if (adRandomGroup.m_FinishTimes == -1)
			{
				missionData.m_CurMissionGroup = 1;
			}
			if (adRandomGroup.m_FinishTimes > 0)
			{
				if (adRandomGroup.m_FinishTimes > missionData.m_CurGroupTimes)
				{
					missionData.m_CurMissionGroup = 1;
				}
				else
				{
					missionData.m_CurMissionGroup = -1;
				}
			}
		}
		if (missionData.m_CurMissionGroup != -1)
		{
			return GetRandomMission(missionData.m_QCID, missionData.m_CurMissionGroup);
		}
		return 0;
	}

	public static void LoadAdTypeMonster()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_KillMonster");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		NpcType item = default(NpcType);
		while (sqliteDataReader.Read())
		{
			TypeMonsterData typeMonsterData = new TypeMonsterData();
			typeMonsterData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeMonsterData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeMonsterData.m_Desc = PELocalization.GetString(strId);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsterID"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('_');
				if (array2.Length == 2)
				{
					item.npcs = new List<int>();
					string[] array3 = array2[0].Split(',');
					for (int j = 0; j < array3.Length; j++)
					{
						item.npcs.Add(Convert.ToInt32(array3[j]));
					}
					item.type = Convert.ToInt32(array2[1]);
					typeMonsterData.m_MonsterList.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetPos"));
			array = @string.Split('_');
			if (array.Length == 4)
			{
				AdMissionRand adMissionRand = default(AdMissionRand);
				typeMonsterData.m_mr.refertoType = (ReferToType)Convert.ToInt32(array[0]);
				typeMonsterData.m_mr.referToID = Convert.ToInt32(array[1]);
				typeMonsterData.m_mr.radius1 = Convert.ToInt32(array[2]);
				typeMonsterData.m_mr.radius2 = Convert.ToInt32(array[3]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PathIDTemp"));
			array = @string.Split(';');
			for (int k = 0; k < array.Length; k++)
			{
				string[] array2 = array[k].Split('_');
				if (array2.Length == 3)
				{
					CreMons item2 = default(CreMons);
					item2.type = Convert.ToInt32(array2[0]);
					item2.monID = Convert.ToInt32(array2[1]);
					item2.monNum = Convert.ToInt32(array2[2]);
					typeMonsterData.m_CreMonList.Add(item2);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					typeMonsterData.m_ReceiveList.Add(Convert.ToInt32(array[l]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DestroyTown"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				typeMonsterData.m_destroyTown = true;
				typeMonsterData.m_campID = Convert.ToInt32(array[0]);
				typeMonsterData.m_townNum = Convert.ToInt32(array[1]);
			}
			AddAdTypeMonsterData(typeMonsterData.m_TargetID, typeMonsterData);
		}
	}

	public static void LoadAdTypeCollect()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_LootItem");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeCollectData typeCollectData = new TypeCollectData();
			typeCollectData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeCollectData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeCollectData.m_Desc = PELocalization.GetString(strId);
			typeCollectData.m_Type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			typeCollectData.ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeCollectData.ItemNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemNum")));
			typeCollectData.m_TargetItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetItemID")));
			typeCollectData.m_MaxNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MaxNum")));
			typeCollectData.m_Chance = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Chance")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetPos"));
			string[] array = @string.Split(',');
			if (array.Length == 2)
			{
				typeCollectData.m_AdDist = Convert.ToInt32(array[0]);
				typeCollectData.m_AdRadius = Convert.ToInt32(array[1]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					typeCollectData.m_ReceiveList.Add(Convert.ToInt32(array[i]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RandNum"));
			string[] array2 = @string.Split(',');
			if (array2.Length == 3)
			{
				typeCollectData.m_randItemNum[0] = Convert.ToInt32(array2[0]);
				typeCollectData.m_randItemNum[1] = Convert.ToInt32(array2[1]);
				typeCollectData.m_randItemNum[2] = Convert.ToInt32(array2[2]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RandID"));
			array2 = @string.Split(',');
			for (int j = 0; j < array2.Length; j++)
			{
				int num = Convert.ToInt32(array2[j]);
				if (num != 0)
				{
					typeCollectData.m_randItemID.Add(num);
				}
			}
			AddAdTypeCollectData(typeCollectData.m_TargetID, typeCollectData);
		}
	}

	public static void LoadAdTypeFollow()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_HuSong");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		AdNpcInfo adNpcRadius = default(AdNpcInfo);
		AdTalkInfo1 item = default(AdTalkInfo1);
		while (sqliteDataReader.Read())
		{
			TypeFollowData typeFollowData = new TypeFollowData();
			typeFollowData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeFollowData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeFollowData.m_Desc = PELocalization.GetString(strId);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcRadius"));
			string[] array;
			if (@string != "0")
			{
				array = @string.Split(',');
				adNpcRadius.num = 1;
				adNpcRadius.dist = Convert.ToInt32(array[0]);
				if (array.Length == 2)
				{
					adNpcRadius.num = Convert.ToInt32(array[1]);
				}
				typeFollowData.m_AdNpcRadius = adNpcRadius;
			}
			array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Emode")).Split('_');
			if (array.Length == 2)
			{
				typeFollowData.m_EMode = Convert.ToInt32(array[0]);
				typeFollowData.m_isAttack = Convert.ToInt32(array[1]);
			}
			AdMissionRand adDistPos = default(AdMissionRand);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DistPos"));
			array = @string.Split('_');
			if (array.Length == 4)
			{
				adDistPos.refertoType = (ReferToType)Convert.ToInt32(array[0]);
				adDistPos.referToID = Convert.ToInt32(array[1]);
				adDistPos.radius1 = Convert.ToInt32(array[2]);
				adDistPos.radius2 = Convert.ToInt32(array[3]);
			}
			typeFollowData.m_AdDistPos = adDistPos;
			typeFollowData.m_TrackRadius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TrackRadius")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkInfo"));
			if (@string != "0")
			{
				array = @string.Split(',');
				if (array.Length == 2)
				{
					item.time = Convert.ToInt32(array[0]);
					item.talkid = Convert.ToInt32(array[1]);
					typeFollowData.m_AdTalkInfo.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkID"));
			array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "0")
				{
					continue;
				}
				string[] array2 = array[i].Split(',');
				if (array2.Length != 2)
				{
					continue;
				}
				AdTalkInfo adTalkInfo = new AdTalkInfo();
				adTalkInfo.dist = Convert.ToInt32(array2[0]);
				string[] array3 = array2[1].Split('_');
				if (array3.Length != 2)
				{
					continue;
				}
				adTalkInfo.radius = Convert.ToInt32(array3[0]);
				string[] array4 = array2[1].Split('#');
				for (int j = 0; j < array4.Length; j++)
				{
					if (!(array4[j] == "0"))
					{
						adTalkInfo.talkid.Add(Convert.ToInt32(array4[j]));
					}
				}
				typeFollowData.m_AdTalkID.Add(adTalkInfo);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Monster"));
			array = @string.Split(':');
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] == "0")
				{
					continue;
				}
				string[] array2 = array[k].Split('_');
				if (array2.Length == 4)
				{
					MonsterIDNum item2 = default(MonsterIDNum);
					item2.id = Convert.ToInt32(array2[0]);
					item2.num = Convert.ToInt32(array2[1]);
					item2.radius = Convert.ToInt32(array2[2]);
					string[] array3 = array2[3].Split(',');
					if (array3.Length == 3)
					{
						float x = Convert.ToSingle(array3[0]);
						float y = Convert.ToSingle(array3[1]);
						float z = Convert.ToSingle(array3[2]);
						item2.pos = new Vector3(x, y, z);
					}
					typeFollowData.m_Monster.Add(item2);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ComTalkID"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					typeFollowData.m_ComTalkID.Add(Convert.ToInt32(array[l]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("WaitDist"));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				typeFollowData.m_WaitDist.Add(Convert.ToInt32(array[0]));
				typeFollowData.m_WaitDist.Add(Convert.ToInt32(array[1]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				if (!(array[m] == "0"))
				{
					typeFollowData.m_ReceiveList.Add(Convert.ToInt32(array[m]));
				}
			}
			typeFollowData.m_BuildID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("BuildID")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcNum"));
			array = @string.Split(',');
			for (int n = 0; n < array.Length; n++)
			{
				if (!(array[n] == "0"))
				{
					typeFollowData.m_CreateNpcList.Add(Convert.ToInt32(array[n]));
				}
			}
			AddAdTypeFollowData(typeFollowData.m_TargetID, typeFollowData);
		}
	}

	public static void LoadAdTypeSearch()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_EnterArea");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeSearchData typeSearchData = new TypeSearchData();
			typeSearchData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeSearchData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeSearchData.m_Desc = PELocalization.GetString(strId);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DistPos"));
			string[] array = @string.Split('_');
			AdMissionRand mr = default(AdMissionRand);
			if (array.Length == 4)
			{
				mr.refertoType = (ReferToType)Convert.ToInt32(array[0]);
				mr.referToID = Convert.ToInt32(array[1]);
				mr.radius1 = Convert.ToInt32(array[2]);
				mr.radius2 = Convert.ToInt32(array[3]);
			}
			typeSearchData.m_mr = mr;
			typeSearchData.m_TrackRadius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TrackRadius")));
			typeSearchData.m_notForDungeon = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NotForDungeon"))) == 1;
			typeSearchData.m_AdNpcRadius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcRadius")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Prompt"));
			array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "0")
				{
					continue;
				}
				string[] array2 = array[i].Split(',');
				if (array2.Length != 2)
				{
					continue;
				}
				AdTalkInfo adTalkInfo = new AdTalkInfo();
				adTalkInfo.dist = Convert.ToInt32(array2[0]);
				string[] array3 = array2[1].Split('_');
				if (array3.Length != 2)
				{
					continue;
				}
				adTalkInfo.radius = Convert.ToInt32(array3[0]);
				string[] array4 = array2[1].Split('#');
				for (int j = 0; j < array4.Length; j++)
				{
					if (!(array4[j] == "0"))
					{
						adTalkInfo.talkid.Add(Convert.ToInt32(array4[j]));
					}
				}
				typeSearchData.m_AdPrompt.Add(adTalkInfo);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkID"));
			array = @string.Split(';');
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] == "0")
				{
					continue;
				}
				string[] array2 = array[k].Split(',');
				if (array2.Length != 2)
				{
					continue;
				}
				AdTalkInfo adTalkInfo2 = new AdTalkInfo();
				adTalkInfo2.dist = Convert.ToInt32(array2[0]);
				string[] array3 = array2[1].Split('_');
				if (array3.Length != 2)
				{
					continue;
				}
				adTalkInfo2.radius = Convert.ToInt32(array3[0]);
				string[] array4 = array2[1].Split('#');
				for (int l = 0; l < array4.Length; l++)
				{
					if (!(array4[l] == "0"))
					{
						adTalkInfo2.talkid.Add(Convert.ToInt32(array4[l]));
					}
				}
				typeSearchData.m_AdTalkID.Add(adTalkInfo2);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				if (!(array[m] == "0"))
				{
					typeSearchData.m_ReceiveList.Add(Convert.ToInt32(array[m]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcNum"));
			array = @string.Split(',');
			for (int n = 0; n < array.Length; n++)
			{
				if (!(array[n] == "0"))
				{
					typeSearchData.m_CreateNpcList.Add(Convert.ToInt32(array[n]));
				}
			}
			AddAdTypeSearchData(typeSearchData.m_TargetID, typeSearchData);
		}
	}

	public static void LoadAdTypeUseItem()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_UseItem");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeUseItemData typeUseItemData = new TypeUseItemData();
			typeUseItemData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeUseItemData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeUseItemData.m_Desc = PELocalization.GetString(strId);
			typeUseItemData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeUseItemData.m_UseNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UseNum")));
			typeUseItemData.m_allowOld = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AllowOld"))) == 1;
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Pos"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				typeUseItemData.m_Type = 1;
				if (!(array[i] == "0"))
				{
					string[] array2 = array[i].Split('_');
					if (array2.Length == 4)
					{
						AdMissionRand adDistPos = default(AdMissionRand);
						adDistPos.refertoType = (ReferToType)Convert.ToInt32(array2[0]);
						adDistPos.referToID = Convert.ToInt32(array2[1]);
						adDistPos.radius1 = Convert.ToInt32(array2[2]);
						adDistPos.radius2 = Convert.ToInt32(array2[3]);
						typeUseItemData.m_AdDistPos = adDistPos;
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UsedPrompt"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (!(array[j] == "0"))
				{
					typeUseItemData.m_UsedPrompt.Add(Convert.ToInt32(array[j]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkID"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					typeUseItemData.m_TalkID.Add(Convert.ToInt32(array[k]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FailPrompt"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					typeUseItemData.m_FailPrompt.Add(Convert.ToInt32(array[l]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				if (!(array[m] == "0"))
				{
					typeUseItemData.m_ReceiveList.Add(Convert.ToInt32(array[m]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ComMission"));
			typeUseItemData.m_comMission = Convert.ToInt32(@string) == 1;
			AddAdTypeUseItemData(typeUseItemData.m_TargetID, typeUseItemData);
		}
	}

	public static void LoadAdTypeMessenger()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_Delivery");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeMessengerData typeMessengerData = new TypeMessengerData();
			typeMessengerData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeMessengerData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeMessengerData.m_Desc = PELocalization.GetString(strId);
			MissionRand adNpcRadius = default(MissionRand);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcRadius"));
			string[] array;
			if (@string != "0")
			{
				array = @string.Split(',');
				adNpcRadius.dist = Convert.ToInt32(array[0]);
				if (array.Length == 2)
				{
					adNpcRadius.radius = Convert.ToInt32(array[1]);
				}
			}
			typeMessengerData.m_AdNpcRadius = adNpcRadius;
			typeMessengerData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeMessengerData.m_ItemNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemNum")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					typeMessengerData.m_ReceiveList.Add(Convert.ToInt32(array[i]));
				}
			}
			AddAdTypeMessengerData(typeMessengerData.m_TargetID, typeMessengerData);
		}
	}

	public static void LoadAdTypeTowerDefends()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_TowerDefence");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeTowerDefendsData typeTowerDefendsData = new TypeTowerDefendsData();
			typeTowerDefendsData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeTowerDefendsData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int result = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeTowerDefendsData.m_Desc = PELocalization.GetString(result);
			typeTowerDefendsData.m_Time = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Time")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Pos"));
			string[] array = @string.Split('_');
			if (array.Length == 2)
			{
				typeTowerDefendsData.m_Pos.type = (TypeTowerDefendsData.PosType)Convert.ToInt32(array[0]);
				if (typeTowerDefendsData.m_Pos.type != 0 && array.Length == 2)
				{
					string[] array2 = array[1].Split(',');
					if (array2.Length == 3)
					{
						float x = Convert.ToSingle(array2[0]);
						float y = Convert.ToSingle(array2[1]);
						float z = Convert.ToSingle(array2[2]);
						typeTowerDefendsData.m_Pos.pos = new Vector3(x, y, z);
					}
					else
					{
						typeTowerDefendsData.m_Pos.id = Convert.ToInt32(array2[0]);
					}
				}
			}
			else if (array.Length == 1)
			{
				typeTowerDefendsData.m_Pos.type = TypeTowerDefendsData.PosType.pos;
				array = @string.Split(',');
				if (array.Length == 3)
				{
					float x = Convert.ToSingle(array[0]);
					float y = Convert.ToSingle(array[1]);
					float z = Convert.ToSingle(array[2]);
					typeTowerDefendsData.m_Pos.pos = new Vector3(x, y, z);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcList"));
			array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0") && int.TryParse(array[i], out result))
				{
					typeTowerDefendsData.m_iNpcList.Add(result);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ObjectList"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (!(array[j] == "0"))
				{
					typeTowerDefendsData.m_ObjectList.Add(Convert.ToInt32(array[j]));
				}
			}
			typeTowerDefendsData.m_Count = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Count")));
			typeTowerDefendsData.m_TdInfoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("td_id")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					typeTowerDefendsData.m_ReceiveList.Add(Convert.ToInt32(array[k]));
				}
			}
			AddAdTypeTowerDefendsData(typeTowerDefendsData.m_TargetID, typeTowerDefendsData);
		}
	}

	public static void LoadRMissionGroup()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuestChain");
		GroupInfo item = default(GroupInfo);
		GroupInfo item2 = default(GroupInfo);
		GroupInfo item3 = default(GroupInfo);
		GroupInfo item4 = default(GroupInfo);
		GroupInfo item5 = default(GroupInfo);
		GroupInfo item6 = default(GroupInfo);
		GroupInfo item7 = default(GroupInfo);
		GroupInfo item8 = default(GroupInfo);
		GroupInfo item9 = default(GroupInfo);
		GroupInfo item10 = default(GroupInfo);
		GroupInfo item11 = default(GroupInfo);
		while (sqliteDataReader.Read())
		{
			AdRandomGroup adRandomGroup = new AdRandomGroup();
			List<GroupInfo> list = new List<GroupInfo>();
			adRandomGroup.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QC_ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PreLimit"));
			string[] array = @string.Split('_');
			if (array.Length == 2)
			{
				adRandomGroup.m_requstAll = Convert.ToInt32(array[0]) == 2;
				string[] array2 = array[1].Split(',');
				for (int i = 0; i < array2.Length; i++)
				{
					adRandomGroup.m_preLimit.Add(Convert.ToInt32(array2[i]));
				}
			}
			adRandomGroup.m_FinishTimes = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FinishTimes")));
			adRandomGroup.m_Area = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Area")));
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Muti")));
			adRandomGroup.m_IsMultiMode = num == 1;
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup1"));
			string[] array3 = string2.Split(';');
			for (int j = 0; j < array3.Length; j++)
			{
				string[] array4 = array3[j].Split(',');
				if (array4.Length == 2)
				{
					item.id = Convert.ToInt32(array4[0]);
					item.radius = Convert.ToInt32(array4[1]);
					list.Add(item);
				}
			}
			adRandomGroup.m_GroupList.Add(1, list);
			List<GroupInfo> list2 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup2"));
			array3 = string2.Split(';');
			for (int k = 0; k < array3.Length; k++)
			{
				string[] array4 = array3[k].Split(',');
				if (array4.Length == 2)
				{
					item2.id = Convert.ToInt32(array4[0]);
					item2.radius = Convert.ToInt32(array4[1]);
					list2.Add(item2);
				}
			}
			adRandomGroup.m_GroupList.Add(2, list2);
			List<GroupInfo> list3 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup3"));
			array3 = string2.Split(';');
			for (int l = 0; l < array3.Length; l++)
			{
				string[] array4 = array3[l].Split(',');
				if (array4.Length == 2)
				{
					item3.id = Convert.ToInt32(array4[0]);
					item3.radius = Convert.ToInt32(array4[1]);
					list3.Add(item3);
				}
			}
			adRandomGroup.m_GroupList.Add(3, list3);
			List<GroupInfo> list4 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup4"));
			array3 = string2.Split(';');
			for (int m = 0; m < array3.Length; m++)
			{
				string[] array4 = array3[m].Split(',');
				if (array4.Length == 2)
				{
					item4.id = Convert.ToInt32(array4[0]);
					item4.radius = Convert.ToInt32(array4[1]);
					list4.Add(item4);
				}
			}
			adRandomGroup.m_GroupList.Add(4, list4);
			List<GroupInfo> list5 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup5"));
			array3 = string2.Split(';');
			for (int n = 0; n < array3.Length; n++)
			{
				string[] array4 = array3[n].Split(',');
				if (array4.Length == 2)
				{
					item5.id = Convert.ToInt32(array4[0]);
					item5.radius = Convert.ToInt32(array4[1]);
					list5.Add(item5);
				}
			}
			adRandomGroup.m_GroupList.Add(5, list5);
			List<GroupInfo> list6 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup6"));
			array3 = string2.Split(';');
			for (int num2 = 0; num2 < array3.Length; num2++)
			{
				string[] array4 = array3[num2].Split(',');
				if (array4.Length == 2)
				{
					item6.id = Convert.ToInt32(array4[0]);
					item6.radius = Convert.ToInt32(array4[1]);
					list6.Add(item6);
				}
			}
			adRandomGroup.m_GroupList.Add(6, list6);
			List<GroupInfo> list7 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup7"));
			array3 = string2.Split(';');
			for (int num3 = 0; num3 < array3.Length; num3++)
			{
				string[] array4 = array3[num3].Split(',');
				if (array4.Length == 2)
				{
					item7.id = Convert.ToInt32(array4[0]);
					item7.radius = Convert.ToInt32(array4[1]);
					list7.Add(item7);
				}
			}
			adRandomGroup.m_GroupList.Add(7, list7);
			List<GroupInfo> list8 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup8"));
			array3 = string2.Split(';');
			for (int num4 = 0; num4 < array3.Length; num4++)
			{
				string[] array4 = array3[num4].Split(',');
				if (array4.Length == 2)
				{
					item8.id = Convert.ToInt32(array4[0]);
					item8.radius = Convert.ToInt32(array4[1]);
					list8.Add(item8);
				}
			}
			adRandomGroup.m_GroupList.Add(8, list8);
			List<GroupInfo> list9 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup9"));
			array3 = string2.Split(';');
			for (int num5 = 0; num5 < array3.Length; num5++)
			{
				string[] array4 = array3[num5].Split(',');
				if (array4.Length == 2)
				{
					item9.id = Convert.ToInt32(array4[0]);
					item9.radius = Convert.ToInt32(array4[1]);
					list9.Add(item9);
				}
			}
			adRandomGroup.m_GroupList.Add(9, list9);
			List<GroupInfo> list10 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup10"));
			array3 = string2.Split(';');
			for (int num6 = 0; num6 < array3.Length; num6++)
			{
				string[] array4 = array3[num6].Split(',');
				if (array4.Length == 2)
				{
					item10.id = Convert.ToInt32(array4[0]);
					item10.radius = Convert.ToInt32(array4[1]);
					list10.Add(item10);
				}
			}
			adRandomGroup.m_GroupList.Add(10, list10);
			List<GroupInfo> list11 = new List<GroupInfo>();
			string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup11"));
			array3 = string2.Split(';');
			for (int num7 = 0; num7 < array3.Length; num7++)
			{
				string[] array4 = array3[num7].Split(',');
				if (array4.Length == 2)
				{
					item11.id = Convert.ToInt32(array4[0]);
					item11.radius = Convert.ToInt32(array4[1]);
					list11.Add(item11);
				}
			}
			adRandomGroup.m_GroupList.Add(11, list11);
			m_AdRandomGroup.Add(adRandomGroup.m_ID, adRandomGroup);
		}
	}

	public static void LoadAdRandMission()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdRandomQuest_List");
		sqliteDataReader.Read();
		TargetListInfo item = default(TargetListInfo);
		MissionIDNum item3 = default(MissionIDNum);
		MissionIDNum item4 = default(MissionIDNum);
		MissionIDNum item5 = default(MissionIDNum);
		MissionIDNum item6 = default(MissionIDNum);
		MissionIDNum item7 = default(MissionIDNum);
		MissionIDNum item8 = default(MissionIDNum);
		TargetListInfo item9 = default(TargetListInfo);
		TargetListInfo item10 = default(TargetListInfo);
		TargetListInfo item11 = default(TargetListInfo);
		TargetListInfo item12 = default(TargetListInfo);
		TargetListInfo item13 = default(TargetListInfo);
		TargetListInfo item14 = default(TargetListInfo);
		while (sqliteDataReader.Read())
		{
			MissionCommonData missionCommonData = new MissionCommonData();
			RandomField randomField = new RandomField();
			missionCommonData.m_Type = MissionType.MissionType_Sub;
			missionCommonData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MissionName")));
			missionCommonData.m_MissionName = PELocalization.GetString(strId);
			missionCommonData.m_MaxNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MaxNum")));
			missionCommonData.m_Type = (MissionType)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetIDList"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					item.listid = new List<int>();
					string[] array2 = array[i].Split(',');
					for (int j = 0; j < array2.Length; j++)
					{
						item.listid.Add(Convert.ToInt32(array2[j]));
						missionCommonData.m_TargetIDList.Add(Convert.ToInt32(array2[j]));
					}
					randomField.TargetIDMap.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Reputation"));
			array = @string.Split('_');
			if (array.Length == 3)
			{
				ReputationPreLimit item2 = default(ReputationPreLimit);
				item2.type = Convert.ToInt32(array[0]);
				item2.min = Convert.ToInt32(array[1]);
				item2.max = Convert.ToInt32(array[2]);
				item2.campID = -1;
				missionCommonData.m_reputationPre.Add(item2);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PreLimit"));
			array = @string.Split(':');
			if (array.Length == 2)
			{
				missionCommonData.m_PreLimit.type = Convert.ToInt32(array[0]);
				string[] array2 = array[1].Split(',');
				for (int k = 0; k < array2.Length; k++)
				{
					missionCommonData.m_PreLimit.idlist.Add(Convert.ToInt32(array2[k]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DemandItem"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					string[] array2 = array[l].Split('_');
					if (array2.Length == 2)
					{
						item3.id = Convert.ToInt32(array2[0]);
						item3.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Get_DemandItem.Add(item3);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DeleteItem"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				if (!(array[m] == "0"))
				{
					string[] array2 = array[m].Split('_');
					if (array2.Length == 2)
					{
						item4.id = Convert.ToInt32(array2[0]);
						item4.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Get_DeleteItem.Add(item4);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_MissionItem"));
			array = @string.Split(',');
			for (int n = 0; n < array.Length; n++)
			{
				if (!(array[n] == "0"))
				{
					string[] array2 = array[n].Split('_');
					if (array2.Length == 2)
					{
						item5.id = Convert.ToInt32(array2[0]);
						item5.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Get_MissionItem.Add(item5);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoSelRewardItem"));
			array = @string.Split(';');
			for (int num = 0; num < array.Length; num++)
			{
				if (array[num] == "0")
				{
					continue;
				}
				string[] array2 = array[num].Split(',');
				List<MissionIDNum> list = new List<MissionIDNum>();
				for (int num2 = 0; num2 < array2.Length; num2++)
				{
					string[] array3 = array2[num2].Split('_');
					if (array3.Length == 2)
					{
						item6.id = Convert.ToInt32(array3[0]);
						item6.num = Convert.ToInt32(array3[1]);
						list.Add(item6);
					}
				}
				if (list.Count != 0)
				{
					randomField.RewardMap.Add(list);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RewardItem"));
			array = @string.Split(',');
			string[] array4 = array;
			foreach (string text in array4)
			{
				string[] array2 = text.Split('_');
				if (array2.Length == 2)
				{
					item7.id = Convert.ToInt32(array2[0]);
					item7.num = Convert.ToInt32(array2[1]);
					randomField.FixedRewardMap.Add(item7);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoRemoveItem"));
			array = @string.Split(',');
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				if (!(array[num4] == "0"))
				{
					string[] array2 = array[num4].Split('_');
					if (array2.Length == 2)
					{
						item8.id = Convert.ToInt32(array2[0]);
						item8.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Com_RemoveItem.Add(item8);
					}
				}
			}
			strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Description")));
			missionCommonData.m_Description = PELocalization.GetString(strId);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("player_talk"));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				missionCommonData.m_PlayerTalk[0] = Convert.ToInt32(array[0]);
				missionCommonData.m_PlayerTalk[1] = Convert.ToInt32(array[1]);
			}
			else if (array.Length == 1 && array[0] != "0")
			{
				missionCommonData.m_PlayerTalk[0] = Convert.ToInt32(array[0]);
				missionCommonData.m_PlayerTalk[1] = 0;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkOP"));
			array = @string.Split(';');
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				if (array[num5] == "0")
				{
					continue;
				}
				item9.listid = new List<int>();
				string[] array2 = array[num5].Split(',');
				for (int num6 = 0; num6 < array2.Length; num6++)
				{
					item9.listid.Add(Convert.ToInt32(array2[num6]));
				}
				if (array.Length == 1)
				{
					for (int num7 = 0; num7 < array2.Length; num7++)
					{
						missionCommonData.m_TalkOP.Add(Convert.ToInt32(array2[num7]));
					}
				}
				randomField.TalkOPMap.Add(item9);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN"));
			array = @string.Split(';');
			for (int num8 = 0; num8 < array.Length; num8++)
			{
				if (array[num8] == "0")
				{
					continue;
				}
				item10.listid = new List<int>();
				string[] array2 = array[num8].Split(',');
				for (int num9 = 0; num9 < array2.Length; num9++)
				{
					item10.listid.Add(Convert.ToInt32(array2[num9]));
				}
				if (array.Length == 1)
				{
					for (int num10 = 0; num10 < array2.Length; num10++)
					{
						missionCommonData.m_TalkIN.Add(Convert.ToInt32(array2[num10]));
					}
				}
				randomField.TalkINMap.Add(item10);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED"));
			array = @string.Split(';');
			for (int num11 = 0; num11 < array.Length; num11++)
			{
				if (array[num11] == "0")
				{
					continue;
				}
				item11.listid = new List<int>();
				string[] array2 = array[num11].Split(',');
				for (int num12 = 0; num12 < array2.Length; num12++)
				{
					item11.listid.Add(Convert.ToInt32(array2[num12]));
				}
				if (array.Length == 1)
				{
					for (int num13 = 0; num13 < array2.Length; num13++)
					{
						missionCommonData.m_TalkED.Add(Convert.ToInt32(array2[num13]));
					}
				}
				randomField.TalkEDMap.Add(item11);
			}
			if (Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bGiveUp"))) == 0)
			{
				missionCommonData.m_bGiveUp = false;
			}
			else
			{
				missionCommonData.m_bGiveUp = true;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkOP_SP"));
			array = @string.Split(';');
			for (int num14 = 0; num14 < array.Length; num14++)
			{
				if (!(array[num14] == "0"))
				{
					item12.listid = new List<int>();
					string[] array2 = array[num14].Split(',');
					for (int num15 = 0; num15 < array2.Length; num15++)
					{
						item12.listid.Add(Convert.ToInt32(array2[num15]));
					}
					randomField.TalkOPSMap.Add(item12);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN_SP"));
			array = @string.Split(';');
			for (int num16 = 0; num16 < array.Length; num16++)
			{
				if (!(array[num16] == "0"))
				{
					item13.listid = new List<int>();
					string[] array2 = array[num16].Split(',');
					for (int num17 = 0; num17 < array2.Length; num17++)
					{
						item13.listid.Add(Convert.ToInt32(array2[num17]));
					}
					randomField.TalkINSMap.Add(item13);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED_SP"));
			array = @string.Split(';');
			for (int num18 = 0; num18 < array.Length; num18++)
			{
				if (!(array[num18] == "0"))
				{
					item14.listid = new List<int>();
					string[] array2 = array[num18].Split(',');
					for (int num19 = 0; num19 < array2.Length; num19++)
					{
						item14.listid.Add(Convert.ToInt32(array2[num19]));
					}
					randomField.TalkEDSMap.Add(item14);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NextQuest"));
			if (@string != "0")
			{
				array = @string.Split(',');
				for (int num20 = 0; num20 < array.Length; num20++)
				{
					missionCommonData.m_EDID.Add(Convert.ToInt32(array[num20]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AutoReply"));
			if (@string.Equals("1"))
			{
				missionCommonData.isAutoReply = true;
			}
			else
			{
				missionCommonData.isAutoReply = false;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RewardSP"));
			missionCommonData.addSpValue = Convert.ToInt32(@string);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AdvPlot"));
			array = @string.Split(',');
			for (int num21 = 0; num21 < array.Length; num21++)
			{
				string[] array2 = array[num21].Split('_');
				if (array2.Length == 2)
				{
					StoryInfo item15 = default(StoryInfo);
					item15.type = (Story_Info)Convert.ToInt32(array2[0]);
					item15.storyid = Convert.ToInt32(array2[1]);
					missionCommonData.m_StoryInfo.Add(item15);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MutexLimit"));
			array = @string.Split(',');
			for (int num22 = 0; num22 < array.Length; num22++)
			{
				if (array[num22] == "0")
				{
					continue;
				}
				if (num22 == 0)
				{
					string[] array5 = array[num22].Split(':');
					if (array5.Length == 2)
					{
						missionCommonData.m_MutexLimit.type = Convert.ToInt32(array5[0]);
						if (array5[1] != "0")
						{
							missionCommonData.m_MutexLimit.idlist.Add(Convert.ToInt32(array5[1]));
						}
					}
					else if (array5.Length == 1 && array.Length == 1 && array5[0] != "0")
					{
						missionCommonData.m_MutexLimit.idlist.Add(Convert.ToInt32(array5[0]));
					}
				}
				else if (array[num22] != "0")
				{
					missionCommonData.m_MutexLimit.idlist.Add(Convert.ToInt32(array[num22]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("GuanLianList"));
			array = @string.Split(',');
			for (int num23 = 0; num23 < array.Length; num23++)
			{
				if (!(array[num23] == "0"))
				{
					missionCommonData.m_GuanLianList.Add(Convert.ToInt32(array[num23]));
				}
			}
			missionCommonData.m_increaseChain = !(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("IncreaseChain")) == "0");
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ChangeReputation"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				missionCommonData.m_changeReputation[0] = 1;
				missionCommonData.m_changeReputation[1] = Convert.ToInt32(array[0]);
				missionCommonData.m_changeReputation[2] = Convert.ToInt32(array[1]);
			}
			randomField.keepItem = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("KeepItem"))) == 1;
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TempLimit"));
			array = @string.Split(',');
			string[] array6 = array;
			foreach (string value in array6)
			{
				int num25 = Convert.ToInt32(value);
				if (num25 != 0)
				{
					missionCommonData.m_tempLimit.Add(num25);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Dungeon"));
			array = @string.Split('_');
			if (array.Length == 3)
			{
				missionCommonData.creDungeon.effect = true;
				missionCommonData.creDungeon.npcID = Convert.ToInt32(array[0]);
				missionCommonData.creDungeon.radius = Convert.ToInt32(array[1]);
				missionCommonData.creDungeon.dungeonLevel = Convert.ToInt32(array[2]);
			}
			m_AdRandomFieldMap.Add(missionCommonData.m_ID, randomField);
			m_AdRandMisMap.Add(missionCommonData.m_ID, missionCommonData);
		}
	}

	public static void LoadData()
	{
		LoadAdTypeMonster();
		LoadAdTypeCollect();
		LoadAdTypeFollow();
		LoadAdTypeSearch();
		LoadAdTypeUseItem();
		LoadAdTypeMessenger();
		LoadAdTypeTowerDefends();
		LoadRMissionGroup();
		LoadAdRandMission();
	}

	public static void CreateRandomMission(int id, int oidx = -1, int rewardIdx = -1)
	{
		if (MissionManager.Instance.HasMission(id))
		{
			return;
		}
		MissionCommonData adRandomMission = GetAdRandomMission(id);
		if (adRandomMission == null)
		{
			return;
		}
		adRandomMission.m_TargetIDList.Clear();
		adRandomMission.m_TalkOP.Clear();
		adRandomMission.m_TalkIN.Clear();
		adRandomMission.m_TalkED.Clear();
		adRandomMission.m_PromptOP.Clear();
		adRandomMission.m_PromptIN.Clear();
		adRandomMission.m_PromptED.Clear();
		adRandomMission.m_Com_RewardItem.Clear();
		adRandomMission.m_Com_RemoveItem.Clear();
		if (!m_AdRandomFieldMap.ContainsKey(id))
		{
			return;
		}
		RandomField randomField = m_AdRandomFieldMap[id];
		if (randomField.RewardMap.Count != 0)
		{
			int num = ((rewardIdx != -1) ? rewardIdx : UnityEngine.Random.Range(0, randomField.RewardMap.Count));
			if (num < randomField.RewardMap.Count)
			{
				for (int i = 0; i < randomField.RewardMap[num].Count; i++)
				{
					adRandomMission.m_Com_RewardItem.Add(randomField.RewardMap[num][i]);
				}
			}
		}
		foreach (MissionIDNum item2 in randomField.FixedRewardMap)
		{
			adRandomMission.m_Com_RewardItem.Add(item2);
		}
		int num2 = ((oidx != -1) ? oidx : UnityEngine.Random.Range(0, randomField.TargetIDMap.Count));
		if (randomField.TargetIDMap.Count != 0 && randomField.TargetIDMap.Count <= num2)
		{
			return;
		}
		if (randomField.TargetIDMap.Count > num2)
		{
			TargetListInfo targetListInfo = randomField.TargetIDMap[num2];
			MissionIDNum item = default(MissionIDNum);
			for (int j = 0; j < targetListInfo.listid.Count; j++)
			{
				int num3 = targetListInfo.listid[j] / 1000;
				if (num3 == 2)
				{
					TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(targetListInfo.listid[j]);
					if (typeCollectData == null)
					{
						continue;
					}
					if (!randomField.keepItem)
					{
						item.id = typeCollectData.ItemID;
						item.num = typeCollectData.ItemNum;
						adRandomMission.m_Com_RemoveItem.Add(item);
					}
				}
				adRandomMission.m_TargetIDList.Add(targetListInfo.listid[j]);
			}
		}
		if (randomField.TalkOPMap.Count > num2)
		{
			TargetListInfo targetListInfo = randomField.TalkOPMap[num2];
			for (int k = 0; k < targetListInfo.listid.Count; k++)
			{
				adRandomMission.m_TalkOP.Add(targetListInfo.listid[k]);
			}
		}
		if (randomField.TalkINMap.Count > num2)
		{
			TargetListInfo targetListInfo = randomField.TalkINMap[num2];
			for (int l = 0; l < targetListInfo.listid.Count; l++)
			{
				adRandomMission.m_TalkIN.Add(targetListInfo.listid[l]);
			}
		}
		if (randomField.TalkEDMap.Count > num2)
		{
			TargetListInfo targetListInfo = randomField.TalkEDMap[num2];
			for (int m = 0; m < targetListInfo.listid.Count; m++)
			{
				adRandomMission.m_TalkED.Add(targetListInfo.listid[m]);
			}
		}
		if (randomField.TalkOPSMap.Count > num2)
		{
			TargetListInfo targetListInfo = randomField.TalkOPSMap[num2];
			for (int n = 0; n < targetListInfo.listid.Count; n++)
			{
				adRandomMission.m_PromptOP.Add(targetListInfo.listid[n]);
			}
		}
		if (randomField.TalkINSMap.Count > num2)
		{
			TargetListInfo targetListInfo = randomField.TalkINSMap[num2];
			for (int num4 = 0; num4 < targetListInfo.listid.Count; num4++)
			{
				adRandomMission.m_PromptIN.Add(targetListInfo.listid[num4]);
			}
		}
		if (randomField.TalkEDSMap.Count > num2)
		{
			TargetListInfo targetListInfo = randomField.TalkEDSMap[num2];
			for (int num5 = 0; num5 < targetListInfo.listid.Count; num5++)
			{
				adRandomMission.m_PromptED.Add(targetListInfo.listid[num5]);
			}
		}
	}

	public static void Export(BinaryWriter bw)
	{
		PlayerMission playerMission = MissionManager.Instance.m_PlayerMission;
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in playerMission.m_MissionInfo)
		{
			if (m_AdRandMisMap.ContainsKey(item.Key))
			{
				list.Add(item.Key);
			}
		}
		bw.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(list[i]);
			if (missionCommonData == null)
			{
				continue;
			}
			bw.Write(missionCommonData.m_ID);
			bw.Write(missionCommonData.m_MissionName);
			bw.Write(missionCommonData.m_iNpc);
			bw.Write(missionCommonData.m_iReplyNpc);
			bw.Write((int)missionCommonData.m_Type);
			bw.Write(missionCommonData.m_MaxNum);
			bw.Write(missionCommonData.m_Description);
			bw.Write(missionCommonData.m_TargetIDList.Count);
			for (int j = 0; j < missionCommonData.m_TargetIDList.Count; j++)
			{
				int num = missionCommonData.m_TargetIDList[j];
				bw.Write(num);
				switch (MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[j]))
				{
				case TargetType.TargetType_Follow:
				{
					bw.Write(3);
					TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(num);
					if (typeFollowData == null)
					{
						bw.Write(0);
						break;
					}
					bw.Write(1);
					bw.Write(typeFollowData.m_iNpcList.Count);
					for (int l = 0; l < typeFollowData.m_iNpcList.Count; l++)
					{
						bw.Write(typeFollowData.m_iNpcList[l]);
					}
					Serialize.WriteVector3(bw, typeFollowData.m_DistPos);
					break;
				}
				case TargetType.TargetType_Discovery:
				{
					bw.Write(4);
					TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(num);
					if (typeSearchData == null)
					{
						bw.Write(0);
						break;
					}
					bw.Write(1);
					Serialize.WriteVector3(bw, typeSearchData.m_DistPos);
					break;
				}
				case TargetType.TargetType_UseItem:
				{
					bw.Write(5);
					TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(num);
					if (typeUseItemData == null)
					{
						bw.Write(0);
						break;
					}
					bw.Write(1);
					Serialize.WriteVector3(bw, typeUseItemData.m_Pos);
					break;
				}
				case TargetType.TargetType_TowerDif:
				{
					bw.Write(6);
					TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(num);
					if (typeTowerDefendsData == null)
					{
						bw.Write(0);
						break;
					}
					bw.Write(1);
					bw.Write(typeTowerDefendsData.m_iNpcList.Count);
					for (int k = 0; k < typeTowerDefendsData.m_iNpcList.Count; k++)
					{
						bw.Write(typeTowerDefendsData.m_iNpcList[k]);
					}
					Serialize.WriteVector3(bw, typeTowerDefendsData.finallyPos);
					break;
				}
				default:
					bw.Write(0);
					break;
				}
			}
			bw.Write(missionCommonData.m_PlayerTalk.Length);
			for (int m = 0; m < missionCommonData.m_PlayerTalk.Length; m++)
			{
				bw.Write(missionCommonData.m_PlayerTalk[m]);
			}
			bw.Write(missionCommonData.m_Get_DemandItem.Count);
			for (int n = 0; n < missionCommonData.m_Get_DemandItem.Count; n++)
			{
				bw.Write(missionCommonData.m_Get_DemandItem[n].id);
				bw.Write(missionCommonData.m_Get_DemandItem[n].num);
			}
			bw.Write(missionCommonData.m_Get_DeleteItem.Count);
			for (int num2 = 0; num2 < missionCommonData.m_Get_DeleteItem.Count; num2++)
			{
				bw.Write(missionCommonData.m_Get_DeleteItem[num2].id);
				bw.Write(missionCommonData.m_Get_DeleteItem[num2].num);
			}
			bw.Write(missionCommonData.m_Get_MissionItem.Count);
			for (int num3 = 0; num3 < missionCommonData.m_Get_MissionItem.Count; num3++)
			{
				bw.Write(missionCommonData.m_Get_MissionItem[num3].id);
				bw.Write(missionCommonData.m_Get_MissionItem[num3].num);
			}
			bw.Write(missionCommonData.m_Com_RewardItem.Count);
			for (int num4 = 0; num4 < missionCommonData.m_Com_RewardItem.Count; num4++)
			{
				bw.Write(missionCommonData.m_Com_RewardItem[num4].id);
				bw.Write(missionCommonData.m_Com_RewardItem[num4].num);
			}
			bw.Write(missionCommonData.m_Com_SelRewardItem.Count);
			for (int num5 = 0; num5 < missionCommonData.m_Com_SelRewardItem.Count; num5++)
			{
				bw.Write(missionCommonData.m_Com_SelRewardItem[num5].id);
				bw.Write(missionCommonData.m_Com_SelRewardItem[num5].num);
			}
			bw.Write(missionCommonData.m_Com_RemoveItem.Count);
			for (int num6 = 0; num6 < missionCommonData.m_Com_RemoveItem.Count; num6++)
			{
				bw.Write(missionCommonData.m_Com_RemoveItem[num6].id);
				bw.Write(missionCommonData.m_Com_RemoveItem[num6].num);
			}
			bw.Write(missionCommonData.m_TalkOP.Count);
			for (int num7 = 0; num7 < missionCommonData.m_TalkOP.Count; num7++)
			{
				bw.Write(missionCommonData.m_TalkOP[num7]);
			}
			bw.Write(missionCommonData.m_OPID.Count);
			for (int num8 = 0; num8 < missionCommonData.m_OPID.Count; num8++)
			{
				bw.Write(missionCommonData.m_OPID[num8]);
			}
			bw.Write(missionCommonData.m_TalkIN.Count);
			for (int num9 = 0; num9 < missionCommonData.m_TalkIN.Count; num9++)
			{
				bw.Write(missionCommonData.m_TalkIN[num9]);
			}
			bw.Write(missionCommonData.m_INID.Count);
			for (int num10 = 0; num10 < missionCommonData.m_INID.Count; num10++)
			{
				bw.Write(missionCommonData.m_INID[num10]);
			}
			bw.Write(missionCommonData.m_TalkED.Count);
			for (int num11 = 0; num11 < missionCommonData.m_TalkED.Count; num11++)
			{
				bw.Write(missionCommonData.m_TalkED[num11]);
			}
			bw.Write(missionCommonData.m_EDID.Count);
			for (int num12 = 0; num12 < missionCommonData.m_EDID.Count; num12++)
			{
				bw.Write(missionCommonData.m_EDID[num12]);
			}
		}
	}

	public static void Import(byte[] buffer)
	{
		if (buffer.Length == 0)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		int num2 = 0;
		MissionIDNum item = default(MissionIDNum);
		for (int i = 0; i < num; i++)
		{
			int num3 = binaryReader.ReadInt32();
			if (!m_AdRandMisMap.ContainsKey(num3))
			{
				return;
			}
			MissionCommonData missionCommonData = m_AdRandMisMap[num3];
			if (missionCommonData == null)
			{
				continue;
			}
			missionCommonData.m_TargetIDList.Clear();
			missionCommonData.m_TalkOP.Clear();
			missionCommonData.m_TalkIN.Clear();
			missionCommonData.m_TalkED.Clear();
			missionCommonData.m_PromptOP.Clear();
			missionCommonData.m_PromptIN.Clear();
			missionCommonData.m_PromptED.Clear();
			missionCommonData.m_Com_RewardItem.Clear();
			missionCommonData.m_Com_RemoveItem.Clear();
			for (int j = 0; j < missionCommonData.m_PlayerTalk.Length; j++)
			{
				missionCommonData.m_PlayerTalk[j] = 0;
			}
			missionCommonData.m_Get_DemandItem.Clear();
			missionCommonData.m_Get_DeleteItem.Clear();
			missionCommonData.m_Get_MissionItem.Clear();
			missionCommonData.m_Com_RewardItem.Clear();
			missionCommonData.m_Com_SelRewardItem.Clear();
			missionCommonData.m_Com_RemoveItem.Clear();
			missionCommonData.m_TalkOP.Clear();
			missionCommonData.m_OPID.Clear();
			missionCommonData.m_TalkIN.Clear();
			missionCommonData.m_INID.Clear();
			missionCommonData.m_TalkED.Clear();
			missionCommonData.m_EDID.Clear();
			binaryReader.ReadString();
			missionCommonData.m_iNpc = binaryReader.ReadInt32();
			missionCommonData.m_iReplyNpc = binaryReader.ReadInt32();
			NpcMissionDataRepository.AddReplyMission(missionCommonData.m_iReplyNpc, num3);
			missionCommonData.m_Type = (MissionType)binaryReader.ReadInt32();
			missionCommonData.m_MaxNum = binaryReader.ReadInt32();
			binaryReader.ReadString();
			num2 = binaryReader.ReadInt32();
			for (int k = 0; k < num2; k++)
			{
				int num4 = binaryReader.ReadInt32();
				missionCommonData.m_TargetIDList.Add(num4);
				switch (binaryReader.ReadInt32())
				{
				case 3:
				{
					TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(num4);
					typeFollowData.m_iNpcList.Clear();
					int num5 = binaryReader.ReadInt32();
					if (num5 != 1)
					{
						break;
					}
					int num7 = binaryReader.ReadInt32();
					for (int m = 0; m < num7; m++)
					{
						num3 = binaryReader.ReadInt32();
						if (typeFollowData != null && !typeFollowData.m_iNpcList.Contains(num3))
						{
							typeFollowData.m_iNpcList.Add(num3);
						}
					}
					typeFollowData.m_DistPos = Serialize.ReadVector3(binaryReader);
					break;
				}
				case 4:
				{
					TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(num4);
					int num5 = binaryReader.ReadInt32();
					if (num5 == 1)
					{
						Vector3 distPos = Serialize.ReadVector3(binaryReader);
						if (typeSearchData != null)
						{
							typeSearchData.m_DistPos = distPos;
						}
					}
					break;
				}
				case 5:
				{
					TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(num4);
					int num5 = binaryReader.ReadInt32();
					if (num5 == 1)
					{
						Vector3 pos = Serialize.ReadVector3(binaryReader);
						if (typeUseItemData != null)
						{
							typeUseItemData.m_Pos = pos;
						}
					}
					break;
				}
				case 6:
				{
					TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(num4);
					int num5 = binaryReader.ReadInt32();
					if (num5 == 1)
					{
						int num6 = binaryReader.ReadInt32();
						for (int l = 0; l < num6; l++)
						{
							num3 = binaryReader.ReadInt32();
							typeTowerDefendsData?.m_iNpcList.Add(num3);
						}
						typeTowerDefendsData.m_Pos.type = TypeTowerDefendsData.PosType.pos;
						typeTowerDefendsData.m_Pos.pos = Serialize.ReadVector3(binaryReader);
						typeTowerDefendsData.finallyPos = typeTowerDefendsData.m_Pos.pos;
					}
					break;
				}
				}
			}
			num2 = binaryReader.ReadInt32();
			for (int n = 0; n < num2; n++)
			{
				missionCommonData.m_PlayerTalk[n] = binaryReader.ReadInt32();
			}
			num2 = binaryReader.ReadInt32();
			for (int num8 = 0; num8 < num2; num8++)
			{
				item.id = binaryReader.ReadInt32();
				item.num = binaryReader.ReadInt32();
				missionCommonData.m_Get_DemandItem.Add(item);
			}
			num2 = binaryReader.ReadInt32();
			for (int num9 = 0; num9 < num2; num9++)
			{
				item.id = binaryReader.ReadInt32();
				item.num = binaryReader.ReadInt32();
				missionCommonData.m_Get_DeleteItem.Add(item);
			}
			num2 = binaryReader.ReadInt32();
			for (int num10 = 0; num10 < num2; num10++)
			{
				item.id = binaryReader.ReadInt32();
				item.num = binaryReader.ReadInt32();
				missionCommonData.m_Get_MissionItem.Add(item);
			}
			num2 = binaryReader.ReadInt32();
			for (int num11 = 0; num11 < num2; num11++)
			{
				item.id = binaryReader.ReadInt32();
				item.num = binaryReader.ReadInt32();
				missionCommonData.m_Com_RewardItem.Add(item);
			}
			num2 = binaryReader.ReadInt32();
			for (int num12 = 0; num12 < num2; num12++)
			{
				item.id = binaryReader.ReadInt32();
				item.num = binaryReader.ReadInt32();
				missionCommonData.m_Com_SelRewardItem.Add(item);
			}
			num2 = binaryReader.ReadInt32();
			for (int num13 = 0; num13 < num2; num13++)
			{
				item.id = binaryReader.ReadInt32();
				item.num = binaryReader.ReadInt32();
				missionCommonData.m_Com_RemoveItem.Add(item);
			}
			num2 = binaryReader.ReadInt32();
			for (int num14 = 0; num14 < num2; num14++)
			{
				missionCommonData.m_TalkOP.Add(binaryReader.ReadInt32());
			}
			num2 = binaryReader.ReadInt32();
			for (int num15 = 0; num15 < num2; num15++)
			{
				missionCommonData.m_OPID.Add(binaryReader.ReadInt32());
			}
			num2 = binaryReader.ReadInt32();
			for (int num16 = 0; num16 < num2; num16++)
			{
				missionCommonData.m_TalkIN.Add(binaryReader.ReadInt32());
			}
			num2 = binaryReader.ReadInt32();
			for (int num17 = 0; num17 < num2; num17++)
			{
				missionCommonData.m_INID.Add(binaryReader.ReadInt32());
			}
			num2 = binaryReader.ReadInt32();
			for (int num18 = 0; num18 < num2; num18++)
			{
				missionCommonData.m_TalkED.Add(binaryReader.ReadInt32());
			}
			num2 = binaryReader.ReadInt32();
			for (int num19 = 0; num19 < num2; num19++)
			{
				missionCommonData.m_EDID.Add(binaryReader.ReadInt32());
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}
}
