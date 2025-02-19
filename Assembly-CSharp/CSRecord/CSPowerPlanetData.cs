using System.Collections.Generic;

namespace CSRecord;

public class CSPowerPlanetData : CSObjectData
{
	public Dictionary<int, int> m_ChargingItems;

	public bool bShowElectric;

	public CSPowerPlanetData()
	{
		bShowElectric = true;
		dType = 32;
		m_ChargingItems = new Dictionary<int, int>();
	}
}
