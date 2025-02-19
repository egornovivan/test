using Pathea;

public class PEAT_InWater : PEAbnormalTrigger
{
	public BiologyViewCmpt view { get; set; }

	public PassengerCmpt passenger { get; set; }

	public override bool Hit()
	{
		if (null != passenger && passenger.IsOnCarrier())
		{
			return false;
		}
		return null != view.monoPhyCtrl && view.monoPhyCtrl.headInWater;
	}
}
