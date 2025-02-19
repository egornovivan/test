using System;

namespace Pathea;

[Serializable]
public class Action_PumpWater : PEAction
{
	private const int DumpWaterSkillID = 20110052;

	private PEWaterPump m_WaterPump;

	public override PEActionType ActionType => PEActionType.Pump;

	public PEWaterPump waterPump
	{
		get
		{
			return m_WaterPump;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_WaterPump = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null != waterPump && null != base.skillCmpt && null != waterPump.m_AimAttr.m_AimTrans)
		{
			return VFVoxelWater.self.IsInWater(waterPump.m_AimAttr.m_AimTrans.position);
		}
		return false;
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Pump, state: true);
		if (null != waterPump && null != base.skillCmpt)
		{
			base.skillCmpt.StartSkill(base.skillCmpt, 20110052);
		}
		if (null != m_WaterPump)
		{
			m_WaterPump.SetActiveState(active: true);
		}
	}

	public override bool Update()
	{
		if (null != waterPump && null != base.skillCmpt && null != waterPump.m_AimAttr.m_AimTrans && VFVoxelWater.self.IsInWater(waterPump.m_AimAttr.m_AimTrans.position))
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
		base.motionMgr.SetMaskState(PEActionMask.Pump, state: false);
		if (null != base.skillCmpt)
		{
			base.skillCmpt.CancelSkillById(20110052);
		}
		if (null != m_WaterPump)
		{
			m_WaterPump.SetActiveState(active: false);
		}
	}
}
