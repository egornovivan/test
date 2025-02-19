using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
	public const int m_SpecialMissionID5 = 71;

	public const int m_SpecialMissionID9 = 888;

	public const int m_SpecialMissionID10 = 242;

	public const int m_SpecialMissionID13 = 997;

	public const int m_SpecialMissionID14 = 998;

	public const int m_SpecialMissionID15 = 999;

	public const int m_SpecialMissionID16 = 191;

	public const int m_SpecialMissionID22 = 66;

	public const int m_SpecialMissionID24 = 67;

	public const int m_SpecialMissionID31 = 204;

	public const int m_SpecialMissionID42 = 212;

	public const int m_SpecialMissionID43 = 158;

	public const int m_SpecialMissionID45 = 8;

	public const int m_SpecialMissionID47 = 889;

	public const int m_SpecialMissionID51 = 254;

	public const int m_SpecialMissionID52 = 139;

	public const int m_SpecialMissionID53 = 61;

	public const int m_SpecialMissionID55 = 251;

	public const int m_SpecialMissionID58 = 444;

	public const int m_SpecialMissionID59 = 480;

	public const int m_SpecialMissionID60 = 481;

	public const int m_SpecialMissionID61 = 505;

	public const int m_SpecialMissionID62 = 506;

	public const int m_SpecialMissionID63 = 507;

	public const int m_SpecialMissionID64 = 497;

	public const int m_SpecialMissionID65 = 550;

	public const int m_SpecialMissionID66 = 553;

	public const int m_SpecialMissionID67 = 554;

	public const int m_SpecialMissionID68 = 500;

	public const int m_SpecialMissionID69 = 562;

	public const int m_SpecialMissionID80 = 629;

	public const int m_SpecialMissionID81 = 628;

	public const int m_SpecialMissionID82 = 710;

	public const int m_SpecialMissionID83 = 678;

	public const int m_SpecialMissionID84 = 700;

	public const int m_SpecialMissionID85 = 703;

	public const int m_SpecialMissionID86 = 704;

	public const int m_SpecialMissionID87 = 697;

	public const int m_SpecialMissionID88 = 714;

	public const int m_SpecialMissionID89 = 822;

	public const int m_SpecialMissionID90 = 825;

	public const int m_SpecialMissionID91 = 826;

	public const int m_SpecialMissionID92 = 846;

	public const int m_SpecialMissionID93 = 953;

	public int m_oidx;

	public int m_nNpcObjectID;

	public static MissionManager Manager;

	private Dictionary<int, Dictionary<int, MissionCommonData>> m_AdrmDataMap = new Dictionary<int, Dictionary<int, MissionCommonData>>();

	private Dictionary<int, PlayerMission> m_TeamMissionMap = new Dictionary<int, PlayerMission>();

	private Dictionary<int, PlayerMission> m_PlayerMissionMap = new Dictionary<int, PlayerMission>();

	private void Awake()
	{
		Manager = this;
	}

	public void AddTeamPlayerMission(int team, PlayerMission pm)
	{
		if (!m_TeamMissionMap.ContainsKey(team))
		{
			m_TeamMissionMap.Add(team, pm);
		}
	}

	public PlayerMission GetTeamPlayerMission(int team)
	{
		if (m_TeamMissionMap.ContainsKey(team))
		{
			return m_TeamMissionMap[team];
		}
		return null;
	}

	public PlayerMission GetCurPlayerMission(int playerId)
	{
		if (m_PlayerMissionMap.ContainsKey(playerId))
		{
			return m_PlayerMissionMap[playerId];
		}
		PlayerMission playerMission = new PlayerMission();
		m_PlayerMissionMap.Add(playerId, playerMission);
		return playerMission;
	}

	public PlayerMission GetCurTeamMission(int playerId)
	{
		int playerTeamId = Player.GetPlayerTeamId(playerId);
		PlayerMission playerMission = GetTeamPlayerMission(playerTeamId);
		if (playerMission == null)
		{
			playerMission = new PlayerMission();
			AddTeamPlayerMission(playerTeamId, playerMission);
		}
		return playerMission;
	}

	public PlayerMission GetCurPlayerMissionByMissionId(int playerId, int missionId)
	{
		PlayerMission curTeamMission = GetCurTeamMission(playerId);
		if (curTeamMission != null)
		{
			foreach (KeyValuePair<int, Dictionary<string, string>> item in curTeamMission.m_MissionInfo)
			{
				if (item.Key == missionId)
				{
					return curTeamMission;
				}
			}
			foreach (KeyValuePair<int, int> item2 in curTeamMission.m_MissionState)
			{
				if (item2.Key == missionId)
				{
					return curTeamMission;
				}
			}
		}
		curTeamMission = GetCurPlayerMission(playerId);
		if (curTeamMission != null)
		{
			foreach (KeyValuePair<int, Dictionary<string, string>> item3 in curTeamMission.m_MissionInfo)
			{
				if (item3.Key == missionId)
				{
					return curTeamMission;
				}
			}
			foreach (KeyValuePair<int, int> item4 in curTeamMission.m_MissionState)
			{
				if (item4.Key == missionId)
				{
					return curTeamMission;
				}
			}
		}
		return null;
	}

	public void AddAdrmData(int playerId, MissionCommonData data)
	{
		if (m_AdrmDataMap.ContainsKey(playerId))
		{
			if (m_AdrmDataMap[playerId].ContainsKey(data.m_ID))
			{
				m_AdrmDataMap[playerId][data.m_ID] = data;
			}
			else
			{
				m_AdrmDataMap[playerId].Add(data.m_ID, data);
			}
		}
		else
		{
			Dictionary<int, MissionCommonData> dictionary = new Dictionary<int, MissionCommonData>();
			dictionary.Add(data.m_ID, data);
			m_AdrmDataMap.Add(playerId, dictionary);
		}
		if (!ServerConfig.IsStory && (!ServerConfig.IsAdventure || ServerConfig.IsVS))
		{
			return;
		}
		Player player = ObjNetInterface.Get(playerId) as Player;
		if (!(player != null))
		{
			return;
		}
		if (m_AdrmDataMap.ContainsKey(player.TeamId))
		{
			if (m_AdrmDataMap[player.TeamId].ContainsKey(data.m_ID))
			{
				m_AdrmDataMap[player.TeamId][data.m_ID] = data;
			}
			else
			{
				m_AdrmDataMap[player.TeamId].Add(data.m_ID, data);
			}
		}
		else
		{
			Dictionary<int, MissionCommonData> dictionary2 = new Dictionary<int, MissionCommonData>();
			dictionary2.Add(data.m_ID, data);
			m_AdrmDataMap.Add(player.TeamId, dictionary2);
		}
	}

	public bool DeleteAdrmDataMap(int playerId, int MissionID)
	{
		if (m_AdrmDataMap.ContainsKey(playerId) && m_AdrmDataMap[playerId].ContainsKey(MissionID))
		{
			m_AdrmDataMap[playerId].Remove(MissionID);
			return true;
		}
		return false;
	}

	public Dictionary<int, MissionCommonData> GetAdrmDataMap(int playerId)
	{
		if (m_AdrmDataMap.ContainsKey(playerId))
		{
			return m_AdrmDataMap[playerId];
		}
		return null;
	}

	public MissionCommonData GetAdrmMissionCommonData(int playerId, int MissionID)
	{
		int playerTeamId = Player.GetPlayerTeamId(playerId);
		if (m_AdrmDataMap.ContainsKey(playerTeamId) && m_AdrmDataMap[playerTeamId].ContainsKey(MissionID))
		{
			return m_AdrmDataMap[playerTeamId][MissionID];
		}
		if (m_AdrmDataMap.ContainsKey(playerId) && m_AdrmDataMap[playerId].ContainsKey(MissionID))
		{
			return m_AdrmDataMap[playerId][MissionID];
		}
		return GetMissionCommonData(MissionID);
	}

	public MissionCommonData GetAdrmData(int playerId, int MissionID)
	{
		int playerTeamId = Player.GetPlayerTeamId(playerId);
		if (m_AdrmDataMap.ContainsKey(playerTeamId) && m_AdrmDataMap[playerTeamId].ContainsKey(MissionID))
		{
			return m_AdrmDataMap[playerTeamId][MissionID];
		}
		if (m_AdrmDataMap.ContainsKey(playerId) && m_AdrmDataMap[playerId].ContainsKey(MissionID))
		{
			return m_AdrmDataMap[playerId][MissionID];
		}
		return null;
	}

	public static void LoadData()
	{
		AdRMRepository.LoadData();
		MissionRepository.LoadData();
		RMRepository.LoadRandMission();
	}

	public static bool InvalidNpc(AiAdNpcNetwork npc)
	{
		foreach (KeyValuePair<int, Dictionary<int, MissionCommonData>> item in Manager.m_AdrmDataMap)
		{
			foreach (KeyValuePair<int, MissionCommonData> item2 in item.Value)
			{
				if (item2.Value.m_iNpc == npc.Id || item2.Value.m_iReplyNpc == npc.Id)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static MissionCommonData GetMissionCommonData(int MissionID)
	{
		int num = MissionID / 1000;
		if (num == 9)
		{
			if (ServerConfig.IsStory)
			{
				return RMRepository.GetRandomMission(MissionID);
			}
			return AdRMRepository.GetAdRandomMission(MissionID);
		}
		return MissionRepository.GetMissionCommonData(MissionID);
	}

	public static bool HasRandomMission(int MissionID)
	{
		if (ServerConfig.IsStory)
		{
			return RMRepository.HasRandomMission(MissionID);
		}
		if (MissionID == 9135 || MissionID == 9136 || MissionID == 9137 || MissionID == 9138)
		{
			return false;
		}
		return AdRMRepository.HasAdRandomMission(MissionID);
	}

	public static TypeMonsterData GetTypeMonsterData(int TargetID)
	{
		if (ServerConfig.IsStory)
		{
			return MissionRepository.GetTypeMonsterData(TargetID);
		}
		return AdRMRepository.GetAdTypeMonsterData(TargetID);
	}

	public static TypeCollectData GetTypeCollectData(int TargetID)
	{
		if (ServerConfig.IsStory)
		{
			return MissionRepository.GetTypeCollectData(TargetID);
		}
		return AdRMRepository.GetAdTypeCollectData(TargetID);
	}

	public static TypeFollowData GetTypeFollowData(int TargetID)
	{
		if (ServerConfig.IsStory)
		{
			return MissionRepository.GetTypeFollowData(TargetID);
		}
		return AdRMRepository.GetAdTypeFollowData(TargetID);
	}

	public static TypeSearchData GetTypeSearchData(int TargetID)
	{
		if (ServerConfig.IsStory)
		{
			return MissionRepository.GetTypeSearchData(TargetID);
		}
		return AdRMRepository.GetAdTypeSearchData(TargetID);
	}

	public static TypeUseItemData GetTypeUseItemData(int TargetID)
	{
		if (ServerConfig.IsStory)
		{
			return MissionRepository.GetTypeUseItemData(TargetID);
		}
		return AdRMRepository.GetAdTypeUseItemData(TargetID);
	}

	public static TypeMessengerData GetTypeMessengerData(int TargetID)
	{
		if (ServerConfig.IsStory)
		{
			return MissionRepository.GetTypeMessengerData(TargetID);
		}
		return AdRMRepository.GetAdTypeMessengerData(TargetID);
	}

	public static TypeTowerDefendsData GetTypeTowerDefendsData(int TargetID)
	{
		if (ServerConfig.IsStory)
		{
			return MissionRepository.GetTypeTowerDefendsData(TargetID);
		}
		return AdRMRepository.GetAdTypeTowerDefendsData(TargetID);
	}

	private void SavePlayerMissions()
	{
		if (m_PlayerMissionMap.Count > 0)
		{
			PlayerMissionMgrData playerMissionMgrData = new PlayerMissionMgrData();
			playerMissionMgrData.ExportData(m_PlayerMissionMap);
			AsyncSqlite.AddRecord(playerMissionMgrData);
		}
	}

	private void SaveTeamMissions()
	{
		if (m_TeamMissionMap.Count > 0)
		{
			TeamMissionMgrData teamMissionMgrData = new TeamMissionMgrData();
			teamMissionMgrData.ExportData(m_TeamMissionMap);
			AsyncSqlite.AddRecord(teamMissionMgrData);
		}
	}

	public void LoadPlayerMissionComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("roleid"));
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("pmdata"));
			byte[] array = (byte[])reader.GetValue(reader.GetOrdinal("adrmdata"));
			PlayerMission playerMission = new PlayerMission();
			playerMission.Import(buffer, 0);
			m_PlayerMissionMap.Add(@int, playerMission);
			if (array != null)
			{
				Dictionary<int, MissionCommonData> dictionary = new Dictionary<int, MissionCommonData>();
				if (ServerConfig.IsStory)
				{
					RMRepository.Import(array);
				}
				else
				{
					AdRMRepository.Import(array, dictionary);
				}
				m_AdrmDataMap.Add(@int, dictionary);
			}
		}
	}

	public void LoadPlayerMission()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM playermission;");
			pEDbOp.BindReaderHandler(LoadPlayerMissionComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void LoadTeamMissionComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int num = Convert.ToInt32(reader.GetString(reader.GetOrdinal("team")));
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("pmdata"));
			byte[] array = (byte[])reader.GetValue(reader.GetOrdinal("adrmdata"));
			byte[] buffer2 = (byte[])reader.GetValue(reader.GetOrdinal("missionitems"));
			PlayerMission playerMission = new PlayerMission();
			playerMission.Import(buffer, 1);
			m_TeamMissionMap.Add(num, playerMission);
			MissionPackageMgr.Import(buffer2, num);
			if (array != null)
			{
				Dictionary<int, MissionCommonData> dictionary = new Dictionary<int, MissionCommonData>();
				if (ServerConfig.IsStory)
				{
					RMRepository.Import(array);
				}
				else
				{
					AdRMRepository.Import(array, dictionary);
				}
				m_AdrmDataMap.Add(num, dictionary);
			}
		}
	}

	public void LoadTeamMission()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM teammission;");
			pEDbOp.BindReaderHandler(LoadTeamMissionComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void SaveMissions()
	{
		SavePlayerMissions();
		SaveTeamMissions();
	}
}
