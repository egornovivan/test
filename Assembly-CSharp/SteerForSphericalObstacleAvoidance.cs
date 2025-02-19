using UnityEngine;
using UnitySteer;

public class SteerForSphericalObstacleAvoidance : Steering
{
	public struct PathIntersection
	{
		public bool intersect;

		public float distance;

		public SphericalObstacle obstacle;

		public PathIntersection(SphericalObstacle obstacle)
		{
			this.obstacle = obstacle;
			intersect = false;
			distance = float.MaxValue;
		}
	}

	[SerializeField]
	private float _avoidanceForceFactor = 0.75f;

	[SerializeField]
	private float _minTimeToCollision = 2f;

	public float AvoidanceForceFactor
	{
		get
		{
			return _avoidanceForceFactor;
		}
		set
		{
			_avoidanceForceFactor = value;
		}
	}

	public float MinTimeToCollision
	{
		get
		{
			return _minTimeToCollision;
		}
		set
		{
			_minTimeToCollision = value;
		}
	}

	protected new void Start()
	{
		base.Start();
		base.Vehicle.Radar.ObstacleFactory = SphericalObstacle.GetObstacle;
	}

	protected override Vector3 CalculateForce()
	{
		Vector3 result = Vector3.zero;
		if (base.Vehicle.Radar.Obstacles == null || base.Vehicle.Radar.Obstacles.Count == 0)
		{
			return result;
		}
		PathIntersection pathIntersection = new PathIntersection(null);
		Vector3 vector = base.Vehicle.PredictFuturePosition(_minTimeToCollision);
		Vector3 line = vector - base.Vehicle.Position;
		foreach (Obstacle obstacle in base.Vehicle.Radar.Obstacles)
		{
			SphericalObstacle obs = obstacle as SphericalObstacle;
			PathIntersection pathIntersection2 = FindNextIntersectionWithSphere(obs, line);
			if (!pathIntersection.intersect || (pathIntersection2.intersect && pathIntersection2.distance < pathIntersection.distance))
			{
				pathIntersection = pathIntersection2;
			}
		}
		if (pathIntersection.intersect && pathIntersection.distance < line.magnitude)
		{
			Debug.DrawLine(base.Vehicle.Position, pathIntersection.obstacle.center, Color.red);
			Vector3 source = base.Vehicle.Position - pathIntersection.obstacle.center;
			result = OpenSteerUtility.perpendicularComponent(source, base.transform.forward);
			result.Normalize();
			result *= base.Vehicle.MaxForce;
			result += base.transform.forward * base.Vehicle.MaxForce * _avoidanceForceFactor;
		}
		return result;
	}

	public PathIntersection FindNextIntersectionWithSphere(SphericalObstacle obs, Vector3 line)
	{
		Vector3 rhs = base.Vehicle.Position - obs.center;
		PathIntersection result = new PathIntersection(obs);
		obs.annotatePosition();
		Debug.DrawLine(base.Vehicle.Position, base.Vehicle.Position + line, Color.cyan);
		float sqrMagnitude = line.sqrMagnitude;
		float num = 2f * Vector3.Dot(line, rhs);
		float sqrMagnitude2 = obs.center.sqrMagnitude;
		sqrMagnitude2 += base.Vehicle.Position.sqrMagnitude;
		sqrMagnitude2 -= 2f * Vector3.Dot(obs.center, base.Vehicle.Position);
		sqrMagnitude2 -= Mathf.Pow(obs.radius + base.Vehicle.ScaledRadius, 2f);
		float num2 = num * num - 4f * sqrMagnitude * sqrMagnitude2;
		if (num2 >= 0f)
		{
			result.intersect = true;
			Vector3 dir = Vector3.zero;
			if (num2 == 0f)
			{
				float num3 = (0f - num) / (2f * sqrMagnitude);
				dir = num3 * line;
			}
			else
			{
				float num4 = (0f - num + Mathf.Sqrt(num2)) / (2f * sqrMagnitude);
				float num5 = (0f - num - Mathf.Sqrt(num2)) / (2f * sqrMagnitude);
				if (num4 < 0f && num5 < 0f)
				{
					result.intersect = false;
				}
				else
				{
					dir = ((!(Mathf.Abs(num4) < Mathf.Abs(num5))) ? (num5 * line) : (num4 * line));
				}
			}
			Debug.DrawRay(base.Vehicle.Position, dir, Color.red);
			result.distance = dir.magnitude;
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		if (!(base.Vehicle != null))
		{
			return;
		}
		foreach (Obstacle obstacle in base.Vehicle.Radar.Obstacles)
		{
			SphericalObstacle sphericalObstacle = obstacle as SphericalObstacle;
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(sphericalObstacle.center, sphericalObstacle.radius);
		}
	}
}
