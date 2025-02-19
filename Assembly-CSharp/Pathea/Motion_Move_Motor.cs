using Pathea.Operate;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class Motion_Move_Motor : Motion_Move, IPeMsg
{
	private static string ground = string.Empty;

	private static string water = string.Empty;

	private static string waterSurface = string.Empty;

	private static string air = string.Empty;

	private static Keyframe[] NearMoveVelocity = new Keyframe[3]
	{
		new Keyframe(0f, 4f),
		new Keyframe(220f, 4f),
		new Keyframe(700f, 8f)
	};

	private static Keyframe[] LongMoveVelocity = new Keyframe[3]
	{
		new Keyframe(0f, 4f),
		new Keyframe(220f, 4f),
		new Keyframe(700f, 15f)
	};

	private static Keyframe[] NearMoveTime = new Keyframe[3]
	{
		new Keyframe(0f, 0.25f),
		new Keyframe(220f, 0.25f),
		new Keyframe(700f, 0.6f)
	};

	private static Keyframe[] LongMoveTime = new Keyframe[3]
	{
		new Keyframe(0f, 0.25f),
		new Keyframe(220f, 0.25f),
		new Keyframe(700f, 0.7f)
	};

	private static Keyframe[] MoveStopTime = new Keyframe[1]
	{
		new Keyframe(0f, 4f)
	};

	private static Keyframe[] MoveWentflyTime = new Keyframe[1]
	{
		new Keyframe(0f, 5f)
	};

	private float m_gravity = -1f;

	private MovementField m_Field;

	private bool m_CanMove = true;

	private Vector3 m_MoveDirection;

	private Vector3 m_MoveDestination;

	private Vector3 m_RotDirection;

	private PEMotor m_Motor;

	private PEPathfinder m_Path;

	private PeTrans m_PeTrans;

	private AnimatorCmpt m_Animator;

	private SkAliveEntity m_Attribute;

	private TargetCmpt m_target;

	private BeatParam m_Param;

	private float m_Speed;

	private float m_CurrentSpeed;

	private Vector3 m_CurMovement;

	private Vector3 m_CurMovementDirection;

	private Vector3 m_CurFaceDirection;

	private Vector3 m_CurMoveDestination;

	private Vector3 m_CurAvoidDirection;

	private float m_LastMoveTime;

	private float m_SpeedTime;

	private float m_SpeedScale;

	private bool m_Proxy;

	private Vector3 m_NetPos;

	private Vector3 m_NetRot;

	private static readonly int layer = 2163968;

	private static readonly int voxelLayer = 2177024;

	public MovementField Field
	{
		get
		{
			return m_Field;
		}
		set
		{
			m_Field = value;
		}
	}

	public float CurrentSpeed => m_CurrentSpeed;

	public PEMotor motor => m_Motor;

	public override MovementState state
	{
		get
		{
			return m_State;
		}
		set
		{
			if (m_State != value)
			{
				if (m_State != 0)
				{
					OnMovementStateExit(m_State);
				}
				m_State = value;
				if (m_State != 0)
				{
					OnMovementStateEnter(m_State);
				}
			}
		}
	}

	public override Vector3 velocity => (!(m_Motor != null)) ? Vector3.zero : m_Motor.velocity;

	public override Vector3 movement => m_CurMovement;

	public override float gravity
	{
		get
		{
			if (m_Motor != null)
			{
				m_gravity = m_Motor.gravity;
			}
			return m_gravity;
		}
		set
		{
			if (m_Motor != null)
			{
				m_Motor.gravity = value;
			}
			m_gravity = value;
		}
	}

	public override bool grounded => m_Motor != null && m_Motor.grounded;

	public override SpeedState speed
	{
		set
		{
			if (m_SpeedState == value)
			{
				return;
			}
			m_SpeedState = value;
			if (m_Attribute != null)
			{
				switch (m_SpeedState)
				{
				case SpeedState.None:
					m_Speed = 0f;
					break;
				case SpeedState.Walk:
					m_Speed = m_Attribute.GetAttribute(AttribType.WalkSpeed);
					break;
				case SpeedState.Run:
					m_Speed = m_Attribute.GetAttribute(AttribType.RunSpeed);
					break;
				case SpeedState.Sprint:
					m_Speed = m_Attribute.GetAttribute(AttribType.SprintSpeed);
					break;
				case SpeedState.Retreat:
					m_Speed = m_Attribute.GetAttribute(AttribType.WalkSpeed);
					break;
				default:
					m_Speed = 0f;
					break;
				}
			}
			if (m_Motor != null && m_Motor is PEMotorAnimator)
			{
				(m_Motor as PEMotorAnimator).speedState = m_SpeedState;
			}
		}
	}

	public override void Stop()
	{
		m_MoveDestination = Vector3.zero;
		m_MoveDirection = Vector3.zero;
		m_RotDirection = Vector3.zero;
		if (m_Motor != null)
		{
			m_Motor.Stop();
		}
	}

	private bool CanMove()
	{
		return m_CanMove && !base.Entity.isRagdoll;
	}

	private void OnMovementStateEnter(MovementState state)
	{
		switch (state)
		{
		case MovementState.None:
			break;
		case MovementState.Ground:
			m_Animator.SetBool(ground, value: true);
			break;
		case MovementState.Water:
			m_Animator.SetBool(water, value: true);
			break;
		case MovementState.WaterSurface:
			m_Animator.SetBool(waterSurface, value: true);
			break;
		case MovementState.Air:
			m_Animator.SetBool(air, value: true);
			break;
		}
	}

	private void OnMovementStateTick(MovementState state)
	{
	}

	private void OnMovementStateExit(MovementState state)
	{
		switch (state)
		{
		case MovementState.None:
			break;
		case MovementState.Ground:
			break;
		case MovementState.Water:
			break;
		case MovementState.WaterSurface:
			break;
		case MovementState.Air:
			break;
		}
	}

	private void UpdateMovementState()
	{
		if (VFVoxelWater.self != null && VFVoxelWater.self.IsInWater(m_PeTrans.headTop))
		{
			state = MovementState.Water;
		}
		else if (VFVoxelWater.self != null && VFVoxelWater.self.IsInWater(m_PeTrans.position))
		{
			state = MovementState.WaterSurface;
		}
		else if (m_Motor != null)
		{
			if (m_Motor.grounded)
			{
				state = MovementState.Ground;
			}
			else
			{
				state = MovementState.Air;
			}
		}
		if (state != 0)
		{
			OnMovementStateTick(m_State);
		}
	}

	private void UpdateRotation()
	{
		if (m_Motor == null)
		{
			return;
		}
		if (m_CurFaceDirection != Vector3.zero)
		{
			m_Motor.desiredFacingDirection = m_CurFaceDirection.normalized;
		}
		else if (m_CurMovementDirection != Vector3.zero)
		{
			m_Motor.desiredFacingDirection = m_CurMovementDirection.normalized;
		}
		else
		{
			m_Motor.desiredFacingDirection = Vector3.zero;
		}
		Vector3 desiredFacingDirection = m_Motor.desiredFacingDirection;
		Vector3 v = Quaternion.Inverse(m_Motor.transform.rotation) * desiredFacingDirection;
		if (PEUtil.SqrMagnitudeH(v) > 0.0025000002f)
		{
			m_Animator.SetFloat("Angle", Mathf.Atan2(v.x, v.z) * 180f / 3.14159f);
		}
		else
		{
			m_Animator.SetFloat("Angle", 0f);
		}
		PEMotorPhysics pEMotorPhysics = m_Motor as PEMotorPhysics;
		if (pEMotorPhysics != null)
		{
			float num = m_Attribute.GetAttribute(AttribType.RotationSpeed);
			if (pEMotorPhysics.velocity.sqrMagnitude > 0.0625f)
			{
				num = ((m_SpeedState != SpeedState.Walk) ? pEMotorPhysics.runSmoothSpeed : pEMotorPhysics.walkSmoothSpeed);
			}
			if (m_Animator != null)
			{
				float @float = m_Animator.GetFloat("RotationSpeed");
				num = ((!(@float > 0f)) ? num : @float);
			}
			pEMotorPhysics.maxRotationSpeed = num;
		}
	}

	private void CalculateDestination()
	{
		m_CurMoveDestination = m_MoveDestination;
		if (m_CurMoveDestination != Vector3.zero && m_Motor != null)
		{
			float num = PEUtil.SqrMagnitude(m_Motor.transform.position, m_CurMoveDestination, m_Motor.gravity < float.Epsilon);
			Enemy enemy = ((!(m_target != null)) ? null : m_target.GetAttackEnemy());
			if (enemy != null && PEUtil.SqrMagnitude(m_CurMoveDestination, enemy.position, is3D: false) < 0.0625f)
			{
				num = ((!(m_Motor.gravity < float.Epsilon)) ? enemy.SqrDistanceXZ : enemy.SqrDistance);
			}
			if (num < 0.010000001f)
			{
				m_CurMoveDestination = Vector3.zero;
			}
		}
	}

	private void CalculateSpeed()
	{
		m_SpeedScale = 1f;
		float num = m_Speed;
		if (m_CurrentSpeed > float.Epsilon)
		{
			m_SpeedTime = 0f;
		}
		else if (m_CurMovement == Vector3.zero)
		{
			m_SpeedTime = Time.time;
		}
		if (m_CurMoveDestination != Vector3.zero && m_MoveDirection == Vector3.zero)
		{
			float num2 = 0f;
			float num3 = 0f;
			Enemy enemy = ((!(m_target != null)) ? null : m_target.GetAttackEnemy());
			if (!Enemy.IsNullOrInvalid(enemy))
			{
				num3 = enemy.radius + m_PeTrans.radius;
			}
			num2 = PEUtil.Magnitude(m_Motor.transform.position, m_CurMoveDestination, base.Entity.gravity < float.Epsilon);
			num2 = Mathf.Max(0f, num2 - num3);
			if (m_SpeedState == SpeedState.Run && enemy != null)
			{
				float attribute = m_Attribute.GetAttribute(AttribType.WalkSpeed);
				float num4 = m_Attribute.GetAttribute(AttribType.RunSpeed);
				if (Field == MovementField.Sky)
				{
					num4 *= Mathf.Lerp(1f, 2f, Mathf.Max(0f, enemy.DistanceXZ - 32f) / 64f);
				}
				num = Mathf.Lerp(attribute, num4, Mathf.InverseLerp(0.5f, 2f, num2));
			}
			Vector3 from = Vector3.ProjectOnPlane(m_Motor.transform.forward, Vector3.up);
			Vector3 to = Vector3.ProjectOnPlane(m_CurMovementDirection, Vector3.up);
			float num5 = Vector3.Angle(from, to);
			m_SpeedScale *= Mathf.Lerp(1f, 0.5f, num5 / 150f);
			num *= m_SpeedScale;
			if (m_Motor is PEMotorPhysics)
			{
				num = Mathf.Lerp(0.1f, num, Time.time - m_SpeedTime);
			}
		}
		float num6 = ((!(m_CurrentSpeed > float.Epsilon)) ? num : m_CurrentSpeed);
		m_Motor.maxVelocityChange = num6;
		m_Motor.maxForwardSpeed = num6;
		m_Motor.maxSidewaysSpeed = num6;
		m_Motor.maxBackwardsSpeed = num6;
	}

	private void CalculateMovement()
	{
		CalculateDestination();
		if (AstarPath.active != null && m_Path != null)
		{
			m_Path.SetTargetposition(m_CurMoveDestination);
		}
		Vector3 vector = Vector3.zero;
		if (m_MoveDirection != Vector3.zero)
		{
			vector = m_MoveDirection;
		}
		else if (m_CurMoveDestination == Vector3.zero)
		{
			vector = Vector3.zero;
		}
		else
		{
			if (m_Path != null && m_Motor.gravity > float.Epsilon && AstarPath.active != null)
			{
				vector = m_Path.CalculateVelocity(m_Motor.transform.position);
			}
			if (vector == Vector3.zero)
			{
				vector = m_CurMoveDestination - m_Motor.transform.position;
			}
		}
		CalculateAvoid(vector);
		vector = vector.normalized + m_CurAvoidDirection.normalized;
		if (vector == Vector3.zero)
		{
			m_CurMovementDirection = Vector3.zero;
		}
		else if (m_CurMovementDirection == Vector3.zero)
		{
			m_CurMovementDirection = vector;
		}
		else
		{
			Vector3 curMovementDirection = m_CurMovementDirection;
			curMovementDirection.y = 0f;
			Vector3 vector2 = vector;
			vector2.y = 0f;
			Vector3 toDirection = Util.ConstantSlerp(curMovementDirection, vector2, 90f * Time.deltaTime);
			m_CurMovementDirection = Quaternion.FromToRotation(vector2, toDirection) * vector;
		}
		if (m_CurMovementDirection == Vector3.zero || !CanMove())
		{
			m_CurMovement = Vector3.zero;
		}
		else if (m_MoveDirection != Vector3.zero)
		{
			m_CurMovement = m_CurMovementDirection;
		}
		else
		{
			float num = 90f;
			PEMotorPhysics pEMotorPhysics = m_Motor as PEMotorPhysics;
			if (pEMotorPhysics != null)
			{
				num = ((m_SpeedState != SpeedState.Walk) ? pEMotorPhysics.runSmoothSpeed : pEMotorPhysics.walkSmoothSpeed);
			}
			Vector3 forward = m_Motor.transform.forward;
			forward.y = 0f;
			Vector3 curMovementDirection2 = m_CurMovementDirection;
			curMovementDirection2.y = 0f;
			Vector3 toDirection2 = Util.ConstantSlerp(forward, curMovementDirection2, num * Time.deltaTime);
			m_CurMovement = Quaternion.FromToRotation(curMovementDirection2, toDirection2) * m_CurMovementDirection;
		}
		if (m_CurMoveDestination != Vector3.zero)
		{
			Debug.DrawLine(m_Motor.transform.position, m_CurMoveDestination, Color.yellow);
		}
		Debug.DrawRay(m_Motor.transform.position, m_CurMovement.normalized * (10f + m_PeTrans.radius), Color.red);
		Debug.DrawRay(m_Motor.transform.position, m_CurMovementDirection.normalized * (6f + m_PeTrans.radius), Color.blue);
	}

	private bool Match(RaycastHit hitInfo)
	{
		if (hitInfo.collider == null || hitInfo.collider.isTrigger)
		{
			return false;
		}
		if (hitInfo.transform.IsChildOf(base.transform))
		{
			return false;
		}
		if (m_target != null && m_target.GetAttackEnemy() != null && m_target.GetAttackEnemy().skTarget != null && hitInfo.collider.transform.IsChildOf(m_target.GetAttackEnemy().skTarget.transform))
		{
			return false;
		}
		return true;
	}

	private void CalculateAvoid(Vector3 movement)
	{
		if (null == m_PeTrans || null == m_Motor)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		if (movement == Vector3.zero || m_MoveDirection != Vector3.zero)
		{
			m_CurAvoidDirection = Vector3.zero;
		}
		else
		{
			float num = m_Motor.maxForwardSpeed * Time.deltaTime * 10f;
			Vector3 position = m_PeTrans.position;
			Vector3 point = m_PeTrans.position + m_PeTrans.bound.size.y * Vector3.up;
			float radius = m_PeTrans.bound.extents.x + 0.5f;
			float maxDistance = m_PeTrans.bound.extents.z + num + 1f;
			RaycastHit hitInfo;
			if (Field == MovementField.Land)
			{
				Vector3 zero2 = Vector3.zero;
				RaycastHit[] array = Physics.CapsuleCastAll(position, point, radius, movement, maxDistance, layer);
				for (int i = 0; i < array.Length; i++)
				{
					Collider collider = array[i].collider;
					if (null == collider)
					{
						continue;
					}
					if (m_CurMoveDestination != Vector3.zero)
					{
						Bounds bounds = collider.bounds;
						Vector3 vector = m_CurMoveDestination;
						PeEntity componentInParent = collider.gameObject.GetComponentInParent<PeEntity>();
						if (componentInParent != null)
						{
							bounds = componentInParent.bounds;
							if (null == componentInParent.tr)
							{
								continue;
							}
							vector = componentInParent.tr.InverseTransformPoint(vector);
						}
						else
						{
							Operation_Multiple componentInParent2 = collider.gameObject.GetComponentInParent<Operation_Multiple>();
							if (componentInParent2 != null && !componentInParent2.Equals(null))
							{
								bounds = componentInParent2.LocalBounds;
								vector = componentInParent2.transform.InverseTransformPoint(vector);
							}
						}
						bounds.Expand(2f);
						vector.y = bounds.center.y;
						if (bounds.Contains(vector))
						{
							continue;
						}
					}
					if (!collider.transform.IsChildOf(base.transform) && (!(m_target != null) || m_target.GetAttackEnemy() == null || !(m_target.GetAttackEnemy().trans != null) || !collider.transform.IsChildOf(m_target.GetAttackEnemy().trans)) && (!(m_target != null) || !(m_target.Treat != null) || !collider.transform.IsChildOf(m_target.Treat.transform)))
					{
						zero2 = m_PeTrans.position - collider.transform.position;
						zero2.y = 0f;
						zero += zero2.normalized;
					}
				}
			}
			else if ((Field == MovementField.Sky || Field == MovementField.water) && Physics.CapsuleCast(position, point, radius, movement, out hitInfo, maxDistance, voxelLayer))
			{
				zero += hitInfo.normal;
			}
			if (zero != Vector3.zero)
			{
				if (m_CurAvoidDirection == Vector3.zero)
				{
					m_CurAvoidDirection = zero.normalized;
				}
				else
				{
					m_CurAvoidDirection = Util.ConstantSlerp(m_CurAvoidDirection, zero, Time.deltaTime * 90f);
				}
			}
			else
			{
				m_CurAvoidDirection = Vector3.Lerp(m_CurAvoidDirection, Vector3.zero, 2f * Time.deltaTime);
				if (m_CurAvoidDirection.sqrMagnitude < 0.0225f)
				{
					m_CurAvoidDirection = Vector3.zero;
				}
			}
		}
		if (m_CurAvoidDirection != Vector3.zero)
		{
			Debug.DrawRay(m_Motor.transform.position, m_CurAvoidDirection.normalized * 6.5f, Color.green);
		}
	}

	private void UpdatePosition()
	{
		if (!(m_Motor == null))
		{
			m_CurFaceDirection = m_RotDirection;
			m_CurMoveDestination = m_MoveDestination;
			CalculateSpeed();
			CalculateMovement();
			m_Motor.desiredMovementDirection = Quaternion.Inverse(m_Motor.transform.rotation) * m_CurMovement.normalized;
		}
	}

	private void UpdateStuck()
	{
		if (m_CurMovementDirection == Vector3.zero || velocity.sqrMagnitude > 0.0225f)
		{
			m_LastMoveTime = Time.time;
		}
	}

	private void UpdateNetMovement()
	{
		m_PeTrans.position = Vector3.Lerp(m_PeTrans.position, m_NetPos, 5f * Time.deltaTime);
	}

	private void UpdateNetRot()
	{
		m_PeTrans.rotation = Quaternion.Slerp(m_PeTrans.rotation, Quaternion.Euler(m_NetRot), 5f * Time.deltaTime);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (PeGameMgr.IsMulti && m_Proxy)
		{
			UpdateNetMovement();
			UpdateNetRot();
		}
		else if (base.Entity.hasView)
		{
			UpdateMovementState();
			UpdatePosition();
			UpdateRotation();
			UpdateStuck();
		}
	}

	public override void Start()
	{
		base.Start();
		m_SpeedState = SpeedState.None;
		m_PeTrans = base.Entity.peTrans;
		m_Animator = base.Entity.GetCmpt<AnimatorCmpt>();
		m_Attribute = base.Entity.GetCmpt<SkAliveEntity>();
		m_target = GetComponent<TargetCmpt>();
		if (m_Attribute != null)
		{
			m_Attribute.deathEvent += OnDeath;
		}
	}

	public override bool Stucking(float time)
	{
		if (base.Entity.isRagdoll)
		{
			return false;
		}
		if (Time.time - m_LastMoveTime > time || !base.Entity.hasView)
		{
			return true;
		}
		return false;
	}

	public override void ApplyForce(Vector3 power, ForceMode mode)
	{
		if (m_Motor != null || base.Entity.GetAttribute(AttribType.Rigid) < 0f)
		{
			m_Motor.desiredMovementEffect = power;
		}
	}

	public override void Move(Vector3 dir, SpeedState state = SpeedState.Walk)
	{
		m_MoveDirection = dir;
		speed = state;
		if (dir != Vector3.zero)
		{
			m_MoveDestination = Vector3.zero;
			m_RotDirection = Vector3.zero;
		}
	}

	public override void SetSpeed(float Speed)
	{
		m_CurrentSpeed = Speed;
	}

	public override void MoveTo(Vector3 targetPos, SpeedState state = SpeedState.Walk, bool avoid = true)
	{
		speed = state;
		m_MoveDestination = targetPos;
		if (m_MoveDestination != Vector3.zero)
		{
			m_MoveDirection = Vector3.zero;
			m_RotDirection = Vector3.zero;
		}
	}

	public override void NetMoveTo(Vector3 position, Vector3 moveVelocity, bool immediately = false)
	{
		m_NetPos = position;
		if (immediately && m_PeTrans != null)
		{
			m_PeTrans.position = position;
		}
	}

	public override void NetRotateTo(Vector3 eulerAngle)
	{
		m_NetRot = eulerAngle;
	}

	public override void RotateTo(Vector3 targetDir)
	{
		m_RotDirection = targetDir;
	}

	public override void Jump()
	{
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
		{
			BiologyViewRoot biologyViewRoot = args[1] as BiologyViewRoot;
			m_Motor = biologyViewRoot.motor;
			m_Steer = biologyViewRoot.steerAgent;
			m_Path = biologyViewRoot.pathFinder;
			m_Param = biologyViewRoot.beatParam;
			InitParam();
			if (m_Motor is PEMotorPhysics)
			{
				(m_Motor as PEMotorPhysics).Init(base.Entity);
			}
			m_LastMoveTime = Time.time;
			break;
		}
		case EMsg.Net_Controller:
			m_Proxy = false;
			break;
		case EMsg.Net_Proxy:
			m_Proxy = true;
			break;
		case EMsg.State_Die:
			if (m_Motor != null)
			{
				m_Motor.Reset();
			}
			base.enabled = false;
			break;
		case EMsg.State_Revive:
			base.enabled = true;
			break;
		}
	}

	private void OnDeath(SkEntity sk1, SkEntity sk2)
	{
		base.enabled = false;
	}

	private void OnEnable()
	{
		if (m_Motor != null)
		{
			m_Motor.enabled = true;
			if (m_Motor is PEMotorAnimator && base.Entity.Rigid != null)
			{
				base.Entity.Rigid.useGravity = true;
			}
		}
	}

	private void OnDisable()
	{
		if (m_Motor != null)
		{
			m_Motor.enabled = false;
			if (m_Motor is PEMotorAnimator && base.Entity.Rigid != null)
			{
				base.Entity.Rigid.useGravity = false;
			}
		}
	}

	private void InitParam()
	{
		if (!(null != m_Param) || base.Entity.proto != EEntityProto.Monster)
		{
			return;
		}
		MonsterProtoDb.Item item = MonsterProtoDb.Get(base.Entity.ProtoID);
		if (item == null)
		{
			return;
		}
		if (item.RepulsedType > 0 && base.Entity.GetAttribute(AttribType.ThresholdRepulsed) > 550f)
		{
			base.Entity.SetAttribute(AttribType.ThresholdRepulsed, 550f);
		}
		if (item.RepulsedType == 2)
		{
			if (base.Entity.GetAttribute(AttribType.ThresholdRepulsed) > 130f)
			{
				base.Entity.SetAttribute(AttribType.ThresholdRepulsed, 130f);
			}
			m_Param.m_ForceToVelocity.keys = LongMoveVelocity;
			m_Param.m_ForceToMoveTime.keys = LongMoveTime;
		}
		else
		{
			m_Param.m_ForceToVelocity.keys = NearMoveVelocity;
			m_Param.m_ForceToMoveTime.keys = NearMoveTime;
		}
		m_Param.m_ApplyMoveStopTime.keys = MoveStopTime;
		m_Param.m_WentflyTimeCurve.keys = MoveWentflyTime;
	}
}
