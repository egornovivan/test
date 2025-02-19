using UnityEngine;

namespace EVP;

public class VehicleRandomInput : MonoBehaviour
{
	public float steerInterval = 2f;

	public float steerIntervalTolerance = 1f;

	public float steerChangeRate = 1f;

	[Range(0f, 1f)]
	public float steerStraightRandom = 0.4f;

	[Space(5f)]
	public float throttleInterval = 5f;

	public float throttleIntervalTolerance = 2f;

	public float throttleChangeRate = 3f;

	[Range(0f, 1f)]
	public float throttleForwardRandom = 0.8f;

	private float m_targetSteer;

	private float m_nextSteerTime;

	private float m_targetThrottle;

	private float m_targetBrake;

	private float m_nextThrottleTime;

	private VehicleController m_vehicle;

	private void OnEnable()
	{
		m_vehicle = GetComponent<VehicleController>();
	}

	private void Update()
	{
		if (Time.time > m_nextSteerTime)
		{
			if (Random.value < steerStraightRandom)
			{
				m_targetSteer = 0f;
			}
			else
			{
				m_targetSteer = Random.Range(-1f, 1f);
			}
			m_nextSteerTime = Time.time + steerInterval + Random.Range(0f - steerIntervalTolerance, steerIntervalTolerance);
		}
		if (Time.time > m_nextThrottleTime)
		{
			float num = throttleForwardRandom;
			float magnitude = m_vehicle.cachedRigidbody.velocity.magnitude;
			if (magnitude < 0.1f && m_targetBrake < 0.001f && m_targetThrottle >= 0f)
			{
				num *= 0.4f;
			}
			if (Random.value < num)
			{
				m_targetThrottle = Random.value;
				m_targetBrake = 0f;
			}
			else if (magnitude < 0.5f)
			{
				m_targetBrake = 0f;
				m_targetThrottle = 0f - Random.value;
			}
			else
			{
				m_targetThrottle = 0f;
				m_targetBrake = Random.value;
			}
			m_nextThrottleTime = Time.time + throttleInterval + Random.Range(0f - throttleIntervalTolerance, throttleIntervalTolerance);
		}
		m_vehicle.steerInput = Mathf.MoveTowards(m_vehicle.steerInput, m_targetSteer, steerChangeRate * Time.deltaTime);
		m_vehicle.throttleInput = Mathf.MoveTowards(m_vehicle.throttleInput, m_targetThrottle, throttleChangeRate * Time.deltaTime);
		m_vehicle.brakeInput = m_targetBrake;
		m_vehicle.handbrakeInput = 0f;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (base.enabled && collision.contacts.Length > 0)
		{
			float num = Vector3.Dot(base.transform.forward, collision.contacts[0].normal);
			if (num > 0.8f || num < -0.8f)
			{
				m_nextThrottleTime -= throttleInterval * 0.5f;
			}
			if (num > -0.4f && num < 0.4f)
			{
				m_nextSteerTime -= steerInterval * 0.5f;
			}
		}
	}
}
