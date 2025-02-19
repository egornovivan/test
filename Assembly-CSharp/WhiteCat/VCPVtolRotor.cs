using System;
using UnityEngine;

namespace WhiteCat;

public class VCPVtolRotor : VCPart
{
	[SerializeField]
	private float _forceFactor = 1f;

	[SerializeField]
	private float _originalRadius = 0.38f;

	[SerializeField]
	private bool _isBig;

	[SerializeField]
	private float _maxDeflectionAngle = 15f;

	[SerializeField]
	private Transform _rotatePivot;

	[SerializeField]
	private Transform _deflectionPivot;

	[SerializeField]
	private Transform _forcePivot;

	[SerializeField]
	[Header("Effects")]
	private AudioSource _sound;

	[SerializeField]
	private float _maxPitch = 1f;

	[SerializeField]
	private float _basePitch = 0.5f;

	[SerializeField]
	private float _maxVolume = 1f;

	[SerializeField]
	private float _volumeSpeed = 2f;

	private HelicopterController _controller;

	private Direction _forceDirection;

	private Vector3 _localAngularDirection;

	private Vector3 _localForwardDirection;

	private Vector3 _localTargetUp;

	private bool _rotateRight;

	private bool _rotateLeft;

	private float _maxForce;

	private float _accelerate;

	private float _decelerate;

	private float _maxRotateSpeed;

	private float _currentRotateSpeed;

	private float _currentYAngle;

	public float maxLiftForce
	{
		get
		{
			if (_forceDirection == Direction.Up)
			{
				return _maxForce * (base.transform.localRotation * Vector3.up).y;
			}
			return 0f;
		}
	}

	private float maxRotateSpeed
	{
		get
		{
			if (_forceDirection == Direction.Up)
			{
				float y = base.transform.up.y;
				if (y > 0.1f)
				{
					return Mathf.Clamp(_maxRotateSpeed / y, _maxRotateSpeed, PEVCConfig.instance.rotorMaxRotateSpeed);
				}
			}
			return _maxRotateSpeed;
		}
	}

	public int sizeType => (!_isBig) ? 1 : 0;

	public int directionType => (int)_forceDirection;

	public void Init(HelicopterController controller)
	{
		_controller = controller;
		_forceDirection = VCPart.GetDirection(base.transform.localRotation * Vector3.up);
		float num = (base.transform.localScale.x + base.transform.localScale.z) * 0.5f * _originalRadius;
		_maxForce = _forceFactor * num;
		_accelerate = Mathf.Pow(1.15f, 0f - num) * PEVCConfig.instance.rotorAccelerateFactor;
		_decelerate = Mathf.Pow(1.15f, 0f - num) * PEVCConfig.instance.rotorDecelerateFactor;
		Vector3 vector = _controller.transform.InverseTransformPoint(_forcePivot.position) - controller.rigidbody.centerOfMass;
		_localAngularDirection = Vector3.right;
		Vector3 rhs = vector;
		rhs.y = 0f;
		if (rhs.sqrMagnitude > 0.25f)
		{
			_localAngularDirection = Vector3.Cross(Vector3.up, rhs).normalized;
		}
		_localAngularDirection = Quaternion.Inverse(base.transform.localRotation) * _localAngularDirection;
		_localForwardDirection = Quaternion.Inverse(base.transform.localRotation) * Vector3.forward;
		float num2 = Vector3.Dot(Vector3.up, _localAngularDirection);
		if (_forceDirection == Direction.Up)
		{
			float num3 = Mathf.Sign(vector.z) * Mathf.Log(Mathf.Abs(vector.z) + 1f);
			num3 = num3 * PEVCConfig.instance.rotorBalanceAdjust + vector.z * (1f - PEVCConfig.instance.rotorBalanceAdjust);
			num3 *= PEVCConfig.instance.rotorBalaceScale.Evaluate(Mathf.Abs(num3));
			Vector3 centerOfMass = controller.rigidbody.centerOfMass;
			centerOfMass.x += vector.x;
			centerOfMass.y += vector.y;
			centerOfMass.z += num3;
			_forcePivot.position = _controller.transform.TransformPoint(centerOfMass);
		}
		else if (_forceDirection == Direction.Forward || _forceDirection == Direction.Back)
		{
			_rotateRight = num2 > 0.5f;
			_rotateLeft = num2 < -0.5f;
			Utility.ClosestPoint(_forcePivot.position + new Vector3(0f, 100f, 0f), _forcePivot.position + new Vector3(0f, -100f, 0f), controller.rigidbody.worldCenterOfMass + base.transform.up * 100f, controller.rigidbody.worldCenterOfMass - base.transform.up * 100f, out var pointA, out var _);
			_forcePivot.position = pointA;
		}
		else
		{
			_rotateRight = num2 > 0f;
			_rotateLeft = num2 < 0f;
		}
		if (_forceDirection != Direction.Down)
		{
			base.enabled = true;
		}
	}

	public void InitSoundScale(int count)
	{
		_maxVolume *= Mathf.Pow(0.75f, count - 1);
		_volumeSpeed *= Mathf.Pow(0.75f, count - 1);
	}

	public void InitMaxRotateSpeed(float upValue)
	{
		if (_forceDirection == Direction.Up)
		{
			_maxRotateSpeed = upValue;
		}
		else
		{
			_maxRotateSpeed = PEVCConfig.instance.rotorMaxRotateSpeed;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(_forcePivot.position, 0.2f);
		Gizmos.DrawRay(base.transform.position, base.transform.TransformDirection(_localAngularDirection));
	}

	private void FixedUpdate()
	{
		_localTargetUp = Vector3.up;
		int num = -1;
		float num2 = 0f;
		if (_controller.isEnergyEnough(0.01f) && _controller.hasDriver)
		{
			switch (_forceDirection)
			{
			case Direction.Up:
			{
				float num3 = 0f;
				if (_controller.inputVertical > 0.01f)
				{
					num3 = _controller.inputVertical * PEVCConfig.instance.helicopterMaxUpSpeed;
				}
				else if (_controller.inputVertical < -0.01f)
				{
					num3 = _controller.inputVertical * PEVCConfig.instance.helicopterMaxDownSpeed;
				}
				float y = _controller.rigidbody.velocity.y;
				num = ((y < num3 - 0.1f) ? 1 : ((y > num3 + 0.1f) ? (-1) : 0));
				Vector3 vector = _controller.inputX * _localAngularDirection + _controller.inputY * _localForwardDirection;
				if (vector != Vector3.zero)
				{
					_localTargetUp = Vector3.RotateTowards(Vector3.up, vector, _maxDeflectionAngle * ((float)Math.PI / 180f), 0f);
				}
				num2 = 1f;
				break;
			}
			case Direction.Left:
			case Direction.Right:
				if (Mathf.Abs(_controller.inputX) > 0.01f)
				{
					if ((!(_controller.inputX > 0f)) ? _rotateLeft : _rotateRight)
					{
						_localTargetUp = Vector3.RotateTowards(Vector3.up, _controller.inputX * _localAngularDirection, _maxDeflectionAngle * ((float)Math.PI / 180f), 0f);
						num = 1;
						num2 = 1f;
					}
				}
				else
				{
					if (!(Mathf.Abs(_controller.rigidbody.angularVelocity.y) > 0.26f))
					{
						break;
					}
					if (_controller.rigidbody.angularVelocity.y > 0f)
					{
						if (_rotateLeft)
						{
							num = 1;
							num2 = 1f;
						}
					}
					else if (_rotateRight)
					{
						num = 1;
						num2 = 1f;
					}
				}
				break;
			case Direction.Forward:
			case Direction.Back:
				if (Mathf.Abs(_controller.inputX) > 0.01f)
				{
					if ((!(_controller.inputX > 0f)) ? _rotateLeft : _rotateRight)
					{
						_localTargetUp = Vector3.RotateTowards(Vector3.up, _controller.inputX * _localAngularDirection, _maxDeflectionAngle * ((float)Math.PI / 180f), 0f);
						num = 1;
						num2 = 1f;
						break;
					}
					if ((!(_controller.inputX > 0f)) ? _rotateRight : _rotateLeft)
					{
						break;
					}
				}
				else if (Mathf.Abs(_controller.rigidbody.angularVelocity.y) > 0.26f)
				{
					if (_controller.rigidbody.angularVelocity.y > 0f)
					{
						if (_rotateLeft)
						{
							num = 1;
							num2 = 1f;
							break;
						}
						if (_rotateRight)
						{
							break;
						}
					}
					else
					{
						if (_rotateRight)
						{
							num = 1;
							num2 = 1f;
							break;
						}
						if (_rotateLeft)
						{
							break;
						}
					}
				}
				if (_forceDirection == Direction.Forward)
				{
					num = ((_controller.inputY > 0.01f) ? 1 : (-1));
					if (num == 1)
					{
						num2 = _controller.inputY;
					}
				}
				else
				{
					num = ((_controller.inputY < -0.01f) ? 1 : (-1));
					if (num == 1)
					{
						num2 = 0f - _controller.inputY;
					}
				}
				break;
			}
		}
		Quaternion to = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, _localTargetUp), _localTargetUp);
		_deflectionPivot.localRotation = Quaternion.RotateTowards(_deflectionPivot.localRotation, to, PEVCConfig.instance.rotorDeflectSpeed * Time.deltaTime);
		switch (num)
		{
		case 1:
			_currentRotateSpeed = Mathf.Min(maxRotateSpeed, _currentRotateSpeed + _accelerate * Time.deltaTime);
			break;
		case -1:
			_currentRotateSpeed = Mathf.Max(0f, _currentRotateSpeed - _decelerate * Time.deltaTime);
			break;
		}
		_currentYAngle = (_currentYAngle + _currentRotateSpeed * Time.deltaTime) % 360f;
		_rotatePivot.localEulerAngles = new Vector3(0f, _currentYAngle, 0f);
		if (num2 > 0f)
		{
			float num4 = _currentRotateSpeed / PEVCConfig.instance.rotorMaxRotateSpeed * _maxForce * num2;
			if (_forceDirection == Direction.Up)
			{
				num4 *= _controller.liftForceFactor;
				_controller.rigidbody.AddTorque(_controller.inputX * num4 * PEVCConfig.instance.rotorSteerHelp * base.transform.up);
				_controller.rigidbody.AddForceAtPosition(base.transform.up * num4 * _controller.speedScale, _forcePivot.position);
				_controller.rigidbody.AddForceAtPosition(Vector3.ProjectOnPlane(_rotatePivot.up * num4 * _controller.speedScale, base.transform.up), (_forcePivot.position + _controller.rigidbody.worldCenterOfMass) * 0.5f);
			}
			else
			{
				_controller.rigidbody.AddForceAtPosition(_rotatePivot.up * num4 * _controller.speedScale, _forcePivot.position);
			}
			if (_controller.isPlayerHost)
			{
				_controller.ExpendEnergy(num4 * Time.deltaTime * PEVCConfig.instance.rotorEnergySpeed);
			}
		}
		UpdateSound();
	}

	private void UpdateSound()
	{
		float num = Mathf.Clamp01(Mathf.Abs(_currentRotateSpeed / PEVCConfig.instance.rotorMaxRotateSpeed));
		if (num > 0.01f)
		{
			_sound.pitch = num * (_maxPitch - _basePitch) + _basePitch;
			_sound.volume = Mathf.Clamp(num * _volumeSpeed, 0f, _maxVolume) * SystemSettingData.Instance.AbsEffectVolume;
			if (!_sound.isPlaying)
			{
				_sound.Play();
			}
		}
		else if (_sound.isPlaying)
		{
			_sound.Stop();
		}
	}
}
