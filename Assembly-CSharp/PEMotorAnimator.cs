using Pathea;
using UnityEngine;

public class PEMotorAnimator : PEMotor
{
	public float maxRotationSpeed = 270f;

	private Animator animator;

	private Rigidbody rigid;

	private Vector3 deltaPosition;

	private Quaternion rootRotation;

	private Locomotion locomotion;

	private PeEntity m_Entity;

	public SpeedState speedState;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		rigid = GetComponent<Rigidbody>();
		locomotion = new Locomotion(animator);
	}

	public new void Start()
	{
		base.Start();
		m_Entity = GetComponentInParent<PeEntity>();
	}

	private void FixedUpdate()
	{
		if (Time.deltaTime != 0f && Time.timeScale != 0f)
		{
			UpdateVelocity();
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
			Vector3 vector = Vector3.Cross(base.transform.forward, v.normalized);
			animator.SetFloat("Direction", (vector.y > 0f) ? 1 : (-1), 0.15f, Time.deltaTime);
		}
		else
		{
			animator.SetFloat("Direction", 0f, 0.15f, Time.deltaTime);
		}
	}

	private void UpdateVelocity()
	{
		if (animator == null || m_Entity == null)
		{
			return;
		}
		bool applyRootMotion = base.desiredMovementEffect.sqrMagnitude < 0.0625f && (m_Entity.netCmpt == null || m_Entity.netCmpt.IsController);
		animator.applyRootMotion = applyRootMotion;
		if (base.desiredMovementEffect.sqrMagnitude < 0.0625f)
		{
			float speed = 0f;
			if (m_Entity.netCmpt == null || m_Entity.netCmpt.IsController)
			{
				if (base.desiredVelocity != Vector3.zero)
				{
					speed = ((speedState != SpeedState.Retreat) ? maxForwardSpeed : (0f - maxForwardSpeed));
				}
			}
			else if (velocity.sqrMagnitude > 0.010000001f)
			{
				Vector3 vector = Util.ProjectOntoPlane(velocity, base.transform.up);
				speed = ((!(Vector3.Dot(base.transform.forward, vector.normalized) > 0f) && !(Vector3.Angle(base.transform.forward, vector.normalized) < 165f)) ? (0f - maxForwardSpeed) : maxForwardSpeed);
			}
			float @float = animator.GetFloat("Angle");
			locomotion.Do(speed, @float);
		}
		else
		{
			if (base.desiredMovementEffect.sqrMagnitude < 0.80999994f && rigid.velocity.sqrMagnitude < 1f)
			{
				base.desiredMovementEffect = Vector3.zero;
			}
			Vector3 vector2 = rigid.velocity;
			if (base.grounded)
			{
				vector2 = Util.ProjectOntoPlane(vector2, base.transform.up);
			}
			Vector3 vector3 = base.desiredMovementEffect;
			Vector3 force = vector3 - vector2;
			rigid.AddForce(force, ForceMode.VelocityChange);
		}
	}

	private void OnCollisionStay()
	{
		base.grounded = true;
	}

	private void OnEnable()
	{
		animator.applyRootMotion = true;
	}

	private void OnDisable()
	{
		animator.applyRootMotion = false;
		rigid.AddForce(-rigid.velocity, ForceMode.VelocityChange);
	}
}
