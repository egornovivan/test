using Pathea;
using Pathea.Operate;

public class DragItemMousePickBed : DragItemMousePick
{
	private ItemScript_Bed mBedView;

	private ItemScript_Bed bedView
	{
		get
		{
			if (mBedView == null)
			{
				mBedView = GetComponent<ItemScript_Bed>();
			}
			return mBedView;
		}
	}

	protected override string tipsText
	{
		get
		{
			if (Operatable())
			{
				return base.tipsText + "\n" + PELocalization.GetString(8000120);
			}
			return string.Empty;
		}
	}

	protected override void InitCmd(CmdList cmdList)
	{
		cmdList.Add("Turn", Turn90Degree);
		cmdList.Add("Get", OnGetBtn);
		if (!bedView.peSleep.CanOperateMask(EOperationMask.Sleep))
		{
			return;
		}
		cmdList.Add("Sleep", delegate
		{
			if (EntityMonsterBeacon.IsRunning())
			{
				PeTipMsg.Register(PELocalization.GetString(8000596), PeTipMsg.EMsgLevel.Warning);
			}
			else if (bedView.peSleep.CanOperateMask(EOperationMask.Sleep))
			{
				OperateCmpt operateCmpt = PeSingleton<MainPlayer>.Instance.entity.operateCmpt;
				if (!(null != operateCmpt) || !operateCmpt.HasOperate)
				{
					MotionMgrCmpt cmpt = PeSingleton<MainPlayer>.Instance.entity.GetCmpt<MotionMgrCmpt>();
					if (!(null != cmpt) || (!cmpt.IsActionRunning(PEActionType.Sleep) && cmpt.CanDoAction(PEActionType.Sleep)))
					{
						GameUI.Instance.mItemOp.ShowSleepWnd(show: true, this, bedView.peSleep, PeSingleton<MainPlayer>.Instance.entity);
					}
				}
			}
		});
	}

	public override bool CanCmd()
	{
		return base.CanCmd() && Operatable();
	}

	private bool Operatable()
	{
		if (bedView == null || bedView.peSleep == null)
		{
			return false;
		}
		if (!bedView.peSleep.CanOperateMask(EOperationMask.Sleep))
		{
			return false;
		}
		return true;
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (!PeInput.Get(PeInput.LogicFunction.InteractWithItem) || !CanCmd())
		{
			return;
		}
		if (EntityMonsterBeacon.IsRunning())
		{
			PeTipMsg.Register(PELocalization.GetString(8000596), PeTipMsg.EMsgLevel.Warning);
		}
		else
		{
			if (!Operatable())
			{
				return;
			}
			OperateCmpt operateCmpt = PeSingleton<MainPlayer>.Instance.entity.operateCmpt;
			if (!(null != operateCmpt) || !operateCmpt.HasOperate)
			{
				MotionMgrCmpt motionMgr = PeSingleton<MainPlayer>.Instance.entity.motionMgr;
				if (!(null != motionMgr) || (!motionMgr.IsActionRunning(PEActionType.Sleep) && motionMgr.CanDoAction(PEActionType.Sleep)))
				{
					GameUI.Instance.mItemOp.ShowSleepWnd(show: true, this, bedView.peSleep, PeSingleton<MainPlayer>.Instance.entity);
				}
			}
		}
	}
}
