using System.Collections.Generic;

public class CSTreatData : CSObjectData
{
	public int m_ObjID = -1;

	public List<int> npcIds = new List<int>();

	public float m_CurTime = -1f;

	public float m_Time = -1f;

	public bool isNpcReady;

	public bool occupied;

	public CSTreatData()
	{
		dType = 13;
	}
}
