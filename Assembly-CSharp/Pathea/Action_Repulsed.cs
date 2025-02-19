using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Repulsed : PEAction
{
	[HideInInspector]
	public Motion_Move_Motor m_Move;

	public BehaveCmpt m_Behave;

	public AnimationCurve m_TimeVelocityScale;

	public AnimationCurve m_ForceToVelocity;

	public AnimationCurve m_ForceToMoveTime;

	public AnimationCurve m_ApplyMoveStopTime;

	[HideInInspector]
	public BeatParam m_Param;

	private Vector3 m_MoveDir;

	private float m_MoveVelocity;

	private float m_FinalVelocity;

	private float m_MoveTime;

	private float m_ElapseTime;

	private float m_MoveStopTime;

	private bool m_ApplyStopEffect;

	private static readonly float BlockForceScale = 0.5f;

	private static readonly float LerpScale = 5f;

	public AnimationCurve m_WentflyTimeCurve;

	public float m_ForceScale = 1.5f;

	public string m_ApplayForceBoneName = "Bip01 Spine2";

	private float m_WentflyTime;

	private float m_ForcePower;

	private Transform m_AddForceTrans;

	private bool m_IsBlockRepulsed;

	public HumanPhyCtrl phyCtrl { get; set; }

	public override PEActionType ActionType => PEActionType.Repulsed;

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == base.trans) && !(null == m_Param))
		{
			m_IsBlockRepulsed = base.motionMgr.IsActionRunning(PEActionType.HoldShield) && Vector3.Angle(-base.entity.forward, m_MoveDir) < 30f;
			PEActionParamVVF param = PEActionParamVVF.param;
			base.motionMgr.SetMaskState(PEActionMask.Repulsed, state: true);
			base.trans.position = (base.trans.position + param.vec1) / 2f;
			m_MoveDir = param.vec2;
			m_MoveDir = Vector3.ProjectOnPlane(m_MoveDir, base.trans.existent.up).normalized;
			m_ForcePower = param.f * ((!m_IsBlockRepulsed) ? 1f : BlockForceScale);
			m_AddForceTrans = m_Param.m_ApplyWentflyBone;
			m_MoveVelocity = m_Param.m_ForceToVelocity.Evaluate(m_ForcePower);
			m_MoveTime = m_Param.m_ForceToMoveTime.Evaluate(m_ForcePower);
			m_MoveStopTime = m_Param.m_ApplyMoveStopTime.Evaluate(m_ForcePower);
			m_WentflyTime = m_Param.m_WentflyTimeCurve.Evaluate(m_ForcePower);
			m_ElapseTime = 0f;
			m_ApplyStopEffect = false;
			if (null != base.anim)
			{
				base.anim.SetFloat("SheildBlockF", (!m_IsBlockRepulsed) ? 0f : 1f);
			}
			if (null != m_Move)
			{
				m_Move.ApplyForce(m_MoveVelocity * m_MoveDir, ForceMode.VelocityChange);
			}
		}
	}

	public override bool Update()
	{
		if (null == base.trans || null == m_Param)
		{
			EndImmediately();
			return true;
		}
		if (null != m_AddForceTrans && m_ElapseTime > m_WentflyTime)
		{
			PEActionParamVFNS param = PEActionParamVFNS.param;
			param.vec = m_MoveDir.normalized;
			param.f = m_Param.m_ToWentflyForceScale * m_ForcePower;
			param.n = base.motionMgr.Entity.Id;
			param.str = m_AddForceTrans.name;
			base.motionMgr.DoAction(PEActionType.Wentfly, param);
			EndImmediately();
			return true;
		}
		if (m_ElapseTime < m_MoveTime)
		{
			float num = 1f;
			if (m_Param.m_TimeVelocityScale != null)
			{
				num = m_Param.m_TimeVelocityScale.Evaluate(m_ElapseTime / m_MoveTime);
			}
			m_FinalVelocity = m_MoveVelocity * num;
			if (null != phyCtrl)
			{
				phyCtrl.ApplyMoveRequest(m_MoveDir * m_FinalVelocity);
			}
			if (null != m_Move)
			{
				m_Move.ApplyForce(m_MoveDir * m_FinalVelocity, ForceMode.VelocityChange);
			}
			if (!m_ApplyStopEffect && m_ElapseTime > m_MoveStopTime)
			{
				m_ApplyStopEffect = true;
				base.motionMgr.DoAction(PEActionType.Halt);
			}
			m_ElapseTime += Time.deltaTime;
			if (null != base.anim && null != phyCtrl)
			{
				Vector3 vector = Quaternion.Inverse(base.trans.rotation) * phyCtrl.velocity;
				base.anim.SetFloat("ForwardSpeed", vector.z);
				base.anim.SetFloat("RightSpeed", vector.x);
				base.anim.SetFloat("BeatWeight", Mathf.Lerp(base.anim.GetFloat("BeatWeight"), 1f, LerpScale * Time.deltaTime));
				base.anim.SetFloat("SheildBlockF", Mathf.Lerp(base.anim.GetFloat("SheildBlockF"), (!m_IsBlockRepulsed) ? 0f : 1f, LerpScale * Time.deltaTime));
			}
			return false;
		}
		EndImmediately();
		return true;
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.SetFloat("BeatWeight", 0f);
			base.anim.SetFloat("SheildBlockF", 0f);
		}
		base.motionMgr.SetMaskState(PEActionMask.Repulsed, state: false);
		m_ElapseTime = m_MoveTime;
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
			phyCtrl.CancelMoveRequest();
		}
		if (null != m_Move)
		{
			m_Move.ApplyForce(Vector3.zero, ForceMode.VelocityChange);
		}
	}
}
