using System.Collections;
using UnityEngine;

namespace UnitySteer;

public class SteerLibrary : SteeringVehicle
{
	public SteerLibrary(Vector3 position, float mass)
		: base(position, mass)
	{
	}

	public SteerLibrary(Transform transform, float mass)
		: base(transform, mass)
	{
	}

	public SteerLibrary(Rigidbody rigidbody)
		: base(rigidbody)
	{
	}

	public void resetSteering()
	{
	}

	private bool isAhead(Vector3 target)
	{
		return isAhead(target, 0.707f);
	}

	private bool isAside(Vector3 target)
	{
		return isAside(target, 0.707f);
	}

	private bool isBehind(Vector3 target)
	{
		return isBehind(target, -0.707f);
	}

	private bool isAhead(Vector3 target, float cosThreshold)
	{
		Vector3 rhs = target - base.Position;
		rhs.Normalize();
		return Vector3.Dot(base.Forward, rhs) > cosThreshold;
	}

	private bool isAside(Vector3 target, float cosThreshold)
	{
		Vector3 rhs = target - base.Position;
		rhs.Normalize();
		float num = Vector3.Dot(base.Forward, rhs);
		return num < cosThreshold && num > 0f - cosThreshold;
	}

	private bool isBehind(Vector3 target, float cosThreshold)
	{
		Vector3 rhs = target - base.Position;
		rhs.Normalize();
		return Vector3.Dot(base.Forward, rhs) < cosThreshold;
	}

	public virtual void annotatePathFollowing(Vector3 future, Vector3 onPath, Vector3 target, float outside)
	{
		Debug.DrawLine(base.Position, future, Color.white);
		Debug.DrawLine(base.Position, onPath, Color.yellow);
		Debug.DrawLine(base.Position, target, Color.magenta);
	}

	public virtual void annotateAvoidCloseNeighbor(SteeringVehicle otherVehicle, Vector3 component)
	{
		Debug.DrawLine(base.Position, otherVehicle.Position, Color.red);
		Debug.DrawRay(base.Position, component * 3f, Color.yellow);
	}

	public Vector3 steerForSeek(Vector3 target)
	{
		Vector3 vector = target - base.Position;
		return vector - base.Velocity;
	}

	public Vector3 steerToStayOnPath(float predictionTime, Pathway path)
	{
		Vector3 vector = predictFuturePosition(predictionTime);
		mapReturnStruct tStruct = default(mapReturnStruct);
		Vector3 vector2 = path.mapPointToPath(vector, ref tStruct);
		if (tStruct.outside < 0f)
		{
			return Vector3.zero;
		}
		annotatePathFollowing(vector, vector2, vector2, tStruct.outside);
		return steerForSeek(vector2);
	}

	public Vector3 steerToAvoidObstacle(float minTimeToCollision, Obstacle obstacle)
	{
		return obstacle.steerToAvoid(this, minTimeToCollision);
	}

	public Vector3 steerToAvoidCloseNeighbors(float minSeparationDistance, ArrayList others)
	{
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < others.Count; i++)
		{
			SteeringVehicle steeringVehicle = (SteeringVehicle)others[i];
			if (steeringVehicle != this)
			{
				float num = base.Radius + steeringVehicle.Radius;
				float num2 = minSeparationDistance + num;
				Vector3 vector2 = steeringVehicle.Position - base.Position;
				float magnitude = vector2.magnitude;
				if (magnitude < num2)
				{
					vector = OpenSteerUtility.perpendicularComponent(-vector2, base.Forward);
					annotateAvoidCloseNeighbor(steeringVehicle, vector);
				}
			}
		}
		return vector;
	}
}
