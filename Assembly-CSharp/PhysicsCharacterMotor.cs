using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PhysicsCharacterMotor : MonoBehaviour
{
	[HideInInspector]
	public float maxForwardSpeed = 25f;

	[HideInInspector]
	public float maxBackwardsSpeed = 15f;

	[HideInInspector]
	public float maxSidewaysSpeed = 18f;

	private float maxVelocityChange = 10f;

	public float gravity = 10f;

	private float mImpactTime;

	public bool mInWater;

	public float mDisToWater;

	private float FluidDragF = 0.16f;

	private float AreaDragF = 0.006f;

	private float MoveAcc = 40f;

	public float SpeedReduce = 0.1f;

	public bool ExStop;

	private float MaxMoveAngle = 80f;

	private Vector3 forwardVector = Vector3.forward;

	protected Quaternion alignCorrection;

	private bool m_Grounded;

	private bool mFreezeGravity = true;

	public bool mGliderMode;

	public bool mJumpFlag;

	private Vector3 m_desiredMovementDirection;

	private Vector3 m_desiredFacingDirection;

	public Vector3 Dir = Vector3.zero;

	public bool FreezeGravity
	{
		get
		{
			return mFreezeGravity;
		}
		set
		{
			mFreezeGravity = value;
		}
	}

	public bool grounded
	{
		get
		{
			return m_Grounded;
		}
		protected set
		{
			m_Grounded = value;
		}
	}

	public Vector3 Velocity => GetComponent<Rigidbody>().velocity;

	public Vector3 desiredMovementDirection
	{
		get
		{
			return m_desiredMovementDirection;
		}
		set
		{
			m_desiredMovementDirection = value;
			if (m_desiredMovementDirection.magnitude > 1f)
			{
				m_desiredMovementDirection = m_desiredMovementDirection.normalized;
			}
		}
	}

	public Vector3 desiredVelocity
	{
		get
		{
			if (m_desiredMovementDirection == Vector3.zero)
			{
				return Vector3.zero;
			}
			float num = ((!(m_desiredMovementDirection.z > 0f)) ? maxBackwardsSpeed : maxForwardSpeed) / maxSidewaysSpeed;
			Vector3 normalized = new Vector3(m_desiredMovementDirection.x, 0f, m_desiredMovementDirection.z / num).normalized;
			float num2 = new Vector3(normalized.x, 0f, normalized.z * num).magnitude * maxSidewaysSpeed;
			Vector3 vector = m_desiredMovementDirection * num2;
			return base.transform.rotation * vector;
		}
	}

	public void ResetSpeed(float mspeed)
	{
		maxForwardSpeed = mspeed;
		maxBackwardsSpeed = maxForwardSpeed * 0.75f;
		maxSidewaysSpeed = maxForwardSpeed * 0.95f;
		maxVelocityChange = maxForwardSpeed * 0.3f;
	}

	private void Awake()
	{
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
	}

	public void ApplyImpact(Vector3 speedImpact)
	{
		GetComponent<Rigidbody>().AddForce(speedImpact, ForceMode.VelocityChange);
		mImpactTime = Time.time;
		grounded = false;
	}

	private void UpdateVelocity()
	{
		if (GetComponent<Rigidbody>().isKinematic)
		{
			return;
		}
		if (!mGliderMode)
		{
			Vector3 vector = GetComponent<Rigidbody>().velocity;
			if (grounded)
			{
				vector = Util.ProjectOntoPlane(vector, base.transform.up);
			}
			Vector3 vector2 = desiredVelocity;
			Vector3 vector3 = vector2 - vector;
			Vector3 vector4 = Vector3.zero;
			if (vector2.sqrMagnitude > float.Epsilon)
			{
				if (vector3.sqrMagnitude > float.Epsilon)
				{
					if (!mInWater)
					{
						vector3.y = 0f;
					}
					vector4 = vector3.normalized;
				}
			}
			else if (grounded && ExStop)
			{
				Vector3 velocity = GetComponent<Rigidbody>().velocity;
				velocity.y = 0f;
				if (velocity.sqrMagnitude > float.Epsilon)
				{
					GetComponent<Rigidbody>().AddForce((0f - SpeedReduce) * velocity, ForceMode.VelocityChange);
				}
			}
			GetComponent<Rigidbody>().AddForce(vector4 * MoveAcc, ForceMode.Acceleration);
			if (mInWater)
			{
				GetComponent<Rigidbody>().AddForce((0f - FluidDragF) * GetComponent<Rigidbody>().velocity.sqrMagnitude * GetComponent<Rigidbody>().velocity.normalized, ForceMode.Acceleration);
			}
			else
			{
				GetComponent<Rigidbody>().AddForce((0f - AreaDragF) * GetComponent<Rigidbody>().velocity.sqrMagnitude * GetComponent<Rigidbody>().velocity.normalized, ForceMode.Acceleration);
			}
			GetComponent<Rigidbody>().AddForce(gravity * Vector3.down, ForceMode.Acceleration);
			if (base.transform.position.y < -10f)
			{
				base.transform.position = new Vector3(base.transform.position.x, -10f, base.transform.position.z);
			}
		}
		grounded = false;
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		if (Time.time - mImpactTime > 0.1f)
		{
			grounded = true;
		}
		if (!GetComponent<Rigidbody>().isKinematic && collisionInfo.gameObject.layer == 8)
		{
			GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.y);
		}
	}

	private void FixedUpdate()
	{
		UpdateVelocity();
		GetComponent<Rigidbody>().AddForce(50f * Dir, ForceMode.Acceleration);
	}
}
