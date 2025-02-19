using CSRecord;

public class ColonyTreat : ColonyBase
{
	private CSTreatData _MyData;

	public ColonyTreat(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTreatData();
		_MyData = (CSTreatData)_RecordData;
	}
}
