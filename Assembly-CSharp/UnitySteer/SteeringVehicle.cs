using UnityEngine;

namespace UnitySteer;

public class SteeringVehicle
{
	private Transform transform;

	private Rigidbody rigidbody;

	private float internalMass;

	private float radius;

	private float speed;

	private float maxSpeed;

	private float maxForce;

	private bool movesVertically = true;

	private Vector3 internalPosition;

	private Vector3 internalSide;

	private Vector3 internalForward;

	private Vector3 internalUp;

	public Vector3 Position
	{
		get
		{
			if (rigidbody != null)
			{
				return rigidbody.position;
			}
			if (transform != null)
			{
				return transform.position;
			}
			return internalPosition;
		}
		set
		{
			if (!MovesVertically)
			{
				value.y = Position.y;
			}
			if (rigidbody != null)
			{
				rigidbody.MovePosition(value);
			}
			else if (transform != null)
			{
				transform.position = value;
			}
			else
			{
				internalPosition = value;
			}
		}
	}

	public Vector3 Forward
	{
		get
		{
			if (rigidbody != null)
			{
				return rigidbody.transform.forward;
			}
			if (transform != null)
			{
				return transform.forward;
			}
			return internalForward;
		}
		set
		{
			if (!MovesVertically)
			{
				value = new Vector3(value.x, Forward.y, value.z);
			}
			if (rigidbody != null)
			{
				rigidbody.transform.forward = value;
				return;
			}
			if (transform != null)
			{
				transform.forward = value;
				return;
			}
			internalForward = value;
			RecalculateSide();
		}
	}

	public Vector3 Side
	{
		get
		{
			if (rigidbody != null)
			{
				return rigidbody.transform.right;
			}
			if (transform != null)
			{
				return transform.right;
			}
			return internalSide;
		}
	}

	public Vector3 Up
	{
		get
		{
			if (rigidbody != null)
			{
				return rigidbody.transform.up;
			}
			if (transform != null)
			{
				return transform.up;
			}
			return internalUp;
		}
		set
		{
			if (rigidbody != null)
			{
				rigidbody.transform.up = value;
				return;
			}
			if (transform != null)
			{
				transform.up = value;
				return;
			}
			internalUp = value;
			RecalculateSide();
		}
	}

	public float Mass
	{
		get
		{
			if (rigidbody != null)
			{
				return rigidbody.mass;
			}
			return internalMass;
		}
		set
		{
			if (rigidbody != null)
			{
				rigidbody.mass = value;
			}
			else
			{
				internalMass = value;
			}
		}
	}

	public float Speed
	{
		get
		{
			return speed;
		}
		set
		{
			speed = value;
		}
	}

	public float MaxSpeed
	{
		get
		{
			return maxSpeed;
		}
		set
		{
			maxSpeed = value;
		}
	}

	public float MaxForce
	{
		get
		{
			return maxForce;
		}
		set
		{
			maxForce = value;
		}
	}

	public bool MovesVertically
	{
		get
		{
			return movesVertically;
		}
		set
		{
			movesVertically = value;
		}
	}

	public Vector3 Velocity => Forward * speed;

	public float Radius
	{
		get
		{
			return radius;
		}
		set
		{
			radius = value;
		}
	}

	protected Transform Transform => (!(rigidbody != null)) ? transform : rigidbody.transform;

	protected GameObject GameObject => (!(rigidbody != null)) ? transform.gameObject : rigidbody.gameObject;

	public SteeringVehicle(Vector3 position, float mass)
	{
		Position = position;
		internalMass = mass;
	}

	public SteeringVehicle(Transform transform, float mass)
	{
		this.transform = transform;
		internalMass = mass;
	}

	public SteeringVehicle(Rigidbody rigidbody)
	{
		this.rigidbody = rigidbody;
	}

	public virtual Vector3 predictFuturePosition(float predictionTime)
	{
		return Vector3.zero;
	}

	private void RecalculateSide()
	{
		internalSide = Vector3.Cross(internalForward, internalUp);
		internalSide.Normalize();
	}
}
