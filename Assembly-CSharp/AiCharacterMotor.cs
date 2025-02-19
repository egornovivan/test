using System;
using System.Collections;
using UnityEngine;

public abstract class AiCharacterMotor : MonoBehaviour
{
	[NonSerialized]
	public float maxWalkSpeed = 1.5f;

	[NonSerialized]
	public float maxRunSpeed = 2f;

	public float maxForwardSpeed = 1.5f;

	[NonSerialized]
	public float maxBackwardsSpeed = 1.5f;

	[NonSerialized]
	public float maxSidewaysSpeed = 1.5f;

	[NonSerialized]
	public float maxVelocityChange = 100f;

	[NonSerialized]
	public float maxRotationSpeed = 270f;

	[HideInInspector]
	public LayerMask groundLayers;

	public bool showGizmos;

	public LifeArea habit;

	public float gravity = 10f;

	public bool canMove = true;

	public bool canJump = true;

	public float jumpHeight = 1f;

	public bool useCentricGravity;

	protected Vector3 forwardVector = Vector3.forward;

	protected Quaternion alignCorrection;

	protected AiSeeker seeker;

	private AiObject aiObject;

	private float mTurnAngle;

	private Vector3 m_destination;

	private Vector3 m_faceDirection = Vector3.zero;

	private Transform m_LooAtTran;

	private bool m_jump;

	private bool m_stuck;

	private bool m_Grounded;

	private bool m_Jumping;

	private Vector3 m_desiredMovementDirection;

	private Vector3 m_desiredFacingDirection;

	private Vector3 m_lastPosition;

	private Quaternion m_lastRotation;

	public Vector3 desiredMoveDestination
	{
		get
		{
			return m_destination;
		}
		set
		{
			m_destination = value;
		}
	}

	public Vector3 desiredFaceDirection
	{
		get
		{
			return m_faceDirection;
		}
		set
		{
			m_faceDirection = value;
			if (m_faceDirection.magnitude > 1f)
			{
				m_faceDirection = m_faceDirection.normalized;
			}
		}
	}

	public Transform desiredLookAtTran
	{
		get
		{
			return m_LooAtTran;
		}
		set
		{
			m_LooAtTran = value;
		}
	}

	public bool jump
	{
		get
		{
			return m_jump;
		}
		set
		{
			m_jump = value;
		}
	}

	public virtual Vector3 velocity => Vector3.zero;

	public virtual float radius => 0f;

	public virtual float height => 0f;

	public bool stucking => m_stuck;

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
			if (aiObject != null && !aiObject.CanRotate())
			{
				return Vector3.zero;
			}
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
			if (m_desiredMovementDirection == Vector3.zero || !canMove || (aiObject != null && !aiObject.CanMove()))
			{
				return Vector3.zero;
			}
			float num = ((!(m_desiredMovementDirection.z > 0f)) ? maxBackwardsSpeed : maxForwardSpeed) / maxSidewaysSpeed;
			float y = ((!(gravity > 0f)) ? m_desiredMovementDirection.y : 0f);
			Vector3 normalized = new Vector3(m_desiredMovementDirection.x, y, m_desiredMovementDirection.z / num).normalized;
			float num2 = new Vector3(normalized.x, normalized.y, normalized.z * num).magnitude * maxSidewaysSpeed;
			Vector3 vector = m_desiredMovementDirection;
			if (gravity > float.Epsilon)
			{
				vector.y = 0f;
			}
			vector = vector.normalized * num2;
			return base.transform.rotation * vector;
		}
	}

	private void Clear()
	{
		desiredLookAtTran = null;
		desiredMoveDestination = Vector3.zero;
		desiredMovementDirection = Vector3.zero;
	}

	public void FollowPath(Vector3[] paths)
	{
		if (seeker != null)
		{
			seeker.SetFollowPath(paths);
		}
	}

	public void ClearPath()
	{
		if (seeker != null)
		{
			seeker.ClearFollowPath();
		}
	}

	private IEnumerator Stuck()
	{
		float time = 0f;
		while (true)
		{
			float sqrDis = (base.transform.position - m_lastPosition).sqrMagnitude;
			float ang = Quaternion.Angle(m_lastRotation, base.transform.rotation);
			if ((double)desiredVelocity.sqrMagnitude < 0.02250000089406967 || sqrDis > 0.0225f || ang > 1f)
			{
				m_stuck = false;
				time = Time.time;
			}
			else if (Time.time - time > 5f)
			{
				m_stuck = true;
				Debug.LogWarning(((!(aiObject != null)) ? string.Empty : aiObject.name) + " --> stucking!!");
			}
			m_lastPosition = base.transform.position;
			m_lastRotation = base.transform.rotation;
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator PathFinding()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
		}
	}

	protected void DodgeNeighbours(ref Vector3 movement)
	{
		if (!(aiObject == null) && !(movement == Vector3.zero))
		{
			movement = Quaternion.AngleAxis(mTurnAngle, Vector3.up) * movement;
		}
	}

	private bool CheckMovementValidLand(Vector3 movement)
	{
		float num = ((!(this is AiPhysicsCharacterMotor)) ? Time.deltaTime : Time.fixedDeltaTime);
		Vector3 vector = movement;
		vector.y = 0f;
		Vector3 vector2 = vector.normalized * maxForwardSpeed * num * 10f;
		Vector3 v = ((!(aiObject != null)) ? base.transform.position : aiObject.center);
		v += base.transform.forward * radius;
		v += vector2;
		if (AiUtil.CheckPositionUnderWater(v))
		{
			return false;
		}
		return true;
	}

	private bool CheckMovemenValidtWater(Vector3 movement)
	{
		return false;
	}

	private bool CheckMovementValidSky(Vector3 movement)
	{
		return true;
	}

	public virtual bool CheckMovementValid(Vector3 movement)
	{
		return habit switch
		{
			LifeArea.LA_None => true, 
			LifeArea.LA_Land => CheckMovementValidLand(movement), 
			LifeArea.LA_Water => CheckMovemenValidtWater(movement), 
			LifeArea.LA_Sky => CheckMovementValidSky(movement), 
			LifeArea.LA_Max => true, 
			_ => true, 
		};
	}

	protected virtual void OnEnable()
	{
		StopAllCoroutines();
		StartCoroutine(Stuck());
		StartCoroutine(PathFinding());
	}

	protected virtual void Start()
	{
		seeker = GetComponentInChildren<AiSeeker>();
		aiObject = GetComponent<AiObject>();
		alignCorrection = default(Quaternion);
		alignCorrection.SetLookRotation(forwardVector, Vector3.up);
		alignCorrection = Quaternion.Inverse(alignCorrection);
		if (habit == LifeArea.LA_Water || habit == LifeArea.LA_Sky)
		{
			gravity = 0f;
		}
	}
}
