using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Pathea.Operate;
using PETools;
using SkillSystem;
using Steer3D;
using UnityEngine;
using WhiteCat;

namespace Behave.Runtime;

public class BTNormal : BTAction
{
	private const float HungerFloor = 0.1f;

	private const float ComfortFloor = 0.1f;

	internal static int WanderLayer = 6144;

	internal static int IgnoreWanderLayer = 65536;

	private IAgent m_Agent;

	private PeEntity m_Entity;

	private BehaveCmpt m_Behave;

	private SkAliveEntity m_Alive;

	private PeTrans m_Transform;

	private BiologyViewCmpt m_View;

	private Motion_Move m_Motor;

	private AnimatorCmpt m_Animator;

	private RequestCmpt m_Request;

	private TargetCmpt m_Target;

	private IKAimCtrl m_IKAim;

	private SkEntity m_SkEntity;

	private Motion_Equip m_Equipment;

	private NpcCmpt m_Npc;

	private OperateCmpt m_Operator;

	private MotionMgrCmpt m_Motion;

	private UseItemCmpt m_UseItem;

	private CommonCmpt m_Common;

	private PassengerCmpt m_Passenger;

	private MonsterCmpt m_Monster;

	internal PeEntity entity => m_Entity;

	internal SkEntity skEntity => m_SkEntity;

	internal BehaveCmpt behave => m_Behave;

	internal float HpPercent
	{
		get
		{
			PESkEntity pESkEntity = m_SkEntity as PESkEntity;
			return (!(pESkEntity != null)) ? 0f : pESkEntity.HPPercent;
		}
	}

	internal GameObject TDObj => (!(m_Common != null)) ? null : m_Common.TDObj;

	internal Vector3 TDpos => (!(m_Common != null)) ? Vector3.zero : m_Common.TDpos;

	internal Vector3 center => (!(m_Transform != null)) ? Vector3.zero : m_Transform.center;

	internal Vector3 position => (!(m_Transform != null)) ? Vector3.zero : m_Transform.position;

	internal Quaternion rotation => (!(m_Transform != null)) ? Quaternion.identity : m_Transform.rotation;

	internal Transform transform => (!(m_Transform != null)) ? null : m_Transform.trans;

	internal Transform existent => (!(m_Transform != null)) ? null : m_Transform.existent;

	internal float radius => (!(m_Transform != null)) ? 0f : m_Transform.radius;

	internal Vector3 spawnPosition => (!(m_Transform != null)) ? Vector3.zero : m_Transform.spawnPosition;

	internal Vector3 spawnForward => (!(m_Transform != null)) ? Vector3.zero : m_Transform.spawnForward;

	internal Rigidbody modelRigid => (!(m_View != null) || !(m_View.modelTrans != null)) ? null : m_View.modelTrans.GetComponentInChildren<Rigidbody>();

	internal bool hasModel => m_View != null && m_View.hasView;

	internal float gravity => (!(m_Motor != null)) ? (-1f) : m_Motor.gravity;

	internal Vector3 velocity => (!(m_Motor != null)) ? Vector3.zero : m_Motor.velocity;

	internal bool grounded => m_Motor != null && m_Motor.grounded;

	internal MovementField field
	{
		get
		{
			if (m_Motor is Motion_Move_Motor)
			{
				return (m_Motor as Motion_Move_Motor).Field;
			}
			if (m_Motor is Motion_Move_Human)
			{
				return MovementField.Land;
			}
			return MovementField.None;
		}
	}

	internal bool hasAnyRequest => m_Request != null && m_Request.HasRequest();

	internal bool IsOnVCCarrier => m_Passenger != null && m_Passenger.IsOnVCCarrier;

	internal bool IsOnRail => m_Passenger != null && m_Passenger.IsOnRail;

	internal bool hasAnyEnemy => m_Target != null && m_Target.HasAnyEnemy();

	internal bool hasAttackEnemy => m_Target != null && m_Target.GetAttackEnemy() != null;

	internal Enemy attackEnemy => (!(m_Target != null)) ? null : m_Target.GetAttackEnemy();

	internal Enemy followEnemy => (!(m_Target != null)) ? null : m_Target.GetFollowEnemy();

	internal Enemy escapeEnemy => (!(m_Target != null)) ? null : m_Target.GetEscapeEnemy();

	internal Enemy threatEnemy => (!(m_Target != null)) ? null : m_Target.GetThreatEnemy();

	internal Enemy afraidEnemy => (!(m_Target != null)) ? null : m_Target.GetAfraidEnemy();

	internal List<Enemy> Enemies => (!(m_Target != null)) ? null : m_Target.GetEnemies();

	internal Enemy selectattackEnemy => attackEnemy;

	internal IWeapon Weapon
	{
		get
		{
			if (m_Equipment != null && m_Equipment.Weapon != null && !m_Equipment.Weapon.Equals(null))
			{
				return m_Equipment.Weapon;
			}
			return null;
		}
	}

	internal bool IsNpcFollowerWork => m_Npc != null && m_Npc.FollowerWork;

	internal bool IsNpcFollowerSentry => m_Npc != null && m_Npc.FollowerSentry;

	internal bool IsNpcFollowerCut => m_Npc != null && m_Npc.FollowerCut;

	internal bool IsNpcProcessing => m_Npc != null && m_Npc.Processing;

	internal bool IsNpcTrainning => m_Npc != null && m_Npc.IsTrainning;

	internal bool IsNpc => m_Npc != null;

	internal bool IsNpcBase => m_Npc != null && m_Npc.Creater != null && m_Npc.Creater.Assembly != null;

	internal bool IsNpcFollower => m_Npc != null && m_Npc.Master != null;

	internal bool IsNpcCampsite => m_Npc != null && m_Npc.Campsite != null;

	internal bool CanNpcWander => m_Npc != null && m_Npc.CanWander;

	internal ENpcJob NpcJob => (m_Npc != null) ? m_Npc.Job : ENpcJob.None;

	internal ENpcState NpcJobStae => (m_Npc != null) ? m_Npc.State : ENpcState.UnKnown;

	internal ENpcMedicalState NpcMedicalState => (m_Npc != null) ? m_Npc.MedicalState : ENpcMedicalState.None;

	internal ENpcType NpcType => (m_Npc != null) ? m_Npc.Type : ENpcType.None;

	internal ETrainerType NpcTrainerType => (m_Npc != null) ? m_Npc.TrainerType : ETrainerType.none;

	internal ETrainingType NpcTrainingType => (m_Npc != null) ? m_Npc.TrainningType : ETrainingType.Skill;

	internal ENpcSoldier NpcSoldier => (m_Npc != null) ? m_Npc.Soldier : ENpcSoldier.None;

	internal ServantLeaderCmpt NpcMaster => (!(m_Npc != null)) ? null : m_Npc.Master;

	internal Vector3 GuardPosition => (!(m_Npc != null)) ? Vector3.zero : m_Npc.GuardPosition;

	internal float GuardRadius => (!(m_Npc != null)) ? 0f : m_Npc.GuardRadius;

	internal CSCreator Creater => (!(m_Npc != null)) ? null : m_Npc.Creater;

	internal Camp Campsite => (!(m_Npc != null)) ? null : m_Npc.Campsite;

	internal List<CSEntity> BaseEntities => (!(m_Npc != null)) ? null : m_Npc.BaseEntities;

	internal IOperation Sleep
	{
		get
		{
			object result;
			if (m_Npc != null)
			{
				IOperation sleep = m_Npc.Sleep;
				result = sleep;
			}
			else
			{
				result = null;
			}
			return (IOperation)result;
		}
	}

	internal IOperation Work
	{
		get
		{
			object result;
			if (m_Npc != null)
			{
				IOperation work = m_Npc.Work;
				result = work;
			}
			else
			{
				result = null;
			}
			return (IOperation)result;
		}
	}

	internal IOperation Cured
	{
		get
		{
			object result;
			if (m_Npc != null)
			{
				IOperation cure = m_Npc.Cure;
				result = cure;
			}
			else
			{
				result = null;
			}
			return (IOperation)result;
		}
	}

	internal IOperation Trainner
	{
		get
		{
			object result;
			if (m_Npc != null)
			{
				IOperation trainner = m_Npc.Trainner;
				result = trainner;
			}
			else
			{
				result = null;
			}
			return (IOperation)result;
		}
	}

	internal bool AskStop => m_Npc != null && m_Npc.MisstionAskStop;

	internal Vector3 FollowerHidePos => (!(m_Npc != null)) ? Vector3.zero : m_Npc.FollowerHidePostion;

	internal Vector3 FixedPointPostion => (!(m_Npc != null)) ? Vector3.zero : m_Npc.FixedPointPos;

	internal int NpcCmdId => (m_Npc != null) ? m_Npc.NpcControlCmdId : 0;

	internal ENpcBattle NpcBattle => (!(m_Npc != null)) ? ENpcBattle.Defence : m_Npc.Battle;

	internal PEBuilding NpcOccpyBuild => (!(m_Npc != null)) ? null : m_Npc.OccopyBuild;

	internal bool IsSkillCast => m_Npc != null && m_Npc.InAllys;

	internal PeEntity SkillTarget => (!(m_Npc != null) || !(m_Npc.NpcSkillTarget != null)) ? null : m_Npc.NpcSkillTarget.Entity;

	internal Vector3 CostPos => (!(m_Npc != null)) ? Vector3.zero : m_Npc.NpcPostion;

	internal Vector3 AllyTargetPos => (!(m_Npc != null) || !(m_Npc.NpcSkillTarget != null)) ? Vector3.zero : m_Npc.NpcSkillTarget.NpcPostion;

	internal CSEntity WorkEntity => (!(m_Npc != null)) ? null : m_Npc.WorkEntity;

	internal IOperator Operator => m_Operator;

	internal bool IsNeedMedicine => m_Npc != null && m_Npc.IsNeedMedicine;

	internal NativeProfession nativeProfession => (m_Monster != null) ? m_Monster.Profession : NativeProfession.None;

	internal NativeAge nativeAge => (m_Monster != null) ? m_Monster.Age : NativeAge.None;

	internal NativeSex nativeSex => (m_Monster != null) ? m_Monster.Sex : NativeSex.None;

	internal override void InitAgent(IAgent argAgent)
	{
		m_Agent = argAgent;
		if (m_Agent != null && !m_Agent.Equals(null))
		{
			m_Behave = m_Agent as BehaveCmpt;
			m_Entity = m_Behave.Entity;
			m_Alive = m_Entity.aliveEntity;
			m_Transform = m_Entity.peTrans;
			m_View = m_Entity.biologyViewCmpt;
			m_Motor = m_Entity.motionMove;
			m_Animator = m_Entity.animCmpt;
			m_Request = m_Entity.requestCmpt;
			m_Target = m_Entity.target;
			m_SkEntity = m_Entity.skEntity;
			m_Equipment = m_Entity.motionEquipment;
			m_Npc = m_Entity.NpcCmpt;
			m_Operator = m_Entity.operateCmpt;
			m_UseItem = m_Entity.UseItem;
			m_Common = m_Entity.commonCmpt;
			m_Passenger = m_Entity.passengerCmpt;
			m_Monster = m_Entity.monster;
			m_Motion = m_Entity.motionMgr;
			m_IKAim = m_Entity.biologyViewCmpt.monoIKAimCtrl;
		}
	}

	internal float GetAttribute(AttribType type)
	{
		if (m_Alive != null)
		{
			return m_Alive.GetAttribute(type);
		}
		return 0f;
	}

	internal bool InBody(Vector3 pos)
	{
		if (m_Transform != null)
		{
			return m_Transform.InsideBody(pos);
		}
		return false;
	}

	internal Transform GetModelName(string name)
	{
		Transform transform = null;
		if (m_View != null)
		{
			transform = m_View.GetModelTransform(name);
		}
		if (transform == null && m_View != null)
		{
			return m_View.centerTransform;
		}
		return transform;
	}

	internal bool Stucking(float time = 2f)
	{
		return m_Motor != null && m_Motor.Stucking(time);
	}

	internal Request GetRequest(EReqType type)
	{
		return (!(m_Request != null)) ? null : m_Request.GetRequest(type);
	}

	internal void RemoveRequest(EReqType type)
	{
		if (m_Request != null)
		{
			m_Request.RemoveRequest(type);
		}
	}

	internal void RemoveRequest(Request request)
	{
		if (m_Request != null)
		{
			m_Request.RemoveRequest(request);
		}
	}

	internal bool ContainsRequest(EReqType type)
	{
		if (m_Request != null)
		{
			return m_Request.Contains(type);
		}
		return false;
	}

	internal PeEntity GetAfraidTarget()
	{
		if (m_Target != null)
		{
			return m_Target.GetAfraidTarget();
		}
		return null;
	}

	internal PeEntity GetDoubtTarget()
	{
		if (m_Target != null)
		{
			return m_Target.GetDoubtTarget();
		}
		return null;
	}

	internal void SetCambat(bool value)
	{
		if (m_Target != null)
		{
			m_Target.CanActiveAttck = value;
		}
	}

	internal void UseTool(bool value)
	{
		if (m_Target != null)
		{
			m_Target.UseTool = value;
		}
	}

	internal SkInst StartSkill(PeEntity target, int id)
	{
		if (m_SkEntity != null)
		{
			if (!Enemy.IsNullOrInvalid(attackEnemy) && attackEnemy.entityTarget != null)
			{
				attackEnemy.entityTarget.DispatchTargetSkill(entity.skEntity);
			}
			SkEntity target2 = ((!(target != null)) ? null : target.skEntity);
			return m_SkEntity.StartSkill(target2, id);
		}
		return null;
	}

	internal SkInst StartSkillSkEntity(SkEntity target, int id)
	{
		if (m_SkEntity != null)
		{
			return m_SkEntity.StartSkill(target, id);
		}
		return null;
	}

	internal void StopSkill(int id)
	{
		if (m_SkEntity != null)
		{
			m_SkEntity.CancelSkillById(id);
		}
	}

	internal bool IsSkillRunning(int id, bool cdInclude = false)
	{
		if (m_SkEntity != null)
		{
			return m_SkEntity.IsSkillRunning(id, cdInclude);
		}
		return false;
	}

	internal bool IsSkillRunnable(int id)
	{
		if (m_SkEntity != null)
		{
			return m_SkEntity.IsSkillRunnable(id);
		}
		return false;
	}

	internal void WeaponAttack(IWeapon weapon, Enemy _attackEnmey, int index = 0, SkEntity targetEntity = null)
	{
		if (weapon != null && !weapon.Equals(null))
		{
			weapon.Attack(index, targetEntity);
			if (entity.proto == EEntityProto.Monster && entity.commonCmpt != null && entity.commonCmpt.Race == ERace.Mankind && !Enemy.IsNullOrInvalid(_attackEnmey) && _attackEnmey.entityTarget != null)
			{
				_attackEnmey.entityTarget.DispatchWeaponAttack(entity.skEntity);
			}
		}
	}

	internal List<IWeapon> GetWeaponList()
	{
		if (m_Equipment != null)
		{
			return m_Equipment.GetWeaponList();
		}
		return new List<IWeapon>();
	}

	internal bool WeaponCanUse(IWeapon weapon)
	{
		if (m_Equipment != null)
		{
			return m_Equipment.WeaponCanUse(weapon);
		}
		return false;
	}

	internal void ActiveWeapon(bool value)
	{
		if (m_Equipment != null)
		{
			m_Equipment.ActiveWeapon(value);
		}
	}

	internal void ActiveWeapon(PEHoldAbleEquipment handEquipment, bool active, bool immediately = false)
	{
		if (m_Equipment != null)
		{
			m_Equipment.ActiveWeapon(handEquipment, active, immediately);
		}
	}

	internal bool GetEquipMaskState(PEActionMask mask)
	{
		if (m_Equipment != null)
		{
			return m_Motion.GetMaskState(mask);
		}
		return false;
	}

	internal bool IsActionRunning()
	{
		if (m_Motion != null)
		{
			return m_Motion.IsActionRunning();
		}
		return false;
	}

	internal bool IsActionRunning(PEActionType type)
	{
		if (m_Motion != null)
		{
			return m_Motion.IsActionRunning(type);
		}
		return false;
	}

	internal Action_Fell SetGlobalTreeInfo(GlobalTreeInfo treeInfo)
	{
		if (m_Motion != null)
		{
			Action_Fell action = m_Motion.GetAction<Action_Fell>();
			if (action != null)
			{
				action.treeInfo = treeInfo;
			}
			return action;
		}
		return null;
	}

	internal Action_Gather SetGlobalGatherInfo(GlobalTreeInfo treeInfo)
	{
		if (m_Motion != null)
		{
			Action_Gather action = m_Motion.GetAction<Action_Gather>();
			if (action != null)
			{
				action.treeInfo = treeInfo;
			}
			return action;
		}
		return null;
	}

	internal void ActivateEnergyShield(bool value)
	{
		if (m_Equipment != null && m_Equipment.energySheild != null)
		{
			if (value)
			{
				m_Equipment.energySheild.ActiveSheild(fullCharge: true);
			}
			else
			{
				m_Equipment.energySheild.DeactiveSheild();
			}
		}
	}

	internal bool TowerAngle(Vector3 position, float angle)
	{
		return entity.Tower != null && entity.Tower.Angle(position, angle);
	}

	internal bool TowerPitchAngle(Vector3 position, float angle)
	{
		return entity.Tower != null && entity.Tower.PitchAngle(position, angle);
	}

	internal bool TowerCanAttack(Vector3 position, Transform target = null)
	{
		return entity.Tower != null && entity.Tower.CanAttack(position, target);
	}

	internal bool TowerIsEnable()
	{
		return entity.Tower != null && entity.Tower.IsEnable;
	}

	internal bool TowerHaveCost()
	{
		return entity.Tower != null && entity.Tower.HaveCost();
	}

	internal bool TowerSkillRunning()
	{
		return entity.Tower != null && entity.Tower.IsSkillRunning();
	}

	internal int GetAreadySkill()
	{
		if (m_Npc != null)
		{
			return m_Npc.GetReadySkill();
		}
		return -1;
	}

	internal float GetSkillRange(int SkillId)
	{
		if (m_Npc != null)
		{
			return m_Npc.GetNpcSkillRange(SkillId);
		}
		return 0f;
	}

	internal bool GetItemsSkill(Vector3 pos, float percent)
	{
		if (m_Npc != null)
		{
			return m_Npc.TryGetItemSkill(pos, percent);
		}
		return false;
	}

	internal void SetOccpyBuild(PEBuilding buid)
	{
		if (m_Npc != null)
		{
			m_Npc.OccopyBuild = buid;
		}
	}

	internal bool GetHpJudge(int SkillId)
	{
		if (m_Npc != null && m_Npc.NpcSkillTarget != null)
		{
			float npcChange_Hp = m_Npc.GetNpcChange_Hp(SkillId);
			if (npcChange_Hp == 0f)
			{
				return true;
			}
			float npcHppercent = m_Npc.NpcSkillTarget.NpcHppercent;
			return npcHppercent <= npcChange_Hp;
		}
		return false;
	}

	internal void CanWander(bool Iswork)
	{
		if (m_Npc != null)
		{
			m_Npc.CanWander = Iswork;
		}
	}

	internal void CallBackFollower()
	{
		if (m_Npc != null)
		{
			m_Npc.FollowerWork = false;
		}
	}

	internal void EndFollowerCut()
	{
		if (m_Npc != null)
		{
			m_Npc.FollowerCut = false;
			m_Npc.RmoveTalkInfo(ENpcTalkType.Follower_cut);
		}
	}

	internal void SetNpcAiType(ENpcAiType type)
	{
		if (m_Npc != null)
		{
			m_Npc.AiType = type;
		}
	}

	internal bool sendTalkMgs(int TalkId, float time = 0f, ENpcSpeakType type = ENpcSpeakType.TopHead)
	{
		if (m_Npc != null)
		{
			return m_Npc.SendTalkMsg(TalkId, time, type);
		}
		return false;
	}

	internal bool SkillOver()
	{
		if (m_Npc != null)
		{
			m_Npc.Req_Remove(EReqType.UseSkill);
			m_Npc.InAllys = false;
			m_Npc.NpcSkillTarget = null;
			return true;
		}
		return false;
	}

	internal bool ContainAllys()
	{
		if (m_Npc != null)
		{
			return m_Npc.Containself();
		}
		return false;
	}

	internal bool ContainsTitle(ENpcTitle title)
	{
		if (m_Npc != null)
		{
			return m_Npc.ContainsTitle(title);
		}
		return false;
	}

	internal bool NpcCanWalkPos(Vector3 center, float radiu, out Vector3 walkPos)
	{
		Vector3 direction = position - center;
		for (int i = 0; i < 10; i++)
		{
			Vector3 randomPositionInCircle = PEUtil.GetRandomPositionInCircle(center, radiu * 0.7f, radiu, direction, 60f, 100f);
			randomPositionInCircle = PEUtil.CheckPosForNpcStand(randomPositionInCircle);
			if (randomPositionInCircle != Vector3.zero)
			{
				walkPos = randomPositionInCircle;
				float num = PEUtil.Magnitude(center, walkPos);
				if (num <= radiu && num > radiu * 0.7f)
				{
					return true;
				}
			}
		}
		for (int j = 0; j < 10; j++)
		{
			Vector3 randomPositionInCircle = PEUtil.GetRandomPositionOnGroundForWander(center, 5f, radiu);
			randomPositionInCircle = PEUtil.CheckPosForNpcStand(randomPositionInCircle);
			if (randomPositionInCircle != Vector3.zero)
			{
				walkPos = randomPositionInCircle;
				float num2 = PEUtil.Magnitude(center, walkPos);
				if (num2 <= radiu)
				{
					return true;
				}
			}
		}
		walkPos = PEUtil.GetRandomPositionOnGroundForWander(center, 5f, radiu);
		walkPos = PEUtil.CheckPosForNpcStand(walkPos);
		return walkPos != Vector3.zero;
	}

	internal Vector3 CampiseChanceTalk()
	{
		if (m_Npc != null && m_Npc.Campsite != null)
		{
			return m_Npc.Campsite.CalculatePostion(m_Npc.Entity.Id, m_Npc.NpcPostion, 2.5f);
		}
		return Vector3.zero;
	}

	internal bool CampiseChanceTalk(out PeEntity talkTarg)
	{
		if (m_Npc != null && m_Npc.Campsite != null && m_Npc.Campsite.CalculatePostion(entity, 2.5f, out talkTarg))
		{
			return true;
		}
		talkTarg = null;
		return false;
	}

	internal bool CantainTalkTarget(PeEntity Target)
	{
		if (m_Npc != null && m_Npc.Campsite != null)
		{
			return m_Npc.Campsite.CantainTarget(2.5f, entity, Target);
		}
		return false;
	}

	internal bool IsSelfSleep(int entityID, out SleepPostion sleepInfo)
	{
		if (m_Npc != null && m_Npc.Campsite != null)
		{
			sleepInfo = m_Npc.Campsite.HasSleep(entityID);
			if (sleepInfo != null)
			{
				return true;
			}
		}
		sleepInfo = null;
		return false;
	}

	internal bool IsMotionRunning(PEActionType type)
	{
		if (m_Motion != null)
		{
			return m_Motion.IsActionRunning(type);
		}
		return false;
	}

	internal bool CanDoAction(PEActionType type, PEActionParam objs = null)
	{
		if (m_Motion != null)
		{
			return m_Motion.CanDoAction(type, objs);
		}
		return false;
	}

	internal bool DoAction(PEActionType type, PEActionParam objs = null)
	{
		if (m_Motion != null)
		{
			return m_Motion.DoAction(type, objs);
		}
		return false;
	}

	internal void DoActionImmediately(PEActionType type, PEActionParam objs)
	{
		if (m_Motion != null)
		{
			m_Motion.DoActionImmediately(type, objs);
		}
	}

	internal bool EndAction(PEActionType type)
	{
		if (m_Motion != null)
		{
			return m_Motion.EndAction(type);
		}
		return false;
	}

	internal void EndImmediately(PEActionType type)
	{
		if (m_Motion != null)
		{
			m_Motion.EndImmediately(type);
		}
	}

	internal bool GetBool(string name)
	{
		if (m_Animator != null)
		{
			return m_Animator.GetBool(name);
		}
		return false;
	}

	internal void SetMedicineSate(ENpcMedicalState type)
	{
		if (m_Npc != null)
		{
			m_Npc.MedicalState = type;
		}
	}

	internal bool IsFly()
	{
		if (m_Monster != null)
		{
			return m_Monster.IsFly;
		}
		return false;
	}

	internal bool InsidePolarShield(Vector3 pos, out Vector3 position, out float radius)
	{
		radius = 0f;
		position = Vector3.zero;
		if (entity.monster == null)
		{
			return false;
		}
		if (entity.commonCmpt != null && entity.commonCmpt.TDObj != null)
		{
			return false;
		}
		return PolarShield.GetPolarShield(pos, entity.monster.InjuredLevel, out position, out radius);
	}

	internal bool EvadePolarShield(Vector3 pos)
	{
		if (entity.monster == null)
		{
			return true;
		}
		if (entity.commonCmpt != null && entity.commonCmpt.TDObj != null)
		{
			return true;
		}
		return !PolarShield.IsInsidePolarShield(pos, entity.monster.InjuredLevel);
	}

	internal Vector3 GetEvadePolarShieldPosition(Vector3 pos)
	{
		if (!EvadePolarShield(pos))
		{
			return PolarShield.GetRandomPosition(pos, entity.monster.InjuredLevel);
		}
		return Vector3.zero;
	}

	internal void SetAttribute(int key, float value)
	{
		if (m_SkEntity != null)
		{
			m_SkEntity.SetAttribute(key, value, eventOff: false);
		}
	}

	internal void SetModelFadeIn()
	{
		StandardAlphaAnimator componentInChildren = m_Npc.GetComponentInChildren<StandardAlphaAnimator>();
		if (componentInChildren != null)
		{
			componentInChildren._GenFadeIn = true;
		}
	}

	internal void ClearEscape()
	{
		if (m_Target != null)
		{
			m_Target.ClearEscapeEnemy();
		}
	}

	internal void ClearEnemy()
	{
		if (m_Target != null)
		{
			m_Target.ClearEnemy();
		}
	}

	internal void SetFloat(string name, float value)
	{
		if (m_Animator != null)
		{
			m_Animator.SetFloat(name, value);
		}
	}

	internal void CallHelp(float radius)
	{
		if (m_Target != null)
		{
			m_Target.CallHelp(radius);
		}
	}

	internal void SetEscapeEntity(PeEntity entity)
	{
		if (m_Target != null)
		{
			m_Target.SetEscapeEntity(entity);
		}
	}

	internal void ActivateGravity(bool value)
	{
		if (m_Monster != null)
		{
			m_Monster.ActivateGravity(value);
		}
	}

	internal void Fly(bool value)
	{
		if (m_Monster != null)
		{
			m_Monster.Fly(value);
		}
	}

	internal Seek AlterSeekBehaviour(Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		return (!(m_Motor != null)) ? null : m_Motor.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight);
	}

	internal Seek AlterSeekBehaviour(Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		return (!(m_Motor != null)) ? null : m_Motor.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight);
	}

	internal void MoveToPosition(Vector3 pos, SpeedState state = SpeedState.Walk, bool avoid = true)
	{
		if (m_Motor != null)
		{
			m_Motor.MoveTo(pos, state, avoid);
		}
	}

	internal bool ReachToPostion(Vector3 pos, SpeedState state = SpeedState.Walk)
	{
		if (m_Motor != null && m_Transform != null)
		{
			m_Motor.MoveTo(pos, state);
			if (PEUtil.SqrMagnitudeH(m_Transform.position, pos) < 5f)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	internal bool IsReached(Vector3 pos, Vector3 targetPos, bool Is3D = false, float radiu = 1f)
	{
		float num = PEUtil.Magnitude(pos, targetPos, Is3D);
		return num < radiu;
	}

	internal void MoveDirection(Vector3 dir, SpeedState state = SpeedState.Walk)
	{
		if (m_Motor != null)
		{
			m_Motor.Move(dir, state);
		}
	}

	internal void FaceDirection(Vector3 dir)
	{
		if (m_Motor != null)
		{
			m_Motor.RotateTo(dir);
		}
	}

	internal void StopMove()
	{
		if (m_Motor != null)
		{
			m_Motor.Stop();
		}
	}

	internal void SetSpeed(float speed)
	{
		if (m_Motor != null)
		{
			m_Motor.SetSpeed(speed);
		}
	}

	internal void SetPosition(Vector3 setPos, bool neeedrepair = true)
	{
		Vector3 vector = setPos;
		if (!(m_Transform != null))
		{
			return;
		}
		if (!PEUtil.CheckErrorPos(vector))
		{
			Debug.LogError("[ERROR]Try to set error pos[" + vector.x + "," + vector.y + "," + vector.z + "] to entity " + m_Entity.name + " From  " + base.Name);
		}
		if (GameConfig.IsMultiClient)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(entity.Id);
			if (networkInterface != null && !networkInterface.hasAuth)
			{
				if (entity.NpcCmpt != null)
				{
					entity.NpcCmpt.Req_Translate(vector);
				}
			}
			else if (!(null == networkInterface) && networkInterface.hasOwnerAuth && networkInterface is AiAdNpcNetwork)
			{
				AiAdNpcNetwork aiAdNpcNetwork = (AiAdNpcNetwork)networkInterface;
				aiAdNpcNetwork.RequestResetPosition(vector);
				m_Transform.position = vector;
				SceneMan.SetDirty(m_Entity.lodCmpt);
			}
		}
		else
		{
			if (neeedrepair)
			{
				vector = PEUtil.CorrectionPostionToStand(vector);
			}
			m_Transform.position = vector;
			m_Entity.DispatchOnTranslate(vector);
			SceneMan.SetDirty(m_Entity.lodCmpt);
		}
	}

	internal void SetRotation(Quaternion rot)
	{
		if (m_Transform != null)
		{
			m_Transform.rotation = rot;
		}
	}

	internal void ClearNpcMount()
	{
		if (m_Npc != null)
		{
			m_Npc.MountID = 0;
		}
	}

	internal void SetNpcUpdateCampsite(bool value)
	{
		if (m_Npc != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiAdNpcNetwork).RequestUpdateCampsite(value);
			}
			m_Npc.UpdateCampsite = value;
		}
	}

	internal void SetNpcState(ENpcState state)
	{
		if (m_Npc != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiAdNpcNetwork).RequestState((int)state);
			}
			m_Npc.State = state;
		}
	}

	internal void GetOn(CarrierController clr, int index)
	{
		if (m_Passenger != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiAdNpcNetwork).RequestGetOn(clr.creationController.creationData.m_ObjectID, index);
			}
			if (PeGameMgr.IsSingle && !entity.isRagdoll)
			{
				m_Passenger.GetOn(clr, index, checkState: false);
			}
		}
	}

	internal void GetOn(int railRouteId, int entityId, bool checkState = true)
	{
		if (m_Passenger != null)
		{
			if (GameConfig.IsMultiMode)
			{
				PeSingleton<RailwayOperate>.Instance.RequestGetOnTrain(railRouteId, entityId);
			}
			else if (!entity.isRagdoll)
			{
				PeSingleton<RailwayOperate>.Instance.RequestGetOnTrain(railRouteId, entityId);
			}
		}
	}

	internal void GetOff()
	{
		if (m_Passenger != null)
		{
			if (GameConfig.IsMultiMode)
			{
				m_Passenger.GetOffCarrier();
			}
			else if (!entity.isRagdoll)
			{
				m_Passenger.GetOffCarrier();
			}
		}
	}

	internal void GetOffRailRoute()
	{
		if (m_Passenger != null)
		{
			if (GameConfig.IsMultiMode)
			{
				PeSingleton<RailwayOperate>.Instance.RequestGetOffTrain(m_Passenger.railRouteId, m_Passenger.Entity.Id, PeSingleton<PeCreature>.Instance.mainPlayer.position);
			}
			else if (!entity.isRagdoll)
			{
				PeSingleton<RailwayOperate>.Instance.RequestGetOffTrain(m_Passenger.railRouteId, m_Passenger.Entity.Id, PeSingleton<PeCreature>.Instance.mainPlayer.position);
			}
		}
	}

	internal void SetTowerAimPosition(Transform target)
	{
		if (!(entity.Tower != null) || !(entity.Tower.Target != target))
		{
			return;
		}
		if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
		{
			if (m_SkEntity._net is AiTowerNetwork)
			{
				(m_SkEntity._net as AiTowerNetwork).RequestAimTarget(attackEnemy.entityTarget.Id);
			}
			else if (m_SkEntity._net is MapObjNetwork)
			{
				(m_SkEntity._net as MapObjNetwork).RequestAimTarget(attackEnemy.entityTarget.Id);
			}
		}
		entity.Tower.Target = target;
	}

	internal void TowerFire(SkEntity target)
	{
		if (!(entity.Tower != null))
		{
			return;
		}
		if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
		{
			if (m_SkEntity._net is AiTowerNetwork)
			{
				(m_SkEntity._net as AiTowerNetwork).RequestFire(target.GetId());
			}
			else if (m_SkEntity._net is MapObjNetwork)
			{
				(m_SkEntity._net as MapObjNetwork).RequestFire(target.GetId());
			}
		}
		entity.Tower.Fire(target);
	}

	internal void SetBool(string name, bool value)
	{
		if (name.Length != 0 && !(m_Animator == null) && m_Animator != null && m_Animator.GetBool(name) != value)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController() && m_SkEntity._net is AiNetwork)
			{
				(m_SkEntity._net as AiNetwork).RequestSetBool(Animator.StringToHash(name), value);
			}
			m_Animator.SetBool(name, value);
		}
	}

	internal void SetTrigger(string name)
	{
		if (m_Animator != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiNetwork).RequestSetTrigger(name);
			}
			m_Animator.SetTrigger(name);
		}
	}

	internal void SetMoveMode(MoveMode mode)
	{
		if (m_Motor != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiNetwork).RequestSetMoveMode((int)mode);
			}
			m_Motor.mode = mode;
		}
	}

	internal void HoldWeapon(IWeapon weapon, bool value)
	{
		if (weapon != null && !weapon.Equals(null))
		{
			if (!(m_SkEntity != null) || !(m_SkEntity._net != null) || m_SkEntity.IsController())
			{
			}
			weapon.HoldWeapon(value);
		}
	}

	internal void SwitchHoldWeapon(IWeapon oldWeapon, IWeapon newWeapon)
	{
		if (m_Equipment != null)
		{
			if (!(m_SkEntity != null) || !(m_SkEntity._net != null) || m_SkEntity.IsController())
			{
			}
			m_Equipment.SwitchHoldWeapon(oldWeapon, newWeapon);
		}
	}

	internal void SwordAttack(Vector3 dir)
	{
		if (m_Equipment != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiNetwork).RequestSwordAttack(dir);
			}
			m_Equipment.SwordAttack(dir);
		}
	}

	internal void TwoHandWeaponAttack(Vector3 dir, int handType = 0, int time = 0)
	{
		if (m_Equipment != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiNetwork).RequestTwoHandWeaponAttack(dir, handType, time);
			}
			m_Equipment.TwoHandWeaponAttack(dir, handType, time);
		}
	}

	internal void UseItem(ItemObject item)
	{
		if (m_UseItem != null)
		{
			m_UseItem.Use(item);
		}
	}

	internal float GetCdByProtoId(int protoId)
	{
		if (m_UseItem != null)
		{
			return m_UseItem.GetCdByItemProtoId(protoId);
		}
		return 0f;
	}

	internal void SetIKAim(Transform aimTarget)
	{
		if (m_IKAim != null)
		{
			if (!(m_SkEntity != null) || !(m_SkEntity._net != null) || m_SkEntity.IsController())
			{
			}
			m_IKAim.SetTarget(aimTarget);
			m_IKAim.SetActive(null != aimTarget);
		}
	}

	internal void SetIkTarget(Transform aimTran)
	{
		if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
		{
			entity.IKCmpt.iKAimCtrl.SetTarget(aimTran);
		}
	}

	internal void SetIKTargetPos(Vector3 targetPos)
	{
		if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
		{
			SetIkTarget(null);
			entity.IKCmpt.iKAimCtrl.targetPos = targetPos;
		}
	}

	internal void SetIKActive(bool active)
	{
		if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
		{
			entity.IKCmpt.iKAimCtrl.SetActive(active);
		}
	}

	internal void SetIKLerpspeed(float lerpSpeed)
	{
		if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
		{
			entity.IKCmpt.iKAimCtrl.m_LerpSpeed = lerpSpeed;
		}
	}

	internal void SetIKFadeInTime(float time)
	{
		if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
		{
			entity.IKCmpt.iKAimCtrl.m_FadeInTime = time;
		}
	}

	internal void SetIKFadeOutTime(float time)
	{
		if (entity.IKCmpt != null && entity.IKCmpt.iKAimCtrl != null)
		{
			entity.IKCmpt.iKAimCtrl.m_FadeOutTime = time;
		}
	}

	internal void Fadein(float time)
	{
		if (m_View != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiNetwork).RequestFadein(time);
			}
			m_View.Fadein(time);
		}
	}

	internal void SetViewActive(bool value)
	{
		if (m_View != null && m_View.tView != null)
		{
			m_View.tView.gameObject.SetActive(value);
		}
	}

	internal void Fadeout(float time)
	{
		if (m_View != null)
		{
			if (m_SkEntity != null && m_SkEntity._net != null && m_SkEntity.IsController())
			{
				(m_SkEntity._net as AiNetwork).RequestFadeout(time);
			}
			m_View.Fadeout(time);
		}
	}

	internal bool IsBlock()
	{
		if (attackEnemy != null)
		{
			RaycastHit[] array = Physics.RaycastAll(center, attackEnemy.centerPos - center, Vector3.Distance(center, attackEnemy.centerPos));
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].collider.isTrigger && !array[i].transform.IsChildOf(entity.transform) && !array[i].transform.IsChildOf(attackEnemy.trans))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected T GetCmpt<T>(Tree sender) where T : PeCmpt
	{
		return (sender.ActiveAgent as BehaveCmpt).gameObject.GetComponent<T>();
	}

	protected bool GetData<T>(Tree sender, ref T t)
	{
		if (m_TreeDataList.ContainsKey(sender.ActiveStringParameter))
		{
			try
			{
				t = (T)m_TreeDataList[sender.ActiveStringParameter];
				return t != null;
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
				return false;
			}
		}
		return false;
	}
}
