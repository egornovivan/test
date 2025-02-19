using UnityEngine;

namespace EVP;

public class VehicleViewConfig : MonoBehaviour
{
	public Transform lookAtPoint;

	public Transform driverView;

	public float viewDistance = 10f;

	public float viewHeight = 3.5f;

	public float viewDamping = 3f;

	public float viewMinDistance = 3.8f;

	public float viewMinHeight;
}
