using UnityEngine;

namespace EVP;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyImpulse : MonoBehaviour
{
	public float velocity = 6f;

	public Vector3 direction = Vector3.up;

	public KeyCode key = KeyCode.E;

	private Rigidbody m_rigidbody;

	private void OnEnable()
	{
		m_rigidbody = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(key))
		{
			m_rigidbody.AddForce(direction.normalized * velocity, ForceMode.VelocityChange);
		}
	}
}
