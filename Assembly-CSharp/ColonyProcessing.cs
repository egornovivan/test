using CSRecord;

public class ColonyProcessing : ColonyBase
{
	public ColonyProcessing(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSProcessingData();
	}
}
