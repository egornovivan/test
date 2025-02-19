using UnityEngine;
using UnitySteer;

public class AutonomousVehicle : Vehicle
{
	private Vector3 _smoothedAcceleration;

	private Rigidbody _rigidbody;

	private CharacterController _characterController;

	private void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_characterController = GetComponent<CharacterController>();
	}

	private void FixedUpdate()
	{
		Vector3 zero = Vector3.zero;
		Steering[] steerings = base.Steerings;
		foreach (Steering steering in steerings)
		{
			if (steering.enabled)
			{
				zero += steering.WeighedForce;
			}
		}
		ApplySteeringForce(zero, Time.fixedDeltaTime);
	}

	private void ApplySteeringForce(Vector3 force, float elapsedTime)
	{
		if (base.MaxForce != 0f && base.MaxSpeed != 0f && elapsedTime != 0f)
		{
			Vector3 vector = Vector3.ClampMagnitude(force, base.MaxForce);
			Vector3 newValue = vector / base.Mass;
			if (newValue.sqrMagnitude == 0f && !base.HasInertia)
			{
				base.Speed = 0f;
			}
			Vector3 velocity = base.Velocity;
			_smoothedAcceleration = OpenSteerUtility.blendIntoAccumulator(0.4f, newValue, _smoothedAcceleration);
			velocity += _smoothedAcceleration * elapsedTime;
			velocity = Vector3.ClampMagnitude(velocity, base.MaxSpeed);
			if (base.IsPlanar)
			{
				velocity.y = base.Velocity.y;
			}
			base.Speed = velocity.magnitude;
			Vector3 vector2 = velocity * elapsedTime;
			if (_characterController != null)
			{
				_characterController.Move(vector2);
			}
			else if (_rigidbody == null || _rigidbody.isKinematic)
			{
				base.transform.position += vector2;
			}
			else
			{
				_rigidbody.MovePosition(_rigidbody.position + vector2);
			}
			RegenerateLocalSpace(velocity);
		}
	}
}
