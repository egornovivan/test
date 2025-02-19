using System.Collections;
using uLink;

public class NetworkManager : NetworkInterface
{
	private static NetworkManager _instance;

	public static NetworkManager Instance => _instance;

	protected override void OnPEAwake()
	{
		_instance = this;
	}

	protected override void OnPEStart()
	{
		BindAction(EPacketType.PT_Common_InitGameTimer, GameTime.RPC_S2C_SyncInitGameTimer);
		BindAction(EPacketType.PT_Common_GameTimer, GameTime.RPC_S2C_SyncGameTimer);
		BindAction(EPacketType.PT_Common_ElapseSpeedShift, GameTime.RPC_S2C_ElapseSpeedShift);
		BindAction(EPacketType.PT_Common_ServerInfo, GameClientNetwork.RPC_S2C_ServerInfo);
		BindAction(EPacketType.PT_InRoom_ForceSetting, RPC_S2C_ForceSetting);
		BindAction(EPacketType.PT_Common_WeatherChange, RPC_S2C_WeatherChange);
		BindAction(EPacketType.PT_Common_RequestUGC, SteamWorkShop.RPC_S2C_ResponseUGC);
		BindAction(EPacketType.PT_Common_WorkshopShared, SteamWorkShop.RPC_S2C_WorkShopShared);
		BindAction(EPacketType.PT_Common_UGCList, SteamWorkShop.RPC_S2C_UGCGenerateList);
		BindAction(EPacketType.PT_Common_UGCData, SteamWorkShop.RPC_S2C_RequestUGCData);
		BindAction(EPacketType.PT_Common_InvalidUGC, SteamWorkShop.RPC_S2C_FileDontExists);
		BindAction(EPacketType.PT_Common_PreUpload, SteamWorkShop.RPC_S2C_StartSendFile);
		BindAction(EPacketType.PT_Common_UGCUpload, SteamWorkShop.RPC_S2C_SendFile);
		BindAction(EPacketType.PT_Common_DownTimeOut, SteamWorkShop.RPC_S2C_DownTimeOut);
		BindAction(EPacketType.PT_Common_GetRandIsoFileIds, SteamWorkShop.RPC_S2C_GetRandIsoFileIds);
		BindAction(EPacketType.PT_Common_SendRandIsoFileIds, SteamWorkShop.RPC_S2C_SendRandIsoFileIds);
		BindAction(EPacketType.PT_Common_ExportRandIso, SteamWorkShop.RPC_S2C_ExportRandIso);
		BindAction(EPacketType.PT_Common_IsoExportSuccessed, SteamFileItem.RPC_S2C_IsoExportSuccessed);
		BindAction(EPacketType.PT_Reputation_SetValue, ReputationSystem.RPC_S2C_SetValue);
		BindAction(EPacketType.PT_Reputation_SetExValue, ReputationSystem.RPC_S2C_SetExValue);
		BindAction(EPacketType.PT_Reputation_SetActive, ReputationSystem.RPC_S2C_SetActive);
		BindAction(EPacketType.PT_Reputation_SyncValue, ReputationSystem.RPC_S2C_SyncValue);
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		_instance = null;
	}

	public static void SyncServer(params object[] args)
	{
		if (null != _instance)
		{
			_instance.RPCServer(args);
		}
	}

	public static void WaitCoroutine(IEnumerator coroutine)
	{
		if (null != _instance)
		{
			_instance.StartCoroutine(coroutine);
		}
	}

	private static void RPC_S2C_WeatherChange(BitStream stream, NetworkMessageInfo info)
	{
	}

	private static void RPC_S2C_ForceSetting(BitStream stream, NetworkMessageInfo info)
	{
	}
}
