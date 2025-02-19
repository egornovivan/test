using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CustomCharactor;
using CustomData;
using ItemAsset;
using Pathea;
using PeCustom;
using PeMap;
using PETools;
using Railway;
using RandomItem;
using ScenarioRTL;
using SkillAsset;
using SkillSystem;
using uLink;
using UnityEngine;
using WhiteCat;

public class PlayerNetwork : SkNetworkInterface, INetworkEvent
{
	private static bool isTerrainDataOk;

	private static Dictionary<int, Bounds> LimitBounds = new Dictionary<int, Bounds>();

	private static List<PlayerNetwork> PlayerList = new List<PlayerNetwork>();

	private static Dictionary<int, PlayerNetwork> PlayerDic = new Dictionary<int, PlayerNetwork>();

	public bool _initOk;

	public bool _gameStarted;

	private BaseNetwork networkBase;

	private PeEntity entity;

	private PeTrans _transCmpt;

	private PlayerSynAttribute mPlayerSynAttribute = new PlayerSynAttribute();

	private PlayerArmorCmpt _playerArmor;

	public int _curSceneId;

	private BattleInfo _battleInfo;

	public static bool _missionInited = false;

	private List<string> _commandCache = new List<string>();

	private int _mission953Item;

	public static List<int> _storyPlot = new List<int>();

	public SkillTreeUnitMgr _learntSkills;

	public static int mainPlayerId { get; private set; }

	public static PlayerNetwork mainPlayer { get; private set; }

	public int BaseTeamId => BaseNetwork.MainPlayer.TeamId;

	public PeEntity PlayerEntity => entity;

	public Vector3 PlayerPos => _transCmpt.position;

	public PlayerArmorCmpt PlayerArmor
	{
		get
		{
			if (_playerArmor == null)
			{
				_playerArmor = entity.GetCmpt<PlayerArmorCmpt>();
			}
			return _playerArmor;
		}
	}

	public string RoleName { get; private set; }

	public byte Sex { get; private set; }

	public int originTeamId { get; private set; }

	public int colorIndex { get; private set; }

	internal CreationNetwork DriveCreation { get; set; }

	internal EVCComponent SeatType { get; set; }

	internal int SeatIndex { get; set; }

	public bool isOriginTeam => originTeamId == base.TeamId;

	public BattleInfo Battle => _battleInfo;

	public static event System.Action OnTeamChangedEventHandler;

	public event Action<NetworkInterface> OnPrefabViewBuildEvent;

	public event Action<NetworkInterface> OnPrefabViewDestroyEvent;

	public event Action<int> OnCustomDeathEventHandler;

	public event Action<int, int, float> OnCustomDamageEventHandler;

	public event Action<ItemObject> OnCustomUseItemEventHandler;

	public event Action<ItemObject> OnCustomPutOutItemEventHandler;

	public void CreateAccountItems(int itemType, int amount)
	{
		if (AccountItems.self.CheckCreateItems(itemType, amount))
		{
			RPCServer(EPacketType.PT_InGame_AccItems_CreateItem, itemType, amount);
		}
	}

	private void RPC_AccItems_CreateItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemType = stream.Read<int>(new object[0]);
		int amount = stream.Read<int>(new object[0]);
		AccountItems.self.DeleteItems(itemType, amount);
		UILobbyShopItemMgr._self.MallItemEvent(0, UIMallWnd.Instance.mCurrentTab);
	}

	public static bool IsOnline(int id)
	{
		return NetworkInterface.Get(id) is PlayerNetwork;
	}

	public static void ResetTerrainState()
	{
		isTerrainDataOk = false;
	}

	private static void OnTeamChanged(int teamId)
	{
		if (null == mainPlayer)
		{
			return;
		}
		PlayerAction(delegate(PlayerNetwork p)
		{
			if (!object.ReferenceEquals(p, mainPlayer) && null != p.PlayerEntity)
			{
				p.PlayerEntity.SendMsg(EMsg.Net_Destroy);
				EntityInfoCmpt cmpt = p.PlayerEntity.GetCmpt<EntityInfoCmpt>();
				if (null != cmpt)
				{
					if (p.TeamId == mainPlayer.TeamId)
					{
						cmpt.mapIcon = 11;
					}
					else
					{
						cmpt.mapIcon = 12;
					}
					p.PlayerEntity.SendMsg(EMsg.Net_Instantiate);
				}
			}
		});
		if (teamId == mainPlayer.TeamId && PlayerNetwork.OnTeamChangedEventHandler != null)
		{
			PlayerNetwork.OnTeamChangedEventHandler();
		}
	}

	public static void OnLimitBoundsAdd(int id, Bounds areaBounds)
	{
		if (!LimitBounds.ContainsKey(id))
		{
			LimitBounds.Add(id, areaBounds);
		}
	}

	public static void OnLimitBoundsDel(int id)
	{
		if (LimitBounds.ContainsKey(id))
		{
			LimitBounds.Remove(id);
		}
	}

	public static bool OnLimitBoundsCheck(Bounds targetBounds)
	{
		return LimitBounds.All(delegate(KeyValuePair<int, Bounds> iter)
		{
			if (targetBounds.Intersects(iter.Value))
			{
				NetworkInterface networkInterface = NetworkInterface.Get(iter.Key);
				if (null == networkInterface)
				{
					return true;
				}
				if (networkInterface.TeamId == mainPlayer.originTeamId)
				{
					return false;
				}
			}
			return true;
		});
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_curSceneId = -1;
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		networkBase = BaseNetwork.GetBaseNetwork(base.Id);
		if (null == networkBase)
		{
			Debug.LogErrorFormat("NetworkBase is null.id:{0}", base.Id);
			return;
		}
		originTeamId = networkBase.TeamId;
		colorIndex = networkBase.ColorIndex;
		RoleName = networkBase.RoleName;
		Sex = networkBase.Sex;
		base.authId = base.Id;
		base.name = RoleName + "_" + base.Id;
		AddPlayer();
		if (base.IsOwner)
		{
			mainPlayerId = base.Id;
			mainPlayer = this;
		}
	}

	protected override void OnPEStart()
	{
		BindSkAction();
		BindAction(EPacketType.PT_InGame_TerrainDataOk, RPC_S2C_TerrainDataOk);
		BindAction(EPacketType.PT_InGame_RequestInit, RPC_S2C_RequestInit);
		BindAction(EPacketType.PT_InGame_InitStatus, RPC_S2C_InitStatus);
		BindAction(EPacketType.PT_Common_InitAdminData, RPC_S2C_InitAdminData);
		BindAction(EPacketType.PT_InGame_InitPackage, RPC_S2C_InitPackage);
		BindAction(EPacketType.PT_InGame_PackageIndex, RPC_S2C_PackageIndex);
		BindAction(EPacketType.PT_InGame_InitShortcut, RPC_S2C_InitShortcut);
		BindAction(EPacketType.PT_InGame_InitLearntSkills, RPC_S2C_InitLearntSkills);
		BindAction(EPacketType.PT_InGame_PlayerMoney, RPC_S2C_PlayerMoney);
		BindAction(EPacketType.PT_InGame_CurSceneId, RPC_S2C_CurSceneId);
		BindAction(EPacketType.PT_InGame_MoneyType, RPC_S2C_MoneyType);
		BindAction(EPacketType.PT_InGame_MissionPackageIndex, RPC_S2C_MissionPackageIndex);
		BindAction(EPacketType.PT_InGame_RailwayData, RPC_S2C_SyncRailwayData);
		BindAction(EPacketType.PT_InGame_FarmInfo, RPC_S2C_FarmInfo);
		BindAction(EPacketType.PT_InGame_PlayerBattleInfo, RPC_S2C_PlayerBattleInfo);
		BindAction(EPacketType.PT_InGame_EquipedItem, RPC_S2C_EquipedItems);
		BindAction(EPacketType.PT_InGame_PutOnEquipment, RPC_S2C_PutOnEquipment);
		BindAction(EPacketType.PT_InGame_TakeOffEquipment, RPC_S2C_TakeOffEquipment);
		BindAction(EPacketType.PT_InGame_InitDataOK, RPC_S2C_InitDataOK);
		BindAction(EPacketType.PT_InGame_GetOnVehicle, RPC_S2C_GetOnVehicle);
		BindAction(EPacketType.PT_InGame_GetOffVehicle, RPC_S2C_GetOffVehicle);
		BindAction(EPacketType.PT_InGame_RepairVehicle, RPC_S2C_RepairVehicle);
		BindAction(EPacketType.PT_InGame_MakeMask, RPC_S2C_MakeMask);
		BindAction(EPacketType.PT_InGame_RemoveMask, RPC_S2C_RemoveMask);
		BindAction(EPacketType.PT_InGame_FastTransfer, RPC_S2C_FastTransfer);
		BindAction(EPacketType.PT_InGame_SwitchScene, RPC_S2C_SwitchScene);
		BindAction(EPacketType.PT_InGame_DelSceneObjects, RPC_S2C_DelSceneObjects);
		BindAction(EPacketType.PT_InGame_PlayerPosition, RPC_S2C_PlayerMovePosition);
		BindAction(EPacketType.PT_InGame_PlayerRot, RPC_S2C_PlayerMoveRotationY);
		BindAction(EPacketType.PT_InGame_PlayerState, RPC_S2C_PlayerMovePlayerState);
		BindAction(EPacketType.PT_InGame_PlayerOnGround, RPC_S2C_PlayerMoveGrounded);
		BindAction(EPacketType.PT_InGame_PlayerShootTarget, RPC_S2C_PlayerMoveShootTarget);
		BindAction(EPacketType.PT_InGame_GliderStatus, RPC_S2C_SyncGliderStatus);
		BindAction(EPacketType.PT_InGame_ParachuteStatus, RPC_S2C_SyncParachuteStatus);
		BindAction(EPacketType.PT_InGame_JetPackStatus, RPC_S2C_SyncJetPackStatus);
		BindAction(EPacketType.PT_InGame_GetAllDeadObjItem, RPC_S2C_GetDeadObjAllItems);
		BindAction(EPacketType.PT_InGame_GetDeadObjItem, RPC_S2C_GetDeadObjItem);
		BindAction(EPacketType.PT_InGame_GetItemBack, RPC_S2C_GetItemBack);
		BindAction(EPacketType.PT_InGame_PreGetItemBack, RPC_S2C_PreGetItemBack);
		BindAction(EPacketType.PT_InGame_GetLootItemBack, RPC_S2C_GetLootItemBack);
		BindAction(EPacketType.PT_InGame_NewItemList, RPC_S2C_NewItemList);
		BindAction(EPacketType.PT_InGame_PackageDelete, RPC_S2C_DeleteItemInPackage);
		BindAction(EPacketType.PT_InGame_UseItem, RPC_S2C_UseItem);
		BindAction(EPacketType.PT_InGame_PutItem, RPC_S2C_PutItem);
		BindAction(EPacketType.PT_InGame_Turn, RPC_S2C_Turn);
		BindAction(EPacketType.PT_InGame_PackageSplit, RPC_S2C_SplitItem);
		BindAction(EPacketType.PT_InGame_ExchangeItem, RPC_S2C_ExchangeItem);
		BindAction(EPacketType.PT_InGame_SceneObject, RPC_S2C_SceneObject);
		BindAction(EPacketType.PT_Common_ErrorMsg, RPC_S2C_ErrorMsg);
		BindAction(EPacketType.PT_Common_ErrorMsgBox, RPC_S2C_ErrorMsgBox);
		BindAction(EPacketType.PT_Common_ErrorMsgCode, RPC_S2C_ErrorMsgCode);
		BindAction(EPacketType.PT_InGame_SendMsg, RPC_S2C_SendMsg);
		BindAction(EPacketType.PT_InGame_PlayerRevive, RPC_S2C_PlayerRevive);
		BindAction(EPacketType.PT_InGame_ApplyDamage, RPC_S2C_ApplyHpChange);
		BindAction(EPacketType.PT_InGame_PlayerDeath, RPC_S2C_Death);
		BindAction(EPacketType.PT_InGame_ApplyComfort, RPC_S2C_ApplyComfortChange);
		BindAction(EPacketType.PT_InGame_ApplySatiation, RPC_S2C_ApplySatiationChange);
		BindAction(EPacketType.PT_InGame_ItemRemove, RPC_S2C_RemoveItemFromPackage);
		BindAction(EPacketType.PT_InGame_PlayerReset, RPC_S2C_PlayerReset);
		BindAction(EPacketType.PT_InGame_SetShortcut, RPC_S2C_SetShortcut);
		BindAction(EPacketType.PT_InGame_SyncMission, RPC_S2C_SyncMissions);
		BindAction(EPacketType.PT_InGame_NewMission, RPC_S2C_CreateMission);
		BindAction(EPacketType.PT_InGame_AccessMission, RPC_S2C_AccessMission);
		BindAction(EPacketType.PT_InGame_MissionMonsterPos, RPC_S2C_CreateKillMonsterPos);
		BindAction(EPacketType.PT_InGame_MissionFollowPos, RPC_S2C_CreateFollowPos);
		BindAction(EPacketType.PT_InGame_MissionDiscoveryPos, RPC_S2C_CreateDiscoveryPos);
		BindAction(EPacketType.PT_InGame_MissionItemUsePos, RPC_S2C_SyncUseItemPos);
		BindAction(EPacketType.PT_InGame_DeleteMission, RPC_S2C_DeleteMission);
		BindAction(EPacketType.PT_InGame_CompleteTarget, RPC_S2C_CompleteTarget);
		BindAction(EPacketType.PT_InGame_ModifyMissionFlag, RPC_S2C_ModifyMissionFlag);
		BindAction(EPacketType.PT_InGame_CompleteMission, RPC_S2C_ReplyCompleteMission);
		BindAction(EPacketType.PT_InGame_MissionFailed, RPC_S2C_FailMission);
		BindAction(EPacketType.PT_InGame_AddNpcToColony, RPC_S2C_AddNpcToColony);
		BindAction(EPacketType.PT_InGame_MissionKillMonster, RPC_S2C_MissionKillMonster);
		BindAction(EPacketType.PT_InGame_SetMission, RPC_S2C_SetMission);
		BindAction(EPacketType.PT_InGame_Mission953, RPC_S2C_Mission953);
		BindAction(EPacketType.PT_InGame_LanguegeSkill, RPC_S2C_LanguegeSkill);
		BindAction(EPacketType.PT_InGame_MonsterBook, RPC_S2C_MonsterBook);
		BindAction(EPacketType.PT_InGame_EntityReach, RPC_S2C_EntityReach);
		BindAction(EPacketType.PT_InGame_RequestAdMissionData, RPC_S2C_RequestAdMissionData);
		BindAction(EPacketType.PT_InGame_SetCollectItemID, RPC_S2C_SetCollectItem);
		BindAction(EPacketType.PT_InGame_AddBlackList, RPC_S2C_AddBlackList);
		BindAction(EPacketType.PT_InGame_DelBlackList, RPC_S2C_DeleteBlackList);
		BindAction(EPacketType.PT_InGame_ClearBlackList, RPC_S2C_ClearBlackList);
		BindAction(EPacketType.PT_InGame_AddAssistant, RPC_S2C_AddAssistants);
		BindAction(EPacketType.PT_InGame_DelAssistant, RPC_S2C_DeleteAssistants);
		BindAction(EPacketType.PT_InGame_ClearAssistant, RPC_S2C_ClearAssistants);
		BindAction(EPacketType.PT_InGame_BuildLock, RPC_S2C_BuildLock);
		BindAction(EPacketType.PT_InGame_BuildUnLock, RPC_S2C_BuildUnLock);
		BindAction(EPacketType.PT_InGame_ClearBuildLock, RPC_S2C_ClearBuildLock);
		BindAction(EPacketType.PT_InGame_ClearVoxel, RPC_S2C_ClearVoxelData);
		BindAction(EPacketType.PT_InGame_ClearAllVoxel, RPC_S2C_ClearAllVoxelData);
		BindAction(EPacketType.PT_InGame_AreaLock, RPC_S2C_LockArea);
		BindAction(EPacketType.PT_InGame_AreaUnLock, RPC_S2C_UnLockArea);
		BindAction(EPacketType.PT_InGame_BlockLock, RPC_S2C_BuildChunk);
		BindAction(EPacketType.PT_InGame_LoginBan, RPC_S2C_JoinGame);
		BindAction(EPacketType.PT_InGame_CreateBuilding, RPC_S2C_BuildBuildingBlock);
		BindAction(EPacketType.PT_InGame_InitShop, RPC_S2C_InitNpcShop);
		BindAction(EPacketType.PT_InGame_RepurchaseIndex, RPC_S2C_SyncRepurchaseItemIDs);
		BindAction(EPacketType.PT_InGame_ChangeCurrency, RPC_S2C_ChangeCurrency);
		BindAction(EPacketType.PT_InGame_SkillCast, RPC_S2C_SkillCast);
		BindAction(EPacketType.PT_InGame_SkillShoot, RPC_S2C_SkillCastShoot);
		BindAction(EPacketType.PT_InGame_MergeSkillList, RPC_S2C_MergeSkillList);
		BindAction(EPacketType.PT_InGame_MetalScanList, RPC_S2C_MetalScanList);
		BindAction(EPacketType.PT_InGame_SynthesisSuccess, RPC_S2C_ReplicateSuccess);
		BindAction(EPacketType.PT_InGame_PersonalStorageStore, RPC_S2C_PersonalStorageStore);
		BindAction(EPacketType.PT_InGame_PersonalStroageDelete, RPC_S2C_PersonalStorageDelete);
		BindAction(EPacketType.PT_InGame_PersonalStorageFetch, RPC_S2C_PersonalStorageFetch);
		BindAction(EPacketType.PT_InGame_PersonalStorageSplit, RPC_S2C_PersonalStorageSplit);
		BindAction(EPacketType.PT_InGame_PersonalStorageExchange, RPC_S2C_PersonalStorageExchange);
		BindAction(EPacketType.PT_InGame_PersonalStorageSort, RPC_S2C_PersonalStorageSort);
		BindAction(EPacketType.PT_InGame_PersonalStorageIndex, RPC_S2C_PersonalStorageIndex);
		BindAction(EPacketType.PT_InGame_PublicStorageStore, RPC_S2C_PublicStorageStore);
		BindAction(EPacketType.PT_InGame_PublicStorageFetch, RPC_S2C_PublicStorageFetch);
		BindAction(EPacketType.PT_InGame_PublicStorageDelete, RPC_S2C_PublicStorageDelete);
		BindAction(EPacketType.PT_InGame_PublicStorageSplit, RPC_S2C_PublicStorageSplit);
		BindAction(EPacketType.PT_InGame_PublicStorageIndex, RPC_S2C_PublicStorageIndex);
		BindAction(EPacketType.PT_InGame_TownAreaArray, RPC_S2C_TownAreaList);
		BindAction(EPacketType.PT_InGame_CampAreaArray, RPC_S2C_CampAreaList);
		BindAction(EPacketType.PT_InGame_MaskAreaArray, RPC_S2C_MaskAreaList);
		BindAction(EPacketType.PT_InGame_TownArea, RPC_S2C_AddTownArea);
		BindAction(EPacketType.PT_InGame_CampArea, RPC_S2C_AddCampArea);
		BindAction(EPacketType.PT_InGame_ExploredArea, RPC_S2C_ExploredArea);
		BindAction(EPacketType.PT_InGame_ExploredAreaArray, RPC_S2C_ExploredAreas);
		BindAction(EPacketType.PT_InGame_Plant_GetBack, RPC_S2C_Plant_GetBack);
		BindAction(EPacketType.PT_InGame_Plant_PutOut, RPC_S2C_Plant_PutOut);
		BindAction(EPacketType.PT_InGame_Plant_VFTerrainTarget, RPC_S2C_Plant_VFTerrainTarget);
		BindAction(EPacketType.PT_InGame_Plant_FarmInfo, RPC_S2C_Plant_FarmInfo);
		BindAction(EPacketType.PT_InGame_Plant_Water, RPC_S2C_Plant_Water);
		BindAction(EPacketType.PT_InGame_Plant_Clean, RPC_S2C_Plant_Clean);
		BindAction(EPacketType.PT_InGame_Plant_Clear, RPC_S2C_Plant_Clear);
		BindAction(EPacketType.PT_InGame_Railway_AddPoint, RPC_S2C_Railway_AddPoint);
		BindAction(EPacketType.PT_InGame_Railway_PrePointChange, RPC_S2C_Railway_PrePointChange);
		BindAction(EPacketType.PT_InGame_Railway_NextPointChange, RPC_S2C_Raileway_NextPointChange);
		BindAction(EPacketType.PT_InGame_Railway_Recycle, RPC_S2C_Railway_Recycle);
		BindAction(EPacketType.PT_InGame_Railway_Route, RPC_S2C_Railway_Route);
		BindAction(EPacketType.PT_InGame_Railway_GetOnTrain, RPC_S2C_Railway_GetOnTrain);
		BindAction(EPacketType.PT_InGame_Railway_GetOffTrain, RPC_S2C_Railway_GetOffTrain);
		BindAction(EPacketType.PT_InGame_Railway_DeleteRoute, RPC_S2C_Railway_DeleteRoute);
		BindAction(EPacketType.PT_InGame_Railway_SetRouteTrain, RPC_S2C_Railway_SetRouteTrain);
		BindAction(EPacketType.PT_InGame_Railway_ChangeStationRot, RPC_S2C_Railway_ChangeStationRot);
		BindAction(EPacketType.PT_InGame_Railway_GetOffTrainEx, RPC_S2C_Railway_GetOffTrainEx);
		BindAction(EPacketType.PT_InGame_Railway_ResetPointName, RPC_S2C_Railway_ResetPointName);
		BindAction(EPacketType.PT_InGame_Railway_ResetRouteName, RPC_S2C_Railway_ResetRouteName);
		BindAction(EPacketType.PT_InGame_Railway_ResetPointTime, RPC_S2C_Railway_ResetPointTime);
		BindAction(EPacketType.PT_InGame_Railway_AutoCreateRoute, RPC_S2C_Railway_AutoCreateRoute);
		BindAction(EPacketType.PT_InGame_AccItems_CreateItem, RPC_AccItems_CreateItem);
		BindAction(EPacketType.PT_InGame_SKTLevelUp, RPC_S2C_SKTLevelUp);
		BindAction(EPacketType.PT_Test_PutOnEquipment, RPC_S2C_Test_PutOnEquipment);
		BindAction(EPacketType.PT_InGame_RandomItem, RPC_S2C_GenRandomItem);
		BindAction(EPacketType.PT_InGame_RandomItemRare, RPC_S2C_GenRandomItemRare);
		BindAction(EPacketType.PT_InGame_RandomIsoCode, RPC_S2C_GetRandomIsoCode);
		BindAction(EPacketType.PT_InGame_RandomItemRareAry, RPC_S2C_RandomItemRareAry);
		BindAction(EPacketType.PT_InGame_RandomItemFetch, RPC_S2C_RandomItemFetch);
		BindAction(EPacketType.PT_InGame_RandomItemFetchAll, RPC_S2C_RandomItemFetchAll);
		BindAction(EPacketType.PT_InGame_RandomFeces, RPC_S2C_GenRandomFeces);
		BindAction(EPacketType.PT_InGame_RandomItemClicked, RPC_S2C_RandomItemClicked);
		BindAction(EPacketType.PT_InGame_RandomItemDestroy, RPC_S2C_RandomItemDestroy);
		BindAction(EPacketType.PT_InGame_RandomItemDestroyList, RPC_S2C_RandomItemDestroyList);
		BindAction(EPacketType.PT_InGame_EnterDungeon, RPC_S2C_EnterDungeon);
		BindAction(EPacketType.PT_InGame_ExitDungeon, RPC_S2C_ExitDungeon);
		BindAction(EPacketType.PT_InGame_GenDunEntrance, RPC_S2C_GenDunEntrance);
		BindAction(EPacketType.PT_InGame_GenDunEntranceList, RPC_S2C_GenDunEntranceList);
		BindAction(EPacketType.PT_InGame_InitWhenSpawn, RPC_S2C_InitWhenSpawn);
		BindAction(EPacketType.PT_InGame_CreateTeam, RPC_S2C_CreateNewTeam);
		BindAction(EPacketType.PT_InGame_JoinTeam, RPC_S2C_JoinTeam);
		BindAction(EPacketType.PT_InGame_ApproveJoin, RPC_S2C_ApproveJoin);
		BindAction(EPacketType.PT_InGame_DenyJoin, RPC_S2C_DenyJoin);
		BindAction(EPacketType.PT_InGame_Invitation, RPC_S2C_Invitation);
		BindAction(EPacketType.PT_InGame_AcceptJoinTeam, RPC_S2C_AcceptJoinTeam);
		BindAction(EPacketType.PT_InGame_KickSB, RPC_S2C_KickSB);
		BindAction(EPacketType.PT_InGame_LeaderDeliver, RPC_S2C_LeaderDeliver);
		BindAction(EPacketType.PT_InGame_QuitTeam, RPC_S2C_QuitTeam);
		BindAction(EPacketType.PT_InGame_DissolveTeam, RPC_S2C_DissolveTeam);
		BindAction(EPacketType.PT_InGame_TeamInfo, RPC_S2C_TeamInfo);
		BindAction(EPacketType.PT_InGame_ReviveSB, RPC_S2C_ReviveSB);
		BindAction(EPacketType.PT_Common_FoundMapLable, RPC_S2C_FoundMapLable);
		BindAction(EPacketType.PT_InGame_SyncArmorInfo, RPC_S2C_SyncArmorInfo);
		BindAction(EPacketType.PT_InGame_ArmorDurability, RPC_S2C_ArmorDurability);
		BindAction(EPacketType.PT_InGame_SwitchArmorSuit, RPC_C2S_SwitchArmorSuit);
		BindAction(EPacketType.PT_InGame_EquipArmorPart, RPC_C2S_EquipArmorPart);
		BindAction(EPacketType.PT_InGame_RemoveArmorPart, RPC_C2S_RemoveArmorPart);
		BindAction(EPacketType.PT_InGame_SwitchArmorPartMirror, RPC_C2S_SwitchArmorPartMirror);
		BindAction(EPacketType.PT_InGame_SyncArmorPartPos, RPC_C2S_SyncArmorPartPos);
		BindAction(EPacketType.PT_InGame_SyncArmorPartRot, RPC_C2S_SyncArmorPartRot);
		BindAction(EPacketType.PT_InGame_SyncArmorPartScale, RPC_C2S_SyncArmorPartScale);
		BindAction(EPacketType.PT_CL_MGR_InitData, RPC_S2C_Mgr_InitData);
		BindAction(EPacketType.PT_Custom_CheckResult, RPC_S2C_CustomCheckResult);
		BindAction(EPacketType.PT_Custom_AddQuest, RPC_S2C_CustomAddQuest);
		BindAction(EPacketType.PT_Custom_RemoveQuest, RPC_S2C_CustomRemoveQuest);
		BindAction(EPacketType.PT_Custom_AddChoice, RPC_S2C_CustomAddChoice);
		BindAction(EPacketType.PT_Custom_EnableSpawn, RPC_S2C_EnableSpawn);
		BindAction(EPacketType.PT_Custom_DisableSpawn, RPC_S2C_DisableSpawn);
		BindAction(EPacketType.PT_Custom_OrderTarget, RPC_S2C_OrderTarget);
		BindAction(EPacketType.PT_Custom_CancelOrder, RPC_S2C_CancelOrder);
		BindAction(EPacketType.PT_Custom_StartAnimation, RPC_S2C_PlayAnimation);
		BindAction(EPacketType.PT_Custom_StopAnimation, RPC_S2C_StopAnimation);
		BindAction(EPacketType.PT_Common_ScenarioId, RPC_S2C_ScenarioId);
		BindAction(EPacketType.PT_InGame_CurTeamId, RPC_S2C_CurTeamId);
		BindAction(EPacketType.PT_InGame_ClearGrass, RPC_S2C_ClearGrass);
		BindAction(EPacketType.PT_InGame_ClearTree, RPC_S2C_ClearTree);
		BindAction(EPacketType.PT_CheatingChecked, RPC_S2C_CheatingChecked);
		BindAction(EPacketType.PT_Mount_ReqMonsterCtrl, RPC_S2C_ReqMonsterCtrl);
		BindAction(EPacketType.PT_Mount_AddMountMonster, RPC_S2C_AddMountMonster);
		BindAction(EPacketType.PT_Mount_DelMountMonster, RPC_S2C_DelMountMonste);
		BindAction(EPacketType.PT_Mount_SyncPlayerRot, RPC_S2C_SyncPlayerRot);
		if (base.IsOwner)
		{
			RequestTerrainData();
		}
		StartCoroutine(WaitForTerrainData());
	}

	protected override void OnPEDestroy()
	{
		if (null != entity)
		{
			entity.SendMsg(EMsg.Net_Destroy);
		}
		base.OnPEDestroy();
		RemovePlayer();
		GroupNetwork.DelJoinRequest(this);
		GroupNetwork.RemoveFromTeam(base.TeamId, this);
		if (base.Id != mainPlayerId)
		{
			OnTeamChanged(base.TeamId);
			string @string = PELocalization.GetString(8000168);
			new PeTipMsg(@string.Replace("Playername%", RoleName), PeTipMsg.EMsgLevel.Norm);
			return;
		}
		DetachUIEvent();
		if (PeGameMgr.IsMultiStory)
		{
			PeGameMgr.yirdName = null;
		}
	}

	public override void OnPeMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
			OnPrefabViewBuild();
			break;
		case EMsg.View_Prefab_Destroy:
			OnOnPrefabViewDestroy();
			break;
		}
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		if (null == entity.netCmpt)
		{
			entity.netCmpt = entity.Add<NetCmpt>();
		}
		entity.netCmpt.network = this;
		_learntSkills = runner.SkEntityPE.gameObject.GetComponent<SkillTreeUnitMgr>();
		if (_learntSkills != null)
		{
			_learntSkills.SetNet(this);
		}
		RequestData();
	}

	public override void SetTeamId(int teamId)
	{
		base.SetTeamId(teamId);
		ForceSetting.AddPlayer(base.Id, base.TeamId, EPlayerType.Human, RoleName);
	}

	private void OnPrefabViewBuild()
	{
		if (this.OnPrefabViewBuildEvent != null)
		{
			this.OnPrefabViewBuildEvent(this);
		}
	}

	private void OnOnPrefabViewDestroy()
	{
		if (this.OnPrefabViewDestroyEvent != null)
		{
			this.OnPrefabViewDestroyEvent(this);
		}
	}

	public static PlayerNetwork GetNearestPlayer(Vector3 pos)
	{
		List<PlayerNetwork> list = NetworkInterface.Get<PlayerNetwork>();
		if (list == null || list.Count == 0 || list[0] == null)
		{
			return null;
		}
		PlayerNetwork playerNetwork = list[0];
		foreach (PlayerNetwork item in list)
		{
			if (item != null && Vector3.Distance(item.transform.position, pos) < Vector3.Distance(playerNetwork.transform.position, pos))
			{
				playerNetwork = item;
			}
		}
		return playerNetwork;
	}

	private void AddPlayer()
	{
		if (PlayerDic.ContainsKey(base.Id))
		{
			PlayerDic.Remove(base.Id);
		}
		PlayerDic.Add(base.Id, this);
		PlayerList.Add(this);
	}

	private void RemovePlayer()
	{
		if (PlayerDic.ContainsKey(base.Id))
		{
			PlayerDic.Remove(base.Id);
		}
		PlayerList.Remove(this);
	}

	public static PlayerNetwork GetPlayer(int id)
	{
		if (!PlayerDic.ContainsKey(id))
		{
			return null;
		}
		return PlayerDic[id];
	}

	public static void PlayerAction(Action<PlayerNetwork> action)
	{
		foreach (PlayerNetwork player in PlayerList)
		{
			action(player);
		}
	}

	public void CreateMapObj(int objType, MapObj[] mapObj)
	{
		if (mapObj != null && mapObj.Count() != 0)
		{
			RPCServer(EPacketType.PT_InGame_CreateMapObj, objType, mapObj.ToArray(), true);
		}
	}

	public void CreateSceneBox(int boxId)
	{
		RPCServer(EPacketType.PT_InGame_CreateSceneBox, boxId);
	}

	public void CreateSceneItem(string sceneItemName, Vector3 pos, string items, int idx = -1, bool precise = false)
	{
		RPCServer(EPacketType.PT_InGame_CreateSceneItem, sceneItemName, pos, items, idx, precise);
	}

	public void DestroySceneItem(Vector3 pos)
	{
		if (base.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_MO_Destroy, pos);
		}
	}

	private IEnumerator HeartBeat()
	{
		while (true)
		{
			yield return new WaitForSeconds(2f);
			RPCServer(EPacketType.PT_InGame_HeartBeat);
		}
	}

	private IEnumerator PlayerMove()
	{
		while (true)
		{
			if (null != _transCmpt && _move != null && (Mathf.Abs(_transCmpt.position.x - mPlayerSynAttribute.mv3Postion.x) > 0.1f || Mathf.Abs(_transCmpt.position.y - mPlayerSynAttribute.mv3Postion.y) > 0.1f || Mathf.Abs(_transCmpt.position.z - mPlayerSynAttribute.mv3Postion.z) > 0.1f || Mathf.Abs(_transCmpt.rotation.eulerAngles.y - mPlayerSynAttribute.mfRotationY) > 0.1f))
			{
				URPCServer(EPacketType.PT_InGame_PlayerPosition, _transCmpt.position, (byte)_move.speed, _transCmpt.rotation.eulerAngles.y, GameTime.Timer.Second);
				mPlayerSynAttribute.mv3Postion = _transCmpt.position;
				mPlayerSynAttribute.mnPlayerState = _move.speed;
				mPlayerSynAttribute.mfRotationY = _transCmpt.rotation.eulerAngles.y;
				PlayerNetwork playerNetwork = this;
				Vector3 position = _transCmpt.position;
				base.transform.position = position;
				playerNetwork._pos = position;
				PlayerNetwork playerNetwork2 = this;
				Quaternion rotation = _transCmpt.rotation;
				base.transform.rotation = rotation;
				playerNetwork2.rot = rotation;
			}
			yield return new WaitForSeconds(1f / uLink.Network.sendRate);
		}
	}

	private void InitializePeEntity()
	{
		CustomCharactor.CustomData data = networkBase.Role.CreateCustomData();
		entity = PeSingleton<PeEntityCreator>.Instance.CreatePlayer(base.Id, Vector3.zero, Quaternion.identity, Vector3.one, data);
		if (base.IsProxy)
		{
			MainPlayerCmpt cmpt = entity.GetCmpt<MainPlayerCmpt>();
			if (null != cmpt)
			{
				entity.Remove(cmpt);
			}
		}
		else
		{
			MapCmpt cmpt2 = entity.GetCmpt<MapCmpt>();
			if (null != cmpt2)
			{
				entity.Remove(cmpt2);
			}
			PeSingleton<MainPlayer>.Instance.SetEntityId(base.Id);
			AttachUIEvent();
			_missionInited = false;
		}
		_transCmpt = entity.peTrans;
		if (null != _transCmpt)
		{
			_transCmpt.position = base.transform.position;
		}
		_move = entity.GetCmpt<Motion_Move>();
		if (_move != null && mainPlayerId != base.Id)
		{
			_move.NetMoveTo(base.transform.position, Vector3.zero, immediately: true);
		}
		base.OnSkAttrInitEvent += InitForceData;
		OnSpawned(entity.GetGameObject());
	}

	public override void InitForceData()
	{
		ForceSetting.AddPlayer(base.Id, base.TeamId, EPlayerType.Human, RoleName);
		ForceSetting.AddPlayer(base.TeamId, base.TeamId, EPlayerType.Human, "Team" + base.TeamId);
		if (PeGameMgr.IsSurvive)
		{
			ForceSetting.AddForce(base.TeamId, PeGameMgr.EGameType.Survive);
		}
		if (null != entity)
		{
			entity.SetAttribute(AttribType.DefaultPlayerID, base.Id);
			entity.SetAttribute(AttribType.CampID, base.TeamId);
			entity.SetAttribute(AttribType.DamageID, base.TeamId);
		}
	}

	private void AttachUIEvent()
	{
		CSUI_TeamInfoMgr.CreatTeamEvent += RequestNewTeam;
		CSUI_TeamInfoMgr.JoinTeamEvent += RequestJoinTeam;
		CSUI_TeamInfoMgr.KickTeamEvent += RequestKickSB;
		CSUI_TeamInfoMgr.AcceptJoinTeamEvent += SyncAcceptJoinTeam;
		CSUI_TeamInfoMgr.OnAgreeJoinEvent += RequestApproveJoin;
		CSUI_TeamInfoMgr.OnDeliverToEvent += RequestLeaderDeliver;
		CSUI_TeamInfoMgr.OnMemberQuitTeamEvent += RequestQuitTeam;
		CSUI_TeamInfoMgr.OnInvitationEvent += RequestInvitation;
		CSUI_TeamInfoMgr.OnDissolveEvent += RequestDissolveTeam;
	}

	private void DetachUIEvent()
	{
		CSUI_TeamInfoMgr.CreatTeamEvent -= RequestNewTeam;
		CSUI_TeamInfoMgr.JoinTeamEvent -= RequestJoinTeam;
		CSUI_TeamInfoMgr.KickTeamEvent -= RequestKickSB;
		CSUI_TeamInfoMgr.AcceptJoinTeamEvent -= SyncAcceptJoinTeam;
		CSUI_TeamInfoMgr.OnAgreeJoinEvent -= RequestApproveJoin;
		CSUI_TeamInfoMgr.OnDeliverToEvent -= RequestLeaderDeliver;
		CSUI_TeamInfoMgr.OnMemberQuitTeamEvent -= RequestQuitTeam;
		CSUI_TeamInfoMgr.OnInvitationEvent -= RequestInvitation;
		CSUI_TeamInfoMgr.OnDissolveEvent -= RequestDissolveTeam;
	}

	private IEnumerator GetOnVehicle(int id)
	{
		while (true)
		{
			DriveCreation = NetworkInterface.Get<CreationNetwork>(id);
			if (!(null != DriveCreation))
			{
				break;
			}
			if (DriveCreation.GetOn(base.Runner, SeatIndex))
			{
				if (SeatIndex == -1)
				{
					DriveCreation.Driver = this;
				}
				else
				{
					DriveCreation.AddPassanger(this);
				}
				break;
			}
			yield return null;
		}
	}

	public void GetOffVehicle(Vector3 pos, EVCComponent seatType)
	{
		if (null != DriveCreation)
		{
			DriveCreation.GetOff(pos, seatType);
			DriveCreation = null;
		}
	}

	private IEnumerator WaitForTerrainData()
	{
		while (!isTerrainDataOk)
		{
			yield return null;
		}
		RequestInitData();
	}

	public Vector3 GetCustomModePos()
	{
		Vector3 pos;
		if (null == entity)
		{
			ForceSetting.GetForcePos(base.TeamId, out pos);
		}
		else if (!ForceSetting.GetScenarioPos(entity.scenarioId, out pos))
		{
			ForceSetting.GetForcePos(base.TeamId, out pos);
		}
		return pos;
	}

	public void OnCustomDeath(int scenarioId)
	{
		if (this.OnCustomDeathEventHandler != null)
		{
			this.OnCustomDeathEventHandler(scenarioId);
		}
	}

	public void OnCustomDamage(int scenarioId, int casterScenarioId, float damage)
	{
		if (this.OnCustomDamageEventHandler != null)
		{
			this.OnCustomDamageEventHandler(scenarioId, casterScenarioId, damage);
		}
	}

	public void OnCustomUseItem(int customId, int itemInstanceId)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemInstanceId);
		if (itemObject != null && this.OnCustomUseItemEventHandler != null)
		{
			this.OnCustomUseItemEventHandler(itemObject);
		}
	}

	public void OnCustomPutoutItem(int customId, int itemInstanceId)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemInstanceId);
		if (itemObject != null && this.OnCustomPutOutItemEventHandler != null)
		{
			this.OnCustomPutOutItemEventHandler(itemObject);
		}
	}

	public void RequestTerrainData()
	{
		RPCServer(EPacketType.PT_InGame_TerrainData);
	}

	public void RequestInitData()
	{
		RPCServer(EPacketType.PT_InGame_RequestInit);
	}

	public void RequestData()
	{
		RPCServer(EPacketType.PT_InGame_RequestData, base.IsOwner, SteamMgr.steamId.m_SteamID);
	}

	public void SyncPostion(Vector3 pos)
	{
		URPCServer(EPacketType.PT_InGame_PlayerPosition, pos);
	}

	public void SyncRotY(float rotY)
	{
		URPCServer(EPacketType.PT_InGame_PlayerRot, rotY);
	}

	public void SyncSpeedState(SpeedState speed)
	{
		URPCServer(EPacketType.PT_InGame_PlayerState, (byte)speed);
	}

	public void SyncSpawnPos(byte[] binPos)
	{
		RPCServer(EPacketType.PT_AI_SpawnPos, binPos);
	}

	public void RequestAddItem(int objID, int splitNum)
	{
		RPCServer(EPacketType.PT_Test_AddItem, objID, splitNum);
	}

	public void RequestMoveNpc(int proid, Vector3 pos)
	{
		RPCServer(EPacketType.PT_Test_MoveNpc, proid, pos);
	}

	public void RequestSplitItem(int objID, int splitNum)
	{
		RPCServer(EPacketType.PT_InGame_PackageSplit, objID, splitNum);
	}

	public void RequestDeleteItem(int objID, int tabIndex, int itemIndex)
	{
		RPCServer(EPacketType.PT_InGame_PackageDelete, objID, tabIndex, itemIndex);
	}

	public void RequestSortPackage(int tabIndex)
	{
		RPCServer(EPacketType.PT_InGame_PackageSort, tabIndex);
	}

	public void RequestExchangeItem(ItemObject itemObj, int srcIndex, int destIndex)
	{
		if (itemObj != null)
		{
			RPCServer(EPacketType.PT_InGame_ExchangeItem, itemObj.instanceId, srcIndex, destIndex);
		}
	}

	public void RequestPutOnEquipment(ItemObject itemObj, int index)
	{
		if (itemObj != null)
		{
			RPCServer(EPacketType.PT_InGame_PutOnEquipment, itemObj.instanceId, index);
		}
	}

	public void RequestAddFountMapLable(int mapLableId)
	{
		RPCServer(EPacketType.PT_Common_FoundMapLable, mapLableId);
	}

	public void RequestTakeOffEquipment(ItemObject itemObj)
	{
		if (itemObj != null && itemObj.GetCmpt<Equip>() != null)
		{
			if (!PeGender.IsMatch(itemObj.protoData.equipSex, PeGender.Convert(Sex)))
			{
				string @string = PELocalization.GetString(8000093);
				MessageBox_N.ShowOkBox(@string);
			}
			else
			{
				RPCServer(EPacketType.PT_InGame_TakeOffEquipment, itemObj.instanceId);
			}
		}
	}

	public void RequestPublicStorageStore(int objID, int storageIndex)
	{
		RPCServer(EPacketType.PT_InGame_PublicStorageStore, objID, storageIndex);
	}

	public void RequestPublicStorageExchange(int objID, int srcIndex, int destIndex)
	{
		RPCServer(EPacketType.PT_InGame_PublicStorageExchange, objID, srcIndex, destIndex);
	}

	public void RequestPublicStorageFetch(int objID, int packageIndex)
	{
		RPCServer(EPacketType.PT_InGame_PublicStorageFetch, objID, packageIndex);
	}

	public void RequestPublicStroageDelete(int objID)
	{
		RPCServer(EPacketType.PT_InGame_PublicStorageDelete, objID);
	}

	public void RequestPublicStorageSort(int tab)
	{
		RPCServer(EPacketType.PT_InGame_PublicStorageSort, tab);
	}

	public void RequestPublicStorageSplite(int objID, int num)
	{
		RPCServer(EPacketType.PT_InGame_PublicStorageSplit, objID, num);
	}

	public void RequestPersonalStorageSort(int tabIndex)
	{
		RPCServer(EPacketType.PT_InGame_PersonalStorageSort, tabIndex);
	}

	public void RequestPersonalStorageSplit(int objID, int num)
	{
		RPCServer(EPacketType.PT_InGame_PersonalStorageSplit, objID, num);
	}

	public void RequestPersonalStorageStore(int objID, int dstIndex)
	{
		RPCServer(EPacketType.PT_InGame_PersonalStorageStore, objID, dstIndex);
	}

	public void RequestPersonalStorageExchange(int objID, int originIndex, int destIndex)
	{
		RPCServer(EPacketType.PT_InGame_PersonalStorageExchange, objID, originIndex, destIndex);
	}

	public void RequestPersonalStorageFetch(int objID, int dstIndex)
	{
		RPCServer(EPacketType.PT_InGame_PersonalStorageFetch, objID, dstIndex);
	}

	public void RequestPersonalStorageDelete(int objID)
	{
		RPCServer(EPacketType.PT_InGame_PersonalStroageDelete, objID);
	}

	public void RequestCreateBuildingWithItem(BuildingID buildingId, List<CreatItemInfo> itemInfoList, Vector3 root, int id, int rotation)
	{
		RPCServer(EPacketType.PT_InGame_CreateBuilding, buildingId, itemInfoList.ToArray(), root, id, rotation);
	}

	public void RequestMergeSkill(int mCurrentMergeId, int mCurrentNum)
	{
		RPCServer(EPacketType.PT_InGame_MergeSkill, mCurrentMergeId, mCurrentNum);
	}

	public void RequestReload(int id, int objId, int oldProtoId, int newProtoId, float magazineSize)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_WeaponReload, id, objId, oldProtoId, newProtoId, magazineSize);
		}
	}

	public void RequestGunEnergyReload(int id, int weaponId, float num)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_GunEnergyReload, id, weaponId, num);
		}
	}

	public void RequestBatteryEnergyReload(int id, int weaponId, float num)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_BatteryEnergyReload, id, weaponId, num);
		}
	}

	public void RequestJetPackEnergyReload(int id, int weaponId, float num)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_JetPackEnergyReload, id, weaponId, num);
		}
	}

	public void RequestWeaponDurability(int id, int weaponId)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_WeaponDurability, id, weaponId);
		}
	}

	public void RequestArmorDurability(int id, int[] equipIds, float damage, SkEntity caster)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (null != networkInterface && null != caster && caster.IsController())
		{
			RPCServer(EPacketType.PT_InGame_ArmorDurability, id, equipIds, damage);
		}
	}

	public void RequestAttrChanged(int entityId, int objId, float costNum, int bulletProtoId)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(entityId);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_ItemAttrChanged, entityId, objId, costNum, bulletProtoId);
		}
	}

	public void RequestThrow(int entityId, int objId, float costNum)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(entityId);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_EquipItemCost, entityId, objId, costNum);
		}
	}

	public void RequestItemCost(int entityId, int objId, float costNum)
	{
		NetworkInterface networkInterface = NetworkInterface.Get(entityId);
		if (null != networkInterface && networkInterface.hasOwnerAuth)
		{
			RPCServer(EPacketType.PT_InGame_PackageItemCost, entityId, objId, costNum);
		}
	}

	public void RequestRedo(int opType, IntVector3[] indexes, BSVoxel[] oldvoxels, BSVoxel[] voxels, EBSBrushMode mode, int dsType, float scale)
	{
		byte[] array = Serialize.Export(delegate(BinaryWriter w)
		{
			BufferHelper.Serialize(w, opType);
			BufferHelper.Serialize(w, (int)mode);
			BufferHelper.Serialize(w, dsType);
			BufferHelper.Serialize(w, scale);
			BufferHelper.Serialize(w, indexes.Length);
			for (int i = 0; i < indexes.Length; i++)
			{
				BufferHelper.Serialize(w, indexes[i]);
				BufferHelper.Serialize(w, oldvoxels[i]);
				BufferHelper.Serialize(w, voxels[i]);
			}
		});
		RPCServer(EPacketType.PT_InGame_BlockRedo, array);
	}

	public static IEnumerator RequestCreateAdNpc(int npcId, Vector3 pos)
	{
		while (null == mainPlayer)
		{
			yield return null;
		}
		mainPlayer.RPCServer(EPacketType.PT_NPC_CreateAd, npcId, pos);
	}

	public static IEnumerator RequestCreateAdMainNpc(int npcId, Vector3 pos)
	{
		while (null == mainPlayer)
		{
			yield return null;
		}
		mainPlayer.RPCServer(EPacketType.PT_NPC_CreateAdMainNpc, npcId, pos);
	}

	public void RequestCreateStRdNpc(int npcId, Vector3 pos, int protoId)
	{
		if (NetworkInterface.Get(npcId) == null && npcId <= 10000)
		{
			RPCServer(EPacketType.PT_NPC_CreateStRd, npcId, pos, protoId);
		}
	}

	public void RequestCreateStNpc(int npcId, Vector3 pos, int protoId)
	{
		if (NetworkInterface.Get(npcId) == null && npcId <= 10000)
		{
			RPCServer(EPacketType.PT_NPC_CreateSt, npcId, pos, protoId);
		}
	}

	public void RequestTownNpc(Vector3 pos, int key, int type = 0, bool isStand = false, float rotY = 0f)
	{
		RPCServer(EPacketType.PT_NPC_CreateTown, pos, key, type, isStand, rotY);
	}

	public static IEnumerator RequestCreateGroupAi(int aiId, Vector3 pos)
	{
		while (null == mainPlayer)
		{
			yield return null;
		}
		mainPlayer.RPCServer(EPacketType.PT_AI_SpawnGroupAI, aiId, pos);
	}

	public static IEnumerator RequestCreateAi(int aiId, Vector3 pos, int groupId, int tdId, int dungeonId, int colorType = -1, int playerId = -1, int buffId = 0)
	{
		while (null == mainPlayer)
		{
			yield return null;
		}
		mainPlayer.RPCServer(EPacketType.PT_AI_SpawnAI, aiId, pos, groupId, tdId, dungeonId, colorType, playerId, buffId);
	}

	public void RequestSetShortcuts(int itemId, int srcIndex, int destIndex, ItemPlaceType place)
	{
		RPCServer(EPacketType.PT_InGame_SetShortcut, itemId, srcIndex, destIndex, place);
	}

	public void RequestSendMsg(EMsgType msgtype, string msg)
	{
		RPCServer(EPacketType.PT_InGame_SendMsg, msgtype, msg);
	}

	public void RequestDeadObjAllItems(int id)
	{
		RPCServer(EPacketType.PT_InGame_GetAllDeadObjItem, id);
	}

	public void RequestDeadObjItem(int id, int index, int itemId)
	{
		RPCServer(EPacketType.PT_InGame_GetDeadObjItem, id, index, itemId);
	}

	public void RequestFastTravel(int wrapType, Vector3 pos, int cost)
	{
		RPCServer(EPacketType.PT_InGame_FastTransfer, pos, cost, wrapType);
	}

	public void RequestGetItemBack(int objId)
	{
		RPCServer(EPacketType.PT_InGame_GetItemBack, objId);
	}

	public static void PreRequestGetItemBack(int objId)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_PreGetItemBack, objId);
		}
	}

	public void RequestGetLootItemBack(int objId, bool bTimeout)
	{
		RPCServer(EPacketType.PT_InGame_GetLootItemBack, objId, bTimeout);
	}

	public void RequestGetItemListBack(int objId, ItemSample[] items)
	{
		RPCServer(EPacketType.PT_InGame_GetItemListBack, objId, items, false);
	}

	private static void RequestNewTeam()
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_CreateTeam);
		}
	}

	private static void RequestJoinTeam(int teamId, bool freeJoin)
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_JoinTeam, teamId, freeJoin);
		}
	}

	private static void RequestKickSB(PlayerNetwork player)
	{
		if (!(null == player) && !(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_KickSB, player.Id);
		}
	}

	private static void RequestApproveJoin(bool isAgree, PlayerNetwork player)
	{
		if (!(null == mainPlayer) && !(null == player))
		{
			if (isAgree)
			{
				mainPlayer.RPCServer(EPacketType.PT_InGame_ApproveJoin, player.Id);
			}
			else
			{
				mainPlayer.RPCServer(EPacketType.PT_InGame_DenyJoin, player.Id);
			}
			GroupNetwork.DelJoinRequest(player);
		}
	}

	private static void SyncAcceptJoinTeam(int inviterId, int teamId)
	{
		mainPlayer.RPCServer(EPacketType.PT_InGame_AcceptJoinTeam, inviterId, teamId);
	}

	public static void RequestInvitation(int id)
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_Invitation, id);
		}
	}

	public static void RequestDissolveTeam()
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_DissolveTeam);
		}
	}

	public static void RequestLeaderDeliver(int id)
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_LeaderDeliver, id);
		}
	}

	public static void RequestQuitTeam()
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_QuitTeam);
		}
	}

	public static void RequestReviveSB(int id)
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_ReviveSB, id);
		}
	}

	public static void RequestApprovalRevive(int id)
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_ApprovalRevive, id);
		}
	}

	public void RequestGameStarted(bool gameStarted)
	{
		_gameStarted = gameStarted;
		RPCServer(EPacketType.PT_InGame_GameStarted);
	}

	public static void RequestDismissNpc(int npcId)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_NPC_Dismiss, npcId);
		}
	}

	public static void SyncAbnormalConditionStart(int entityId, int type, byte[] data)
	{
		if (null != mainPlayer)
		{
			if (data == null)
			{
				mainPlayer.RPCServer(EPacketType.PT_InGame_AbnormalConditionStart, entityId, type, 0);
			}
			else
			{
				mainPlayer.RPCServer(EPacketType.PT_InGame_AbnormalConditionStart, entityId, type, 1, data);
			}
		}
	}

	public static void SyncAbnormalConditionEnd(int entityId, int type)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_AbnormalConditionEnd, entityId, type);
		}
	}

	public static void RequestServantRevive(int npcId, Vector3 pos)
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_NPC_ServentRevive, npcId, pos);
		}
	}

	public static void RequestServantAutoRevive(int npcId, Vector3 pos)
	{
		if (!(null == mainPlayer))
		{
			mainPlayer.RPCServer(EPacketType.PT_NPC_Revive, npcId, pos);
		}
	}

	public static void RequestSwitchArmorSuit(int newSuitIndex)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_SwitchArmorSuit, newSuitIndex);
		}
	}

	public static void RequestEquipArmorPart(int itemID, int typeValue, int boneGroup, int boneIndex)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_EquipArmorPart, itemID, typeValue, boneGroup, boneIndex);
		}
	}

	public static void RequestRemoveArmorPart(int boneGroup, int boneIndex, bool isDecoration)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_RemoveArmorPart, boneGroup, boneIndex, isDecoration);
		}
	}

	public static void RequestSwitchArmorPartMirror(int boneGroup, int boneIndex, bool isDecoration)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_SwitchArmorPartMirror, boneGroup, boneIndex, isDecoration);
		}
	}

	public static void SyncArmorPartPos(int boneGroup, int boneIndex, bool isDecoration, Vector3 position)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_SyncArmorPartPos, boneGroup, boneIndex, isDecoration, position);
		}
	}

	public static void SyncArmorPartRot(int boneGroup, int boneIndex, bool isDecoration, Quaternion rotation)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_SyncArmorPartRot, boneGroup, boneIndex, isDecoration, rotation);
		}
	}

	public static void SyncArmorPartScale(int boneGroup, int boneIndex, bool isDecoration, Vector3 scale)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_SyncArmorPartScale, boneGroup, boneIndex, isDecoration, scale);
		}
	}

	public static void RequestNpcRecruit(int npcId, bool findPlayer = false)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_NPC_Recruit, npcId, findPlayer);
		}
	}

	public static void RequestReqMonsterCtrl(int monsterEntityID)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_Mount_ReqMonsterCtrl, monsterEntityID);
		}
	}

	public static void RequestAddRideMonster(int monsterEntityID)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_Mount_AddMountMonster, monsterEntityID);
		}
	}

	public static void RequestDelMountMonster(int monsterEntityID)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_Mount_DelMountMonster, monsterEntityID);
		}
	}

	public void RequestSyncRotation(Vector3 rotation)
	{
		URPCServer(EPacketType.PT_Mount_SyncPlayerRot, rotation);
	}

	public static void RequestServer(params object[] objs)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(objs);
		}
	}

	public static void RequestuseItem(int itemObjId)
	{
		if (null != mainPlayer)
		{
			mainPlayer.RequestUseItem(itemObjId);
		}
	}

	private void RPC_S2C_InitDataOK(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_gameStarted = stream.Read<bool>(new object[0]);
		NetCmpt cmpt = entity.GetCmpt<NetCmpt>();
		if (null != cmpt)
		{
			cmpt.SetController(base.IsOwner);
		}
		if (null != mainPlayer && base.TeamId == mainPlayer.TeamId)
		{
			entity.SendMsg(EMsg.Net_Instantiate);
		}
		if (base.IsOwner)
		{
			if (Physics.Raycast(base.transform.position + 500f * Vector3.up, Vector3.down, out var hitInfo, 1000f, 595968))
			{
				base.transform.position = hitInfo.point;
			}
			base._pos = (mPlayerSynAttribute.mv3Postion = base.transform.position);
			StartCoroutine(PlayerMove());
			StartCoroutine(HeartBeat());
		}
		else
		{
			string @string = PELocalization.GetString(8000167);
			new PeTipMsg(@string.Replace("Playername%", RoleName), PeTipMsg.EMsgLevel.Norm);
		}
		ServerAdministrator.ProxyPlayerAdmin(this);
		RPCServer(EPacketType.PT_InGame_SKDAQueryEntityState);
		RequestAbnormalCondition();
		_initOk = true;
		if (PeGameMgr.IsMultiStory)
		{
			if (mainPlayerId == base.Id)
			{
				MissionManager.Instance.CheckEnableDienShipLight(_curSceneId);
			}
			if (_curSceneId != 4 && mainPlayerId == base.Id)
			{
				MissionManager.Instance.EnablePajaShipLight(bEnable: false);
			}
		}
	}

	private void RPC_S2C_SceneObject(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		SceneObject[] array = stream.Read<SceneObject[]>(new object[0]);
		SceneObject[] array2 = array;
		foreach (SceneObject sceneObject in array2)
		{
			switch (sceneObject.Type)
			{
			case ESceneObjType.ITEM:
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(sceneObject.Id);
				if (itemObject == null)
				{
					break;
				}
				Drag cmpt = itemObject.GetCmpt<Drag>();
				if (cmpt != null)
				{
					if (itemObject.protoId == 1339)
					{
						KillNPC.ashBox_inScene++;
					}
					DragArticleAgent dragArticleAgent = DragArticleAgent.Create(cmpt, sceneObject.Pos, sceneObject.Scale, sceneObject.Rot, sceneObject.Id);
					if (dragArticleAgent != null)
					{
						dragArticleAgent.ScenarioId = sceneObject.ScenarioId;
					}
				}
				break;
			}
			case ESceneObjType.DOODAD:
			{
				SceneEntityPosAgent sceneEntityPosAgent = DoodadEntityCreator.CreateAgent(sceneObject.Pos, sceneObject.ProtoId, sceneObject.Scale, sceneObject.Rot);
				if (sceneEntityPosAgent != null)
				{
					sceneEntityPosAgent.ScenarioId = sceneObject.ScenarioId;
					sceneEntityPosAgent.Id = sceneObject.Id;
					SceneMan.AddSceneObj(sceneEntityPosAgent);
				}
				break;
			}
			case ESceneObjType.EFFECT:
			{
				SceneStaticEffectAgent sceneStaticEffectAgent = SceneStaticEffectAgent.Create(sceneObject.ProtoId, sceneObject.Pos, sceneObject.Rot, sceneObject.Scale, sceneObject.Id);
				if (sceneStaticEffectAgent != null)
				{
					sceneStaticEffectAgent.ScenarioId = sceneObject.ScenarioId;
					SceneMan.AddSceneObj(sceneStaticEffectAgent);
				}
				break;
			}
			case ESceneObjType.DROPITEM:
				if (PeSingleton<ItemMgr>.Instance.Get(sceneObject.Id) == null)
				{
					Debug.LogError("LootItem is null id = " + sceneObject.Id);
					return;
				}
				PeSingleton<LootItemMgr>.Instance.NetAddLootItem(sceneObject.Pos, sceneObject.Id);
				break;
			}
		}
	}

	private void RPC_S2C_InitStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		stream.Read<bool>(new object[0]);
		stream.Read<bool>(new object[0]);
		stream.Read<Vector3>(new object[0]);
	}

	private void RPC_S2C_GetOnVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		SeatIndex = stream.Read<int>(new object[0]);
		StartCoroutine(GetOnVehicle(id));
	}

	private void RPC_S2C_GetOffVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		EVCComponent seatType = stream.Read<EVCComponent>(new object[0]);
		GetOffVehicle(vector, seatType);
		if (null != base.MtCmpt)
		{
			base.MtCmpt.EndAction(PEActionType.Drive);
		}
		if (null != base.Trans)
		{
			base.Trans.position = vector;
		}
	}

	private void RPC_S2C_RepairVehicle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		CreationNetwork creationNetwork = (CreationNetwork)NetworkInterface.Get(id);
		if (!(creationNetwork != null))
		{
		}
	}

	private void RPC_S2C_TerrainDataOk(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		isTerrainDataOk = true;
	}

	private void RPC_S2C_RequestInit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.transform.position = stream.Read<Vector3>(new object[0]);
		base.transform.rotation = Quaternion.Euler(0f, stream.Read<float>(new object[0]), 0f);
		InitializePeEntity();
	}

	private void RPC_S2C_FastTransfer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (!base.IsOwner)
		{
			base.transform.position = vector;
			if (null != _move)
			{
				_move.NetMoveTo(vector, Vector3.zero, immediately: true);
			}
		}
		if (!base.IsOwner)
		{
			return;
		}
		FastTravel.TravelTo(vector);
		ServantLeaderCmpt cmpt = entity.GetCmpt<ServantLeaderCmpt>();
		if (null == cmpt)
		{
			return;
		}
		NpcCmpt[] mFollowers = cmpt.mFollowers;
		foreach (NpcCmpt npcCmpt in mFollowers)
		{
			if (!(null == npcCmpt))
			{
				PeTrans peTrans = npcCmpt.Entity.peTrans;
				if (!(null == peTrans))
				{
					peTrans.position = PEUtil.GetRandomPosition(vector, 1.5f, 3f);
					(npcCmpt.Net as AiAdNpcNetwork).NpcMove();
				}
			}
		}
		foreach (NpcCmpt mForcedFollower in cmpt.mForcedFollowers)
		{
			if (!(null == mForcedFollower))
			{
				PeTrans peTrans2 = mForcedFollower.Entity.peTrans;
				if (!(null == peTrans2))
				{
					peTrans2.position = PEUtil.GetRandomPosition(vector, 1.5f, 3f);
					(mForcedFollower.Net as AiAdNpcNetwork).NpcMove();
				}
			}
		}
		GC.Collect();
		Resources.UnloadUnusedAssets();
	}

	private void RPC_S2C_SwitchScene(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int worldIndex = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		PeSingleton<FastTravelMgr>.Instance.TravelTo(worldIndex, pos);
	}

	private void RPC_S2C_DelSceneObjects(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = array;
		foreach (int id in array2)
		{
			ISceneObjAgent sceneObjById = SceneMan.GetSceneObjById(id);
			if (!object.Equals(null, sceneObjById))
			{
				SceneMan.RemoveSceneObj(sceneObjById);
			}
			PeSingleton<LootItemMgr>.Instance.RemoveLootItem(id);
		}
	}

	private void RPC_S2C_PlayerMovePosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base._pos = stream.Read<Vector3>(new object[0]);
		byte speed = stream.Read<byte>(new object[0]);
		float y = stream.Read<float>(new object[0]);
		double controllerTime = stream.Read<double>(new object[0]);
		base.rot = Quaternion.Euler(0f, y, 0f);
		base.transform.position = base._pos;
		base.transform.rotation = base.rot;
		if (!(_move == null))
		{
			_move.AddNetTransInfo(base._pos, base.rot.eulerAngles, (SpeedState)speed, controllerTime);
		}
	}

	private void RPC_S2C_PlayerMoveRotationY(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<float>(out var value))
		{
			base.rot = Quaternion.Euler(0f, value, 0f);
			base.transform.rotation = base.rot;
			if (null != entity && null != _transCmpt)
			{
				_transCmpt.rotation = base.rot;
			}
		}
	}

	private void RPC_S2C_PlayerMovePlayerState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<byte>(out var value) && _move != null)
		{
			_move.speed = (SpeedState)value;
		}
	}

	private void RPC_S2C_PlayerMoveGrounded(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_PlayerMoveShootTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<Vector3>(new object[0]);
	}

	private void RPC_S2C_SyncGliderStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_SyncParachuteStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_SyncJetPackStatus(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_PlayerRevive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float value = stream.Read<float>(new object[0]);
		float value2 = stream.Read<float>(new object[0]);
		float value3 = stream.Read<float>(new object[0]);
		base.transform.position = stream.Read<Vector3>(new object[0]);
		base.transform.rotation = stream.Read<Quaternion>(new object[0]);
		if (!(runner == null) && !(runner.SkEntityPE == null))
		{
			runner.SkEntityPE.SetAttribute(AttribType.Hp, value);
			runner.SkEntityPE.SetAttribute(AttribType.Oxygen, value2);
			runner.SkEntityPE.SetAttribute(AttribType.Stamina, value3);
			if (null != entity && null != _move)
			{
				_move.NetMoveTo(base.transform.position, Vector3.zero, immediately: true);
			}
			MotionMgrCmpt cmpt = runner.SkEntityPE.Entity.GetCmpt<MotionMgrCmpt>();
			if (cmpt != null)
			{
				PEActionParamB param = PEActionParamB.param;
				param.b = true;
				cmpt.DoActionImmediately(PEActionType.Revive, param);
			}
			if (object.Equals(mainPlayer, this))
			{
				GameUI.Instance.mRevive.Hide();
			}
		}
	}

	private void RPC_S2C_PlayerReset(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.IsOwner)
		{
		}
	}

	private void RPC_S2C_SetShortcut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int id2 = stream.Read<int>(new object[0]);
		if (num != -1)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
			if (itemObject == null)
			{
				cmpt.shortCutSlotList.PutItem(null, num);
			}
			else
			{
				cmpt.shortCutSlotList.PutItemObj(itemObject, num);
			}
		}
		if (num2 != -1)
		{
			ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.Get(id2);
			if (itemObject2 == null)
			{
				cmpt.shortCutSlotList.PutItem(null, num2);
			}
			else
			{
				cmpt.shortCutSlotList.PutItemObj(itemObject2, num2);
			}
		}
	}

	private void RPC_S2C_SendMsg(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<EMsgType>(new object[0]);
		string content = stream.Read<string>(new object[0]);
		if (null != UITalkwithctr.Instance && UITalkwithctr.Instance.isShow)
		{
			if (base.Id == mainPlayer.Id)
			{
				UITalkwithctr.Instance.AddTalk(RoleName, content, "99C68B");
			}
			else
			{
				UITalkwithctr.Instance.AddTalk(RoleName, content, "EDB1A6");
			}
		}
	}

	private void RPC_S2C_ApplyHpChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<float>(out var value);
		stream.TryRead<float>(out var value2);
		stream.TryRead<uLink.NetworkViewID>(out var value3);
		CommonInterface caster = null;
		uLink.NetworkView networkView = uLink.NetworkView.Find(value3);
		if (null != networkView)
		{
			NetworkInterface component = networkView.GetComponent<NetworkInterface>();
			if (null != component && null != component.Runner)
			{
				caster = component.Runner;
			}
		}
		if (null != base.Runner)
		{
			base.Runner.NetworkApplyDamage(caster, value, (int)value2);
		}
	}

	private void RPC_S2C_ApplyComfortChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_ApplySatiationChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_Death(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if ((bool)(networkInterface as SkNetworkInterface) && networkInterface.Runner != null && !(networkInterface.Runner.SkEntityPE != null))
		{
		}
	}

	private void RPC_S2C_ErrorMsgBox(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string text = stream.Read<string>(new object[0]);
		MessageBox_N.ShowOkBox(text);
	}

	private void RPC_S2C_ErrorMsg(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string content = stream.Read<string>(new object[0]);
		new PeTipMsg(content, PeTipMsg.EMsgLevel.Error);
	}

	private void RPC_S2C_ErrorMsgCode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int strId = stream.Read<int>(new object[0]);
		string @string = PELocalization.GetString(strId);
		new PeTipMsg(@string, PeTipMsg.EMsgLevel.Error);
	}

	private void RPC_S2C_Test_PutOnEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		EquipmentCmpt component = entity.GetGameObject().GetComponent<EquipmentCmpt>();
		if (component != null)
		{
			ItemObject itemObj = PeSingleton<ItemMgr>.Instance.CreateItem(1);
			component.PutOnEquipment(itemObj, addToReceiver: false, null, netRequest: true);
		}
	}

	private void RPC_S2C_CreateNewTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_JoinTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		PlayerNetwork playerNetwork = NetworkInterface.Get<PlayerNetwork>(id);
		if (!(null == playerNetwork))
		{
			GroupNetwork.AddJoinRequest(base.TeamId, playerNetwork);
			if (null != CSUI_TeamInfoMgr.Intance)
			{
				CSUI_TeamInfoMgr.Intance.JoinApply(base.TeamId);
			}
			OnTeamChanged(base.TeamId);
		}
	}

	private void RPC_S2C_ApproveJoin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_DenyJoin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		PlayerNetwork playerNetwork = NetworkInterface.Get<PlayerNetwork>(id);
		if (null != playerNetwork)
		{
			new PeTipMsg(PELocalization.GetString(8000856), PeTipMsg.EMsgLevel.Warning);
		}
	}

	private void RPC_S2C_Invitation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		PlayerNetwork playerNetwork = NetworkInterface.Get<PlayerNetwork>(id);
		if (!(null == playerNetwork) && null != CSUI_TeamInfoMgr.Intance)
		{
			CSUI_TeamInfoMgr.Intance.Invitation(playerNetwork);
		}
	}

	private void RPC_S2C_KickSB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		new PeTipMsg(PELocalization.GetString(8000857), PeTipMsg.EMsgLevel.Warning);
	}

	private void RPC_S2C_LeaderDeliver(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_QuitTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_DissolveTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_TeamInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int leader = stream.Read<int>(new object[0]);
		TeamData teamData = GroupNetwork.AddToTeam(num, this);
		if (teamData == null)
		{
			return;
		}
		teamData.SetLeader(leader);
		int teamId = base.TeamId;
		SetTeamId(num);
		if (teamId != -1 && num != teamId)
		{
			if (base.Id == mainPlayerId)
			{
				GroupNetwork.ClearJoinRequest();
			}
			ForceSetting.RemoveAllyForce(originTeamId, teamId);
			GroupNetwork.RemoveFromTeam(teamId, this);
			OnTeamChanged(teamId);
			if (null != CSUI_TeamInfoMgr.Intance)
			{
				CSUI_TeamInfoMgr.Intance.RefreshTeamGrid(teamId);
			}
		}
		GroupNetwork.AddToTeam(num, this);
		ForceSetting.AddAllyForce(originTeamId, num);
		OnTeamChanged(num);
		if (null != CSUI_TeamInfoMgr.Intance)
		{
			CSUI_TeamInfoMgr.Intance.RefreshTeamGrid(num);
		}
	}

	private void RPC_S2C_AcceptJoinTeam(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_ReviveSB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		PlayerNetwork playerNetwork = NetworkInterface.Get<PlayerNetwork>(id);
		if (!(null == playerNetwork))
		{
			MessageBox_N.ShowYNBox(string.Format(PELocalization.GetString(8000503), playerNetwork.RoleName), delegate
			{
				RequestApprovalRevive(id);
			});
		}
	}

	private void RPC_S2C_FoundMapLable(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			StaticPoint.StaticPointBeFound(array[i]);
		}
	}

	private void RPC_S2C_SyncArmorInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		if (array.Length <= 0)
		{
			return;
		}
		using MemoryStream input = new MemoryStream(array);
		using BinaryReader r = new BinaryReader(input);
		if (PlayerArmor != null)
		{
			PlayerArmor.Deserialize(r);
			PlayerArmor.Init(this);
		}
	}

	private void RPC_S2C_ArmorDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		float[] array2 = stream.Read<float[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(array[i]);
			if (itemObject == null)
			{
				continue;
			}
			NetworkInterface networkInterface = NetworkInterface.Get(id);
			if (networkInterface != null && networkInterface is PlayerNetwork)
			{
				Durability cmpt = itemObject.GetCmpt<Durability>();
				if (cmpt != null)
				{
					cmpt.floatValue.current = array2[i];
				}
				if (itemObject.protoData.tabIndex == 3 && cmpt.floatValue.current <= 0f)
				{
					(networkInterface as PlayerNetwork).PlayerArmor.RemoveBufferWhenBroken(itemObject);
				}
			}
		}
	}

	private void RPC_C2S_SwitchArmorSuit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int newSuitIndex = stream.Read<int>(new object[0]);
		bool success = stream.Read<bool>(new object[0]);
		PlayerArmor.S2C_SwitchArmorSuit(newSuitIndex, success);
	}

	private void RPC_C2S_EquipArmorPart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemID = stream.Read<int>(new object[0]);
		int typeValue = stream.Read<int>(new object[0]);
		int boneGroup = stream.Read<int>(new object[0]);
		int boneIndex = stream.Read<int>(new object[0]);
		bool success = stream.Read<bool>(new object[0]);
		PlayerArmor.S2C_EquipArmorPartFromPackage(itemID, typeValue, boneGroup, boneIndex, success);
	}

	private void RPC_C2S_RemoveArmorPart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int boneGroup = stream.Read<int>(new object[0]);
		int boneIndex = stream.Read<int>(new object[0]);
		bool isDecoration = stream.Read<bool>(new object[0]);
		bool success = stream.Read<bool>(new object[0]);
		PlayerArmor.S2C_RemoveArmorPart(boneGroup, boneIndex, isDecoration, success);
	}

	private void RPC_C2S_SwitchArmorPartMirror(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int boneGroup = stream.Read<int>(new object[0]);
		int boneIndex = stream.Read<int>(new object[0]);
		bool isDecoration = stream.Read<bool>(new object[0]);
		bool success = stream.Read<bool>(new object[0]);
		PlayerArmor.S2C_SwitchArmorPartMirror(boneGroup, boneIndex, isDecoration, success);
	}

	private void RPC_C2S_SyncArmorPartPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int boneGroup = stream.Read<int>(new object[0]);
		int boneIndex = stream.Read<int>(new object[0]);
		bool isDecoration = stream.Read<bool>(new object[0]);
		Vector3 position = stream.Read<Vector3>(new object[0]);
		PlayerArmor.S2C_SyncArmorPartPosition(boneGroup, boneIndex, isDecoration, position);
	}

	private void RPC_C2S_SyncArmorPartRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int boneGroup = stream.Read<int>(new object[0]);
		int boneIndex = stream.Read<int>(new object[0]);
		bool isDecoration = stream.Read<bool>(new object[0]);
		Quaternion rotation = stream.Read<Quaternion>(new object[0]);
		PlayerArmor.S2C_SyncArmorPartRotation(boneGroup, boneIndex, isDecoration, rotation);
	}

	private void RPC_C2S_SyncArmorPartScale(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int boneGroup = stream.Read<int>(new object[0]);
		int boneIndex = stream.Read<int>(new object[0]);
		bool isDecoration = stream.Read<bool>(new object[0]);
		Vector3 scale = stream.Read<Vector3>(new object[0]);
		PlayerArmor.S2C_SyncArmorPartScale(boneGroup, boneIndex, isDecoration, scale);
	}

	private void RPC_S2C_Mgr_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] packData = stream.Read<byte[]>(new object[0]);
		CSMgCreator creator = MultiColonyManager.GetCreator(BaseTeamId);
		creator.InitMultiData(packData);
	}

	private void RPC_S2C_ScenarioId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int scenarioId = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (null != peEntity)
		{
			peEntity.scenarioId = scenarioId;
		}
	}

	private void RPC_S2C_CustomCheckResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		bool result = stream.Read<bool>(new object[0]);
		ConditionReq.AlterReq(id, result);
	}

	private void RPC_S2C_CustomAddQuest(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			int quest_id = r.ReadInt32();
			string text = r.ReadString();
			PeCustomScene.Self.scenario.dialogMgr.SetQuest(obj.Group, obj.Id, quest_id, text);
		});
	}

	private void RPC_S2C_CustomRemoveQuest(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		BufferHelper.Import(buffer, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			int quest_id = r.ReadInt32();
			PeCustomScene.Self.scenario.dialogMgr.RemoveQuest(obj.Group, obj.Id, quest_id);
		});
	}

	private void RPC_S2C_CustomAddChoice(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int choose_id = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		PeCustomScene.Self.scenario.dialogMgr.AddChoose(choose_id, text);
	}

	private void RPC_S2C_EnableSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			PeScenarioUtility.EnableSpawnPoint(obj, enable: true);
		});
	}

	private void RPC_S2C_DisableSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			PeScenarioUtility.EnableSpawnPoint(obj, enable: false);
		});
	}

	private void RPC_S2C_OrderTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			BufferHelper.ReadObject(r, out var obj2);
			ECommand eCommand = (ECommand)r.ReadByte();
			PeEntity peEntity = PeScenarioUtility.GetEntity(obj);
			PeEntity peEntity2 = PeScenarioUtility.GetEntity(obj2);
			if (peEntity != null && peEntity2 != null && (peEntity.proto == EEntityProto.Npc || peEntity.proto == EEntityProto.RandomNpc || peEntity.proto == EEntityProto.Monster) && peEntity.requestCmpt != null)
			{
				switch (eCommand)
				{
				case ECommand.MoveTo:
					peEntity.requestCmpt.Register(EReqType.FollowTarget, peEntity2.Id);
					break;
				case ECommand.FaceAt:
					peEntity.requestCmpt.Register(EReqType.Dialogue, string.Empty, peEntity2.peTrans);
					break;
				}
			}
		});
	}

	private void RPC_S2C_CancelOrder(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			PeEntity peEntity = PeScenarioUtility.GetEntity(obj);
			if (peEntity != null && peEntity.requestCmpt != null)
			{
				if (peEntity.requestCmpt.Contains(EReqType.Dialogue))
				{
					peEntity.requestCmpt.RemoveRequest(EReqType.Dialogue);
				}
				if (peEntity.requestCmpt.Contains(EReqType.MoveToPoint))
				{
					peEntity.requestCmpt.RemoveRequest(EReqType.MoveToPoint);
				}
				if (peEntity.requestCmpt.Contains(EReqType.FollowTarget))
				{
					peEntity.requestCmpt.RemoveRequest(EReqType.FollowTarget);
				}
			}
		});
	}

	private void RPC_S2C_PlayAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			string text = r.ReadString();
			PeEntity peEntity = PeScenarioUtility.GetEntity(obj);
			if (!(null == peEntity))
			{
				if (obj.isCurrentPlayer)
				{
					PlayAnimAction.playerAniming = true;
					peEntity.animCmpt.AnimEvtString += delegate(string param)
					{
						if (param == "OnCustomAniEnd" && obj.isCurrentPlayer)
						{
							PlayAnimAction.playerAniming = false;
						}
					};
				}
				string text2 = text.Split('_')[text.Split('_').Length - 1];
				if (text2 == "Once")
				{
					peEntity.animCmpt.SetTrigger(text);
				}
				else if (text2 == "Muti")
				{
					peEntity.animCmpt.SetBool(text, value: true);
				}
			}
		});
	}

	private void RPC_S2C_StopAnimation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			BufferHelper.ReadObject(r, out var obj);
			PeEntity peEntity = PeScenarioUtility.GetEntity(obj);
			if (!(null == peEntity))
			{
				peEntity.animCmpt.SetTrigger("Custom_ResetAni");
			}
		});
	}

	private void RPC_S2C_CurTeamId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int curSurviveTeamId = stream.Read<int>(new object[0]);
		Singleton<ForceSetting>.Instance.InitGameForces(curSurviveTeamId);
	}

	private void RPC_S2C_ClearGrass(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadVector3(r, out var _value);
				DigTerrainManager.DeleteGrass(_value);
			}
		});
	}

	private void RPC_S2C_ClearTree(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				BufferHelper.ReadVector3(r, out var _value);
				DigTerrainManager.DeleteTree(_value);
			}
		});
	}

	private void RPC_S2C_CheatingChecked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		new PeTipMsg(PELocalization.GetString(8000895), PeTipMsg.EMsgLevel.Warning);
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000895));
	}

	private void RPC_S2C_ReqMonsterCtrl(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int entityId = stream.Read<int>(new object[0]);
		int entityId2 = stream.Read<int>(new object[0]);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(entityId2);
		if (!peEntity || !peEntity.operateCmpt || !peEntity2 || !peEntity2 || !peEntity2.biologyViewCmpt || !peEntity2.biologyViewCmpt.biologyViewRoot || !peEntity2.biologyViewCmpt.biologyViewRoot.modelController)
		{
			return;
		}
		MousePickRides component = peEntity2.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
		if ((bool)component)
		{
			PlayerPackageCmpt playerPackageCmpt = peEntity.packageCmpt as PlayerPackageCmpt;
			ItemObject itemObject = playerPackageCmpt.package.FindItemByProtoId(MousePickRides.RideItemID);
			if (itemObject != null && component.ExecRide(peEntity))
			{
				RequestItemCost(entityId, itemObject.instanceId, 1f);
			}
		}
	}

	private void RPC_S2C_AddMountMonster(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		AiNetwork aiNetwork = NetworkInterface.Get<AiNetwork>(num2);
		if (null != aiNetwork && (bool)aiNetwork._entity)
		{
			aiNetwork._entity.SetAttribute(AttribType.DefaultPlayerID, num);
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(num);
		PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(num2);
		if ((bool)peEntity && (bool)peEntity.mountCmpt && (bool)peEntity2)
		{
			peEntity.mountCmpt.SetMount(peEntity2);
		}
	}

	private void RPC_S2C_DelMountMonste(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int entityId = stream.Read<int>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		AiNetwork aiNetwork = NetworkInterface.Get<AiNetwork>(id);
		if (null != aiNetwork && (bool)aiNetwork._entity)
		{
			aiNetwork._entity.SetAttribute(AttribType.DefaultPlayerID, num);
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if ((bool)peEntity && (bool)peEntity.mountCmpt)
		{
			peEntity.mountCmpt.DelMount();
		}
	}

	private void RPC_S2C_SyncPlayerRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.TryRead<Vector3>(out var value))
		{
			base.rot = Quaternion.Euler(value);
			base.transform.rotation = base.rot;
			if (null != entity && null != _transCmpt)
			{
				_transCmpt.rotation = base.rot;
			}
		}
	}

	private void RPC_S2C_PlayerBattleInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_battleInfo = stream.Read<BattleInfo>(new object[0]);
	}

	public void RequestDragOut(int objId, Vector3 pos, Vector3 scale, Quaternion rot, byte terrainType)
	{
		RPCServer(EPacketType.PT_InGame_PutItem, objId, pos, scale, rot, terrainType);
	}

	public void RequestDragTower(int objId, Vector3 pos, Quaternion rot)
	{
		RPCServer(EPacketType.PT_InGame_PutOutTower, objId, pos, rot);
	}

	public void RequestDragFlag(int objId, Vector3 pos, Quaternion rot)
	{
		RPCServer(EPacketType.PT_InGame_PutOutFlag, objId, pos, rot);
	}

	public void RequestUseItem(int itemObjId)
	{
		RPCServer(EPacketType.PT_InGame_UseItem, itemObjId);
	}

	public void RequestNpcPutOnEquip(int npcId, int objId, ItemPlaceType place)
	{
		AiAdNpcNetwork aiAdNpcNetwork = NetworkInterface.Get<AiAdNpcNetwork>(npcId);
		if (!(null == aiAdNpcNetwork))
		{
			RPCServer(EPacketType.PT_NPC_PutOnEquip, npcId, objId, place);
		}
	}

	public void RequestNpcTakeOffEquip(int npcId, int objId, int destIndex)
	{
		AiAdNpcNetwork aiAdNpcNetwork = NetworkInterface.Get<AiAdNpcNetwork>(npcId);
		if (!(null == aiAdNpcNetwork))
		{
			RPCServer(EPacketType.PT_NPC_TakeOffEquip, npcId, objId, destIndex);
		}
	}

	public void RequestGiveItem2Npc(int tabIndex, int npcId, int objId, ItemPlaceType place)
	{
		AiAdNpcNetwork aiAdNpcNetwork = NetworkInterface.Get<AiAdNpcNetwork>(npcId);
		if (!(null == aiAdNpcNetwork))
		{
			RPCServer(EPacketType.PT_NPC_GetItem, tabIndex, npcId, objId, place);
		}
	}

	public void RequestGetItemFromNpc(int tabIndex, int npcId, int objId, int destIndex)
	{
		AiAdNpcNetwork aiAdNpcNetwork = NetworkInterface.Get<AiAdNpcNetwork>(npcId);
		if (!(null == aiAdNpcNetwork))
		{
			RPCServer(EPacketType.PT_NPC_DeleteItem, tabIndex, npcId, objId, destIndex);
		}
	}

	public void RequestGetAllItemFromNpc(int npcId, int tabIndex)
	{
		AiAdNpcNetwork aiAdNpcNetwork = NetworkInterface.Get<AiAdNpcNetwork>(npcId);
		if (!(null == aiAdNpcNetwork))
		{
			RPCServer(EPacketType.PT_NPC_DeleteAllItem, npcId, tabIndex);
		}
	}

	public void RequestNpcPackageSort(int npcId, int tabIndex)
	{
		AiAdNpcNetwork aiAdNpcNetwork = NetworkInterface.Get<AiAdNpcNetwork>(npcId);
		if (!(null == aiAdNpcNetwork))
		{
			RPCServer(EPacketType.PT_NPC_SortPackage, npcId, tabIndex);
		}
	}

	public void RequestChangeScene(int sceneId)
	{
		RPCServer(EPacketType.PT_InGame_CurSceneId, sceneId);
	}

	private void RPC_S2C_InitPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemMax = stream.Read<int>(new object[0]);
		int equipmentMax = stream.Read<int>(new object[0]);
		int recourceMax = stream.Read<int>(new object[0]);
		int armMax = stream.Read<int>(new object[0]);
		if (!object.Equals(null, entity))
		{
			PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
			if (null != cmpt)
			{
				cmpt.package.ExtendPackage(itemMax, equipmentMax, recourceMax, armMax);
			}
		}
	}

	private void RPC_S2C_PackageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		if (object.Equals(null, entity))
		{
			return;
		}
		PlayerPackageCmpt pkg = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == pkg)
		{
			return;
		}
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				int num2 = BufferHelper.ReadInt32(r);
				int id = BufferHelper.ReadInt32(r);
				int tab = num2 >> 16;
				int index = num2 & 0xFFFF;
				pkg.package.ResetPackageItems(tab, index, id, bMission: false);
			}
		});
		if (base.IsOwner)
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}

	private void RPC_S2C_MissionPackageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		if (object.Equals(null, entity))
		{
			return;
		}
		PlayerPackageCmpt pkg = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == pkg)
		{
			return;
		}
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				int num2 = BufferHelper.ReadInt32(r);
				int id = BufferHelper.ReadInt32(r);
				int tab = num2 >> 16;
				int index = num2 & 0xFFFF;
				pkg.package.ResetPackageItems(tab, index, id, bMission: true);
			}
		});
		if (base.IsOwner)
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}

	private void RPC_S2C_EquipedItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject[] itemList = stream.Read<ItemObject[]>(new object[0]);
		if (!object.Equals(null, entity))
		{
			EquipmentCmpt cmpt = entity.GetCmpt<EquipmentCmpt>();
			if (null != cmpt)
			{
				cmpt.ApplyEquipment(itemList);
			}
		}
	}

	private void RPC_S2C_PutOnEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject[] array = stream.Read<ItemObject[]>(new object[0]);
		if (object.Equals(null, entity))
		{
			return;
		}
		EquipmentCmpt cmpt = entity.GetCmpt<EquipmentCmpt>();
		if (null != cmpt)
		{
			ItemObject[] array2 = array;
			foreach (ItemObject itemObj in array2)
			{
				cmpt.PutOnEquipment(itemObj, addToReceiver: false, null, netRequest: true);
			}
		}
	}

	private void RPC_S2C_TakeOffEquipment(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		if (object.Equals(null, entity))
		{
			return;
		}
		EquipmentCmpt cmpt = entity.GetCmpt<EquipmentCmpt>();
		if (null != cmpt)
		{
			int[] array2 = array;
			foreach (int id in array2)
			{
				ItemObject itemObj = PeSingleton<ItemMgr>.Instance.Get(id);
				cmpt.TakeOffEquipment(itemObj, addToReceiver: false, null, netRequest: true);
			}
		}
	}

	private void RPC_S2C_InitShortcut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		if (object.Equals(null, entity))
		{
			return;
		}
		Dictionary<int, int> shortcut = new Dictionary<int, int>();
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				int key = BufferHelper.ReadInt32(r);
				int value = BufferHelper.ReadInt32(r);
				shortcut[key] = value;
			}
		});
		PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return;
		}
		foreach (KeyValuePair<int, int> item in shortcut)
		{
			ItemObject itemObj = PeSingleton<ItemMgr>.Instance.Get(item.Value);
			cmpt.shortCutSlotList.PutItemObj(itemObj, item.Key);
		}
	}

	private void RPC_S2C_PlayerMoney(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int current = stream.Read<int>(new object[0]);
		if (!object.Equals(null, entity))
		{
			PackageCmpt cmpt = entity.GetCmpt<PackageCmpt>();
			if (!(null == cmpt))
			{
				cmpt.money.current = current;
			}
		}
	}

	private void RPC_S2C_MoneyType(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		switch (stream.Read<int>(new object[0]))
		{
		case 2:
			Money.Digital = true;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(value: false);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(value: true);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(value: true);
			break;
		case 1:
			Money.Digital = false;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(value: true);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(value: false);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(value: false);
			break;
		}
	}

	private void RPC_S2C_CurSceneId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_curSceneId = stream.Read<int>(new object[0]);
		if (!PeGameMgr.IsMultiStory)
		{
			return;
		}
		PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(242);
		if (doodadEntities.Length > 0)
		{
			SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
			if (component != null)
			{
				component.IsShown = true;
			}
		}
		doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(240);
		if (doodadEntities.Length > 0)
		{
			SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
			if (component != null)
			{
				component.IsShown = true;
			}
		}
		doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(324);
		if (doodadEntities.Length > 0)
		{
			SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
			if (component != null)
			{
				component.IsShown = true;
			}
		}
		doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(326);
		if (doodadEntities.Length > 0)
		{
			SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
			if (component != null)
			{
				component.IsShown = true;
			}
		}
		doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(327);
		if (doodadEntities.Length > 0)
		{
			SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
			if (null != component)
			{
				component.IsShown = true;
			}
		}
		for (int i = 461; i < 464; i++)
		{
			doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(i);
			if (doodadEntities.Length > 0)
			{
				SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
				if (null != component)
				{
					component.IsShown = true;
				}
			}
		}
		if (mainPlayerId == base.Id)
		{
			MissionManager.Instance.CheckEnableDienShipLight(_curSceneId);
			MissionManager.Instance.EnablePajaShipLight(bEnable: false);
			if (_curSceneId == 4)
			{
				MissionManager.Instance.EnablePajaShipLight(bEnable: true);
			}
		}
	}

	private void RPC_S2C_FarmInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<ItemObject[]>(new object[0]);
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		if (null == FarmManager.Instance)
		{
			return;
		}
		List<FarmPlantInitData> list = FarmManager.Instance.ImportPlantData(buffer);
		foreach (FarmPlantInitData item in list)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(item.mPlantInstanceId);
			DragArticleAgent dragArticleAgent = DragArticleAgent.Create(itemObject.GetCmpt<Drag>(), item.mPos, Vector3.one, item.mRot, item.mPlantInstanceId);
			FarmPlantLogic farmPlantLogic = dragArticleAgent.itemLogic as FarmPlantLogic;
			farmPlantLogic.InitDataFromPlant(item);
			FarmManager.Instance.AddPlant(farmPlantLogic);
			farmPlantLogic.UpdateInMultiMode();
		}
	}

	private void RPC_S2C_GetDeadObjAllItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		AiNetwork aiNetwork = NetworkInterface.Get<AiNetwork>(id);
		if (null == aiNetwork || null == aiNetwork.Runner)
		{
			return;
		}
		ItemDropPeEntity component = aiNetwork.Runner.GetComponent<ItemDropPeEntity>();
		if (!(null == component))
		{
			component.RemoveDroppableItemAll();
			if (null != GameUI.Instance.mItemGet)
			{
				GameUI.Instance.mItemGet.Reflash();
			}
		}
	}

	private void RPC_S2C_GetDeadObjItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		AiNetwork aiNetwork = NetworkInterface.Get<AiNetwork>(id);
		if (null == aiNetwork || null == aiNetwork.Runner)
		{
			return;
		}
		ItemDropPeEntity component = aiNetwork.Runner.GetComponent<ItemDropPeEntity>();
		if (null == component)
		{
			return;
		}
		ItemSample itemSample = component.Get(index);
		if (itemSample != null && itemSample.protoId == num)
		{
			component.RemoveDroppableItem(itemSample);
			if (null != GameUI.Instance.mItemGet)
			{
				GameUI.Instance.mItemGet.Reflash();
			}
		}
	}

	private void RPC_S2C_GetItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		DragItemAgent byId = DragItemAgent.GetById(id);
		DragItemAgent.Destory(byId);
	}

	private void RPC_S2C_PreGetItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		NetworkInterface networkInterface = NetworkInterface.Get(num);
		if (null == networkInterface || Singleton<ForceSetting>.Instance.AllyForce(networkInterface.TeamId, base.TeamId))
		{
			return;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(num);
		if (null == peEntity)
		{
			return;
		}
		int playerID = (int)peEntity.GetAttribute(AttribType.DefaultPlayerID);
		List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(base._pos, 64f, playerID, isDeath: false, entity);
		for (int i = 0; i < entities.Count; i++)
		{
			if (!entities[i].Equals(entity) && entities[i].target != null)
			{
				entities[i].target.TransferHatred(entity, 5f);
			}
		}
	}

	private void RPC_S2C_GetLootItemBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int lootItemid = stream.Read<int>(new object[0]);
		PeSingleton<LootItemMgr>.Instance.NetFetch(lootItemid, base.Id);
	}

	private void RPC_S2C_NewItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemSample[] array = stream.Read<ItemSample[]>(new object[0]);
		if (array == null || array.Length <= 0)
		{
			return;
		}
		ItemSample[] array2 = array;
		foreach (ItemSample itemSample in array2)
		{
			if (itemSample != null)
			{
				if (null != MissionManager.Instance)
				{
					MissionManager.Instance.ProcessCollectMissionByID(itemSample.protoId);
				}
				ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(itemSample.protoId);
				if (itemProto != null)
				{
					string content = itemProto.GetName() + " X " + itemSample.stackCount;
					new PeTipMsg(content, itemProto.icon[0], PeTipMsg.EMsgLevel.Norm);
				}
			}
		}
	}

	private void RPC_S2C_DeleteItemInPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(value);
		if (itemObject != null)
		{
			PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
			if (null != cmpt)
			{
				cmpt.Remove(itemObject);
			}
			if (base.IsOwner)
			{
				GameUI.Instance.mItemPackageCtrl.ResetItem();
			}
		}
	}

	private void RPC_S2C_UseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			UseItemCmpt cmpt = entity.GetCmpt<UseItemCmpt>();
			if (cmpt != null)
			{
				cmpt.UseFromNet(itemObject);
			}
		}
	}

	private void RPC_S2C_SplitItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int slotIndex = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
			if (!(null == cmpt))
			{
				cmpt.package.PutItem(itemObject, slotIndex, (ItemPackage.ESlotType)itemObject.protoData.tabIndex);
			}
		}
	}

	private void RPC_S2C_ExchangeItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int slotIndex = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int slotIndex2 = stream.Read<int>(new object[0]);
		PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
		if (null == cmpt)
		{
			return;
		}
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject == null)
		{
			return;
		}
		if (num == -1)
		{
			cmpt.Remove(itemObject);
		}
		else
		{
			ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.Get(num);
			if (itemObject2 != null)
			{
				cmpt.package.PutItem(itemObject2, slotIndex2, (ItemPackage.ESlotType)itemObject.protoData.tabIndex);
			}
		}
		cmpt.package.PutItem(itemObject, slotIndex, (ItemPackage.ESlotType)itemObject.protoData.tabIndex);
	}

	[Obsolete]
	private void RPC_S2C_PutItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Vector3 scl = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			Drag cmpt = itemObject.GetCmpt<Drag>();
			if (cmpt != null)
			{
				DragArticleAgent.Create(cmpt, pos, scl, quaternion, id);
			}
		}
	}

	private void RPC_S2C_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		Quaternion rotation = stream.Read<Quaternion>(new object[0]);
		DragItemAgent byId = DragItemAgent.GetById(id);
		byId.rotation = rotation;
		ItemObject itemObj = byId.itemDrag.itemObj;
		if (itemObj != null)
		{
		}
	}

	private void RPC_S2C_RemoveItemFromPackage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			PlayerPackageCmpt cmpt = entity.GetCmpt<PlayerPackageCmpt>();
			if (null != cmpt)
			{
				cmpt.Remove(itemObject);
			}
		}
		if (base.IsOwner)
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}

	private void RPC_S2C_PublicStorageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int type = stream.Read<int>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		SlotList slotList = GroupNetwork.GetSlotList((ItemPackage.ESlotType)type);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == -1)
			{
				slotList[i] = null;
				continue;
			}
			ItemObject value = PeSingleton<ItemMgr>.Instance.Get(array[i]);
			slotList[i] = value;
		}
	}

	private void RPC_S2C_PublicStorageStore(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<string>(new object[0]);
		stream.Read<int>(new object[0]);
	}

	private void RPC_S2C_PublicStorageFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<string>(new object[0]);
		stream.Read<int>(new object[0]);
	}

	private void RPC_S2C_PublicStorageDelete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<string>(new object[0]);
		stream.Read<int>(new object[0]);
	}

	private void RPC_S2C_PublicStorageSplit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<string>(new object[0]);
		stream.Read<int>(new object[0]);
	}

	private void RPC_S2C_PersonalStorageIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		NpcStorage storage = NpcStorageMgr.GetStorage(base.Id);
		if (storage != null)
		{
			storage.Package.Clear((ItemPackage.ESlotType)num);
			SlotList slotList = storage.Package.GetSlotList((ItemPackage.ESlotType)num);
			for (int i = 0; i < array.Length; i++)
			{
				ItemObject value = PeSingleton<ItemMgr>.Instance.Get(array[i]);
				slotList[i] = value;
			}
			storage.Reset();
		}
	}

	private void RPC_S2C_PersonalStorageStore(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int codedIndex = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			NpcStorage storage = NpcStorageMgr.GetStorage(base.Id);
			if (storage != null)
			{
				storage.Package.PutItem(itemObject, codedIndex);
				storage.Reset();
			}
		}
	}

	private void RPC_S2C_PersonalStorageDelete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			NpcStorage storage = NpcStorageMgr.GetStorage(base.Id);
			if (storage != null)
			{
				storage.Package.RemoveItem(itemObject);
				storage.Reset();
			}
		}
	}

	private void RPC_S2C_PersonalStorageFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			NpcStorage storage = NpcStorageMgr.GetStorage(base.Id);
			if (storage != null)
			{
				storage.Package.RemoveItem(itemObject);
				storage.Reset();
			}
		}
	}

	private void RPC_S2C_PersonalStorageSplit(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int codedIndex = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject != null)
		{
			NpcStorage storage = NpcStorageMgr.GetStorage(base.Id);
			if (storage != null)
			{
				storage.Package.PutItem(itemObject, codedIndex);
				storage.Reset();
			}
		}
	}

	private void RPC_S2C_PersonalStorageExchange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int codedIndex = stream.Read<int>(new object[0]);
		int id2 = stream.Read<int>(new object[0]);
		int codedIndex2 = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject == null)
		{
			return;
		}
		NpcStorage storage = NpcStorageMgr.GetStorage(base.Id);
		if (storage != null)
		{
			ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.Get(id2);
			if (itemObject2 != null)
			{
				storage.Package.PutItem(itemObject2, codedIndex2);
			}
			else
			{
				storage.Package.RemoveItem(itemObject);
			}
			storage.Package.PutItem(itemObject, codedIndex);
			storage.Reset();
		}
	}

	private void RPC_S2C_PersonalStorageSort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		NpcStorage storage = NpcStorageMgr.GetStorage(base.Id);
		if (storage != null)
		{
			storage.Package.Clear((ItemPackage.ESlotType)num);
			SlotList slotList = storage.Package.GetSlotList((ItemPackage.ESlotType)num);
			for (int i = 0; i < array.Length; i++)
			{
				ItemObject value = PeSingleton<ItemMgr>.Instance.Get(array[i]);
				slotList[i] = value;
			}
			storage.Reset();
		}
	}

	public static void RPC_S2C_BuildBuildingBlock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	public static void RPC_S2C_SyncRailwayData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		if (array.Length > 0)
		{
			PeSingleton<Manager>.Instance.Import(array);
		}
	}

	public void RequestMakeMask(byte index, Vector3 pos, int iconId, string desc)
	{
		RPCServer(EPacketType.PT_InGame_MakeMask, index, pos, iconId, desc);
	}

	public void RequestRemoveMask(byte index)
	{
		RPCServer(EPacketType.PT_InGame_RemoveMask, index);
	}

	private void RPC_S2C_MakeMask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte index = stream.Read<byte>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int icon = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		UserLabel userLabel2 = (UserLabel)PeSingleton<LabelMgr>.Instance.Find(delegate(ILabel iter)
		{
			if (iter is UserLabel)
			{
				UserLabel userLabel = (UserLabel)iter;
				return userLabel.playerID == base.Id && userLabel.index == index;
			}
			return false;
		});
		if (userLabel2 != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(userLabel2);
		}
		userLabel2 = new UserLabel();
		userLabel2.pos = pos;
		userLabel2.icon = icon;
		userLabel2.text = text;
		userLabel2.index = index;
		userLabel2.playerID = base.Id;
		PeSingleton<LabelMgr>.Instance.Add(userLabel2);
	}

	private void RPC_S2C_RemoveMask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte index = stream.Read<byte>(new object[0]);
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		UserLabel userLabel2 = (UserLabel)PeSingleton<LabelMgr>.Instance.Find(delegate(ILabel iter)
		{
			if (iter is UserLabel)
			{
				UserLabel userLabel = (UserLabel)iter;
				return userLabel.playerID == base.Id && userLabel.index == index;
			}
			return false;
		});
		if (userLabel2 != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(userLabel2);
		}
	}

	private void RPC_S2C_TownAreaList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] array = stream.Read<Vector3[]>(new object[0]);
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		Vector3[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Vector3 vector = array2[i];
			IntVector2 key = new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.z));
			if (!VArtifactTownManager.Instance.TownPosInfo.ContainsKey(key))
			{
				continue;
			}
			VArtifactTown vArtifactTown = VArtifactTownManager.Instance.TownPosInfo[key];
			if (VArtifactTownManager.Instance.IsCaptured(vArtifactTown.townId))
			{
				RandomMapIconMgr.AddDestroyedTownIcon(vArtifactTown);
			}
			else
			{
				RandomMapIconMgr.AddTownIcon(vArtifactTown);
				PeSingleton<DetectedTownMgr>.Instance.AddDetectedTown(vArtifactTown);
			}
			foreach (VArtifactUnit vAUnit in vArtifactTown.VAUnits)
			{
				vAUnit.isDoodadNpcRendered = true;
			}
			vArtifactTown.IsExplored = true;
			VArtifactTownManager.Instance.DetectTowns(vArtifactTown);
		}
	}

	private void RPC_S2C_CampAreaList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] array = stream.Read<Vector3[]>(new object[0]);
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		Vector3[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Vector3 vector = array2[i];
			IntVector2 key = new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.z));
			if (!VArtifactTownManager.Instance.TownPosInfo.ContainsKey(key))
			{
				continue;
			}
			VArtifactTown vArtifactTown = VArtifactTownManager.Instance.TownPosInfo[key];
			if (VArtifactTownManager.Instance.IsCaptured(vArtifactTown.townId))
			{
				RandomMapIconMgr.AddDestroyedTownIcon(vArtifactTown);
			}
			else
			{
				RandomMapIconMgr.AddNativeIcon(vArtifactTown);
			}
			foreach (VArtifactUnit vAUnit in vArtifactTown.VAUnits)
			{
				vAUnit.isDoodadNpcRendered = true;
			}
			vArtifactTown.IsExplored = true;
			VArtifactTownManager.Instance.DetectTowns(vArtifactTown);
		}
	}

	private void RPC_S2C_MaskAreaList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buff = stream.Read<byte[]>(new object[0]);
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			for (int i = 0; i < num; i++)
			{
				byte index = BufferHelper.ReadByte(r);
				int icon = BufferHelper.ReadInt32(r);
				BufferHelper.ReadVector3(r, out var _value);
				string text = BufferHelper.ReadString(r);
				UserLabel item = new UserLabel
				{
					pos = _value,
					icon = icon,
					text = text,
					index = index,
					playerID = base.Id
				};
				PeSingleton<LabelMgr>.Instance.Add(item);
			}
		});
	}

	private void RPC_S2C_AddTownArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (LogFilter.logDebug)
		{
			Debug.LogFormat("<color=blue>Add town pos:{0}</color>", vector);
		}
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		IntVector2 key = new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.z));
		if (!VArtifactTownManager.Instance.TownPosInfo.ContainsKey(key))
		{
			return;
		}
		VArtifactTown vArtifactTown = VArtifactTownManager.Instance.TownPosInfo[key];
		PeSingleton<DetectedTownMgr>.Instance.AddDetectedTown(vArtifactTown);
		RandomMapIconMgr.AddTownIcon(vArtifactTown);
		foreach (VArtifactUnit vAUnit in vArtifactTown.VAUnits)
		{
			vAUnit.isDoodadNpcRendered = true;
		}
		vArtifactTown.IsExplored = true;
		VArtifactTownManager.Instance.DetectTowns(vArtifactTown);
	}

	private void RPC_S2C_AddCampArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		IntVector2 key = new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.z));
		if (!VArtifactTownManager.Instance.TownPosInfo.ContainsKey(key))
		{
			return;
		}
		VArtifactTown vArtifactTown = VArtifactTownManager.Instance.TownPosInfo[key];
		RandomMapIconMgr.AddNativeIcon(vArtifactTown);
		foreach (VArtifactUnit vAUnit in vArtifactTown.VAUnits)
		{
			vAUnit.isDoodadNpcRendered = true;
		}
		vArtifactTown.IsExplored = true;
		VArtifactTownManager.Instance.DetectTowns(vArtifactTown);
	}

	private void RPC_S2C_ExploredArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		if (!Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			Vector2 centerPos = PeSingleton<MaskTile.Mgr>.Instance.GetCenterPos(index);
			byte type = PeSingleton<MaskTile.Mgr>.Instance.GetType((int)centerPos.x, (int)centerPos.y);
			MaskTile maskTile = PeSingleton<MaskTile.Mgr>.Instance.Get(index);
			if (maskTile == null)
			{
				maskTile = new MaskTile();
				maskTile.index = index;
				maskTile.forceGroup = -1;
				maskTile.type = type;
				PeSingleton<MaskTile.Mgr>.Instance.Add(index, maskTile);
			}
		}
	}

	private void RPC_S2C_ExploredAreas(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		if (Singleton<ForceSetting>.Instance.Conflict(base.Id, mainPlayerId))
		{
			return;
		}
		int[] array2 = array;
		foreach (int index in array2)
		{
			Vector2 centerPos = PeSingleton<MaskTile.Mgr>.Instance.GetCenterPos(index);
			byte type = PeSingleton<MaskTile.Mgr>.Instance.GetType((int)centerPos.x, (int)centerPos.y);
			MaskTile maskTile = PeSingleton<MaskTile.Mgr>.Instance.Get(index);
			if (maskTile == null)
			{
				maskTile = new MaskTile();
				maskTile.index = index;
				maskTile.forceGroup = -1;
				maskTile.type = type;
				PeSingleton<MaskTile.Mgr>.Instance.Add(index, maskTile);
			}
		}
	}

	private bool StroyNpcInitCheck()
	{
		List<AiAdNpcNetwork> list = NetworkInterface.Get<AiAdNpcNetwork>();
		if (list == null || list.Count == 0)
		{
			return false;
		}
		foreach (AiAdNpcNetwork item in list)
		{
			if (item == null || item.npcCmpt == null || !item._npcMissionInited)
			{
				return false;
			}
		}
		return true;
	}

	private void AdventureInitStart()
	{
		if (PeGameMgr.IsAdventure && !PeGameMgr.IsVS && !MissionManager.Instance.m_PlayerMission.HasMission(9027) && !MissionManager.Instance.m_PlayerMission.HadCompleteMission(9027))
		{
			MissionManager.Instance.StartCoroutine(AdventureInit());
		}
	}

	private IEnumerator AdventureInit()
	{
		while (VArtifactTownManager.Instance == null)
		{
			yield return 0;
		}
		int missionStartNpcEntityId = 1;
		while (!PeSingleton<EntityMgr>.Instance.Get(missionStartNpcEntityId))
		{
			yield return 0;
		}
		PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(missionStartNpcEntityId);
		GameUI.Instance.mNpcWnd.m_CurSelNpc = npc;
		GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(9027, 1);
		GameUI.Instance.mNPCTalk.PreShow();
		MissionManager.Instance.m_PlayerMission.UpdateAllNpcMisTex();
	}

	private IEnumerator WaitForMissionModule(byte[] pmData, byte[] tmData)
	{
		while (null == MissionManager.Instance)
		{
			yield return null;
		}
		bool hasData = false;
		if (base.IsOwner)
		{
			hasData = MissionManager.Instance.m_PlayerMission.ImportNetwork(pmData);
			hasData |= MissionManager.Instance.m_PlayerMission.ImportNetwork(tmData);
		}
		else
		{
			hasData = MissionManager.Instance.m_PlayerMission.ImportNetwork(pmData, 1);
			hasData |= MissionManager.Instance.m_PlayerMission.ImportNetwork(tmData, 1);
		}
		while (null == mainPlayer || !StroyNpcInitCheck() || PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			yield return null;
		}
		if (hasData)
		{
			MissionManager.Instance.InitPlayerMission();
			ExcuCacheCommands();
			if (PeGameMgr.IsMultiStory)
			{
				StroyManager.Instance.InitMission();
				GameUI.Instance.mUIMissionWndCtrl.ReGetAllMission();
			}
		}
		else
		{
			MissionManager.Instance.m_bHadInitMission = true;
		}
		ExcuCacheCommands();
		_missionInited = true;
		if (PeGameMgr.IsAdventure && !PeGameMgr.IsVS)
		{
			AdventureInitStart();
		}
	}

	public static IEnumerator RequestKillMonster(int missionId, int targetId)
	{
		while (null == mainPlayer)
		{
			yield return null;
		}
		mainPlayer.RPCServer(EPacketType.PT_InGame_MissionKillMonster, missionId, targetId);
	}

	public static IEnumerator RequestTowerDefense(int missionId, int targetId)
	{
		while (null == mainPlayer)
		{
			yield return null;
		}
		Vector3 mPos = AiTowerDefense.GetTdGenPos(targetId);
		mainPlayer.RPCServer(EPacketType.PT_InGame_MissionTowerDefense, missionId, targetId, mPos);
	}

	private void ExcuCacheCommands()
	{
		foreach (string item in _commandCache)
		{
			string[] array = item.Split('@');
			if (array[0] == "CreateMission")
			{
				CreateMission(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), Convert.ToInt32(array[3]));
			}
			else if (array[0] == "AccessMission")
			{
				if (base.IsOwner)
				{
					AccessMission(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]));
				}
				else
				{
					AccessMission(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), Convert.ToBoolean(array[3]), Encoding.Default.GetBytes(array[4]));
				}
			}
			else if (array[0] == "DeleteMission")
			{
				DeleteMission(Convert.ToInt32(array[1]));
			}
			else if (array[0] == "ModifyMissionFlag")
			{
				ModifyMissionFlag(Convert.ToInt32(array[1]), array[2], array[3]);
			}
			else if (array[0] == "CompleteTarget")
			{
				CompleteTarget(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), Convert.ToInt32(array[3]));
			}
			else if (array[0] == "ReplyCompleteMission")
			{
				ReplyCompleteMission(Convert.ToInt32(array[1]), Convert.ToInt32(array[2]), Convert.ToBoolean(array[3]));
			}
			else if (array[0] == "FailMission")
			{
				FailMission(Convert.ToInt32(array[1]));
			}
		}
		_commandCache.Clear();
	}

	private void CreateMission(int nMissionID, int idx, int rewardIdx)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(nMissionID);
		if (missionCommonData != null)
		{
			if (PeGameMgr.IsMultiStory)
			{
				RMRepository.CreateRandomMission(nMissionID, idx, rewardIdx);
			}
			else
			{
				AdRMRepository.CreateRandomMission(nMissionID, idx, rewardIdx);
			}
		}
	}

	private void AccessMission(int nMissionID, int nNpcID, bool bCheck = true, byte[] adrmData = null)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(nNpcID);
		if (base.IsOwner)
		{
			MissionManager.Instance.ProcessSingleMode(nMissionID, peEntity, MissionManager.TakeMissionType.TakeMissionType_Get, bCheck);
		}
		else
		{
			if (PeGameMgr.IsMultiStory)
			{
				RMRepository.Import(adrmData);
			}
			else
			{
				AdRMRepository.Import(adrmData);
			}
			AiAdNpcNetwork adNpc = NetworkInterface.Get<AiAdNpcNetwork>(nNpcID);
			MissionManager.Instance.ProcessSingleMode(nMissionID, peEntity, MissionManager.TakeMissionType.TakeMissionType_Get, bCheck, adNpc);
		}
		if (null != peEntity)
		{
			peEntity.SetAttribute(AttribType.DefaultPlayerID, base.Id, offEvent: false);
		}
	}

	private void DeleteMission(int missionid)
	{
		MissionManager.Instance.m_PlayerMission.AbortMission(missionid);
	}

	private void UpdateMissionMapLabelPos(int missionId, int targetId, Vector3 pos)
	{
		UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(missionId);
		if (missionView != null)
		{
			UIMissionMgr.TargetShow tarshow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, targetId));
			if (tarshow != null && PeSingleton<LabelMgr>.Instance.Find(delegate(ILabel item)
			{
				if (item is MissionLabel)
				{
					MissionLabel missionLabel = item as MissionLabel;
					if (missionLabel.m_missionID == missionId && missionLabel.m_type == MissionLabelType.misLb_target && missionLabel.m_target == tarshow)
					{
						return true;
					}
				}
				return false;
			}) is MissionLabel missionLabel2)
			{
				missionLabel2.SetLabelPos(pos, needOneRefreshPos: true);
			}
		}
	}

	private void ModifyMissionFlag(int missionid, string missionflag, string missionvalue)
	{
		MissionManager.Instance.ModifyQuestVariable(missionid, missionflag, missionvalue);
	}

	private void CompleteTarget(int targetid, int missionid, int playerId)
	{
		MissionManager.Instance.CompleteTarget(targetid, missionid, forceComplete: false, bFromNet: true);
	}

	private void ReplyCompleteMission(int nMissionID, int nTargetID, bool bCheck)
	{
		MissionManager.Instance.CompleteMission(nMissionID, nTargetID, bCheck);
	}

	private void FailMission(int nMissionID)
	{
		MissionManager.Instance.FailureMission(nMissionID);
	}

	private void Mission953(int nMissionID, int itemId)
	{
		if (nMissionID != 953)
		{
			return;
		}
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemId);
		if (itemObject == null)
		{
			return;
		}
		int num = 0;
		num = ((itemObject.protoId <= 100000000) ? itemObject.protoId : StroyManager.Instance.ItemClassIdtoProtoId(itemObject.protoData.itemClassId));
		if (nMissionID != 953 || MissionManager.Instance.m_PlayerMission.IsSpecialID(num) != ECreation.SimpleObject)
		{
			return;
		}
		CreationData creation = CreationMgr.GetCreation(itemObject.instanceId);
		if (creation == null)
		{
			return;
		}
		int num2 = 0;
		foreach (int value in creation.m_Attribute.m_Cost.Values)
		{
			num2 += value;
		}
		if (num2 <= 300)
		{
			StroyManager.Instance.GetMissionOrPlotById(10954);
		}
		else
		{
			StroyManager.Instance.GetMissionOrPlotById(10955);
		}
	}

	private void RPC_S2C_CreateMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nMissionID = stream.Read<int>(new object[0]);
		int idx = stream.Read<int>(new object[0]);
		int rewardIdx = stream.Read<int>(new object[0]);
		if (MissionManager.Instance.m_bHadInitMission)
		{
			CreateMission(nMissionID, idx, rewardIdx);
			return;
		}
		string item = "CreateMission@" + nMissionID + "@" + idx + "@" + rewardIdx + "@";
		mainPlayer._commandCache.Add(item);
	}

	private void RPC_S2C_AccessMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nMissionID = stream.Read<int>(new object[0]);
		int nNpcID = stream.Read<int>(new object[0]);
		bool bCheck = stream.Read<bool>(new object[0]);
		if (MissionManager.Instance.m_bHadInitMission)
		{
			if (!base.IsOwner)
			{
				byte[] adrmData = stream.Read<byte[]>(new object[0]);
				AccessMission(nMissionID, nNpcID, bCheck, adrmData);
			}
			else
			{
				AccessMission(nMissionID, nNpcID, bCheck);
			}
		}
		else if (!base.IsOwner)
		{
			byte[] bytes = stream.Read<byte[]>(new object[0]);
			string item = "AccessMission@" + nMissionID + "@" + nNpcID + "@" + Encoding.Default.GetString(bytes) + "@";
			mainPlayer._commandCache.Add(item);
		}
		else
		{
			string item2 = "AccessMission@" + nMissionID + "@" + nNpcID + "@";
			mainPlayer._commandCache.Add(item2);
		}
	}

	private void RPC_S2C_DeleteMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		if (MissionManager.Instance.m_bHadInitMission)
		{
			DeleteMission(value);
			return;
		}
		string item = "DeleteMission@" + value + "@";
		mainPlayer._commandCache.Add(item);
	}

	private void RPC_S2C_ModifyMissionFlag(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<string>(out var value2);
		stream.TryRead<string>(out var value3);
		if (MissionManager.Instance.m_bHadInitMission)
		{
			ModifyMissionFlag(value, value2, value3);
			return;
		}
		string item = "ModifyMissionFlag@" + value + "@" + value2 + "@" + value3 + "@";
		mainPlayer._commandCache.Add(item);
	}

	private void RPC_S2C_CompleteTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<int>(out var value2);
		stream.TryRead<int>(out var value3);
		if (MissionManager.Instance.m_bHadInitMission)
		{
			CompleteTarget(value, value2, value3);
			return;
		}
		string item = "CompleteTarget@" + value + "@" + value2 + "@" + value3 + "@";
		mainPlayer._commandCache.Add(item);
	}

	private void RPC_S2C_ReplyCompleteMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nTargetID = stream.Read<int>(new object[0]);
		int nMissionID = stream.Read<int>(new object[0]);
		bool bCheck = stream.Read<bool>(new object[0]);
		if (MissionManager.Instance.m_bHadInitMission)
		{
			ReplyCompleteMission(nMissionID, nTargetID, bCheck);
			Mission953(nMissionID, _mission953Item);
			return;
		}
		string item = "ReplyCompleteMission@" + nMissionID + "@" + nTargetID + "@" + bCheck + "@";
		mainPlayer._commandCache.Add(item);
	}

	private void RPC_S2C_FailMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		if (MissionManager.Instance.m_bHadInitMission)
		{
			FailMission(value);
			return;
		}
		string item = "FailMission@" + value + "@";
		mainPlayer._commandCache.Add(item);
	}

	private void RPC_S2C_SyncMissions(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] pmData = stream.Read<byte[]>(new object[0]);
		byte[] array = stream.Read<byte[]>(new object[0]);
		byte[] tmData = stream.Read<byte[]>(new object[0]);
		byte[] array2 = stream.Read<byte[]>(new object[0]);
		if (PeGameMgr.IsMultiStory)
		{
			if (array != null)
			{
				RMRepository.Import(array);
			}
			if (array2 != null)
			{
				RMRepository.Import(array2);
			}
		}
		else
		{
			if (array != null)
			{
				AdRMRepository.Import(array);
			}
			if (array2 != null)
			{
				AdRMRepository.Import(array2);
			}
		}
		StartCoroutine(WaitForMissionModule(pmData, tmData));
	}

	private void RPC_S2C_CreateKillMonsterPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<Vector3>(out var _);
		stream.TryRead<float>(out var _);
		stream.TryRead<int[]>(out var value3);
		stream.TryRead<int[]>(out var value4);
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < value4.Length; i++)
		{
			for (int j = 0; j < value4[i]; j++)
			{
			}
		}
		Vector3[] array = list.ToArray();
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_MissionMonsterPos, array, value3, value4);
		}
	}

	private void RPC_S2C_CreateFollowPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<float>(out var value);
		stream.TryRead<float>(out var value2);
		stream.TryRead<int>(out var value3);
		stream.TryRead<int>(out var value4);
		Vector3 vector = new Vector3(value, VFDataRTGen.GetPosTop(new IntVector2((int)value, (int)value2)), value2);
		if (null != mainPlayer)
		{
			mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFollowPos, vector, value3);
		}
		TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(value3);
		if (typeFollowData == null)
		{
			return;
		}
		typeFollowData.m_DistPos = vector;
		typeFollowData.m_DistRadius = typeFollowData.m_AdDistPos.radius2;
		if (typeFollowData.m_AdNpcRadius.num > 0)
		{
			typeFollowData.m_LookNameID = StroyManager.Instance.CreateMissionRandomNpc(typeFollowData.m_DistPos, typeFollowData.m_AdNpcRadius.num);
		}
		if (base.IsOwner)
		{
			if (typeFollowData.m_AdDistPos.refertoType == ReferToType.Transcript)
			{
				RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)typeFollowData.m_DistPos.x, (int)typeFollowData.m_DistPos.z), typeFollowData.m_AdDistPos.referToID);
			}
			for (int i = 0; i < typeFollowData.m_CreateNpcList.Count; i++)
			{
				Vector3 patrolPoint = StroyManager.Instance.GetPatrolPoint(typeFollowData.m_DistPos, 3, 8, bCheck: false);
				EntityCreateMgr.Instance.CreateRandomNpc(typeFollowData.m_CreateNpcList[i], patrolPoint);
			}
			MissionManager.Instance.m_PlayerMission.ProcessFollowMission(value4, value3);
		}
		UpdateMissionMapLabelPos(value4, value3, vector);
	}

	private void RPC_S2C_CreateDiscoveryPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float x = stream.Read<float>(new object[0]);
		float z = stream.Read<float>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int missionId = stream.Read<int>(new object[0]);
		TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(num);
		if (typeSearchData == null)
		{
			return;
		}
		typeSearchData.m_DistPos = new Vector3(x, VFDataRTGen.GetPosHeight(x, z), z);
		typeSearchData.m_DistRadius = typeSearchData.m_mr.radius2;
		if (base.IsOwner)
		{
			if (typeSearchData.m_mr.refertoType == ReferToType.Transcript)
			{
				RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)typeSearchData.m_DistPos.x, (int)typeSearchData.m_DistPos.z), typeSearchData.m_mr.referToID);
			}
			if (MissionManager.Instance.m_bHadInitMission)
			{
				for (int i = 0; i < typeSearchData.m_CreateNpcList.Count; i++)
				{
					Vector3 patrolPoint = StroyManager.Instance.GetPatrolPoint(typeSearchData.m_DistPos, 3, 8, bCheck: false);
					EntityCreateMgr.Instance.CreateRandomNpc(typeSearchData.m_CreateNpcList[i], patrolPoint);
				}
			}
			if (null != mainPlayer)
			{
				mainPlayer.RPCServer(EPacketType.PT_InGame_MissionDiscoveryPos, typeSearchData.m_DistPos, num);
			}
		}
		UpdateMissionMapLabelPos(missionId, num, typeSearchData.m_DistPos);
	}

	private void RPC_S2C_SyncUseItemPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int missionId = stream.Read<int>(new object[0]);
		TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(num);
		if (typeUseItemData != null)
		{
			typeUseItemData.m_Pos = pos;
			typeUseItemData.m_Radius = typeUseItemData.m_AdDistPos.radius2;
			UpdateMissionMapLabelPos(missionId, num, pos);
		}
	}

	private void RPC_S2C_AddNpcToColony(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		int teamNum = stream.Read<int>(new object[0]);
		int dwellingId = stream.Read<int>(new object[0]);
		MultiColonyManager.Instance.AddNpcToColony(id, teamNum, dwellingId);
	}

	private void RPC_S2C_MissionKillMonster(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int missionId = stream.Read<int>(new object[0]);
		int targetId = stream.Read<int>(new object[0]);
		SceneEntityCreator.self.AddMissionPoint(missionId, targetId);
	}

	private void RPC_S2C_SetMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int mission = stream.Read<int>(new object[0]);
		MissionManager.Instance.m_PlayerMission.SetMission(mission);
	}

	private void RPC_S2C_SetCollectItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int targetID = stream.Read<int>(new object[0]);
		int itemID = stream.Read<int>(new object[0]);
		int itemNum = stream.Read<int>(new object[0]);
		MissionManager.GetTypeCollectData(targetID)?.muiSetItemActive(itemID, itemNum);
	}

	private void RPC_S2C_EntityReach(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = num % 10000;
		if (mainPlayerId != num2 && !_storyPlot.Contains(num3) && (num3 == 446 || num3 == 449 || num3 == 476 || num3 == 477 || num3 == 479 || num3 == 480 || num3 == 416))
		{
			StroyManager.Instance.GetMissionOrPlotById(num);
		}
	}

	private void RPC_S2C_RequestAdMissionData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		int level = stream.Read<int>(new object[0]);
		RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)vector.x, (int)vector.z), level);
	}

	private void RPC_S2C_Mission953(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_mission953Item = stream.Read<int>(new object[0]);
	}

	private void RPC_S2C_LanguegeSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		MissionManager.Instance.m_PlayerMission.LanguegeSkill = stream.Read<int>(new object[0]);
	}

	private void RPC_S2C_MonsterBook(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<bool>(new object[0]))
		{
			MonsterHandbookData.Deserialize(stream.Read<byte[]>(new object[0]));
		}
		else
		{
			MonsterHandbookData.AddMhByKilledMonsterID(stream.Read<int>(new object[0]));
		}
	}

	public void RequestEnterDungeon(Vector3 enterPos)
	{
		RPCServer(EPacketType.PT_InGame_EnterDungeon, enterPos);
	}

	public void RequestExitDungeon()
	{
		RPCServer(EPacketType.PT_InGame_ExitDungeon);
	}

	public void RequestUploadDungeonSeed(Vector3 entrancePos, int seed)
	{
		RPCServer(EPacketType.PT_InGame_UploadDungeonSeed, entrancePos, seed);
	}

	public void RequestGenDunEntrance(Vector3 entrancePos, int dungeonId)
	{
		RPCServer(EPacketType.PT_InGame_GenDunEntrance, entrancePos, dungeonId);
	}

	private void RPC_S2C_EnterDungeon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!stream.Read<bool>(new object[0]))
		{
			MessageBox_N.CancelMask(MsgInfoType.DungeonGeneratingMask);
			return;
		}
		Vector3 posByGenPlayerPos = stream.Read<Vector3>(new object[0]);
		int seed = stream.Read<int>(new object[0]);
		int dungeonId = stream.Read<int>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		RandomDungenMgr.Instance.LoadDataFromId(id);
		RandomDungenMgrData.DungeonId = dungeonId;
		RandomDungenMgrData.SetPosByGenPlayerPos(posByGenPlayerPos);
		int num = 0;
		while (!RandomDungenMgr.Instance.GenDungeon(seed))
		{
			num++;
			Debug.Log("generation failed: " + num);
		}
		RandomDungenMgr.Instance.LoadPathFinding();
		RequestFastTravel(0, RandomDungenMgrData.revivePos, 0);
	}

	private void RPC_S2C_ExitDungeon(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RequestFastTravel(0, RandomDungenMgrData.enterPos, 0);
	}

	private void RPC_S2C_GenDunEntrance(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 genPos = stream.Read<Vector3>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		DungeonBaseData dataFromId = RandomDungeonDataBase.GetDataFromId(id);
		RandomDungenMgr.Instance.GenDunEntrance(genPos, dataFromId);
	}

	private void RPC_S2C_GenDunEntranceList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<Vector3> list = stream.Read<Vector3[]>(new object[0]).ToList();
		List<int> list2 = stream.Read<int[]>(new object[0]).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			DungeonBaseData dataFromId = RandomDungeonDataBase.GetDataFromId(list2[i]);
			RandomDungenMgr.Instance.GenDunEntrance(list[i], dataFromId);
		}
	}

	private void RPC_S2C_InitWhenSpawn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (PeGameMgr.randomMap && !RandomDungenMgrData.InDungeon)
		{
			RandomMapConfig.SetGlobalFogHeight();
		}
	}

	private void RPC_S2C_GenRandomItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int[] itemIdNum = stream.Read<int[]>(new object[0]);
		RandomItemBoxInfo boxInfoById = RandomItemBoxInfo.GetBoxInfoById(num);
		if (boxInfoById != null)
		{
			RandomItemMgr.Instance.AddItmeResult(pos, quaternion, num, itemIdNum, boxInfoById.boxModelPath);
		}
	}

	private void RPC_S2C_GenRandomItemRare(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
	}

	private void RPC_S2C_RandomItemRareAry(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<Vector3> list = stream.Read<Vector3[]>(new object[0]).ToList();
		List<Quaternion> list2 = stream.Read<Quaternion[]>(new object[0]).ToList();
		List<int> list3 = stream.Read<int[]>(new object[0]).ToList();
		List<int> list4 = stream.Read<int[]>(new object[0]).ToList();
		List<int> list5 = stream.Read<int[]>(new object[0]).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Vector3 pos = list[i];
			Quaternion quaternion = list2[i];
			int num = list3[i];
			int num2 = list4[i];
			int[] array = new int[num2];
			Array.Copy(list5.ToArray(), array, num2);
			list5.RemoveRange(0, num2);
			RandomItemBoxInfo boxInfoById = RandomItemBoxInfo.GetBoxInfoById(num);
			if (boxInfoById != null)
			{
				RandomItemMgr.Instance.AddRareItmeResult(pos, quaternion, num, array, boxInfoById.boxModelPath);
			}
		}
	}

	private void RPC_S2C_GetRandomIsoCode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
	}

	private void RPC_S2C_RandomItemFetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		int protoId = stream.Read<int>(new object[0]);
		int count = stream.Read<int>(new object[0]);
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
		if (randomItemObj != null)
		{
			randomItemObj.TryFetch(index, protoId, count);
			if (GameUI.Instance.mItemGet != null)
			{
				GameUI.Instance.mItemGet.Reflash();
			}
		}
	}

	private void RPC_S2C_RandomItemFetchAll(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
		if (randomItemObj != null)
		{
			randomItemObj.TryFetchAll();
			if (GameUI.Instance.mItemGet != null)
			{
				GameUI.Instance.mItemGet.Reflash();
			}
		}
	}

	private void RPC_S2C_GenRandomFeces(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int[] itemIdNum = stream.Read<int[]>(new object[0]);
		RandomItemMgr.Instance.AddFecesResult(pos, quaternion, itemIdNum);
	}

	private void RPC_S2C_RandomItemClicked(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int[] items = stream.Read<int[]>(new object[0]);
		RandomItemMgr.Instance.GetRandomItemObj(pos)?.ClickedInMultiMode(items);
	}

	private void RPC_S2C_RandomItemDestroy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		RandomItemMgr.Instance.GetRandomItemObj(pos)?.DestroySelf();
	}

	private void RPC_S2C_RandomItemDestroyList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] array = stream.Read<Vector3[]>(new object[0]);
		Vector3[] array2 = array;
		foreach (Vector3 pos in array2)
		{
			RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(pos);
			if (randomItemObj == null)
			{
				break;
			}
			randomItemObj.DestroySelf();
		}
	}

	private void RPC_S2C_InitAdminData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>(new object[0]);
		ServerAdministrator.DeserializeAdminData(data);
	}

	private void RPC_S2C_AddBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int playerId = stream.Read<int>(new object[0]);
		ServerAdministrator.AddBlacklist(playerId);
	}

	private void RPC_S2C_DeleteBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ServerAdministrator.DeleteBlacklist(id);
	}

	private void RPC_S2C_ClearBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ServerAdministrator.ClearBlacklist();
	}

	private void RPC_S2C_AddAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int playerId = stream.Read<int>(new object[0]);
		ServerAdministrator.AddAssistant(playerId);
	}

	private void RPC_S2C_DeleteAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ServerAdministrator.DeleteAssistant(id);
	}

	private void RPC_S2C_ClearAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ServerAdministrator.ClearAssistant();
	}

	private void RPC_S2C_ClearVoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int key = stream.Read<int>(new object[0]);
		if (!ChunkManager.Instance._areaBlockList.ContainsKey(key))
		{
			return;
		}
		foreach (KeyValuePair<IntVector3, B45Block> item in ChunkManager.Instance._areaBlockList[key])
		{
			Block45Man.self.DataSource.SafeWrite(new B45Block(0, 0), item.Key.x, item.Key.y, item.Key.z);
		}
		ChunkManager.Instance._areaBlockList.Remove(key);
	}

	private void RPC_S2C_ClearAllVoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		foreach (KeyValuePair<int, Dictionary<IntVector3, B45Block>> areaBlock in ChunkManager.Instance._areaBlockList)
		{
			foreach (KeyValuePair<IntVector3, B45Block> item in areaBlock.Value)
			{
				Block45Man.self.DataSource.SafeWrite(new B45Block(0, 0), item.Key.x, item.Key.y, item.Key.z);
			}
		}
		ChunkManager.Instance._areaBlockList.Clear();
	}

	private void RPC_S2C_LockArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		ServerAdministrator.LockArea(index);
	}

	private void RPC_S2C_UnLockArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		ServerAdministrator.UnLockArea(index);
	}

	private void RPC_S2C_BuildLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int playerId = stream.Read<int>(new object[0]);
		ServerAdministrator.BuildLock(playerId);
	}

	private void RPC_S2C_BuildUnLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ServerAdministrator.BuildUnLock(id);
	}

	private void RPC_S2C_ClearBuildLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ServerAdministrator.ClearBuildLock();
	}

	private void RPC_S2C_BuildChunk(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool buildChunk = stream.Read<bool>(new object[0]);
		ServerAdministrator.SetBuildChunk(buildChunk);
	}

	private void RPC_S2C_JoinGame(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool joinGame = stream.Read<bool>(new object[0]);
		ServerAdministrator.SetJoinGame(joinGame);
	}

	public void RequestShopData(int npcId)
	{
		RPCServer(EPacketType.PT_InGame_InitShop, npcId);
	}

	public void RequestBuy(int npcId, int objId, int num)
	{
		RPCServer(EPacketType.PT_InGame_Buy, npcId, objId, num);
	}

	public void RequestRepurchase(int npcId, int objId, int num)
	{
		RPCServer(EPacketType.PT_InGame_Repurchase, npcId, objId, num);
	}

	public void RequestSell(int npcId, int objId, int num)
	{
		RPCServer(EPacketType.PT_InGame_Sell, npcId, objId, num);
	}

	private void RPC_S2C_InitNpcShop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcid = stream.Read<int>(new object[0]);
		int[] ids = stream.Read<int[]>(new object[0]);
		if (GameUI.Instance.mShopWnd.InitNpcShopWhenMultiMode(npcid, ids))
		{
			GameUI.Instance.mShopWnd.Show();
		}
	}

	private void RPC_S2C_ChangeCurrency(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		switch (stream.Read<int>(new object[0]))
		{
		case 2:
			if (Money.Digital)
			{
				break;
			}
			foreach (PeEntity item in PeSingleton<EntityMgr>.Instance.All)
			{
				if (!(item == null) && !(item.GetGameObject() == null))
				{
					NpcPackageCmpt cmpt = item.GetCmpt<NpcPackageCmpt>();
					if (!(cmpt == null))
					{
						cmpt.money.SetCur(cmpt.money.current * 4);
					}
				}
			}
			Money.Digital = true;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(value: false);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(value: true);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(value: true);
			break;
		case 1:
			if (Money.Digital)
			{
				Money.Digital = false;
				GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(value: true);
				GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(value: false);
				GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(value: false);
			}
			break;
		}
	}

	private void RPC_S2C_SyncRepurchaseItemIDs(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		GameUI.Instance.mShopWnd.RepurchaseList.Clear();
		int[] array2 = array;
		foreach (int objID in array2)
		{
			GameUI.Instance.mShopWnd.AddNewRepurchaseItem(objID);
		}
		GameUI.Instance.mShopWnd.ResetItem();
	}

	private void RPC_S2C_SkillCast(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<uLink.NetworkViewID>(out var value2);
		SkillRunner skillRunner = base.Runner as SkillRunner;
		if (null != skillRunner)
		{
			ISkillTarget target = null;
			uLink.NetworkView networkView = uLink.NetworkView.Find(value2);
			if (null != networkView)
			{
				NetworkInterface component = networkView.GetComponent<NetworkInterface>();
				target = ((!(null == component)) ? component.Runner : null);
			}
			skillRunner.RunEffOnProxy(value, target);
		}
	}

	private void RPC_S2C_SkillCastShoot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		stream.TryRead<Vector3>(out var value2);
		DefaultPosTarget target = new DefaultPosTarget(value2);
		SkillRunner skillRunner = base.Runner as SkillRunner;
		if (null != skillRunner)
		{
			skillRunner.RunEffOnProxy(value, target);
		}
	}

	private void RPC_S2C_MergeSkillList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		if (mainPlayer == null || mainPlayer.entity == null)
		{
			return;
		}
		ReplicatorCmpt cmpt = mainPlayer.entity.GetCmpt<ReplicatorCmpt>();
		if (null != cmpt)
		{
			int[] array2 = array;
			foreach (int formulaId in array2)
			{
				cmpt.replicator.AddFormula(formulaId);
			}
		}
	}

	private void RPC_S2C_MetalScanList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] metalID = stream.Read<int[]>(new object[0]);
		bool openWnd = stream.Read<bool>(new object[0]);
		MetalScanData.AddMetalScan(metalID, openWnd);
	}

	private void RPC_S2C_ReplicateSuccess(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_Plant_GetBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		FarmManager.Instance.RemovePlant(num);
		DragArticleAgent.Destory(num);
	}

	private void RPC_S2C_Plant_PutOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		Vector3 scl = stream.Read<Vector3>(new object[0]);
		int id = stream.Read<int>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		DragArticleAgent dragArticleAgent = DragArticleAgent.Create(itemObject.GetCmpt<Drag>(), pos, scl, quaternion, id);
		FarmPlantLogic farmPlantLogic = dragArticleAgent.itemLogic as FarmPlantLogic;
		farmPlantLogic.InitInMultiMode();
		stream.Read<FarmPlantLogic>(new object[0]);
		farmPlantLogic.UpdateInMultiMode();
	}

	private void RPC_S2C_Plant_VFTerrainTarget(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		stream.Read<int>(new object[0]);
		MemoryStream input = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(input);
		Dictionary<IntVector3, VFVoxel> dictionary = new Dictionary<IntVector3, VFVoxel>();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			IntVector3 intVector = new IntVector3(binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32());
			dictionary[intVector] = new VFVoxel(binaryReader.ReadByte(), binaryReader.ReadByte());
			VFVoxelTerrain.self.AlterVoxelInBuild(intVector.ToVector3(), dictionary[intVector]);
		}
	}

	[Obsolete]
	private void RPC_S2C_Plant_FarmInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		List<FarmPlantInitData> list = FarmManager.Instance.ImportPlantData(buffer);
		foreach (FarmPlantInitData item in list)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(item.mPlantInstanceId);
			DragArticleAgent dragArticleAgent = DragArticleAgent.Create(itemObject.GetCmpt<Drag>(), item.mPos, Vector3.one, item.mRot, item.mPlantInstanceId);
			FarmPlantLogic farmPlantLogic = dragArticleAgent.itemLogic as FarmPlantLogic;
			farmPlantLogic.InitDataFromPlant(item);
			FarmManager.Instance.AddPlant(farmPlantLogic);
			farmPlantLogic.UpdateInMultiMode();
		}
	}

	[Obsolete]
	private void RPC_S2C_Plant_Water(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjID = stream.Read<int>(new object[0]);
		double mWater = stream.Read<double>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
		if (plantByItemObjID != null)
		{
			plantByItemObjID.mWater = mWater;
			plantByItemObjID.UpdateInMultiMode();
		}
	}

	[Obsolete]
	private void RPC_S2C_Plant_Clean(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjID = stream.Read<int>(new object[0]);
		double mClean = stream.Read<double>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(itemObjID);
		if (plantByItemObjID != null)
		{
			plantByItemObjID.mClean = mClean;
			plantByItemObjID.UpdateInMultiMode();
		}
	}

	private void RPC_S2C_Plant_Clear(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		FarmPlantLogic plantByItemObjID = FarmManager.Instance.GetPlantByItemObjID(num);
		if (plantByItemObjID != null)
		{
			FarmManager.Instance.RemovePlant(num);
			DragArticleAgent.Destory(num);
		}
	}

	private void RPC_S2C_Railway_AddPoint(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int type = stream.Read<int>(new object[0]);
		int prePointId = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoAddPoint(pos, (Point.EType)type, prePointId);
	}

	private void RPC_S2C_Railway_PrePointChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointId = stream.Read<int>(new object[0]);
		int preID = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoChangePrePoint(pointId, preID);
	}

	private void RPC_S2C_Raileway_NextPointChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>(new object[0]);
		int nextID = stream.Read<int>(new object[0]);
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointID);
		if (point != null)
		{
			PeSingleton<RailwayOperate>.Instance.DoChangeNextPoint(point, nextID);
		}
	}

	private void RPC_S2C_Railway_Recycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoRemovePoint(pointID);
	}

	private void RPC_S2C_Railway_Route(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointId = stream.Read<int>(new object[0]);
		string routeName = stream.Read<string>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoCreateRoute(pointId, routeName);
	}

	private void RPC_S2C_Railway_GetOnTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int routeId = stream.Read<int>(new object[0]);
		int entityId = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoGetOnTrain(entityId, routeId, checkState: false);
	}

	private void RPC_S2C_Railway_GetOffTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int routeId = stream.Read<int>(new object[0]);
		int entityId = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoGetOffTrain(routeId, entityId, pos);
	}

	private void RPC_S2C_Railway_DeleteRoute(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int routeId = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoDeleteRoute(routeId);
	}

	private void RPC_S2C_Railway_SetRouteTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int routeId = stream.Read<int>(new object[0]);
		int trainItemObjId = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoSetRouteTrain(routeId, trainItemObjId);
	}

	private void RPC_S2C_Railway_ChangeStationRot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoChangePointRot(pointID, vector);
	}

	private void RPC_S2C_Railway_GetOffTrainEx(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int type = stream.Read<int>(new object[0]);
		int passengerID = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoRemovePassenger(type, passengerID);
	}

	private void RPC_S2C_Railway_ResetPointName(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoResetPointName(pointID, text);
	}

	private void RPC_S2C_Railway_ResetRouteName(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int routeID = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoResetRouteName(routeID, text);
	}

	private void RPC_S2C_Railway_ResetPointTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>(new object[0]);
		float time = stream.Read<float>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoResetPointTime(pointID, time);
	}

	private void RPC_S2C_Railway_AutoCreateRoute(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int pointID = stream.Read<int>(new object[0]);
		int itemObjID = stream.Read<int>(new object[0]);
		PeSingleton<RailwayOperate>.Instance.DoAutoCreateRoute(pointID, itemObjID);
	}

	public void SKTLearn(int skillType)
	{
		_learntSkills.SKTLearn(skillType);
	}

	private void RPC_S2C_InitLearntSkills(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(array[i]);
			if (skillUnit != null)
			{
				_learntSkills.AddSkillUnit(skillUnit);
			}
		}
		_learntSkills.InitDefaultSkill();
	}

	private void RPC_S2C_SKTLevelUp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int level = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (num2 != mainPlayerId && !(_learntSkills == null))
		{
			SkillTreeUnit skillTreeUnit = _learntSkills.FindSkillUnit(num);
			if (skillTreeUnit != null)
			{
				_learntSkills.RemoveSkillUnit(skillTreeUnit);
			}
			SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(num, level);
			if (skillUnit != null)
			{
				_learntSkills.AddSkillUnit(skillUnit);
			}
		}
	}
}
