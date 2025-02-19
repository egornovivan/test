using System.Collections.Generic;

public class MissionInit
{
	public int m_ID;

	public List<int> m_ComMisID;

	public int m_NComMisID;

	public List<NpcAct> m_NpcAct;

	public List<NpcFace> m_NpcFace;

	public List<NpcCamp> m_NpcCamp;

	public List<int> m_DeleteMonster;

	public List<int> m_iDeleteNpc;

	public List<NpcStyle> m_TransNpc;

	public List<NpcOpen> m_FollowPlayerList;

	public List<MonsterSetInfo> m_MonsterCampList;

	public List<MonsterSetInfo> m_MonsterHarmList;

	public string m_Special;

	public List<int> m_iColonyNoOrderNpcList;

	public List<int> m_monsterHatredList;

	public List<int> m_npcHatredList;

	public List<int> m_harmList;

	public List<int> m_doodadHarmList;

	public List<ENpcBattleInfo> m_npcsBattle;

	public List<string> m_plotMissionTrigger;

	public List<int> cantReviveNpc;

	public List<KillMons> m_killMons;

	public List<SetDoodadEffect> m_doodadEffectList;

	public List<NpcType> m_npcType;

	public List<int> m_triggerPlot;

	public List<int> m_killNpcList;

	public MissionInit()
	{
		m_ComMisID = new List<int>();
		m_NpcAct = new List<NpcAct>();
		m_NpcFace = new List<NpcFace>();
		m_NpcCamp = new List<NpcCamp>();
		m_DeleteMonster = new List<int>();
		m_iDeleteNpc = new List<int>();
		m_TransNpc = new List<NpcStyle>();
		m_FollowPlayerList = new List<NpcOpen>();
		m_MonsterCampList = new List<MonsterSetInfo>();
		m_MonsterHarmList = new List<MonsterSetInfo>();
		m_iColonyNoOrderNpcList = new List<int>();
		m_plotMissionTrigger = new List<string>();
		m_monsterHatredList = new List<int>();
		m_npcHatredList = new List<int>();
		m_harmList = new List<int>();
		m_doodadHarmList = new List<int>();
		cantReviveNpc = new List<int>();
		m_npcsBattle = new List<ENpcBattleInfo>();
		m_killMons = new List<KillMons>();
		m_doodadEffectList = new List<SetDoodadEffect>();
		m_npcType = new List<NpcType>();
		m_triggerPlot = new List<int>();
		m_killNpcList = new List<int>();
	}
}
