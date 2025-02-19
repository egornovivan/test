using System;
using UnityEngine;

[Serializable]
public class Magazine
{
	public float m_Size;

	public float m_Value;

	public Magazine(float max = 30f, float current = 30f)
	{
		m_Size = max;
		m_Value = Mathf.Clamp(current, 0f, m_Size);
	}

	public Magazine(Magazine other)
	{
		if (other != null)
		{
			m_Size = other.m_Size;
			m_Value = other.m_Value;
		}
	}
}
