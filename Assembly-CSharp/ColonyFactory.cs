using CSRecord;

public class ColonyFactory : ColonyBase
{
	private CSFactoryData _MyData;

	public ColonyFactory(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSFactoryData();
		_MyData = (CSFactoryData)_RecordData;
	}
}
