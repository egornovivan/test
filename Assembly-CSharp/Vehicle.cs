using UnityEngine;

public class Vehicle : MonoBehaviour
{
	private static float MIN_FORCE_THRESHOLD = 0.01f;

	private Steering[] _steerings;

	[SerializeField]
	private bool _drawGizmos;

	[SerializeField]
	private Vector3 _center;

	[HideInInspector]
	[SerializeField]
	private Vector3 _scaledCenter;

	[SerializeField]
	private bool _hasInertia;

	[SerializeField]
	private float _internalMass = 1f;

	[SerializeField]
	private bool _isPlanar;

	[SerializeField]
	private float _radius = 1f;

	[HideInInspector]
	[SerializeField]
	private float _scaledRadius = 1f;

	private float _speed;

	[SerializeField]
	private float _maxSpeed = 1f;

	[SerializeField]
	private float _maxForce = 10f;

	[SerializeField]
	private bool _canMove = true;

	private Radar _radar;

	private Transform _transform;

	public bool CanMove
	{
		get
		{
			return _canMove;
		}
		set
		{
			_canMove = value;
		}
	}

	public Vector3 Center
	{
		get
		{
			return _center;
		}
		set
		{
			_center = value;
			RecalculateScaledValues();
		}
	}

	public bool HasInertia
	{
		get
		{
			return _hasInertia;
		}
		set
		{
			_hasInertia = value;
		}
	}

	public bool IsPlanar
	{
		get
		{
			return _isPlanar;
		}
		set
		{
			_isPlanar = value;
		}
	}

	public float Mass
	{
		get
		{
			return (!(GetComponent<Rigidbody>() != null)) ? _internalMass : GetComponent<Rigidbody>().mass;
		}
		set
		{
			if (GetComponent<Rigidbody>() != null)
			{
				GetComponent<Rigidbody>().mass = value;
			}
			else
			{
				_internalMass = value;
			}
		}
	}

	public float MaxForce
	{
		get
		{
			return _maxForce;
		}
		set
		{
			_maxForce = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public float MaxSpeed
	{
		get
		{
			return _maxSpeed;
		}
		set
		{
			_maxSpeed = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public Vector3 Position => _transform.position + _scaledCenter;

	public Radar Radar
	{
		get
		{
			if (_radar == null)
			{
				_radar = GetComponent<Radar>();
			}
			return _radar;
		}
	}

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = Mathf.Clamp(value, 0.01f, float.MaxValue);
			RecalculateScaledValues();
		}
	}

	public Vector3 ScaledCenter => _scaledCenter;

	public float ScaledRadius => _scaledRadius;

	public float Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = Mathf.Clamp(value, 0f, MaxSpeed);
		}
	}

	public Steering[] Steerings => _steerings;

	public Vector3 Velocity => _transform.forward * _speed;

	protected void Awake()
	{
		_steerings = GetComponents<Steering>();
		_transform = GetComponent<Transform>();
		RecalculateScaledValues();
	}

	protected virtual void RegenerateLocalSpace(Vector3 newVelocity)
	{
		if (Speed > 0f && newVelocity.sqrMagnitude > MIN_FORCE_THRESHOLD)
		{
			Vector3 forward = newVelocity / Speed;
			forward.y = ((!IsPlanar) ? forward.y : _transform.forward.y);
			_transform.forward = forward;
		}
	}

	protected virtual Vector3 AdjustRawSteeringForce(Vector3 force)
	{
		return force;
	}

	protected void RecalculateScaledValues()
	{
		Vector3 lossyScale = _transform.lossyScale;
		_scaledRadius = _radius * Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
		_scaledCenter = Vector3.Scale(_center, lossyScale);
	}

	public virtual Vector3 PredictFuturePosition(float predictionTime)
	{
		return _transform.position + Velocity * predictionTime;
	}

	public bool IsInNeighborhood(Vehicle other, float minDistance, float maxDistance, float cosMaxAngle)
	{
		if (other == this)
		{
			return false;
		}
		Vector3 vector = other.Position - Position;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < minDistance * minDistance)
		{
			return true;
		}
		if (sqrMagnitude > maxDistance * maxDistance)
		{
			return false;
		}
		Vector3 rhs = vector / Mathf.Sqrt(sqrMagnitude);
		float num = Vector3.Dot(_transform.forward, rhs);
		return num > cosMaxAngle;
	}

	public Vector3 GetSeekVector(Vector3 target)
	{
		Vector3 result = Vector3.zero;
		float num = Vector3.Distance(Position, target);
		if (num > Radius)
		{
			result = target - Position - Velocity;
		}
		return result;
	}

	public Vector3 GetTargetSpeedVector(float targetSpeed)
	{
		float maxForce = MaxForce;
		float value = targetSpeed - Speed;
		return _transform.forward * Mathf.Clamp(value, 0f - maxForce, maxForce);
	}

	public float DistanceFromPerimeter(Vehicle other)
	{
		return (Position - other.Position).magnitude - ScaledRadius - other.ScaledRadius;
	}

	public void ResetOrientation()
	{
		_transform.up = Vector3.up;
		_transform.forward = Vector3.forward;
	}

	public float PredictNearestApproachTime(Vehicle other)
	{
		Vector3 velocity = Velocity;
		Vector3 velocity2 = other.Velocity;
		Vector3 vector = velocity2 - velocity;
		float magnitude = vector.magnitude;
		if (magnitude == 0f)
		{
			return 0f;
		}
		Vector3 lhs = vector / magnitude;
		Vector3 rhs = Position - other.Position;
		float num = Vector3.Dot(lhs, rhs);
		return num / magnitude;
	}

	public float ComputeNearestApproachPositions(Vehicle other, float time, ref Vector3 ourPosition, ref Vector3 hisPosition)
	{
		Vector3 vector = _transform.forward * Speed * time;
		Vector3 vector2 = other._transform.forward * other.Speed * time;
		ourPosition = Position + vector;
		hisPosition = other.Position + vector2;
		return Vector3.Distance(ourPosition, hisPosition);
	}

	private void OnDrawGizmos()
	{
		if (_drawGizmos)
		{
			if (_transform == null)
			{
				_transform = GetComponent<Transform>();
			}
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(Position, _scaledRadius);
		}
	}
}
