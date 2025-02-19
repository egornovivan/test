using CSRecord;

public class ColonyAssembly : ColonyBase
{
	private CSAssemblyData _MyData;

	public ColonyAssembly(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSAssemblyData();
		_MyData = (CSAssemblyData)_RecordData;
	}
}
