using UnityEngine;

namespace Pathea;

public class AttribPair
{
	public AttribType m_Current;

	public AttribType m_Max;

	public AttribPair(AttribType current, AttribType max)
	{
		m_Current = current;
		m_Max = max;
	}

	public void CheckAttrib(PESkEntity entity, AttribType type, float value)
	{
		if (type == m_Current)
		{
			if (value < 0f || value > entity.GetAttribute(m_Max))
			{
				entity.SetAttribute(m_Current, Mathf.Clamp(value, 0f, entity.GetAttribute(m_Max)));
			}
		}
		else if (type == m_Max)
		{
			float attribute = entity.GetAttribute(m_Current);
			if (attribute > value)
			{
				entity.SetAttribute(m_Current, value);
			}
		}
	}
}
