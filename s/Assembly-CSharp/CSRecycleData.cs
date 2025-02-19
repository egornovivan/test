using System.Collections.Generic;

public class CSRecycleData : CSObjectData
{
	public int m_ItemID = -1;

	public float m_CurTime;

	public float m_Time = -1f;

	public Dictionary<int, int> m_RecycleItems = new Dictionary<int, int>();

	public CSRecycleData()
	{
		dType = 6;
	}
}
