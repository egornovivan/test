using UnityEngine;

public class CameraAutoThirdPerson : CameraThirdPerson
{
	public bool m_AutoFollow;

	public float m_AutoFollowDelay = 1f;

	private Vector3 _lastCharacterPos = Vector3.zero;

	private float _moving_time;

	public override void UserInput()
	{
		base.UserInput();
		if (!(m_Character != null))
		{
			return;
		}
		Vector3 forward = m_Character.transform.forward;
		if (m_AutoFollow && !CamInput.GetKey(m_RotateKey))
		{
			if (_lastCharacterPos.sqrMagnitude > 0.01f)
			{
				float magnitude = (m_Character.position - _lastCharacterPos).magnitude;
				if (magnitude > 0.03f)
				{
					if (_moving_time > m_AutoFollowDelay)
					{
						m_YawWanted = Mathf.Atan2(forward.x, forward.z) * 57.29578f;
						int num = Mathf.RoundToInt((m_Yaw - m_YawWanted) / 360f);
						m_Yaw -= (float)num * 360f;
						m_Yaw = Mathf.Lerp(m_Yaw, m_YawWanted, Mathf.Clamp01(0.03f * m_YawDamper));
					}
					_moving_time += Time.deltaTime;
				}
				else
				{
					if (_moving_time > m_AutoFollowDelay)
					{
						_moving_time = m_AutoFollowDelay;
					}
					_moving_time -= Time.deltaTime;
					if (_moving_time < 0f)
					{
						_moving_time = 0f;
					}
				}
			}
			else
			{
				_moving_time = 0f;
			}
		}
		_lastCharacterPos = m_Character.position;
	}
}
