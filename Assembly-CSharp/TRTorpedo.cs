using PETools;
using UnityEngine;

public class TRTorpedo : TRVLS
{
	[SerializeField]
	private float m_Gravity = 0f - Physics.gravity.y;

	[SerializeField]
	private GameObject m_Effect;

	private bool m_InWater;

	private bool m_UpInWater;

	private float m_RotateSpeed = 5f;

	protected override void UpdateVelocity(float deltaTime)
	{
		CheckInWaterState();
		m_Time += deltaTime;
		UpdateAcceleration(deltaTime);
		UpdateTrack(deltaTime);
		if (m_InWater)
		{
			base.transform.forward = Vector3.Lerp(base.transform.forward, m_Velocity, m_RotateSpeed * deltaTime);
		}
	}

	private void CheckInWaterState()
	{
		m_InWater = PE.PointInWater(base.transform.position) > 0.5f;
		m_UpInWater = PE.PointInWater(base.transform.position + 0.5f * Vector3.up) > 0.5f;
		if (null != m_Effect && m_Effect.activeSelf != m_InWater)
		{
			m_Effect.SetActive(m_InWater);
		}
	}

	protected override void UpdateTrack(float deltaTime)
	{
		if (m_InWater)
		{
			base.UpdateTrack(deltaTime);
			if (!m_UpInWater && (m_Velocity.y > 0f || null == m_Target))
			{
				Vector3 velocity = m_Velocity;
				velocity.y = 0f;
				m_Velocity = Vector3.Slerp(m_Velocity, velocity, m_RotateSpeed * deltaTime).normalized * m_Velocity.magnitude;
			}
		}
	}

	protected override void UpdateAcceleration(float deltaTime)
	{
		if (!m_InWater)
		{
			m_Velocity += m_Gravity * Vector3.down * deltaTime;
		}
		base.UpdateAcceleration(deltaTime);
	}
}
