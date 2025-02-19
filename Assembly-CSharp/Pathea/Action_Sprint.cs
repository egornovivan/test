using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Sprint : PEAction
{
	[HideInInspector]
	public MoveParam m_Param;

	[HideInInspector]
	public float m_MoveSpeed = 8f;

	private Vector3 m_Target;

	private bool m_EndFlag;

	private bool m_FastRotat;

	private Vector3 m_LastVelocity;

	private MoveType m_MoveType;

	private float m_SpeedScale;

	[HideInInspector]
	public bool m_ApplyStopIK;

	public bool m_UseStamina;

	public override PEActionType ActionType => PEActionType.Sprint;

	public HumanPhyCtrl phyCtrl { get; set; }

	public override bool CanDoAction(PEActionParam para = null)
	{
		PEActionParamNVB pEActionParamNVB = para as PEActionParamNVB;
		MoveType n = (MoveType)pEActionParamNVB.n;
		Vector3 vec = pEActionParamNVB.vec;
		if (m_UseStamina && null != m_Param && base.motionMgr.Entity.GetAttribute(AttribType.Stamina) < m_Param.m_MinStamina)
		{
			return false;
		}
		if (n == MoveType.Direction && vec == Vector3.zero)
		{
			return false;
		}
		if (n == MoveType.Target && Vector3.SqrMagnitude(base.trans.position - m_Target) < MoveParam.AutoMoveStopSqrDis * MoveParam.AutoMoveStopSqrDis)
		{
			return false;
		}
		return true;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == phyCtrl) && !(null == base.anim) && !(null == base.trans))
		{
			PEActionParamNVB pEActionParamNVB = para as PEActionParamNVB;
			m_MoveType = (MoveType)pEActionParamNVB.n;
			m_Target = pEActionParamNVB.vec;
			m_FastRotat = pEActionParamNVB.b;
			m_EndFlag = false;
			base.motionMgr.SetMaskState(PEActionMask.Sprint, state: true);
			phyCtrl.ResetSpeed(m_MoveSpeed);
			phyCtrl.desiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
			Vector3 vector = Quaternion.Inverse(base.trans.rotation) * phyCtrl.velocity;
			if (null != base.anim.animator)
			{
				base.anim.animator.SetFloat("ForwardSpeed", vector.z);
				base.anim.animator.SetBool("StartMove", value: true);
			}
			m_SpeedScale = m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle);
		}
	}

	public override void ResetAction(PEActionParam para = null)
	{
		PEActionParamNVB pEActionParamNVB = para as PEActionParamNVB;
		m_MoveType = (MoveType)pEActionParamNVB.n;
		m_Target = pEActionParamNVB.vec;
	}

	public override void PauseAction()
	{
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
		}
		if (null != base.anim && null != base.anim.animator)
		{
			base.anim.animator.SetBool("StartMove", value: false);
		}
	}

	public override void ContinueAction()
	{
		if (null != base.anim && null != base.anim.animator)
		{
			base.anim.animator.SetBool("StartMove", value: true);
		}
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
		}
	}

	public override bool Update()
	{
		if (null == phyCtrl || null == base.anim || null == base.trans || null == m_Param || m_EndFlag)
		{
			EndImmediately();
			return true;
		}
		if (base.pauseAction)
		{
			return false;
		}
		if (m_UseStamina)
		{
			float attribute = base.motionMgr.Entity.GetAttribute(AttribType.Stamina);
			attribute -= m_Param.m_StaminaCostSpeed * base.motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent) * Time.deltaTime;
			if (attribute <= float.Epsilon)
			{
				attribute = 0f;
				base.motionMgr.EndImmediately(ActionType);
				return true;
			}
			base.motionMgr.Entity.SetAttribute(AttribType.Stamina, attribute, offEvent: false);
		}
		Vector3 vector = Vector3.zero;
		switch (m_MoveType)
		{
		case MoveType.Direction:
			if (m_Target != Vector3.zero)
			{
				vector = m_Target.normalized;
			}
			break;
		case MoveType.Target:
			vector = m_Target - base.trans.position;
			if (vector.magnitude < MoveParam.AutoMoveStopSqrDis)
			{
				EndAction();
			}
			if (vector.sqrMagnitude > 1f)
			{
				vector.Normalize();
			}
			break;
		}
		float num = 1f;
		if (m_FastRotat)
		{
			num *= m_Param.m_FastRotatScale;
		}
		if (vector != Vector3.zero)
		{
			Vector3 normalized = vector.normalized;
			if (!phyCtrl.spineInWater)
			{
				normalized = Vector3.ProjectOnPlane(normalized, Vector3.up).normalized;
			}
			normalized = ((!(Vector3.Angle(base.trans.forward, normalized) > 150f)) ? Vector3.Slerp(base.trans.forward, normalized, m_Param.m_SprintRotateSpeed * num * Time.deltaTime) : Vector3.Slerp(base.trans.forward, base.trans.existent.right, m_Param.m_MoveRotateSpeed * Time.deltaTime));
			base.trans.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		}
		else if (phyCtrl.spineInWater)
		{
			Vector3 forward = Vector3.Slerp(base.trans.forward, Vector3.ProjectOnPlane(base.trans.forward, Vector3.up).normalized, m_Param.m_SprintRotateSpeed * Time.deltaTime);
			base.trans.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
		Vector3 vector2 = Quaternion.AngleAxis(0f - phyCtrl.forwardGroundAngle, base.trans.existent.right) * vector;
		Vector3 vector3 = Quaternion.Inverse(base.trans.rotation) * vector2.normalized;
		if (vector == Vector3.zero)
		{
			vector3.z = 0f;
			vector3.y = 0f;
		}
		else
		{
			vector3.z = 1f;
		}
		vector3.x *= m_Param.m_SprintSizeScale;
		if (phyCtrl.spineInWater)
		{
			m_SpeedScale = Mathf.Lerp(m_SpeedScale, 1f, m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
		}
		else
		{
			m_SpeedScale = Mathf.Lerp(m_SpeedScale, m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle), m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
		}
		if (vector != Vector3.zero)
		{
			phyCtrl.ResetSpeed(m_MoveSpeed * m_SpeedScale);
		}
		float num2 = m_MoveSpeed * m_SpeedScale * vector3.magnitude;
		if (base.motionMgr.isInAimState)
		{
			phyCtrl.desiredMovementDirection = base.trans.rotation * vector3;
		}
		else
		{
			phyCtrl.desiredMovementDirection = Vector3.Lerp(phyCtrl.desiredMovementDirection, base.trans.rotation * vector3, m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
		}
		if (null != base.anim && null != base.anim.animator)
		{
			Animator animator = base.anim.animator;
			phyCtrl.desiredMovementDirection *= animator.GetFloat("RunSpeedF");
			Vector3 vector4 = Quaternion.Inverse(base.trans.rotation) * phyCtrl.velocity.normalized * num2;
			vector4.x = 0f;
			animator.SetFloat("ForwardSpeed", Mathf.Lerp(animator.GetFloat("ForwardSpeed"), vector4.z, (!base.motionMgr.isInAimState) ? (5f * Time.deltaTime) : 1f));
			animator.SetFloat("RightSpeed", Mathf.Lerp(animator.GetFloat("RightSpeed"), vector4.x, (!base.motionMgr.isInAimState) ? (5f * Time.deltaTime) : 1f));
		}
		return false;
	}

	public override void EndAction()
	{
		base.motionMgr.DoAction(PEActionType.Halt);
		m_EndFlag = true;
		if (null != base.anim.animator)
		{
			base.anim.animator.SetBool("StartMove", value: false);
		}
		base.motionMgr.SetMaskState(PEActionMask.Sprint, state: false);
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
		}
	}

	public override void EndImmediately()
	{
		if (null != base.anim.animator)
		{
			base.anim.animator.SetBool("StartMove", value: false);
		}
		base.motionMgr.SetMaskState(PEActionMask.Sprint, state: false);
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
		}
	}
}
