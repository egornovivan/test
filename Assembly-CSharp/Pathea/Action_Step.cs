using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Step : PEAction
{
	public float m_InertiaF = 0.2f;

	public float m_StepTime = 0.6f;

	public float m_ReActiveTime = 0.3f;

	public float m_EndFlagTime = 0.5f;

	public float m_StaminaCost = 10f;

	public float m_InvincibleTime = 0.1f;

	[HideInInspector]
	public bool m_UseStamina;

	private float m_StartTime;

	private Vector3 m_MoveDir;

	private bool m_Invincible;

	public override PEActionType ActionType => PEActionType.Step;

	public HumanPhyCtrl phyMotor { get; set; }

	public override bool CanDoAction(PEActionParam para = null)
	{
		return CheckStamina();
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == phyMotor))
		{
			base.motionMgr.SetMaskState(PEActionMask.Step, state: true);
			m_StartTime = Time.time;
			Vector3 velocity = phyMotor.velocity;
			if (velocity.sqrMagnitude > 1f)
			{
				velocity.Normalize();
			}
			PEActionParamV pEActionParamV = para as PEActionParamV;
			m_MoveDir = (pEActionParamV.vec + phyMotor.velocity * m_InertiaF).normalized;
			m_MoveDir.Normalize();
			base.anim.SetTrigger("Step");
			Vector3 vector = Quaternion.Inverse(base.trans.rotation) * m_MoveDir;
			base.anim.SetFloat("StepForward", vector.z);
			base.anim.SetFloat("StepRight", vector.x);
			if (null != base.viewCmpt)
			{
				base.viewCmpt.ActivateInjured(value: false);
			}
			m_Invincible = true;
			CostStamina();
		}
	}

	public override void ResetAction(PEActionParam para = null)
	{
		if (Time.time - m_StartTime > m_ReActiveTime && CheckStamina() && !base.motionMgr.GetMaskState(PEActionMask.InAir))
		{
			DoAction(para);
		}
	}

	public override bool Update()
	{
		float num = Time.time - m_StartTime;
		if (m_Invincible && m_InvincibleTime < num)
		{
			m_Invincible = false;
			if (null != base.viewCmpt)
			{
				base.viewCmpt.ActivateInjured(value: true);
			}
		}
		if (num < m_StepTime)
		{
			if (num > m_EndFlagTime)
			{
				base.motionMgr.SetMaskState(PEActionMask.Step, state: false);
			}
			if (!base.anim.IsInTransition(0))
			{
				base.trans.rotation = base.anim.m_LastRot * base.trans.rotation;
				if (null != phyMotor)
				{
					phyMotor.ApplyMoveRequest(base.anim.m_LastMove / Time.deltaTime);
				}
			}
			num += Time.deltaTime;
			base.skillCmpt._lastestTimeOfConsumingStamina = Time.time;
			return false;
		}
		OnEndAction();
		return true;
	}

	public override void EndImmediately()
	{
		OnEndAction();
		if (null != phyMotor)
		{
			phyMotor.CancelMoveRequest();
			phyMotor.desiredMovementDirection = Vector3.zero;
		}
	}

	private void OnEndAction()
	{
		base.motionMgr.SetMaskState(PEActionMask.Step, state: false);
		if (null != base.viewCmpt)
		{
			base.viewCmpt.ActivateInjured(value: true);
		}
	}

	private bool CheckStamina()
	{
		if (m_UseStamina)
		{
			return base.motionMgr.Entity.GetAttribute(AttribType.Stamina) >= m_StaminaCost * base.motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent);
		}
		return true;
	}

	private void CostStamina()
	{
		if (m_UseStamina)
		{
			float attribute = base.motionMgr.Entity.GetAttribute(AttribType.Stamina);
			attribute = Mathf.Clamp(attribute - m_StaminaCost * base.motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent), 0f, attribute);
			base.motionMgr.Entity.SetAttribute(AttribType.Stamina, attribute, offEvent: false);
		}
	}
}
