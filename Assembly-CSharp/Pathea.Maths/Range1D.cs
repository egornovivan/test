using UnityEngine;

namespace Pathea.Maths;

public struct Range1D
{
	private float m_Center;

	private float m_Extend;

	public float center
	{
		get
		{
			return m_Center;
		}
		set
		{
			m_Center = value;
		}
	}

	public float extend
	{
		get
		{
			return m_Extend;
		}
		set
		{
			m_Extend = value;
		}
	}

	public float min => m_Center - m_Extend;

	public float max => m_Center + m_Extend;

	public void SetMinMax(float _min, float _max)
	{
		m_Center = (_min + _max) * 0.5f;
		m_Extend = Mathf.Abs((_max - _min) * 0.5f);
	}

	public void Merge(Range1D other)
	{
		SetMinMax(Mathf.Min(min, other.min), Mathf.Max(max, other.max));
	}
}
