using System.Collections.Generic;
using System.IO;
using System.Xml;
using ItemAsset;
using Pathea;
using PETools;
using Steamworks;
using UnityEngine;

public class SteamAchievementsSystem : MonoBehaviour
{
	private class AchievementInfo
	{
		public Eachievement m_eAchievementID;

		public Estat m_eStatID;

		public string m_apiName;

		public string m_strDescription;

		public bool m_bAchieved;

		public int m_ProtoId;

		public AchievementInfo(Eachievement achievementID, Estat statID, string name, string desc, int protoId)
		{
			m_eAchievementID = achievementID;
			m_eStatID = statID;
			m_apiName = name;
			m_strDescription = desc;
			m_bAchieved = false;
			m_ProtoId = protoId;
		}
	}

	public class MissionAchievement
	{
		public enum Emisschedule
		{
			Accomplish,
			Get,
			Max
		}

		public int _missionID;

		public Emisschedule _accomplishOrGet;

		public MissionAchievement(int missionid, Emisschedule accomOrget)
		{
			_missionID = missionid;
			_accomplishOrGet = accomOrget;
		}

		public bool EquitMission(int id, Emisschedule accomOrget)
		{
			return _missionID != 0 && _missionID == id && _accomplishOrGet == accomOrget;
		}
	}

	private const int VERSIONS_0 = 0;

	private int CUR_VERSIONS;

	private static SteamAchievementsSystem instance;

	private AchievementInfo[] m_Achievements;

	private MissionAchievement[] m_MissionAchs;

	private string[] m_StatNames;

	private Istat[] m_Stats;

	public List<GameObject> gos;

	private CGameID m_GameID;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	private PeEntity m_Maiplayer;

	private PlayerPackageCmpt m_playerPkg;

	private SteamLeaderboard_t m_steamLeaderboard;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	protected Callback<UserStatsStored_t> m_UserStatsStored;

	protected Callback<UserAchievementStored_t> m_UserAchievementStored;

	protected CallResult<LeaderboardFindResult_t> m_LeaderboardFindResult;

	protected CallResult<LeaderboardScoreUploaded_t> m_LeaderboardScoreUploaded;

	protected CallResult<LeaderboardScoresDownloaded_t> m_LeaderboardScoresDownloaded;

	public static SteamAchievementsSystem Instance => instance;

	private void Awake()
	{
		instance = this;
		InitBaseData();
	}

	private void OnEnable()
	{
		if (SteamManager.Initialized)
		{
			m_GameID = new CGameID(SteamUtils.GetAppID());
			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
			m_bRequestedStats = false;
			m_bStatsValid = false;
		}
	}

	private void Update()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!m_bRequestedStats)
		{
			if (!SteamManager.Initialized)
			{
				m_bRequestedStats = true;
				return;
			}
			bool bRequestedStats = SteamUserStats.RequestCurrentStats();
			m_bRequestedStats = bRequestedStats;
		}
		if (!m_bStatsValid)
		{
			return;
		}
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null && (!m_Maiplayer || !m_Maiplayer.Equals(PeSingleton<PeCreature>.Instance.mainPlayer)))
		{
			m_Maiplayer = PeSingleton<PeCreature>.Instance.mainPlayer;
			m_playerPkg = m_Maiplayer.packageCmpt as PlayerPackageCmpt;
			if (m_playerPkg != null)
			{
				m_playerPkg.package._playerPak.changeEventor.Subscribe(OnItemChange);
			}
		}
		if ((bool)m_Maiplayer && Money.Digital && m_Maiplayer.packageCmpt != null && m_Maiplayer.packageCmpt.money != null && m_Maiplayer.packageCmpt.money.current >= 10000)
		{
			OnGameStateChange(Eachievement.Richer);
		}
		int num = 0;
		for (int i = 0; i < m_Achievements.Length; i++)
		{
			if (m_Achievements[i].m_bAchieved)
			{
				num++;
			}
			else if (m_Stats[(int)m_Achievements[i].m_eAchievementID].IsAccomplish())
			{
				UnlockAchievement(m_Achievements[i]);
			}
		}
		if (num == 15 && !m_Achievements[13].m_bAchieved)
		{
			UnlockAchievement(m_Achievements[13]);
		}
		if (!m_bStoreStats)
		{
			return;
		}
		bool flag = false;
		for (int j = 0; j < m_Achievements.Length; j++)
		{
			if (!string.IsNullOrEmpty(m_StatNames[(int)m_Achievements[j].m_eStatID]))
			{
				flag = StatsUserPrefs.SaveIntValue(m_StatNames[(int)m_Achievements[j].m_eStatID], m_Stats[(int)m_Achievements[j].m_eAchievementID].m_StatValue);
			}
		}
		SteamUserStats.StoreStats();
		m_bStoreStats = !flag;
	}

	private void InitBaseData()
	{
		LoadAchievement((Resources.Load("Achievement/AchievementData") as TextAsset).text);
	}

	private void LoadAchievement(string str)
	{
		XmlDocument xmlDocument = new XmlDocument();
		StringReader stringReader = new StringReader(str);
		if (xmlDocument == null || stringReader == null)
		{
			return;
		}
		xmlDocument.Load(stringReader);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("AchievementData");
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("Achievements");
		XmlNode xmlNode3 = xmlNode.SelectSingleNode("Stats");
		XmlNodeList elementsByTagName = ((XmlElement)xmlNode2).GetElementsByTagName("TriggerData");
		XmlNodeList elementsByTagName2 = ((XmlElement)xmlNode3).GetElementsByTagName("Data");
		int count = elementsByTagName.Count;
		int count2 = elementsByTagName2.Count;
		m_Achievements = new AchievementInfo[count];
		m_MissionAchs = new MissionAchievement[count];
		m_StatNames = new string[1 + count2];
		m_Stats = new Istat[count];
		foreach (XmlNode item in elementsByTagName)
		{
			XmlElement e = item as XmlElement;
			int attributeInt = XmlUtil.GetAttributeInt32(e, "achID");
			int attributeInt2 = XmlUtil.GetAttributeInt32(e, "staID");
			Eachievement achievementID = (Eachievement)attributeInt;
			Estat statID = (Estat)attributeInt2;
			string attributeString = XmlUtil.GetAttributeString(e, "APIname");
			string attributeString2 = XmlUtil.GetAttributeString(e, "AchieveDesc");
			int attributeInt3 = XmlUtil.GetAttributeInt32(e, "MissionID");
			MissionAchievement.Emisschedule attributeInt4 = (MissionAchievement.Emisschedule)XmlUtil.GetAttributeInt32(e, "AccomplishOrGet");
			int attributeInt5 = XmlUtil.GetAttributeInt32(e, "ItemProtoId");
			m_Achievements[attributeInt] = new AchievementInfo(achievementID, statID, attributeString, attributeString2, attributeInt5);
			m_StatNames[attributeInt2] = string.Empty;
			TriggerStat triggerStat = new TriggerStat();
			triggerStat.SetGoal(1);
			m_Stats[attributeInt] = triggerStat;
			m_MissionAchs[attributeInt] = new MissionAchievement(attributeInt3, attributeInt4);
		}
		foreach (XmlNode item2 in elementsByTagName2)
		{
			XmlElement e2 = item2 as XmlElement;
			int attributeInt6 = XmlUtil.GetAttributeInt32(e2, "ID");
			int attributeInt7 = XmlUtil.GetAttributeInt32(e2, "achID");
			string attributeString3 = XmlUtil.GetAttributeString(e2, "StatName");
			int attributeInt8 = XmlUtil.GetAttributeInt32(e2, "Goal");
			if (m_Stats.Length > attributeInt7 && m_Stats[attributeInt7] != null)
			{
				m_Stats[attributeInt7].SetGoal(attributeInt8);
			}
			m_StatNames[attributeInt6] = attributeString3;
		}
	}

	public void OnGameStateChange(Eachievement eNewState, bool isIncrease = true)
	{
		if (m_bStatsValid && m_Stats != null && (int)eNewState <= m_Stats.Length && (int)eNewState <= m_Achievements.Length && !m_Stats[(int)eNewState].IsAccomplish())
		{
			if (isIncrease)
			{
				m_Stats[(int)eNewState].IncreaseStat();
			}
			else
			{
				m_Stats[(int)eNewState].DropStat();
			}
			m_bStoreStats = true;
		}
	}

	public void OnMissionChange(int missionID, int _Emisschedule = 0)
	{
		for (int i = 0; i < m_MissionAchs.Length; i++)
		{
			if (m_MissionAchs[i].EquitMission(missionID, (MissionAchievement.Emisschedule)_Emisschedule))
			{
				OnGameStateChange((Eachievement)i);
				break;
			}
		}
	}

	private void OnItemChange(object sender, ItemPackage.EventArg arg)
	{
		for (int i = 0; i < m_Achievements.Length; i++)
		{
			if (!m_Achievements[i].m_bAchieved && m_Achievements[i].m_ProtoId != 0 && PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				PlayerPackageCmpt playerPackageCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.packageCmpt as PlayerPackageCmpt;
				if (playerPackageCmpt != null)
				{
					int count = playerPackageCmpt.package.GetCount(m_Achievements[i].m_ProtoId);
					OnItemValueChange(m_Achievements[i].m_eAchievementID, count);
				}
			}
		}
	}

	private void OnItemValueChange(Eachievement eNewState, int value)
	{
		if (m_bStatsValid && m_Stats != null && (int)eNewState <= m_Stats.Length && (int)eNewState <= m_Achievements.Length && !m_Stats[(int)eNewState].IsAccomplish())
		{
			m_Stats[(int)eNewState].m_StatValue = value;
			m_bStoreStats = true;
		}
	}

	public void ResetAllAchievements()
	{
		for (int i = 0; i < m_Achievements.Length; i++)
		{
			ResetAchievement(m_Achievements[i]);
		}
	}

	private void ResetAchievement(AchievementInfo info)
	{
		if (SteamUserStats.GetAchievement(info.m_apiName, out info.m_bAchieved) && info.m_bAchieved)
		{
			SteamUserStats.ClearAchievement(info.m_apiName);
			SteamUserStats.RequestCurrentStats();
		}
		else
		{
			Debug.LogWarning("SteamUserStats.GetAchievement failed Rest for Achievement " + info.m_apiName + "\nIs it registered in the Steam Partner site?");
		}
	}

	private void ResetAllStats()
	{
		for (int i = 0; i < m_Stats.Length; i++)
		{
			m_Stats[i].ResetValue();
		}
		m_bStoreStats = true;
	}

	private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
	{
		if (!(pCallback.m_hSteamLeaderboard == m_steamLeaderboard))
		{
			return;
		}
		if (pCallback.m_cEntryCount > 0)
		{
			Debug.LogError("排行榜数据量：" + pCallback.m_cEntryCount);
			for (int i = 0; i < pCallback.m_cEntryCount; i++)
			{
				int[] pDetails = new int[pCallback.m_cEntryCount];
				SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out var pLeaderboardEntry, pDetails, pCallback.m_cEntryCount);
				Debug.LogError(string.Concat("用户ID：", pLeaderboardEntry.m_steamIDUser, "用户分数", pLeaderboardEntry.m_nScore, "用户排名", pLeaderboardEntry.m_nGlobalRank, "Details", pLeaderboardEntry.m_cDetails, "  ", SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser)));
				gos[i].transform.FindChild("name").GetComponent<UILabel>().text = pLeaderboardEntry.m_nGlobalRank + "  " + SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser);
				gos[i].transform.FindChild("score").GetComponent<UILabel>().text = pLeaderboardEntry.m_nScore + string.Empty;
			}
		}
		else
		{
			Debug.LogError("排行榜数据为空！");
		}
	}

	private void OnLeaerboardScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_bSuccess == 1)
		{
			Debug.LogError("成功上传价值数据：" + pCallback.m_nScore + "榜内数据是否需要变更：" + pCallback.m_bScoreChanged + "新的排名：" + pCallback.m_nGlobalRankNew + "上次排名：" + pCallback.m_nGlobalRankPrevious);
			gos[gos.Count - 1].transform.FindChild("name").GetComponent<UILabel>().text = pCallback.m_nGlobalRankNew + "  " + SteamFriends.GetPersonaName();
			gos[gos.Count - 1].transform.FindChild("score").GetComponent<UILabel>().text = pCallback.m_nScore + string.Empty;
			if (pCallback.m_nGlobalRankPrevious == 0 || pCallback.m_bScoreChanged != 0)
			{
			}
			DownloadLeaderboardEntries();
		}
	}

	private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure)
	{
		if (SteamManager.Initialized)
		{
			if (pCallback.m_hSteamLeaderboard.m_SteamLeaderboard == 0L || pCallback.m_bLeaderboardFound == 0)
			{
				Debug.LogError("There is no Leaderboard found");
			}
			else
			{
				m_steamLeaderboard = pCallback.m_hSteamLeaderboard;
			}
		}
	}

	public void UploadScore(int score)
	{
		if (m_steamLeaderboard.m_SteamLeaderboard != 0L)
		{
			Debug.LogError("上传分数");
			SteamAPICall_t hAPICall = SteamUserStats.UploadLeaderboardScore(m_steamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, score, null, 0);
			m_LeaderboardScoreUploaded.Set(hAPICall);
		}
	}

	public void DownloadLeaderboardEntries()
	{
		if (m_steamLeaderboard.m_SteamLeaderboard != 0L)
		{
			Debug.LogError("请求排行榜数据");
			SteamAPICall_t hAPICall = SteamUserStats.DownloadLeaderboardEntries(m_steamLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, 8);
			m_LeaderboardScoresDownloaded.Set(hAPICall);
		}
	}

	private void UnlockAchievement(AchievementInfo achievement)
	{
		achievement.m_bAchieved = true;
		SteamUserStats.SetAchievement(achievement.m_apiName);
		m_bStoreStats = true;
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialized || (ulong)m_GameID != pCallback.m_nGameID)
		{
			return;
		}
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log("Received stats and achievements from Steam\n");
			m_bStatsValid = true;
			for (int i = 0; i < m_Achievements.Length; i++)
			{
				if (SteamUserStats.GetAchievement(m_Achievements[i].m_apiName, out m_Achievements[i].m_bAchieved))
				{
					m_Achievements[i].m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(m_Achievements[i].m_apiName, "desc");
					Debug.Log(m_Achievements[i].m_eAchievementID.ToString() + "  " + m_Achievements[i].m_bAchieved + "  " + m_Achievements[i].m_apiName + "  " + m_Achievements[i].m_strDescription);
				}
				else
				{
					Debug.Log(string.Concat("SteamUserStats.GetAchievement failed for Achievement ", m_Achievements[i].m_eAchievementID, "\nIs it registered in the Steam Partner site?"));
				}
				if (!string.IsNullOrEmpty(m_StatNames[(int)m_Achievements[i].m_eStatID]) && !StatsUserPrefs.GetIntValue(m_StatNames[(int)m_Achievements[i].m_eStatID], out m_Stats[(int)m_Achievements[i].m_eAchievementID].m_StatValue))
				{
					Debug.LogWarning("Do not has " + m_StatNames[(int)m_Achievements[i].m_eStatID]);
				}
			}
		}
		else
		{
			Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("StoreStats - success");
			}
			else if (pCallback.m_eResult == EResult.k_EResultInvalidParam)
			{
				Debug.Log("StoreStats - some failed to validate");
				UserStatsReceived_t pCallback2 = default(UserStatsReceived_t);
				pCallback2.m_eResult = EResult.k_EResultOK;
				pCallback2.m_nGameID = (ulong)m_GameID;
				OnUserStatsReceived(pCallback2);
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}
}
