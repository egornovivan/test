using System.Collections.Generic;
using UnityEngine;

namespace UnitySteer;

public class Obstacle
{
	private static Dictionary<int, Obstacle> _obstacleCache;

	public static Dictionary<int, Obstacle> ObstacleCache => _obstacleCache;

	static Obstacle()
	{
		_obstacleCache = new Dictionary<int, Obstacle>();
	}

	public virtual Vector3 steerToAvoid(SteeringVehicle v, float minTimeToCollision)
	{
		return Vector3.zero;
	}
}
