using UnityEngine;

namespace Pathea.Projectile;

public class SkProjectileDamageScale : MonoBehaviour
{
	public float disDamageF = 0.001f;

	private float m_MoveDis;

	private Vector3 m_LastPos;

	public float damageScale => Mathf.Clamp01(1f - m_MoveDis * disDamageF);

	private void Start()
	{
		m_LastPos = base.transform.position;
	}

	private void Update()
	{
		m_MoveDis += Vector3.Distance(base.transform.position, m_LastPos);
		m_LastPos = base.transform.position;
	}
}
