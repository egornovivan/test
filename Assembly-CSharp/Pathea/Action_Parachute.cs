using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Parachute : PEAction
{
	private const float UPAcc = 15f;

	public HumanPhyCtrl m_PhyCtrl;

	private PEParachute m_Parachute;

	public float m_RotAcc = 5f;

	private Vector3 m_MoveDir;

	public override PEActionType ActionType => PEActionType.Parachute;

	public PEParachute parachute
	{
		get
		{
			return m_Parachute;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Parachute = value;
		}
	}

	public void SetMoveDir(Vector3 moveDir)
	{
		m_MoveDir = moveDir;
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null != parachute && null != m_PhyCtrl)
		{
			return m_PhyCtrl.velocity.y < parachute.m_TurnOnSpeed;
		}
		return false;
	}

	public override void DoAction(PEActionParam para = null)
	{
		m_MoveDir = Vector3.zero;
		base.motionMgr.SetMaskState(PEActionMask.Parachute, state: true);
		if (null != parachute)
		{
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.ResetSpeed(parachute.m_HorizonalSpeed);
				m_PhyCtrl.desiredMovementDirection = m_PhyCtrl.currentDesiredMovementDirection;
			}
			parachute.SetOpenState(open: true);
		}
		if (null != base.anim)
		{
			base.anim.SetTrigger("Fall");
		}
	}

	public override bool Update()
	{
		if (null != parachute && null != base.trans)
		{
			if (m_MoveDir != Vector3.zero && !base.motionMgr.isInAimState)
			{
				base.trans.rotation = Quaternion.Lerp(base.trans.rotation, Quaternion.LookRotation(m_MoveDir, Vector3.up), m_RotAcc * Time.deltaTime);
			}
			if (null != m_PhyCtrl)
			{
				if (null != base.move && base.move.state != MovementState.Air)
				{
					base.motionMgr.EndAction(ActionType);
				}
				else
				{
					m_PhyCtrl.desiredMovementDirection = Vector3.Lerp(m_PhyCtrl.desiredMovementDirection, m_MoveDir, 5f * Time.deltaTime);
					m_PhyCtrl.m_SubAcc = ((!(m_PhyCtrl.velocity.y < parachute.BalanceDownSpeed)) ? Vector3.zero : (15f * Vector3.up));
				}
			}
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Parachute, state: false);
		if (null != parachute)
		{
			parachute.SetOpenState(open: false);
		}
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.m_SubAcc = Vector3.zero;
			m_PhyCtrl.desiredMovementDirection = Vector3.zero;
		}
	}
}
