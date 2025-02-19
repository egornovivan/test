using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Move : PEAction
{
	[HideInInspector]
	public MoveParam m_Param;

	private float m_RunSpeed = 5f;

	private float m_WalkSpeed = 2f;

	private bool m_IsWalk;

	[HideInInspector]
	public bool m_AutoRotate;

	[HideInInspector]
	public bool m_ApplyStopIK;

	[HideInInspector]
	public float rotateSpeedScale = 1f;

	private float m_MoveSpeed = 5f;

	private Vector3 m_Target;

	private MoveType m_MoveType;

	private bool m_EndFlag;

	private Vector3 m_LastMoveDir;

	private Vector3 m_LastVelocity;

	private bool m_InputMove;

	private Vector3 m_LookDir;

	private bool m_RestLookDir;

	private float m_SpeedScale;

	public override PEActionType ActionType => PEActionType.Move;

	public HumanPhyCtrl phyCtrl { get; set; }

	public float runSpeed
	{
		get
		{
			return m_RunSpeed;
		}
		set
		{
			m_RunSpeed = value;
			if (!m_IsWalk)
			{
				m_MoveSpeed = m_RunSpeed;
			}
		}
	}

	public float walkSpeed
	{
		get
		{
			return m_WalkSpeed;
		}
		set
		{
			m_WalkSpeed = value;
			if (m_IsWalk)
			{
				m_MoveSpeed = m_WalkSpeed;
			}
		}
	}

	public void SetLookDir(Vector3 lookDir)
	{
		m_RestLookDir = true;
		m_LookDir = lookDir;
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		PEActionParamNV pEActionParamNV = para as PEActionParamNV;
		MoveType n = (MoveType)pEActionParamNV.n;
		Vector3 vec = pEActionParamNV.vec;
		if (n == MoveType.Direction)
		{
			if (vec == Vector3.zero)
			{
				return false;
			}
		}
		else if (Vector3.SqrMagnitude(base.trans.position - vec) < MoveParam.AutoMoveStopSqrDis * MoveParam.AutoMoveStopSqrDis)
		{
			return false;
		}
		return true;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == phyCtrl) && !(null == base.anim) && !(null == base.anim.animator))
		{
			Animator animator = base.anim.animator;
			PEActionParamNV pEActionParamNV = para as PEActionParamNV;
			m_MoveType = (MoveType)pEActionParamNV.n;
			m_Target = pEActionParamNV.vec;
			m_EndFlag = false;
			phyCtrl.ResetSpeed(m_MoveSpeed);
			Vector3 currentDesiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
			phyCtrl.desiredMovementDirection = currentDesiredMovementDirection;
			m_LastVelocity = currentDesiredMovementDirection;
			m_RestLookDir = false;
			Vector3 vector = Quaternion.Inverse(base.trans.rotation) * m_LastVelocity * m_MoveSpeed;
			animator.SetFloat("ForwardSpeed", vector.z);
			animator.SetFloat("RightSpeed", vector.x);
			animator.SetBool("StartMove", value: true);
			if (null == m_Param && null != base.trans)
			{
				m_SpeedScale = m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle);
			}
			else
			{
				m_SpeedScale = 0f;
			}
		}
	}

	public override void ResetAction(PEActionParam para)
	{
		PEActionParamNV pEActionParamNV = para as PEActionParamNV;
		m_MoveType = (MoveType)pEActionParamNV.n;
		m_Target = pEActionParamNV.vec;
	}

	public override void PauseAction()
	{
		if (null != base.anim && null != base.anim.animator)
		{
			base.anim.animator.SetBool("StartMove", value: false);
		}
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
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
			Vector3 currentDesiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
			phyCtrl.desiredMovementDirection = currentDesiredMovementDirection;
			m_LastVelocity = currentDesiredMovementDirection;
		}
	}

	public override bool Update()
	{
		if (null == phyCtrl || null == base.anim || null == m_Param || m_EndFlag)
		{
			EndImmediately();
			return true;
		}
		if (base.pauseAction)
		{
			return false;
		}
		Vector3 vector = Vector3.zero;
		switch (m_MoveType)
		{
		case MoveType.Direction:
			if (m_Target != Vector3.zero)
			{
				m_Target.Normalize();
			}
			vector = m_Target;
			break;
		case MoveType.Target:
		{
			Vector3 vector2 = m_Target - base.trans.position;
			if (vector2.sqrMagnitude > MoveParam.AutoMoveStopSqrDis * MoveParam.AutoMoveStopSqrDis)
			{
				vector = vector2.normalized;
			}
			else
			{
				EndAction();
			}
			break;
		}
		}
		if (m_AutoRotate)
		{
			if (vector != Vector3.zero)
			{
				Vector3 vector3 = vector;
				if (!phyCtrl.spineInWater)
				{
					vector3 = Vector3.ProjectOnPlane(vector3, Vector3.up).normalized;
				}
				float num = Vector3.Angle(base.trans.forward, vector3);
				vector3 = ((!(num > 150f)) ? Vector3.Slerp(base.trans.forward, vector3, rotateSpeedScale * m_Param.m_MoveRotateSpeed * Time.deltaTime) : Vector3.Slerp(base.trans.forward, base.trans.existent.right, rotateSpeedScale * m_Param.m_MoveRotateSpeed * Time.deltaTime));
				base.trans.rotation = Quaternion.LookRotation(vector3, Vector3.up);
			}
			else if (phyCtrl.spineInWater)
			{
				Vector3 forward = Vector3.Slerp(base.trans.forward, Vector3.ProjectOnPlane(base.trans.forward, Vector3.up).normalized, rotateSpeedScale * m_Param.m_MoveRotateSpeed * Time.deltaTime);
				base.trans.rotation = Quaternion.LookRotation(forward, Vector3.up);
			}
		}
		else if (m_RestLookDir && m_LookDir != Vector3.zero)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(m_LookDir, Vector3.up).normalized;
			base.trans.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		}
		if (vector != Vector3.zero)
		{
			vector = Quaternion.AngleAxis(0f - phyCtrl.forwardGroundAngle, base.trans.existent.right) * vector;
			if (phyCtrl.spineInWater)
			{
				m_SpeedScale = Mathf.Lerp(m_SpeedScale, 1f, m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
			}
			else
			{
				m_SpeedScale = Mathf.Lerp(m_SpeedScale, m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle), m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
			}
			phyCtrl.ResetSpeed(m_MoveSpeed * m_SpeedScale);
		}
		if (base.motionMgr.isInAimState)
		{
			m_LastVelocity = vector;
		}
		else
		{
			m_LastVelocity = Vector3.Lerp(m_LastVelocity, vector, ((!m_ApplyStopIK) ? 3f : 1f) * m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
		}
		Animator animator = base.anim.animator;
		if (null != animator)
		{
			phyCtrl.desiredMovementDirection = m_LastVelocity * animator.GetFloat("RunSpeedF");
			Vector3 vector4 = Quaternion.Inverse(base.trans.rotation) * vector * m_MoveSpeed;
			animator.SetFloat("ForwardSpeed", Mathf.Lerp(animator.GetFloat("ForwardSpeed"), vector4.z * phyCtrl.netMoveSpeedScale, (!base.motionMgr.isInAimState) ? (5f * Time.deltaTime) : 1f));
			animator.SetFloat("RightSpeed", Mathf.Lerp(animator.GetFloat("RightSpeed"), vector4.x, (!base.motionMgr.isInAimState) ? (5f * Time.deltaTime) : 1f));
		}
		return false;
	}

	public override void EndAction()
	{
		base.motionMgr.DoAction(PEActionType.Halt);
		m_Target = Vector3.zero;
		m_EndFlag = true;
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
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
		}
	}

	public void SetWalkState(bool walk)
	{
		m_IsWalk = walk;
		if (m_IsWalk)
		{
			m_MoveSpeed = m_WalkSpeed;
		}
		else
		{
			m_MoveSpeed = m_RunSpeed;
		}
	}
}
