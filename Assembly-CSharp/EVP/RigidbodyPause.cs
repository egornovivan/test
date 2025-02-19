using UnityEngine;

namespace EVP;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPause : MonoBehaviour
{
	public bool pause;

	public KeyCode key = KeyCode.P;

	private Rigidbody m_rigidbody;

	private bool m_pausedState;

	private Vector3 m_velocity = Vector3.zero;

	private Vector3 m_angularVelocity = Vector3.zero;

	private VehicleController m_vehicle;

	private void OnEnable()
	{
		m_rigidbody = GetComponent<Rigidbody>();
		m_vehicle = GetComponent<VehicleController>();
	}

	private void FixedUpdate()
	{
		if (pause && !m_pausedState)
		{
			m_velocity = m_rigidbody.velocity;
			m_angularVelocity = m_rigidbody.angularVelocity;
			m_pausedState = true;
			m_rigidbody.isKinematic = true;
			if ((bool)m_vehicle)
			{
				m_vehicle.enabled = false;
				DisableWheelColliders();
			}
		}
		else if (!pause && m_pausedState)
		{
			m_rigidbody.isKinematic = false;
			if ((bool)m_vehicle)
			{
				EnableWheelColliders();
				m_vehicle.enabled = true;
			}
			m_rigidbody.AddForce(m_velocity, ForceMode.VelocityChange);
			m_rigidbody.AddTorque(m_angularVelocity, ForceMode.VelocityChange);
			m_pausedState = false;
		}
	}

	private void DisableWheelColliders()
	{
		WheelCollider[] componentsInChildren = GetComponentsInChildren<WheelCollider>();
		WheelCollider[] array = componentsInChildren;
		foreach (WheelCollider wheelCollider in array)
		{
			wheelCollider.enabled = false;
		}
	}

	private void EnableWheelColliders()
	{
		WheelCollider[] componentsInChildren = GetComponentsInChildren<WheelCollider>();
		WheelCollider[] array = componentsInChildren;
		foreach (WheelCollider wheelCollider in array)
		{
			wheelCollider.enabled = true;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(key))
		{
			pause = !pause;
		}
	}
}
