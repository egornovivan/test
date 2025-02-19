using UnityEngine;

[RequireComponent(typeof(PETracker))]
public abstract class PEMotor : MonoBehaviour
{
	public float maxForwardSpeed = 1.5f;

	public float maxBackwardsSpeed = 1.5f;

	public float maxSidewaysSpeed = 1.5f;

	public float maxVelocityChange = 0.2f;

	public float gravity = 10f;

	public bool canJump = true;

	public float jumpHeight = 1f;

	public Vector3 forwardVector = Vector3.forward;

	protected Quaternion alignCorrection;

	private bool m_Grounded;

	private bool m_Jumping;

	private PETracker m_Tracker;

	private Vector3 m_desiredMovementEffect;

	private Vector3 m_desiredMovementDirection;

	private Vector3 m_desiredFacingDirection;

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

	public bool jumping
	{
		get
		{
			return m_Jumping;
		}
		protected set
		{
			m_Jumping = value;
		}
	}

	public PETracker tracker => m_Tracker;

	public virtual Vector3 velocity => (!(m_Tracker != null)) ? Vector3.zero : m_Tracker.velocity;

	public virtual Vector3 angleVelocity => (!(m_Tracker != null)) ? Vector3.zero : m_Tracker.angularVelocity;

	public Vector3 desiredMovementEffect
	{
		get
		{
			return m_desiredMovementEffect;
		}
		set
		{
			m_desiredMovementEffect = value;
			if (m_desiredMovementEffect.magnitude > 10f)
			{
				m_desiredMovementEffect = m_desiredMovementEffect.normalized * 10f;
			}
		}
	}

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

	public Vector3 desiredFacingDirection
	{
		get
		{
			return m_desiredFacingDirection;
		}
		set
		{
			m_desiredFacingDirection = value;
			if (m_desiredFacingDirection.magnitude > 1f)
			{
				m_desiredFacingDirection = m_desiredFacingDirection.normalized;
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
			float num = ((maxSidewaysSpeed == 0f) ? 0f : (((!(m_desiredMovementDirection.z > 0f)) ? maxBackwardsSpeed : maxForwardSpeed) / maxSidewaysSpeed));
			Vector3 vector = new Vector3(m_desiredMovementDirection.x, m_desiredMovementDirection.y, (num == 0f) ? 0f : (m_desiredMovementDirection.z / num));
			Vector3 normalized = vector.normalized;
			float num2 = new Vector3(normalized.x, normalized.y, normalized.z * num).magnitude * maxSidewaysSpeed;
			Vector3 vector2 = m_desiredMovementDirection * num2;
			return base.transform.rotation * vector2;
		}
	}

	public virtual void Reset()
	{
	}

	public virtual void Stop()
	{
	}

	public void Start()
	{
		alignCorrection = default(Quaternion);
		alignCorrection.SetLookRotation(forwardVector, Vector3.up);
		alignCorrection = Quaternion.Inverse(alignCorrection);
		m_Tracker = GetComponent<PETracker>();
	}
}
