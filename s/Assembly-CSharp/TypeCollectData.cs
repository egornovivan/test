using System.Collections.Generic;
using UnityEngine;

public class TypeCollectData
{
	public int m_TargetID;

	public int m_ScriptID;

	public string m_Desc;

	public int m_Type;

	public int m_ItemID;

	public int m_ItemNum;

	public int m_TargetItemID;

	public int m_MaxNum;

	public int m_Chance;

	public Vector3 m_TargetPos;

	public int m_TargetRadius;

	public List<int> m_ReceiveList;

	public int m_AdDist;

	public int m_AdRadius;

	public int[] m_randItemNum;

	public List<int> m_randItemID;

	public TypeCollectData()
	{
		m_ReceiveList = new List<int>();
		m_randItemNum = new int[3];
		m_randItemID = new List<int>();
	}

	public void RandItemActive()
	{
		if (m_randItemID.Count > 1)
		{
			m_ItemID = m_randItemID[Random.Range(0, m_randItemID.Count)];
			m_ItemNum = Random.Range(m_randItemNum[0], m_randItemNum[1] + 1) * m_randItemNum[2];
		}
	}
}
