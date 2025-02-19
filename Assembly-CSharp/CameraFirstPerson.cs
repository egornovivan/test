using UnityEngine;

public class CameraFirstPerson : CameraYawPitchRoll
{
	public Transform m_Character;

	private Vector3 m_CharacterPos;

	public bool m_EnableFov = true;

	public ECamKey m_FovAxis = ECamKey.CK_MouseWheel;

	public float m_FovSensitivity = 2f;

	public float m_MaxFov = 20f;

	public float m_MinFov;

	public float m_Fov;

	public float m_FovWanted;

	public bool m_SyncYaw;

	public float m_YawDamper = 5f;

	private float m_YawWanted;

	public bool m_SyncRoll;

	public float m_RollCoef = 0.7f;

	public float m_FovDamper = 5f;

	public float m_PosDamper = 25f;

	public float m_RollDamper = 3f;

	public override void ModeEnter()
	{
		base.ModeEnter();
		m_Fov = 0f;
	}

	public override void UserInput()
	{
		if (m_SyncYaw && m_Character != null && !CamInput.GetKey(m_RotateKey))
		{
			m_YawWanted = Mathf.Atan2(m_Character.transform.forward.x, m_Character.transform.forward.z) * 57.29578f;
			int num = Mathf.RoundToInt((m_Yaw - m_YawWanted) / 360f);
			m_Yaw -= (float)num * 360f;
			m_Yaw = Mathf.Lerp(m_Yaw, m_YawWanted, Mathf.Clamp01(Time.deltaTime * m_YawDamper));
		}
		base.UserInput();
		if (m_EnableFov && !m_Controller.m_MouseOnScroll)
		{
			float num2 = m_FovSensitivity * CamInput.GetAxis(m_FovAxis) * 10f;
			m_FovWanted += num2;
			m_FovWanted = Mathf.Clamp(m_FovWanted, Mathf.Max(-30f, m_MinFov), Mathf.Min(90f, m_MaxFov));
			m_Fov = Mathf.Lerp(m_Fov, m_FovWanted, Mathf.Clamp01(Time.deltaTime * m_FovDamper));
		}
	}

	public override void Do()
	{
		if (m_Character != null)
		{
			m_CharacterPos = m_Character.position;
		}
		else
		{
			m_CharacterPos = m_TargetCam.transform.position;
		}
		if (m_SyncRoll && m_Character != null)
		{
			float num = 0f;
			Vector3 normalized = Vector3.Cross(Vector3.up, m_Character.forward).normalized;
			if (normalized.magnitude > 0.01f)
			{
				float num2 = Vector3.Angle(normalized, m_Character.up);
				float num3 = 180f - num2;
				float num4 = Vector3.Angle(Vector3.up, m_Character.up);
				float num5 = 0f;
				float num6 = 0f;
				if (num4 < 90f)
				{
					num5 = num2 - 90f;
					num6 = num3 - 90f;
				}
				else
				{
					num5 = 270f - num2;
					num6 = 270f - num3;
				}
				num5 *= m_RollCoef;
				num6 *= m_RollCoef;
				num = Mathf.Lerp(num5, num6, (1f - Vector3.Dot(m_TargetCam.transform.forward, m_Character.forward)) * 0.5f);
			}
			while (m_Roll - num > 180f)
			{
				m_Roll -= 360f;
			}
			while (m_Roll - num <= -180f)
			{
				m_Roll += 360f;
			}
			m_Roll = Mathf.Lerp(m_Roll, num, Mathf.Clamp01(Time.deltaTime * m_RollDamper));
		}
		base.Do();
		m_TargetCam.transform.position = Vector3.Lerp(m_TargetCam.transform.position, m_CharacterPos, Mathf.Clamp01(Time.deltaTime * m_PosDamper));
		m_TargetCam.fieldOfView += m_Fov;
	}
}
