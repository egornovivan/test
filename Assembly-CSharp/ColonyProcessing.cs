using CSRecord;

public class ColonyProcessing : ColonyBase
{
	private CSProcessingData _MyData;

	public ColonyProcessing(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSProcessingData();
		_MyData = (CSProcessingData)_RecordData;
	}
}
