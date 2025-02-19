using CSRecord;

public class ColonyStorage : ColonyBase
{
	public ColonyStorage(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSStorageData();
	}
}
