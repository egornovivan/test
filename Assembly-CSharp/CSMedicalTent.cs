using System.Collections.Generic;
using System.IO;
using CSRecord;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSMedicalTent : CSHealth
{
	public const int WORKER_AMOUNT_MAX = 1;

	public CSUI_Hospital uiObj;

	private CounterScript m_Counter;

	public CSTentInfo m_TInfo;

	private CSTentData m_TData;

	private Sickbed[] allSickbeds
	{
		get
		{
			return Data.allSickbeds;
		}
		set
		{
			Data.allSickbeds = value;
		}
	}

	public List<CSPersonnel> patientsInTent
	{
		get
		{
			List<CSPersonnel> list = new List<CSPersonnel>();
			Sickbed[] array = allSickbeds;
			foreach (Sickbed sickbed in array)
			{
				if (sickbed.Npc != null)
				{
					CSPersonnel npc = m_Creator.GetNpc(sickbed.Npc.Id);
					list.Add(npc);
				}
			}
			return list;
		}
	}

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
			if (pePatient != null)
			{
				for (int k = 0; k < allSickbeds.Length; k++)
				{
					if (allSickbeds[k] != null)
					{
						allSickbeds[k].bedLay = pePatient.Lays[k];
					}
				}
			}
			if (!(BuildingLogic != null))
			{
				return;
			}
			workTrans = BuildingLogic.m_WorkTrans;
			resultTrans = BuildingLogic.m_ResultTrans;
			for (int l = 0; l < 8; l++)
			{
				if (allSickbeds[l] != null)
				{
					allSickbeds[l].bedTrans = resultTrans[l];
				}
			}
		}
	}

	public CSBuildingLogic BuildingLogic => gameLogic.GetComponent<CSBuildingLogic>();

	public CSTentInfo Info
	{
		get
		{
			if (m_TInfo == null)
			{
				m_TInfo = m_Info as CSTentInfo;
			}
			return m_TInfo;
		}
	}

	public CSTentData Data
	{
		get
		{
			if (m_TData == null)
			{
				m_TData = m_Data as CSTentData;
			}
			return m_TData;
		}
	}

	public bool IsTent => allPatients.Count >= 8;

	public CSMedicalTent(CSCreator creator)
	{
		m_Creator = creator;
		m_Type = 14;
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
		return base.IsRunning && IsTent;
	}

	public override bool AddWorker(CSPersonnel npc)
	{
		if (base.AddWorker(npc))
		{
			SetNurseIcon(npc);
			return true;
		}
		return false;
	}

	public override void RemoveWorker(CSPersonnel npc)
	{
		base.RemoveWorker(npc);
		SetNurseIcon(null);
	}

	public override void AfterTurn90Degree()
	{
		base.AfterTurn90Degree();
		Sickbed[] array = allSickbeds;
		foreach (Sickbed sickbed in array)
		{
			sickbed.isNpcReady = false;
			sickbed.occupied = false;
		}
	}

	private void BindEvent()
	{
		if (uiObj == null && CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.HospitalUI != null)
		{
			uiObj = CSUI_MainWndCtrl.Instance.HospitalUI;
		}
	}

	private void UnbindEvent()
	{
		uiObj = null;
	}

	private void SetNurseIcon(CSPersonnel p)
	{
		if (uiObj != null)
		{
			uiObj.Nurse = p;
		}
	}

	private void RefreshPatientGrids(List<CSPersonnel> pl)
	{
		if (uiObj != null)
		{
			uiObj.RefreshPatientGrids(pl);
		}
	}

	private void UpdataGridTime(CSPersonnel p, float timeLeft)
	{
		if (uiObj != null)
		{
			uiObj.UpdateNpcGridTime(p, timeLeft);
		}
	}

	public Sickbed FindEmptyBed(PeEntity npc)
	{
		Sickbed[] array = allSickbeds;
		foreach (Sickbed sickbed in array)
		{
			if (sickbed.Npc == npc)
			{
				return sickbed;
			}
		}
		Sickbed[] array2 = allSickbeds;
		foreach (Sickbed sickbed2 in array2)
		{
			if (sickbed2.Npc == null)
			{
				sickbed2.Npc = npc;
				RefreshPatientGrids(patientsInTent);
				return sickbed2;
			}
		}
		return null;
	}

	public bool IsReady(PeEntity npc, out Sickbed sickBed)
	{
		sickBed = FindEmptyBed(npc);
		return sickBed != null;
	}

	public void AppointTent(PeEntity npc)
	{
		if (!allPatients.Contains(npc))
		{
			allPatients.Add(npc);
			Data.npcIds.Add(npc.Id);
		}
	}

	public override void RemoveDeadPatient(int npcId)
	{
		if (PeGameMgr.IsMulti)
		{
			base._Net.RPCServer(EPacketType.PT_CL_TET_RemoveDeadNpc, npcId);
			return;
		}
		base.RemoveDeadPatient(npcId);
		PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(npcId);
		if (allPatients.Count == 0 || !allPatients.Contains(npc))
		{
			return;
		}
		if (allPatients.FindIndex((PeEntity it) => it == npc) < 8)
		{
			allPatients.Remove(npc);
			for (int i = 0; i < 8; i++)
			{
				if (allSickbeds[i].Npc == npc)
				{
					allSickbeds[i].Npc = null;
					allSickbeds[i].StopCounter();
					allSickbeds[i].isNpcReady = false;
					allSickbeds[i].occupied = false;
					RefreshPatientGrids(patientsInTent);
				}
			}
			if (allPatients.Count >= 8)
			{
				allPatients[7].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
			}
		}
		else
		{
			allPatients.Remove(npc);
		}
		Data.npcIds.Remove(npc.Id);
		npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
	}

	public bool StartTent(PeEntity npc)
	{
		if (allPatients.Count <= 0)
		{
			return false;
		}
		Sickbed sickbed = null;
		bool flag = false;
		for (int i = 0; i < 8; i++)
		{
			if (allSickbeds[i].Npc == npc)
			{
				flag = true;
				sickbed = allSickbeds[i];
			}
		}
		if (!flag)
		{
			return false;
		}
		if (!CallDoctor())
		{
		}
		sickbed.isNpcReady = true;
		npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.In_Hospital;
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

	private float CountFinalTime(PeEntity npc)
	{
		if (Application.isEditor)
		{
			return 15f;
		}
		float restoreTime = GetRestoreTime(npc);
		return restoreTime * (1f + m_Workers[0].GetTentTimeSkill);
	}

	public void StartCounter(int index, PeEntity npc)
	{
		float finalTime = CountFinalTime(npc);
		StartCounter(0f, finalTime, index);
	}

	public void StartCounter(float curTime, float finalTime, int index)
	{
		allSickbeds[index].m_Time = finalTime;
		allSickbeds[index].StartCounter(finalTime);
	}

	public void OnTentFinish(Sickbed sickbed)
	{
		CSMain.Instance.RemoveCounter(sickbed.cs);
		if (GameConfig.IsMultiMode)
		{
			sickbed.isNpcReady = false;
			return;
		}
		PeEntity npc = sickbed.Npc;
		sickbed.Npc = null;
		sickbed.isNpcReady = false;
		if (base.m_MgCreator.FindTreatment(npc.Id) != null)
		{
			npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		}
		else
		{
			npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
		}
		allPatients.Remove(npc);
		Data.npcIds.Remove(npc.Id);
		if (allPatients.Count >= 8)
		{
			allPatients[7].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
		}
		RefreshPatientGrids(patientsInTent);
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 14, ref refData) : MultiColonyManager.Instance.AssignData(ID, 14, ref refData, _ColonyObj));
		m_Data = refData as CSTentData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			for (int i = 0; i < allSickbeds.Length; i++)
			{
				allSickbeds[i].tentBuilding = this;
				if (pePatient != null)
				{
					allSickbeds[i].bedLay = pePatient.Lays[i];
				}
				if (resultTrans != null)
				{
					allSickbeds[i].bedTrans = resultTrans[i];
				}
			}
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
		for (int j = 0; j < 8; j++)
		{
			Sickbed sickbed = allSickbeds[j];
			if (sickbed.npcId >= 0)
			{
				sickbed.npc = PeSingleton<EntityMgr>.Instance.Get(sickbed.npcId);
			}
			else
			{
				sickbed.npc = null;
			}
			sickbed.StartCounterFromRecord();
			sickbed.tentBuilding = this;
			if (pePatient != null)
			{
				sickbed.bedLay = pePatient.Lays[j];
			}
			if (resultTrans != null)
			{
				sickbed.bedTrans = resultTrans[j];
			}
		}
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
				if (num < 8)
				{
					RefreshPatientGrids(patientsInTent);
				}
			}
		}
	}

	public override void Update()
	{
		base.Update();
		CheckPatient();
		if (base.IsRunning && IsDoctorReady())
		{
			for (int i = 0; i < 8; i++)
			{
				if (allSickbeds[i].IsPatientReady())
				{
					if (allSickbeds[i].cs == null)
					{
						StartCounter(i, allSickbeds[i].npc);
					}
					if (allSickbeds[i].cs != null)
					{
						allSickbeds[i].cs.enabled = true;
						UpdataGridTime(allSickbeds[i].csp, allSickbeds[i].TimeLeft);
					}
				}
				else
				{
					if (allSickbeds[i].cs != null)
					{
						allSickbeds[i].cs.enabled = false;
					}
					if (allSickbeds[i].csp != null)
					{
						UpdataGridTime(allSickbeds[i].csp, allSickbeds[i].TimeLeft);
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < 8; j++)
			{
				if (allSickbeds[j].cs != null)
				{
					allSickbeds[j].cs.enabled = false;
				}
			}
		}
		for (int k = 0; k < 8; k++)
		{
			allSickbeds[k].Update();
		}
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
	}

	public override void UpdateDataToUI()
	{
		if (base.IsMine)
		{
			BindEvent();
			uiObj.SetTentIcon();
			SetNurseIcon(m_Workers[0]);
			RefreshPatientGrids(patientsInTent);
		}
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
			allPatient.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
		}
		allPatients.Clear();
		Data.npcIds.Clear();
		for (int i = 0; i < 8; i++)
		{
			allSickbeds[i].Npc = null;
			allSickbeds[i].StopCounter();
		}
		if (base.IsMine)
		{
			UnbindEvent();
		}
	}

	public float GetRestoreTime(PeEntity npc)
	{
		return base.m_MgCreator.FindTreatment(npc.Id, needTreat: true)?.restoreTime ?? 0f;
	}

	public override bool IsDoingYou(PeEntity npc)
	{
		if (allPatients.Count == 0)
		{
			return false;
		}
		if (allPatients.FindIndex((PeEntity it) => it == npc) < 8)
		{
			return true;
		}
		return false;
	}

	public void AddNpcResult(List<int> npcIds, int npcid, int index)
	{
		allPatients.Clear();
		foreach (int npcId in npcIds)
		{
			PeEntity item = PeSingleton<EntityMgr>.Instance.Get(npcId);
			allPatients.Add(item);
		}
		if (index >= 0)
		{
			allSickbeds[index].Npc = PeSingleton<EntityMgr>.Instance.Get(npcid);
		}
		Data.npcIds = npcIds;
	}

	public Sickbed CheckNpcBed(PeEntity npc)
	{
		Sickbed[] array = allSickbeds;
		foreach (Sickbed sickbed in array)
		{
			if (sickbed.Npc == npc)
			{
				RefreshPatientGrids(patientsInTent);
				return sickbed;
			}
		}
		return null;
	}

	public Sickbed CheckNpcBed(int npcid)
	{
		Sickbed[] array = allSickbeds;
		foreach (Sickbed sickbed in array)
		{
			if (sickbed.npcId == npcid)
			{
				RefreshPatientGrids(patientsInTent);
				return sickbed;
			}
		}
		return null;
	}

	public void RemoveDeadPatientResult(int npcId)
	{
		base.RemoveDeadPatient(npcId);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		if (allPatients.Count == 0 || !allPatients.Contains(peEntity))
		{
			return;
		}
		allPatients.Remove(peEntity);
		Data.npcIds.Remove(peEntity.Id);
		for (int i = 0; i < 8; i++)
		{
			if (allSickbeds[i].Npc == peEntity)
			{
				allSickbeds[i].Npc = null;
				allSickbeds[i].StopCounter();
				allSickbeds[i].isNpcReady = false;
				allSickbeds[i].occupied = false;
				RefreshPatientGrids(patientsInTent);
			}
		}
		peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
	}

	public void TryStartResult(int npcId)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		Sickbed[] array = allSickbeds;
		foreach (Sickbed sickbed in array)
		{
			if (sickbed.npc == peEntity)
			{
				sickbed.isNpcReady = true;
				peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.In_Hospital;
				break;
			}
		}
	}

	public void SetTent(int npcId)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
	}

	public void TentFinish(int npcId)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		Sickbed sickbed = CheckNpcBed(npcId);
		sickbed.Npc = null;
		sickbed.isNpcReady = false;
		if (base.m_MgCreator.FindTreatment(peEntity.Id) != null)
		{
			peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		}
		else
		{
			peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
		}
		allPatients.Remove(peEntity);
		Data.npcIds.Remove(peEntity.Id);
		if (allPatients.Count >= 8)
		{
			allPatients[7].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
		}
		RefreshPatientGrids(patientsInTent);
	}

	public static void ParseData(byte[] data, CSTentData recordData)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			recordData.npcIds.Add(BufferHelper.ReadInt32(reader));
		}
		for (int j = 0; j < 8; j++)
		{
			Sickbed sickbed = recordData.allSickbeds[j];
			sickbed.npcId = BufferHelper.ReadInt32(reader);
			sickbed.m_CurTime = BufferHelper.ReadSingle(reader);
			sickbed.m_Time = BufferHelper.ReadSingle(reader);
			sickbed.isNpcReady = BufferHelper.ReadBoolean(reader);
			sickbed.occupied = BufferHelper.ReadBoolean(reader);
		}
	}
}
