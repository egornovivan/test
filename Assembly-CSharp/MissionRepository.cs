using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MissionRepository
{
	public static Dictionary<int, MissionCommonData> m_MissionCommonMap = new Dictionary<int, MissionCommonData>();

	public static Dictionary<int, TypeMonsterData> m_TypeMonster = new Dictionary<int, TypeMonsterData>();

	public static Dictionary<int, TypeCollectData> m_TypeCollect = new Dictionary<int, TypeCollectData>();

	public static Dictionary<int, TypeFollowData> m_TypeFollow = new Dictionary<int, TypeFollowData>();

	public static Dictionary<int, TypeSearchData> m_TypeSearch = new Dictionary<int, TypeSearchData>();

	public static Dictionary<int, TypeUseItemData> m_TypeUseItem = new Dictionary<int, TypeUseItemData>();

	public static Dictionary<int, TypeMessengerData> m_TypeMessenger = new Dictionary<int, TypeMessengerData>();

	public static Dictionary<int, TowerDefendsInfoData> m_TDInfoMap = new Dictionary<int, TowerDefendsInfoData>();

	public static Dictionary<int, TypeTowerDefendsData> m_TypeTowerDefends = new Dictionary<int, TypeTowerDefendsData>();

	public static Dictionary<int, List<int>> m_iNpcMissionMap = new Dictionary<int, List<int>>();

	public static Dictionary<int, List<int>> m_iNpcReplyMissionMap = new Dictionary<int, List<int>>();

	public static void AddMissionCommonData(int id, MissionCommonData data)
	{
		if (!m_MissionCommonMap.ContainsKey(id))
		{
			m_MissionCommonMap.Add(id, data);
		}
	}

	public static MissionCommonData GetMissionCommonData(int MissionID)
	{
		if (!m_MissionCommonMap.ContainsKey(MissionID))
		{
			return null;
		}
		return m_MissionCommonMap[MissionID];
	}

	public static void AddTypeMonsterData(int id, TypeMonsterData data)
	{
		if (!m_TypeMonster.ContainsKey(id))
		{
			m_TypeMonster.Add(id, data);
		}
	}

	public static TypeMonsterData GetTypeMonsterData(int MissionID)
	{
		if (!m_TypeMonster.ContainsKey(MissionID))
		{
			return null;
		}
		return m_TypeMonster[MissionID];
	}

	public static void AddTypeCollectData(int id, TypeCollectData data)
	{
		if (!m_TypeCollect.ContainsKey(id))
		{
			m_TypeCollect.Add(id, data);
		}
	}

	public static TypeCollectData GetTypeCollectData(int MissionID)
	{
		if (!m_TypeCollect.ContainsKey(MissionID))
		{
			return null;
		}
		return m_TypeCollect[MissionID];
	}

	public static void AddTypeFollowData(int id, TypeFollowData data)
	{
		if (!m_TypeFollow.ContainsKey(id))
		{
			m_TypeFollow.Add(id, data);
		}
	}

	public static TypeFollowData GetTypeFollowData(int MissionID)
	{
		if (!m_TypeFollow.ContainsKey(MissionID))
		{
			return null;
		}
		return m_TypeFollow[MissionID];
	}

	public static void AddTypeSearchData(int id, TypeSearchData data)
	{
		if (!m_TypeSearch.ContainsKey(id))
		{
			m_TypeSearch.Add(id, data);
		}
	}

	public static TypeSearchData GetTypeSearchData(int MissionID)
	{
		if (!m_TypeSearch.ContainsKey(MissionID))
		{
			return null;
		}
		return m_TypeSearch[MissionID];
	}

	public static void AddTypeUseItemData(int id, TypeUseItemData data)
	{
		if (!m_TypeUseItem.ContainsKey(id))
		{
			m_TypeUseItem.Add(id, data);
		}
	}

	public static TypeUseItemData GetTypeUseItemData(int MissionID)
	{
		if (!m_TypeUseItem.ContainsKey(MissionID))
		{
			return null;
		}
		return m_TypeUseItem[MissionID];
	}

	public static void AddTypeMessengerData(int id, TypeMessengerData data)
	{
		if (!m_TypeMessenger.ContainsKey(id))
		{
			m_TypeMessenger.Add(id, data);
		}
	}

	public static TypeMessengerData GetTypeMessengerData(int MissionID)
	{
		if (!m_TypeMessenger.ContainsKey(MissionID))
		{
			return null;
		}
		return m_TypeMessenger[MissionID];
	}

	public static void AddTypeTowerDefendsData(int id, TypeTowerDefendsData data)
	{
		if (!m_TypeTowerDefends.ContainsKey(id))
		{
			m_TypeTowerDefends.Add(id, data);
		}
	}

	public static TypeTowerDefendsData GetTypeTowerDefendsData(int MissionID)
	{
		if (!m_TypeTowerDefends.ContainsKey(MissionID))
		{
			return null;
		}
		return m_TypeTowerDefends[MissionID];
	}

	public static void AddTDInfoData(int tdID, TowerDefendsInfoData data)
	{
		if (!m_TDInfoMap.ContainsKey(tdID))
		{
			m_TDInfoMap.Add(tdID, data);
		}
	}

	public static TowerDefendsInfoData GetTDInfoData(int tdID)
	{
		if (!m_TDInfoMap.ContainsKey(tdID))
		{
			return null;
		}
		return m_TDInfoMap[tdID];
	}

	public static void DeleteRandomMissionData(int TargetID)
	{
		if (m_TypeMonster.ContainsKey(TargetID))
		{
			m_TypeMonster.Remove(TargetID);
		}
		if (m_TypeCollect.ContainsKey(TargetID))
		{
			m_TypeCollect.Remove(TargetID);
		}
		if (m_TypeFollow.ContainsKey(TargetID))
		{
			m_TypeFollow.Remove(TargetID);
		}
		if (m_TypeSearch.ContainsKey(TargetID))
		{
			m_TypeSearch.Remove(TargetID);
		}
		if (m_TypeUseItem.ContainsKey(TargetID))
		{
			m_TypeUseItem.Remove(TargetID);
		}
		if (m_TypeMessenger.ContainsKey(TargetID))
		{
			m_TypeMessenger.Remove(TargetID);
		}
		if (m_TypeTowerDefends.ContainsKey(TargetID))
		{
			m_TypeTowerDefends.Remove(TargetID);
		}
	}

	public static string GetMissionNpcListName(int MissionID, bool bspe)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return string.Empty;
		}
		int talkID = ((!bspe) ? missionCommonData.m_PlayerTalk[0] : missionCommonData.m_PlayerTalk[1]);
		TalkData talkData = TalkRespository.GetTalkData(talkID);
		if (talkData == null)
		{
			return string.Empty;
		}
		return talkData.m_Content;
	}

	public static string GetMissionName(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return string.Empty;
		}
		return missionCommonData.m_MissionName;
	}

	public static MissionType GetMissionType(int MissionID)
	{
		return MissionManager.GetMissionCommonData(MissionID)?.m_Type ?? MissionType.MissionType_Unkown;
	}

	public static TargetType GetTargetType(int MissionID)
	{
		return (TargetType)(MissionID / 1000);
	}

	public static bool HasTargetType(int MissionID, TargetType type)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			TargetType targetType = GetTargetType(missionCommonData.m_TargetIDList[i]);
			if (targetType == type)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAutoReplyMission(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.m_iReplyNpc == 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsMainMission(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return true;
		}
		if (missionCommonData.m_Type == MissionType.MissionType_Main)
		{
			return true;
		}
		return false;
	}

	public static bool NotUpdateMisTex(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return true;
		}
		if (missionCommonData.m_Type == MissionType.MissionType_Talk)
		{
			return true;
		}
		return false;
	}

	public static bool HaveTalkOP(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.m_TalkOP.Count <= 0)
		{
			return false;
		}
		return true;
	}

	public static bool HaveTalkIN(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.m_TalkIN.Count <= 0)
		{
			return false;
		}
		return true;
	}

	public static bool HaveTalkED(int MissionID, int TargetID = -1)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.m_TalkED.Count > 0)
		{
			return true;
		}
		if (TargetID == -1)
		{
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				switch (GetTargetType(missionCommonData.m_TargetIDList[i]))
				{
				case TargetType.TargetType_Follow:
				{
					TypeFollowData typeFollowData = GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
					if (typeFollowData == null)
					{
						return false;
					}
					if (typeFollowData.m_ComTalkID.Count > 0)
					{
						return true;
					}
					break;
				}
				case TargetType.TargetType_Discovery:
				{
					TypeSearchData typeSearchData = GetTypeSearchData(missionCommonData.m_TargetIDList[i]);
					if (typeSearchData == null)
					{
						return false;
					}
					if (typeSearchData.m_TalkID.Count > 0)
					{
						return true;
					}
					break;
				}
				}
			}
		}
		else
		{
			switch (GetTargetType(TargetID))
			{
			case TargetType.TargetType_Follow:
			{
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(TargetID);
				if (typeFollowData == null)
				{
					return false;
				}
				if (typeFollowData.m_ComTalkID.Count > 0)
				{
					return true;
				}
				break;
			}
			case TargetType.TargetType_Discovery:
			{
				TypeSearchData typeSearchData = GetTypeSearchData(TargetID);
				if (typeSearchData == null)
				{
					return false;
				}
				if (typeSearchData.m_TalkID.Count > 0)
				{
					return true;
				}
				break;
			}
			}
		}
		return false;
	}

	public static void AddNpcMissionMap(int npcid, int id)
	{
		if (m_iNpcMissionMap.ContainsKey(npcid))
		{
			m_iNpcMissionMap[npcid].Add(id);
			return;
		}
		List<int> list = new List<int>();
		list.Add(id);
		m_iNpcMissionMap.Add(npcid, list);
	}

	public static void AddNpcReplyMissionMap(int npcid, int id)
	{
		if (m_iNpcReplyMissionMap.ContainsKey(npcid))
		{
			m_iNpcReplyMissionMap[npcid].Add(id);
			return;
		}
		List<int> list = new List<int>();
		list.Add(id);
		m_iNpcReplyMissionMap.Add(npcid, list);
	}

	public static void LoadMissionCommon()
	{
		SqliteDataReader sqliteDataReader = null;
		sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_List");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		MissionIDNum item = default(MissionIDNum);
		MissionIDNum item2 = default(MissionIDNum);
		MissionIDNum item3 = default(MissionIDNum);
		MissionIDNum item4 = default(MissionIDNum);
		MissionIDNum item5 = default(MissionIDNum);
		MissionIDNum item6 = default(MissionIDNum);
		MissionIDNum item7 = default(MissionIDNum);
		StoryInfo item8 = default(StoryInfo);
		NpcType item11 = default(NpcType);
		while (sqliteDataReader.Read())
		{
			MissionCommonData missionCommonData = new MissionCommonData();
			missionCommonData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MissionName")));
			missionCommonData.m_MissionName = PELocalization.GetString(strId);
			missionCommonData.m_iNpc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Npc")));
			missionCommonData.m_iReplyNpc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReplyNpc")));
			AddNpcMissionMap(missionCommonData.m_iNpc, missionCommonData.m_ID);
			AddNpcReplyMissionMap(missionCommonData.m_iReplyNpc, missionCommonData.m_ID);
			missionCommonData.m_Type = (MissionType)Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			missionCommonData.m_ScriptID = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID"));
			missionCommonData.m_MaxNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MaxNum")));
			strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Description")));
			missionCommonData.m_Description = PELocalization.GetString(strId);
			strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("VarDesc")));
			missionCommonData.m_MulDesc = PELocalization.GetString(strId);
			missionCommonData.m_VarValueID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("VarValueID")));
			missionCommonData.m_VarValue = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("VarValue")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetIDList"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					missionCommonData.m_TargetIDList.Add(Convert.ToInt32(array[i]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PreLimit"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == "0")
				{
					continue;
				}
				if (j == 0)
				{
					string[] array2 = array[j].Split(':');
					if (array2.Length == 2)
					{
						missionCommonData.m_PreLimit.type = Convert.ToInt32(array2[0]);
						if (array2[1] != "0")
						{
							missionCommonData.m_PreLimit.idlist.Add(Convert.ToInt32(array2[1]));
						}
					}
					else if (array2.Length == 1 && array.Length == 1 && array2[0] != "0")
					{
						missionCommonData.m_PreLimit.idlist.Add(Convert.ToInt32(array2[0]));
					}
				}
				else if (array[j] != "0")
				{
					missionCommonData.m_PreLimit.idlist.Add(Convert.ToInt32(array[j]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AfterLimit"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] == "0")
				{
					continue;
				}
				if (k == 0)
				{
					string[] array3 = array[k].Split(':');
					if (array3.Length == 2)
					{
						missionCommonData.m_AfterLimit.type = Convert.ToInt32(array3[0]);
						if (array3[1] != "0")
						{
							missionCommonData.m_AfterLimit.idlist.Add(Convert.ToInt32(array3[1]));
						}
					}
				}
				else if (array[k] != "0")
				{
					missionCommonData.m_AfterLimit.idlist.Add(Convert.ToInt32(array[k]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MutexLimit"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (array[l] == "0")
				{
					continue;
				}
				if (l == 0)
				{
					string[] array4 = array[l].Split(':');
					if (array4.Length == 2)
					{
						missionCommonData.m_MutexLimit.type = Convert.ToInt32(array4[0]);
						if (array4[1] != "0")
						{
							missionCommonData.m_MutexLimit.idlist.Add(Convert.ToInt32(array4[1]));
						}
					}
					else if (array4.Length == 1 && array.Length == 1 && array4[0] != "0")
					{
						missionCommonData.m_MutexLimit.idlist.Add(Convert.ToInt32(array4[0]));
					}
				}
				else if (array[l] != "0")
				{
					missionCommonData.m_MutexLimit.idlist.Add(Convert.ToInt32(array[l]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("GuanLianList"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				if (!(array[m] == "0"))
				{
					missionCommonData.m_GuanLianList.Add(Convert.ToInt32(array[m]));
				}
			}
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
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DemandItem"));
			array = @string.Split(',');
			string[] array5;
			for (int n = 0; n < array.Length; n++)
			{
				if (!(array[n] == "0"))
				{
					array5 = array[n].Split('_');
					if (array5.Length == 2)
					{
						item.id = Convert.ToInt32(array5[0]);
						item.num = Convert.ToInt32(array5[1]);
						missionCommonData.m_Get_DemandItem.Add(item);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DeleteItem"));
			array = @string.Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				if (!(array[num] == "0"))
				{
					array5 = array[num].Split('_');
					if (array5.Length == 2)
					{
						item2.id = Convert.ToInt32(array5[0]);
						item2.num = Convert.ToInt32(array5[1]);
						missionCommonData.m_Get_DeleteItem.Add(item2);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_MissionItem"));
			array = @string.Split(',');
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				if (!(array[num2] == "0"))
				{
					array5 = array[num2].Split('_');
					if (array5.Length == 2)
					{
						item3.id = Convert.ToInt32(array5[0]);
						item3.num = Convert.ToInt32(array5[1]);
						missionCommonData.m_Get_MissionItem.Add(item3);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoRewardItem"));
			array = @string.Split(',');
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				if (!(array[num3] == "0"))
				{
					array5 = array[num3].Split('_');
					if (array5.Length == 2)
					{
						item4.id = Convert.ToInt32(array5[0]);
						item4.num = Convert.ToInt32(array5[1]);
						missionCommonData.m_Com_RewardItem.Add(item4);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoVarRewardItem"));
			array = @string.Split(';');
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				List<MissionIDNum> list = new List<MissionIDNum>();
				string[] array6 = array[num4].Split(',');
				for (int num5 = 0; num5 < array6.Length; num5++)
				{
					if (!(array6[num5] == "0"))
					{
						array5 = array6[num5].Split('_');
						if (array5.Length == 2)
						{
							item5.id = Convert.ToInt32(array5[0]);
							item5.num = Convert.ToInt32(array5[1]);
							list.Add(item5);
						}
					}
				}
				missionCommonData.m_Com_MulRewardItem.Add(num4, list);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoSelRewardItem"));
			array = @string.Split(',');
			for (int num6 = 0; num6 < array.Length; num6++)
			{
				if (!(array[num6] == "0"))
				{
					array5 = array[num6].Split('_');
					if (array5.Length == 2)
					{
						item6.id = Convert.ToInt32(array5[0]);
						item6.num = Convert.ToInt32(array5[1]);
						missionCommonData.m_Com_SelRewardItem.Add(item6);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoRemoveItem"));
			array = @string.Split(',');
			for (int num7 = 0; num7 < array.Length; num7++)
			{
				if (!(array[num7] == "0"))
				{
					array5 = array[num7].Split('_');
					if (array5.Length == 2)
					{
						item7.id = Convert.ToInt32(array5[0]);
						item7.num = Convert.ToInt32(array5[1]);
						missionCommonData.m_Com_RemoveItem.Add(item7);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkOP"));
			array = @string.Split(':');
			if (array.Length > 1)
			{
				for (int num8 = 1; num8 < array.Length; num8++)
				{
					missionCommonData.m_OPID.Add(Convert.ToInt32(array[num8]));
				}
			}
			array5 = array[0].Split(',');
			for (int num9 = 0; num9 < array5.Length; num9++)
			{
				if (!(array5[num9] == "0"))
				{
					missionCommonData.m_TalkOP.Add(Convert.ToInt32(array5[num9]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN"));
			array = @string.Split(':');
			if (array.Length > 1)
			{
				for (int num10 = 1; num10 < array.Length; num10++)
				{
					missionCommonData.m_INID.Add(Convert.ToInt32(array[num10]));
				}
			}
			array5 = array[0].Split(',');
			for (int num11 = 0; num11 < array5.Length; num11++)
			{
				if (!(array5[num11] == "0"))
				{
					missionCommonData.m_TalkIN.Add(Convert.ToInt32(array5[num11]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED"));
			array = @string.Split(':');
			if (array.Length > 1)
			{
				for (int num12 = 1; num12 < array.Length; num12++)
				{
					missionCommonData.m_EDID.Add(Convert.ToInt32(array[num12]));
				}
			}
			array5 = array[0].Split(',');
			for (int num13 = 0; num13 < array5.Length; num13++)
			{
				if (!(array5[num13] == "0"))
				{
					missionCommonData.m_TalkED.Add(Convert.ToInt32(array5[num13]));
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
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("resetid"));
			array = @string.Split(',');
			for (int num14 = 0; num14 < array.Length; num14++)
			{
				if (!(array[num14] == "0"))
				{
					missionCommonData.m_ResetID.Add(Convert.ToInt32(array[num14]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("deleteQid"));
			array = @string.Split(',');
			for (int num15 = 0; num15 < array.Length; num15++)
			{
				if (!(array[num15] == "0"))
				{
					missionCommonData.m_DeleteID.Add(Convert.ToInt32(array[num15]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkOP_SP"));
			array = @string.Split(',');
			for (int num16 = 0; num16 < array.Length; num16++)
			{
				if (!(array[num16] == "0"))
				{
					missionCommonData.m_PromptOP.Add(Convert.ToInt32(array[num16]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN_SP"));
			array = @string.Split(',');
			for (int num17 = 0; num17 < array.Length; num17++)
			{
				if (!(array[num17] == "0"))
				{
					missionCommonData.m_PromptIN.Add(Convert.ToInt32(array[num17]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED_SP"));
			array = @string.Split(',');
			for (int num18 = 0; num18 < array.Length; num18++)
			{
				if (!(array[num18] == "0"))
				{
					missionCommonData.m_PromptED.Add(Convert.ToInt32(array[num18]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("StoryInfo"));
			array = @string.Split(',');
			for (int num19 = 0; num19 < array.Length; num19++)
			{
				array5 = array[num19].Split('_');
				if (array5.Length == 2)
				{
					item8.type = (Story_Info)Convert.ToInt32(array5[0]);
					item8.storyid = Convert.ToInt32(array5[1]);
					missionCommonData.m_StoryInfo.Add(item8);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NeedTime"));
			array = @string.Split('_');
			missionCommonData.m_NeedTime = Convert.ToInt32(array[0]);
			if (array.Length == 2)
			{
				missionCommonData.m_timeOverToPlot = Convert.ToInt32(array[1]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ColonyNPC"));
			array = @string.Split(',');
			for (int num20 = 0; num20 < array.Length; num20++)
			{
				if (!(array[num20] == "0"))
				{
					missionCommonData.m_iColonyNpcList.Add(Convert.ToInt32(array[num20]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCmode"));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				missionCommonData.m_ColonyMis[0] = Convert.ToInt32(array[0]);
				missionCommonData.m_ColonyMis[1] = Convert.ToInt32(array[1]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReputationPuja"));
			array = @string.Split('_');
			if (array.Length == 3)
			{
				ReputationPreLimit item9 = default(ReputationPreLimit);
				item9.type = Convert.ToInt32(array[0]);
				item9.min = Convert.ToInt32(array[1]);
				item9.max = Convert.ToInt32(array[2]);
				item9.campID = 5;
				missionCommonData.m_reputationPre.Add(item9);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReputationPaja"));
			array = @string.Split('_');
			if (array.Length == 3)
			{
				ReputationPreLimit item10 = default(ReputationPreLimit);
				item10.type = Convert.ToInt32(array[0]);
				item10.min = Convert.ToInt32(array[1]);
				item10.max = Convert.ToInt32(array[2]);
				item10.campID = 6;
				missionCommonData.m_reputationPre.Add(item10);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCType"));
			array = @string.Split(';');
			string[] array7 = array;
			foreach (string text in array7)
			{
				if (text.Equals("0"))
				{
					continue;
				}
				item11.npcs = new List<int>();
				item11.type = -1;
				string[] array8 = text.Split('_');
				if (array8.Length == 2)
				{
					string[] array9 = array8[0].Split(',');
					foreach (string value in array9)
					{
						item11.npcs.Add(Convert.ToInt32(value));
					}
					item11.type = Convert.ToInt32(array8[1]);
					missionCommonData.m_npcType.Add(item11);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TempLimit"));
			array = @string.Split(',');
			string[] array10 = array;
			foreach (string text2 in array10)
			{
				if (!text2.Equals("0"))
				{
					missionCommonData.m_tempLimit.Add(Convert.ToInt32(text2));
				}
			}
			missionCommonData.m_replyIconId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReplyIcon")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FailNPCType"));
			array = @string.Split(';');
			string[] array11 = array;
			foreach (string text3 in array11)
			{
				array5 = text3.Split('_');
				if (array5.Length == 2)
				{
					NpcType item12 = default(NpcType);
					item12.npcs = new List<int>();
					item12.type = Convert.ToInt32(array5[1]);
					string[] array12 = array5[0].Split(',');
					foreach (string value2 in array12)
					{
						item12.npcs.Add(Convert.ToInt32(value2));
					}
					missionCommonData.m_failNpcType.Add(item12);
				}
			}
			m_MissionCommonMap.Add(missionCommonData.m_ID, missionCommonData);
		}
	}

	public static void LoadTypeMonster()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_KillMonster");
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
			typeMonsterData.m_mustByPlayer = (sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")).Equals("1") ? true : false);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsterID"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('_');
				if (array2.Length == 3)
				{
					item.npcs = new List<int>();
					string[] array3 = array2[0].Split(',');
					for (int j = 0; j < array3.Length; j++)
					{
						item.npcs.Add(Convert.ToInt32(array3[j]));
					}
					item.type = Convert.ToInt32(array2[1]);
					typeMonsterData.type = Convert.ToInt32(array2[2]);
					typeMonsterData.m_MonsterList.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetPos"));
			array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				typeMonsterData.m_TargetPos = new Vector3(x, y, z);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					typeMonsterData.m_ReceiveList.Add(Convert.ToInt32(array[k]));
				}
			}
			m_TypeMonster.Add(typeMonsterData.m_TargetID, typeMonsterData);
		}
	}

	public static void LoadTypeCollect()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_LootItem");
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
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RandNum"));
			string[] array = @string.Split(',');
			if (array.Length == 3)
			{
				typeCollectData.m_randItemNum[0] = Convert.ToInt32(array[0]);
				typeCollectData.m_randItemNum[1] = Convert.ToInt32(array[1]);
				typeCollectData.m_randItemNum[2] = Convert.ToInt32(array[2]);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RandID"));
			array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				int num = Convert.ToInt32(array[i]);
				if (num != 0)
				{
					typeCollectData.m_randItemID.Add(num);
				}
			}
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetPos"));
			string[] array2 = string2.Split(',', '_');
			if (array2.Length == 4)
			{
				float x = Convert.ToSingle(array2[0]);
				float y = Convert.ToSingle(array2[1]);
				float z = Convert.ToSingle(array2[2]);
				typeCollectData.m_TargetPos = new Vector3(x, y, z);
				typeCollectData.m_TargetRadius = Convert.ToInt32(array2[3]);
			}
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			string[] array3 = string3.Split(',');
			for (int j = 0; j < array3.Length; j++)
			{
				if (!(array3[j] == "0"))
				{
					typeCollectData.m_ReceiveList.Add(Convert.ToInt32(array3[j]));
				}
			}
			m_TypeCollect.Add(typeCollectData.m_TargetID, typeCollectData);
		}
	}

	public static void LoadTypeFollow()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_HuSong");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeFollowData typeFollowData = new TypeFollowData();
			typeFollowData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeFollowData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeFollowData.m_Desc = PELocalization.GetString(strId);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NpcList"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					strId = Convert.ToInt32(array[i]);
					typeFollowData.m_iNpcList.Add(strId);
				}
			}
			typeFollowData.m_SceneType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Scene")));
			array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Emode")).Split('_');
			if (array.Length == 2)
			{
				typeFollowData.m_EMode = Convert.ToInt32(array[0]);
				typeFollowData.m_isAttack = Convert.ToInt32(array[1]);
			}
			typeFollowData.m_LookNameID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("LookName")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DistPos"));
			array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				typeFollowData.m_DistPos = new Vector3(x, y, z);
			}
			typeFollowData.m_DistRadius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DistRadius")));
			typeFollowData.m_TrackRadius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TrackRadius")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ResetPos"));
			array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				typeFollowData.m_ResetPos = new Vector3(x, y, z);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FailResetPos"));
			array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				typeFollowData.m_FailResetPos = new Vector3(x, y, z);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkInfo"));
			array = @string.Split(':');
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == "0")
				{
					continue;
				}
				string[] array2 = array[j].Split('_');
				if (array2.Length != 3)
				{
					continue;
				}
				string[] array3 = array2[0].Split(',');
				if (array3.Length != 3)
				{
					continue;
				}
				TalkInfo talkInfo = new TalkInfo();
				float x = Convert.ToSingle(array3[0]);
				float y = Convert.ToSingle(array3[1]);
				float z = Convert.ToSingle(array3[2]);
				talkInfo.pos = new Vector3(x, y, z);
				talkInfo.radius = Convert.ToInt32(array2[1]);
				array3 = array2[2].Split(',');
				for (int k = 0; k < array3.Length; k++)
				{
					if (!(array3[k] == "0"))
					{
						talkInfo.talkid.Add(Convert.ToInt32(array3[k]));
					}
				}
				typeFollowData.m_TalkInfo.Add(talkInfo);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("WaitTalkList"));
			if (!@string.Equals("0"))
			{
				array = @string.Split(';');
				string[] array4 = array;
				foreach (string text in array4)
				{
					string[] array2 = text.Split('_', ',');
					if (array2.Length == 3)
					{
						typeFollowData.npcid_behindTalk_forwardTalk.Add(Convert.ToInt32(array2[0]), new int[2]
						{
							Convert.ToInt32(array2[1]),
							Convert.ToInt32(array2[2])
						});
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PathList"));
			array = @string.Split(';');
			for (int m = 0; m < array.Length; m++)
			{
				string[] array2 = array[m].Split(',');
				if (array2.Length == 3)
				{
					float x = Convert.ToSingle(array2[0]);
					float y = Convert.ToSingle(array2[1]);
					float z = Convert.ToSingle(array2[2]);
					typeFollowData.m_PathList.Add(new Vector3(x, y, z));
				}
			}
			typeFollowData.m_isNeedReturn = !(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NeedReturn")) == "0");
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Monster"));
			array = @string.Split(':');
			for (int n = 0; n < array.Length; n++)
			{
				if (array[n] == "0")
				{
					continue;
				}
				string[] array2 = array[n].Split('_');
				if (array2.Length == 4)
				{
					MonsterIDNum item = default(MonsterIDNum);
					item.id = Convert.ToInt32(array2[0]);
					item.num = Convert.ToInt32(array2[1]);
					item.radius = Convert.ToInt32(array2[2]);
					string[] array3 = array2[3].Split(',');
					if (array3.Length == 3)
					{
						float x = Convert.ToSingle(array3[0]);
						float y = Convert.ToSingle(array3[1]);
						float z = Convert.ToSingle(array3[2]);
						item.pos = new Vector3(x, y, z);
					}
					typeFollowData.m_Monster.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ComTalkID"));
			array = @string.Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				if (!(array[num] == "0"))
				{
					typeFollowData.m_ComTalkID.Add(Convert.ToInt32(array[num]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FailNpc"));
			array = @string.Split(',');
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				if (!(array[num2] == "0"))
				{
					typeFollowData.m_iFailNpc.Add(Convert.ToInt32(array[num2]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				if (!(array[num3] == "0"))
				{
					typeFollowData.m_ReceiveList.Add(Convert.ToInt32(array[num3]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("WaitDist"));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				typeFollowData.m_WaitDist.Add(Convert.ToInt32(array[0]));
				typeFollowData.m_WaitDist.Add(Convert.ToInt32(array[1]));
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NeedPlayer"));
			typeFollowData.m_isNeedPlayer = Convert.ToInt32(@string) == 1;
			m_TypeFollow.Add(typeFollowData.m_TargetID, typeFollowData);
		}
	}

	public static void LoadTypeSearch()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_EnterArea");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeSearchData typeSearchData = new TypeSearchData();
			typeSearchData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeSearchData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeSearchData.m_Desc = PELocalization.GetString(strId);
			typeSearchData.m_NpcID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("LookName")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DistPos"));
			string[] array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				typeSearchData.m_DistPos = new Vector3(x, y, z);
			}
			typeSearchData.m_SceneType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Scene")));
			typeSearchData.m_DistRadius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DistRadius")));
			typeSearchData.m_TrackRadius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TrackRadius")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Prompt"));
			array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					typeSearchData.m_Prompt.Add(Convert.ToInt32(array[i]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkID"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (!(array[j] == "0"))
				{
					typeSearchData.m_TalkID.Add(Convert.ToInt32(array[j]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					typeSearchData.m_ReceiveList.Add(Convert.ToInt32(array[k]));
				}
			}
			m_TypeSearch.Add(typeSearchData.m_TargetID, typeSearchData);
		}
	}

	public static void LoadTypeUseItem()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_UseItem");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeUseItemData typeUseItemData = new TypeUseItemData();
			typeUseItemData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeUseItemData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			int strId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Desc")));
			typeUseItemData.m_Desc = PELocalization.GetString(strId);
			typeUseItemData.m_Type = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			typeUseItemData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeUseItemData.m_UseNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UseNum")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Pos"));
			string[] array = @string.Split(',');
			if (array.Length == 3)
			{
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				typeUseItemData.m_Pos = new Vector3(x, y, z);
			}
			typeUseItemData.m_Radius = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Radius")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UsedPrompt"));
			array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					typeUseItemData.m_UsedPrompt.Add(Convert.ToInt32(array[i]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkID"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				if (!(array[j] == "0"))
				{
					typeUseItemData.m_TalkID.Add(Convert.ToInt32(array[j]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FailPrompt"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					typeUseItemData.m_FailPrompt.Add(Convert.ToInt32(array[k]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					typeUseItemData.m_ReceiveList.Add(Convert.ToInt32(array[l]));
				}
			}
			typeUseItemData.m_allowOld = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AllowOld")) == "1";
			m_TypeUseItem.Add(typeUseItemData.m_TargetID, typeUseItemData);
		}
	}

	public static void LoadTypeMessenger()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_Delivery");
		sqliteDataReader.Read();
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TypeMessengerData typeMessengerData = new TypeMessengerData();
			typeMessengerData.m_TargetID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetID")));
			typeMessengerData.m_ScriptID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScriptID")));
			typeMessengerData.m_iNpc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Npc")));
			typeMessengerData.m_iReplyNpc = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReplyNpc")));
			typeMessengerData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemID")));
			typeMessengerData.m_ItemNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ItemNum")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					typeMessengerData.m_ReceiveList.Add(Convert.ToInt32(array[i]));
				}
			}
			m_TypeMessenger.Add(typeMessengerData.m_TargetID, typeMessengerData);
		}
	}

	public static void LoadTypeTowerDefends()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_TowerDefence");
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
			typeTowerDefendsData.m_tolTime = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TolTime")));
			typeTowerDefendsData.m_range = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Range")));
			typeTowerDefendsData.m_TdInfoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("td_id")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SweepID"));
			array = @string.Split(',');
			string[] array3 = array;
			foreach (string text in array3)
			{
				if (!(text == "0"))
				{
					typeTowerDefendsData.m_SweepId.Add(Convert.ToInt32(text));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReceiveQuest"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					typeTowerDefendsData.m_ReceiveList.Add(Convert.ToInt32(array[l]));
				}
			}
			m_TypeTowerDefends.Add(typeTowerDefendsData.m_TargetID, typeTowerDefendsData);
		}
	}

	public static void LoadTDInfoData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Quest_TD_Info");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			TowerDefendsInfoData towerDefendsInfoData = new TowerDefendsInfoData();
			towerDefendsInfoData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("td_id")));
			towerDefendsInfoData.m_delay = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("delay")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Info"));
			string[] array = @string.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('_');
				if (array2.Length == 4)
				{
					TDInfo tDInfo = new TDInfo();
					tDInfo.monsterID = Convert.ToInt32(array2[0]);
					tDInfo.num = Convert.ToInt32(array2[1]);
					tDInfo.dir_min = Convert.ToInt32(array2[2]);
					tDInfo.dir_max = Convert.ToInt32(array2[3]);
					towerDefendsInfoData.m_TdInfoList.Add(tDInfo);
				}
			}
			m_TDInfoMap.Add(towerDefendsInfoData.m_ID, towerDefendsInfoData);
		}
	}

	public static void LoadData()
	{
		LoadMissionCommon();
		LoadTypeMonster();
		LoadTypeCollect();
		LoadTypeFollow();
		LoadTypeSearch();
		LoadTypeUseItem();
		LoadTypeMessenger();
		LoadTypeTowerDefends();
		LoadTDInfoData();
	}
}
