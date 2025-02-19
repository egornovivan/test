using System.Collections.Generic;
using CSRecord;

public class ColonyRecycle : ColonyBase
{
	public Dictionary<int, int> m_RecycleItems = new Dictionary<int, int>();

	public ColonyRecycle(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSRecycleData();
	}
}
