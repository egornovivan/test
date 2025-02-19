using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomData;
using ItemAsset;
using UnityEngine;

public class PlayerMission
{
	public enum MissionInfo
	{
		MAX_MISSIONFLAG_LENGTH = 10,
		MAX_MISSIONVALUE_LENGTH = 18,
		MAX_MISSION_COUNT = 20000
	}

	private const int _disErrerRang = 2;

	public AiAdNpcNetwork m_CurNpc;

	public string m_playerName;

	public int m_nTeam = -1;

	private Player m_Player;

	private bool _dirty;

	public List<int> m_GetRewards = new List<int>();

	public Dictionary<int, Vector3> m_iCurFollowStartPos = new Dictionary<int, Vector3>();

	public Dictionary<int, int> m_MissionTargetState = new Dictionary<int, int>();

	public Dictionary<int, int> m_MissionState = new Dictionary<int, int>();

	public int _languegeSkill;

	public Dictionary<int, Dictionary<string, string>> m_MissionInfo = new Dictionary<int, Dictionary<string, string>>();

	public bool dirty => _dirty;

	public bool HasMission(int MissionID)
	{
		return m_MissionInfo.ContainsKey(MissionID);
	}

	public bool HasGetRewards(int MissionID)
	{
		return m_GetRewards.Contains(MissionID);
	}

	private bool CheckGetMission(int MissionID, string MissionFlag, string MissionValue, Player player)
	{
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null)
		{
			return false;
		}
		if (adrmMissionCommonData.m_DeleteID.Count > 0)
		{
			for (int i = 0; i < adrmMissionCommonData.m_DeleteID.Count; i++)
			{
				if (HasMission(adrmMissionCommonData.m_DeleteID[i]))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < adrmMissionCommonData.m_TargetIDList.Count; j++)
		{
			TargetType targetType = MissionRepository.GetTargetType(adrmMissionCommonData.m_TargetIDList[j]);
			if (targetType != TargetType.TargetType_TowerDif && targetType != TargetType.TargetType_Follow)
			{
				continue;
			}
			foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
			{
				MissionCommonData adrmMissionCommonData2 = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, item.Key);
				if (adrmMissionCommonData2 == null)
				{
					continue;
				}
				for (int k = 0; k < adrmMissionCommonData2.m_TargetIDList.Count; k++)
				{
					TargetType targetType2 = MissionRepository.GetTargetType(adrmMissionCommonData2.m_TargetIDList[k]);
					if (targetType2 == TargetType.TargetType_TowerDif || targetType2 == TargetType.TargetType_Follow)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public Dictionary<string, string> GetMissionFlagType(int MissionID)
	{
		return (!m_MissionInfo.ContainsKey(MissionID)) ? null : m_MissionInfo[MissionID];
	}

	public string GetQuestVariable(int MissionID, string MissionFlag)
	{
		MissionFlag = MissionFlag.ToUpper();
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType != null && missionFlagType.ContainsKey(MissionFlag))
		{
			return missionFlagType[MissionFlag];
		}
		return string.Empty;
	}

	public int GetQuestVariable(int MissionID, string MissionFlag, int id)
	{
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			return 0;
		}
		foreach (KeyValuePair<string, string> item in missionFlagType)
		{
			if (!(item.Value == "0"))
			{
				string[] array = item.Value.Split('_');
				if (array.Length == 2 && !(array[0] != id.ToString()))
				{
					return Convert.ToInt32(array[1]);
				}
			}
		}
		return 0;
	}

	public bool ModifyQuestVariable(int MissionID, string MissionFlag, string MissionValue, Player player)
	{
		Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			return false;
		}
		MissionFlag.ToUpper();
		if (missionFlagType.ContainsKey(MissionFlag))
		{
			missionFlagType[MissionFlag] = MissionValue;
		}
		else
		{
			missionFlagType.Add(MissionFlag, MissionValue);
		}
		_dirty = true;
		player.RequestModifyMissionFlag(MissionID, MissionFlag, MissionValue);
		return true;
	}

	public bool ModifyQuestVariable(int MissionID, string MissionFlag, int ItemID, int num, Player player)
	{
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
		if (array.Length != 2)
		{
			return false;
		}
		int num2 = Convert.ToInt32(array[0]);
		if (num2 != ItemID)
		{
			return false;
		}
		int num3 = Convert.ToInt32(array[1]) + num;
		string missionValue = array[0] + "_" + num3;
		return ModifyQuestVariable(MissionID, MissionFlag, missionValue, player);
	}

	private int AddMissionFlagType(int MissionID, string MissionFlag, string MissionValue, Player player)
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
		if (missionFlagType != null)
		{
			ModifyQuestVariable(MissionID, MissionFlag, MissionValue, player);
			return 0;
		}
		missionFlagType = new Dictionary<string, string>();
		missionFlagType.Add(MissionFlag, MissionValue);
		m_MissionInfo.Add(MissionID, missionFlagType);
		_dirty = true;
		return 1;
	}

	public int SetQuestVariable(int missionID, string missionFlag, string missionValue, Player player)
	{
		if (missionFlag == "STEP" && !CheckGetMission(missionID, missionFlag, missionValue, player))
		{
			return 0;
		}
		return SetQuestVariable1(missionID, missionFlag, missionValue, player);
	}

	public int SetQuestVariable1(int MissionID, string MissionFlag, string MissionValue, Player player)
	{
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null)
		{
			return 0;
		}
		if (adrmMissionCommonData.IsTalkMission())
		{
			if (ServerConfig.IsStory)
			{
				CompleteMission(MissionID, player, bCheck: true, bSend: false);
			}
			else
			{
				CompleteMission(MissionID, player);
			}
		}
		else if (AddMissionFlagType(MissionID, MissionFlag, MissionValue, player) < 1)
		{
			return 0;
		}
		if (!HasGetRewards(MissionID))
		{
			ItemSample[] items = adrmMissionCommonData.m_Get_DeleteItem.Select((MissionIDNum iter) => new ItemSample(iter.id, iter.num)).ToArray();
			if (player.Package.HasEnoughItems(items))
			{
				ItemObject[] collection = player.Package.RemoveItem(items);
				List<ItemObject> effItems = new List<ItemObject>();
				effItems.AddRange(collection);
				if (m_nTeam != -1)
				{
					List<Player> teamPlayers = Player.GetTeamPlayers(player.TeamId);
					foreach (Player item in teamPlayers)
					{
						if (!(item != null) || !(item != player))
						{
							continue;
						}
						ItemSample[] array = adrmMissionCommonData.m_Get_MissionItem.Select((MissionIDNum iter) => new ItemSample(iter.id, iter.num)).ToArray();
						ItemSample[] array2 = array;
						foreach (ItemSample itemSample in array2)
						{
							if (itemSample.protoData.category != ItemCategory.IC_QuestItem)
							{
								item.Package.AddSameItems(itemSample, ref effItems);
							}
						}
						item.SyncNewItem(array);
						item.SyncItemList(effItems);
						item.SyncPackageIndex();
					}
				}
				ItemSample[] items2 = adrmMissionCommonData.m_Get_MissionItem.Select((MissionIDNum iter) => new ItemSample(iter.id, iter.num)).ToArray();
				ItemObject[] array3 = player.Package.AddSameItems(items2);
				if (array3 != null)
				{
					effItems.AddRange(array3);
					player.SyncNewItem(items2);
					player.SyncItemList(effItems);
					player.SyncPackageIndex();
					foreach (ItemObject item2 in effItems)
					{
						ReplicatorFormula cmpt = item2.GetCmpt<ReplicatorFormula>();
						if (cmpt != null && player.AddFormula(cmpt))
						{
							if (ServerConfig.IsStory)
							{
								player.SyncTeamFormulaId();
							}
							else
							{
								player.SyncFormulaId();
							}
						}
					}
				}
			}
		}
		for (int j = 0; j < adrmMissionCommonData.m_TargetIDList.Count; j++)
		{
			TargetType targetType = MissionRepository.GetTargetType(adrmMissionCommonData.m_TargetIDList[j]);
			if (targetType == TargetType.TargetType_Collect)
			{
				TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(adrmMissionCommonData.m_TargetIDList[j]);
				if (typeCollectData != null)
				{
					typeCollectData.RandItemActive();
					player.SycTeamCollectItemID(adrmMissionCommonData.m_TargetIDList[j], typeCollectData.m_ItemID, typeCollectData.m_ItemNum);
				}
			}
			if (ServerConfig.IsAdventure)
			{
				AdRandMisOperation(targetType, adrmMissionCommonData.m_TargetIDList[j], MissionID, player);
			}
			else if (ServerConfig.IsStory)
			{
				StoryMisOperation(targetType, adrmMissionCommonData.m_TargetIDList[j], MissionID, player);
			}
		}
		_dirty = true;
		return 1;
	}

	public bool IsGetTakeMission(int MissionID, Player player)
	{
		if (player == null)
		{
			return false;
		}
		if (HasMission(MissionID) || HadCompleteMission(MissionID, player))
		{
			return false;
		}
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null)
		{
			return false;
		}
		bool flag = false;
		if (adrmMissionCommonData.m_PreLimit.idlist.Count > 0)
		{
			if (adrmMissionCommonData.m_PreLimit.type == 2)
			{
				flag = true;
				for (int i = 0; i < adrmMissionCommonData.m_PreLimit.idlist.Count; i++)
				{
					if (adrmMissionCommonData.m_PreLimit.idlist[i] > 999 && adrmMissionCommonData.m_PreLimit.idlist[i] < 10000)
					{
						if (!HadCompleteTarget(adrmMissionCommonData.m_PreLimit.idlist[i]))
						{
							return false;
						}
					}
					else if (!HadCompleteMission(adrmMissionCommonData.m_PreLimit.idlist[i], player))
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < adrmMissionCommonData.m_PreLimit.idlist.Count; j++)
				{
					if (adrmMissionCommonData.m_PreLimit.idlist[j] > 999 && adrmMissionCommonData.m_PreLimit.idlist[j] < 10000)
					{
						if (HadCompleteTarget(adrmMissionCommonData.m_PreLimit.idlist[j]))
						{
							flag = true;
							break;
						}
					}
					else if (HadCompleteMission(adrmMissionCommonData.m_PreLimit.idlist[j], player))
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
		if (adrmMissionCommonData.m_MutexLimit.idlist.Count > 0)
		{
			if (adrmMissionCommonData.m_MutexLimit.type == 1)
			{
				flag = false;
				for (int k = 0; k < adrmMissionCommonData.m_MutexLimit.idlist.Count; k++)
				{
					if (HadCompleteMission(adrmMissionCommonData.m_MutexLimit.idlist[k], player) || HasMission(adrmMissionCommonData.m_MutexLimit.idlist[k]))
					{
						return false;
					}
				}
			}
			else
			{
				for (int l = 0; l < adrmMissionCommonData.m_MutexLimit.idlist.Count; l++)
				{
					if (!HadCompleteMission(adrmMissionCommonData.m_MutexLimit.idlist[l], player))
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
		return player.Package.HasEnoughSpace(adrmMissionCommonData.m_Get_DemandItem);
	}

	private void StoryMisOperation(TargetType curType, int targetid, int missionId, Player player)
	{
		switch (curType)
		{
		case TargetType.TargetType_Collect:
			GetSpecialItem.AddLootSpecialItem(targetid);
			break;
		}
	}

	private void AdRandMisOperation(TargetType curType, int targetid, int missionId, Player player)
	{
		switch (curType)
		{
		case TargetType.TargetType_KillMonster:
			break;
		case TargetType.TargetType_Collect:
			break;
		case TargetType.TargetType_Follow:
			if (!ServerConfig.IsStory)
			{
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(targetid);
				if (typeFollowData != null)
				{
					Vector3 vector7 = typeFollowData.m_AdDistPos.refertoType switch
					{
						ReferToType.Player => player.Pos, 
						ReferToType.Town => BuildingInfoManager.Instance.GetTownPos(typeFollowData.m_AdDistPos.referToID), 
						ReferToType.Npc => (!(ObjNetInterface.Get(typeFollowData.m_AdDistPos.referToID) != null)) ? player.Pos : ObjNetInterface.Get(typeFollowData.m_AdDistPos.referToID).transform.position, 
						ReferToType.Transcript => player.Pos, 
						_ => player.Pos, 
					};
					Vector2 vector8 = UnityEngine.Random.insideUnitCircle.normalized * typeFollowData.m_AdDistPos.radius1;
					Vector2 vector9 = new Vector2(vector7.x + vector8.x, vector7.z + vector8.y);
					typeFollowData.m_DistRadius = typeFollowData.m_AdDistPos.radius2;
					typeFollowData.m_DistPos = new Vector3(vector9.x, 200f, vector9.y);
					player.followdata.Add(new FollowData(player, vector9.x, vector9.y, targetid, missionId));
				}
			}
			break;
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(targetid);
			if (typeSearchData != null)
			{
				Vector3 vector4 = typeSearchData.m_mr.refertoType switch
				{
					ReferToType.Player => player.Pos, 
					ReferToType.Npc => (!(ObjNetInterface.Get(typeSearchData.m_mr.referToID) != null)) ? player.Pos : ObjNetInterface.Get(typeSearchData.m_mr.referToID).transform.position, 
					ReferToType.Town => BuildingInfoManager.Instance.GetTownPos(typeSearchData.m_mr.referToID), 
					ReferToType.Transcript => player.Pos, 
					_ => player.Pos, 
				};
				Vector2 vector5 = UnityEngine.Random.insideUnitCircle.normalized * typeSearchData.m_mr.radius1;
				Vector2 vector6 = new Vector2(vector4.x + vector5.x, vector4.z + vector5.y);
				typeSearchData.m_DistPos = new Vector3(vector6.x, 200f, vector6.y);
				typeSearchData.m_DistRadius = typeSearchData.m_mr.radius2;
				player.RequestDiscovery(vector6.x, vector6.y, targetid, missionId);
			}
			break;
		}
		case TargetType.TargetType_UseItem:
		{
			TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(targetid);
			if (typeUseItemData != null)
			{
				Vector3 vector = typeUseItemData.m_AdDistPos.refertoType switch
				{
					ReferToType.Player => player.Pos, 
					ReferToType.Town => BuildingInfoManager.Instance.GetTownPos(typeUseItemData.m_AdDistPos.referToID), 
					ReferToType.Npc => (!(ObjNetInterface.Get(typeUseItemData.m_AdDistPos.referToID) != null)) ? player.Pos : ObjNetInterface.Get(typeUseItemData.m_AdDistPos.referToID).transform.position, 
					_ => player.Pos, 
				};
				Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * typeUseItemData.m_AdDistPos.radius1;
				Vector2 vector3 = new Vector2(vector.x + vector2.x, vector.z + vector2.y);
				typeUseItemData.m_Pos = new Vector3(vector3.x, 200f, vector3.y);
				typeUseItemData.m_Radius = typeUseItemData.m_AdDistPos.radius2;
				player.SyncUseItemPos(targetid, typeUseItemData.m_Pos, missionId);
			}
			break;
		}
		case TargetType.TargetType_Messenger:
			break;
		case TargetType.TargetType_TowerDif:
			break;
		}
	}

	public bool ProcessMonsterDead(int proid, Player player)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
		{
			MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, item.Key);
			if (adrmMissionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < adrmMissionCommonData.m_TargetIDList.Count; i++)
			{
				switch (MissionRepository.GetTargetType(adrmMissionCommonData.m_TargetIDList[i]))
				{
				case TargetType.TargetType_KillMonster:
				{
					TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(adrmMissionCommonData.m_TargetIDList[i]);
					if (typeMonsterData == null)
					{
						break;
					}
					for (int j = 0; j < typeMonsterData.m_MonsterList.Count; j++)
					{
						int num = i * 10 + j;
						bool flag = false;
						if (typeMonsterData.m_MonsterList[j].id == proid)
						{
							flag = true;
						}
						if (typeMonsterData.m_CommonMonsterIds.Count > 0)
						{
							foreach (int commonMonsterId in typeMonsterData.m_CommonMonsterIds)
							{
								if (commonMonsterId == proid)
								{
									flag = true;
									break;
								}
							}
						}
						if (!flag)
						{
							continue;
						}
						string missionFlag = "MONSTER" + num;
						string questVariable = GetQuestVariable(item.Key, missionFlag);
						string[] array = questVariable.Split('_');
						int num2 = Convert.ToInt32(array[1]) + 1;
						questVariable = array[0] + "_" + num2;
						ModifyQuestVariable(item.Key, missionFlag, questVariable, player);
						if (ServerConfig.IsStory)
						{
							if (MissionRepository.IsAutoReplyMission(adrmMissionCommonData.m_ID) && !dictionary.ContainsKey(adrmMissionCommonData.m_TargetIDList[i]))
							{
								dictionary.Add(adrmMissionCommonData.m_TargetIDList[i], adrmMissionCommonData.m_ID);
							}
						}
						else if ((adrmMissionCommonData.isAutoReply || MissionRepository.IsAutoReplyMission(adrmMissionCommonData.m_ID)) && !dictionary.ContainsKey(adrmMissionCommonData.m_TargetIDList[i]))
						{
							dictionary.Add(adrmMissionCommonData.m_TargetIDList[i], adrmMissionCommonData.m_ID);
						}
					}
					break;
				}
				case TargetType.TargetType_Collect:
					GetSpecialItem.AddLootSpecialItem(adrmMissionCommonData.m_TargetIDList[i]);
					break;
				}
			}
		}
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			CompleteTarget(item2.Key, item2.Value, player);
		}
		return true;
	}

	public void ProcessItemMission(int itemid, Player player, List<IntVector3> vclist, int objId = 0)
	{
		int num = 0;
		if (itemid == 1541)
		{
			_languegeSkill++;
			player.RPCOthers(EPacketType.PT_InGame_LanguegeSkill, _languegeSkill);
		}
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_MissionInfo)
		{
			if (itemid == 1339)
			{
				continue;
			}
			MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, item.Key);
			if (adrmMissionCommonData == null)
			{
				continue;
			}
			Dictionary<string, string> value = item.Value;
			if (value == null)
			{
				continue;
			}
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				int num2 = 0;
				foreach (KeyValuePair<string, string> item2 in value)
				{
					if (num2 == i)
					{
						text = item2.Key;
						text2 = item2.Value;
						break;
					}
					num2++;
				}
				string text3 = text;
				if (!text3.Contains("ITEM"))
				{
					continue;
				}
				string[] array = text2.Split('_');
				if (array.Length != 2 || array[0] != itemid.ToString())
				{
					continue;
				}
				for (int j = 0; j < adrmMissionCommonData.m_TargetIDList.Count; j++)
				{
					TargetType targetType = MissionRepository.GetTargetType(adrmMissionCommonData.m_TargetIDList[j]);
					if (targetType != TargetType.TargetType_UseItem)
					{
						continue;
					}
					num = 0;
					TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(adrmMissionCommonData.m_TargetIDList[j]);
					if (typeUseItemData == null || typeUseItemData.m_ItemID != itemid)
					{
						continue;
					}
					if (typeUseItemData.m_Type == 1)
					{
						if (ServerConfig.IsStory)
						{
							if (vclist == null)
							{
								continue;
							}
							float num3 = 9999f;
							if (item.Key == 550)
							{
								List<ItemObject> itemByProtoId = ItemManager.GetItemByProtoId(1343);
								foreach (ItemObject item3 in itemByProtoId)
								{
									if (item3 == null)
									{
										continue;
									}
									SceneObject sceneObj = GameWorld.GetSceneObj(item3.instanceId, player.WorldId);
									if (sceneObj == null)
									{
										continue;
									}
									for (int k = 0; k < vclist.Count; k++)
									{
										num3 = Vector3.Distance(vclist[k], sceneObj.Pos);
										if (!(num3 > (float)typeUseItemData.m_Radius))
										{
											num++;
										}
									}
								}
							}
							else if (typeUseItemData.m_Pos == new Vector3(-255f, -255f, -255f))
							{
								Vector3 assemblyPos = ColonyMgr._Instance.GetAssemblyPos(player.TeamId);
								for (int l = 0; l < vclist.Count; l++)
								{
									num3 = Vector3.Distance(vclist[l], assemblyPos);
									if (!(num3 > (float)typeUseItemData.m_Radius))
									{
										num++;
									}
								}
							}
							else
							{
								for (int m = 0; m < vclist.Count; m++)
								{
									num3 = Vector3.Distance(vclist[m], typeUseItemData.m_Pos);
									if (!(num3 > (float)typeUseItemData.m_Radius))
									{
										num++;
									}
								}
							}
						}
						else
						{
							for (int n = 0; n < vclist.Count; n++)
							{
								Vector2 a = new Vector2(vclist[n].x, vclist[n].z);
								Vector2 b = new Vector2(typeUseItemData.m_Pos.x, typeUseItemData.m_Pos.z);
								float num4 = Vector2.Distance(a, b);
								if (!(num4 > (float)typeUseItemData.m_Radius))
								{
									num++;
								}
							}
						}
					}
					else
					{
						num++;
					}
					if (num == 0)
					{
						continue;
					}
					int num5 = Convert.ToInt32(array[1]) + num;
					string missionValue = array[0] + "_" + num5;
					ModifyQuestVariable(item.Key, text3, missionValue, player);
					if (num5 < typeUseItemData.m_UseNum)
					{
						continue;
					}
					if (ServerConfig.IsStory)
					{
						if (!MissionRepository.IsAutoReplyMission(adrmMissionCommonData.m_ID))
						{
							continue;
						}
						CompleteTarget(adrmMissionCommonData.m_TargetIDList[j], adrmMissionCommonData.m_ID, player);
						if (item.Key == 953)
						{
							player.RPCOthers(EPacketType.PT_InGame_Mission953, objId);
						}
						return;
					}
					CompleteTarget(adrmMissionCommonData.m_TargetIDList[j], adrmMissionCommonData.m_ID, player);
					return;
				}
			}
		}
	}

	private void ProcessRandomMission(int MissionID, MissionCommonData data, Player player)
	{
		if (MissionManager.Manager.DeleteAdrmDataMap(player.Id, MissionID) && MissionRepository.m_MissionCommonMap.ContainsKey(MissionID))
		{
			if (data.m_TargetIDList.Count > 0)
			{
				MissionRepository.DeleteRandomMissionData(data.m_TargetIDList[0]);
			}
			MissionRepository.m_MissionCommonMap.Remove(MissionID);
		}
		int num = 0;
		num = ((data.m_iReplyNpc == 0) ? data.m_iNpc : data.m_iReplyNpc);
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(num);
		if (null == aiAdNpcNetwork)
		{
			return;
		}
		NpcMissionData mission = aiAdNpcNetwork.mission;
		if (mission == null || mission.m_RandomMission != MissionID)
		{
			return;
		}
		if (mission.m_MissionListReply.Contains(MissionID))
		{
			mission.m_MissionListReply.Remove(MissionID);
		}
		mission.mCurComMisNum++;
		if (ServerConfig.IsAdventure || ServerConfig.IsBuild)
		{
			if (data.m_MaxNum != -1)
			{
				mission.m_CurMissionGroup++;
			}
			mission.m_RandomMission = AdRMRepository.GetRandomMission(mission);
		}
	}

	public void CompleteTarget(int TargetID, int MissionID, Player player)
	{
		if (HadCompleteTarget(TargetID) || !HasMission(MissionID) || !IsReplyTarget(MissionID, TargetID, player))
		{
			return;
		}
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData != null)
		{
			int num = TargetID % 1000;
			if (num < 500)
			{
				m_MissionTargetState.Add(TargetID, 1);
			}
			player.CompleteTarget(TargetID, MissionID);
			ProcessTargetByMissionForServer(-1, MissionID, player);
		}
	}

	public void ProcessTargetByMission(int TargetID, int MissionID, Player player, bool bCheck = true)
	{
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData != null)
		{
			bool flag = true;
			if (!ServerConfig.IsStory || true)
			{
				CompleteMission(MissionID, player, TargetID, bCheck);
			}
		}
	}

	public void ProcessTargetByMissionForServer(int TargetID, int MissionID, Player player, bool bCheck = true)
	{
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < adrmMissionCommonData.m_TargetIDList.Count; i++)
		{
			int num = adrmMissionCommonData.m_TargetIDList[i] % 1000;
			if (ServerConfig.IsStory)
			{
				if (!HadCompleteTarget(adrmMissionCommonData.m_TargetIDList[i]))
				{
					flag = false;
					break;
				}
				continue;
			}
			if (!HadCompleteTarget(adrmMissionCommonData.m_TargetIDList[i]) && num < 500)
			{
				flag = false;
				break;
			}
			if (ForceCompleteMission(adrmMissionCommonData.m_TargetIDList[i]))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			CompleteMission(MissionID, player, TargetID, bCheck);
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

	public void CompleteMission(int MissionID, Player player, bool bCheck = true, bool bSend = true)
	{
		CompleteMission(MissionID, player, -1, bCheck, bSend);
	}

	public void CompleteMission(int MissionID, Player player, int TargetID, bool bCheck = true, bool notTalkMission = true)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return;
		}
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null || (!adrmMissionCommonData.IsTalkMission() && HadCompleteMission(MissionID, player)) || !IsReplyMission(MissionID, player))
		{
			return;
		}
		player.ReplyCompleteMission(TargetID, MissionID, bCheck);
		for (int i = 0; i < adrmMissionCommonData.m_TargetIDList.Count; i++)
		{
			TargetType targetType = MissionRepository.GetTargetType(adrmMissionCommonData.m_TargetIDList[i]);
			if (targetType == TargetType.TargetType_Collect)
			{
				GetSpecialItem.RemoveLootSpecialItem(adrmMissionCommonData.m_TargetIDList[i]);
			}
		}
		DelMissionInfo(MissionID, player);
		if (MissionManager.HasRandomMission(MissionID))
		{
			ProcessRandomMission(MissionID, adrmMissionCommonData, player);
		}
		SetMission(MissionID, player);
		int num = 0;
		int num2 = 0;
		List<ItemObject> effItems = new List<ItemObject>(10);
		for (int j = 0; j < adrmMissionCommonData.m_Com_RemoveItem.Count; j++)
		{
			if (adrmMissionCommonData.m_Com_RemoveItem[j].id <= 0)
			{
				continue;
			}
			num2 = adrmMissionCommonData.m_Com_RemoveItem[j].num;
			if (m_nTeam > -1)
			{
				List<Player> list = ObjNetInterface.Get<Player>();
				foreach (Player item in list)
				{
					if (!(null == item))
					{
						num = item.GetItemNum(adrmMissionCommonData.m_Com_RemoveItem[j].id);
						if (num2 > num)
						{
							player.Package.RemoveItem(adrmMissionCommonData.m_Com_RemoveItem[j].id, num, ref effItems);
							num2 = adrmMissionCommonData.m_Com_RemoveItem[j].num - num;
						}
						else
						{
							player.Package.RemoveItem(adrmMissionCommonData.m_Com_RemoveItem[j].id, num2, ref effItems);
						}
					}
				}
			}
			else
			{
				player.Package.RemoveItem(adrmMissionCommonData.m_Com_RemoveItem[j].id, num2, ref effItems);
			}
		}
		RewardItem(adrmMissionCommonData, player);
		if (adrmMissionCommonData.m_Type == MissionType.MissionType_Var && adrmMissionCommonData.m_VarValueID != 0)
		{
			string questVariable = GetQuestVariable(MissionID, "STEP");
			int num3 = Convert.ToInt32(questVariable) + adrmMissionCommonData.m_VarValue;
			ModifyQuestVariable(adrmMissionCommonData.m_VarValueID, "STEP", num3.ToString(), player);
		}
		if (adrmMissionCommonData.m_increaseChain)
		{
			AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get((adrmMissionCommonData.m_iReplyNpc == 0) ? adrmMissionCommonData.m_iNpc : adrmMissionCommonData.m_iReplyNpc) as AiAdNpcNetwork;
			if (aiAdNpcNetwork == null)
			{
				return;
			}
			NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(aiAdNpcNetwork.Id);
			if (missionData == null)
			{
				return;
			}
			missionData.m_CurMissionGroup++;
			missionData.m_RandomMission = AdRMRepository.GetRandomMission(missionData);
		}
		if (effItems.Count > 0)
		{
			player.SyncItemList(effItems);
		}
		player.SyncPackageIndex();
		ActionEventsMgr._self.ProcessAction(OperatorEnum.Oper_Mission, ActionOpportunity.Opp_OnComplete, player);
	}

	public bool HadCompleteTarget(int TargetID)
	{
		return m_MissionTargetState.ContainsKey(TargetID);
	}

	public bool HadCompleteMission(int MissionID, Player player)
	{
		int playerId = -1;
		if (player != null)
		{
			playerId = player.Id;
		}
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(playerId, MissionID);
		if (adrmMissionCommonData == null)
		{
			return false;
		}
		if (adrmMissionCommonData.m_MaxNum == -1)
		{
			return false;
		}
		if (m_MissionState.ContainsKey(MissionID))
		{
			if (m_MissionState[MissionID] >= adrmMissionCommonData.m_MaxNum)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool SetMission(int MissionID, Player player)
	{
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null)
		{
			return false;
		}
		if (adrmMissionCommonData.m_MaxNum == -1)
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
		return true;
	}

	public int IsSpecialID(int itemid)
	{
		int result = 0;
		switch (itemid)
		{
		case 1322:
			result = 16;
			break;
		case 1323:
			result = 32;
			break;
		case 1324:
			result = 33;
			break;
		case 1325:
			result = 100;
			break;
		case 1326:
			result = 144;
			break;
		case 1327:
			result = 144;
			break;
		case 1328:
			result = 144;
			break;
		case 1329:
			result = 160;
			break;
		case 1330:
			result = 160;
			break;
		}
		return result;
	}

	public bool IsReplyTarget(int MissionID, int TargetID, Player player)
	{
		int num = 0;
		switch (MissionRepository.GetTargetType(TargetID))
		{
		case TargetType.TargetType_Collect:
		{
			TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(TargetID);
			if (typeCollectData == null)
			{
				return false;
			}
			int num4 = IsSpecialID(typeCollectData.m_ItemID);
			if (m_nTeam > -1)
			{
				List<Player> list = ObjNetInterface.Get<Player>();
				foreach (Player item in list)
				{
					if (!(null == item))
					{
						num = ((num4 <= 0) ? (num + item.GetItemNum(typeCollectData.m_ItemID)) : (num + item.Package.GetCreationItemCount((ECreation)num4)));
					}
				}
				if (num < typeCollectData.m_ItemNum)
				{
					return false;
				}
			}
			else if (num4 > 0)
			{
				if (player.Package.GetCreationItemCount((ECreation)num4) < typeCollectData.m_ItemNum)
				{
					return false;
				}
			}
			else if (player.GetItemNum(typeCollectData.m_ItemID) < typeCollectData.m_ItemNum)
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
				num = GetQuestVariable(MissionID, "monster", typeMonsterData.m_MonsterList[i].id);
				if (num < typeMonsterData.m_MonsterList[i].num)
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
			num = GetQuestVariable(MissionID, "item", typeUseItemData.m_ItemID);
			if (num < typeUseItemData.m_UseNum)
			{
				return false;
			}
			break;
		}
		case TargetType.TargetType_Follow:
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(TargetID);
			if (typeFollowData == null)
			{
				return false;
			}
			Vector3 vector = typeFollowData.m_DistPos;
			if (ServerConfig.IsStory)
			{
				if (typeFollowData.m_LookNameID != 0)
				{
					AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(typeFollowData.m_LookNameID);
					if (aiAdNpcNetwork != null)
					{
						vector = aiAdNpcNetwork.transform.position;
					}
				}
				else if (typeFollowData.m_BuildID <= 0)
				{
				}
				Vector2 b = new Vector2(vector.x, vector.z);
				for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
				{
					int id = Convert.ToInt32(typeFollowData.m_iNpcList[j]);
					AiAdNpcNetwork aiAdNpcNetwork2 = ObjNetInterface.Get<AiAdNpcNetwork>(id);
					Vector2 a = new Vector2(aiAdNpcNetwork2.transform.position.x, aiAdNpcNetwork2.transform.position.z);
					float num2 = Vector2.Distance(a, b);
					if (num2 <= (float)(typeFollowData.m_DistRadius + 2))
					{
						break;
					}
				}
				break;
			}
			Vector2 b2 = new Vector2(vector.x, vector.z);
			for (int k = 0; k < typeFollowData.m_iNpcList.Count; k++)
			{
				int id2 = Convert.ToInt32(typeFollowData.m_iNpcList[k]);
				AiAdNpcNetwork aiAdNpcNetwork3 = ObjNetInterface.Get<AiAdNpcNetwork>(id2);
				Vector2 a2 = new Vector2(aiAdNpcNetwork3.transform.position.x, aiAdNpcNetwork3.transform.position.z);
				float num3 = Vector2.Distance(a2, b2);
				if (typeFollowData.m_DistRadius == 0)
				{
					typeFollowData.m_DistRadius = typeFollowData.m_AdDistPos.radius2;
				}
				if (num3 > (float)(typeFollowData.m_DistRadius + 2))
				{
					return false;
				}
			}
			break;
		}
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(TargetID);
			if (typeSearchData == null)
			{
				return false;
			}
			break;
		}
		}
		return true;
	}

	public bool IsReplyMission(int MissionID, Player player)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return false;
		}
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null)
		{
			return false;
		}
		if (!adrmMissionCommonData.IsTalkMission() && (!HasMission(MissionID) || HadCompleteMission(MissionID, player)))
		{
			return false;
		}
		if (!player.Package.HasEnoughSpace(adrmMissionCommonData.m_Com_RewardItem))
		{
			return false;
		}
		if (adrmMissionCommonData.m_Type != MissionType.MissionType_Mul)
		{
			for (int i = 0; i < adrmMissionCommonData.m_TargetIDList.Count; i++)
			{
				if (!IsReplyTarget(MissionID, adrmMissionCommonData.m_TargetIDList[i], player))
				{
					return false;
				}
			}
		}
		else
		{
			for (int j = 0; j < adrmMissionCommonData.m_TargetIDList.Count && !IsReplyTarget(MissionID, adrmMissionCommonData.m_TargetIDList[j], player); j++)
			{
			}
		}
		return true;
	}

	private void RewardItem(MissionCommonData data, Player player)
	{
		List<MissionIDNum> list = null;
		if (data.m_Type == MissionType.MissionType_Mul)
		{
			int num = 0;
			for (int i = 0; i < data.m_TargetIDList.Count; i++)
			{
				if (IsReplyTarget(data.m_ID, data.m_TargetIDList[i], player))
				{
					num++;
				}
			}
			num--;
			if (num >= 0 && data.m_Com_MulRewardItem.ContainsKey(num))
			{
				list = data.m_Com_MulRewardItem[num];
			}
		}
		else
		{
			list = data.m_Com_RewardItem;
		}
		if (list == null)
		{
			return;
		}
		IEnumerable<ItemSample> enumerable = list.Select((MissionIDNum iter) => new ItemSample(iter.id, iter.num));
		if (m_nTeam == -1)
		{
			ItemObject[] array = player.Package.AddSameItems(enumerable);
			if (array != null)
			{
				player.SyncItemList(array);
				player.SyncNewItem(enumerable.ToArray());
			}
			player.SyncPackageIndex();
			return;
		}
		List<Player> list2 = ObjNetInterface.Get<Player>();
		foreach (Player item in list2)
		{
			if (item == null || item.TeamId != m_nTeam)
			{
				continue;
			}
			ItemObject[] array2 = item.Package.AddSameItems(enumerable);
			if (array2 != null)
			{
				ItemObject[] array3 = array2;
				foreach (ItemObject itemObject in array3)
				{
					ReplicatorFormula cmpt = itemObject.GetCmpt<ReplicatorFormula>();
					if (cmpt != null && item.AddFormula(cmpt))
					{
						if (ServerConfig.IsStory)
						{
							item.SyncTeamFormulaId();
						}
						else
						{
							item.SyncFormulaId();
						}
					}
				}
				item.SyncItemList(array2);
				item.SyncNewItem(enumerable.ToArray());
			}
			item.SyncPackageIndex();
		}
	}

	public byte[] Export(int type)
	{
		byte[] array = null;
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(m_GetRewards.Count);
		for (int i = 0; i < m_GetRewards.Count; i++)
		{
			binaryWriter.Write(m_GetRewards[i]);
		}
		binaryWriter.Write(m_MissionTargetState.Count);
		foreach (KeyValuePair<int, int> item in m_MissionTargetState)
		{
			binaryWriter.Write(item.Key);
			binaryWriter.Write(item.Value);
		}
		binaryWriter.Write(m_MissionState.Count);
		foreach (KeyValuePair<int, int> item2 in m_MissionState)
		{
			binaryWriter.Write(item2.Key);
			binaryWriter.Write(item2.Value);
		}
		binaryWriter.Write(m_MissionInfo.Count);
		foreach (KeyValuePair<int, Dictionary<string, string>> item3 in m_MissionInfo)
		{
			binaryWriter.Write(item3.Key);
			binaryWriter.Write(item3.Value.Count);
			foreach (KeyValuePair<string, string> item4 in item3.Value)
			{
				binaryWriter.Write(item4.Key);
				binaryWriter.Write(item4.Value);
			}
		}
		binaryWriter.Write(_languegeSkill);
		if (type == 1)
		{
			binaryWriter.Write(m_nTeam);
			binaryWriter.Write(m_playerName);
		}
		binaryWriter.Close();
		array = memoryStream.ToArray();
		memoryStream.Close();
		return array;
	}

	public void Import(byte[] buffer, int type)
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
				m_MissionInfo.Add(key, dictionary);
			}
			_languegeSkill = binaryReader.ReadInt32();
			if (type == 1)
			{
				m_nTeam = binaryReader.ReadInt32();
				m_playerName = binaryReader.ReadString();
			}
			binaryReader.Close();
		}
		memoryStream.Close();
	}

	public static Vector3 GetPatrolPoint(Vector3 center, int iMin, int iMax)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(iMin, iMax);
		return center + new Vector3(vector.x, center.y, vector.y);
	}

	public void DelMissionInfo(int MissionID, Player player)
	{
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData != null && HasMission(MissionID))
		{
			m_MissionInfo.Remove(MissionID);
			_dirty = true;
		}
	}

	public void FailureMission(Player player, int MissionID)
	{
		if (MissionID < 0 || MissionID > 20000 || !HasMission(MissionID))
		{
			return;
		}
		MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(player.Id, MissionID);
		if (adrmMissionCommonData == null)
		{
			return;
		}
		for (int i = 0; i < adrmMissionCommonData.m_ResetID.Count; i++)
		{
			if (m_MissionState.ContainsKey(adrmMissionCommonData.m_ResetID[i]))
			{
				m_MissionState.Remove(adrmMissionCommonData.m_ResetID[i]);
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(adrmMissionCommonData.m_ResetID[i]);
				for (int j = 0; j < missionCommonData.m_TargetIDList.Count; j++)
				{
					m_MissionTargetState.Remove(missionCommonData.m_TargetIDList[j]);
				}
			}
		}
		player.FailMission(MissionID);
		DelMissionInfo(MissionID, player);
	}
}
