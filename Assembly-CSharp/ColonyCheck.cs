using CSRecord;

public class ColonyCheck : ColonyBase
{
	public ColonyCheck(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSCheckData();
	}
}
