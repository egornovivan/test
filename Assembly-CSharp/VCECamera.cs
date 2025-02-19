using System;
using UnityEngine;

public class VCECamera : MonoBehaviour
{
	private static VCECamera s_Instance;

	public int ControlMode = 2;

	public Vector3 BeginTarget = new Vector3(0f, 0f, 0f);

	public float BeginYaw = 30f;

	public float BeginPitch = 45f;

	public float BeginRoll;

	public float BeginDistance = 2f;

	public Vector3 BeginForward = new Vector3(0.6f, -0.4f, 1f);

	public Vector3 BeginUp = new Vector3(0f, 1f, 0f);

	public float MoveSensitive = 0.5f;

	public float OrbitSensitive = 1f;

	public float ZoomSensitive = 2f;

	public float Damp = 0.15f;

	public bool CanMove = true;

	public bool CanOrbit = true;

	public bool CanZoom = true;

	public bool CanRoll;

	public float MaxDistance = 50f;

	public float MinDistance = 0.05f;

	private Vector3 _TargetWanted;

	private float _YawWanted;

	private float _PitchWanted;

	private float _RollWanted;

	private float _DistWanted;

	private Vector3 _ForwardWanted;

	private Vector3 _Target;

	[SerializeField]
	private float _Yaw;

	[SerializeField]
	private float _Pitch;

	[SerializeField]
	private float _Roll;

	[SerializeField]
	private float _Dist;

	private Vector3 _Forward;

	private Vector3 _Up;

	private int _MoveKey = 2;

	private int _OrbitKey = 1;

	private KeyCode _RollKey = KeyCode.Z;

	public static VCECamera Instance => s_Instance;

	public Vector3 Eye => base.transform.position;

	public Vector3 Target => _Target;

	public float Yaw => _Yaw;

	public float Pitch => _Pitch;

	public float Roll => _Roll;

	public float Distance => _Dist;

	public Ray PickRay => GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

	private void Awake()
	{
		s_Instance = this;
	}

	private void Start()
	{
		Reset();
	}

	public void Reset()
	{
		_Target = BeginTarget;
		_Yaw = BeginYaw;
		_Pitch = BeginPitch;
		_Roll = BeginRoll;
		_Dist = BeginDistance;
		_Forward = BeginForward;
		_Up = BeginUp;
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_RollWanted = BeginRoll;
		_DistWanted = BeginDistance;
		_ForwardWanted = BeginForward;
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
		while (_Roll < BeginRoll - 180f)
		{
			_Roll += 360f;
		}
		while (_Roll > BeginRoll + 180f)
		{
			_Roll -= 360f;
		}
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_RollWanted = BeginRoll;
		_DistWanted = BeginDistance;
		_ForwardWanted = BeginForward;
	}

	public void FixView()
	{
		CanOrbit = false;
		CanZoom = false;
		CanMove = false;
		CanRoll = false;
	}

	public void FreeView()
	{
		CanOrbit = true;
		CanZoom = true;
		CanMove = true;
		CanRoll = true;
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

	public void SetRoll(float roll)
	{
		_RollWanted = roll;
	}

	private void NormalizeVectors()
	{
		_ForwardWanted.Normalize();
		_Forward.Normalize();
		Vector3 rhs = Vector3.Cross(_Up, _Forward);
		_Up = Vector3.Cross(_Forward, rhs);
		_Up.Normalize();
	}

	private void CameraCtrl()
	{
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		float axis3 = Input.GetAxis("Mouse ScrollWheel");
		if (Input.GetMouseButton(_OrbitKey) && CanOrbit)
		{
			if (ControlMode == 1)
			{
				if (Input.GetKey(_RollKey))
				{
					if (CanRoll)
					{
						_RollWanted -= axis * OrbitSensitive * 5f;
					}
				}
				else
				{
					_YawWanted -= axis * OrbitSensitive * 5f;
					_PitchWanted -= axis2 * OrbitSensitive * 5f;
				}
			}
			else if (ControlMode == 2 && !Input.GetKey(_RollKey))
			{
				Vector3 vector = Vector3.Cross(_Up, _Forward);
				_ForwardWanted += axis * OrbitSensitive * vector * 0.1f;
				_ForwardWanted += axis2 * OrbitSensitive * _Up * 0.1f;
			}
		}
		else if (Input.GetMouseButton(_MoveKey) && CanMove)
		{
			Vector3 vector2 = base.transform.right * _Dist * MoveSensitive * axis + base.transform.up * _Dist * MoveSensitive * axis2;
			_TargetWanted -= vector2 * 0.1f;
		}
		if (axis3 != 0f && CanZoom)
		{
			_DistWanted *= Mathf.Pow(ZoomSensitive + 1f, 0f - axis3);
		}
		if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !UICamera.inputHasFocus)
		{
			if (Input.GetKey(KeyCode.W))
			{
				_TargetWanted += base.transform.forward * Time.deltaTime * _DistWanted;
			}
			if (Input.GetKey(KeyCode.S))
			{
				_TargetWanted -= base.transform.forward * Time.deltaTime * _DistWanted;
			}
			if (Input.GetKey(KeyCode.A))
			{
				_TargetWanted -= base.transform.right * Time.deltaTime * _DistWanted;
			}
			if (Input.GetKey(KeyCode.D))
			{
				_TargetWanted += base.transform.right * Time.deltaTime * _DistWanted;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				_TargetWanted += base.transform.up * Time.deltaTime * _DistWanted;
			}
			if (Input.GetKey(KeyCode.Z))
			{
				_TargetWanted -= base.transform.up * Time.deltaTime * _DistWanted;
			}
		}
		if (VCEditor.DocumentOpen() && !Input.GetMouseButton(_MoveKey))
		{
			Bounds bounds = new Bounds(VCEditor.s_Scene.m_Setting.EditorWorldSize * 0.5f, VCEditor.s_Scene.m_Setting.EditorWorldSize * 3f);
			if (_TargetWanted.x < bounds.min.x)
			{
				_TargetWanted.x = bounds.min.x;
			}
			if (_TargetWanted.y < bounds.min.y)
			{
				_TargetWanted.y = bounds.min.y;
			}
			if (_TargetWanted.z < bounds.min.z)
			{
				_TargetWanted.z = bounds.min.z;
			}
			if (_TargetWanted.x > bounds.max.x)
			{
				_TargetWanted.x = bounds.max.x;
			}
			if (_TargetWanted.y > bounds.max.y)
			{
				_TargetWanted.y = bounds.max.y;
			}
			if (_TargetWanted.z > bounds.max.z)
			{
				_TargetWanted.z = bounds.max.z;
			}
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
		if ((double)Mathf.Abs(_Roll - _RollWanted) < 0.1)
		{
			_Roll = _RollWanted;
		}
		else
		{
			_Roll += (_RollWanted - _Roll) * Damp;
		}
		if ((double)Mathf.Abs(_Dist - _DistWanted) < 0.0002)
		{
			_Dist = _DistWanted;
		}
		else
		{
			_Dist += (_DistWanted - _Dist) * Damp;
		}
		if ((double)(_Forward - _ForwardWanted).magnitude < 0.0002)
		{
			_Forward = _ForwardWanted;
		}
		else
		{
			_Forward += (_ForwardWanted - _Forward) * Damp;
		}
	}

	private void CalcTransform()
	{
		if (ControlMode == 1)
		{
			Vector3 position = new Vector3(0f, 0f, 0f);
			position.x = _Dist * Mathf.Cos(_Yaw / 180f * (float)Math.PI) * Mathf.Cos(_Pitch / 180f * (float)Math.PI);
			position.z = _Dist * Mathf.Sin(_Yaw / 180f * (float)Math.PI) * Mathf.Cos(_Pitch / 180f * (float)Math.PI);
			position.y = _Dist * Mathf.Sin(_Pitch / 180f * (float)Math.PI);
			position += _Target;
			base.transform.position = position;
			base.transform.LookAt(_Target);
			base.transform.Rotate(base.transform.forward, _Roll / 180f * (float)Math.PI);
			if (Mathf.Cos(_Pitch / 180f * (float)Math.PI) < 0f)
			{
				base.transform.Rotate(base.transform.forward, (float)Math.PI);
			}
		}
		else if (ControlMode == 2)
		{
			Vector3 target = _Target;
			target -= _Forward * _Dist;
			base.transform.position = target;
			base.transform.LookAt(_Target, _Up);
		}
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		NormalizeVectors();
		CameraCtrl();
		ClampParam();
		CloseToWanted();
		NormalizeVectors();
		CalcTransform();
	}

	private void OnDestroy()
	{
	}
}
