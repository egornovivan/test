using System.Collections.Generic;
using CustomData;

namespace CSRecord;

public class CSStorageData : CSObjectData
{
	public Dictionary<int, int> m_Items;

	public Queue<HistoryStruct> m_History;

	public CSStorageData()
	{
		dType = 2;
		m_Items = new Dictionary<int, int>();
		m_History = new Queue<HistoryStruct>();
	}
}
