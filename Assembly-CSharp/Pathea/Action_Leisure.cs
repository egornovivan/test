using System;

namespace Pathea;

[Serializable]
public class Action_Leisure : PEAction
{
	private string m_AnimName;

	public override PEActionType ActionType => PEActionType.Leisure;

	public override bool CanDoAction(PEActionParam para = null)
	{
		PEActionParamS pEActionParamS = para as PEActionParamS;
		string str = pEActionParamS.str;
		return !string.IsNullOrEmpty(str) && str != "0";
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Talk, state: true);
		PEActionParamS pEActionParamS = para as PEActionParamS;
		m_AnimName = pEActionParamS.str;
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: true);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
	}

	public override bool Update()
	{
		if (null == base.anim.animator)
		{
			EndImmediately();
			return true;
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Talk, state: false);
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: false);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = true;
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType) && "EndLeisure" == eventParam)
		{
			base.motionMgr.EndImmediately(ActionType);
		}
	}
}
