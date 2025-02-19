using PETools;
using UnityEngine;

public class PointPath
{
	private int m_Index;

	private bool m_IsLoop;

	private bool m_IsGravity;

	private float m_PickLength;

	private Vector3[] m_Points;

	public PointPath(Vector3[] points, float pickLength = 0.25f, bool loop = false, bool isGravity = true)
	{
		m_Index = -1;
		m_IsLoop = loop;
		m_IsGravity = isGravity;
		m_PickLength = pickLength;
		m_Points = points;
	}

	private float GetSqrDistance(Vector3 p1, Vector3 p2)
	{
		if (m_IsGravity)
		{
			return PEUtil.SqrMagnitudeH(p1, p2);
		}
		return PEUtil.SqrMagnitude(p1, p2);
	}

	private int GetClosestIndex(Vector3 pos)
	{
		if (m_Points == null || m_Points.Length <= 0)
		{
			return -1;
		}
		int result = 0;
		float num = GetSqrDistance(pos, m_Points[0]);
		for (int i = 1; i < m_Points.Length; i++)
		{
			float sqrDistance = GetSqrDistance(pos, m_Points[i]);
			if (sqrDistance < num)
			{
				result = i;
				num = sqrDistance;
			}
		}
		return result;
	}

	public Vector3 GetNextPoint(Vector3 pos)
	{
		if (m_Index == -1)
		{
			m_Index = GetClosestIndex(pos);
		}
		float sqrDistance = GetSqrDistance(pos, m_Points[m_Index]);
		if (sqrDistance < m_PickLength * m_PickLength)
		{
			m_Index++;
			if (m_Index >= m_Points.Length)
			{
				if (m_IsLoop)
				{
					m_Index = 0;
				}
				else
				{
					m_Index = m_Points.Length - 1;
				}
			}
		}
		if (m_Index >= 0 && m_Index < m_Points.Length)
		{
			return m_Points[m_Index];
		}
		return Vector3.zero;
	}
}
