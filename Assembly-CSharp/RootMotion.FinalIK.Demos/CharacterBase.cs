using UnityEngine;

namespace RootMotion.FinalIK.Demos;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class CharacterBase : MonoBehaviour
{
	protected const float half = 0.5f;

	[SerializeField]
	protected float airborneThreshold = 0.6f;

	[SerializeField]
	private float slopeStartAngle = 50f;

	[SerializeField]
	private float slopeEndAngle = 85f;

	[SerializeField]
	private float spherecastRadius = 0.1f;

	[SerializeField]
	private LayerMask groundLayers;

	[SerializeField]
	private PhysicMaterial zeroFrictionMaterial;

	[SerializeField]
	private PhysicMaterial highFrictionMaterial;

	protected float originalHeight;

	protected Vector3 originalCenter;

	protected CapsuleCollider capsule;

	public abstract void Move(Vector3 deltaPosition);

	protected virtual void Start()
	{
		capsule = GetComponent<Collider>() as CapsuleCollider;
		originalHeight = capsule.height;
		originalCenter = capsule.center;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
	}

	protected virtual RaycastHit GetSpherecastHit()
	{
		Ray ray = new Ray(GetComponent<Rigidbody>().position + Vector3.up * airborneThreshold, Vector3.down);
		RaycastHit hitInfo = default(RaycastHit);
		Physics.SphereCast(ray, spherecastRadius, out hitInfo, airborneThreshold * 2f, groundLayers);
		return hitInfo;
	}

	public float GetAngleFromForward(Vector3 worldDirection)
	{
		Vector3 vector = base.transform.InverseTransformDirection(worldDirection);
		return Mathf.Atan2(vector.x, vector.z) * 57.29578f;
	}

	protected void RigidbodyRotateAround(Vector3 point, Vector3 axis, float angle)
	{
		Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
		Vector3 vector = GetComponent<Rigidbody>().position - point;
		GetComponent<Rigidbody>().MovePosition(point + quaternion * vector);
		GetComponent<Rigidbody>().MoveRotation(quaternion * GetComponent<Rigidbody>().rotation);
	}

	protected void ScaleCapsule(float mlp)
	{
		if (capsule.height != originalHeight * mlp)
		{
			capsule.height = Mathf.MoveTowards(capsule.height, originalHeight * mlp, Time.deltaTime * 4f);
			capsule.center = Vector3.MoveTowards(capsule.center, originalCenter * mlp, Time.deltaTime * 2f);
		}
	}

	protected void HighFriction()
	{
		GetComponent<Collider>().material = highFrictionMaterial;
	}

	protected void ZeroFriction()
	{
		GetComponent<Collider>().material = zeroFrictionMaterial;
	}

	protected float GetSlopeDamper(Vector3 velocity, Vector3 groundNormal)
	{
		float num = 90f - Vector3.Angle(velocity, groundNormal);
		num -= slopeStartAngle;
		float num2 = slopeEndAngle - slopeStartAngle;
		return 1f - Mathf.Clamp(num / num2, 0f, 1f);
	}
}
