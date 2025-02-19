using System;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSMedicalTreat : CSHealth
{
	private const float DEFAULT_TREAT_CHANCE = 1f;

	public const int WORKER_AMOUNT_MAX = 1;

	public PeEntity lastTreatNpc;

	public CSUI_Hospital uiObj;

	public ItemObject medicineItem;

	public CSTreatment treatmentInUse;

	private CounterScript m_Counter;

	public CSTreatInfo m_TInfo;

	private CSTreatData m_TData;

	public bool isNpcReady
	{
		get
		{
			return Data.isNpcReady;
		}
		set
		{
			Data.isNpcReady = value;
		}
	}

	public bool occupied
	{
		get
		{
			return Data.occupied;
		}
		set
		{
			Data.occupied = value;
		}
	}

	public bool IsOccupied => occupied;

	public override GameObject gameLogic
	{
		get
		{
			return base.gameLogic;
		}
		set
		{
			base.gameLogic = value;
			if (!(gameLogic != null))
			{
				return;
			}
			PEMachine component = gameLogic.GetComponent<PEMachine>();
			if (component != null)
			{
				for (int i = 0; i < m_WorkSpaces.Length; i++)
				{
					m_WorkSpaces[i].WorkMachine = component;
				}
			}
			PEDoctor component2 = gameLogic.GetComponent<PEDoctor>();
			if (component2 != null)
			{
				for (int j = 0; j < m_WorkSpaces.Length; j++)
				{
					m_WorkSpaces[j].HospitalMachine = component2;
				}
			}
			PEPatients component3 = gameLogic.GetComponent<PEPatients>();
			pePatient = component3;
			if (BuildingLogic != null)
			{
				workTrans = BuildingLogic.m_WorkTrans;
				resultTrans = BuildingLogic.m_ResultTrans;
			}
		}
	}

	public CSBuildingLogic BuildingLogic => gameLogic.GetComponent<CSBuildingLogic>();

	public CSTreatInfo Info
	{
		get
		{
			if (m_TInfo == null)
			{
				m_TInfo = m_Info as CSTreatInfo;
			}
			return m_TInfo;
		}
	}

	public CSTreatData Data
	{
		get
		{
			if (m_TData == null)
			{
				m_TData = m_Data as CSTreatData;
			}
			return m_TData;
		}
	}

	public bool IsTreat => m_Counter != null;

	public CSMedicalTreat(CSCreator creator)
	{
		m_Creator = creator;
		m_Type = 13;
		m_Workers = new CSPersonnel[1];
		m_WorkSpaces = new PersonnelSpace[1];
		for (int i = 0; i < m_WorkSpaces.Length; i++)
		{
			m_WorkSpaces[i] = new PersonnelSpace(this);
		}
		m_Grade = 3;
		if (base.IsMine)
		{
			BindEvent();
		}
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning && IsTreat;
	}

	public void OccupyMachine()
	{
		occupied = true;
	}

	public void ReleaseMachine()
	{
		occupied = false;
	}

	public override bool AddWorker(CSPersonnel npc)
	{
		if (base.AddWorker(npc))
		{
			SetDoctorIcon(npc);
			return true;
		}
		return false;
	}

	public override void RemoveWorker(CSPersonnel npc)
	{
		base.RemoveWorker(npc);
		SetDoctorIcon(null);
	}

	public override void AfterTurn90Degree()
	{
		base.AfterTurn90Degree();
		isNpcReady = false;
		occupied = false;
	}

	private void BindEvent()
	{
		if (uiObj == null && CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.HospitalUI != null)
		{
			uiObj = CSUI_MainWndCtrl.Instance.HospitalUI;
			CSUI_Hospital cSUI_Hospital = uiObj;
			cSUI_Hospital.mMedicineRealOp = (CSUI_Hospital.MedicineRealOp)Delegate.Combine(cSUI_Hospital.mMedicineRealOp, new CSUI_Hospital.MedicineRealOp(SetMedicineItem));
		}
	}

	private void UnbindEvent()
	{
		if (uiObj != null && CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.HospitalUI != null)
		{
			CSUI_Hospital cSUI_Hospital = uiObj;
			cSUI_Hospital.mMedicineRealOp = (CSUI_Hospital.MedicineRealOp)Delegate.Remove(cSUI_Hospital.mMedicineRealOp, new CSUI_Hospital.MedicineRealOp(SetMedicineItem));
			uiObj = null;
		}
	}

	private void UpdateTimeShow(float timeLeft)
	{
		if (uiObj != null)
		{
			uiObj.TreatTimeShow(timeLeft);
		}
	}

	private void SetDoctorIcon(CSPersonnel npc)
	{
		if (uiObj != null)
		{
			uiObj.TreatDoc = npc;
		}
	}

	private void SetPatientIcon(CSPersonnel npc)
	{
		if (uiObj != null)
		{
			uiObj.TreatmentPatient = npc;
		}
	}

	private void ResetMissionItem(bool _isMis)
	{
		if (base.IsMine)
		{
			if (_isMis)
			{
				GameUI.Instance.mItemPackageCtrl.ResetMissionItem();
			}
			else
			{
				GameUI.Instance.mItemPackageCtrl.ResetItem();
			}
		}
	}

	private void ShowMedicineNeed(ItemIdCount iic)
	{
		if (uiObj != null)
		{
			uiObj.TreatMedicineShow(iic);
		}
	}

	private void ClearMedicineNeed()
	{
		if (uiObj != null)
		{
			uiObj.ClearTreatMedicine();
		}
	}

	private void SetMedicineIcon(ItemObject itemObj, bool inOrOut)
	{
		if (uiObj != null)
		{
			uiObj.SetLocalGrid(itemObj, inOrOut);
		}
	}

	private void RefreshTreatment()
	{
		if (uiObj != null)
		{
			uiObj.RefreshGrid();
		}
	}

	public void SetMedicineItem(ItemPackage _ip, bool _isMis, int _tabIndex, int _index, int _instanceId, bool _inorout)
	{
		if (!base.IsRunning)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRT_SetItem, _isMis, _instanceId, _inorout, _tabIndex, _index);
			return;
		}
		if (_inorout)
		{
			ItemObject item = PeSingleton<ItemMgr>.Instance.Get(_instanceId);
			if (medicineItem != null)
			{
				ItemObject item2 = medicineItem;
				medicineItem = item;
				_ip.PutItem(item2, _index, (ItemPackage.ESlotType)_tabIndex);
			}
			else
			{
				medicineItem = item;
				_ip.RemoveItem(item);
			}
		}
		else
		{
			_ip.PutItem(medicineItem, _index, (ItemPackage.ESlotType)_tabIndex);
			medicineItem = null;
		}
		if (medicineItem != null)
		{
			Data.m_ObjID = medicineItem.instanceId;
		}
		else
		{
			Data.m_ObjID = -1;
		}
		SetMedicineIcon(medicineItem, _inorout);
		ResetMissionItem(_isMis);
	}

	public void AddPatientToUI(PeEntity npc)
	{
		if (npc == null)
		{
			RemovePatientFromUI();
			return;
		}
		treatmentInUse = base.m_MgCreator.FindTreatment(npc.Id, needTreat: true);
		if (treatmentInUse != null && treatmentInUse.medicineList != null)
		{
			SetPatientIcon(m_Creator.GetNpc(npc.Id));
			ShowMedicineNeed(treatmentInUse.medicineList[0]);
		}
	}

	public void RemovePatientFromUI()
	{
		SetPatientIcon(null);
		ClearMedicineNeed();
	}

	public void UpdataPatientToUI()
	{
		if (allPatients.Count > 0)
		{
			AddPatientToUI(allPatients[0]);
		}
		else
		{
			AddPatientToUI(null);
		}
	}

	private float CountFinalTime(PeEntity npc)
	{
		if (Application.isEditor)
		{
			return 15f;
		}
		float treatTime = GetTreatTime(npc);
		return treatTime * (1f + m_Workers[0].GetTreatTimeSkill);
	}

	public void StartCounter(PeEntity npc)
	{
		float finalTime = CountFinalTime(npc);
		StartCounter(0f, finalTime);
	}

	public void StartCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			if (m_Counter == null)
			{
				m_Counter = CSMain.Instance.CreateCounter("MedicalTreat", curTime, finalTime);
			}
			else
			{
				m_Counter.Init(curTime, finalTime);
			}
			m_Counter.OnTimeUp = OnTreatFinish;
		}
	}

	public void StopCounter()
	{
		Data.m_CurTime = -1f;
		Data.m_Time = -1f;
		CSMain.Instance.DestoryCounter(m_Counter);
		m_Counter = null;
	}

	public bool IsReady(PeEntity npc)
	{
		return allPatients.Count <= 0 || allPatients[0] == npc;
	}

	public void AppointTreat(PeEntity npc)
	{
		if (!allPatients.Contains(npc))
		{
			allPatients.Add(npc);
			Data.npcIds.Add(npc.Id);
			if (allPatients.Count == 1)
			{
				AddPatientToUI(npc);
			}
		}
	}

	public bool StartTreat(PeEntity npc)
	{
		if (allPatients.Count <= 0)
		{
			return false;
		}
		if (allPatients[0] != npc)
		{
			return false;
		}
		if (!CallDoctor())
		{
		}
		isNpcReady = true;
		npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Treating;
		return true;
	}

	public override void RemoveDeadPatient(int npcId)
	{
		if (PeGameMgr.IsMulti)
		{
			base._Net.RPCServer(EPacketType.PT_CL_TRT_RemoveDeadNpc, npcId);
			return;
		}
		base.RemoveDeadPatient(npcId);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		if (allPatients.Count == 0 || !allPatients.Contains(peEntity))
		{
			return;
		}
		if (allPatients[0].Id == npcId)
		{
			allPatients.RemoveAt(0);
			StopCounter();
			isNpcReady = false;
			occupied = false;
			UpdatePatientIcon();
			if (allPatients.Count > 0)
			{
				allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
			}
		}
		else
		{
			allPatients.RemoveAll((PeEntity it) => it.Id == npcId);
		}
		Data.npcIds.Remove(npcId);
		peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
	}

	public bool IsPatientAndMedicineReady()
	{
		if (allPatients.Count <= 0 || allPatients[0].GetCmpt<NpcCmpt>().MedicalState != ENpcMedicalState.Treating)
		{
			return false;
		}
		if (treatmentInUse == null)
		{
			return false;
		}
		return true;
	}

	public bool IsMedicineReady()
	{
		return true;
	}

	public bool CallDoctor()
	{
		if (m_Workers[0] == null)
		{
			return false;
		}
		if (!IsDoctorReady())
		{
		}
		return true;
	}

	public bool IsDoctorReady()
	{
		if (m_Workers[0] == null || workTrans == null || workTrans[0] == null)
		{
			return false;
		}
		return true;
	}

	public void OnTreatFinish()
	{
		CSMain.Instance.RemoveCounter(m_Counter);
		if (GameConfig.IsMultiMode)
		{
			isNpcReady = false;
			return;
		}
		bool flag = false;
		System.Random random = new System.Random();
		if (random.NextDouble() > (double)(1f + m_Workers[0].GetTreatChanceSkill))
		{
			flag = false;
		}
		else if (treatmentInUse != null)
		{
			flag = (allPatients[0].GetCmpt<NpcCmpt>().illAbnormals.Contains((PEAbnormalType)treatmentInUse.abnormalId) ? true : false);
		}
		if (flag)
		{
			if (treatmentInUse != null)
			{
				treatmentInUse.needTreatTimes--;
				if (treatmentInUse.needTreatTimes > 0)
				{
					allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
				}
				else
				{
					allPatients[0].GetCmpt<NpcCmpt>().CureSick((PEAbnormalType)treatmentInUse.abnormalId);
					base.m_MgCreator.UpdateTreatment();
					if (base.m_MgCreator.FindTreatment(allPatients[0].Id) == null)
					{
						allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
					}
					else
					{
						allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
					}
				}
			}
			else
			{
				allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
			}
		}
		else
		{
			base.m_MgCreator.RemoveNpcTreatment(allPatients[0].Id);
			allPatients[0].GetCmpt<NpcCmpt>().AddSick(PEAbnormalType.MedicalAccident);
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
		}
		treatmentInUse = null;
		allPatients.RemoveAt(0);
		Data.npcIds.RemoveAt(0);
		isNpcReady = false;
		if (allPatients.Count >= 1)
		{
			AddPatientToUI(allPatients[0]);
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		}
		else
		{
			RemovePatientFromUI();
		}
		RefreshTreatment();
	}

	public void CheckPatient()
	{
		if (Data.npcIds.Count != allPatients.Count)
		{
			allPatients.Clear();
			foreach (int npcId in Data.npcIds)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
				if (peEntity != null)
				{
					allPatients.Add(peEntity);
				}
			}
		}
		for (int num = allPatients.Count - 1; num >= 0; num--)
		{
			if (m_Creator.GetNpc(allPatients[num].Id) == null)
			{
				allPatients.RemoveAt(num);
				Data.npcIds.RemoveAt(num);
				if (num == 0)
				{
					UpdatePatientIcon();
					isNpcReady = false;
				}
			}
		}
	}

	public void UpdatePatientIcon()
	{
		if (allPatients.Count > 0)
		{
			AddPatientToUI(allPatients[0]);
		}
		else
		{
			RemovePatientFromUI();
		}
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 13, ref refData) : MultiColonyManager.Instance.AssignData(ID, 13, ref refData, _ColonyObj));
		m_Data = refData as CSTreatData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			return;
		}
		StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
		StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		if (Data.npcIds.Count > 0)
		{
			foreach (int npcId in Data.npcIds)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
				if (peEntity != null)
				{
					allPatients.Add(peEntity);
				}
			}
		}
		if (allPatients.Count > 0)
		{
			treatmentInUse = base.m_MgCreator.FindTreatment(allPatients[0].Id, needTreat: true);
		}
		if (Data.m_ObjID >= 0)
		{
			medicineItem = PeSingleton<ItemMgr>.Instance.Get(Data.m_ObjID);
		}
		StartCounter(Data.m_CurTime, Data.m_Time);
	}

	public override void Update()
	{
		base.Update();
		CheckPatient();
		if (base.IsRunning && IsDoctorReady() && IsPatientAndMedicineReady())
		{
			if (m_Counter == null && PeGameMgr.IsSingle)
			{
				StartTreatingNpc(allPatients[0]);
			}
			if (m_Counter != null)
			{
				m_Counter.enabled = true;
				UpdateTimeShow(m_Counter.FinalCounter - m_Counter.CurCounter);
			}
		}
		else if (m_Counter != null)
		{
			m_Counter.enabled = false;
		}
		if (m_Counter != null)
		{
			Data.m_CurTime = m_Counter.CurCounter;
			Data.m_Time = m_Counter.FinalCounter;
		}
		else
		{
			Data.m_CurTime = -1f;
			Data.m_Time = -1f;
		}
	}

	public void StartTreatingNpc(PeEntity npc)
	{
		StartCounter(npc);
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		DestroySomeData();
	}

	public override void DestroySomeData()
	{
		foreach (PeEntity allPatient in allPatients)
		{
			allPatient.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		}
		allPatients.Clear();
		Data.npcIds.Clear();
		isNpcReady = false;
		occupied = false;
		if (medicineItem != null)
		{
			Data.m_ObjID = medicineItem.instanceId;
		}
		else
		{
			Data.m_ObjID = -1;
		}
		StopCounter();
		if (base.IsMine)
		{
			UnbindEvent();
		}
	}

	public override void UpdateDataToUI()
	{
		if (base.IsMine)
		{
			BindEvent();
			uiObj.SetTreatIcon();
			uiObj.TreatDoc = m_Workers[0];
			UpdatePatientIcon();
			uiObj.SetLocalGrid(medicineItem);
		}
	}

	public float GetTreatTime(PeEntity npc)
	{
		if (treatmentInUse.npcId != npc.Id)
		{
			treatmentInUse = base.m_MgCreator.FindTreatment(npc.Id, needTreat: true);
		}
		if (treatmentInUse != null)
		{
			return treatmentInUse.treatTime;
		}
		return 0f;
	}

	public override bool IsDoingYou(PeEntity npc)
	{
		if (allPatients.Count == 0)
		{
			return false;
		}
		if (allPatients[0] == npc)
		{
			return true;
		}
		return false;
	}

	public override List<ItemIdCount> GetRequirements()
	{
		return base.GetRequirements();
	}

	public void AddNpcResult(List<int> npcIds)
	{
		allPatients.Clear();
		foreach (int npcId in npcIds)
		{
			PeEntity item = PeSingleton<EntityMgr>.Instance.Get(npcId);
			allPatients.Add(item);
		}
		Data.npcIds = npcIds;
	}

	public void RemoveDeadPatientResult(int npcId)
	{
		base.RemoveDeadPatient(npcId);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		if (allPatients.Count == 0 || !allPatients.Contains(peEntity))
		{
			return;
		}
		if (allPatients[0].Id == npcId)
		{
			allPatients.RemoveAt(0);
			StopCounter();
			isNpcReady = false;
			occupied = false;
		}
		else
		{
			allPatients.RemoveAll((PeEntity it) => it.Id == npcId);
		}
		Data.npcIds.Remove(npcId);
		peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
	}

	public void TryStartResult(int npcId)
	{
		isNpcReady = true;
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Treating;
	}

	public void StartCounterResult()
	{
		StartCounter(allPatients[0]);
	}

	public void SetTreat(CSTreatment tInUse)
	{
		allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		treatmentInUse = base.m_MgCreator.GetTreatment(tInUse.abnormalId, tInUse.npcId, tInUse.needTreatTimes);
		if (treatmentInUse == null)
		{
			Debug.LogError("treatmentInUse == null!");
			return;
		}
		SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
		ShowMedicineNeed(treatmentInUse.medicineList[0]);
	}

	public void SetItemResult(int objId, bool inorout)
	{
		if (objId != -1)
		{
			medicineItem = PeSingleton<ItemMgr>.Instance.Get(objId);
		}
		else
		{
			medicineItem = null;
		}
		SetMedicineIcon(medicineItem, inorout);
		Data.m_ObjID = objId;
	}

	public void DeleteMedicine(int instanceId)
	{
		if (medicineItem != null && medicineItem.instanceId == instanceId)
		{
			SetMedicineIcon(null, inOrOut: false);
			PeSingleton<ItemMgr>.Instance.DestroyItem(instanceId);
			medicineItem = null;
			Data.m_ObjID = -1;
		}
	}

	public void TreatFinish(int npcId, bool treatSuccess)
	{
		StopCounter();
		if (treatSuccess)
		{
			if (treatmentInUse != null)
			{
				treatmentInUse.needTreatTimes--;
				if (treatmentInUse.needTreatTimes > 0)
				{
					allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
				}
				else
				{
					allPatients[0].GetCmpt<NpcCmpt>().CureSick((PEAbnormalType)treatmentInUse.abnormalId);
					base.m_MgCreator.UpdateTreatment();
					if (base.m_MgCreator.FindTreatment(allPatients[0].Id) == null)
					{
						allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
					}
					else
					{
						allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
					}
				}
			}
			else
			{
				allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
			}
		}
		else
		{
			base.m_MgCreator.RemoveNpcTreatment(allPatients[0].Id);
			allPatients[0].GetCmpt<NpcCmpt>().AddSick(PEAbnormalType.MedicalAccident);
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
		}
		treatmentInUse = null;
		allPatients.RemoveAt(0);
		Data.npcIds.RemoveAt(0);
		isNpcReady = false;
		if (allPatients.Count >= 1)
		{
			AddPatientToUI(allPatients[0]);
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		}
		else
		{
			RemovePatientFromUI();
		}
		RefreshTreatment();
	}

	public void ResetNpcToCheck(int npcId)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		if (peEntity != null)
		{
			peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
		}
		allPatients.RemoveAll((PeEntity it) => it.Id == npcId);
		Data.npcIds.Remove(npcId);
		UpdataPatientToUI();
	}
}
