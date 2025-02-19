using CSRecord;

public class ColonyAssembly : ColonyBase
{
	public ColonyAssembly(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSAssemblyData();
	}
}
