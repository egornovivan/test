using UnityEngine;

[RequireComponent(typeof(AlignmentTracker))]
public class HeadLookController : MonoBehaviour
{
	public Transform neck;

	public Transform head;

	public Vector3 headLookVector = Vector3.forward;

	public Vector3 headUpVector = Vector3.up;

	public float rotationMultiplier = 0.5f;

	private Vector3 referenceLookDir;

	private Vector3 referenceUpDir;

	private AlignmentTracker tr;

	private Vector3 lookDir;

	private Vector3 upDir;

	private void Start()
	{
		tr = GetComponent(typeof(AlignmentTracker)) as AlignmentTracker;
		Quaternion rotation = neck.parent.rotation;
		Quaternion quaternion = Quaternion.Inverse(rotation);
		referenceLookDir = quaternion * base.transform.rotation * headLookVector.normalized;
		referenceUpDir = quaternion * base.transform.rotation * headUpVector.normalized;
		lookDir = referenceLookDir;
		upDir = referenceUpDir;
	}

	private void LateUpdate()
	{
		if (Time.deltaTime == 0f)
		{
			return;
		}
		Quaternion rotation = neck.parent.rotation;
		Quaternion quaternion = Quaternion.Inverse(rotation);
		Vector3 vector = base.transform.rotation * headLookVector * 0.01f;
		vector += Util.ProjectOntoPlane(tr.velocity, base.transform.up);
		vector = (Quaternion.AngleAxis(Mathf.Clamp(tr.angularVelocitySmoothed.magnitude / 2f, -120f, 120f), tr.angularVelocitySmoothed) * vector).normalized;
		Vector3 normalized = (quaternion * vector).normalized;
		float num = Vector3.Dot(normalized, referenceLookDir);
		if (num < 0f)
		{
			if (Vector3.Dot(normalized, referenceUpDir) < 0f)
			{
				normalized -= Vector3.Project(normalized, referenceUpDir);
			}
			else
			{
				normalized += Vector3.Project(normalized, referenceUpDir) * num;
			}
		}
		float num2 = Vector3.Angle(referenceLookDir, normalized);
		Vector3 axis = Vector3.Cross(referenceLookDir, normalized);
		if (num2 > 180f)
		{
			num2 -= 360f;
		}
		num2 *= rotationMultiplier;
		normalized = Quaternion.AngleAxis(num2, axis) * referenceLookDir;
		num2 = Vector3.Angle(referenceLookDir, normalized);
		axis = Vector3.Cross(referenceLookDir, normalized);
		if (num2 > 180f)
		{
			num2 -= 360f;
		}
		num2 = Mathf.Clamp(num2, -80f, 80f);
		normalized = Quaternion.AngleAxis(num2, axis) * referenceLookDir;
		Vector3 tangent = referenceUpDir;
		Vector3.OrthoNormalize(ref normalized, ref tangent);
		lookDir = Vector3.Slerp(lookDir, normalized, Time.deltaTime * 5f);
		upDir = Vector3.Slerp(upDir, tangent, Time.deltaTime * 5f);
		Vector3.OrthoNormalize(ref lookDir, ref upDir);
		Quaternion b = rotation * Quaternion.LookRotation(lookDir, upDir) * Quaternion.Inverse(rotation * Quaternion.LookRotation(referenceLookDir, referenceUpDir));
		Quaternion quaternion2 = Quaternion.Slerp(Quaternion.identity, b, 0.5f);
		neck.rotation = quaternion2 * neck.rotation;
		head.rotation = quaternion2 * head.rotation;
	}
}
