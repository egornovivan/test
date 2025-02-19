using CSRecord;

public class ColonyTrain : ColonyBase
{
	public ColonyTrain(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTrainData();
	}
}
