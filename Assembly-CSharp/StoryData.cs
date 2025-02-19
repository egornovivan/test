using System.Collections.Generic;
using UnityEngine;

public class StoryData
{
	public int m_ID;

	public List<int> m_triggerPlot;

	public List<int> m_TalkList;

	public List<int> m_ServantTalkList;

	public List<MissionIDNum> m_CreateMonster;

	public List<int> m_DeleteMonster;

	public List<NpcStyle> m_TransNpc;

	public List<MoveNpcData> m_MoveNpc;

	public NpcRail m_NpcRail;

	public List<NpcFace> m_NpcFace;

	public List<NpcAct> m_NpcAct;

	public List<MonAct> m_MonAct;

	public List<ReputationInfo> m_NpcReq;

	public List<NpcAct> m_NpcAnimator;

	public List<string> m_PlayerAni;

	public List<MotionStyle> m_MotionStyle;

	public List<SenceFace> m_SenceFace;

	public List<SenceAct> m_SenceAct;

	public List<NpcCamp> m_NpcCamp;

	public string m_Special;

	public List<NpcOpen> m_NpcAI;

	public List<NpcOpen> m_NpcInvincible;

	public List<NpcOpen> m_FollowPlayerList;

	public float m_FollowDist;

	public List<MonsterSetInfo> m_MonsterCampList;

	public List<MonsterSetInfo> m_MonsterHarmList;

	public int m_MoveType;

	public int m_PausePlayer;

	public bool m_PauseNPC;

	public bool m_PauseMons;

	public float m_Delay;

	public List<Vector3> m_EffectPosList;

	public int m_EffectID;

	public List<PosData> m_SoundPosList;

	public int m_SoundID;

	public List<CameraInfo> m_CameraList;

	public List<int> m_iColonyNoOrderNpcList;

	public List<int> m_iColonyOrderNpcList;

	public List<int> m_killNpcList;

	public List<int> m_monsterHatredList;

	public List<int> m_npcHatredList;

	public List<int> m_harmList;

	public List<int> m_doodadHarmList;

	public List<SetDoodadEffect> m_doodadEffectList;

	public List<ENpcBattleInfo> m_npcsBattle;

	public List<string> m_plotMissionTrigger;

	public List<int> m_cantReviveNpc;

	public List<AbnormalInfo> m_abnormalInfo;

	public ReputationInfo[] m_reputationChange;

	public ReputationInfo m_nativeAttitude;

	public List<int> oldDoodad;

	public List<int> newDoodad;

	public List<KillMons> m_killMons;

	public List<int> m_stopKillMonsID;

	public List<ChangePartrolmode> m_monPatrolMode;

	public List<NpcType> m_npcType;

	public List<MoveMons> m_moveMons;

	public List<CheckMons> m_checkMons;

	public int m_increaseLangSkill;

	public int m_moveNpc_missionOrPlot_id;

	public int m_moveMons_missionOrPlot_id;

	public List<int> m_attractMons;

	public RandScenariosData m_randScenarios;

	public List<CampState> m_campAlert;

	public List<CampState> m_campActive;

	public List<int> m_comMission;

	public List<ENpcBattleInfo> m_whackedList;

	public List<int> m_getMission;

	public int m_pauseSiege;

	public int m_showTip;

	public StoryData()
	{
		m_Special = string.Empty;
		m_triggerPlot = new List<int>();
		m_TalkList = new List<int>();
		m_ServantTalkList = new List<int>();
		m_CreateMonster = new List<MissionIDNum>();
		m_DeleteMonster = new List<int>();
		m_TransNpc = new List<NpcStyle>();
		m_MoveNpc = new List<MoveNpcData>();
		m_NpcFace = new List<NpcFace>();
		m_NpcAct = new List<NpcAct>();
		m_MonAct = new List<MonAct>();
		m_NpcReq = new List<ReputationInfo>();
		m_SenceFace = new List<SenceFace>();
		m_SenceAct = new List<SenceAct>();
		m_NpcCamp = new List<NpcCamp>();
		m_NpcAI = new List<NpcOpen>();
		m_NpcInvincible = new List<NpcOpen>();
		m_FollowPlayerList = new List<NpcOpen>();
		m_MonsterCampList = new List<MonsterSetInfo>();
		m_MonsterHarmList = new List<MonsterSetInfo>();
		m_EffectPosList = new List<Vector3>();
		m_SoundPosList = new List<PosData>();
		m_CameraList = new List<CameraInfo>();
		m_NpcRail = new NpcRail();
		m_iColonyNoOrderNpcList = new List<int>();
		m_iColonyOrderNpcList = new List<int>();
		m_killNpcList = new List<int>();
		m_monsterHatredList = new List<int>();
		m_npcHatredList = new List<int>();
		m_harmList = new List<int>();
		m_doodadHarmList = new List<int>();
		m_plotMissionTrigger = new List<string>();
		m_cantReviveNpc = new List<int>();
		m_npcsBattle = new List<ENpcBattleInfo>();
		m_abnormalInfo = new List<AbnormalInfo>();
		m_reputationChange = new ReputationInfo[2];
		oldDoodad = new List<int>();
		newDoodad = new List<int>();
		m_doodadEffectList = new List<SetDoodadEffect>();
		m_killMons = new List<KillMons>();
		m_stopKillMonsID = new List<int>();
		m_monPatrolMode = new List<ChangePartrolmode>();
		m_npcType = new List<NpcType>();
		m_checkMons = new List<CheckMons>();
		m_moveMons = new List<MoveMons>();
		m_NpcAnimator = new List<NpcAct>();
		m_PlayerAni = new List<string>();
		m_MotionStyle = new List<MotionStyle>();
		m_attractMons = new List<int>();
		m_campAlert = new List<CampState>();
		m_campActive = new List<CampState>();
		m_comMission = new List<int>();
		m_whackedList = new List<ENpcBattleInfo>();
		m_getMission = new List<int>();
	}
}
