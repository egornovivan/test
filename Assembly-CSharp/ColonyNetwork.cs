using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSRecord;
using CustomData;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class ColonyNetwork : AiNetwork
{
	public ColonyBase _ColonyObj;

	public CSEntity m_Entity;

	public int _ownerId;

	public CSMedicalCheck checkEntity => m_Entity as CSMedicalCheck;

	public CSProcessing csp => m_Entity as CSProcessing;

	public CSMedicalTent tentEntity => m_Entity as CSMedicalTent;

	public CSTrade tradeEntity => m_Entity as CSTrade;

	public CSTraining trainEntity => m_Entity as CSTraining;

	public CSMedicalTreat treatEntity => m_Entity as CSMedicalTreat;

	public bool BelongToOwner => base.TeamId == BaseNetwork.MainPlayer.TeamId;

	private void OnTeamChange()
	{
		if (!(null == Singleton<ForceSetting>.Instance) && m_Entity != null)
		{
			Vector3 zero = Vector3.zero;
			CSBuildingLogic cSBuildingLogic = ((!(m_Entity.gameLogic == null)) ? m_Entity.gameLogic.GetComponent<CSBuildingLogic>() : null);
			zero = ((!(cSBuildingLogic != null) || !(cSBuildingLogic.travelTrans != null)) ? (base._pos + new Vector3(0f, 2f, 0f)) : cSBuildingLogic.travelTrans.position);
			if (Singleton<ForceSetting>.Instance.Conflict(base.TeamId, PlayerNetwork.mainPlayerId))
			{
				ColonyLabel.Remove(zero);
			}
			else if (!ColonyLabel.ContainsIcon(base._pos))
			{
				new ColonyLabel(zero);
			}
		}
	}

	private void RPC_S2C_InitDataAssembly(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSAssemblyData cSAssemblyData = (CSAssemblyData)_ColonyObj._RecordData;
		cSAssemblyData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSAssemblyData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSAssemblyData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSAssemblyData.m_Durability = stream.Read<float>(new object[0]);
		cSAssemblyData.m_RepairTime = stream.Read<float>(new object[0]);
		cSAssemblyData.m_RepairValue = stream.Read<float>(new object[0]);
		cSAssemblyData.m_Level = stream.Read<int>(new object[0]);
		cSAssemblyData.m_TimeTicks = stream.Read<long>(new object[0]);
		cSAssemblyData.m_UpgradeTime = stream.Read<float>(new object[0]);
		cSAssemblyData.m_CurUpgradeTime = stream.Read<float>(new object[0]);
		cSAssemblyData.m_ShowShield = stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_CounterTick(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float counter = stream.Read<float>(new object[0]);
		CSAssembly cSAssembly = m_Entity as CSAssembly;
		cSAssembly.SetCounter(counter);
	}

	private void RPC_S2C_ASB_LevelUp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int level = stream.Read<int>(new object[0]);
		CSAssemblyData cSAssemblyData = (CSAssemblyData)_ColonyObj._RecordData;
		cSAssemblyData.m_UpgradeTime = -1f;
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		CSAssembly cSAssembly = m_Entity as CSAssembly;
		cSAssembly.StopCounter();
		cSAssembly.SetLevel(level);
		cSAssembly.ChangeState();
		cSAssembly.RefreshErodeMap();
		cSAssembly.RefreshAssemblyObject();
		cSAssembly.ExcuteEvent(2003);
	}

	private void RPC_S2C_ASB_QueryTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSAssemblyData cSAssemblyData = (CSAssemblyData)_ColonyObj._RecordData;
		cSAssemblyData.m_CurUpgradeTime = stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_ASB_LevelUpStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSAssemblyData cSAssemblyData = (CSAssemblyData)_ColonyObj._RecordData;
		cSAssemblyData.m_CurUpgradeTime = stream.Read<float>(new object[0]);
		cSAssemblyData.m_UpgradeTime = stream.Read<float>(new object[0]);
		string rolename = stream.Read<string>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogWarning("entity not ready");
			return;
		}
		CSAssembly cSAssembly = m_Entity as CSAssembly;
		cSAssembly.StartUpgradeCounter(cSAssemblyData.m_CurUpgradeTime, cSAssemblyData.m_UpgradeTime);
		if (CSUI_MainWndCtrl.Instance.AssemblyUI != null && flag)
		{
			CSUI_MainWndCtrl.Instance.AssemblyUI.UpgradeStartSuccuss(cSAssembly, rolename);
		}
	}

	private void RPC_S2C_ASB_HideShield(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool bShowShield = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		CSAssembly cSAssembly = m_Entity as CSAssembly;
		cSAssembly.bShowShield = bShowShield;
	}

	private void RPC_S2C_ASB_ShowTips(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int type = stream.Read<int>(new object[0]);
		int replaceStrId = stream.Read<int>(new object[0]);
		if (PlayerNetwork.mainPlayer != null && CSAutocycleMgr.Instance != null && base.TeamId == BaseNetwork.MainPlayer.TeamId)
		{
			CSAutocycleMgr.Instance.ShowTips((ETipType)type, replaceStrId);
		}
	}

	public void ASB_QueryCurUpTime()
	{
		RPCServer(EPacketType.PT_CL_ASB_QueryTime);
	}

	public void ASB_LevelUp()
	{
		RPCServer(EPacketType.PT_CL_ASB_LevelUp);
	}

	public void ASB_HideShield(bool showShield)
	{
		RPCServer(EPacketType.PT_CL_ASB_HideShield, showShield);
	}

	private void RPC_S2C_InitDataCheck(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSCheckData cSCheckData = (CSCheckData)_ColonyObj._RecordData;
		cSCheckData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSCheckData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSCheckData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSCheckData.m_Durability = stream.Read<float>(new object[0]);
		cSCheckData.m_RepairTime = stream.Read<float>(new object[0]);
		cSCheckData.m_RepairValue = stream.Read<float>(new object[0]);
		cSCheckData.npcIds = stream.Read<int[]>(new object[0]).ToList();
		cSCheckData.m_CurTime = stream.Read<float>(new object[0]);
		cSCheckData.m_Time = stream.Read<float>(new object[0]);
		cSCheckData.isNpcReady = stream.Read<bool>(new object[0]);
		cSCheckData.occupied = stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_CHK_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> npcIds = stream.Read<int[]>(new object[0]).ToList();
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			checkEntity.AddNpcResult(npcIds);
		}
	}

	private void RPC_S2C_CHK_SetDiagnose(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			checkEntity.SetDiagnose();
		}
	}

	private void RPC_S2C_CHK_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			checkEntity.TryStartResult(npcId);
		}
	}

	private void RPC_S2C_CHK_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			checkEntity.RemoveDeadPatientResult(npcId);
		}
	}

	private void RPC_S2C_CHK_CheckFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		List<CSTreatment> csts = stream.Read<CSTreatment[]>(new object[0]).ToList();
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			checkEntity.CheckFinish(npcId, csts);
		}
	}

	private void RPC_S2C_InitDataDwellings(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSDwellingsData cSDwellingsData = (CSDwellingsData)_ColonyObj._RecordData;
		cSDwellingsData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSDwellingsData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSDwellingsData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSDwellingsData.m_Durability = stream.Read<float>(new object[0]);
		cSDwellingsData.m_RepairTime = stream.Read<float>(new object[0]);
		cSDwellingsData.m_RepairValue = stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_DWL_SyncNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = array;
		foreach (int num in array2)
		{
			if (num == 0)
			{
				continue;
			}
			CSDwellings cSDwellings = m_Entity as CSDwellings;
			CSMgCreator creator = MultiColonyManager.GetCreator(base.TeamId, createNewIfNone: false);
			if (creator != null)
			{
				CSPersonnel npc = creator.GetNpc(num);
				if (npc != null && npc.Dwellings != null)
				{
					npc.Dwellings.RemoveNpc(npc);
				}
				if (npc != null)
				{
					cSDwellings?.AddNpcs(npc);
				}
			}
		}
	}

	private void RPC_S2C_InitDataEnhance(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSEnhanceData cSEnhanceData = (CSEnhanceData)_ColonyObj._RecordData;
		cSEnhanceData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSEnhanceData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSEnhanceData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSEnhanceData.m_Durability = stream.Read<float>(new object[0]);
		cSEnhanceData.m_RepairTime = stream.Read<float>(new object[0]);
		cSEnhanceData.m_RepairValue = stream.Read<float>(new object[0]);
		cSEnhanceData.m_ObjID = stream.Read<int>(new object[0]);
		cSEnhanceData.m_CurTime = stream.Read<float>(new object[0]);
		cSEnhanceData.m_Time = stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_EHN_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		if (flag)
		{
			CSEnhanceData cSEnhanceData = (CSEnhanceData)_ColonyObj._RecordData;
			cSEnhanceData.m_ObjID = num;
		}
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.SetResult(flag, num, m_Entity);
		}
	}

	private void RPC_S2C_EHN_Fetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		if (flag)
		{
			CSEnhanceData cSEnhanceData = (CSEnhanceData)_ColonyObj._RecordData;
			cSEnhanceData.m_ObjID = 0;
		}
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.FetchResult(flag, m_Entity);
		}
	}

	private void RPC_S2C_EHN_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string rolename = stream.Read<string>(new object[0]);
		if (!stream.Read<bool>(new object[0]))
		{
			return;
		}
		CSEnhanceData cSEnhanceData = (CSEnhanceData)_ColonyObj._RecordData;
		cSEnhanceData.m_CurTime = 0f;
		cSEnhanceData.m_Time = ((CSEnhance)m_Entity).CountFinalTime();
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		((CSEnhance)m_Entity).StartCounter(cSEnhanceData.m_CurTime, cSEnhanceData.m_Time);
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.StartWorkerResult(4, m_Entity, rolename);
		}
	}

	private void RPC_S2C_EHN_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else if (flag)
		{
			((CSEnhance)m_Entity).StopCounter();
		}
	}

	private void RPC_S2C_EHN_End(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSEnhanceData cSEnhanceData = (CSEnhanceData)_ColonyObj._RecordData;
		cSEnhanceData.m_CurTime = -1f;
		cSEnhanceData.m_Time = -1f;
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		((CSEnhance)m_Entity).StopCounter();
		((CSEnhance)m_Entity).OnEnhanced();
	}

	private void RPC_S2C_EHN_SyncTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<float>(new object[0]);
	}

	public void EHN_SetItem(ItemObject item)
	{
		if (item != null && (PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(item) || PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(item)))
		{
			RPCServer(EPacketType.PT_CL_EHN_SetItem, item.instanceId);
		}
	}

	public void EHN_Start()
	{
		RPCServer(EPacketType.PT_CL_EHN_Start);
	}

	public void EHN_Stop()
	{
		RPCServer(EPacketType.PT_CL_EHN_Stop);
	}

	public void EHN_Fetch()
	{
		RPCServer(EPacketType.PT_CL_EHN_Fetch);
	}

	private void RPC_S2C_InitDataFactory(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFactoryData cSFactoryData = (CSFactoryData)_ColonyObj._RecordData;
		cSFactoryData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSFactoryData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSFactoryData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSFactoryData.m_Durability = stream.Read<float>(new object[0]);
		cSFactoryData.m_RepairTime = stream.Read<float>(new object[0]);
		cSFactoryData.m_RepairValue = stream.Read<float>(new object[0]);
		CompoudItem[] array = stream.Read<CompoudItem[]>(new object[0]);
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				cSFactoryData.m_CompoudItems.Add(array[i]);
			}
		}
	}

	private void RPC_S2C_FCT_IsReady(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			((CSFactory)m_Entity).MultiModeIsReady(index);
		}
	}

	private void RPC_S2C_FCT_AddCompoudList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CompoudItem compoudItem = stream.Read<CompoudItem>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			((CSFactory)m_Entity).SetCompoudItem(compoudItem.itemID, compoudItem.itemCnt, compoudItem.time);
		}
	}

	private void RPC_S2C_FCT_RemoveCompoudList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			((CSFactory)m_Entity).MultiModeTakeAwayCompoudItem(index);
		}
	}

	private void RPC_S2C_FCT_SyncItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		CompoudItem value = stream.Read<CompoudItem>(new object[0]);
		CSFactoryData cSFactoryData = (CSFactoryData)_ColonyObj._RecordData;
		if (cSFactoryData.m_CompoudItems.Count > num)
		{
			cSFactoryData.m_CompoudItems[num] = value;
		}
	}

	private void RPC_S2C_FCT_Fetch(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (stream.Read<bool>(new object[0]))
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayCompoundItem.GetString(), ItemProto.GetItemData(id).GetName()));
		}
	}

	private void RPC_S2C_FCT_Compoud(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<bool>(new object[0]))
		{
			int item_id = stream.Read<int>(new object[0]);
			if (CSUI_MainWndCtrl.Instance.FactoryUI != null)
			{
				CSUI_MainWndCtrl.Instance.FactoryUI.OnCompoundBtnClickSuccess(item_id, (CSFactory)m_Entity);
			}
		}
	}

	private void RPC_S2C_FCT_SyncAllItems(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CompoudItem[] allItems = stream.Read<CompoudItem[]>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			((CSFactory)m_Entity).SetAllItems(allItems);
		}
	}

	private void RPC_S2C_FCT_GenFactoryCancel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int[] items = stream.Read<int[]>(new object[0]);
		RandomItemMgr.Instance.GenFactoryCancel(pos, quaternion, items);
	}

	public void FCT_Compoud(int skillId, int count)
	{
		RPCServer(EPacketType.PT_CL_FCT_Compoud, skillId, count);
	}

	public void FCT_Fetch(int index)
	{
		RPCServer(EPacketType.PT_CL_FCT_Fetch, index);
	}

	private void RPC_S2C_InitDataFarm(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		cSFarmData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSFarmData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSFarmData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSFarmData.m_Durability = stream.Read<float>(new object[0]);
		cSFarmData.m_RepairTime = stream.Read<float>(new object[0]);
		cSFarmData.m_RepairValue = stream.Read<float>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = stream.Read<int[]>(new object[0]);
		int[] array3 = stream.Read<int[]>(new object[0]);
		int[] array4 = stream.Read<int[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			cSFarmData.m_PlantSeeds[array[i]] = array2[i];
		}
		for (int j = 0; j < array3.Length; j++)
		{
			cSFarmData.m_Tools[array3[j]] = array4[j];
		}
		cSFarmData.m_AutoPlanting = stream.Read<bool>(new object[0]);
		cSFarmData.m_SequentialPlanting = stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_FARM_SetPlantSeed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			cSFarmData.m_PlantSeeds[num] = num2;
		}
		if (CSUI_MainWndCtrl.Instance.FarmUI != null)
		{
			CSUI_MainWndCtrl.Instance.FarmUI.SetPlantSeedResult(flag, num2, num, m_Entity);
		}
	}

	private void RPC_C2S_FARM_SetPlantTool(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			cSFarmData.m_Tools[num] = num2;
		}
		if (CSUI_MainWndCtrl.Instance.FarmUI != null)
		{
			CSUI_MainWndCtrl.Instance.FarmUI.SetPlantToolResult(flag, num2, num, m_Entity);
		}
	}

	private void RPC_S2C_FARM_SetSequentialActive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		bool active = (cSFarmData.m_SequentialPlanting = stream.Read<bool>(new object[0]));
		if (CSUI_MainWndCtrl.Instance.FarmUI != null)
		{
			CSUI_MainWndCtrl.Instance.FarmUI.SetSequentialActiveResult(active, m_Entity);
		}
	}

	private void RPC_S2C_FARM_SetAutoPlanting(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		bool autoPlanting = stream.Read<bool>(new object[0]);
		cSFarmData.m_AutoPlanting = autoPlanting;
	}

	private void RPC_S2C_FARM_FetchSeedItemResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			cSFarmData.m_PlantSeeds[num] = 0;
		}
		if (CSUI_MainWndCtrl.Instance.FarmUI != null)
		{
			CSUI_MainWndCtrl.Instance.FarmUI.FetchSeedResult(flag, num, m_Entity);
		}
	}

	private void RPC_S2C_FARM_FetchToolItemResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			cSFarmData.m_Tools[num] = 0;
		}
		if (CSUI_MainWndCtrl.Instance.FarmUI != null)
		{
			CSUI_MainWndCtrl.Instance.FarmUI.FetchToolResult(flag, num, m_Entity);
		}
	}

	private void RPC_S2C_FARM_DeleteSeed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		int num = stream.Read<int>(new object[0]);
		int num2 = -1;
		foreach (KeyValuePair<int, int> plantSeed in cSFarmData.m_PlantSeeds)
		{
			if (plantSeed.Value == num)
			{
				num2 = plantSeed.Key;
				break;
			}
		}
		if (num2 != -1 && CSUI_MainWndCtrl.Instance.FarmUI != null)
		{
			CSUI_MainWndCtrl.Instance.FarmUI.DeleteSeedResult(m_Entity, num, num2);
		}
	}

	private void RPC_S2C_FARM_DeletePlantTool(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSFarmData cSFarmData = (CSFarmData)_ColonyObj._RecordData;
		int num = stream.Read<int>(new object[0]);
		int num2 = -1;
		foreach (KeyValuePair<int, int> tool in cSFarmData.m_Tools)
		{
			if (tool.Value == num)
			{
				num2 = tool.Key;
				return;
			}
		}
		if (num2 != -1)
		{
			cSFarmData.m_Tools.Remove(num2);
		}
		if (CSUI_MainWndCtrl.Instance.FarmUI != null)
		{
			CSUI_MainWndCtrl.Instance.FarmUI.DeleteToolResult(m_Entity, num);
		}
	}

	private void RPC_S2C_FARM_RestoreWater(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			(m_Entity as CSFarm).RestoreWateringPlant(plant);
		}
	}

	private void RPC_S2C_FARM_RestoreClean(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			(m_Entity as CSFarm).RestoreCleaningPlant(plant);
		}
	}

	private void RPC_S2C_FARM_RestoreGetBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		FarmPlantLogic plant = stream.Read<FarmPlantLogic>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			(m_Entity as CSFarm).RestoreRipePlant(plant);
		}
	}

	public void SetPlantSeed(int index, int itemObjId)
	{
		RPCServer(EPacketType.PT_CL_FARM_SetPlantSeed, index, itemObjId);
	}

	public void SetPlantTool(int index, int itemObjId)
	{
		RPCServer(EPacketType.PT_CL_FARM_SetPlantTool, index, itemObjId);
	}

	public void SetSequentialActive(bool bActive)
	{
		RPCServer(EPacketType.PT_CL_FARM_SetSequentialActive, bActive);
	}

	public void SetAutoPlanting(bool bActive)
	{
		RPCServer(EPacketType.PT_CL_FARM_SetAutoPlanting, bActive);
	}

	public void FetchSeedItem(int index)
	{
		RPCServer(EPacketType.PT_CL_FARM_FetchSeedItem, index);
	}

	public void FetchToolItem(int index)
	{
		RPCServer(EPacketType.PT_CL_FARM_FetchToolItem, index);
	}

	private void RPC_S2C_InitDataPPCoal(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSPPCoalData cSPPCoalData = (CSPPCoalData)_ColonyObj._RecordData;
		cSPPCoalData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSPPCoalData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSPPCoalData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSPPCoalData.m_Durability = stream.Read<float>(new object[0]);
		cSPPCoalData.m_RepairTime = stream.Read<float>(new object[0]);
		cSPPCoalData.m_RepairValue = stream.Read<float>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = stream.Read<int[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			cSPPCoalData.m_ChargingItems[array[i]] = array2[i];
		}
		cSPPCoalData.m_WorkedTime = stream.Read<float>(new object[0]);
		cSPPCoalData.m_CurWorkedTime = stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_PPC_WorkedTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSPPCoalData cSPPCoalData = (CSPPCoalData)_ColonyObj._RecordData;
		cSPPCoalData.m_CurWorkedTime = stream.Read<float>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		CSPPCoal cSPPCoal = m_Entity as CSPPCoal;
		cSPPCoal.StartWorkingCounter(cSPPCoalData.m_CurWorkedTime);
		if (CSUI_MainWndCtrl.Instance.PPCoalUI != null)
		{
			CSUI_MainWndCtrl.Instance.PPCoalUI.AddFuelSuccess(cSPPCoal);
		}
	}

	public void PPC_AddFuel()
	{
		RPCServer(EPacketType.PT_CL_PPC_AddFuel);
	}

	public void PPC_QueryCurWorkedTime()
	{
		RPCServer(EPacketType.PT_CL_PPC_WorkedTime);
	}

	public void PPC_ShowElectric(bool active)
	{
	}

	private void RPC_S2C_PPC_NoPower(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		((CSPPCoal)m_Entity).StopCounter();
		((CSPPCoal)m_Entity).OnWorked();
	}

	private void RPC_S2C_PPC_AddFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_InitDataPowerPlanet(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSPowerPlanetData cSPowerPlanetData = (CSPowerPlanetData)_ColonyObj._RecordData;
		cSPowerPlanetData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSPowerPlanetData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSPowerPlanetData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSPowerPlanetData.m_Durability = stream.Read<float>(new object[0]);
		cSPowerPlanetData.m_RepairTime = stream.Read<float>(new object[0]);
		cSPowerPlanetData.m_RepairValue = stream.Read<float>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = stream.Read<int[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			cSPowerPlanetData.m_ChargingItems[array[i]] = array2[i];
		}
	}

	public void POW_RemoveChargItem(int objId)
	{
		RPCServer(EPacketType.PT_CL_POW_GetChargItem, objId);
	}

	private void RPC_S2C_POW_AddChargItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		if (flag)
		{
			((CSPowerPlant)m_Entity).m_ChargingItems[num] = PeSingleton<ItemMgr>.Instance.Get(num2).GetCmpt<Energy>();
			((CSPowerPlanetData)_ColonyObj._RecordData).m_ChargingItems[num] = num2;
		}
		if (m_Entity is CSPPCoal)
		{
			CSUI_MainWndCtrl.Instance.PPCoalUI.AddChargeItemResult(flag, num, num2, (CSPPCoal)m_Entity);
		}
	}

	private void RPC_S2C_POW_GetChargItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		if (flag)
		{
			Energy[] chargingItems = ((CSPowerPlant)m_Entity).m_ChargingItems;
			Dictionary<int, int> chargingItems2 = ((CSPowerPlanetData)_ColonyObj._RecordData).m_ChargingItems;
			for (int i = 0; i < chargingItems.Length; i++)
			{
				if (chargingItems[i] != null && chargingItems[i].itemObj.instanceId == num)
				{
					chargingItems[i] = null;
					chargingItems2.Remove(i);
					break;
				}
			}
		}
		if (m_Entity is CSPPCoal)
		{
			CSUI_MainWndCtrl.Instance.PPCoalUI.GetChargItemResult(flag, num, (CSPPCoal)m_Entity);
		}
	}

	public void POW_AddChargItem(int index, ItemObject item)
	{
		if (item == null || PeSingleton<PeCreature>.Instance == null || null == PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			return;
		}
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (!(null == cmpt) && cmpt.package != null && cmpt.package.HasItemObj(item))
		{
			EquipmentCmpt cmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<EquipmentCmpt>();
			if (!(null == cmpt2) && cmpt2._ItemList != null && !cmpt2._ItemList.Contains(item))
			{
				RPCServer(EPacketType.PT_CL_POW_AddChargItem, index, item.instanceId);
			}
		}
	}

	private void RPC_S2C_InitDataProcessing(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSProcessingData cSProcessingData = (CSProcessingData)_ColonyObj._RecordData;
		cSProcessingData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSProcessingData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSProcessingData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSProcessingData.m_Durability = stream.Read<float>(new object[0]);
		cSProcessingData.m_RepairTime = stream.Read<float>(new object[0]);
		cSProcessingData.m_RepairValue = stream.Read<float>(new object[0]);
		byte[] data = stream.Read<byte[]>(new object[0]);
		CSProcessing.ParseData(data, cSProcessingData);
	}

	private void RPC_S2C_PRC_AddItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ItemIdCount[] source = stream.Read<ItemIdCount[]>(new object[0]);
		if (csp == null)
		{
			Debug.LogError("processing not exist");
			return;
		}
		if (num < 0 || num >= csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
			return;
		}
		if (csp.mTaskTable[num] == null)
		{
			csp.mTaskTable[num] = new ProcessingTask();
		}
		csp.mTaskTable[num].itemList = source.ToList();
		csp.AddItemResult(num);
	}

	private void RPC_S2C_PRC_RemoveItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		ItemIdCount[] source = stream.Read<ItemIdCount[]>(new object[0]);
		if (csp == null)
		{
			Debug.LogError("processing not exist");
			return;
		}
		if (num < 0 || num >= csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
			return;
		}
		if (csp.mTaskTable[num] == null)
		{
			csp.mTaskTable[num] = new ProcessingTask();
		}
		csp.mTaskTable[num].itemList = source.ToList();
		csp.RemoveItemResult(num);
	}

	private void RPC_S2C_PRC_AddNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_PRC_RemoveNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_PRC_SetRound(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int runCount = stream.Read<int>(new object[0]);
		if (csp.mTaskTable[num] == null)
		{
			csp.mTaskTable[num] = new ProcessingTask();
		}
		csp.mTaskTable[num].SetRunCount(runCount);
		csp.SetRoundResult(num);
	}

	private void RPC_S2C_PRC_SetAuto(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool isAuto = stream.Read<bool>(new object[0]);
		if (csp == null)
		{
			Debug.LogError("processing not exist");
		}
		else
		{
			csp.IsAuto = isAuto;
		}
	}

	private void RPC_S2C_PRC_StartTask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (csp == null)
		{
			Debug.LogError("processing not exist");
		}
		else if (num < 0 || num >= csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
		}
		else if (csp.mTaskTable[num] != null)
		{
			csp.StartProcessing(num);
			csp.StartResult(num);
		}
	}

	private void RPC_S2C_PRC_StopTask(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		if (csp == null)
		{
			Debug.LogError("processing not exist");
			return;
		}
		if (num < 0 || num >= csp.mTaskTable.Length)
		{
			Debug.LogError("index illegel");
			return;
		}
		csp.Stop(num);
		csp.StopResult(num);
	}

	private void RPC_S2C_PRC_GenResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int[] items = stream.Read<int[]>(new object[0]);
		if (!(PlayerNetwork.mainPlayer == null))
		{
			if (base.TeamId == BaseNetwork.MainPlayer.TeamId)
			{
				CSUtils.ShowTips(82201068);
			}
			RandomItemMgr.Instance.GenProcessingItem(pos, quaternion, items);
		}
	}

	private void RPC_S2C_PRC_FinishToStorage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!(PlayerNetwork.mainPlayer == null) && base.TeamId == BaseNetwork.MainPlayer.TeamId)
		{
			CSUtils.ShowTips(82201067);
		}
	}

	private void RPC_S2C_PRC_SyncAllCounter(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (csp == null)
		{
			Debug.LogError("processing not exist");
			return;
		}
		List<float> list = stream.Read<float[]>(new object[0]).ToList();
		List<float> list2 = stream.Read<float[]>(new object[0]).ToList();
		List<int> list3 = stream.Read<int[]>(new object[0]).ToList();
		for (int i = 0; i < 4; i++)
		{
			if (list[i] < 0f)
			{
				csp.SyncStop(i);
				csp.StopResult(i);
			}
			else
			{
				csp.SetCounter(i, list[i], list2[i], list3[i]);
			}
		}
	}

	public void InitResultPos(Vector3[] resultTrans)
	{
		RPCServer(EPacketType.PT_CL_PRC_InitResultPos, resultTrans);
	}

	private void RPC_S2C_InitDataRecycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSRecycleData cSRecycleData = (CSRecycleData)_ColonyObj._RecordData;
		cSRecycleData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSRecycleData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSRecycleData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSRecycleData.m_Durability = stream.Read<float>(new object[0]);
		cSRecycleData.m_RepairTime = stream.Read<float>(new object[0]);
		cSRecycleData.m_RepairValue = stream.Read<float>(new object[0]);
		cSRecycleData.m_ObjID = stream.Read<int>(new object[0]);
		cSRecycleData.m_CurTime = stream.Read<float>(new object[0]);
		cSRecycleData.m_Time = stream.Read<float>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = stream.Read<int[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			((ColonyRecycle)_ColonyObj).m_RecycleItems[array[i]] = array2[i];
		}
	}

	private void RPC_S2C_RCY_SyncRecycleItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		stream.Read<int>(new object[0]);
	}

	private void RPC_S2C_RCY_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			CSRecycleData cSRecycleData = (CSRecycleData)_ColonyObj._RecordData;
			cSRecycleData.m_ObjID = num;
		}
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.SetResult(flag, num, m_Entity);
		}
	}

	private void RPC_S2C_RCY_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!stream.Read<bool>(new object[0]))
		{
			return;
		}
		CSRecycleData cSRecycleData = (CSRecycleData)_ColonyObj._RecordData;
		cSRecycleData.m_CurTime = 0f;
		cSRecycleData.m_Time = ((CSRecycle)m_Entity).CountFinalTime();
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		((CSRecycle)m_Entity).StartCounter(cSRecycleData.m_CurTime, cSRecycleData.m_Time);
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.StartWorkerResult(6, m_Entity, string.Empty);
		}
	}

	private void RPC_S2C_RCY_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else if (flag)
		{
			((CSRecycle)m_Entity).StopCounter();
		}
	}

	private void RPC_S2C_RCY_FetchMaterial(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_RCY_End(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.TeamId == BaseNetwork.MainPlayer.TeamId)
		{
			CSUtils.ShowTips(82201094);
			ResetRecycle();
		}
	}

	private void RPC_S2C_RCY_MatsToStorage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!(PlayerNetwork.mainPlayer == null) && base.TeamId == BaseNetwork.MainPlayer.TeamId)
		{
			CSUtils.ShowTips(82201095);
			ResetRecycle();
		}
	}

	private void RPC_S2C_RCY_MatsToResult(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int[] items = stream.Read<int[]>(new object[0]);
		if (!(PlayerNetwork.mainPlayer == null) && base.TeamId == BaseNetwork.MainPlayer.TeamId)
		{
			CSUtils.ShowTips(82201096);
			RandomItemMgr.Instance.GenProcessingItem(pos, quaternion, items);
			ResetRecycle();
		}
	}

	private void ResetRecycle()
	{
		if (_ColonyObj == null)
		{
			return;
		}
		CSRecycleData cSRecycleData = (CSRecycleData)_ColonyObj._RecordData;
		if (cSRecycleData != null)
		{
			cSRecycleData.m_CurTime = -1f;
			cSRecycleData.m_Time = -1f;
			if (m_Entity == null)
			{
				Debug.LogError("entity not ready");
				return;
			}
			((CSRecycle)m_Entity).StopCounter();
			((CSRecycle)m_Entity).m_Item = null;
			((CSRecycle)m_Entity).onRecylced();
		}
	}

	private void RPC_S2C_RCY_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			CSRecycleData cSRecycleData = (CSRecycleData)_ColonyObj._RecordData;
			cSRecycleData.m_ObjID = 0;
		}
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.FetchResult(flag, m_Entity);
		}
	}

	private void RPC_S2C_RCY_SyncTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<float>(new object[0]);
	}

	public void RCY_SetItem(ItemObject item)
	{
		if (item != null && (PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(item) || PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(item)))
		{
			RPCServer(EPacketType.PT_CL_RCY_SetItem, item.instanceId);
		}
	}

	public void RCY_Start()
	{
		RPCServer(EPacketType.PT_CL_RCY_Start);
	}

	public void RCY_Stop()
	{
		RPCServer(EPacketType.PT_CL_RCY_Stop);
	}

	public void RCY_FetchMaterial(int itemId)
	{
		RPCServer(EPacketType.PT_CL_RCY_FetchMaterial, itemId);
	}

	public void RCY_FetchItem()
	{
		RPCServer(EPacketType.PT_CL_RCY_FetchItem);
	}

	private void RPC_S2C_InitDataRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSRepairData cSRepairData = (CSRepairData)_ColonyObj._RecordData;
		cSRepairData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSRepairData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSRepairData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSRepairData.m_Durability = stream.Read<float>(new object[0]);
		cSRepairData.m_RepairTime = stream.Read<float>(new object[0]);
		cSRepairData.m_RepairValue = stream.Read<float>(new object[0]);
		cSRepairData.m_ObjID = stream.Read<int>(new object[0]);
		cSRepairData.m_CurTime = stream.Read<float>(new object[0]);
		cSRepairData.m_Time = stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_RPA_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			CSRepairData cSRepairData = (CSRepairData)_ColonyObj._RecordData;
			cSRepairData.m_ObjID = num;
		}
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.SetResult(flag, num, m_Entity);
		}
	}

	private void RPC_S2C_RPA_Start(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string rolename = stream.Read<string>(new object[0]);
		if (!stream.Read<bool>(new object[0]))
		{
			return;
		}
		CSRepairData cSRepairData = (CSRepairData)_ColonyObj._RecordData;
		cSRepairData.m_CurTime = 0f;
		cSRepairData.m_Time = ((CSRepair)m_Entity).CountFinalTime();
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		((CSRepair)m_Entity).StartCounter(cSRepairData.m_CurTime, cSRepairData.m_Time);
		if (CSUI_MainWndCtrl.Instance.EngineeringUI != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.StartWorkerResult(5, m_Entity, rolename);
		}
	}

	private void RPC_S2C_RPA_Stop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.Read<bool>(new object[0]))
		{
			((CSRepair)m_Entity).StopCounter();
		}
	}

	private void RPC_S2C_RPA_End(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSRepairData cSRepairData = (CSRepairData)_ColonyObj._RecordData;
		cSRepairData.m_CurTime = -1f;
		cSRepairData.m_Time = -1f;
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		((CSRepair)m_Entity).StopCounter();
		((CSRepair)m_Entity).OnRepairItemEnd();
	}

	private void RPC_S2C_RPA_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		if (flag)
		{
			CSRepairData cSRepairData = (CSRepairData)_ColonyObj._RecordData;
			cSRepairData.m_ObjID = 0;
		}
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else if (CSUI_MainWndCtrl.Instance.EngineeringUI != null && CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering != null)
		{
			CSUI_MainWndCtrl.Instance.EngineeringUI.m_SubEngneering.FetchResult(flag, m_Entity);
		}
	}

	private void RPC_S2C_RPA_SyncTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<float>(new object[0]);
	}

	public void RPA_Stop()
	{
		RPCServer(EPacketType.PT_CL_RPA_Stop);
	}

	public void RPA_Start()
	{
		RPCServer(EPacketType.PT_CL_RPA_Start);
	}

	public void RPA_SetItem(int objId)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
		if (itemObject != null && (PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(itemObject) || PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(itemObject)))
		{
			RPCServer(EPacketType.PT_CL_RPA_SetItem, objId);
		}
	}

	public void RPA_FetchItem()
	{
		RPCServer(EPacketType.PT_CL_RPA_FetchItem);
	}

	private void RPC_S2C_InitDataStorage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
		cSStorageData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSStorageData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSStorageData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSStorageData.m_Durability = stream.Read<float>(new object[0]);
		cSStorageData.m_RepairTime = stream.Read<float>(new object[0]);
		cSStorageData.m_RepairValue = stream.Read<float>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = stream.Read<int[]>(new object[0]);
		for (int i = 0; i < array.Length; i++)
		{
			cSStorageData.m_Items[array[i]] = array2[i];
		}
	}

	private void RPC_S2C_STO_Delete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
		if (itemObject == null)
		{
			return;
		}
		int index = -1;
		if (flag)
		{
			CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
			foreach (KeyValuePair<int, int> item in cSStorageData.m_Items)
			{
				if (item.Value == num)
				{
					cSStorageData.m_Items.Remove(item.Key);
					ItemPackage package = ((CSStorage)m_Entity).m_Package;
					int length = package.GetSlotList().Length;
					index = item.Key % length;
					package.GetSlotList(itemObject.protoId)[index] = null;
					break;
				}
			}
		}
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
		{
			CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultDelete(flag, index, num, (CSStorage)m_Entity);
		}
		if (flag)
		{
			PeSingleton<ItemMgr>.Instance.DestroyItem(itemObject.instanceId);
		}
	}

	private void RPC_S2C_STO_Store(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
		if (itemObject != null)
		{
			int tabIndex = itemObject.protoData.tabIndex;
			int num2 = IndexToKey(index, tabIndex);
			if (flag)
			{
				CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
				cSStorageData.m_Items[num2] = num;
				ItemPackage package = ((CSStorage)m_Entity).m_Package;
				int length = package.GetSlotList().Length;
				int index2 = num2 % length;
				package.GetSlotList(itemObject.protoId)[index2] = itemObject;
			}
			if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
			{
				CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultStore(flag, index, num, (CSStorage)m_Entity);
			}
		}
	}

	private void RPC_S2C_STO_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
		if (itemObject == null)
		{
			return;
		}
		if (flag)
		{
			CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
			foreach (KeyValuePair<int, int> item in cSStorageData.m_Items)
			{
				if (item.Value == num)
				{
					cSStorageData.m_Items.Remove(item.Key);
					ItemPackage package = ((CSStorage)m_Entity).m_Package;
					package.RemoveItem(itemObject);
					break;
				}
			}
		}
		if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
		{
			CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultFetch(flag, num, (CSStorage)m_Entity);
		}
	}

	private void RPC_S2C_STO_Exchange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		int num4 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
		if (itemObject == null)
		{
			return;
		}
		ItemObject itemObject2;
		if (num3 == -1)
		{
			itemObject2 = null;
		}
		else
		{
			itemObject2 = PeSingleton<ItemMgr>.Instance.Get(num3);
			if (itemObject2 == null)
			{
				return;
			}
		}
		int tabIndex = itemObject.protoData.tabIndex;
		int key = IndexToKey(num2, tabIndex);
		int key2 = IndexToKey(num4, tabIndex);
		if (flag)
		{
			CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
			cSStorageData.m_Items[key] = num;
			ItemPackage package = ((CSStorage)m_Entity).m_Package;
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)tabIndex);
			slotList.Swap(num4, num2);
			if (itemObject2 != null)
			{
				cSStorageData.m_Items[key2] = num3;
			}
			else
			{
				cSStorageData.m_Items.Remove(key2);
			}
		}
		if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
		{
			CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultExchange(flag, num, num2, num3, num4, (CSStorage)m_Entity);
		}
	}

	private void RPC_S2C_STO_Split(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
		if (itemObject != null)
		{
			if (flag)
			{
				CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
				int key = IndexToKey(num2, itemObject.protoData.tabIndex);
				cSStorageData.m_Items[key] = num;
				ItemPackage package = ((CSStorage)m_Entity).m_Package;
				SlotList slotList = package.GetSlotList(itemObject.protoId);
				slotList[num2] = itemObject;
			}
			if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
			{
				CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultSplit(flag, num, num2, (CSStorage)m_Entity);
			}
		}
	}

	private void RPC_S2C_STO_Sort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!stream.Read<bool>(new object[0]))
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		int[] array = stream.Read<int[]>(new object[0]);
		CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
		int num2 = IndexToKey(0, num);
		ItemPackage package = ((CSStorage)m_Entity).m_Package;
		SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)num);
		int length = slotList.Length;
		for (int i = 0; i < array.Length; i++)
		{
			int num3 = num2 + i;
			int index = num3 % length;
			if (array[i] == -1)
			{
				cSStorageData.m_Items.Remove(num3);
				slotList[index] = null;
				continue;
			}
			cSStorageData.m_Items[num3] = array[i];
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(array[i]);
			if (itemObject != null)
			{
				slotList[index] = itemObject;
			}
		}
		if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
		{
			CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreSortSuccess(num, array, (CSStorage)m_Entity);
		}
	}

	private void RPC_S2C_STO_SyncItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		int[] array2 = stream.Read<int[]>(new object[0]);
		CSStorageData cSStorageData = (CSStorageData)_ColonyObj._RecordData;
		ItemPackage package = ((CSStorage)m_Entity).m_Package;
		for (int i = 0; i < array.Length; i++)
		{
			int key = array[i];
			int type;
			int index = KeyToIndex(key, out type);
			SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)type);
			if (array2[i] == -1)
			{
				cSStorageData.m_Items.Remove(key);
				slotList[index] = null;
				continue;
			}
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(array2[i]);
			if (itemObject != null)
			{
				cSStorageData.m_Items[key] = array2[i];
				slotList[index] = itemObject;
			}
		}
		if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
		{
			CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStorageResultSyncItemList((CSStorage)m_Entity);
		}
	}

	public void Delete(int objId)
	{
		RPCServer(EPacketType.PT_CL_STO_Delete, objId);
	}

	public void STO_Store(int index, ItemObject item)
	{
		if (item != null && (PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(item) || PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(item)))
		{
			RPCServer(EPacketType.PT_CL_STO_Store, index, item.instanceId);
		}
	}

	public void STO_Fetch(int objId, int index)
	{
		RPCServer(EPacketType.PT_CL_STO_Fetch, index, objId);
	}

	public void STO_Exchange(int objId, int originIndex, int destIndex)
	{
		RPCServer(EPacketType.PT_CL_STO_Exchange, objId, originIndex, destIndex);
	}

	public void STO_Split(int objId, int num)
	{
		RPCServer(EPacketType.PT_CL_STO_Split, objId, num);
	}

	public void STO_Sort(int tabIndex)
	{
		RPCServer(EPacketType.PT_CL_STO_Sort, tabIndex);
	}

	public int IndexToKey(int index, int type)
	{
		int length = ((CSStorage)m_Entity).m_Package.GetSlotList().Length;
		int num = 0;
		switch (type)
		{
		case 1:
			num = 1;
			break;
		case 2:
			num = 2;
			break;
		}
		return index + num * length;
	}

	public int KeyToIndex(int key, out int type)
	{
		if (key < CSInfoMgr.m_StorageInfo.m_MaxItem)
		{
			type = 0;
			return key;
		}
		key -= CSInfoMgr.m_StorageInfo.m_MaxItem;
		if (key < CSInfoMgr.m_StorageInfo.m_MaxEquip)
		{
			type = 1;
			return key;
		}
		key -= CSInfoMgr.m_StorageInfo.m_MaxEquip;
		if (key < CSInfoMgr.m_StorageInfo.m_MaxRecource)
		{
			type = 2;
			return key;
		}
		key -= CSInfoMgr.m_StorageInfo.m_MaxRecource;
		if (key < CSInfoMgr.m_StorageInfo.m_MaxArmor)
		{
			type = 3;
			return key;
		}
		type = -1;
		return -1;
	}

	private void RPC_S2C_InitDataTent(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTentData cSTentData = (CSTentData)_ColonyObj._RecordData;
		cSTentData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSTentData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSTentData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSTentData.m_Durability = stream.Read<float>(new object[0]);
		cSTentData.m_RepairTime = stream.Read<float>(new object[0]);
		cSTentData.m_RepairValue = stream.Read<float>(new object[0]);
		byte[] data = stream.Read<byte[]>(new object[0]);
		CSMedicalTent.ParseData(data, cSTentData);
	}

	private void RPC_S2C_TET_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> npcIds = stream.Read<int[]>(new object[0]).ToList();
		int npcid = stream.Read<int>(new object[0]);
		int index = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			tentEntity.AddNpcResult(npcIds, npcid, index);
		}
	}

	private void RPC_S2C_TET_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			tentEntity.TryStartResult(npcId);
		}
	}

	private void RPC_S2C_TET_SetTent(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int tent = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			tentEntity.SetTent(tent);
		}
	}

	private void RPC_S2C_TET_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			tentEntity.RemoveDeadPatientResult(npcId);
		}
	}

	private void RPC_S2C_TET_TentFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			tentEntity.TentFinish(npcId);
		}
	}

	private void RPC_S2C_InitDataTrade(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTradeData cSTradeData = (CSTradeData)_ColonyObj._RecordData;
		cSTradeData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSTradeData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSTradeData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSTradeData.m_Durability = stream.Read<float>(new object[0]);
		cSTradeData.m_RepairTime = stream.Read<float>(new object[0]);
		cSTradeData.m_RepairValue = stream.Read<float>(new object[0]);
	}

	private void RPC_S2C_BuyItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		if (tradeEntity != null)
		{
			tradeEntity.UpdateBuyResultMulti(instanceId);
		}
	}

	private void RPC_S2C_SellItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		if (tradeEntity != null)
		{
			tradeEntity.UpdateSellResultMulti(instanceId);
		}
	}

	private void RPC_S2C_RepurchaseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		if (tradeEntity != null)
		{
			tradeEntity.UpdateRepurchaseResultMulti(instanceId);
		}
	}

	private void RPC_S2C_UpdateBuyItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> instanceIdList = stream.Read<int[]>(new object[0]).ToList();
		if (tradeEntity != null)
		{
			tradeEntity.UpdateBuyItemMulti(instanceIdList);
		}
	}

	private void RPC_S2C_UpdateRepurchaseItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> instanceIdList = stream.Read<int[]>(new object[0]).ToList();
		if (tradeEntity != null)
		{
			tradeEntity.UpdateRepurchaseMulti(instanceIdList);
		}
	}

	private void RPC_S2C_UpdateMoney(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int money = stream.Read<int>(new object[0]);
		if (tradeEntity != null)
		{
			tradeEntity.UpdateMoneyMulti(money);
		}
	}

	private void RPC_S2C_InitDataTrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTrainData cSTrainData = (CSTrainData)_ColonyObj._RecordData;
		cSTrainData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSTrainData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSTrainData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSTrainData.m_Durability = stream.Read<float>(new object[0]);
		cSTrainData.m_RepairTime = stream.Read<float>(new object[0]);
		cSTrainData.m_RepairValue = stream.Read<float>(new object[0]);
		byte[] data = stream.Read<byte[]>(new object[0]);
		CSTraining.ParseData(data, cSTrainData);
	}

	private void RPC_S2C_TRN_StartSkillTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> skillIds = stream.Read<int[]>(new object[0]).ToList();
		int instructorId = stream.Read<int>(new object[0]);
		int traineeId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			trainEntity.OnStartSkillTrainingResult(skillIds, instructorId, traineeId);
		}
	}

	private void RPC_S2C_TRN_StartAttributeTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instructorId = stream.Read<int>(new object[0]);
		int traineeId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			trainEntity.OnTrainAttributeTrainingResult(instructorId, traineeId);
		}
	}

	private void RPC_S2C_TRN_SetInstructor(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		CSPersonnel npc = trainEntity.m_MgCreator.GetNpc(id);
		trainEntity.AddInstructor(npc);
		trainEntity.SetCount();
	}

	private void RPC_S2C_TRN_SetTrainee(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
			return;
		}
		CSPersonnel npc = trainEntity.m_MgCreator.GetNpc(id);
		trainEntity.AddTrainee(npc);
		trainEntity.SetCount();
	}

	private void RPC_S2C_TRN_SkillTrainFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> collection = stream.Read<int[]>(new object[0]).ToList();
		int traineeId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			trainEntity.LearnSkillFinishResult(new Ablities(collection), traineeId);
		}
	}

	private void RPC_S2C_TRN_AttributeTrainFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instructorId = stream.Read<int>(new object[0]);
		int traineeId = stream.Read<int>(new object[0]);
		int upgradeTimes = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			trainEntity.AttributeFinish(instructorId, traineeId, upgradeTimes);
		}
	}

	private void RPC_S2C_TRN_StopTraining(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			trainEntity.StopTrainingrResult();
		}
	}

	private void RPC_S2C_TRN_SyncCounter(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float curTime = stream.Read<float>(new object[0]);
		float finalTime = stream.Read<float>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			trainEntity.SetCounter(curTime, finalTime);
		}
	}

	private void RPC_S2C_InitDataTreat(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTreatData cSTreatData = (CSTreatData)_ColonyObj._RecordData;
		cSTreatData.m_CurDeleteTime = stream.Read<float>(new object[0]);
		cSTreatData.m_CurRepairTime = stream.Read<float>(new object[0]);
		cSTreatData.m_DeleteTime = stream.Read<float>(new object[0]);
		cSTreatData.m_Durability = stream.Read<float>(new object[0]);
		cSTreatData.m_RepairTime = stream.Read<float>(new object[0]);
		cSTreatData.m_RepairValue = stream.Read<float>(new object[0]);
		cSTreatData.m_ObjID = stream.Read<int>(new object[0]);
		cSTreatData.npcIds = stream.Read<int[]>(new object[0]).ToList();
		cSTreatData.m_CurTime = stream.Read<float>(new object[0]);
		cSTreatData.m_Time = stream.Read<float>(new object[0]);
		cSTreatData.isNpcReady = stream.Read<bool>(new object[0]);
		cSTreatData.occupied = stream.Read<bool>(new object[0]);
	}

	private void RPC_S2C_TRT_FindMachine(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		List<int> npcIds = stream.Read<int[]>(new object[0]).ToList();
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.AddNpcResult(npcIds);
		}
	}

	private void RPC_S2C_TRT_SetTreat(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSTreatment treat = stream.Read<CSTreatment>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.SetTreat(treat);
		}
	}

	private void RPC_S2C_TRT_TryStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.TryStartResult(npcId);
		}
	}

	private void RPC_S2C_TRT_StartTreatCounter(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.StartCounterResult();
		}
	}

	private void RPC_S2C_TRT_SetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>(new object[0]);
		bool inorout = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.SetItemResult(objId, inorout);
		}
	}

	private void RPC_S2C_TRT_DeleteItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.DeleteMedicine(instanceId);
		}
	}

	private void RPC_S2C_TRT_RemoveDeadNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.RemoveDeadPatientResult(npcId);
		}
	}

	private void RPC_S2C_TRT_TreatFinish(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		bool treatSuccess = stream.Read<bool>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.TreatFinish(npcId, treatSuccess);
		}
	}

	private void RPC_S2C_TRT_ResetNpcToCheck(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int npcId = stream.Read<int>(new object[0]);
		if (m_Entity == null)
		{
			Debug.LogError("entity not ready");
		}
		else
		{
			treatEntity.ResetNpcToCheck(npcId);
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		base._pos = base.transform.position;
		switch (base.ExternId)
		{
		case 1127:
			PlayerNetwork.OnTeamChangedEventHandler += OnTeamChange;
			PlayerNetwork.OnLimitBoundsAdd(base.Id, new Bounds(base._pos, new Vector3(128f, 128f, 128f)));
			_ColonyObj = new ColonyAssembly(this);
			RPCServer(EPacketType.PT_CL_InitDataAssembly);
			break;
		case 1128:
			_ColonyObj = new ColonyPPCoal(this);
			RPCServer(EPacketType.PT_CL_InitDataPPCoal);
			break;
		case 1129:
			_ColonyObj = new ColonyStorage(this);
			RPCServer(EPacketType.PT_CL_InitDataStorage);
			break;
		case 1130:
			_ColonyObj = new ColonyRepair(this);
			RPCServer(EPacketType.PT_CL_InitDataRepair);
			break;
		case 1131:
			_ColonyObj = new ColonyDwellings(this);
			RPCServer(EPacketType.PT_CL_InitDataDwellings);
			break;
		case 1132:
			_ColonyObj = new ColonyEnhance(this);
			RPCServer(EPacketType.PT_CL_InitDataEnhance);
			break;
		case 1133:
			_ColonyObj = new ColonyRecycle(this);
			RPCServer(EPacketType.PT_CL_InitDataRecycle);
			break;
		case 1134:
			_ColonyObj = new ColonyFarm(this);
			RPCServer(EPacketType.PT_CL_InitDataFarm);
			break;
		case 1135:
			_ColonyObj = new ColonyFactory(this);
			RPCServer(EPacketType.PT_CL_InitDataFactory);
			break;
		case 1356:
			_ColonyObj = new ColonyProcessing(this);
			RPCServer(EPacketType.PT_CL_InitDataProcessing);
			break;
		case 1357:
			_ColonyObj = new ColonyTrade(this);
			RPCServer(EPacketType.PT_CL_InitDataTrade);
			break;
		case 1424:
			_ColonyObj = new ColonyCheck(this);
			RPCServer(EPacketType.PT_CL_InitDataCheck);
			break;
		case 1422:
			_ColonyObj = new ColonyTreat(this);
			RPCServer(EPacketType.PT_CL_InitDataTreat);
			break;
		case 1421:
			_ColonyObj = new ColonyTent(this);
			RPCServer(EPacketType.PT_CL_InitDataTent);
			break;
		case 1423:
			_ColonyObj = new ColonyTrain(this);
			RPCServer(EPacketType.PT_CL_InitDataTrain);
			break;
		case 1558:
			_ColonyObj = new ColonyPPFusion(this);
			RPCServer(EPacketType.PT_CL_InitDataPPCoal);
			break;
		default:
			Debug.LogError("ColonySystem itemid is wrong id = " + base.ExternId);
			break;
		}
		RPCServer(EPacketType.PT_CL_InitData);
	}

	protected override void OnPEStart()
	{
		BindSkAction();
		BindAction(EPacketType.PT_CL_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_CL_Turn, RPC_S2C_Turn);
		BindAction(EPacketType.PT_CL_SyncItem, RPC_S2C_SyncItem);
		BindAction(EPacketType.PT_CL_SyncCreationFuel, RPC_S2C_SyncCreationFuel);
		BindAction(EPacketType.PT_CL_SyncCreationHP, RPC_S2C_SyncCreationHP);
		BindAction(EPacketType.PT_CL_RepairStart, RPC_S2C_RepairStart);
		BindAction(EPacketType.PT_CL_RepairEnd, RPC_S2C_RepairEnd);
		BindAction(EPacketType.PT_CL_BeginRecycle, RPC_S2C_BeginRecycle);
		BindAction(EPacketType.PT_CL_EndRecycle, RPC_S2C_EndRecycle);
		BindAction(EPacketType.PT_CL_SyncColonyDurability, RPC_S2C_SyncColonyDurability);
		BindAction(EPacketType.PT_CL_RemoveColonyEntity, RPC_S2C_RemoveColonyEntity);
		BindAction(EPacketType.PT_CL_InitDataAssembly, RPC_S2C_InitDataAssembly);
		BindAction(EPacketType.PT_CL_ASB_LevelUp, RPC_S2C_ASB_LevelUp);
		BindAction(EPacketType.PT_CL_ASB_LevelUpStart, RPC_S2C_ASB_LevelUpStart);
		BindAction(EPacketType.PT_CL_ASB_QueryTime, RPC_S2C_ASB_QueryTime);
		BindAction(EPacketType.PT_CL_ASB_HideShield, RPC_S2C_ASB_HideShield);
		BindAction(EPacketType.PT_CL_ASB_ShowTip, RPC_S2C_ASB_ShowTips);
		BindAction(EPacketType.PT_CL_Counter_Tick, RPC_S2C_CounterTick);
		BindAction(EPacketType.PT_CL_InitDataDwellings, RPC_S2C_InitDataDwellings);
		BindAction(EPacketType.PT_CL_DWL_SyncNpc, RPC_S2C_DWL_SyncNpc);
		BindAction(EPacketType.PT_CL_InitDataEnhance, RPC_S2C_InitDataEnhance);
		BindAction(EPacketType.PT_CL_EHN_SetItem, RPC_S2C_EHN_SetItem);
		BindAction(EPacketType.PT_CL_EHN_Fetch, RPC_S2C_EHN_Fetch);
		BindAction(EPacketType.PT_CL_EHN_Start, RPC_S2C_EHN_Start);
		BindAction(EPacketType.PT_CL_EHN_Stop, RPC_S2C_EHN_Stop);
		BindAction(EPacketType.PT_CL_EHN_End, RPC_S2C_EHN_End);
		BindAction(EPacketType.PT_CL_EHN_SyncTime, RPC_S2C_EHN_SyncTime);
		BindAction(EPacketType.PT_CL_InitDataFactory, RPC_S2C_InitDataFactory);
		BindAction(EPacketType.PT_CL_FCT_IsReady, RPC_S2C_FCT_IsReady);
		BindAction(EPacketType.PT_CL_FCT_AddCompoudList, RPC_S2C_FCT_AddCompoudList);
		BindAction(EPacketType.PT_CL_FCT_RemoveCompoudList, RPC_S2C_FCT_RemoveCompoudList);
		BindAction(EPacketType.PT_CL_FCT_SyncItem, RPC_S2C_FCT_SyncItem);
		BindAction(EPacketType.PT_CL_FCT_Fetch, RPC_S2C_FCT_Fetch);
		BindAction(EPacketType.PT_CL_FCT_Compoud, RPC_S2C_FCT_Compoud);
		BindAction(EPacketType.PT_CL_FCT_SyncAllItems, RPC_S2C_FCT_SyncAllItems);
		BindAction(EPacketType.PT_CL_FCT_GenFactoryCancel, RPC_S2C_FCT_GenFactoryCancel);
		BindAction(EPacketType.PT_CL_InitDataFarm, RPC_S2C_InitDataFarm);
		BindAction(EPacketType.PT_CL_FARM_SetPlantSeed, RPC_S2C_FARM_SetPlantSeed);
		BindAction(EPacketType.PT_CL_FARM_SetPlantTool, RPC_C2S_FARM_SetPlantTool);
		BindAction(EPacketType.PT_CL_FARM_SetSequentialActive, RPC_S2C_FARM_SetSequentialActive);
		BindAction(EPacketType.PT_CL_FARM_SetAutoPlanting, RPC_S2C_FARM_SetAutoPlanting);
		BindAction(EPacketType.PT_CL_FARM_FetchSeedItem, RPC_S2C_FARM_FetchSeedItemResult);
		BindAction(EPacketType.PT_CL_FARM_FetchToolItem, RPC_S2C_FARM_FetchToolItemResult);
		BindAction(EPacketType.PT_CL_FARM_DeleteSeed, RPC_S2C_FARM_DeleteSeed);
		BindAction(EPacketType.PT_CL_FARM_DeletePlantTool, RPC_S2C_FARM_DeletePlantTool);
		BindAction(EPacketType.PT_CL_FARM_RestoreWater, RPC_S2C_FARM_RestoreWater);
		BindAction(EPacketType.PT_CL_FARM_RestoreClean, RPC_S2C_FARM_RestoreClean);
		BindAction(EPacketType.PT_CL_FARM_RestoreGetBack, RPC_S2C_FARM_RestoreGetBack);
		BindAction(EPacketType.PT_CL_InitDataPowerPlanet, RPC_S2C_InitDataPowerPlanet);
		BindAction(EPacketType.PT_CL_POW_AddChargItem, RPC_S2C_POW_AddChargItem);
		BindAction(EPacketType.PT_CL_POW_GetChargItem, RPC_S2C_POW_GetChargItem);
		BindAction(EPacketType.PT_CL_InitDataPPCoal, RPC_S2C_InitDataPPCoal);
		BindAction(EPacketType.PT_CL_PPC_AddFuel, RPC_S2C_PPC_AddFuel);
		BindAction(EPacketType.PT_CL_PPC_WorkedTime, RPC_S2C_PPC_WorkedTime);
		BindAction(EPacketType.PT_CL_PPC_NoPower, RPC_S2C_PPC_NoPower);
		BindAction(EPacketType.PT_CL_InitDataRecycle, RPC_S2C_InitDataRecycle);
		BindAction(EPacketType.PT_CL_RCY_SetItem, RPC_S2C_RCY_SetItem);
		BindAction(EPacketType.PT_CL_RCY_Start, RPC_S2C_RCY_Start);
		BindAction(EPacketType.PT_CL_RCY_Stop, RPC_S2C_RCY_Stop);
		BindAction(EPacketType.PT_CL_RCY_FetchMaterial, RPC_S2C_RCY_FetchMaterial);
		BindAction(EPacketType.PT_CL_RCY_FetchItem, RPC_S2C_RCY_FetchItem);
		BindAction(EPacketType.PT_CL_RCY_SyncTime, RPC_S2C_RCY_SyncTime);
		BindAction(EPacketType.PT_CL_RCY_SyncRecycleItem, RPC_S2C_RCY_SyncRecycleItem);
		BindAction(EPacketType.PT_CL_RCY_End, RPC_S2C_RCY_End);
		BindAction(EPacketType.PT_CL_RCY_MatsToStorage, RPC_S2C_RCY_MatsToStorage);
		BindAction(EPacketType.PT_CL_RCY_MatsToResult, RPC_S2C_RCY_MatsToResult);
		BindAction(EPacketType.PT_CL_InitDataStorage, RPC_S2C_InitDataStorage);
		BindAction(EPacketType.PT_CL_STO_Delete, RPC_S2C_STO_Delete);
		BindAction(EPacketType.PT_CL_STO_Store, RPC_S2C_STO_Store);
		BindAction(EPacketType.PT_CL_STO_Fetch, RPC_S2C_STO_FetchItem);
		BindAction(EPacketType.PT_CL_STO_Exchange, RPC_S2C_STO_Exchange);
		BindAction(EPacketType.PT_CL_STO_Split, RPC_S2C_STO_Split);
		BindAction(EPacketType.PT_CL_STO_Sort, RPC_S2C_STO_Sort);
		BindAction(EPacketType.PT_CL_STO_SyncItemList, RPC_S2C_STO_SyncItemList);
		BindAction(EPacketType.PT_CL_InitDataRepair, RPC_S2C_InitDataRepair);
		BindAction(EPacketType.PT_CL_RPA_SetItem, RPC_S2C_RPA_SetItem);
		BindAction(EPacketType.PT_CL_RPA_Start, RPC_S2C_RPA_Start);
		BindAction(EPacketType.PT_CL_RPA_Stop, RPC_S2C_RPA_Stop);
		BindAction(EPacketType.PT_CL_RPA_End, RPC_S2C_RPA_End);
		BindAction(EPacketType.PT_CL_RPA_FetchItem, RPC_S2C_RPA_FetchItem);
		BindAction(EPacketType.PT_CL_RPA_SyncTime, RPC_S2C_RPA_SyncTime);
		BindAction(EPacketType.PT_CL_InitDataProcessing, RPC_S2C_InitDataProcessing);
		BindAction(EPacketType.PT_CL_PRC_AddItem, RPC_S2C_PRC_AddItem);
		BindAction(EPacketType.PT_CL_PRC_RemoveItem, RPC_S2C_PRC_RemoveItem);
		BindAction(EPacketType.PT_CL_PRC_AddNpc, RPC_S2C_PRC_AddNpc);
		BindAction(EPacketType.PT_CL_PRC_RemoveNpc, RPC_S2C_PRC_RemoveNpc);
		BindAction(EPacketType.PT_CL_PRC_SetRound, RPC_S2C_PRC_SetRound);
		BindAction(EPacketType.PT_CL_PRC_SetAuto, RPC_S2C_PRC_SetAuto);
		BindAction(EPacketType.PT_CL_PRC_Start, RPC_S2C_PRC_StartTask);
		BindAction(EPacketType.PT_CL_PRC_Stop, RPC_S2C_PRC_StopTask);
		BindAction(EPacketType.PT_CL_PRC_GenResult, RPC_S2C_PRC_GenResult);
		BindAction(EPacketType.PT_CL_PRC_FinishToStorage, RPC_S2C_PRC_FinishToStorage);
		BindAction(EPacketType.PT_CL_PRC_SyncAllCounter, RPC_S2C_PRC_SyncAllCounter);
		BindAction(EPacketType.PT_CL_InitDataTrade, RPC_S2C_InitDataTrade);
		BindAction(EPacketType.PT_CL_TRD_BuyItem, RPC_S2C_BuyItem);
		BindAction(EPacketType.PT_CL_TRD_SellItem, RPC_S2C_SellItem);
		BindAction(EPacketType.PT_CL_TRD_RepurchaseItem, RPC_S2C_RepurchaseItem);
		BindAction(EPacketType.PT_CL_TRD_UpdateBuyItem, RPC_S2C_UpdateBuyItem);
		BindAction(EPacketType.PT_CL_TRD_UpdateRepurchaseItem, RPC_S2C_UpdateRepurchaseItem);
		BindAction(EPacketType.PT_CL_TRD_UpdateMoney, RPC_S2C_UpdateMoney);
		BindAction(EPacketType.PT_CL_InitDataCheck, RPC_S2C_InitDataCheck);
		BindAction(EPacketType.PT_CL_CHK_FindMachine, RPC_S2C_CHK_FindMachine);
		BindAction(EPacketType.PT_CL_CHK_SetDiagnose, RPC_S2C_CHK_SetDiagnose);
		BindAction(EPacketType.PT_CL_CHK_TryStart, RPC_S2C_CHK_TryStart);
		BindAction(EPacketType.PT_CL_CHK_RemoveDeadNpc, RPC_S2C_CHK_RemoveDeadNpc);
		BindAction(EPacketType.PT_CL_CHK_CheckFinish, RPC_S2C_CHK_CheckFinish);
		BindAction(EPacketType.PT_CL_InitDataTreat, RPC_S2C_InitDataTreat);
		BindAction(EPacketType.PT_CL_TRT_FindMachine, RPC_S2C_TRT_FindMachine);
		BindAction(EPacketType.PT_CL_TRT_SetTreat, RPC_S2C_TRT_SetTreat);
		BindAction(EPacketType.PT_CL_TRT_TryStart, RPC_S2C_TRT_TryStart);
		BindAction(EPacketType.PT_CL_TRT_StartTreatCounter, RPC_S2C_TRT_StartTreatCounter);
		BindAction(EPacketType.PT_CL_TRT_SetItem, RPC_S2C_TRT_SetItem);
		BindAction(EPacketType.PT_CL_TRT_DeleteItem, RPC_S2C_TRT_DeleteItem);
		BindAction(EPacketType.PT_CL_TRT_RemoveDeadNpc, RPC_S2C_TRT_RemoveDeadNpc);
		BindAction(EPacketType.PT_CL_TRT_TreatFinish, RPC_S2C_TRT_TreatFinish);
		BindAction(EPacketType.PT_CL_TRT_ResetNpcToCheck, RPC_S2C_TRT_ResetNpcToCheck);
		BindAction(EPacketType.PT_CL_InitDataTent, RPC_S2C_InitDataTent);
		BindAction(EPacketType.PT_CL_TET_FindMachine, RPC_S2C_TET_FindMachine);
		BindAction(EPacketType.PT_CL_TET_TryStart, RPC_S2C_TET_TryStart);
		BindAction(EPacketType.PT_CL_TET_SetTent, RPC_S2C_TET_SetTent);
		BindAction(EPacketType.PT_CL_TET_RemoveDeadNpc, RPC_S2C_TET_RemoveDeadNpc);
		BindAction(EPacketType.PT_CL_TET_TentFinish, RPC_S2C_TET_TentFinish);
		BindAction(EPacketType.PT_CL_InitDataTrain, RPC_S2C_InitDataTrain);
		BindAction(EPacketType.PT_CL_TRN_StartSkillTraining, RPC_S2C_TRN_StartSkillTraining);
		BindAction(EPacketType.PT_CL_TRN_StartAttributeTraining, RPC_S2C_TRN_StartAttributeTraining);
		BindAction(EPacketType.PT_CL_TRN_SetInstructor, RPC_S2C_TRN_SetInstructor);
		BindAction(EPacketType.PT_CL_TRN_SetTrainee, RPC_S2C_TRN_SetTrainee);
		BindAction(EPacketType.PT_CL_TRN_SkillTrainFinish, RPC_S2C_TRN_SkillTrainFinish);
		BindAction(EPacketType.PT_CL_TRN_AttributeTrainFinish, RPC_S2C_TRN_AttributeTrainFinish);
		BindAction(EPacketType.PT_CL_TRN_StopTraining, RPC_S2C_TRN_StopTraining);
		BindAction(EPacketType.PT_CL_TRN_SyncCounter, RPC_S2C_TRN_SyncCounter);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
	}

	protected override void OnPEDestroy()
	{
		StopAllCoroutines();
		int externId = base.ExternId;
		if (externId == 1127)
		{
			PlayerNetwork.OnTeamChangedEventHandler -= OnTeamChange;
			PlayerNetwork.OnLimitBoundsDel(base.Id);
		}
		DragArticleAgent.Destory(base.Id);
		if (null != base.Runner)
		{
			Object.Destroy(base.Runner.gameObject);
		}
	}

	protected override IEnumerator SyncMove()
	{
		yield break;
	}

	public override void InitForceData()
	{
		if (null != _entity)
		{
			_entity.SetAttribute(AttribType.DefaultPlayerID, base.TeamId, offEvent: false);
			_entity.SetAttribute(AttribType.CampID, base.TeamId, offEvent: false);
			_entity.SetAttribute(AttribType.DamageID, base.TeamId, offEvent: false);
		}
	}

	protected override void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ItemObject itemObject = stream.Read<ItemObject>(new object[0]);
		float y = stream.Read<float>(new object[0]);
		Quaternion rotation = Quaternion.Euler(0f, y, 0f);
		base.transform.rotation = rotation;
		base.rot = rotation;
		_ColonyObj._RecordData.m_Position = base.transform.position;
		_ColonyObj._RecordData.ItemID = base.ExternId;
		_ColonyObj._RecordData.ID = base.Id;
		MultiColonyManager.Instance.AddDataToCreator(this, base.TeamId);
		if (itemObject == null)
		{
			return;
		}
		Drag cmpt = itemObject.GetCmpt<Drag>();
		if (cmpt == null)
		{
			return;
		}
		DragArticleAgent dragArticleAgent = DragArticleAgent.Create(cmpt, base.transform.position, base.transform.localScale, base.transform.rotation, base.Id, this);
		if (dragArticleAgent.itemLogic != null)
		{
			CSBuildingLogic cSBuildingLogic = dragArticleAgent.itemLogic as CSBuildingLogic;
			if (cSBuildingLogic != null)
			{
				cSBuildingLogic.InitInMultiMode(m_Entity, _ownerId);
				OnTeamChange();
				_entity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
				OnSpawned(cSBuildingLogic.gameObject);
			}
		}
	}

	protected override void RPC_S2C_Turn(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.transform.rotation = stream.Read<Quaternion>(new object[0]);
		CSBuildingLogic component = base.Runner.GetComponent<CSBuildingLogic>();
		if (component != null)
		{
			DragItemAgent byId = DragItemAgent.GetById(component.id);
			byId.rotation = base.transform.rotation;
		}
		if (null != base.Runner)
		{
			CSEntityObject componentInChildren = base.Runner.GetComponentInChildren<CSEntityObject>();
			if (componentInChildren != null && component != null)
			{
				componentInChildren.Init(component, componentInChildren.m_Creator, bFight: false);
			}
			DragItemMousePickColony componentInChildren2 = base.Runner.GetComponentInChildren<DragItemMousePickColony>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.OnItemOpGUIHide();
			}
		}
	}

	private void RPC_S2C_SyncItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	private void RPC_S2C_SyncCreationFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		float current = stream.Read<float>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		Energy cmpt = itemObject.GetCmpt<Energy>();
		cmpt.floatValue.current = current;
	}

	private void RPC_S2C_SyncCreationHP(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		float current = stream.Read<float>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		LifeLimit cmpt = itemObject.GetCmpt<LifeLimit>();
		cmpt.floatValue.current = current;
	}

	private void RPC_S2C_RepairStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_ColonyObj._RecordData.m_CurRepairTime = stream.Read<float>(new object[0]);
		_ColonyObj._RecordData.m_RepairTime = stream.Read<float>(new object[0]);
		_ColonyObj._RecordData.m_RepairValue = stream.Read<float>(new object[0]);
		m_Entity.StartRepairCounter(_ColonyObj._RecordData.m_CurRepairTime, _ColonyObj._RecordData.m_RepairTime, _ColonyObj._RecordData.m_RepairValue);
		CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Entity.Name));
	}

	private void RPC_S2C_RepairEnd(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_ColonyObj._RecordData.m_Durability = stream.Read<float>(new object[0]);
		_ColonyObj._RecordData.m_RepairTime = -1f;
		_ColonyObj._RecordData.m_RepairValue = 0f;
	}

	private void RPC_S2C_BeginRecycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_ColonyObj._RecordData.m_DeleteTime = stream.Read<float>(new object[0]);
		m_Entity.StartDeleteCounter(0f, _ColonyObj._RecordData.m_DeleteTime);
		CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToDelete.GetString(), m_Entity.Name));
	}

	private void RPC_S2C_EndRecycle(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = stream.Read<bool>(new object[0]);
		_ColonyObj._RecordData.m_CurDeleteTime = -1f;
		_ColonyObj._RecordData.m_DeleteTime = -1f;
		if (flag)
		{
			m_Entity.m_Creator.RemoveEntity(base.Id);
		}
	}

	private void RPC_S2C_SyncColonyDurability(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float durability = stream.Read<float>(new object[0]);
		_ColonyObj._RecordData.m_Durability = durability;
	}

	private void RPC_S2C_RemoveColonyEntity(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		if (BelongToOwner)
		{
			CSMain.s_MgCreator.RemoveEntity(id, bRemoveData: false);
		}
		else
		{
			MultiColonyManager.GetCreator(base.TeamId).RemoveEntity(id, bRemoveData: false);
		}
	}
}
