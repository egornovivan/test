using System.Collections.Generic;

public class MissionCommonData
{
	public int m_ID;

	public string m_MissionName;

	public int m_iNpc;

	public int m_iReplyNpc;

	public MissionType m_Type;

	public string m_ScriptID;

	public int m_MaxNum;

	public string m_Description;

	public string m_MulDesc;

	public int m_VarValueID;

	public int m_VarValue;

	public List<int> m_TargetIDList;

	public PreLimit m_PreLimit;

	public PreLimit m_AfterLimit;

	public PreLimit m_MutexLimit;

	public List<ReputationPreLimit> m_reputationPre;

	public List<int> m_GuanLianList;

	public int[] m_PlayerTalk;

	public List<MissionIDNum> m_Get_DemandItem;

	public List<MissionIDNum> m_Get_DeleteItem;

	public List<MissionIDNum> m_Get_MissionItem;

	public List<MissionIDNum> m_Com_RewardItem;

	public Dictionary<int, List<MissionIDNum>> m_Com_MulRewardItem;

	public List<MissionIDNum> m_Com_SelRewardItem;

	public List<MissionIDNum> m_Com_RemoveItem;

	public List<int> m_TalkOP;

	public List<int> m_OPID;

	public List<int> m_TalkIN;

	public List<int> m_INID;

	public List<int> m_TalkED;

	public List<int> m_EDID;

	public bool m_bGiveUp;

	public List<int> m_ResetID;

	public List<int> m_DeleteID;

	public List<int> m_PromptOP;

	public List<int> m_PromptIN;

	public List<int> m_PromptED;

	public List<StoryInfo> m_StoryInfo;

	public int m_NeedTime;

	public int m_timeOverToPlot;

	public List<int> m_iColonyNpcList;

	public int[] m_ColonyMis;

	public bool isAutoReply;

	public int addSpValue;

	public List<NpcType> m_npcType;

	public List<int> m_tempLimit;

	public int m_replyIconId;

	public bool m_increaseChain;

	public int[] m_changeReputation;

	public CreDungeon creDungeon;

	public List<NpcType> m_failNpcType;

	public MissionCommonData()
	{
		m_MissionName = string.Empty;
		m_ScriptID = string.Empty;
		m_Description = string.Empty;
		m_PreLimit = new PreLimit();
		m_AfterLimit = new PreLimit();
		m_MutexLimit = new PreLimit();
		m_TargetIDList = new List<int>();
		m_GuanLianList = new List<int>();
		m_PlayerTalk = new int[2];
		m_Get_DemandItem = new List<MissionIDNum>();
		m_Get_DeleteItem = new List<MissionIDNum>();
		m_Get_MissionItem = new List<MissionIDNum>();
		m_Com_RewardItem = new List<MissionIDNum>();
		m_Com_MulRewardItem = new Dictionary<int, List<MissionIDNum>>();
		m_Com_SelRewardItem = new List<MissionIDNum>();
		m_Com_RemoveItem = new List<MissionIDNum>();
		m_TalkOP = new List<int>();
		m_OPID = new List<int>();
		m_TalkIN = new List<int>();
		m_INID = new List<int>();
		m_TalkED = new List<int>();
		m_EDID = new List<int>();
		m_ResetID = new List<int>();
		m_DeleteID = new List<int>();
		m_PromptOP = new List<int>();
		m_PromptIN = new List<int>();
		m_PromptED = new List<int>();
		m_StoryInfo = new List<StoryInfo>();
		m_iColonyNpcList = new List<int>();
		m_ColonyMis = new int[2];
		m_npcType = new List<NpcType>();
		m_tempLimit = new List<int>();
		m_changeReputation = new int[3];
		m_reputationPre = new List<ReputationPreLimit>();
		creDungeon = default(CreDungeon);
		m_failNpcType = new List<NpcType>();
	}

	public bool IsTalkMission()
	{
		if (m_Type == MissionType.MissionType_Time)
		{
			return false;
		}
		if (m_Type == MissionType.MissionType_Talk)
		{
			return true;
		}
		if (m_TargetIDList.Count == 0)
		{
			return true;
		}
		return false;
	}

	public bool IsTimeMission()
	{
		if (m_Type == MissionType.MissionType_Time)
		{
			return true;
		}
		return false;
	}

	public List<int> HasStory(Story_Info type)
	{
		List<int> list = new List<int>();
		if (m_StoryInfo.Count == 0)
		{
			list.Add(-1);
		}
		for (int i = 0; i < m_StoryInfo.Count; i++)
		{
			StoryInfo storyInfo = m_StoryInfo[i];
			if (storyInfo.type == type)
			{
				list.Add(storyInfo.storyid);
			}
		}
		return list;
	}
}
