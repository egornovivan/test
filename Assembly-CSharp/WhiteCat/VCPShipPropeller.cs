using UnityEngine;

namespace WhiteCat;

public class VCPShipPropeller : VCPart
{
	[SerializeField]
	private float _forceFactor = 200000f;

	[SerializeField]
	private float _maxRotateSpeed = 1801f;

	[SerializeField]
	private float _accelerate = 900f;

	[SerializeField]
	private Transform _rotatePivot;

	[SerializeField]
	private Transform _forcePivot;

	private BoatController _controller;

	private Direction _forceDirection;

	private Vector3 _forceApplyPoint;

	private Vector3 _localAngularDirection;

	private float _maxForce;

	private bool _rotateRight;

	private bool _rotateLeft;

	private float _currentRotateSpeed;

	private float _currentZAngle;

	public void Init(BoatController controller, bool isSubmarine)
	{
		_controller = controller;
		_forceDirection = VCPart.GetDirection(base.transform.localRotation * Vector3.forward);
		_maxForce = (base.transform.localScale.x + base.transform.localScale.y) * 0.5f * _forceFactor;
		Vector3 vector = _controller.transform.InverseTransformPoint(_forcePivot.position) - controller.rigidbody.centerOfMass;
		_localAngularDirection = Vector3.right;
		Vector3 rhs = vector;
		rhs.y = 0f;
		if (rhs.sqrMagnitude > 0.25f)
		{
			_localAngularDirection = Vector3.Cross(Vector3.up, rhs).normalized;
		}
		_localAngularDirection = Quaternion.Inverse(base.transform.localRotation) * _localAngularDirection;
		float num = Vector3.Dot(Vector3.forward, _localAngularDirection);
		if (_forceDirection == Direction.Up || _forceDirection == Direction.Down)
		{
			float num2 = Mathf.Sign(vector.z) * Mathf.Log(Mathf.Abs(vector.z) + 1f);
			num2 = num2 * PEVCConfig.instance.rotorBalanceAdjust + vector.z * (1f - PEVCConfig.instance.rotorBalanceAdjust);
			num2 *= PEVCConfig.instance.rotorBalaceScale.Evaluate(Mathf.Abs(num2));
			Vector3 centerOfMass = controller.rigidbody.centerOfMass;
			centerOfMass.x += vector.x;
			centerOfMass.y += vector.y;
			centerOfMass.z += num2;
			_forcePivot.position = _controller.transform.TransformPoint(centerOfMass);
		}
		else if (_forceDirection == Direction.Forward || _forceDirection == Direction.Back)
		{
			_rotateRight = num > 0.5f;
			_rotateLeft = num < -0.5f;
			Vector3 vector2 = Vector3.ProjectOnPlane(controller.rigidbody.worldCenterOfMass, Vector3.Cross(Vector3.up, base.transform.forward));
			Utility.ClosestPoint(_forcePivot.position + new Vector3(0f, 20f, 0f), _forcePivot.position + new Vector3(0f, -20f, 0f), vector2 + base.transform.forward * 20f, vector2 - base.transform.forward * 20f, out var pointA, out var _);
			_forcePivot.position = (pointA + _forcePivot.position) * 0.5f;
		}
		else
		{
			_rotateRight = num > 0f;
			_rotateLeft = num < 0f;
			Vector3 centerOfMass2 = controller.rigidbody.centerOfMass;
			centerOfMass2.x += vector.x;
			centerOfMass2.z += vector.z;
			_forcePivot.position = _controller.transform.TransformPoint(centerOfMass2);
		}
		base.enabled = isSubmarine || (_forceDirection != Direction.Up && _forceDirection != Direction.Down);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(_forcePivot.position, 0.2f);
		Gizmos.DrawRay(base.transform.position, base.transform.TransformDirection(_localAngularDirection));
	}

	private void FixedUpdate()
	{
		float num = 0f;
		float num2 = 0f;
		if (_controller.isEnergyEnough(0.01f) && _controller.hasDriver)
		{
			switch (_forceDirection)
			{
			case Direction.Up:
			case Direction.Down:
				num = ((_forceDirection != Direction.Up) ? (0f - _maxRotateSpeed) : _maxRotateSpeed);
				num *= _controller.inputVertical;
				num2 = 1f;
				break;
			case Direction.Left:
			case Direction.Right:
				if (Mathf.Abs(_controller.inputX) > 0.01f)
				{
					if (_rotateRight)
					{
						num = ((!(_controller.inputX > 0f)) ? (0f - _maxRotateSpeed) : _maxRotateSpeed);
					}
					if (_rotateLeft)
					{
						num = ((!(_controller.inputX > 0f)) ? _maxRotateSpeed : (0f - _maxRotateSpeed));
					}
				}
				num2 = 1f;
				break;
			case Direction.Forward:
			case Direction.Back:
				num2 = Mathf.Clamp(Vector3.Dot(_controller.rigidbody.velocity, (!(_currentRotateSpeed > 0f)) ? (-base.transform.forward) : base.transform.forward), 0f, 20f);
				num2 = 1f - 0.0025f * num2 * num2;
				if (Mathf.Abs(_controller.inputX) > 0.01f)
				{
					if (_rotateRight)
					{
						num = ((!(_controller.inputX > 0f)) ? (0f - _maxRotateSpeed) : _maxRotateSpeed);
						break;
					}
					if (_rotateLeft)
					{
						num = ((!(_controller.inputX > 0f)) ? _maxRotateSpeed : (0f - _maxRotateSpeed));
						break;
					}
				}
				num = ((_forceDirection != 0) ? ((0f - _controller.inputY) * _maxRotateSpeed) : (_controller.inputY * _maxRotateSpeed));
				break;
			}
		}
		if (num > _currentRotateSpeed)
		{
			_currentRotateSpeed = Mathf.Min(num, _currentRotateSpeed + _accelerate * Time.deltaTime);
		}
		else
		{
			_currentRotateSpeed = Mathf.Max(num, _currentRotateSpeed - _accelerate * Time.deltaTime);
		}
		_currentZAngle = (_currentZAngle + _currentRotateSpeed * Time.deltaTime) % 360f;
		_rotatePivot.localEulerAngles = new Vector3(0f, 0f, _currentZAngle);
		if (num2 > 0f)
		{
			float num3 = _currentRotateSpeed / _maxRotateSpeed * _maxForce * num2;
			if (VFVoxelWater.self.IsInWater(base.transform.position))
			{
				_controller.rigidbody.AddForceAtPosition(base.transform.forward * num3 * _controller.speedScale, _forcePivot.position);
			}
			if (_controller.isPlayerHost)
			{
				_controller.ExpendEnergy(num3 * Time.deltaTime * PEVCConfig.instance.boatPropellerEnergySpeed);
			}
		}
	}
}
