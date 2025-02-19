using System.Collections.Generic;
using System.IO;
using CSRecord;
using CustomData;
using UnityEngine;

public class CSDataInst
{
	private bool debugSwitch;

	public int m_ID;

	public CSDataMonsterSiege m_Siege;

	public Dictionary<int, CSDefaultData> m_ObjectDatas;

	public Dictionary<int, CSPersonnelData> m_PersonnelDatas;

	public List<CSTreatment> treatmentList = new List<CSTreatment>();

	public List<int> addedTradeNpc = new List<int>();

	public List<int> addedStoreId = new List<int>();

	public int colonyMoney = 5000;

	public CSConst.CreatorType m_Type;

	public CSDataInst()
	{
		m_Siege = new CSDataMonsterSiege();
		m_ObjectDatas = new Dictionary<int, CSDefaultData>();
		m_PersonnelDatas = new Dictionary<int, CSPersonnelData>();
	}

	public bool AddData(CSPersonnelData data)
	{
		if (m_PersonnelDatas.ContainsKey(data.ID))
		{
			return false;
		}
		m_PersonnelDatas.Add(data.ID, data);
		return true;
	}

	public bool AssignData(int id, int type, ref CSDefaultData refData)
	{
		if (type == 50)
		{
			if (m_PersonnelDatas.ContainsKey(id))
			{
				Debug.Log("The Personnel Data ID [" + id + "] is exist.");
				refData = m_PersonnelDatas[id];
				return false;
			}
			refData = new CSPersonnelData();
			refData.ID = id;
			m_PersonnelDatas.Add(id, refData as CSPersonnelData);
			return true;
		}
		if (m_ObjectDatas.ContainsKey(id))
		{
			refData = m_ObjectDatas[id];
			return false;
		}
		switch (type)
		{
		case 1:
			refData = new CSAssemblyData();
			break;
		case 2:
			refData = new CSStorageData();
			break;
		case 3:
			refData = new CSEngineerData();
			break;
		case 4:
			refData = new CSEnhanceData();
			break;
		case 5:
			refData = new CSRepairData();
			break;
		case 6:
			refData = new CSRecycleData();
			break;
		case 33:
			refData = new CSPPCoalData();
			break;
		case 34:
			refData = new CSPPSolarData();
			break;
		case 21:
			refData = new CSDwellingsData();
			break;
		case 7:
			refData = new CSFarmData();
			break;
		case 8:
			refData = new CSFactoryData();
			break;
		case 9:
			refData = new CSProcessingData();
			break;
		case 10:
			refData = new CSTradeData();
			break;
		case 11:
			refData = new CSTrainData();
			break;
		case 12:
			refData = new CSCheckData();
			break;
		case 13:
			refData = new CSTreatData();
			break;
		case 14:
			refData = new CSTentData();
			break;
		case 35:
			refData = new CSPPFusionData();
			break;
		default:
			refData = new CSDefaultData();
			break;
		}
		refData.ID = id;
		m_ObjectDatas.Add(id, refData);
		return true;
	}

	public void RemoveData(int id, int type)
	{
		if (type == 50)
		{
			RemovePersonnelData(id);
		}
		else
		{
			RemoveObjectData(id);
		}
	}

	public void RemoveObjectData(int id)
	{
		if (!m_ObjectDatas.ContainsKey(id))
		{
			Debug.LogWarning("You want to remove a object data, but it not exist!");
		}
		else
		{
			m_ObjectDatas.Remove(id);
		}
	}

	public void RemovePersonnelData(int id)
	{
		if (!m_PersonnelDatas.ContainsKey(id))
		{
			Debug.LogWarning("You want to remove a Personnel data, but it not exist!");
		}
		else
		{
			m_PersonnelDatas.Remove(id);
		}
	}

	public Dictionary<int, CSDefaultData> GetObjectRecords()
	{
		return m_ObjectDatas;
	}

	public Dictionary<int, CSPersonnelData>.ValueCollection GetPersonnelRecords()
	{
		return m_PersonnelDatas.Values;
	}

	public void ClearData()
	{
		m_ObjectDatas.Clear();
		m_PersonnelDatas.Clear();
		treatmentList.Clear();
		addedTradeNpc.Clear();
		addedStoreId.Clear();
	}

	public bool Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num >= 15111720)
		{
			m_Siege.Import(r);
		}
		if (num >= 15042418)
		{
			m_Type = (CSConst.CreatorType)r.ReadInt32();
			if (debugSwitch)
			{
				Debug.Log(string.Concat("<color=yellow>m_Type: ", m_Type, "</color>"));
			}
			int num2 = r.ReadInt32();
			if (debugSwitch)
			{
				Debug.Log("<color=yellow>rcnt: " + num2 + "</color>");
			}
			for (int i = 0; i < num2; i++)
			{
				CSDefaultData cSDefaultData = null;
				int num3 = r.ReadInt32();
				if (debugSwitch)
				{
					Debug.Log("<color=yellow>type: " + num3 + "</color>");
				}
				switch (num3)
				{
				case 1:
					cSDefaultData = _readAssemblyData(r, num);
					break;
				case 33:
					cSDefaultData = _readPPCoalData(r, num);
					break;
				case 34:
					cSDefaultData = _readPPSolarData(r, num);
					break;
				case 2:
					cSDefaultData = _readStorageData(r, num);
					break;
				case 4:
					cSDefaultData = _readEnhanceData(r, num);
					break;
				case 5:
					cSDefaultData = _readRepairData(r, num);
					break;
				case 6:
					cSDefaultData = _readRecycleData(r, num);
					break;
				case 21:
					cSDefaultData = _readDwellingsData(r, num);
					break;
				case 7:
					cSDefaultData = _readFarmData(r, num);
					break;
				case 8:
					cSDefaultData = _readFactoryData(r, num);
					break;
				case 9:
					cSDefaultData = _readProcessingData(r, num);
					break;
				case 10:
					cSDefaultData = _readTradeData(r, num);
					break;
				case 11:
					cSDefaultData = _readTrainData(r, num);
					break;
				case 12:
					cSDefaultData = _readCheckData(r, num);
					break;
				case 13:
					cSDefaultData = _readTreatData(r, num);
					break;
				case 14:
					cSDefaultData = _readTentData(r, num);
					break;
				case 35:
					cSDefaultData = _readFusionData(r, num);
					break;
				}
				if (cSDefaultData != null)
				{
					m_ObjectDatas.Add(cSDefaultData.ID, cSDefaultData);
				}
			}
			_readPesonnelData(r, num);
			switch (num)
			{
			default:
				if (num >= 15091800)
				{
					int num4 = r.ReadInt32();
					for (int j = 0; j < num4; j++)
					{
						treatmentList.Add(CSTreatment._readTreatmentData(r, num));
					}
				}
				if (num >= 16101900)
				{
					int num5 = r.ReadInt32();
					for (int k = 0; k < num5; k++)
					{
						addedTradeNpc.Add(r.ReadInt32());
					}
					int num6 = r.ReadInt32();
					for (int l = 0; l < num6; l++)
					{
						addedStoreId.Add(r.ReadInt32());
					}
				}
				if (num >= 16102100)
				{
					colonyMoney = r.ReadInt32();
				}
				return true;
			}
		}
		return false;
	}

	public void Export(BinaryWriter w)
	{
		w.Write(16102100);
		if (debugSwitch)
		{
			Debug.Log("<color=yellow>version: " + 15042418 + "</color>");
		}
		m_Siege.Export(w);
		w.Write((int)m_Type);
		if (debugSwitch)
		{
			Debug.Log(string.Concat("<color=yellow>m_Type: ", m_Type, "</color>"));
		}
		w.Write(m_ObjectDatas.Count);
		if (debugSwitch)
		{
			Debug.Log("<color=yellow>m_ObjectDatas.Count: " + m_ObjectDatas.Count + "</color>");
		}
		foreach (KeyValuePair<int, CSDefaultData> objectData in m_ObjectDatas)
		{
			w.Write(objectData.Value.dType);
			if (debugSwitch)
			{
				Debug.Log("<color=yellow>dType: " + objectData.Value.dType + "</color>");
			}
			switch (objectData.Value.dType)
			{
			case 1:
			{
				CSAssemblyData cSAssemblyData = objectData.Value as CSAssemblyData;
				_writeCSObjectData(w, cSAssemblyData);
				w.Write(cSAssemblyData.m_ShowShield);
				w.Write(cSAssemblyData.m_Level);
				w.Write(cSAssemblyData.m_UpgradeTime);
				w.Write(cSAssemblyData.m_CurUpgradeTime);
				w.Write(cSAssemblyData.m_TimeTicks);
				w.Write(cSAssemblyData.m_MedicineResearchState);
				w.Write(cSAssemblyData.m_MedicineResearchTimes);
				break;
			}
			case 33:
			{
				CSPPCoalData cSPPCoalData = objectData.Value as CSPPCoalData;
				_writeCSObjectData(w, cSPPCoalData);
				w.Write(cSPPCoalData.bShowElectric);
				w.Write(cSPPCoalData.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> chargingItem in cSPPCoalData.m_ChargingItems)
				{
					w.Write(chargingItem.Key);
					w.Write(chargingItem.Value);
				}
				w.Write(cSPPCoalData.m_CurWorkedTime);
				w.Write(cSPPCoalData.m_WorkedTime);
				break;
			}
			case 35:
			{
				CSPPFusionData cSPPFusionData = objectData.Value as CSPPFusionData;
				_writeCSObjectData(w, cSPPFusionData);
				w.Write(cSPPFusionData.bShowElectric);
				w.Write(cSPPFusionData.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> chargingItem2 in cSPPFusionData.m_ChargingItems)
				{
					w.Write(chargingItem2.Key);
					w.Write(chargingItem2.Value);
				}
				w.Write(cSPPFusionData.m_CurWorkedTime);
				w.Write(cSPPFusionData.m_WorkedTime);
				break;
			}
			case 34:
			{
				CSPowerPlanetData cSPowerPlanetData = objectData.Value as CSPowerPlanetData;
				_writeCSObjectData(w, cSPowerPlanetData);
				w.Write(cSPowerPlanetData.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> chargingItem3 in cSPowerPlanetData.m_ChargingItems)
				{
					w.Write(chargingItem3.Key);
					w.Write(chargingItem3.Value);
				}
				break;
			}
			case 2:
			{
				CSStorageData cSStorageData = objectData.Value as CSStorageData;
				_writeCSObjectData(w, cSStorageData);
				w.Write(cSStorageData.m_Items.Count);
				foreach (KeyValuePair<int, int> item in cSStorageData.m_Items)
				{
					w.Write(item.Key);
					w.Write(item.Value);
				}
				break;
			}
			case 3:
			{
				CSEngineerData cSEngineerData = objectData.Value as CSEngineerData;
				_writeCSObjectData(w, cSEngineerData);
				w.Write(cSEngineerData.m_EnhanceItemID);
				w.Write(cSEngineerData.m_CurEnhanceTime);
				w.Write(cSEngineerData.m_EnhanceTime);
				w.Write(cSEngineerData.m_PatchItemID);
				w.Write(cSEngineerData.m_CurPatchTime);
				w.Write(cSEngineerData.m_PatchTime);
				w.Write(cSEngineerData.m_RecycleItemID);
				w.Write(cSEngineerData.m_CurRecycleTime);
				w.Write(cSEngineerData.m_RecycleTime);
				break;
			}
			case 4:
			{
				CSEnhanceData cSEnhanceData = objectData.Value as CSEnhanceData;
				_writeCSObjectData(w, cSEnhanceData);
				w.Write(cSEnhanceData.m_ObjID);
				w.Write(cSEnhanceData.m_CurTime);
				w.Write(cSEnhanceData.m_Time);
				break;
			}
			case 5:
			{
				CSRepairData cSRepairData = objectData.Value as CSRepairData;
				_writeCSObjectData(w, cSRepairData);
				w.Write(cSRepairData.m_ObjID);
				w.Write(cSRepairData.m_CurTime);
				w.Write(cSRepairData.m_Time);
				break;
			}
			case 6:
			{
				CSRecycleData cSRecycleData = objectData.Value as CSRecycleData;
				_writeCSObjectData(w, cSRecycleData);
				w.Write(cSRecycleData.m_ObjID);
				w.Write(cSRecycleData.m_CurTime);
				w.Write(cSRecycleData.m_Time);
				break;
			}
			case 21:
			{
				CSDwellingsData csod = objectData.Value as CSDwellingsData;
				_writeCSObjectData(w, csod);
				break;
			}
			case 7:
			{
				CSFarmData cSFarmData = objectData.Value as CSFarmData;
				_writeCSObjectData(w, cSFarmData);
				w.Write(cSFarmData.m_PlantSeeds.Count);
				foreach (KeyValuePair<int, int> plantSeed in cSFarmData.m_PlantSeeds)
				{
					w.Write(plantSeed.Key);
					w.Write(plantSeed.Value);
				}
				w.Write(cSFarmData.m_Tools.Count);
				foreach (KeyValuePair<int, int> tool in cSFarmData.m_Tools)
				{
					w.Write(tool.Key);
					w.Write(tool.Value);
				}
				w.Write(cSFarmData.m_AutoPlanting);
				w.Write(cSFarmData.m_SequentialPlanting);
				break;
			}
			case 8:
			{
				CSFactoryData cSFactoryData = objectData.Value as CSFactoryData;
				_writeCSObjectData(w, cSFactoryData);
				w.Write(cSFactoryData.m_CompoudItems.Count);
				foreach (CompoudItem compoudItem in cSFactoryData.m_CompoudItems)
				{
					w.Write(compoudItem.curTime);
					w.Write(compoudItem.time);
					w.Write(compoudItem.itemID);
					w.Write(compoudItem.itemCnt);
				}
				break;
			}
			case 9:
			{
				CSProcessingData cSProcessingData = objectData.Value as CSProcessingData;
				_writeCSObjectData(w, cSProcessingData);
				w.Write(cSProcessingData.isAuto);
				w.Write(cSProcessingData.TaskCount);
				for (int i = 0; i < 4; i++)
				{
					if (cSProcessingData.HasLine(i))
					{
						ProcessingTask processingTask = cSProcessingData.mTaskTable[i];
						w.Write(i);
						List<ItemIdCount> itemList = processingTask.itemList;
						int count = itemList.Count;
						w.Write(count);
						for (int j = 0; j < itemList.Count; j++)
						{
							w.Write(itemList[j].protoId);
							w.Write(itemList[j].count);
						}
						w.Write(processingTask.runCount);
						w.Write(processingTask.m_CurTime);
						w.Write(processingTask.m_Time);
					}
				}
				break;
			}
			case 10:
			{
				CSTradeData cSTradeData = objectData.Value as CSTradeData;
				_writeCSObjectData(w, cSTradeData);
				w.Write(cSTradeData.mShopList.Count);
				foreach (KeyValuePair<int, stShopData> mShop in cSTradeData.mShopList)
				{
					w.Write(mShop.Key);
					w.Write(mShop.Value.ItemObjID);
					w.Write(mShop.Value.CreateTime);
				}
				break;
			}
			case 12:
			{
				CSCheckData cSCheckData = objectData.Value as CSCheckData;
				_writeCSObjectData(w, cSCheckData);
				_writeCSCheckData(w, cSCheckData);
				break;
			}
			case 13:
			{
				CSTreatData cSTreatData = objectData.Value as CSTreatData;
				_writeCSObjectData(w, cSTreatData);
				_writeCSTreatData(w, cSTreatData);
				break;
			}
			case 14:
			{
				CSTentData cSTentData = objectData.Value as CSTentData;
				_writeCSObjectData(w, cSTentData);
				_writeCSTentData(w, cSTentData);
				break;
			}
			case 11:
			{
				CSTrainData cSTrainData = objectData.Value as CSTrainData;
				_writeCSObjectData(w, cSTrainData);
				_writeCSTrainData(w, cSTrainData);
				break;
			}
			}
		}
		w.Write(m_PersonnelDatas.Count);
		if (debugSwitch)
		{
			Debug.Log("<color=yellow>m_PersonnelDatas.Count: " + m_PersonnelDatas.Count + "</color>");
		}
		foreach (KeyValuePair<int, CSPersonnelData> personnelData in m_PersonnelDatas)
		{
			w.Write(personnelData.Value.ID);
			w.Write(personnelData.Value.dType);
			w.Write(personnelData.Value.m_State);
			w.Write(personnelData.Value.m_DwellingsID);
			w.Write(personnelData.Value.m_WorkRoomID);
			w.Write(personnelData.Value.m_Occupation);
			w.Write(personnelData.Value.m_WorkMode);
			w.Write(personnelData.Value.m_GuardPos.x);
			w.Write(personnelData.Value.m_GuardPos.y);
			w.Write(personnelData.Value.m_GuardPos.z);
			w.Write(personnelData.Value.m_ProcessingIndex);
			w.Write(personnelData.Value.m_IsProcessing);
			w.Write(personnelData.Value.m_TrainerType);
			w.Write(personnelData.Value.m_TrainingType);
			w.Write(personnelData.Value.m_IsTraining);
		}
		w.Write(treatmentList.Count);
		for (int k = 0; k < treatmentList.Count; k++)
		{
			treatmentList[k]._writeTreatmentData(w);
		}
		w.Write(addedTradeNpc.Count);
		for (int l = 0; l < addedTradeNpc.Count; l++)
		{
			w.Write(addedTradeNpc[l]);
		}
		w.Write(addedStoreId.Count);
		for (int m = 0; m < addedStoreId.Count; m++)
		{
			w.Write(addedStoreId[m]);
		}
		w.Write(colonyMoney);
	}

	public void _writeCSCheckData(BinaryWriter w, CSCheckData csd)
	{
		w.Write(csd.npcIds.Count);
		for (int i = 0; i < csd.npcIds.Count; i++)
		{
			w.Write(csd.npcIds[i]);
		}
		w.Write(csd.m_CurTime);
		w.Write(csd.m_Time);
		w.Write(csd.isNpcReady);
		w.Write(csd.occupied);
	}

	public void _writeCSTreatData(BinaryWriter w, CSTreatData csd)
	{
		w.Write(csd.npcIds.Count);
		for (int i = 0; i < csd.npcIds.Count; i++)
		{
			w.Write(csd.npcIds[i]);
		}
		w.Write(csd.m_ObjID);
		w.Write(csd.m_CurTime);
		w.Write(csd.m_Time);
		w.Write(csd.isNpcReady);
		w.Write(csd.occupied);
	}

	public void _writeCSTentData(BinaryWriter w, CSTentData csd)
	{
		w.Write(csd.npcIds.Count);
		for (int i = 0; i < csd.npcIds.Count; i++)
		{
			w.Write(csd.npcIds[i]);
		}
		for (int j = 0; j < 8; j++)
		{
			w.Write(csd.allSickbeds[j].npcId);
			w.Write(csd.allSickbeds[j].m_CurTime);
			w.Write(csd.allSickbeds[j].m_Time);
			w.Write(csd.allSickbeds[j].isNpcReady);
			w.Write(csd.allSickbeds[j].IsOccupied);
		}
	}

	public void _writeCSTrainData(BinaryWriter w, CSTrainData csd)
	{
		int count = csd.instructors.Count;
		w.Write(count);
		for (int i = 0; i < count; i++)
		{
			w.Write(csd.instructors[i]);
		}
		int count2 = csd.trainees.Count;
		w.Write(count2);
		for (int j = 0; j < count2; j++)
		{
			w.Write(csd.trainees[j]);
		}
		w.Write(csd.instructorNpcId);
		w.Write(csd.traineeNpcId);
		w.Write(csd.trainingType);
		int count3 = csd.LearningSkillIds.Count;
		w.Write(count3);
		for (int k = 0; k < count3; k++)
		{
			w.Write(csd.LearningSkillIds[k]);
		}
		w.Write(csd.m_CurTime);
		w.Write(csd.m_Time);
	}

	private void _readPesonnelData(BinaryReader r, int version)
	{
		if (version < 15042418)
		{
			return;
		}
		int num = r.ReadInt32();
		if (debugSwitch)
		{
			Debug.Log("<color=yellow>m_PersonnelDatas.Count: " + num + "</color>");
		}
		for (int i = 0; i < num; i++)
		{
			CSPersonnelData cSPersonnelData = new CSPersonnelData();
			cSPersonnelData.ID = r.ReadInt32();
			cSPersonnelData.dType = r.ReadInt32();
			cSPersonnelData.m_State = r.ReadInt32();
			cSPersonnelData.m_DwellingsID = r.ReadInt32();
			cSPersonnelData.m_WorkRoomID = r.ReadInt32();
			cSPersonnelData.m_Occupation = r.ReadInt32();
			cSPersonnelData.m_WorkMode = r.ReadInt32();
			cSPersonnelData.m_GuardPos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			cSPersonnelData.m_ProcessingIndex = r.ReadInt32();
			cSPersonnelData.m_IsProcessing = r.ReadBoolean();
			if (version >= 15110600)
			{
				cSPersonnelData.m_TrainerType = r.ReadInt32();
				cSPersonnelData.m_TrainingType = r.ReadInt32();
				cSPersonnelData.m_IsTraining = r.ReadBoolean();
			}
			m_PersonnelDatas.Add(cSPersonnelData.ID, cSPersonnelData);
		}
	}

	private CSAssemblyData _readAssemblyData(BinaryReader r, int version)
	{
		CSAssemblyData cSAssemblyData = null;
		if (version >= 15042418)
		{
			cSAssemblyData = new CSAssemblyData();
			_readCSObjectData(r, cSAssemblyData, version);
			cSAssemblyData.m_ShowShield = r.ReadBoolean();
			cSAssemblyData.m_Level = r.ReadInt32();
			cSAssemblyData.m_UpgradeTime = r.ReadSingle();
			cSAssemblyData.m_CurUpgradeTime = r.ReadSingle();
			cSAssemblyData.m_TimeTicks = r.ReadInt64();
		}
		if (version >= 16091400)
		{
			cSAssemblyData.m_MedicineResearchState = r.ReadDouble();
			cSAssemblyData.m_MedicineResearchTimes = r.ReadInt32();
		}
		return cSAssemblyData;
	}

	private CSPPCoalData _readPPCoalData(BinaryReader r, int version)
	{
		CSPPCoalData cSPPCoalData = null;
		if (version >= 15042418)
		{
			cSPPCoalData = new CSPPCoalData();
			_readCSObjectData(r, cSPPCoalData, version);
			if (version >= 16052514)
			{
				cSPPCoalData.bShowElectric = r.ReadBoolean();
			}
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSPPCoalData.m_ChargingItems.Add(r.ReadInt32(), r.ReadInt32());
			}
			cSPPCoalData.m_CurWorkedTime = r.ReadSingle();
			cSPPCoalData.m_WorkedTime = r.ReadSingle();
		}
		return cSPPCoalData;
	}

	private CSPPFusionData _readFusionData(BinaryReader r, int version)
	{
		CSPPFusionData cSPPFusionData = null;
		cSPPFusionData = new CSPPFusionData();
		_readCSObjectData(r, cSPPFusionData, version);
		cSPPFusionData.bShowElectric = r.ReadBoolean();
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			cSPPFusionData.m_ChargingItems.Add(r.ReadInt32(), r.ReadInt32());
		}
		cSPPFusionData.m_CurWorkedTime = r.ReadSingle();
		cSPPFusionData.m_WorkedTime = r.ReadSingle();
		return cSPPFusionData;
	}

	private CSPPSolarData _readPPSolarData(BinaryReader r, int version)
	{
		CSPPSolarData cSPPSolarData = null;
		if (version >= 15042418)
		{
			cSPPSolarData = new CSPPSolarData();
			_readCSObjectData(r, cSPPSolarData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSPPSolarData.m_ChargingItems.Add(r.ReadInt32(), r.ReadInt32());
			}
		}
		return cSPPSolarData;
	}

	private CSStorageData _readStorageData(BinaryReader r, int version)
	{
		CSStorageData cSStorageData = null;
		if (version >= 15042418)
		{
			cSStorageData = new CSStorageData();
			_readCSObjectData(r, cSStorageData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSStorageData.m_Items.Add(r.ReadInt32(), r.ReadInt32());
			}
		}
		return cSStorageData;
	}

	private CSEnhanceData _readEnhanceData(BinaryReader r, int version)
	{
		CSEnhanceData cSEnhanceData = null;
		if (version >= 15042418)
		{
			cSEnhanceData = new CSEnhanceData();
			_readCSObjectData(r, cSEnhanceData, version);
			cSEnhanceData.m_ObjID = r.ReadInt32();
			cSEnhanceData.m_CurTime = r.ReadSingle();
			cSEnhanceData.m_Time = r.ReadSingle();
		}
		return cSEnhanceData;
	}

	private CSRepairData _readRepairData(BinaryReader r, int version)
	{
		CSRepairData cSRepairData = null;
		if (version >= 15042418)
		{
			cSRepairData = new CSRepairData();
			_readCSObjectData(r, cSRepairData, version);
			cSRepairData.m_ObjID = r.ReadInt32();
			cSRepairData.m_CurTime = r.ReadSingle();
			cSRepairData.m_Time = r.ReadSingle();
		}
		return cSRepairData;
	}

	private CSRecycleData _readRecycleData(BinaryReader r, int version)
	{
		CSRecycleData cSRecycleData = null;
		if (version >= 15042418)
		{
			cSRecycleData = new CSRecycleData();
			_readCSObjectData(r, cSRecycleData, version);
			cSRecycleData.m_ObjID = r.ReadInt32();
			cSRecycleData.m_CurTime = r.ReadSingle();
			cSRecycleData.m_Time = r.ReadSingle();
		}
		return cSRecycleData;
	}

	private CSDwellingsData _readDwellingsData(BinaryReader r, int version)
	{
		CSDwellingsData cSDwellingsData = null;
		if (version >= 15042418)
		{
			cSDwellingsData = new CSDwellingsData();
			_readCSObjectData(r, cSDwellingsData, version);
		}
		return cSDwellingsData;
	}

	private CSFarmData _readFarmData(BinaryReader r, int version)
	{
		CSFarmData cSFarmData = null;
		if (version >= 15042418)
		{
			cSFarmData = new CSFarmData();
			_readCSObjectData(r, cSFarmData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSFarmData.m_PlantSeeds.Add(r.ReadInt32(), r.ReadInt32());
			}
			num = r.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				cSFarmData.m_Tools.Add(r.ReadInt32(), r.ReadInt32());
			}
			cSFarmData.m_AutoPlanting = r.ReadBoolean();
			cSFarmData.m_SequentialPlanting = r.ReadBoolean();
		}
		return cSFarmData;
	}

	private CSFactoryData _readFactoryData(BinaryReader r, int version)
	{
		CSFactoryData cSFactoryData = null;
		if (version >= 15042418)
		{
			cSFactoryData = new CSFactoryData();
			_readCSObjectData(r, cSFactoryData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				CompoudItem compoudItem = new CompoudItem();
				compoudItem.curTime = r.ReadSingle();
				compoudItem.time = r.ReadSingle();
				compoudItem.itemID = r.ReadInt32();
				compoudItem.itemCnt = r.ReadInt32();
				cSFactoryData.m_CompoudItems.Add(compoudItem);
			}
		}
		return cSFactoryData;
	}

	private CSProcessingData _readProcessingData(BinaryReader r, int version)
	{
		CSProcessingData cSProcessingData = null;
		if (version >= 15042418)
		{
			cSProcessingData = new CSProcessingData();
			_readCSObjectData(r, cSProcessingData, version);
			cSProcessingData.isAuto = r.ReadBoolean();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int num2 = r.ReadInt32();
				ProcessingTask processingTask = new ProcessingTask();
				int num3 = r.ReadInt32();
				for (int j = 0; j < num3; j++)
				{
					ItemIdCount itemIdCount = new ItemIdCount();
					itemIdCount.protoId = r.ReadInt32();
					itemIdCount.count = r.ReadInt32();
					processingTask.itemList.Add(itemIdCount);
				}
				if (version >= 16071518)
				{
					processingTask.runCount = r.ReadInt32();
				}
				processingTask.m_CurTime = r.ReadSingle();
				processingTask.m_Time = r.ReadSingle();
				cSProcessingData.mTaskTable[num2] = processingTask;
			}
		}
		return cSProcessingData;
	}

	private CSTradeData _readTradeData(BinaryReader r, int version)
	{
		CSTradeData cSTradeData = null;
		if (version >= 15042418)
		{
			cSTradeData = new CSTradeData();
			_readCSObjectData(r, cSTradeData, version);
		}
		if (version >= 15091800 && version < 16102000)
		{
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				IntVector2 pos = new IntVector2(r.ReadInt32(), r.ReadInt32());
				TownTradeItemInfo townTradeItemInfo = new TownTradeItemInfo(pos);
				townTradeItemInfo.m_CurTime = r.ReadSingle();
				townTradeItemInfo.m_Time = r.ReadSingle();
				townTradeItemInfo.csti = CSTradeInfoData.GetData(r.ReadInt32());
				int num2 = r.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					townTradeItemInfo.needItems.Add(new TradeObj(r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));
				}
				int num3 = r.ReadInt32();
				for (int k = 0; k < num3; k++)
				{
					townTradeItemInfo.rewardItems.Add(new TradeObj(r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));
				}
			}
		}
		if (version >= 16102000)
		{
			int num4 = r.ReadInt32();
			for (int l = 0; l < num4; l++)
			{
				int key = r.ReadInt32();
				int itemObjId = r.ReadInt32();
				double createTime = r.ReadDouble();
				stShopData value = new stShopData(itemObjId, createTime);
				cSTradeData.mShopList.Add(key, value);
			}
		}
		return cSTradeData;
	}

	private CSTrainData _readTrainData(BinaryReader r, int version)
	{
		CSTrainData cSTrainData = null;
		if (version >= 15110600)
		{
			cSTrainData = new CSTrainData();
			_readCSObjectData(r, cSTrainData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSTrainData.instructors.Add(r.ReadInt32());
			}
			int num2 = r.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				cSTrainData.trainees.Add(r.ReadInt32());
			}
			cSTrainData.instructorNpcId = r.ReadInt32();
			cSTrainData.traineeNpcId = r.ReadInt32();
			cSTrainData.trainingType = r.ReadInt32();
			int num3 = r.ReadInt32();
			for (int k = 0; k < num3; k++)
			{
				cSTrainData.LearningSkillIds.Add(r.ReadInt32());
			}
			cSTrainData.m_CurTime = r.ReadSingle();
			cSTrainData.m_Time = r.ReadSingle();
		}
		return cSTrainData;
	}

	private CSCheckData _readCheckData(BinaryReader r, int version)
	{
		CSCheckData cSCheckData = null;
		if (version >= 15042418)
		{
			cSCheckData = new CSCheckData();
			_readCSObjectData(r, cSCheckData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSCheckData.npcIds.Add(r.ReadInt32());
			}
			cSCheckData.m_CurTime = r.ReadSingle();
			cSCheckData.m_Time = r.ReadSingle();
			cSCheckData.isNpcReady = r.ReadBoolean();
			cSCheckData.occupied = r.ReadBoolean();
		}
		return cSCheckData;
	}

	private CSTreatData _readTreatData(BinaryReader r, int version)
	{
		CSTreatData cSTreatData = null;
		if (version >= 15042418)
		{
			cSTreatData = new CSTreatData();
			_readCSObjectData(r, cSTreatData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSTreatData.npcIds.Add(r.ReadInt32());
			}
			cSTreatData.m_ObjID = r.ReadInt32();
			cSTreatData.m_CurTime = r.ReadSingle();
			cSTreatData.m_Time = r.ReadSingle();
			cSTreatData.isNpcReady = r.ReadBoolean();
			cSTreatData.occupied = r.ReadBoolean();
		}
		return cSTreatData;
	}

	private CSTentData _readTentData(BinaryReader r, int version)
	{
		CSTentData cSTentData = null;
		if (version >= 15042418)
		{
			cSTentData = new CSTentData();
			_readCSObjectData(r, cSTentData, version);
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				cSTentData.npcIds.Add(r.ReadInt32());
			}
			for (int j = 0; j < 8; j++)
			{
				Sickbed sickbed = new Sickbed();
				sickbed.npcId = r.ReadInt32();
				sickbed.m_CurTime = r.ReadSingle();
				sickbed.m_Time = r.ReadSingle();
				sickbed.isNpcReady = r.ReadBoolean();
				sickbed.occupied = r.ReadBoolean();
				cSTentData.allSickbeds[j] = sickbed;
			}
		}
		return cSTentData;
	}

	private void _readCSObjectData(BinaryReader r, CSObjectData csod, int version)
	{
		if (version < 15042418)
		{
			csod.ID = r.ReadInt32();
			csod.ItemID = r.ReadInt32();
			csod.m_Alive = r.ReadBoolean();
			csod.m_Name = r.ReadString();
			csod.m_Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			csod.m_Durability = r.ReadSingle();
			csod.m_CurRepairTime = r.ReadSingle();
			csod.m_RepairTime = r.ReadSingle();
			csod.m_RepairValue = r.ReadSingle();
			csod.m_CurDeleteTime = r.ReadSingle();
			csod.m_DeleteTime = r.ReadSingle();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				csod.m_DeleteGetsItem.Add(r.ReadInt32(), r.ReadInt32());
			}
		}
		if (version >= 15042418)
		{
			csod.ID = r.ReadInt32();
			csod.ItemID = r.ReadInt32();
			csod.m_Alive = r.ReadBoolean();
			csod.m_Name = r.ReadString();
			csod.m_Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			csod.m_Durability = r.ReadSingle();
			csod.m_CurRepairTime = r.ReadSingle();
			csod.m_RepairTime = r.ReadSingle();
			csod.m_RepairValue = r.ReadSingle();
			csod.m_CurDeleteTime = r.ReadSingle();
			csod.m_DeleteTime = r.ReadSingle();
			csod.m_Bounds = new Bounds(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()), new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
			int num2 = r.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				csod.m_DeleteGetsItem.Add(r.ReadInt32(), r.ReadInt32());
			}
		}
	}

	private void _writeCSObjectData(BinaryWriter w, CSObjectData csod)
	{
		w.Write(csod.ID);
		w.Write(csod.ItemID);
		w.Write(csod.m_Alive);
		w.Write(csod.m_Name);
		w.Write(csod.m_Position.x);
		w.Write(csod.m_Position.y);
		w.Write(csod.m_Position.z);
		w.Write(csod.m_Durability);
		w.Write(csod.m_CurRepairTime);
		w.Write(csod.m_RepairTime);
		w.Write(csod.m_RepairValue);
		w.Write(csod.m_CurDeleteTime);
		w.Write(csod.m_DeleteTime);
		w.Write(csod.m_Bounds.center.x);
		w.Write(csod.m_Bounds.center.y);
		w.Write(csod.m_Bounds.center.z);
		w.Write(csod.m_Bounds.size.x);
		w.Write(csod.m_Bounds.size.y);
		w.Write(csod.m_Bounds.size.z);
		w.Write(csod.m_DeleteGetsItem.Count);
		foreach (KeyValuePair<int, int> item in csod.m_DeleteGetsItem)
		{
			w.Write(item.Key);
			w.Write(item.Value);
		}
	}
}
