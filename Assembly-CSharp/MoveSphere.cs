using UnityEngine;
using WhiteCat;

public class MoveSphere : MonoBehaviour
{
	public Path path;

	public float resetHeight = -1f;

	public float forceScale = 1f;

	private void Update()
	{
		Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		GetComponent<Rigidbody>().AddForce(Camera.main.transform.TransformDirection(direction) * forceScale);
		if (base.transform.position.y < resetHeight)
		{
			path.GetClosestPathPosition(base.transform.position, 1f, out var splineIndex, out var splineTime);
			base.transform.position = path.GetSplinePoint(splineIndex, splineTime) + Vector3.up;
		}
		Camera.main.transform.LookAt(base.transform);
	}
}
