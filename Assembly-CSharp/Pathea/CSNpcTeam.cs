using System.Collections.Generic;
using ItemAsset;
using Pathea.PeEntityExt;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class CSNpcTeam : TeamAgent
{
	public static int EAT_Mumber_Num = 4;

	public static int Sleep_Mumber_Num = 5;

	public static float Sleep_Batch_Time = 10f;

	public static float Awake_Batch_Time = 10f;

	public static float Sleep_ALl_Time = 9f;

	public static int Chat_member_Num = 5;

	public static float Chat_Radiu = 1f;

	public static float Atk_JionR = 16f;

	public static float Atk_RunR = 32f;

	private static int mCsNpcNumber;

	private static NpcCheckTime mCheckTime;

	private static CSSleepSlots mCsSleepSlots;

	private CSCreator mCScreator;

	public bool mIndanger;

	private bool mInit;

	private TeamLine[] m_TeamLines;

	private List<PeEntity>[] m_beInLine;

	private List<PeEntity>[] m_CSQuadrantPos;

	private List<PeEntity> m_InLines;

	private List<PeEntity> m_IdleLists;

	private List<PeEntity> m_TeamEnemies;

	private Dictionary<PeEntity, List<PeEntity>> m_assemblyEnemies;

	private List<stEnemies> m_mapEnemies;

	private List<PeEntity>[] m_DicteamEnemies;

	private List<PeEntity>[] m_DicEquipMembers;

	private List<PeEntity>[] m_DicJobMembers;

	private List<PeEntity> m_EatQueues;

	private Dictionary<int, CSStorage> m_mapCStorages;

	private List<PeEntity> m_tempList;

	private double starttime;

	private double getUpstartTime;

	public static int CsNpcNumber => mCsNpcNumber;

	public static NpcCheckTime checkTime => mCheckTime;

	public static CSSleepSlots CsSleepSlots => mCsSleepSlots;

	public CSNpcTeam()
	{
		mInit = false;
		m_TeamLines = new TeamLine[6];
		m_beInLine = new List<PeEntity>[6];
		m_CSQuadrantPos = new List<PeEntity>[5];
		m_CSQuadrantPos[1] = new List<PeEntity>();
		m_CSQuadrantPos[2] = new List<PeEntity>();
		m_CSQuadrantPos[3] = new List<PeEntity>();
		m_CSQuadrantPos[4] = new List<PeEntity>();
		m_IdleLists = new List<PeEntity>();
		m_InLines = new List<PeEntity>();
		m_TeamEnemies = new List<PeEntity>();
		m_tempList = new List<PeEntity>();
		m_DicteamEnemies = new List<PeEntity>[2];
		m_DicteamEnemies[0] = new List<PeEntity>();
		m_DicteamEnemies[1] = new List<PeEntity>();
		m_DicEquipMembers = new List<PeEntity>[2];
		m_DicEquipMembers[0] = new List<PeEntity>();
		m_DicEquipMembers[1] = new List<PeEntity>();
		m_DicJobMembers = new List<PeEntity>[9];
		m_EatQueues = new List<PeEntity>();
		m_mapCStorages = new Dictionary<int, CSStorage>();
		m_assemblyEnemies = new Dictionary<PeEntity, List<PeEntity>>();
		m_mapEnemies = new List<stEnemies>();
	}

	public void setCSCreator(CSCreator creator)
	{
		if (!(creator == null) && creator.Assembly != null && (mCScreator == null || !mCScreator.Equals(creator)))
		{
			mCScreator = creator;
			mInit = false;
		}
	}

	private stEnemies GetByTarget(PeEntity target)
	{
		for (int i = 0; i < m_mapEnemies.Count; i++)
		{
			if (m_mapEnemies[i].BeTarget(target))
			{
				return m_mapEnemies[i];
			}
		}
		return null;
	}

	private stEnemies GetByChase(PeEntity chase)
	{
		for (int i = 0; i < m_mapEnemies.Count; i++)
		{
			if (m_mapEnemies[i].InChaseLists(chase))
			{
				return m_mapEnemies[i];
			}
		}
		return null;
	}

	private bool mapEnemiesContainTarget(PeEntity target)
	{
		for (int i = 0; i < m_mapEnemies.Count; i++)
		{
			if (m_mapEnemies[i].BeTarget(target))
			{
				return true;
			}
		}
		return false;
	}

	private bool mapEnemiesContainChase(PeEntity chase)
	{
		for (int i = 0; i < m_mapEnemies.Count; i++)
		{
			if (m_mapEnemies[i].InChaseLists(chase))
			{
				return true;
			}
		}
		return false;
	}

	private bool mapEnemiesRemoveTaregt(PeEntity target)
	{
		stEnemies byTarget = GetByTarget(target);
		if (byTarget == null)
		{
			return false;
		}
		byTarget.ClearChaseLists();
		return m_mapEnemies.Remove(byTarget);
	}

	private void mapEnemiesAddTarget(PeEntity target)
	{
		if (!mapEnemiesContainTarget(target))
		{
			m_mapEnemies.Add(new stEnemies(target));
		}
	}

	private void mapEnemiesAddChase(PeEntity chase, params object[] objs)
	{
		if (objs == null)
		{
			return;
		}
		PeEntity peEntity = (PeEntity)objs[0];
		if (!(peEntity == null))
		{
			stEnemies byTarget = GetByTarget(peEntity);
			if (byTarget != null && !byTarget.InChaseLists(chase))
			{
				byTarget.AddInChaseLists(chase);
			}
		}
	}

	private void mapEnemiesRemoveChase(PeEntity chase)
	{
		GetByChase(chase)?.RemoveChaseLists(chase);
	}

	private void AddToAssemblyEnemies(PeEntity enemy)
	{
		int num = 0;
		num = ((enemy.monsterProtoDb == null || enemy.monsterProtoDb.AtkDb == null) ? 4 : enemy.monsterProtoDb.AtkDb.mNumber);
		if (!m_assemblyEnemies.ContainsKey(enemy))
		{
			m_assemblyEnemies.Add(enemy, new List<PeEntity>(num));
		}
	}

	private bool assemblyEnemiesContains(PeEntity member)
	{
		foreach (PeEntity key in m_assemblyEnemies.Keys)
		{
			if (m_assemblyEnemies[key].Contains(member))
			{
				return true;
			}
		}
		return false;
	}

	private void AddToAssemblyProtectMember(PeEntity target, PeEntity member)
	{
		foreach (PeEntity key in m_assemblyEnemies.Keys)
		{
			if (m_assemblyEnemies[key].Contains(member))
			{
				m_assemblyEnemies[key].Remove(member);
			}
		}
		m_assemblyEnemies[target].Add(member);
	}

	private bool RemoveFromAssemblyEnemies(PeEntity enemy)
	{
		return m_assemblyEnemies.Remove(enemy);
	}

	private bool BeinLineContainKey(ELineType type)
	{
		return m_beInLine[(int)type] != null;
	}

	private void AddInBeinLine(ELineType type, List<PeEntity> lists)
	{
		m_beInLine[(int)type] = lists;
	}

	private void removeBeinLine(ELineType type)
	{
		m_beInLine[(int)type].Clear();
		m_beInLine[(int)type] = null;
	}

	private bool teamLinesContain(ELineType type)
	{
		return m_TeamLines[(int)type] != null;
	}

	private void addInTeamLines(ELineType type, TeamLine line)
	{
		m_TeamLines[(int)type] = line;
	}

	private void removeTeamLines(ELineType type)
	{
		m_TeamLines[(int)type] = null;
	}

	private void clearTeamLine(ELineType type)
	{
		m_TeamLines[(int)type].ClearMembers();
	}

	private void AddToTeamEnemies(PeEntity enemy)
	{
		if (!m_TeamEnemies.Contains(enemy))
		{
			SwichAddDicteamEnemies(enemy);
			m_TeamEnemies.Add(enemy);
		}
	}

	private void RemoveFormTeamEnemies(PeEntity enemy)
	{
		if (m_TeamEnemies.Contains(enemy))
		{
			RemoveFromDicteamEnemies(enemy);
			m_TeamEnemies.Remove(enemy);
		}
	}

	private bool TeamEnemiesContain(PeEntity enemy)
	{
		return m_TeamEnemies.Contains(enemy);
	}

	private bool DicteamEnemiesContain(PeEntity enemy)
	{
		if (enemy == null || enemy.monsterProtoDb == null)
		{
			return false;
		}
		if (enemy.monsterProtoDb.AtkDb.mNeeedEqup)
		{
			if (m_DicteamEnemies[1] == null)
			{
				return false;
			}
			return m_DicteamEnemies[1].Contains(enemy);
		}
		return false;
	}

	private void SwichAddDicteamEnemies(PeEntity enemy)
	{
		if (!(enemy == null) && enemy.monsterProtoDb != null)
		{
			if (enemy.monsterProtoDb.AtkDb.mNeeedEqup)
			{
				AddToDicteamEnemies(AttackType.Ranged, enemy);
			}
			else
			{
				AddToDicteamEnemies(AttackType.Melee, enemy);
			}
		}
	}

	private void AddToDicteamEnemies(AttackType Eatk, PeEntity enemy)
	{
		if (m_DicteamEnemies[(int)Eatk] == null)
		{
			m_DicteamEnemies[(int)Eatk] = new List<PeEntity>();
		}
		m_DicteamEnemies[(int)Eatk].Add(enemy);
	}

	private void RemoveFromDicteamEnemies(PeEntity enemy)
	{
		for (int i = 0; i < m_DicteamEnemies.Length; i++)
		{
			if (m_DicteamEnemies[i] != null && m_DicteamEnemies[i].Contains(enemy))
			{
				m_DicteamEnemies[i].Remove(enemy);
			}
		}
	}

	private void ClearDicJobMembers()
	{
		for (int i = 0; i < m_DicJobMembers.Length; i++)
		{
			if (m_DicJobMembers[i] != null)
			{
				m_DicJobMembers[i].Clear();
			}
		}
	}

	private void AddToDicJobMembers(PeEntity member)
	{
		if (!(member.NpcCmpt == null) && member.NpcCmpt.Job != 0)
		{
			if (m_DicJobMembers[(int)member.NpcCmpt.Job] == null)
			{
				m_DicJobMembers[(int)member.NpcCmpt.Job] = new List<PeEntity>();
			}
			m_DicJobMembers[(int)member.NpcCmpt.Job].Add(member);
		}
	}

	private void ClearCSQuadrantPos()
	{
		for (int i = 1; i < m_CSQuadrantPos.Length; i++)
		{
			if (m_CSQuadrantPos[i] != null)
			{
				m_CSQuadrantPos[i].Clear();
			}
		}
	}

	private void AddToQuadrantPosList(PeEntity member, Vector3 _centerPos)
	{
		EQuadrant quadrant = Quadrant.GetQuadrant(member.position - _centerPos);
		m_CSQuadrantPos[(int)quadrant].Add(member);
	}

	private EQuadrant GetQuadrant(PeEntity entity)
	{
		for (int i = 1; i < m_CSQuadrantPos.Length; i++)
		{
			if (m_CSQuadrantPos[i] != null && m_CSQuadrantPos[i].Contains(entity))
			{
				return (EQuadrant)i;
			}
		}
		return EQuadrant.None;
	}

	private void ClearEatQueue()
	{
		m_EatQueues.Clear();
		m_mapCStorages.Clear();
	}

	private void AddToEatQueue(PeEntity npc)
	{
		if (!m_EatQueues.Contains(npc) && checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay) && NpcEatDb.IsNeedEatsth(npc))
		{
			m_EatQueues.Add(npc);
		}
	}

	private void RemoveFormEatQueue(PeEntity npc)
	{
		if (m_EatQueues.Contains(npc))
		{
			RemoveFromLine(ELineType.TeamEat, npc);
			m_mapCStorages.Remove(npc.Id);
			m_EatQueues.Remove(npc);
		}
	}

	private bool IsContinueEat(PeEntity npc)
	{
		if (!checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay))
		{
			return false;
		}
		if (!NpcThinkDb.CanDo(npc, EThinkingType.Dining))
		{
			return false;
		}
		return NpcEatDb.IsNeedEatsth(npc);
	}

	private void ClearDicEquip()
	{
		for (int i = 0; i < m_DicEquipMembers.Length; i++)
		{
			if (m_DicEquipMembers[i] != null)
			{
				m_DicEquipMembers[i].Clear();
			}
		}
	}

	private void AddToDicEquip(PeEntity member)
	{
		if (SelectItem.HasCanAttackEquip(member, AttackType.Ranged))
		{
			member.NpcCmpt.SetAtkType(AttackType.Ranged);
			m_DicEquipMembers[1].Add(member);
		}
		else
		{
			member.NpcCmpt.SetAtkType(AttackType.Melee);
			m_DicEquipMembers[0].Add(member);
		}
	}

	private void ReflashDicLines()
	{
		for (int i = 0; i < m_TeamLines.Length; i++)
		{
			if (m_TeamLines[i] != null)
			{
				m_TeamLines[i].Go();
			}
		}
	}

	private void ReflashAtkTarget()
	{
		if (mCScreator != null && mCScreator.Assembly != null && m_TeamEnemies.Count > 0)
		{
			m_tempList.Clear();
			for (int i = 0; i < m_TeamEnemies.Count; i++)
			{
				float num = PEUtil.MagnitudeH(mCScreator.Assembly.Position, m_TeamEnemies[i].position);
				if (num >= mCScreator.Assembly.Radius * 1.5f)
				{
					m_tempList.Add(m_TeamEnemies[i]);
				}
			}
			for (int j = 0; j < m_tempList.Count; j++)
			{
				m_TeamEnemies.Remove(m_tempList[j]);
				OnTargetLost(m_tempList[j]);
			}
		}
		if (m_TeamEnemies.Count <= 0)
		{
			if (teamLinesContain(ELineType.TeamAtk))
			{
				clearTeamLine(ELineType.TeamAtk);
				removeTeamLines(ELineType.TeamAtk);
			}
			if (BeinLineContainKey(ELineType.TeamAtk))
			{
				removeBeinLine(ELineType.TeamAtk);
			}
			mIndanger = false;
		}
	}

	private void ReflashLists()
	{
		if (mCScreator != null && mCScreator.Assembly != null)
		{
			ClearCSQuadrantPos();
			ClearDicEquip();
			for (int i = 0; i < teamMembers.Count; i++)
			{
				AddToQuadrantPosList(teamMembers[i], mCScreator.Assembly.Position);
				AddToDicEquip(teamMembers[i]);
			}
			ClearDicJobMembers();
			for (int j = 0; j < m_IdleLists.Count; j++)
			{
				AddToDicJobMembers(m_IdleLists[j]);
				AddToEatQueue(m_IdleLists[j]);
			}
		}
	}

	private void ReflashEatState()
	{
		m_tempList.Clear();
		for (int i = 0; i < m_EatQueues.Count; i++)
		{
			if (!IsContinueEat(m_EatQueues[i]))
			{
				m_tempList.Add(m_EatQueues[i]);
			}
		}
		for (int j = 0; j < m_tempList.Count; j++)
		{
			RemoveFormEatQueue(m_tempList[j]);
		}
	}

	private bool ComPearPriority(TeamLine _newline, TeamLine _oldline)
	{
		return _newline.Priority < _oldline.Priority;
	}

	private void AddinLine(ELineType type, PeEntity member, params object[] objs)
	{
		if (!BeinLineContainKey(type))
		{
			AddInBeinLine(type, new List<PeEntity>());
		}
		ELineType lineType = member.NpcCmpt.lineType;
		if ((!teamLinesContain(lineType) || !m_TeamLines[(int)lineType].ContainEntity(member) || ComPearPriority(m_TeamLines[(int)type], m_TeamLines[(int)lineType])) && m_TeamLines[(int)type].AddIn(member, objs))
		{
			if (type == ELineType.TeamAtk)
			{
				mapEnemiesAddChase(member, objs);
			}
			if (!m_beInLine[(int)type].Contains(member))
			{
				m_beInLine[(int)type].Add(member);
			}
			if (m_IdleLists.Contains(member))
			{
				m_IdleLists.Remove(member);
			}
			if (!m_InLines.Contains(member))
			{
				m_InLines.Add(member);
			}
		}
	}

	private void RemoveFromLine(ELineType type, PeEntity member)
	{
		if (type == ELineType.TeamAtk)
		{
			mapEnemiesRemoveChase(member);
		}
		if (!m_IdleLists.Contains(member))
		{
			m_IdleLists.Add(member);
		}
		if (m_InLines.Contains(member))
		{
			m_InLines.Remove(member);
		}
		if (BeinLineContainKey(type) && m_beInLine[(int)type].Contains(member))
		{
			m_beInLine[(int)type].Remove(member);
		}
		if (teamLinesContain(type) && m_TeamLines[(int)type].ContainEntity(member))
		{
			m_TeamLines[(int)type].RemoveOut(member);
		}
		if (teamLinesContain(type) && m_TeamLines[(int)type].Linemembers.Count == 0)
		{
			removeTeamLines(type);
		}
	}

	private void RemoveFromLines(List<PeEntity> lists)
	{
		for (int i = 0; i < lists.Count; i++)
		{
			m_InLines.Remove(lists[i]);
		}
	}

	private bool CanAddInLine(ELineType type, PeEntity member, params object[] objs)
	{
		return !member.IsDeath() && BeinLineContainKey(type) && !m_beInLine[(int)type].Contains(member) && needAddInMember(type) && suitLine(type, member, objs);
	}

	private bool needAddInMember(ELineType type)
	{
		return teamLinesContain(type) && m_TeamLines[(int)type].needAddMemeber();
	}

	private bool suitLine(ELineType type, PeEntity member, params object[] objs)
	{
		return m_TeamLines[(int)type].Suitline(member, objs);
	}

	private bool ContainInLine(PeEntity member, ELineType type)
	{
		return teamLinesContain(type) && m_TeamLines[(int)type].ContainEntity(member);
	}

	private AttackLine NewAtkLine()
	{
		ELineType type = ELineType.TeamAtk;
		AttackLine attackLine = new AttackLine();
		addInTeamLines(type, attackLine);
		if (!BeinLineContainKey(type))
		{
			AddInBeinLine(type, new List<PeEntity>());
		}
		return attackLine;
	}

	private void NewChatLine()
	{
		ELineType type = ELineType.TeamChat;
		chatLine line = new chatLine(4);
		addInTeamLines(type, line);
		CreatChatCentor(m_DicJobMembers[1]);
		if (!BeinLineContainKey(type))
		{
			AddInBeinLine(type, new List<PeEntity>());
		}
	}

	private void NewSleepLine()
	{
		ELineType type = ELineType.TeamSleep;
		SleepLine line = new SleepLine();
		addInTeamLines(type, line);
		if (!BeinLineContainKey(type))
		{
			AddInBeinLine(type, new List<PeEntity>());
		}
	}

	private void NewEatLine()
	{
		ELineType type = ELineType.TeamEat;
		EatLine line = new EatLine();
		addInTeamLines(type, line);
		if (!BeinLineContainKey(type))
		{
			AddInBeinLine(type, new List<PeEntity>());
		}
		MsgLine(ELineType.TeamEat, ELineMsg.Add_Eat, EAT_Mumber_Num);
	}

	private bool Find(ELineType type, List<PeEntity> findlists, params object[] objs)
	{
		for (int i = 0; i < findlists.Count; i++)
		{
			if (CanAddInLine(type, findlists[i], objs))
			{
				AddinLine(type, findlists[i], objs);
			}
			if (!needAddInMember(type))
			{
				return true;
			}
		}
		return false;
	}

	private void DistributeByQuadrant(PeEntity target, int _needNum)
	{
		ELineType type = ELineType.TeamAtk;
		EQuadrant quadrant = Quadrant.GetQuadrant(target.position - mCScreator.Assembly.Position);
		EQuadrant eQuadrant = Quadrant.Add(quadrant);
		EQuadrant eQuadrant2 = Quadrant.Minus(quadrant);
		EQuadrant eQuadrant3 = Quadrant.Add(eQuadrant);
		if (target.target != null)
		{
			Enemy attackEnemy = target.target.GetAttackEnemy();
			if (attackEnemy != null && attackEnemy.entityTarget != null && teamMembers != null && teamMembers.Contains(attackEnemy.entityTarget) && CanAddInLine(type, attackEnemy.entityTarget, target))
			{
				AddinLine(type, attackEnemy.entityTarget, target);
			}
		}
		List<PeEntity> findlists = m_CSQuadrantPos[(int)quadrant];
		if (Find(type, findlists, target))
		{
			return;
		}
		findlists = m_CSQuadrantPos[(int)eQuadrant];
		if (!Find(type, findlists, target))
		{
			findlists = m_CSQuadrantPos[(int)eQuadrant2];
			if (!Find(type, findlists, target))
			{
				findlists = m_CSQuadrantPos[(int)eQuadrant3];
				Find(type, findlists, target);
			}
		}
	}

	private void DistributeByEquip(PeEntity target, int _needNum)
	{
		ELineType type = ELineType.TeamAtk;
		List<PeEntity> findlists = m_DicEquipMembers[1];
		Find(type, findlists, target);
	}

	private void DistributeAtkLine()
	{
		if (mCScreator == null || mCScreator.Assembly == null)
		{
			return;
		}
		if (m_TeamEnemies.Count > 0 && !teamLinesContain(ELineType.TeamAtk))
		{
			NewAtkLine();
		}
		if (teamLinesContain(ELineType.TeamAtk))
		{
			for (int i = 0; i < m_TeamEnemies.Count; i++)
			{
				MsgLine(ELineType.TeamAtk, ELineMsg.ADD_Target, m_TeamEnemies[i]);
			}
		}
		bool flag = needAddInMember(ELineType.TeamAtk);
		int needNum = 0;
		if (teamLinesContain(ELineType.TeamAtk))
		{
			needNum = m_TeamLines[1].GetNeedMemNumber();
		}
		bool flag2 = false;
		if (flag)
		{
			for (int j = 0; j < m_TeamEnemies.Count; j++)
			{
				if (DicteamEnemiesContain(m_TeamEnemies[j]))
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				List<PeEntity> list = m_DicteamEnemies[1];
				List<PeEntity> list2 = m_DicteamEnemies[0];
				for (int k = 0; k < list.Count; k++)
				{
					DistributeByEquip(list[k], needNum);
				}
				for (int l = 0; l < m_TeamEnemies.Count; l++)
				{
					if (!DicteamEnemiesContain(m_TeamEnemies[l]))
					{
						DistributeByQuadrant(m_TeamEnemies[l], needNum);
					}
				}
			}
			else
			{
				for (int m = 0; m < m_TeamEnemies.Count; m++)
				{
					DistributeByQuadrant(m_TeamEnemies[m], needNum);
				}
			}
		}
		DistributeAtkBench();
	}

	private void DistributeAtkBench()
	{
		int count = m_IdleLists.Count;
		for (int i = 0; i < m_TeamEnemies.Count; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (m_IdleLists[j] == null || m_IdleLists[j].Equals(null) || m_IdleLists[j].IsDead() || m_IdleLists[j].isRagdoll)
				{
					continue;
				}
				stEnemies byChase = GetByChase(m_IdleLists[j]);
				if (byChase != null)
				{
					m_IdleLists[j].target.SetEnityCanAttack(canAttackOrNot: true);
					m_IdleLists[j].NpcCmpt.BattleMgr.ChoiceTheEnmey(m_IdleLists[j], byChase.mTarget);
					continue;
				}
				stEnemies byTarget = GetByTarget(m_TeamEnemies[i]);
				bool flag = byTarget == null || m_TeamEnemies[i].monsterProtoDb == null || byTarget.ChasesCout < m_TeamEnemies[i].monsterProtoDb.AtkDb.mChaseNumber;
				bool flag2 = SelectItem.MatchEnemyEquip(m_IdleLists[j], m_TeamEnemies[i]);
				bool flag3 = flag2 && PEUtil.InRange(m_TeamEnemies[i].position, m_IdleLists[j].position, Atk_JionR);
				bool flag4 = PEUtil.InRange(m_TeamEnemies[i].position, m_IdleLists[j].position, Atk_RunR);
				if (!flag || !flag2)
				{
					if (flag4)
					{
						m_IdleLists[j].NpcCmpt.SetCanIdle(_canIdle: false);
					}
					else
					{
						m_IdleLists[j].NpcCmpt.SetCanIdle(_canIdle: true);
					}
				}
				else if (flag3)
				{
					m_IdleLists[j].target.SetEnityCanAttack(canAttackOrNot: true);
					m_IdleLists[j].NpcCmpt.BattleMgr.ChoiceTheEnmey(m_IdleLists[j], m_TeamEnemies[i]);
					mapEnemiesAddChase(m_IdleLists[j], m_TeamEnemies[i]);
				}
				else if (flag4)
				{
					m_IdleLists[j].NpcCmpt.SetCanIdle(_canIdle: false);
				}
				else
				{
					m_IdleLists[j].NpcCmpt.SetCanIdle(_canIdle: true);
				}
			}
		}
	}

	private void DistributeAsseblyProtect(PeEntity target)
	{
		if (m_assemblyEnemies == null || !m_assemblyEnemies.ContainsKey(target) || teamMembers == null || target.monsterProtoDb == null || target.monsterProtoDb.AtkDb == null || m_assemblyEnemies[target].Count >= target.monsterProtoDb.AtkDb.mNumber || m_TeamLines == null || !teamLinesContain(ELineType.TeamAtk) || !(m_TeamLines[1] is AttackLine attackLine))
		{
			return;
		}
		AtkCooperation atkCooperation = attackLine.GetAtkCooperByTarget(target);
		if (atkCooperation == null)
		{
			atkCooperation = attackLine.NewTargetAtkCooperation(target);
		}
		if (atkCooperation != null && !atkCooperation.IsneedMember())
		{
			return;
		}
		for (int i = 0; i < teamMembers.Count; i++)
		{
			if (m_assemblyEnemies[target].Count >= target.monsterProtoDb.AtkDb.mNumber)
			{
				break;
			}
			if (!SelectItem.MatchEnemyEquip(teamMembers[i], target) || assemblyEnemiesContains(teamMembers[i]))
			{
				continue;
			}
			if (atkCooperation.ContainMember(teamMembers[i]))
			{
				AddToAssemblyProtectMember(target, teamMembers[i]);
				continue;
			}
			AtkCooperation atkCooperByMember = attackLine.GetAtkCooperByMember(teamMembers[i]);
			if (atkCooperByMember != null)
			{
				RemoveFromLine(ELineType.TeamAtk, teamMembers[i]);
			}
			if (!teamLinesContain(ELineType.TeamAtk))
			{
				AttackLine attackLine2 = NewAtkLine();
				attackLine2.AddAktTarget(target);
				attackLine2.NewTargetAtkCooperation(target);
			}
			AddinLine(ELineType.TeamAtk, teamMembers[i], target);
			AddToAssemblyProtectMember(target, teamMembers[i]);
		}
	}

	private void DistributeChatLine()
	{
		if (!(mCScreator == null) && mCScreator.Assembly != null && m_DicJobMembers[1] == null)
		{
			return;
		}
		ELineType eLineType = ELineType.TeamChat;
		bool flag = checkTime.IsChatTimeSlot((float)GameTime.Timer.HourInDay);
		bool flag2 = teamLinesContain(eLineType);
		if (flag && !mIndanger)
		{
			bool flag3 = needAddInMember(eLineType);
			if (flag && !flag2)
			{
				NewChatLine();
			}
			if (flag3)
			{
				List<PeEntity> findlists = m_DicJobMembers[1];
				Find(eLineType, findlists, null);
			}
		}
		else if (flag2)
		{
			m_tempList.Clear();
			m_tempList.AddRange(m_TeamLines[(int)eLineType].Linemembers);
			for (int i = 0; i < m_tempList.Count; i++)
			{
				RemoveFromLine(eLineType, m_tempList[i]);
			}
			MsgLine(ELineType.TeamChat, ELineMsg.Clear_chat);
			if (teamLinesContain(eLineType))
			{
				removeTeamLines(eLineType);
			}
		}
	}

	private void CreatChatCentor(List<PeEntity> lists)
	{
		int num = 1 + Mathf.Abs(lists.Count - 10) / 10;
		for (int i = 0; i < lists.Count; i++)
		{
			if (m_TeamLines[5] is chatLine chatLine2 && chatLine2.CenterNum < num)
			{
				EQuadrant quadrant = GetQuadrant(lists[i]);
				if (suitLine(ELineType.TeamChat, lists[i], quadrant))
				{
					MsgLine(ELineType.TeamChat, ELineMsg.Add_chatCentor, quadrant, lists[i]);
				}
				if (CanAddInLine(ELineType.TeamChat, lists[i], quadrant))
				{
					AddinLine(ELineType.TeamChat, lists[i], null);
				}
			}
		}
	}

	private void DistributeSleepLine()
	{
		if (!(mCScreator != null) && mCScreator.Assembly == null)
		{
			return;
		}
		ELineType eLineType = ELineType.TeamSleep;
		if (mIndanger)
		{
			if (teamLinesContain(eLineType) && m_TeamLines[(int)eLineType] is SleepLine { Linemembers: not null } sleepLine && sleepLine.Linemembers.Count > 0)
			{
				m_tempList.Clear();
				m_tempList.AddRange(sleepLine.Linemembers);
				for (int i = 0; i < m_tempList.Count; i++)
				{
					RemoveFromLine(eLineType, m_tempList[i]);
				}
			}
			return;
		}
		bool flag = CsSleepSlots.startSlots.InSlot((float)GameTime.Timer.HourInDay);
		bool flag2 = CsSleepSlots.endSlots.InSlot((float)GameTime.Timer.HourInDay);
		if (flag)
		{
			if (!teamLinesContain(eLineType))
			{
				starttime = GameTime.Timer.Minute;
				NewSleepLine();
			}
			if (GameTime.Timer.Minute - starttime > (double)Sleep_Batch_Time)
			{
				if (m_TeamLines[(int)eLineType].Linemembers.Count >= m_IdleLists.Count)
				{
					return;
				}
				MsgLine(eLineType, ELineMsg.Add_Sleep, Sleep_Mumber_Num, GameTime.Timer.Hour);
				starttime = GameTime.Timer.Minute;
				List<PeEntity> idleLists = m_IdleLists;
				Find(eLineType, idleLists);
			}
		}
		if (flag2)
		{
			if (getUpstartTime == 0.0)
			{
				getUpstartTime = GameTime.Timer.Minute;
			}
			if (GameTime.Timer.Minute - getUpstartTime > (double)Awake_Batch_Time)
			{
				if (teamLinesContain(eLineType) && m_TeamLines[(int)eLineType] is SleepLine sleepLine2)
				{
					Cooperation upCooper = sleepLine2.GetUpCooper();
					if (upCooper != null)
					{
						m_tempList.Clear();
						m_tempList.AddRange(upCooper.GetCooperMembers());
						for (int j = 0; j < m_tempList.Count; j++)
						{
							RemoveFromLine(eLineType, m_tempList[j]);
						}
					}
				}
				getUpstartTime = GameTime.Timer.Minute;
			}
		}
		if (!flag2 && !flag && teamLinesContain(eLineType) && m_TeamLines[(int)eLineType] is SleepLine { Linemembers: not null } sleepLine3 && sleepLine3.Linemembers.Count > 0)
		{
			m_tempList.Clear();
			m_tempList.AddRange(sleepLine3.Linemembers);
			for (int k = 0; k < m_tempList.Count; k++)
			{
				RemoveFromLine(eLineType, m_tempList[k]);
			}
		}
	}

	private void DistributeEatLine()
	{
		if (!(mCScreator != null) && mCScreator.Assembly == null)
		{
			return;
		}
		ELineType eLineType = ELineType.TeamEat;
		bool flag = teamLinesContain(eLineType);
		if (checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay) && !mIndanger)
		{
			if (!flag)
			{
				NewEatLine();
			}
			if (m_TeamLines[(int)eLineType].Linemembers.Count < EAT_Mumber_Num)
			{
				List<PeEntity> eatQueues = m_EatQueues;
				Find(eLineType, eatQueues);
			}
		}
	}

	public override void InitTeam()
	{
		if (!mInit && mCScreator != null && mCScreator.Assembly != null)
		{
			base.InitTeam();
			Singleton<PeEventGlobal>.Instance.DeathEvent.AddListener(OnEntityDeath);
			Singleton<PeEventGlobal>.Instance.DestroyEvent.AddListener(OnEntityDestroy);
			if (m_TeamLines == null)
			{
				m_TeamLines = new TeamLine[6];
			}
			mCScreator.Assembly.AddEventListener(OnEntityEventListener);
			InitCheckTime();
			mInit = true;
		}
	}

	private void InitCheckTime()
	{
		mCheckTime = new NpcCheckTime();
		mCheckTime.AddSlots(EScheduleType.Chat);
		mCheckTime.AddSlots(EScheduleType.Eat);
		NPCScheduleData.Item item = NPCScheduleData.Get(1);
		mCsSleepSlots = new CSSleepSlots(item.Slots[0], item.Slots[1]);
	}

	public override List<PeEntity> GetTeamMembers()
	{
		return teamMembers;
	}

	public override bool ContainMember(PeEntity members)
	{
		return teamMembers.Contains(members);
	}

	public override bool RemoveFromTeam(PeEntity member)
	{
		if (teamMembers.Contains(member))
		{
			member.target.SetEnityCanAttack(canAttackOrNot: true);
			return teamMembers.Remove(member);
		}
		return false;
	}

	public override bool RemoveFromTeam(List<PeEntity> members)
	{
		for (int i = 0; i < members.Count; i++)
		{
			RemoveFromTeam(members[i]);
		}
		return true;
	}

	public override bool AddInTeam(List<PeEntity> members, bool Isclear = true)
	{
		if (teamMembers == null)
		{
			return false;
		}
		if (Isclear)
		{
			ClearTeam();
		}
		for (int i = 0; i < members.Count; i++)
		{
			AddInTeam(members[i]);
		}
		return true;
	}

	public override bool AddInTeam(PeEntity member)
	{
		if (teamMembers == null || member.NpcCmpt == null || member.NpcCmpt.Creater == null || member.NpcCmpt.IsFollower)
		{
			return false;
		}
		member.target.SetEnityCanAttack(canAttackOrNot: true);
		member.NpcCmpt.BattleMgr.SetSelectEnemy(null);
		teamMembers.Add(member);
		return true;
	}

	public override bool ClearTeam()
	{
		for (int i = 0; i < teamMembers.Count; i++)
		{
			teamMembers[i].target.SetEnityCanAttack(canAttackOrNot: true);
		}
		teamMembers.Clear();
		return true;
	}

	public override bool DissolveTeam()
	{
		ClearTeam();
		m_IdleLists.Clear();
		m_InLines.Clear();
		return true;
	}

	public override bool ReFlashTeam()
	{
		if (mCScreator != null && mCScreator.Assembly != null)
		{
			mCsNpcNumber = teamMembers.Count;
			m_IdleLists.Clear();
			ReflashAtkTarget();
			for (int i = 0; i < teamMembers.Count; i++)
			{
				if (!ContainInLine(teamMembers[i], ELineType.TeamAtk))
				{
					teamMembers[i].NpcCmpt.SetCanIdle(_canIdle: true);
					m_IdleLists.Add(teamMembers[i]);
				}
			}
			ReflashLists();
			ReflashEatState();
			DistributeChatLine();
			DistributeEatLine();
			DistributeSleepLine();
			ReflashDicLines();
			return true;
		}
		return false;
	}

	private void OnEntityEventListener(int event_id, CSEntity entity, object arg)
	{
		if (event_id == 1)
		{
			DissolveTeam();
			if (mCScreator != null && mCScreator.Assembly != null)
			{
				mCScreator.Assembly.RemoveEventListener(OnEntityEventListener);
				mCScreator = null;
			}
		}
	}

	public override void OnAlertInform(PeEntity enemy)
	{
		if (!TeamEnemiesContain(enemy))
		{
			AddToTeamEnemies(enemy);
			mapEnemiesAddTarget(enemy);
			if (teamLinesContain(ELineType.TeamAtk))
			{
				m_TeamLines[1].Go();
			}
			mIndanger = true;
		}
	}

	public override void OnClearAlert()
	{
		if (m_beInLine[1].Count > 0)
		{
			m_TeamLines[1].ClearMembers();
			m_beInLine[1].Clear();
		}
	}

	private void OnEntityDeath(SkEntity skSelf, SkEntity skCaster)
	{
		PeEntity component = skSelf.GetComponent<PeEntity>();
		if (!(component == null))
		{
			OnTargetLost(component);
			OnTeammberDeath(component);
		}
	}

	private void OnEntityDestroy(SkEntity entity)
	{
		PeEntity component = entity.GetComponent<PeEntity>();
		if (!(component == null))
		{
			OnTargetLost(component);
			RemoveFromTeam(component);
		}
	}

	public void OnTeammberDeath(PeEntity entity)
	{
		if (ContainMember(entity))
		{
			RemoveFromLine(ELineType.TeamAtk, entity);
		}
	}

	public void OnTargetLost(PeEntity entity)
	{
		if (entity == null || !m_TeamEnemies.Contains(entity))
		{
			return;
		}
		if (teamLinesContain(ELineType.TeamAtk) && m_TeamLines[1] is AttackLine attackLine)
		{
			m_tempList.Clear();
			List<PeEntity> atkMemberByTarget = attackLine.GetAtkMemberByTarget(entity);
			if (atkMemberByTarget != null)
			{
				m_tempList.AddRange(atkMemberByTarget);
			}
			for (int i = 0; i < m_tempList.Count; i++)
			{
				RemoveFromLine(ELineType.TeamAtk, m_tempList[i]);
			}
			RemoveFormTeamEnemies(entity);
			mapEnemiesRemoveTaregt(entity);
			attackLine.DissolveTheline(entity);
			if (mCScreator == null || mCScreator.Assembly == null)
			{
				for (int j = 0; j < m_tempList.Count; j++)
				{
					m_tempList[j].target.SetEnityCanAttack(canAttackOrNot: true);
				}
			}
		}
		RemoveFromAssemblyEnemies(entity);
	}

	public void OnAssemblyHpChange(SkEntity caster, float hpChange)
	{
		PESkEntity pESkEntity = PEUtil.GetCaster(caster) as PESkEntity;
		if (pESkEntity == null)
		{
			return;
		}
		PeEntity component = pESkEntity.GetComponent<PeEntity>();
		if (component == null || teamMembers == null)
		{
			return;
		}
		for (int i = 0; i < teamMembers.Count; i++)
		{
			if (!teamMembers[i].Equals(null) && teamMembers[i].target != null)
			{
				teamMembers[i].target.TransferHatred(component, Mathf.Abs(hpChange * 10f));
			}
		}
	}

	private void MsgLine(ELineType type, params object[] objs)
	{
		if (teamLinesContain(type))
		{
			m_TeamLines[(int)type].OnMsgLine(objs);
		}
	}

	public void OnAttackTypeChange(PeEntity entity, AttackType oldType, AttackType newType)
	{
		if (oldType == AttackType.Ranged && newType == AttackType.Melee && teamLinesContain(ELineType.TeamAtk) && m_TeamLines[1] is AttackLine attackLine && attackLine.ContainEntity(entity))
		{
			RemoveFromLine(ELineType.TeamAtk, entity);
		}
	}

	public void OnLineChange(PeEntity member, ELineType oldType, ELineType newType)
	{
		if (teamLinesContain(oldType) && m_TeamLines[(int)oldType].ContainEntity(member) && teamLinesContain(newType) && ComPearPriority(m_TeamLines[(int)newType], m_TeamLines[(int)oldType]))
		{
			RemoveFromLine(oldType, member);
		}
	}

	public void OnCsNpcJobChange(PeEntity member, ENpcJob oldJob, ENpcJob newJob)
	{
		if (oldJob != ENpcJob.Resident)
		{
			return;
		}
		switch (newJob)
		{
		case ENpcJob.None:
		case ENpcJob.Resident:
			break;
		case ENpcJob.Follower:
		{
			List<ELineType> list = new List<ELineType>();
			if (teamMembers == null || !teamMembers.Contains(member) || m_TeamLines == null)
			{
				break;
			}
			for (int i = 0; i < m_TeamLines.Length; i++)
			{
				if (m_TeamLines[i] != null && m_TeamLines[i].ContainEntity(member))
				{
					list.Add((ELineType)i);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				RemoveFromLine(list[j], member);
			}
			RemoveFromTeam(member);
			break;
		}
		default:
		{
			ELineType eLineType = ELineType.TeamChat;
			if (teamLinesContain(eLineType) && m_TeamLines[(int)eLineType].ContainEntity(member))
			{
				RemoveFromLine(eLineType, member);
			}
			break;
		}
		}
	}

	public void OnCsNPcRemove(PeEntity member)
	{
		RemoveFromTeam(member);
	}
}
