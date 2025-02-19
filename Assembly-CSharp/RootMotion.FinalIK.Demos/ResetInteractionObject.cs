using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class ResetInteractionObject : MonoBehaviour
{
	public float resetDelay = 1f;

	private Vector3 defaultPosition;

	private Quaternion defaultRotation;

	private void Start()
	{
		defaultPosition = base.transform.position;
		defaultRotation = base.transform.rotation;
	}

	private void OnPickUp(Transform t)
	{
		StopAllCoroutines();
		StartCoroutine(ResetObject(Time.time + resetDelay));
	}

	private IEnumerator ResetObject(float resetTime)
	{
		while (Time.time < resetTime)
		{
			yield return null;
		}
		Poser poser = base.transform.parent.GetComponent<Poser>();
		if (poser != null)
		{
			poser.poseRoot = null;
			poser.weight = 0f;
		}
		base.transform.parent = null;
		base.transform.position = defaultPosition;
		base.transform.rotation = defaultRotation;
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}
