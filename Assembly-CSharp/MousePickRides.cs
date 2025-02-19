using ItemAsset;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class MousePickRides : MousePickableChildCollider
{
	private const float OpIntervalTime = 0.5f;

	private PERides rides;

	private PeTrans _playerTrans;

	private PeEntity monsterEntity;

	public static readonly int RideItemID = 1740;

	public static readonly int RideOnTipsID = 8000982;

	public static readonly int RideOnDescribeID = 8000983;

	public static readonly int LackRideItemTipsID = 8000984;

	public static readonly int LowHpTipsID = 8000985;

	private float _rideTime;

	private PERides _rides
	{
		get
		{
			if (rides == null)
			{
				rides = GetComponent<PERides>();
			}
			return rides;
		}
	}

	private Vector3 _playerPos
	{
		get
		{
			if (null == _playerTrans && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				_playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PeTrans>();
			}
			if (null == _playerTrans)
			{
				return Vector3.zero;
			}
			return _playerTrans.position;
		}
	}

	private OperateCmpt _playerOperate => (!(null == PeSingleton<MainPlayer>.Instance.entity)) ? PeSingleton<MainPlayer>.Instance.entity.operateCmpt : null;

	private PeEntity _monsterEntity
	{
		get
		{
			if (null == monsterEntity)
			{
				monsterEntity = base.transform.GetComponentInParent<PeEntity>();
			}
			return monsterEntity;
		}
	}

	protected override string tipsText => null;

	private void Update()
	{
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && Time.realtimeSinceStartup - _rideTime > 0.5f && (bool)_rides && _rides.HasOperater(_playerOperate) && (bool)_playerOperate && _playerOperate.IsActionRunning(PEActionType.Ride))
		{
			ExecUnRide(PeSingleton<MainPlayer>.Instance.entity);
		}
	}

	public void Reset()
	{
		CollectColliders();
		AdjustOpDistance();
	}

	public bool ExecRide(PeEntity playerEntity)
	{
		if ((bool)_monsterEntity && (bool)playerEntity && (bool)_rides)
		{
			MotionMgrCmpt motionMgr = playerEntity.motionMgr;
			OperateCmpt operateCmpt = playerEntity.operateCmpt;
			if (null != motionMgr && !motionMgr.IsActionRunning(PEActionType.Ride) && null != operateCmpt)
			{
				PERide useable = _rides.GetUseable();
				if ((bool)useable && useable.CanOperateMask(EOperationMask.Ride))
				{
					return useable.StartOperate(operateCmpt, EOperationMask.Ride);
				}
				Debug.Log("Try exec ride failed!！ ride is null!");
			}
			else
			{
				Debug.LogFormat("Try exec ride failed!！ mmc is null:{0} ; operate is null:{1} ", null == motionMgr, null == operateCmpt);
			}
		}
		else
		{
			Debug.LogFormat("Try exec ride failed!！ _monsterEntity is null:{0} ; playerEntity is null:{1} ; _rides is null:{2} ", null == _monsterEntity, null == playerEntity, null == _rides);
		}
		return false;
	}

	public bool RecoverExecRide(PeEntity playerEntity)
	{
		if ((bool)_monsterEntity && (bool)playerEntity && (bool)_rides)
		{
			MotionMgrCmpt motionMgr = playerEntity.motionMgr;
			OperateCmpt operateCmpt = playerEntity.operateCmpt;
			if (null != motionMgr && null != operateCmpt)
			{
				PERide useable = _rides.GetUseable();
				if ((bool)useable && useable.CanOperateMask(EOperationMask.Ride))
				{
					if (motionMgr.IsActionRunning(PEActionType.Ride))
					{
						motionMgr.EndImmediately(PEActionType.Ride);
					}
					return useable.StartOperate(operateCmpt, EOperationMask.Ride);
				}
				Debug.Log("Try recover ride failed!！ ride is null!");
			}
			else
			{
				Debug.LogFormat("Try recover ride failed!！ mmc is null:{0} ; operate is null:{1} ", null == motionMgr, null == operateCmpt);
			}
		}
		else
		{
			Debug.LogFormat("Try recover ride failed!！ _monsterEntity is null:{0} ; playerEntity is null:{1} ; _rides is null:{2} ", null == _monsterEntity, null == playerEntity, null == _rides);
		}
		return false;
	}

	public bool ExecUnRide(PeEntity playerEntity)
	{
		if ((bool)_monsterEntity && (bool)playerEntity && (bool)_rides)
		{
			MotionMgrCmpt motionMgr = playerEntity.motionMgr;
			OperateCmpt operateCmpt = playerEntity.operateCmpt;
			if (null != motionMgr && motionMgr.IsActionRunning(PEActionType.Ride) && null != operateCmpt)
			{
				PERide rideByOperater = _rides.GetRideByOperater(operateCmpt);
				if ((bool)rideByOperater)
				{
					return rideByOperater.StopOperate(operateCmpt, EOperationMask.Ride);
				}
				Debug.Log("Try exec unRide failed!！ ride is null!");
			}
			else
			{
				Debug.LogFormat("Try exec unRide failed!！ mmc is null:{0} ; operate is null:{1} ", null == motionMgr, null == operateCmpt);
			}
		}
		else
		{
			Debug.LogFormat("Try exec ride failed!！ _monsterEntity is null:{0} ; playerEntity is null:{1} ; _rides is null:{2} ", null == _monsterEntity, null == playerEntity, null == _rides);
		}
		return false;
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
		{
			TryExecAction();
		}
	}

	protected override bool CheckPick(Ray ray, out float dis)
	{
		return base.CheckPick(ray, out dis) && null != _playerOperate && !_playerOperate.IsActionRunning(PEActionType.Ride) && _rides.HasRide() && null != _monsterEntity && _monsterEntity.monster.CanRide;
	}

	private bool CanCmd()
	{
		if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
		{
			return false;
		}
		if (DistanceInRange(_playerPos, operateDistance))
		{
			SkAliveEntity component = GetComponent<SkAliveEntity>();
			if (component != null && component.isDead)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private void TryExecAction()
	{
		if (null == _playerOperate || null == _monsterEntity)
		{
			return;
		}
		_rideTime = Time.realtimeSinceStartup;
		if (_monsterEntity.HPPercent > 0.5f)
		{
			PeTipMsg.Register(PELocalization.GetString(LowHpTipsID), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		PlayerPackageCmpt playerPackageCmpt = PeSingleton<MainPlayer>.Instance.entity.packageCmpt as PlayerPackageCmpt;
		ItemObject itemObject = playerPackageCmpt.package.FindItemByProtoId(RideItemID);
		if (itemObject == null || PeSingleton<MainPlayer>.Instance.entity.UseItem.GetCdByItemProtoId(RideItemID) > float.Epsilon)
		{
			PeTipMsg.Register(PELocalization.GetString(LackRideItemTipsID), PeTipMsg.EMsgLevel.Warning);
		}
		else if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.RequestReqMonsterCtrl(_monsterEntity.Id);
		}
		else if (ExecRide(PeSingleton<MainPlayer>.Instance.entity))
		{
			playerPackageCmpt.DestroyItem(itemObject.instanceId, 1);
		}
	}

	private void AdjustOpDistance()
	{
		Bounds modelBounds = SpeciesViewStudio.GetModelBounds(base.gameObject);
		if (modelBounds.size == Vector3.zero)
		{
			operateDistance = 0f;
		}
		else
		{
			operateDistance = Mathf.Max(modelBounds.extents.x, modelBounds.extents.z) + 1.5f;
		}
	}
}
