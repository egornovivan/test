using ItemAsset;
using Pathea;
using UnityEngine;

public class PEPujaGun : PEGun
{
	public string m_AimAnim;

	private AnimatorCmpt m_Anim;

	private bool m_AimState;

	public override bool Aimed => base.Aimed && m_AimState;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Anim = m_Entity.GetCmpt<AnimatorCmpt>();
	}

	public override void SetAimState(bool aimState)
	{
		m_AimState = aimState;
		if (m_AimState)
		{
			m_MotionMgr.ContinueAction(m_HandChangeAttr.m_ActiveActionType, m_HandChangeAttr.m_ActiveActionType);
		}
		else
		{
			m_MotionMgr.PauseAction(m_HandChangeAttr.m_ActiveActionType, m_HandChangeAttr.m_ActiveActionType);
		}
		if (!(null != m_Anim) || !(string.Empty != m_AimAnim))
		{
			return;
		}
		m_Anim.SetBool(m_AimAnim, aimState);
		if (PeGameMgr.IsMulti && null != m_Entity.netCmpt)
		{
			AiNetwork aiNetwork = m_Entity.netCmpt.network as AiNetwork;
			if (null != aiNetwork)
			{
				aiNetwork.RequestSetBool(Animator.StringToHash(m_AimAnim), aimState);
			}
		}
	}

	public override void HoldWeapon(bool hold)
	{
		base.HoldWeapon(hold);
		SetAimState(hold);
	}
}
