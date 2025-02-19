using UnityEngine;

namespace EpsilonIndi;

public class ShipSightRotate : MonoBehaviour
{
	public float selfSpeed;

	public bool clockwise;

	public float rotationAngle;

	public float startAngle;

	private float sumAngle;

	[HideInInspector]
	public float rotateY;

	[HideInInspector]
	public Quaternion m_rotation;

	private void FixedUpdate()
	{
		sumAngle += selfSpeed * Time.deltaTime;
		sumAngle = ((!(sumAngle > 360f)) ? sumAngle : (sumAngle - 360f));
		rotateY = (sumAngle + startAngle) * ((!clockwise) ? (-1f) : 1f);
		m_rotation = Quaternion.AngleAxis(rotationAngle, base.transform.right) * Quaternion.AngleAxis(rotateY, Vector3.up);
		base.transform.rotation = m_rotation;
	}
}
