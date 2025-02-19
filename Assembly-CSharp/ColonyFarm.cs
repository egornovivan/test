using CSRecord;

public class ColonyFarm : ColonyBase
{
	private CSFarmData _MyData;

	public ColonyFarm(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSFarmData();
		_MyData = (CSFarmData)_RecordData;
	}
}
