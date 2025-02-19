using System;
using Pathea;

[Serializable]
public class Action_TwoHandWeaponHold : Action_HandChangeEquipHold
{
	private bool m_SubWeaponHoldState;

	public override PEActionType ActionType => PEActionType.TwoHandSwordHold;

	public PETwoHandWeapon twoHandWeapon
	{
		get
		{
			return base.handChangeEquipment as PETwoHandWeapon;
		}
		set
		{
			base.handChangeEquipment = value;
		}
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.DoAction(para);
		m_SubWeaponHoldState = false;
	}

	public override void EndImmediately()
	{
		base.EndImmediately();
		if (null != twoHandWeapon)
		{
			base.viewCmpt.Reattach(twoHandWeapon.m_LHandWeapon, twoHandWeapon.m_LHandPutOffBone);
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType))
		{
			switch (eventParam)
			{
			case "PutOnAnimEnd":
				m_PutOnAnimEnd = true;
				break;
			case "PutOffAnimEnd":
				m_PutOffAnimEnd = true;
				break;
			case "PutOnEquipment":
				ChangeHoldState(holdEquip: true);
				break;
			case "PutOffEquipment":
				ChangeHoldState(holdEquip: false);
				break;
			case "PutOnLHandEquipment":
				ChangeLHandHoldState(hold: true);
				break;
			case "PutOffLHandEquipment":
				ChangeLHandHoldState(hold: false);
				break;
			}
		}
	}

	private void ChangeLHandHoldState(bool hold)
	{
		if (null == twoHandWeapon)
		{
			return;
		}
		if (hold)
		{
			if (!m_SubWeaponHoldState)
			{
				m_SubWeaponHoldState = true;
				if (null != base.viewCmpt)
				{
					base.viewCmpt.Reattach(twoHandWeapon.m_LHandWeapon, twoHandWeapon.m_LHandPutOnBone);
				}
			}
		}
		else if (m_SubWeaponHoldState)
		{
			m_SubWeaponHoldState = false;
			if (null != base.viewCmpt)
			{
				base.viewCmpt.Reattach(twoHandWeapon.m_LHandWeapon, twoHandWeapon.m_LHandPutOffBone);
			}
		}
	}
}
