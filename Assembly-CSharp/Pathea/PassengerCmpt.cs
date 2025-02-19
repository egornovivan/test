using System;
using System.IO;
using PETools;
using Railway;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class PassengerCmpt : PeCmpt, IPassenger, IVCPassenger
{
	private PeTrans mPeTrans;

	private PESkEntity mSkEntity;

	private MotionMgrCmpt mMotionMgr;

	private int mRailRouteId = -1;

	private bool m_UpperAir;

	private float m_UpperAirStartTime;

	private VCPBaseSeat m_Seat;

	private CarrierController m_DrivingController;

	public Action<CarrierController> onGetOnCarrier;

	public Action<CarrierController> onGetOffCarrier;

	private bool mIsOnVCCarrier;

	public VCPBaseSeat seat => m_Seat;

	public CarrierController carrier => (!(m_Seat != null)) ? null : m_Seat.drivingController;

	public CarrierController drivingController => m_DrivingController;

	public bool IsOnRail
	{
		get
		{
			if (mRailRouteId == -1)
			{
				return false;
			}
			return true;
		}
	}

	public int railRouteId => mRailRouteId;

	public bool IsOnVCCarrier => mIsOnVCCarrier;

	void IPassenger.GetOn(string pose)
	{
		mMotionMgr.FreezePhyState(GetType(), v: true);
		mMotionMgr.SetMaskState(PEActionMask.OnVehicle, state: true);
		PEActionParamS param = PEActionParamS.param;
		param.str = pose;
		mMotionMgr.DoActionImmediately(PEActionType.GetOnTrain, param);
	}

	void IPassenger.GetOff(Vector3 pos)
	{
		mMotionMgr.FreezePhyState(GetType(), v: false);
		mMotionMgr.SetMaskState(PEActionMask.OnVehicle, state: false);
	}

	void IPassenger.UpdateTrans(Transform trans)
	{
		mPeTrans.position = trans.position;
		mPeTrans.rotation = trans.rotation;
	}

	void IVCPassenger.GetOn(string sitAnimName, VCPBaseSeat seat)
	{
		m_Seat = seat;
		if (null != mMotionMgr)
		{
			mMotionMgr.GetAction<Action_Drive>()?.SetSeat(sitAnimName, seat);
		}
	}

	void IVCPassenger.GetOff()
	{
		if (null != mMotionMgr)
		{
			m_DrivingController = null;
			m_Seat = null;
			mIsOnVCCarrier = false;
			if (onGetOffCarrier != null)
			{
				onGetOffCarrier(carrier);
			}
		}
	}

	void IVCPassenger.Sync(Vector3 position, Quaternion rotation)
	{
		if (null != mPeTrans)
		{
			mPeTrans.position = position;
			mPeTrans.rotation = rotation;
		}
	}

	void IVCPassenger.SetHands(Transform left, Transform right)
	{
		if (null != mMotionMgr)
		{
			mMotionMgr.GetAction<Action_Drive>()?.SetHand(left, right);
		}
	}

	public void UpdateHeadInfo()
	{
		if (!(mSkEntity != null))
		{
			return;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(mSkEntity.GetId());
		if (peEntity != null)
		{
			EntityInfoCmpt cmpt = peEntity.GetCmpt<EntityInfoCmpt>();
			if (cmpt != null)
			{
				cmpt.OverHead.UpdateTransform();
			}
		}
	}

	public override void Start()
	{
		base.Start();
		mPeTrans = base.Entity.peTrans;
		mSkEntity = base.Entity.GetComponent<PESkEntity>();
		mMotionMgr = base.Entity.GetCmpt<MotionMgrCmpt>();
		if (mRailRouteId != -1)
		{
			if (PeSingleton<Manager>.Instance.GetRoute(mRailRouteId) != null)
			{
				DoGetOn(mRailRouteId);
			}
			else
			{
				mRailRouteId = -1;
			}
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdatePlayerUpperAir();
	}

	private void UpdatePlayerUpperAir()
	{
		if (!PeGameMgr.IsMulti && base.Entity == PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			if (carrier == null || !(carrier is HelicopterController))
			{
				m_UpperAir = false;
			}
			else
			{
				float terrainHeight = PeSingleton<PeMappingMgr>.Instance.GetTerrainHeight(base.Entity.position);
				bool flag = base.Entity.position.y - terrainHeight > 30f;
				bool flag2 = carrier.rigidbody != null && carrier.rigidbody.velocity.sqrMagnitude > 1f;
				m_UpperAir = flag && flag2;
			}
		}
		if (!m_UpperAir)
		{
			m_UpperAirStartTime = 0f;
		}
		else if (m_UpperAirStartTime < float.Epsilon)
		{
			m_UpperAirStartTime = Time.time;
		}
		if (m_UpperAir && m_UpperAirStartTime > 0f && Time.time - m_UpperAirStartTime > 10f)
		{
			m_UpperAirStartTime = Time.time;
			float value = UnityEngine.Random.value;
			if (value < 0.015f)
			{
				Vector3 randomPosition = PEUtil.GetRandomPosition(base.Entity.position, 25f, 35f);
				MonsterEntityCreator.CreateMonster(73, randomPosition);
				Debug.Log("Spawn caelum rex in upper air : " + value + " time : " + Time.time);
			}
		}
	}

	private bool DoGetOn(int railRouteId, bool checkState = true)
	{
		if (checkState && !mMotionMgr.CanDoAction(PEActionType.GetOnTrain))
		{
			return false;
		}
		Route route = PeSingleton<Manager>.Instance.GetRoute(railRouteId);
		if (route == null)
		{
			Debug.LogError("cant find route to get on, route id:" + railRouteId);
			return false;
		}
		if (!route.AddPassenger(this))
		{
			Debug.LogError("get on failed, route id:" + railRouteId);
			return false;
		}
		mRailRouteId = railRouteId;
		return true;
	}

	public bool GetOn(int railRouteId, bool checkState = true)
	{
		if (railRouteId == -1)
		{
			return false;
		}
		DoGetOn(railRouteId, checkState);
		return true;
	}

	public bool GetOff(Vector3 getOffPos)
	{
		Route route = PeSingleton<Manager>.Instance.GetRoute(mRailRouteId);
		if (route == null)
		{
			Debug.LogError("cant find route to get off, route id:" + mRailRouteId);
			return false;
		}
		mMotionMgr.EndAction(PEActionType.GetOnTrain);
		mRailRouteId = -1;
		return route.RemovePassenger(this, getOffPos);
	}

	public bool GetOff()
	{
		Route route = PeSingleton<Manager>.Instance.GetRoute(mRailRouteId);
		if (route == null)
		{
			Debug.LogError("cant find route to get off, route id:" + mRailRouteId);
			return false;
		}
		mMotionMgr.EndAction(PEActionType.GetOnTrain);
		mRailRouteId = -1;
		return route.RemovePassenger(this);
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		mRailRouteId = r.ReadInt32();
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(mRailRouteId);
	}

	private void OnGetOnSucceed(CarrierController controller, int seatIndex)
	{
		m_DrivingController = controller;
		mIsOnVCCarrier = true;
		if (onGetOnCarrier != null)
		{
			onGetOnCarrier(carrier);
		}
	}

	public void GetOn(CarrierController controller, int seatIndex, bool checkState)
	{
		if (null != mMotionMgr)
		{
			PEActionParamDrive param = PEActionParamDrive.param;
			param.controller = controller;
			param.seatIndex = seatIndex;
			if (!checkState)
			{
				mMotionMgr.DoActionImmediately(PEActionType.Drive, param);
				OnGetOnSucceed(controller, seatIndex);
			}
			else if (mMotionMgr.DoAction(PEActionType.Drive, param))
			{
				OnGetOnSucceed(controller, seatIndex);
			}
		}
	}

	public void GetOffCarrier()
	{
		if (!(null == seat) && seat.FindGetOffPosition(out var position))
		{
			if (GameConfig.IsMultiMode)
			{
				mSkEntity._net.RPCServer(EPacketType.PT_InGame_GetOffVehicle, position);
			}
			else if (mMotionMgr.EndAction(PEActionType.Drive))
			{
				mPeTrans.position = position;
			}
		}
	}

	public void GetOffCarrier(Vector3 pos)
	{
		m_DrivingController = null;
		m_Seat = null;
		mIsOnVCCarrier = false;
		if (onGetOffCarrier != null && null != carrier)
		{
			onGetOffCarrier(carrier);
		}
	}

	public bool IsOnCarrier()
	{
		return mIsOnVCCarrier || IsOnRail;
	}
}
