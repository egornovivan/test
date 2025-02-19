using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AiPhysicsCharacterMotor : AiCharacterMotor
{
	public Vector3 gravityCenter = Vector3.zero;

	public override Vector3 velocity
	{
		get
		{
			if (GetComponent<Rigidbody>() != null)
			{
				return GetComponent<Rigidbody>().velocity;
			}
			return base.velocity;
		}
	}

	public override float radius
	{
		get
		{
			if (GetComponent<Rigidbody>().GetComponent<Collider>() != null)
			{
				Bounds bounds = GetComponent<Rigidbody>().GetComponent<Collider>().bounds;
				return Mathf.Max(bounds.extents.x, bounds.extents.z);
			}
			return base.radius;
		}
	}

	public override float height
	{
		get
		{
			if (GetComponent<Rigidbody>().GetComponent<Collider>() != null)
			{
				return GetComponent<Rigidbody>().GetComponent<Collider>().bounds.size.y;
			}
			return base.radius;
		}
	}

	private void Awake()
	{
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
	}

	private void AdjustToGravity()
	{
		int layer = base.gameObject.layer;
		base.gameObject.layer = 2;
		Vector3 up = base.transform.up;
		float num = Mathf.Clamp01(Time.deltaTime * 5f);
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < 8; i++)
		{
			Vector3 origin = base.transform.position + base.transform.up + Quaternion.AngleAxis((float)(360 * i) / 8f, base.transform.up) * (base.transform.right * 0.5f) + base.desiredVelocity * 0.2f;
			if (Physics.Raycast(origin, base.transform.up * -2f, out var hitInfo, 10f, groundLayers.value) && !(Mathf.Abs(hitInfo.point.y - base.transform.position.y) > 1f) && !(Vector3.Angle(hitInfo.normal, Vector3.up) > 45f))
			{
				zero += hitInfo.normal;
			}
		}
		zero = (up + zero).normalized;
		Vector3 normalized = (up + zero * num).normalized;
		float num2 = Vector3.Angle(up, normalized);
		if ((double)num2 > 0.01)
		{
			Vector3 normalized2 = Vector3.Cross(up, normalized).normalized;
			Quaternion quaternion = Quaternion.AngleAxis(num2, normalized2);
			base.transform.rotation = quaternion * base.transform.rotation;
		}
		base.gameObject.layer = layer;
	}

	private void UpdateFacingDirection()
	{
		float magnitude = base.desiredFacingDirection.magnitude;
		Vector3 v = base.transform.rotation * base.desiredMovementDirection * (1f - magnitude) + base.desiredFacingDirection * magnitude;
		v = Util.ProjectOntoPlane(v, base.transform.up);
		v = alignCorrection * v;
		if (v.sqrMagnitude > 0.1f)
		{
			Vector3 v2 = Util.ConstantSlerp(base.transform.forward, v, maxRotationSpeed * Time.deltaTime);
			v2 = Util.ProjectOntoPlane(v2, base.transform.up);
			Quaternion rotation = default(Quaternion);
			rotation.SetLookRotation(v2, base.transform.up);
			base.transform.rotation = rotation;
		}
	}

	protected virtual void UpdateVelocity()
	{
		Vector3 vector = GetComponent<Rigidbody>().velocity;
		if (base.grounded)
		{
			vector = Util.ProjectOntoPlane(vector, base.transform.up);
		}
		base.jumping = false;
		if (base.grounded || gravity < float.Epsilon)
		{
			Vector3 force = base.desiredVelocity - vector;
			if (force.magnitude > maxVelocityChange)
			{
				force = force.normalized * maxVelocityChange;
			}
			GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
		}
		GetComponent<Rigidbody>().AddForce(base.transform.up * (0f - gravity) * GetComponent<Rigidbody>().mass);
		base.grounded = false;
	}

	private void UpdateMove()
	{
		Vector3 movement = Vector3.zero;
		if (base.desiredMoveDestination != Vector3.zero && showGizmos)
		{
			Debug.DrawLine(base.transform.position, base.desiredMoveDestination, Color.red);
		}
		if (seeker != null && (seeker.followPathing || base.desiredMoveDestination != Vector3.zero))
		{
			movement = seeker.movement;
		}
		if (base.desiredMoveDestination != Vector3.zero && movement == Vector3.zero)
		{
			movement = base.desiredMoveDestination - base.transform.position;
		}
		DodgeNeighbours(ref movement);
		if (base.desiredLookAtTran != null)
		{
			base.desiredFacingDirection = (base.desiredLookAtTran.position - base.transform.position).normalized;
		}
		else if (base.desiredFaceDirection != Vector3.zero)
		{
			base.desiredFacingDirection = base.desiredFaceDirection.normalized;
		}
		else if (movement != Vector3.zero)
		{
			base.desiredFacingDirection = movement.normalized;
		}
		else
		{
			base.desiredFacingDirection = Vector3.zero;
		}
		if (movement != Vector3.zero && CheckMovementValid(movement))
		{
			if (habit == LifeArea.LA_Land && base.desiredFaceDirection == Vector3.zero && base.desiredLookAtTran == null)
			{
				float num = Vector3.Angle(base.transform.forward, Util.ProjectOntoPlane(movement, base.transform.up));
				if (num < 30f)
				{
					base.desiredMovementDirection = Quaternion.Inverse(base.transform.rotation) * movement;
				}
				else
				{
					base.desiredMovementDirection = Vector3.zero;
				}
			}
			else
			{
				base.desiredMovementDirection = Quaternion.Inverse(base.transform.rotation) * movement;
			}
		}
		else
		{
			base.desiredMovementDirection = Vector3.zero;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == 8)
		{
			GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
		}
	}

	private void OnCollisionStay()
	{
		base.grounded = true;
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.layer == 8)
		{
			GetComponent<Rigidbody>().constraints &= (RigidbodyConstraints)(-5);
		}
	}

	private void Update()
	{
		if (useCentricGravity)
		{
			AdjustToGravity();
		}
		UpdateMove();
		UpdateFacingDirection();
	}

	private void FixedUpdate()
	{
		UpdateVelocity();
	}
}
