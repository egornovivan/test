using UnityEngine;

public class SteerForNeighborAvoidance : Steering
{
	[SerializeField]
	private float _avoidAngleCos = 0.707f;

	[SerializeField]
	private float _minTimeToCollision = 2f;

	public float AvoidAngleCos
	{
		get
		{
			return _avoidAngleCos;
		}
		set
		{
			_avoidAngleCos = value;
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

	protected override Vector3 CalculateForce()
	{
		float num = 0f;
		Vehicle vehicle = null;
		float num2 = _minTimeToCollision;
		Vector3 vector = Vector3.zero;
		Vector3 position = Vector3.zero;
		foreach (Vehicle vehicle2 in base.Vehicle.Radar.Vehicles)
		{
			if (!(vehicle2 != this))
			{
				continue;
			}
			float num3 = base.Vehicle.ScaledRadius + vehicle2.ScaledRadius;
			float num4 = base.Vehicle.PredictNearestApproachTime(vehicle2);
			if (num4 >= 0f && num4 < num2)
			{
				Vector3 ourPosition = Vector3.zero;
				Vector3 hisPosition = Vector3.zero;
				float num5 = base.Vehicle.ComputeNearestApproachPositions(vehicle2, num4, ref ourPosition, ref hisPosition);
				if (num5 < num3)
				{
					num2 = num4;
					vehicle = vehicle2;
					vector = hisPosition;
					position = ourPosition;
				}
			}
		}
		if (vehicle != null)
		{
			float num6 = Vector3.Dot(base.transform.forward, vehicle.transform.forward);
			if (num6 < 0f - _avoidAngleCos)
			{
				Vector3 lhs = vector - base.Vehicle.Position;
				float num7 = Vector3.Dot(lhs, base.transform.right);
				num = ((!(num7 > 0f)) ? 1f : (-1f));
			}
			else if (num6 > _avoidAngleCos)
			{
				Vector3 lhs2 = vehicle.Position - base.Vehicle.Position;
				float num8 = Vector3.Dot(lhs2, base.transform.right);
				num = ((!(num8 > 0f)) ? 1f : (-1f));
			}
			else if (base.Vehicle.Speed < vehicle.Speed || vehicle.Speed == 0f || base.gameObject.GetInstanceID() < vehicle.gameObject.GetInstanceID())
			{
				float num9 = Vector3.Dot(base.transform.right, vehicle.Velocity);
				num = ((!(num9 > 0f)) ? 1f : (-1f));
			}
			num *= base.Vehicle.ScaledRadius + vehicle.ScaledRadius;
			AnnotateAvoidNeighbor(vehicle, num, position, vector);
		}
		return base.transform.right * num;
	}

	protected virtual void AnnotateAvoidNeighbor(Vehicle vehicle, float steer, Vector3 position, Vector3 threatPosition)
	{
		Debug.DrawLine(base.Vehicle.Position, vehicle.Position, Color.red);
		Debug.DrawLine(base.Vehicle.Position, position, Color.green);
		Debug.DrawLine(base.Vehicle.Position, threatPosition, Color.magenta);
	}
}
