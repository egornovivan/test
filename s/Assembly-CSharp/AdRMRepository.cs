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

	public static void LoadAdTypeMonster()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdQuest_KillMonster");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		MissionIDNum item = default(MissionIDNum);
		MissionIDNum item2 = default(MissionIDNum);
		while (sqliteDataReader.Read())
		{
			TypeMonsterData typeMonsterData = new TypeMonsterData();
			typeMonsterData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeMonsterData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			typeMonsterData.m_Desc = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc"));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsterID"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('_');
				if (array2.Length == 2)
				{
					item.id = Convert.ToInt32(array2[0]);
					item.num = Convert.ToInt32(array2[1]);
					typeMonsterData.m_MonsterList.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetPos"));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				typeMonsterData.m_AdDist = Convert.ToInt32(array[0]);
				typeMonsterData.m_AdRadius = Convert.ToInt32(array[1]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PathID"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				string[] array2 = array[j].Split('_');
				if (array2.Length == 2)
				{
					item2.id = Convert.ToInt32(array2[0]);
					item2.num = Convert.ToInt32(array2[1]);
					typeMonsterData.m_CreateMonsterList.Add(item2);
				}
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
			typeCollectData.m_Desc = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc"));
			typeCollectData.m_Type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			typeCollectData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeCollectData.m_ItemNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemNum")));
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
			for (int i = 0; i < array2.Length; i++)
			{
				int num = Convert.ToInt32(array2[i]);
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
			typeFollowData.m_Desc = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc"));
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
			typeSearchData.m_Desc = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc"));
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
			typeUseItemData.m_Desc = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc"));
			typeUseItemData.m_Type = 1;
			typeUseItemData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeUseItemData.m_UseNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UseNum")));
			MissionRand missionRand = default(MissionRand);
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
						typeUseItemData.m_Radius = adDistPos.radius2;
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
			typeMessengerData.m_Desc = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc"));
			MissionRand adNpcRadius = default(MissionRand);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcRadius"));
			if (@string != "0")
			{
				string[] array = @string.Split(',');
				adNpcRadius.dist = Convert.ToInt32(array[0]);
				if (array.Length == 2)
				{
					adNpcRadius.radius = Convert.ToInt32(array[1]);
				}
			}
			typeMessengerData.m_AdNpcRadius = adNpcRadius;
			typeMessengerData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeMessengerData.m_ItemNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemNum")));
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
					typeTowerDefendsData.m_NpcList.Add(result);
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
		while (sqliteDataReader.Read())
		{
			AdRandomGroup adRandomGroup = new AdRandomGroup();
			List<GroupInfo> list = new List<GroupInfo>();
			adRandomGroup.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QC_ID")));
			adRandomGroup.m_FinishTimes = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FinishTimes")));
			adRandomGroup.m_Area = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Area")));
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Muti")));
			adRandomGroup.IsMultiMode = num == 1;
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup1"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				if (array2.Length == 2)
				{
					item.id = Convert.ToInt32(array2[0]);
					item.radius = Convert.ToInt32(array2[1]);
					list.Add(item);
				}
			}
			adRandomGroup.m_GroupList.Add(1, list);
			List<GroupInfo> list2 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup2"));
			array = @string.Split(';');
			for (int j = 0; j < array.Length; j++)
			{
				string[] array2 = array[j].Split(',');
				if (array2.Length == 2)
				{
					item2.id = Convert.ToInt32(array2[0]);
					item2.radius = Convert.ToInt32(array2[1]);
					list2.Add(item2);
				}
			}
			adRandomGroup.m_GroupList.Add(2, list2);
			List<GroupInfo> list3 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup3"));
			array = @string.Split(';');
			for (int k = 0; k < array.Length; k++)
			{
				string[] array2 = array[k].Split(',');
				if (array2.Length == 2)
				{
					item3.id = Convert.ToInt32(array2[0]);
					item3.radius = Convert.ToInt32(array2[1]);
					list3.Add(item3);
				}
			}
			adRandomGroup.m_GroupList.Add(3, list3);
			List<GroupInfo> list4 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup4"));
			array = @string.Split(';');
			for (int l = 0; l < array.Length; l++)
			{
				string[] array2 = array[l].Split(',');
				if (array2.Length == 2)
				{
					item4.id = Convert.ToInt32(array2[0]);
					item4.radius = Convert.ToInt32(array2[1]);
					list4.Add(item4);
				}
			}
			adRandomGroup.m_GroupList.Add(4, list4);
			List<GroupInfo> list5 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup5"));
			array = @string.Split(';');
			for (int m = 0; m < array.Length; m++)
			{
				string[] array2 = array[m].Split(',');
				if (array2.Length == 2)
				{
					item5.id = Convert.ToInt32(array2[0]);
					item5.radius = Convert.ToInt32(array2[1]);
					list5.Add(item5);
				}
			}
			adRandomGroup.m_GroupList.Add(5, list5);
			List<GroupInfo> list6 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup6"));
			array = @string.Split(';');
			for (int n = 0; n < array.Length; n++)
			{
				string[] array2 = array[n].Split(',');
				if (array2.Length == 2)
				{
					item6.id = Convert.ToInt32(array2[0]);
					item6.radius = Convert.ToInt32(array2[1]);
					list6.Add(item6);
				}
			}
			adRandomGroup.m_GroupList.Add(6, list6);
			List<GroupInfo> list7 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup7"));
			array = @string.Split(';');
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				string[] array2 = array[num2].Split(',');
				if (array2.Length == 2)
				{
					item7.id = Convert.ToInt32(array2[0]);
					item7.radius = Convert.ToInt32(array2[1]);
					list7.Add(item7);
				}
			}
			adRandomGroup.m_GroupList.Add(7, list7);
			List<GroupInfo> list8 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup8"));
			array = @string.Split(';');
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				string[] array2 = array[num3].Split(',');
				if (array2.Length == 2)
				{
					item8.id = Convert.ToInt32(array2[0]);
					item8.radius = Convert.ToInt32(array2[1]);
					list8.Add(item8);
				}
			}
			adRandomGroup.m_GroupList.Add(8, list8);
			List<GroupInfo> list9 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup9"));
			array = @string.Split(';');
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				string[] array2 = array[num4].Split(',');
				if (array2.Length == 2)
				{
					item9.id = Convert.ToInt32(array2[0]);
					item9.radius = Convert.ToInt32(array2[1]);
					list9.Add(item9);
				}
			}
			adRandomGroup.m_GroupList.Add(9, list9);
			List<GroupInfo> list10 = new List<GroupInfo>();
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("QusetGroup10"));
			array = @string.Split(';');
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				string[] array2 = array[num5].Split(',');
				if (array2.Length == 2)
				{
					item10.id = Convert.ToInt32(array2[0]);
					item10.radius = Convert.ToInt32(array2[1]);
					list10.Add(item10);
				}
			}
			adRandomGroup.m_GroupList.Add(10, list10);
			m_AdRandomGroup.Add(adRandomGroup.m_ID, adRandomGroup);
		}
	}

	public static void LoadAdRandMission()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdRandomQuest_List");
		sqliteDataReader.Read();
		TargetListInfo item = default(TargetListInfo);
		MissionIDNum item2 = default(MissionIDNum);
		MissionIDNum item3 = default(MissionIDNum);
		MissionIDNum item4 = default(MissionIDNum);
		MissionIDNum item5 = default(MissionIDNum);
		MissionIDNum item6 = default(MissionIDNum);
		MissionIDNum item7 = default(MissionIDNum);
		TargetListInfo item8 = default(TargetListInfo);
		TargetListInfo item9 = default(TargetListInfo);
		TargetListInfo item10 = default(TargetListInfo);
		TargetListInfo item11 = default(TargetListInfo);
		TargetListInfo item12 = default(TargetListInfo);
		TargetListInfo item13 = default(TargetListInfo);
		while (sqliteDataReader.Read())
		{
			MissionCommonData missionCommonData = new MissionCommonData();
			RandomField randomField = new RandomField();
			missionCommonData.m_Type = MissionType.MissionType_Sub;
			missionCommonData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			int stringId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MissionName")));
			missionCommonData.m_MissionName = PELocalization.GetString(stringId);
			missionCommonData.m_MaxNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MaxNum")));
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
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DemandItem"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					string[] array2 = array[k].Split('_');
					if (array2.Length == 2)
					{
						item2.id = Convert.ToInt32(array2[0]);
						item2.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Get_DemandItem.Add(item2);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DeleteItem"));
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
						missionCommonData.m_Get_DeleteItem.Add(item3);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_MissionItem"));
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
						missionCommonData.m_Get_MissionItem.Add(item4);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoSelRewardItem"));
			array = @string.Split(';');
			for (int n = 0; n < array.Length; n++)
			{
				if (array[n] == "0")
				{
					continue;
				}
				string[] array2 = array[n].Split(',');
				List<MissionIDNum> list = new List<MissionIDNum>();
				for (int num = 0; num < array2.Length; num++)
				{
					string[] array3 = array2[num].Split('_');
					if (array3.Length == 2)
					{
						item5.id = Convert.ToInt32(array3[0]);
						item5.num = Convert.ToInt32(array3[1]);
						list.Add(item5);
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
					item6.id = Convert.ToInt32(array2[0]);
					item6.num = Convert.ToInt32(array2[1]);
					randomField.FixedRewardMap.Add(item6);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoRemoveItem"));
			array = @string.Split(',');
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				if (!(array[num3] == "0"))
				{
					string[] array2 = array[num3].Split('_');
					if (array2.Length == 2)
					{
						item7.id = Convert.ToInt32(array2[0]);
						item7.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Com_RemoveItem.Add(item7);
					}
				}
			}
			stringId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Description")));
			missionCommonData.m_Description = PELocalization.GetString(stringId);
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
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				if (!(array[num4] == "0"))
				{
					item8.listid = new List<int>();
					string[] array2 = array[num4].Split(',');
					for (int num5 = 0; num5 < array2.Length; num5++)
					{
						item8.listid.Add(Convert.ToInt32(array2[num5]));
					}
					if (item8.listid.Count == 1)
					{
						missionCommonData.m_TalkOP.Add(Convert.ToInt32(array2[0]));
					}
					randomField.TalkOPMap.Add(item8);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN"));
			array = @string.Split(';');
			for (int num6 = 0; num6 < array.Length; num6++)
			{
				if (!(array[num6] == "0"))
				{
					item9.listid = new List<int>();
					string[] array2 = array[num6].Split(',');
					for (int num7 = 0; num7 < array2.Length; num7++)
					{
						item9.listid.Add(Convert.ToInt32(array2[num7]));
					}
					randomField.TalkINMap.Add(item9);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED"));
			array = @string.Split(';');
			for (int num8 = 0; num8 < array.Length; num8++)
			{
				if (!(array[num8] == "0"))
				{
					item10.listid = new List<int>();
					string[] array2 = array[num8].Split(',');
					for (int num9 = 0; num9 < array2.Length; num9++)
					{
						item10.listid.Add(Convert.ToInt32(array2[num9]));
					}
					randomField.TalkEDMap.Add(item10);
				}
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
			for (int num10 = 0; num10 < array.Length; num10++)
			{
				if (!(array[num10] == "0"))
				{
					item11.listid = new List<int>();
					string[] array2 = array[num10].Split(',');
					for (int num11 = 0; num11 < array2.Length; num11++)
					{
						item11.listid.Add(Convert.ToInt32(array2[num11]));
					}
					randomField.TalkOPSMap.Add(item11);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN_SP"));
			array = @string.Split(';');
			for (int num12 = 0; num12 < array.Length; num12++)
			{
				if (!(array[num12] == "0"))
				{
					item12.listid = new List<int>();
					string[] array2 = array[num12].Split(',');
					for (int num13 = 0; num13 < array2.Length; num13++)
					{
						item12.listid.Add(Convert.ToInt32(array2[num13]));
					}
					randomField.TalkINSMap.Add(item12);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED_SP"));
			array = @string.Split(';');
			for (int num14 = 0; num14 < array.Length; num14++)
			{
				if (!(array[num14] == "0"))
				{
					item13.listid = new List<int>();
					string[] array2 = array[num14].Split(',');
					for (int num15 = 0; num15 < array2.Length; num15++)
					{
						item13.listid.Add(Convert.ToInt32(array2[num15]));
					}
					randomField.TalkEDSMap.Add(item13);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NextQuest"));
			if (@string != "0")
			{
				array = @string.Split(',');
				for (int num16 = 0; num16 < array.Length; num16++)
				{
					missionCommonData.m_EDID.Add(Convert.ToInt32(array[num16]));
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
			missionCommonData.m_increaseChain = !(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("IncreaseChain")) == "0");
			randomField.keepItem = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("KeepItem"))) == 1;
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

	public static MissionCommonData CreateRandomMission(int id, ref int idx, ref int rewardIdx, int npcId)
	{
		MissionCommonData adRandomMission = GetAdRandomMission(id);
		if (adRandomMission == null)
		{
			return null;
		}
		if (!m_AdRandomFieldMap.ContainsKey(id))
		{
			return null;
		}
		RandomField randomField = m_AdRandomFieldMap[id];
		if (-1 >= idx || randomField.TargetIDMap.Count <= idx)
		{
			idx = UnityEngine.Random.Range(0, randomField.TargetIDMap.Count);
		}
		MissionCommonData missionCommonData = new MissionCommonData(adRandomMission);
		missionCommonData.m_TargetIDList.Clear();
		missionCommonData.m_TalkOP.Clear();
		missionCommonData.m_TalkIN.Clear();
		missionCommonData.m_TalkED.Clear();
		missionCommonData.m_PromptOP.Clear();
		missionCommonData.m_PromptIN.Clear();
		missionCommonData.m_PromptED.Clear();
		missionCommonData.m_Com_RewardItem.Clear();
		missionCommonData.m_Com_RemoveItem.Clear();
		missionCommonData.m_iNpc = npcId;
		if (randomField.TargetIDMap.Count > idx)
		{
			TargetListInfo targetListInfo = randomField.TargetIDMap[idx];
			MissionIDNum item = default(MissionIDNum);
			for (int i = 0; i < targetListInfo.listid.Count; i++)
			{
				int num = targetListInfo.listid[i] / 1000;
				if (num == 2)
				{
					TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(targetListInfo.listid[i]);
					if (typeCollectData == null)
					{
						continue;
					}
					if (!randomField.keepItem)
					{
						item.id = typeCollectData.m_ItemID;
						item.num = typeCollectData.m_ItemNum;
						missionCommonData.m_Com_RemoveItem.Add(item);
					}
				}
				missionCommonData.m_TargetIDList.Add(targetListInfo.listid[i]);
			}
		}
		if (randomField.TalkOPMap.Count > idx)
		{
			TargetListInfo targetListInfo = randomField.TalkOPMap[idx];
			for (int j = 0; j < targetListInfo.listid.Count; j++)
			{
				missionCommonData.m_TalkOP.Add(targetListInfo.listid[j]);
			}
		}
		if (randomField.TalkINMap.Count > idx)
		{
			TargetListInfo targetListInfo = randomField.TalkINMap[idx];
			for (int k = 0; k < targetListInfo.listid.Count; k++)
			{
				missionCommonData.m_TalkIN.Add(targetListInfo.listid[k]);
			}
		}
		if (randomField.TalkEDMap.Count > idx)
		{
			TargetListInfo targetListInfo = randomField.TalkEDMap[idx];
			for (int l = 0; l < targetListInfo.listid.Count; l++)
			{
				missionCommonData.m_TalkED.Add(targetListInfo.listid[l]);
			}
		}
		if (randomField.TalkOPSMap.Count > idx)
		{
			TargetListInfo targetListInfo = randomField.TalkOPSMap[idx];
			for (int m = 0; m < targetListInfo.listid.Count; m++)
			{
				missionCommonData.m_PromptOP.Add(targetListInfo.listid[m]);
			}
		}
		if (randomField.TalkINSMap.Count > idx)
		{
			TargetListInfo targetListInfo = randomField.TalkINSMap[idx];
			for (int n = 0; n < targetListInfo.listid.Count; n++)
			{
				missionCommonData.m_PromptIN.Add(targetListInfo.listid[n]);
			}
		}
		if (randomField.TalkEDSMap.Count > idx)
		{
			TargetListInfo targetListInfo = randomField.TalkEDSMap[idx];
			for (int num2 = 0; num2 < targetListInfo.listid.Count; num2++)
			{
				missionCommonData.m_PromptED.Add(targetListInfo.listid[num2]);
			}
		}
		if (randomField.RewardMap.Count != 0)
		{
			rewardIdx = UnityEngine.Random.Range(0, randomField.RewardMap.Count);
			if (rewardIdx < randomField.RewardMap.Count)
			{
				for (int num3 = 0; num3 < randomField.RewardMap[rewardIdx].Count; num3++)
				{
					missionCommonData.m_Com_RewardItem.Add(randomField.RewardMap[rewardIdx][num3]);
				}
			}
		}
		foreach (MissionIDNum item2 in randomField.FixedRewardMap)
		{
			missionCommonData.m_Com_RewardItem.Add(item2);
		}
		return missionCommonData;
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
					missionData.m_CurGroupTimes++;
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

	public static bool IsMultiModeGroup(NpcMissionData missionData)
	{
		return GetAdRandomGroup(missionData.m_QCID)?.IsMultiMode ?? false;
	}

	public static void WriteToBytes(MissionCommonData data, BinaryWriter _out)
	{
		_out.Write(data.m_ID);
		_out.Write(data.m_MissionName);
		_out.Write(data.m_iNpc);
		_out.Write(data.m_iReplyNpc);
		_out.Write((int)data.m_Type);
		_out.Write(data.m_MaxNum);
		_out.Write(data.m_Description);
		_out.Write(data.m_TargetIDList.Count);
		for (int i = 0; i < data.m_TargetIDList.Count; i++)
		{
			int num = data.m_TargetIDList[i];
			_out.Write(num);
			switch (MissionRepository.GetTargetType(data.m_TargetIDList[i]))
			{
			case TargetType.TargetType_Follow:
			{
				_out.Write(3);
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(num);
				if (typeFollowData == null)
				{
					_out.Write(0);
					break;
				}
				_out.Write(1);
				_out.Write(typeFollowData.m_iNpcList.Count);
				for (int k = 0; k < typeFollowData.m_iNpcList.Count; k++)
				{
					_out.Write(typeFollowData.m_iNpcList[k]);
				}
				Serialize.WriteVector(_out, typeFollowData.m_DistPos);
				break;
			}
			case TargetType.TargetType_Discovery:
			{
				_out.Write(4);
				TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(num);
				if (typeSearchData == null)
				{
					_out.Write(0);
					break;
				}
				_out.Write(1);
				Serialize.WriteVector(_out, typeSearchData.m_DistPos);
				break;
			}
			case TargetType.TargetType_UseItem:
			{
				_out.Write(5);
				TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(num);
				if (typeUseItemData == null)
				{
					_out.Write(0);
					break;
				}
				_out.Write(1);
				Serialize.WriteVector(_out, typeUseItemData.m_Pos);
				break;
			}
			case TargetType.TargetType_TowerDif:
			{
				_out.Write(6);
				TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(num);
				if (typeTowerDefendsData == null)
				{
					_out.Write(0);
					break;
				}
				_out.Write(1);
				_out.Write(typeTowerDefendsData.m_NpcList.Count);
				for (int j = 0; j < typeTowerDefendsData.m_NpcList.Count; j++)
				{
					_out.Write(typeTowerDefendsData.m_NpcList[j]);
				}
				Serialize.WriteVector(_out, typeTowerDefendsData.finallyPos);
				break;
			}
			default:
				_out.Write(0);
				break;
			}
		}
		_out.Write(data.m_PlayerTalk.Length);
		for (int l = 0; l < data.m_PlayerTalk.Length; l++)
		{
			_out.Write(data.m_PlayerTalk[l]);
		}
		_out.Write(data.m_Get_DemandItem.Count);
		for (int m = 0; m < data.m_Get_DemandItem.Count; m++)
		{
			_out.Write(data.m_Get_DemandItem[m].id);
			_out.Write(data.m_Get_DemandItem[m].num);
		}
		_out.Write(data.m_Get_DeleteItem.Count);
		for (int n = 0; n < data.m_Get_DeleteItem.Count; n++)
		{
			_out.Write(data.m_Get_DeleteItem[n].id);
			_out.Write(data.m_Get_DeleteItem[n].num);
		}
		_out.Write(data.m_Get_MissionItem.Count);
		for (int num2 = 0; num2 < data.m_Get_MissionItem.Count; num2++)
		{
			_out.Write(data.m_Get_MissionItem[num2].id);
			_out.Write(data.m_Get_MissionItem[num2].num);
		}
		_out.Write(data.m_Com_RewardItem.Count);
		for (int num3 = 0; num3 < data.m_Com_RewardItem.Count; num3++)
		{
			_out.Write(data.m_Com_RewardItem[num3].id);
			_out.Write(data.m_Com_RewardItem[num3].num);
		}
		_out.Write(data.m_Com_SelRewardItem.Count);
		for (int num4 = 0; num4 < data.m_Com_SelRewardItem.Count; num4++)
		{
			_out.Write(data.m_Com_SelRewardItem[num4].id);
			_out.Write(data.m_Com_SelRewardItem[num4].num);
		}
		_out.Write(data.m_Com_RemoveItem.Count);
		for (int num5 = 0; num5 < data.m_Com_RemoveItem.Count; num5++)
		{
			_out.Write(data.m_Com_RemoveItem[num5].id);
			_out.Write(data.m_Com_RemoveItem[num5].num);
		}
		_out.Write(data.m_TalkOP.Count);
		for (int num6 = 0; num6 < data.m_TalkOP.Count; num6++)
		{
			_out.Write(data.m_TalkOP[num6]);
		}
		_out.Write(data.m_OPID.Count);
		for (int num7 = 0; num7 < data.m_OPID.Count; num7++)
		{
			_out.Write(data.m_OPID[num7]);
		}
		_out.Write(data.m_TalkIN.Count);
		for (int num8 = 0; num8 < data.m_TalkIN.Count; num8++)
		{
			_out.Write(data.m_TalkIN[num8]);
		}
		_out.Write(data.m_INID.Count);
		for (int num9 = 0; num9 < data.m_INID.Count; num9++)
		{
			_out.Write(data.m_INID[num9]);
		}
		_out.Write(data.m_TalkED.Count);
		for (int num10 = 0; num10 < data.m_TalkED.Count; num10++)
		{
			_out.Write(data.m_TalkED[num10]);
		}
		_out.Write(data.m_EDID.Count);
		for (int num11 = 0; num11 < data.m_EDID.Count; num11++)
		{
			_out.Write(data.m_EDID[num11]);
		}
	}

	public static byte[] Export(int playerId, PlayerMission pm)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in pm.m_MissionInfo)
		{
			if (m_AdRandMisMap.ContainsKey(item.Key))
			{
				list.Add(item.Key);
			}
		}
		binaryWriter.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(playerId, list[i]);
			if (adrmMissionCommonData != null)
			{
				WriteToBytes(adrmMissionCommonData, binaryWriter);
			}
		}
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	public static void Import(byte[] buffer, Dictionary<int, MissionCommonData> dataMap)
	{
		if (buffer.Length == 0)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int key = binaryReader.ReadInt32();
			if (!m_AdRandMisMap.ContainsKey(key))
			{
				return;
			}
			MissionCommonData missionCommonData = m_AdRandMisMap[key];
			if (missionCommonData != null)
			{
				ReadByBytes(missionCommonData, binaryReader);
				dataMap.Add(key, missionCommonData);
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public static void ReadByBytes(MissionCommonData data, BinaryReader _in)
	{
		data.m_TargetIDList.Clear();
		data.m_TalkOP.Clear();
		data.m_TalkIN.Clear();
		data.m_TalkED.Clear();
		data.m_PromptOP.Clear();
		data.m_PromptIN.Clear();
		data.m_PromptED.Clear();
		data.m_Com_RewardItem.Clear();
		data.m_Com_RemoveItem.Clear();
		data.m_MissionName = _in.ReadString();
		data.m_iNpc = _in.ReadInt32();
		data.m_iReplyNpc = _in.ReadInt32();
		NpcMissionDataRepository.AddReplyMission(data.m_iReplyNpc, data.m_ID);
		data.m_Type = (MissionType)_in.ReadInt32();
		data.m_MaxNum = _in.ReadInt32();
		data.m_Description = _in.ReadString();
		int num = _in.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = _in.ReadInt32();
			data.m_TargetIDList.Add(num2);
			switch (_in.ReadInt32())
			{
			case 3:
			{
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(num2);
				int num3 = _in.ReadInt32();
				if (num3 == 1)
				{
					int num5 = _in.ReadInt32();
					for (int k = 0; k < num5; k++)
					{
						int item2 = _in.ReadInt32();
						typeFollowData?.m_iNpcList.Add(item2);
					}
					typeFollowData.m_DistPos = Record.ReadVector3(_in);
				}
				break;
			}
			case 4:
			{
				TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(num2);
				int num3 = _in.ReadInt32();
				if (num3 == 1)
				{
					Vector3 distPos = Record.ReadVector3(_in);
					if (typeSearchData != null)
					{
						typeSearchData.m_DistPos = distPos;
					}
				}
				break;
			}
			case 5:
			{
				TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(num2);
				int num3 = _in.ReadInt32();
				if (num3 == 1)
				{
					Vector3 pos = Record.ReadVector3(_in);
					if (typeUseItemData != null)
					{
						typeUseItemData.m_Pos = pos;
					}
				}
				break;
			}
			case 6:
			{
				TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(num2);
				int num3 = _in.ReadInt32();
				if (num3 == 1)
				{
					int num4 = _in.ReadInt32();
					for (int j = 0; j < num4; j++)
					{
						int item = _in.ReadInt32();
						typeTowerDefendsData?.m_NpcList.Add(item);
					}
					typeTowerDefendsData.m_Pos.type = TypeTowerDefendsData.PosType.pos;
					typeTowerDefendsData.m_Pos.pos = Serialize.ReadVector(_in);
					typeTowerDefendsData.finallyPos = typeTowerDefendsData.m_Pos.pos;
				}
				break;
			}
			}
		}
		num = _in.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			data.m_PlayerTalk[l] = _in.ReadInt32();
		}
		num = _in.ReadInt32();
		MissionIDNum item3 = default(MissionIDNum);
		for (int m = 0; m < num; m++)
		{
			item3.id = _in.ReadInt32();
			item3.num = _in.ReadInt32();
			data.m_Get_DemandItem.Add(item3);
		}
		num = _in.ReadInt32();
		for (int n = 0; n < num; n++)
		{
			item3.id = _in.ReadInt32();
			item3.num = _in.ReadInt32();
			data.m_Get_DeleteItem.Add(item3);
		}
		num = _in.ReadInt32();
		for (int num6 = 0; num6 < num; num6++)
		{
			item3.id = _in.ReadInt32();
			item3.num = _in.ReadInt32();
			data.m_Get_MissionItem.Add(item3);
		}
		num = _in.ReadInt32();
		for (int num7 = 0; num7 < num; num7++)
		{
			item3.id = _in.ReadInt32();
			item3.num = _in.ReadInt32();
			data.m_Com_RewardItem.Add(item3);
		}
		num = _in.ReadInt32();
		for (int num8 = 0; num8 < num; num8++)
		{
			item3.id = _in.ReadInt32();
			item3.num = _in.ReadInt32();
			data.m_Com_SelRewardItem.Add(item3);
		}
		num = _in.ReadInt32();
		for (int num9 = 0; num9 < num; num9++)
		{
			item3.id = _in.ReadInt32();
			item3.num = _in.ReadInt32();
			data.m_Com_RemoveItem.Add(item3);
		}
		num = _in.ReadInt32();
		for (int num10 = 0; num10 < num; num10++)
		{
			data.m_TalkOP.Add(_in.ReadInt32());
		}
		num = _in.ReadInt32();
		for (int num11 = 0; num11 < num; num11++)
		{
			data.m_OPID.Add(_in.ReadInt32());
		}
		num = _in.ReadInt32();
		for (int num12 = 0; num12 < num; num12++)
		{
			data.m_TalkIN.Add(_in.ReadInt32());
		}
		num = _in.ReadInt32();
		for (int num13 = 0; num13 < num; num13++)
		{
			data.m_INID.Add(_in.ReadInt32());
		}
		num = _in.ReadInt32();
		for (int num14 = 0; num14 < num; num14++)
		{
			data.m_TalkED.Add(_in.ReadInt32());
		}
		num = _in.ReadInt32();
		for (int num15 = 0; num15 < num; num15++)
		{
			data.m_EDID.Add(_in.ReadInt32());
		}
	}
}
