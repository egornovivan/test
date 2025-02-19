using System.Collections.Generic;
using UnityEngine;

namespace CSRecord;

public class CSObjectData : CSDefaultData
{
	public bool m_Alive;

	public string m_Name;

	public Vector3 m_Position;

	public float m_Durability;

	public float m_CurRepairTime;

	public float m_RepairTime;

	public float m_RepairValue;

	public float m_CurDeleteTime;

	public float m_DeleteTime;

	public Bounds m_Bounds;

	public Dictionary<int, int> m_DeleteGetsItem;

	public CSObjectData()
	{
		m_DeleteGetsItem = new Dictionary<int, int>();
		m_Bounds = default(Bounds);
	}
}
