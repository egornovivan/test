using UnityEngine;

public class CameraYawPitchRoll : CamMode
{
	public bool m_EnableRotate = true;

	public ECamKey m_RotateKey = ECamKey.CK_Mouse1;

	public float m_RotateSensitivity = 1f;

	public float m_MaxPitch = 89f;

	public float m_MinPitch = -89f;

	public float m_Yaw;

	public float m_Pitch;

	public float m_Roll;

	public float m_RotDamper = 15f;

	public override void ModeEnter()
	{
		Vector3 eulerAngles = m_TargetCam.transform.eulerAngles;
		m_Yaw = eulerAngles.y;
		m_Pitch = eulerAngles.x;
		m_Roll = 0f;
		if (m_Pitch > 180f)
		{
			m_Pitch -= 360f;
		}
	}

	public override void UserInput()
	{
		bool flag = m_EnableRotate && (m_LockCursor || (CamInput.GetKey(m_RotateKey) && !m_Controller.m_MouseOpOnGUI));
		Cursor.lockState = (m_LockCursor ? CursorLockMode.Locked : (Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None));
		Cursor.visible = !m_LockCursor;
		if (flag)
		{
			float num = (CamInput.GetAxis(ECamKey.CK_MouseX) + CamInput.GetAxis(ECamKey.CK_JoyStickX)) * m_RotateSensitivity;
			float num2 = (CamInput.GetAxis(ECamKey.CK_MouseY) + CamInput.GetAxis(ECamKey.CK_JoyStickY)) * m_RotateSensitivity;
			m_Yaw += num;
			m_Pitch += num2;
		}
		m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
	}

	public override void Do()
	{
		Quaternion identity = Quaternion.identity;
		identity.eulerAngles = new Vector3(m_Pitch, m_Yaw, m_Roll);
		m_TargetCam.transform.rotation = Quaternion.Slerp(m_TargetCam.transform.rotation, identity, Mathf.Clamp01(Time.deltaTime * m_RotDamper));
	}

	protected virtual void OnGUI()
	{
		if (m_ShowTarget)
		{
			CamMediator.DrawAimTextureGUI(m_TargetViewportPos);
		}
	}
}
