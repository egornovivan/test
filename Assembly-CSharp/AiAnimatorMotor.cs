using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AiAnimatorMotor : AiCharacterMotor
{
	public float timeScale = 1f;

	private float m_startStuckTime;

	private Animator animator;

	public override Vector3 velocity
	{
		get
		{
			if (animator != null)
			{
				return animator.deltaPosition;
			}
			return Vector3.zero;
		}
	}

	public override float radius => GetComponent<CharacterController>().radius;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		UpdateMove();
		UpdateFacingDirection();
		UpdateVelocity();
	}

	private void UpdateMove()
	{
		Vector3 movement = Vector3.zero;
		if (base.desiredMoveDestination != Vector3.zero)
		{
			Debug.DrawLine(base.transform.position, base.desiredMoveDestination, Color.red);
		}
		if (base.desiredMoveDestination != Vector3.zero && seeker != null)
		{
			movement = seeker.movement;
		}
		if (base.desiredMoveDestination != Vector3.zero && movement == Vector3.zero)
		{
			movement = base.desiredMoveDestination - base.transform.position;
		}
		if (base.desiredMoveDestination != Vector3.zero && AiUtil.SqrMagnitudeH(base.desiredMoveDestination - base.transform.position) < 1f)
		{
			movement = Vector3.zero;
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
			if (Vector3.Dot(base.transform.forward, v.normalized) > 0f)
			{
				animator.SetFloat("Direction", Vector3.Cross(base.transform.forward, v.normalized).y, 0f, Time.deltaTime);
			}
			else
			{
				animator.SetFloat("Direction", (Vector3.Cross(base.transform.forward, v.normalized).y > 0f) ? 1 : (-1), 0.25f, Time.deltaTime);
			}
		}
		else
		{
			animator.SetFloat("Direction", 0f, 0.25f, Time.deltaTime);
		}
	}

	private void UpdateVelocity()
	{
		if (animator == null)
		{
			return;
		}
		animator.speed = timeScale;
		if (base.desiredVelocity == Vector3.zero)
		{
			animator.SetFloat("Speed", 0f, 0f, Time.deltaTime);
			return;
		}
		float value = 1f;
		if (maxRunSpeed - maxWalkSpeed > float.Epsilon)
		{
			value = Mathf.Clamp((Mathf.Clamp(maxForwardSpeed, maxWalkSpeed, maxRunSpeed) - maxWalkSpeed) / (maxRunSpeed - maxWalkSpeed), 0.15f, 1f);
		}
		else
		{
			Debug.LogWarning(base.name + " maxRunSpeed[" + maxRunSpeed + "] not big than maxWalkSpeed[" + maxWalkSpeed + "].");
		}
		Vector3 vector = Util.ProjectOntoPlane(base.desiredVelocity, base.transform.up);
		if (Vector3.Dot(base.transform.forward, vector.normalized) > 0f)
		{
			animator.SetFloat("Speed", value, 0.25f, Time.deltaTime);
		}
		else if (base.desiredLookAtTran == null)
		{
			animator.SetFloat("Speed", 0f, 0.25f, Time.deltaTime);
		}
		else
		{
			animator.SetFloat("Speed", -1f, 0.25f, Time.deltaTime);
		}
	}
}
