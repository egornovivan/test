using System.Collections.Generic;

public class CSPowerPlanetData : CSObjectData
{
	public Dictionary<int, int> m_ChargingItems;

	public CSPowerPlanetData()
	{
		dType = 32;
		m_ChargingItems = new Dictionary<int, int>();
	}
}
