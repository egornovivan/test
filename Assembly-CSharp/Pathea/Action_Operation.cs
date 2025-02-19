using System;

namespace Pathea;

[Serializable]
public class Action_Operation : PEAction
{
	private string m_AnimName;

	private bool m_EndAction;

	public override PEActionType ActionType => PEActionType.Operation;

	public override void DoAction(PEActionParam para = null)
	{
		PEActionParamVQS pEActionParamVQS = para as PEActionParamVQS;
		base.motionMgr.SetMaskState(PEActionMask.Operation, state: true);
		m_EndAction = false;
		if (null != base.trans)
		{
			base.trans.position = pEActionParamVQS.vec;
			base.trans.rotation = pEActionParamVQS.q;
		}
		if (null != base.anim)
		{
			m_AnimName = pEActionParamVQS.str;
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.SetBool(m_AnimName, value: true);
		}
	}

	public override bool Update()
	{
		if (m_EndAction)
		{
			base.motionMgr.SetMaskState(PEActionMask.Operation, state: false);
			return true;
		}
		return false;
	}

	public override void EndAction()
	{
		m_EndAction = true;
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
			base.anim.SetBool(m_AnimName, value: false);
		}
		base.motionMgr.SetMaskState(PEActionMask.Operation, state: false);
	}
}
