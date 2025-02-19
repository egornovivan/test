using UnityEngine;

public class SteerForTether : Steering
{
	[SerializeField]
	private float _maximumDistance = 30f;

	[SerializeField]
	private Vector3 _tetherPosition;

	public float MaximumDistance
	{
		get
		{
			return _maximumDistance;
		}
		set
		{
			_maximumDistance = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public Vector3 TetherPosition
	{
		get
		{
			return _tetherPosition;
		}
		set
		{
			_tetherPosition = value;
		}
	}

	protected override Vector3 CalculateForce()
	{
		Vector3 result = Vector3.zero;
		Vector3 vector = TetherPosition - base.Vehicle.Position;
		float magnitude = vector.magnitude;
		if (magnitude > _maximumDistance)
		{
			result = vector - base.Vehicle.Velocity;
		}
		return result;
	}
}
