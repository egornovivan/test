using UnityEngine;
using UnitySteer.Helpers;

public class Steering : MonoBehaviour, ITick
{
	private Vector3 _force = Vector3.zero;

	private Vehicle _vehicle;

	[SerializeField]
	private Tick _tick;

	[SerializeField]
	private float _weight = 1f;

	public Vector3 Force
	{
		get
		{
			if (Tick.ShouldTick())
			{
				_force = CalculateForce();
			}
			return _force;
		}
	}

	public Vector3 WeighedForce => Force * _weight;

	public Tick Tick => _tick;

	public Vehicle Vehicle => _vehicle;

	public float Weight
	{
		get
		{
			return _weight;
		}
		set
		{
			_weight = value;
		}
	}

	protected void Start()
	{
		_vehicle = GetComponent<Vehicle>();
	}

	protected virtual Vector3 CalculateForce()
	{
		return Vector3.zero;
	}
}
