using Pathea;
using Pathea.Operate;

namespace WhiteCat;

public class VCPBed : VCSimpleObjectPart
{
	public PESleep sleepPivot;

	public override CmdList GetCmdList()
	{
		CmdList cmdList = base.GetCmdList();
		if (sleepPivot.CanOperateMask(EOperationMask.Sleep))
		{
			cmdList.Add("Sleep", delegate
			{
				if (sleepPivot.CanOperateMask(EOperationMask.Sleep))
				{
					GameUI.Instance.mItemOp.ShowSleepWnd(show: true, this, sleepPivot, PeSingleton<MainPlayer>.Instance.entity);
				}
			});
		}
		return cmdList;
	}
}
