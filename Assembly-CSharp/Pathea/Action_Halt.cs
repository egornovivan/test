using PEIK;
using UnityEngine;

namespace Pathea;

[HideInInspector]
public class Action_Halt : PEAction
{
	public override PEActionType ActionType => PEActionType.Halt;

	public IKAnimEffectCtrl animEffect { get; set; }

	public HumanPhyCtrl phyMotor { get; set; }

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (PeGameMgr.IsMulti && !base.skillCmpt.IsController())
		{
			return false;
		}
		return !base.motionMgr.isInAimState;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null != animEffect && null != phyMotor)
		{
			animEffect.StopMove(phyMotor.velocity);
		}
		base.motionMgr.SetMaskState(PEActionMask.Halt, state: true);
	}

	public override bool Update()
	{
		if (PeGameMgr.IsMulti && null != base.skillCmpt && !base.skillCmpt.IsController())
		{
			base.motionMgr.SetMaskState(PEActionMask.Halt, state: false);
			return true;
		}
		if (null != animEffect && null != phyMotor && animEffect.moveEffectRunning)
		{
			return false;
		}
		base.motionMgr.SetMaskState(PEActionMask.Halt, state: false);
		return true;
	}

	public override void EndImmediately()
	{
		if (null != animEffect)
		{
			animEffect.EndMoveEffect();
		}
		base.motionMgr.SetMaskState(PEActionMask.Halt, state: false);
	}
}
