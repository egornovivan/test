using System.Collections.Generic;

namespace CSRecord;

public class CSCheckData : CSObjectData
{
	public List<int> npcIds = new List<int>();

	public float m_CurTime;

	public float m_Time = -1f;

	public bool isNpcReady;

	public bool occupied;

	public CSCheckData()
	{
		dType = 12;
	}
}
