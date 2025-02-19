using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class PlatformRotator : MonoBehaviour
{
	public float maxAngle = 70f;

	public float switchRotationTime = 0.5f;

	public float random = 0.5f;

	public float rotationSpeed = 50f;

	public Vector3 movePosition;

	public float moveSpeed = 5f;

	private Quaternion defaultRotation;

	private Quaternion targetRotation;

	private Vector3 targetPosition;

	private Vector3 velocity;

	private void Start()
	{
		defaultRotation = base.transform.rotation;
		targetPosition = base.transform.position + movePosition;
		StartCoroutine(SwitchRotation());
	}

	private void FixedUpdate()
	{
		GetComponent<Rigidbody>().MovePosition(Vector3.SmoothDamp(GetComponent<Rigidbody>().position, targetPosition, ref velocity, 1f, moveSpeed));
		if (Vector3.Distance(GetComponent<Rigidbody>().position, targetPosition) < 0.1f)
		{
			movePosition = -movePosition;
			targetPosition += movePosition;
		}
		GetComponent<Rigidbody>().rotation = Quaternion.RotateTowards(GetComponent<Rigidbody>().rotation, targetRotation, rotationSpeed * Time.deltaTime);
	}

	private IEnumerator SwitchRotation()
	{
		while (true)
		{
			float angle = Random.Range(0f - maxAngle, maxAngle);
			Vector3 axis = Random.onUnitSphere;
			targetRotation = Quaternion.AngleAxis(angle, axis) * defaultRotation;
			yield return new WaitForSeconds(switchRotationTime + Random.value * random);
		}
	}
}
