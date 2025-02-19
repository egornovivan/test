using System;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;
using Behave.Runtime.Action;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class TargetCmpt : PeCmpt, IPeMsg
{
	private const float SwitchValue = 1.1f;

	private const float Multiple = 3f;

	private const float ThreatIntervalDistance = 0.5f;

	private const float ThreatIntervalTime = 0.5f;

	public Action<PeEntity, PeEntity, float> HatredEvent;

	private bool m_FirstDamage;

	private bool m_CanSearch;

	private bool m_CanAttack;

	private PeEntity m_Entity;

	private SkEntity m_SkEntity;

	private PeTrans m_Trans;

	private AnimatorCmpt m_Animator;

	private RequestCmpt m_Request;

	private Motion_Equip m_Equipment;

	private CommonCmpt m_Common;

	private NpcCmpt m_Npc;

	private PEMonster m_Monster;

	private List<Enemy> m_Enemies = new List<Enemy>();

	private List<PeEntity> _tmpEntities = new List<PeEntity>();

	private List<PeEntity> _tmpReputationEntities = new List<PeEntity>();

	private List<PeEntity> _corpses = new List<PeEntity>();

	private List<PeEntity> _treats = new List<PeEntity>();

	private List<PeEntity> _Melees = new List<PeEntity>();

	private List<BTAction> _actions = new List<BTAction>();

	private List<IAttack> _Attacks = new List<IAttack>();

	private List<IWeapon> _Weapons = new List<IWeapon>();

	private Enemy m_Enemy;

	private Enemy m_Escape;

	private Enemy m_Specified;

	private IAttack m_Attack;

	private PEVision[] m_Visions;

	private PEHearing[] m_Hears;

	private float m_EscapeBase;

	private PeTrans m_mainPlayerTran;

	private long m_FrameCount;

	public static float Combatpercent = 1.5f;

	public static float HPpercent = 0.4f;

	public static float Atkpercent = 0.4f;

	public static float Defpercent = 0.2f;

	private PeEntity m_Afraid;

	private PeEntity m_Doubt;

	private PeEntity m_Chat;

	private PeEntity m_Food;

	private PeEntity m_Treat;

	private Vector3 m_HidePistion;

	private float m_EscapeProp;

	private bool m_Scan = true;

	private bool m_IsAddHatred = true;

	private bool m_CanTransferHatred = true;

	private bool m_CanActiveAttack = true;

	private bool m_UseTool;

	private bool m_beSkillTarget;

	private List<NpcCmpt> mAllys = new List<NpcCmpt>();

	private Vector3 direction = Vector3.back;

	private Enemy mHideEnemy;

	private List<Vector3> mdirs = new List<Vector3>();

	public Enemy enemy
	{
		get
		{
			if (!Enemy.IsNullOrInvalid(m_Enemy))
			{
				return m_Enemy;
			}
			return null;
		}
	}

	public PeEntity Afraid
	{
		get
		{
			return m_Afraid;
		}
		set
		{
			m_Afraid = value;
		}
	}

	public PeEntity Doubt
	{
		get
		{
			return m_Doubt;
		}
		set
		{
			m_Doubt = value;
		}
	}

	public PeEntity Chat
	{
		get
		{
			return m_Chat;
		}
		set
		{
			m_Chat = value;
		}
	}

	public PeEntity Food
	{
		get
		{
			return m_Food;
		}
		set
		{
			m_Food = value;
		}
	}

	public PeEntity Treat
	{
		get
		{
			return m_Treat;
		}
		set
		{
			m_Treat = value;
		}
	}

	public Vector3 HidePistion => m_HidePistion;

	public float EscapeBase
	{
		get
		{
			return m_EscapeBase;
		}
		set
		{
			m_EscapeBase = value;
		}
	}

	public float EscapeProp
	{
		get
		{
			return m_EscapeProp;
		}
		set
		{
			m_EscapeProp = value;
		}
	}

	public bool Scan
	{
		get
		{
			return m_Scan;
		}
		set
		{
			m_Scan = value;
		}
	}

	public bool IsAddHatred
	{
		get
		{
			return m_IsAddHatred;
		}
		set
		{
			m_IsAddHatred = value;
		}
	}

	public bool CanTransferHatred
	{
		get
		{
			return m_CanTransferHatred;
		}
		set
		{
			m_CanTransferHatred = value;
		}
	}

	public bool CanActiveAttck
	{
		get
		{
			return m_CanActiveAttack;
		}
		set
		{
			m_CanActiveAttack = value;
		}
	}

	public bool UseTool
	{
		get
		{
			return m_UseTool;
		}
		set
		{
			m_UseTool = value;
		}
	}

	public bool beSkillTarget => m_beSkillTarget;

	public List<IAttack> Attacks => _Attacks;

	public void SetActions(List<BTAction> actions)
	{
		_actions = actions;
		for (int i = 0; i < _actions.Count; i++)
		{
			if (_actions[i] == null || !(_actions[i] is BTAttackBase))
			{
				continue;
			}
			foreach (KeyValuePair<string, object> data in _actions[i].GetDatas())
			{
				if (data.Value != null && data.Value is IAttack)
				{
					_Attacks.Add((IAttack)data.Value);
				}
			}
		}
	}

	public bool ContainEnemy(Enemy enemy)
	{
		return m_Enemies.Contains(enemy);
	}

	public Enemy GetEnemy(PeEntity targetEntity)
	{
		return m_Enemies.Find((Enemy ret) => ret != null && ret.isValid && ret.entityTarget == targetEntity);
	}

	public List<Enemy> GetEnemies()
	{
		return m_Enemies;
	}

	public bool HasAnyEnemy()
	{
		return m_Enemies.Count > 0;
	}

	public bool HasEnemy()
	{
		return GetAttackEnemy() != null;
	}

	public bool ContainsMelee(PeEntity peEntity)
	{
		return _Melees.Contains(peEntity);
	}

	public void AddMelee(PeEntity peEntity, int n = 3)
	{
		if (!_Melees.Contains(peEntity) && _Melees.Count < n)
		{
			_Melees.Add(peEntity);
		}
	}

	public void RemoveMelee(PeEntity peEntity)
	{
		if (_Melees.Contains(peEntity))
		{
			_Melees.Remove(peEntity);
		}
	}

	public List<PeEntity> GetMelees()
	{
		return _Melees;
	}

	public int GetMeleeCount()
	{
		return _Melees.Count;
	}

	public bool HasHatred(PeEntity entity)
	{
		Enemy enemy = m_Enemies.Find((Enemy ret) => ret != null && ret.entityTarget == entity);
		if (!Enemy.IsNullOrInvalid(enemy) && enemy.Hatred > float.Epsilon)
		{
			return true;
		}
		return false;
	}

	public void ClearEscapeEnemy()
	{
		m_Enemies.Remove(m_Escape);
		m_Escape = null;
	}

	public Enemy GetEscapeEnemy()
	{
		if (IsDeath() || m_Entity.IsDarkInDaytime)
		{
			return null;
		}
		if ((m_Common != null && m_Common.TDObj != null) || m_Entity.IsSeriousInjury)
		{
			return null;
		}
		if (!Enemy.IsNullOrInvalid(m_Escape))
		{
			return m_Escape;
		}
		return null;
	}

	public Enemy GetEscapeEnemyUnit()
	{
		if (IsDeath())
		{
			return null;
		}
		if (m_Common != null && m_Common.TDObj != null)
		{
			return null;
		}
		if (!Enemy.IsNullOrInvalid(m_Escape))
		{
			return m_Escape;
		}
		return null;
	}

	public Enemy GetFollowEnemy()
	{
		if (!Enemy.IsNullOrInvalid(m_Enemy))
		{
			return null;
		}
		return m_Enemies.Find((Enemy ret) => !Enemy.IsNullOrInvalid(ret) && ret.canFollowed);
	}

	public Enemy GetAfraidEnemy()
	{
		return m_Enemies.Find((Enemy ret) => ret?.canAfraid ?? false);
	}

	public Enemy GetThreatEnemy()
	{
		return m_Enemies.Find((Enemy ret) => ret?.canThreat ?? false);
	}

	public Enemy GetAttackEnemy()
	{
		if (!Enemy.IsNullOrInvalid(m_Enemy))
		{
			return m_Enemy;
		}
		return null;
	}

	public PeEntity GetAfraidTarget()
	{
		int p = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
		for (int i = 0; i < _tmpReputationEntities.Count; i++)
		{
			if (_tmpReputationEntities[i] != null)
			{
				int p2 = (int)_tmpReputationEntities[i].GetAttribute(AttribType.DefaultPlayerID);
				if (PeSingleton<ReputationSystem>.Instance.HasReputation(p2, p))
				{
					return _tmpEntities[i].GetComponent<PeEntity>();
				}
			}
		}
		return null;
	}

	public PeEntity GetDoubtTarget()
	{
		int p = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
		for (int i = 0; i < _tmpReputationEntities.Count; i++)
		{
			if (_tmpReputationEntities[i] != null)
			{
				int p2 = (int)_tmpReputationEntities[i].GetAttribute(AttribType.DefaultPlayerID);
				if (PeSingleton<ReputationSystem>.Instance.HasReputation(p2, p))
				{
					return _tmpEntities[i].GetComponent<PeEntity>();
				}
			}
		}
		return null;
	}

	public void ClearEnemy()
	{
		int count = m_Enemies.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Enemies[i] != null)
			{
				m_Enemies[i].Dispose();
			}
		}
		if (m_Enemy != null)
		{
			OnEnemyLost(m_Enemy);
		}
		if (m_Npc != null)
		{
			m_Npc.ClearLockedEnemies();
		}
		m_Enemy = null;
		m_Enemies.Clear();
	}

	public void AddDamageHatred(PeEntity argEntity, float hatred)
	{
		if (!(m_Entity == null) && !(argEntity == null) && !(argEntity == m_Entity) && !IsDeath())
		{
			Enemy enemy = m_Enemies.Find((Enemy ret) => ret != null && ret.entityTarget != null && ret.entityTarget == argEntity);
			if (enemy == null)
			{
				enemy = new Enemy(m_Entity, argEntity);
				AddEnemy(enemy);
			}
			enemy.OnDamage(hatred);
			if (HatredEvent != null)
			{
				HatredEvent(m_Entity, argEntity, hatred);
			}
			PeNpcGroup.Instance.OnCSAddDamageHaterd(argEntity, m_Entity, hatred);
		}
	}

	public void AddSharedHatred(PeEntity argEntity, float hatred)
	{
		if (!(m_Entity == null) && !(argEntity == null) && !(argEntity == m_Entity) && !IsDeath() && CanAddDamageHatred(argEntity) && (!(m_Entity.NpcCmpt != null) || m_CanTransferHatred))
		{
			Enemy enemy = m_Enemies.Find((Enemy ret) => ret != null && ret.entityTarget != null && ret.entityTarget == argEntity);
			if (enemy != null)
			{
				enemy.AddHatred(hatred);
			}
			else
			{
				AddEnemy(new Enemy(m_Entity, argEntity, hatred));
			}
			if (HatredEvent != null)
			{
				HatredEvent(m_Entity, argEntity, hatred);
			}
			PeNpcGroup.Instance.OnCSAddDamageHaterd(argEntity, m_Entity, hatred);
		}
	}

	public void CallHelp(float radius)
	{
		if (m_Entity == null)
		{
			return;
		}
		int playerID = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
		List<PeEntity> entitiesFriendly = PeSingleton<EntityMgr>.Instance.GetEntitiesFriendly(m_Trans.position, radius, playerID, m_Entity.ProtoID, isDeath: false, m_Entity);
		for (int i = 0; i < entitiesFriendly.Count; i++)
		{
			TargetCmpt component = entitiesFriendly[i].GetComponent<TargetCmpt>();
			if (component != null)
			{
				component.CopyEnemies(this);
			}
		}
	}

	public void CopyEnemies(TargetCmpt target)
	{
		List<Enemy> enemies = target.GetEnemies();
		int count = enemies.Count;
		for (int i = 0; i < count; i++)
		{
			Enemy enemy = enemies[i];
			float num = ((GetAttackEnemy() == null) ? ((!enemy.Equals(target.GetAttackEnemy())) ? 0.125f : 0.25f) : 0.032f);
			float hatred = Mathf.Clamp(enemy.Hatred * num, 1f, 100f);
			AddSharedHatred(enemy.entityTarget, hatred);
		}
	}

	public void OnDamageMember(PeEntity entity, float damage)
	{
		AddSharedHatred(entity, damage * 0.5f);
		if (m_Entity != null && m_Entity.Group != null && m_Entity.Group.AlivePercent <= 0.5f && UnityEngine.Random.value < 0.5f)
		{
			SetEscape(entity);
		}
	}

	public void OnTargetDiscover(PeEntity entity)
	{
		if (!ContainsEnemy(entity))
		{
			SelectEntity(entity);
		}
	}

	public void TransferHatred(PeEntity argEntity, float hatred)
	{
		if (!m_CanTransferHatred || argEntity == null || m_Entity == null || argEntity == m_Entity || IsDeath() || (m_Npc != null && m_Npc.Battle == ENpcBattle.Passive))
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			int srcPlayerID = (int)argEntity.GetAttribute(AttribType.DefaultPlayerID);
			int dstPlayerID = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
			if (Singleton<ForceSetting>.Instance.AllyPlayer(srcPlayerID, dstPlayerID))
			{
				return;
			}
		}
		SkEntity caster = PEUtil.GetCaster(argEntity.skEntity);
		if (!(caster == null))
		{
			PeEntity component = caster.GetComponent<PeEntity>();
			float num = ((GetAttackEnemy() == null) ? 0.125f : 0.032f);
			AddSharedHatred(component, hatred * num);
		}
	}

	public PeEntity GetReputation(ReputationSystem.ReputationLevel minLevel, ReputationSystem.ReputationLevel maxLevel)
	{
		for (int i = 0; i < _tmpEntities.Count; i++)
		{
			int num = (int)_tmpEntities[i].GetAttribute(AttribType.DefaultPlayerID);
			int num2 = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
			if (PeSingleton<ReputationSystem>.Instance.HasReputation(num, num2))
			{
				ReputationSystem.ReputationLevel reputationLevel = PeSingleton<ReputationSystem>.Instance.GetReputationLevel(num, num2);
				if (reputationLevel <= maxLevel && reputationLevel >= minLevel)
				{
					return _tmpEntities[i];
				}
			}
		}
		return null;
	}

	public override void Start()
	{
		base.Start();
		m_FirstDamage = true;
		m_CanSearch = true;
		m_CanAttack = true;
		m_Entity = GetComponent<PeEntity>();
		m_SkEntity = GetComponent<SkEntity>();
		m_Trans = GetComponent<PeTrans>();
		m_Animator = GetComponent<AnimatorCmpt>();
		m_Request = GetComponent<RequestCmpt>();
		m_Equipment = GetComponent<Motion_Equip>();
		m_Common = GetComponent<CommonCmpt>();
		m_Npc = GetComponent<NpcCmpt>();
		PESkEntity pESkEntity = m_Entity.skEntity as PESkEntity;
		if (pESkEntity != null)
		{
			pESkEntity.deathEvent += OnDeath;
		}
		StartCoroutine(ClearEnemyEnumerator());
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_FrameCount++;
		if (m_FrameCount % 15 != 0L)
		{
			return;
		}
		_tmpEntities.Clear();
		_tmpEntities.AddRange(GetMelees());
		for (int i = 0; i < _tmpEntities.Count; i++)
		{
			if (_tmpEntities[i] == null || _tmpEntities[i].IsDeath() || !_tmpEntities[i].hasView)
			{
				RemoveMelee(_tmpEntities[i]);
			}
		}
		if (base.Entity.hasView && (!(base.Entity.netCmpt != null) || base.Entity.netCmpt.IsController))
		{
			if (!base.Entity.Equals(PeSingleton<PeCreature>.Instance.mainPlayer))
			{
				CalculateEnemy();
				CalculateAllys();
				CalculateSectorField();
			}
			else
			{
				UpdateEnemies();
			}
		}
	}

	private void CalculateEnemy()
	{
		CollectEnemies();
		UpdateEnemies();
		SelectEnemy();
	}

	private void CalculateAllys()
	{
		if (!(m_Npc != null) || !m_Npc.IsFollower || m_Npc.IsOnVCCarrier || m_Npc.IsOnRail)
		{
			return;
		}
		mAllys = m_Npc.Allys;
		if (mAllys == null)
		{
			return;
		}
		for (int i = 0; i < mAllys.Count; i++)
		{
			if (mAllys[i].IsOnRail || mAllys[i].IsOnVCCarrier)
			{
				return;
			}
		}
		for (int j = 0; j < mAllys.Count; j++)
		{
			NpcCmpt npcCmpt = mAllys[j];
			if (!npcCmpt.IsOnVCCarrier && !npcCmpt.IsOnRail && ConditionDecide(m_Npc, npcCmpt))
			{
				m_Npc.Req_UseSkill();
				m_Npc.NpcSkillTarget = npcCmpt;
				m_Npc.InAllys = true;
			}
		}
	}

	private bool IsDeath()
	{
		return m_Entity == null || m_Entity.IsDeath();
	}

	public bool CanAttack()
	{
		return m_Entity != null && m_Entity.hasView && m_CanAttack && !m_Entity.IsSeriousInjury && !m_Entity.IsDarkInDaytime && GetEscapeEnemy() == null;
	}

	public void SetEnityCanAttack(bool canAttackOrNot)
	{
		m_CanAttack = canAttackOrNot;
	}

	public void SetCanAtiveWeapon(bool value)
	{
		m_CanActiveAttack = value;
	}

	private bool ContainsEnemy(PeEntity entity)
	{
		for (int i = 0; i < m_Enemies.Count; i++)
		{
			if (m_Enemies[i] != null && m_Enemies[i].entityTarget == entity)
			{
				return true;
			}
		}
		return false;
	}

	private void AddEnemy(Enemy enemy)
	{
		if (!IsDeath() && !m_Enemies.Contains(enemy))
		{
			m_Enemies.Add(enemy);
		}
	}

	private bool ContainsAction(Type type)
	{
		for (int i = 0; i < _actions.Count; i++)
		{
			if (type.IsInstanceOfType(_actions[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void SetEscape(PeEntity escape)
	{
		if (escape != null && ContainsAction(typeof(BTEscape)) && !RandomDunGenUtil.IsInDungeon(base.Entity))
		{
			m_Escape = m_Enemies.Find((Enemy ret) => !Enemy.IsNullOrInvalid(ret) && ret.entityTarget.Equals(escape));
			if (base.Entity.Group != null && !Enemy.IsNullOrInvalid(m_Escape))
			{
				base.Entity.Group.SetEscape(m_Entity, escape);
			}
		}
	}

	public void SetEscapeEntity(PeEntity escape)
	{
		if (escape != null && ContainsAction(typeof(BTEscape)))
		{
			m_Escape = m_Enemies.Find((Enemy ret) => !Enemy.IsNullOrInvalid(ret) && ret.entityTarget.Equals(escape));
		}
	}

	private bool CanSelectEntity(PeEntity entity)
	{
		if (ContainsEnemy(entity))
		{
			return false;
		}
		int num = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
		int num2 = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
		int src = (int)m_Entity.GetAttribute(AttribType.CampID);
		int dst = (int)entity.GetAttribute(AttribType.CampID);
		return PEUtil.CanAttackReputation(num, num2) && Singleton<ForceSetting>.Instance.Conflict(num, num2) && (float)Mathf.Abs(ThreatData.GetInitData(src, dst)) > float.Epsilon;
	}

	private bool CanAddDamageHatred(PeEntity target)
	{
		if (!m_IsAddHatred)
		{
			return false;
		}
		if (!CanAttackCarrier(target))
		{
			return false;
		}
		int src = Convert.ToInt32(m_Entity.GetAttribute(AttribType.DamageID));
		int dst = Convert.ToInt32(target.GetAttribute(AttribType.DamageID));
		return PEUtil.CanDamageReputation(m_Entity, target) && DamageData.GetValue(src, dst) > 0;
	}

	private bool CanAttackCarrier(PeEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (m_Monster == null || !m_Monster.isAttackCarrier)
		{
			return true;
		}
		if (entity.carrier != null && entity.carrier is HelicopterController)
		{
			return true;
		}
		return false;
	}

	private bool CanAttackEntity(PeEntity entity)
	{
		return true;
	}

	private bool CanAttackEnemy(PeEntity entity)
	{
		int num = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
		int num2 = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
		int src = Convert.ToInt32(m_Entity.GetAttribute(AttribType.DamageID));
		int dst = Convert.ToInt32(entity.GetAttribute(AttribType.DamageID));
		return PEUtil.CanDamageReputation(num, num2) && Singleton<ForceSetting>.Instance.Conflict(num, num2) && DamageData.GetValue(src, dst) != 0;
	}

	private bool GetHpJudge(NpcCmpt Slef, NpcCmpt Target, int SkillId)
	{
		if (Slef != null)
		{
			float npcChange_Hp = Slef.GetNpcChange_Hp(SkillId);
			if (npcChange_Hp == 0f)
			{
				return true;
			}
			float npcHppercent = Target.NpcHppercent;
			return npcHppercent <= npcChange_Hp;
		}
		return false;
	}

	private bool ConditionDecide(NpcCmpt SelfNpc, NpcCmpt targetNpc)
	{
		if (SelfNpc.GetReadySkill() == -1)
		{
			return false;
		}
		int readySkill = SelfNpc.GetReadySkill();
		float num = Vector3.Distance(SelfNpc.NpcPostion, targetNpc.NpcPostion);
		float npcSkillRange = SelfNpc.GetNpcSkillRange(readySkill);
		if (npcSkillRange == -1f)
		{
			return false;
		}
		return num < npcSkillRange && GetHpJudge(SelfNpc, targetNpc, readySkill);
	}

	private void hideDirction(Enemy enemie, Vector3 player)
	{
		mdirs.Add((player - enemie.position).normalized);
	}

	private Vector3 betterHideDirtion(List<Vector3> dirs)
	{
		if (dirs.Count <= 0)
		{
			return Vector3.back;
		}
		Vector3 vector = dirs[0];
		for (int i = 1; i < dirs.Count; i++)
		{
			vector = (vector + dirs[i]).normalized;
		}
		return vector;
	}

	private Vector3 calculateSteering(PeTrans selfTran, Vector3 targetPos, float max_velocity = 0.6f)
	{
		Vector3 forward = selfTran.forward;
		Vector3 normalized = (targetPos - selfTran.position).normalized;
		return (normalized - forward).normalized * max_velocity;
	}

	public float Steering_max_velocity(Vector3 PlayerPostion, Vector3 NpcPostion, float maxRadius = 6f, float minRadius = 4f)
	{
		float num = PEUtil.SqrMagnitudeH(PlayerPostion, NpcPostion);
		if (num > maxRadius * maxRadius)
		{
			return 0f;
		}
		if (num > minRadius * minRadius && num < maxRadius * maxRadius)
		{
			return (maxRadius - Mathf.Sqrt(num)) / 2f;
		}
		if (num < minRadius * minRadius)
		{
			return 1f;
		}
		return 0f;
	}

	private Vector3 flee_steeringPos(Vector3 dirPos, Vector3 plyer, PeTrans npcTran, float max_velocity = 0.5f)
	{
		float num = PEUtil.SqrMagnitudeH(dirPos, npcTran.position);
		if (num <= 4f)
		{
			return dirPos;
		}
		Vector3 position = npcTran.position;
		Vector3 forward = npcTran.forward;
		Vector3 normalized = (dirPos - npcTran.position).normalized;
		Vector3 vector = (normalized - forward).normalized * max_velocity;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		float max_velocity2 = 0f;
		for (int i = 0; i < m_Enemies.Count; i++)
		{
			if (!m_Enemies[i].canAttacked)
			{
				continue;
			}
			num = PEUtil.SqrMagnitudeH(m_Enemies[i].position, m_Trans.position);
			if (!(num >= 100f))
			{
				if (num <= 16f && num > 4f)
				{
					max_velocity2 = 0.6f;
				}
				if (num <= 4f)
				{
					max_velocity2 = 4f;
				}
				zero2 = -calculateSteering(npcTran, m_Enemies[i].position, max_velocity2);
				zero += zero2;
			}
		}
		return position + (npcTran.forward + vector + zero).normalized * 4f;
	}

	private void CalculateSectorField()
	{
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null && m_mainPlayerTran == null)
		{
			m_mainPlayerTran = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		}
		if (!(m_mainPlayerTran != null))
		{
			return;
		}
		if (m_Enemies != null && m_Enemies.Count > 0)
		{
			mdirs.Clear();
			for (int i = 0; i < m_Enemies.Count; i++)
			{
				float num = PEUtil.SqrMagnitudeH(m_Enemies[i].position, m_Trans.position);
				if (!(num >= 100f) && m_Enemies[i].canAttacked)
				{
					hideDirction(m_Enemies[i], m_mainPlayerTran.position);
				}
			}
			direction = betterHideDirtion(mdirs);
			if (direction != Vector3.back && direction != Vector3.zero)
			{
				m_HidePistion = m_mainPlayerTran.position + direction * 8f;
			}
			else
			{
				m_HidePistion = Vector3.zero;
			}
			if (m_HidePistion != Vector3.zero)
			{
				m_HidePistion = flee_steeringPos(m_HidePistion, m_mainPlayerTran.position, m_Trans);
			}
		}
		else
		{
			m_HidePistion = Vector3.zero;
		}
	}

	private bool MatchAttack(IAttack attack, Enemy enemy)
	{
		return attack.ReadyAttack(enemy);
	}

	private bool MatchAttackPositive(IAttack attack)
	{
		return attack is IAttackPositive;
	}

	private IEnumerator SwitchAttack()
	{
		while (true)
		{
			if (!Enemy.IsNullOrInvalid(m_Enemy) && (m_Enemy.Attack == null || !m_Enemy.Attack.IsRunning(m_Enemy)))
			{
				IAttack tmp = null;
				for (int i = 0; i < _Attacks.Count; i++)
				{
					if (_Attacks[i].IsReadyCD(m_Enemy) && _Attacks[i].ReadyAttack(m_Enemy))
					{
						tmp = _Attacks[i];
						break;
					}
				}
				if (tmp == null)
				{
					for (int j = 0; j < _Attacks.Count; j++)
					{
						if (_Attacks[j].IsReadyCD(m_Enemy) && _Attacks[j] is IAttackPositive)
						{
							tmp = _Attacks[j];
							break;
						}
					}
				}
				m_Enemy.Attack = tmp;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator SwitchWeapon()
	{
		while (m_Equipment != null)
		{
			bool canSwitchWeapon = true;
			if (base.Entity.motionMgr != null && base.Entity.motionMgr.IsActionRunning(PEActionType.SwordAttack))
			{
				canSwitchWeapon = false;
			}
			if (base.Entity.motionMgr != null && base.Entity.motionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
			{
				canSwitchWeapon = false;
			}
			if (m_Equipment.IsSwitchWeapon())
			{
				canSwitchWeapon = false;
			}
			if (base.Entity.isRagdoll)
			{
				canSwitchWeapon = false;
			}
			if (base.Entity.netCmpt != null && !base.Entity.netCmpt.IsController)
			{
				canSwitchWeapon = false;
			}
			if (canSwitchWeapon)
			{
				if (!Enemy.IsNullOrInvalid(m_Enemy))
				{
					Vector3 dir1 = Vector3.ProjectOnPlane(base.Entity.tr.forward, Vector3.up);
					Vector3 dir2 = m_Enemy.DirectionXZ;
					bool canSwitchNow = (base.Entity.Race != ERace.Paja && base.Entity.Race != ERace.Puja) || Vector3.Angle(dir1, dir2) < 90f;
					IWeapon weapon = SwitchWeapon(m_Enemy);
					if (weapon != null && !weapon.Equals(null) && canSwitchNow)
					{
						if (m_Equipment.Weapon == null || m_Equipment.Weapon.Equals(null))
						{
							if (!weapon.HoldReady)
							{
								weapon.HoldWeapon(hold: true);
								while (weapon != null && !weapon.Equals(null) && !weapon.HoldReady && Time.time < 5f)
								{
									yield return new WaitForSeconds(1f);
								}
							}
						}
						else if (!m_Equipment.Weapon.Equals(weapon))
						{
							m_Equipment.SwitchHoldWeapon(m_Equipment.Weapon, weapon);
							yield return new WaitForSeconds(5f);
						}
					}
				}
				else if (m_Equipment.Weapon != null && !m_Equipment.Weapon.Equals(null) && !m_UseTool)
				{
					m_Equipment.Weapon.HoldWeapon(hold: false);
					yield return new WaitForSeconds(2f);
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator ClearEnemyEnumerator()
	{
		while (true)
		{
			m_CanSearch = true;
			if (m_Enemy != null && m_Npc != null && m_Npc.IsFollower && m_Npc.FollowDistance > 4096f)
			{
				m_CanSearch = false;
				ClearEnemy();
			}
			yield return new WaitForSeconds(5f);
		}
	}

	private void CollectEnemies()
	{
		if (!m_Scan || !m_CanSearch || IsDeath())
		{
			return;
		}
		_tmpEntities.Clear();
		_tmpReputationEntities.Clear();
		int p = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
		int num = 0;
		if (m_Hears != null && m_Hears.Length > 0)
		{
			num = m_Hears.Length;
			for (int i = 0; i < num; i++)
			{
				PEHearing pEHearing = m_Hears[i];
				if (!(pEHearing != null) || pEHearing.Entities == null || pEHearing.Entities.Count <= 0)
				{
					continue;
				}
				int count = pEHearing.Entities.Count;
				for (int j = 0; j < count; j++)
				{
					PeEntity peEntity = pEHearing.Entities[j];
					if (peEntity != null && !_tmpEntities.Contains(peEntity))
					{
						_tmpEntities.Add(peEntity);
					}
				}
			}
		}
		if (m_Visions != null && m_Visions.Length > 0)
		{
			num = m_Visions.Length;
			for (int k = 0; k < num; k++)
			{
				PEVision pEVision = m_Visions[k];
				if (!(pEVision != null) || pEVision.Entities == null || pEVision.Entities.Count <= 0)
				{
					continue;
				}
				int count2 = pEVision.Entities.Count;
				for (int l = 0; l < count2; l++)
				{
					PeEntity peEntity2 = pEVision.Entities[l];
					if (peEntity2 != null && !_tmpEntities.Contains(peEntity2))
					{
						_tmpEntities.Add(peEntity2);
					}
				}
			}
		}
		num = _tmpEntities.Count;
		for (int m = 0; m < num; m++)
		{
			PeEntity peEntity3 = _tmpEntities[m];
			if (peEntity3 == null || m_Entity.Equals(peEntity3))
			{
				continue;
			}
			if (peEntity3.IsDeath())
			{
				if (m_Entity.Race == ERace.Monster && peEntity3.Race == ERace.Monster && !_corpses.Contains(peEntity3))
				{
					_corpses.Add(peEntity3);
				}
				continue;
			}
			int num2 = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
			int num3 = (int)peEntity3.GetAttribute(AttribType.DefaultPlayerID);
			if (peEntity3.IsSeriousInjury && !_treats.Contains(peEntity3) && !peEntity3.IsDeath() && peEntity3.hasView && num2 == num3)
			{
				if (UnityEngine.Random.value < 1f)
				{
					m_Treat = peEntity3;
				}
				_treats.Add(peEntity3);
			}
			int p2 = (int)peEntity3.GetAttribute(AttribType.DefaultPlayerID);
			if (!_tmpReputationEntities.Contains(peEntity3) && PeSingleton<ReputationSystem>.Instance.HasReputation(p2, p))
			{
				_tmpReputationEntities.Add(peEntity3);
			}
			if (CanSelectEntity(peEntity3) && CanAttackEntity(peEntity3) && CanAttackCarrier(peEntity3))
			{
				SelectEntity(peEntity3);
				if (m_Entity.Group != null)
				{
					m_Entity.Group.OnTargetDiscover(m_Entity, peEntity3);
				}
			}
		}
		_tmpEntities.Clear();
		SpecialHatred.IsHaveEnnemy(m_Entity, ref _tmpEntities);
		num = _tmpEntities.Count;
		for (int n = 0; n < num; n++)
		{
			if (_tmpEntities[n] != null && !ContainsEnemy(_tmpEntities[n]))
			{
				SelectEntity(_tmpEntities[n], 100f);
				if (m_Entity.Group != null)
				{
					m_Entity.Group.OnTargetDiscover(m_Entity, _tmpEntities[n]);
				}
			}
		}
	}

	private void UpdateEnemies()
	{
		int count = m_Enemies.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			Enemy enemy = m_Enemies[num];
			if (enemy != null)
			{
				if (enemy.CanDelete() || !CanAttackCarrier(enemy.entityTarget) || !CanAttackEnemy(enemy.entityTarget))
				{
					if (!base.Entity.IsAttacking || !enemy.Equals(m_Enemy))
					{
						m_Enemies[num].Dispose();
						m_Enemies.Remove(enemy);
					}
				}
				else
				{
					if (enemy.entityTarget != null && enemy.entityTarget.vehicle != null && enemy.entityTarget.vehicle.creationPeEntity != null && !ContainsEnemy(enemy.entityTarget.vehicle.creationPeEntity))
					{
						AddEnemy(new Enemy(m_Entity, enemy.entityTarget.vehicle.creationPeEntity));
					}
					if (enemy.entityTarget != null && enemy.skTarget != null && enemy.skTarget is CreationSkEntity)
					{
						enemy.ThreatShared = 0f;
						CarrierController carrier = enemy.entityTarget.carrier;
						if (carrier != null)
						{
							carrier.ForeachPassenger(delegate(PassengerCmpt passenger, bool isDriver)
							{
								Enemy enemy2 = m_Enemies.Find((Enemy ret) => ret.entityTarget != null && ret.entityTarget.Equals(passenger.Entity));
								if (enemy2 != null)
								{
									enemy.ThreatShared += enemy2.Threat * 10f;
								}
							});
						}
					}
					enemy.Update();
				}
			}
		}
		m_Enemies.Sort((Enemy a, Enemy b) => b.Hatred.CompareTo(a.Hatred));
	}

	private void SelectEnemy()
	{
		int num = m_Enemies.FindIndex((Enemy ret) => ret.canAttacked);
		if (!CanAttack() || num < 0)
		{
			if (m_Enemy != null)
			{
				OnEnemyLost(m_Enemy);
				m_Enemy = null;
			}
			if (m_Equipment != null && m_Equipment.Weapon != null)
			{
				m_Equipment.Weapon.HoldWeapon(hold: false);
			}
		}
		else if (m_Enemy == null || !m_Enemy.isValid)
		{
			m_Enemy = m_Enemies[num];
			OnEnemyAchieve(m_Enemy);
		}
		else if (!m_Enemy.canAttacked || !m_Enemies.Contains(m_Enemy) || m_Enemies[num].Hatred > m_Enemy.Hatred * 1.1f)
		{
			OnEnemyChange(m_Enemy, m_Enemies[num]);
			m_Enemy = m_Enemies[num];
		}
		if (Enemy.IsNullOrInvalid(m_Enemy) || !(m_Enemy.entityTarget.target != null) || m_Enemy.GroupAttack != 0)
		{
			return;
		}
		List<PeEntity> melees = m_Enemy.entityTarget.target.GetMelees();
		for (int i = 0; i < melees.Count; i++)
		{
			Enemy enemy = m_Enemy.entityTarget.target.GetEnemy(melees[i]);
			if (!Enemy.IsNullOrInvalid(enemy) && m_Enemy.Distance < enemy.Distance * 0.8f)
			{
				int n = ((m_Enemy.entityTarget.monsterProtoDb == null || m_Enemy.entityTarget.monsterProtoDb.AtkDb == null) ? 3 : m_Enemy.entityTarget.monsterProtoDb.AtkDb.mNumber);
				m_Enemy.entityTarget.target.RemoveMelee(enemy.entityTarget);
				m_Enemy.entityTarget.target.AddMelee(m_Entity, n);
			}
		}
	}

	private void SelectEntity(PeEntity entity, float hatred = 0f)
	{
		if (!(m_Entity == null))
		{
			Enemy enemy = new Enemy(m_Entity, entity, hatred);
			AddEnemy(enemy);
			if ((float)enemy.ThreatInit < -1E-45f && UnityEngine.Random.value < (float)Mathf.Abs(enemy.ThreatInit) / 100f)
			{
				SetEscape(entity);
			}
		}
	}

	private bool Match(IWeapon weapon, Enemy e)
	{
		if (e == null)
		{
			return false;
		}
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			if (attackMode[i].type == AttackType.Melee)
			{
				if (m_Npc != null)
				{
					if (e.IsInAir)
					{
						continue;
					}
				}
				else if (!e.IsOnLand)
				{
					continue;
				}
			}
			return true;
		}
		return false;
	}

	public bool CanAttackWeapon(IWeapon weapon, Enemy enemy)
	{
		if (m_Equipment.WeaponCanUse(weapon))
		{
			AttackMode[] attackMode = weapon.GetAttackMode();
			for (int i = 0; i < attackMode.Length; i++)
			{
				if (attackMode[i].type != 0 || ((!(m_Npc != null) || !enemy.IsInAir) && (!(m_Npc == null) || base.Entity.Race == ERace.Mankind || enemy.IsOnLand)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<IWeapon> GetCanUseWeaponList(Enemy enemy)
	{
		_Weapons.Clear();
		if (m_CanActiveAttack)
		{
			if (m_Npc == null)
			{
				_Weapons = m_Equipment.GetWeaponList();
			}
			else
			{
				_Weapons = m_Equipment.GetCanUseWeaponList(m_Entity);
			}
			_Weapons = _Weapons.FindAll((IWeapon ret) => CanAttackWeapon(ret, enemy));
		}
		return _Weapons;
	}

	public IWeapon SwitchWeapon(Enemy e)
	{
		IWeapon weapon = null;
		if (m_CanActiveAttack && !Enemy.IsNullOrInvalid(e))
		{
			float num = float.PositiveInfinity;
			List<IWeapon> list = ((!(m_Npc != null)) ? m_Equipment.GetWeaponList() : m_Equipment.GetCanUseWeaponList(m_Entity));
			for (int i = 0; i < list.Count; i++)
			{
				if (!m_Equipment.WeaponCanUse(list[i]) || !Match(list[i], e))
				{
					continue;
				}
				float num2 = float.PositiveInfinity;
				AttackMode[] attackMode = list[i].GetAttackMode();
				bool flag = false;
				for (int j = 0; j < attackMode.Length; j++)
				{
					if (attackMode[j].type == AttackType.Ranged)
					{
						TargetCmpt target = enemy.entityTarget.target;
						if (target != null)
						{
							PeEntity peEntity = ((target.enemy == null) ? null : target.enemy.entityTarget);
							if (peEntity == null || !peEntity.Equals(base.Entity))
							{
								weapon = list[i];
								flag = true;
								break;
							}
						}
					}
					num2 = Mathf.Min(Mathf.Abs(e.DistanceXZ - attackMode[j].minRange), Mathf.Abs(e.DistanceXZ - attackMode[j].maxRange));
				}
				if (flag && weapon != null)
				{
					break;
				}
				if (num2 < num)
				{
					num = num2;
					weapon = list[i];
				}
			}
		}
		return weapon;
	}

	public bool HasAttackRanged()
	{
		if (m_Entity.Tower != null)
		{
			return true;
		}
		if (m_Equipment != null && m_Equipment.GetWeaponList().Count > 0)
		{
			List<IWeapon> weaponList = m_Equipment.GetWeaponList();
			for (int i = 0; i < weaponList.Count; i++)
			{
				AttackMode[] attackMode = weaponList[i].GetAttackMode();
				for (int j = 0; j < attackMode.Length; j++)
				{
					if (attackMode[j].type == AttackType.Ranged)
					{
						return true;
					}
				}
			}
		}
		else
		{
			if (Attacks == null || Attacks.Count <= 0)
			{
				return true;
			}
			List<IAttack> attacks = Attacks;
			for (int k = 0; k < attacks.Count; k++)
			{
				if (!(attacks[k] is BTMelee) && !(attacks[k] is BTMeleeAttack))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnEnemyEnter(Enemy enemy)
	{
		if (m_Entity.peSkEntity != null)
		{
			m_Entity.peSkEntity.DispatchEnemyEnterEvent(enemy.entityTarget);
		}
		if (enemy.entityTarget != null && enemy.entityTarget.peSkEntity != null)
		{
			enemy.entityTarget.peSkEntity.DispatchBeEnemyEnterEvent(m_Entity);
		}
	}

	private void OnEnemyExit(Enemy enemy)
	{
		if (m_Entity.peSkEntity != null)
		{
			m_Entity.peSkEntity.DispatchEnemyExitEvent(enemy.entityTarget);
		}
		if (enemy.entityTarget != null && enemy.entityTarget.peSkEntity != null)
		{
			enemy.entityTarget.peSkEntity.DispatchBeEnemyExitEvent(m_Entity);
		}
		if (enemy.entityTarget != null && enemy.entityTarget.target != null)
		{
			enemy.entityTarget.target.RemoveMelee(base.Entity);
		}
	}

	private void OnEnemyAchieve(Enemy enemy)
	{
		if (m_Animator != null)
		{
			m_Animator.SetBool("Combat", value: true);
		}
		if (m_Request != null && (!(m_Npc != null) || m_Npc.hasAnyRequest))
		{
			m_Request.Register(EReqType.Attack);
		}
		if (m_Entity.peSkEntity != null)
		{
			m_Entity.peSkEntity.DispatchEnemyAchieveEvent(enemy.entityTarget);
		}
		OnEnemyEnter(enemy);
	}

	private void OnEnemyChange(Enemy oldEnemy, Enemy newEnemy)
	{
		OnEnemyExit(oldEnemy);
		OnEnemyEnter(newEnemy);
	}

	private void OnEnemyLost(Enemy enemy)
	{
		m_FirstDamage = true;
		if (m_Animator != null)
		{
			m_Animator.SetBool("Combat", value: false);
		}
		if (m_Entity.Tower != null)
		{
			m_Entity.Tower.Target = null;
		}
		if (m_Entity.animCmpt != null && m_Entity.animCmpt.GetBool("Squat"))
		{
			m_Entity.animCmpt.SetBool("Squat", value: false);
		}
		if (m_Request != null)
		{
			m_Request.RemoveRequest(EReqType.Attack);
		}
		if (m_Entity.peSkEntity != null)
		{
			m_Entity.peSkEntity.DispatchEnemyLostEvent(enemy.entityTarget);
		}
		OnEnemyExit(enemy);
		PeNpcGroup.Instance.OnEnemyLost(enemy);
		NpcHatreTargets.Instance.OnEnemyLost(enemy.entityTarget);
		if (PeGameMgr.IsMulti && m_SkEntity.IsController() && base.Entity.Tower != null)
		{
			if (m_SkEntity._net is AiTowerNetwork)
			{
				(m_SkEntity._net as AiTowerNetwork).RequestEnemyLost();
			}
			else if (m_SkEntity._net is MapObjNetwork)
			{
				(m_SkEntity._net as MapObjNetwork).RequestEnemyLost();
			}
		}
	}

	private void OnDeath(SkEntity self, SkEntity killer)
	{
		ClearEnemy();
	}

	private void OnDamage(PeEntity entity, float damage)
	{
		if (!m_IsAddHatred || IsDeath())
		{
			return;
		}
		if (CanAddDamageHatred(entity))
		{
			AddDamageHatred(entity, damage * ((!m_FirstDamage) ? 1f : 3f));
			m_FirstDamage = false;
			if (m_Entity.HPPercent <= m_EscapeBase)
			{
				if ((base.Entity.Race == ERace.Puja || base.Entity.Race == ERace.Paja) && GetEscapeEnemy() != null && UnityEngine.Random.value <= 0.3f)
				{
					m_Escape = null;
					m_Entity.IsSeriousInjury = true;
				}
				if (!m_Entity.IsSeriousInjury && UnityEngine.Random.value <= m_EscapeProp)
				{
					SetEscape(entity);
				}
			}
			if (Enemy.IsNullOrInvalid(m_Escape))
			{
				Enemy enemy = m_Enemies.Find((Enemy ret) => ret.entityTarget != null && ret.entityTarget == entity);
				if (!Enemy.IsNullOrInvalid(enemy) && (float)enemy.ThreatInit < -1E-45f && Enemy.IsNullOrInvalid(m_Enemy))
				{
					SetEscape(entity);
				}
			}
		}
		m_Food = null;
		m_beSkillTarget = false;
		CancelInvoke("ReflashSelf");
	}

	public void OnTargetSkill(SkEntity entity)
	{
		if (m_beSkillTarget)
		{
			return;
		}
		float hatred = 5f;
		m_beSkillTarget = true;
		SkEntity caster = PEUtil.GetCaster(entity);
		if (!(caster != null))
		{
			return;
		}
		PeEntity component = caster.GetComponent<PeEntity>();
		if (!(component == m_Entity))
		{
			if (!HasHatred(component))
			{
				AddSharedHatred(component, hatred);
			}
			Invoke("ReflashSelf", 2f);
		}
	}

	private void ReflashSelf()
	{
		m_beSkillTarget = false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (m_Entity != null && m_Entity.skEntity != null)
		{
			PESkEntity pESkEntity = m_Entity.skEntity as PESkEntity;
			if (pESkEntity != null)
			{
				pESkEntity.deathEvent -= OnDeath;
			}
		}
		ClearEnemy();
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Model_Build:
		{
			BiologyViewRoot biologyViewRoot = args[1] as BiologyViewRoot;
			m_Visions = biologyViewRoot.visions;
			m_Hears = biologyViewRoot.hears;
			m_Monster = biologyViewRoot.monster;
			break;
		}
		case EMsg.Battle_HPChange:
		{
			SkEntity skEntity2 = (SkEntity)args[0];
			float num = (float)args[1];
			if (!(skEntity2 != null) || !(num < float.Epsilon))
			{
				break;
			}
			SkEntity caster = PEUtil.GetCaster(skEntity2);
			if (!(caster != null))
			{
				break;
			}
			PeEntity component = caster.GetComponent<PeEntity>();
			if (component != null)
			{
				if (m_Entity.Group != null)
				{
					m_Entity.Group.OnDamageMember(m_Entity, component, Mathf.Abs(num));
				}
				OnDamage(component, Mathf.Abs(num));
			}
			break;
		}
		case EMsg.Battle_TargetSkill:
		{
			SkEntity skEntity = (SkEntity)args[0];
			if (skEntity != null)
			{
				OnTargetSkill(skEntity);
			}
			break;
		}
		case EMsg.Skill_Interrupt:
			if (!Enemy.IsNullOrInvalid(m_Enemy) && m_Attack != null && m_Attack.IsRunning(m_Enemy) && !m_Attack.CanInterrupt())
			{
			}
			break;
		}
	}
}
