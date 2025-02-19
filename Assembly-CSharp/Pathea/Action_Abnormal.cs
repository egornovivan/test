namespace Pathea;

public class Action_Abnormal : PEAction
{
	private string m_AnimName;

	public override PEActionType ActionType => PEActionType.Abnormal;

	public override bool CanDoAction(PEActionParam para = null)
	{
		PEActionParamS pEActionParamS = para as PEActionParamS;
		string str = pEActionParamS.str;
		if (str != "Pant" && base.motionMgr.IsActionRunning(PEActionType.Move))
		{
			return false;
		}
		return !string.IsNullOrEmpty(str) && str != "0";
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Abnormal, state: true);
		PEActionParamS pEActionParamS = para as PEActionParamS;
		m_AnimName = pEActionParamS.str;
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: true);
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
		base.motionMgr.SetMaskState(PEActionMask.Abnormal, state: false);
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: false);
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
