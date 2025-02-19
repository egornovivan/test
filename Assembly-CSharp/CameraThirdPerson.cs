using UnityEngine;

public class CameraThirdPerson : CameraYawPitchRoll
{
	public Transform m_Character;

	public Transform m_Bone;

	public float m_BoneFactor = 0.4f;

	private Vector3 m_CharacterPos;

	public bool m_EnableZoom = true;

	public ECamKey m_ZoomAxis = ECamKey.CK_MouseWheel;

	public float m_ZoomSensitivity = 1f;

	public float m_MaxDistance = 50f;

	public float m_MinDistance = 1f;

	public bool m_AutoSetDistWhenEnterMode;

	public float m_Distance = 10f;

	public float m_DistanceWanted = 10f;

	public bool m_SyncYaw;

	public float m_YawDamper = 5f;

	protected float m_YawWanted;

	public bool m_SyncPitch;

	public float m_PitchDamper = 5f;

	protected float m_PitchWanted;

	public bool m_SyncRoll;

	public AnimationCurve m_RollCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float m_DistDamper = 5f;

	public float m_PosDamper = 25f;

	public float m_RollDamper = 3f;

	public bool m_AlwaysRenderCharacter;

	public Material m_InvisibleMaterial;

	public float m_OffsetZDis;

	public float m_OffsetYDis;

	public float m_OffsetXDis;

	private Vector3 m_CharacterOriginalPos;

	public bool m_EnableFov = true;

	public ECamKey m_FovAxis = ECamKey.CK_MouseWheel;

	public float m_FovSensitivity = 2f;

	public float m_MaxFov = 20f;

	public float m_MinFov;

	public float m_Fov;

	public float m_FovWanted;

	public float m_FovDamper = 5f;

	public bool m_AutoLerp;

	public void EnterShoot()
	{
	}

	public void QuitShoot()
	{
	}

	public override void ModeEnter()
	{
		base.ModeEnter();
		if (m_AutoSetDistWhenEnterMode && m_Character != null)
		{
			m_DistanceWanted = Vector3.Distance(m_TargetCam.transform.position, m_Character.position);
			m_Distance = m_DistanceWanted;
		}
		if (m_AutoLerp)
		{
			m_Distance = Mathf.Lerp(m_Distance, m_DistanceWanted, Mathf.Clamp01(Time.deltaTime * m_DistDamper));
		}
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
		if (m_SyncPitch && m_Character != null && !CamInput.GetKey(m_RotateKey))
		{
			m_PitchWanted = (0f - Mathf.Asin(Mathf.Clamp(m_Character.transform.forward.y, -1f, 1f))) * 57.29578f;
			m_PitchWanted = Mathf.Clamp(m_PitchWanted, m_MinPitch, m_MaxPitch);
			m_Pitch = Mathf.Lerp(m_Pitch, m_PitchWanted, Mathf.Clamp01(Time.deltaTime * m_PitchDamper));
		}
		base.UserInput();
		if (m_EnableZoom && !m_Controller.m_MouseOnScroll)
		{
			float num2 = Mathf.Pow(m_ZoomSensitivity + 1f, CamInput.GetAxis(m_ZoomAxis));
			m_DistanceWanted *= num2;
			m_DistanceWanted = Mathf.Clamp(m_DistanceWanted, Mathf.Max(0.001f, m_MinDistance), m_MaxDistance);
			m_Distance = Mathf.Lerp(m_Distance, m_DistanceWanted, Mathf.Clamp01(Time.deltaTime * m_DistDamper));
		}
	}

	public override void Do()
	{
		if (m_Character != null)
		{
			if (m_Bone != null)
			{
				m_CharacterPos = m_Character.position * (1f - m_BoneFactor) + m_Bone.position * m_BoneFactor;
			}
			else
			{
				m_CharacterPos = m_Character.position;
			}
			m_CharacterPos += m_TargetCam.transform.right * m_OffsetXDis + m_TargetCam.transform.up * m_OffsetYDis + m_TargetCam.transform.forward * m_OffsetZDis;
		}
		else
		{
			m_CharacterPos = m_TargetCam.transform.position + m_TargetCam.transform.forward * m_Distance;
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
				num5 = m_RollCurve.Evaluate(Mathf.Abs(num5) / 180f) * 180f * Mathf.Sign(num5);
				num6 = m_RollCurve.Evaluate(Mathf.Abs(num6) / 180f) * 180f * Mathf.Sign(num6);
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
		Vector3 b = m_CharacterPos - m_TargetCam.transform.forward * m_Distance;
		m_TargetCam.transform.position = Vector3.Lerp(m_TargetCam.transform.position, b, Mathf.Clamp01(Time.deltaTime * m_PosDamper));
		if (m_AlwaysRenderCharacter && m_Character != null && !(m_InvisibleMaterial != null))
		{
		}
	}
}
