using System.Collections.Generic;
using UnityEngine;

public class TypeTowerDefendsData
{
	public enum PosType
	{
		getPos,
		pos,
		npcPos,
		doodadPos,
		conoly,
		camp
	}

	public struct MyPos
	{
		public PosType type;

		public Vector3 pos;

		public int id;
	}

	public int m_TargetID;

	public int m_ScriptID;

	public string m_Desc;

	public int m_Time;

	public MyPos m_Pos;

	public List<int> m_iNpcList;

	public List<int> m_ObjectList;

	public int m_tolTime;

	public int m_range;

	public int m_Count;

	public int m_TdInfoId;

	public List<int> m_SweepId;

	public List<int> m_ReceiveList;

	public Vector3 finallyPos = default(Vector3);

	public TypeTowerDefendsData()
	{
		m_iNpcList = new List<int>();
		m_ObjectList = new List<int>();
		m_ReceiveList = new List<int>();
		m_SweepId = new List<int>();
	}
}
