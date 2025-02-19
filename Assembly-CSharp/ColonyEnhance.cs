using CSRecord;

public class ColonyEnhance : ColonyBase
{
	private CSEnhanceData _MyData;

	public ColonyEnhance(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSEnhanceData();
		_MyData = (CSEnhanceData)_RecordData;
	}
}
