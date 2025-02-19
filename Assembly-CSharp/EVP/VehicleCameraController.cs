using UnityEngine;

namespace EVP;

public class VehicleCameraController : MonoBehaviour
{
	public enum Mode
	{
		Fixed,
		SmoothFollow,
		MouseOrbit
	}

	public Mode mode = Mode.SmoothFollow;

	public Transform target;

	public Transform targetFixedPosition;

	public KeyCode changeCameraKey = KeyCode.C;

	public CameraOrbitSettings orbitSettings = new CameraOrbitSettings();

	public CameraSmoothFollowSettings smoothFollowSettings = new CameraSmoothFollowSettings();

	private Transform m_transform;

	private Vector3 m_smoothLastPos = Vector3.zero;

	private Vector3 m_smoothVelocity = Vector3.zero;

	private float m_smoothTargetAngle;

	private float m_orbitX;

	private float m_orbitY;

	private float m_orbitDistance;

	private void OnEnable()
	{
		m_transform = GetComponent<Transform>();
		InitializeMouseOrbit();
		ResetCamera();
	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown(changeCameraKey))
		{
			if (mode == Mode.MouseOrbit)
			{
				mode = Mode.Fixed;
			}
			else
			{
				mode++;
			}
		}
		switch (mode)
		{
		case Mode.Fixed:
			DoFixedCamera();
			break;
		case Mode.SmoothFollow:
			DoSmoothFollow();
			break;
		case Mode.MouseOrbit:
			DoMouseOrbit();
			break;
		}
	}

	public void ResetCamera()
	{
		ResetSmoothFollow();
	}

	private void DoFixedCamera()
	{
		Transform transform = ((!(targetFixedPosition != null)) ? target : targetFixedPosition);
		if (!(transform == null))
		{
			m_transform.position = transform.position;
			m_transform.rotation = transform.rotation;
		}
	}

	private void ResetSmoothFollow()
	{
		if (!(target == null))
		{
			m_smoothLastPos = target.position;
			m_smoothVelocity = target.forward * 2f;
			m_smoothTargetAngle = target.eulerAngles.y;
		}
	}

	private void DoSmoothFollow()
	{
		if (!(target == null))
		{
			Vector3 b = (target.position - m_smoothLastPos) / Time.deltaTime;
			m_smoothLastPos = target.position;
			b.y = 0f;
			if (b.magnitude > 1f)
			{
				m_smoothVelocity = Vector3.Lerp(m_smoothVelocity, b, smoothFollowSettings.velocityDamping * Time.deltaTime);
				m_smoothTargetAngle = Mathf.Atan2(m_smoothVelocity.x, m_smoothVelocity.z) * 57.29578f;
			}
			if (!smoothFollowSettings.followVelocity)
			{
				m_smoothTargetAngle = target.transform.eulerAngles.y;
			}
			float b2 = target.position.y + smoothFollowSettings.height;
			float y = base.transform.eulerAngles.y;
			float y2 = base.transform.position.y;
			y = Mathf.LerpAngle(y, m_smoothTargetAngle, smoothFollowSettings.rotationDamping * Time.deltaTime);
			y2 = Mathf.Lerp(y2, b2, smoothFollowSettings.heightDamping * Time.deltaTime);
			Quaternion quaternion = Quaternion.Euler(0f, y, 0f);
			m_transform.position = target.position;
			m_transform.position -= quaternion * Vector3.forward * smoothFollowSettings.distance;
			Vector3 position = base.transform.position;
			position.y = y2;
			base.transform.position = position;
			base.transform.LookAt(target.position + Vector3.up * smoothFollowSettings.height * smoothFollowSettings.viewHeightRatio);
		}
	}

	private void InitializeMouseOrbit()
	{
		m_orbitDistance = orbitSettings.distance;
		Vector3 eulerAngles = m_transform.eulerAngles;
		m_orbitX = eulerAngles.y;
		m_orbitY = eulerAngles.x;
	}

	private void DoMouseOrbit()
	{
		if (!(target == null))
		{
			m_orbitX += Input.GetAxis("Mouse X") * orbitSettings.horizontalSpeed;
			m_orbitY -= Input.GetAxis("Mouse Y") * orbitSettings.verticalSpeed;
			orbitSettings.distance -= Input.GetAxis("Mouse ScrollWheel") * orbitSettings.distanceSpeed;
			m_orbitY = Mathf.Clamp(m_orbitY, orbitSettings.minVerticalAngle, orbitSettings.maxVerticalAngle);
			orbitSettings.distance = Mathf.Clamp(orbitSettings.distance, orbitSettings.minDistance, orbitSettings.maxDistance);
			m_orbitDistance = Mathf.Lerp(m_orbitDistance, orbitSettings.distance, orbitSettings.distanceDamping * Time.deltaTime);
			m_transform.rotation = Quaternion.Slerp(m_transform.rotation, Quaternion.Euler(m_orbitY, m_orbitX, 0f), Time.deltaTime * orbitSettings.orbitDamping);
			m_transform.position = target.position + m_transform.rotation * new Vector3(0f, 0f, 0f - m_orbitDistance);
		}
	}
}
