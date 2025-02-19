using UnityEngine;

public class PickupMotion : MonoBehaviour
{
	public float SpinSpeed = 90f;

	public float BobSpeed = 1f;

	public float BobDistance = 1f;

	private Vector3 positionOffset;

	protected virtual void Update()
	{
		base.transform.position -= positionOffset;
		positionOffset = base.transform.up * Mathf.Sin(Time.time * BobSpeed) * BobDistance;
		base.transform.position += positionOffset;
		base.transform.rotation *= Quaternion.AngleAxis(SpinSpeed * Time.deltaTime, base.transform.up);
	}
}
