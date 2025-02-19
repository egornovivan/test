using UnityEngine;

public class SteerForEvasion : Steering
{
	[SerializeField]
	private Vehicle _menace;

	[SerializeField]
	private float _predictionTime;

	public float PredictionTime
	{
		get
		{
			return _predictionTime;
		}
		set
		{
			_predictionTime = value;
		}
	}

	public Vehicle Menace
	{
		get
		{
			return _menace;
		}
		set
		{
			_menace = value;
		}
	}

	protected override Vector3 CalculateForce()
	{
		float magnitude = (_menace.Position - base.Vehicle.Position).magnitude;
		float num = magnitude / _menace.Speed;
		float predictionTime = ((!(num > _predictionTime)) ? num : _predictionTime);
		Vector3 vector = _menace.PredictFuturePosition(predictionTime);
		Vector3 vector2 = base.Vehicle.Position - vector;
		return vector2 - base.Vehicle.Velocity;
	}
}
