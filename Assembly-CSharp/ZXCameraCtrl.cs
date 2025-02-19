using System;
using UnityEngine;

public class ZXCameraCtrl : MonoBehaviour
{
	private static ZXCameraCtrl m_Instance;

	public Vector3 BeginTarget = new Vector3(0f, 0f, 0f);

	public float BeginYaw = 30f;

	public float BeginPitch = 45f;

	public float BeginDistance = 2f;

	public Transform Following;

	public float MoveSensitive = 0.5f;

	public float OrbitSensitive = 1f;

	public float ZoomSensitive = 2f;

	public float Damp = 0.15f;

	public bool CanMove = true;

	public bool CanOrbit = true;

	public bool AutoOrbit;

	public bool CanZoom = true;

	public float MaxDistance = 5f;

	public float MinDistance = 0.05f;

	private Vector3 _TargetWanted;

	private float _YawWanted;

	private float _PitchWanted;

	private float _DistWanted;

	private Vector3 _Target;

	private float _Yaw;

	private float _Pitch;

	private float _Dist;

	private int _MoveKey = 2;

	private int _OrbitKey = 1;

	public static ZXCameraCtrl Instance => m_Instance;

	public Vector3 Eye => base.transform.position;

	public Vector3 Target => _Target;

	public float Yaw => _Yaw;

	public float Pitch => _Pitch;

	public float Distance => _Dist;

	private void Awake()
	{
		m_Instance = this;
		Camera.main.depthTextureMode |= DepthTextureMode.Depth;
		Camera.main.depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	private void Start()
	{
		_Target = BeginTarget;
		_Yaw = BeginYaw;
		_Pitch = BeginPitch;
		_Dist = BeginDistance;
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_DistWanted = BeginDistance;
	}

	public void Reset()
	{
		_Target = BeginTarget;
		_Yaw = BeginYaw;
		_Pitch = BeginPitch;
		_Dist = BeginDistance;
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_DistWanted = BeginDistance;
	}

	public void SmoothReset()
	{
		while (_Yaw < BeginYaw - 180f)
		{
			_Yaw += 360f;
		}
		while (_Yaw > BeginYaw + 180f)
		{
			_Yaw -= 360f;
		}
		while (_Pitch < BeginPitch - 180f)
		{
			_Pitch += 360f;
		}
		while (_Pitch > BeginPitch + 180f)
		{
			_Pitch -= 360f;
		}
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_DistWanted = BeginDistance;
	}

	private void LateUpdate()
	{
		CameraCtrl();
		if (Following != null)
		{
			_TargetWanted = Following.position;
			_Target = Following.position;
		}
		ClampParam();
		CloseToWanted();
		base.transform.position = CalcPostion();
		base.transform.LookAt(_Target);
		if (Mathf.Cos(_Pitch / 180f * (float)Math.PI) < 0f)
		{
			base.transform.RotateAround(base.transform.forward, (float)Math.PI);
		}
	}

	public void SetTarget(Vector3 newTarget)
	{
		_TargetWanted = newTarget;
	}

	public void SetDistance(float dist)
	{
		_DistWanted = dist;
	}

	public void SetYaw(float yaw)
	{
		_YawWanted = yaw;
	}

	public void SetPitch(float pitch)
	{
		_PitchWanted = pitch;
	}

	private void CameraCtrl()
	{
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		float axis3 = Input.GetAxis("Mouse ScrollWheel");
		if ((Input.GetMouseButton(_OrbitKey) && CanOrbit) || AutoOrbit)
		{
			_YawWanted -= axis * OrbitSensitive * 5f;
			_PitchWanted -= axis2 * OrbitSensitive * 5f;
		}
		else if (Input.GetMouseButton(_MoveKey) && CanMove)
		{
			Vector3 vector = base.transform.right * _Dist * MoveSensitive * axis + base.transform.up * _Dist * MoveSensitive * axis2;
			_TargetWanted -= vector * 0.1f;
		}
		if (axis3 != 0f && CanZoom)
		{
			_DistWanted *= Mathf.Pow(ZoomSensitive + 1f, 0f - axis3);
		}
	}

	private void ClampParam()
	{
		if (MaxDistance < MinDistance)
		{
			MaxDistance = MinDistance;
		}
		if (_DistWanted > MaxDistance)
		{
			_DistWanted = MaxDistance;
		}
		else if (_DistWanted < MinDistance)
		{
			_DistWanted = MinDistance;
		}
	}

	private void CloseToWanted()
	{
		if ((double)(_Target - _TargetWanted).magnitude < 0.0002)
		{
			_Target = _TargetWanted;
		}
		else
		{
			_Target += (_TargetWanted - _Target) * Damp;
		}
		if ((double)Mathf.Abs(_Yaw - _YawWanted) < 0.1)
		{
			_Yaw = _YawWanted;
		}
		else
		{
			_Yaw += (_YawWanted - _Yaw) * Damp;
		}
		if ((double)Mathf.Abs(_Pitch - _PitchWanted) < 0.1)
		{
			_Pitch = _PitchWanted;
		}
		else
		{
			_Pitch += (_PitchWanted - _Pitch) * Damp;
		}
		if ((double)Mathf.Abs(_Dist - _DistWanted) < 0.0002)
		{
			_Dist = _DistWanted;
		}
		else
		{
			_Dist += (_DistWanted - _Dist) * Damp;
		}
	}

	private Vector3 CalcPostion()
	{
		Vector3 vector = new Vector3(0f, 0f, 0f);
		vector.x = _Dist * Mathf.Cos(_Yaw / 180f * (float)Math.PI) * Mathf.Cos(_Pitch / 180f * (float)Math.PI);
		vector.z = _Dist * Mathf.Sin(_Yaw / 180f * (float)Math.PI) * Mathf.Cos(_Pitch / 180f * (float)Math.PI);
		vector.y = _Dist * Mathf.Sin(_Pitch / 180f * (float)Math.PI);
		return vector + _Target;
	}

	public Ray PickRay()
	{
		return GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
	}
}
