using UnityEngine;

namespace WhiteCat;

public class VCPVtolThruster : VCPart
{
	[SerializeField]
	private float _forceFactor = 500000f;

	[SerializeField]
	private float _increaseSpeed = 0.3f;

	[SerializeField]
	private float _decreaseSpeed = 0.3f;

	[SerializeField]
	private GameObject _effect;

	[SerializeField]
	private Light _light;

	[SerializeField]
	private Transform _pivot;

	private HelicopterController _controller;

	private Direction _forceDirection;

	private Vector3 _localAngularDirection;

	private bool _rotateRight;

	private bool _rotateLeft;

	private float _maxForce;

	private float _maxForceRatio;

	private float _currentForceRatio;

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

	private float maxForceRatio
	{
		get
		{
			if (_forceDirection == Direction.Up)
			{
				float y = base.transform.up.y;
				if (y > 0.1f)
				{
					return Mathf.Clamp(_maxForceRatio / y, _maxForceRatio, 1f);
				}
			}
			return _maxForceRatio;
		}
	}

	public void Init(HelicopterController controller)
	{
		_controller = controller;
		_forceDirection = VCPart.GetDirection(base.transform.localRotation * Vector3.up);
		_maxForce = _forceFactor * (base.transform.localScale.x + base.transform.localScale.z) * 0.5f;
		Vector3 vector = _controller.transform.InverseTransformPoint(_pivot.position) - controller.rigidbody.centerOfMass;
		_localAngularDirection = Vector3.right;
		Vector3 rhs = vector;
		rhs.y = 0f;
		if (rhs.sqrMagnitude > 0.25f)
		{
			_localAngularDirection = Vector3.Cross(Vector3.up, rhs).normalized;
		}
		_localAngularDirection = Quaternion.Inverse(base.transform.localRotation) * _localAngularDirection;
		float num = Vector3.Dot(Vector3.up, _localAngularDirection);
		if (_forceDirection == Direction.Up)
		{
			float num2 = Mathf.Sign(vector.z) * Mathf.Log(Mathf.Abs(vector.z) + 1f);
			num2 = num2 * PEVCConfig.instance.rotorBalanceAdjust + vector.z * (1f - PEVCConfig.instance.rotorBalanceAdjust);
			num2 *= PEVCConfig.instance.rotorBalaceScale.Evaluate(Mathf.Abs(num2));
			Vector3 centerOfMass = controller.rigidbody.centerOfMass;
			centerOfMass.x += vector.x;
			centerOfMass.y += vector.y;
			centerOfMass.z += num2;
			_pivot.position = _controller.transform.TransformPoint(centerOfMass);
		}
		else if (_forceDirection == Direction.Forward || _forceDirection == Direction.Back)
		{
			_rotateRight = num > 0.5f;
			_rotateLeft = num < -0.5f;
			Utility.ClosestPoint(_pivot.position + new Vector3(0f, 100f, 0f), _pivot.position + new Vector3(0f, -100f, 0f), controller.rigidbody.worldCenterOfMass + base.transform.up * 100f, controller.rigidbody.worldCenterOfMass - base.transform.up * 100f, out var pointA, out var _);
			_pivot.position = pointA;
		}
		else
		{
			_rotateRight = num > 0f;
			_rotateLeft = num < 0f;
		}
		if (_forceDirection != Direction.Down)
		{
			base.enabled = true;
		}
	}

	public void InitMaxForceRatio(float upRatio)
	{
		if (_forceDirection == Direction.Up)
		{
			_maxForceRatio = upRatio;
		}
		else
		{
			_maxForceRatio = 1f;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(_pivot.position, 0.2f);
		Gizmos.DrawRay(base.transform.position, base.transform.TransformDirection(_localAngularDirection));
	}

	private void FixedUpdate()
	{
		int num = -1;
		float num2 = 0f;
		if (_controller.isEnergyEnough(0.01f) && _controller.hasDriver)
		{
			switch (_forceDirection)
			{
			case Direction.Up:
			{
				float num7 = 0f;
				if (_controller.inputVertical > 0.01f)
				{
					num7 = _controller.inputVertical * PEVCConfig.instance.helicopterMaxUpSpeed;
				}
				else if (_controller.inputVertical < -0.01f)
				{
					num7 = _controller.inputVertical * PEVCConfig.instance.helicopterMaxDownSpeed;
				}
				float y = _controller.rigidbody.velocity.y;
				num = ((y < num7 - 0.1f) ? 1 : ((y > num7 + 0.1f) ? (-1) : 0));
				num2 = 1f;
				break;
			}
			case Direction.Left:
			case Direction.Right:
				if (Mathf.Abs(_controller.inputX) > 0.01f)
				{
					if (_controller.inputX > 0f && _rotateRight)
					{
						float num5 = _controller.rigidbody.angularVelocity.y / 1.6f;
						num = ((!(num5 >= 1f)) ? 1 : 0);
						num2 = Mathf.Clamp01(1f - num5);
					}
					else if (_controller.inputX < 0f && _rotateLeft)
					{
						float num6 = (0f - _controller.rigidbody.angularVelocity.y) / 1.6f;
						num = ((!(num6 >= 1f)) ? 1 : 0);
						num2 = Mathf.Clamp01(1f - num6);
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
						if (_controller.inputX > 0f && _rotateRight)
						{
							float num3 = _controller.rigidbody.angularVelocity.y / 1.5f;
							num = ((!(num3 >= 1f)) ? 1 : 0);
							num2 = Mathf.Clamp01(1f - num3);
						}
						else if (_controller.inputX < 0f && _rotateLeft)
						{
							float num4 = (0f - _controller.rigidbody.angularVelocity.y) / 1.5f;
							num = ((!(num4 >= 1f)) ? 1 : 0);
							num2 = Mathf.Clamp01(1f - num4);
						}
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
		switch (num)
		{
		case 1:
			_currentForceRatio = Mathf.Min(maxForceRatio, _currentForceRatio + _increaseSpeed * Time.deltaTime);
			break;
		case -1:
			_currentForceRatio = Mathf.Max(0f, _currentForceRatio - _decreaseSpeed * Time.deltaTime);
			break;
		}
		if (num2 > 0f)
		{
			float num8 = _currentForceRatio * _maxForce * num2;
			if (_forceDirection == Direction.Up)
			{
				num8 *= _controller.liftForceFactor;
				_controller.rigidbody.AddTorque(_controller.inputX * num8 * PEVCConfig.instance.thrusterSteerHelp * base.transform.up);
			}
			_controller.rigidbody.AddForceAtPosition(num8 * base.transform.up * _controller.speedScale, _pivot.position);
			if (_controller.isPlayerHost)
			{
				_controller.ExpendEnergy(num8 * Time.deltaTime * PEVCConfig.instance.thrusterEnergySpeed);
			}
		}
		if (_effect.activeSelf)
		{
			if (num == -1 && _currentForceRatio < 0.05f)
			{
				_effect.SetActive(value: false);
			}
		}
		else if (num == 1 && _currentForceRatio > 0.05f)
		{
			_effect.SetActive(value: true);
		}
		_light.intensity = _currentForceRatio * 2f;
	}
}
