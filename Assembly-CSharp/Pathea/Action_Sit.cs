using System;
using System.Collections.Generic;
using SkillSystem;

namespace Pathea;

[Serializable]
public class Action_Sit : PEAction
{
	private string m_AnimName;

	private bool m_EndAction;

	private int m_BuffID;

	public override PEActionType ActionType => PEActionType.Sit;

	public override void DoAction(PEActionParam para = null)
	{
		PEActionParamVQSN pEActionParamVQSN = para as PEActionParamVQSN;
		base.motionMgr.SetMaskState(PEActionMask.Sit, state: true);
		m_EndAction = false;
		if (null != base.trans)
		{
			base.trans.position = pEActionParamVQSN.vec;
			base.trans.rotation = pEActionParamVQSN.q;
		}
		if (null != base.anim)
		{
			m_AnimName = pEActionParamVQSN.str;
			base.anim.SetBool(m_AnimName, value: true);
			base.anim.ResetTrigger("ResetFullBody");
		}
		m_BuffID = pEActionParamVQSN.n;
		if (m_BuffID != 0 && null != base.motionMgr.Entity.skEntity)
		{
			SkEntity.MountBuff(base.motionMgr.Entity.skEntity, m_BuffID, new List<int>(), new List<float>());
		}
		base.motionMgr.FreezePhyState(GetType(), v: true);
		if (null != base.motionMgr.Entity.IKCmpt)
		{
			base.motionMgr.Entity.IKCmpt.ikEnable = false;
		}
	}

	public override bool Update()
	{
		if (m_EndAction)
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
		}
		OnEndAction();
	}

	private void OnEndAction()
	{
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: false);
		}
		if (m_BuffID != 0 && null != base.motionMgr.Entity.skEntity)
		{
			base.motionMgr.Entity.skEntity.CancelBuffById(m_BuffID);
		}
		base.motionMgr.FreezePhyState(GetType(), v: false);
		base.motionMgr.SetMaskState(PEActionMask.Sit, state: false);
		if (null != base.motionMgr.Entity.IKCmpt)
		{
			base.motionMgr.Entity.IKCmpt.ikEnable = true;
		}
	}
}
