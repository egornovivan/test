using C5;
using UnityEngine;
using UnitySteer;
using UnitySteer.Helpers;

public class Radar : MonoBehaviour, ITick
{
	private SteeringEventHandler<Radar> _onDetected;

	[SerializeField]
	private Tick _tick;

	[SerializeField]
	private LayerMask _obstacleLayer;

	[SerializeField]
	private LayerMask _layersChecked;

	private IList<Collider> _detected;

	private IList<Vehicle> _vehicles = new ArrayList<Vehicle>();

	private IList<Obstacle> _obstacles = new ArrayList<Obstacle>();

	private ObstacleFactory _obstacleFactory;

	private Vehicle _vehicle;

	public IList<Collider> Detected
	{
		get
		{
			ExecuteRadar();
			return _detected;
		}
	}

	public IList<Obstacle> Obstacles
	{
		get
		{
			ExecuteRadar();
			return new GuardedList<Obstacle>(_obstacles);
		}
	}

	public SteeringEventHandler<Radar> OnDetected
	{
		get
		{
			return _onDetected;
		}
		set
		{
			_onDetected = value;
		}
	}

	public Vehicle Vehicle => _vehicle;

	public IList<Vehicle> Vehicles
	{
		get
		{
			ExecuteRadar();
			return new GuardedList<Vehicle>(_vehicles);
		}
	}

	public LayerMask ObstacleLayer
	{
		get
		{
			return _obstacleLayer;
		}
		set
		{
			_obstacleLayer = value;
		}
	}

	public ObstacleFactory ObstacleFactory
	{
		get
		{
			return _obstacleFactory;
		}
		set
		{
			_obstacleFactory = value;
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

	public Tick Tick => _tick;

	protected virtual void Awake()
	{
		_vehicle = GetComponent<Vehicle>();
	}

	private void ExecuteRadar()
	{
		if (_tick.ShouldTick())
		{
			_detected = Detect();
			FilterDetected();
			if (_onDetected != null)
			{
				_onDetected(new SteeringEvent<Radar>(null, "detect", this));
			}
		}
	}

	protected virtual IList<Collider> Detect()
	{
		return new ArrayList<Collider>();
	}

	protected virtual void FilterDetected()
	{
		_vehicles.Clear();
		_obstacles.Clear();
		foreach (Collider item2 in _detected)
		{
			Vehicle component = item2.gameObject.GetComponent<Vehicle>();
			if (component != null && item2.gameObject != base.gameObject)
			{
				_vehicles.Add(component);
			}
			if (ObstacleFactory != null && ((1 << item2.gameObject.layer) & (int)ObstacleLayer) > 0)
			{
				Obstacle item = ObstacleFactory(item2.gameObject);
				_obstacles.Add(item);
			}
		}
	}
}
