using System.Collections.Generic;
using ItemAsset;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSelectAttackWeapon), "SelectAttackWeapon")]
public class BTSelectAttackWeapon : BTNormal
{
	private const float SpritTime = 5f;

	private const float RunTime = 3f;

	private bool m_Attacked;

	private int m_Index;

	private float m_LastSwitchTime;

	private float m_LastAttackTime;

	private float m_StartAttackTime;

	private float m_LastRetreatTime;

	private float m_LastChangeTime;

	private float m_StartDefenceTime;

	private IWeapon m_Weapon;

	private AttackMode m_Mode;

	private Vector3 m_Local;

	private Vector3 m_RetreatLocal;

	private Vector3 m_ChangeLocal;

	private List<int> m_ModeIndex = new List<int>();

	private float m_StartChaseTime;

	private Vector3 GetRetreatPos(Vector3 TargetPos, Transform selfTrans, float minr, float maxr)
	{
		Vector3 vector = selfTrans.position;
		Vector3 normalized = (vector - TargetPos).normalized;
		Vector3 vector2 = PEUtil.GetRandomPosition(selfTrans.position, normalized, minr, maxr, -90f, 90f);
		if (PEUtil.CheckPositionUnderWater(vector2) || PEUtil.CheckPositionInSky(vector2))
		{
			normalized = ((!((double)Random.value > 0.5)) ? (-selfTrans.right) : selfTrans.right);
			vector2 = vector + normalized * 20f;
		}
		return vector2;
	}

	private bool CanAttack(IWeapon weapon, int index)
	{
		AttackMode[] attackMode = weapon.GetAttackMode();
		if (index < 0 || index >= attackMode.Length)
		{
			return false;
		}
		AttackMode attackMode2 = attackMode[index];
		return attackMode2.ignoreTerrain || !PEUtil.IsBlocked(base.entity, base.selectattackEnemy.entityTarget);
	}

	private int SwitchAttackIndex(IWeapon weapon)
	{
		AttackMode[] attackMode = weapon.GetAttackMode();
		if (attackMode.Length == 0)
		{
			return -1;
		}
		m_ModeIndex.Clear();
		int num = attackMode.Length;
		if (base.entity.Group != null)
		{
			for (int i = 0; i < num; i++)
			{
				AttackMode attackMode2 = attackMode[i];
				if (!attackMode2.IsInCD() && attackMode2.type == AttackType.Ranged)
				{
					m_ModeIndex.Add(i);
				}
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				AttackMode attackMode3 = attackMode[j];
				if (!attackMode3.IsInCD() && base.selectattackEnemy.SqrDistanceLogic >= attackMode3.minRange * attackMode3.minRange && base.selectattackEnemy.SqrDistanceLogic <= attackMode3.maxRange * attackMode3.maxRange)
				{
					m_ModeIndex.Add(j);
				}
			}
		}
		if (m_ModeIndex.Count == 0)
		{
			return Random.Range(0, attackMode.Length);
		}
		return m_ModeIndex[Random.Range(0, m_ModeIndex.Count)];
	}

	private Vector3 GetLocalCenterPos()
	{
		return base.selectattackEnemy.position + m_Local + Vector3.up * base.entity.maxHeight * 0.5f;
	}

	private Vector3 GetLocalPos(Enemy e, AttackMode attack)
	{
		if (!PEUtil.IsBlocked(e.entity, e.entityTarget) && !Stucking() && !PEUtil.IsNpcsuperposition(e.entity, e))
		{
			m_Local = Vector3.zero;
		}
		else if (m_Local == Vector3.zero || PEUtil.IsBlocked(e.entityTarget, GetLocalCenterPos()) || PEUtil.IsNpcsuperposition(GetLocalCenterPos(), e))
		{
			Vector3 local = Vector3.zero;
			float num = ((!(e.entityTarget.bounds.extents.x > e.entityTarget.bounds.extents.z)) ? e.entityTarget.bounds.extents.z : e.entityTarget.bounds.extents.x);
			for (int i = 0; i < 5; i++)
			{
				Vector3 randomPositionOnGround = PEUtil.GetRandomPositionOnGround(e.position, attack.minRange + num, attack.maxRange + num);
				Vector3 dirPos = randomPositionOnGround + Vector3.up * base.entity.maxHeight * 0.5f;
				if (!PEUtil.IsBlocked(e.entityTarget, dirPos) && !PEUtil.IsNpcsuperposition(dirPos, e))
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

	private bool IsInEnemyFoward(Enemy enemy, PeEntity self)
	{
		Vector3 vector = self.position;
		Vector3 vector2 = enemy.position;
		Vector3 forward = enemy.entityTarget.peTrans.forward;
		Vector3 normalized = (vector - vector2).normalized;
		float num = Mathf.Abs(PEUtil.Angle(forward, normalized));
		return num <= 90f;
	}

	private bool InRadiu(Vector3 self, Vector3 target, float radiu)
	{
		float num = PEUtil.SqrMagnitudeH(self, target);
		return num < radiu * radiu;
	}

	private void DoStep()
	{
		Vector3 vec = base.position - base.selectattackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		DoAction(PEActionType.Step, param);
	}

	private void DoSheid()
	{
		DoAction(PEActionType.HoldShield);
		Vector3 dir = base.selectattackEnemy.position - base.position;
		FaceDirection(dir);
	}

	private bool EndSheid()
	{
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		return !IsMotionRunning(PEActionType.HoldShield);
	}

	private bool CanStep()
	{
		Vector3 vec = base.position - base.selectattackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		return InRadiu(base.position, base.selectattackEnemy.position, 3f) && IsInEnemyFoward(base.selectattackEnemy, base.entity) && CanDoAction(PEActionType.Step, param);
	}

	private bool RunAway()
	{
		if (InRadiu(base.entity.position, base.selectattackEnemy.position, 3f))
		{
			Vector3 vector = base.position - base.selectattackEnemy.position;
			vector.y = 0f;
			Vector3 pos = base.position + vector * 3f;
			MoveToPosition(pos, SpeedState.Sprint);
			return true;
		}
		return false;
	}

	private IWeapon SwitchWeapon(Enemy e)
	{
		IWeapon weapon = null;
		if (!Enemy.IsNullOrInvalid(e) && base.entity.motionEquipment != null)
		{
			float num = float.PositiveInfinity;
			List<IWeapon> canUseWeaponList = base.entity.motionEquipment.GetCanUseWeaponList(base.entity);
			for (int i = 0; i < canUseWeaponList.Count; i++)
			{
				if (!base.entity.motionEquipment.WeaponCanUse(canUseWeaponList[i]))
				{
					continue;
				}
				float num2 = float.PositiveInfinity;
				AttackMode[] attackMode = canUseWeaponList[i].GetAttackMode();
				bool flag = false;
				for (int j = 0; j < attackMode.Length; j++)
				{
					if (attackMode[j].type == AttackType.Ranged)
					{
						TargetCmpt target = e.entityTarget.target;
						if (target != null)
						{
							PeEntity peEntity = ((target.enemy == null) ? null : target.enemy.entityTarget);
							if (peEntity == null || !peEntity.Equals(base.entity))
							{
								weapon = canUseWeaponList[i];
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
					weapon = canUseWeaponList[i];
				}
			}
		}
		return weapon;
	}

	private void AimTarget(IWeapon _weapon)
	{
		if (_weapon == null || _weapon.Equals(null))
		{
			return;
		}
		m_Index = SwitchAttackIndex(_weapon);
		if (m_Index < 0 || m_Index >= _weapon.GetAttackMode().Length)
		{
			return;
		}
		m_Mode = _weapon.GetAttackMode()[m_Index];
		if (m_Mode != null && _weapon is IAimWeapon aimWeapon)
		{
			if (m_Mode.type == AttackType.Ranged)
			{
				aimWeapon.SetAimState(aimState: true);
				aimWeapon.SetTarget(base.selectattackEnemy.CenterBone);
			}
			else
			{
				aimWeapon.SetAimState(aimState: false);
				aimWeapon.SetTarget(null);
			}
		}
	}

	private SpeedState CalculateChaseSpeed()
	{
		if (m_StartChaseTime == 0f)
		{
			m_StartChaseTime = Time.time;
		}
		if (Time.time - m_StartChaseTime <= 5f)
		{
			return SpeedState.Sprint;
		}
		if (Time.time - m_StartChaseTime > 5f && Time.time - m_StartChaseTime <= 8f)
		{
			return SpeedState.Run;
		}
		m_StartChaseTime = Time.time;
		return SpeedState.Run;
	}

	private BehaveResult Init(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy) || !base.entity.target.ContainEnemy(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		m_Attacked = false;
		m_LastAttackTime = 0f;
		m_StartAttackTime = Time.time;
		m_StartDefenceTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy) || !base.entity.target.ContainEnemy(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		base.entity.NpcCmpt.EqSelect.ClearSelect();
		base.entity.NpcCmpt.EqSelect.ClearAtkSelects();
		if (base.entity.NpcCmpt.EqSelect.SetSelectObjsAtk(base.entity, EeqSelect.combat))
		{
			base.entity.NpcCmpt.EqSelect.GetBetterAtkObj(base.entity, base.selectattackEnemy);
		}
		if (base.entity.NpcCmpt.EqSelect.BetterAtkObj != null && base.Weapon != null && !base.Weapon.Equals(null) && base.entity.NpcCmpt.EqSelect.BetterAtkObj != base.Weapon.ItemObj)
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase && !SelectItem.MatchEnemyAttack(base.entity, base.selectattackEnemy.entityTarget))
		{
			return BehaveResult.Failure;
		}
		bool flag = (!PeGameMgr.IsAdventure || !(RandomDungenMgr.Instance != null) || RandomDungenMgrData.dungeonBaseData == null) && (!PeGameMgr.IsStory || !PeGameMgr.IsSingle || SingleGameStory.curType == SingleGameStory.StoryScene.MainLand) && (!PeGameMgr.IsStory || !PeGameMgr.IsTutorial || SingleGameStory.curType == SingleGameStory.StoryScene.MainLand) && (!PeGameMgr.IsStory || !PeGameMgr.IsMulti || !(PlayerNetwork.mainPlayer != null) || PlayerNetwork.mainPlayer._curSceneId == 0) && PEUtil.IsBlocked(base.entity, base.selectattackEnemy.entityTarget);
		if (flag)
		{
			Vector3 movePos = GetMovePos(base.selectattackEnemy);
			Vector3 setPos = base.position + base.transform.forward;
			bool flag2 = PEUtil.IsUnderBlock(base.entity);
			bool flag3 = PEUtil.IsForwardBlock(base.entity, base.existent.forward, 2f);
			if (flag2)
			{
				if (flag3 || flag)
				{
					if (movePos.y >= setPos.y)
					{
						setPos.y = movePos.y;
					}
					SetPosition(setPos);
				}
				else
				{
					MoveDirection(movePos - base.position, SpeedState.Run);
				}
			}
			else
			{
				if (Stucking())
				{
					if (movePos.y >= setPos.y)
					{
						setPos.y = movePos.y;
					}
					SetPosition(setPos);
				}
				MoveToPosition(movePos, SpeedState.Run);
			}
			return BehaveResult.Running;
		}
		if (base.Weapon == null || base.Weapon.Equals(null))
		{
			bool flag4 = true;
			if (base.entity.motionMgr != null && base.entity.motionMgr.IsActionRunning(PEActionType.SwordAttack))
			{
				flag4 = false;
			}
			if (base.entity.motionMgr != null && base.entity.motionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
			{
				flag4 = false;
			}
			if (base.entity.motionEquipment.IsSwitchWeapon())
			{
				flag4 = false;
			}
			if (base.entity.isRagdoll)
			{
				flag4 = false;
			}
			if (base.entity.netCmpt != null && !base.entity.netCmpt.IsController)
			{
				flag4 = false;
			}
			if (flag4)
			{
				IWeapon weapon = SwitchWeapon(base.selectattackEnemy);
				if (weapon != null && !weapon.Equals(null))
				{
					if (base.entity.motionEquipment.Weapon == null || base.entity.motionEquipment.Weapon.Equals(null))
					{
						if (!weapon.HoldReady)
						{
							StopMove();
							weapon.HoldWeapon(hold: true);
							AimTarget(base.Weapon);
							return BehaveResult.Running;
						}
					}
					else if (!base.entity.motionEquipment.Weapon.Equals(weapon))
					{
						StopMove();
						base.entity.motionEquipment.SwitchHoldWeapon(base.entity.motionEquipment.Weapon, weapon);
						AimTarget(base.Weapon);
						return BehaveResult.Running;
					}
				}
			}
			return BehaveResult.Running;
		}
		m_Index = SwitchAttackIndex(base.Weapon);
		if (m_Index < 0 || m_Index >= base.Weapon.GetAttackMode().Length)
		{
			return BehaveResult.Failure;
		}
		m_Mode = base.Weapon.GetAttackMode()[m_Index];
		if (Time.time - m_LastAttackTime <= m_Mode.frequency)
		{
			return BehaveResult.Failure;
		}
		if (base.Weapon == null || base.Weapon.Equals(null) || m_Mode == null)
		{
			return BehaveResult.Failure;
		}
		if (base.Weapon is PEGloves && base.entity.motionEquipment.ActiveableEquipment != null)
		{
			SelectItem.TakeOffEquip(base.entity);
		}
		IAimWeapon aimWeapon = base.Weapon as IAimWeapon;
		if (aimWeapon != null)
		{
			if (m_Mode.type == AttackType.Ranged)
			{
				aimWeapon.SetAimState(aimState: true);
				aimWeapon.SetTarget(base.selectattackEnemy.CenterBone);
			}
			else
			{
				aimWeapon.SetAimState(aimState: false);
				aimWeapon.SetTarget(null);
			}
		}
		if (base.selectattackEnemy.entityTarget.target != null)
		{
			int n = ((base.selectattackEnemy.entityTarget.monsterProtoDb == null || base.selectattackEnemy.entityTarget.monsterProtoDb.AtkDb == null) ? 3 : base.selectattackEnemy.entityTarget.monsterProtoDb.AtkDb.mNumber);
			if (m_Mode.type == AttackType.Melee)
			{
				base.selectattackEnemy.entityTarget.target.AddMelee(base.entity, n);
			}
			else
			{
				base.selectattackEnemy.entityTarget.target.RemoveMelee(base.entity);
			}
		}
		if (base.selectattackEnemy.GroupAttack == EAttackGroup.Threat)
		{
			return BehaveResult.Failure;
		}
		float minRange = m_Mode.minRange;
		float maxRange = m_Mode.maxRange;
		float sqrDistanceLogic = base.selectattackEnemy.SqrDistanceLogic;
		Vector3 direction = base.selectattackEnemy.Direction;
		bool flag5 = !m_Mode.ignoreTerrain && (PEUtil.IsBlocked(base.entity, base.selectattackEnemy.entityTarget) || PEUtil.IsNpcsuperposition(base.entity, base.selectattackEnemy));
		bool flag6 = sqrDistanceLogic <= maxRange * maxRange && sqrDistanceLogic >= minRange * minRange;
		bool flag7 = PEUtil.IsScopeAngle(direction, base.transform.forward, Vector3.up, m_Mode.minAngle, m_Mode.maxAngle);
		bool flag8 = flag6 && flag7 && !flag5;
		bool flag9 = m_Mode.type == AttackType.Melee || aimWeapon == null || aimWeapon.Aimed;
		bool flag10 = !flag5 && m_Mode.type == AttackType.Ranged && base.entity.target.beSkillTarget;
		m_Local = GetLocalPos(base.selectattackEnemy, m_Mode);
		m_Local = Vector3.ProjectOnPlane(m_Local, Vector3.up);
		float num = minRange;
		if (m_Mode.type == AttackType.Ranged)
		{
			num += Mathf.Lerp(minRange, maxRange, 0.2f);
		}
		bool beSkillTarget = base.entity.target.beSkillTarget;
		if (Time.time - m_LastRetreatTime > 3f)
		{
			m_RetreatLocal = Vector3.zero;
			m_LastRetreatTime = Time.time;
			m_ChangeLocal = Vector3.zero;
			m_LastChangeTime = Time.time;
		}
		if (sqrDistanceLogic > maxRange * maxRange || flag5)
		{
			Vector3 movePos2 = GetMovePos(base.selectattackEnemy);
			if (flag5)
			{
				Vector3 setPos2 = base.position + base.transform.forward;
				if (Stucking())
				{
					if (movePos2.y >= setPos2.y)
					{
						setPos2.y = movePos2.y;
					}
					SetPosition(setPos2);
				}
				MoveToPosition(movePos2, SpeedState.Run);
			}
			else
			{
				Vector3 vector = movePos2 - base.position;
				Vector3 dir = ((!beSkillTarget) ? vector : Vector3.Lerp(Vector3.Cross(vector, Vector3.up), vector, Time.time));
				SpeedState state = CalculateChaseSpeed();
				MoveDirection(dir, state);
			}
		}
		else if (sqrDistanceLogic < num * num)
		{
			if (Time.time - m_LastRetreatTime < 2f)
			{
				if (m_RetreatLocal == Vector3.zero)
				{
					m_RetreatLocal = GetRetreatPos(base.selectattackEnemy.position, base.transform, minRange, maxRange);
				}
				FaceDirection(direction);
				MoveToPosition(m_RetreatLocal, SpeedState.Run);
			}
			else
			{
				StopMove();
			}
		}
		else if (flag10)
		{
			if (Time.time - m_LastChangeTime < 2f)
			{
				if (m_ChangeLocal == Vector3.zero)
				{
					m_ChangeLocal = GetRetreatPos(base.selectattackEnemy.position, base.transform, minRange, maxRange);
				}
				FaceDirection(direction);
				MoveToPosition(m_ChangeLocal, SpeedState.Run);
			}
		}
		else
		{
			StopMove();
		}
		if (!flag5)
		{
			if (!flag7 || !flag9)
			{
				FaceDirection(direction);
			}
			else
			{
				FaceDirection(Vector3.zero);
			}
		}
		if (base.selectattackEnemy.entityTarget.target != null)
		{
			Enemy enemy = base.selectattackEnemy.entityTarget.target.GetAttackEnemy();
			if (!Enemy.IsNullOrInvalid(enemy) && base.selectattackEnemy.entityTarget.IsAttacking && enemy.entityTarget == base.entity && IsInEnemyFoward(base.selectattackEnemy, base.entity) && Time.time - m_StartDefenceTime >= 3f)
			{
				m_StartDefenceTime = Time.time;
				bool flag11 = CanDoAction(PEActionType.HoldShield);
				bool flag12 = CanStep();
				if (flag11 && flag12)
				{
					if (Random.value > 0.5f)
					{
						DoStep();
					}
					else
					{
						DoSheid();
					}
					m_Attacked = false;
					m_StartAttackTime = Time.time;
					return BehaveResult.Running;
				}
				if (flag11)
				{
					DoSheid();
					m_Attacked = false;
					m_StartAttackTime = Time.time;
					return BehaveResult.Running;
				}
				if (flag12)
				{
					DoStep();
					m_Attacked = false;
					m_StartAttackTime = Time.time;
					return BehaveResult.Running;
				}
			}
		}
		if (!m_Attacked)
		{
			if (flag8 && flag9)
			{
				WeaponAttack(base.Weapon, base.selectattackEnemy, m_Index);
				m_Attacked = true;
			}
			return BehaveResult.Running;
		}
		bool flag13 = m_Mode != null && m_Mode.type == AttackType.Ranged && base.Weapon.IsInCD();
		if (flag8 && base.Weapon != null && !base.Weapon.Equals(null) && !flag13)
		{
			m_StartAttackTime = Time.time;
			WeaponAttack(base.Weapon, base.selectattackEnemy, m_Index);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (m_StartAttackTime > float.Epsilon)
		{
			FaceDirection(Vector3.zero);
			m_StartAttackTime = 0f;
		}
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			MoveDirection(Vector3.zero);
			StopMove();
		}
		if (!Enemy.IsNullOrInvalid(base.selectattackEnemy) && base.entity != null && base.selectattackEnemy.entityTarget != null && base.selectattackEnemy.entityTarget.target != null)
		{
			base.selectattackEnemy.entityTarget.target.RemoveMelee(base.entity);
		}
	}
}
