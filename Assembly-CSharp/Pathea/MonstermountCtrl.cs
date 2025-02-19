using System.Collections;
using System.Collections.Generic;
using System.IO;
using PeEvent;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class MonstermountCtrl : PeCmpt
{
	public delegate void AccomplishMove();

	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	private const int TameCampID = 28;

	private const int TameDefaultPlayerID = 1;

	private const int TameDamageID = 28;

	private const float m_MoveAttackSpeed = 4f;

	private const float m_SprintAttackSpeed = 6f;

	private const float RestoreWaitTime = 20f;

	private Motion_Move_Motor m_Move;

	private Vector3 m_AutoMoveDir = Vector3.zero;

	private SpeedState m_AutoSpeed = SpeedState.Walk;

	private float m_AutoTime0;

	private float m_AutoTime1;

	private int m_TameSkill;

	private PeEntity m_Master;

	private Vector3 m_MoveDir = Vector3.zero;

	private bool m_Init;

	private bool m_MoveWalk;

	private AccomplishMove m_CallBack;

	public ForceData m_PlayerForceDb = new ForceData();

	public ForceData m_MountsForceDb;

	public BaseSkillData m_SkillData = new BaseSkillData();

	public ECtrlType ctrlType { get; private set; }

	[ContextMenu("StopAI")]
	public void StopAI()
	{
		if (base.Entity != null && base.Entity.BehaveCmpt != null)
		{
			base.Entity.BehaveCmpt.Stop();
		}
	}

	[ContextMenu("ExcuteAI")]
	private void ExcuteAI()
	{
		if (base.Entity != null && base.Entity.BehaveCmpt != null)
		{
			base.Entity.BehaveCmpt.Excute();
		}
	}

	private void UpdateMoveState()
	{
		if (ctrlType == ECtrlType.Mount && !base.Entity.skEntity.IsSkillRunning() && !(m_Move == null))
		{
			Vector3 vector = PeInput.GetAxisH() * Vector3.right + PeInput.GetAxisV() * Vector3.forward;
			m_MoveDir = Vector3.ProjectOnPlane(PEUtil.MainCamTransform.rotation * vector, Vector3.up);
			if (PeInput.Get(PeInput.LogicFunction.SwitchWalkRun))
			{
				m_MoveWalk = !m_MoveWalk;
			}
			SpeedState state = (m_MoveWalk ? SpeedState.Walk : SpeedState.Run);
			if (PeInput.Get(PeInput.LogicFunction.Sprint) && base.Entity.GetAttribute(AttribType.SprintSpeed) > 0f)
			{
				state = SpeedState.Sprint;
			}
			m_Move.Move(m_MoveDir.normalized, state);
			if ((m_SkillData.canSpace() || m_SkillData.canProunce()) && PeInput.Get(PeInput.LogicFunction.Jump))
			{
				Jump();
			}
			if (m_SkillData.canAttack() && PeInput.Get(PeInput.LogicFunction.Attack))
			{
				AttackL();
			}
		}
	}

	private void UpdataTamingMove()
	{
		if (ctrlType != ECtrlType.Taming)
		{
			return;
		}
		if (m_AutoTime1 != -1f)
		{
			if (Time.time - m_AutoTime0 < m_AutoTime1)
			{
				m_Move.Move(m_AutoMoveDir.normalized, m_AutoSpeed);
			}
			if (Time.time - m_AutoTime0 >= m_AutoTime1)
			{
				m_Move.Stop();
				m_AutoTime1 = -1f;
				if (m_CallBack != null)
				{
					m_CallBack();
				}
			}
		}
		if (m_TameSkill != 0 && !base.Entity.skEntity.IsSkillRunning(m_TameSkill))
		{
			m_TameSkill = 0;
			if (m_CallBack != null)
			{
				m_CallBack();
			}
		}
	}

	private bool InitMountData()
	{
		if (!base.Entity)
		{
			return false;
		}
		m_Move = base.Entity.motionMove as Motion_Move_Motor;
		m_Move.Stop();
		SetctrlType(ECtrlType.Mount);
		PeSingleton<FastTravelMgr>.Instance.OnFastTravel += OnFastTravel;
		m_PlayerForceDb = new ForceData((int)m_Master.GetAttribute(AttribType.CampID), (int)m_Master.GetAttribute(AttribType.DamageID), (int)m_Master.GetAttribute(AttribType.DefaultPlayerID));
		if (m_MountsForceDb == null)
		{
			m_MountsForceDb = new ForceData((int)base.Entity.GetAttribute(AttribType.CampID), (int)base.Entity.GetAttribute(AttribType.DamageID), (int)base.Entity.GetAttribute(AttribType.DefaultPlayerID));
		}
		if (!m_SkillData.canUse())
		{
			m_SkillData.Reset(MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_L), MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_Space), MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_pounce));
		}
		StartMountsForceDb();
		DispatchEvent(base.Entity);
		if (base.Entity.animCmpt != null)
		{
			base.Entity.animCmpt.SetBool("Sleep", value: false);
		}
		m_Init = true;
		return true;
	}

	private void InitTameData(PeEntity master)
	{
		if ((bool)base.Entity)
		{
			m_Move = base.Entity.motionMove as Motion_Move_Motor;
			m_Master = master;
			SetctrlType(ECtrlType.Free);
			if (!m_SkillData.canUse())
			{
				m_SkillData.Reset(MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_L), MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_Space), MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_pounce));
			}
			m_PlayerForceDb = new ForceData((int)m_Master.GetAttribute(AttribType.CampID), (int)m_Master.GetAttribute(AttribType.DamageID), (int)m_Master.GetAttribute(AttribType.DefaultPlayerID));
			if (m_MountsForceDb == null)
			{
				m_MountsForceDb = new ForceData((int)base.Entity.GetAttribute(AttribType.CampID), (int)base.Entity.GetAttribute(AttribType.DamageID), (int)base.Entity.GetAttribute(AttribType.DefaultPlayerID));
			}
		}
	}

	private void EndMount()
	{
		m_Move = null;
		m_Init = false;
		SetctrlType(ECtrlType.Free);
		if ((bool)base.Entity.target)
		{
			base.Entity.target.ClearEnemy();
		}
		PeSingleton<FastTravelMgr>.Instance.OnFastTravel -= OnFastTravel;
		DelEvent(base.Entity);
	}

	private void StartMountsForceDb()
	{
		if (!(base.Entity == null) && m_PlayerForceDb != null)
		{
			base.Entity.SetAttribute(AttribType.CampID, m_PlayerForceDb._campID);
			base.Entity.SetAttribute(AttribType.DamageID, m_PlayerForceDb._damageID);
			base.Entity.SetAttribute(AttribType.DefaultPlayerID, m_PlayerForceDb._defaultPlyerID);
		}
	}

	private void StartTamingForceDb()
	{
		if (!(base.Entity == null))
		{
			base.Entity.SetAttribute(AttribType.CampID, 28f);
			base.Entity.SetAttribute(AttribType.DamageID, 28f);
			base.Entity.SetAttribute(AttribType.DefaultPlayerID, 1f);
		}
	}

	private void RestoreMonsterForceDb()
	{
		if ((bool)base.Entity && m_MountsForceDb != null)
		{
			base.Entity.SetAttribute(AttribType.CampID, m_MountsForceDb._campID);
			base.Entity.SetAttribute(AttribType.DamageID, m_MountsForceDb._damageID);
			base.Entity.SetAttribute(AttribType.DefaultPlayerID, m_MountsForceDb._defaultPlyerID);
			base.Entity.target.ClearEnemy();
			m_MountsForceDb = null;
		}
	}

	private SpeedState calculateSpeed(SpeedState speed)
	{
		switch (speed)
		{
		case SpeedState.Sprint:
			if (base.Entity.GetAttribute(AttribType.SprintSpeed) > 0f)
			{
				return SpeedState.Sprint;
			}
			if (base.Entity.GetAttribute(AttribType.RunSpeed) > 0f)
			{
				return SpeedState.Run;
			}
			return SpeedState.Walk;
		case SpeedState.Run:
			if (base.Entity.GetAttribute(AttribType.RunSpeed) > 0f)
			{
				return SpeedState.Run;
			}
			return SpeedState.Walk;
		default:
			return SpeedState.Walk;
		}
	}

	private void OnAttack(SkEntity skEntity, float damage)
	{
		PeEntity component = skEntity.GetComponent<PeEntity>();
		TransferHared(component, damage);
	}

	private void OnDamage(SkEntity entity, float damage)
	{
		if (!(null == base.Entity) && !(null == entity))
		{
			PeEntity component = entity.GetComponent<PeEntity>();
			if (!(component == base.Entity))
			{
				TransferHared(component, damage);
			}
		}
	}

	private void OnSkillTarget(SkEntity caster)
	{
		if (!(null == base.Entity) && !(null == caster))
		{
			PeEntity component = caster.GetComponent<PeEntity>();
			TransferHared(component, 10f);
		}
	}

	private void DispatchEvent(PeEntity mount)
	{
		if ((bool)mount && (bool)mount.aliveEntity)
		{
			mount.aliveEntity.attackEvent += OnAttack;
			mount.aliveEntity.onHpReduce += OnDamage;
			mount.aliveEntity.onSkillEvent += OnSkillTarget;
		}
	}

	private void DelEvent(PeEntity mount)
	{
		if ((bool)mount && (bool)mount.aliveEntity)
		{
			mount.aliveEntity.attackEvent -= OnAttack;
			mount.aliveEntity.onHpReduce -= OnDamage;
			mount.aliveEntity.onSkillEvent -= OnSkillTarget;
		}
	}

	private void TransferHared(PeEntity targetentity, float damage)
	{
		float radius = ((!targetentity.IsBoss) ? 64f : 128f);
		int playerID = (int)base.Entity.GetAttribute(AttribType.DefaultPlayerID);
		List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(base.Entity.position, radius, playerID, isDeath: false, base.Entity);
		for (int i = 0; i < entities.Count; i++)
		{
			if (!entities[i].Equals(targetentity) && entities[i].target != null)
			{
				entities[i].target.TransferHatred(targetentity, damage);
			}
		}
	}

	private void ResetMonster()
	{
		m_Move.Stop();
		m_Move.SetSpeed(0f);
		base.Entity.animCmpt.SetBool(m_SkillData.m_PounceAnim, value: false);
		m_Move.Move(Vector3.zero);
		m_Move.RotateTo(Vector3.zero);
	}

	public void OnFastTravel(Vector3 pos)
	{
		if (!(base.Entity == null) && ctrlType == ECtrlType.Mount)
		{
			base.Entity.peTrans.position = pos;
			SubscribeEndLoad();
		}
	}

	private void OnResponse(object sender, EventArg arg)
	{
		StartCoroutine(EmutiWait(arg));
	}

	public void InitTame(PeEntity master)
	{
		if ((bool)base.Entity && (bool)master)
		{
			InitTameData(master);
		}
	}

	public void StartPlayerCtrl(PeEntity master)
	{
		if ((bool)base.Entity && (bool)master)
		{
			m_Master = master;
			base.Entity.BehaveCmpt.Stop();
			if (!m_Init)
			{
				InitMountData();
			}
			SetctrlType(ECtrlType.Mount);
		}
	}

	public void LoadCtrl(PeEntity master, MousePickRides peride)
	{
		m_Master = master;
		StartCoroutine(EloadMount(peride));
	}

	public void PausePlayerCtrl()
	{
		SetctrlType(ECtrlType.Wait);
	}

	private void FreeMonster()
	{
		if ((bool)base.Entity)
		{
			if (m_SkillData != null)
			{
				m_SkillData.Reset();
			}
			StartCoroutine(ERestoreMonsterWait(Time.time));
			EndMount();
		}
	}

	public void Taming()
	{
		SetctrlType(ECtrlType.Taming);
		StartTamingForceDb();
	}

	public void TameSucceed()
	{
		base.Entity.BehaveCmpt.Stop();
		SetctrlType(ECtrlType.Mount);
		InitMountData();
		SubscribeEndLoad();
		SteamAchievementsSystem.Instance.OnGameStateChange(Eachievement.Mounts_Rider);
	}

	public void TameFailure()
	{
		SetctrlType(ECtrlType.Free);
		StopAllCoroutines();
		UnsubscribeEndLoad();
		ResetMonster();
		FreeMonster();
	}

	public void MoveDirtion(Vector3 dir, AccomplishMove callback, float time = 2f, SpeedState state = SpeedState.Walk)
	{
		if (!(dir == Vector3.zero))
		{
			m_AutoMoveDir = dir;
			m_CallBack = callback;
			m_AutoTime0 = Time.time;
			m_AutoTime1 = time;
			m_AutoSpeed = calculateSpeed(state);
		}
	}

	public void Forward(AccomplishMove callback, float time = 2f, SpeedState state = SpeedState.Walk)
	{
		Vector3 forward = Vector3.forward;
		m_AutoMoveDir = Vector3.ProjectOnPlane(PEUtil.MainCamTransform.rotation * forward, Vector3.up);
		m_CallBack = callback;
		m_AutoTime0 = Time.time;
		m_AutoTime1 = time;
		m_AutoSpeed = calculateSpeed(state);
	}

	public void Back(AccomplishMove callback, float time = 2f, SpeedState state = SpeedState.Walk)
	{
		Vector3 vector = -Vector3.forward;
		m_AutoMoveDir = Vector3.ProjectOnPlane(PEUtil.MainCamTransform.rotation * vector, Vector3.up);
		m_CallBack = callback;
		m_AutoTime0 = Time.time;
		m_AutoTime1 = time;
		m_AutoSpeed = calculateSpeed(state);
	}

	public void Right(AccomplishMove callback, float time = 2f, SpeedState state = SpeedState.Walk)
	{
		Vector3 right = Vector3.right;
		m_AutoMoveDir = Vector3.ProjectOnPlane(PEUtil.MainCamTransform.rotation * right, Vector3.up);
		m_CallBack = callback;
		m_AutoTime0 = Time.time;
		m_AutoTime1 = time;
		m_AutoSpeed = calculateSpeed(state);
	}

	public void Left(AccomplishMove callback, float time = 2f, SpeedState state = SpeedState.Walk)
	{
		Vector3 vector = -Vector3.right;
		m_AutoMoveDir = Vector3.ProjectOnPlane(PEUtil.MainCamTransform.rotation * vector, Vector3.up);
		m_CallBack = callback;
		m_AutoTime0 = Time.time;
		m_AutoTime1 = time;
		m_AutoSpeed = calculateSpeed(state);
	}

	public void Jump()
	{
		if (m_SkillData.canSpace())
		{
			m_Move.Stop();
			base.Entity.StartSkill(null, m_SkillData.m_SKillIDSpace);
		}
		else if (m_SkillData.canProunce() && (bool)m_Master)
		{
			m_Move.Stop();
			if (PeGameMgr.IsMulti && base.Entity != null && base.Entity.skEntity != null && base.Entity.skEntity._net != null && base.Entity.skEntity.IsController() && base.Entity.skEntity._net is AiNetwork)
			{
				(base.Entity.skEntity._net as AiNetwork).RequestSetBool(Animator.StringToHash(m_SkillData.m_PounceAnim), b: true);
			}
			base.Entity.animCmpt.SetBool(m_SkillData.m_PounceAnim, value: true);
			base.Entity.StartSkill(m_Master.skEntity, m_SkillData.m_Skillpounce);
			Vector3 dirtionPostion = PEUtil.GetDirtionPostion(base.Entity.position, base.Entity.forward, 8f, 20f, 0f, 0f, 12f);
			if (dirtionPostion != Vector3.zero)
			{
				StartCoroutine(Epounce(Time.time, dirtionPostion));
			}
		}
	}

	public void Jump(AccomplishMove callback)
	{
		int randomSkill = MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_tame);
		if (randomSkill != 0)
		{
			m_Move.Stop();
			base.Entity.StartSkill(null, randomSkill);
			m_TameSkill = randomSkill;
			m_CallBack = callback;
		}
	}

	public void HitFly(Vector3 forceDir, string trsStr = "Bip01 L Thigh", float forcePower = 750f)
	{
		if (!(m_Master == null) && !(m_Master.skEntity == null) && !(m_Master.motionBeat == null))
		{
			PEActionParamVFNS param = PEActionParamVFNS.param;
			param.vec = forceDir;
			param.f = forcePower;
			param.n = m_Master.Id;
			param.str = trsStr;
			m_Master.motionMgr.DoAction(PEActionType.Wentfly, param);
		}
	}

	public bool HasJump()
	{
		return MountsSkillDb.GetRandomSkill(base.Entity.ProtoID, MountsSkillKey.Mskill_tame) != 0;
	}

	public void AttackL()
	{
		if (m_SkillData.m_SkillL != 0)
		{
			m_Move.Stop();
			base.Entity.StartSkill(null, m_SkillData.m_SkillL);
		}
	}

	public void ResetMountsSkill(BaseSkillData data)
	{
		if (m_SkillData == null)
		{
			m_SkillData = new BaseSkillData();
		}
		m_SkillData.Reset(data.m_SkillL, data.m_SKillIDSpace, data.m_Skillpounce);
	}

	public void SetctrlType(ECtrlType type)
	{
		ctrlType = type;
	}

	public void SubscribeEndLoad()
	{
		PeLauncher.Instance.eventor.Subscribe(OnResponse);
	}

	public void UnsubscribeEndLoad()
	{
		PeLauncher.Instance.eventor.Unsubscribe(OnResponse);
	}

	private IEnumerator ERestoreMonsterWait(float time)
	{
		while (Time.time - time < 20f)
		{
			if (ctrlType != 0)
			{
				yield break;
			}
			yield return null;
		}
		if (ctrlType == ECtrlType.Free)
		{
			RestoreMonsterForceDb();
		}
	}

	private IEnumerator EmutiWait(EventArg arg)
	{
		if (PeGameMgr.IsMulti && NetworkInterface.IsClient && arg is PeLauncher.LoadFinishedArg)
		{
			while (base.Entity == null || PeSingleton<PeCreature>.Instance == null || null == base.Entity.biologyViewCmpt || null == base.Entity.biologyViewCmpt.biologyViewRoot || null == base.Entity.biologyViewCmpt.biologyViewRoot.modelController)
			{
				yield return null;
			}
			base.Entity.BehaveCmpt.Stop();
			if (null != m_Master && m_Master.Id == PeSingleton<PeCreature>.Instance.mainPlayerId)
			{
				MousePickRides rides = base.Entity.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
				if ((bool)rides)
				{
					rides.RecoverExecRide(m_Master);
				}
			}
		}
		yield return null;
	}

	private IEnumerator Epounce(float time, Vector3 targetPos)
	{
		if (base.Entity == null || m_Move == null || base.Entity.animCmpt == null)
		{
			yield break;
		}
		PounceData m_Data = null;
		if (MonsterXmlData.GetData(base.Entity.ProtoID, ref m_Data))
		{
			float _speed = (PEUtil.MagnitudeH(base.Entity.position, targetPos) + 1f) / (m_Data._endTime - m_Data._startTime);
			while (Time.time - time < m_Data._startTime)
			{
				m_Move.Stop();
				yield return null;
			}
			while (Time.time - time <= m_Data._endTime)
			{
				m_Move.SetSpeed(_speed);
				m_Move.Move(base.Entity.forward);
				Vector3 m_Direction = targetPos - base.Entity.position;
				Vector3 m_DirectionXZ = Vector3.ProjectOnPlane(m_Direction, Vector3.up);
				m_Move.RotateTo(m_DirectionXZ);
				yield return null;
			}
			ResetMonster();
			yield return null;
		}
	}

	private IEnumerator EloadMount(MousePickRides rides)
	{
		if (base.Entity == null || !base.Entity.hasView || null == base.Entity.biologyViewCmpt || null == base.Entity.biologyViewCmpt.monoRagdollCtrlr)
		{
			yield return null;
		}
		yield return null;
		if ((bool)rides && rides.ExecRide(m_Master))
		{
			StartPlayerCtrl(m_Master);
			if ((bool)m_Master.mountCmpt)
			{
				m_Master.mountCmpt.SetMount(base.Entity);
			}
		}
	}

	public override void Start()
	{
		base.Start();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdataTamingMove();
		UpdateMoveState();
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		r.ReadInt32();
		ctrlType = (ECtrlType)r.ReadInt32();
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(0);
		w.Write((int)ctrlType);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (PeGameMgr.IsMulti && m_Master != null && PeSingleton<PeCreature>.Instance != null && base.Entity.biologyViewCmpt != null && base.Entity.biologyViewCmpt.biologyViewRoot != null && base.Entity.biologyViewCmpt.biologyViewRoot.modelController != null && m_Master.Id == PeSingleton<PeCreature>.Instance.mainPlayerId)
		{
			MousePickRides component = base.Entity.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
			if (component != null)
			{
				component.ExecUnRide(m_Master);
			}
		}
	}
}
