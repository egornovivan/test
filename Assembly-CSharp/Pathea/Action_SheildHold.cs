using System;
using SkillSystem;

namespace Pathea;

[Serializable]
public class Action_SheildHold : PEAction, iEquipHideAbleAction
{
	private const int SheidBuffID = 30200177;

	private PESheild m_Sheild;

	private bool m_HideEquipInactive;

	public override PEActionType ActionType => PEActionType.HoldShield;

	public PESheild sheild
	{
		get
		{
			return m_Sheild;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Sheild = value;
		}
	}

	public bool hideEquipInactive
	{
		get
		{
			return m_HideEquipInactive;
		}
		set
		{
			m_HideEquipInactive = value;
			if (null != sheild)
			{
				sheild.gameObject.SetActive(!m_HideEquipInactive);
			}
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		return null != sheild;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == sheild) && !(null == base.anim))
		{
			base.anim.SetBool("SheildBlock", value: true);
			base.motionMgr.SetMaskState(PEActionMask.HoldShield, state: true);
			SkEntity.MountBuff(base.skillCmpt, 30200177, null, null);
			if (null != base.move && !base.motionMgr.IsActionRunning(PEActionType.EquipmentHold))
			{
				base.move.style = MoveStyle.Sword;
			}
			if (null != base.skillCmpt)
			{
				base.skillCmpt.SetAttribute(AttribType.EnableShieldBlock, 1f);
			}
			if (null != sheild && hideEquipInactive)
			{
				sheild.gameObject.SetActive(value: true);
			}
		}
	}

	public override bool Update()
	{
		if (null == sheild || null == base.anim)
		{
			return true;
		}
		if (null != base.move && !base.motionMgr.IsActionRunning(PEActionType.EquipmentHold))
		{
			base.move.style = MoveStyle.Sword;
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.HoldShield, state: false);
		if (null != base.anim)
		{
			base.anim.SetBool("SheildBlock", value: false);
		}
		if (null != base.skillCmpt)
		{
			base.skillCmpt.SetAttribute(AttribType.EnableShieldBlock, 0f);
		}
		if (null != base.move && !base.motionMgr.IsActionRunning(PEActionType.EquipmentHold))
		{
			base.move.style = base.move.baseMoveStyle;
		}
		if (null != sheild && hideEquipInactive)
		{
			sheild.gameObject.SetActive(value: false);
		}
		if (null != base.skillCmpt)
		{
			base.skillCmpt.CancelBuffById(30200177);
		}
	}
}
