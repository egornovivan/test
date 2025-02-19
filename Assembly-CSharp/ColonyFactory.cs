using CSRecord;

public class ColonyFactory : ColonyBase
{
	public ColonyFactory(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSFactoryData();
	}
}
