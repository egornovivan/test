using UnityEngine;

namespace WhiteCat;

public class VCPVehicleWheel : VCPart
{
	private const float massToBrakeRatio = 100f;

	[SerializeField]
	private float _mass = 100f;

	[SerializeField]
	private float _radius = 0.5f;

	[SerializeField]
	private float _motorTorqueScale = 1f;

	[Range(1f, 2f)]
	[SerializeField]
	private float _stiffness = 1.5f;

	[Space(8f)]
	[SerializeField]
	private Transform _wheelModel;

	[SerializeField]
	private WheelCollider wheelCollider;

	[SerializeField]
	[Header("Effects")]
	private ParticleSystem _dirtEffect;

	[SerializeField]
	private float _rpmToStartSpeed;

	[SerializeField]
	private float _rpmToEmissionRate;

	[HideInInspector]
	public bool isMotorWheel;

	[HideInInspector]
	public bool isSteerWheel;

	private VehicleController _controller;

	private float _steerLeft;

	private float _steerRight;

	private static Vector3 _pos;

	private static Quaternion _rot;

	private float _directionChangeTime;

	private bool _lastDirection;

	private Vector3 _effectOffset;

	public float rpm => wheelCollider.rpm;

	public void InitLayer()
	{
		wheelCollider.gameObject.layer = VCConfig.s_WheelLayer;
	}

	public void Init(VehicleController controller, float steerLeft, float steerRight)
	{
		_controller = controller;
		_steerLeft = steerLeft;
		_steerRight = steerRight;
		wheelCollider.gameObject.layer = VCConfig.s_WheelLayer;
		wheelCollider.mass = _mass;
		wheelCollider.radius = _radius;
		wheelCollider.wheelDampingRate = 0.5f;
		Vector3 vector = _controller.rigidbody.transform.InverseTransformPoint(wheelCollider.transform.position);
		float num = _controller.rigidbody.centerOfMass.y - vector.y + wheelCollider.radius;
		wheelCollider.forceAppPointDistance = num + PEVCConfig.instance.wheelForceAppPointOffset;
		WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
		sidewaysFriction.stiffness = _stiffness * PEVCConfig.instance.sideStiffnessFactor + PEVCConfig.instance.sideStiffnessBase;
		wheelCollider.sidewaysFriction = sidewaysFriction;
		sidewaysFriction = wheelCollider.forwardFriction;
		sidewaysFriction.stiffness = _stiffness * PEVCConfig.instance.fwdStiffnessFactor + PEVCConfig.instance.fwdStiffnessBase;
		wheelCollider.forwardFriction = sidewaysFriction;
		_effectOffset = _dirtEffect.transform.localPosition;
	}

	public void OnFixedUpdate(float averageSprungMass)
	{
		float sprungMass = wheelCollider.sprungMass;
		sprungMass = ((!float.IsNaN(sprungMass)) ? Mathf.Min(sprungMass, averageSprungMass * 5f) : averageSprungMass);
		JointSpring suspensionSpring = wheelCollider.suspensionSpring;
		suspensionSpring.spring = sprungMass * PEVCConfig.instance.naturalFrequency * PEVCConfig.instance.naturalFrequency;
		suspensionSpring.damper = 2f * PEVCConfig.instance.dampingRatio * Mathf.Sqrt(suspensionSpring.spring * sprungMass);
		suspensionSpring.targetPosition = 0.5f;
		wheelCollider.suspensionSpring = suspensionSpring;
		wheelCollider.suspensionDistance = sprungMass * Mathf.Abs(Physics.gravity.y) / (suspensionSpring.targetPosition * suspensionSpring.spring);
		UpdateEffect();
	}

	private void UpdateEffect()
	{
		if (wheelCollider.isGrounded && Mathf.Abs(wheelCollider.rpm) > 5f)
		{
			if (!_dirtEffect.isPlaying)
			{
				_dirtEffect.Play();
			}
			_dirtEffect.startSpeed = Mathf.Min(Mathf.Abs(wheelCollider.rpm * _rpmToStartSpeed), 5f);
			_dirtEffect.emissionRate = Mathf.Min(Mathf.Abs(wheelCollider.rpm * _rpmToEmissionRate), 100f);
			if (_lastDirection != wheelCollider.rpm > 0f)
			{
				_directionChangeTime += Time.deltaTime;
				if (_directionChangeTime > 0.5f)
				{
					_directionChangeTime = 0f;
					_lastDirection = !_lastDirection;
				}
			}
			else
			{
				_directionChangeTime = 0f;
			}
			_dirtEffect.transform.localEulerAngles = new Vector3(-18f, ((!_lastDirection) ? 0f : (-180f)) + wheelCollider.steerAngle, 0f);
			_dirtEffect.transform.localPosition = _effectOffset + _wheelModel.localPosition;
		}
		else if (_dirtEffect.isPlaying)
		{
			_dirtEffect.Stop();
		}
	}

	public void OnUpdate(float motorTorque, float rotateFactor)
	{
		if (isSteerWheel)
		{
			wheelCollider.steerAngle = _controller.inputX * rotateFactor * ((!(_controller.inputX > 0f)) ? _steerLeft : _steerRight);
		}
		if (isMotorWheel)
		{
			wheelCollider.motorTorque = motorTorque * _motorTorqueScale;
		}
		wheelCollider.brakeTorque = _controller.inputBrake * _mass * 100f;
		wheelCollider.GetWorldPose(out _pos, out _rot);
		_wheelModel.position = _pos;
		_wheelModel.rotation = _rot;
	}
}
