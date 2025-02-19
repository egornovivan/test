using System;
using System.Collections.Generic;
using Pathea.Operate;
using SkillSystem;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Sleep : PEAction
{
	public HumanPhyCtrl m_PhyCtrl;

	public float m_MoveTime = 0.01f;

	private float m_MoveElapseTime;

	private Vector3 m_StartPos;

	private Quaternion m_StartRot;

	private Vector3 m_TargetPos;

	private Quaternion m_TargetRot;

	private int m_BuffID;

	private string m_AnimName;

	private bool m_EndAnim;

	private bool m_EndAction;

	public override PEActionType ActionType => PEActionType.Sleep;

	public event Action<int> startSleepEvt;

	public event Action<int> endSleepEvt;

	public override void PreDoAction()
	{
		base.PreDoAction();
		base.motionMgr.SetMaskState(PEActionMask.Sleep, state: true);
	}

	public override void DoAction(PEActionParam para = null)
	{
		PEActionParamVQNS pEActionParamVQNS = para as PEActionParamVQNS;
		if (null != base.trans)
		{
			m_StartPos = base.trans.position;
			m_StartRot = base.trans.rotation;
			m_TargetPos = pEActionParamVQNS.vec;
			m_TargetRot = pEActionParamVQNS.q;
			m_MoveElapseTime = 0f;
		}
		m_AnimName = pEActionParamVQNS.str;
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: true);
			base.anim.ResetTrigger("ResetFullBody");
		}
		m_BuffID = pEActionParamVQNS.n;
		if (null != base.skillCmpt && m_BuffID != 0)
		{
			SkEntity.MountBuff(base.skillCmpt, m_BuffID, new List<int>(), new List<float>());
		}
		m_EndAnim = false;
		m_EndAction = false;
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.velocity = Vector3.zero;
			m_PhyCtrl.CancelMoveRequest();
		}
		if (this.startSleepEvt != null)
		{
			this.startSleepEvt(m_BuffID);
		}
	}

	public override void EndAction()
	{
		m_EndAction = true;
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: false);
		}
		if (null != base.skillCmpt && m_BuffID != 0)
		{
			base.skillCmpt.CancelBuffById(m_BuffID);
		}
		if (this.endSleepEvt != null)
		{
			this.endSleepEvt(m_BuffID);
		}
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
		if (m_EndAction && (m_EndAnim || null == base.anim.animator))
		{
			OnEndAction();
			return true;
		}
		if (!m_EndAction)
		{
			CheckOperateEnd();
		}
		return false;
	}

	private void CheckOperateEnd()
	{
		if (PeSingleton<MainPlayer>.Instance.entity == base.motionMgr.Entity && !base.entity.operateCmpt.HasOperate && (!PeGameMgr.IsMulti || base.skillCmpt.IsController()))
		{
			base.motionMgr.EndImmediately(ActionType);
		}
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
			base.anim.SetBool(m_AnimName, value: false);
		}
		if (this.endSleepEvt != null)
		{
			this.endSleepEvt(m_BuffID);
		}
		OnEndAction();
	}

	private void OnEndAction()
	{
		if (null != base.skillCmpt && m_BuffID != 0)
		{
			base.skillCmpt.CancelBuffById(m_BuffID);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = true;
		}
		base.motionMgr.SetMaskState(PEActionMask.Sleep, state: false);
		if (null != base.motionMgr.Entity.operateCmpt && base.motionMgr.Entity.operateCmpt.Operate is PESleep)
		{
			base.motionMgr.Entity.operateCmpt.Operate.StopOperate(base.motionMgr.Entity.operateCmpt, EOperationMask.Sleep);
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType) && "EndSleep" == eventParam)
		{
			m_EndAnim = true;
		}
	}
}
