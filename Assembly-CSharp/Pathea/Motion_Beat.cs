using System;
using PEIK;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class Motion_Beat : PeCmpt, IPeMsg
{
	public enum EffectType
	{
		Null,
		Whacked,
		Repulsed,
		Wentfly,
		Knocked
	}

	private SkAliveEntity m_SkillCmpt;

	private PeTrans m_PeTrans;

	private HumanPhyCtrl m_PhyCtrl;

	private MotionMgrCmpt m_MotionMgr;

	private IKAnimEffectCtrl m_IKAnimCtrl;

	public float m_RandomScale = 0.1f;

	public float m_ThresholdScaleInAir = 0.5f;

	private BeatParam m_Param;

	private float m_LastHitSoundTime;

	public Action_Whacked m_Whacked;

	public Action_Repulsed m_Repulsed;

	public Action_Wentfly m_Wentfly;

	public Action_Knocked m_Knocked;

	public Action_GetUp m_GetUp;

	public Action_Death m_Death;

	public Action_AlienDeath m_AlienDeath;

	public Action_Revive m_Revive;

	public event Action<PeEntity> onHitTarget;

	public override void Start()
	{
		base.Start();
		m_SkillCmpt = base.Entity.aliveEntity;
		m_PeTrans = base.Entity.peTrans;
		m_Repulsed.m_Behave = base.Entity.GetCmpt<BehaveCmpt>();
		m_Repulsed.m_Move = base.Entity.GetCmpt<Motion_Move_Motor>();
		m_MotionMgr = base.Entity.motionMgr;
		if (null != m_MotionMgr)
		{
			m_MotionMgr.AddAction(m_Whacked);
			m_MotionMgr.AddAction(m_Repulsed);
			m_MotionMgr.AddAction(m_Wentfly);
			m_MotionMgr.AddAction(m_Knocked);
			m_MotionMgr.AddAction(m_GetUp);
			m_MotionMgr.AddAction(m_Death);
			m_MotionMgr.AddAction(m_AlienDeath);
			m_MotionMgr.AddAction(m_Revive);
		}
	}

	private void ApplyHitEffect(Transform trans, Vector3 forceDir, float forcePower)
	{
		if (!(null == m_Param))
		{
			float time = forcePower * (1f + UnityEngine.Random.Range(0f - m_Param.m_RandomScale, m_Param.m_RandomScale));
			float weight = m_Param.m_ForceToHitWeight.Evaluate(time);
			float effectTime = m_Param.m_ForceToHitTime.Evaluate(time);
			if (null != m_IKAnimCtrl)
			{
				m_IKAnimCtrl.OnHit(trans, forceDir, weight, effectTime);
			}
		}
	}

	private void ApplyMoveEffect(SkEntity skEntity, Transform trans, Vector3 forceDir, float forcePower)
	{
		if (null == m_Param)
		{
			return;
		}
		float time = Vector3.Angle(m_PeTrans.existent.forward, forceDir);
		float num = m_Param.m_AngleThresholdScale.Evaluate(time);
		forcePower *= m_Param.m_AngleForceScale.Evaluate(time);
		num *= ((!m_MotionMgr.GetMaskState(PEActionMask.InAir)) ? 1f : m_Param.m_ThresholdScaleInAir);
		EffectType effectType = EffectType.Repulsed;
		effectType = ((!(forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdWhacked) * num)) ? ((forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdRepulsed) * num) ? EffectType.Whacked : ((forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdWentfly) * num) ? EffectType.Repulsed : ((!(forcePower < m_SkillCmpt.GetAttribute(AttribType.ThresholdKnocked) * num)) ? EffectType.Knocked : EffectType.Wentfly))) : EffectType.Null);
		SkAliveEntity skAliveEntity = skEntity as SkAliveEntity;
		if (!(null != skAliveEntity))
		{
			return;
		}
		switch (effectType)
		{
		case EffectType.Null:
			ApplyHitEffect(trans, forceDir, forcePower);
			break;
		case EffectType.Whacked:
			m_MotionMgr.DoAction(PEActionType.Whacked);
			break;
		case EffectType.Repulsed:
		{
			ApplyHitEffect(trans, forceDir, forcePower);
			PEActionParamVVF param2 = PEActionParamVVF.param;
			param2.vec1 = m_PeTrans.position;
			param2.vec2 = forceDir;
			param2.f = forcePower;
			m_MotionMgr.DoAction(PEActionType.Repulsed, param2);
			break;
		}
		case EffectType.Wentfly:
		case EffectType.Knocked:
		{
			PEActionParamVFNS param = PEActionParamVFNS.param;
			param.vec = forceDir;
			param.f = forcePower;
			param.n = skAliveEntity.Entity.Id;
			if (null != trans)
			{
				param.str = trans.name;
			}
			else
			{
				param.str = string.Empty;
			}
			m_MotionMgr.DoAction(PEActionType.Wentfly, param);
			break;
		}
		}
	}

	private void ApplyBeenHitSound(Transform trans)
	{
		if (base.Entity.IsDeath())
		{
			return;
		}
		switch (base.Entity.proto)
		{
		case EEntityProto.Monster:
		{
			MonsterProtoDb.Item item = MonsterProtoDb.Get(base.Entity.ProtoID);
			if (item.beHitSound[0] > 0 && Time.time - m_LastHitSoundTime >= (float)item.beHitSound[item.beHitSound.Length - 1])
			{
				m_LastHitSoundTime = Time.time;
				Vector3 position = ((!(null != trans)) ? base.Entity.position : trans.position);
				AudioManager.instance.Create(position, item.beHitSound[0]);
			}
			break;
		}
		case EEntityProto.Player:
		case EEntityProto.Npc:
			PlayMaleAudio();
			break;
		case EEntityProto.RandomNpc:
			break;
		}
	}

	private void PlayMaleAudio()
	{
		AudioManager.instance.Create(base.Entity.position, UnityEngine.Random.Range(935, 937));
	}

	public void Beat(SkEntity skEntity, Transform trans, Vector3 forceDir, float forcePower)
	{
		ApplyMoveEffect(skEntity, trans, forceDir, forcePower);
		ApplyBeenHitSound(trans);
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
		{
			BiologyViewRoot biologyViewRoot = args[1] as BiologyViewRoot;
			m_Param = biologyViewRoot.beatParam;
			m_PhyCtrl = biologyViewRoot.humanPhyCtrl;
			m_IKAnimCtrl = biologyViewRoot.ikAnimEffectCtrl;
			m_Repulsed.phyCtrl = m_PhyCtrl;
			m_Wentfly.phyCtrl = m_PhyCtrl;
			m_Knocked.phyCtrl = m_PhyCtrl;
			m_Repulsed.m_Param = m_Param;
			break;
		}
		case EMsg.View_Ragdoll_Fall_Finished:
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.grounded = true;
			}
			break;
		case EMsg.Battle_AttackHit:
			if (this.onHitTarget != null)
			{
				PeEntity componentInParent = ((PECapsuleHitResult)args[0]).hitTrans.GetComponentInParent<PeEntity>();
				this.onHitTarget(componentInParent);
			}
			break;
		}
	}
}
