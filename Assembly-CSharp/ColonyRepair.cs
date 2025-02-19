using CSRecord;

public class ColonyRepair : ColonyBase
{
	private CSRepairData _MyData;

	public ColonyRepair(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSRepairData();
		_MyData = (CSRepairData)_RecordData;
	}
}
