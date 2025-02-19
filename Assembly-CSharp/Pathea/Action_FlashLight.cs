using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_FlashLight : PEAction
{
	private PEFlashLight m_FlashLight;

	public override PEActionType ActionType => PEActionType.HoldFlashLight;

	public PEFlashLight flashLight
	{
		get
		{
			return m_FlashLight;
		}
		set
		{
			m_FlashLight = value;
			if (null != m_FlashLight)
			{
				if (null != base.ikCmpt.m_IKFlashLight)
				{
					base.ikCmpt.m_IKFlashLight.aimTrans = m_FlashLight.aimTrans;
				}
				base.motionMgr.DoActionImmediately(PEActionType.HoldFlashLight);
			}
			else
			{
				base.motionMgr.EndAction(PEActionType.HoldFlashLight);
			}
		}
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.HoldFlashingLight, state: true);
		if (null != base.anim)
		{
			base.anim.SetBool("FlashLight", value: true);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.aimActive = true;
			base.ikCmpt.flashLightActive = true;
			base.ikCmpt.aimTargetPos = Vector3.zero;
		}
	}

	public override void PauseAction()
	{
		if (null != base.anim)
		{
			base.anim.SetBool("FlashLight", value: false);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.aimActive = false;
			base.ikCmpt.flashLightActive = false;
		}
	}

	public override void ContinueAction()
	{
		if (null != base.anim)
		{
			base.anim.SetBool("FlashLight", value: true);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.aimActive = true;
			base.ikCmpt.flashLightActive = true;
		}
	}

	public override void OnModelBuild()
	{
		if (null != base.ikCmpt)
		{
			base.ikCmpt.aimActive = true;
			base.ikCmpt.flashLightActive = true;
			if (null != base.ikCmpt.m_IKFlashLight && null != flashLight)
			{
				base.ikCmpt.m_IKFlashLight.aimTrans = flashLight.aimTrans;
			}
		}
	}

	public override void OnModelDestroy()
	{
	}

	public override bool Update()
	{
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.HoldFlashingLight, state: false);
		if (null != base.anim)
		{
			base.anim.SetBool("FlashLight", value: false);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.aimActive = false;
			base.ikCmpt.flashLightActive = false;
		}
	}
}
