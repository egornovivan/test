using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Lie : PEAction
{
	private float m_TimeCount;

	private bool m_EndAction;

	public override PEActionType ActionType => PEActionType.Lie;

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.anim)
		{
			base.anim.SetBool("Lie", value: true);
		}
		m_EndAction = false;
		base.motionMgr.SetMaskState(PEActionMask.Lie, state: true);
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
	}

	public override void OnModelBuild()
	{
		if (null != base.anim)
		{
			base.anim.SetBool("Lie", value: true);
		}
		m_EndAction = false;
		base.motionMgr.SetMaskState(PEActionMask.Lie, state: true);
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
	}

	public override void OnModelDestroy()
	{
	}

	public override bool Update()
	{
		if (m_EndAction)
		{
			m_TimeCount -= Time.deltaTime;
			if (m_TimeCount < 0f)
			{
				base.motionMgr.SetMaskState(PEActionMask.Lie, state: false);
				if (null != base.ikCmpt)
				{
					base.ikCmpt.ikEnable = true;
				}
				return true;
			}
		}
		return false;
	}

	public override void EndAction()
	{
		m_EndAction = true;
		m_TimeCount = 4.5f;
		if (null != base.anim)
		{
			base.anim.SetBool("Lie", value: false);
		}
	}
}
