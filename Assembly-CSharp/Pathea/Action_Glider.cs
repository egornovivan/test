using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Glider : PEAction
{
	private const float UPAcc = 15f;

	public HumanPhyCtrl m_PhyCtrl;

	private PEGlider m_Glider;

	private float m_ForwardAreaF;

	private float m_UpAreaF;

	private Vector2 m_TargetOpDir = Vector2.zero;

	private Vector2 m_CurrentOpDir = Vector2.zero;

	private bool m_PauseByPhy;

	private Vector3 m_PauseVel;

	public override PEActionType ActionType => PEActionType.Glider;

	public PEGlider glider
	{
		get
		{
			return m_Glider;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Glider = value;
		}
	}

	public void SetMoveDir(Vector3 moveDir)
	{
		m_TargetOpDir.x = moveDir.x;
		m_TargetOpDir.y = moveDir.z;
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null != glider && null != m_PhyCtrl)
		{
			return m_PhyCtrl.velocity.y < glider.m_TurnOnSpeed;
		}
		return false;
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Glider, state: true);
		m_PauseByPhy = false;
		if (null != base.anim)
		{
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.SetBool("Glider", value: true);
		}
		if (null != glider)
		{
			m_ForwardAreaF = glider.m_BoostPower / (m_PhyCtrl.m_AirDrag * glider.m_BalanceForwardSpeed * glider.m_BalanceForwardSpeed);
			m_UpAreaF = (0f - m_PhyCtrl.gravity) / (m_PhyCtrl.m_AirDrag * glider.m_BalanceDownSpeed * glider.m_BalanceDownSpeed);
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.freezeUpdate = true;
				m_PhyCtrl.desiredMovementDirection = Vector3.zero;
			}
			glider.SetOpenState(open: true);
		}
		m_CurrentOpDir = Vector2.zero;
	}

	public override bool Update()
	{
		if (null == glider || null == base.trans || null == m_PhyCtrl || (PeGameMgr.gamePause && !PeGameMgr.IsMulti))
		{
			return false;
		}
		if (m_PauseByPhy)
		{
			if (!m_PhyCtrl._rigidbody.isKinematic)
			{
				m_PhyCtrl.velocity = m_PauseVel;
				m_PauseByPhy = false;
			}
		}
		else if (m_PhyCtrl._rigidbody.isKinematic)
		{
			m_PauseVel = m_PhyCtrl.inertiaVelocity;
			m_PauseByPhy = true;
		}
		if (base.move.state != MovementState.Air)
		{
			base.motionMgr.EndImmediately(ActionType);
		}
		else
		{
			float y = base.trans.existent.eulerAngles.y;
			m_CurrentOpDir.x = Mathf.Lerp(m_CurrentOpDir.x, 30f * m_TargetOpDir.x, glider.m_RotateAcc * Time.deltaTime);
			m_CurrentOpDir.y = Mathf.Lerp(m_CurrentOpDir.y, 30f * m_TargetOpDir.y, glider.m_RotateAcc * Time.deltaTime / 2f);
			y += m_CurrentOpDir.x / 2f * Time.deltaTime;
			Vector3 b = Vector3.Project(new Vector3(m_PhyCtrl.velocity.x, 0f, m_PhyCtrl.velocity.z), new Vector3(base.trans.existent.forward.x, 0f, base.trans.existent.forward.z));
			b.y = m_PhyCtrl.velocity.y;
			m_PhyCtrl.velocity = Vector3.Lerp(m_PhyCtrl.velocity, b, Time.deltaTime);
			base.trans.rotation = Quaternion.AngleAxis(y, Vector3.up) * Quaternion.AngleAxis(m_CurrentOpDir.y, Vector3.right) * Quaternion.AngleAxis(0f - m_CurrentOpDir.x, Vector3.forward);
			Vector3 vector = Vector3.Project(m_PhyCtrl.velocity, base.trans.existent.up);
			Vector3 vector2 = Vector3.Project(m_PhyCtrl.velocity, base.trans.existent.forward);
			Vector3 force = 10f * Vector3.down;
			force += -vector2.normalized * glider.m_AreaDragF * m_ForwardAreaF * vector2.sqrMagnitude;
			force += -vector.normalized * glider.m_AreaDragF * m_UpAreaF * vector.sqrMagnitude;
			force += glider.m_BoostPower * base.trans.existent.forward;
			m_PhyCtrl._rigidbody.AddForce(force, ForceMode.Acceleration);
			m_PhyCtrl.ResetSpeed(m_PhyCtrl.velocity.magnitude);
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Glider, state: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
			base.anim.SetBool("Glider", value: false);
		}
		if (null != glider)
		{
			glider.SetOpenState(open: false);
		}
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.freezeUpdate = false;
		}
		base.trans.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(base.trans.forward, Vector3.up));
	}
}
