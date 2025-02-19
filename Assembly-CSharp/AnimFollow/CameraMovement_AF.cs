using UnityEngine;

namespace AnimFollow;

public class CameraMovement_AF : MonoBehaviour
{
	public float movementSmooth = 1.5f;

	public float rotationSmooth = 7f;

	public Transform lookAtTransform;

	private Vector3 relCameraPos;

	private Vector3 absCameraPos;

	private void Awake()
	{
		if (!lookAtTransform)
		{
			Debug.LogWarning("The lookAtTransform is not assigned on " + base.name);
			lookAtTransform = base.transform;
		}
		else if (lookAtTransform.root.GetComponentsInChildren<Rigidbody>().Length == 0)
		{
			Debug.Log("The Camera " + base.name + " is looking at a model with no rigid body components.\nIf this is a AnimFollow system it is better to look at the ragdoll");
		}
		relCameraPos = new Vector3(Vector3.Dot(base.transform.position - lookAtTransform.position, lookAtTransform.forward), base.transform.position.y - lookAtTransform.position.y, Vector3.Dot(base.transform.position - lookAtTransform.position, lookAtTransform.up));
	}

	private void FixedUpdate()
	{
		Vector3 position = lookAtTransform.position;
		float x = relCameraPos.x;
		Vector3 vector = new Vector3(lookAtTransform.forward.x, 0f, lookAtTransform.forward.z);
		Vector3 vector2 = position + x * vector.normalized + relCameraPos.y * Vector3.up;
		float z = relCameraPos.z;
		Vector3 vector3 = new Vector3(lookAtTransform.up.x, 0f, lookAtTransform.up.z);
		Vector3 b = vector2 + z * vector3.normalized;
		base.transform.position = Vector3.Lerp(base.transform.position, b, movementSmooth * Time.deltaTime);
		SmoothLookAt();
	}

	private void SmoothLookAt()
	{
		Vector3 forward = lookAtTransform.position + 0.01f * Vector3.up - base.transform.position;
		Quaternion b = Quaternion.LookRotation(forward, Vector3.up);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, rotationSmooth * Time.deltaTime);
	}
}
