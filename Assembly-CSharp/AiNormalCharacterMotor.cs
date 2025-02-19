using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AiNormalCharacterMotor : AiCharacterMotor
{
	private bool firstframe = true;

	private CharacterController controller;

	public override Vector3 velocity
	{
		get
		{
			if (controller != null)
			{
				return controller.velocity;
			}
			return Vector3.zero;
		}
	}

	public override float radius
	{
		get
		{
			if (controller == null)
			{
				return 0f;
			}
			return controller.radius;
		}
	}

	public override float height
	{
		get
		{
			if (controller == null)
			{
				return 0f;
			}
			return controller.height;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		firstframe = true;
		controller = GetComponent(typeof(CharacterController)) as CharacterController;
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
			Vector3 origin = base.transform.position + Vector3.up + Quaternion.AngleAxis((float)(360 * i) / 8f, Vector3.up) * (Vector3.right * 0.5f) + base.desiredVelocity * 0.2f;
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
		if (v.sqrMagnitude > 0.01f)
		{
			Vector3 v2 = Util.ConstantSlerp(base.transform.forward, v, maxRotationSpeed * Time.deltaTime);
			v2 = Util.ProjectOntoPlane(v2, base.transform.up);
			Quaternion rotation = default(Quaternion);
			rotation.SetLookRotation(v2, base.transform.up);
			base.transform.rotation = rotation;
		}
	}

	private void UpdateVelocity()
	{
		if (controller == null || !controller.enabled)
		{
			return;
		}
		Vector3 vector = controller.velocity;
		if (firstframe)
		{
			vector = Vector3.zero;
			firstframe = false;
		}
		if (base.grounded)
		{
			vector = Util.ProjectOntoPlane(vector, Vector3.up);
		}
		Vector3 vector2 = vector;
		base.jumping = false;
		if (base.grounded || gravity < float.Epsilon)
		{
			Vector3 vector3 = base.desiredVelocity - vector;
			if (vector3.magnitude > maxVelocityChange)
			{
				vector3 = vector3.normalized * maxVelocityChange;
			}
			vector2 += vector3;
			if (base.desiredVelocity == Vector3.zero)
			{
				vector2 = Vector3.zero;
			}
		}
		float num = 1f;
		AlignmentTracker component = GetComponent<AlignmentTracker>();
		if (component != null && Mathf.Abs(component.velocitySmoothed.y) > num)
		{
			vector2 *= Mathf.Max(0f, Mathf.Abs(num / component.velocitySmoothed.y));
		}
		Vector3 moveDeta = vector2 * Time.deltaTime;
		if (!CheckMoveDeta(moveDeta))
		{
			vector2 = Vector3.zero;
		}
		vector2 += Vector3.up * (0f - gravity) * Time.deltaTime;
		if (base.jumping)
		{
			vector2 -= Vector3.up * (0f - gravity) * Time.deltaTime / 2f;
		}
		CollisionFlags collisionFlags = controller.Move(vector2 * Time.deltaTime);
		base.grounded = (collisionFlags & CollisionFlags.Below) != 0;
	}

	protected virtual bool CheckMoveDeta(Vector3 moveDeta)
	{
		return true;
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

	private void Update()
	{
		if (useCentricGravity)
		{
			AdjustToGravity();
		}
		UpdateMove();
		UpdateFacingDirection();
		UpdateVelocity();
	}
}
