using System;
using Pathea;
using PeMap;
using SkillSystem;
using UnityEngine;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat;

public abstract class CarrierController : BehaviourController, IPeMsg
{
	private const float _rayMaxDistance = 300f;

	public const float inputSensitivity = 4f;

	public const int moveForwardBit = 0;

	public const int moveBackwardBit = 1;

	public const int moveLeftBit = 2;

	public const int moveRightBit = 3;

	public const int jetBit = 4;

	public const int lightBit = 5;

	public const int attackModeBit = 6;

	public const int brakeBit = 7;

	public const int moveUpBit = 8;

	public const int moveDownBit = 9;

	private static int ImpactTargetSkillID = 30100562;

	private static int ImpactSelfSkillID = 20110054;

	private VCPCockpit _cockpit;

	private VCPSideSeat[] _sideSeats;

	private VCPJetExhaust[] _jetExhausts;

	private VCPCarrierLight[] _lights;

	private int _passengerCount;

	private uint _inputState;

	private PESkEntity _aimEntity;

	private Vector3 _aimPoint;

	private float _timeToLock = -1f;

	private PESkEntity _targetToLock;

	private PESkEntity _lockedTarget;

	private ViewCmpt _targetViewToLock;

	private ViewCmpt _lockedTargetView;

	private float _inputX;

	private float _inputY;

	private float _inputVertical;

	private bool _autoDrive;

	private bool _isJetting;

	private bool _isLightOn;

	private bool _isAttackMode;

	private float _jetRestValue = 1f;

	private float _jetCDTime;

	private static CarrierController _playerDriving;

	private static GameObject _attackUICanvas;

	private static Ray _ray;

	private static PeInput.LogicFunction[] _weaponGroupKey = new PeInput.LogicFunction[4]
	{
		PeInput.LogicFunction.VehicleWeaponGrp1,
		PeInput.LogicFunction.VehicleWeaponGrp2,
		PeInput.LogicFunction.VehicleWeaponGrp3,
		PeInput.LogicFunction.VehicleWeaponGrp4
	};

	public static CarrierController playerDriving => _playerDriving;

	public float inputX => _inputX;

	public float inputY => _inputY;

	public float inputVertical => _inputVertical;

	public bool isJetting => _isJetting;

	public bool isLightOn => _isLightOn;

	public PESkEntity lockedTarget => _lockedTarget;

	public PESkEntity targetToLock => _targetToLock;

	public float timeToLock => _timeToLock;

	public PESkEntity aimEntity => _aimEntity;

	public VCPSideSeat[] sideSeats => _sideSeats;

	public bool hasDriver => _cockpit.passenger != null && !_cockpit.passenger.Equals(null);

	public bool isPlayerDriver => _playerDriving == this;

	public int passengerCount => _passengerCount;

	public PeEntity driver => (_cockpit.passenger == null || _cockpit.passenger.Equals(null)) ? null : (_cockpit.passenger as PassengerCmpt).Entity;

	public override SkEntity attackTargetEntity => _lockedTarget;

	public override Vector3 attackTargetPoint => _aimPoint;

	public override bool isAttackMode => _isAttackMode;

	public int FindEmptySeatIndex()
	{
		if (_cockpit.passenger == null || _cockpit.passenger.Equals(null))
		{
			return -1;
		}
		for (int i = 0; i < _sideSeats.Length; i++)
		{
			if (_sideSeats[i].passenger == null || _sideSeats[i].passenger.Equals(null))
			{
				return i;
			}
		}
		return -2;
	}

	public void GetOn(PeEntity entity, int seatIndex)
	{
		VCPBaseSeat vCPBaseSeat = null;
		if (seatIndex < 0)
		{
			vCPBaseSeat = _cockpit;
			ChangeOwner(entity);
			ResetHost(entity.Id);
			if (base.isPlayerHost)
			{
				_playerDriving = this;
				UIDrivingCtrl.Instance.Show(() => base.maxHp, () => base.hp, () => base.maxEnergy, () => base.energy, delegate
				{
					Vector3 velocity = base.rigidbody.velocity;
					return Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z) * 3.6f;
				}, () => (_jetExhausts.Length <= 0) ? 0f : _jetRestValue);
				vCPBaseSeat.getOffCallback += delegate
				{
					_playerDriving = null;
					UIDrivingCtrl.Instance.Hide();
					UIDrivingCtrl.Instance.SetWweaponGroupTogglesVisible(visible: false, this);
					UISightingTelescope.Instance.Show(UISightingTelescope.SightingType.Null);
				};
				TutorialData.AddActiveTutorialID(15);
			}
			vCPBaseSeat.getOffCallback += delegate
			{
				_inputState = OnDriverGetOff(_inputState);
				_aimEntity = null;
				_lockedTarget = null;
				_timeToLock = -1f;
				_isAttackMode = false;
				DisableAllWeaponControl();
				ActivateImpactDamage(isActive: false);
			};
			if (!GameConfig.IsMultiMode || IsController())
			{
				ActivateImpactDamage(isActive: true);
			}
		}
		else
		{
			vCPBaseSeat = _sideSeats[seatIndex];
		}
		_passengerCount++;
		vCPBaseSeat.GetOn(entity.GetCmpt<PassengerCmpt>());
		vCPBaseSeat.getOffCallback += delegate
		{
			_passengerCount--;
			if (_passengerCount == 0)
			{
				ChangeOwner(null);
			}
		};
	}

	public void ForeachPassenger(Action<PESkEntity, bool> action)
	{
		PassengerCmpt passengerCmpt = _cockpit.passenger as PassengerCmpt;
		if (passengerCmpt != null)
		{
			PESkEntity component = passengerCmpt.GetComponent<PESkEntity>();
			if (component != null)
			{
				action(component, arg2: true);
			}
		}
		for (int i = 0; i < _sideSeats.Length; i++)
		{
			passengerCmpt = _sideSeats[i].passenger as PassengerCmpt;
			if (passengerCmpt != null)
			{
				PESkEntity component = passengerCmpt.GetComponent<PESkEntity>();
				if (component != null)
				{
					action(component, arg2: false);
				}
			}
		}
	}

	public void ForeachPassenger(Action<PassengerCmpt, bool> action)
	{
		PassengerCmpt passengerCmpt = _cockpit.passenger as PassengerCmpt;
		if (passengerCmpt != null)
		{
			action(passengerCmpt, arg2: true);
		}
		for (int i = 0; i < _sideSeats.Length; i++)
		{
			passengerCmpt = _sideSeats[i].passenger as PassengerCmpt;
			if (passengerCmpt != null)
			{
				action(passengerCmpt, arg2: false);
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
		_netInput = new NetData<ushort>((ushort last) => (ushort)_inputState != last, () => (ushort)_inputState, delegate(ushort value)
		{
			_inputState = value;
		});
	}

	protected override void InitOtherThings()
	{
		base.gameObject.AddComponent<ImpactAtkTriggrer>();
		if (!_attackUICanvas)
		{
			_attackUICanvas = UnityEngine.Object.Instantiate(PEVCConfig.instance.canvasObject);
		}
		base.gameObject.AddComponent<ItemScript_Carrier>();
		base.gameObject.AddComponent<DragItemMousePickCarrier>();
		LoadPart(ref _cockpit);
		LoadParts(ref _sideSeats);
		LoadParts(ref _jetExhausts);
		LoadParts(ref _lights);
		VCPCarrierLight[] lights = _lights;
		foreach (VCPCarrierLight vCPCarrierLight in lights)
		{
			vCPCarrierLight.enabled = true;
		}
		float num = PEVCConfig.instance.maxJetAccelerate * base.rigidbody.mass;
		VCPJetExhaust[] jetExhausts = _jetExhausts;
		foreach (VCPJetExhaust vCPJetExhaust in jetExhausts)
		{
			vCPJetExhaust.Init(this, num / (float)_jetExhausts.Length, _jetExhausts.Length);
			vCPJetExhaust.enabled = true;
		}
		_cockpit.DestroyHumanModel();
		VCPSideSeat[] array = _sideSeats;
		foreach (VCPSideSeat vCPSideSeat in array)
		{
			vCPSideSeat.DestroyHumanModel();
		}
		CarrierMark carrierMark = new CarrierMark();
		carrierMark.carrierController = this;
		PeSingleton<LabelMgr>.Instance.Add(carrierMark);
	}

	protected override void OnHpChange(float deltaHp, bool isDead)
	{
		base.OnHpChange(deltaHp, isDead);
		if (!base.isPlayerHost)
		{
			return;
		}
		if (isDead)
		{
			ForeachPassenger(delegate(PESkEntity passenger, bool isDriver)
			{
				passenger.Kill(eventOff: false);
			});
			RemoveMapMark();
		}
		else if (deltaHp < 0f)
		{
			ForeachPassenger(delegate(PESkEntity passenger, bool isDriver)
			{
				float num = passenger.GetAttribute(AttribType.HpMax) * (deltaHp / base.maxHp) * PEVCConfig.instance.randomPassengerDamage;
				passenger.SetAttribute(AttribType.Hp, passenger.GetAttribute(AttribType.Hp) + num, eventOff: false);
			});
		}
	}

	protected override void OnNetworkSync()
	{
		_cockpit.SyncPassenger();
		for (int i = 0; i < _sideSeats.Length; i++)
		{
			_sideSeats[i].SyncPassenger();
		}
	}

	protected virtual uint OnDriverGetOff(uint inputState)
	{
		inputState = inputState.SetBit(0, is1: false);
		inputState = inputState.SetBit(1, is1: false);
		inputState = inputState.SetBit(2, is1: false);
		inputState = inputState.SetBit(3, is1: false);
		inputState = inputState.SetBit(4, is1: false);
		inputState = inputState.SetBit(6, is1: false);
		return inputState;
	}

	protected virtual uint EncodeInput(uint inputState)
	{
		inputState = inputState.SetBit(0, PeInput.Get(PeInput.LogicFunction.MoveForward));
		inputState = inputState.SetBit(1, PeInput.Get(PeInput.LogicFunction.MoveBackward));
		inputState = inputState.SetBit(2, PeInput.Get(PeInput.LogicFunction.MoveLeft));
		inputState = inputState.SetBit(3, PeInput.Get(PeInput.LogicFunction.MoveRight));
		if (PeInput.Get(PeInput.LogicFunction.AutoRunOnOff))
		{
			_autoDrive = !_autoDrive;
		}
		if (inputState.GetBit(1))
		{
			_autoDrive = false;
		}
		if (_autoDrive)
		{
			inputState = inputState.SetBit1(0);
		}
		if (PeInput.Get(PeInput.LogicFunction.SwitchLight))
		{
			inputState = inputState.ReverseBit(5);
		}
		if (PeInput.Get(PeInput.LogicFunction.Vehicle_AttackModeOnOff))
		{
			inputState = inputState.ReverseBit(6);
		}
		if (_isJetting)
		{
			_jetCDTime = PEVCConfig.instance.jetDecToIncInterval;
			if (PeInput.Get(PeInput.LogicFunction.Vehicle_Sprint) && base.energy > 0f)
			{
				_jetRestValue = Mathf.Clamp01(_jetRestValue - PEVCConfig.instance.jetDecreaseSpeed * Time.deltaTime);
				_isJetting = _jetRestValue > 0f;
			}
			else
			{
				_isJetting = false;
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Vehicle_Sprint))
		{
			_isJetting = _jetRestValue > 0f;
		}
		else
		{
			_jetCDTime -= Time.deltaTime;
			if (_jetCDTime < 0f)
			{
				_jetCDTime = 0f;
			}
			if (_jetCDTime == 0f)
			{
				_jetRestValue = Mathf.Clamp01(_jetRestValue + PEVCConfig.instance.jetIncreaseSpeed * Time.deltaTime);
			}
			_isJetting = false;
		}
		inputState = inputState.SetBit(4, _isJetting);
		inputState = inputState.SetBit(8, PeInput.Get(PeInput.LogicFunction.Vehicle_LiftUp));
		inputState = inputState.SetBit(9, PeInput.Get(PeInput.LogicFunction.Vehicle_LiftDown));
		return inputState;
	}

	protected virtual void DecodeInput(uint inputState)
	{
		float num = 0f;
		num += ((!inputState.GetBit(3)) ? 0f : 1f);
		num -= ((!inputState.GetBit(2)) ? 0f : 1f);
		_inputX = Mathf.MoveTowards(_inputX, num, 4f * Time.deltaTime);
		num = 0f;
		num += ((!inputState.GetBit(0)) ? 0f : 1f);
		num -= ((!inputState.GetBit(1)) ? 0f : 1f);
		_inputY = Mathf.MoveTowards(_inputY, num, 4f * Time.deltaTime);
		_isJetting = inputState.GetBit(4);
		_isLightOn = inputState.GetBit(5);
		if (_isAttackMode != inputState.GetBit(6))
		{
			_isAttackMode = !_isAttackMode;
			if (_isAttackMode)
			{
				EnterAttactMode();
			}
			else
			{
				ExitAttackMode();
			}
		}
		num = 0f;
		num += ((!inputState.GetBit(8)) ? 0f : 1f);
		num -= ((!inputState.GetBit(9)) ? 0f : 1f);
		_inputVertical = Mathf.MoveTowards(_inputVertical, num, 4f * Time.deltaTime);
	}

	private void HandleNormalInput()
	{
		if (_isAttackMode)
		{
			RaycastHit[] array = Physics.RaycastAll(_ray = PeCamera.mouseRay, 300f, PEVCConfig.instance.attackRayLayerMask);
			int num = -1;
			if (array.Length > 0)
			{
				float num2 = float.MaxValue;
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].collider.isTrigger && array[i].distance < num2 && !array[i].transform.IsChildOf(base.transform))
					{
						num = i;
						num2 = array[i].distance;
					}
				}
			}
			if (num >= 0)
			{
				_aimPoint = array[num].point;
				_aimEntity = array[num].collider.GetComponentInParent<PESkEntity>();
				if (_aimEntity != null && (Singleton<ForceSetting>.Instance.AllyPlayer((int)_aimEntity.GetAttribute(AttribType.DefaultPlayerID), (int)base.creationSkEntity.GetAttribute(AttribType.DefaultPlayerID)) || _aimEntity.isDead))
				{
					_aimEntity = null;
				}
			}
			else
			{
				_aimPoint = _ray.direction * 300f + _ray.origin;
				_aimEntity = null;
			}
			UpdateLockTarget();
			SetWeaponControlEnabled(WeaponType.Missile, PeInput.Get(PeInput.LogicFunction.MissleLaunch));
			if (PeInput.Get(PeInput.LogicFunction.Vehicle_BegFixedShooting))
			{
				SetWeaponControlEnabled(WeaponType.Cannon, enabled: true);
			}
			if (PeInput.Get(PeInput.LogicFunction.Vehicle_EndFixedShooting))
			{
				SetWeaponControlEnabled(WeaponType.Cannon, enabled: false);
			}
			if (PeInput.Get(PeInput.LogicFunction.Vehicle_BegUnfixedShooting))
			{
				SetWeaponControlEnabled(WeaponType.Gun, enabled: true);
			}
			if (PeInput.Get(PeInput.LogicFunction.Vehicle_EndUnfixedShooting))
			{
				SetWeaponControlEnabled(WeaponType.Gun, enabled: false);
			}
			for (int j = 0; j < _weaponGroupKey.Length; j++)
			{
				if (PeInput.Get(_weaponGroupKey[j]))
				{
					ReverseWeaponGroupEnabled(j);
					UIDrivingCtrl.Instance.SetWweaponGroupToggles(j, IsWeaponGroupEnabled(j));
				}
			}
		}
		else
		{
			_timeToLock = -1f;
			_targetToLock = null;
			_lockedTarget = null;
			DisableAllWeaponControl();
		}
	}

	private void UpdateLockTarget()
	{
		if (PeInput.Get(PeInput.LogicFunction.MissleTarget))
		{
			_lockedTarget = null;
			_lockedTargetView = null;
			if ((bool)_aimEntity)
			{
				_targetToLock = _aimEntity;
				_targetViewToLock = _aimEntity.GetComponent<ViewCmpt>();
				_timeToLock = PEVCConfig.instance.lockTargetDuration;
			}
			else
			{
				_targetToLock = null;
				_targetViewToLock = null;
				_timeToLock = -1f;
			}
		}
		if (_timeToLock > 0f)
		{
			if (_targetToLock == _aimEntity && (bool)_targetToLock && (!_targetViewToLock || _targetViewToLock.hasView))
			{
				_timeToLock -= Time.deltaTime;
				if (_timeToLock <= 0f)
				{
					_lockedTarget = _targetToLock;
					_lockedTargetView = _targetViewToLock;
				}
			}
			else
			{
				_targetToLock = null;
				_targetViewToLock = null;
				_timeToLock = -1f;
			}
		}
		else if (((bool)_lockedTarget && _lockedTarget.isDead) || ((bool)_lockedTargetView && !_lockedTargetView.hasView))
		{
			_lockedTarget = null;
			_lockedTargetView = null;
		}
	}

	private void EnterAttactMode()
	{
		if (isPlayerDriver)
		{
			UISightingTelescope.Instance.Show(UISightingTelescope.SightingType.Default);
			UIDrivingCtrl.Instance.SetWweaponGroupTogglesVisible(visible: true, this);
		}
	}

	private void ExitAttackMode()
	{
		if (isPlayerDriver)
		{
			UISightingTelescope.Instance.Show(UISightingTelescope.SightingType.Null);
			UIDrivingCtrl.Instance.SetWweaponGroupTogglesVisible(visible: false, this);
		}
	}

	private void ActivateImpactDamage(bool isActive)
	{
		if (!isActive)
		{
			base.creationPeEntity.StopSkill(ImpactTargetSkillID);
			return;
		}
		base.creationPeEntity.StopSkill(ImpactTargetSkillID);
		base.creationPeEntity.StartSkill(null, ImpactTargetSkillID);
	}

	protected virtual void Update()
	{
		if (isPlayerDriver)
		{
			_inputState = EncodeInput(_inputState);
			DecodeInput(_inputState);
			HandleNormalInput();
		}
		else
		{
			DecodeInput(_inputState);
		}
		if ((bool)_cockpit)
		{
			_cockpit.UpdateCockpit(_inputX, _inputY, hasDriver && isEnergyEnough(0.01f));
		}
	}

	private void Start()
	{
		PeEntity component = GetComponent<PeEntity>();
		if (null != component)
		{
			component.AddMsgListener(this);
		}
	}

	private void OnDestroy()
	{
		PeEntity component = GetComponent<PeEntity>();
		if (null != component)
		{
			component.RemoveMsgListener(this);
		}
		RemoveMapMark();
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.Net_Controller:
			ActivateImpactDamage(isActive: true);
			break;
		case EMsg.Net_Proxy:
			ActivateImpactDamage(isActive: false);
			break;
		}
	}

	private void RemoveMapMark()
	{
		CarrierMark carrierMark = new CarrierMark();
		carrierMark.carrierController = this;
		PeSingleton<LabelMgr>.Instance.Remove(carrierMark);
	}
}
