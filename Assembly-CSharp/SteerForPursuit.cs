using UnityEngine;
using UnitySteer;
using UnitySteer.Helpers;

public class SteerForPursuit : Steering
{
	private bool _reportedArrival;

	private SteeringEventHandler<Vehicle> _onArrival;

	[SerializeField]
	private Vehicle _quarry;

	[SerializeField]
	private float _maxPredictionTime = 5f;

	public float MaxPredictionTime
	{
		get
		{
			return _maxPredictionTime;
		}
		set
		{
			_maxPredictionTime = value;
		}
	}

	public SteeringEventHandler<Vehicle> OnArrival
	{
		get
		{
			return _onArrival;
		}
		set
		{
			_onArrival = value;
		}
	}

	public Vehicle Quarry
	{
		get
		{
			return _quarry;
		}
		set
		{
			if (_quarry != value)
			{
				_reportedArrival = false;
				_quarry = value;
			}
		}
	}

	protected override Vector3 CalculateForce()
	{
		if (_quarry == null)
		{
			base.enabled = false;
			return Vector3.zero;
		}
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = _quarry.Position - base.Vehicle.Position;
		float magnitude = vector2.magnitude;
		float num = base.Vehicle.ScaledRadius + _quarry.ScaledRadius;
		if (magnitude > num)
		{
			Vector3 rhs = vector2 / magnitude;
			float x = Vector3.Dot(base.transform.forward, _quarry.transform.forward);
			float x2 = Vector3.Dot(base.transform.forward, rhs);
			float num2 = magnitude / base.Vehicle.Speed;
			int num3 = OpenSteerUtility.intervalComparison(x2, -0.707f, 0.707f);
			int num4 = OpenSteerUtility.intervalComparison(x, -0.707f, 0.707f);
			float num5 = 0f;
			switch (num3)
			{
			case 1:
				switch (num4)
				{
				case 1:
					num5 = 4f;
					break;
				case 0:
					num5 = 1.8f;
					break;
				case -1:
					num5 = 0.85f;
					break;
				}
				break;
			case 0:
				switch (num4)
				{
				case 1:
					num5 = 1f;
					break;
				case 0:
					num5 = 0.8f;
					break;
				case -1:
					num5 = 4f;
					break;
				}
				break;
			case -1:
				switch (num4)
				{
				case 1:
					num5 = 0.5f;
					break;
				case 0:
					num5 = 2f;
					break;
				case -1:
					num5 = 2f;
					break;
				}
				break;
			}
			float num6 = num2 * num5;
			float predictionTime = ((!(num6 > _maxPredictionTime)) ? num6 : _maxPredictionTime);
			Vector3 vector3 = _quarry.PredictFuturePosition(predictionTime);
			vector = base.Vehicle.GetSeekVector(vector3);
			Debug.DrawLine(base.Vehicle.Position, vector, Color.blue);
			Debug.DrawLine(Quarry.Position, vector3, Color.cyan);
			Debug.DrawRay(vector3, Vector3.up * 4f, Color.cyan);
		}
		if (!_reportedArrival && _onArrival != null && vector == Vector3.zero)
		{
			_reportedArrival = true;
			_onArrival(new SteeringEvent<Vehicle>(this, "arrived", Quarry));
		}
		else
		{
			_reportedArrival = vector == Vector3.zero;
		}
		return vector;
	}
}
