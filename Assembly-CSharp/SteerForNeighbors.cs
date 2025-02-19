using UnityEngine;
using UnitySteer;

public class SteerForNeighbors : Steering
{
	[SerializeField]
	private float _minRadius = 3f;

	[SerializeField]
	private float _maxRadius = 7.5f;

	[SerializeField]
	private float _angleCos = 0.7f;

	[SerializeField]
	private LayerMask _layersChecked;

	public float AngleCos
	{
		get
		{
			return _angleCos;
		}
		set
		{
			_angleCos = Mathf.Clamp(value, -1f, 1f);
		}
	}

	public float AngleDeg
	{
		get
		{
			return OpenSteerUtility.DegreesFromCos(_angleCos);
		}
		set
		{
			_angleCos = OpenSteerUtility.CosFromDegrees(value);
		}
	}

	public LayerMask LayersChecked
	{
		get
		{
			return _layersChecked;
		}
		set
		{
			_layersChecked = value;
		}
	}

	public float MinRadius
	{
		get
		{
			return _minRadius;
		}
		set
		{
			_minRadius = value;
		}
	}

	public float MaxRadius
	{
		get
		{
			return _maxRadius;
		}
		set
		{
			_maxRadius = value;
		}
	}

	protected override Vector3 CalculateForce()
	{
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = 0; i < base.Vehicle.Radar.Vehicles.Count; i++)
		{
			Vehicle vehicle = base.Vehicle.Radar.Vehicles[i];
			if (((1 << vehicle.gameObject.layer) & (int)LayersChecked) != 0 && base.Vehicle.IsInNeighborhood(vehicle, MinRadius, MaxRadius, AngleCos))
			{
				Debug.DrawLine(base.Vehicle.Position, vehicle.Position, Color.magenta);
				zero += CalculateNeighborContribution(vehicle);
				num++;
			}
		}
		if (num > 0)
		{
			zero /= (float)num;
			zero.Normalize();
		}
		return zero;
	}

	protected virtual Vector3 CalculateNeighborContribution(Vehicle other)
	{
		return Vector3.zero;
	}
}
