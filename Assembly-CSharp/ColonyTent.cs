using CSRecord;

public class ColonyTent : ColonyBase
{
	private CSTentData _MyData;

	public ColonyTent(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTentData();
		_MyData = (CSTentData)_RecordData;
	}
}
