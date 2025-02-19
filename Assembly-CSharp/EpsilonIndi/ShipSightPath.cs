using UnityEngine;

namespace EpsilonIndi;

public class ShipSightPath : MonoBehaviour
{
	public float orbitalRadius_km;

	public float periodSpeed;

	public float startAngle;

	public Transform primaryStar;

	public bool clockwise;

	public Vector3 offset;

	private float w_angle;

	private Vector3 vector;

	[HideInInspector]
	public Vector3 m_position;

	private void FixedUpdate()
	{
		w_angle += periodSpeed * Time.deltaTime;
		w_angle = ((!(w_angle > 360f)) ? w_angle : (w_angle - 360f));
		vector = Quaternion.AngleAxis((w_angle + startAngle) * ((!clockwise) ? (-1f) : 1f), Vector3.up) * (Vector3.forward * orbitalRadius_km + offset);
		m_position = primaryStar.GetComponent<OrbitalPath>().m_position + vector;
		base.transform.position = m_position;
	}
}
