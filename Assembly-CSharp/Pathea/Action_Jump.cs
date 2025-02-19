using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Jump : PEAction
{
	public float m_JumpHeight = 2f;

	public float m_JumpMoveF = 0.3f;

	public float m_JumpDT = 0.1f;

	public float m_LongJumpSpeed = 3.5f;

	public float m_RotateAcc = 5f;

	public float m_AddGravityTime = 3f;

	public float m_AddGravityPower = 1f;

	public float m_JumpEndTime = 3f;

	public float m_StaminaCost = 10f;

	public float m_MinSpeed = 3f;

	public float m_GroundHeight = 0.3f;

	public float m_CheckRadius = 0.3f;

	[HideInInspector]
	public bool m_AutoRotate;

	private Vector3 m_JumpStartVelocity;

	private Vector3 m_AddMove;

	private bool m_EndFlag;

	private float m_ElapseTime;

	private Vector3 m_LookDir;

	private bool m_RestLookDir;

	public override PEActionType ActionType => PEActionType.Jump;

	public HumanPhyCtrl phyMotor { get; set; }

	public FullBodyBipedIK fBBIK { get; set; }

	public override bool CanDoAction(PEActionParam para = null)
	{
		return base.motionMgr.Entity.GetAttribute(AttribType.Stamina) >= m_StaminaCost * base.motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent);
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null == phyMotor || null == base.trans || null == base.anim)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			if (!base.skillCmpt.IsController())
			{
				base.anim.SetTrigger("Jump");
				return;
			}
			if (null != base.entity.netCmpt && null != base.entity.netCmpt.network)
			{
				SkNetworkInterface skNetworkInterface = base.entity.netCmpt.network as SkNetworkInterface;
				if (null != skNetworkInterface)
				{
					skNetworkInterface.RequestJump(GameTime.Timer.Second);
				}
			}
		}
		base.motionMgr.Entity.SetAttribute(AttribType.Stamina, base.motionMgr.Entity.GetAttribute(AttribType.Stamina) - m_StaminaCost * base.motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent), offEvent: false);
		if (phyMotor.moveSpeed < m_MinSpeed)
		{
			phyMotor.ResetSpeed(m_MinSpeed);
		}
		m_JumpStartVelocity = Quaternion.Inverse(base.trans.rotation) * phyMotor.currentDesiredMovementDirection;
		base.anim.ResetTrigger("EndJump");
		base.anim.SetTrigger("Jump");
		base.anim.SetFloat("JumpForward", m_JumpStartVelocity.z * phyMotor.moveSpeed);
		base.motionMgr.SetMaskState(PEActionMask.Jump, state: true);
		phyMotor.ApplyImpact(Mathf.Sqrt(20f * m_JumpHeight) * Vector3.up);
		m_EndFlag = false;
		m_ElapseTime = 0f;
		m_RestLookDir = false;
		if (null != fBBIK)
		{
			fBBIK.solver.SetIKPositionWeight(0f);
		}
	}

	public void SetMoveDir(Vector3 dir)
	{
		m_AddMove = dir;
	}

	public void SetLookDir(Vector3 lookDir)
	{
		m_RestLookDir = true;
		m_LookDir = lookDir;
	}

	public override bool Update()
	{
		if (m_EndFlag || null == phyMotor || null == base.trans)
		{
			OnEndAction();
			return true;
		}
		Animator animator = base.anim.animator;
		bool flag = CheckGround() || phyMotor.grounded;
		if (m_StaminaCost > float.Epsilon)
		{
			base.skillCmpt._lastestTimeOfConsumingStamina = Time.time;
		}
		if (m_ElapseTime > m_JumpDT && (flag || phyMotor.feetInWater))
		{
			if (null != animator)
			{
				if (flag)
				{
					animator.SetBool("OnGround", value: true);
				}
				animator.SetFloat("JumpForward", m_AddMove.magnitude * animator.GetFloat("JumpForward"));
			}
			phyMotor.desiredMovementDirection = Vector3.zero;
			base.motionMgr.SetMaskState(PEActionMask.Jump, state: false);
			if (null != fBBIK)
			{
				fBBIK.solver.SetIKPositionWeight(1f);
			}
			return true;
		}
		m_ElapseTime += Time.deltaTime;
		if (m_ElapseTime > m_AddGravityTime)
		{
			phyMotor.GetComponent<Rigidbody>().AddForce(m_AddGravityPower * Vector3.down, ForceMode.Acceleration);
		}
		phyMotor.desiredMovementDirection = m_AddMove * m_JumpMoveF + base.trans.rotation * m_JumpStartVelocity * (1f - m_JumpMoveF);
		if (m_AutoRotate)
		{
			if (m_AddMove.sqrMagnitude > float.Epsilon)
			{
				Vector3 forward = Vector3.Slerp(base.trans.forward, m_AddMove.normalized, m_RotateAcc * m_AddMove.magnitude * Time.deltaTime);
				base.trans.rotation = Quaternion.LookRotation(forward, Vector3.up);
			}
		}
		else if (m_RestLookDir)
		{
			base.trans.rotation = Quaternion.LookRotation(m_LookDir, Vector3.up);
		}
		if (null != animator)
		{
			animator.SetFloat("JumpForward", Mathf.Lerp(animator.GetFloat("JumpForward"), (Quaternion.Inverse(base.trans.rotation) * phyMotor.velocity).z, 5f * Time.deltaTime));
		}
		m_AddMove = Vector3.zero;
		if (m_ElapseTime > m_JumpEndTime)
		{
			base.motionMgr.SetMaskState(PEActionMask.Jump, state: false);
			if (null != fBBIK)
			{
				fBBIK.solver.SetIKPositionWeight(1f);
			}
			return true;
		}
		return false;
	}

	public override void EndAction()
	{
		m_EndFlag = true;
		OnEndAction();
	}

	public override void EndImmediately()
	{
		OnEndAction();
		if (null != base.anim)
		{
			base.anim.SetTrigger("EndJump");
		}
	}

	private void OnEndAction()
	{
		if (null != phyMotor)
		{
			phyMotor.desiredMovementDirection = Vector3.zero;
		}
		base.motionMgr.SetMaskState(PEActionMask.Jump, state: false);
		if (null != fBBIK)
		{
			fBBIK.solver.SetIKPositionWeight(1f);
		}
	}

	private bool CheckGround()
	{
		if (null != phyMotor && Physics.CapsuleCast(base.trans.position + m_CheckRadius * Vector3.up, base.trans.position + 2f * m_CheckRadius * Vector3.up, m_CheckRadius, Vector3.down, out var hitInfo, m_GroundHeight, phyMotor.m_GroundLayer.value) && hitInfo.distance < m_GroundHeight)
		{
			return true;
		}
		return false;
	}
}
