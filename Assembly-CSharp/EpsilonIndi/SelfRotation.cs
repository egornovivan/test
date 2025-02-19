using UnityEngine;

namespace EpsilonIndi;

public class SelfRotation : MonoBehaviour
{
	public float selfSpeed;

	public bool clockwise;

	public float rotationAngle;

	public float startAngle;

	private float sumAngle;

	[HideInInspector]
	public float s_angle;

	[HideInInspector]
	public Quaternion m_rotationX;

	[HideInInspector]
	public Quaternion m_rotationY;

	public Quaternion UpdateRotateY(float dt)
	{
		sumAngle += selfSpeed * dt;
		sumAngle = ((!(sumAngle > 360f)) ? sumAngle : (sumAngle - 360f));
		s_angle = (sumAngle + startAngle) * ((!clockwise) ? (-1f) : 1f);
		m_rotationY = Quaternion.AngleAxis(s_angle, Vector3.up);
		return m_rotationY;
	}

	public void TestUpdate()
	{
		base.transform.rotation = m_rotationX * m_rotationY;
	}

	private void FixedUpdate()
	{
		m_rotationX = Quaternion.AngleAxis(rotationAngle, Vector3.right);
	}
}
