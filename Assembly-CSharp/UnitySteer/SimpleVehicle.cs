using UnityEngine;

namespace UnitySteer;

public class SimpleVehicle : SteerLibrary
{
	private float _curvature;

	private Vector3 _lastForward;

	private Vector3 _lastPosition;

	private Vector3 _smoothedPosition;

	private float _smoothedCurvature;

	private Vector3 _smoothedAcceleration;

	public int SerialNumber => base.GameObject.GetInstanceID();

	public SimpleVehicle(Vector3 position, float mass)
		: base(position, mass)
	{
		reset();
	}

	public SimpleVehicle(Transform transform, float mass)
		: base(transform, mass)
	{
		reset();
	}

	public SimpleVehicle(Rigidbody rigidbody)
		: base(rigidbody)
	{
		reset();
	}

	public virtual void reset()
	{
		resetSteering();
		base.Speed = 0f;
		resetSmoothedPosition(Vector3.zero);
		resetSmoothedCurvature(0f);
		resetSmoothedAcceleration(Vector3.zero);
	}

	private float curvature()
	{
		return _curvature;
	}

	private float smoothedCurvature()
	{
		return _smoothedCurvature;
	}

	private float resetSmoothedCurvature(float value)
	{
		_lastForward = Vector3.zero;
		_lastPosition = Vector3.zero;
		return _smoothedCurvature = (_curvature = value);
	}

	private Vector3 smoothedAcceleration()
	{
		return _smoothedAcceleration;
	}

	private Vector3 resetSmoothedAcceleration(Vector3 value)
	{
		return _smoothedAcceleration = value;
	}

	private Vector3 smoothedPosition()
	{
		return _smoothedPosition;
	}

	private Vector3 resetSmoothedPosition(Vector3 value)
	{
		return _smoothedPosition = value;
	}

	public virtual Vector3 AdjustRawSteeringForce(Vector3 force)
	{
		float num = 0.2f * base.MaxSpeed;
		if (base.Speed > num || force == Vector3.zero)
		{
			return force;
		}
		float f = base.Speed / num;
		float cosineOfConeAngle = Mathf.Lerp(1f, -1f, Mathf.Pow(f, 20f));
		return OpenSteerUtility.limitMaxDeviationAngle(force, cosineOfConeAngle, base.Forward);
	}

	private void applyBrakingForce(float rate, float elapsedTime)
	{
		float num = base.Speed * rate;
		float num2 = ((!(num < base.MaxForce)) ? base.MaxForce : num);
		base.Speed -= num2 * elapsedTime;
	}

	public void regenerateLocalSpaceForBanking(Vector3 newVelocity, float elapsedTime)
	{
		Vector3 vector = new Vector3(0f, 0.2f, 0f);
		Vector3 vector2 = _smoothedAcceleration * 0.05f;
		Vector3 newValue = vector2 + vector;
		float smoothRate = elapsedTime * 3f;
		Vector3 up = base.Up;
		up = OpenSteerUtility.blendIntoAccumulator(smoothRate, newValue, up);
		up.Normalize();
		base.Up = up;
		if (base.Speed > 0f)
		{
			base.Forward = newVelocity / base.Speed;
		}
	}

	private void measurePathCurvature(float elapsedTime)
	{
		if (elapsedTime > 0f)
		{
			Vector3 position = base.Position;
			Vector3 forward = base.Forward;
			Vector3 vector = _lastPosition - position;
			Vector3 source = (_lastForward - forward) / vector.magnitude;
			Vector3 lhs = OpenSteerUtility.perpendicularComponent(source, forward);
			float num = ((!(Vector3.Dot(lhs, base.Side) < 0f)) ? (-1f) : 1f);
			_curvature = lhs.magnitude * num;
			_smoothedCurvature = OpenSteerUtility.blendIntoAccumulator(elapsedTime * 4f, _curvature, _smoothedCurvature);
			_lastForward = forward;
			_lastPosition = position;
		}
	}

	public override Vector3 predictFuturePosition(float predictionTime)
	{
		return base.Position + base.Velocity * predictionTime;
	}
}
