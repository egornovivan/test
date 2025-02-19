using CSRecord;

public class ColonyRepair : ColonyBase
{
	public ColonyRepair(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSRepairData();
	}
}
