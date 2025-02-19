using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Knocked : PEAction
{
	public float m_GetUpTime = 3f;

	private float m_AutoGetUpTime;

	private bool m_AutoGetUp;

	public override PEActionType ActionType => PEActionType.Knocked;

	public HumanPhyCtrl phyCtrl { get; set; }

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.viewCmpt)
		{
			if (!base.viewCmpt.IsRagdoll)
			{
				base.viewCmpt.ActivateRagdoll(null, isGetupReady: false);
			}
			base.viewCmpt.ActivateInjured(value: false);
		}
		base.motionMgr.SetMaskState(PEActionMask.Knocked, state: true);
		base.motionMgr.FreezePhyState(GetType(), v: true);
		m_AutoGetUp = true;
		m_AutoGetUpTime = m_GetUpTime;
		if (null != phyCtrl)
		{
			phyCtrl.desiredMovementDirection = Vector3.zero;
			phyCtrl.CancelMoveRequest();
		}
	}

	public override bool Update()
	{
		if (m_AutoGetUp)
		{
			m_AutoGetUpTime -= Time.deltaTime;
			if (m_AutoGetUpTime <= 0f)
			{
				m_AutoGetUp = false;
				OnEndAction();
				base.motionMgr.DoActionImmediately(PEActionType.GetUp);
			}
		}
		return false;
	}

	public override void EndImmediately()
	{
		OnEndAction();
	}

	private void OnEndAction()
	{
		base.motionMgr.FreezePhyState(GetType(), v: false);
		base.motionMgr.SetMaskState(PEActionMask.Knocked, state: false);
		if (null != base.viewCmpt)
		{
			base.viewCmpt.ActivateInjured(value: true, 1f);
		}
	}
}
