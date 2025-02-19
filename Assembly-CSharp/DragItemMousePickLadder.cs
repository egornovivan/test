using Pathea;
using Pathea.Operate;

public class DragItemMousePickLadder : DragItemMousePick
{
	private ItemScript_ClimbLadder mClimb;

	public bool rightMouse = true;

	private ItemScript_ClimbLadder climb
	{
		get
		{
			if (mClimb == null)
			{
				mClimb = GetComponent<ItemScript_ClimbLadder>();
			}
			return mClimb;
		}
	}

	protected override string tipsText
	{
		get
		{
			if (CanPlayerClimb())
			{
				if (base.tipsText != string.Empty)
				{
					return base.tipsText + "\n" + PELocalization.GetString(8000128);
				}
				return PELocalization.GetString(8000128);
			}
			return base.tipsText;
		}
	}

	private bool CanPlayerClimb()
	{
		return true;
	}

	public void TryClimbLadder(PeCmpt who)
	{
		OperateCmpt operateCmpt = who.Entity.operateCmpt;
		if (!(operateCmpt == null) && !operateCmpt.HasOperate && !(null == operateCmpt.Entity.motionMgr))
		{
			if (operateCmpt.Entity.motionMgr.IsActionRunning(PEActionType.Climb))
			{
				climb.opClimb.StopOperate(operateCmpt, EOperationMask.ClimbLadder);
			}
			else
			{
				climb.opClimb.StartOperate(operateCmpt, EOperationMask.ClimbLadder);
			}
		}
	}

	public override bool CanCmd()
	{
		return base.CanCmd() && climb.opClimb.IsIdle();
	}

	protected override void CheckOperate()
	{
		if (rightMouse)
		{
			base.CheckOperate();
		}
	}
}
