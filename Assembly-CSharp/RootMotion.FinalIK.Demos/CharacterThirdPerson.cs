using System;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class CharacterThirdPerson : CharacterBase
{
	[Serializable]
	public enum MoveMode
	{
		Directional,
		Strafe
	}

	public struct AnimState
	{
		public Vector3 moveDirection;

		public bool jump;

		public bool crouch;

		public bool onGround;

		public bool isStrafing;

		public float yVelocity;
	}

	[Serializable]
	public class AdvancedSettings
	{
		public float stationaryTurnSpeedMlp = 1f;

		public float movingTurnSpeed = 5f;

		public float lookResponseSpeed = 2f;

		public float jumpRepeatDelayTime = 0.25f;

		public float groundStickyEffect = 5f;

		public float platformFriction = 7f;

		public float maxVerticalVelocityOnGround = 3f;

		public float crouchCapsuleScaleMlp = 0.6f;

		public float velocityToGroundTangentWeight = 1f;

		public float wallRunMaxLength = 1f;

		public float wallRunMinHorVelocity = 3f;

		public float wallRunMinVelocityY = -1f;

		public float wallRunRotationSpeed = 1.5f;

		public float wallRunMaxRotationAngle = 70f;

		public float wallRunWeightSpeed = 5f;
	}

	[SerializeField]
	private CharacterAnimationThirdPerson characterAnimation;

	[SerializeField]
	protected Grounder grounder;

	[SerializeField]
	private LayerMask wallRunLayers;

	[Range(1f, 4f)]
	[SerializeField]
	private float gravityMultiplier = 2f;

	public MoveMode moveMode;

	public float accelerationTime = 0.2f;

	public float airSpeed = 6f;

	public float airControl = 2f;

	public float jumpPower = 12f;

	public bool lookInCameraDirection;

	public AdvancedSettings advancedSettings;

	private UserControlThirdPerson.State inputState = default(UserControlThirdPerson.State);

	private Vector3 moveDirection;

	private Vector3 lookPosSmooth;

	private Animator animator;

	private Vector3 normal;

	private Vector3 platformVelocity;

	private RaycastHit hit;

	private float jumpLeg;

	private float jumpEndTime;

	private float forwardMlp;

	private float groundDistance;

	private float lastAirTime;

	private float stickyForce;

	private Vector3 wallNormal = Vector3.up;

	private Vector3 moveDirectionVelocity;

	private float wallRunWeight;

	private float lastWallRunWeight;

	private AnimState animState = default(AnimState);

	public bool onGround { get; private set; }

	protected override void Start()
	{
		base.Start();
		animator = GetComponent<Animator>();
		wallNormal = Vector3.up;
		lookPosSmooth = base.transform.position + base.transform.forward * 10f;
	}

	private void OnAnimatorMove()
	{
		Move(animator.deltaPosition);
	}

	private bool CanWallRun()
	{
		if (Time.time < jumpEndTime - 0.1f)
		{
			return false;
		}
		if (Time.time > jumpEndTime - 0.1f + advancedSettings.wallRunMaxLength)
		{
			return false;
		}
		if (GetComponent<Rigidbody>().velocity.y < advancedSettings.wallRunMinVelocityY)
		{
			return false;
		}
		if (inputState.move == Vector3.zero)
		{
			return false;
		}
		Vector3 velocity = GetComponent<Rigidbody>().velocity;
		velocity.y = 0f;
		if (velocity.magnitude < advancedSettings.wallRunMinHorVelocity)
		{
			return false;
		}
		return true;
	}

	private void WallRun()
	{
		wallRunWeight = Mathf.MoveTowards(wallRunWeight, (!CanWallRun()) ? 0f : 1f, Time.deltaTime * advancedSettings.wallRunWeightSpeed);
		if (wallRunWeight <= 0f && lastWallRunWeight > 0f)
		{
			base.transform.rotation = Quaternion.LookRotation(new Vector3(base.transform.forward.x, 0f, base.transform.forward.z), Vector3.up);
			wallNormal = Vector3.up;
		}
		lastWallRunWeight = wallRunWeight;
		if (!(wallRunWeight <= 0f))
		{
			if (onGround && GetComponent<Rigidbody>().velocity.y < 0f)
			{
				GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0f, GetComponent<Rigidbody>().velocity.z);
			}
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			RaycastHit hitInfo = default(RaycastHit);
			hitInfo.normal = Vector3.up;
			Physics.Raycast((!onGround) ? GetComponent<Collider>().bounds.center : base.transform.position, forward, out hitInfo, 3f, wallRunLayers);
			wallNormal = Vector3.Slerp(wallNormal, hitInfo.normal, Time.deltaTime * advancedSettings.wallRunRotationSpeed);
			wallNormal = Vector3.RotateTowards(Vector3.up, wallNormal, advancedSettings.wallRunMaxRotationAngle * ((float)Math.PI / 180f), 0f);
			Vector3 tangent = base.transform.forward;
			Vector3 vector = wallNormal;
			Vector3.OrthoNormalize(ref vector, ref tangent);
			base.transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(forward, Vector3.up), Quaternion.LookRotation(tangent, wallNormal), wallRunWeight);
		}
	}

	public override void Move(Vector3 deltaPosition)
	{
		WallRun();
		Vector3 vector = deltaPosition / Time.deltaTime;
		vector += new Vector3(platformVelocity.x, 0f, platformVelocity.z);
		if (onGround)
		{
			if (advancedSettings.velocityToGroundTangentWeight > 0f)
			{
				Quaternion b = Quaternion.FromToRotation(base.transform.up, normal);
				vector = Quaternion.Lerp(Quaternion.identity, b, advancedSettings.velocityToGroundTangentWeight) * vector;
			}
		}
		else
		{
			vector = Vector3.Lerp(b: new Vector3(inputState.move.x * airSpeed, 0f, inputState.move.z * airSpeed), a: GetComponent<Rigidbody>().velocity, t: Time.deltaTime * airControl);
		}
		if (onGround && Time.time > jumpEndTime)
		{
			GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity - base.transform.up * stickyForce * Time.deltaTime;
		}
		vector.y = Mathf.Clamp(GetComponent<Rigidbody>().velocity.y, GetComponent<Rigidbody>().velocity.y, (!onGround) ? GetComponent<Rigidbody>().velocity.y : advancedSettings.maxVerticalVelocityOnGround);
		GetComponent<Rigidbody>().velocity = vector;
		float b3 = (onGround ? GetSlopeDamper(-deltaPosition / Time.deltaTime, normal) : 1f);
		forwardMlp = Mathf.Lerp(forwardMlp, b3, Time.deltaTime * 5f);
	}

	public void UpdateState(UserControlThirdPerson.State _inputState)
	{
		inputState = _inputState;
		Look();
		GroundCheck();
		if (inputState.move == Vector3.zero && groundDistance < airborneThreshold * 0.5f)
		{
			HighFriction();
		}
		else
		{
			ZeroFriction();
		}
		if (onGround)
		{
			animState.jump = Jump();
		}
		else
		{
			GetComponent<Rigidbody>().AddForce(Physics.gravity * gravityMultiplier - Physics.gravity);
		}
		ScaleCapsule((!inputState.crouch) ? 1f : advancedSettings.crouchCapsuleScaleMlp);
		animState.moveDirection = GetMoveDirection();
		animState.crouch = inputState.crouch;
		animState.onGround = onGround;
		animState.yVelocity = GetComponent<Rigidbody>().velocity.y;
		animState.isStrafing = moveMode == MoveMode.Strafe;
		characterAnimation.UpdateState(animState);
	}

	private Vector3 GetMoveDirection()
	{
		switch (moveMode)
		{
		case MoveMode.Directional:
			moveDirection = Vector3.SmoothDamp(moveDirection, new Vector3(0f, 0f, inputState.move.magnitude), ref moveDirectionVelocity, accelerationTime);
			return moveDirection * forwardMlp;
		case MoveMode.Strafe:
			moveDirection = Vector3.SmoothDamp(moveDirection, inputState.move, ref moveDirectionVelocity, accelerationTime);
			return base.transform.InverseTransformDirection(moveDirection);
		default:
			return Vector3.zero;
		}
	}

	private void Look()
	{
		lookPosSmooth = Vector3.Lerp(lookPosSmooth, inputState.lookPos, Time.deltaTime * advancedSettings.lookResponseSpeed);
		float num = GetAngleFromForward(GetLookDirection());
		if (inputState.move == Vector3.zero)
		{
			num *= (1.01f - Mathf.Abs(num) / 180f) * advancedSettings.stationaryTurnSpeedMlp;
		}
		RigidbodyRotateAround(characterAnimation.GetPivotPoint(), base.transform.up, num * Time.deltaTime * advancedSettings.movingTurnSpeed);
	}

	private Vector3 GetLookDirection()
	{
		bool flag = inputState.move != Vector3.zero;
		switch (moveMode)
		{
		case MoveMode.Directional:
			if (flag)
			{
				return inputState.move;
			}
			return (!lookInCameraDirection) ? base.transform.forward : (inputState.lookPos - GetComponent<Rigidbody>().position);
		case MoveMode.Strafe:
			if (flag)
			{
				return inputState.lookPos - GetComponent<Rigidbody>().position;
			}
			return (!lookInCameraDirection) ? base.transform.forward : (inputState.lookPos - GetComponent<Rigidbody>().position);
		default:
			return Vector3.zero;
		}
	}

	private bool Jump()
	{
		if (!inputState.jump)
		{
			return false;
		}
		if (inputState.crouch)
		{
			return false;
		}
		if (!characterAnimation.animationGrounded)
		{
			return false;
		}
		if (Time.time < lastAirTime + advancedSettings.jumpRepeatDelayTime)
		{
			return false;
		}
		onGround = false;
		jumpEndTime = Time.time + 0.1f;
		Vector3 velocity = inputState.move * airSpeed;
		GetComponent<Rigidbody>().velocity = velocity;
		GetComponent<Rigidbody>().velocity += base.transform.up * jumpPower;
		return true;
	}

	private RaycastHit GetHit()
	{
		if (grounder == null)
		{
			return GetSpherecastHit();
		}
		if (grounder.solver.quality != Grounding.Quality.Best)
		{
			return GetSpherecastHit();
		}
		if (grounder.enabled && grounder.weight > 0f)
		{
			return grounder.solver.rootHit;
		}
		return grounder.solver.GetRootHit();
	}

	private void GroundCheck()
	{
		Vector3 b = Vector3.zero;
		float num = 0f;
		hit = GetHit();
		normal = hit.normal;
		groundDistance = GetComponent<Rigidbody>().position.y - hit.point.y;
		if (Time.time > jumpEndTime && GetComponent<Rigidbody>().velocity.y < jumpPower * 0.5f)
		{
			bool flag = onGround;
			onGround = false;
			float num2 = (flag ? airborneThreshold : (airborneThreshold * 0.5f));
			Vector3 velocity = GetComponent<Rigidbody>().velocity;
			velocity.y = 0f;
			float magnitude = velocity.magnitude;
			if (groundDistance < num2)
			{
				num = advancedSettings.groundStickyEffect * magnitude * num2;
				if (hit.rigidbody != null)
				{
					b = hit.rigidbody.GetPointVelocity(hit.point);
				}
				onGround = true;
			}
		}
		platformVelocity = Vector3.Lerp(platformVelocity, b, Time.deltaTime * advancedSettings.platformFriction);
		stickyForce = num;
		if (!onGround)
		{
			lastAirTime = Time.time;
		}
	}
}
