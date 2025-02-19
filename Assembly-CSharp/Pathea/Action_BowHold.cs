using System;

namespace Pathea;

[Serializable]
public class Action_BowHold : Action_AimEquipHold
{
	private PEBow m_Bow;

	public bool m_IgnoreItem;

	public override PEActionType ActionType => PEActionType.BowHold;

	public PEBow bow
	{
		get
		{
			return m_Bow;
		}
		set
		{
			base.aimAbleEquip = value;
			m_Bow = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (m_IgnoreItem)
		{
			return true;
		}
		return null != base.packageCmpt && null != bow && base.packageCmpt.GetItemCount(bow.curItemID) > 0;
	}

	public override void OnModelBuild()
	{
		base.OnModelBuild();
		if (null != bow)
		{
			bow.SetArrowShowState(show: true);
			bow.SetBowOpenState(openBow: true);
		}
	}

	public override void EndAction()
	{
		if (!(null == base.anim) && !(null == bow))
		{
			base.EndAction();
			bow.SetBowOpenState(openBow: false);
			bow.SetArrowShowState(show: false);
		}
	}

	public override void EndImmediately()
	{
		base.EndImmediately();
		if (null != bow)
		{
			bow.SetArrowShowState(show: false);
			bow.SetBowOpenState(openBow: false);
		}
	}

	public override void ChangeHoldState(bool hold, bool checkState = true, bool isReattach = true)
	{
		base.ChangeHoldState(hold, checkState, isReattach);
		if (!hold && null != bow)
		{
			bow.SetArrowShowState(show: false);
			bow.SetBowOpenState(openBow: false);
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		base.OnAnimEvent(eventParam);
		if (null != bow && base.motionMgr.IsActionRunning(ActionType))
		{
			switch (eventParam)
			{
			case "ShowArrow":
				bow.SetArrowShowState(show: true);
				break;
			case "OpenBow":
				bow.SetBowOpenState(openBow: true);
				break;
			case "CloseBow":
				bow.SetBowOpenState(openBow: false);
				break;
			}
		}
	}
}
