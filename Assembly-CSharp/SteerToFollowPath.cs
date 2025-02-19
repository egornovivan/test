using UnityEngine;
using UnitySteer;

public class SteerToFollowPath : Steering
{
	private FollowDirection _direction = FollowDirection.Forward;

	private float _predictionTime = 2f;

	private Pathway _path;

	public FollowDirection Direction
	{
		get
		{
			return _direction;
		}
		set
		{
			_direction = value;
		}
	}

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

	public Pathway Path
	{
		get
		{
			return _path;
		}
		set
		{
			_path = value;
		}
	}

	protected override Vector3 CalculateForce()
	{
		if (_path == null)
		{
			return Vector3.zero;
		}
		float num = (float)_direction * _predictionTime * base.Vehicle.Speed;
		Vector3 point = base.Vehicle.PredictFuturePosition(_predictionTime);
		float num2 = _path.mapPointToPathDistance(base.Vehicle.Position);
		float num3 = _path.mapPointToPathDistance(point);
		bool flag = ((!(num > 0f)) ? (num2 > num3) : (num2 < num3));
		mapReturnStruct tStruct = default(mapReturnStruct);
		_path.mapPointToPath(point, ref tStruct);
		if (tStruct.outside < 0f && flag)
		{
			return Vector3.zero;
		}
		float pathDistance = num2 + num;
		Vector3 target = _path.mapPathDistanceToPoint(pathDistance);
		return base.Vehicle.GetSeekVector(target);
	}
}
