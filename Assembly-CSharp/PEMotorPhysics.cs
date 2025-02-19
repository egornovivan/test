using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class PEMotorPhysics : PEMotor
{
	public float maxRotationSpeed = 270f;

	public float walkSmoothSpeed = 30f;

	public float runSmoothSpeed = 90f;

	public bool useCentricGravity;

	public LayerMask groundLayers;

	public Vector3 gravityCenter = Vector3.zero;

	private Animator animator;

	private Rigidbody rigid;

	private MovementPrison m_Prison;

	private PeEntity m_Entity;

	private HashSet<string> parameters;

	private bool animMoving;

	private bool m_AnimRotation;

	private Vector3 m_AnimPos = Vector3.zero;

	private Quaternion m_AnimRot = Quaternion.identity;

	private bool _bFixedUpdated;

	public override Vector3 velocity => (base.tracker != null) ? base.tracker.velocity : ((!(rigid != null)) ? Vector3.zero : rigid.velocity);

	public override Vector3 angleVelocity => (base.tracker != null) ? base.tracker.angularVelocity : ((!(rigid != null)) ? Vector3.zero : rigid.angularVelocity);

	public void Init(PeEntity entity)
	{
		m_Prison = new MovementPrison(entity, this, rigid);
	}

	private void Awake()
	{
		rigid = GetComponent<Rigidbody>();
		groundLayers = 6144;
		if (rigid != null)
		{
			rigid.freezeRotation = true;
			rigid.useGravity = false;
		}
		animator = GetComponentInChildren<Animator>();
		parameters = new HashSet<string>();
		if (animator != null)
		{
			AnimatorControllerParameter[] array = animator.parameters;
			for (int i = 0; i < array.Length; i++)
			{
				parameters.Add(array[i].name);
			}
		}
	}

	public new void Start()
	{
		base.Start();
		m_Entity = GetComponentInParent<PeEntity>();
	}

	private void SetFloat(string name, float value)
	{
		if (animator != null && (parameters == null || parameters.Contains(name)))
		{
			animator.SetFloat(name, value);
		}
	}

	private void SetFloat(string name, float value, float dampTime, float deltaTime)
	{
		if (animator != null && (parameters == null || parameters.Contains(name)))
		{
			animator.SetFloat(name, value, dampTime, deltaTime);
		}
	}

	private Vector3 GetAdjuestedUp()
	{
		Vector3 zero = Vector3.zero;
		if (m_Entity != null)
		{
			Bounds bounds = m_Entity.bounds;
			Vector3[] array = new Vector3[8];
			for (int i = 0; i < 8; i++)
			{
				if ((i & 4) == 0)
				{
					if ((i & 1) == 0)
					{
						array[i] -= bounds.extents.x * new Vector3(1f, 0f, 0f);
					}
					else
					{
						array[i] += bounds.extents.x * new Vector3(1f, 0f, 0f);
					}
					if ((i & 2) == 0)
					{
						array[i] -= bounds.extents.z * new Vector3(0f, 0f, 1f);
					}
					else
					{
						array[i] += bounds.extents.z * new Vector3(0f, 0f, 1f);
					}
				}
				else if ((i & 2) == 0)
				{
					if ((i & 1) == 0)
					{
						array[i] -= bounds.extents.x * new Vector3(1f, 0f, 0f);
					}
					else
					{
						array[i] += bounds.extents.x * new Vector3(1f, 0f, 0f);
					}
				}
				else if ((i & 1) == 0)
				{
					array[i] -= bounds.extents.z * new Vector3(0f, 0f, 1f);
				}
				else
				{
					array[i] += bounds.extents.z * new Vector3(0f, 0f, 1f);
				}
				Vector3 origin = base.transform.TransformPoint(array[i]);
				if (Physics.Raycast(origin, -base.transform.up, out var hitInfo, 5f, groundLayers.value) && Vector3.Angle(hitInfo.normal, Vector3.up) < 75f)
				{
					zero += hitInfo.normal;
				}
			}
		}
		return zero;
	}

	private void AdjustToGravity()
	{
		int layer = base.gameObject.layer;
		base.gameObject.layer = 2;
		Vector3 up = base.transform.up;
		float num = Mathf.Clamp01(Time.deltaTime * 5f);
		Vector3 adjuestedUp = GetAdjuestedUp();
		adjuestedUp = (up + adjuestedUp).normalized;
		Vector3 normalized = (up + adjuestedUp * num).normalized;
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
		if (m_Entity != null && m_Entity.netCmpt != null && !m_Entity.netCmpt.IsController)
		{
			SetFloat("Direction", angleVelocity.y, 0.25f, Time.deltaTime);
			return;
		}
		if (m_AnimRotation)
		{
			base.transform.rotation = m_AnimRot;
			SetFloat("Direction", 0f);
			return;
		}
		float magnitude = base.desiredFacingDirection.magnitude;
		Vector3 v = base.transform.rotation * base.desiredMovementDirection * (1f - magnitude) + base.desiredFacingDirection * magnitude;
		v = Util.ProjectOntoPlane(v, base.transform.up);
		v = alignCorrection * v;
		if (v.sqrMagnitude > 0.0001f)
		{
			Vector3 v2 = Util.ConstantSlerp(base.transform.forward, v, maxRotationSpeed * Time.deltaTime);
			v2 = Util.ProjectOntoPlane(v2, base.transform.up);
			Quaternion rotation = default(Quaternion);
			rotation.SetLookRotation(v2, base.transform.up);
			base.transform.rotation = rotation;
			float num = Vector3.Angle(base.transform.forward, v.normalized) / 135f;
			if (Vector3.Cross(base.transform.forward, v.normalized).y > 0f)
			{
				SetFloat("Direction", num, 0.25f, Time.deltaTime);
			}
			else
			{
				SetFloat("Direction", 0f - num, 0.25f, Time.deltaTime);
			}
		}
		else
		{
			SetFloat("Direction", 0f, 0.25f, Time.deltaTime);
		}
	}

	private void UpdateVelocity()
	{
		if (m_Entity != null && m_Entity.netCmpt != null && !m_Entity.netCmpt.IsController)
		{
			Vector3 vector = Util.ProjectOntoPlane(velocity, base.transform.up);
			if (velocity.sqrMagnitude <= 0.010000001f)
			{
				SetFloat("Speed", 0f);
			}
			else
			{
				float num = PEUtil.Magnitude(velocity, is3D: false);
				if (Vector3.Dot(base.transform.forward, vector.normalized) > 0f || Vector3.Angle(base.transform.forward, vector.normalized) < 150f)
				{
					SetFloat("Speed", num, 0.15f, Time.deltaTime);
				}
				else
				{
					SetFloat("Speed", 0f - num, 0.15f, Time.deltaTime);
				}
			}
		}
		else if (m_AnimPos != Vector3.zero)
		{
			Vector3 force = m_AnimPos - base.transform.position;
			if (m_Prison == null || m_Prison.CalculateVelocity(ref force))
			{
				rigid.AddForce(force, ForceMode.VelocityChange);
			}
			else
			{
				rigid.velocity = Vector3.zero;
			}
			SetFloat("Speed", 0f);
		}
		else
		{
			Vector3 vector2 = rigid.velocity;
			if (base.grounded)
			{
				vector2 = Util.ProjectOntoPlane(vector2, base.transform.up);
			}
			base.jumping = false;
			if (base.grounded || Mathf.Abs(gravity) < float.Epsilon)
			{
				Vector3 zero = base.desiredVelocity;
				if (base.desiredMovementEffect.sqrMagnitude > 0.010000001f)
				{
					zero = base.desiredMovementEffect;
				}
				if (m_Prison != null && !m_Prison.CalculateVelocity(ref zero))
				{
					zero = Vector3.zero;
				}
				Vector3 force2 = zero - vector2;
				if (force2.magnitude > maxVelocityChange && base.desiredMovementEffect.sqrMagnitude < 0.010000001f)
				{
					force2 = force2.normalized * maxVelocityChange;
				}
				if (gravity > float.Epsilon && force2.y > 0.2f)
				{
					force2 = new Vector3(force2.x, 0.2f, force2.z);
				}
				rigid.AddForce(force2, ForceMode.VelocityChange);
			}
			else if (base.desiredVelocity == Vector3.zero && gravity > float.Epsilon)
			{
				Vector3 vector3 = vector2;
				vector3.y = 0f;
				rigid.AddForce(-vector3, ForceMode.VelocityChange);
			}
			rigid.AddForce(base.transform.up * (0f - gravity) * rigid.mass);
			Vector3 vector4 = Util.ProjectOntoPlane(rigid.velocity, base.transform.up);
			if (rigid.velocity.sqrMagnitude <= 0.010000001f)
			{
				SetFloat("Speed", 0f);
			}
			else
			{
				float num2 = PEUtil.Magnitude(rigid.velocity, is3D: false);
				if (Vector3.Dot(base.transform.forward, vector4.normalized) > 0f || Vector3.Angle(base.transform.forward, vector4.normalized) < 150f)
				{
					SetFloat("Speed", num2, 0.15f, Time.deltaTime);
				}
				else
				{
					SetFloat("Speed", 0f - num2, 0.15f, Time.deltaTime);
				}
			}
		}
		base.grounded = false;
	}

	private void OnCollisionStay()
	{
		base.grounded = true;
	}

	private void FixedUpdate()
	{
		if (!_bFixedUpdated)
		{
			_bFixedUpdated = true;
			UpdateVelocity();
		}
	}

	private void Update()
	{
		_bFixedUpdated = false;
		if (useCentricGravity)
		{
			AdjustToGravity();
		}
		UpdateFacingDirection();
	}

	public void OnAnimatorMove()
	{
		if (rigid == null)
		{
			return;
		}
		Vector3 deltaPosition = animator.deltaPosition;
		if (deltaPosition.sqrMagnitude > 0.010000001f)
		{
			if (m_AnimPos == Vector3.zero)
			{
				rigid.velocity = Vector3.zero;
			}
			m_AnimPos = base.transform.position + deltaPosition;
		}
		else
		{
			if (m_AnimPos != Vector3.zero)
			{
				rigid.velocity = Vector3.zero;
			}
			m_AnimPos = Vector3.zero;
		}
		float x = animator.deltaRotation.x;
		float y = animator.deltaRotation.y;
		float z = animator.deltaRotation.z;
		float num = x * x + y * y + z * z;
		if (num > 0.0001f)
		{
			m_AnimRotation = true;
			m_AnimRot = animator.deltaRotation * base.transform.rotation;
		}
		else
		{
			m_AnimRotation = false;
			m_AnimRot = Quaternion.identity;
		}
	}

	public override void Stop()
	{
		base.Stop();
		if (rigid != null)
		{
			rigid.velocity = Vector3.zero;
		}
	}

	public override void Reset()
	{
		m_AnimPos = Vector3.zero;
		m_AnimRot = Quaternion.identity;
	}
}
