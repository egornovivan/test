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

	public ReputationPreLimit m_PujaReputationPre;

	public ReputationPreLimit m_PajaReputationPre;

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

	public bool m_increaseChain;

	public List<NpcType> m_npcType;

	public List<int> m_tempLimit;

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
	}

	public MissionCommonData(MissionCommonData template)
	{
		m_ID = template.m_ID;
		m_MissionName = template.m_MissionName;
		m_iNpc = template.m_iNpc;
		m_iReplyNpc = template.m_iReplyNpc;
		m_Type = template.m_Type;
		m_ScriptID = template.m_ScriptID;
		m_MaxNum = template.m_MaxNum;
		m_Description = template.m_Description;
		m_MulDesc = template.m_MulDesc;
		m_VarValueID = template.m_VarValueID;
		m_VarValue = template.m_VarValue;
		m_TargetIDList = new List<int>();
		m_PreLimit = template.m_PreLimit;
		m_AfterLimit = template.m_AfterLimit;
		m_MutexLimit = template.m_MutexLimit;
		m_PujaReputationPre = template.m_PujaReputationPre;
		m_PajaReputationPre = template.m_PajaReputationPre;
		m_GuanLianList = template.m_GuanLianList;
		m_PlayerTalk = template.m_PlayerTalk;
		m_Get_DemandItem = template.m_Get_DemandItem;
		m_Get_DeleteItem = template.m_Get_DeleteItem;
		m_Get_MissionItem = template.m_Get_MissionItem;
		m_Com_RewardItem = new List<MissionIDNum>();
		m_Com_MulRewardItem = template.m_Com_MulRewardItem;
		m_Com_SelRewardItem = template.m_Com_SelRewardItem;
		m_Com_RemoveItem = new List<MissionIDNum>();
		m_TalkOP = new List<int>();
		m_OPID = template.m_OPID;
		m_TalkIN = new List<int>();
		m_INID = template.m_INID;
		m_TalkED = new List<int>();
		m_EDID = template.m_EDID;
		m_bGiveUp = template.m_bGiveUp;
		m_ResetID = new List<int>();
		m_ResetID.AddRange(template.m_ResetID);
		m_DeleteID = new List<int>();
		m_DeleteID.AddRange(template.m_DeleteID);
		m_PromptOP = new List<int>();
		m_PromptIN = new List<int>();
		m_PromptED = new List<int>();
		m_StoryInfo = template.m_StoryInfo;
		m_NeedTime = template.m_NeedTime;
		m_timeOverToPlot = template.m_timeOverToPlot;
		m_iColonyNpcList = template.m_iColonyNpcList;
		m_ColonyMis = template.m_ColonyMis;
		isAutoReply = template.isAutoReply;
		addSpValue = template.addSpValue;
		m_npcType = template.m_npcType;
		m_tempLimit = template.m_tempLimit;
		m_increaseChain = template.m_increaseChain;
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
