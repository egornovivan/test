using System;
using PETools;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Rotate : PEAction
{
	public float m_MinAngle = 3f;

	public float m_AnimRotateAngleLimit = 160f;

	public float m_SpeedThreshold = 1f;

	public float m_SpeedThresholdRun = 3f;

	public float m_AnimRotateSpeedMax = 100f;

	public float m_AnimRotateAcc = 15f;

	public float m_StartRotateAngle = 2f;

	public float m_MaxAngleThreshold = 30f;

	public float m_RotateAnglePerSecond = 50f;

	private Vector3 m_TargetDir;

	private float m_AnimRotateSpeed;

	private Vector3 m_AddDir;

	private bool m_EndRote;

	public bool m_FullAnimRotate;

	private bool m_CampMaxThreshold;

	public override PEActionType ActionType => PEActionType.Rotate;

	public HumanPhyCtrl phyMotor { get; set; }

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null == base.trans || null == phyMotor)
		{
			return false;
		}
		PEActionParamVBB pEActionParamVBB = para as PEActionParamVBB;
		Vector3 vec = pEActionParamVBB.vec;
		if (vec == Vector3.zero)
		{
			return false;
		}
		bool b = pEActionParamVBB.b1;
		float num = Vector3.Angle(vec, base.trans.forward);
		if (num < m_MinAngle)
		{
			return false;
		}
		float magnitude = phyMotor.velocity.magnitude;
		if (b)
		{
			if (!phyMotor.spineInWater && num > m_AnimRotateAngleLimit && magnitude > m_SpeedThresholdRun)
			{
				return true;
			}
		}
		else if (num > m_StartRotateAngle && magnitude < m_SpeedThreshold)
		{
			return true;
		}
		return false;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null == base.trans || null == base.anim)
		{
			return;
		}
		PEActionParamVBB pEActionParamVBB = para as PEActionParamVBB;
		m_TargetDir = pEActionParamVBB.vec;
		if (PE.PointInWater(base.trans.position) < 0.5f)
		{
			m_TargetDir = Vector3.ProjectOnPlane(m_TargetDir, Vector3.up);
		}
		if (null == phyMotor)
		{
			base.trans.rotation = Quaternion.LookRotation(m_TargetDir, Vector3.up);
			return;
		}
		m_FullAnimRotate = pEActionParamVBB.b1;
		m_CampMaxThreshold = pEActionParamVBB.b2;
		m_TargetDir.Normalize();
		Animator animator = base.anim.animator;
		if (null != animator)
		{
			animator.SetFloat("RotateSpeed", 0f);
			animator.SetBool("EndRotate", value: false);
		}
		m_EndRote = false;
		float magnitude = phyMotor.velocity.magnitude;
		if (m_FullAnimRotate)
		{
			base.motionMgr.SetMaskState(PEActionMask.Rotate, state: true);
			if (null != animator)
			{
				animator.SetFloat("RotationAgr", 0f);
				animator.SetFloat("RunRotate", (!(magnitude > m_SpeedThresholdRun)) ? 0f : 1f);
				animator.SetTrigger("RotateAnim");
			}
			m_AnimRotateSpeed = 0f;
			base.motionMgr.EndImmediately(PEActionType.Move);
			base.motionMgr.EndImmediately(PEActionType.Sprint);
		}
		else if (null != animator)
		{
			animator.SetFloat("RotationAgr", 0f);
			animator.SetFloat("RunRotate", 0f);
		}
	}

	public override void ResetAction(PEActionParam para = null)
	{
		base.ResetAction(para);
		PEActionParamVBB pEActionParamVBB = para as PEActionParamVBB;
		m_TargetDir = pEActionParamVBB.vec;
		m_TargetDir.Normalize();
		m_AddDir = m_TargetDir;
		if (m_AddDir.sqrMagnitude > 1f)
		{
			m_AddDir.Normalize();
		}
	}

	public override bool Update()
	{
		if (null == base.trans || null == base.anim || null == phyMotor)
		{
			return true;
		}
		Animator animator = base.anim.animator;
		if (m_FullAnimRotate)
		{
			if (m_EndRote)
			{
				base.motionMgr.SetMaskState(PEActionMask.Rotate, state: false);
				return true;
			}
			m_AnimRotateSpeed = Mathf.Lerp(m_AnimRotateSpeed, m_AnimRotateSpeedMax * Vector3.Project(m_AddDir, m_TargetDir).magnitude, m_AnimRotateAcc * Time.deltaTime);
			if (null != animator)
			{
				animator.SetFloat("RotateSpeed", m_AnimRotateSpeed);
			}
			if (!base.anim.IsInTransition(0))
			{
				base.trans.rotation = base.anim.m_LastRot * base.trans.rotation;
				Vector3 forward = Vector3.ProjectOnPlane(base.trans.forward, Vector3.up);
				base.trans.rotation = Quaternion.LookRotation(forward, Vector3.up);
				phyMotor.ApplyMoveRequest(base.anim.m_LastMove / Time.deltaTime);
			}
		}
		else
		{
			float num = Vector3.Angle(base.trans.forward, m_TargetDir);
			if (m_TargetDir == Vector3.zero || num < 1f)
			{
				if (null != animator)
				{
					animator.SetFloat("RotationAgr", 0f);
				}
				base.motionMgr.SetMaskState(PEActionMask.Rotate, state: false);
				return true;
			}
			float num2 = Mathf.Clamp(m_RotateAnglePerSecond * Time.deltaTime, 0f, num);
			if (m_CampMaxThreshold && num - num2 > m_MaxAngleThreshold)
			{
				num2 = num - m_MaxAngleThreshold;
			}
			Vector3 forward2 = Vector3.Slerp(base.trans.forward, m_TargetDir.normalized, num2 / num);
			base.trans.rotation = Quaternion.LookRotation(forward2, Vector3.up);
			float num3 = Vector3.Angle(base.trans.existent.right, m_TargetDir);
			if (num3 > 90f)
			{
				num *= -1f;
			}
			if (null != animator)
			{
				animator.SetFloat("RotationAgr", num);
			}
		}
		return false;
	}

	public void AnimEvent(string name)
	{
		if (base.motionMgr.IsActionRunning(ActionType))
		{
			switch (name)
			{
			case "EndRotate":
				m_EndRote = true;
				break;
			}
		}
	}

	public override void EndImmediately()
	{
		if (null != phyMotor)
		{
			phyMotor.CancelMoveRequest();
		}
		if (m_FullAnimRotate)
		{
			base.motionMgr.ContinueAction(PEActionType.Move, ActionType);
			base.motionMgr.ContinueAction(PEActionType.Sprint, ActionType);
		}
		m_TargetDir = base.trans.forward;
		base.anim.SetBool("EndRotate", value: true);
		base.anim.SetFloat("RotationAgr", 0f);
		base.motionMgr.SetMaskState(PEActionMask.Rotate, state: false);
	}
}
