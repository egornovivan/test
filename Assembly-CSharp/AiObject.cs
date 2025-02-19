using System;
using System.Collections.Generic;
using AiAsset;
using ItemAsset;
using Pathea;
using SkillAsset;
using UnityEngine;
using WhiteCat;

public class AiObject : SkillRunner
{
	public delegate void DelegateAiObject(AiObject aiObject);

	protected string m_currentAnimName = string.Empty;

	private bool m_updateDefaultAnimation = true;

	private static float SpeedDampTime = 0.25f;

	private static float DirectionDampTime = 0.25f;

	private float mLookAtWeight;

	private float mLeftHandIKWeight;

	private float mRightHandWeight;

	private float mLeftFootIKWeight;

	private float mRightFootWeight;

	private Vector3 mLookAtPosition = Vector3.zero;

	private Vector3 mLeftHandIKPosition = Vector3.zero;

	private Vector3 mRightHandIKPosition = Vector3.zero;

	private Vector3 mLeftFootIKPosition = Vector3.zero;

	private Vector3 mRightFootIKPosition = Vector3.zero;

	private Vector3 mLastPos;

	private Quaternion mLastRot;

	protected bool m_isAttackIdle;

	protected bool m_isAttacking;

	protected bool m_isHurted = true;

	protected float m_lastDamageTime;

	protected float m_deathStartTime = float.PositiveInfinity;

	protected Vector3 m_spawnPosition;

	protected Vector3 m_offset;

	protected float m_speed;

	protected float m_turnSpeed;

	protected bool m_canMove;

	protected bool m_canRotate;

	protected bool m_canAttack;

	protected bool m_isStuck;

	protected Bounds m_bound;

	protected AiCharacterMotor m_motor;

	public GameObject aiObject;

	public Transform model;

	public Transform ragdoll;

	[NonSerialized]
	public bool isMission;

	protected int m_camp;

	protected int m_harm;

	protected int m_life;

	protected int m_soundId;

	protected bool m_isDead;

	protected bool m_isSleep;

	protected bool m_isActive;

	protected bool m_isConceal;

	protected float m_startShowLifeBarTime;

	protected Transform mCenter;

	protected Bounds mHideBounds;

	protected Transform m_tdInfo;

	protected Animation m_animation;

	protected Animator m_animator;

	protected Rigidbody m_rigidbody;

	protected CharacterController m_controller;

	protected EffSkillBuffSum m_buffSum;

	protected EffSkillBuffMultiply m_buffSumMul;

	protected AiBehave m_behave;

	public bool isPlaying
	{
		get
		{
			if (m_animation == null)
			{
				return false;
			}
			return m_animation.isPlaying;
		}
	}

	public Animation anim => m_animation;

	public bool updateDefaultAnimation
	{
		set
		{
			m_updateDefaultAnimation = value;
		}
	}

	public virtual float animationTime => 0f;

	public virtual float minDamageRange => 0f;

	public virtual float maxDamageRange => 0f;

	public bool isAttackIdle
	{
		get
		{
			return m_isAttackIdle;
		}
		set
		{
			m_isAttackIdle = value;
		}
	}

	public bool isAttacking
	{
		get
		{
			return m_isAttacking;
		}
		set
		{
			m_isAttacking = value;
		}
	}

	public bool isHurted
	{
		get
		{
			return m_isHurted;
		}
		set
		{
			m_isHurted = value;
		}
	}

	public float deathStartTime => m_deathStartTime;

	public float lastDamageTime => m_lastDamageTime;

	protected virtual float CorpseTime => 0f;

	public Bounds bound => (!(GetComponent<Collider>() != null)) ? default(Bounds) : GetComponent<Collider>().bounds;

	public AiCharacterMotor motor => m_motor;

	public Vector3 position => base.transform.position;

	public Vector3 forward => base.transform.forward * radius;

	public Vector3 up => base.transform.up * bound.extents.y * 2f;

	public Vector3 spawnPosition => m_spawnPosition;

	public Vector3 forwardPoint => base.transform.position + base.transform.forward * radius;

	public float width => bound.extents.x;

	public virtual Vector3 center
	{
		get
		{
			if (mCenter != null && mCenter.GetComponent<Rigidbody>() != null)
			{
				return mCenter.GetComponent<Rigidbody>().worldCenterOfMass;
			}
			return base.transform.position + base.transform.up * bound.extents.y;
		}
	}

	public float height
	{
		get
		{
			if (GetComponent<Collider>() != null)
			{
				return AiUtil.GetColliderHeight(GetComponent<Collider>());
			}
			return bound.extents.y * 2f;
		}
	}

	public float radius
	{
		get
		{
			if (GetComponent<Collider>() != null)
			{
				return AiUtil.GetColliderRadius(GetComponent<Collider>());
			}
			return Mathf.Max(bound.extents.x, bound.extents.z);
		}
	}

	public Vector3 extents
	{
		get
		{
			if (GetComponent<Collider>() != null)
			{
				if (GetComponent<Collider>() is CapsuleCollider)
				{
					CapsuleCollider capsuleCollider = GetComponent<Collider>() as CapsuleCollider;
					if (capsuleCollider.direction == 2)
					{
						return new Vector3(capsuleCollider.radius, capsuleCollider.radius, capsuleCollider.height * 0.5f);
					}
					if (capsuleCollider.direction == 1)
					{
						return new Vector3(capsuleCollider.radius, capsuleCollider.height * 0.5f, capsuleCollider.radius);
					}
				}
				else if (GetComponent<Collider>() is CharacterController)
				{
					CharacterController characterController = GetComponent<Collider>() as CharacterController;
					return new Vector3(characterController.radius, characterController.height * 0.5f, characterController.radius);
				}
			}
			else
			{
				CreationController componentOrOnParent = VCUtils.GetComponentOrOnParent<CreationController>(base.gameObject);
				if (componentOrOnParent != null)
				{
					return AiUtil.TransfromOBB2AABB(base.transform, componentOrOnParent.bounds).extents;
				}
			}
			return Vector3.zero;
		}
	}

	public virtual float turnSpeed
	{
		get
		{
			return m_turnSpeed;
		}
		set
		{
			if (m_turnSpeed != value && m_motor != null)
			{
				m_motor.maxRotationSpeed = value;
			}
			m_turnSpeed = value;
		}
	}

	public float speed
	{
		get
		{
			return m_speed;
		}
		set
		{
			if ((double)value < 0.1)
			{
				value = 0.1f;
			}
			if (m_motor != null)
			{
				m_motor.maxForwardSpeed = value;
				m_motor.maxSidewaysSpeed = value;
				m_motor.maxBackwardsSpeed = value;
			}
			m_speed = value;
		}
	}

	public bool canAttack
	{
		get
		{
			return m_canAttack;
		}
		set
		{
			m_canAttack = value;
		}
	}

	public bool canMove
	{
		get
		{
			return m_canMove;
		}
		set
		{
			m_canMove = value;
		}
	}

	public bool canRotate
	{
		get
		{
			return m_canRotate;
		}
		set
		{
			m_canRotate = value;
		}
	}

	public bool IsMoving
	{
		get
		{
			if (motor.desiredMoveDestination == Vector3.zero)
			{
				return true;
			}
			return false;
		}
	}

	public virtual Vector3 desiredMoveDestination
	{
		get
		{
			return motor.desiredMoveDestination;
		}
		set
		{
			if (!(motor == null) && (!GameConfig.IsMultiMode || IsController))
			{
				if (CanMove())
				{
					motor.desiredMoveDestination = value;
				}
				else
				{
					motor.desiredMoveDestination = Vector3.zero;
				}
			}
		}
	}

	public virtual Vector3 desiredMovementDirection
	{
		get
		{
			return motor.desiredMovementDirection;
		}
		set
		{
			if (!(motor == null) && (!GameConfig.IsMultiMode || IsController))
			{
				if (CanMove())
				{
					motor.desiredMoveDestination = Vector3.zero;
					motor.desiredMovementDirection = value;
				}
				else
				{
					motor.desiredMovementDirection = Vector3.zero;
				}
			}
		}
	}

	public virtual Vector3 desiredFaceDirection
	{
		set
		{
			if (!(motor == null) && (!GameConfig.IsMultiMode || IsController))
			{
				if (CanRotate())
				{
					motor.desiredFaceDirection = value;
				}
				else
				{
					motor.desiredFaceDirection = Vector3.zero;
				}
			}
		}
	}

	public Transform desiredLookAtTransform
	{
		get
		{
			return motor.desiredLookAtTran;
		}
		set
		{
			if (!(motor == null))
			{
				motor.desiredLookAtTran = value;
			}
		}
	}

	public Vector3 offset => m_offset;

	public Animator animator => m_animator;

	public Rigidbody rigid => m_rigidbody;

	public AiBehave behave => m_behave;

	public CharacterController controller => m_controller;

	public virtual bool isActive => m_isActive;

	public Bounds hideBounds => mHideBounds;

	public bool conceal
	{
		get
		{
			return m_isConceal;
		}
		set
		{
			m_isConceal = value;
		}
	}

	public virtual Transform tdInfo
	{
		get
		{
			return m_tdInfo;
		}
		set
		{
			m_tdInfo = value;
			if (value != null)
			{
				SetCamp(27);
			}
		}
	}

	public virtual Bounds arriveBound => bound;

	public virtual Bounds attackBound => bound;

	public virtual bool isboss => false;

	public virtual int camp
	{
		get
		{
			return m_camp;
		}
		set
		{
			SetCamp(value);
		}
	}

	public virtual int harm
	{
		get
		{
			return m_harm;
		}
		set
		{
			m_harm = value;
		}
	}

	public virtual string xmlPath => string.Empty;

	public virtual bool dead
	{
		get
		{
			return m_isDead;
		}
		set
		{
			m_isDead = value;
		}
	}

	public virtual int life
	{
		get
		{
			return m_life;
		}
		set
		{
			m_life = value;
		}
	}

	public virtual int maxLife => 0;

	public virtual float lifePercent
	{
		get
		{
			return (float)life / (float)maxLife;
		}
		set
		{
			life = (int)Mathf.Clamp((float)maxLife * value, 0f, maxLife);
			if (life <= 0 && !m_isDead)
			{
				m_isDead = true;
				OnDeath();
			}
		}
	}

	public virtual float attackRadius => 0f;

	public virtual int damage => 0;

	public virtual int buildDamage => 0;

	public virtual int defence => 0;

	public virtual float walkSpeed => 1f;

	public virtual float runSpeed => 1f;

	public virtual int defenceType => 0;

	public bool sleeping
	{
		get
		{
			return m_isSleep;
		}
		set
		{
			m_isSleep = value;
		}
	}

	public event DelegateAiObject SpawnedHandlerEvent;

	public event DelegateAiObject DeathHandlerEvent;

	public event DelegateAiObject DestroyHandlerEvent;

	public event DelegateAiObject ActiveHandlerEvent;

	public event DelegateAiObject InactiveHandlerEvent;

	protected virtual void PickDefaltAnimation()
	{
	}

	public void CrossFade(string name, bool isPlay = true)
	{
		if (m_animation != null)
		{
			if (isPlay)
			{
				PlayAiAnimation(name);
			}
		}
		else if (m_animator != null)
		{
			SetBool(name, isPlay);
		}
	}

	public void PlayDefaultAnimation(string name)
	{
		if (!(m_animation == null) && !(m_animation[name] == null) && !m_animation.IsPlaying(name))
		{
			m_animation.CrossFade(name);
		}
	}

	public virtual void PlayAiAnimation(string name)
	{
		if (!(m_animation == null) && !(m_animation[name] == null) && !(m_currentAnimName == name))
		{
			m_currentAnimName = name;
			m_animation.CrossFade(name);
			PlayAnimationAudio(name);
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_Animation, name);
			}
		}
	}

	public AnimationState GetAnimationState(string name)
	{
		if (m_animation == null)
		{
			return null;
		}
		return m_animation[name];
	}

	public void ClearAnimation()
	{
		m_currentAnimName = string.Empty;
	}

	public virtual bool IsPlaying(string name)
	{
		if (m_animation != null)
		{
			return m_animation.IsPlaying(name);
		}
		if (m_animator != null)
		{
			for (int i = 0; i < m_animator.layerCount; i++)
			{
				AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(i);
				int num = Animator.StringToHash(m_animator.GetLayerName(i) + "." + name);
				if (currentAnimatorStateInfo.nameHash == num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPlayingAny(string name)
	{
		if (m_animation == null)
		{
			return false;
		}
		int animationCountByName = GetAnimationCountByName(name);
		if (animationCountByName <= 0)
		{
			return false;
		}
		for (int i = 0; i < animationCountByName; i++)
		{
			if (m_animation.IsPlaying(name + i))
			{
				return true;
			}
		}
		return false;
	}

	public int GetAnimationCountByName(string name)
	{
		if (m_animation == null)
		{
			return 0;
		}
		int num = 0;
		foreach (AnimationState item in m_animation)
		{
			if (IsExist(item.name, name) && name != item.name)
			{
				num++;
			}
		}
		return num;
	}

	protected virtual void UpdateAnimation()
	{
		if (!(m_animation == null) && IsPickDefaltAnimation())
		{
			PickDefaltAnimation();
		}
	}

	protected bool IsPickDefaltAnimation()
	{
		if (!m_updateDefaultAnimation || m_animation == null || m_isSleep)
		{
			return false;
		}
		if (m_currentAnimName == string.Empty)
		{
			return true;
		}
		if (IsPlaying(m_currentAnimName))
		{
			return false;
		}
		m_currentAnimName = string.Empty;
		return true;
	}

	protected AnimationState GetCurrentAnimationState()
	{
		if (m_animation == null || !m_animation.isPlaying)
		{
			return null;
		}
		foreach (AnimationState item in m_animation)
		{
			if (IsPlaying(item.name))
			{
				return item;
			}
		}
		return null;
	}

	public List<AnimationState> GetSimilarSequence(string name)
	{
		if (m_animation == null)
		{
			return null;
		}
		List<AnimationState> list = new List<AnimationState>();
		foreach (AnimationState item in m_animation)
		{
			if (IsExist(item.name, name) && name != item.name)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private bool IsExist(string whole, string subString)
	{
		if (!whole.StartsWith(subString))
		{
			return false;
		}
		string message = whole.Substring(subString.Length);
		int result;
		return AiMath.IsNumberic(message, out result);
	}

	protected virtual void UpdateAnimator()
	{
		if (!(m_animator == null))
		{
			if (m_motor != null && !(m_motor is AiAnimatorMotor) && (!GameConfig.IsMultiMode || IsController))
			{
				UpdateMoveAnimator();
			}
			if (GameConfig.IsMultiMode && !IsController)
			{
				PickDefaltAnimator();
			}
		}
	}

	private void PickDefaltAnimator()
	{
		Vector3 direction = base.transform.position - mLastPos;
		if (direction.sqrMagnitude <= 0.0025000002f)
		{
			SetFloat("Speed", 0f);
		}
		else if (base.transform.InverseTransformDirection(direction).z > float.Epsilon)
		{
			SetFloat("Speed", 1f, 0.25f, Time.deltaTime);
		}
		else
		{
			SetFloat("Speed", -1f, 0.25f, Time.deltaTime);
		}
		Vector3 vector = Quaternion.Inverse(mLastRot) * base.transform.forward;
		if (vector.x < -0.1f)
		{
			SetFloat("Direction", -1f, 0.25f, Time.deltaTime);
		}
		else if (vector.x > 0.1f)
		{
			SetFloat("Direction", 1f, 0.25f, Time.deltaTime);
		}
		else
		{
			SetFloat("Direction", 0f);
		}
		mLastPos = base.transform.position;
		mLastRot = base.transform.rotation;
	}

	public void ApplyRootMotion(bool activate)
	{
		if (!(m_animator == null))
		{
			m_animator.applyRootMotion = activate;
		}
	}

	private void UpdateMoveAnimator()
	{
		Vector3 v = motor.transform.rotation * motor.desiredMovementDirection;
		v = Util.ProjectOntoPlane(v, motor.transform.up);
		if (v == Vector3.zero)
		{
			SetFloat("Speed", 0f, 0.25f, Time.deltaTime);
			return;
		}
		float value = 1f;
		if (motor.maxRunSpeed - motor.maxWalkSpeed > float.Epsilon)
		{
			value = Mathf.Clamp((Mathf.Clamp(motor.maxForwardSpeed, motor.maxWalkSpeed, motor.maxRunSpeed) - motor.maxWalkSpeed) / (motor.maxRunSpeed - motor.maxWalkSpeed), 0.15f, 1f);
		}
		else
		{
			Debug.LogWarning(base.name + " maxRunSpeed[" + motor.maxRunSpeed + "] not big than maxWalkSpeed[" + motor.maxWalkSpeed + "].");
		}
		Vector3 vector = Util.ProjectOntoPlane(motor.desiredVelocity, base.transform.up);
		if (Vector3.Dot(base.transform.forward, vector.normalized) > 0f)
		{
			SetFloat("Speed", value, 0.25f, Time.deltaTime);
		}
		else if (motor.desiredLookAtTran == null)
		{
			SetFloat("Speed", 0f, 0.25f, Time.deltaTime);
		}
		else
		{
			SetFloat("Speed", -1f, 0.25f, Time.deltaTime);
		}
	}

	protected virtual void OnAnimatorIK(int layerIndex)
	{
		if (!(m_animator == null))
		{
			SetLookAtWeight(mLookAtWeight);
			SetLookAtPosition(mLookAtPosition);
			SetIKPositionWeight(AvatarIKGoal.LeftHand, mLeftHandIKWeight);
			SetIKPosition(AvatarIKGoal.LeftHand, mLeftHandIKPosition);
			SetIKPositionWeight(AvatarIKGoal.RightHand, mRightHandWeight);
			SetIKPosition(AvatarIKGoal.RightHand, mRightHandIKPosition);
			SetIKPositionWeight(AvatarIKGoal.LeftFoot, mLeftFootIKWeight);
			SetIKPosition(AvatarIKGoal.LeftFoot, mLeftFootIKPosition);
			SetIKPositionWeight(AvatarIKGoal.RightFoot, mRightFootWeight);
			SetIKPosition(AvatarIKGoal.RightFoot, mRightFootIKPosition);
		}
	}

	public void SetLeftHandIKWeight(float value)
	{
		if (mLeftHandIKWeight != value)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.LeftHand, value);
			}
			mLeftHandIKWeight = value;
		}
	}

	public void SetLeftHandIKPosition(Vector3 ikPosition)
	{
		if (mLeftHandIKPosition != ikPosition)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.LeftHand, ikPosition);
			}
			mLeftHandIKPosition = ikPosition;
		}
	}

	public void SetRightHandIKWeight(float value)
	{
		if (mRightHandWeight != value)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.RightHand, value);
			}
			mRightHandWeight = value;
		}
	}

	public void SetRightHandIKPosition(Vector3 ikPosition)
	{
		if (mRightHandIKPosition != ikPosition)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.RightHand, ikPosition);
			}
			mRightHandIKPosition = ikPosition;
		}
	}

	public void SetLeftFootIKWeight(float value)
	{
		if (mLeftFootIKWeight != value)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.LeftFoot, value);
			}
			mLeftFootIKWeight = value;
		}
	}

	public void SetLeftFootIKPosition(Vector3 ikPosition)
	{
		if (mLeftFootIKPosition != ikPosition)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.LeftFoot, ikPosition);
			}
			mLeftFootIKPosition = ikPosition;
		}
	}

	public void SetRightFootIKWeight(float value)
	{
		if (mRightFootWeight != value)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosWeight, AvatarIKGoal.RightFoot, value);
			}
			mRightFootWeight = value;
		}
	}

	public void SetRightFootIKPosition(Vector3 ikPosition)
	{
		if (mRightFootIKPosition != ikPosition)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_IKPosition, AvatarIKGoal.RightFoot, ikPosition);
			}
			mRightFootIKPosition = ikPosition;
		}
	}

	public void LookAtWeight(float value)
	{
		if (mLookAtWeight != value)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_LookAtWeight, value);
			}
			mLookAtWeight = value;
		}
	}

	public void LookAtPosition(Vector3 ikPosition)
	{
		if (mLookAtPosition != ikPosition)
		{
			if (GameConfig.IsMultiMode && IsController)
			{
				RPCServer(EPacketType.PT_AI_LookAtPos, ikPosition);
			}
			mLookAtPosition = ikPosition;
		}
	}

	public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex)
	{
		return m_animator.GetCurrentAnimatorStateInfo(layerIndex);
	}

	public bool IsInTransition(int layerIndex)
	{
		return m_animator.IsInTransition(layerIndex);
	}

	public void SetFloat(string name, float value)
	{
		m_animator.SetFloat(name, value);
	}

	public void SetFloat(int id, float value)
	{
		m_animator.SetFloat(id, value);
	}

	public void SetFloat(string name, float value, float dampTime, float deltaTime)
	{
		m_animator.SetFloat(name, value, dampTime, deltaTime);
	}

	public void SetFloat(int id, float value, float dampTime, float deltaTime)
	{
		m_animator.SetFloat(id, value, dampTime, deltaTime);
	}

	public void SetBool(string name, bool value)
	{
		if (GameConfig.IsMultiMode && IsController && value != m_animator.GetBool(name))
		{
			RPCServer(EPacketType.PT_AI_BoolString, name, value);
		}
		m_animator.SetBool(name, value);
	}

	public void SetBool(int id, bool value)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_BoolInt, id, value);
		}
		m_animator.SetBool(id, value);
	}

	public void SetVector(string name, Vector3 value)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_VectorString, name, value);
		}
		m_animator.SetVector(name, value);
	}

	public void SetVector(int id, Vector3 value)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_VectorInt, id, value);
		}
		m_animator.SetVector(id, value);
	}

	public void SetInteger(string name, int value)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_IntString, name, value);
		}
		m_animator.SetInteger(name, value);
	}

	public void SetInteger(int id, int value)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_IntInt, id, value);
		}
		m_animator.SetInteger(id, value);
	}

	public virtual void SetLayerWeight(int layerIndex, float weight)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_LayerWeight, layerIndex, weight);
		}
		m_animator.SetLayerWeight(layerIndex, weight);
	}

	public void SetLookAtWeight(float weight)
	{
		m_animator.SetLookAtWeight(weight);
	}

	public void SetLookAtPosition(Vector3 lookAtPosition)
	{
		m_animator.SetLookAtPosition(lookAtPosition);
	}

	public void SetIKPositionWeight(AvatarIKGoal goal, float value)
	{
		m_animator.SetIKPositionWeight(goal, value);
	}

	public void SetIKPosition(AvatarIKGoal goal, Vector3 goalPosition)
	{
		m_animator.SetIKPosition(goal, goalPosition);
	}

	public void SetIKRotationWeight(AvatarIKGoal goal, float value)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_IKRotWeight, goal, value);
		}
		m_animator.SetIKRotationWeight(goal, value);
	}

	public void SetIKRotation(AvatarIKGoal goal, Quaternion goalPosition)
	{
		if (GameConfig.IsMultiMode && IsController)
		{
			RPCServer(EPacketType.PT_AI_IKRotation, goal, goalPosition);
		}
		m_animator.SetIKRotation(goal, goalPosition);
	}

	public Transform GetBoneTransform(HumanBodyBones humanBodyId)
	{
		return m_animator.GetBoneTransform(humanBodyId);
	}

	public Vector3 GetIKPosition(AvatarIKGoal goal)
	{
		return m_animator.GetIKPosition(goal);
	}

	public float GetIKPositionWeight(AvatarIKGoal goal)
	{
		return m_animator.GetIKPositionWeight(goal);
	}

	public void NetWorkSetFloat(string name, float value)
	{
		m_animator.SetFloat(name, value);
	}

	public void NetWorkSetFloat(int id, float value)
	{
		m_animator.SetFloat(id, value);
	}

	public void NetWorkSetFloat(string name, float value, float dampTime, float deltaTime)
	{
		m_animator.SetFloat(name, value, dampTime, deltaTime);
	}

	public void NetWorkSetFloat(int id, float value, float dampTime, float deltaTime)
	{
		m_animator.SetFloat(id, value, dampTime, deltaTime);
	}

	public void NetWorkSetBool(string name, bool value)
	{
		m_animator.SetBool(name, value);
	}

	public void NetWorkSetBool(int id, bool value)
	{
		m_animator.SetBool(id, value);
	}

	public void NetWorkSetVector(string name, Vector3 value)
	{
		m_animator.SetVector(name, value);
	}

	public void NetWorkSetVector(int id, Vector3 value)
	{
		m_animator.SetVector(id, value);
	}

	public void NetWorkSetInteger(string name, int value)
	{
		m_animator.SetInteger(name, value);
	}

	public void NetWorkSetInteger(int id, int value)
	{
		m_animator.SetInteger(id, value);
	}

	public void NetWorkSetLayerWeight(int layerIndex, float weight)
	{
		m_animator.SetLayerWeight(layerIndex, weight);
	}

	public void NetWorkSetLookAtWeight(float weight)
	{
		m_animator.SetLookAtWeight(weight);
	}

	public void NetWorkSetLookAtPosition(Vector3 lookAtPosition)
	{
		m_animator.SetLookAtPosition(lookAtPosition);
	}

	public void NetWorkSetIKPositionWeight(AvatarIKGoal goal, float value)
	{
		m_animator.SetIKPositionWeight(goal, value);
	}

	public void NetWorkSetIKPosition(AvatarIKGoal goal, Vector3 goalPosition)
	{
		m_animator.SetIKPosition(goal, goalPosition);
	}

	public void NetWorkSetIKRotationWeight(AvatarIKGoal goal, float value)
	{
		m_animator.SetIKRotationWeight(goal, value);
	}

	public void NetWorkSetIKRotation(AvatarIKGoal goal, Quaternion goalPosition)
	{
		m_animator.SetIKRotation(goal, goalPosition);
	}

	protected virtual void InitAttackData()
	{
	}

	public virtual void ApplyDamage(float damage)
	{
	}

	public virtual void ApplyDamage(Transform hurter, float damage)
	{
	}

	protected virtual void OnDeath()
	{
		HandleTheDeathEvent();
	}

	protected virtual void OnDamage(float damage)
	{
	}

	protected virtual void OnDamage(Transform hurter, float damage)
	{
	}

	public virtual void OnKilled(GameObject dead)
	{
	}

	protected virtual void OnBeKilled(GameObject killer)
	{
	}

	internal override byte GetBuilderId()
	{
		return 0;
	}

	internal override float GetAtkDist(ISkillTarget target)
	{
		return 0f;
	}

	internal override ItemPackage GetItemPackage()
	{
		return null;
	}

	internal override bool IsEnemy(ISkillTarget target)
	{
		return true;
	}

	internal override ISkillTarget GetTargetInDist(float dist, int targetMask)
	{
		return null;
	}

	internal override List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target)
	{
		List<ISkillTarget> list = new List<ISkillTarget>();
		Vector3 vector = base.transform.position;
		if (scope.m_centerType == -1)
		{
			vector = base.transform.position;
		}
		if (scope.m_centerType == -2)
		{
			vector = center;
		}
		Collider[] array = Physics.OverlapSphere(vector, scope.m_radius + radius, 0);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.isTrigger || collider.gameObject == base.gameObject)
			{
				continue;
			}
			if (GameConfig.IsMultiMode)
			{
				SkillRunner componentOrOnParent = VCUtils.GetComponentOrOnParent<SkillRunner>(collider.gameObject);
				if (null == componentOrOnParent)
				{
					continue;
				}
				int dstHarmID = AiUtil.GetHarm(componentOrOnParent.gameObject);
				if (AiHarmData.GetHarmValue(harm, dstHarmID) == 0)
				{
					continue;
				}
			}
			else
			{
				int dstHarmID2 = AiUtil.GetHarm(collider.gameObject);
				if (AiHarmData.GetHarmValue(harm, dstHarmID2) == 0)
				{
					continue;
				}
			}
			float f = AiMath.Dot(base.transform, collider.transform);
			float num = Mathf.Acos(f) * 57.29578f;
			if (!(num > scope.m_degEnd) && !(num < scope.m_degStart))
			{
				SkillRunner componentOrOnParent2 = VCUtils.GetComponentOrOnParent<SkillRunner>(collider.gameObject);
				if (componentOrOnParent2 != null && componentOrOnParent2 != this && !list.Contains(componentOrOnParent2))
				{
					list.Add(componentOrOnParent2);
				}
			}
		}
		return list;
	}

	internal override void ApplyDistRepel(SkillRunner caster, float distRepel)
	{
	}

	internal override void ApplyHpChange(SkillRunner caster, float hpChange, float damagePercent, int type)
	{
		if (OnBuff(Buff_Sp.INVENSIBLE) || GameConfig.IsMultiMode)
		{
			return;
		}
		float num = (float)defence / (float)defence;
		if (caster != null)
		{
			float num2 = caster.GetAttribute(AttribType.Atk) * damagePercent + hpChange;
			float num3 = num2 * (1f - num) * UnityEngine.Random.Range(0.9f, 1.1f);
			int damageType = ((type != 0) ? type : AiDamageTypeData.GetDamageType(caster));
			num3 *= AiDamageTypeData.GetDamageScale(damageType, defenceType);
			if (caster.gameObject.Equals(base.gameObject))
			{
				ApplyDamage(Mathf.CeilToInt(num3));
			}
			else
			{
				ApplyDamage(caster.transform, Mathf.CeilToInt(num3));
			}
		}
		else
		{
			float f = hpChange * (1f - num) * UnityEngine.Random.Range(0.9f, 1.1f);
			ApplyDamage(Mathf.CeilToInt(f));
		}
	}

	internal override void ApplyComfortChange(float comfortChange)
	{
	}

	internal override void ApplySatiationChange(float satiationChange)
	{
	}

	internal override void ApplyThirstLvChange(float thirstLvChange)
	{
	}

	internal override void ApplyAnim(List<string> animName)
	{
	}

	public void StopMove()
	{
		desiredMoveDestination = Vector3.zero;
		desiredMovementDirection = Vector3.zero;
	}

	public void StopRotation()
	{
		desiredLookAtTransform = null;
		desiredFaceDirection = Vector3.zero;
	}

	public void StopMoveAndRotation()
	{
		if (!GameConfig.IsMultiMode || IsController)
		{
			StopMove();
			StopRotation();
		}
	}

	protected void InitializeControllerData()
	{
		speed = walkSpeed;
		m_canAttack = true;
		m_canMove = true;
		m_canRotate = true;
	}

	public virtual bool CanRotate()
	{
		return m_canRotate && !m_isDead && !OnBuff(Buff_Sp.STUNNED);
	}

	public virtual bool CanMove()
	{
		return m_canMove && !m_isDead && !OnBuff(Buff_Sp.MOVE_NOT) && !OnBuff(Buff_Sp.STUNNED);
	}

	public virtual bool CanAttack()
	{
		return m_canAttack && !OnBuff(Buff_Sp.ATTACK_NOT);
	}

	public virtual bool CanAiWorking()
	{
		return !m_isDead && !OnBuff(Buff_Sp.STUNNED);
	}

	protected virtual void OnShowLifeBar()
	{
	}

	protected virtual void PlayAnimationAudio(string name)
	{
	}

	protected virtual void Awake()
	{
		m_motor = GetComponent<AiCharacterMotor>();
		m_controller = GetComponent<CharacterController>();
		m_rigidbody = GetComponent<Rigidbody>();
		m_animation = GetComponentInChildren<Animation>();
		m_animator = GetComponentInChildren<Animator>();
		InitializeData();
		InitializeControllerData();
		InitAttackData();
		InitCenter();
		ActivateRagdoll(active: false);
	}

	private void InitCenter()
	{
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = componentsInChildren;
		foreach (Rigidbody rigidbody in array)
		{
			if (!(rigidbody == null) && rigidbody.gameObject.layer == 15 && !(rigidbody.GetComponent<CharacterJoint>() != null))
			{
				mCenter = rigidbody.transform;
				break;
			}
		}
	}

	public void RemoveAiBehave()
	{
		if (m_behave != null)
		{
			UnityEngine.Object.Destroy(m_behave.gameObject);
			m_behave = null;
		}
	}

	public void SetupAibehave(GameObject aiPrefab)
	{
		if (aiPrefab == null)
		{
			return;
		}
		if (m_behave != null)
		{
			UnityEngine.Object.Destroy(m_behave.gameObject);
			m_behave = null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(aiPrefab, base.transform.position, base.transform.rotation) as GameObject;
		if (gameObject == null)
		{
			Debug.LogError("Instantiate ai prefab failed.");
			return;
		}
		gameObject.transform.parent = base.transform;
		m_behave = gameObject.GetComponent<AiBehave>();
		if (m_behave == null)
		{
			Debug.LogError("cant find AiBehaveTree.");
			return;
		}
		AiBehaveSingle aiBehaveSingle = m_behave as AiBehaveSingle;
		if (aiBehaveSingle != null)
		{
			aiBehaveSingle.RegisterAiObject(this);
		}
		else
		{
			Debug.LogWarning(string.Concat("ai [", m_behave, "] is not a AiBehaveSingle"));
		}
	}

	protected virtual void Start()
	{
		m_spawnPosition = base.transform.position;
		m_offset = base.transform.localPosition;
		SetupAibehave(aiObject);
		HandleTheSpawnedEvent();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		m_effSkillInsts.Clear();
		m_effShareSkillInsts.Clear();
		m_buffSum.Clear();
	}

	protected virtual void OnDestroy()
	{
		this.DeathHandlerEvent = null;
		HandleTheDestroyEvent();
	}

	private void RemoveItem()
	{
	}

	public virtual void DestroyLOD(Vector3 center, Vector3 size, float delayTime = 0f)
	{
		Delete(delayTime);
	}

	public virtual void Delete(float delayTime = 0f)
	{
		RemoveItem();
		AiAlpha componentInChildren = GetComponentInChildren<AiAlpha>();
		if (componentInChildren != null && base.gameObject.activeSelf)
		{
			componentInChildren.ChangeAlphaToValue(0f, delayTime);
			UnityEngine.Object.Destroy(base.gameObject, delayTime + 2f);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject, delayTime);
		}
	}

	protected virtual void Update()
	{
		AiUpdate();
		SumupBuff();
	}

	private void SumupBuff()
	{
		if (m_effSkillBuffManager.m_bEffBuffDirty)
		{
			m_buffSum = EffSkillBuffSum.Sumup(m_effSkillBuffManager.m_effBuffInstList);
			m_buffSumMul.ResetBuffMultiply(m_effSkillBuffManager.m_effBuffInstList);
			m_effSkillBuffManager.m_bEffBuffDirty = false;
		}
	}

	public void SetCamp(int iCamp)
	{
		m_camp = iCamp;
	}

	public void HandleTheSpawnedEvent()
	{
		if (this.SpawnedHandlerEvent != null)
		{
			this.SpawnedHandlerEvent(this);
		}
	}

	public void HandleTheDeathEvent()
	{
		if (this.DeathHandlerEvent != null)
		{
			this.DeathHandlerEvent(this);
		}
	}

	public void HandleTheDestroyEvent()
	{
		if (this.DestroyHandlerEvent != null)
		{
			this.DestroyHandlerEvent(this);
		}
	}

	public void HandleActiveEvent()
	{
		if (this.ActiveHandlerEvent != null)
		{
			this.ActiveHandlerEvent(this);
		}
	}

	public void HandleInactiveEvent()
	{
		if (this.InactiveHandlerEvent != null)
		{
			this.InactiveHandlerEvent(this);
		}
	}

	public virtual void SetOwennerView()
	{
		if (IsController && m_isActive)
		{
			if (motor != null && !m_isDead)
			{
				motor.enabled = true;
			}
			if (null != behave && !m_isDead)
			{
				behave.enabled = true;
			}
			if (GetComponent<Rigidbody>() != null)
			{
				GetComponent<Rigidbody>().WakeUp();
			}
			Animator component = GetComponent<Animator>();
			if (component != null && !m_isDead)
			{
				component.applyRootMotion = true;
			}
		}
		else
		{
			if (motor != null)
			{
				motor.enabled = false;
			}
			if (null != behave)
			{
				behave.enabled = false;
			}
			if (GetComponent<Rigidbody>() != null)
			{
				GetComponent<Rigidbody>().Sleep();
			}
			Animator component2 = GetComponent<Animator>();
			if (component2 != null)
			{
				component2.applyRootMotion = false;
			}
		}
	}

	protected virtual void ActivateRagdoll(bool active)
	{
		if (model == null)
		{
			return;
		}
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			if (active || m_isDead)
			{
				component.enabled = false;
			}
			else
			{
				component.enabled = true;
			}
		}
		Animator component2 = GetComponent<Animator>();
		if (component2 != null)
		{
			if (active || m_isDead)
			{
				component2.enabled = false;
			}
			else
			{
				component2.enabled = true;
			}
		}
		Animation component3 = model.GetComponent<Animation>();
		if (component3 != null)
		{
			if (active || m_isDead)
			{
				component3.enabled = false;
			}
			else
			{
				component3.enabled = true;
			}
		}
		ArmAimer component4 = GetComponent<ArmAimer>();
		if (component4 != null)
		{
			if (active || m_isDead)
			{
				component4.enabled = false;
			}
			else
			{
				component4.enabled = true;
			}
		}
		LegAnimator componentInChildren = GetComponentInChildren<LegAnimator>();
		if (componentInChildren != null && active)
		{
			componentInChildren.enabled = false;
		}
		Rigidbody[] componentsInChildren = model.GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = componentsInChildren;
		foreach (Rigidbody rigidbody in array)
		{
			if (active)
			{
				rigidbody.WakeUp();
			}
			else
			{
				rigidbody.Sleep();
			}
			rigidbody.isKinematic = !active;
		}
	}

	private void ActivateAlpha(bool value)
	{
		if (value)
		{
			SetActive(value: true);
			AiAlpha componentInChildren = GetComponentInChildren<AiAlpha>();
			if (componentInChildren != null)
			{
				componentInChildren.ChangeAlphaToValue(1f);
			}
			if (IsInvoking("DeActive"))
			{
				CancelInvoke("DeActive");
			}
		}
		else
		{
			AiAlpha componentInChildren2 = GetComponentInChildren<AiAlpha>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.ChangeAlphaToValue(0f);
				Invoke("DeActive", 2f);
			}
		}
	}

	protected virtual void ActivateMultiOwener(bool value)
	{
		m_isActive = value;
		SetOwennerView();
		ActivateAlpha(value);
	}

	protected virtual void ActivateMultiProxy(bool value)
	{
		m_isActive = value;
		SetOwennerView();
		ActivateAlpha(value);
	}

	protected virtual void ActivateSingleMode(bool value)
	{
		m_isActive = value;
		if (motor != null)
		{
			if (m_isActive && !m_isDead)
			{
				motor.enabled = true;
			}
			else
			{
				motor.enabled = false;
			}
		}
		if (behave != null)
		{
			if (m_isActive && !m_isDead)
			{
				behave.enabled = true;
			}
			else
			{
				behave.enabled = false;
			}
		}
		Animator component = GetComponent<Animator>();
		if (component != null)
		{
			if (m_isActive && !m_isDead)
			{
				component.applyRootMotion = true;
			}
			else
			{
				component.applyRootMotion = false;
			}
		}
		ActivateAlpha(value);
	}

	protected void NetwrokSwitchController(bool value)
	{
		if (IsController)
		{
			ActivateMultiOwener(value);
		}
		else
		{
			ActivateMultiProxy(value);
		}
	}

	public virtual void Activate(bool value)
	{
		if (m_isActive == value)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			NetwrokSwitchController(value);
		}
		else
		{
			ActivateSingleMode(value);
		}
		if (m_isDead)
		{
			if (m_isActive)
			{
				ActivateRagdoll(active: true);
			}
			else
			{
				ActivateRagdoll(active: false);
			}
		}
	}

	public virtual void Activate(bool value, IntVector4 node)
	{
	}

	public virtual void Activate(bool value, Bounds bounds)
	{
		if (value)
		{
			mHideBounds = default(Bounds);
		}
		else
		{
			mHideBounds = bounds;
		}
		if (GameConfig.IsMultiMode)
		{
			Activate(value);
		}
		else
		{
			Activate(value);
		}
	}

	private void DeActive()
	{
		SetActive(value: false);
	}

	private void SetActive(bool value)
	{
		if (base.gameObject.activeSelf != value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				HandleActiveEvent();
			}
			else
			{
				HandleInactiveEvent();
			}
		}
	}

	public bool OnBuff(Buff_Sp _sp)
	{
		return (m_buffSum.m_buffSp & (short)(1 << (short)_sp - 1)) > 0;
	}

	public void CancelBuffSp(Buff_Sp _sp)
	{
		List<EffSkillBuffInst> list = m_effSkillBuffManager.m_effBuffInstList.FindAll((EffSkillBuffInst ret) => ret.m_buff.m_buffSp == (short)_sp);
		foreach (EffSkillBuffInst item in list)
		{
			m_effSkillBuffManager.m_effBuffInstList.Remove(item);
		}
	}

	protected virtual void AiUpdate()
	{
		if (m_animation != null)
		{
			UpdateAnimation();
		}
		if (m_animator != null)
		{
			UpdateAnimator();
		}
		OnShowLifeBar();
	}

	protected virtual void InitializeData()
	{
		m_camp = 0;
		m_life = 1;
		m_startShowLifeBarTime = 0f;
		m_isDead = false;
		m_isSleep = false;
		m_isActive = true;
		m_buffSum = new EffSkillBuffSum();
		m_buffSumMul = new EffSkillBuffMultiply();
	}

	protected virtual bool ClearTarget()
	{
		return true;
	}
}
