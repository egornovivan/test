using UnityEngine;
using UnitySteer;

public class SteerForWander : Steering
{
	private float _wanderSide;

	private float _wanderUp;

	[SerializeField]
	private float _maxLatitudeSide = 2f;

	[SerializeField]
	private float _maxLatitudeUp = 2f;

	public float MaxLatitudeSide
	{
		get
		{
			return _maxLatitudeSide;
		}
		set
		{
			_maxLatitudeSide = value;
		}
	}

	public float MaxLatitudeUp
	{
		get
		{
			return _maxLatitudeUp;
		}
		set
		{
			_maxLatitudeUp = value;
		}
	}

	protected override Vector3 CalculateForce()
	{
		float maxSpeed = base.Vehicle.MaxSpeed;
		_wanderSide = OpenSteerUtility.scalarRandomWalk(_wanderSide, maxSpeed, 0f - _maxLatitudeSide, _maxLatitudeSide);
		_wanderUp = OpenSteerUtility.scalarRandomWalk(_wanderUp, maxSpeed, 0f - _maxLatitudeUp, _maxLatitudeUp);
		return base.transform.right * _wanderSide + base.transform.up * _wanderUp;
	}
}
