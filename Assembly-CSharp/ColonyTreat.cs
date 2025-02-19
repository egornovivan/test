using CSRecord;

public class ColonyTreat : ColonyBase
{
	public ColonyTreat(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTreatData();
	}
}
