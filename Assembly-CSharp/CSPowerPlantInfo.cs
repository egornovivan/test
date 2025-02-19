using System;
using System.Collections.Generic;

[Serializable]
public class CSPowerPlantInfo : CSInfo
{
	public float m_Radius;

	public float m_WorkedTime;

	public float m_ChargingRate;

	public List<int> m_WorkedTimeItemID = new List<int>();

	public List<int> m_WorkedTimeItemCnt = new List<int>();
}
