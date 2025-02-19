using UnityEngine;

namespace EVP;

public class WheelData
{
	public Wheel wheel;

	public Transform transform;

	public WheelCollider collider;

	public float forceDistance;

	public float steerAngle;

	public bool grounded;

	public WheelHit hit;

	public float suspensionCompression;

	public float downforce;

	public Vector3 velocity = Vector3.zero;

	public Vector2 localVelocity = Vector2.zero;

	public Vector2 localRigForce = Vector2.zero;

	public Vector2 tireSlip = Vector2.zero;

	public Vector2 tireForce = Vector2.zero;

	public float angularVelocity;

	public float angularPosition;

	public float slipRatio;

	public float elongation;

	public float elongationTarget;

	public float sleep_time;
}
