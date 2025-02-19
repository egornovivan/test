using UnityEngine;

namespace AnimFollow;

[RequireComponent(typeof(Rigidbody))]
public class BallTest_AF : MonoBehaviour
{
	public Transform hitTransform;

	public float ballVelocity = 20f;

	public float massOfBall = 40f;

	private void Awake()
	{
		GetComponent<Rigidbody>().isKinematic = false;
		if (!hitTransform)
		{
			Debug.LogWarning("hitTransform on " + base.name + " is not assigned.");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.B))
		{
			if (!hitTransform)
			{
				Debug.LogWarning("hitTransform on " + base.name + " is not assigned.");
				return;
			}
			GetComponent<Rigidbody>().mass = massOfBall;
			GetComponent<Rigidbody>().useGravity = false;
			GetComponent<Rigidbody>().velocity = (hitTransform.position - base.transform.position).normalized * ballVelocity;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		GetComponent<Rigidbody>().useGravity = true;
	}
}
