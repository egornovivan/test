using System;
using UnityEngine;

public class TRVLS : Trajectory
{
	[SerializeField]
	private float m_StartSpeed = 5f;

	[SerializeField]
	private float m_BalanceSpeed = 50f;

	[SerializeField]
	private float m_AccelerationTime = 2f;

	[SerializeField]
	private float m_TrackStartTime = 1.5f;

	[SerializeField]
	private float m_TrackEndTime = 999f;

	[SerializeField]
	private float m_TrackPower = 0.5f;

	[SerializeField]
	private float m_OffsetRadius = 3f;

	[SerializeField]
	private float m_OffsetPeriod = 1f;

	[SerializeField]
	private float m_RandomF = 0.1f;

	protected Vector3 m_OffsetPhase = Vector3.zero;

	protected float m_PeriodW;

	protected float m_Time;

	protected float m_Acceleration;

	public override void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(caster, emitter, target, targetPosition, index);
		Init();
	}

	public void Init()
	{
		m_Velocity = base.transform.forward * m_StartSpeed;
		m_Acceleration = ((!(m_AccelerationTime > 0f)) ? 0f : ((m_BalanceSpeed - m_StartSpeed) / m_AccelerationTime));
		InitOffset();
	}

	public override Vector3 Track(float deltaTime)
	{
		UpdateVelocity(deltaTime);
		return m_Velocity * deltaTime;
	}

	private void InitOffset()
	{
		m_PeriodW = ((!(m_OffsetPeriod > 0f)) ? 0f : ((float)Math.PI * 2f / m_OffsetPeriod));
		m_OffsetPhase.x = (float)Math.PI * 2f * UnityEngine.Random.value;
		m_OffsetPhase.y = (float)Math.PI * 2f * UnityEngine.Random.value;
		m_OffsetPhase.z = (float)Math.PI * 2f * UnityEngine.Random.value;
	}

	protected virtual void UpdateTrack(float deltaTime)
	{
		if (null != m_Target && m_Time > m_TrackStartTime && m_Time < m_TrackEndTime)
		{
			m_OffsetPhase.x += m_PeriodW * UnityEngine.Random.value * m_RandomF * deltaTime;
			m_OffsetPhase.y += m_PeriodW * UnityEngine.Random.value * m_RandomF * deltaTime;
			m_OffsetPhase.z += m_PeriodW * UnityEngine.Random.value * m_RandomF * deltaTime;
			Vector3 vector = m_OffsetRadius * new Vector3(Mathf.Sin(m_OffsetPhase.x), Mathf.Sin(m_OffsetPhase.y), Mathf.Sin(m_OffsetPhase.z));
			Vector3 vector2 = GetTargetCenter(m_Target) + vector;
			Vector3 vector3 = vector2 - base.transform.position;
			Vector3 tangent = vector3;
			Vector3 normal = m_Velocity;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Vector3 vector4 = Vector3.Project(vector3, tangent);
			float num = ((!(m_OffsetRadius > 0f)) ? 1f : Mathf.Clamp01(vector4.magnitude / m_OffsetRadius));
			vector4 = vector4.normalized * m_TrackPower * num;
			float magnitude = m_Velocity.magnitude;
			m_Velocity += vector4 * deltaTime;
			m_Velocity = m_Velocity.normalized * magnitude;
		}
		m_Velocity = m_Velocity.normalized * Mathf.Lerp(m_StartSpeed, m_BalanceSpeed, Mathf.Clamp(m_Time, 0f, m_AccelerationTime) / m_AccelerationTime);
	}

	protected virtual void UpdateAcceleration(float deltaTime)
	{
		if (m_Time < m_AccelerationTime)
		{
			m_Velocity += m_Velocity.normalized * m_Acceleration * deltaTime;
		}
	}

	protected virtual void UpdateVelocity(float deltaTime)
	{
		m_Time += deltaTime;
		UpdateAcceleration(deltaTime);
		UpdateTrack(deltaTime);
		base.transform.forward = m_Velocity;
	}
}
