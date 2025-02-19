using System.Collections.Generic;
using Pathea;
using PETools;
using SkillSystem;
using UnityEngine;

namespace WhiteCat;

public abstract class AIBehaviourController : BehaviourController
{
	private SkAliveEntity _attackOwnerEntity;

	private ViewCmpt _attackOwnerView;

	private SkAliveEntity _attackSelfEntity;

	private ViewCmpt _attackSelfView;

	private SkAliveEntity _ownerAttackEntity;

	private ViewCmpt _ownerAttackView;

	private CarrierController _ownerCarrier;

	private AIMode _aiMode;

	private float _timeCountForFindEnemy;

	private float _timeCountForCure;

	private bool _isAttackMode;

	private SkEntity _aimEntity;

	private ViewCmpt _aimView;

	private Vector3 _aimPoint;

	private List<float> _cureValueList;

	private static List<int> _cureIdxList;

	protected abstract AIMode defaultAIMode { get; }

	public AIMode aiMode
	{
		get
		{
			return _aiMode;
		}
		set
		{
			_aiMode = value;
		}
	}

	public override bool isAttackMode => _isAttackMode;

	public CarrierController ownerCarrier => _ownerCarrier;

	public override SkEntity attackTargetEntity => _aimEntity;

	public override Vector3 attackTargetPoint => _aimPoint;

	protected virtual float cureOwnerHpPerSecond => 0f;

	static AIBehaviourController()
	{
		_cureIdxList = new List<int>();
		_cureIdxList.Add(0);
	}

	public void SetCreater(int peEntityID)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(peEntityID);
		if ((bool)peEntity)
		{
			if (base.energy > 0f)
			{
				SetOwner(peEntity);
			}
		}
		else
		{
			SetOwner(null);
		}
	}

	protected override void OnOwnerChange(PESkEntity oldOwner, PESkEntity newOwner)
	{
		if ((bool)oldOwner)
		{
			oldOwner.onHpReduce -= OnOwnerHpReduce;
			oldOwner.attackEvent -= OnOwnerAttack;
		}
		if ((bool)newOwner)
		{
			newOwner.onHpReduce += OnOwnerHpReduce;
			newOwner.attackEvent += OnOwnerAttack;
		}
	}

	private void OnOwnerHpReduce(SkEntity enemy, float value)
	{
		enemy = PEUtil.GetCaster(enemy);
		if (enemy is SkAliveEntity && !enemy.Equals(null))
		{
			_attackOwnerEntity = enemy as SkAliveEntity;
			_attackOwnerView = enemy.GetComponent<ViewCmpt>();
		}
	}

	private void OnOwnerAttack(SkEntity enemy, float value)
	{
		if (enemy is SkAliveEntity && !enemy.Equals(null))
		{
			_ownerAttackEntity = enemy as SkAliveEntity;
			_ownerAttackView = enemy.GetComponent<ViewCmpt>();
		}
	}

	protected override void InitOtherThings()
	{
		if (!PeGameMgr.IsMulti)
		{
			if (base.energy > 0f)
			{
				SetOwner(PeSingleton<MainPlayer>.Instance.entity);
			}
			ResetHost(PeSingleton<MainPlayer>.Instance.entity.Id);
		}
		base.creationSkEntity.onHpReduce += delegate(SkEntity enemy, float value)
		{
			if (enemy is SkAliveEntity && !enemy.Equals(null))
			{
				_attackSelfEntity = enemy as SkAliveEntity;
				_attackSelfView = enemy.GetComponent<ViewCmpt>();
			}
		};
		_aiMode = defaultAIMode;
		_cureValueList = new List<float>();
		_cureValueList.Add(0f);
	}

	private void CheckOwnerCarrier()
	{
		if ((bool)base.ownerEntity && _ownerCarrier != base.ownerEntity.passengerCmpt.carrier)
		{
			if ((bool)_ownerCarrier)
			{
				_ownerCarrier.creationSkEntity.onHpReduce -= OnOwnerHpReduce;
				_ownerCarrier.creationSkEntity.attackEvent -= OnOwnerAttack;
			}
			_ownerCarrier = base.ownerEntity.passengerCmpt.carrier;
			if ((bool)_ownerCarrier)
			{
				_ownerCarrier.creationSkEntity.onHpReduce += OnOwnerHpReduce;
				_ownerCarrier.creationSkEntity.attackEvent += OnOwnerAttack;
			}
		}
	}

	protected void UpdateAttactTarget()
	{
		CheckOwnerCarrier();
		SkEntity skEntity = null;
		ViewCmpt viewCmpt = null;
		if (_aiMode != AIMode.Passive && _aiMode != AIMode.Cure)
		{
			if ((bool)_attackOwnerEntity && (bool)_attackOwnerView && !_attackOwnerEntity.isDead && _attackOwnerView.hasView && _attackOwnerEntity.Entity != base.ownerEntity)
			{
				skEntity = _attackOwnerEntity;
				viewCmpt = _attackOwnerView;
			}
			else
			{
				_attackOwnerEntity = null;
				_attackOwnerView = null;
				if ((bool)_attackSelfEntity && (bool)_attackSelfView && !_attackSelfEntity.isDead && _attackSelfView.hasView && _attackSelfEntity.Entity != base.ownerEntity)
				{
					skEntity = _attackSelfEntity;
					viewCmpt = _attackSelfView;
				}
				else
				{
					_attackSelfEntity = null;
					_attackSelfView = null;
					if ((bool)_ownerAttackEntity && (bool)_ownerAttackView && !_ownerAttackEntity.isDead && _ownerAttackView.hasView && _ownerAttackEntity.Entity != base.ownerEntity)
					{
						skEntity = _ownerAttackEntity;
						viewCmpt = _ownerAttackView;
					}
					else
					{
						_ownerAttackEntity = null;
						_ownerAttackView = null;
						if (_aiMode == AIMode.Attack)
						{
							if ((bool)_aimEntity && (bool)_aimView && _aimView.hasView && _aimEntity is SkAliveEntity && !(_aimEntity as SkAliveEntity).isDead && (_aimView.Entity.position - base.transform.position).sqrMagnitude < PEVCConfig.instance.sqrRobotAttackRange)
							{
								skEntity = _aimEntity;
								viewCmpt = _aimView;
							}
							else
							{
								_timeCountForFindEnemy += Time.deltaTime;
								if (_timeCountForFindEnemy > 0.77f)
								{
									_timeCountForFindEnemy = 0f;
									foreach (KeyValuePair<int, PeEntity> item in PeSingleton<EntityMgr>.Instance.mDicEntity)
									{
										if (item.Value.hasView && !item.Value.IsDeath() && (item.Value.position - base.transform.position).sqrMagnitude < PEVCConfig.instance.sqrRobotAttackRange && PEUtil.CanAttack(base.creationPeEntity, item.Value))
										{
											skEntity = item.Value.skEntity;
											viewCmpt = item.Value.viewCmpt;
											break;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		else if (_aiMode == AIMode.Cure)
		{
			OnCureModeUpdate();
		}
		_isAttackMode = skEntity;
		if (_isAttackMode)
		{
			_aimPoint = viewCmpt.centerPosition;
		}
		_aimEntity = skEntity;
		_aimView = viewCmpt;
		SetWeaponControlEnabled(WeaponType.AI, isAttackMode && (attackTargetPoint - base.transform.position).sqrMagnitude < PEVCConfig.instance.sqrRobotAttackRange);
	}

	private void OnCureModeUpdate()
	{
		_timeCountForCure += Time.deltaTime;
		if (_timeCountForCure >= 1f)
		{
			_timeCountForCure = 0f;
			if (base.ownerSkEntity != null && !base.ownerSkEntity.isDead && base.ownerSkEntity.HPPercent < 1f)
			{
				ExpendEnergy(PEVCConfig.instance.robotCureExpendEnergyPerSecond);
				_cureValueList[0] = cureOwnerHpPerSecond;
				SkEntity.MountBuff(base.ownerSkEntity, 30200169, _cureIdxList, _cureValueList);
			}
		}
	}

	protected override void InitNetwork()
	{
		_netPosition = new NetData<Vector3>((Vector3 last) => (base.transform.position - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrDistance, () => base.transform.position, delegate(Vector3 value)
		{
			if (!base.rigidbody.isKinematic)
			{
				base.rigidbody.position = value;
			}
		});
		_netRotation = new NetData<Quaternion>((Quaternion last) => Quaternion.Angle(base.transform.rotation, last) >= PEVCConfig.instance.minSyncAngle, () => base.transform.rotation, delegate(Quaternion value)
		{
			if (!base.rigidbody.isKinematic)
			{
				base.rigidbody.rotation = value;
			}
		});
		_netVelocity = new NetData<Vector3>((Vector3 last) => (base.rigidbody.velocity - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrSpeed, () => base.rigidbody.velocity, delegate(Vector3 value)
		{
			base.rigidbody.velocity = value;
		});
		_netAngularVelocity = new NetData<Vector3>((Vector3 last) => (base.rigidbody.angularVelocity - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrAngularSpeed, () => base.rigidbody.angularVelocity, delegate(Vector3 value)
		{
			base.rigidbody.angularVelocity = value;
		});
		_netAimPoint = new NetData<Vector3>((Vector3 last) => (_aimPoint - last).sqrMagnitude >= PEVCConfig.instance.minSyncSqrAimPoint, () => _aimPoint, delegate(Vector3 value)
		{
			_aimPoint = value;
		});
		_netInput = new NetData<ushort>((ushort last) => _isAttackMode != (last == 1), () => (ushort)(_isAttackMode ? 1u : 0u), delegate(ushort value)
		{
			_isAttackMode = value == 1;
		});
	}

	protected override void OnNetworkSync()
	{
	}
}
