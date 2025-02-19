using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CSRecord;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class CSMgCreator : CSCreator
{
	public delegate void UpdateAddedStoreIdEvent();

	public delegate void UpdateMoneyEvent();

	public delegate void StoreIdAddedEvent(List<int> storeIdList);

	private CSAssembly m_Assembly;

	private Dictionary<int, CSCommon> m_CommonEntities;

	public Dictionary<int, CSBuildingLogic> allBuildingLogic = new Dictionary<int, CSBuildingLogic>();

	public CSClod m_Clod;

	private bool m_IsSiege;

	private PETimer m_Timer;

	private List<CSPersonnel> m_MainNpcs;

	private List<CSPersonnel> m_RandomNpcs;

	private int m_CurPatrolNpcNum;

	private int m_SoldierNum;

	private HashSet<CSPersonnel> m_Soldiers = new HashSet<CSPersonnel>();

	private int m_CurWorkerNum;

	private int m_WorkerNum;

	private HashSet<CSPersonnel> m_Workers = new HashSet<CSPersonnel>();

	private int m_FarmerNum;

	private HashSet<CSPersonnel> m_Farmers = new HashSet<CSPersonnel>();

	private int m_FarmMgNum;

	private int m_FarmHarvestNum;

	private int m_FarmPlantNum;

	private int m_CurProcessorNum;

	private int m_ProcessorNum;

	private HashSet<CSPersonnel> m_Processors = new HashSet<CSPersonnel>();

	private int m_TrainerNum;

	private HashSet<CSPersonnel> m_Trainers = new HashSet<CSPersonnel>();

	private bool IsAdjustingClods;

	public bool m_PauseTimer;

	public override CSAssembly Assembly => m_Assembly;

	public List<CSTreatment> m_TreatmentList => m_DataInst.treatmentList;

	public bool IsSiege => m_IsSiege;

	public int ColonyMoney
	{
		get
		{
			if (m_DataInst == null)
			{
				return -1;
			}
			return m_DataInst.colonyMoney;
		}
		set
		{
			if (m_DataInst != null)
			{
				m_DataInst.colonyMoney = value;
			}
		}
	}

	public override PETimer Timer => m_Timer;

	public List<CSPersonnel> MainNpcs => m_RandomNpcs;

	public List<CSPersonnel> RandomNpcs => m_RandomNpcs;

	public int CurPatrolNpcNum => m_CurPatrolNpcNum;

	public int SoldierNpcNum => m_SoldierNum;

	public HashSet<CSPersonnel> Soldiers => m_Soldiers;

	public int CurWorkerNum => m_CurWorkerNum;

	public int WorkerNum => m_WorkerNum;

	public HashSet<CSPersonnel> Workers => m_Workers;

	public int FarmerNum => m_FarmerNum;

	public HashSet<CSPersonnel> Farmers => m_Farmers;

	public int FarmMgNum => m_FarmMgNum;

	public int FarmHarvestNum => m_FarmHarvestNum;

	public int FarmPlantNum => m_FarmPlantNum;

	public int CurProcessorNum => m_CurProcessorNum;

	public int ProcessorNum => m_ProcessorNum;

	public HashSet<CSPersonnel> Processors => m_Processors;

	public int TrainerNum => m_TrainerNum;

	public HashSet<CSPersonnel> Trainers => m_Trainers;

	public int GetNpcCount => m_RandomNpcs.Count + m_MainNpcs.Count;

	public List<int> AddedStoreId => m_DataInst.addedStoreId;

	public List<int> AddedNpcId => m_DataInst.addedTradeNpc;

	public event UpdateAddedStoreIdEvent UpdateAddedStoreIdListener;

	public event UpdateMoneyEvent UpdateMoneyListener;

	public event StoreIdAddedEvent StoreIdAddedListener;

	public void InitMultiCSTreatment(List<CSTreatment> cstList)
	{
		m_DataInst.treatmentList = cstList;
	}

	public void InitMultiData(byte[] packData)
	{
		ParseData(packData);
	}

	public void ParseData(byte[] packData)
	{
		using MemoryStream input = new MemoryStream(packData);
		using BinaryReader reader = new BinaryReader(input);
		ColonyMoney = BufferHelper.ReadInt32(reader);
		int num = BufferHelper.ReadInt32(reader);
		List<int> list = new List<int>();
		for (int i = 0; i < num; i++)
		{
			list.Add(BufferHelper.ReadInt32(reader));
		}
		m_DataInst.addedStoreId = list;
	}

	public void RegistStoreIdAddedEvent(StoreIdAddedEvent addEvent)
	{
		this.StoreIdAddedListener = (StoreIdAddedEvent)Delegate.Remove(this.StoreIdAddedListener, addEvent);
		this.StoreIdAddedListener = (StoreIdAddedEvent)Delegate.Combine(this.StoreIdAddedListener, addEvent);
	}

	public void UnRegistStoreIdAddedEvent(StoreIdAddedEvent addEvent)
	{
		this.StoreIdAddedListener = (StoreIdAddedEvent)Delegate.Remove(this.StoreIdAddedListener, addEvent);
	}

	public void RegistUpdateAddedStoreIdEvent(UpdateAddedStoreIdEvent addEvent)
	{
		this.UpdateAddedStoreIdListener = (UpdateAddedStoreIdEvent)Delegate.Remove(this.UpdateAddedStoreIdListener, addEvent);
		this.UpdateAddedStoreIdListener = (UpdateAddedStoreIdEvent)Delegate.Combine(this.UpdateAddedStoreIdListener, addEvent);
	}

	public void UnRegistUpdateAddedStoreIdEvent(UpdateAddedStoreIdEvent addEvent)
	{
		this.UpdateAddedStoreIdListener = (UpdateAddedStoreIdEvent)Delegate.Remove(this.UpdateAddedStoreIdListener, addEvent);
	}

	public void RegistUpdateMoneyEvent(UpdateMoneyEvent addEvent)
	{
		this.UpdateMoneyListener = (UpdateMoneyEvent)Delegate.Remove(this.UpdateMoneyListener, addEvent);
		this.UpdateMoneyListener = (UpdateMoneyEvent)Delegate.Combine(this.UpdateMoneyListener, addEvent);
	}

	public void UnRegistUpdateMoneyEvent(UpdateMoneyEvent addEvent)
	{
		this.UpdateMoneyListener = (UpdateMoneyEvent)Delegate.Remove(this.UpdateMoneyListener, addEvent);
	}

	public override int CreateEntity(CSEntityAttr attr, out CSEntity outEnti)
	{
		outEnti = null;
		if (attr.m_Type == 1)
		{
			Vector3 zero = Vector3.zero;
			CSBuildingLogic cSBuildingLogic = ((!(attr.m_LogicObj == null)) ? attr.m_LogicObj.GetComponent<CSBuildingLogic>() : null);
			zero = ((!(cSBuildingLogic != null) || !(cSBuildingLogic.travelTrans != null)) ? (m_Assembly.Position + new Vector3(0f, 2f, 0f)) : cSBuildingLogic.travelTrans.position);
			if (m_Assembly == null)
			{
				if (GameConfig.IsMultiMode && attr.m_ColonyBase == null)
				{
					return 5;
				}
				outEnti = _createEntity(attr);
				m_Assembly.ChangeState();
				if (CSMain.s_MgCreator == this)
				{
					ColonyLabel.Remove(zero);
					new ColonyLabel(zero);
				}
			}
			else if (m_Assembly.ID == attr.m_InstanceId)
			{
				outEnti = m_Assembly;
				outEnti.gameLogic = attr.m_LogicObj;
				outEnti.gameObject = attr.m_Obj;
				outEnti.Position = attr.m_Pos;
				outEnti.ItemID = attr.m_protoId;
				outEnti.Bound = attr.m_Bound;
				m_Assembly.Data.m_Alive = true;
				if (CSMain.s_MgCreator == this)
				{
					ColonyLabel.Remove(zero);
					new ColonyLabel(zero);
				}
			}
			else if (m_Assembly != null)
			{
				return 1;
			}
		}
		else if (m_CommonEntities.ContainsKey(attr.m_InstanceId))
		{
			outEnti = m_CommonEntities[attr.m_InstanceId];
			outEnti.gameLogic = attr.m_LogicObj;
			outEnti.gameObject = attr.m_Obj;
			outEnti.Position = attr.m_Pos;
			outEnti.ItemID = attr.m_protoId;
			outEnti.BaseData.m_Alive = true;
			outEnti.Bound = attr.m_Bound;
		}
		else
		{
			if (m_Assembly == null)
			{
				return 0;
			}
			if (!m_Assembly.InRange(attr.m_Pos))
			{
				return 2;
			}
			CSConst.ObjectType type = (CSConst.ObjectType)attr.m_Type;
			if (!m_Assembly.OutOfCount(type))
			{
				return 3;
			}
			if (GameConfig.IsMultiMode && attr.m_ColonyBase == null)
			{
				return 5;
			}
			outEnti = _createEntity(attr);
			CSCommon csc = outEnti as CSCommon;
			m_Assembly.AttachCommonEntity(csc);
		}
		ExecuteEvent(1001, outEnti);
		return 4;
	}

	public CSEntity _createEntity(CSEntityAttr attr)
	{
		if (attr.m_Type == 1)
		{
			m_Assembly = new CSAssembly();
			m_Assembly.m_Info = CSInfoMgr.m_AssemblyInfo;
			m_Assembly.ID = attr.m_InstanceId;
			m_Assembly.gameLogic = attr.m_LogicObj;
			m_Assembly.gameObject = attr.m_Obj;
			m_Assembly.m_Creator = this;
			m_Assembly._ColonyObj = attr.m_ColonyBase;
			m_Assembly.CreateData();
			m_Assembly.Position = attr.m_Pos;
			m_Assembly.ItemID = attr.m_protoId;
			m_Assembly.Bound = attr.m_Bound;
			m_Assembly.Data.m_Alive = true;
			m_Assembly.InitErodeMap(attr.m_Pos, m_Assembly.Radius);
			m_Timer.Tick = m_Assembly.Data.m_TimeTicks;
			return m_Assembly;
		}
		CSCommon cSCommon = _CreateCommon(attr.m_Type);
		cSCommon.ID = attr.m_InstanceId;
		cSCommon._ColonyObj = attr.m_ColonyBase;
		cSCommon.CreateData();
		cSCommon.Position = attr.m_Pos;
		cSCommon.m_Power = attr.m_Power;
		cSCommon.gameLogic = attr.m_LogicObj;
		cSCommon.gameObject = attr.m_Obj;
		cSCommon.ItemID = attr.m_protoId;
		cSCommon.Bound = attr.m_Bound;
		cSCommon.BaseData.m_Alive = true;
		m_CommonEntities.Add(cSCommon.ID, cSCommon);
		return cSCommon;
	}

	private CSCommon _CreateCommon(int type)
	{
		CSCommon cSCommon = null;
		switch (type)
		{
		case 2:
		{
			cSCommon = new CSStorage();
			CSStorage cSStorage = cSCommon as CSStorage;
			cSStorage.m_Info = CSInfoMgr.m_StorageInfo;
			cSStorage.m_Creator = this;
			cSStorage.m_Package.ExtendPackage(CSInfoMgr.m_StorageInfo.m_MaxItem, CSInfoMgr.m_StorageInfo.m_MaxEquip, CSInfoMgr.m_StorageInfo.m_MaxRecource, CSInfoMgr.m_StorageInfo.m_MaxArmor);
			break;
		}
		case 4:
		{
			cSCommon = new CSEnhance();
			CSEnhance cSEnhance = cSCommon as CSEnhance;
			cSEnhance.m_Creator = this;
			cSEnhance.m_Info = CSInfoMgr.m_EnhanceInfo;
			break;
		}
		case 5:
		{
			cSCommon = new CSRepair();
			CSRepair cSRepair = cSCommon as CSRepair;
			cSRepair.m_Creator = this;
			cSRepair.m_Info = CSInfoMgr.m_RepairInfo;
			break;
		}
		case 6:
		{
			cSCommon = new CSRecycle();
			CSRecycle cSRecycle = cSCommon as CSRecycle;
			cSRecycle.m_Creator = this;
			cSRecycle.m_Info = CSInfoMgr.m_RecycleInfo;
			break;
		}
		case 21:
		{
			cSCommon = new CSDwellings();
			CSDwellings cSDwellings = cSCommon as CSDwellings;
			cSDwellings.m_Creator = this;
			cSDwellings.m_Info = CSInfoMgr.m_DwellingsInfo;
			if (PeGameMgr.IsMulti)
			{
				break;
			}
			int num = 0;
			foreach (KeyValuePair<int, CSCommon> commonEntity in m_CommonEntities)
			{
				if (num >= cSDwellings.m_NPCS.Length)
				{
					break;
				}
				if (commonEntity.Value.m_Type != 21)
				{
					continue;
				}
				CSDwellings cSDwellings2 = commonEntity.Value as CSDwellings;
				if (cSDwellings2.IsRunning)
				{
					continue;
				}
				for (int i = 0; i < cSDwellings2.m_NPCS.Length; i++)
				{
					if (cSDwellings2.m_NPCS[i] != null)
					{
						cSDwellings.AddNpcs(cSDwellings2.m_NPCS[i]);
						cSDwellings2.RemoveNpc(cSDwellings2.m_NPCS[i]);
						num++;
					}
				}
			}
			break;
		}
		case 33:
		{
			cSCommon = new CSPPCoal();
			CSPPCoal cSPPCoal = cSCommon as CSPPCoal;
			cSPPCoal.m_Creator = this;
			cSPPCoal.m_Power = 10000f;
			cSPPCoal.m_RestPower = 10000f;
			cSPPCoal.m_Info = CSInfoMgr.m_ppCoal;
			break;
		}
		case 34:
		{
			cSCommon = new CSPPSolar();
			CSPPSolar cSPPSolar = cSCommon as CSPPSolar;
			cSPPSolar.m_Creator = this;
			cSPPSolar.m_Power = 10000f;
			cSPPSolar.m_RestPower = 10000f;
			cSPPSolar.m_Info = CSInfoMgr.m_ppCoal;
			break;
		}
		case 7:
			cSCommon = new CSFarm();
			cSCommon.m_Creator = this;
			cSCommon.m_Info = CSInfoMgr.m_FarmInfo;
			break;
		case 8:
			cSCommon = new CSFactory();
			cSCommon.m_Creator = this;
			cSCommon.m_Info = CSInfoMgr.m_FactoryInfo;
			break;
		case 9:
		{
			cSCommon = new CSProcessing(this);
			CSProcessing cSProcessing = cSCommon as CSProcessing;
			cSProcessing.m_Info = CSInfoMgr.m_ProcessingInfo;
			break;
		}
		case 10:
		{
			cSCommon = new CSTrade(this);
			CSTrade cSTrade = cSCommon as CSTrade;
			cSTrade.m_Info = CSInfoMgr.m_Trade;
			break;
		}
		case 11:
		{
			cSCommon = new CSTraining(this);
			CSTraining cSTraining = cSCommon as CSTraining;
			cSTraining.m_Info = CSInfoMgr.m_Train;
			break;
		}
		case 12:
		{
			cSCommon = new CSMedicalCheck(this);
			CSMedicalCheck cSMedicalCheck = cSCommon as CSMedicalCheck;
			cSMedicalCheck.m_Info = CSInfoMgr.m_Check;
			break;
		}
		case 13:
		{
			cSCommon = new CSMedicalTreat(this);
			CSMedicalTreat cSMedicalTreat = cSCommon as CSMedicalTreat;
			cSMedicalTreat.m_Info = CSInfoMgr.m_Treat;
			break;
		}
		case 14:
		{
			cSCommon = new CSMedicalTent(this);
			CSMedicalTent cSMedicalTent = cSCommon as CSMedicalTent;
			cSMedicalTent.m_Info = CSInfoMgr.m_Tent;
			break;
		}
		case 35:
		{
			cSCommon = new CSPPFusion();
			CSPPFusion cSPPFusion = cSCommon as CSPPFusion;
			cSPPFusion.m_Creator = this;
			cSPPFusion.m_Power = 10000f;
			cSPPFusion.m_RestPower = 100000f;
			cSPPFusion.m_Info = CSInfoMgr.m_ppFusion;
			break;
		}
		}
		return cSCommon;
	}

	public override CSEntity RemoveEntity(int id, bool bRemoveData = true)
	{
		CSEntity cSEntity = null;
		if (m_Assembly != null && m_Assembly.ID == id)
		{
			cSEntity = m_Assembly;
			m_Assembly.Data.m_Alive = false;
			m_Assembly.RemoveErodeMap();
			if (bRemoveData)
			{
				m_Assembly.RemoveData();
			}
			if (CSMain.s_MgCreator == this)
			{
				Vector3 zero = Vector3.zero;
				CSBuildingLogic component = m_Assembly.gameLogic.GetComponent<CSBuildingLogic>();
				if (component != null && component.travelTrans != null)
				{
					zero = component.travelTrans.position;
					ColonyLabel.Remove(zero);
				}
				else
				{
					zero = m_Assembly.Position + new Vector3(0f, 2f, 0f);
					ColonyLabel.Remove(zero);
				}
			}
			m_Assembly.DestroySelf();
			m_Assembly = null;
			ExecuteEvent(1002, cSEntity);
		}
		else if (m_CommonEntities.ContainsKey(id))
		{
			cSEntity = m_CommonEntities[id];
			cSEntity.BaseData.m_Alive = false;
			if (bRemoveData)
			{
				m_CommonEntities[id].RemoveData();
			}
			m_CommonEntities.Remove(id);
			ExecuteEvent(1002, cSEntity);
			cSEntity.DestroySelf();
		}
		else
		{
			Debug.LogWarning("The Common Entity that you want to Remove is not contained!");
		}
		return cSEntity;
	}

	public void SetSiege(bool value)
	{
		if (m_IsSiege != value)
		{
			m_IsSiege = value;
		}
	}

	public override void RemoveLogic(int id)
	{
		allBuildingLogic.Remove(id);
	}

	public override void AddLogic(int id, CSBuildingLogic csb)
	{
		allBuildingLogic.Add(id, csb);
	}

	public override CSCommon GetCommonEntity(int ID)
	{
		if (m_CommonEntities.ContainsKey(ID))
		{
			return m_CommonEntities[ID];
		}
		return null;
	}

	public override int GetCommonEntityCnt()
	{
		return m_CommonEntities.Count;
	}

	public override Dictionary<int, CSCommon> GetCommonEntities()
	{
		return m_CommonEntities;
	}

	public override int CanCreate(int type, Vector3 pos)
	{
		if (RandomDungenMgrData.InDungeon)
		{
			return 9;
		}
		if (type == 1)
		{
			if (m_Assembly != null)
			{
				return 1;
			}
			if (!MissionManager.CanDragAssembly(pos, out var num))
			{
				switch (num)
				{
				case 0:
					return 6;
				case 1:
					return 7;
				case 2:
					return 8;
				}
			}
		}
		else
		{
			if (m_Assembly == null)
			{
				return 0;
			}
			if (!m_Assembly.InRange(pos))
			{
				return 2;
			}
			if (!m_Assembly.OutOfCount((CSConst.ObjectType)type))
			{
				return 3;
			}
		}
		return 4;
	}

	public int GetEmptyBedCnt()
	{
		if (m_Assembly == null)
		{
			return 0;
		}
		CSCommon[] array = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
		if (array.Length == 0)
		{
			return 0;
		}
		int num = 0;
		CSCommon[] array2 = array;
		foreach (CSCommon cSCommon in array2)
		{
			CSDwellings cSDwellings = cSCommon as CSDwellings;
			num += cSDwellings.GetEmptySpace();
		}
		return num;
	}

	public bool ObjectInPowerPlant(Vector3 pos)
	{
		if (Assembly == null)
		{
			return false;
		}
		List<CSCommon> allPowerPlants = Assembly.AllPowerPlants;
		for (int i = 0; i < allPowerPlants.Count; i++)
		{
			CSPowerPlant cSPowerPlant = allPowerPlants[i] as CSPowerPlant;
			if (cSPowerPlant.InRange(pos))
			{
				return true;
			}
		}
		return false;
	}

	public override bool CanAddNpc()
	{
		if (m_Assembly == null)
		{
			Debug.Log("There is no assembly In the word!");
			return false;
		}
		CSCommon[] array = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
		if (array.Length == 0)
		{
			Debug.Log("There is not enough Dwellings for this NPC");
			return false;
		}
		CSCommon[] array2 = array;
		foreach (CSCommon cSCommon in array2)
		{
			CSDwellings cSDwellings = cSCommon as CSDwellings;
			if (cSDwellings.HasSpace())
			{
				return true;
			}
		}
		return false;
	}

	public override bool AddNpc(PeEntity npc, bool bSetPos = false)
	{
		if (npc.IsRecruited())
		{
			Debug.Log("This npc is already a CSPersonnel object!");
			return false;
		}
		CSPersonnel cSPersonnel = new CSPersonnel();
		cSPersonnel.ID = npc.Id;
		cSPersonnel.NPC = npc;
		cSPersonnel.m_Creator = this;
		cSPersonnel.CreateData();
		if (cSPersonnel.Dwellings == null)
		{
			if (m_Assembly == null)
			{
				Debug.Log("There is no assembly In the word!");
				cSPersonnel.RemoveData();
				return false;
			}
			CSCommon[] array = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
			if (array.Length == 0)
			{
				Debug.Log("There is not enough Dwellings for this NPC");
				cSPersonnel.RemoveData();
				return false;
			}
			CSCommon[] array2 = array;
			foreach (CSCommon cSCommon in array2)
			{
				CSDwellings cSDwellings = cSCommon as CSDwellings;
				if (cSDwellings.AddNpcs(cSPersonnel))
				{
					break;
				}
			}
			if (cSPersonnel.Dwellings == null)
			{
				Debug.Log("There is not enough Dwellings for this NPC");
				cSPersonnel.RemoveData();
				return false;
			}
		}
		if (npc.IsRandomNpc())
		{
			m_RandomNpcs.Add(cSPersonnel);
			PeEntityCreator.RecruitRandomNpc(npc);
		}
		else
		{
			m_MainNpcs.Add(cSPersonnel);
			PeEntityCreator.RecruitMainNpc(npc);
		}
		if (npc.NpcCmpt != null)
		{
			npc.NpcCmpt.SendTalkMsg(1, 0f, ENpcSpeakType.Both);
		}
		ExecuteEventPersonnel(1003, cSPersonnel);
		_increaseOccupationNum(cSPersonnel, cSPersonnel.Occupation);
		_increaseWorkModeNum(cSPersonnel, cSPersonnel.m_WorkMode);
		cSPersonnel.UpdateNpcCmpt();
		return true;
	}

	public bool AddNpc(PeEntity npc, CSPersonnelData data, bool bSetPos = false)
	{
		if (npc.IsRecruited())
		{
			Debug.Log("This npc is already a CSPersonnel object!");
			return false;
		}
		CSPersonnel cSPersonnel = new CSPersonnel();
		cSPersonnel.ID = npc.Id;
		cSPersonnel.NPC = npc;
		cSPersonnel.m_Creator = this;
		cSPersonnel.CreateData(data);
		if (npc.IsRandomNpc())
		{
			m_RandomNpcs.Add(cSPersonnel);
			PeEntityCreator.RecruitRandomNpc(npc);
		}
		else
		{
			m_MainNpcs.Add(cSPersonnel);
			PeEntityCreator.RecruitMainNpc(npc);
		}
		ExecuteEventPersonnel(1003, cSPersonnel);
		_increaseOccupationNum(cSPersonnel, cSPersonnel.Occupation);
		_increaseWorkModeNum(cSPersonnel, cSPersonnel.m_WorkMode);
		cSPersonnel.UpdateNpcCmpt();
		return true;
	}

	public bool AddNpcInMultiMode(PeEntity npc, int dwellingId, bool bSetPos = false)
	{
		if (npc.IsRecruited())
		{
			Debug.Log("This npc is already a CSPersonnel object!");
			return false;
		}
		CSPersonnel cSPersonnel = new CSPersonnel();
		cSPersonnel.ID = npc.Id;
		cSPersonnel.NPC = npc;
		cSPersonnel.m_Creator = this;
		cSPersonnel.CreateData();
		if (cSPersonnel.Dwellings == null)
		{
			if (m_Assembly == null)
			{
				Debug.Log("There is no assembly In the word!");
				cSPersonnel.RemoveData();
				return false;
			}
			if (m_CommonEntities.ContainsKey(dwellingId) && m_CommonEntities[dwellingId] is CSDwellings cSDwellings)
			{
				cSDwellings.AddNpcs(cSPersonnel);
			}
			if (cSPersonnel.Dwellings == null)
			{
				Debug.Log("There is not enough Dwellings for this NPC");
				cSPersonnel.RemoveData();
				return false;
			}
		}
		if (npc.IsRandomNpc())
		{
			m_RandomNpcs.Add(cSPersonnel);
		}
		else
		{
			m_MainNpcs.Add(cSPersonnel);
		}
		ExecuteEventPersonnel(1003, cSPersonnel);
		_increaseOccupationNum(cSPersonnel, cSPersonnel.Occupation);
		_increaseWorkModeNum(cSPersonnel, cSPersonnel.m_WorkMode);
		cSPersonnel.UpdateNpcCmpt();
		return true;
	}

	public override void RemoveNpc(PeEntity npc)
	{
		CSPersonnel colonyNpc = CSMain.GetColonyNpc(npc.Id);
		if (colonyNpc == null)
		{
			Debug.LogWarning("The npc you want to kick out is not a recruit.");
			return;
		}
		colonyNpc.RemoveData();
		if (colonyNpc.Dwellings != null)
		{
			colonyNpc.Dwellings.RemoveNpc(colonyNpc);
		}
		if (colonyNpc.WorkRoom != null)
		{
			colonyNpc.WorkRoom.RemoveWorker(colonyNpc);
		}
		m_RandomNpcs.Remove(colonyNpc);
		m_MainNpcs.Remove(colonyNpc);
		ExecuteEventPersonnel(1004, colonyNpc);
		_decreaseOccupationNum(colonyNpc, colonyNpc.Occupation);
		_decreaseWorkModeNum(colonyNpc, colonyNpc.m_WorkMode);
		colonyNpc.m_Creator = null;
		if (npc.IsRandomNpc())
		{
			PeEntityCreator.ExileRandomNpc(npc);
		}
		else
		{
			PeEntityCreator.ExileMainNpc(npc);
		}
		colonyNpc.UpdateNpcCmpt();
		PeNpcGroup.Instance.OnRemoveCsNpc(npc);
	}

	public override CSPersonnel[] GetNpcs()
	{
		List<CSPersonnel> list = new List<CSPersonnel>();
		list.AddRange(m_RandomNpcs);
		list.AddRange(m_MainNpcs);
		return list.ToArray();
	}

	public override CSPersonnel GetNpc(int id)
	{
		if (id < 0)
		{
			return null;
		}
		foreach (CSPersonnel randomNpc in m_RandomNpcs)
		{
			if (randomNpc.ID == id)
			{
				return randomNpc;
			}
		}
		foreach (CSPersonnel mainNpc in m_MainNpcs)
		{
			if (mainNpc.ID == id)
			{
				return mainNpc;
			}
		}
		return null;
	}

	private void _increaseStateNum(CSPersonnel person, int state)
	{
		switch (state)
		{
		case 8:
			m_CurPatrolNpcNum++;
			break;
		case 4:
			m_CurWorkerNum++;
			break;
		}
	}

	private void _decreaseStateNum(CSPersonnel person, int state)
	{
		switch (state)
		{
		case 8:
			m_CurPatrolNpcNum = Mathf.Max(0, m_CurPatrolNpcNum - 1);
			break;
		case 4:
			m_CurWorkerNum = Mathf.Max(0, m_CurWorkerNum - 1);
			break;
		}
	}

	private void _increaseOccupationNum(CSPersonnel person, int occupation)
	{
		switch (occupation)
		{
		case 1:
			if (!m_Workers.Contains(person))
			{
				m_Workers.Add(person);
			}
			m_WorkerNum++;
			break;
		case 2:
			if (!m_Soldiers.Contains(person))
			{
				m_Soldiers.Add(person);
			}
			m_SoldierNum++;
			break;
		case 3:
			if (!m_Farmers.Contains(person))
			{
				m_Farmers.Add(person);
			}
			m_FarmerNum++;
			break;
		case 5:
			if (!m_Processors.Contains(person))
			{
				m_Processors.Add(person);
			}
			m_ProcessorNum++;
			break;
		case 7:
			if (!Trainers.Contains(person))
			{
				Trainers.Add(person);
			}
			m_TrainerNum++;
			break;
		}
	}

	private void _decreaseOccupationNum(CSPersonnel person, int occupation)
	{
		switch (occupation)
		{
		case 1:
			m_Workers.Remove(person);
			m_WorkerNum = Mathf.Max(0, m_WorkerNum - 1);
			break;
		case 2:
			m_Soldiers.Remove(person);
			m_SoldierNum = Mathf.Max(0, m_SoldierNum - 1);
			break;
		case 3:
			m_Farmers.Remove(person);
			m_FarmerNum = Mathf.Max(0, m_FarmerNum - 1);
			break;
		case 5:
			m_Processors.Remove(person);
			m_ProcessorNum = Mathf.Max(0, m_ProcessorNum - 1);
			break;
		case 7:
			m_Trainers.Remove(person);
			m_TrainerNum = Mathf.Max(0, m_TrainerNum - 1);
			break;
		}
	}

	private void _increaseWorkModeNum(CSPersonnel person, int mode)
	{
		switch (mode)
		{
		case 4:
			m_FarmMgNum++;
			break;
		case 5:
			m_FarmHarvestNum++;
			break;
		case 6:
			m_FarmPlantNum++;
			break;
		}
	}

	private void _decreaseWorkModeNum(CSPersonnel person, int mode)
	{
		switch (mode)
		{
		case 4:
			m_FarmMgNum = Mathf.Max(0, m_FarmMgNum - 1);
			break;
		case 5:
			m_FarmHarvestNum = Mathf.Max(0, m_FarmHarvestNum - 1);
			break;
		case 6:
			m_FarmPlantNum = Mathf.Max(0, m_FarmPlantNum - 1);
			break;
		}
	}

	private void OnPersonnelChangeState(CSPersonnel person, int prvState)
	{
		if (!(person.m_Creator != this))
		{
			_decreaseStateNum(person, prvState);
		}
	}

	private void OnPersonnelChangeOccupation(CSPersonnel person, int prvState)
	{
		if (!(person.m_Creator != this))
		{
			_increaseOccupationNum(person, person.Occupation);
			_decreaseOccupationNum(person, prvState);
		}
	}

	private void OnPersonnelChangeWorkType(CSPersonnel person, int prvState)
	{
		if (!(person.m_Creator != this))
		{
			_increaseWorkModeNum(person, person.m_WorkMode);
			_decreaseWorkModeNum(person, prvState);
		}
	}

	private void OnDestroy()
	{
		m_CommonEntities.Clear();
		m_MainNpcs.Clear();
		m_RandomNpcs.Clear();
		CSPersonnel.UnRegisterStateChangedListener(OnPersonnelChangeState);
		CSPersonnel.UnRegisterStateChangedListener(OnPersonnelChangeOccupation);
		CSPersonnel.UnregisterWorkTypeChangedListener(OnPersonnelChangeWorkType);
	}

	private void Awake()
	{
		m_Type = CSConst.CreatorType.Managed;
		m_CommonEntities = new Dictionary<int, CSCommon>();
		m_MainNpcs = new List<CSPersonnel>();
		m_RandomNpcs = new List<CSPersonnel>();
		CSPersonnel.RegisterStateChangedListener(OnPersonnelChangeState);
		CSPersonnel.RegisterOccupaChangedListener(OnPersonnelChangeOccupation);
		CSPersonnel.RegisterWorkTypeChangedListener(OnPersonnelChangeWorkType);
		m_Timer = new PETimer();
	}

	private void Start()
	{
		if (!GameConfig.IsMultiMode)
		{
			Dictionary<int, CSDefaultData> objectRecords = m_DataInst.GetObjectRecords();
			foreach (CSDefaultData value in objectRecords.Values)
			{
				if (!(value is CSObjectData { ID: not -100, m_Alive: not false } cSObjectData))
				{
					continue;
				}
				CSEntityAttr attr = default(CSEntityAttr);
				attr.m_InstanceId = cSObjectData.ID;
				attr.m_Type = cSObjectData.dType;
				attr.m_Pos = cSObjectData.m_Position;
				attr.m_protoId = cSObjectData.ItemID;
				attr.m_Bound = cSObjectData.m_Bounds;
				CSEntity cSEntity = _createEntity(attr);
				if (cSObjectData.dType == 1)
				{
					CSAssembly cSAssembly = cSEntity as CSAssembly;
					cSAssembly.ChangeState();
					continue;
				}
				CSCommon csc = cSEntity as CSCommon;
				if (m_Assembly != null)
				{
					m_Assembly.AttachCommonEntity(csc);
				}
			}
			StartCoroutine(InitColonyNpc());
		}
		else
		{
			Debug.Log("<color=red>Creator start! Desc:" + base.gameObject.name + "</color>");
		}
	}

	private IEnumerator InitColonyNpc()
	{
		if (GameConfig.IsMultiMode)
		{
			yield break;
		}
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (PeSingleton<EntityMgr>.Instance != null)
		{
			Dictionary<int, CSPersonnelData>.ValueCollection personnelRecords = m_DataInst.GetPersonnelRecords();
			foreach (CSPersonnelData pdata in personnelRecords)
			{
				if (pdata.m_WorkRoomID == -100)
				{
					pdata.m_WorkRoomID = -1;
				}
				PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(pdata.ID);
				if (npc != null)
				{
					AddNpc(npc, pdata, bSetPos: true);
				}
			}
		}
		InitColonyAfterReady();
	}

	private IEnumerator AdjustClodsProc()
	{
		if (Assembly == null)
		{
			yield break;
		}
		IsAdjustingClods = true;
		int frame_count = 0;
		List<Vector3> remove_list = new List<Vector3>();
		foreach (KeyValuePair<IntVec3, ClodChunk> clodChunk in m_Clod.m_ClodChunks)
		{
			foreach (Vector3 pos in clodChunk.Value.m_Clods.Values)
			{
				if (!Assembly.InLargestRange(pos))
				{
					remove_list.Add(pos);
				}
			}
		}
		yield return 0;
		frame_count = 0;
		if (Assembly == null)
		{
			IsAdjustingClods = false;
			yield break;
		}
		int i = 0;
		while (i < remove_list.Count)
		{
			m_Clod.DeleteClod(remove_list[i]);
			i++;
			frame_count++;
		}
		yield return 0;
		IsAdjustingClods = false;
	}

	private void Update()
	{
		if (!m_PauseTimer)
		{
			if (m_Assembly != null)
			{
				m_Assembly.Update();
				m_Timer.ElapseSpeed = GameTime.Timer.ElapseSpeed;
				m_Timer.Update(Time.deltaTime);
			}
			else
			{
				m_Timer.ElapseSpeed = 0f;
			}
		}
		foreach (KeyValuePair<int, CSCommon> commonEntity in m_CommonEntities)
		{
			commonEntity.Value.Update();
		}
		foreach (CSPersonnel mainNpc in m_MainNpcs)
		{
			mainNpc.Update();
		}
		foreach (CSPersonnel randomNpc in m_RandomNpcs)
		{
			randomNpc.Update();
		}
		if (m_Clod != null && Time.frameCount % 8192 == 0 && Assembly != null && Assembly.Farm != null && Assembly.Farm.IsRunning && !IsAdjustingClods)
		{
			StartCoroutine(AdjustClodsProc());
		}
		if (m_Clod != null && Time.frameCount % 8192 == 4096 && Assembly != null && Assembly.Farm != null && !Assembly.isSearchingClod && Assembly.Farm.IsRunning && Assembly.ModelObj != null)
		{
			StartCoroutine(CSMain.Instance.SearchVaildClodForAssembly(Assembly));
		}
	}

	public CSTreatment FindTreatment(int id, bool needTreat = false)
	{
		foreach (CSTreatment treatment in m_TreatmentList)
		{
			if (treatment.npcId == id)
			{
				if (!needTreat)
				{
					return treatment;
				}
				if (treatment.needTreatTimes > 0)
				{
					return treatment;
				}
			}
		}
		return null;
	}

	public List<CSTreatment> FindNpcTreatments(int npcid)
	{
		List<CSTreatment> list = new List<CSTreatment>();
		foreach (CSTreatment treatment in m_TreatmentList)
		{
			if (treatment.npcId == npcid)
			{
				list.Add(treatment);
			}
		}
		return list;
	}

	public CSTreatment GetTreatment(int abnormalId, int npcId, int needTreatTimes)
	{
		return m_TreatmentList.Find((CSTreatment it) => it.abnormalId == abnormalId && it.npcId == npcId && it.needTreatTimes == needTreatTimes);
	}

	public void AddTreatment(List<CSTreatment> cst)
	{
		m_TreatmentList.AddRange(cst);
	}

	public void RemoveNpcTreatment(int npcid)
	{
		m_TreatmentList.RemoveAll((CSTreatment it) => it.npcId == npcid);
	}

	public void UpdateTreatment()
	{
		m_TreatmentList.RemoveAll((CSTreatment it) => it.needTreatTimes <= 0);
	}

	public void InitColonyAfterReady()
	{
		if (Assembly != null)
		{
			Assembly.InitAfterAllDataReady();
		}
		foreach (CSCommon value in m_CommonEntities.Values)
		{
			value.InitAfterAllDataReady();
		}
	}

	public void AddStoreId(List<int> storeIdList)
	{
		List<int> list = new List<int>();
		foreach (int storeId in storeIdList)
		{
			List<int> allIdOfSameItem = ShopRespository.GetAllIdOfSameItem(storeId);
			int limitNum = ShopRespository.GetLimitNum(storeId);
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			foreach (int item in allIdOfSameItem)
			{
				if (item == storeId)
				{
					list2.Add(item);
				}
				else if (ShopRespository.GetLimitNum(item) >= limitNum)
				{
					list2.Add(item);
				}
				else if (ShopRespository.GetLimitNum(item) < limitNum)
				{
					list3.Add(item);
				}
			}
			bool flag = true;
			foreach (int item2 in list2)
			{
				if (AddedStoreId.Contains(item2))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (int item3 in list3)
			{
				if (AddedStoreId.Contains(item3))
				{
					AddedStoreId.Remove(item3);
					break;
				}
			}
			AddedStoreId.Add(storeId);
			list.Add(storeId);
		}
		if (list.Count > 0 && this.UpdateAddedStoreIdListener != null)
		{
			this.UpdateAddedStoreIdListener();
		}
	}

	public void RefreshMoney()
	{
		ColonyMoney = 5000;
		if (this.UpdateMoneyListener != null)
		{
			this.UpdateMoneyListener();
		}
	}
}
