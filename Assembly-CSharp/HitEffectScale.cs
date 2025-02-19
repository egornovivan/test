using UnityEngine;

public class HitEffectScale : MonoBehaviour
{
	public ParticleSystem root;

	public ParticleSystem[] childs;

	private float m_Scale;

	private Vector3 m_EmissionScale = Vector3.one;

	public Vector3 EmissionScale
	{
		set
		{
			if (m_EmissionScale != value)
			{
				m_EmissionScale = value;
				base.transform.localScale = m_EmissionScale;
			}
		}
	}

	public float Scale
	{
		set
		{
			if (Mathf.Abs(m_Scale - value) > 0.1f && value > float.Epsilon)
			{
				m_Scale = value;
				for (int i = 0; i < childs.Length; i++)
				{
					childs[i].startSize *= m_Scale * 0.5f;
				}
			}
		}
	}
}
