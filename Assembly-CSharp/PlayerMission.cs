using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtPlayerPackage;
using Pathea.PeEntityExtTrans;
using PETools;
using PeUIMap;
using SkillSystem;
using TrainingScene;
using UnityEngine;
using WhiteCat;

public class PlayerMission
{
	public enum MissionInfo
	{
		MAX_MISSIONFLAG_LENGTH = 10,
		MAX_MISSIONVALUE_LENGTH = 18,
		MAX_MISSION_COUNT = 20000
	}

	public const int Version_0 = 0;

	public const int Version_1 = 1;

	public const int Version_2 = 2;

	public const int Version_3 = 3;

	public const int Version_4 = 4;

	public const int Version_5 = 5;

	public const int Version_6 = 6;

	public const int Version_7 = 7;

	public const int Version_8 = 8;

	public const int Version_9 = 9;

	public const int Version_10 = 10;

	private const int CurrentVersion = 10;

	public static string MissionFlagItem = "ITEM";

	public static string MissionFlagStep = "STEP";

	public static string MissionFlagMonster = "MONSTER";

	public static string MissionFlagTDMonster = "TDMONS";

	public string m_FollowPlayerName;

	public Dictionary<int, Dictionary<string, string>> m_RecordMisInfo = new Dictionary<int, Dictionary<string, string>>();

	public List<Vector3> m_SpeVecList = new List<Vector3>();

	public List<int> m_GetRewards = new List<int>();

	public Dictionary<int, int> m_MissionTargetState = new Dictionary<int, int>();

	public Dictionary<int, int> m_MissionState = new Dictionary<int, int>();

	public Dictionary<int, Dictionary<string, string>> m_MissionInfo = new Dictionary<int, Dictionary<string, string>>();

	public Dictionary<int, double> m_MissionTime = new Dictionary<int, double>();

	public Dictionary<int, Vector3> m_iCurFollowStartPos = new Dictionary<int, Vector3>();

	public List<NpcCmpt> followers = new List<NpcCmpt>();

	public List<NpcCmpt> pathFollowers = new List<NpcCmpt>();

	private int languegeSkill;

	public Dictionary<int, string> recordNpcName = new Dictionary<int, string>();

	public TowerInfoUIData m_TowerUIData = new TowerInfoUIData();

	public List<string> recordCreationName = new List<string>();

	public List<Vector3> recordCretionPos = new List<Vector3>();

	public bool isRecordCreation;

	private int mOpMissionID;

	private string mOpMissionFlag;

	private string mOpMissionValue;

	public int pajaLanguageBePickup;

	private static Dictionary<int, Vector3> mID_buildPos = new Dictionary<int, Vector3>();

	public Dictionary<int, Vector3> textSamples = new Dictionary<int, Vector3>();

	public List<int> recordAndHer = new List<int>();

	public List<int[]> recordKillNpcItem = new List<int[]>();

	public Dictionary<int, int> adId_entityId = new Dictionary<int, int>();

	public int LanguegeSkill
	{
		get
		{
			return languegeSkill;
		}
		set
		{
			languegeSkill = value;
		}
	}

	public bool HasGetRewards(int MissionID)
	{
		return m_GetRewards.Contains(MissionID);
	}

	private int AddMissionFlagType(int MissionID, string MissionFlag, string MissionValue)
	{
		if (MissionFlag.Length > 10)
		{
			return -1;
		}
		if (MissionValue.Length > 18)
		{
			return -1;
		}
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		MissionFlag.ToUpper();
		if (missionFlagType != null)
		{
			ModifyQuestVariable(MissionID, MissionFlag, MissionValue);
			return 0;
		}
		missionFlagType = new Dictionary<string, string>();
		missionFlagType.Add(MissionFlag, MissionValue);
		m_MissionInfo.Add(MissionID, missionFlagType);
		return 1;
	}

	public bool ConTainsMission(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		return m_MissionInfo.ContainsKey(MissionID);
	}

	public Dictionary<string, string> GetMissionFlagType(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return null;
		}
		return (!m_MissionInfo.ContainsKey(MissionID)) ? null : m_MissionInfo[MissionID];
	}

	public bool HasQuestVariable(int MissionID, string MissionFlag)
	{
		if (m_MissionInfo.Count == 0)
		{
			return false;
		}
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		MissionFlag.ToUpper();
		return missionFlagType?.ContainsKey(MissionFlag) ?? false;
	}

	public string GetQuestVariable(int MissionID, string MissionFlag)
	{
		if (m_MissionInfo.Count == 0)
		{
			return "0";
		}
		if (MissionID < 0 || MissionID > 20000)
		{
			return "0";
		}
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		MissionFlag = MissionFlag.ToUpper();
		if (missionFlagType != null && missionFlagType.ContainsKey(MissionFlag))
		{
			return missionFlagType[MissionFlag];
		}
		return "0";
	}

	public int GetQuestVariable(int MissionID, int id)
	{
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<string, string> item in missionFlagType)
		{
			if (!(item.Value == "0"))
			{
				string[] array = item.Value.Split('_');
				if (array.Length == 2 && !(array[0] != id.ToString()))
				{
					int num2 = Convert.ToInt32(array[1]);
					num = ((num2 <= num) ? num : num2);
				}
			}
		}
		return num;
	}

	public bool DelQuestVariable(int MissionID, string MissionFlag)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		if (HadCompleteMission(MissionID))
		{
			return false;
		}
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		MissionFlag.ToUpper();
		if (missionFlagType != null && missionFlagType.ContainsKey(MissionFlag))
		{
			missionFlagType.Remove(MissionFlag);
		}
		return true;
	}

	public bool ModifyQuestVariable(int MissionID, string MissionFlag, string MissionValue)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		if (MissionFlag.Length > 10)
		{
			return false;
		}
		if (MissionValue.Length > 18)
		{
			return false;
		}
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			return false;
		}
		MissionFlag.ToUpper();
		missionFlagType[MissionFlag] = MissionValue;
		string[] flagvalue = MissionValue.Split('_');
		if (flagvalue.Length < 2)
		{
			return false;
		}
		string value = MissionFlag.Substring(4, 1);
		int num = MissionManager.GetMissionCommonData(MissionID).m_TargetIDList.Find(delegate(int ite)
		{
			TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(ite);
			if (typeMonsterData == null)
			{
				return false;
			}
			foreach (NpcType monster in typeMonsterData.m_MonsterList)
			{
				if (monster.npcs.Contains(Convert.ToInt32(flagvalue[0])))
				{
					return true;
				}
			}
			return false;
		});
		if (MissionFlag.Substring(0, 4) == MissionFlagItem)
		{
			MissionManager.Instance.UpdateUseMissionTrack(MissionID, Convert.ToInt32(value), Convert.ToInt32(flagvalue[1]));
		}
		else if (num == 0)
		{
			MissionManager.Instance.UpdateMissionTrack(MissionID, Convert.ToInt32(flagvalue[1]));
		}
		else
		{
			MissionManager.Instance.UpdateMissionTrack(MissionID, Convert.ToInt32(flagvalue[1]), num);
		}
		UpdateAllNpcMisTex();
		return true;
	}

	public bool ModifyQuestVariable(int MissionID, string MissionFlag, int ItemID, int num)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			return false;
		}
		MissionFlag.ToUpper();
		if (!missionFlagType.ContainsKey(MissionFlag))
		{
			return false;
		}
		string text = missionFlagType[MissionFlag];
		string[] array = text.Split('_');
		if (array.Length < 2)
		{
			return false;
		}
		int num2 = Convert.ToInt32(array[0]);
		if (num2 != ItemID)
		{
			return false;
		}
		if (MissionID == 822)
		{
			isRecordCreation = true;
		}
		int num3 = Convert.ToInt32(array[1]) + num;
		string missionValue = array[0] + "_" + num3;
		ModifyQuestVariable(MissionID, MissionFlag, missionValue);
		return true;
	}

	public bool HasMission(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		if (m_MissionInfo.ContainsKey(MissionID))
		{
			return true;
		}
		return false;
	}

	public bool HasTarget(int targetID)
	{
		bool result = false;
		foreach (int key in m_MissionInfo.Keys)
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(key);
			if (missionCommonData == null || !missionCommonData.m_TargetIDList.Contains(targetID))
			{
				continue;
			}
			result = true;
			break;
		}
		return result;
	}

	public List<int> GetCollectMissionListByID(int ItemID)
	{
		List<int> list = new List<int>();
		foreach (int key in m_MissionInfo.Keys)
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(key);
			if (missionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]);
				if (targetType != TargetType.TargetType_Collect)
				{
					continue;
				}
				TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(missionCommonData.m_TargetIDList[i]);
				if (typeCollectData != null)
				{
					if (typeCollectData.ItemID == ItemID)
					{
						list.Add(key);
					}
					else if (typeCollectData.m_TargetItemID == ItemID)
					{
						list.Add(key);
					}
				}
			}
		}
		return list;
	}

	public void ProcessUseItemByID(int ItemID, Vector3 pos, int addOrSubstract = 1, ItemObject itemobj = null)
	{
		if (ItemID == 1339)
		{
			KillNPC.ashBox_inScene++;
		}
		if (ItemID == 1541)
		{
			MissionManager.Instance.m_PlayerMission.LanguegeSkill++;
		}
		List<int> list = new List<int>(m_MissionInfo.Keys);
		isRecordCreation = false;
		foreach (int item in list)
		{
			if (item == 242 || item == 629)
			{
				continue;
			}
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(item);
			if (missionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				switch (MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]))
				{
				case TargetType.TargetType_UseItem:
				{
					TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(missionCommonData.m_TargetIDList[i]);
					if (typeUseItemData == null)
					{
						break;
					}
					if (typeUseItemData.m_Type == 1)
					{
						float num;
						if (PeGameMgr.IsStory)
						{
							if (item == 550)
							{
								GameObject gameObject = GameObject.Find("satellite_receiver_base");
								if (gameObject == null)
								{
									break;
								}
								num = Vector3.Distance(pos, gameObject.transform.position);
							}
							else if (typeUseItemData.m_Pos == new Vector3(-255f, -255f, -255f))
							{
								if (!CSMain.GetAssemblyPos(out var pos2))
								{
									break;
								}
								num = Vector3.Distance(pos, pos2);
							}
							else
							{
								num = Vector3.Distance(pos, typeUseItemData.m_Pos);
							}
						}
						else
						{
							Vector2 a = new Vector2(pos.x, pos.z);
							Vector2 b = new Vector2(typeUseItemData.m_Pos.x, typeUseItemData.m_Pos.z);
							num = Vector2.Distance(a, b);
						}
						if (num > (float)typeUseItemData.m_Radius)
						{
							break;
						}
					}
					string missionFlag = MissionFlagItem + i;
					ModifyQuestVariable(missionCommonData.m_ID, missionFlag, ItemID, addOrSubstract);
					MissionManager.Instance.CompleteTarget(missionCommonData.m_TargetIDList[i], missionCommonData.m_ID);
					if (item != 953 || IsSpecialID(ItemID) != ECreation.SimpleObject)
					{
						break;
					}
					CreationData creation = CreationMgr.GetCreation(itemobj.instanceId);
					if (creation == null)
					{
						break;
					}
					int num2 = 0;
					foreach (int value in creation.m_Attribute.m_Cost.Values)
					{
						num2 += value;
					}
					if (num2 <= 300)
					{
						StroyManager.Instance.GetMissionOrPlotById(10954);
					}
					else
					{
						StroyManager.Instance.GetMissionOrPlotById(10955);
					}
					break;
				}
				case TargetType.TargetType_Collect:
				{
					TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(missionCommonData.m_TargetIDList[i]);
					if (typeCollectData.ItemID == ItemID)
					{
						MissionManager.Instance.UpdateMissionTrack(item);
					}
					break;
				}
				}
			}
		}
	}

	public bool HadCompleteTarget(int TargetID)
	{
		if (m_MissionTargetState.ContainsKey(TargetID))
		{
			return true;
		}
		return false;
	}

	public bool HadCompleteMission(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.m_MaxNum == -1)
		{
			return false;
		}
		if (m_MissionState.ContainsKey(MissionID))
		{
			if (m_MissionState[MissionID] >= missionCommonData.m_MaxNum)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool HadCompleteMissionAnyNum(int MissionID)
	{
		if (m_MissionState.ContainsKey(MissionID))
		{
			return true;
		}
		return false;
	}

	public bool SetMission(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		UIMissionMgr.Instance.DeleteGetableMission(MissionID);
		if (missionCommonData.m_MaxNum == -1)
		{
			return false;
		}
		if (m_MissionState.ContainsKey(MissionID))
		{
			Dictionary<int, int> missionState;
			Dictionary<int, int> dictionary = (missionState = m_MissionState);
			int key;
			int key2 = (key = MissionID);
			key = missionState[key];
			dictionary[key2] = key + 1;
		}
		else
		{
			m_MissionState.Add(MissionID, 1);
		}
		MessageData.AddMsgByCompletedMissionID(MissionID);
		MissionManager.Instance.CheckAllGetableMission();
		if (PeGameMgr.IsStory && MissionID == 383 && null != GameUI.Instance)
		{
			UIStroyMap uIStroyMap = (UIStroyMap)GameUI.Instance.mUIWorldMap.CurMap;
			uIStroyMap.ShowFindNpcRange = true;
		}
		return true;
	}

	public void DelMissionInfo(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData != null)
		{
			AiAdNpcNetwork aiAdNpcNetwork = NetworkInterface.Get<AiAdNpcNetwork>(missionCommonData.m_iNpc);
			if (null != aiAdNpcNetwork)
			{
				aiAdNpcNetwork.InitForceData();
			}
		}
		if (HasMission(MissionID))
		{
			m_MissionInfo.Remove(MissionID);
		}
	}

	public int GetMissionInfoCount()
	{
		return m_MissionInfo.Count;
	}

	public bool IsGetTakeMission(int MissionID, bool isPreLimitOn = true)
	{
		if (MissionID <= 0 || MissionID > 20000)
		{
			return false;
		}
		if (HasMission(MissionID) || HadCompleteMission(MissionID))
		{
			return false;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		bool flag = false;
		if (isPreLimitOn && missionCommonData.m_PreLimit.idlist.Count > 0)
		{
			if (missionCommonData.m_PreLimit.type == 2)
			{
				flag = true;
				for (int i = 0; i < missionCommonData.m_PreLimit.idlist.Count; i++)
				{
					if (missionCommonData.m_PreLimit.idlist[i] > 999 && missionCommonData.m_PreLimit.idlist[i] < 9000)
					{
						if (!HadCompleteTarget(missionCommonData.m_PreLimit.idlist[i]))
						{
							return false;
						}
					}
					else if (!HadCompleteMission(missionCommonData.m_PreLimit.idlist[i]))
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < missionCommonData.m_PreLimit.idlist.Count; j++)
				{
					if (missionCommonData.m_PreLimit.idlist[j] > 999 && missionCommonData.m_PreLimit.idlist[j] < 9000)
					{
						if (HadCompleteTarget(missionCommonData.m_PreLimit.idlist[j]))
						{
							flag = true;
							break;
						}
					}
					else if (HadCompleteMission(missionCommonData.m_PreLimit.idlist[j]))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		flag = true;
		if (missionCommonData.m_AfterLimit.idlist.Count > 0)
		{
			for (int k = 0; k < missionCommonData.m_AfterLimit.idlist.Count; k++)
			{
				if (missionCommonData.m_AfterLimit.type == 1)
				{
					if (missionCommonData.m_AfterLimit.idlist[k] > 999 && missionCommonData.m_AfterLimit.idlist[k] < 9000)
					{
						if (HasTarget(missionCommonData.m_AfterLimit.idlist[k]) || HadCompleteTarget(missionCommonData.m_AfterLimit.idlist[k]))
						{
							flag = false;
							break;
						}
					}
					else if (HasMission(missionCommonData.m_AfterLimit.idlist[k]) || HadCompleteMission(missionCommonData.m_AfterLimit.idlist[k]))
					{
						flag = false;
						break;
					}
				}
				else if (missionCommonData.m_AfterLimit.idlist[k] > 999 && missionCommonData.m_AfterLimit.idlist[k] < 9000)
				{
					if (HadCompleteTarget(missionCommonData.m_AfterLimit.idlist[k]))
					{
						flag = false;
						break;
					}
				}
				else if (HadCompleteMission(missionCommonData.m_AfterLimit.idlist[k]))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		flag = true;
		if (missionCommonData.m_MutexLimit.idlist.Count > 0)
		{
			if (missionCommonData.m_MutexLimit.type == 1)
			{
				flag = false;
				for (int l = 0; l < missionCommonData.m_MutexLimit.idlist.Count; l++)
				{
					if (missionCommonData.m_MutexLimit.idlist[l] > 999 && missionCommonData.m_MutexLimit.idlist[l] < 9000)
					{
						if (HadCompleteTarget(missionCommonData.m_MutexLimit.idlist[l]) || HasTarget(missionCommonData.m_MutexLimit.idlist[l]))
						{
							return false;
						}
					}
					else if (HadCompleteMission(missionCommonData.m_MutexLimit.idlist[l]) || HasMission(missionCommonData.m_MutexLimit.idlist[l]))
					{
						return false;
					}
				}
			}
			else
			{
				for (int m = 0; m < missionCommonData.m_MutexLimit.idlist.Count; m++)
				{
					if (missionCommonData.m_MutexLimit.idlist[m] > 999 && missionCommonData.m_MutexLimit.idlist[m] < 9000)
					{
						if (!HadCompleteTarget(missionCommonData.m_MutexLimit.idlist[m]))
						{
							flag = false;
							break;
						}
					}
					else if (!HadCompleteMission(missionCommonData.m_MutexLimit.idlist[m]))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return false;
			}
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int n = 0; n < missionCommonData.m_Get_DemandItem.Count; n++)
		{
			ECreation eCreation = IsSpecialID(missionCommonData.m_Get_DemandItem[n].id);
			num = ((eCreation == ECreation.Null) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetPkgItemCount(missionCommonData.m_Get_DemandItem[n].id) : PeSingleton<PeCreature>.Instance.mainPlayer.GetCreationItemCount(eCreation));
			if (num < missionCommonData.m_Get_DemandItem[n].num)
			{
				return false;
			}
		}
		for (int num4 = 0; num4 < missionCommonData.m_Get_MissionItem.Count; num4++)
		{
			ItemProto itemData = ItemProto.GetItemData(missionCommonData.m_Get_MissionItem[num4].id);
			if (itemData == null)
			{
				continue;
			}
			if (itemData.tabIndex == 0 && itemData.maxStackNum > 0)
			{
				num += (missionCommonData.m_Get_MissionItem[num4].num - 1) / itemData.maxStackNum + 1;
			}
			if (itemData.tabIndex == 1)
			{
				if (itemData.maxStackNum > 0)
				{
					num2 += (missionCommonData.m_Get_MissionItem[num4].num - 1) / itemData.maxStackNum + 1;
				}
			}
			else if (itemData.maxStackNum > 0)
			{
				num3 += (missionCommonData.m_Get_MissionItem[num4].num - 1) / itemData.maxStackNum + 1;
			}
		}
		if (null == PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			return false;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			int playerID = Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
			for (int num5 = 0; num5 < missionCommonData.m_reputationPre.Count; num5++)
			{
				int targetPlayerID;
				if (missionCommonData.m_reputationPre[num5].campID == -1)
				{
					if (GameUI.Instance.mNpcWnd.m_CurSelNpc == null)
					{
						continue;
					}
					targetPlayerID = Mathf.RoundToInt(GameUI.Instance.mNpcWnd.m_CurSelNpc.GetAttribute(AttribType.DefaultPlayerID));
				}
				else
				{
					targetPlayerID = missionCommonData.m_reputationPre[num5].campID;
				}
				if (missionCommonData.m_reputationPre[num5].type == 1)
				{
					if (missionCommonData.m_reputationPre[num5].min >= PeSingleton<ReputationSystem>.Instance.GetReputationValue(playerID, targetPlayerID) || PeSingleton<ReputationSystem>.Instance.GetReputationValue(playerID, targetPlayerID) > missionCommonData.m_reputationPre[num5].max)
					{
						return false;
					}
				}
				else if (missionCommonData.m_reputationPre[num5].min < PeSingleton<ReputationSystem>.Instance.GetReputationValue(playerID, targetPlayerID) && PeSingleton<ReputationSystem>.Instance.GetReputationValue(playerID, targetPlayerID) <= missionCommonData.m_reputationPre[num5].max)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void AbortFollowMission()
	{
		int num = -1;
		if (PeGameMgr.IsMulti)
		{
			num = MissionManager.Instance.HasFollowMissionNet();
			if (num == -1)
			{
				return;
			}
		}
		List<int> list = new List<int>();
		foreach (int key in m_MissionInfo.Keys)
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(key);
			if (missionCommonData != null && missionCommonData.m_TargetIDList.Find((int ite) => MissionRepository.GetTargetType(ite) == TargetType.TargetType_Follow) != 0)
			{
				list.Add(key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			AbortMission(list[i]);
		}
	}

	public void AbortMission(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000 || !HasMission(MissionID))
		{
			return;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return;
		}
		SceneEntityCreator.self.RemoveMissionPoint(MissionID, -1);
		DelMissionInfo(MissionID);
		if (PeGameMgr.IsStory)
		{
			List<int> idlist = missionCommonData.HasStory(Story_Info.Story_Info_Fail);
			StroyManager.Instance.PushStoryList(idlist);
		}
		else if (PeGameMgr.IsAdventure)
		{
			List<int> idlist2 = missionCommonData.HasStory(Story_Info.Story_Info_Fail);
			StroyManager.Instance.PushAdStoryList(idlist2, missionCommonData.m_iNpc);
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
		if (peEntity != null)
		{
			MissionManager.Instance.UpdateMissionMainGUI(MissionID);
			UIMissionMgr.GetableMisView getableMisView = new UIMissionMgr.GetableMisView(MissionID, missionCommonData.m_MissionName, peEntity.ExtGetPos(), peEntity.Id);
			getableMisView.TargetNpcInfo.mName = peEntity.ExtGetName();
			getableMisView.TargetNpcInfo.mNpcIcoStr = peEntity.ExtGetFaceIcon();
			UIMissionMgr.Instance.AddGetableMission(getableMisView);
		}
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			switch (MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]))
			{
			case TargetType.TargetType_Follow:
			{
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
				if (typeFollowData == null)
				{
					break;
				}
				if (typeFollowData.m_LookNameID != 0)
				{
					PeEntity peEntity3 = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_LookNameID);
					if (peEntity3 != null && peEntity3.IsRecruited())
					{
						peEntity3.NpcCmpt.BaseNpcOutMission = false;
					}
				}
				if (missionCommonData.m_TargetIDList[i] == 3031)
				{
					PassengerCmpt passengerCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
					passengerCmpt.onGetOnCarrier = (Action<CarrierController>)Delegate.Remove(passengerCmpt.onGetOnCarrier, new Action<CarrierController>(MissionJoyrideOn));
					PassengerCmpt passengerCmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
					passengerCmpt2.onGetOffCarrier = (Action<CarrierController>)Delegate.Remove(passengerCmpt2.onGetOffCarrier, new Action<CarrierController>(MissionJoyrideOff));
				}
				for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
				{
					peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
					if (!(peEntity == null))
					{
						NpcCmpt npcCmpt = peEntity.NpcCmpt;
						if (npcCmpt != null)
						{
							npcCmpt.Battle = ENpcBattle.Defence;
							npcCmpt.FixedPointPos = peEntity.position;
							npcCmpt.CanTalk = true;
						}
						if (typeFollowData.m_EMode == 1)
						{
							followers.Remove(peEntity.NpcCmpt);
							StroyManager.Instance.RemoveReq(peEntity, EReqType.FollowTarget);
						}
						else
						{
							StroyManager.Instance.RemoveReq(peEntity, EReqType.MoveToPoint);
						}
						if (typeFollowData.m_PathList.Count > 0)
						{
							StroyManager.Instance.RemoveReq(peEntity, EReqType.FollowPath);
						}
						if (peEntity.GetUserData() is NpcMissionData npcMissionData)
						{
							npcMissionData.mInFollowMission = false;
						}
						peEntity.SetAttackMode(EAttackMode.Attack);
						peEntity.SetInvincible(value: true);
						if (MissionManager.HasRandomMission(MissionID))
						{
							GoHome(peEntity);
						}
					}
				}
				break;
			}
			case TargetType.TargetType_TowerDif:
			{
				TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(missionCommonData.m_TargetIDList[i]);
				if (typeTowerDefendsData != null && typeTowerDefendsData.m_iNpcList.Count != 0)
				{
					peEntity = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_iNpcList[0]);
					if (!(peEntity == null))
					{
					}
				}
				break;
			}
			case TargetType.TargetType_Discovery:
			{
				TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(missionCommonData.m_TargetIDList[i]);
				if (typeSearchData != null && typeSearchData.m_NpcID != 0)
				{
					PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
					if (peEntity2 != null && peEntity2.IsRecruited())
					{
						peEntity2.NpcCmpt.BaseNpcOutMission = false;
					}
				}
				break;
			}
			}
		}
		switch (MissionID)
		{
		case 139:
		case 254:
			peEntity = PeSingleton<EntityMgr>.Instance.Get(9003);
			if (peEntity != null)
			{
				peEntity.ExtSetPos(Vector3.zero);
				peEntity.SetStayPos(Vector3.zero);
			}
			break;
		case 158:
			peEntity = PeSingleton<EntityMgr>.Instance.Get(9019);
			if (peEntity != null)
			{
				peEntity.ExtSetPos(Vector3.zero);
				peEntity.SetStayPos(Vector3.zero);
			}
			break;
		default:
			if (MissionManager.HasRandomMission(MissionID))
			{
				peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iReplyNpc);
				if (peEntity != null && peEntity.GetUserData() is NpcMissionData npcMissionData2)
				{
					npcMissionData2.m_MissionListReply.Clear();
				}
			}
			break;
		}
		if (missionCommonData.m_failNpcType.Count > 0)
		{
			foreach (NpcType item in missionCommonData.m_failNpcType)
			{
				item.npcs.ForEach(delegate(int n)
				{
					PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(n);
					NpcCmpt nc;
					if (npc != null && (bool)(nc = npc.GetComponent<NpcCmpt>()))
					{
						nc.NpcControlCmdId = item.type;
					}
				});
			}
		}
		for (int k = 0; k < missionCommonData.m_ResetID.Count; k++)
		{
			if (m_MissionState.ContainsKey(missionCommonData.m_ResetID[k]))
			{
				m_MissionState.Remove(missionCommonData.m_ResetID[k]);
				MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(missionCommonData.m_ResetID[k]);
				for (int l = 0; l < missionCommonData2.m_TargetIDList.Count; l++)
				{
					m_MissionTargetState.Remove(missionCommonData2.m_TargetIDList[l]);
				}
			}
		}
		UpdateAllNpcMisTex();
		if (PeGameMgr.IsMulti)
		{
			for (int m = 0; m < missionCommonData.m_TargetIDList.Count; m++)
			{
				m_MissionTargetState.Remove(missionCommonData.m_TargetIDList[m]);
			}
		}
		if (GameConfig.IsMultiMode)
		{
			ReplyDeleteMission(MissionID);
		}
	}

	public void FailureMission(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000 || !HasMission(MissionID))
		{
			return;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return;
		}
		if (!missionCommonData.m_MissionName.Equals("0"))
		{
			new PeTipMsg("[C8C800]" + PELocalization.GetString(8000158) + missionCommonData.m_MissionName + "[-]", PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
		}
		SceneEntityCreator.self.RemoveMissionPoint(MissionID, -1);
		DelMissionInfo(MissionID);
		MissionManager.Instance.UpdateMissionMainGUI(MissionID);
		if (PeGameMgr.IsStory)
		{
			List<int> idlist = missionCommonData.HasStory(Story_Info.Story_Info_Fail);
			StroyManager.Instance.PushStoryList(idlist);
		}
		else if (PeGameMgr.IsAdventure)
		{
			List<int> idlist2 = missionCommonData.HasStory(Story_Info.Story_Info_Fail);
			StroyManager.Instance.PushAdStoryList(idlist2, missionCommonData.m_iNpc);
		}
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			switch (MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]))
			{
			case TargetType.TargetType_Follow:
			{
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
				if (typeFollowData == null)
				{
					break;
				}
				if (typeFollowData.m_LookNameID != 0)
				{
					PeEntity peEntity3 = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_LookNameID);
					if (peEntity3 != null && peEntity3.IsRecruited())
					{
						peEntity3.NpcCmpt.BaseNpcOutMission = false;
					}
				}
				if (missionCommonData.m_TargetIDList[i] == 3031)
				{
					PassengerCmpt passengerCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
					passengerCmpt.onGetOnCarrier = (Action<CarrierController>)Delegate.Remove(passengerCmpt.onGetOnCarrier, new Action<CarrierController>(MissionJoyrideOn));
					PassengerCmpt passengerCmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
					passengerCmpt2.onGetOffCarrier = (Action<CarrierController>)Delegate.Remove(passengerCmpt2.onGetOffCarrier, new Action<CarrierController>(MissionJoyrideOff));
				}
				for (int k = 0; k < typeFollowData.m_iNpcList.Count; k++)
				{
					PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[k]);
					if (!(peEntity2 == null))
					{
						if (peEntity2.GetUserData() is NpcMissionData npcMissionData)
						{
							npcMissionData.mInFollowMission = false;
						}
						NpcCmpt npcCmpt = peEntity2.NpcCmpt;
						if (npcCmpt != null)
						{
							npcCmpt.Battle = ENpcBattle.Defence;
							npcCmpt.FixedPointPos = peEntity2.position;
							npcCmpt.CanTalk = true;
						}
						if (typeFollowData.m_EMode == 1)
						{
							followers.Remove(peEntity2.NpcCmpt);
							StroyManager.Instance.RemoveReq(peEntity2, EReqType.FollowTarget);
						}
						if (typeFollowData.m_PathList.Count > 0)
						{
							StroyManager.Instance.RemoveReq(peEntity2, EReqType.FollowPath);
						}
						if (MissionManager.HasRandomMission(MissionID))
						{
							GoHome(peEntity2);
						}
					}
				}
				break;
			}
			case TargetType.TargetType_TowerDif:
			{
				if (UITowerInfo.Instance != null)
				{
					UITowerInfo.Instance.Hide();
				}
				TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(missionCommonData.m_TargetIDList[i]);
				if (typeTowerDefendsData == null)
				{
					break;
				}
				for (int j = 0; j < typeTowerDefendsData.m_iNpcList.Count; j++)
				{
					PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_iNpcList[j]);
					if (!(peEntity2 == null))
					{
						peEntity2.SetInvincible(value: true);
					}
				}
				break;
			}
			case TargetType.TargetType_Discovery:
			{
				TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(missionCommonData.m_TargetIDList[i]);
				if (typeSearchData != null && typeSearchData.m_NpcID != 0)
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
					if (peEntity != null && peEntity.IsRecruited())
					{
						peEntity.NpcCmpt.BaseNpcOutMission = false;
					}
				}
				break;
			}
			}
		}
		if (missionCommonData.m_failNpcType.Count > 0)
		{
			foreach (NpcType item in missionCommonData.m_failNpcType)
			{
				item.npcs.ForEach(delegate(int n)
				{
					PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(n);
					NpcCmpt nc;
					if (npc != null && (bool)(nc = npc.GetComponent<NpcCmpt>()))
					{
						nc.NpcControlCmdId = item.type;
					}
				});
			}
		}
		for (int l = 0; l < missionCommonData.m_ResetID.Count; l++)
		{
			if (m_MissionState.ContainsKey(missionCommonData.m_ResetID[l]))
			{
				m_MissionState.Remove(missionCommonData.m_ResetID[l]);
				MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(missionCommonData.m_ResetID[l]);
				for (int m = 0; m < missionCommonData2.m_TargetIDList.Count; m++)
				{
					m_MissionTargetState.Remove(missionCommonData2.m_TargetIDList[m]);
				}
			}
		}
		UpdateAllNpcMisTex();
		SpecialMissionFailureHandle(MissionID);
	}

	private void SpecialMissionFailureHandle(int missionID)
	{
		if (missionID == 18)
		{
			PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(9009);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9008);
			NpcCmpt npcCmpt = peEntity.NpcCmpt;
			npcCmpt.Req_Remove(EReqType.Salvation);
			StroyManager.Instance.CarryUp(npc, 9008, bCarryUp: false);
			peEntity.NpcCmpt.Req_Remove(EReqType.Idle);
			BiologyViewCmpt biologyViewCmpt = peEntity.biologyViewCmpt;
			if (null != biologyViewCmpt)
			{
				biologyViewCmpt.ActivateInjured(value: false);
			}
			StroyManager.Instance.SetIdle(peEntity, "InjuredSit");
		}
	}

	public bool IsReplyTarget(int MissionID, int TargetID)
	{
		switch (TargetID)
		{
		case 3031:
		case 3032:
			if (!HadCompleteTarget(TargetID))
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9004);
				if (peEntity == null)
				{
					return false;
				}
				if (!peEntity.IsOnCarrier())
				{
					return false;
				}
			}
			break;
		case 5004:
			if (!IsReplySpeMis())
			{
				return false;
			}
			break;
		}
		switch (MissionRepository.GetTargetType(TargetID))
		{
		case TargetType.TargetType_Collect:
		{
			TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(TargetID);
			if (typeCollectData == null)
			{
				return false;
			}
			ECreation eCreation = IsSpecialID(typeCollectData.ItemID);
			if (eCreation != 0)
			{
				int num = PeSingleton<PeCreature>.Instance.mainPlayer.GetCreationItemCount(eCreation);
				if (num < typeCollectData.ItemNum)
				{
					return false;
				}
			}
			else if (PeSingleton<PeCreature>.Instance.mainPlayer.GetPkgItemCount(typeCollectData.ItemID) < typeCollectData.ItemNum)
			{
				return false;
			}
			break;
		}
		case TargetType.TargetType_KillMonster:
		{
			TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(TargetID);
			if (typeMonsterData == null)
			{
				return false;
			}
			for (int i = 0; i < typeMonsterData.m_MonsterList.Count; i++)
			{
				int num = GetQuestVariable(MissionID, typeMonsterData.m_MonsterList[i].npcs[UnityEngine.Random.Range(0, typeMonsterData.m_MonsterList[i].npcs.Count)]);
				if (PeGameMgr.IsMulti)
				{
					foreach (int npc in typeMonsterData.m_MonsterList[i].npcs)
					{
						num += GetQuestVariable(MissionID, npc);
					}
				}
				if (num < typeMonsterData.m_MonsterList[i].type)
				{
					return false;
				}
			}
			break;
		}
		case TargetType.TargetType_UseItem:
		{
			TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(TargetID);
			if (typeUseItemData == null)
			{
				return false;
			}
			int num = GetQuestVariable(MissionID, typeUseItemData.m_ItemID);
			if (num < typeUseItemData.m_UseNum)
			{
				return false;
			}
			break;
		}
		case TargetType.TargetType_TowerDif:
		{
			Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
			if (missionFlagType == null)
			{
				return false;
			}
			foreach (KeyValuePair<string, string> item in missionFlagType)
			{
				if (!(item.Key == MissionFlagStep))
				{
					string[] array = item.Value.Split('_');
					if (array.Length != 5)
					{
						return false;
					}
					int num = Convert.ToInt32(array[1]);
					int num2 = Convert.ToInt32(array[2]);
					if (num2 > num)
					{
						return false;
					}
					int num3 = Convert.ToInt32(array[3]);
					if (num3 != 1)
					{
						return false;
					}
					if (Convert.ToInt32(array[4]) == 0)
					{
						return false;
					}
				}
			}
			break;
		}
		}
		return true;
	}

	public bool IsShowWnd(NpcCmpt npc)
	{
		if (followers.Contains(npc) || ServantLeaderCmpt.Instance.mFollowers.Contains(npc) || ServantLeaderCmpt.Instance.mForcedFollowers.Contains(npc))
		{
			return false;
		}
		return true;
	}

	public bool IsReplySpeMis()
	{
		if (MissionManager.Instance.HasMission(242))
		{
			int num = 0;
			for (int i = 0; i < MissionManager.Instance.m_PlayerMission.m_SpeVecList.Count; i++)
			{
				if (MissionManager.Instance.m_PlayerMission.m_SpeVecList[i] == Vector3.zero)
				{
					return false;
				}
				Vector3 vector = MissionManager.Instance.m_PlayerMission.m_SpeVecList[i];
				IntVector3 intVector = new IntVector3(Mathf.FloorToInt(vector.x * 2f), Mathf.FloorToInt(vector.y * 2f), Mathf.FloorToInt(vector.z * 2f));
				bool flag = false;
				for (int j = 0; j < 5; j++)
				{
					if (Block45Man.self.DataSource.SafeRead(intVector.x, intVector.y + j, intVector.z).blockType >> 2 != 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					num++;
				}
			}
			if (num > 1)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsReplyMission(int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (!missionCommonData.IsTalkMission() && (!HasMission(MissionID) || HadCompleteMission(MissionID)))
		{
			return false;
		}
		if (m_MissionTime.ContainsKey(MissionID))
		{
			double num = m_MissionTime[MissionID];
			if (GameTime.Timer.Second - num < (double)missionCommonData.m_NeedTime)
			{
				return false;
			}
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < missionCommonData.m_Com_RewardItem.Count; i++)
		{
			ItemProto itemData = ItemProto.GetItemData(missionCommonData.m_Com_RewardItem[i].id);
			if (itemData == null)
			{
				continue;
			}
			if (itemData.tabIndex == 0 && itemData.maxStackNum > 0)
			{
				num2 += (missionCommonData.m_Com_RewardItem[i].num - 1) / itemData.maxStackNum + 1;
			}
			if (itemData.tabIndex == 1)
			{
				if (itemData.maxStackNum > 0)
				{
					num3 += (missionCommonData.m_Com_RewardItem[i].num - 1) / itemData.maxStackNum + 1;
				}
			}
			else if (itemData.maxStackNum > 0)
			{
				num4 += (missionCommonData.m_Com_RewardItem[i].num - 1) / itemData.maxStackNum + 1;
			}
		}
		if (missionCommonData.m_Type != MissionType.MissionType_Mul)
		{
			for (int j = 0; j < missionCommonData.m_TargetIDList.Count; j++)
			{
				TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[j]);
				if (targetType != TargetType.TargetType_Discovery)
				{
					if (targetType == TargetType.TargetType_Follow && missionCommonData.m_TargetIDList[j] % 1000 <= 500 && !HadCompleteTarget(missionCommonData.m_TargetIDList[j]))
					{
						return false;
					}
					if (!IsReplyTarget(MissionID, missionCommonData.m_TargetIDList[j]))
					{
						return false;
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < missionCommonData.m_TargetIDList.Count && !IsReplyTarget(MissionID, missionCommonData.m_TargetIDList[k]); k++)
			{
			}
		}
		if (MissionID == 497)
		{
			CSCreator creator = CSMain.GetCreator(0);
			if (creator == null)
			{
				return false;
			}
			if (creator.Assembly == null)
			{
				return false;
			}
			Vector3 position = creator.Assembly.Position;
			MapMaskData mapMaskData = MapMaskData.s_tblMaskData.Find((MapMaskData ret) => ret.mId == 29);
			if (mapMaskData == null)
			{
				return false;
			}
			Vector3 mPosition = mapMaskData.mPosition;
			return PERailwayCtrl.HasRoute(position, mPosition);
		}
		return true;
	}

	private void ProcessRandomMission(int MissionID, MissionCommonData data)
	{
		if (MissionRepository.m_MissionCommonMap.ContainsKey(MissionID))
		{
			if (data.m_TargetIDList.Count > 0)
			{
				MissionRepository.DeleteRandomMissionData(data.m_TargetIDList[0]);
			}
			MissionRepository.m_MissionCommonMap.Remove(MissionID);
		}
		if (PeGameMgr.IsBuild)
		{
			return;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get((data.m_iReplyNpc == 0) ? data.m_iNpc : data.m_iReplyNpc);
		if (peEntity == null || !(peEntity.GetUserData() is NpcMissionData npcMissionData) || npcMissionData.m_RandomMission != MissionID)
		{
			return;
		}
		if (npcMissionData.m_MissionListReply.Contains(MissionID))
		{
			npcMissionData.m_MissionListReply.Remove(MissionID);
		}
		npcMissionData.mCurComMisNum++;
		if (!PeGameMgr.IsAdventure && !PeGameMgr.IsMultiAdventure && !PeGameMgr.IsMultiBuild)
		{
			return;
		}
		if (data.m_MaxNum != -1)
		{
			npcMissionData.m_CurMissionGroup++;
		}
		int randomMission = AdRMRepository.GetRandomMission(npcMissionData);
		npcMissionData.m_RandomMission = randomMission;
		if (MissionManager.IsTalkMission(MissionID))
		{
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, npcMissionData.m_RandomMission, peEntity.Id);
			}
			else
			{
				AdRMRepository.CreateRandomMission(npcMissionData.m_RandomMission);
			}
		}
	}

	private int ProcessCompleteTarget(int TargetID, int MissionID, bool bFromNet = false)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return -1;
		}
		int num = TargetID % 1000;
		if (num < 500)
		{
			if (PeGameMgr.IsMulti && !bFromNet)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_CompleteTarget, TargetID, MissionID);
				return 0;
			}
			m_MissionTargetState[TargetID] = 1;
		}
		List<int> list = null;
		TargetType targetType = MissionRepository.GetTargetType(TargetID);
		switch (targetType)
		{
		case TargetType.TargetType_Collect:
		{
			TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(TargetID);
			if (typeCollectData != null)
			{
				list = typeCollectData.m_ReceiveList;
			}
			break;
		}
		case TargetType.TargetType_Follow:
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetName() != m_FollowPlayerName && m_FollowPlayerName != null)
			{
				return -1;
			}
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(TargetID);
			if (typeFollowData == null)
			{
				return -1;
			}
			if (typeFollowData.m_LookNameID != 0)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_LookNameID);
				if (peEntity.IsRecruited())
				{
					peEntity.NpcCmpt.BaseNpcOutMission = false;
				}
			}
			if (TargetID == 3031)
			{
				PassengerCmpt passengerCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
				passengerCmpt.onGetOnCarrier = (Action<CarrierController>)Delegate.Remove(passengerCmpt.onGetOnCarrier, new Action<CarrierController>(MissionJoyrideOn));
				PassengerCmpt passengerCmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
				passengerCmpt2.onGetOffCarrier = (Action<CarrierController>)Delegate.Remove(passengerCmpt2.onGetOffCarrier, new Action<CarrierController>(MissionJoyrideOff));
			}
			list = typeFollowData.m_ReceiveList;
			if (typeFollowData.m_ComTalkID.Count > 0)
			{
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionCommonData.m_ID, 5, TargetID);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
			{
				if (TargetID == 3099)
				{
					break;
				}
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
				if (peEntity == null)
				{
					continue;
				}
				if (!(peEntity.GetUserData() is NpcMissionData npcMissionData))
				{
					return -1;
				}
				if (npcMissionData.m_bRandomNpc)
				{
					peEntity.SetAttackMode(EAttackMode.Attack);
					peEntity.SetInvincible(value: true);
				}
				npcMissionData.mInFollowMission = false;
				if (TargetID == 3018)
				{
					peEntity.DisableMoveCheck();
				}
				NpcCmpt npcCmpt = peEntity.NpcCmpt;
				if (!(null != npcCmpt))
				{
					continue;
				}
				if (!typeFollowData.m_isNeedReturn)
				{
					npcCmpt.FixedPointPos = peEntity.position;
				}
				if (typeFollowData.m_EMode == 1)
				{
					StroyManager.Instance.RemoveReq(peEntity, EReqType.FollowTarget);
					peEntity.NpcCmpt.CanTalk = true;
					if (followers.Contains(npcCmpt))
					{
						followers.Remove(npcCmpt);
					}
				}
				npcCmpt.Battle = ENpcBattle.Defence;
			}
			break;
		}
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(TargetID);
			if (typeSearchData == null)
			{
				break;
			}
			if (typeSearchData.m_NpcID != 0)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
				if (peEntity.IsRecruited())
				{
					peEntity.NpcCmpt.BaseNpcOutMission = false;
				}
			}
			list = typeSearchData.m_ReceiveList;
			if (typeSearchData.m_Prompt.Count > 0 || typeSearchData.m_TalkID.Count > 0)
			{
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionCommonData.m_ID, 5, TargetID);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			break;
		}
		case TargetType.TargetType_UseItem:
		{
			TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(TargetID);
			if (typeUseItemData != null)
			{
				list = typeUseItemData.m_ReceiveList;
			}
			break;
		}
		case TargetType.TargetType_Messenger:
		{
			TypeMessengerData typeMessengerData = MissionManager.GetTypeMessengerData(TargetID);
			if (typeMessengerData != null)
			{
				list = typeMessengerData.m_ReceiveList;
				if (PeGameMgr.IsSingle)
				{
					PeSingleton<PeCreature>.Instance.mainPlayer.RemoveFromPkg(typeMessengerData.m_ItemID, typeMessengerData.m_ItemNum);
				}
				else
				{
					Debug.LogError("PeCreature.Instance.mainPlayer.RemoveFromPkg");
				}
			}
			break;
		}
		case TargetType.TargetType_KillMonster:
		{
			TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(TargetID);
			if (typeMonsterData != null)
			{
				list = typeMonsterData.m_ReceiveList;
			}
			break;
		}
		case TargetType.TargetType_TowerDif:
		{
			TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(TargetID);
			if (typeTowerDefendsData == null)
			{
				break;
			}
			list = typeTowerDefendsData.m_ReceiveList;
			for (int i = 0; i < typeTowerDefendsData.m_iNpcList.Count; i++)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_iNpcList[i]);
				if (!(peEntity == null))
				{
					peEntity.SetInvincible(value: true);
				}
			}
			if (UITowerInfo.Instance != null)
			{
				UITowerInfo.Instance.Hide();
			}
			break;
		}
		}
		if (list != null && GameUI.Instance != null && MissionManager.Instance != null)
		{
			for (int k = 0; k < list.Count; k++)
			{
				if (MissionRepository.HaveTalkOP(list[k]))
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(list[k], 1);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				else if (IsGetTakeMission(list[k]))
				{
					MissionManager.Instance.SetGetTakeMission(list[k], null, MissionManager.TakeMissionType.TakeMissionType_Get);
				}
			}
		}
		if (targetType == TargetType.TargetType_Follow || targetType == TargetType.TargetType_Discovery)
		{
			MissionManager.Instance.UpdateMissionTrack(MissionID, 0, TargetID);
		}
		return 1;
	}

	public void CompleteTarget(int TargetID, int MissionID, bool forceComplete = false, bool bFromNet = false, bool isOwner = true)
	{
		if ((!forceComplete && HadCompleteTarget(TargetID)) || (!forceComplete && !IsReplyTarget(MissionID, TargetID)) || ProcessCompleteTarget(TargetID, MissionID, bFromNet) == 0)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (PeGameMgr.IsMultiStory)
			{
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
				if (missionCommonData == null)
				{
					return;
				}
				int num = TargetID % 1000;
				bool flag = true;
				for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
				{
					if (!HadCompleteTarget(missionCommonData.m_TargetIDList[i]) && num < 500)
					{
						flag = false;
						break;
					}
				}
				if (flag && isOwner)
				{
					RequestCompleteMission(MissionID, -1, bCheck: false);
				}
			}
			else if (isOwner)
			{
				RequestCompleteMission(MissionID, TargetID, bCheck: false);
			}
			return;
		}
		MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData2 == null)
		{
			return;
		}
		int num2 = TargetID % 1000;
		bool flag2 = true;
		for (int j = 0; j < missionCommonData2.m_TargetIDList.Count; j++)
		{
			if (!HadCompleteTarget(missionCommonData2.m_TargetIDList[j]) && num2 < 500)
			{
				flag2 = false;
				break;
			}
			if (ForceCompleteMission(missionCommonData2.m_TargetIDList[j]))
			{
				flag2 = true;
				break;
			}
		}
		if (flag2)
		{
			UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(MissionID);
			if (missionView != null)
			{
				missionView.mComplete = true;
			}
			TargetType targetType = MissionRepository.GetTargetType(TargetID);
			if (missionCommonData2.isAutoReply || MissionRepository.IsAutoReplyMission(MissionID) || (RMRepository.HasRandomMission(MissionID) && (targetType == TargetType.TargetType_Follow || targetType == TargetType.TargetType_Discovery)))
			{
				MissionManager.Instance.CompleteMission(MissionID, -1, bCheck: false);
			}
			else
			{
				UpdateAllNpcMisTex();
			}
		}
	}

	private bool ForceCompleteMission(int targetid)
	{
		TargetType targetType = MissionRepository.GetTargetType(targetid);
		if (targetType != TargetType.TargetType_UseItem)
		{
			return false;
		}
		return MissionManager.GetTypeUseItemData(targetid)?.m_comMission ?? false;
	}

	public void CompleteMission(int MissionID, int TargetID = -1, bool bCheck = true, bool pushStory = true)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null || (!missionCommonData.IsTalkMission() && bCheck && (!HasMission(MissionID) || HadCompleteMission(MissionID))) || (!GameConfig.IsMultiMode && bCheck && !IsReplyMission(MissionID)))
		{
			return;
		}
		if (!missionCommonData.m_MissionName.Equals("0"))
		{
			new PeTipMsg("[C8C800]" + PELocalization.GetString(8000157) + missionCommonData.m_MissionName + "[-]", PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
		}
		InGameAidData.CheckCompleteTask(MissionID);
		if (PeGameMgr.IsAdventure && RandomMapConfig.useSkillTree && missionCommonData.addSpValue != 0)
		{
			SkEntity component = PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<SkEntity>();
			if (null != component)
			{
				List<int> list = new List<int>();
				list.Add(0);
				List<float> list2 = new List<float>();
				list2.Add(5f);
				SkEntity.MountBuff(component, 30200126, list, list2);
				new PeTipMsg("[C8C800]" + PELocalization.GetString(82209005) + list2[0], PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
			}
		}
		if (missionCommonData.m_increaseChain)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get((missionCommonData.m_iReplyNpc == 0) ? missionCommonData.m_iNpc : missionCommonData.m_iReplyNpc);
			if (peEntity == null || !(peEntity.GetUserData() is NpcMissionData npcMissionData))
			{
				return;
			}
			npcMissionData.m_CurMissionGroup++;
			npcMissionData.m_RandomMission = AdRMRepository.GetRandomMission(npcMissionData);
		}
		if (missionCommonData.m_changeReputation[0] == 1)
		{
			int playerID = Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
			int targetPlayerID = Mathf.RoundToInt(GameUI.Instance.mNpcWnd.m_CurSelNpc.GetAttribute(AttribType.DefaultPlayerID));
			if (missionCommonData.m_changeReputation[1] == 0)
			{
				PeSingleton<ReputationSystem>.Instance.SetReputationValue(playerID, targetPlayerID, missionCommonData.m_changeReputation[2]);
			}
			else
			{
				PeSingleton<ReputationSystem>.Instance.ChangeReputationValue(playerID, targetPlayerID, missionCommonData.m_changeReputation[1] * missionCommonData.m_changeReputation[2]);
			}
		}
		SceneEntityCreator.self.RemoveMissionPoint(MissionID, TargetID);
		SpecialMissionEndHandle(MissionID);
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]);
			if (targetType == TargetType.TargetType_Collect)
			{
				GetSpecialItem.RemoveLootSpecialItem(missionCommonData.m_TargetIDList[i]);
				TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(missionCommonData.m_TargetIDList[i]);
				if (typeCollectData != null && typeCollectData.m_randItemID.Count > 1)
				{
					PeSingleton<PeCreature>.Instance.mainPlayer.RemoveFromPkg(typeCollectData.ItemID, typeCollectData.ItemNum);
				}
			}
		}
		DelMissionInfo(MissionID);
		MissionManager.Instance.UpdateMissionMainGUI(MissionID);
		if (!MissionManager.HasRandomMission(MissionID))
		{
			if (missionCommonData.m_GuanLianList.Count == 0)
			{
				SetMission(MissionID);
			}
		}
		else
		{
			if (GameConfig.IsMultiMode)
			{
				if (PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetName() == m_FollowPlayerName || m_FollowPlayerName == null)
				{
					ProcessRandomMission(MissionID, missionCommonData);
				}
			}
			else
			{
				ProcessRandomMission(MissionID, missionCommonData);
			}
			SetMission(MissionID);
		}
		string text = string.Empty;
		string text2 = "receive item :";
		string text3 = string.Empty;
		if (!GameConfig.IsMultiMode)
		{
			for (int j = 0; j < missionCommonData.m_Com_RemoveItem.Count; j++)
			{
				ECreation eCreation = IsSpecialID(missionCommonData.m_Com_RemoveItem[j].id);
				if (eCreation != 0)
				{
					DelSpecialItem((int)eCreation, missionCommonData.m_Com_RemoveItem[j].id, missionCommonData.m_Com_RemoveItem[j].num);
				}
				else if (missionCommonData.m_Com_RemoveItem[j].id > 0)
				{
					PeSingleton<PeCreature>.Instance.mainPlayer.RemoveFromPkg(missionCommonData.m_Com_RemoveItem[j].id, missionCommonData.m_Com_RemoveItem[j].num);
				}
				ProcessCollectMissionByID(missionCommonData.m_Com_RemoveItem[j].id);
			}
			List<MissionIDNum> list3 = null;
			if (missionCommonData.m_Type == MissionType.MissionType_Mul)
			{
				int num = 0;
				for (int k = 0; k < missionCommonData.m_TargetIDList.Count; k++)
				{
					if (IsReplyTarget(missionCommonData.m_ID, missionCommonData.m_TargetIDList[k]))
					{
						num++;
					}
				}
				num--;
				if (num >= 0 && missionCommonData.m_Com_MulRewardItem.ContainsKey(num))
				{
					list3 = missionCommonData.m_Com_MulRewardItem[num];
				}
			}
			else
			{
				list3 = missionCommonData.m_Com_RewardItem;
			}
			if (list3 != null)
			{
				for (int l = 0; l < list3.Count; l++)
				{
					if (list3[l].id > 0)
					{
						ItemProto itemData = ItemProto.GetItemData(list3[l].id);
						if (itemData != null && PeGender.IsMatch(itemData.equipSex, PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetSex()))
						{
							PeSingleton<PeCreature>.Instance.mainPlayer.AddToPkg(list3[l].id, list3[l].num);
							string text4 = text;
							text = text4 + "GetItem: " + itemData.GetName() + "  Num: " + list3[l].num;
							text += "\n";
							text4 = text3;
							text3 = text4 + " " + itemData.GetName() + " x" + list3[l].num;
						}
					}
				}
			}
			if (null != GameUI.Instance)
			{
				GameUI.Instance.mItemPackageCtrl.ResetItem();
			}
		}
		if (PeGameMgr.IsStory && pushStory)
		{
			List<int> idlist = missionCommonData.HasStory(Story_Info.Story_Info_Complete);
			StroyManager.Instance.PushStoryList(idlist);
		}
		else if (PeGameMgr.IsAdventure)
		{
			List<int> idlist2 = missionCommonData.HasStory(Story_Info.Story_Info_Complete);
			StroyManager.Instance.PushAdStoryList(idlist2, missionCommonData.m_iNpc);
		}
		if (MissionRepository.HaveTalkED(missionCommonData.m_ID, TargetID) || missionCommonData.m_PromptED.Count > 0)
		{
			if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
			{
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionCommonData.m_ID, 3, TargetID);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			else
			{
				GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionCommonData.m_ID, 3, TargetID);
			}
			if (missionCommonData.m_PromptED.Count > 0)
			{
				GameUI.Instance.mNPCTalk.SpTalkSymbol(spOrHalf: true);
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionCommonData.m_ID, 8, 0, bFailed: false, isClearTalkList: true);
				}
				else
				{
					GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionCommonData.m_ID, 8);
				}
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.SPTalkClose();
				}
			}
		}
		else
		{
			for (int m = 0; m < missionCommonData.m_EDID.Count; m++)
			{
				if (PeGameMgr.IsAdventure && MissionManager.HasRandomMission(missionCommonData.m_EDID[m]))
				{
					if (PeGameMgr.IsSingle)
					{
						AdRMRepository.CreateRandomMission(missionCommonData.m_EDID[m]);
					}
					else
					{
						PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, missionCommonData.m_EDID[m], missionCommonData.m_iNpc);
					}
				}
				if (!IsGetTakeMission(missionCommonData.m_EDID[m]))
				{
					continue;
				}
				if (MissionRepository.HaveTalkOP(missionCommonData.m_EDID[m]))
				{
					GameUI.Instance.mNPCTalk.NormalOrSP(0);
					if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
					{
						GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionCommonData.m_EDID[m], 1);
						GameUI.Instance.mNPCTalk.PreShow();
					}
					else
					{
						GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionCommonData.m_EDID[m], 1);
					}
				}
				else if (IsGetTakeMission(missionCommonData.m_EDID[m]))
				{
					if (PeGameMgr.IsAdventure)
					{
						MissionManager.Instance.SetGetTakeMission(missionCommonData.m_EDID[m], (missionCommonData.m_iNpc == 0) ? GameUI.Instance.mNpcWnd.m_CurSelNpc : PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc), MissionManager.TakeMissionType.TakeMissionType_Get);
						continue;
					}
					if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
					{
						MissionManager.Instance.SetGetTakeMission(missionCommonData.m_EDID[m], GameUI.Instance.mNpcWnd.m_CurSelNpc, MissionManager.TakeMissionType.TakeMissionType_Get);
						continue;
					}
					MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(missionCommonData.m_EDID[m]);
					MissionManager.Instance.SetGetTakeMission(missionCommonData.m_EDID[m], PeSingleton<EntityMgr>.Instance.Get(missionCommonData2.m_iNpc), MissionManager.TakeMissionType.TakeMissionType_Get);
				}
			}
		}
		if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
		{
			GameUI.Instance.mNpcWnd.m_CurSelNpc.CmdFaceToPoint(Vector3.zero);
		}
		UpdateAllNpcMisTex();
		if (!GameConfig.IsMultiMode && missionCommonData.m_Type == MissionType.MissionType_Var && missionCommonData.m_VarValueID != 0)
		{
			string questVariable = GetQuestVariable(MissionID, MissionFlagStep);
			int num2 = Convert.ToInt32(questVariable) + missionCommonData.m_VarValue;
			ModifyQuestVariable(missionCommonData.m_VarValueID, MissionFlagStep, num2.ToString());
		}
		if (!missionCommonData.IsTalkMission())
		{
			for (int n = 0; n < missionCommonData.m_PromptED.Count; n++)
			{
				GameUI.Instance.mServantTalk.AddTalk(missionCommonData.m_PromptED[n]);
			}
			if (text3 != string.Empty)
			{
				text3 = text2 + text3;
			}
			Debug.Log("mission is complete!!!!");
			if (GameUI.Instance != null && GameUI.Instance.mNpcWnd != null)
			{
				GameUI.Instance.mNpcWnd.UpdateMission();
			}
			SteamAchievementsSystem.Instance.OnMissionChange(MissionID);
		}
	}

	private void SpecialMissionEndHandle(int missionID)
	{
		switch (missionID)
		{
		case 242:
			m_SpeVecList.Clear();
			break;
		case 629:
			m_SpeVecList.Clear();
			break;
		case 500:
			GlobalEvent.OnPlayerGetOnTrain -= StroyManager.Instance.PlayerGetOnTrain;
			break;
		case 125:
		{
			GameObject gameObject3 = GameObject.Find("alien_cage_01B");
			if (gameObject3 != null)
			{
				BoxCollider componentInChildren2 = gameObject3.GetComponentInChildren<BoxCollider>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.enabled = false;
				}
			}
			gameObject3 = GameObject.Find("alien_cage_01A");
			if (gameObject3 != null)
			{
				MeshCollider component3 = gameObject3.GetComponent<MeshCollider>();
				if (component3 != null)
				{
					component3.enabled = false;
				}
			}
			break;
		}
		case 126:
		{
			GameObject gameObject2 = GameObject.Find("alien_cage_01B");
			if (gameObject2 != null)
			{
				BoxCollider componentInChildren = gameObject2.GetComponentInChildren<BoxCollider>();
				if (componentInChildren != null)
				{
					componentInChildren.enabled = false;
				}
			}
			gameObject2 = GameObject.Find("alien_cage_01A");
			if (gameObject2 != null)
			{
				MeshCollider component2 = gameObject2.GetComponent<MeshCollider>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
			}
			break;
		}
		case 710:
		{
			PeEntity curSelNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
			NpcMissionData npcMissionData2 = curSelNpc.GetUserData() as NpcMissionData;
			if (npcMissionData2.m_MissionList.Contains(710))
			{
				npcMissionData2.m_MissionList.Remove(710);
			}
			break;
		}
		case 628:
			foreach (PeEntity item in PeSingleton<EntityMgr>.Instance.All)
			{
				if ((item.proto == EEntityProto.Npc || item.proto == EEntityProto.RandomNpc) && item.GetUserData() is NpcMissionData npcMissionData && npcMissionData.m_MissionList.Contains(710))
				{
					npcMissionData.m_MissionList.Remove(710);
				}
			}
			break;
		case 848:
		{
			for (int i = 0; i < recordCreationName.Count; i++)
			{
				GameObject gameObject = GameObject.Find(recordCreationName[i]);
				if (!(gameObject == null))
				{
					DragItemMousePick component = gameObject.GetComponent<DragItemMousePick>();
					if (!(component == null))
					{
						component.cancmd = true;
					}
				}
			}
			isRecordCreation = false;
			break;
		}
		}
		if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
		{
			TrainingTaskManager.Instance.CompleteMission(missionID);
		}
	}

	public bool ProcessMonsterDead(int proid, int autoid)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(item.Key);
			if (missionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				switch (MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]))
				{
				case TargetType.TargetType_KillMonster:
				{
					TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(missionCommonData.m_TargetIDList[i]);
					if (typeMonsterData == null)
					{
						break;
					}
					for (int m = 0; m < typeMonsterData.m_MonsterList.Count; m++)
					{
						int num8 = i * 10 + m;
						if (typeMonsterData.m_MonsterList[m].npcs.Contains(proid))
						{
							string missionFlag3 = MissionFlagMonster + num8;
							string questVariable3 = GetQuestVariable(item.Key, missionFlag3);
							string[] array3 = questVariable3.Split('_');
							int num9 = Convert.ToInt32(array3[1]) + 1;
							questVariable3 = array3[0] + "_" + num9;
							ModifyQuestVariable(item.Key, missionFlag3, questVariable3);
							if (MissionRepository.IsAutoReplyMission(missionCommonData.m_ID) && !dictionary.ContainsKey(missionCommonData.m_TargetIDList[i]))
							{
								dictionary.Add(missionCommonData.m_TargetIDList[i], missionCommonData.m_ID);
							}
						}
					}
					break;
				}
				case TargetType.TargetType_TowerDif:
				{
					if (!EntityCreateMgr.Instance.m_TowerDefineMonsterMap.ContainsKey(autoid))
					{
						break;
					}
					TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(missionCommonData.m_TargetIDList[i]);
					if (typeTowerDefendsData == null)
					{
						break;
					}
					AISpawnAutomatic automatic = AISpawnAutomatic.GetAutomatic(typeTowerDefendsData.m_TdInfoId);
					if (automatic == null)
					{
						break;
					}
					for (int j = 0; j < automatic.data.Count; j++)
					{
						AISpawnWaveData aISpawnWaveData = automatic.data[j];
						if (aISpawnWaveData == null)
						{
							continue;
						}
						bool flag = true;
						for (int k = 0; k < aISpawnWaveData.data.data.Count; k++)
						{
							AISpawnData aISpawnData = aISpawnWaveData.data.data[k];
							if (aISpawnData == null)
							{
								continue;
							}
							int num = i * 100 + j * 10 + k;
							string missionFlag = MissionFlagTDMonster + num;
							string questVariable = GetQuestVariable(item.Key, missionFlag);
							string[] array = questVariable.Split('_');
							if (array.Length != 5)
							{
								flag = false;
								continue;
							}
							int num2 = Convert.ToInt32(array[3]);
							if (num2 != 1)
							{
								flag = false;
								break;
							}
							int num3 = Convert.ToInt32(array[4]);
							if (num3 == 1)
							{
								flag = false;
								break;
							}
							int num4 = Convert.ToInt32(array[1]);
							int num5 = Convert.ToInt32(array[2]);
							if (Convert.ToInt32(array[0]) != proid)
							{
								if (num5 > num4)
								{
									flag = false;
								}
								continue;
							}
							object[] obj = new object[9]
							{
								array[0],
								"_",
								null,
								null,
								null,
								null,
								null,
								null,
								null
							};
							int num6 = ++num4;
							obj[2] = num6.ToString();
							obj[3] = "_";
							obj[4] = array[2];
							obj[5] = "_";
							obj[6] = num2;
							obj[7] = "_";
							obj[8] = num3;
							questVariable = string.Concat(obj);
							ModifyQuestVariable(item.Key, missionFlag, questVariable);
							m_TowerUIData.bRefurbish = true;
							if (num5 > num4)
							{
								flag = false;
							}
						}
						if (!flag)
						{
							continue;
						}
						for (int l = 0; l < aISpawnWaveData.data.data.Count; l++)
						{
							AISpawnData aISpawnData2 = aISpawnWaveData.data.data[l];
							if (aISpawnData2 != null)
							{
								int num7 = i * 100 + j * 10 + l;
								string missionFlag2 = MissionFlagTDMonster + num7;
								string questVariable2 = GetQuestVariable(item.Key, missionFlag2);
								string[] array2 = questVariable2.Split('_');
								if (array2.Length != 5)
								{
									flag = false;
									break;
								}
								string text = "_1";
								questVariable2 = array2[0] + "_" + array2[1] + "_" + array2[2] + "_" + array2[3] + text;
								ModifyQuestVariable(item.Key, missionFlag2, questVariable2);
							}
						}
						if (j + 1 >= automatic.data.Count)
						{
							if (MissionRepository.IsAutoReplyMission(item.Key) && !dictionary.ContainsKey(typeTowerDefendsData.m_TargetID))
							{
								dictionary.Add(typeTowerDefendsData.m_TargetID, item.Key);
							}
						}
						else
						{
							EntityCreateMgr.Instance.StartTowerMission(item.Key, j + 1, typeTowerDefendsData, 0f);
						}
						break;
					}
					break;
				}
				}
			}
		}
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			MissionManager.Instance.CompleteTarget(item2.Key, item2.Value);
		}
		return true;
	}

	public bool CheckHeroMis()
	{
		if (GameUI.Instance.mNpcWnd.m_CurSelNpc == null)
		{
			return false;
		}
		if (!(GameUI.Instance.mNpcWnd.m_CurSelNpc.GetUserData() is NpcMissionData npcMissionData))
		{
			return false;
		}
		if (!HasMission(npcMissionData.m_RandomMission))
		{
			return true;
		}
		GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(562, 1);
		GameUI.Instance.mNPCTalk.PreShow();
		return false;
	}

	public bool CheckCSCreatorMis(int MissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.m_ColonyMis[0] == 0)
		{
			for (int i = 0; i < missionCommonData.m_iColonyNpcList.Count; i++)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iColonyNpcList[i]);
				if (!(peEntity == null) && !peEntity.IsRecruited())
				{
					List<int> list = new List<int>();
					list.Add(2173);
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(list, missionCommonData);
					GameUI.Instance.mNPCTalk.PreShow();
					return false;
				}
			}
		}
		return true;
	}

	private void DeleteMission()
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(mOpMissionID);
		if (missionCommonData == null)
		{
			return;
		}
		for (int i = 0; i < missionCommonData.m_DeleteID.Count; i++)
		{
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, missionCommonData.m_DeleteID[i]);
			}
			else
			{
				DelMissionInfo(missionCommonData.m_DeleteID[i]);
				MissionManager.Instance.UpdateMissionMainGUI(missionCommonData.m_DeleteID[i]);
			}
			if (GameUI.Instance.mNpcWnd.isShow)
			{
				GameUI.Instance.mNpcWnd.Hide();
			}
		}
		if (PeGameMgr.IsMulti)
		{
			mOpMissionID = 0;
			mOpMissionFlag = string.Empty;
			mOpMissionValue = string.Empty;
		}
		else
		{
			SetQuestVariable1(mOpMissionID, mOpMissionFlag, mOpMissionValue);
			Debug.Log("\ufffd\u05b6ε\ufffdID:" + mOpMissionID);
			mOpMissionID = 0;
			mOpMissionFlag = string.Empty;
			mOpMissionValue = string.Empty;
		}
	}

	private bool CheckGetMission(int MissionID, string MissionFlag, string MissionValue)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		string text = PELocalization.GetString(8000008);
		bool flag = false;
		if (missionCommonData.m_DeleteID.Count > 0)
		{
			for (int i = 0; i < missionCommonData.m_DeleteID.Count; i++)
			{
				if (HasMission(missionCommonData.m_DeleteID[i]))
				{
					flag = true;
				}
				text = text + "\"" + MissionRepository.GetMissionName(missionCommonData.m_DeleteID[i]) + "\"";
				if (missionCommonData.m_DeleteID.Count - 1 > i)
				{
					text += ", ";
				}
			}
			if (flag)
			{
				text += PELocalization.GetString(8000009);
				mOpMissionID = MissionID;
				mOpMissionFlag = MissionFlag;
				mOpMissionValue = MissionValue;
				MessageBox_N.ShowYNBox(text, DeleteMission);
				return false;
			}
		}
		for (int j = 0; j < missionCommonData.m_TargetIDList.Count; j++)
		{
			TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[j]);
			if (targetType != TargetType.TargetType_TowerDif && targetType != TargetType.TargetType_Follow)
			{
				continue;
			}
			foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
			{
				MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(item.Key);
				if (missionCommonData2 == null)
				{
					continue;
				}
				for (int k = 0; k < missionCommonData2.m_TargetIDList.Count; k++)
				{
					TargetType targetType2 = MissionRepository.GetTargetType(missionCommonData2.m_TargetIDList[k]);
					if (targetType2 == TargetType.TargetType_TowerDif || targetType2 == TargetType.TargetType_Follow)
					{
						if (missionCommonData2.m_TargetIDList[k] != missionCommonData.m_TargetIDList[j])
						{
							text = PELocalization.GetString(8000007);
							MessageBox_N.ShowOkBox(text);
						}
						return false;
					}
				}
			}
		}
		return true;
	}

	public int SetQuestVariable(int MissionID, string MissionFlag, string MissionValue, bool pushStory = true, bool isRecord = false)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return 0;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return 0;
		}
		if (MissionFlag == MissionFlagStep && !CheckGetMission(MissionID, MissionFlag, MissionValue))
		{
			return 1;
		}
		bool flag = true;
		if (SingleGameStory.curType == SingleGameStory.StoryScene.MainLand && missionCommonData.m_ColonyMis[0] == 0)
		{
			for (int i = 0; i < missionCommonData.m_iColonyNpcList.Count; i++)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iColonyNpcList[i]);
				if (!(peEntity == null))
				{
					CSMain.GetAssemblyPos(out var _);
					if (!peEntity.IsRecruited())
					{
						List<int> list = new List<int>();
						list.Add(2173);
						GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(list, missionCommonData);
						GameUI.Instance.mNPCTalk.PreShow();
						flag = false;
						return 1;
					}
				}
			}
		}
		if (flag)
		{
			for (int j = 0; j < missionCommonData.m_iColonyNpcList.Count; j++)
			{
				PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iColonyNpcList[j]);
				if (peEntity2 == null)
				{
					continue;
				}
				CSMain.GetAssemblyPos(out var pos2);
				if (Vector3.Distance(peEntity2.position, pos2) > 150f)
				{
					CSPersonnel colonyNpc = CSMain.GetColonyNpc(peEntity2.Id);
					if (colonyNpc != null)
					{
						colonyNpc.TrySetOccupation(0);
						StroyManager.Instance.MoveTo(peEntity2, pos2, 1f);
					}
				}
			}
		}
		SetQuestVariable1(MissionID, MissionFlag, MissionValue, pushStory, isRecord);
		return 1;
	}

	public int SetQuestVariable1(int MissionID, string MissionFlag, string MissionValue, bool pushStory = true, bool isRecord = false)
	{
		if ((MissionID == 697 || MissionID == 714) && (bool)PeSingleton<MainPlayer>.Instance.entity.GetCmpt<Motion_Equip>())
		{
			PeSingleton<MainPlayer>.Instance.entity.GetCmpt<Motion_Equip>().ActiveWeapon(active: false);
		}
		if (MissionID < 0 || MissionID > 20000)
		{
			return 0;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return 0;
		}
		if (!missionCommonData.m_MissionName.Equals("0"))
		{
			new PeTipMsg("[C8C800]" + PELocalization.GetString(8000156) + missionCommonData.m_MissionName + "[-]", PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
		}
		InGameAidData.CheckJoinMission(MissionID);
		if (missionCommonData.m_npcType.Count > 0)
		{
			foreach (NpcType item in missionCommonData.m_npcType)
			{
				item.npcs.ForEach(delegate(int n)
				{
					PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(n);
					NpcCmpt nc;
					if (npc != null && (bool)(nc = npc.GetComponent<NpcCmpt>()))
					{
						nc.NpcControlCmdId = item.type;
					}
				});
			}
		}
		if (missionCommonData.m_TargetIDList.Count > 0 && !GameUI.Instance.mMissionTrackWnd.isShow)
		{
			GameUI.Instance.mMissionTrackWnd.Show();
		}
		if (missionCommonData.m_ID == 66 || missionCommonData.m_ID == 67 || missionCommonData.m_ID == 756)
		{
			VCEditor.Open();
			TutorMgr.Load();
		}
		if (missionCommonData.IsTalkMission())
		{
			if (PeGameMgr.IsStory)
			{
				List<int> idlist = missionCommonData.HasStory(Story_Info.Story_Info_Get);
				if (pushStory)
				{
					StroyManager.Instance.PushStoryList(idlist);
				}
			}
			else if (PeGameMgr.IsAdventure)
			{
				List<int> idlist2 = missionCommonData.HasStory(Story_Info.Story_Info_Get);
				StroyManager.Instance.PushAdStoryList(idlist2, missionCommonData.m_iNpc);
			}
			MissionManager.Instance.CompleteMission(MissionID);
		}
		else
		{
			if (AddMissionFlagType(MissionID, MissionFlag, MissionValue) < 1)
			{
				return 0;
			}
			UIMissionMgr.Instance.UpdateGetableMission();
			if (PeGameMgr.IsStory)
			{
				List<int> idlist3 = missionCommonData.HasStory(Story_Info.Story_Info_Get);
				if (pushStory)
				{
					StroyManager.Instance.PushStoryList(idlist3);
				}
			}
			else if (PeGameMgr.IsAdventure)
			{
				List<int> idlist4 = missionCommonData.HasStory(Story_Info.Story_Info_Get);
				StroyManager.Instance.PushAdStoryList(idlist4, missionCommonData.m_iNpc);
			}
			if (MissionManager.Instance.m_bHadInitMission)
			{
			}
			UIMissionMgr.Instance.DeleteGetableMission(MissionID);
		}
		if (MissionRepository.m_MissionCommonMap.ContainsKey(MissionID) && MissionRepository.m_MissionCommonMap[MissionID].m_PromptOP.Count > 0)
		{
			GameUI.Instance.mNPCTalk.SpTalkSymbol(spOrHalf: true);
			if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
			{
				GameUI.Instance.mNPCTalk.AddNpcTalkInfo(MissionID, 6, 0, bFailed: false, isClearTalkList: true);
			}
			else
			{
				GameUI.Instance.mNPCTalk.AddNpcTalkInfo(MissionID, 6);
			}
			if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
			{
				GameUI.Instance.mNPCTalk.SPTalkClose();
			}
		}
		for (int i = 0; i < missionCommonData.m_PromptOP.Count; i++)
		{
			GameUI.Instance.mServantTalk.AddTalk(missionCommonData.m_PromptOP[i]);
		}
		if (MissionID == 500)
		{
			GlobalEvent.OnPlayerGetOnTrain += StroyManager.Instance.PlayerGetOnTrain;
			GlobalEvent.OnPlayerGetOffTrain += StroyManager.Instance.PlayerGetOffTrain;
		}
		if (missionCommonData.m_NeedTime > 0 && !m_MissionTime.ContainsKey(MissionID))
		{
			m_MissionTime.Add(MissionID, GameTime.Timer.Second);
		}
		if (MissionManager.Instance.m_bHadInitMission && !HasGetRewards(MissionID))
		{
			for (int j = 0; j < missionCommonData.m_Get_DeleteItem.Count; j++)
			{
				if (missionCommonData.m_Get_DeleteItem[j].id > 0)
				{
					PeSingleton<PeCreature>.Instance.mainPlayer.RemoveFromPkg(missionCommonData.m_Get_DeleteItem[j].id, missionCommonData.m_Get_DeleteItem[j].num);
					ProcessCollectMissionByID(missionCommonData.m_Get_DeleteItem[j].id);
				}
			}
			for (int k = 0; k < missionCommonData.m_Get_MissionItem.Count; k++)
			{
				m_GetRewards.Add(MissionID);
				if (missionCommonData.m_Get_MissionItem[k].id > 0)
				{
					ItemProto itemData = ItemProto.GetItemData(missionCommonData.m_Get_MissionItem[k].id);
					if (itemData != null && PeGender.IsMatch(itemData.equipSex, PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetSex()) && !GameConfig.IsMultiMode)
					{
						PeSingleton<PeCreature>.Instance.mainPlayer.AddToPkg(missionCommonData.m_Get_MissionItem[k].id, missionCommonData.m_Get_MissionItem[k].num);
					}
				}
			}
		}
		for (int l = 0; l < missionCommonData.m_TargetIDList.Count; l++)
		{
			TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[l]);
			if (!isRecord && targetType == TargetType.TargetType_Collect && !PeGameMgr.IsMulti)
			{
				MissionManager.GetTypeCollectData(missionCommonData.m_TargetIDList[l])?.RandItemActive();
			}
			if (PeGameMgr.IsSingleAdventure || PeGameMgr.IsMultiAdventure)
			{
				MissionOperationAdRand(targetType, missionCommonData.m_TargetIDList[l], MissionID);
			}
			else
			{
				MissionOperationStory(targetType, missionCommonData.m_TargetIDList[l], MissionID);
			}
		}
		UpdateAllNpcMisTex();
		SpecialMissionStartHandle(MissionID);
		MissionManager.Instance.UpdateMissionMainGUI(MissionID, bComplete: false);
		return 1;
	}

	private void SpecialMissionStartHandle(int missionID)
	{
		switch (missionID)
		{
		case 242:
			KillNPC.burriedNum = 0;
			break;
		case 629:
			KillNPC.burriedNum = 0;
			break;
		case 628:
			foreach (PeEntity item in PeSingleton<EntityMgr>.Instance.All)
			{
				NpcCmpt npcCmpt;
				if (!(item == null) && (item.proto == EEntityProto.Npc || item.proto == EEntityProto.RandomNpc) && (bool)(npcCmpt = item.NpcCmpt) && !npcCmpt.IsNeedMedicine && item.Id != 9026)
				{
					NpcMissionData npcMissionData = item.GetUserData() as NpcMissionData;
					if (!npcMissionData.m_MissionList.Contains(710))
					{
						npcMissionData.m_MissionList.Add(710);
					}
				}
			}
			break;
		case 9114:
			if (CSMain.s_MgCreator != null)
			{
				int count = CSMain.GetCSNpcs().Count;
				if (count >= 16)
				{
					CompleteMission(missionID);
				}
				else
				{
					CSMain.s_MgCreator.RegisterPersonnelListener(MissionAdCountCsEntity);
				}
			}
			break;
		case 9137:
			if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
			{
				MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(9137);
				missionCommonData2.m_iReplyNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
			}
			break;
		case 9138:
			if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
			{
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(9138);
				missionCommonData.m_iReplyNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
			}
			break;
		}
		if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
		{
			TrainingTaskManager.Instance.InitMission(missionID);
		}
	}

	public void SetMissionState(PeEntity npc, NpcMissionState state)
	{
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(npc.Id);
			if (networkInterface != null)
			{
				networkInterface.RPCServer(EPacketType.PT_NPC_MissionState, (int)state);
			}
		}
		npc.SetState(state);
	}

	private void MissionAdCountCsEntity(int event_type, CSPersonnel p)
	{
		int count = CSMain.GetCSNpcs().Count;
		if (count >= 16)
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.RequestCompleteMission(9114);
			}
			else
			{
				CompleteMission(9114);
			}
			CSMain.s_MgCreator.UnregisterPeronnelListener(MissionAdCountCsEntity);
		}
	}

	private void MissionJoyrideOn(CarrierController tmp)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9004);
		StroyManager.Instance.FollowTarget(peEntity, PeSingleton<PeCreature>.Instance.mainPlayer.Id, Vector3.zero, 0, 0f);
		if (peEntity.GetUserData() is NpcMissionData npcMissionData)
		{
			npcMissionData.mInFollowMission = true;
		}
		peEntity.NpcCmpt.CanTalk = false;
		followers.Add(peEntity.NpcCmpt);
		UIMissionMgr.Instance.DeleteMission(peEntity);
	}

	private void MissionJoyrideOff(CarrierController tmp)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9004);
		StroyManager.Instance.RemoveReq(peEntity, EReqType.FollowTarget);
		if (peEntity.GetUserData() is NpcMissionData npcMissionData)
		{
			npcMissionData.mInFollowMission = false;
		}
		peEntity.NpcCmpt.CanTalk = true;
		followers.Remove(peEntity.NpcCmpt);
		UIMissionMgr.Instance.AddMission(peEntity);
	}

	public void ProcessFollowMission(int MissionID, int TargetID)
	{
		if (HadCompleteTarget(TargetID))
		{
			return;
		}
		if (TargetID == 3031)
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer.IsOnCarrier())
			{
				MissionJoyrideOn(null);
			}
			PassengerCmpt passengerCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
			passengerCmpt.onGetOnCarrier = (Action<CarrierController>)Delegate.Combine(passengerCmpt.onGetOnCarrier, new Action<CarrierController>(MissionJoyrideOn));
			PassengerCmpt passengerCmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.passengerCmpt;
			passengerCmpt2.onGetOffCarrier = (Action<CarrierController>)Delegate.Combine(passengerCmpt2.onGetOffCarrier, new Action<CarrierController>(MissionJoyrideOff));
			return;
		}
		TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(TargetID);
		if (typeFollowData == null)
		{
			return;
		}
		ENpcBattle battle = ENpcBattle.Defence;
		if (typeFollowData.m_isAttack != 0)
		{
			battle = (ENpcBattle)(typeFollowData.m_isAttack - 1);
		}
		Vector3 pos = typeFollowData.m_DistPos;
		PeEntity peEntity = null;
		if (typeFollowData.m_LookNameID != 0)
		{
			peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_LookNameID);
			if (peEntity != null)
			{
				pos = peEntity.position;
			}
		}
		else if (typeFollowData.m_BuildID > 0)
		{
			GetBuildingPos((MissionID != 9032) ? MissionID : 0, out pos);
		}
		if (pos == Vector3.zero)
		{
			Debug.LogWarning("Exception: follow mission npc is null.");
		}
		List<Vector3> meetingPosition = StroyManager.Instance.GetMeetingPosition(pos, typeFollowData.m_iNpcList.Count, 2f);
		List<int> list = new List<int>();
		for (int i = 0; i < typeFollowData.m_iNpcList.Count; i++)
		{
			PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[i]);
			if (npc == null)
			{
				continue;
			}
			NpcMissionData npcMissionData = npc.GetUserData() as NpcMissionData;
			SetMissionState(npc, NpcMissionState.Max);
			if (npcMissionData != null)
			{
				if (TargetID == 3081 && npc.Id == 9033)
				{
					continue;
				}
				npcMissionData.mInFollowMission = true;
			}
			if (!m_iCurFollowStartPos.ContainsKey(typeFollowData.m_iNpcList[i]))
			{
				m_iCurFollowStartPos.Add(typeFollowData.m_iNpcList[i], npc.ExtGetPos());
			}
			if (typeFollowData.m_EMode == 1)
			{
				if (peEntity != null)
				{
					StroyManager.Instance.FollowTarget(npc, PeSingleton<PeCreature>.Instance.mainPlayer.Id, Vector3.zero, peEntity.Id, typeFollowData.m_DistRadius, typeFollowData.m_iNpcList.Count <= 1);
				}
				else
				{
					StroyManager.Instance.FollowTarget(npc, PeSingleton<PeCreature>.Instance.mainPlayer.Id, pos, 0, typeFollowData.m_DistRadius, typeFollowData.m_iNpcList.Count <= 1);
				}
				if (typeFollowData.m_iNpcList.Count > 1)
				{
					list.Add(npc.Id);
				}
				npc.NpcCmpt.CanTalk = false;
				followers.Add(npc.NpcCmpt);
				NpcCmpt npcCmpt = ServantLeaderCmpt.Instance.mForcedFollowers.Find(delegate(NpcCmpt n)
				{
					if (n.Entity == null)
					{
						return false;
					}
					return (n.Entity.Id == npc.Id) ? true : false;
				});
				if (npcCmpt != null)
				{
					ServantLeaderCmpt.Instance.mForcedFollowers.Remove(npcCmpt);
				}
			}
			else
			{
				if (npcMissionData != null && npcMissionData.m_bRandomNpc)
				{
					npc.SetAttackMode(EAttackMode.Defence);
					npc.SetInvincible(value: false);
				}
				if (typeFollowData.m_PathList.Count > 0)
				{
					typeFollowData.m_PathList.Add(pos);
					typeFollowData.m_PathList.Add(meetingPosition[i]);
					StroyManager.Instance.MoveToByPath(npc, typeFollowData.m_PathList.ToArray());
					typeFollowData.m_PathList.Remove(pos);
					typeFollowData.m_PathList.Remove(meetingPosition[i]);
				}
				else
				{
					StroyManager.Instance.MoveTo(npc, meetingPosition[i], 1f, bForce: true, SpeedState.Run);
				}
				pathFollowers.Add(npc.NpcCmpt);
				npc.NpcCmpt.FixedPointPos = meetingPosition[i];
			}
			if (npc.NpcCmpt != null)
			{
				npc.NpcCmpt.Battle = battle;
			}
			UIMissionMgr.Instance.DeleteMission(npc);
		}
		if (list.Count > 0 && PeGameMgr.IsMulti)
		{
			StroyManager.Instance.FollowTarget(list, PeSingleton<PeCreature>.Instance.mainPlayer.Id);
		}
	}

	private void ProcessTowerMission(int MissionID, int TargetID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		int num = -1;
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			if (missionCommonData.m_TargetIDList[i] == TargetID)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(TargetID);
		if (typeTowerDefendsData == null)
		{
			return;
		}
		PeEntity peEntity = null;
		for (int j = 0; j < typeTowerDefendsData.m_iNpcList.Count; j++)
		{
			peEntity = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_iNpcList[j]);
			if (!(peEntity == null))
			{
				peEntity.SetInvincible(value: false);
			}
		}
		AISpawnAutomatic automatic = AISpawnAutomatic.GetAutomatic(typeTowerDefendsData.m_TdInfoId);
		if (automatic == null)
		{
			return;
		}
		m_TowerUIData.MaxCount = typeTowerDefendsData.m_Count;
		m_TowerUIData.MissionID = MissionID;
		UITowerInfo.Instance.SetInfo(m_TowerUIData);
		UnityEngine.Object @object = Resources.Load("Prefab/Mission/TowerMission");
		if (null != @object)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
			gameObject.name = "TowerMission";
			Vector3 pos = default(Vector3);
			switch (typeTowerDefendsData.m_Pos.type)
			{
			case TypeTowerDefendsData.PosType.getPos:
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			case TypeTowerDefendsData.PosType.pos:
				pos = typeTowerDefendsData.m_Pos.pos;
				break;
			case TypeTowerDefendsData.PosType.npcPos:
				pos = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_Pos.id).position;
				break;
			case TypeTowerDefendsData.PosType.doodadPos:
				pos = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(typeTowerDefendsData.m_Pos.id)[0].position;
				break;
			case TypeTowerDefendsData.PosType.conoly:
				if (!CSMain.GetAssemblyPos(out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			case TypeTowerDefendsData.PosType.camp:
				if (VArtifactUtil.GetTownPos(typeTowerDefendsData.m_Pos.id, out pos))
				{
					pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				}
				break;
			}
			typeTowerDefendsData.finallyPos = pos;
			gameObject.transform.position = pos;
			if (typeTowerDefendsData.m_iNpcList.Count > 0 && peEntity != null)
			{
				gameObject.transform.position = peEntity.ExtGetPos();
			}
		}
		if (MissionManager.Instance.m_bHadInitMission)
		{
			for (int k = 0; k < automatic.data.Count; k++)
			{
				AISpawnWaveData aISpawnWaveData = automatic.data[k];
				if (aISpawnWaveData == null)
				{
					continue;
				}
				for (int l = 0; l < aISpawnWaveData.data.data.Count; l++)
				{
					AISpawnData aISpawnData = aISpawnWaveData.data.data[l];
					if (aISpawnData != null)
					{
						int num2 = num * 100 + k * 10 + l;
						int num3 = UnityEngine.Random.Range(aISpawnData.minCount, aISpawnData.maxCount);
						string text = "_0";
						string text2 = "_0";
						string text3 = "_0";
						string missionValue = aISpawnData.spID + text + "_" + num3 + text2 + text3;
						ModifyQuestVariable(MissionID, MissionFlagTDMonster + num2, missionValue);
					}
				}
			}
			m_TowerUIData.PreTime = typeTowerDefendsData.m_Time;
			m_TowerUIData.bRefurbish = true;
			EntityCreateMgr.Instance.StartTowerMission(MissionID, 0, typeTowerDefendsData, typeTowerDefendsData.m_Time);
		}
		else
		{
			MissionManager.Instance.StartCoroutine(WaitingTowerMission(num, MissionID, automatic));
		}
		if (!(UITowerInfo.Instance == null))
		{
			UITowerInfo.Instance.Show();
		}
	}

	private IEnumerator WaitingTowerMission(int idxI, int MissionID, AISpawnAutomatic aisa)
	{
		while (!MissionManager.Instance.m_bHadInitMission)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Vector3 center = EntityCreateMgr.Instance.GetPlayerPos();
		Vector3 dir = EntityCreateMgr.Instance.GetPlayerDir();
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			yield break;
		}
		foreach (KeyValuePair<string, string> ite in missionFlagType)
		{
			if (ite.Key == MissionFlagStep)
			{
				continue;
			}
			string[] tmplist = ite.Value.Split('_');
			if (tmplist.Length != 5)
			{
				continue;
			}
			int comTar = Convert.ToInt32(tmplist[4]);
			if (comTar == 1 || Convert.ToInt32(tmplist[3]) == 0)
			{
				continue;
			}
			int num = Convert.ToInt32(tmplist[1]);
			int count = Convert.ToInt32(tmplist[2]);
			num = count - num;
			if (num == 0)
			{
				continue;
			}
			int minAngle = 0;
			int maxAngle = 0;
			float delaytime = 0f;
			for (int m = 0; m < aisa.data.Count; m++)
			{
				AISpawnWaveData aiwd = aisa.data[m];
				if (aiwd == null)
				{
					continue;
				}
				AISpawnData aisd;
				for (int n = 0; n < aiwd.data.data.Count; n++)
				{
					aisd = aiwd.data.data[n];
					if (aisd == null)
					{
						continue;
					}
					int idx = idxI * 100 + m * 10 + n;
					string tmpKey = MissionFlagTDMonster + idx;
					if (ite.Key != tmpKey)
					{
						continue;
					}
					goto IL_028b;
				}
				continue;
				IL_028b:
				minAngle = aisd.minAngle;
				maxAngle = aisd.maxAngle;
				delaytime = aiwd.delayTime;
				break;
			}
			m_TowerUIData.PreTime = delaytime;
			for (int i = 0; i < num; i++)
			{
				SceneMan.AddSceneObj(new EntityPosAgent
				{
					entitytype = EntityType.EntityType_MonsterTD,
					position = AiUtil.GetRandomPosition(center, 0f, 45f, dir, minAngle, maxAngle),
					bMission = true,
					proid = Convert.ToInt32(tmplist[0])
				});
			}
		}
		m_TowerUIData.bRefurbish = true;
		MissionManager.Instance.UpdateMissionTrack(MissionID);
	}

	private void MissionOperationStory(TargetType curType, int targetid, int MissionID)
	{
		switch (curType)
		{
		case TargetType.TargetType_Follow:
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(targetid);
			if (typeFollowData.m_LookNameID != 0)
			{
				PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_LookNameID);
				if (peEntity2 != null && peEntity2.IsRecruited() && !peEntity2.NpcCmpt.BaseNpcOutMission)
				{
					if (CSMain.GetAssemblyPos(out var pos4))
					{
						peEntity2.NpcCmpt.FixedPointPos = pos4;
					}
					if (peEntity2.NpcCmpt.Job == ENpcJob.Processor && NpcMgr.CallBackColonyNpcImmediately(peEntity2))
					{
						peEntity2.NpcCmpt.FixedPointPos = peEntity2.position;
					}
					peEntity2.NpcCmpt.BaseNpcOutMission = true;
				}
				if (peEntity2 != null && peEntity2.IsRecruited() && peEntity2.NpcCmpt.BaseNpcOutMission && peEntity2.NpcCmpt.FixedPointPos.y < -5000f && CSMain.GetAssemblyPos(out var pos5))
				{
					peEntity2.NpcCmpt.FixedPointPos = pos5;
				}
				if (peEntity2 != null && !peEntity2.IsRecruited() && peEntity2.NpcCmpt.BaseNpcOutMission && !WorldCollider.IsPointInWorld(peEntity2.NpcCmpt.FixedPointPos) && CSMain.GetAssemblyPos(out var pos6))
				{
					peEntity2.NpcCmpt.FixedPointPos = pos6;
				}
			}
			ProcessFollowMission(MissionID, targetid);
			break;
		}
		case TargetType.TargetType_KillMonster:
			MissionMonsterKill.ProcessMission(MissionID, targetid);
			break;
		case TargetType.TargetType_TowerDif:
			MissionTowerDefense.ProcessMission(MissionID, targetid);
			break;
		case TargetType.TargetType_Collect:
		{
			GetSpecialItem.AddLootSpecialItem(targetid);
			TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(targetid);
			if (typeCollectData != null)
			{
				ProcessCollectMissionByID(typeCollectData.ItemID);
			}
			break;
		}
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(targetid);
			if (typeSearchData.m_NpcID == 0)
			{
				break;
			}
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
			if (peEntity != null && peEntity.IsRecruited() && !peEntity.NpcCmpt.BaseNpcOutMission)
			{
				if (CSMain.GetAssemblyPos(out var pos))
				{
					peEntity.NpcCmpt.FixedPointPos = pos;
				}
				if (peEntity.NpcCmpt.Job == ENpcJob.Processor && NpcMgr.CallBackColonyNpcImmediately(peEntity))
				{
					peEntity.NpcCmpt.FixedPointPos = peEntity.position;
				}
				peEntity.NpcCmpt.BaseNpcOutMission = true;
			}
			if (peEntity != null && peEntity.IsRecruited() && peEntity.NpcCmpt.BaseNpcOutMission && peEntity.NpcCmpt.FixedPointPos.y < -5000f && CSMain.GetAssemblyPos(out var pos2))
			{
				peEntity.NpcCmpt.FixedPointPos = pos2;
			}
			if (peEntity != null && !peEntity.IsRecruited() && peEntity.NpcCmpt.BaseNpcOutMission && !WorldCollider.IsPointInWorld(peEntity.NpcCmpt.FixedPointPos) && CSMain.GetAssemblyPos(out var pos3))
			{
				peEntity.NpcCmpt.FixedPointPos = pos3;
			}
			break;
		}
		case TargetType.TargetType_UseItem:
		{
			TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(targetid);
			if (typeUseItemData != null)
			{
			}
			break;
		}
		}
	}

	public IntVector2 AvoidTownPos(IntVector2 vec)
	{
		int num = 0;
		IntVector2 townPosCenter;
		while (VArtifactUtil.IsInTown(vec, out townPosCenter))
		{
			Vector2 vector = ((Vector2)(vec - townPosCenter)).normalized * 10f;
			vec = new IntVector2(vec.x + (int)vector.x, vec.y + (int)vector.y);
			num++;
			if (num > 20)
			{
				break;
			}
		}
		return vec;
	}

	private void MissionOperationAdRand(TargetType curType, int targetid, int MissionID)
	{
		switch (curType)
		{
		case TargetType.TargetType_Collect:
		{
			TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(targetid);
			if (typeCollectData != null)
			{
				if (typeCollectData.m_AdDist > 0)
				{
					int iMin = typeCollectData.m_AdDist - typeCollectData.m_AdRadius;
					int iMax = typeCollectData.m_AdDist + typeCollectData.m_AdRadius;
					typeCollectData.m_TargetPos = StroyManager.Instance.GetPatrolPoint(StroyManager.Instance.GetPlayerPos(), iMin, iMax, bCheck: false);
				}
				ProcessCollectMissionByID(typeCollectData.ItemID);
			}
			break;
		}
		case TargetType.TargetType_Follow:
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(targetid);
			if (typeFollowData == null)
			{
				break;
			}
			if (PeGameMgr.IsSingle)
			{
				typeFollowData.m_DistRadius = typeFollowData.m_AdDistPos.radius2;
				if (MissionManager.Instance.m_bHadInitMission)
				{
					Vector3 pos;
					switch (typeFollowData.m_AdDistPos.refertoType)
					{
					case ReferToType.Player:
						pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						break;
					case ReferToType.Town:
						VArtifactUtil.GetTownPos(typeFollowData.m_AdDistPos.referToID, out pos);
						break;
					case ReferToType.Npc:
						if (adId_entityId.ContainsKey(typeFollowData.m_AdDistPos.referToID))
						{
							PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(adId_entityId[typeFollowData.m_AdDistPos.referToID]);
							pos = ((!(peEntity != null)) ? PeSingleton<PeCreature>.Instance.mainPlayer.position : peEntity.position);
						}
						else
						{
							pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						}
						break;
					case ReferToType.Transcript:
						pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						break;
					default:
						pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						break;
					}
					Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * typeFollowData.m_AdDistPos.radius1;
					Vector2 vector2 = new Vector2(pos.x + vector.x, pos.z + vector.y);
					IntVector2 intVector = new IntVector2((int)vector2.x, (int)vector2.y);
					if (typeFollowData.m_AdDistPos.refertoType == ReferToType.Transcript)
					{
						intVector = AvoidTownPos(intVector);
					}
					if (VFDataRTGen.IsTownAvailable((int)vector2.x, (int)vector2.y))
					{
						typeFollowData.m_DistPos = new Vector3(intVector.x, VFDataRTGen.GetPosHeightWithTown(intVector), intVector.y);
					}
					else
					{
						typeFollowData.m_DistPos = new Vector3(intVector.x, VFDataRTGen.GetPosHeight(intVector, inWater: true), intVector.y);
					}
					if (typeFollowData.m_AdNpcRadius.num > 0)
					{
						typeFollowData.m_LookNameID = StroyManager.Instance.CreateMissionRandomNpc(typeFollowData.m_DistPos, typeFollowData.m_AdNpcRadius.num);
					}
					for (int i = 0; i < typeFollowData.m_CreateNpcList.Count; i++)
					{
						Vector3 patrolPoint = StroyManager.Instance.GetPatrolPoint(typeFollowData.m_DistPos, 3, 8, bCheck: false);
						EntityCreateMgr.Instance.CreateRandomNpc(typeFollowData.m_CreateNpcList[i], patrolPoint);
					}
				}
				if (typeFollowData.m_AdDistPos.refertoType == ReferToType.Transcript)
				{
					if (RandomDungenMgr.Instance == null)
					{
						RandomDungenMgrData.AddInitTaskEntrance(new IntVector2((int)typeFollowData.m_DistPos.x, (int)typeFollowData.m_DistPos.z), typeFollowData.m_AdDistPos.referToID);
					}
					else
					{
						RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)typeFollowData.m_DistPos.x, (int)typeFollowData.m_DistPos.z), typeFollowData.m_AdDistPos.referToID);
					}
				}
				ProcessFollowMission(MissionID, targetid);
			}
			else
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RequestAdMissionData, (int)curType, targetid, MissionID);
			}
			break;
		}
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(targetid);
			if (typeSearchData == null)
			{
				break;
			}
			if (PeGameMgr.IsSingle)
			{
				typeSearchData.m_DistRadius = typeSearchData.m_mr.radius2;
				if (MissionManager.Instance.m_bHadInitMission)
				{
					Vector3 pos2;
					switch (typeSearchData.m_mr.refertoType)
					{
					case ReferToType.Player:
						pos2 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						break;
					case ReferToType.Town:
						VArtifactUtil.GetTownPos(typeSearchData.m_mr.referToID, out pos2);
						break;
					case ReferToType.Npc:
						if (adId_entityId.ContainsKey(typeSearchData.m_mr.referToID))
						{
							PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(adId_entityId[typeSearchData.m_mr.referToID]);
							pos2 = ((!(peEntity != null)) ? PeSingleton<PeCreature>.Instance.mainPlayer.position : peEntity.position);
						}
						else
						{
							pos2 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						}
						break;
					case ReferToType.Transcript:
						pos2 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						break;
					default:
						pos2 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
						break;
					}
					Vector2 vector3 = UnityEngine.Random.insideUnitCircle.normalized * typeSearchData.m_mr.radius1;
					Vector2 vector4 = new Vector2(pos2.x + vector3.x, pos2.z + vector3.y);
					IntVector2 intVector2 = new IntVector2((int)vector4.x, (int)vector4.y);
					if (typeSearchData.m_mr.refertoType == ReferToType.Transcript)
					{
						intVector2 = AvoidTownPos(intVector2);
					}
					if (VFDataRTGen.IsTownAvailable((int)vector4.x, (int)vector4.y))
					{
						typeSearchData.m_DistPos = new Vector3(intVector2.x, VFDataRTGen.GetPosHeightWithTown(intVector2), intVector2.y);
					}
					else
					{
						typeSearchData.m_DistPos = new Vector3(intVector2.x, VFDataRTGen.GetPosHeight(intVector2, inWater: true), intVector2.y);
					}
					for (int j = 0; j < typeSearchData.m_CreateNpcList.Count; j++)
					{
						Vector3 patrolPoint2 = StroyManager.Instance.GetPatrolPoint(typeSearchData.m_DistPos, 3, 8, bCheck: false);
						EntityCreateMgr.Instance.CreateRandomNpc(typeSearchData.m_CreateNpcList[j], patrolPoint2);
					}
				}
				if (typeSearchData.m_mr.refertoType == ReferToType.Transcript)
				{
					if (RandomDungenMgr.Instance == null)
					{
						RandomDungenMgrData.AddInitTaskEntrance(AvoidTownPos(new IntVector2((int)typeSearchData.m_DistPos.x, (int)typeSearchData.m_DistPos.z)), typeSearchData.m_mr.referToID);
					}
					else
					{
						RandomDungenMgr.Instance.GenTaskEntrance(AvoidTownPos(new IntVector2((int)typeSearchData.m_DistPos.x, (int)typeSearchData.m_DistPos.z)), typeSearchData.m_mr.referToID);
					}
				}
			}
			else
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RequestAdMissionData, (int)curType, targetid, MissionID);
			}
			break;
		}
		case TargetType.TargetType_UseItem:
		{
			TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(targetid);
			if (typeUseItemData == null)
			{
				break;
			}
			if (PeGameMgr.IsSingle)
			{
				Vector3 pos3;
				switch (typeUseItemData.m_AdDistPos.refertoType)
				{
				case ReferToType.Player:
					pos3 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
					break;
				case ReferToType.Town:
					VArtifactUtil.GetTownPos(typeUseItemData.m_AdDistPos.referToID, out pos3);
					break;
				case ReferToType.Npc:
					if (adId_entityId.ContainsKey(typeUseItemData.m_AdDistPos.referToID))
					{
						PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(adId_entityId[typeUseItemData.m_AdDistPos.referToID]);
						pos3 = ((!(peEntity != null)) ? PeSingleton<PeCreature>.Instance.mainPlayer.position : peEntity.position);
					}
					else
					{
						pos3 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
					}
					break;
				default:
					pos3 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
					break;
				}
				Vector2 vector5 = UnityEngine.Random.insideUnitCircle.normalized * typeUseItemData.m_AdDistPos.radius1;
				Vector2 vector6 = new Vector2(pos3.x + vector5.x, pos3.z + vector5.y);
				if (VFDataRTGen.IsTownAvailable((int)vector6.x, (int)vector6.y))
				{
					typeUseItemData.m_Pos = new Vector3(vector6.x, VFDataRTGen.GetPosHeightWithTown(new IntVector2((int)vector6.x, (int)vector6.y)), vector6.y);
				}
				else
				{
					typeUseItemData.m_Pos = new Vector3(vector6.x, VFDataRTGen.GetPosHeight(new IntVector2((int)vector6.x, (int)vector6.y)), vector6.y);
				}
				typeUseItemData.m_Radius = typeUseItemData.m_AdDistPos.radius2;
			}
			else
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RequestAdMissionData, (int)curType, targetid, MissionID);
			}
			break;
		}
		case TargetType.TargetType_Messenger:
		{
			TypeMessengerData typeMessengerData = MissionManager.GetTypeMessengerData(targetid);
			if (typeMessengerData != null)
			{
			}
			break;
		}
		case TargetType.TargetType_KillMonster:
		{
			TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(targetid);
			if (typeMonsterData != null)
			{
				MissionMonsterKill.ProcessMission(MissionID, targetid);
			}
			break;
		}
		case TargetType.TargetType_TowerDif:
		{
			TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(targetid);
			if (typeTowerDefendsData != null)
			{
				MissionTowerDefense.ProcessMission(MissionID, targetid);
			}
			break;
		}
		}
	}

	public void UpdateAllNpcMisTex()
	{
		foreach (PeEntity item in PeSingleton<EntityMgr>.Instance.All)
		{
			if (!(item == null) && (item.proto == EEntityProto.Npc || item.proto == EEntityProto.RandomNpc) && item.NpcCmpt != null)
			{
				if (!item.NpcCmpt.IsFollower)
				{
					UpdateNpcMissionTex(item);
				}
				else
				{
					SetMissionState(item, NpcMissionState.Max);
				}
			}
		}
	}

	public void UpdateAllNpcMapPos()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(item.Key);
			if (missionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				switch (MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]))
				{
				case TargetType.TargetType_TowerDif:
				{
					TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(missionCommonData.m_TargetIDList[i]);
					if (typeTowerDefendsData == null)
					{
						break;
					}
					for (int k = 0; k < typeTowerDefendsData.m_iNpcList.Count; k++)
					{
						if (!list.Contains(typeTowerDefendsData.m_iNpcList[k]))
						{
							list.Add(typeTowerDefendsData.m_iNpcList[k]);
						}
					}
					break;
				}
				case TargetType.TargetType_Follow:
				{
					TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
					if (typeFollowData == null)
					{
						break;
					}
					for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
					{
						if (!list.Contains(typeFollowData.m_iNpcList[j]))
						{
							list.Add(typeFollowData.m_iNpcList[j]);
						}
					}
					break;
				}
				}
			}
		}
		foreach (PeEntity item2 in PeSingleton<EntityMgr>.Instance.All)
		{
			if (!(item2 == null) && !list.Contains(item2.Id))
			{
				UpdateNpcMapPos(item2);
			}
		}
	}

	public void UpdateNpcMapPos(int npcid)
	{
		PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(npcid);
		UpdateNpcMapPos(npc);
	}

	public void UpdateNpcMapPos(PeEntity npc)
	{
		if (npc == null || !(npc.GetUserData() is NpcMissionData npcMissionData))
		{
			return;
		}
		for (int i = 0; i < npcMissionData.m_MissionList.Count; i++)
		{
			int missionID = npcMissionData.m_MissionList[i];
			if (!MissionRepository.IsMainMission(missionID) || HasMission(missionID))
			{
			}
		}
	}

	public bool IsShowNpcMapLabel(PeEntity npc)
	{
		if (npc.enityInfoCmpt.MissionState == NpcMissionState.MainCanGet || npc.enityInfoCmpt.MissionState == NpcMissionState.MainCanSubmit)
		{
			return true;
		}
		if (npc.GetUserData() is NpcMissionData { mInFollowMission: not false })
		{
			return true;
		}
		return false;
	}

	public void UpdateNpcMissionTex(PeEntity npc)
	{
		if (npc == null || (npc.proto != EEntityProto.Npc && npc.proto != EEntityProto.RandomNpc) || !(npc.GetUserData() is NpcMissionData npcMissionData))
		{
			return;
		}
		bool isMain = false;
		bool isHave = false;
		if (npcMissionData.m_MissionListReply.Count > 0)
		{
			npcMissionData.m_MissionListReply.ForEach(delegate(int tmp)
			{
				if (!MissionRepository.NotUpdateMisTex(tmp) && IsReplyMission(tmp))
				{
					MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(tmp);
					if (missionCommonData != null)
					{
						if (tmp == 9137 || tmp == 9138)
						{
							if (missionCommonData.m_iReplyNpc == npc.Id)
							{
								isHave = true;
							}
						}
						else
						{
							isHave = true;
						}
						if (missionCommonData.m_Type == MissionType.MissionType_Main)
						{
							isMain = true;
						}
					}
				}
			});
			if (isHave)
			{
				if (isMain)
				{
					SetMissionState(npc, NpcMissionState.MainCanSubmit);
				}
				else
				{
					SetMissionState(npc, NpcMissionState.CanSubmit);
				}
				return;
			}
		}
		isMain = false;
		isHave = false;
		if (npcMissionData.m_MissionList.Count > 0)
		{
			npcMissionData.m_MissionList.ForEach(delegate(int tmp)
			{
				if (!MissionRepository.NotUpdateMisTex(tmp) && !HasMission(tmp) && IsGetTakeMission(tmp))
				{
					isHave = true;
					if (MissionManager.GetMissionCommonData(tmp) != null && MissionManager.GetMissionCommonData(tmp).m_Type == MissionType.MissionType_Main)
					{
						isMain = true;
					}
				}
			});
			if (isHave)
			{
				if (isMain)
				{
					SetMissionState(npc, NpcMissionState.MainCanGet);
				}
				else
				{
					SetMissionState(npc, NpcMissionState.CanGet);
				}
				return;
			}
		}
		isMain = false;
		isHave = false;
		if (m_MissionInfo.Count > 0)
		{
			MissionCommonData missionCommonData2 = null;
			foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
			{
				missionCommonData2 = MissionManager.GetMissionCommonData(item.Key);
				if (missionCommonData2 != null && missionCommonData2.m_iReplyNpc == npc.Id)
				{
					isHave = true;
					if (missionCommonData2.m_Type == MissionType.MissionType_Main)
					{
						isMain = true;
					}
				}
			}
			if (isHave)
			{
				if (isMain)
				{
					SetMissionState(npc, NpcMissionState.MainHasGet);
				}
				else
				{
					SetMissionState(npc, NpcMissionState.HasGet);
				}
				return;
			}
		}
		if (npcMissionData.m_bRandomNpc && npcMissionData.m_RandomMission != 0 && GameUI.Instance.mNpcWnd.CheckAddMissionListID(npcMissionData.m_RandomMission, npcMissionData))
		{
			MissionCommonData missionCommonData3 = MissionManager.GetMissionCommonData(npcMissionData.m_RandomMission);
			if (missionCommonData3.m_Type == MissionType.MissionType_Main)
			{
				SetMissionState(npc, NpcMissionState.MainCanGet);
			}
			else
			{
				SetMissionState(npc, NpcMissionState.CanGet);
			}
		}
		else
		{
			SetMissionState(npc, NpcMissionState.Max);
		}
	}

	public ECreation IsSpecialID(int itemid)
	{
		ECreation result = ECreation.Null;
		switch (itemid)
		{
		case 1322:
			result = ECreation.Sword;
			break;
		case 1323:
			result = ECreation.HandGun;
			break;
		case 1324:
			result = ECreation.Rifle;
			break;
		case 1326:
			result = ECreation.Vehicle;
			break;
		case 1327:
			result = ECreation.Vehicle;
			break;
		case 1328:
			result = ECreation.Vehicle;
			break;
		case 1329:
			result = ECreation.Aircraft;
			break;
		case 1330:
			result = ECreation.Aircraft;
			break;
		case 1542:
			result = ECreation.SimpleObject;
			break;
		}
		return result;
	}

	public bool MacthProductType(int type, int itemid)
	{
		int type2 = (int)CreationMgr.GetCreation(itemid).m_Attribute.m_Type;
		switch (type)
		{
		case 100:
			if (type2 == 32 || type2 == 33)
			{
				return true;
			}
			break;
		case 200:
			if (type2 == 144)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public int GetCollectSpecialItem(int type, int itemid)
	{
		return 0;
	}

	public void DelSpecialItem(int type, int itemid, int delNum)
	{
	}

	private void GoHome(PeEntity npc)
	{
		if (!(npc == null) && m_iCurFollowStartPos.ContainsKey(npc.Id))
		{
			StroyManager.Instance.MoveTo(npc, m_iCurFollowStartPos[npc.Id], 1f, bForce: true, SpeedState.Run);
			m_iCurFollowStartPos.Remove(npc.Id);
		}
	}

	public int GetTowerDefineKillNum(int MissionID)
	{
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<string, string> item in missionFlagType)
		{
			if (!(item.Key == MissionFlagStep))
			{
				string[] array = item.Value.Split('_');
				if (array.Length == 5)
				{
					int num2 = Convert.ToInt32(array[1]);
					num += num2;
				}
			}
		}
		return num;
	}

	public void ProcessCollectMissionByID(int ItemID)
	{
		List<int> collectMissionListByID = GetCollectMissionListByID(ItemID);
		if (collectMissionListByID.Count > 0)
		{
			for (int i = 0; i < collectMissionListByID.Count; i++)
			{
				CheckAutoCollectMissionComplete(collectMissionListByID[i]);
				MissionManager.Instance.UpdateMissionTrack(collectMissionListByID[i]);
				UpdateAllNpcMisTex();
			}
		}
	}

	private void CheckAutoCollectMissionComplete(int missionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(missionID);
		List<int> list = missionCommonData.m_TargetIDList.FindAll((int ite) => ite / 1000 == 2);
		if ((PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story && (missionCommonData == null || missionCommonData.m_iReplyNpc != 0 || list.Count == 0)) || (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Adventure && (missionCommonData == null || !missionCommonData.isAutoReply || list.Count == 0)))
		{
			return;
		}
		int num = 0;
		foreach (int item in list)
		{
			TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(item);
			if (typeCollectData == null)
			{
				continue;
			}
			ECreation eCreation = IsSpecialID(typeCollectData.ItemID);
			if (eCreation != 0)
			{
				if (PeSingleton<PeCreature>.Instance.mainPlayer.GetCreationItemCount(eCreation) >= typeCollectData.ItemNum)
				{
					num++;
				}
			}
			else if (PeSingleton<PeCreature>.Instance.mainPlayer.GetPkgItemCount(typeCollectData.ItemID) >= typeCollectData.ItemNum)
			{
				num++;
			}
		}
		if (num >= list.Count)
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.RequestCompleteMission(missionID);
			}
			else
			{
				MissionManager.Instance.CompleteMission(missionID);
			}
		}
	}

	private List<Vector3> GetPos(Vector3 center, float radius, List<MissionIDNum> data)
	{
		if (data == null || data.Count == 0)
		{
			return new List<Vector3>();
		}
		List<Vector3> list = new List<Vector3>();
		foreach (MissionIDNum datum in data)
		{
			for (int i = 0; i < datum.num; i++)
			{
				Vector3 randomPosition = AiUtil.GetRandomPosition(center, 0f, radius, 10f, AiUtil.groundedLayer, 10);
				if (randomPosition != Vector3.zero)
				{
					list.Add(randomPosition);
				}
				else
				{
					i--;
				}
			}
		}
		return list;
	}

	public static void StoreBuildingPos(int missionID, Vector3 pos)
	{
		mID_buildPos[missionID] = pos;
	}

	public static bool GetBuildingPos(int missionID, out Vector3 pos)
	{
		pos = Vector3.zero;
		if (mID_buildPos == null)
		{
			return false;
		}
		if (!mID_buildPos.ContainsKey(missionID))
		{
			return false;
		}
		pos = mID_buildPos[missionID];
		return true;
	}

	private List<int> StoreCollectRand()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(item.Key);
			List<int> list2 = missionCommonData.m_TargetIDList.FindAll((int ite) => MissionRepository.GetTargetType(ite) == TargetType.TargetType_Collect);
			foreach (int item2 in list2)
			{
				TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(item2);
				if (typeCollectData.m_randItemID.Count > 1)
				{
					list.Add(item2);
					list.Add(typeCollectData.ItemID);
					list.Add(typeCollectData.ItemNum);
				}
			}
		}
		return list;
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(m_GetRewards.Count);
		for (int i = 0; i < m_GetRewards.Count; i++)
		{
			bw.Write(m_GetRewards[i]);
		}
		bw.Write(m_MissionTargetState.Count);
		foreach (KeyValuePair<int, int> item in m_MissionTargetState)
		{
			bw.Write(item.Key);
			bw.Write(item.Value);
		}
		bw.Write(m_MissionState.Count);
		foreach (KeyValuePair<int, int> item2 in m_MissionState)
		{
			bw.Write(item2.Key);
			bw.Write(item2.Value);
		}
		bw.Write(m_MissionInfo.Count);
		foreach (KeyValuePair<int, Dictionary<string, string>> item3 in m_MissionInfo)
		{
			bw.Write(item3.Key);
			bw.Write(item3.Value.Count);
			foreach (KeyValuePair<string, string> item4 in item3.Value)
			{
				bw.Write(item4.Key);
				bw.Write(item4.Value);
			}
		}
		if (m_SpeVecList.Count == 0)
		{
			bw.Write(0);
		}
		else
		{
			bw.Write(m_SpeVecList.Count);
			for (int j = 0; j < m_SpeVecList.Count; j++)
			{
				Serialize.WriteVector3(bw, m_SpeVecList[j]);
			}
		}
		if (m_iCurFollowStartPos.Count == 0)
		{
			bw.Write(0);
		}
		else
		{
			bw.Write(m_iCurFollowStartPos.Count);
			foreach (KeyValuePair<int, Vector3> iCurFollowStartPo in m_iCurFollowStartPos)
			{
				bw.Write(iCurFollowStartPo.Key);
				Serialize.WriteVector3(bw, iCurFollowStartPo.Value);
			}
		}
		if (HadCompleteMission(822) && !HadCompleteMission(826))
		{
			bw.Write(recordCreationName.Count);
			foreach (string item5 in recordCreationName)
			{
				bw.Write(item5);
			}
		}
		bw.Write(10);
		bw.Write(languegeSkill);
		bw.Write(recordNpcName.Count);
		foreach (KeyValuePair<int, string> item6 in recordNpcName)
		{
			bw.Write(item6.Key);
			bw.Write(item6.Value);
		}
		bw.Write(pajaLanguageBePickup);
		bw.Write(mID_buildPos.Count);
		foreach (KeyValuePair<int, Vector3> mID_buildPo in mID_buildPos)
		{
			bw.Write(mID_buildPo.Key);
			bw.Write(mID_buildPo.Value.x);
			bw.Write(mID_buildPo.Value.y);
			bw.Write(mID_buildPo.Value.z);
		}
		List<int> list = StoreCollectRand();
		bw.Write(list.Count / 3);
		foreach (int item7 in list)
		{
			bw.Write(item7);
		}
		bw.Write(textSamples.Count);
		foreach (KeyValuePair<int, Vector3> textSample in textSamples)
		{
			bw.Write(textSample.Key);
			bw.Write(textSample.Value.x);
			bw.Write(textSample.Value.y);
			bw.Write(textSample.Value.z);
		}
		bw.Write(recordAndHer.Count);
		for (int k = 0; k < recordAndHer.Count; k++)
		{
			bw.Write(recordAndHer[k]);
		}
		bw.Write(recordKillNpcItem.Count);
		for (int l = 0; l < recordKillNpcItem.Count; l++)
		{
			bw.Write(recordKillNpcItem[l][0]);
			bw.Write(recordKillNpcItem[l][1]);
			bw.Write(recordKillNpcItem[l][2]);
		}
		bw.Write(adId_entityId.Count);
		foreach (KeyValuePair<int, int> item8 in adId_entityId)
		{
			bw.Write(item8.Key);
			bw.Write(item8.Value);
		}
		bw.Write(recordCretionPos.Count);
		foreach (Vector3 recordCretionPo in recordCretionPos)
		{
			bw.Write(recordCretionPo.x);
			bw.Write(recordCretionPo.y);
			bw.Write(recordCretionPo.z);
		}
		bw.Write(pajaLanguageBePickup);
	}

	public void Import(byte[] buffer)
	{
		if (buffer == null || buffer.Length <= 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(buffer);
		using (BinaryReader binaryReader = new BinaryReader(memoryStream))
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				m_GetRewards.Add(binaryReader.ReadInt32());
			}
			num = binaryReader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				m_MissionTargetState.Add(binaryReader.ReadInt32(), binaryReader.ReadInt32());
			}
			num = binaryReader.ReadInt32();
			for (int k = 0; k < num; k++)
			{
				m_MissionState.Add(binaryReader.ReadInt32(), binaryReader.ReadInt32());
			}
			num = binaryReader.ReadInt32();
			for (int l = 0; l < num; l++)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int key = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				for (int m = 0; m < num2; m++)
				{
					dictionary.Add(binaryReader.ReadString(), binaryReader.ReadString());
				}
				m_RecordMisInfo.Add(key, dictionary);
			}
			num = binaryReader.ReadInt32();
			for (int n = 0; n < num; n++)
			{
				m_SpeVecList.Add(Serialize.ReadVector3(binaryReader));
			}
			num = binaryReader.ReadInt32();
			for (int num3 = 0; num3 < num; num3++)
			{
				m_iCurFollowStartPos.Add(binaryReader.ReadInt32(), Serialize.ReadVector3(binaryReader));
			}
			if (HadCompleteMission(822) && !HadCompleteMission(826))
			{
				int num4 = binaryReader.ReadInt32();
				for (int num5 = 0; num5 < num4; num5++)
				{
					binaryReader.ReadString();
				}
			}
			int num6 = binaryReader.ReadInt32();
			if (num6 >= 1)
			{
				languegeSkill = binaryReader.ReadInt32();
				if (num6 >= 2)
				{
					int num7 = binaryReader.ReadInt32();
					for (int num8 = 0; num8 < num7; num8++)
					{
						recordNpcName.Add(binaryReader.ReadInt32(), binaryReader.ReadString());
					}
					if (num6 >= 3)
					{
						binaryReader.ReadInt32();
					}
				}
			}
			if (num6 >= 4)
			{
				if (mID_buildPos != null)
				{
					mID_buildPos.Clear();
				}
				mID_buildPos = new Dictionary<int, Vector3>();
				int num9 = binaryReader.ReadInt32();
				for (int num10 = 0; num10 < num9; num10++)
				{
					int key2 = binaryReader.ReadInt32();
					float x = binaryReader.ReadSingle();
					float y = binaryReader.ReadSingle();
					float z = binaryReader.ReadSingle();
					mID_buildPos.Add(key2, new Vector3(x, y, z));
				}
			}
			if (num6 >= 5)
			{
				int num11 = binaryReader.ReadInt32();
				for (int num12 = 0; num12 < num11; num12++)
				{
					TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(binaryReader.ReadInt32());
					if (typeCollectData == null)
					{
						Debug.LogError("Mission TypeCollectData is wrong!");
						binaryReader.ReadInt32();
						binaryReader.ReadInt32();
					}
					else
					{
						typeCollectData.ItemID = binaryReader.ReadInt32();
						typeCollectData.ItemNum = binaryReader.ReadInt32();
					}
				}
			}
			textSamples.Clear();
			if (num6 >= 6)
			{
				int num13 = binaryReader.ReadInt32();
				for (int num14 = 0; num14 < num13; num14++)
				{
					int num15 = binaryReader.ReadInt32();
					float x2 = binaryReader.ReadSingle();
					float y2 = binaryReader.ReadSingle();
					float z2 = binaryReader.ReadSingle();
					StroyManager.CreateLanguageSample_byIndex(num15);
					textSamples.Add(num15, new Vector3(x2, y2, z2));
				}
				num13 = binaryReader.ReadInt32();
				for (int num16 = 0; num16 < num13; num16++)
				{
					StroyManager.CreateAndHeraNest(binaryReader.ReadInt32());
				}
			}
			recordKillNpcItem.Clear();
			if (num6 >= 7)
			{
				int num17 = binaryReader.ReadInt32();
				for (int num18 = 0; num18 < num17; num18++)
				{
					int npcid = binaryReader.ReadInt32();
					int itemProtoID = binaryReader.ReadInt32();
					int itemNum = binaryReader.ReadInt32();
					KillNPC.NPCaddItem(npcid, itemProtoID, itemNum);
				}
			}
			if (num6 >= 8)
			{
				int num19 = binaryReader.ReadInt32();
				for (int num20 = 0; num20 < num19; num20++)
				{
					adId_entityId.Add(binaryReader.ReadInt32(), binaryReader.ReadInt32());
				}
			}
			if (num6 >= 9)
			{
				int num21 = binaryReader.ReadInt32();
				for (int num22 = 0; num22 < num21; num22++)
				{
					recordCretionPos.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
				}
			}
			if (num6 >= 10)
			{
				pajaLanguageBePickup = binaryReader.ReadInt32();
			}
			binaryReader.Close();
		}
		memoryStream.Close();
	}

	public void ClearMission()
	{
		m_GetRewards.Clear();
		m_MissionTargetState.Clear();
		m_MissionState.Clear();
		m_RecordMisInfo.Clear();
		m_SpeVecList.Clear();
		m_iCurFollowStartPos.Clear();
		m_MissionInfo.Clear();
	}

	public bool ImportNetwork(byte[] buffer, int type = 0)
	{
		bool result = false;
		if (buffer == null || buffer.Length <= 0)
		{
			return result;
		}
		using MemoryStream memoryStream = new MemoryStream(buffer);
		using (BinaryReader binaryReader = new BinaryReader(memoryStream))
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int item = binaryReader.ReadInt32();
				if (!m_GetRewards.Contains(item))
				{
					m_GetRewards.Add(item);
				}
				result = true;
			}
			num = binaryReader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				int key = binaryReader.ReadInt32();
				int value = binaryReader.ReadInt32();
				m_MissionTargetState[key] = value;
				result = true;
			}
			num = binaryReader.ReadInt32();
			for (int k = 0; k < num; k++)
			{
				int key2 = binaryReader.ReadInt32();
				int value2 = binaryReader.ReadInt32();
				m_MissionState[key2] = value2;
				result = true;
			}
			num = binaryReader.ReadInt32();
			for (int l = 0; l < num; l++)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				int key3 = binaryReader.ReadInt32();
				int num2 = binaryReader.ReadInt32();
				for (int m = 0; m < num2; m++)
				{
					string key4 = binaryReader.ReadString();
					string value3 = binaryReader.ReadString();
					dictionary[key4] = value3;
				}
				m_RecordMisInfo[key3] = dictionary;
				result = true;
			}
			MissionManager.Instance.m_PlayerMission.LanguegeSkill = binaryReader.ReadInt32();
			if (type == 1)
			{
				binaryReader.ReadInt32();
				m_FollowPlayerName = binaryReader.ReadString();
			}
			binaryReader.Close();
		}
		memoryStream.Close();
		return result;
	}

	private void MulAdRandMisOperation(TargetType curType, int targetid)
	{
		switch (curType)
		{
		case TargetType.TargetType_KillMonster:
			break;
		case TargetType.TargetType_Follow:
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(targetid);
			if (typeFollowData == null)
			{
				break;
			}
			typeFollowData.m_DistRadius = typeFollowData.m_AdDistPos.radius2;
			if (PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetName() != m_FollowPlayerName && m_FollowPlayerName != null)
			{
				break;
			}
			for (int i = 0; i < typeFollowData.m_iNpcList.Count; i++)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[i]);
				if (peEntity == null)
				{
				}
			}
			break;
		}
		case TargetType.TargetType_UseItem:
		{
			TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(targetid);
			if (typeUseItemData != null)
			{
				typeUseItemData.m_Radius = typeUseItemData.m_AdDistPos.radius2;
			}
			break;
		}
		case TargetType.TargetType_Collect:
		case TargetType.TargetType_Discovery:
			break;
		}
	}

	public void RequestCompleteMission(int MissionID, int TargetID = -1, bool bCheck = true)
	{
		PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_CompleteMission, TargetID, MissionID, bCheck);
	}

	public void ReplyDeleteMission(int nMissionID)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(nMissionID);
		if (missionCommonData != null)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_DeleteMission, nMissionID);
		}
	}
}
