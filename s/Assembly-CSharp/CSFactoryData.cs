using System.Collections.Generic;
using CustomData;

public class CSFactoryData : CSObjectData
{
	public List<CompoudItem> m_CompoudItems;

	public CSFactoryData()
	{
		dType = 8;
		m_CompoudItems = new List<CompoudItem>();
	}
}
