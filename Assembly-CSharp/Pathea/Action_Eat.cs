using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Eat : PEAction
{
	public float m_MoveTime = 0.01f;

	private float m_MoveElapseTime;

	private Vector3 m_StartPos;

	private Quaternion m_StartRot;

	private Vector3 m_TargetPos;

	private Quaternion m_TargetRot;

	private bool m_EndAnim;

	public override PEActionType ActionType => PEActionType.Eat;

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("Eat");
			base.anim.ResetTrigger("ResetUpbody");
		}
		if (null != base.trans)
		{
			m_StartPos = base.trans.position;
			m_StartRot = base.trans.rotation;
			m_TargetPos = base.trans.position;
			m_TargetRot = base.trans.rotation;
			m_MoveElapseTime = 0f;
		}
		m_EndAnim = false;
		base.motionMgr.SetMaskState(PEActionMask.Eat, state: true);
	}

	public override bool Update()
	{
		if (null != base.trans && m_MoveElapseTime < m_MoveTime)
		{
			m_MoveElapseTime = Mathf.Clamp(m_MoveElapseTime + Time.deltaTime, 0f, m_MoveTime);
			if (m_MoveElapseTime > m_MoveTime)
			{
				m_MoveElapseTime = m_MoveTime;
			}
			base.trans.position = Vector3.Lerp(m_StartPos, m_TargetPos, m_MoveElapseTime / m_MoveTime);
			base.trans.rotation = Quaternion.Lerp(m_StartRot, m_TargetRot, m_MoveElapseTime / m_MoveTime);
		}
		if (null != base.anim)
		{
			if (m_EndAnim)
			{
				base.motionMgr.SetMaskState(PEActionMask.Eat, state: false);
			}
			return m_EndAnim;
		}
		base.motionMgr.SetMaskState(PEActionMask.Eat, state: false);
		return true;
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetUpbody");
			base.anim.ResetTrigger("Eat");
		}
		base.motionMgr.SetMaskState(PEActionMask.Eat, state: false);
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType) && "EatEnd" == eventParam)
		{
			m_EndAnim = true;
		}
	}
}
