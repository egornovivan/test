using UnityEngine;

public class SphericalObstacleData : MonoBehaviour
{
	[SerializeField]
	private Vector3 _center = Vector3.zero;

	[SerializeField]
	private float _radius = 1f;

	public Vector3 Center => _center;

	public float Radius => _radius;

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.position + Center, Radius);
	}
}
