using System.Collections.Generic;
using CustomData;

namespace CSRecord;

public class CSFactoryData : CSObjectData
{
	public List<CompoudItem> m_CompoudItems;

	public CSFactoryData()
	{
		dType = 8;
		m_CompoudItems = new List<CompoudItem>();
	}
}
