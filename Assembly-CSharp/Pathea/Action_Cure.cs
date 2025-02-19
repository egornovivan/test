using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Cure : PEAction
{
	private float m_TimeCount;

	private bool m_EndAction;

	private string m_AnimName;

	private Vector3 m_EndPos;

	private float m_EndRotY;

	public override PEActionType ActionType => PEActionType.Cure;

	public override void DoAction(PEActionParam para = null)
	{
		PEActionParamVFVFS pEActionParamVFVFS = para as PEActionParamVFVFS;
		base.motionMgr.Entity.position = pEActionParamVFVFS.vec1;
		base.motionMgr.Entity.rotation = Quaternion.Euler(0f, pEActionParamVFVFS.f1, 0f);
		m_EndPos = pEActionParamVFVFS.vec2;
		m_EndRotY = pEActionParamVFVFS.f2;
		m_AnimName = pEActionParamVFVFS.str;
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: true);
		}
		m_EndAction = false;
		base.motionMgr.SetMaskState(PEActionMask.Cure, state: true);
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
		base.motionMgr.FreezePhyState(GetType(), v: true);
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
				OnEndCure();
				return true;
			}
		}
		return false;
	}

	public override void EndAction()
	{
		m_EndAction = true;
		m_TimeCount = 0f;
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: false);
		}
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
		}
		OnEndCure();
	}

	private void OnEndCure()
	{
		base.motionMgr.SetMaskState(PEActionMask.Cure, state: false);
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: false);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = true;
		}
		base.motionMgr.FreezePhyState(GetType(), v: false);
		base.motionMgr.Entity.position = m_EndPos;
		base.motionMgr.Entity.rotation = Quaternion.Euler(0f, m_EndRotY, 0f);
	}
}
