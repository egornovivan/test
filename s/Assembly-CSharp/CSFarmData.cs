using System.Collections.Generic;

public class CSFarmData : CSObjectData
{
	public Dictionary<int, int> m_PlantSeeds;

	public Dictionary<int, int> m_Tools;

	public bool m_AutoPlanting;

	public bool m_SequentialPlanting;

	public CSFarmData()
	{
		dType = 7;
		m_PlantSeeds = new Dictionary<int, int>();
		m_Tools = new Dictionary<int, int>();
	}
}
