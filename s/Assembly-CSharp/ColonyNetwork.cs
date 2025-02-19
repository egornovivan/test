using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomData;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class ColonyNetwork : AiObject
{
	internal ColonyBase runner;

	public ColonyCheck checkEntity => runner as ColonyCheck;

	public ColonyProcessing processingEntity => runner as ColonyProcessing;

	public ColonyTent tentEntity => runner as ColonyTent;

	public ColonyTrade tradeEntity => runner as ColonyTrade;

	public ColonyTrain trainEntity => runner as ColonyTrain;

	public ColonyTreat treatEntity => runner as ColonyTreat;

	public int ownerId { get; protected set; }

	private void RPC_C2S_InitDataAssembly(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSAssemblyData cSAssemblyData = (CSAssemblyData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataAssembly, cSAssemblyData.m_CurDeleteTime, cSAssemblyData.m_CurRepairTime, cSAssemblyData.m_DeleteTime, runner.Durability, cSAssemblyData.m_RepairTime, cSAssemblyData.m_RepairValue, cSAssemblyData.m_Level, cSAssemblyData.m_TimeTicks, cSAssemblyData.m_UpgradeTime, cSAssemblyData.m_CurUpgradeTime, cSAssemblyData.m_ShowShield);
	}

	private void RPC_C2S_ASB_LevelUp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null && !((ColonyAssembly)runner).LevelUp(player))
		{
			RPCOthers(EPacketType.PT_CL_ASB_LevelUpStart, -1f, -1f, player.roleName, false);
		}
	}

	private void RPC_C2S_ASB_QueryTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSAssemblyData cSAssemblyData = (CSAssemblyData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_ASB_QueryTime, cSAssemblyData.m_CurUpgradeTime);
	}

	private void RPC_C2S_ASB_HideShield(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSAssemblyData cSAssemblyData = (CSAssemblyData)runner._RecordData;
		cSAssemblyData.m_ShowShield = stream.Read<bool>(new object[0]);
		RPCOthers(EPacketType.PT_CL_ASB_HideShield, cSAssemblyData.m_ShowShield);
	}

	private void RPC_C2S_InitDataCheck(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSCheckData cSCheckData = (CSCheckData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataCheck, cSCheckData.m_CurDeleteTime, cSCheckData.m_CurRepairTime, cSCheckData.m_DeleteTime, runner.Durability, cSCheckData.m_RepairTime, cSCheckData.m_RepairValue, cSCheckData.npcIds.ToArray(), cSCheckData.m_CurTime, cSCheckData.m_Time, cSCheckData.isNpcReady, cSCheckData.occupied);
	}

	private void RPC_C2S_CHK_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(id);
		checkEntity.AddNpc(npcByID);
	}

	private void RPC_C2S_CHK_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(id);
		checkEntity.Start(npcByID);
	}

	private void RPC_C2S_CHK_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		checkEntity.RemoveDeadPatient(npcId);
	}

	private void RPC_C2S_InitDataDwellings(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSDwellingsData cSDwellingsData = (CSDwellingsData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataDwellings, cSDwellingsData.m_CurDeleteTime, cSDwellingsData.m_CurRepairTime, cSDwellingsData.m_DeleteTime, runner.Durability, cSDwellingsData.m_RepairTime, cSDwellingsData.m_RepairValue);
	}

	private void RPC_C2S_DWL_SyncNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] allNpcs = (runner as ColonyDwellings).GetAllNpcs();
		RPCPeer(info.sender, EPacketType.PT_CL_DWL_SyncNpc, allNpcs);
	}

	private void RPC_C2S_InitDataEnhance(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSEnhanceData cSEnhanceData = (CSEnhanceData)runner._RecordData;
		if (cSEnhanceData.m_ItemID > 0)
		{
			ChannelNetwork.SyncItem(info.sender, ItemManager.GetItemByID(cSEnhanceData.m_ItemID));
		}
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataEnhance, cSEnhanceData.m_CurDeleteTime, cSEnhanceData.m_CurRepairTime, cSEnhanceData.m_DeleteTime, runner.Durability, cSEnhanceData.m_RepairTime, cSEnhanceData.m_RepairValue, cSEnhanceData.m_ItemID, cSEnhanceData.m_CurTime, cSEnhanceData.m_Time);
	}

	private void RPC_C2S_EHN_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (!(player != null))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID != null)
		{
			if (((ColonyEnhance)runner).SetItem(num, player))
			{
				RPCOthers(EPacketType.PT_CL_EHN_SetItem, num, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_EHN_SetItem, num, false);
			}
		}
	}

	private void RPC_C2S_EHN_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyEnhance)runner).Start(player))
			{
				RPCOthers(EPacketType.PT_CL_EHN_Start, player.roleName, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_EHN_Start, player.roleName, false);
			}
		}
	}

	private void RPC_C2S_EHN_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (((ColonyEnhance)runner).Stop())
		{
			RPCOthers(EPacketType.PT_CL_EHN_Stop, true);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_CL_EHN_Stop, false);
		}
	}

	private void RPC_C2S_EHN_Fetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyEnhance)runner).FetchItem(player))
			{
				RPCOthers(EPacketType.PT_CL_EHN_Fetch, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_EHN_Fetch, false);
			}
		}
	}

	private void RPC_C2S_InitDataFactory(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFactoryData cSFactoryData = (CSFactoryData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataFactory, cSFactoryData.m_CurDeleteTime, cSFactoryData.m_CurRepairTime, cSFactoryData.m_DeleteTime, runner.Durability, cSFactoryData.m_RepairTime, cSFactoryData.m_RepairValue, cSFactoryData.m_CompoudItems.ToArray(), true);
	}

	private void RPC_C2S_FCT_Compoud(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int skillId = stream.Read<int>(new object[0]);
		int productCount = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			Replicator.Formula formula = ((ColonyFactory)runner).MultiMergeSkill(player, skillId, productCount);
			if (formula != null)
			{
				RPCOthers(EPacketType.PT_CL_FCT_Compoud, true, formula.productItemId);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_FCT_Compoud, false);
			}
		}
	}

	private void RPC_C2S_FCT_Fetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			int num = ((ColonyFactory)runner).Fetch(player, index);
			if (num != 0)
			{
				RPCOthers(EPacketType.PT_CL_FCT_Fetch, num, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_FCT_Fetch, num, false);
			}
		}
	}

	private void PRC_C2S_FCT_GenFactoryCancel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		CompoudItem ci = stream.Read<CompoudItem>(new object[0]);
		((ColonyFactory)runner).OnCancelCompound(index, ci);
	}

	private void RPC_C2S_InitDataFarm(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)runner._RecordData;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<ItemObject> list3 = new List<ItemObject>();
		foreach (KeyValuePair<int, int> plantSeed in cSFarmData.m_PlantSeeds)
		{
			list.Add(plantSeed.Key);
			list2.Add(plantSeed.Value);
			list3.Add(ItemManager.GetItemByID(plantSeed.Value));
		}
		List<int> list4 = new List<int>();
		List<int> list5 = new List<int>();
		foreach (KeyValuePair<int, int> tool in cSFarmData.m_Tools)
		{
			list4.Add(tool.Key);
			list5.Add(tool.Value);
			list3.Add(ItemManager.GetItemByID(tool.Value));
		}
		if (list3.Count > 0)
		{
			ChannelNetwork.SyncItemList(info.sender, list3.ToArray());
		}
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataFarm, cSFarmData.m_CurDeleteTime, cSFarmData.m_CurRepairTime, cSFarmData.m_DeleteTime, runner.Durability, cSFarmData.m_RepairTime, cSFarmData.m_RepairValue, list.ToArray(), list2.ToArray(), list4.ToArray(), list5.ToArray(), cSFarmData.m_AutoPlanting, cSFarmData.m_SequentialPlanting);
	}

	private void RPC_C2S_FARM_SetPlantSeed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = false;
		ItemObject itemByID = ItemManager.GetItemByID(num2);
		if (itemByID != null)
		{
			Player player = Player.GetPlayer(info.sender);
			if (player != null && ((ColonyFarm)runner).SetPlantSeed(player, num, itemByID))
			{
				flag = true;
			}
		}
		RPCOthers(EPacketType.PT_CL_FARM_SetPlantSeed, num, num2, flag);
	}

	private void RPC_C2S_FARM_SetPlantTool(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = false;
		ItemObject itemByID = ItemManager.GetItemByID(num2);
		if (itemByID != null)
		{
			Player player = Player.GetPlayer(info.sender);
			if (player != null && ((ColonyFarm)runner).SetPlantTool(player, num, itemByID))
			{
				flag = true;
			}
		}
		RPCOthers(EPacketType.PT_CL_FARM_SetPlantTool, num, num2, flag);
	}

	private void RPC_C2S_FARM_SetSequentialActive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		((ColonyFarm)runner).SetSequentialActive(flag);
		RPCOthers(EPacketType.PT_CL_FARM_SetSequentialActive, flag);
	}

	private void RPC_C2S_FARM_SetAutoPlanting(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		((ColonyFarm)runner).SetAutoPlanting(flag);
		RPCOthers(EPacketType.PT_CL_FARM_SetAutoPlanting, flag);
	}

	private void RPC_C2S_FARM_GetSeed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
	}

	private void RPC_C2S_FARM_FetchSeedItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		bool flag = false;
		if (player != null)
		{
			flag = ((ColonyFarm)runner).FetchSeedItem(player, num);
		}
		RPCOthers(EPacketType.PT_CL_FARM_FetchSeedItem, num, flag);
	}

	private void RPC_C2S_FARM_FetchToolItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		bool flag = false;
		if (player != null)
		{
			flag = ((ColonyFarm)runner).FetchPlantTool(player, num);
		}
		RPCOthers(EPacketType.PT_CL_FARM_FetchToolItem, num, flag);
	}

	private void RPC_C2S_InitDataPPCoal(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSPPCoalData cSPPCoalData = (CSPPCoalData)runner._RecordData;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<ItemObject> list3 = new List<ItemObject>();
		foreach (KeyValuePair<int, int> chargingItem in cSPPCoalData.m_ChargingItems)
		{
			list.Add(chargingItem.Key);
			list2.Add(chargingItem.Value);
			ItemObject itemByID = ItemManager.GetItemByID(chargingItem.Value);
			if (itemByID != null)
			{
				list3.Add(itemByID);
			}
		}
		if (list3.Count > 0)
		{
			ChannelNetwork.SyncItemList(info.sender, list3.ToArray());
		}
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataPPCoal, cSPPCoalData.m_CurDeleteTime, cSPPCoalData.m_CurRepairTime, cSPPCoalData.m_DeleteTime, runner.Durability, cSPPCoalData.m_RepairTime, cSPPCoalData.m_RepairValue, list.ToArray(), list2.ToArray(), cSPPCoalData.m_WorkedTime, cSPPCoalData.m_CurWorkedTime);
	}

	private void RPC_C2S_PPC_AddFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyPPCoal)runner).AddFuel(player))
			{
				RPCOthers(EPacketType.PT_CL_PPC_AddFuel, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_PPC_AddFuel, false);
			}
		}
	}

	private void RPC_C2S_PPC_WorkedTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCPeer(info.sender, EPacketType.PT_CL_PPC_WorkedTime, ((CSPPCoalData)runner._RecordData).m_CurWorkedTime);
	}

	private void RPC_C2S_InitDataPowerPlanet(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSPowerPlanetData cSPowerPlanetData = (CSPowerPlanetData)runner._RecordData;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, int> chargingItem in cSPowerPlanetData.m_ChargingItems)
		{
			list.Add(chargingItem.Key);
			list2.Add(chargingItem.Value);
		}
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataPowerPlanet, cSPowerPlanetData.m_CurDeleteTime, cSPowerPlanetData.m_CurRepairTime, cSPowerPlanetData.m_DeleteTime, runner.Durability, cSPowerPlanetData.m_RepairTime, cSPowerPlanetData.m_RepairValue, list.ToArray(), list2.ToArray(), true);
	}

	private void RPC_C2S_POW_AddChargItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (!(player != null))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(num2);
		if (itemByID != null)
		{
			if (((ColonyPowerPlant)runner).AddChargeItem(num, num2, player))
			{
				RPCOthers(EPacketType.PT_CL_POW_AddChargItem, num, num2, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_POW_AddChargItem, num, num2, false);
			}
		}
	}

	private void RPC_C2S_POW_GetChargItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (!(player != null))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID != null)
		{
			if (((ColonyPowerPlant)runner).RemoveChargeItem(itemByID, player))
			{
				RPCOthers(EPacketType.PT_CL_POW_GetChargItem, num, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_POW_GetChargItem, num, false);
			}
		}
	}

	private void RPC_C2S_InitDataProcessing(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSProcessingData cSProcessingData = (CSProcessingData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataProcessing, cSProcessingData.m_CurDeleteTime, cSProcessingData.m_CurRepairTime, cSProcessingData.m_DeleteTime, runner.Durability, cSProcessingData.m_RepairTime, cSProcessingData.m_RepairValue, (runner as ColonyProcessing).TaskTableToByte());
	}

	private void RPC_C2S_PRC_AddItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int protoType = stream.Read<int>(new object[0]);
		int count = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		if (processingEntity.AddItemToTask(protoType, count, num))
		{
			processingEntity.SyncSave();
			RPCOthers(EPacketType.PT_CL_PRC_AddItem, num, processingEntity.GetItemList(num).ToArray());
		}
	}

	private void RPC_C2S_PRC_RemoveItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int protoId = stream.Read<int>(new object[0]);
		processingEntity.RemoveItemFromTask(num, protoId, out var needRefresh);
		if (needRefresh)
		{
			processingEntity.SyncSave();
			RPCOthers(EPacketType.PT_CL_PRC_RemoveItem, num, processingEntity.GetItemList(num).ToArray());
		}
	}

	private void RPC_C2S_PRC_AddNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCOthers(EPacketType.PT_CL_PRC_AddNpc);
	}

	private void RPC_C2S_PRC_RemoveNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCOthers(EPacketType.PT_CL_PRC_RemoveNpc);
	}

	private void RPC_C2S_PRC_SetAuto(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool isAuto = stream.Read<bool>(new object[0]);
		processingEntity.IsAuto = isAuto;
		processingEntity.SyncSave();
		RPCOthers(EPacketType.PT_CL_PRC_SetAuto, processingEntity.IsAuto);
	}

	private void RPC_C2S_PRC_StartTask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (processingEntity.StartProcessing(num))
		{
			processingEntity.SyncSave();
			RPCOthers(EPacketType.PT_CL_PRC_Start, num);
		}
	}

	private void RPC_C2S_PRC_SetRound(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		processingEntity.SetRunCount(num, num2);
		RPCOthers(EPacketType.PT_CL_PRC_SetRound, num, num2);
	}

	private void RPC_C2S_PRC_StopTask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		processingEntity.StopProcessing(index);
		processingEntity.SyncSave();
	}

	private void RPC_C2S_PRC_InitrResultPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_C2S_InitDataRecycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSRecycleData cSRecycleData = (CSRecycleData)runner._RecordData;
		if (cSRecycleData.m_ItemID > 0 && ItemManager.GetItemByID(cSRecycleData.m_ItemID) == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("CSRecycle Not exist m_ItemID: " + cSRecycleData.m_ItemID);
			}
			else
			{
				ChannelNetwork.SyncItem(info.sender, ItemManager.GetItemByID(cSRecycleData.m_ItemID));
			}
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, int> recycleItem in cSRecycleData.m_RecycleItems)
		{
			list.Add(recycleItem.Key);
			list2.Add(recycleItem.Value);
		}
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataRecycle, cSRecycleData.m_CurDeleteTime, cSRecycleData.m_CurRepairTime, cSRecycleData.m_DeleteTime, runner.Durability, cSRecycleData.m_RepairTime, cSRecycleData.m_RepairValue, cSRecycleData.m_ItemID, cSRecycleData.m_CurTime, cSRecycleData.m_Time, list.ToArray(), list2.ToArray(), true);
	}

	private void RPC_C2S_RCY_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectId = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (!(player != null))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		if (itemByID != null)
		{
			if (((ColonyRecycle)runner).SetItem(itemByID, player))
			{
				RPCOthers(EPacketType.PT_CL_RCY_SetItem, itemByID.instanceId, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_RCY_SetItem, itemByID.instanceId, false);
			}
		}
	}

	private void RPC_C2S_RCY_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (((ColonyRecycle)runner).StartRecycle())
		{
			RPCOthers(EPacketType.PT_CL_RCY_Start, true);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_CL_RCY_Start, false);
		}
	}

	private void RPC_C2S_RCY_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (((ColonyRecycle)runner).StopRecycle())
		{
			RPCOthers(EPacketType.PT_CL_RCY_Stop, true);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_CL_RCY_Stop, false);
		}
	}

	private void RPC_C2S_RCY_FetchMaterial(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyRecycle)runner).FetchMaterial(num, player))
			{
				RPCOthers(EPacketType.PT_CL_RCY_FetchMaterial, num, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_RCY_FetchMaterial, num, false);
			}
		}
	}

	private void RPC_C2S_RCY_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyRecycle)runner).FetchItem(player))
			{
				RPCOthers(EPacketType.PT_CL_RCY_FetchItem, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_RCY_FetchItem, false);
			}
		}
	}

	private void RPC_C2S_InitDataRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSRepairData cSRepairData = (CSRepairData)runner._RecordData;
		if (cSRepairData.m_ItemID > 0)
		{
			ChannelNetwork.SyncItem(info.sender, ItemManager.GetItemByID(cSRepairData.m_ItemID));
		}
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataRepair, cSRepairData.m_CurDeleteTime, cSRepairData.m_CurRepairTime, cSRepairData.m_DeleteTime, runner.Durability, cSRepairData.m_RepairTime, cSRepairData.m_RepairValue, cSRepairData.m_ItemID, cSRepairData.m_CurTime, cSRepairData.m_Time);
	}

	private void RPC_C2S_RPA_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyRepair)runner).Start(player))
			{
				RPCOthers(EPacketType.PT_CL_RPA_Start, player.roleName, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_RPA_Start, player.roleName, false);
			}
		}
	}

	private void RPC_C2S_RPA_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (((ColonyRepair)runner).Stop())
		{
			RPCOthers(EPacketType.PT_CL_RPA_Stop, true);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_CL_RPA_Stop, false);
		}
	}

	private void RPC_C2S_RPA_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (!(player != null))
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(num);
		if (itemByID != null)
		{
			if (((ColonyRepair)runner).SetItem(itemByID.instanceId, player))
			{
				RPCOthers(EPacketType.PT_CL_RPA_SetItem, num, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_RPA_SetItem, num, false);
			}
		}
	}

	private void RPC_C2S_RPA_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyRepair)runner).FetchItem(player))
			{
				RPCOthers(EPacketType.PT_CL_RPA_FetchItem, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_RPA_FetchItem, false);
			}
		}
	}

	private void RPC_C2S_InitDataStorage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSStorageData cSStorageData = (CSStorageData)runner._RecordData;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<ItemObject> list3 = new List<ItemObject>();
		foreach (KeyValuePair<int, int> item in cSStorageData.m_Items)
		{
			ItemObject itemByID = ItemManager.GetItemByID(item.Value);
			if (itemByID != null)
			{
				list.Add(item.Key);
				list2.Add(item.Value);
				list3.Add(itemByID);
			}
		}
		if (list3.Count > 0)
		{
			ChannelNetwork.SyncItemList(info.sender, list3.ToArray());
		}
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataStorage, cSStorageData.m_CurDeleteTime, cSStorageData.m_CurRepairTime, cSStorageData.m_DeleteTime, runner.Durability, cSStorageData.m_RepairTime, cSStorageData.m_RepairValue, list.ToArray(), list2.ToArray(), cSStorageData.m_History.ToArray(), true);
	}

	private void RPC_C2S_STO_Delete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (((ColonyStorage)runner).Delete(num))
		{
			RPCOthers(EPacketType.PT_CL_STO_Delete, num, true);
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_CL_STO_Delete, num, false);
		}
	}

	private void RPC_C2S_STO_Store(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			index = ((ColonyStorage)runner).Store(index, num, player);
			if (index != -1)
			{
				RPCOthers(EPacketType.PT_CL_STO_Store, index, num, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_STO_Store, index, num, false);
			}
		}
	}

	private void RPC_C2S_STO_Fetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			if (((ColonyStorage)runner).Fetch(num, player, index))
			{
				RPCOthers(EPacketType.PT_CL_STO_Fetch, num, true);
			}
			else
			{
				RPCPeer(info.sender, EPacketType.PT_CL_STO_Fetch, num, false);
			}
		}
	}

	private void RPC_C2S_STO_Exchange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		if (num2 != num3 && !((ColonyStorage)runner).Exchange(num, num2, num3))
		{
			RPCPeer(info.sender, EPacketType.PT_CL_STO_Exchange, num, num3, -1, num2, false);
		}
	}

	private void RPC_C2S_STO_Split(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objID = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player != null && !((ColonyStorage)runner).Split(player, objID, num))
		{
			RPCPeer(info.sender, EPacketType.PT_CL_STO_Split, 0, -1, false);
		}
	}

	private void RPC_C2S_STO_Sort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int tabIndex = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player != null && !((ColonyStorage)runner).Sort(player, tabIndex))
		{
			RPCPeer(info.sender, EPacketType.PT_CL_STO_Sort, false);
		}
	}

	private void RPC_C2S_InitDataTent(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTentData cSTentData = (CSTentData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataTent, cSTentData.m_CurDeleteTime, cSTentData.m_CurRepairTime, cSTentData.m_DeleteTime, runner.Durability, cSTentData.m_RepairTime, cSTentData.m_RepairValue, (runner as ColonyTent).AllDataToByte());
	}

	private void RPC_C2S_TET_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(id);
		tentEntity.AddNpc(npcByID);
	}

	private void RPC_C2S_TET_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(id);
		tentEntity.Start(npcByID);
	}

	private void RPC_C2S_TET_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		tentEntity.RemoveDeadNpc(npcId);
	}

	private void RPC_C2S_InitDataTrade(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTradeData cSTradeData = (CSTradeData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataTrade, cSTradeData.m_CurDeleteTime, cSTradeData.m_CurRepairTime, cSTradeData.m_DeleteTime, runner.Durability, cSTradeData.m_RepairTime, cSTradeData.m_RepairValue);
	}

	private void RPC_C2S_TRD_RequestShop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (tradeEntity != null)
		{
			tradeEntity.UpdateShop(Player.GetPlayer(info.sender));
		}
	}

	private void RPC_C2S_TRD_BuyItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		int count = stream.Read<int>(new object[0]);
		if (tradeEntity != null)
		{
			tradeEntity.BuyItem(instanceId, count, Player.GetPlayer(info.sender));
		}
	}

	private void RPC_C2S_TRD_SellItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		int count = stream.Read<int>(new object[0]);
		if (tradeEntity != null)
		{
			tradeEntity.SellItem(instanceId, count, Player.GetPlayer(info.sender));
		}
	}

	private void RPC_C2S_TRD_RepurchaseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		int count = stream.Read<int>(new object[0]);
		if (tradeEntity != null)
		{
			tradeEntity.RepurchaseItem(instanceId, count, Player.GetPlayer(info.sender));
		}
	}

	private void RPC_C2S_InitDataTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTrainData cSTrainData = (CSTrainData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataTrain, cSTrainData.m_CurDeleteTime, cSTrainData.m_CurRepairTime, cSTrainData.m_DeleteTime, runner.Durability, cSTrainData.m_RepairTime, cSTrainData.m_RepairValue, (runner as ColonyTrain).AllDataToByte());
	}

	private void RPC_C2S_TRN_StartSkillTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> list = stream.Read<int[]>(new object[0]).ToList();
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (!trainEntity.IsTraining && trainEntity.CheckInstructorAndTraineeId(num, num2))
		{
			trainEntity.trainingType = ETrainingType.Skill;
			trainEntity.InstructorNpcId = num;
			trainEntity.TraineeNpcId = num2;
			trainEntity.SetNpcIsTraining(flag: true);
			trainEntity.StartSkillCounter(list);
			RPCOthers(EPacketType.PT_CL_TRN_StartSkillTraining, list.ToArray(), num, num2);
			trainEntity.SyncSave();
		}
	}

	private void RPC_C2S_TRN_StartAttributeTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (trainEntity.CheckInstructorAndTraineeId(num, num2))
		{
			ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(num2);
			if (npcByID.CanAttributeUp())
			{
				trainEntity.trainingType = ETrainingType.Attribute;
				trainEntity.InstructorNpcId = num;
				trainEntity.TraineeNpcId = num2;
				ColonyNpc npcByID2 = ColonyNpcMgr.GetNpcByID(num);
				trainEntity.StartAttributeCounter(npcByID2, npcByID);
				RPCOthers(EPacketType.PT_CL_TRN_StartAttributeTraining, num, num2);
				trainEntity.SyncSave();
			}
		}
	}

	private void RPC_C2S_TRN_SetInstructor(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (trainEntity.CheckTrainerId(num))
		{
			ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(num);
			if (trainEntity.AddInstructor(npcByID))
			{
				RPCOthers(EPacketType.PT_CL_TRN_SetInstructor, num);
			}
			trainEntity.SyncSave();
		}
	}

	private void RPC_C2S_TRN_SetTrainee(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (trainEntity.CheckTrainerId(num))
		{
			ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(num);
			if (trainEntity.AddTrainee(npcByID))
			{
				RPCOthers(EPacketType.PT_CL_TRN_SetTrainee, num);
			}
			trainEntity.SyncSave();
		}
	}

	private void RPC_C2S_TRN_StopTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		trainEntity.StopTraining();
		RPCOthers(EPacketType.PT_CL_TRN_StopTraining);
		trainEntity.SyncSave();
	}

	private void RPC_C2S_InitDataTreat(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTreatData cSTreatData = (CSTreatData)runner._RecordData;
		RPCPeer(info.sender, EPacketType.PT_CL_InitDataTreat, cSTreatData.m_CurDeleteTime, cSTreatData.m_CurRepairTime, cSTreatData.m_DeleteTime, runner.Durability, cSTreatData.m_RepairTime, cSTreatData.m_RepairValue, cSTreatData.m_ObjID, cSTreatData.npcIds.ToArray(), cSTreatData.m_CurTime, cSTreatData.m_Time, cSTreatData.isNpcReady, cSTreatData.occupied);
	}

	private void RPC_C2S_TRT_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(id);
		treatEntity.AddNpc(npcByID);
	}

	private void RPC_C2S_TRT_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		ColonyNpc npcByID = ColonyNpcMgr.GetNpcByID(id);
		treatEntity.Start(npcByID);
	}

	private void RPC_C2S_TRT_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		treatEntity.RemoveDeadPatient(npcId);
	}

	private void RPC_C2S_TRT_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool isMis = stream.Read<bool>(new object[0]);
		int instanceId = stream.Read<int>(new object[0]);
		bool inOrOut = stream.Read<bool>(new object[0]);
		int tabindex = stream.Read<int>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		treatEntity.SetItem(isMis, instanceId, inOrOut, index, tabindex, Player.GetPlayer(info.sender));
	}

	public override void SkCreater()
	{
		if (!(_skEntity == null) && _skEntity._attribs != null)
		{
			_skEntity.SetAllAttribute(AttribType.HpMax, ColonyMgr.GetInfo(base.ExternId).m_Durability);
			_skEntity.SetAllAttribute(AttribType.Hp, runner.Durability);
			_skEntity.SetAllAttribute(AttribType.CampID, base.TeamId);
			_skEntity.SetAllAttribute(AttribType.DefaultPlayerID, base.TeamId);
			_skEntity.SetAllAttribute(AttribType.DamageID, base.TeamId);
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("ColonyNetMgr");
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.group;
		Add(this);
		InitializeData();
		switch (base.ExternId)
		{
		case 1127:
			new ColonyAssembly(this);
			GameWorld.OccupyArea(base.WorldId, base.TeamId, base.transform.position, 377);
			break;
		case 1128:
			new ColonyPPCoal(this);
			break;
		case 1129:
			new ColonyStorage(this);
			break;
		case 1130:
			new ColonyRepair(this);
			break;
		case 1131:
			new ColonyDwellings(this);
			break;
		case 1132:
			new ColonyEnhance(this);
			break;
		case 1133:
			new ColonyRecycle(this);
			break;
		case 1134:
			new ColonyFarm(this);
			break;
		case 1135:
			new ColonyFactory(this);
			break;
		case 1356:
			new ColonyProcessing(this);
			break;
		case 1357:
			new ColonyTrade(this);
			break;
		case 1424:
			new ColonyCheck(this);
			break;
		case 1422:
			new ColonyTreat(this);
			break;
		case 1421:
			new ColonyTent(this);
			break;
		case 1423:
			new ColonyTrain(this);
			break;
		case 1558:
			new ColonyPPFusion(this);
			break;
		default:
			if (LogFilter.logDebug)
			{
				Debug.LogError("ColonySystem itemid is wrong id = " + base.ExternId);
			}
			break;
		}
		base.PEInstantiateEvent += DropItemManager.OnInitialized;
		base.PEDestroyEvent += DropItemManager.OnDestroy;
		base.PEDestroyEvent += runner.OnDestroy;
		Player.PlayerDisconnected += OnPlayerDisconnect;
		if (!DropItemManager.HasRecord(base.Id))
		{
			SyncSave();
		}
		AddSkEntity();
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_CL_InitData, RPC_C2S_InitData);
		BindAction(EPacketType.PT_CL_Turn, RPC_C2S_Turn);
		BindAction(EPacketType.PT_CL_Recycle, RPC_C2S_Recycle);
		BindAction(EPacketType.PT_CL_Repair, RPC_C2S_Repair);
		BindAction(EPacketType.PT_CL_InitDataAssembly, RPC_C2S_InitDataAssembly);
		BindAction(EPacketType.PT_CL_ASB_LevelUp, RPC_C2S_ASB_LevelUp);
		BindAction(EPacketType.PT_CL_ASB_QueryTime, RPC_C2S_ASB_QueryTime);
		BindAction(EPacketType.PT_CL_ASB_HideShield, RPC_C2S_ASB_HideShield);
		BindAction(EPacketType.PT_CL_InitDataDwellings, RPC_C2S_InitDataDwellings);
		BindAction(EPacketType.PT_CL_DWL_SyncNpc, RPC_C2S_DWL_SyncNpc);
		BindAction(EPacketType.PT_CL_InitDataEnhance, RPC_C2S_InitDataEnhance);
		BindAction(EPacketType.PT_CL_EHN_SetItem, RPC_C2S_EHN_SetItem);
		BindAction(EPacketType.PT_CL_EHN_Start, RPC_C2S_EHN_Start);
		BindAction(EPacketType.PT_CL_EHN_Stop, RPC_C2S_EHN_Stop);
		BindAction(EPacketType.PT_CL_EHN_Fetch, RPC_C2S_EHN_Fetch);
		BindAction(EPacketType.PT_CL_InitDataFactory, RPC_C2S_InitDataFactory);
		BindAction(EPacketType.PT_CL_FCT_Compoud, RPC_C2S_FCT_Compoud);
		BindAction(EPacketType.PT_CL_FCT_Fetch, RPC_C2S_FCT_Fetch);
		BindAction(EPacketType.PT_CL_FCT_GenFactoryCancel, PRC_C2S_FCT_GenFactoryCancel);
		BindAction(EPacketType.PT_CL_InitDataFarm, RPC_C2S_InitDataFarm);
		BindAction(EPacketType.PT_CL_FARM_SetPlantSeed, RPC_C2S_FARM_SetPlantSeed);
		BindAction(EPacketType.PT_CL_FARM_SetPlantTool, RPC_C2S_FARM_SetPlantTool);
		BindAction(EPacketType.PT_CL_FARM_SetSequentialActive, RPC_C2S_FARM_SetSequentialActive);
		BindAction(EPacketType.PT_CL_FARM_SetAutoPlanting, RPC_C2S_FARM_SetAutoPlanting);
		BindAction(EPacketType.PT_CL_FARM_GetSeed, RPC_C2S_FARM_GetSeed);
		BindAction(EPacketType.PT_CL_FARM_FetchSeedItem, RPC_C2S_FARM_FetchSeedItem);
		BindAction(EPacketType.PT_CL_FARM_FetchToolItem, RPC_C2S_FARM_FetchToolItem);
		BindAction(EPacketType.PT_CL_InitDataPowerPlanet, RPC_C2S_InitDataPowerPlanet);
		BindAction(EPacketType.PT_CL_POW_AddChargItem, RPC_C2S_POW_AddChargItem);
		BindAction(EPacketType.PT_CL_POW_GetChargItem, RPC_C2S_POW_GetChargItem);
		BindAction(EPacketType.PT_CL_InitDataPPCoal, RPC_C2S_InitDataPPCoal);
		BindAction(EPacketType.PT_CL_PPC_AddFuel, RPC_C2S_PPC_AddFuel);
		BindAction(EPacketType.PT_CL_PPC_WorkedTime, RPC_C2S_PPC_WorkedTime);
		BindAction(EPacketType.PT_CL_InitDataRecycle, RPC_C2S_InitDataRecycle);
		BindAction(EPacketType.PT_CL_RCY_SetItem, RPC_C2S_RCY_SetItem);
		BindAction(EPacketType.PT_CL_RCY_Start, RPC_C2S_RCY_Start);
		BindAction(EPacketType.PT_CL_RCY_Stop, RPC_C2S_RCY_Stop);
		BindAction(EPacketType.PT_CL_RCY_FetchMaterial, RPC_C2S_RCY_FetchMaterial);
		BindAction(EPacketType.PT_CL_RCY_FetchItem, RPC_C2S_RCY_FetchItem);
		BindAction(EPacketType.PT_CL_InitDataStorage, RPC_C2S_InitDataStorage);
		BindAction(EPacketType.PT_CL_STO_Delete, RPC_C2S_STO_Delete);
		BindAction(EPacketType.PT_CL_STO_Store, RPC_C2S_STO_Store);
		BindAction(EPacketType.PT_CL_STO_Fetch, RPC_C2S_STO_Fetch);
		BindAction(EPacketType.PT_CL_STO_Exchange, RPC_C2S_STO_Exchange);
		BindAction(EPacketType.PT_CL_STO_Split, RPC_C2S_STO_Split);
		BindAction(EPacketType.PT_CL_STO_Sort, RPC_C2S_STO_Sort);
		BindAction(EPacketType.PT_CL_InitDataRepair, RPC_C2S_InitDataRepair);
		BindAction(EPacketType.PT_CL_RPA_Start, RPC_C2S_RPA_Start);
		BindAction(EPacketType.PT_CL_RPA_Stop, RPC_C2S_RPA_Stop);
		BindAction(EPacketType.PT_CL_RPA_SetItem, RPC_C2S_RPA_SetItem);
		BindAction(EPacketType.PT_CL_RPA_FetchItem, RPC_C2S_RPA_FetchItem);
		BindAction(EPacketType.PT_CL_InitDataProcessing, RPC_C2S_InitDataProcessing);
		BindAction(EPacketType.PT_CL_PRC_AddItem, RPC_C2S_PRC_AddItem);
		BindAction(EPacketType.PT_CL_PRC_RemoveItem, RPC_C2S_PRC_RemoveItem);
		BindAction(EPacketType.PT_CL_PRC_AddNpc, RPC_C2S_PRC_AddNpc);
		BindAction(EPacketType.PT_CL_PRC_RemoveNpc, RPC_C2S_PRC_RemoveNpc);
		BindAction(EPacketType.PT_CL_PRC_SetRound, RPC_C2S_PRC_SetRound);
		BindAction(EPacketType.PT_CL_PRC_SetAuto, RPC_C2S_PRC_SetAuto);
		BindAction(EPacketType.PT_CL_PRC_Start, RPC_C2S_PRC_StartTask);
		BindAction(EPacketType.PT_CL_PRC_Stop, RPC_C2S_PRC_StopTask);
		BindAction(EPacketType.PT_CL_PRC_InitResultPos, RPC_C2S_PRC_InitrResultPos);
		BindAction(EPacketType.PT_CL_InitDataTrade, RPC_C2S_InitDataTrade);
		BindAction(EPacketType.PT_CL_TRD_RequestShop, RPC_C2S_TRD_RequestShop);
		BindAction(EPacketType.PT_CL_TRD_BuyItem, RPC_C2S_TRD_BuyItem);
		BindAction(EPacketType.PT_CL_TRD_SellItem, RPC_C2S_TRD_SellItem);
		BindAction(EPacketType.PT_CL_TRD_RepurchaseItem, RPC_C2S_TRD_RepurchaseItem);
		BindAction(EPacketType.PT_CL_InitDataCheck, RPC_C2S_InitDataCheck);
		BindAction(EPacketType.PT_CL_CHK_FindMachine, RPC_C2S_CHK_FindMachine);
		BindAction(EPacketType.PT_CL_CHK_TryStart, RPC_C2S_CHK_TryStart);
		BindAction(EPacketType.PT_CL_CHK_RemoveDeadNpc, RPC_C2S_CHK_RemoveDeadNpc);
		BindAction(EPacketType.PT_CL_InitDataTreat, RPC_C2S_InitDataTreat);
		BindAction(EPacketType.PT_CL_TRT_FindMachine, RPC_C2S_TRT_FindMachine);
		BindAction(EPacketType.PT_CL_TRT_TryStart, RPC_C2S_TRT_TryStart);
		BindAction(EPacketType.PT_CL_TRT_SetItem, RPC_C2S_TRT_SetItem);
		BindAction(EPacketType.PT_CL_TRT_RemoveDeadNpc, RPC_C2S_TRT_RemoveDeadNpc);
		BindAction(EPacketType.PT_CL_InitDataTent, RPC_C2S_InitDataTent);
		BindAction(EPacketType.PT_CL_TET_FindMachine, RPC_C2S_TET_FindMachine);
		BindAction(EPacketType.PT_CL_TET_TryStart, RPC_C2S_TET_TryStart);
		BindAction(EPacketType.PT_CL_TET_RemoveDeadNpc, RPC_C2S_TET_RemoveDeadNpc);
		BindAction(EPacketType.PT_CL_InitDataTrain, RPC_C2S_InitDataTrain);
		BindAction(EPacketType.PT_CL_TRN_StartSkillTraining, RPC_C2S_TRN_StartSkillTraining);
		BindAction(EPacketType.PT_CL_TRN_StartAttributeTraining, RPC_C2S_TRN_StartAttributeTraining);
		BindAction(EPacketType.PT_CL_TRN_SetInstructor, RPC_C2S_TRN_SetInstructor);
		BindAction(EPacketType.PT_CL_TRN_SetTrainee, RPC_C2S_TRN_SetTrainee);
		BindAction(EPacketType.PT_CL_TRN_StopTraining, RPC_C2S_TRN_StopTraining);
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		int externId = base.ExternId;
		if (externId == 1127)
		{
			GameWorld.LoseArea(base.WorldId, base.TeamId, base.transform.position);
		}
		Player.PlayerDisconnected -= OnPlayerDisconnect;
		base.PEInstantiateEvent -= DropItemManager.OnInitialized;
		base.PEDestroyEvent -= DropItemManager.OnDestroy;
		if (runner != null)
		{
			base.PEDestroyEvent -= runner.OnDestroy;
		}
	}

	protected override void InitializeData()
	{
		InitCmpt();
	}

	internal override void OnDeath(int casterId = 0)
	{
		base.OnDeath(casterId);
		Delete();
		runner.OnDeath();
	}

	public void Delete()
	{
		NetObjectData netObjectData = new NetObjectData();
		netObjectData.DeleteData(base.Id);
		AsyncSqlite.AddRecord(netObjectData);
	}

	public void SyncSave()
	{
		ItemObject itemByID = ItemManager.GetItemByID(base.Id);
		if (itemByID != null)
		{
			ColonyNetData colonyNetData = new ColonyNetData();
			colonyNetData.ExportData(this);
			AsyncSqlite.AddRecord(colonyNetData);
		}
	}

	public static void RenewSave(ColonyNetworkRenewData renewData)
	{
		ItemObject itemByID = ItemManager.GetItemByID(renewData.id);
		if (itemByID != null)
		{
			ColonyNetData colonyNetData = new ColonyNetData();
			colonyNetData.ExportData(renewData);
			AsyncSqlite.AddRecord(colonyNetData);
		}
	}

	internal override void OnDamage(int casterId, float damge)
	{
		runner.Durability += damge;
		if (runner.Durability < -0.001f)
		{
			runner.Durability = -0.001f;
		}
		base.OnDamage(casterId, damge);
	}

	internal void DestroyMe()
	{
		StartCoroutine(DestroyCoroutine());
	}

	protected IEnumerator DestroyCoroutine()
	{
		yield return new WaitForSeconds(2f);
		NetInterface.NetDestroy(this);
	}

	public bool GetBack()
	{
		float durability = runner.Durability;
		float durability2 = ColonyMgr.GetInfo(base.ExternId).m_Durability;
		if (durability < durability2 * 0.15f)
		{
			return false;
		}
		runner.Durability = durability - Mathf.Floor(durability2 * 0.1f);
		SyncDurability();
		runner.DestroySelf();
		runner.SyncSave();
		return true;
	}

	public void SyncDurability()
	{
		RPCOthers(EPacketType.PT_CL_SyncColonyDurability, runner.Durability);
	}

	public void RemoveEntity()
	{
		RPCOthers(EPacketType.PT_CL_RemoveColonyEntity, base.Id);
	}

	private void RPC_C2S_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemByID = ItemManager.GetItemByID(base.Id);
		if (itemByID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("item==null,id= " + base.Id);
			}
		}
		else
		{
			RPCPeer(info.sender, EPacketType.PT_CL_InitData, itemByID, base.transform.rotation.eulerAngles.y);
		}
	}

	private void RPC_C2S_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.transform.Rotate(Vector3.up, 90f, Space.World);
		RPCOthers(EPacketType.PT_CL_Turn, base.transform.rotation.eulerAngles.y);
		SyncSave();
	}

	private void RPC_C2S_Recycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (null != player)
		{
			runner.BeginRecycle(player.roleName);
		}
	}

	private void RPC_C2S_Repair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Player player = Player.GetPlayer(info.sender);
		if (player != null)
		{
			runner.RepairItems(player, base.ExternId);
		}
	}
}
