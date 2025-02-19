using UnityEngine;

namespace AnimFollow;

public class CameraMovement1_AF : MonoBehaviour
{
	public float movementSmooth = 15f;

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
		relCameraPos = base.transform.position - lookAtTransform.position;
	}

	private void FixedUpdate()
	{
		Vector3 b = lookAtTransform.position + relCameraPos;
		base.transform.position = Vector3.Lerp(base.transform.position, b, movementSmooth * Time.deltaTime);
	}

	private void SmoothLookAt()
	{
		Vector3 forward = lookAtTransform.position + 0.01f * Vector3.up - base.transform.position;
		Quaternion b = Quaternion.LookRotation(forward, Vector3.up);
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, rotationSmooth * Time.deltaTime);
	}
}
