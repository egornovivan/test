using System;
using System.Collections.Generic;
using CSRecord;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSMedicalCheck : CSHealth
{
	private const float DEFAULT_CHECK_CHANGCE = 0.9f;

	public const int WORKER_AMOUNT_MAX = 1;

	public CSUI_Hospital uiObj;

	public PeEntity lastCheckNpc;

	private CounterScript m_Counter;

	public CSCheckInfo m_TInfo;

	private CSCheckData m_TData;

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

	public CSCheckInfo Info
	{
		get
		{
			if (m_TInfo == null)
			{
				m_TInfo = m_Info as CSCheckInfo;
			}
			return m_TInfo;
		}
	}

	public CSCheckData Data
	{
		get
		{
			if (m_TData == null)
			{
				m_TData = m_Data as CSCheckData;
			}
			return m_TData;
		}
	}

	public bool IsChecking => m_Counter != null;

	public CSMedicalCheck(CSCreator creator)
	{
		m_Creator = creator;
		m_Type = 12;
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
		return base.IsRunning && IsChecking;
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
		}
	}

	private void UnbindEvent()
	{
		uiObj = null;
	}

	private void SetDoctorIcon(CSPersonnel p)
	{
		if (uiObj != null)
		{
			uiObj.ExamineDoc = p;
		}
	}

	private void SetPatientIcon(CSPersonnel p)
	{
		if (uiObj != null)
		{
			uiObj.ExaminedPatient = p;
		}
	}

	private void UpdateTimeShow(float timeLeft)
	{
		if (uiObj != null)
		{
			uiObj.CheckTimeShow(timeLeft);
		}
	}

	private void RefreshTreatment()
	{
		if (uiObj != null)
		{
			uiObj.RefreshGrid();
		}
	}

	private float CountFinalTime(PeEntity npc)
	{
		if (Application.isEditor)
		{
			return 15f;
		}
		float diagnoseTime = GetDiagnoseTime(npc);
		return diagnoseTime * (1f + m_Workers[0].GetDiagnoseTimeSkill);
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
				m_Counter = CSMain.Instance.CreateCounter("MedicalCheck", curTime, finalTime);
			}
			else
			{
				m_Counter.Init(curTime, finalTime);
			}
			m_Counter.OnTimeUp = OnCheckFinish;
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

	public bool AppointCheck(PeEntity npc)
	{
		if (npc.GetCmpt<NpcCmpt>().illAbnormals == null || npc.GetCmpt<NpcCmpt>().illAbnormals.Count <= 0)
		{
			return false;
		}
		if (!allPatients.Contains(npc))
		{
			allPatients.Add(npc);
			Data.npcIds.Add(npc.Id);
			if (allPatients.Count == 1)
			{
				UpdatePatientIcon();
			}
		}
		return true;
	}

	public void GetCheck()
	{
		if (allPatients.Count > 0 && !(m_Counter != null))
		{
		}
	}

	public bool StartCheck(PeEntity npc)
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
		npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Diagnosing;
		return true;
	}

	public bool IsPatientReady()
	{
		return allPatients.Count > 0 && allPatients[0].GetCmpt<NpcCmpt>().MedicalState == ENpcMedicalState.Diagnosing;
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

	public void CheckPatient()
	{
		if (Data == null || Data.npcIds == null || allPatients == null)
		{
			return;
		}
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
			if (m_Creator != null && m_Creator.GetNpc(allPatients[num].Id) == null)
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

	public void OnCheckFinish()
	{
		Data.m_CurTime = -1f;
		Data.m_Time = -1f;
		CSMain.Instance.RemoveCounter(m_Counter);
		if (GameConfig.IsMultiMode)
		{
			isNpcReady = false;
			return;
		}
		PeEntity peEntity = allPatients[0];
		List<CSTreatment> list = new List<CSTreatment>();
		System.Random random = new System.Random();
		if (random.NextDouble() > (double)(0.9f + m_Workers[0].GetDiagnoseChanceSkill))
		{
			list.Add(CSTreatment.GetRandomTreatment(peEntity));
		}
		else
		{
			list = CSTreatment.CreateTreatment(peEntity);
		}
		base.m_MgCreator.RemoveNpcTreatment(peEntity.Id);
		if (list != null && list.Count > 0)
		{
			base.m_MgCreator.AddTreatment(list);
			peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		}
		else
		{
			peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
		}
		allPatients.RemoveAt(0);
		Data.npcIds.RemoveAt(0);
		isNpcReady = false;
		if (allPatients.Count >= 1)
		{
			SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
		}
		else
		{
			SetPatientIcon(null);
		}
		RefreshTreatment();
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 12, ref refData) : MultiColonyManager.Instance.AssignData(ID, 12, ref refData, _ColonyObj));
		m_Data = refData as CSCheckData;
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
		StartCounter(Data.m_CurTime, Data.m_Time);
	}

	public override void Update()
	{
		base.Update();
		CheckPatient();
		if (base.IsRunning && IsDoctorReady() && IsPatientReady())
		{
			if (lastCheckNpc != allPatients[0])
			{
				StopCounter();
				lastCheckNpc = allPatients[0];
				UpdateTimeShow(0f);
			}
			else
			{
				if (m_Counter == null)
				{
					StartCheckingNpc(allPatients[0]);
				}
				if (m_Counter != null)
				{
					m_Counter.enabled = true;
					UpdateTimeShow(m_Counter.FinalCounter - m_Counter.CurCounter);
				}
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

	public void StartCheckingNpc(PeEntity npc)
	{
		StartCounter(npc);
		SetPatientIcon(m_Creator.GetNpc(npc.Id));
	}

	public void UpdatePatientIcon()
	{
		if (allPatients.Count > 0)
		{
			SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
		}
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
			allPatient.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
		}
		allPatients.Clear();
		Data.npcIds.Clear();
		isNpcReady = false;
		occupied = false;
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
			uiObj.SetCheckIcon();
			SetDoctorIcon(m_Workers[0]);
			if (allPatients.Count > 0)
			{
				SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
			}
			else
			{
				SetPatientIcon(null);
			}
		}
	}

	public float GetDiagnoseTime(PeEntity npc)
	{
		List<PEAbnormalType> illAbnormals = npc.GetCmpt<NpcCmpt>().illAbnormals;
		if (illAbnormals == null || illAbnormals.Count == 0)
		{
			return 0f;
		}
		float num = AbnormalTypeTreatData.GetDiagnosingTime((int)illAbnormals[0]);
		for (int i = 1; i < illAbnormals.Count; i++)
		{
			float diagnosingTime = AbnormalTypeTreatData.GetDiagnosingTime((int)illAbnormals[i]);
			if (diagnosingTime > num)
			{
				num = diagnosingTime;
			}
		}
		return num;
	}

	public override void RemoveDeadPatient(int npcId)
	{
		if (PeGameMgr.IsMulti)
		{
			base._Net.RPCServer(EPacketType.PT_CL_CHK_RemoveDeadNpc, npcId);
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
				allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
			}
		}
		else
		{
			allPatients.RemoveAll((PeEntity it) => it.Id == npcId);
		}
		Data.npcIds.Remove(npcId);
		peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
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

	public void TryStartResult(int npcId)
	{
		isNpcReady = true;
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Diagnosing;
	}

	public void SetDiagnose()
	{
		allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
		UpdatePatientIcon();
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

	public void CheckFinish(int npcId, List<CSTreatment> csts)
	{
		StopCounter();
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcId);
		int teamId = _ColonyObj._Network.TeamId;
		CSMgCreator creator = MultiColonyManager.GetCreator(teamId);
		creator.RemoveNpcTreatment(npcId);
		if (csts != null && csts.Count > 0)
		{
			creator.AddTreatment(csts);
			peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
		}
		else
		{
			peEntity.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
		}
		allPatients.RemoveAll((PeEntity it) => it.Id == npcId);
		Data.npcIds.Remove(npcId);
		isNpcReady = false;
		if (allPatients.Count >= 1)
		{
			SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
		}
		else
		{
			SetPatientIcon(null);
		}
		RefreshTreatment();
	}
}
