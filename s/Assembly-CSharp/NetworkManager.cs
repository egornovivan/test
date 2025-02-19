using uLink;

public class NetworkManager : NetInterface
{
	private static NetworkManager _instance;

	protected override void OnPEAwake()
	{
		_instance = this;
	}

	protected override void OnPEStart()
	{
		BindAction(EPacketType.PT_Common_RequestUGC, SteamWorks.RPC_C2S_RequestUGC);
		BindAction(EPacketType.PT_Common_WorkshopShared, SteamWorks.RPC_C2S_WorkShopShared);
		BindAction(EPacketType.PT_Common_UGCDownloaded, SteamWorks.RPC_C2S_UGCDownLoadComplete);
		BindAction(EPacketType.PT_Common_UGCSkill, SteamWorks.RPC_S2C_UGCSkillData);
		BindAction(EPacketType.PT_Common_UGCItem, SteamWorks.RPC_C2S_UGCItemData);
		BindAction(EPacketType.PT_Common_UGCData, SteamWorks.RPC_C2S_UGCData);
		BindAction(EPacketType.PT_Common_InvalidUGC, SteamWorks.RPC_C2S_FileDontExists);
		BindAction(EPacketType.PT_Common_UGCUpload, SteamWorks.RPC_C2S_SendFile);
		BindAction(EPacketType.PT_Common_DownTimeOut, SteamWorks.RPC_C2S_DownTimeOut);
		BindAction(EPacketType.PT_Common_SendRandIsoFileIds, SteamWorks.RPC_C2S_SendRandIsoFileIds);
		BindAction(EPacketType.PT_Common_ExportRandIso, SteamWorks.RPC_C2S_ExportRandIso);
		BindAction(EPacketType.PT_Common_SendRandIsoHash, SteamWorks.RPC_C2S_SendRandIsoHash);
		BindAction(EPacketType.PT_Reputation_SetValue, ReputationSystem.RPC_C2S_SetValue);
		BindAction(EPacketType.PT_Reputation_SetExValue, ReputationSystem.RPC_C2S_SetExValue);
		BindAction(EPacketType.PT_Reputation_SetActive, ReputationSystem.RPC_C2S_SetActive);
		BindAction(EPacketType.PT_Common_InitTownPos, BuildingInfoManager.RPC_C2S_InitTownPos);
	}

	protected override void OnPEDestroy()
	{
		_instance = null;
	}

	public static void SyncProxy(params object[] args)
	{
		if (null != _instance)
		{
			_instance.RPCProxy(args);
		}
	}

	public static void SyncPeer(NetworkPlayer peer, params object[] args)
	{
		if (null != _instance)
		{
			_instance.RPCPeer(peer, args);
		}
	}
}
