using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackWeapon), "AttackWeapon")]
public class BTAttackWeapon : BTAttackBase
{
	private const float Mortar_MaxRange = 55f;

	private const float Mortar_MinRange = 8f;

	private const float Mortar_Offset = 30f;

	private bool m_DamageBlock;

	private bool m_Attacked;

	private bool m_MortarWaitFor;

	private int m_Index;

	private float m_LastRetreatTime;

	private float m_CanAttackModeTime;

	private IWeapon m_Weapon;

	private AttackMode m_Mode;

	private Vector3 m_Local;

	private Vector3 m_RetreatLocal;

	private List<int> m_ModeIndex = new List<int>();

	private List<IWeapon> m_Weapons = new List<IWeapon>();

	private Vector3 GetRetreatPos(Vector3 TargetPos, Transform selfTrans)
	{
		Vector3 vector = selfTrans.position;
		Vector3 normalized = (vector - TargetPos).normalized;
		Vector3 vector2 = PEUtil.GetRandomPosition(vector, normalized, 5f, 10f, -75f, 75f);
		if (PEUtil.CheckPositionUnderWater(vector2) || PEUtil.CheckPositionInSky(vector2))
		{
			normalized = selfTrans.right;
			vector2 = vector + normalized * 10f;
		}
		if (AiUtil.GetNearNodePosWalkable(vector2, out var outPos))
		{
			return outPos;
		}
		return vector2;
	}

	private void SingTarget(IAimWeapon _aimWeapon, Enemy _attackenmy)
	{
		if (_aimWeapon != null && _attackenmy.CenterBone != null)
		{
			_aimWeapon.SetTarget(_attackenmy.CenterBone);
		}
		if (_aimWeapon != null && _attackenmy == null)
		{
			_aimWeapon.SetTarget(null);
		}
	}

	private bool CanAttackMode(AttackMode mode, Enemy enemy)
	{
		return enemy.SqrDistance >= mode.minRange * mode.minRange && enemy.SqrDistance <= mode.maxRange * mode.maxRange;
	}

	private AttackMode GetMode(IWeapon weapon, int index)
	{
		AttackMode[] attackMode = weapon.GetAttackMode();
		if (index < 0 || index >= attackMode.Length)
		{
			return null;
		}
		return attackMode[index];
	}

	private bool CanSelectRangedWeapon(AttackMode mode)
	{
		if (mode.frequency < 1.5f || !mode.IsInCD())
		{
			if (!base.attackEnemy.entityTarget.Equals(PeSingleton<PeCreature>.Instance.mainPlayer))
			{
				return true;
			}
			if (CanAttackMode(mode, base.attackEnemy))
			{
				m_CanAttackModeTime = Time.time;
			}
			if (Time.time - m_CanAttackModeTime < 2f)
			{
				return true;
			}
		}
		return false;
	}

	private int SwitchAttackIndex(IWeapon weapon, Enemy enemy)
	{
		int num = -1;
		m_ModeIndex.Clear();
		AttackMode[] attackMode = weapon.GetAttackMode();
		if (m_DamageBlock)
		{
			for (int i = 0; i < attackMode.Length; i++)
			{
				if (attackMode[i].type == AttackType.Melee)
				{
					m_ModeIndex.Add(i);
				}
			}
		}
		else
		{
			TargetCmpt target = base.attackEnemy.entityTarget.target;
			if (target != null)
			{
				PeEntity peEntity = target.GetAttackEnemy()?.entityTarget;
				if (peEntity == null || !peEntity.Equals(base.entity))
				{
					for (int j = 0; j < attackMode.Length; j++)
					{
						if (attackMode[j].type == AttackType.Ranged && CanSelectRangedWeapon(attackMode[j]))
						{
							m_ModeIndex.Add(j);
						}
					}
				}
			}
			if (m_ModeIndex.Count == 0)
			{
				for (int k = 0; k < attackMode.Length; k++)
				{
					if (attackMode[k].type == AttackType.Ranged && CanAttackMode(attackMode[k], base.attackEnemy) && (attackMode[k].frequency < 1.5f || !attackMode[k].IsInCD()))
					{
						m_ModeIndex.Add(k);
					}
				}
			}
			if (m_ModeIndex.Count == 0)
			{
				for (int l = 0; l < attackMode.Length; l++)
				{
					if (attackMode[l].type == AttackType.Melee && CanAttackMode(attackMode[l], base.attackEnemy) && (attackMode[l].frequency < 1.5f || !attackMode[l].IsInCD()))
					{
						m_ModeIndex.Add(l);
					}
				}
			}
			if (m_ModeIndex.Count == 0)
			{
				float num2 = float.PositiveInfinity;
				for (int m = 0; m < attackMode.Length; m++)
				{
					float num3 = Mathf.Min(Mathf.Abs(enemy.DistanceXZ - attackMode[m].minRange), Mathf.Abs(enemy.DistanceXZ - attackMode[m].maxRange));
					if (num3 < num2)
					{
						num = m;
						num2 = num3;
					}
				}
			}
		}
		if (num == -1 && m_ModeIndex.Count > 0)
		{
			num = m_ModeIndex[Random.Range(0, m_ModeIndex.Count)];
		}
		return num;
	}

	private Vector3 GetLocalCenterPos()
	{
		return base.attackEnemy.position + m_Local + Vector3.up * base.entity.maxHeight * 0.5f;
	}

	private Vector3 GetLocalPos(Enemy e, AttackMode attack)
	{
		if (!PEUtil.IsBlocked(e.entity, e.entityTarget) && !Stucking())
		{
			m_Local = Vector3.zero;
		}
		else if (m_Local == Vector3.zero || PEUtil.IsBlocked(e.entityTarget, GetLocalCenterPos()))
		{
			Vector3 local = Vector3.zero;
			for (int i = 0; i < 5; i++)
			{
				Vector3 randomPositionOnGround = PEUtil.GetRandomPositionOnGround(e.position, attack.minRange, attack.maxRange);
				Vector3 vector = randomPositionOnGround + Vector3.up * base.entity.maxHeight * 0.5f;
				if (!PEUtil.IsBlocked(e.entityTarget, vector))
				{
					local = randomPositionOnGround - e.position;
					break;
				}
			}
			m_Local = local;
		}
		return m_Local;
	}

	private Vector3 GetMovePos(Enemy e)
	{
		return e.position + m_Local;
	}

	private void StandFromSquat()
	{
		if (GetBool("Squat"))
		{
			SetBool("Squat", value: false);
		}
	}

	private void MortarTakeOff()
	{
		if (base.Weapon != null && !base.Weapon.Equals(null) && GetBool("Mortar"))
		{
			base.Weapon.HoldWeapon(hold: false);
		}
	}

	private bool CanAttackWeaponMotar(IWeapon weapon, Enemy enemy)
	{
		PEEquipment pEEquipment = weapon as PEEquipment;
		if (pEEquipment.m_ItemObj == null || pEEquipment.m_ItemObj.protoId != 1143)
		{
			return true;
		}
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			if (enemy.Distance > attackMode[i].minRange)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanAttackWeapon(IWeapon weapon, Enemy enemy)
	{
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			if (enemy.Distance > attackMode[i].minRange && enemy.Distance < attackMode[i].maxRange)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsRangedWeapon(IWeapon weapon)
	{
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			if (attackMode[i].type == AttackType.Ranged)
			{
				return true;
			}
		}
		return false;
	}

	private float GetWeaponScale(IWeapon weapon, Enemy enemy)
	{
		float result = float.PositiveInfinity;
		AttackMode[] attackMode = weapon.GetAttackMode();
		for (int i = 0; i < attackMode.Length; i++)
		{
			result = Mathf.Min(Mathf.Abs(enemy.Distance - attackMode[i].minRange), Mathf.Abs(enemy.Distance - attackMode[i].maxRange));
		}
		return result;
	}

	private IWeapon GetWeapon()
	{
		IWeapon weapon = null;
		m_Weapons.Clear();
		m_Weapons = base.entity.target.GetCanUseWeaponList(base.attackEnemy);
		for (int i = 0; i < m_Weapons.Count; i++)
		{
			if (CanAttackWeapon(m_Weapons[i], base.attackEnemy))
			{
				weapon = m_Weapons[i];
				if (IsRangedWeapon(m_Weapons[i]))
				{
					break;
				}
			}
		}
		if (weapon == null)
		{
			float num = float.PositiveInfinity;
			for (int j = 0; j < m_Weapons.Count; j++)
			{
				if (CanAttackWeaponMotar(m_Weapons[j], base.attackEnemy))
				{
					float weaponScale = GetWeaponScale(m_Weapons[j], base.attackEnemy);
					if (weaponScale < num)
					{
						num = weaponScale;
						weapon = m_Weapons[j];
					}
				}
			}
		}
		return weapon;
	}

	private bool CanHoldWeapon(IWeapon weapon)
	{
		PEEquipment pEEquipment = weapon as PEEquipment;
		if (pEEquipment.m_ItemObj != null && pEEquipment.m_ItemObj.protoId == 1143)
		{
			m_MortarWaitFor = true;
			return base.attackEnemy.Distance > 8f && base.attackEnemy.Distance < 30f && base.attackEnemy.CanHoldWeapon();
		}
		m_MortarWaitFor = false;
		return base.attackEnemy.CanHoldWeapon();
	}

	private BehaveResult Tick(Tree sender)
	{
		if (m_Attacked)
		{
			if (base.Weapon == null || base.Weapon.Equals(null) || base.Weapon.AttackEnd(m_Index))
			{
				m_Attacked = false;
				return BehaveResult.Success;
			}
			bool flag = false;
			if (base.attackEnemy != null)
			{
				float minRange = m_Mode.minRange;
				float maxRange = m_Mode.maxRange;
				float sqrDistanceLogic = base.attackEnemy.SqrDistanceLogic;
				Vector3 direction = base.attackEnemy.Direction;
				bool flag2 = !m_Mode.ignoreTerrain && PEUtil.IsBlocked(base.entity, base.attackEnemy.entityTarget);
				bool flag3 = sqrDistanceLogic <= maxRange * maxRange && sqrDistanceLogic >= minRange * minRange;
				bool flag4 = PEUtil.IsScopeAngle(direction, base.transform.forward, Vector3.up, m_Mode.minAngle, m_Mode.maxAngle);
				flag = flag3 && flag4 && !flag2;
			}
			if (flag)
			{
				WeaponAttack(base.Weapon, base.attackEnemy, m_Index);
			}
			return BehaveResult.Running;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if ((base.Weapon == null || base.Weapon.Equals(null) || !CanAttackWeapon(base.Weapon, base.attackEnemy)) && !base.entity.motionEquipment.IsSwitchWeapon())
		{
			IWeapon weapon = GetWeapon();
			if (weapon != null && !weapon.Equals(null))
			{
				if (base.Weapon == null || base.Weapon.Equals(null))
				{
					if (!weapon.HoldReady && CanHoldWeapon(weapon))
					{
						weapon.HoldWeapon(hold: true);
					}
				}
				else if (!base.Weapon.Equals(weapon))
				{
					m_Index = -1;
					base.entity.motionEquipment.SwitchHoldWeapon(base.Weapon, weapon);
				}
			}
		}
		if (base.attackEnemy.GroupAttack == EAttackGroup.Threat)
		{
			return BehaveResult.Failure;
		}
		if (base.Weapon == null || base.Weapon.Equals(null) || !base.Weapon.HoldReady)
		{
			if (base.Weapon == null)
			{
				if (m_MortarWaitFor)
				{
					Vector3 vector = base.position - base.attackEnemy.position;
					vector.y = 0f;
					vector = vector.normalized * 30f;
					MoveToPosition(base.attackEnemy.position + vector, SpeedState.Run);
				}
				else
				{
					StopMove();
				}
			}
			else
			{
				float num = float.PositiveInfinity;
				float num2 = 0f;
				AttackMode[] attackMode = base.Weapon.GetAttackMode();
				for (int i = 0; i < attackMode.Length; i++)
				{
					num = Mathf.Min(num, attackMode[i].minRange);
					num2 = Mathf.Max(num2, attackMode[i].maxRange);
				}
				Vector3 vector2 = base.position - base.attackEnemy.position;
				vector2.y = 0f;
				vector2 = vector2.normalized * (num + num2) * 0.7f;
				MoveToPosition(base.attackEnemy.position + vector2, SpeedState.Run);
			}
		}
		else
		{
			m_DamageBlock = PEUtil.IsDamageBlock(base.entity);
			m_Index = SwitchAttackIndex(base.Weapon, base.attackEnemy);
			if (m_Index >= 0 && m_Index < base.Weapon.GetAttackMode().Length)
			{
				m_Mode = base.Weapon.GetAttackMode()[m_Index];
				if (base.attackEnemy.entityTarget.target != null)
				{
					if (m_Mode.type == AttackType.Melee)
					{
						base.attackEnemy.entityTarget.target.AddMelee(base.entity);
					}
					else
					{
						base.attackEnemy.entityTarget.target.RemoveMelee(base.entity);
					}
				}
				float minRange2 = m_Mode.minRange;
				float maxRange2 = m_Mode.maxRange;
				float sqrDistanceLogic2 = base.attackEnemy.SqrDistanceLogic;
				Vector3 direction2 = base.attackEnemy.Direction;
				bool flag5 = !m_Mode.ignoreTerrain && PEUtil.IsBlocked(base.entity, base.attackEnemy.entityTarget);
				bool flag6 = sqrDistanceLogic2 <= maxRange2 * maxRange2 && sqrDistanceLogic2 >= minRange2 * minRange2;
				bool flag7 = PEUtil.IsScopeAngle(direction2, base.transform.forward, Vector3.up, m_Mode.minAngle, m_Mode.maxAngle);
				bool flag8 = flag6 && flag7 && !flag5;
				bool flag9 = false;
				IAimWeapon aimWeapon = base.Weapon as IAimWeapon;
				bool flag10 = m_Mode.type == AttackType.Melee || aimWeapon == null || aimWeapon.Aimed;
				if (aimWeapon != null)
				{
					if (m_Mode.type == AttackType.Ranged)
					{
						aimWeapon.SetAimState(aimState: true);
						aimWeapon.SetTarget(base.attackEnemy.CenterBone);
					}
					else
					{
						aimWeapon.SetAimState(aimState: false);
						aimWeapon.SetTarget(null);
					}
				}
				if (m_DamageBlock)
				{
					m_Attacked = true;
					WeaponAttack(base.Weapon, base.attackEnemy, m_Index, Block45Man.self);
				}
				else
				{
					m_Local = GetLocalPos(base.attackEnemy, m_Mode);
					m_Local.y = 0f;
					float num3 = minRange2;
					if (m_Mode.type == AttackType.Ranged)
					{
						num3 += Mathf.Lerp(minRange2, maxRange2, 0.2f);
					}
					if (sqrDistanceLogic2 > maxRange2 * maxRange2 || flag5)
					{
						StandFromSquat();
						if (!flag5)
						{
							MortarTakeOff();
						}
						MoveToPosition(GetMovePos(base.attackEnemy), SpeedState.Run);
					}
					else if (sqrDistanceLogic2 < num3 * num3)
					{
						StandFromSquat();
						if (Time.time - m_LastRetreatTime > 3f)
						{
							m_LastRetreatTime = Time.time;
							m_RetreatLocal = GetRetreatPos(base.attackEnemy.position, base.transform);
						}
						if (!GetBool("Mortar") && m_RetreatLocal != Vector3.zero && PEUtil.SqrMagnitude(m_RetreatLocal, base.position) > 0.25f)
						{
							Vector3 vector3 = m_RetreatLocal - base.position;
							Vector3 direction3 = Vector3.ProjectOnPlane(vector3, Vector3.up);
							if (!Physics.Raycast(base.entity.centerPos, direction3, 2f, 2193408))
							{
								flag9 = true;
								FaceDirection(direction2);
								MoveDirection(vector3, SpeedState.Retreat);
							}
							else
							{
								FaceDirection(direction2);
								MoveDirection(Vector3.zero);
							}
						}
						else
						{
							StopMove();
						}
					}
					else
					{
						StopMove();
					}
					if (flag6 && !flag5)
					{
						if (flag7 && !flag9)
						{
							FaceDirection(Vector3.zero);
						}
						else
						{
							StandFromSquat();
							FaceDirection(direction2);
						}
					}
					if (flag8 && flag10 && !base.Weapon.IsInCD(m_Index))
					{
						WeaponAttack(base.Weapon, base.attackEnemy, m_Index);
						m_Attacked = true;
					}
				}
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			m_Attacked = false;
		}
	}
}
