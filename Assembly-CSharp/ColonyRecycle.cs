using System.Collections.Generic;
using CSRecord;

public class ColonyRecycle : ColonyBase
{
	private CSRecycleData _MyData;

	public Dictionary<int, int> m_RecycleItems = new Dictionary<int, int>();

	public ColonyRecycle(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSRecycleData();
		_MyData = (CSRecycleData)_RecordData;
	}
}
