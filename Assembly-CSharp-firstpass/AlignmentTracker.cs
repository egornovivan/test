using UnityEngine;

public class AlignmentTracker : MonoBehaviour
{
	public bool fixedUpdate;

	private float m_CurrentFixedTime;

	private float m_CurrentLateTime;

	private Vector3 m_Position = Vector3.zero;

	private Vector3 m_PositionPrev = Vector3.zero;

	private Vector3 m_Velocity = Vector3.zero;

	private Vector3 m_VelocityPrev = Vector3.zero;

	private Vector3 m_VelocitySmoothed = Vector3.zero;

	private Vector3 m_Acceleration = Vector3.zero;

	private Vector3 m_AccelerationSmoothed = Vector3.zero;

	private Quaternion m_Rotation = Quaternion.identity;

	private Quaternion m_RotationPrev = Quaternion.identity;

	private Vector3 m_AngularVelocity = Vector3.zero;

	private Vector3 m_AngularVelocitySmoothed = Vector3.zero;

	private Rigidbody m_Rigidbody;

	private Transform m_Transform;

	public Vector3 position => m_Position;

	public Vector3 velocity => m_Velocity;

	public Vector3 velocitySmoothed => m_VelocitySmoothed;

	public Vector3 acceleration => m_Acceleration;

	public Vector3 accelerationSmoothed => m_AccelerationSmoothed;

	public Quaternion rotation => m_Rotation;

	public Vector3 angularVelocity => m_AngularVelocity;

	public Vector3 angularVelocitySmoothed => m_AngularVelocitySmoothed;

	private void Awake()
	{
		Reset();
	}

	private void OnEnable()
	{
		Reset();
	}

	public void Reset()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Transform = base.transform;
		m_CurrentLateTime = -1f;
		m_CurrentFixedTime = -1f;
		m_Position = (m_PositionPrev = m_Transform.position);
		m_Rotation = (m_RotationPrev = m_Transform.rotation);
		m_Velocity = Vector3.zero;
		m_VelocityPrev = Vector3.zero;
		m_VelocitySmoothed = Vector3.zero;
		m_Acceleration = Vector3.zero;
		m_AccelerationSmoothed = Vector3.zero;
		m_AngularVelocity = Vector3.zero;
		m_AngularVelocitySmoothed = Vector3.zero;
	}

	private Vector3 CalculateAngularVelocity(Quaternion prev, Quaternion current)
	{
		Quaternion quaternion = Quaternion.Inverse(prev) * current;
		float angle = 0f;
		Vector3 axis = Vector3.zero;
		quaternion.ToAngleAxis(out angle, out axis);
		if (axis == Vector3.zero || axis.x == float.PositiveInfinity || axis.x == float.NegativeInfinity)
		{
			return Vector3.zero;
		}
		if (angle > 180f)
		{
			angle -= 360f;
		}
		angle /= Time.deltaTime;
		return axis.normalized * angle;
	}

	private void UpdateTracking()
	{
		m_Position = m_Transform.position;
		m_Rotation = m_Transform.rotation;
		if (m_Rigidbody != null)
		{
			m_Velocity = (m_Position - m_PositionPrev) / Time.deltaTime;
			m_AngularVelocity = CalculateAngularVelocity(m_RotationPrev, m_Rotation);
		}
		else
		{
			m_Velocity = (m_Position - m_PositionPrev) / Time.deltaTime;
			m_AngularVelocity = CalculateAngularVelocity(m_RotationPrev, m_Rotation);
		}
		m_Acceleration = (m_Velocity - m_VelocityPrev) / Time.deltaTime;
		m_PositionPrev = m_Position;
		m_RotationPrev = m_Rotation;
		m_VelocityPrev = m_Velocity;
	}

	public void ControlledFixedUpdate()
	{
		if (Time.deltaTime != 0f && Time.timeScale != 0f && m_CurrentFixedTime != Time.time)
		{
			m_CurrentFixedTime = Time.time;
			if (fixedUpdate)
			{
				UpdateTracking();
			}
		}
	}

	public void ControlledLateUpdate()
	{
		if (Time.deltaTime != 0f && Time.timeScale != 0f && m_CurrentLateTime != Time.time)
		{
			m_CurrentLateTime = Time.time;
			if (!fixedUpdate)
			{
				UpdateTracking();
			}
			m_VelocitySmoothed = Vector3.Lerp(m_VelocitySmoothed, m_Velocity, Time.deltaTime * 10f);
			m_AccelerationSmoothed = Vector3.Lerp(m_AccelerationSmoothed, m_Acceleration, Time.deltaTime * 3f);
			m_AngularVelocitySmoothed = Vector3.Lerp(m_AngularVelocitySmoothed, m_AngularVelocity, Time.deltaTime * 3f);
			if (fixedUpdate)
			{
				m_Position += m_Velocity * Time.deltaTime;
			}
		}
	}
}
