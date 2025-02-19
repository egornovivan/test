using System;

namespace Pathea;

[Serializable]
public class Action_Revive : PEAction
{
	public override PEActionType ActionType => PEActionType.Revive;

	public override bool CanDoAction(PEActionParam para = null)
	{
		return null == base.viewCmpt || base.viewCmpt.IsReadyGetUp();
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.skillCmpt)
		{
			if (base.skillCmpt.GetAttribute(AttribType.Hp) < 1f)
			{
				base.skillCmpt.SetAttribute(AttribType.Hp, 1f);
			}
			base.skillCmpt.SetAttribute(AttribType.Oxygen, base.skillCmpt.GetAttribute(AttribType.OxygenMax));
			base.skillCmpt.SetAttribute(AttribType.Stamina, base.skillCmpt.GetAttribute(AttribType.StaminaMax));
		}
		base.motionMgr.SetMaskState(PEActionMask.Revive, state: true);
		base.motionMgr.FreezePhyState(GetType(), v: true);
		base.motionMgr.Entity.SendMsg(EMsg.State_Revive);
		PEActionParamB pEActionParamB = para as PEActionParamB;
		bool b = pEActionParamB.b;
		if (null != base.viewCmpt)
		{
			base.viewCmpt.DeactivateRagdoll(b);
			base.viewCmpt.ActivateInjured(value: false);
		}
	}

	public override bool Update()
	{
		if (null != base.viewCmpt && base.viewCmpt.IsRagdoll)
		{
			return false;
		}
		OnEndAction();
		return true;
	}

	public override void EndImmediately()
	{
		OnEndAction();
	}

	private void OnEndAction()
	{
		base.motionMgr.SetMaskState(PEActionMask.Revive, state: false);
		base.motionMgr.FreezePhyState(GetType(), v: false);
		if (null != base.viewCmpt)
		{
			base.viewCmpt.ActivateInjured(value: true, 1.5f);
		}
	}
}
