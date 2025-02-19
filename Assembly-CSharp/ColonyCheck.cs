using CSRecord;

public class ColonyCheck : ColonyBase
{
	private CSCheckData _MyData;

	public ColonyCheck(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSCheckData();
		_MyData = (CSCheckData)_RecordData;
	}
}
