using System;
using System.Collections.Generic;
using System.IO;
using CSRecord;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSTraining : CSElectric
{
	public const int WORKER_AMOUNT_MAX = 24;

	public const int MAX_INSTRUCTOR_NUM = 8;

	public const int MAX_TRAINEE_NUM = 8;

	public const int MAX_SKILL_COUNT = 5;

	private CounterScript m_Counter;

	private CSPersonnel csp_instructorNpc;

	private CSPersonnel csp_traineeNpc;

	public CSUI_NpcInstructor uiObjSet;

	public CSUI_TrainMgr uiObjTrain;

	public CSTrainingInfo m_TInfo;

	private CSTrainData m_TData;

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
			PETrainner component2 = gameLogic.GetComponent<PETrainner>();
			if (component2 != null)
			{
				for (int j = 0; j < m_WorkSpaces.Length; j++)
				{
					m_WorkSpaces[j].TrainerMachine = component2;
				}
			}
			if (BuildingLogic != null)
			{
				workTrans = BuildingLogic.m_WorkTrans;
				resultTrans = BuildingLogic.m_ResultTrans;
			}
		}
	}

	public CSBuildingLogic BuildingLogic => gameLogic.GetComponent<CSBuildingLogic>();

	public CSTrainingInfo Info
	{
		get
		{
			if (m_TInfo == null)
			{
				m_TInfo = m_Info as CSTrainingInfo;
			}
			return m_TInfo;
		}
	}

	public CSTrainData Data
	{
		get
		{
			if (m_TData == null)
			{
				m_TData = m_Data as CSTrainData;
			}
			return m_TData;
		}
	}

	public List<int> InstructorList => Data.instructors;

	public List<int> TraineeList => Data.trainees;

	public ETrainingType trainingType
	{
		get
		{
			return (ETrainingType)Data.trainingType;
		}
		set
		{
			Data.trainingType = (int)value;
		}
	}

	public int InstructorNpcId
	{
		get
		{
			return Data.instructorNpcId;
		}
		set
		{
			Data.instructorNpcId = value;
		}
	}

	public int TraineeNpcId
	{
		get
		{
			return Data.traineeNpcId;
		}
		set
		{
			Data.traineeNpcId = value;
		}
	}

	public List<int> LearningSkills => Data.LearningSkillIds;

	public float m_CurTime
	{
		get
		{
			return Data.m_CurTime;
		}
		set
		{
			Data.m_CurTime = value;
		}
	}

	public float m_Time
	{
		get
		{
			return Data.m_Time;
		}
		set
		{
			Data.m_Time = value;
		}
	}

	public bool IsTrainning => m_Time > 0f;

	public CSTraining(CSCreator creator)
	{
		m_Creator = creator;
		m_Type = 11;
		m_Workers = new CSPersonnel[24];
		m_WorkSpaces = new PersonnelSpace[24];
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
		return base.IsRunning && IsTrainning;
	}

	private void BindEvent()
	{
		if (uiObjSet == null && GameUI.Instance != null && GameUI.Instance.mCSUI_MainWndCtrl != null && GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI != null && GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NpcInstructor != null && (bool)GameUI.Instance.mCSUI_MainWndCtrl.TrainUI)
		{
			uiObjSet = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NpcInstructor;
			uiObjSet.onInstructorClicked += SetInstructor;
			uiObjSet.onTraineeClicked += SetTrainee;
			uiObjSet.onLabelChanged += SetCount;
			uiObjTrain = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI;
			uiObjTrain.OnStartTrainingEvent += OnStartTraining;
			uiObjTrain.OnStopTrainingEvent += OnStopTraining;
		}
	}

	private void UnbindEvent()
	{
		if (uiObjSet != null)
		{
			uiObjSet.onInstructorClicked -= SetInstructor;
			uiObjSet.onTraineeClicked -= SetTrainee;
			uiObjSet.onLabelChanged -= SetCount;
		}
		if (uiObjTrain != null)
		{
			uiObjTrain.OnStartTrainingEvent -= OnStartTraining;
			uiObjTrain.OnStopTrainingEvent -= OnStopTraining;
		}
		uiObjSet = null;
		uiObjTrain = null;
	}

	private void LockUI(bool flag)
	{
		if (uiObjTrain != null)
		{
			uiObjTrain.mTrainingLock = flag;
		}
	}

	private void SetTrainingTime(float time)
	{
		if (uiObjTrain != null)
		{
			uiObjTrain.LearnSkillTimeShow(time);
		}
	}

	private void PreviewTrainingTime()
	{
		if (uiObjTrain != null)
		{
			float num = 0f;
			num = ((uiObjTrain.m_TrainLearnPageCtrl.TrainingType != 0) ? CountAttributeFinalTime() : CountSkillFinalTime(uiObjTrain.GetStudyList()));
			uiObjTrain.LearnSkillTimeShow(num);
		}
	}

	private void UpdateUI()
	{
		if (uiObjTrain != null)
		{
			uiObjTrain.RefreshAfterTraining(base.m_MgCreator.GetNpc(InstructorNpcId), base.m_MgCreator.GetNpc(TraineeNpcId));
		}
	}

	public void SetStopBtn(bool flag)
	{
		if (uiObjTrain != null)
		{
			uiObjTrain.SetBtnState(flag);
		}
	}

	public void SetInstructorNpcUI(int instructorId)
	{
		if (uiObjTrain != null)
		{
			uiObjTrain.m_TrainLearnPageCtrl.InsNpc = base.m_MgCreator.GetNpc(instructorId);
		}
	}

	public void SetTraineeNpcUI(int traineeId)
	{
		if (uiObjTrain != null)
		{
			uiObjTrain.m_TrainLearnPageCtrl.TraineeNpc = base.m_MgCreator.GetNpc(traineeId);
		}
	}

	public void SetLearnSkillListUI(List<int> skillIds)
	{
		if (uiObjTrain != null)
		{
			uiObjTrain.SetStudyListInterface(skillIds);
		}
	}

	private void OnStartTraining(ETrainingType ttype, List<int> skillIds, CSPersonnel instructorNpc, CSPersonnel traineeNpc)
	{
		if (!base.IsRunning || instructorNpc == null || traineeNpc == null || m_Counter != null || !CheckInstructorAndTraineeId(instructorNpc.ID, traineeNpc.ID) || !CheckNpcState(instructorNpc, traineeNpc) || (ttype == ETrainingType.Skill && (skillIds == null || skillIds.Count == 0)) || (ttype == ETrainingType.Attribute && !traineeNpc.CanUpgradeAttribute()))
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			switch (ttype)
			{
			case ETrainingType.Skill:
				base._Net.RPCServer(EPacketType.PT_CL_TRN_StartSkillTraining, skillIds.ToArray(), instructorNpc.ID, traineeNpc.ID);
				break;
			case ETrainingType.Attribute:
				base._Net.RPCServer(EPacketType.PT_CL_TRN_StartAttributeTraining, instructorNpc.ID, traineeNpc.ID);
				break;
			}
		}
		else
		{
			switch (ttype)
			{
			case ETrainingType.Skill:
				StartSkillCounter(skillIds);
				break;
			case ETrainingType.Attribute:
				StartAttributeCounter();
				break;
			}
			instructorNpc.trainingType = ttype;
			traineeNpc.trainingType = ttype;
			trainingType = ttype;
			InstructorNpcId = instructorNpc.ID;
			TraineeNpcId = traineeNpc.ID;
			instructorNpc.IsTraining = true;
			traineeNpc.IsTraining = true;
			SetStopBtn(flag: true);
		}
		if (base.m_MgCreator.GetNpc(InstructorNpcId) != null && base.m_MgCreator.GetNpc(TraineeNpcId) != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(8000896), base.m_MgCreator.GetNpc(InstructorNpcId).FullName, base.m_MgCreator.GetNpc(TraineeNpcId).FullName));
		}
	}

	public void OnStopTraining()
	{
		if (base.IsRunning)
		{
			if (PeGameMgr.IsMulti)
			{
				base._Net.RPCServer(EPacketType.PT_CL_TRN_StopTraining);
			}
			else
			{
				StopCounter();
			}
		}
	}

	public void SetInstructor(CSPersonnel p)
	{
		if (base.IsRunning && CheckNpcState(p))
		{
			if (PeGameMgr.IsMulti)
			{
				base._Net.RPCServer(EPacketType.PT_CL_TRN_SetInstructor, p.ID);
			}
			else
			{
				AddInstructor(p);
			}
		}
	}

	public void SetTrainee(CSPersonnel p)
	{
		if (base.IsRunning && CheckNpcState(p))
		{
			if (PeGameMgr.IsMulti)
			{
				base._Net.RPCServer(EPacketType.PT_CL_TRN_SetTrainee, p.ID);
			}
			else
			{
				AddTrainee(p);
			}
		}
	}

	public void SetCount()
	{
		if (base.IsRunning && uiObjSet != null)
		{
			uiObjSet.SetCountLabel(InstructorList.Count, 8, TraineeList.Count, 8);
		}
	}

	public bool AddInstructor(CSPersonnel p)
	{
		if (InstructorList.Contains(p.ID))
		{
			return true;
		}
		if (InstructorList.Count >= 8)
		{
			return false;
		}
		if (TraineeNpcId == p.ID)
		{
			if (m_Counter != null)
			{
				StopCounter();
			}
			TraineeNpcId = -1;
		}
		TraineeList.Remove(p.ID);
		InstructorList.Add(p.ID);
		p.trainerType = ETrainerType.Instructor;
		UpdateUI();
		return true;
	}

	public bool AddTrainee(CSPersonnel p)
	{
		if (TraineeList.Contains(p.ID))
		{
			return true;
		}
		if (TraineeList.Count >= 8)
		{
			return false;
		}
		if (InstructorNpcId == p.ID)
		{
			if (m_Counter != null)
			{
				StopCounter();
			}
			InstructorNpcId = -1;
		}
		InstructorList.Remove(p.ID);
		TraineeList.Add(p.ID);
		p.trainerType = ETrainerType.Trainee;
		UpdateUI();
		return true;
	}

	public void SetNpcIsTraining(bool flag)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(InstructorNpcId);
		CSPersonnel npc2 = base.m_MgCreator.GetNpc(TraineeNpcId);
		if (npc != null)
		{
			npc.IsTraining = flag;
			npc.trainingType = trainingType;
			if (!flag)
			{
				InstructorNpcId = -1;
			}
		}
		else
		{
			InstructorNpcId = -1;
		}
		if (npc2 != null)
		{
			npc2.IsTraining = flag;
			npc2.trainingType = trainingType;
			if (!flag)
			{
				TraineeNpcId = -1;
			}
		}
		else
		{
			TraineeNpcId = -1;
		}
	}

	private float CountSkillFinalTime(List<int> skillIds)
	{
		float num = 0f;
		foreach (int skillId in skillIds)
		{
			num += NpcAblitycmpt.GetLearnTime(skillId);
		}
		return num;
	}

	private float CountAttributeFinalTime()
	{
		return Info.m_BaseTime;
	}

	public void StartSkillCounter(List<int> skillIds)
	{
		float finalTime = CountSkillFinalTime(skillIds);
		Data.LearningSkillIds = skillIds;
		StartCounter(0f, finalTime, ETrainingType.Skill);
	}

	public void StartAttributeCounter()
	{
		float finalTime = CountAttributeFinalTime();
		StartCounter(0f, finalTime, ETrainingType.Attribute);
	}

	public void StartCounterFromRecord(float curTime, float finalTime)
	{
		StartCounter(curTime, finalTime, trainingType);
	}

	public void StartCounter(float curTime, float finalTime, ETrainingType ttype)
	{
		if (finalTime < 0f)
		{
			return;
		}
		LockUI(flag: true);
		if (m_Counter == null)
		{
			m_Counter = CSMain.Instance.CreateCounter("Train", curTime, finalTime);
		}
		else
		{
			m_Counter.Init(curTime, finalTime);
		}
		if (!GameConfig.IsMultiMode)
		{
			switch (ttype)
			{
			case ETrainingType.Skill:
				m_Counter.OnTimeUp = OnLearnSkillFinish;
				break;
			case ETrainingType.Attribute:
				m_Counter.OnTimeUp = OnTrainAttributeFinish;
				break;
			}
		}
	}

	public void StopCounter()
	{
		Data.m_CurTime = -1f;
		Data.m_Time = -1f;
		CSMain.Instance.DestoryCounter(m_Counter);
		m_Counter = null;
		SetNpcIsTraining(flag: false);
		LockUI(flag: false);
		SetStopBtn(flag: false);
	}

	public void OnLearnSkillFinish()
	{
		m_CurTime = -1f;
		m_Time = -1f;
		CSMain.Instance.RemoveCounter(m_Counter);
		if (PeGameMgr.IsMulti)
		{
			return;
		}
		CSPersonnel npc = base.m_MgCreator.GetNpc(TraineeNpcId);
		Ablities getNpcAllSkill = npc.GetNpcAllSkill;
		Ablities coverSkill = new Ablities();
		foreach (int learningSkill in LearningSkills)
		{
			List<int> coverAbilityId = NpcAblitycmpt.GetCoverAbilityId(learningSkill);
			foreach (int item in coverAbilityId)
			{
				if (getNpcAllSkill.Contains(item))
				{
					coverSkill.Add(item);
				}
			}
		}
		getNpcAllSkill.RemoveAll((int it) => coverSkill.Contains(it));
		int num = getNpcAllSkill.Count + LearningSkills.Count - 5;
		if (num > 0)
		{
			System.Random random = new System.Random();
			for (int i = 0; i < num; i++)
			{
				getNpcAllSkill.RemoveAt(random.Next(getNpcAllSkill.Count));
			}
		}
		getNpcAllSkill.AddRange(LearningSkills);
		npc.GetNpcAllSkill = getNpcAllSkill;
		SetNpcIsTraining(flag: false);
		SetStopBtn(flag: false);
		LockUI(flag: false);
		UpdateUI();
	}

	public void OnTrainAttributeFinish()
	{
		m_CurTime = -1f;
		m_Time = -1f;
		CSMain.Instance.RemoveCounter(m_Counter);
		if (!PeGameMgr.IsMulti)
		{
			csp_instructorNpc = m_Creator.GetNpc(InstructorNpcId);
			csp_traineeNpc = m_Creator.GetNpc(TraineeNpcId);
			AttribType attribType = ((!csp_instructorNpc.IsRandomNpc) ? AttPlusNPCData.GetProtoMaxAttribute(csp_instructorNpc.m_Npc.entityProto.protoId, csp_instructorNpc.m_Npc.GetCmpt<SkAliveEntity>()) : AttPlusNPCData.GetRandMaxAttribute(csp_instructorNpc.m_Npc.entityProto.protoId, csp_instructorNpc.m_Npc.GetCmpt<SkAliveEntity>()));
			AttPlusNPCData.AttrPlus.RandomInt Rand = default(AttPlusNPCData.AttrPlus.RandomInt);
			AttPlusNPCData.GetRandom(csp_instructorNpc.m_Npc.entityProto.protoId, attribType, out Rand);
			float num = new System.Random().Next(Rand.m_Min, Rand.m_Max + 1);
			Debug.LogError("Train sucess: " + attribType.ToString() + ":" + num);
			csp_traineeNpc.UpgradeAttribute(attribType, num);
			SetNpcIsTraining(flag: false);
			SetStopBtn(flag: false);
			LockUI(flag: false);
			UpdateUI();
		}
	}

	public bool CheckNpc()
	{
		if (InstructorNpcId <= 0 || TraineeNpcId <= 0)
		{
			return false;
		}
		CSPersonnel npc = base.m_MgCreator.GetNpc(InstructorNpcId);
		if (npc == null)
		{
			return false;
		}
		if (npc.Occupation != 7)
		{
			InstructorList.Remove(npc.ID);
			npc.trainerType = ETrainerType.none;
			InstructorNpcId = -1;
			return false;
		}
		CSPersonnel npc2 = base.m_MgCreator.GetNpc(TraineeNpcId);
		if (npc2 == null)
		{
			return false;
		}
		if (npc2.Occupation != 7)
		{
			TraineeList.Remove(npc2.ID);
			npc2.trainerType = ETrainerType.none;
			TraineeNpcId = -1;
			return false;
		}
		return true;
	}

	public bool CheckNpcPosition()
	{
		if (InstructorNpcId <= 0 || TraineeNpcId <= 0)
		{
			return false;
		}
		CSPersonnel npc = base.m_MgCreator.GetNpc(InstructorNpcId);
		if (npc == null)
		{
			return false;
		}
		if ((npc.m_Pos - base.Position).magnitude > 4f)
		{
			return false;
		}
		CSPersonnel npc2 = base.m_MgCreator.GetNpc(TraineeNpcId);
		if (npc2 == null)
		{
			return false;
		}
		if ((npc2.m_Pos - base.Position).magnitude > 4f)
		{
			return false;
		}
		return true;
	}

	public override void Update()
	{
		base.Update();
		if (!CheckNpc() && m_Counter != null)
		{
			StopCounter();
		}
		if (base.IsRunning)
		{
			if (m_Counter != null && CheckNpcPosition())
			{
				m_Counter.enabled = true;
				SetTrainingTime(m_Counter.FinalCounter - m_Counter.CurCounter);
			}
			else if (m_Counter != null)
			{
				m_Counter.enabled = false;
			}
			else
			{
				PreviewTrainingTime();
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

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 11, ref refData) : MultiColonyManager.Instance.AssignData(ID, 11, ref refData, _ColonyObj));
		m_Data = refData as CSTrainData;
		InitNPC();
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		}
	}

	private void StartTrainingFromRecord(float m_CurTime, float m_Time)
	{
		SetInstructorNpcUI(InstructorNpcId);
		SetTraineeNpcUI(TraineeNpcId);
		if (trainingType == ETrainingType.Skill)
		{
			SetLearnSkillListUI(LearningSkills);
		}
		SetNpcIsTraining(flag: true);
		SetStopBtn(flag: true);
		StartCounterFromRecord(m_CurTime, m_Time);
		LockUI(flag: true);
	}

	public override void InitAfterAllDataReady()
	{
		if (m_Time > 0f)
		{
			StartTrainingFromRecord(m_CurTime, m_Time);
		}
	}

	private void InitNPC()
	{
		CSMgCreator cSMgCreator = m_Creator as CSMgCreator;
		if (!(cSMgCreator != null))
		{
			return;
		}
		foreach (CSPersonnel trainer in cSMgCreator.Trainers)
		{
			if (AddWorker(trainer))
			{
				trainer.WorkRoom = this;
			}
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

	public override bool AddWorker(CSPersonnel npc)
	{
		if (base.AddWorker(npc))
		{
			if (npc.trainerType != 0)
			{
				switch (npc.trainerType)
				{
				case ETrainerType.Instructor:
					AddInstructor(npc);
					break;
				case ETrainerType.Trainee:
					AddTrainee(npc);
					break;
				}
			}
			return true;
		}
		return false;
	}

	public override void RemoveWorker(CSPersonnel npc)
	{
		base.RemoveWorker(npc);
		if (InstructorNpcId == npc.ID)
		{
			if (m_Counter != null)
			{
				StopCounter();
			}
			TraineeNpcId = -1;
		}
		if (TraineeNpcId == npc.ID)
		{
			if (m_Counter != null)
			{
				StopCounter();
			}
			TraineeNpcId = -1;
		}
		InstructorList.Remove(npc.ID);
		TraineeList.Remove(npc.ID);
		UpdateUI();
	}

	public override void UpdateDataToUI()
	{
		if (base.IsMine)
		{
			BindEvent();
			LockUI(flag: false);
			if (InstructorNpcId > 0)
			{
				SetInstructorNpcUI(InstructorNpcId);
			}
			if (TraineeNpcId > 0)
			{
				SetTraineeNpcUI(TraineeNpcId);
			}
			if (m_Counter == null)
			{
				LockUI(flag: false);
			}
			else
			{
				LockUI(flag: true);
			}
		}
	}

	public override void DestroySomeData()
	{
		StopCounter();
		if (base.IsMine)
		{
			UnbindEvent();
		}
	}

	public override void StopWorking(int npcId)
	{
		if ((npcId == TraineeNpcId || npcId == InstructorNpcId) && m_Counter != null)
		{
			StopCounter();
		}
	}

	public static void ParseData(byte[] data, CSTrainData recordData)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		recordData.instructors = new List<int>();
		for (int i = 0; i < num; i++)
		{
			recordData.instructors.Add(BufferHelper.ReadInt32(reader));
		}
		int num2 = BufferHelper.ReadInt32(reader);
		recordData.trainees = new List<int>();
		for (int j = 0; j < num2; j++)
		{
			recordData.trainees.Add(BufferHelper.ReadInt32(reader));
		}
		recordData.instructorNpcId = BufferHelper.ReadInt32(reader);
		recordData.traineeNpcId = BufferHelper.ReadInt32(reader);
		recordData.trainingType = BufferHelper.ReadInt32(reader);
		int num3 = BufferHelper.ReadInt32(reader);
		recordData.LearningSkillIds = new List<int>();
		for (int k = 0; k < num3; k++)
		{
			recordData.LearningSkillIds.Add(BufferHelper.ReadInt32(reader));
		}
		recordData.m_CurTime = BufferHelper.ReadSingle(reader);
		recordData.m_Time = BufferHelper.ReadSingle(reader);
	}

	public bool CheckInstructorAndTraineeId(int instructorId, int traineeId)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(instructorId);
		CSPersonnel npc2 = base.m_MgCreator.GetNpc(traineeId);
		if (npc == null || npc2 == null)
		{
			return false;
		}
		if (npc.m_Occupation != 7 || npc2.m_Occupation != 7)
		{
			return false;
		}
		if (!InstructorList.Contains(instructorId) || !TraineeList.Contains(traineeId))
		{
			return false;
		}
		return true;
	}

	public bool CheckTrainerId(int trainerId)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(trainerId);
		if (npc == null || npc.m_Occupation != 7)
		{
			return false;
		}
		return true;
	}

	public bool CheckInstructorId(int instructorId)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(instructorId);
		if (npc == null || npc.m_Occupation != 7)
		{
			return false;
		}
		if (!InstructorList.Contains(instructorId))
		{
			return false;
		}
		return true;
	}

	public bool CheckTraineeId(int traineeId)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(traineeId);
		if (npc == null || npc.m_Occupation != 7)
		{
			return false;
		}
		if (!TraineeList.Contains(traineeId))
		{
			return false;
		}
		return true;
	}

	public bool CheckNpcState(CSPersonnel _instructorNpc, CSPersonnel _traineeNpc)
	{
		if (!_instructorNpc.CanTrain)
		{
			CSUtils.ShowCannotWorkReason(_instructorNpc.CannotWorkReason, _instructorNpc.FullName);
			return false;
		}
		if (!_traineeNpc.CanTrain)
		{
			CSUtils.ShowCannotWorkReason(_traineeNpc.CannotWorkReason, _traineeNpc.FullName);
			return false;
		}
		return true;
	}

	public bool CheckNpcState(CSPersonnel trainerNpc)
	{
		if (!trainerNpc.CanTrain)
		{
			CSUtils.ShowCannotWorkReason(trainerNpc.CannotWorkReason, trainerNpc.FullName);
			return false;
		}
		return true;
	}

	public void OnStartSkillTrainingResult(List<int> skillIds, int instructorId, int traineeId)
	{
		trainingType = ETrainingType.Skill;
		InstructorNpcId = instructorId;
		TraineeNpcId = traineeId;
		SetInstructorNpcUI(instructorId);
		SetTraineeNpcUI(traineeId);
		SetLearnSkillListUI(skillIds);
		SetNpcIsTraining(flag: true);
		SetStopBtn(flag: true);
		StartSkillCounter(skillIds);
		LockUI(flag: true);
		if (base.IsMine && base.m_MgCreator.GetNpc(InstructorNpcId) != null && base.m_MgCreator.GetNpc(TraineeNpcId) != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(8000896), base.m_MgCreator.GetNpc(InstructorNpcId).FullName, base.m_MgCreator.GetNpc(TraineeNpcId).FullName));
		}
	}

	public void OnTrainAttributeTrainingResult(int instructorId, int traineeId)
	{
		trainingType = ETrainingType.Attribute;
		InstructorNpcId = instructorId;
		TraineeNpcId = traineeId;
		SetInstructorNpcUI(instructorId);
		SetTraineeNpcUI(traineeId);
		SetNpcIsTraining(flag: true);
		SetStopBtn(flag: true);
		StartAttributeCounter();
		LockUI(flag: true);
		if (base.IsMine && base.m_MgCreator.GetNpc(InstructorNpcId) != null && base.m_MgCreator.GetNpc(TraineeNpcId) != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(8000896), base.m_MgCreator.GetNpc(InstructorNpcId).FullName, base.m_MgCreator.GetNpc(TraineeNpcId).FullName));
		}
	}

	public void StopTrainingrResult()
	{
		StopCounter();
	}

	public void LearnSkillFinishResult(Ablities skillIds, int traineeId)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(traineeId);
		if (npc != null)
		{
			npc.GetNpcAllSkill = skillIds;
		}
		StopCounter();
		UpdateUI();
	}

	public void AttributeFinish(int instructorId, int traineeId, int upgradeTimes)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(traineeId);
		npc.UpgradeTimes = upgradeTimes;
		StopCounter();
		UpdateUI();
	}

	public void SetCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			LockUI(flag: true);
			if (m_Counter == null)
			{
				m_Counter = CSMain.Instance.CreateCounter("Train", curTime, finalTime);
			}
			else
			{
				m_Counter.Init(curTime, finalTime);
			}
			SetTrainingTime(m_Counter.FinalCounter - m_Counter.CurCounter);
		}
	}
}
