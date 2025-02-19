using CSRecord;

public class ColonyTrain : ColonyBase
{
	private CSTrainData _MyData;

	public ColonyTrain(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTrainData();
		_MyData = (CSTrainData)_RecordData;
	}
}
