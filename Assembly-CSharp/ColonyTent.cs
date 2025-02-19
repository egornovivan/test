using CSRecord;

public class ColonyTent : ColonyBase
{
	public ColonyTent(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTentData();
	}
}
