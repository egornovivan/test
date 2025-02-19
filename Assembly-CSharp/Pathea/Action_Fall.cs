using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Fall : PEAction
{
	public float m_RotateAcc = 5f;

	public float m_MinSpeed = 3f;

	public AnimationCurve m_MoveAcc;

	public AnimationCurve m_CenterAcc;

	private Vector3 m_Target;

	public override PEActionType ActionType => PEActionType.Fall;

	public HumanPhyCtrl phyMotor { get; set; }

	public void SetMoveDir(Vector3 dir)
	{
		m_Target = dir;
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		return base.move.state == MovementState.Air;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == phyMotor) && !(null == base.anim))
		{
			if (phyMotor.moveSpeed < m_MinSpeed)
			{
				phyMotor.ResetSpeed(m_MinSpeed);
			}
			phyMotor.desiredMovementDirection = phyMotor.currentDesiredMovementDirection;
			base.anim.SetTrigger("Fall");
			base.motionMgr.SetMaskState(PEActionMask.Fall, state: true);
		}
	}

	public override bool Update()
	{
		if (null == phyMotor || null == base.anim)
		{
			base.motionMgr.SetMaskState(PEActionMask.Fall, state: false);
			return true;
		}
		if (phyMotor.grounded || phyMotor.feetInWater)
		{
			base.anim.ResetTrigger("Fall");
			if (phyMotor.grounded)
			{
				base.anim.SetBool("OnGround", value: true);
			}
			base.anim.SetFloat("JumpForward", m_Target.magnitude * base.anim.GetFloat("JumpForward"));
			if (null != phyMotor)
			{
				phyMotor.desiredMovementDirection = Vector3.zero;
			}
			base.motionMgr.SetMaskState(PEActionMask.Fall, state: false);
			return true;
		}
		if (m_Target != Vector3.zero && !base.motionMgr.GetMaskState(PEActionMask.AimEquipHold) && !base.motionMgr.GetMaskState(PEActionMask.GunHold) && !base.motionMgr.GetMaskState(PEActionMask.BowHold))
		{
			Vector3 forward = Vector3.Slerp(base.trans.forward, m_Target.normalized, m_RotateAcc * m_Target.magnitude * Time.deltaTime);
			base.trans.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
		if (null != base.anim.animator)
		{
			base.anim.animator.SetFloat("JumpForward", Mathf.Lerp(base.anim.animator.GetFloat("JumpForward"), (Quaternion.Inverse(base.trans.rotation) * phyMotor.velocity).z, 5f * Time.deltaTime));
		}
		phyMotor.desiredMovementDirection = Vector3.Lerp(phyMotor.desiredMovementDirection, m_Target, 5f * Time.deltaTime);
		return false;
	}

	public override void EndImmediately()
	{
		if (null != phyMotor)
		{
			phyMotor.desiredMovementDirection = Vector3.zero;
		}
		base.motionMgr.SetMaskState(PEActionMask.Fall, state: false);
		if (null != base.anim)
		{
			base.anim.ResetTrigger("Fall");
		}
	}
}
