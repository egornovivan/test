using System;
using ItemAsset;
using Pathea;
using PETools;
using UnityEngine;

namespace WhiteCat;

public class RobotController : AIBehaviourController
{
	private static RobotController _playerFollower;

	private VCPRobotController _controller;

	private TrailRenderer _trail;

	private bool _active;

	private float _updateTargetPositionCountDown;

	private Vector3 _relativeTargetPosition;

	private Vector3 _targetPosition;

	private bool _collided;

	public static RobotController playerFollower => _playerFollower;

	public bool isActive => _active;

	public override bool isAttackMode => _active && base.isAttackMode;

	protected override AIMode defaultAIMode => AIMode.Defence;

	protected override float mass => Mathf.Clamp(base.creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.robotMassScale, PEVCConfig.instance.robotMinMass, PEVCConfig.instance.robotMaxMass);

	protected override Vector3 centerOfMass => Vector3.zero;

	protected override Vector3 inertiaTensorScale => Vector3.one;

	protected override float cureOwnerHpPerSecond => _controller.cureOwnerHpPerSecond;

	public static event Action<ItemObject, GameObject> onPlayerGetRobot;

	public static event Action onPlayerLoseRobot;

	protected override void OnOwnerChange(PESkEntity oldOwner, PESkEntity newOwner)
	{
		base.OnOwnerChange(oldOwner, newOwner);
		if ((bool)oldOwner)
		{
			PeEntity component = oldOwner.GetComponent<PeEntity>();
			if (PeSingleton<PeCreature>.Instance.mainPlayerId == component.Id)
			{
				_playerFollower = null;
				if (RobotController.onPlayerLoseRobot != null)
				{
					RobotController.onPlayerLoseRobot();
				}
			}
		}
		if (!newOwner)
		{
			return;
		}
		PeEntity component2 = newOwner.GetComponent<PeEntity>();
		if (PeSingleton<PeCreature>.Instance.mainPlayerId == component2.Id)
		{
			_playerFollower = this;
			if (RobotController.onPlayerGetRobot != null)
			{
				RobotController.onPlayerGetRobot(base.itemObject, base.gameObject);
			}
		}
	}

	protected override void InitDrags(out float standardDrag, out float underwaterDrag, out float standardAngularDrag, out float underwaterAngularDrag)
	{
		standardDrag = PEVCConfig.instance.robotStandardDrag;
		underwaterDrag = PEVCConfig.instance.robotUnderwaterDrag;
		standardAngularDrag = PEVCConfig.instance.robotStandardAngularDrag;
		underwaterAngularDrag = PEVCConfig.instance.robotUnderwaterAngularDrag;
	}

	protected override void InitOtherThings()
	{
		base.InitOtherThings();
		Transform transform = UnityEngine.Object.Instantiate(PEVCConfig.instance.robotTrail).transform;
		transform.SetParent(base.transform, worldPositionStays: false);
		_trail = transform.GetComponentInChildren<TrailRenderer>();
		base.gameObject.AddComponent<ItemScript>();
		base.gameObject.AddComponent<DragItemMousePickRobot>().Init(this);
		_controller = GetComponentInChildren<VCPRobotController>();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		_active = base.energy > 0f && (bool)base.ownerSkEntity;
		base.rigidbody.useGravity = !_active;
		base.rigidbody.constraints = ((!_active) ? ((RigidbodyConstraints)122) : RigidbodyConstraints.None);
		if (!_active)
		{
			ChangeOwner(null);
		}
		else
		{
			if (!base.isPlayerHost)
			{
				return;
			}
			ExpendEnergy(_controller.energyExpendSpeed * Time.deltaTime);
			UpdateAttactTarget();
			_updateTargetPositionCountDown -= Time.deltaTime;
			if (_collided || _updateTargetPositionCountDown <= 0f)
			{
				if (!base.creationController.visible && (base.ownerEntity.position - base.transform.position).sqrMagnitude > 10000f)
				{
					FlashMove();
				}
				UpdateRelativeTargetPosition();
				_collided = false;
				_updateTargetPositionCountDown = UnityEngine.Random.Range(1f, 10f);
			}
			_targetPosition = base.ownerEntity.position + _relativeTargetPosition;
			UpdateBehaviour();
			_trail.time = Mathf.Clamp(base.rigidbody.velocity.sqrMagnitude, 0f, 2f);
		}
	}

	private void OnCollisionEnter()
	{
		_collided = true;
	}

	private void FlashMove()
	{
		Transform mainCamTransform = PEUtil.MainCamTransform;
		Vector3 vector = ((!base.ownerCarrier) ? (base.ownerEntity.position + Vector3.up * 2f) : base.ownerCarrier.creationController.boundsCenterInWorld);
		Vector3 vector2 = Vector3.zero;
		for (int i = 0; i < 16; i++)
		{
			switch (i)
			{
			case 0:
				vector2.x = (vector2.z = (mainCamTransform.position - vector).magnitude);
				vector2 = (mainCamTransform.TransformPoint(vector2) - vector).normalized;
				break;
			case 1:
				vector2.x = 0f - (vector2.z = (mainCamTransform.position - vector).magnitude);
				vector2 = (mainCamTransform.TransformPoint(vector2) - vector).normalized;
				break;
			default:
				vector2 = UnityEngine.Random.onUnitSphere;
				vector2.y = Mathf.Abs(vector2.y);
				break;
			}
			float num = ((!base.ownerCarrier) ? 0.5f : base.ownerCarrier.creationController.BoundsRadius);
			RaycastHit hitInfo;
			Vector3 vector3 = ((!Physics.SphereCast(vector + num * vector2, base.creationController.robotRadius, vector2, out hitInfo, 64f, PEVCConfig.instance.getOffLayerMask)) ? (vector + 63f * vector2) : (hitInfo.point - vector2));
			if (Vector3.Dot((vector3 - mainCamTransform.position).normalized, mainCamTransform.forward) < 0.7f)
			{
				base.transform.position = vector3;
				break;
			}
		}
	}

	private void UpdateRelativeTargetPosition()
	{
		if ((bool)base.ownerCarrier)
		{
			float num;
			if (isAttackMode)
			{
				_relativeTargetPosition = attackTargetPoint - base.ownerCarrier.creationController.boundsCenterInWorld;
				num = Mathf.Atan2(_relativeTargetPosition.x, _relativeTargetPosition.z);
				num += UnityEngine.Random.Range(-0.25f, 0.25f) * (float)Math.PI;
			}
			else
			{
				num = UnityEngine.Random.Range(-1f, 1f) * (float)Math.PI;
			}
			float num2 = PEVCConfig.instance.randomRobotDistance + base.ownerCarrier.creationController.BoundsRadius;
			_relativeTargetPosition.x = Mathf.Sin(num) * num2;
			_relativeTargetPosition.z = Mathf.Cos(num) * num2;
			_relativeTargetPosition.y = PEVCConfig.instance.randomRobotHeight + base.ownerCarrier.creationController.BoundsRadius;
			_relativeTargetPosition = _relativeTargetPosition + base.ownerCarrier.creationController.boundsCenterInWorld - base.ownerEntity.position;
		}
		else
		{
			float num;
			if (isAttackMode)
			{
				_relativeTargetPosition = attackTargetPoint - base.ownerEntity.position;
				num = Mathf.Atan2(_relativeTargetPosition.x, _relativeTargetPosition.z);
				num += UnityEngine.Random.Range(-0.25f, 0.25f) * (float)Math.PI;
			}
			else
			{
				num = UnityEngine.Random.Range(-1f, 1f) * (float)Math.PI;
			}
			float randomRobotDistance = PEVCConfig.instance.randomRobotDistance;
			_relativeTargetPosition.x = Mathf.Sin(num) * randomRobotDistance;
			_relativeTargetPosition.z = Mathf.Cos(num) * randomRobotDistance;
			_relativeTargetPosition.y = PEVCConfig.instance.randomRobotHeight;
		}
	}

	private void UpdateBehaviour()
	{
		Vector3 target = (_targetPosition - base.transform.position) * PEVCConfig.instance.robotSpeedScale;
		if (target.sqrMagnitude > PEVCConfig.instance.maxSqrRigidbodySpeed)
		{
			target *= PEVCConfig.instance.maxRigidbodySpeed / target.magnitude;
		}
		target = Vector3.RotateTowards(base.rigidbody.velocity, target, Time.deltaTime * PEVCConfig.instance.robotVelocityRotateSpeed, Time.deltaTime * PEVCConfig.instance.robotVelocityChangeSpeed);
		float num = Time.timeSinceLevelLoad % PEVCConfig.instance.robotSwingPeriod / PEVCConfig.instance.robotSwingPeriod;
		base.rigidbody.velocity = target + Mathf.Sin(num * 2f * (float)Math.PI) * PEVCConfig.instance.robotSwingRange * Vector3.up;
		Quaternion to = (isAttackMode ? Quaternion.LookRotation(attackTargetPoint - base.transform.position) : ((!(target.x * target.x + target.z * target.z > 0.25f)) ? base.transform.rotation : Quaternion.LookRotation(target)));
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * PEVCConfig.instance.robotRotateSpeed);
	}

	protected override void OnHpChange(float deltaHp, bool isDead)
	{
		base.OnHpChange(deltaHp, isDead);
		if (isDead && _playerFollower == this)
		{
			_playerFollower = null;
			if (RobotController.onPlayerLoseRobot != null)
			{
				RobotController.onPlayerLoseRobot();
			}
		}
	}
}
