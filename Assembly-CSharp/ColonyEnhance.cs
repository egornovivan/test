using CSRecord;

public class ColonyEnhance : ColonyBase
{
	public ColonyEnhance(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSEnhanceData();
	}
}
