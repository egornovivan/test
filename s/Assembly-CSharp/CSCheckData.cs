using System.Collections.Generic;

public class CSCheckData : CSObjectData
{
	public List<int> npcIds = new List<int>();

	public float m_CurTime = -1f;

	public float m_Time = -1f;

	public bool isNpcReady;

	public bool occupied;

	public CSCheckData()
	{
		dType = 12;
	}
}
