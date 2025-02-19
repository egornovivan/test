using System;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime.Action;
using ItemAsset;
using Pathfinding;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class Enemy : IDisposable
{
	public const float ThreatIntervalDistance = 0.5f;

	public const float ThreatIntervalTime = 0.5f;

	public const float AttackBorderValue = 200f;

	public const float DamageHatredScale = 0.6f;

	public const float FollowUpDistance = 80f;

	public const float FollowUpDistanceInterval = 2f;

	private PeEntity m_Entity;

	private PeEntity m_EntityTarget;

	private float m_ThreatExtra;

	private float m_ThreatDistance;

	private float m_StartTime;

	private float m_HoldWeaponDelayTime;

	private float m_LastFollowTime;

	private float m_LastAttacktime;

	private float m_LastCanMoveTime;

	private float m_LastCanAttacktime;

	private Vector3 m_LastAttackPos;

	public bool m_Ignore;

	private EEnemySpace m_Space;

	private EEnemyMoveDir m_MoveDir;

	public EAttackGroup m_GroupAttack = EAttackGroup.Attack;

	private float m_Hatred;

	private float m_LastDamagetime;

	private float m_Threat;

	private int m_ThreatInit;

	private float m_ThreatShared;

	private float m_ThreatDamage;

	private float m_ThreatSharedDamage;

	private float m_SqrDistanceXZ;

	private float m_SqrDistance;

	private float m_DistanceXZ;

	private float m_Distance;

	private float m_AttackDistance;

	private Vector3 m_Direction;

	private Vector3 m_DirectionXZ;

	private Vector3 m_ClosetPoint;

	private Vector3 m_FarthestPoint;

	private bool m_Inside;

	private bool m_InHeight;

	private Pathfinding.Path m_Path;

	private IAttack m_Attack;

	public EEnemyMoveDir MoveDir => m_MoveDir;

	public EAttackGroup GroupAttack
	{
		get
		{
			return m_GroupAttack;
		}
		set
		{
			m_GroupAttack = value;
		}
	}

	public bool IsOnLand => !IsInWater && !IsInAir;

	public bool IsInWater => IsDeepWater || IsShallowWater;

	public bool IsDeepWater => m_Space == EEnemySpace.Water;

	public bool IsShallowWater => m_Space == EEnemySpace.WaterSurface && (m_EntityTarget == null || m_EntityTarget.motionMove == null || !m_EntityTarget.motionMove.grounded);

	public bool IsInAir => m_Space == EEnemySpace.Sky;

	public float Hatred => m_Hatred;

	public float LastDamagetime => m_LastDamagetime;

	public float Threat => m_Threat;

	public int ThreatInit => m_ThreatInit;

	public float ThreatShared
	{
		get
		{
			return m_ThreatShared;
		}
		set
		{
			m_ThreatShared = value;
		}
	}

	public float ThreatDamage => m_ThreatDamage;

	public float ThreatSharedDamage => m_ThreatSharedDamage;

	public float SqrDistanceXZ => m_SqrDistanceXZ;

	public float SqrDistance => m_SqrDistance;

	public float DistanceXZ => m_DistanceXZ;

	public float Distance => m_Distance;

	public float AttackDistance => m_Distance;

	public Vector3 Direction => m_Direction;

	public Vector3 DirectionXZ => m_DirectionXZ;

	public Vector3 closetPoint => m_ClosetPoint;

	public Vector3 farthestPoint => m_FarthestPoint;

	public bool Inside => m_Inside && m_DirectionXZ.sqrMagnitude < 0.25f;

	public Vector3 velocity => (!(m_EntityTarget != null)) ? Vector3.zero : m_EntityTarget.velocity;

	public float SqrDistanceLogic
	{
		get
		{
			if (m_Entity.Field == MovementField.Land && m_InHeight)
			{
				return m_SqrDistanceXZ;
			}
			return m_SqrDistance;
		}
	}

	public IAttack Attack
	{
		get
		{
			return m_Attack;
		}
		set
		{
			m_Attack = value;
		}
	}

	public SkEntity skTarget => m_EntityTarget.skEntity;

	public PeEntity entity => m_Entity;

	public PeEntity entityTarget => m_EntityTarget;

	public bool isValid
	{
		get
		{
			if (m_Entity == null || m_EntityTarget == null)
			{
				return false;
			}
			if (!m_Entity.gameObject.activeSelf || !m_EntityTarget.gameObject.activeSelf)
			{
				return false;
			}
			if (m_EntityTarget.IsDeath() || !m_EntityTarget.hasView)
			{
				return false;
			}
			return true;
		}
	}

	public bool isCarrierAndSky
	{
		get
		{
			if (m_EntityTarget != null && m_EntityTarget.skEntity is CreationSkEntity && (!Physics.Raycast(m_EntityTarget.position, Vector3.down, out var hitInfo, 128f, 71680) || Mathf.Abs(m_EntityTarget.position.y - hitInfo.point.y) > 10f))
			{
				return true;
			}
			return false;
		}
	}

	public bool canAttacked => isValid && !m_Ignore && Hatred > float.Epsilon && CanFollow() && CanAttack();

	public bool canFollowed => isValid && !m_Ignore && Hatred > float.Epsilon && !CanAttack();

	public bool canEscaped => isValid && (float)ThreatInit < -1E-45f;

	public bool canAfraid
	{
		get
		{
			_ = isValid;
			return false;
		}
	}

	public bool canThreat
	{
		get
		{
			_ = isValid;
			return false;
		}
	}

	public Transform trans => (!(m_EntityTarget != null)) ? null : m_EntityTarget.transform;

	public Transform modelTrans => (!(m_EntityTarget != null)) ? null : m_EntityTarget.tr;

	public Vector3 position => (!(m_EntityTarget != null)) ? Vector3.zero : m_EntityTarget.position;

	public Vector3 centerPos => (!(m_EntityTarget != null)) ? Vector3.zero : m_EntityTarget.centerPos;

	public Quaternion rotation => (!(m_EntityTarget != null)) ? Quaternion.identity : m_EntityTarget.rotation;

	public Transform CenterBone => (!(m_EntityTarget != null)) ? null : m_EntityTarget.centerBone;

	public float radius => (!(m_EntityTarget != null)) ? 0f : m_EntityTarget.maxRadius;

	public float height => (!(m_EntityTarget != null)) ? 0f : m_EntityTarget.maxHeight;

	public bool isRagdoll => m_EntityTarget != null && m_EntityTarget.isRagdoll;

	public Enemy(PeEntity argSelf, PeEntity argTarget, float argHatred = 0f)
	{
		m_Entity = argSelf;
		m_EntityTarget = argTarget;
		m_StartTime = Time.time;
		m_HoldWeaponDelayTime = UnityEngine.Random.Range(0f, 1f);
		m_LastFollowTime = -1000f;
		m_LastAttackPos = m_Entity.position;
		m_ThreatExtra = 0f;
		m_LastDamagetime = Time.time;
		m_LastAttacktime = Time.time;
		m_LastCanAttacktime = Time.time;
		m_ThreatInit = GetThreatInit(m_Entity, m_EntityTarget);
		AddHatred(argHatred);
		if (m_EntityTarget != null && m_EntityTarget.peSkEntity != null)
		{
			m_EntityTarget.peSkEntity.onHpReduce += OnDamageTarget;
		}
	}

	public static bool IsNullOrInvalid(Enemy enemy)
	{
		return enemy == null || !enemy.isValid;
	}

	private void OnPathComplete(Pathfinding.Path _p)
	{
		ABPath aBPath = _p as ABPath;
		aBPath.Claim(this);
		if (aBPath.error)
		{
			aBPath.Release(this);
			Debug.LogError("error!!");
		}
		if (m_Path != null)
		{
			m_Path.Release(this);
		}
		m_Path = aBPath;
	}

	private IEnumerator AssessPath()
	{
		while (true)
		{
			if (isValid)
			{
				ABPath.Construct(m_Entity.position, m_EntityTarget.position, OnPathComplete);
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void Update()
	{
		if (!isValid)
		{
			return;
		}
		UpdateMoveDir();
		UpdateHatred();
		UpdateFollow();
		UpdateCanMove();
		UpdateSpace();
		UpdateCanAttack();
		m_ClosetPoint = m_Entity.tr.TransformPoint(m_Entity.bounds.ClosestPoint(m_Entity.tr.InverseTransformPoint(m_EntityTarget.position)));
		m_FarthestPoint = m_EntityTarget.tr.TransformPoint(m_EntityTarget.bounds.ClosestPoint(m_EntityTarget.tr.InverseTransformPoint(m_Entity.position)));
		m_Direction = m_EntityTarget.position - m_Entity.position;
		m_DirectionXZ = Vector3.ProjectOnPlane(m_Direction, Vector3.up);
		m_Distance = PEUtil.Magnitude(m_ClosetPoint, m_FarthestPoint);
		m_SqrDistance = PEUtil.SqrMagnitude(m_ClosetPoint, m_FarthestPoint);
		Vector3 vector = Vector3.ProjectOnPlane(m_FarthestPoint - m_ClosetPoint, Vector3.up);
		m_DistanceXZ = PEUtil.Magnitude(m_ClosetPoint, m_FarthestPoint, is3D: false);
		m_SqrDistanceXZ = PEUtil.SqrMagnitude(m_ClosetPoint, m_FarthestPoint, is3D: false);
		float num = m_EntityTarget.position.y - 0.5f;
		float num2 = m_EntityTarget.position.y + m_EntityTarget.maxHeight + 0.5f;
		float y = m_Entity.position.y;
		float num3 = m_Entity.position.y + m_Entity.maxHeight;
		m_InHeight = (y <= num2 && y >= num) || (num3 <= num2 && num3 >= num);
		if (Vector3.Dot(m_DirectionXZ.normalized, vector.normalized) >= 0f)
		{
			m_Inside = false;
			return;
		}
		m_SqrDistanceXZ = 0f;
		if (Mathf.Abs(m_DirectionXZ.x) < m_Entity.bounds.extents.x || Mathf.Abs(m_DirectionXZ.z) < m_Entity.bounds.extents.z)
		{
			m_Inside = true;
		}
		else
		{
			m_Inside = false;
		}
	}

	private void UpdateCanMove()
	{
		if (m_LastCanMoveTime < float.Epsilon && m_Entity.attackEnemy != null && m_Entity.attackEnemy.Equals(this) && m_Entity.Stucking())
		{
			m_LastCanMoveTime = Time.time;
		}
		if (m_LastCanMoveTime > float.Epsilon && m_Distance < 2f)
		{
			m_LastCanMoveTime = 0f;
		}
	}

	private void UpdateSpace()
	{
		if (PEUtil.CheckPositionUnderWater(m_EntityTarget.peTrans.centerUp))
		{
			m_Space = EEnemySpace.Water;
		}
		else if (PEUtil.CheckPositionUnderWater(m_EntityTarget.peTrans.position))
		{
			m_Space = EEnemySpace.WaterSurface;
		}
		else if (!Physics.Raycast(m_EntityTarget.position + Vector3.up * 0.1f, Vector3.down, m_Entity.maxHeight, 71680) && !Physics.Raycast(m_EntityTarget.position + m_EntityTarget.tr.up * m_EntityTarget.maxHeight, -m_EntityTarget.tr.up, m_EntityTarget.maxHeight, 71680))
		{
			m_Space = EEnemySpace.Sky;
		}
		else
		{
			m_Space = EEnemySpace.Land;
		}
	}

	private void UpdateMoveDir()
	{
		Vector3 rhs = Vector3.ProjectOnPlane(m_EntityTarget.velocity, Vector3.up);
		if (rhs.sqrMagnitude < 0.0225f)
		{
			m_MoveDir = EEnemyMoveDir.Stop;
		}
		else if (Vector3.Dot(m_DirectionXZ, rhs) > 0f)
		{
			m_MoveDir = EEnemyMoveDir.Away;
		}
		else
		{
			m_MoveDir = EEnemyMoveDir.Close;
		}
	}

	private void UpdateThreat()
	{
		float num = m_ThreatDamage + m_ThreatSharedDamage;
		if (HasAttackRanged() && isReadyAttack())
		{
			num += 1000f;
			if (m_EntityTarget.Tower != null)
			{
				num += 1000f;
			}
		}
		m_Threat = (float)m_ThreatInit + num + m_ThreatExtra + m_ThreatShared;
	}

	public void UpdateExtra()
	{
		if (m_Entity == null || m_EntityTarget == null)
		{
			return;
		}
		if (SpecialHatred.IsHaveSpecialHatred(m_Entity, m_EntityTarget, out var ignoreOrHighHatred))
		{
			switch (ignoreOrHighHatred)
			{
			case 1:
				m_Ignore = true;
				m_ThreatExtra = 0f;
				break;
			case 2:
				m_Ignore = false;
				m_ThreatExtra = 10000f;
				break;
			}
		}
		else
		{
			m_Ignore = false;
			m_ThreatExtra = 0f;
		}
	}

	public void UpdateFollow()
	{
		if (isValid)
		{
			float num = ((m_Entity.Field != MovementField.Sky) ? 128f : 256f);
			if (m_Entity.ProtoID == 71)
			{
				num = 35f;
			}
			if (m_LastFollowTime < float.Epsilon && PEUtil.SqrMagnitudeH(m_Entity.position, m_LastAttackPos) > num * num && Time.time - m_LastDamagetime > 15f)
			{
				m_LastFollowTime = Time.time;
			}
			if (m_LastFollowTime < float.Epsilon && m_EntityTarget.target != null && m_EntityTarget.target.GetEscapeEnemy() != null && Time.time - m_LastDamagetime > 15f && Time.time - m_LastAttacktime > 15f)
			{
				m_LastFollowTime = Time.time;
			}
			if (m_LastFollowTime > float.Epsilon && Time.time - m_LastFollowTime > 5f && SqrDistanceXZ < 25f)
			{
				m_LastFollowTime = -1000f;
				m_LastAttackPos = m_Entity.position;
			}
		}
	}

	public bool CheckCanAttack()
	{
		if (!isValid)
		{
			return false;
		}
		if (!IsFollowPolarShield())
		{
			return false;
		}
		if (!m_Entity.target.CanAttack())
		{
			return false;
		}
		if (m_EntityTarget != null && m_Entity.target != null && m_EntityTarget != null)
		{
			if (m_Entity.Tower != null)
			{
				return m_Entity.Tower.EstimatedAttack(m_EntityTarget.centerPos, m_EntityTarget.transform);
			}
			if (m_Entity.motionEquipment != null && m_Entity.motionEquipment.GetWeaponList().Count > 0)
			{
				return m_Entity.target.GetCanUseWeaponList(this).Count > 0;
			}
			if (m_Entity.target.Attacks != null && m_Entity.target.Attacks.Count > 0)
			{
				return m_Entity.target.Attacks.Find((IAttack ret) => ret.CanAttack(this)) != null;
			}
			if (m_Entity.Field == MovementField.water && m_Space != EEnemySpace.Water && m_Space != EEnemySpace.WaterSurface)
			{
				return false;
			}
			if (m_Entity.Field == MovementField.Sky && m_Space == EEnemySpace.Water)
			{
				return false;
			}
			if (m_Entity.Field == MovementField.Land && m_Space != EEnemySpace.Land)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public bool HasAttackRanged()
	{
		if (m_Entity.Tower != null)
		{
			return true;
		}
		if (m_Entity.motionEquipment != null && m_Entity.motionEquipment.GetWeaponList().Count > 0)
		{
			List<IWeapon> weaponList = m_Entity.motionEquipment.GetWeaponList();
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
			if (m_Entity.target.Attacks == null || m_Entity.target.Attacks.Count <= 0)
			{
				return true;
			}
			List<IAttack> attacks = m_Entity.target.Attacks;
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

	private bool CanAttackMode(AttackMode mode)
	{
		if (Distance < mode.minRange || Distance > mode.maxRange)
		{
			return false;
		}
		return mode.ignoreTerrain || !PEUtil.IsBlocked(m_Entity, m_EntityTarget);
	}

	private bool isReadyAttack()
	{
		if (m_Entity.Tower != null)
		{
			return m_Entity.Tower.EstimatedAttack(m_EntityTarget.centerPos, m_EntityTarget.transform);
		}
		if (m_Entity.motionEquipment != null && m_Entity.motionEquipment.Weapon != null)
		{
			AttackMode[] attackMode = m_Entity.motionEquipment.Weapon.GetAttackMode();
			for (int i = 0; i < attackMode.Length; i++)
			{
				if (attackMode[i].type == AttackType.Ranged && CanAttackMode(attackMode[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void UpdateCanAttack()
	{
		if (CheckCanAttack())
		{
			m_LastCanAttacktime = Time.time;
		}
	}

	public void UpdateHatredDistance()
	{
		if (isValid && m_Threat > float.Epsilon)
		{
			if ((m_Entity.Field != MovementField.water && m_Entity.Field != MovementField.Sky) || !m_Entity.IsAttacking)
			{
				float num = Mathf.Max(0f, 1225f - m_SqrDistanceXZ);
				if (entityTarget.target != null && !entityTarget.target.ContainsMelee(entity) && !HasAttackRanged())
				{
					num = Mathf.Lerp(num, 0f, (float)entityTarget.target.GetMeleeCount() * 0.333f);
				}
				m_ThreatDistance = num;
			}
		}
		else
		{
			m_ThreatDistance = 0f;
		}
	}

	private void UpdateHatred()
	{
		m_ThreatInit = GetThreatInit(m_Entity, m_EntityTarget);
		UpdateThreat();
		UpdateExtra();
		UpdateHatredDistance();
		if (CanBeThreat())
		{
			m_GroupAttack = EAttackGroup.Threat;
		}
		else
		{
			m_GroupAttack = EAttackGroup.Attack;
		}
		if ((float)m_ThreatInit < -1E-45f)
		{
			m_Hatred = 0f;
		}
		else
		{
			m_Hatred = m_Threat * 0.5f + m_ThreatDistance * 0.5f;
		}
	}

	public bool CanBeThreat()
	{
		if (entity.NpcCmpt == null)
		{
			return !HasAttackRanged() && entityTarget.target != null && entityTarget.target.GetMeleeCount() > 2 && !entityTarget.target.ContainsMelee(entity) && entity.target != null && Equals(entity.target.GetAttackEnemy());
		}
		int num = ((!(entityTarget != null) || entityTarget.monsterProtoDb == null || entityTarget.monsterProtoDb.AtkDb == null) ? 3 : entityTarget.monsterProtoDb.AtkDb.mNumber);
		if ((!HasAttackRanged() && entityTarget.target != null && entityTarget.target.GetMeleeCount() > num - 1 && !entityTarget.target.ContainsMelee(entity) && entity.target != null && Equals(entity.target.GetAttackEnemy())) || !SelectItem.MatchEnemyAttack(entity, entityTarget))
		{
			return true;
		}
		return false;
	}

	public bool CanHoldWeapon()
	{
		return Time.time - m_StartTime > m_HoldWeaponDelayTime;
	}

	public void AddHatred(float damage)
	{
		if (isValid)
		{
			m_LastFollowTime = -1000f;
			m_LastAttackPos = m_Entity.position;
			m_ThreatSharedDamage += damage;
		}
	}

	public void OnDamage(float damage)
	{
		if (isValid)
		{
			m_LastDamagetime = Time.time;
			m_LastFollowTime = -1000f;
			m_LastCanMoveTime = 0f;
			m_LastAttackPos = m_Entity.position;
			m_ThreatDamage += damage;
			m_AttackDistance = Mathf.Max(m_AttackDistance, Vector3.Distance(m_Entity.position, m_EntityTarget.position));
		}
	}

	private void OnDamageTarget(SkEntity caster, float damage)
	{
		SkEntity caster2 = PEUtil.GetCaster(caster);
		if (caster2 != null && m_Entity != null && m_Entity.skEntity == caster2)
		{
			m_LastAttacktime = Time.time;
		}
	}

	private MovementField GetMovementLimiter(Motion_Move mover)
	{
		if (mover == null)
		{
			return MovementField.None;
		}
		if (mover is Motion_Move_Motor)
		{
			return (mover as Motion_Move_Motor).Field;
		}
		if (mover is Motion_Move_Human)
		{
			return MovementField.Land;
		}
		return MovementField.None;
	}

	private bool CanAddHatred(PeEntity e1, PeEntity e2)
	{
		int src = Convert.ToInt32(e1.GetAttribute(AttribType.DamageID));
		int dst = Convert.ToInt32(e2.GetAttribute(AttribType.DamageID));
		return DamageData.GetValue(src, dst) > 0;
	}

	public bool matchAttackType(PeEntity self, PeEntity target)
	{
		if (self == null || target == null)
		{
			return false;
		}
		if (self.NpcCmpt == null || target.entityProto == null)
		{
			return true;
		}
		MonsterProtoDb.Item item = MonsterProtoDb.Get(target.entityProto.protoId);
		if (item == null)
		{
			return true;
		}
		return item.attackType switch
		{
			0 => false, 
			1 => true, 
			-1 => GetAttackValue(target) >= GetAttackValue(self) * TargetCmpt.Combatpercent, 
			_ => true, 
		};
	}

	private float GetAttackValue(PeEntity entity)
	{
		return TargetCmpt.HPpercent * entity.GetAttribute(AttribType.Hp) + TargetCmpt.Atkpercent * entity.GetAttribute(AttribType.Atk) + TargetCmpt.Defpercent * entity.GetAttribute(AttribType.Def);
	}

	private int GetThreatInit(PeEntity e1, PeEntity e2)
	{
		if (matchAttackType(e1, e2))
		{
			int src = (int)e1.GetAttribute(AttribType.CampID);
			int dst = (int)e2.GetAttribute(AttribType.CampID);
			return ThreatData.GetInitData(src, dst);
		}
		return 0;
	}

	public override bool Equals(object obj)
	{
		if (obj is Enemy enemy && enemy.skTarget != null)
		{
			return enemy.skTarget.Equals(skTarget);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public bool CanAttack()
	{
		if (m_EntityTarget.vehicle != null)
		{
			return false;
		}
		return Time.time - m_LastCanAttacktime < 10f;
	}

	public bool CanFollow()
	{
		return m_LastFollowTime < float.Epsilon;
	}

	private bool CanSelectForReputation()
	{
		if (m_EntityTarget == null || m_Entity == null)
		{
			return false;
		}
		if (m_ThreatDamage > float.Epsilon)
		{
			return PEUtil.CanDamageReputation(m_Entity, m_EntityTarget);
		}
		return PEUtil.CanAttackReputation(m_Entity, m_EntityTarget);
	}

	private bool Conflict()
	{
		if (m_EntityTarget == null || m_Entity == null)
		{
			return false;
		}
		int srcID = (int)m_Entity.GetAttribute(AttribType.DefaultPlayerID);
		int dstID = (int)m_EntityTarget.GetAttribute(AttribType.DefaultPlayerID);
		return CanSelectForReputation() && Singleton<ForceSetting>.Instance.Conflict(srcID, dstID);
	}

	public bool CanDelete()
	{
		if (m_Entity == null || m_EntityTarget == null || m_EntityTarget.IsDeath())
		{
			return true;
		}
		if (!Conflict())
		{
			return true;
		}
		if (m_LastFollowTime > float.Epsilon && Time.time - m_LastFollowTime > 20f)
		{
			return true;
		}
		if (Time.time - m_LastCanAttacktime > 20f)
		{
			return true;
		}
		if (Time.time - m_LastDamagetime > 15f && Time.time - m_LastAttacktime > 15f && m_Entity.NpcCmpt != null)
		{
			return true;
		}
		return false;
	}

	public bool IsFollowPolarShield()
	{
		if (m_Entity.monster == null)
		{
			return true;
		}
		if (m_Entity.commonCmpt != null && m_Entity.commonCmpt.TDObj != null)
		{
			return true;
		}
		return !PolarShield.IsInsidePolarShield(m_EntityTarget.position, m_Entity.monster.InjuredLevel);
	}

	public void Dispose()
	{
		if (m_EntityTarget != null && m_EntityTarget.peSkEntity != null)
		{
			m_EntityTarget.peSkEntity.onHpReduce -= OnDamageTarget;
		}
	}
}
