using System;

namespace Pathea;

[Serializable]
public class Action_DrawWater : PEAction
{
	private const int DrawWaterSkillID = 20110051;

	private PEWaterPitcher m_WaterPitcher;

	public override PEActionType ActionType => PEActionType.Draw;

	public PEWaterPitcher waterPitcher
	{
		get
		{
			return m_WaterPitcher;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_WaterPitcher = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null != waterPitcher && null != base.skillCmpt && null != base.trans)
		{
			return VFVoxelWater.self.IsInWater(base.trans.position);
		}
		return false;
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Draw, state: true);
		if (null != waterPitcher && null != base.skillCmpt)
		{
			base.skillCmpt.StartSkill(base.skillCmpt, 20110051);
		}
		if (null != base.anim)
		{
			base.anim.ResetTrigger("ResetFullBody");
		}
	}

	public override bool Update()
	{
		if (null != base.skillCmpt && base.skillCmpt.IsSkillRunning(20110051))
		{
			return false;
		}
		base.motionMgr.SetMaskState(PEActionMask.Draw, state: false);
		return true;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Draw, state: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
		}
		if (null != base.skillCmpt)
		{
			base.skillCmpt.CancelSkillById(20110051);
		}
	}
}
