using CSRecord;

public class ColonyStorage : ColonyBase
{
	private CSStorageData _MyData;

	public ColonyStorage(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSStorageData();
		_MyData = (CSStorageData)_RecordData;
	}
}
