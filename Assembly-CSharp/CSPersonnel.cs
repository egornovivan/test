using System;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using Pathea.Operate;
using Pathea.PeEntityExt;
using UnityEngine;

public class CSPersonnel : PersonnelBase
{
	public enum EFarmWorkType
	{
		None,
		Watering,
		Cleaning,
		Harvesting,
		Planting
	}

	protected class FarmWorkInfo
	{
		public FarmPlantLogic m_Plant;

		public Vector3 m_Pos;

		public ClodChunk m_ClodChunk;

		public FarmWorkInfo(FarmPlantLogic plant)
		{
			if (plant == null)
			{
				Debug.LogError("Giving plant must be not null.");
				Debug.DebugBreak();
			}
			m_Plant = plant;
			m_Pos = plant.mPos;
		}

		public FarmWorkInfo(ClodChunk clodChunk, Vector3 pos)
		{
			m_ClodChunk = clodChunk;
			m_Pos = pos;
		}
	}

	public delegate void StateChangedDel(CSPersonnel person, int prvState);

	public delegate void LineStateChangedDel(CSPersonnel person, int oldLine, int index);

	public delegate void LineStateInitDel(CSPersonnel person);

	private CSCreator mCreator;

	public PeEntity m_Npc;

	private SkAliveEntity m_SkAlive;

	private EntityInfoCmpt m_NpcInfo;

	private PeTrans m_Trans;

	private CommonCmpt m_NpcCommonInfo;

	private NpcCmpt m_NpcCmpt;

	public Request currentRequest;

	public Request lastRequest;

	private CSDwellings m_Dwellings;

	private bool m_Running;

	protected CSPersonnelData m_Data;

	private CSCommon m_WorkRoom;

	private float m_Satmina = 500f;

	private float m_SatminaDecimal;

	private PEBed bed;

	private CSAssembly m_Assembly;

	protected int m_RetainState;

	protected Dictionary<EFarmWorkType, FarmWorkInfo> m_FarmWorkMap = new Dictionary<EFarmWorkType, FarmWorkInfo>();

	public RandomItemObj resultItems;

	public List<CSEntity> guardEntities;

	public CSCreator m_Creator
	{
		get
		{
			return mCreator;
		}
		set
		{
			mCreator = value;
			if (m_NpcCmpt != null)
			{
				m_NpcCmpt.Creater = value;
			}
		}
	}

	public CSMgCreator mgCreator => m_Creator as CSMgCreator;

	public PeEntity NPC
	{
		get
		{
			return m_Npc;
		}
		set
		{
			m_Npc = value;
			if (m_Npc == null)
			{
				m_SkAlive = null;
				m_NpcInfo = null;
				m_NpcCommonInfo = null;
				m_NpcCmpt = null;
				m_Trans = null;
			}
			else
			{
				m_SkAlive = m_Npc.GetCmpt<SkAliveEntity>();
				m_NpcInfo = m_Npc.GetCmpt<EntityInfoCmpt>();
				m_NpcCommonInfo = m_Npc.GetCmpt<CommonCmpt>();
				m_NpcCmpt = m_Npc.GetCmpt<NpcCmpt>();
				m_Trans = m_Npc.peTrans;
			}
		}
	}

	public SkAliveEntity SkAlive => m_SkAlive;

	public List<NpcAbility> Npcabliys => m_NpcCmpt.Npcskillcmpt.CurNpcAblitys;

	public float GetEnhanceSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Reinforce);

	public float GetRecycleSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Disassembly);

	public float GetRepairSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Repair);

	public float GetCompoundSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Arts);

	public float GetDiagnoseTimeSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Diagnose);

	public float GetDiagnoseChanceSkill => m_NpcCmpt.Npcskillcmpt.GetCorrectRate(AblityType.Diagnose);

	public float GetTreatTimeSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Medical);

	public float GetTreatChanceSkill => m_NpcCmpt.Npcskillcmpt.GetCorrectRate(AblityType.Medical);

	public float GetTentTimeSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Nurse);

	public float GetProcessingTimeSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Explore);

	public float GetFarmingSkill => m_NpcCmpt.Npcskillcmpt.GetTalentPercent(AblityType.Farming);

	public Ablities GetNpcAllSkill
	{
		get
		{
			return m_NpcCmpt.AbilityIDs;
		}
		set
		{
			m_NpcCmpt.SetAbilityIDs(value);
		}
	}

	public int UpgradeTimes
	{
		get
		{
			return m_NpcCmpt.mAttributeUpTimes;
		}
		set
		{
			m_NpcCmpt.mAttributeUpTimes = value;
		}
	}

	public int State => (int)m_NpcCmpt.State;

	public Texture RandomNpcFace => (!(m_NpcInfo == null)) ? m_NpcInfo.faceTex : null;

	public string MainNpcFace => (!(m_NpcInfo == null)) ? m_NpcInfo.faceIcon : null;

	public string GivenName => (!(m_NpcInfo == null)) ? m_NpcInfo.characterName.givenName : string.Empty;

	public string FullName => (!(m_NpcInfo == null)) ? m_NpcInfo.characterName.fullName : string.Empty;

	public string FamilyName => (!(m_NpcInfo == null)) ? m_NpcInfo.characterName.familyName : string.Empty;

	public PeSex Sex => (!(m_NpcCommonInfo == null)) ? m_NpcCommonInfo.sex : PeSex.Male;

	public bool IsRandomNpc => !(m_Npc == null) && m_Npc.IsRandomNpc();

	public bool IsFollower => !(m_Npc == null) && m_Npc.IsFollower();

	public Transform transform => (!(m_Trans == null)) ? m_Trans.transform : null;

	public bool CanTrain => !(m_NpcCmpt == null) && !m_NpcCmpt.NpcUnableWork;

	public bool CanChangeOccupation => NpcTypeDb.CanRun(m_NpcCmpt.NpcControlCmdId, ENpcControlType.ChangeRole) && !m_NpcCmpt.BaseNpcOutMission;

	public bool CanProcess => !(m_NpcCmpt == null) && !m_NpcCmpt.NpcUnableProcess;

	public bool ShouldStopProcessing => m_NpcCmpt == null || m_NpcCmpt.NpcShouldStopProcessing;

	public ENpcUnableWorkType CannotWorkReason => (!(m_NpcCmpt == null)) ? m_NpcCmpt.unableWorkReason : ENpcUnableWorkType.None;

	public CSDwellings Dwellings
	{
		get
		{
			return m_Dwellings;
		}
		set
		{
			if (m_Dwellings != value)
			{
				if (value != null)
				{
					value.AddEventListener(OnDwellingsChangeState);
					m_Running = value.IsRunning;
				}
				if (m_Dwellings != null)
				{
					m_Dwellings.RemoveEventListener(OnDwellingsChangeState);
				}
			}
			m_Dwellings = value;
		}
	}

	public bool Running => m_Running;

	public CSPersonnelData Data => m_Data;

	public int Occupation => m_Occupation;

	public int m_Occupation
	{
		get
		{
			return Data.m_Occupation;
		}
		set
		{
			_updateOccupation(Data.m_Occupation, value);
		}
	}

	public CSCommon WorkRoom
	{
		get
		{
			return m_WorkRoom;
		}
		set
		{
			SetWorkRoom(value);
		}
	}

	public PEMachine WorkMachine { get; set; }

	public PEDoctor HospitalMachine { get; set; }

	public PETrainner TrainerMachine { get; set; }

	public int m_WorkMode
	{
		get
		{
			return Data.m_WorkMode;
		}
		set
		{
			_updateWorkType(m_Data.m_WorkMode, value);
		}
	}

	public float Stamina
	{
		get
		{
			return (!(m_SkAlive == null)) ? (m_SkAlive.GetAttribute(AttribType.Comfort) + m_SatminaDecimal) : m_Satmina;
		}
		set
		{
			m_Satmina = value;
			m_SkAlive.SetAttribute(AttribType.Comfort, Mathf.FloorToInt(value));
			m_SatminaDecimal = Mathf.Max(0f, CSUtils.SplitDecimals(value));
		}
	}

	public float MaxStamina => (!(m_SkAlive == null)) ? m_SkAlive.GetAttribute(AttribType.ComfortMax) : m_Satmina;

	public float HalfStamina => (!(m_SkAlive == null)) ? m_SkAlive.GetAttribute(AttribType.ComfortMax) : 250f;

	public PEBed Bed
	{
		get
		{
			return bed;
		}
		set
		{
			bed = value;
			if (m_NpcCmpt != null)
			{
				m_NpcCmpt.Sleep = bed;
			}
		}
	}

	public override GameObject m_Go => m_Npc.GetGameObject();

	public override Vector3 m_Pos
	{
		get
		{
			if (m_Npc != null)
			{
				return m_Npc.position;
			}
			return Vector3.zero;
		}
		set
		{
			if (m_Npc != null)
			{
				m_Npc.MoveToPosition(value);
			}
		}
	}

	public override Quaternion m_Rot
	{
		get
		{
			if (m_Npc != null)
			{
				return m_Npc.rotation;
			}
			return Quaternion.identity;
		}
		set
		{
		}
	}

	public override float WalkSpeed => 0f;

	public override float RunSpeed => 0f;

	public override string m_Name => FullName;

	public override bool EqupsRangeWeapon => false;

	public override bool EqupsMeleeWeapon => !EqupsRangeWeapon;

	private int processingIndex
	{
		get
		{
			return Data.m_ProcessingIndex;
		}
		set
		{
			Data.m_ProcessingIndex = value;
		}
	}

	public int ProcessingIndex
	{
		get
		{
			return processingIndex;
		}
		set
		{
			SetProcessingIndex(value);
		}
	}

	private bool isProcessing
	{
		get
		{
			return Data.m_IsProcessing;
		}
		set
		{
			Data.m_IsProcessing = value;
		}
	}

	public bool IsProcessing
	{
		get
		{
			return isProcessing;
		}
		set
		{
			SetProcessing(value);
		}
	}

	public List<CSEntity> GuardEntities
	{
		get
		{
			return guardEntities;
		}
		set
		{
			guardEntities = value;
			UpdateNpcCmptGuardEntities();
		}
	}

	public bool IsTraining
	{
		get
		{
			return Data.m_IsTraining;
		}
		set
		{
			Data.m_IsTraining = value;
			UpdateNpcCmptTraining();
		}
	}

	public ETrainingType trainingType
	{
		get
		{
			return (ETrainingType)Data.m_TrainingType;
		}
		set
		{
			Data.m_TrainingType = (int)value;
			UpdateNpcCmptTraining();
		}
	}

	public ETrainerType trainerType
	{
		get
		{
			return (ETrainerType)Data.m_TrainerType;
		}
		set
		{
			Data.m_TrainerType = (int)value;
			UpdateNpcCmptTraining();
		}
	}

	private static event StateChangedDel m_StateChangedListener;

	private static event StateChangedDel m_OccupaChangedListener;

	private static event StateChangedDel m_WorkTypeChangedListener;

	private static event LineStateChangedDel m_ProcessingIndexChangeListenner;

	private static event LineStateInitDel m_ProcessingIndexInitListenner;

	public void UpgradeAttribute(AttribType type, float plusValue)
	{
		m_NpcCmpt.AttributeUpgrade(type, plusValue);
	}

	public bool CanUpgradeAttribute()
	{
		return m_NpcCmpt.CanAttributeUp();
	}

	public bool TrySetOccupation(int occup)
	{
		if (Occupation == occup)
		{
			return true;
		}
		if (occup != 0 && !CanChangeOccupation)
		{
			return false;
		}
		if (PeGameMgr.IsMulti)
		{
			((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetOccupation, occup);
		}
		else
		{
			if (m_Occupation == 5 && occup != 5)
			{
				TrySetProcessingIndex(-1);
			}
			m_Occupation = occup;
		}
		return true;
	}

	public void TrySetWorkRoom(CSCommon w)
	{
		if (WorkRoom != w)
		{
			if (PeGameMgr.IsMulti)
			{
				((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetWorkRoomID, w.ID);
			}
			else
			{
				WorkRoom = w;
			}
		}
	}

	public void TrySetWorkMode(int wm)
	{
		if (m_WorkMode != wm)
		{
			if (PeGameMgr.IsMulti)
			{
				((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetWorkMode, wm);
			}
			else
			{
				m_WorkMode = wm;
			}
		}
	}

	public float GetAttribute(AttribType type)
	{
		return m_SkAlive.GetAttribute(type);
	}

	public void SetAttribute(AttribType type, float value)
	{
		m_SkAlive.SetAttribute(type, value);
	}

	public void CreateData(CSPersonnelData data)
	{
		bool flag = m_Creator.m_DataInst.AddData(data);
		m_Data = data;
		if (!flag || PeGameMgr.IsMulti)
		{
			Dwellings = m_Creator.GetCommonEntity(m_Data.m_DwellingsID) as CSDwellings;
			if (Dwellings != null)
			{
				Dwellings.AddNpcs(this);
			}
			CSCommon commonEntity = m_Creator.GetCommonEntity(m_Data.m_WorkRoomID);
			if (commonEntity != null)
			{
				WorkRoom = commonEntity;
			}
			if (processingIndex >= 0 && CSPersonnel.m_ProcessingIndexInitListenner != null)
			{
				CSPersonnel.m_ProcessingIndexInitListenner(this);
			}
		}
	}

	private void _updateState(int old_state, int new_state)
	{
	}

	private void _updateOccupation(int old_occupa, int new_occupa)
	{
		if (old_occupa == new_occupa)
		{
			return;
		}
		ClearFarmWorks();
		switch (new_occupa)
		{
		case 0:
			if (m_Npc != null && m_Npc.IsFollower())
			{
				m_Npc.SetFollower(bFlag: false);
			}
			WorkRoom = null;
			m_WorkMode = 0;
			break;
		case 1:
			if (m_Npc != null && m_Npc.IsFollower())
			{
				m_Npc.SetFollower(bFlag: false);
			}
			WorkRoom = null;
			m_WorkMode = 1;
			break;
		case 3:
			if (m_Npc != null && m_Npc.IsFollower())
			{
				m_Npc.SetFollower(bFlag: false);
			}
			if (mgCreator != null && mgCreator.Assembly != null)
			{
				WorkRoom = mgCreator.Assembly.Farm;
			}
			else
			{
				WorkRoom = null;
			}
			m_WorkMode = 4;
			break;
		case 2:
			if (m_Npc != null && m_Npc.IsFollower())
			{
				m_Npc.SetFollower(bFlag: false);
			}
			WorkRoom = null;
			m_WorkMode = 7;
			break;
		case 4:
			WorkRoom = null;
			m_WorkMode = 0;
			break;
		case 5:
			isProcessing = false;
			processingIndex = -1;
			if (mgCreator != null && mgCreator.Assembly != null)
			{
				WorkRoom = CSMain.s_MgCreator.Assembly.ProcessingFacility;
			}
			else
			{
				WorkRoom = null;
			}
			m_WorkMode = 0;
			break;
		case 6:
			if (m_Npc != null && m_Npc.IsFollower())
			{
				m_Npc.SetFollower(bFlag: false);
			}
			WorkRoom = null;
			m_WorkMode = 0;
			break;
		case 7:
			if (m_Npc != null && m_Npc.IsFollower())
			{
				m_Npc.SetFollower(bFlag: false);
			}
			if (mgCreator != null && mgCreator.Assembly != null)
			{
				WorkRoom = CSMain.s_MgCreator.Assembly.TrainingCenter;
			}
			else
			{
				WorkRoom = null;
			}
			m_WorkMode = 0;
			break;
		}
		m_Data.m_Occupation = new_occupa;
		if (CSPersonnel.m_OccupaChangedListener != null)
		{
			CSPersonnel.m_OccupaChangedListener(this, old_occupa);
		}
		UpdateNpcCmptOccupation();
	}

	private void _updateWorkType(int old_type, int new_type)
	{
		if (old_type == new_type)
		{
			return;
		}
		switch (old_type)
		{
		}
		if (new_type == 8)
		{
		}
		if (new_type == 7)
		{
		}
		ClearFarmWorks();
		m_Data.m_WorkMode = new_type;
		if (CSPersonnel.m_WorkTypeChangedListener != null)
		{
			CSPersonnel.m_WorkTypeChangedListener(this, old_type);
		}
		GuardEntities = GetProtectedEntities();
		UpdateNpcCmptWorkMode();
	}

	public static void RegisterStateChangedListener(StateChangedDel listener)
	{
		CSPersonnel.m_StateChangedListener = (StateChangedDel)Delegate.Combine(CSPersonnel.m_StateChangedListener, listener);
	}

	public static void UnRegisterStateChangedListener(StateChangedDel listener)
	{
		CSPersonnel.m_StateChangedListener = (StateChangedDel)Delegate.Remove(CSPersonnel.m_StateChangedListener, listener);
	}

	public static void RegisterOccupaChangedListener(StateChangedDel listener)
	{
		CSPersonnel.m_OccupaChangedListener = (StateChangedDel)Delegate.Combine(CSPersonnel.m_OccupaChangedListener, listener);
	}

	public static void UnregisterOccupaChangedListener(StateChangedDel listener)
	{
		CSPersonnel.m_OccupaChangedListener = (StateChangedDel)Delegate.Remove(CSPersonnel.m_OccupaChangedListener, listener);
	}

	public static void RegisterWorkTypeChangedListener(StateChangedDel listener)
	{
		CSPersonnel.m_WorkTypeChangedListener = (StateChangedDel)Delegate.Combine(CSPersonnel.m_WorkTypeChangedListener, listener);
	}

	public static void UnregisterWorkTypeChangedListener(StateChangedDel listener)
	{
		CSPersonnel.m_WorkTypeChangedListener = (StateChangedDel)Delegate.Remove(CSPersonnel.m_WorkTypeChangedListener, listener);
	}

	public override PersonnelSpace[] GetWorkSpaces()
	{
		if (WorkRoom == null)
		{
			return null;
		}
		return WorkRoom.WorkSpaces;
	}

	private void OnDwellingsChangeState(int event_type, CSEntity entiy, object arg)
	{
		if (event_type == 4001)
		{
			bool flag = (bool)arg;
			if (flag)
			{
				CSAssembly assembly = Dwellings.Assembly;
				assembly.AddEventListener(OnAssemblyEventHandler);
				m_Assembly = assembly;
			}
			else if (m_Assembly != null)
			{
				m_Assembly.RemoveEventListener(OnAssemblyEventHandler);
				m_Assembly = null;
			}
			m_Running = flag;
		}
	}

	private void OnAssemblyEventHandler(int event_type, CSEntity entity, object arg)
	{
		if (event_type == 2001)
		{
			CSCommon cSCommon = arg as CSCommon;
			if (cSCommon == null)
			{
				Debug.LogError("The argument is error");
			}
		}
	}

	public void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = m_Creator.m_DataInst.AssignData(ID, 50, ref refData);
		m_Data = refData as CSPersonnelData;
		if (!flag)
		{
			Dwellings = m_Creator.GetCommonEntity(m_Data.m_DwellingsID) as CSDwellings;
			Dwellings.AddNpcs(this);
			CSCommon commonEntity = m_Creator.GetCommonEntity(m_Data.m_WorkRoomID);
			WorkRoom = commonEntity;
			if (processingIndex >= 0 && CSPersonnel.m_ProcessingIndexChangeListenner != null)
			{
				CSPersonnel.m_ProcessingIndexChangeListenner(this, -1, processingIndex);
			}
		}
	}

	public void RemoveData()
	{
		m_Creator.m_DataInst.RemovePersonnelData(m_Data.ID);
	}

	public override void Update()
	{
		base.Update();
		if (!(m_Npc == null))
		{
			if (Dwellings != null)
			{
				m_Data.m_DwellingsID = Dwellings.ID;
			}
			else
			{
				m_Data.m_DwellingsID = -1;
			}
		}
	}

	public void ProtectedEntityDamaged(CSEntity entiy, GameObject caster, float damage)
	{
	}

	public List<CSEntity> GetProtectedEntities()
	{
		if (m_Creator == null)
		{
			return new List<CSEntity>();
		}
		CSMgCreator cSMgCreator = m_Creator as CSMgCreator;
		if (cSMgCreator == null)
		{
			Debug.LogWarning("The Creator is not a Managed creator, it cant produce the protected entities ");
			return new List<CSEntity>();
		}
		List<CSEntity> list = new List<CSEntity>();
		if (cSMgCreator.Assembly != null)
		{
			list.Add(cSMgCreator.Assembly);
			foreach (KeyValuePair<CSConst.ObjectType, List<CSCommon>> item in cSMgCreator.Assembly.m_BelongObjectsMap)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					list.Add(item.Value[i]);
				}
			}
		}
		return list;
	}

	public void UpdateNpcCmpt()
	{
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.Creater = mCreator;
			switch (Data.m_Occupation)
			{
			case 0:
				m_NpcCmpt.Job = ENpcJob.Resident;
				break;
			case 1:
				m_NpcCmpt.Job = ENpcJob.Worker;
				break;
			case 3:
				m_NpcCmpt.Job = ENpcJob.Farmer;
				break;
			case 2:
				m_NpcCmpt.Job = ENpcJob.Soldier;
				break;
			case 4:
				m_NpcCmpt.Job = ENpcJob.Follower;
				break;
			case 5:
				m_NpcCmpt.Job = ENpcJob.Processor;
				break;
			case 6:
				m_NpcCmpt.Job = ENpcJob.Doctor;
				break;
			case 7:
				m_NpcCmpt.Job = ENpcJob.Trainer;
				break;
			default:
				m_NpcCmpt.Job = ENpcJob.None;
				break;
			}
			switch (Data.m_WorkMode)
			{
			case 4:
				m_NpcCmpt.AddTitle(ENpcTitle.Manage);
				break;
			case 6:
				m_NpcCmpt.AddTitle(ENpcTitle.Plant);
				break;
			case 5:
				m_NpcCmpt.AddTitle(ENpcTitle.Harvest);
				break;
			default:
				m_NpcCmpt.AddTitle(ENpcTitle.None);
				break;
			}
			m_NpcCmpt.Sleep = Bed;
			m_NpcCmpt.BaseEntities = GuardEntities;
			m_NpcCmpt.WorkEntity = WorkRoom;
			m_NpcCmpt.Work = WorkMachine;
			m_NpcCmpt.Cure = HospitalMachine;
			m_NpcCmpt.Trainner = TrainerMachine;
		}
	}

	public void UpdateNpcCmptWorkMode()
	{
		switch (Data.m_WorkMode)
		{
		case 4:
			m_NpcCmpt.AddTitle(ENpcTitle.Manage);
			break;
		case 6:
			m_NpcCmpt.AddTitle(ENpcTitle.Plant);
			break;
		case 5:
			m_NpcCmpt.AddTitle(ENpcTitle.Harvest);
			break;
		case 7:
			m_NpcCmpt.Soldier = ENpcSoldier.Patrol;
			break;
		case 8:
			m_NpcCmpt.Soldier = ENpcSoldier.Guard;
			break;
		default:
			m_NpcCmpt.AddTitle(ENpcTitle.None);
			break;
		}
		m_NpcCmpt.BaseEntities = GuardEntities;
		m_NpcCmpt.WorkEntity = WorkRoom;
		m_NpcCmpt.Work = WorkMachine;
		m_NpcCmpt.Cure = HospitalMachine;
		m_NpcCmpt.Trainner = TrainerMachine;
	}

	public void UpdateNpcCmptOccupation()
	{
		switch (Data.m_Occupation)
		{
		case 0:
			m_NpcCmpt.Job = ENpcJob.Resident;
			break;
		case 1:
			m_NpcCmpt.Job = ENpcJob.Worker;
			break;
		case 3:
			m_NpcCmpt.Job = ENpcJob.Farmer;
			break;
		case 2:
			m_NpcCmpt.Job = ENpcJob.Soldier;
			break;
		case 4:
			m_NpcCmpt.Job = ENpcJob.Follower;
			break;
		case 5:
			m_NpcCmpt.Job = ENpcJob.Processor;
			break;
		case 6:
			m_NpcCmpt.Job = ENpcJob.Doctor;
			break;
		case 7:
			m_NpcCmpt.Job = ENpcJob.Trainer;
			break;
		default:
			m_NpcCmpt.Job = ENpcJob.None;
			break;
		}
		m_NpcCmpt.BaseEntities = GuardEntities;
		m_NpcCmpt.WorkEntity = WorkRoom;
		m_NpcCmpt.Work = WorkMachine;
		m_NpcCmpt.Cure = HospitalMachine;
		m_NpcCmpt.Trainner = TrainerMachine;
	}

	public void UpdateNpcCmptGuardEntities()
	{
		m_NpcCmpt.BaseEntities = GuardEntities;
	}

	public void UpdateNpcCmptTraining()
	{
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.IsTrainning = IsTraining;
			m_NpcCmpt.TrainerType = trainerType;
			m_NpcCmpt.TrainningType = trainingType;
		}
	}

	public void ResetCmd()
	{
	}

	private void SetWorkRoom(CSCommon workRoom)
	{
		if (m_WorkRoom == workRoom)
		{
			return;
		}
		if (m_WorkRoom != null)
		{
			m_WorkRoom.RemoveWorker(this);
			if (workRoom == null)
			{
				WorkMachine = null;
			}
			else
			{
				PersonnelSpace personnelSpace = workRoom.FindEmptySpace(this);
				if (personnelSpace != null)
				{
					personnelSpace.m_Person = this;
					WorkMachine = personnelSpace.WorkMachine;
					HospitalMachine = personnelSpace.HospitalMachine;
					TrainerMachine = personnelSpace.TrainerMachine;
				}
				workRoom.AddWorker(this);
			}
		}
		else
		{
			PersonnelSpace personnelSpace2 = workRoom.FindEmptySpace(this);
			if (personnelSpace2 != null)
			{
				personnelSpace2.m_Person = this;
				WorkMachine = personnelSpace2.WorkMachine;
				HospitalMachine = personnelSpace2.HospitalMachine;
				TrainerMachine = personnelSpace2.TrainerMachine;
			}
			workRoom.AddWorker(this);
		}
		m_WorkRoom = workRoom;
		if (workRoom != null)
		{
			Data.m_WorkRoomID = workRoom.ID;
		}
		else
		{
			Data.m_WorkRoomID = -1;
		}
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.WorkEntity = m_WorkRoom;
			m_NpcCmpt.Work = WorkMachine;
			m_NpcCmpt.Cure = HospitalMachine;
			m_NpcCmpt.Trainner = TrainerMachine;
		}
	}

	public override void UpdateWorkSpace(PersonnelSpace ps)
	{
		WorkMachine = ps.WorkMachine;
		HospitalMachine = ps.HospitalMachine;
		TrainerMachine = ps.TrainerMachine;
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.Work = WorkMachine;
			m_NpcCmpt.Cure = HospitalMachine;
			m_NpcCmpt.Trainner = TrainerMachine;
		}
	}

	public override void UpdateWorkMachine(PEMachine pm)
	{
		WorkMachine = pm;
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.Work = WorkMachine;
		}
	}

	public override void UpdateHospitalMachine(PEDoctor pd)
	{
		HospitalMachine = pd;
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.Cure = HospitalMachine;
		}
	}

	public override void UpdateTrainerMachine(PETrainner pt)
	{
		TrainerMachine = pt;
		if (m_NpcCmpt != null)
		{
			m_NpcCmpt.Trainner = TrainerMachine;
		}
	}

	public void ClearWorkRoom()
	{
		if (WorkRoom != null)
		{
			WorkRoom.RemoveWorker(this);
			WorkRoom = null;
		}
	}

	public bool FollowMe(bool follow)
	{
		if (follow)
		{
			if (m_Npc != null)
			{
				m_Npc.SetFollower(bFlag: true);
				return true;
			}
		}
		else if (m_Npc != null)
		{
			m_Npc.SetFollower(bFlag: false);
			return true;
		}
		return false;
	}

	public bool FollowMe(int hero_id)
	{
		if (m_Npc == null)
		{
			Debug.LogWarning("This npc cannot be a follower");
			return false;
		}
		m_Npc.SetFollower(bFlag: true, hero_id);
		m_RetainState = 5;
		return true;
	}

	public void KickOut()
	{
		if (m_Creator == null)
		{
			Debug.LogWarning("The Creator is not exsit.");
		}
		else if (!PeGameMgr.IsMulti)
		{
			m_Creator.RemoveNpc(m_Npc);
			m_Npc.Dismiss();
		}
		else
		{
			((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_RemoveNpc);
		}
	}

	public override void MoveToImmediately(Vector3 destPos)
	{
	}

	public override bool CanBehave()
	{
		return false;
	}

	public override void Sleep(bool v)
	{
	}

	public override void Stay()
	{
	}

	public override void PlayAnimation(EAnimateType type, bool v)
	{
	}

	protected override void OnRestToDest(PersonnelBase npc)
	{
		base.OnRestToDest(npc);
	}

	protected override void OnRestMeetBlock(PersonnelBase npc)
	{
		base.OnRestMeetBlock(npc);
	}

	protected override void OnIdleToDest(PersonnelBase npc)
	{
		base.OnIdleToDest(npc);
	}

	protected override void OnWorkToDest(PersonnelBase npc)
	{
		base.OnWorkToDest(npc);
		if (WorkRoom.m_Type != 7)
		{
		}
	}

	protected override void OnWorkMeetBlock(PersonnelBase npc)
	{
		base.OnWorkMeetBlock(npc);
		Debug.Log("I cant work Now, I meet block. Shit!!! Shit!!!");
	}

	public void ClearFarmWorks()
	{
		if (WorkRoom == null)
		{
			return;
		}
		if (!(WorkRoom is CSFarm cSFarm))
		{
			m_FarmWorkMap.Clear();
			return;
		}
		foreach (KeyValuePair<EFarmWorkType, FarmWorkInfo> item in m_FarmWorkMap)
		{
			switch (item.Key)
			{
			case EFarmWorkType.Watering:
				cSFarm.RestoreWateringPlant(item.Value.m_Plant);
				break;
			case EFarmWorkType.Cleaning:
				cSFarm.RestoreCleaningPlant(item.Value.m_Plant);
				break;
			case EFarmWorkType.Harvesting:
				cSFarm.RestoreRipePlant(item.Value.m_Plant);
				break;
			case EFarmWorkType.Planting:
			{
				CSMgCreator cSMgCreator = m_Creator as CSMgCreator;
				if (cSMgCreator == null)
				{
					Debug.Log(" CSCreator is error");
				}
				else
				{
					cSMgCreator.m_Clod.DirtyTheChunk(item.Value.m_ClodChunk.m_ChunkIndex, dirty: false);
				}
				break;
			}
			}
		}
		m_FarmWorkMap.Clear();
	}

	private void FarmerTick()
	{
	}

	private void _farmWorkStyle()
	{
		if (!(WorkRoom is CSFarm cSFarm))
		{
			return;
		}
		FarmPlantLogic farmPlantLogic = null;
		CSMgCreator cSMgCreator = m_Creator as CSMgCreator;
		if (m_WorkMode == 4)
		{
			if (m_FarmWorkMap.Count != 0)
			{
				return;
			}
			ItemObject plantTool = cSFarm.GetPlantTool(0);
			ItemObject plantTool2 = cSFarm.GetPlantTool(1);
			farmPlantLogic = ((plantTool != null) ? cSFarm.AssignOutWateringPlant() : null);
			if (farmPlantLogic != null)
			{
				FarmWorkInfo value = new FarmWorkInfo(farmPlantLogic);
				m_FarmWorkMap.Add(EFarmWorkType.Watering, value);
				return;
			}
			farmPlantLogic = ((plantTool2 != null) ? cSFarm.AssignOutCleaningPlant() : null);
			if (farmPlantLogic != null)
			{
				FarmWorkInfo value2 = new FarmWorkInfo(farmPlantLogic);
				m_FarmWorkMap.Add(EFarmWorkType.Cleaning, value2);
			}
		}
		else if (m_WorkMode == 5)
		{
			if (m_FarmWorkMap.Count != 0)
			{
				return;
			}
			CSStorage cSStorage = null;
			foreach (CSStorage item in cSFarm.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Storage])
			{
				SlotList slotList = item.m_Package.GetSlotList();
				if (slotList.GetVacancyCount() >= 2)
				{
					cSStorage = item;
					break;
				}
			}
			if (cSStorage != null)
			{
				farmPlantLogic = cSFarm.AssignOutRipePlant();
				if (farmPlantLogic != null)
				{
					FarmWorkInfo value3 = new FarmWorkInfo(farmPlantLogic);
					m_FarmWorkMap.Add(EFarmWorkType.Harvesting, value3);
				}
			}
		}
		else
		{
			if (m_WorkMode != 6)
			{
				return;
			}
			if (m_FarmWorkMap.Count == 0)
			{
				ClodChunk clodChunk = cSMgCreator.m_Clod.FindCleanChunk(cSFarm.Assembly.Position, cSFarm.Assembly.Radius);
				if (cSFarm.HasPlantSeed() && clodChunk != null && clodChunk.FindCleanClod(out var pos))
				{
					cSMgCreator.m_Clod.DirtyTheChunk(clodChunk.m_ChunkIndex, dirty: true);
					FarmWorkInfo farmWorkInfo = new FarmWorkInfo(clodChunk, pos);
					m_FarmWorkMap.Add(EFarmWorkType.Planting, farmWorkInfo);
					_sendToWorkOnFarm(farmWorkInfo.m_Pos);
				}
			}
			else if (m_FarmWorkMap.ContainsKey(EFarmWorkType.Planting) && m_FarmWorkMap[EFarmWorkType.Planting].m_Pos == Vector3.zero)
			{
				if (cSFarm.HasPlantSeed())
				{
					FarmWorkInfo farmWorkInfo2 = m_FarmWorkMap[EFarmWorkType.Planting];
					if (!farmWorkInfo2.m_ClodChunk.FindCleanClod(out farmWorkInfo2.m_Pos))
					{
						m_FarmWorkMap.Remove(EFarmWorkType.Planting);
					}
				}
				else
				{
					FarmWorkInfo farmWorkInfo3 = m_FarmWorkMap[EFarmWorkType.Planting];
					cSMgCreator.m_Clod.DirtyTheChunk(farmWorkInfo3.m_ClodChunk.m_ChunkIndex, dirty: false);
					m_FarmWorkMap.Remove(EFarmWorkType.Planting);
				}
			}
			if (m_FarmWorkMap.ContainsKey(EFarmWorkType.Planting))
			{
			}
		}
	}

	private void _sendToWorkOnFarm(Vector3 pos)
	{
	}

	public static void RegisterProcessingIndexChangedListener(LineStateChangedDel listener)
	{
		CSPersonnel.m_ProcessingIndexChangeListenner = (LineStateChangedDel)Delegate.Combine(CSPersonnel.m_ProcessingIndexChangeListenner, listener);
	}

	public static void UnRegisterProcessingIndexChangedListener(LineStateChangedDel listener)
	{
		CSPersonnel.m_ProcessingIndexChangeListenner = (LineStateChangedDel)Delegate.Remove(CSPersonnel.m_ProcessingIndexChangeListenner, listener);
	}

	public static void RegisterProcessingIndexInitListener(LineStateInitDel listener)
	{
		CSPersonnel.m_ProcessingIndexInitListenner = (LineStateInitDel)Delegate.Combine(CSPersonnel.m_ProcessingIndexInitListenner, listener);
	}

	public static void UnRegisterProcessingIndexInitListener(LineStateInitDel listener)
	{
		CSPersonnel.m_ProcessingIndexInitListenner = (LineStateInitDel)Delegate.Remove(CSPersonnel.m_ProcessingIndexInitListenner, listener);
	}

	public void TrySetProcessingIndex(int index)
	{
		if (processingIndex == index)
		{
			return;
		}
		if (index >= 0 && !CanProcess)
		{
			CSUtils.ShowCannotWorkReason(CannotWorkReason, FullName);
		}
		else if (PeGameMgr.IsMulti)
		{
			if ((AiAdNpcNetwork)NetworkInterface.Get(ID) != null)
			{
				((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetProcessingIndex, index);
			}
		}
		else
		{
			ProcessingIndex = index;
		}
	}

	private void SetProcessing(bool curValue)
	{
		isProcessing = curValue;
		UpdateNpcCmptProcessing();
	}

	private void SetProcessingIndex(int index)
	{
		if (processingIndex != index)
		{
			int oldLine = processingIndex;
			processingIndex = index;
			if (CSPersonnel.m_ProcessingIndexChangeListenner != null)
			{
				CSPersonnel.m_ProcessingIndexChangeListenner(this, oldLine, processingIndex);
			}
		}
	}

	public void UpdateNpcCmptProcessing()
	{
		m_NpcCmpt.Processing = isProcessing;
	}

	public void StopWork()
	{
		TrySetProcessingIndex(-1);
	}

	public void ShowTips(string content)
	{
		CSUI_MainWndCtrl.ShowStatusBar(content);
	}
}
