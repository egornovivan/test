using Pathea;
using WhiteCat;

public class ItemScript_Carrier : ItemScript
{
	private PassengerCmpt mPlayerPassengerCmpt;

	public PassengerCmpt playerPassengerCmpt
	{
		get
		{
			if (null == mPlayerPassengerCmpt && null != PeSingleton<PeCreature>.Instance.mainPlayer)
			{
				mPlayerPassengerCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PassengerCmpt>();
			}
			return mPlayerPassengerCmpt;
		}
	}

	public bool IsPlayerOnCarrier()
	{
		if (playerPassengerCmpt == null)
		{
			return false;
		}
		return playerPassengerCmpt.IsOnCarrier();
	}

	public int PassengerCountOnSeat()
	{
		CarrierController component = GetComponent<CarrierController>();
		if (component == null)
		{
			return 0;
		}
		return component.passengerCount;
	}

	public void GetOn()
	{
		if (playerPassengerCmpt == null)
		{
			return;
		}
		CarrierController component = GetComponent<CarrierController>();
		int num = component.FindEmptySeatIndex();
		if (num < -1)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			MotionMgrCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
			if (null != cmpt)
			{
				PEActionParamDrive param = PEActionParamDrive.param;
				param.controller = component;
				param.seatIndex = num;
				if (cmpt.CanDoAction(PEActionType.Drive, param) && null != mNetlayer && null != PlayerNetwork.mainPlayer && !Singleton<ForceSetting>.Instance.Conflict(mNetlayer.TeamId, PlayerNetwork.mainPlayerId))
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_GetOnVehicle, mNetlayer.Id);
				}
			}
		}
		else if (null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			PassengerCmpt cmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PassengerCmpt>();
			if (null != cmpt2)
			{
				cmpt2.GetOn(component, num, checkState: true);
			}
		}
	}

	public void Repair()
	{
		if (null != mNetlayer && null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RepairVehicle, mNetlayer.Id);
		}
	}

	public void Charge()
	{
		if (null != mNetlayer && null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ChargeVehicle, mNetlayer.Id);
		}
	}
}
