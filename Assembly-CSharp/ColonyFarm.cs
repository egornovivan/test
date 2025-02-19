using CSRecord;

public class ColonyFarm : ColonyBase
{
	public ColonyFarm(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSFarmData();
	}
}
