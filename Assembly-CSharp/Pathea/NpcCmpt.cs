using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea.Operate;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using PeMap;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class NpcCmpt : PeCmpt, IPeMsg
{
	private const int Version_0000 = 0;

	private const int Version_0001 = 1;

	private const int Version_0002 = 2;

	private const int Version_0003 = 3;

	private const int Version_0004 = 4;

	private const int Version_0005 = 5;

	private const int Version_0006 = 6;

	private const int Version_0007 = 7;

	private const int Version_0008 = 8;

	private const int Version_0009 = 9;

	private const int Version_0010 = 10;

	private const int Version_0011 = 11;

	private const int Version_0012 = 12;

	private const int Version_Current = 12;

	private const float m_ChenZhenPlayMusicRadio = 3f;

	private const int m_ChenZhenID = 9007;

	private const int m_ChenZhenMusicID = 4513;

	private const float m_PlayIntervalTime = 18000f;

	private const int m_ChenZhenMusicMissionID = 10047;

	private NetworkInterface _net;

	private Transform m_Mount;

	private RequestCmpt m_Request;

	private BehaveCmpt m_Behave;

	private PeEntity m_RobotEntity;

	private NpcPackageCmpt mNpcPackage;

	private PeEntity mChatTarget;

	private ENpcType m_Type = ENpcType.Field;

	private ENpcJob m_Job;

	private ETrainingType e_TrainningType;

	private ETrainerType e_TrainerType;

	private ENpcTalkType e_NpcTalkType;

	private ENpcBattProfession m_Profession = ENpcBattProfession.AD;

	private BattleMgr m_BattleMgr;

	private ENpcBattle m_Battle = ENpcBattle.Defence;

	private ENpcTitle m_Title;

	private ENpcSoldier m_Soldier;

	private ENpcState m_State;

	private ENpcAiType m_AiType;

	private ENpcMotionStyle m_MotionStyle = ENpcMotionStyle.Normal;

	private ELineType m_LineType;

	private object[] mTeamData;

	private bool m_caCanIdle = true;

	private AttackType mNpcAtkType;

	private List<PeEntity> beEnemiesLocked;

	private CSCreator m_Creater;

	private IOperation m_Work;

	private IOperation m_Cure;

	private IOperation m_Sleep;

	private IOperation m_trainner;

	private CSEntity m_WorkEntity;

	private float m_GuardRadius;

	private Vector3 m_GuardPosition;

	private List<CSEntity> m_baseEntities;

	private Ablities m_AbilityIdes = new Ablities(5);

	private NpcAblitycmpt m_NpcSkillcmpt;

	private List<NpcCmpt> m_Allys;

	private NpcCmpt m_NpcSkillTarget;

	public int mAttributeUpTimes;

	private ENpcMedicalState m_MedicalState;

	private List<PEAbnormalType> m_illAbnormals;

	private bool m_IsNeedMedicine;

	private PassengerCmpt m_Passenger;

	private EquipSelect m_EqSelect;

	private bool m_FollowerWork;

	public Action FollowerWorkStateChangeEvent;

	private bool m_FollowerSentry;

	private bool m_FollowerCut;

	private bool m_servantCallback;

	private float m_FollowerReviceTime = 1200f;

	private float m_FollowerCurReviveTime;

	private ServantLeaderCmpt m_Master;

	private Vector3 mRecruitPos;

	private int m_GatherprotoTypeIdx = -99;

	private bool _IsCsBacking;

	private bool _NeedSeekHelp;

	private bool _MisstionAskStop;

	private bool m_isStoreNpc;

	private bool m_InAllys;

	private bool m_IsHunger;

	private float LOW_PERCENT = 0.15f;

	private bool m_IsUncomfortable;

	private bool m_IsLowHp;

	private bool m_IsInSleepTime;

	private bool m_IsInDinnerTime;

	private bool m_Processing;

	private bool m_IsTrainning;

	private bool m_CanWander;

	private bool m_CanTalk = true;

	private bool m_CanHanded;

	private bool m_NpcInAlert;

	private bool m_BaseNpcOutMission;

	private bool m_HasNearleague;

	private bool m_bRunAway;

	private NpaTalkAgent mTalkAgent;

	private NpcThinkAgent mThinkAgent;

	private NpcCheckTime mNpcCheckTime;

	private Camp m_Camp;

	private bool m_UpdateCampsite;

	private RandomItemObj m_RandomItemobj;

	private int m_ReviveTime = 10;

	private int m_MountID;

	private Vector3 mFixedPointPos;

	private float mStandRotate;

	private int mNpcControlCmdId = 1;

	private List<Collider> m_IgnorePlantColliders;

	private PeTrans m_mianplayerTran;

	private PEBuilding m_OccopyBuild;

	private bool NpcHasSleep;

	private int NpcsleepBuffId;

	private float sleepWaitTime = 3f;

	private float sleepStartTime;

	private float m_DistanceWithPlayer = float.MaxValue;

	private float m_WaitPlayStartTime = -18000f;

	private AudioController m_ChenZhenMusicAduioCtrl;

	private bool battachEvent;

	private ulong updateCnt;

	public NetworkInterface Net
	{
		get
		{
			if (base.Entity != null && PeGameMgr.IsMulti && _net == null)
			{
				_net = NetworkInterface.Get(base.Entity.Id);
			}
			return _net;
		}
	}

	public SkAliveEntity Alive => base.Entity.aliveEntity;

	public NpcPackageCmpt NpcPackage => mNpcPackage;

	public PeEntity ChatTarget
	{
		get
		{
			return mChatTarget;
		}
		set
		{
			mChatTarget = value;
		}
	}

	public ENpcType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public ENpcJob Job
	{
		get
		{
			if (!NpcTypeDb.CanRun(NpcControlCmdId, ENpcControlType.Work))
			{
				return ENpcJob.Resident;
			}
			if (lineType != 0 || !m_caCanIdle || (m_Job != ENpcJob.Resident && m_Job != ENpcJob.Processor && !NpcThinkDb.CanDoing(base.Entity, EThinkingType.Work)))
			{
				return ENpcJob.Resident;
			}
			return m_Job;
		}
		set
		{
			PeNpcGroup.Instance.OnCsJobChange(base.Entity, m_Job, value);
			m_Job = value;
			if (m_Job == ENpcJob.Resident || m_Job == ENpcJob.None)
			{
				ThinkRemove(EThinkingType.Work);
			}
			else
			{
				ThinkAdd(EThinkingType.Work);
			}
			m_CanWander = ((m_Job == ENpcJob.Resident || m_Job == ENpcJob.None) ? true : false);
		}
	}

	public ETrainingType TrainningType
	{
		get
		{
			return e_TrainningType;
		}
		set
		{
			e_TrainningType = value;
		}
	}

	public ETrainerType TrainerType
	{
		get
		{
			return e_TrainerType;
		}
		set
		{
			e_TrainerType = value;
		}
	}

	public ENpcTalkType NpcTalkTYpe
	{
		get
		{
			return e_NpcTalkType;
		}
		set
		{
			e_NpcTalkType = value;
		}
	}

	public ENpcBattProfession Profession
	{
		get
		{
			return m_Profession;
		}
		set
		{
			m_Profession = value;
		}
	}

	public BattleMgr BattleMgr
	{
		get
		{
			if (m_BattleMgr == null)
			{
				m_BattleMgr = new BattleMgr(base.Entity);
			}
			return m_BattleMgr;
		}
	}

	public ENpcBattle Battle
	{
		get
		{
			return m_Battle;
		}
		set
		{
			if (m_Battle != value)
			{
				m_Battle = value;
				UpdateBattle();
			}
		}
	}

	public ENpcSoldier Soldier
	{
		get
		{
			return m_Soldier;
		}
		set
		{
			m_Soldier = value;
		}
	}

	public ENpcState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public ENpcAiType AiType
	{
		get
		{
			return m_AiType;
		}
		set
		{
			m_AiType = value;
		}
	}

	public ENpcMotionStyle MotionStyle
	{
		get
		{
			return m_MotionStyle;
		}
		set
		{
			m_MotionStyle = value;
			UpdateNpcMotionStyle();
		}
	}

	public ELineType lineType => m_LineType;

	public object[] TeamData => mTeamData;

	public bool csCanIdle => m_caCanIdle;

	public CSCreator Creater
	{
		get
		{
			if (m_BaseNpcOutMission || IsFollower)
			{
				return null;
			}
			return m_Creater;
		}
		set
		{
			if (m_Creater != null && value == null)
			{
				SendTalkMsg(2, 0f, ENpcSpeakType.Both);
			}
			m_Creater = value;
			UpdateType();
			if (m_Creater != null && m_Behave != null)
			{
				m_Behave.Excute();
			}
		}
	}

	public IOperation Work
	{
		get
		{
			return m_Work;
		}
		set
		{
			m_Work = value;
		}
	}

	public IOperation Cure
	{
		get
		{
			return m_Cure;
		}
		set
		{
			m_Cure = value;
		}
	}

	public IOperation Sleep
	{
		get
		{
			return m_Sleep;
		}
		set
		{
			m_Sleep = value;
		}
	}

	public IOperation Trainner
	{
		get
		{
			return m_trainner;
		}
		set
		{
			m_trainner = value;
		}
	}

	public CSEntity WorkEntity
	{
		get
		{
			return m_WorkEntity;
		}
		set
		{
			m_WorkEntity = value;
		}
	}

	public float GuardRadius
	{
		get
		{
			return m_GuardRadius;
		}
		set
		{
			m_GuardRadius = value;
		}
	}

	public Vector3 GuardPosition
	{
		get
		{
			return m_GuardPosition;
		}
		set
		{
			m_GuardPosition = value;
		}
	}

	public List<CSEntity> BaseEntities
	{
		get
		{
			return m_baseEntities;
		}
		set
		{
			m_baseEntities = value;
		}
	}

	public Ablities AbilityIDs => m_AbilityIdes;

	public NpcAblitycmpt Npcskillcmpt
	{
		get
		{
			if (m_NpcSkillcmpt == null)
			{
				m_NpcSkillcmpt = new NpcAblitycmpt(base.Entity.aliveEntity);
			}
			return m_NpcSkillcmpt;
		}
		set
		{
			m_NpcSkillcmpt = value;
		}
	}

	public List<NpcCmpt> Allys
	{
		get
		{
			UpdateAllys();
			return m_Allys;
		}
	}

	public NpcCmpt NpcSkillTarget
	{
		get
		{
			return m_NpcSkillTarget;
		}
		set
		{
			m_NpcSkillTarget = value;
		}
	}

	public int curAttributeUpTimes => mAttributeUpTimes;

	public ENpcMedicalState MedicalState
	{
		get
		{
			return m_MedicalState;
		}
		set
		{
			m_MedicalState = value;
		}
	}

	public List<PEAbnormalType> illAbnormals
	{
		get
		{
			return m_illAbnormals;
		}
		set
		{
			m_illAbnormals = value;
		}
	}

	public bool IsNeedMedicine
	{
		get
		{
			return m_IsNeedMedicine;
		}
		set
		{
			m_IsNeedMedicine = value;
		}
	}

	public PassengerCmpt Passenger => (!(base.Entity != null)) ? null : base.Entity.passengerCmpt;

	public bool IsOnVCCarrier => Passenger != null && Passenger.IsOnVCCarrier;

	public bool IsOnRail => Passenger != null && Passenger.IsOnRail;

	public EquipSelect EqSelect
	{
		get
		{
			if (m_EqSelect == null)
			{
				m_EqSelect = new EquipSelect();
			}
			return m_EqSelect;
		}
	}

	public bool FollowerWork
	{
		get
		{
			return m_FollowerWork;
		}
		set
		{
			if (!value)
			{
				RelashModel();
			}
			if (m_FollowerWork != value)
			{
				AddTalkInfo(ENpcTalkType.Business_trip, ENpcSpeakType.TopHead);
				m_FollowerWork = value;
				if (FollowerWorkStateChangeEvent != null)
				{
					FollowerWorkStateChangeEvent();
				}
			}
		}
	}

	public bool FollowerSentry
	{
		get
		{
			return m_FollowerSentry;
		}
		set
		{
			m_FollowerSentry = value;
		}
	}

	public bool FollowerCut
	{
		get
		{
			return m_FollowerCut;
		}
		set
		{
			m_FollowerCut = value;
		}
	}

	public bool servantCallback
	{
		get
		{
			return m_servantCallback;
		}
		set
		{
			m_servantCallback = value;
		}
	}

	public float FollowerReviceTime => m_FollowerReviceTime;

	public float FollowerCurReviveTime
	{
		get
		{
			return m_FollowerCurReviveTime;
		}
		set
		{
			m_FollowerCurReviveTime = value;
		}
	}

	public PeEntity Follwerentity => base.Entity;

	public Vector3 FollowerHidePostion => (!(base.Entity.target != null)) ? Vector3.zero : base.Entity.target.HidePistion;

	public ServantLeaderCmpt Master => m_Master;

	public Vector3 RecruitPos
	{
		get
		{
			return mRecruitPos;
		}
		set
		{
			mRecruitPos = value;
		}
	}

	public int GatherprotoTypeIdx
	{
		get
		{
			return m_GatherprotoTypeIdx;
		}
		set
		{
			m_GatherprotoTypeIdx = value;
		}
	}

	public float FollowDistance
	{
		get
		{
			if (!IsFollower)
			{
				return 0f;
			}
			if (Master != null)
			{
				PeTrans component = Master.GetComponent<PeTrans>();
				if (component != null)
				{
					return PEUtil.SqrMagnitude(base.Entity.peTrans.position, component.position);
				}
			}
			if (m_Request != null && m_Request.GetRequest(EReqType.FollowTarget) is RQFollowTarget { id: not 0 } rQFollowTarget)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(rQFollowTarget.id);
				if (peEntity != null)
				{
					return PEUtil.SqrMagnitude(base.Entity.peTrans.position, peEntity.position);
				}
			}
			return 0f;
		}
	}

	public bool CsBacking => _IsCsBacking;

	public bool NeedSeekHelp
	{
		get
		{
			return _NeedSeekHelp;
		}
		set
		{
			_NeedSeekHelp = value;
		}
	}

	public bool MisstionAskStop
	{
		get
		{
			return _MisstionAskStop;
		}
		set
		{
			_MisstionAskStop = value;
		}
	}

	public bool IsServant => m_Master != null;

	public bool IsFollower => IsServant || (m_Request != null && m_Request.GetRequest(EReqType.FollowTarget) != null);

	public bool IsFlollowTarget => m_Request.GetRequest(EReqType.FollowTarget) != null;

	public bool IsStoreNpc
	{
		get
		{
			return m_isStoreNpc;
		}
		set
		{
			m_isStoreNpc = value;
		}
	}

	public bool InAllys
	{
		get
		{
			return m_InAllys;
		}
		set
		{
			m_InAllys = value;
		}
	}

	public bool CanRecive => !(null == this) && base.Entity.viewCmpt != null && base.Entity.viewCmpt.hasView;

	public bool hasAnyRequest => m_Request != null && m_Request.HasAnyRequest();

	public bool IsHunger => m_IsHunger;

	public bool IsUncomfortable => m_IsUncomfortable;

	public bool IsLowHp => m_IsLowHp;

	public bool IsInSleepTime => m_IsInSleepTime;

	public bool IsInDinnerTime => m_IsInDinnerTime;

	public bool NpcUnableProcess => m_BaseNpcOutMission || !NpcTypeDb.CanRun(NpcControlCmdId, ENpcControlType.Work) || hasAnyRequest || IsNeedMedicine || lineType == ELineType.TeamEat || lineType == ELineType.TeamSleep;

	public bool NpcUnableWork => m_BaseNpcOutMission || !NpcTypeDb.CanRun(NpcControlCmdId, ENpcControlType.Work) || hasAnyRequest || IsNeedMedicine || lineType == ELineType.TeamEat || lineType == ELineType.TeamSleep;

	public bool NpcShouldStopProcessing => m_BaseNpcOutMission || hasAnyRequest || !NpcTypeDb.CanRun(NpcControlCmdId, ENpcControlType.Work);

	public ENpcUnableWorkType unableWorkReason
	{
		get
		{
			if (hasAnyRequest || m_BaseNpcOutMission)
			{
				return ENpcUnableWorkType.HasRequest;
			}
			if (IsNeedMedicine)
			{
				return ENpcUnableWorkType.IsNeedMedicine;
			}
			if (IsHunger || lineType == ELineType.TeamEat)
			{
				return ENpcUnableWorkType.IsHunger;
			}
			if (IsUncomfortable || lineType == ELineType.TeamEat)
			{
				return ENpcUnableWorkType.IsUncomfortable;
			}
			if (IsLowHp || lineType == ELineType.TeamEat)
			{
				return ENpcUnableWorkType.IsHpLow;
			}
			if (IsInSleepTime || lineType == ELineType.TeamSleep)
			{
				return ENpcUnableWorkType.IsSleeepTime;
			}
			if (IsInDinnerTime || lineType == ELineType.TeamEat)
			{
				return ENpcUnableWorkType.IsDinnerTime;
			}
			return ENpcUnableWorkType.None;
		}
	}

	public bool Processing
	{
		get
		{
			return m_Processing;
		}
		set
		{
			if (m_Processing && !value)
			{
				CallBackProcess();
			}
			m_Processing = value;
			CanWander = !m_Processing;
		}
	}

	public bool NpcCanChat => mChatTarget != null;

	public bool IsTrainning
	{
		get
		{
			return m_IsTrainning;
		}
		set
		{
			setIsTrainning(value);
			CanWander = !m_IsTrainning;
		}
	}

	public bool CanWander
	{
		get
		{
			return m_CanWander;
		}
		set
		{
			m_CanWander = value;
		}
	}

	public bool CanTalk
	{
		get
		{
			bool flag = base.Entity != null && RandomDunGenUtil.IsInDungeon(base.Entity);
			if (IsServant || base.Entity.IsDeath() || flag)
			{
				return false;
			}
			return m_CanTalk;
		}
		set
		{
			m_CanTalk = value;
		}
	}

	public bool CanHanded
	{
		get
		{
			return m_CanHanded;
		}
		set
		{
			m_CanHanded = value;
		}
	}

	public bool HasConsume => IsServant || Creater != null;

	public bool NpcInAlert
	{
		get
		{
			return m_NpcInAlert;
		}
		set
		{
			m_NpcInAlert = value;
		}
	}

	public bool BaseNpcOutMission
	{
		get
		{
			return m_BaseNpcOutMission;
		}
		set
		{
			if (!m_BaseNpcOutMission && value)
			{
				MissionReady();
			}
			if (m_BaseNpcOutMission && !value)
			{
				MissionFinish();
			}
			m_BaseNpcOutMission = value;
		}
	}

	public bool HasNearleague => m_HasNearleague;

	public bool bRunAway
	{
		get
		{
			return m_bRunAway;
		}
		set
		{
			m_bRunAway = value;
		}
	}

	public int voiceType { get; set; }

	public NpaTalkAgent TalkAngent
	{
		get
		{
			if (mTalkAgent == null)
			{
				mTalkAgent = new NpaTalkAgent(base.Entity);
			}
			return mTalkAgent;
		}
	}

	public NpcThinkAgent ThinkAgent
	{
		get
		{
			if (mThinkAgent == null)
			{
				mThinkAgent = new NpcThinkAgent();
			}
			return mThinkAgent;
		}
	}

	public NpcCheckTime npcCheck
	{
		get
		{
			if (mNpcCheckTime == null)
			{
				mNpcCheckTime = new NpcCheckTime();
			}
			return mNpcCheckTime;
		}
	}

	public Camp Campsite
	{
		get
		{
			return m_Camp;
		}
		set
		{
			if (m_Camp != value)
			{
				m_Camp = value;
				UpdateType();
			}
		}
	}

	public bool UpdateCampsite
	{
		set
		{
			m_UpdateCampsite = value;
		}
	}

	public RandomItemObj mRandomItemObj
	{
		get
		{
			return m_RandomItemobj;
		}
		set
		{
			if (m_RandomItemobj != null && value == null)
			{
				m_RandomItemobj.TryGenObject();
			}
			m_RandomItemobj = value;
		}
	}

	public int ReviveTime
	{
		get
		{
			return m_ReviveTime;
		}
		set
		{
			m_ReviveTime = value;
		}
	}

	public int MountID
	{
		get
		{
			return m_MountID;
		}
		set
		{
			if (m_MountID == value)
			{
				return;
			}
			if (value == 0)
			{
				m_Mount = null;
			}
			else
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(value);
				if (peEntity != null)
				{
					PeTrans component = peEntity.GetComponent<PeTrans>();
					Transform child = PEUtil.GetChild(component.existent, "CarryUp");
					if (child != null)
					{
						if (base.Entity.biologyViewCmpt != null)
						{
							base.Entity.biologyViewCmpt.ActivateCollider(value: false);
						}
						if (base.Entity.motionMgr != null)
						{
							base.Entity.motionMgr.FreezePhyState(GetType(), v: true);
						}
						if (base.Entity.enityInfoCmpt != null)
						{
							base.Entity.enityInfoCmpt.ShowName(show: false);
							base.Entity.enityInfoCmpt.ShowMissionMark(show: false);
						}
						if (base.Entity.biologyViewCmpt != null)
						{
							base.Entity.biologyViewCmpt.ActivateInjured(value: false);
						}
						Req_SetIdle("BeCarry");
						m_Mount = child;
					}
				}
			}
			m_MountID = value;
			Req_Mount(value);
		}
	}

	public Vector3 NpcPostion => base.Entity.peTrans.trans.position;

	public Vector3 FixedPointPos
	{
		get
		{
			return mFixedPointPos;
		}
		set
		{
			SetFixPos(value);
		}
	}

	public float StandRotate
	{
		get
		{
			return mStandRotate;
		}
		set
		{
			mStandRotate = value;
		}
	}

	public int NpcControlCmdId
	{
		get
		{
			return mNpcControlCmdId;
		}
		set
		{
			UpdateNpcControlInfo(value);
			mNpcControlCmdId = value;
		}
	}

	public Vector3 PlayerPostion
	{
		get
		{
			if (m_mianplayerTran == null)
			{
				m_mianplayerTran = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
			}
			return m_mianplayerTran.position;
		}
	}

	public PEBuilding OccopyBuild
	{
		get
		{
			return m_OccopyBuild;
		}
		set
		{
			m_OccopyBuild = value;
		}
	}

	public float NpcHppercent => (!(base.Entity != null)) ? 0f : base.Entity.HPPercent;

	public event Action<int> OnAddEnemyLock;

	public event Action<int> OnRemoveEnemyLock;

	public event Action OnClearEnemyLocked;

	public event Action<PeEntity> OnServentDie;

	public event Action<PeEntity> OnServentRevive;

	public void DispatchAddEnemyLock(int entityId)
	{
		if (this.OnAddEnemyLock != null)
		{
			this.OnAddEnemyLock(entityId);
		}
	}

	public void DispatchRemoveEnemyLock(int entityId)
	{
		if (this.OnRemoveEnemyLock != null)
		{
			this.OnRemoveEnemyLock(entityId);
		}
	}

	public void DispatchClearEnemyLock()
	{
		if (this.OnClearEnemyLocked != null)
		{
			this.OnClearEnemyLocked();
		}
	}

	public void SetLineType(ELineType type)
	{
		if (!type.Equals(null) && !m_LineType.Equals(null))
		{
			if (m_LineType != type)
			{
				PeNpcGroup.Instance.OnCsLineChange(base.Entity, m_LineType, type);
			}
			m_LineType = type;
		}
	}

	public void setTeamData(params object[] objs)
	{
		mTeamData = objs;
	}

	public void SetCanIdle(bool _canIdle)
	{
		m_caCanIdle = _canIdle;
	}

	public void SetAtkType(AttackType type)
	{
		if (mNpcAtkType != type)
		{
			PeNpcGroup.Instance.OnCsAttackTypeChange(base.Entity, mNpcAtkType, type);
		}
		mNpcAtkType = type;
	}

	public bool AddEnemyLocked(PeEntity enemy)
	{
		if (m_MountID != 0)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_MountID);
			if (peEntity != null)
			{
				return transferAddEnemyLocked(peEntity, enemy);
			}
			return false;
		}
		if (beEnemiesLocked == null)
		{
			beEnemiesLocked = new List<PeEntity>();
		}
		if (!beEnemiesLocked.Contains(enemy))
		{
			beEnemiesLocked.Add(enemy);
			DispatchAddEnemyLock(enemy.Id);
		}
		return true;
	}

	public bool RemoveEnemyLocked(PeEntity enemy)
	{
		if (m_MountID != 0)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_MountID);
			if (peEntity != null)
			{
				return transferRemoveEnemyLocked(peEntity, enemy);
			}
			return false;
		}
		if (beEnemiesLocked == null)
		{
			return false;
		}
		if (beEnemiesLocked.Contains(enemy))
		{
			DispatchRemoveEnemyLock(enemy.Id);
		}
		return beEnemiesLocked.Remove(enemy);
	}

	public void ClearLockedEnemies()
	{
		if (beEnemiesLocked != null && beEnemiesLocked.Count > 0)
		{
			DispatchClearEnemyLock();
			beEnemiesLocked.Clear();
		}
	}

	public bool HasEnemyLocked()
	{
		if (beEnemiesLocked == null)
		{
			return false;
		}
		for (int i = 0; i < beEnemiesLocked.Count; i++)
		{
			if (beEnemiesLocked[i] != null && beEnemiesLocked[i].hasView)
			{
				return true;
			}
		}
		ClearLockedEnemies();
		return false;
	}

	public bool transferAddEnemyLocked(PeEntity other, PeEntity enemy)
	{
		if (other == null || other.NpcCmpt == null)
		{
			return false;
		}
		return other.NpcCmpt.AddEnemyLocked(enemy);
	}

	public bool transferRemoveEnemyLocked(PeEntity other, PeEntity enemy)
	{
		if (other == null || other.NpcCmpt == null)
		{
			return false;
		}
		return other.NpcCmpt.RemoveEnemyLocked(enemy);
	}

	public void AddFollowRobot(PeEntity robot)
	{
		m_RobotEntity = robot;
	}

	public void AddTitle(ENpcTitle title)
	{
		m_Title = ENpcTitle.None;
		m_Title |= title;
	}

	public void RemoveTitle(ENpcTitle title)
	{
		m_Title &= ~title;
	}

	public bool ContainsTitle(ENpcTitle title)
	{
		return (m_Title & title) != 0;
	}

	public void SetAbilityIDs(Ablities abl)
	{
		m_AbilityIdes = abl;
		m_AbilityIdes.SetDirty(bDirty: true);
	}

	public void AddAbility(int Id)
	{
		if (!m_AbilityIdes.Contains(Id))
		{
			m_AbilityIdes.Add(Id);
		}
	}

	public bool RemoveAbliy(int Id)
	{
		if (m_AbilityIdes.Remove(Id))
		{
			return true;
		}
		return false;
	}

	public bool HasAllys()
	{
		UpdateAllys();
		return m_Allys.Count > 1;
	}

	public bool Containself()
	{
		if (m_Allys == null)
		{
			return false;
		}
		return m_Allys.Contains(this);
	}

	public List<int> GetSkllIds()
	{
		if (Npcskillcmpt == null)
		{
			return null;
		}
		return Npcskillcmpt.GetSkillIDs();
	}

	public int GetReadySkill()
	{
		List<int> skllIds = GetSkllIds();
		if (skllIds == null)
		{
			return -1;
		}
		foreach (int item in skllIds)
		{
			if (!base.Entity.aliveEntity.IsSkillRunning(item))
			{
				return item;
			}
		}
		return -1;
	}

	public float GetNpcSkillRange(int skillId)
	{
		if (m_NpcSkillcmpt == null)
		{
			return 0f;
		}
		return m_NpcSkillcmpt.GetCmptSkillRange(skillId);
	}

	public bool TryGetItemSkill(Vector3 pos, float percent = 1f)
	{
		if (m_NpcSkillcmpt != null)
		{
			return m_NpcSkillcmpt.TryGetItemskill(pos, percent) != null;
		}
		return false;
	}

	public float GetHpPerChange()
	{
		List<int> skllIds = GetSkllIds();
		if (skllIds == null)
		{
			return 0f;
		}
		using (List<int>.Enumerator enumerator = skllIds.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				return GetNpcChange_Hp(current);
			}
		}
		return 0f;
	}

	public float GetNpcChange_Hp(int SkillId)
	{
		if (m_NpcSkillcmpt == null)
		{
			return 0f;
		}
		return m_NpcSkillcmpt.GetChangeHpPer(SkillId);
	}

	public void AttributeUpgrade(AttribType type, float value)
	{
		if (CanAttributeUp())
		{
			AttPlusBuffDb.Item item = AttPlusBuffDb.Get(type);
			if (item != null)
			{
				List<int> list = new List<int>();
				list.Add(0);
				List<float> list2 = new List<float>();
				list2.Add(value);
				mAttributeUpTimes++;
				SkEntity.MountBuff(base.Entity.aliveEntity, item._buffId, list, list2);
			}
		}
	}

	public bool CanAttributeUp()
	{
		return AttPlusNPCData.ComparePlusCout(base.Entity.entityProto.protoId, mAttributeUpTimes);
	}

	public void AddSick(PEAbnormalType type)
	{
		if (base.Entity != null && base.Entity.Alnormal != null)
		{
			base.Entity.Alnormal.StartAbnormalCondition(type);
		}
	}

	public void CureSick(PEAbnormalType type)
	{
		if (base.Entity.Alnormal != null)
		{
			base.Entity.Alnormal.EndAbnormalCondition(type);
			if (m_illAbnormals != null && m_illAbnormals.Contains(type))
			{
				m_illAbnormals.Remove(type);
			}
			int cureSkillId = AbnormalTypeTreatData.GetCureSkillId((int)type);
			if (cureSkillId > 0)
			{
				SkEntity.MountBuff(base.Entity.aliveEntity, cureSkillId, new List<int>(), new List<float>());
			}
			m_IsNeedMedicine = NeedToMedical();
		}
	}

	private bool NeedToMedical()
	{
		m_illAbnormals.Clear();
		int count = AbnormalTypeTreatData.treatmentDatas.Count;
		for (int i = 0; i < count; i++)
		{
			int abnormalId = AbnormalTypeTreatData.treatmentDatas[i].abnormalId;
			if (base.Entity.Alnormal.CheckAbnormalCondition((PEAbnormalType)abnormalId) && AbnormalTypeTreatData.CanBeTreatInColony(abnormalId) && !m_illAbnormals.Contains((PEAbnormalType)abnormalId))
			{
				m_illAbnormals.Add((PEAbnormalType)abnormalId);
			}
		}
		return m_illAbnormals.Count > 0;
	}

	public void ServantCallBack()
	{
		if (!(Master == null))
		{
			if (base.Entity.target != null)
			{
				base.Entity.target.ClearEnemy();
			}
			if (base.Entity.biologyViewCmpt != null)
			{
				base.Entity.biologyViewCmpt.Fadein();
			}
			Vector3 position = PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetPos();
			position.z += 2f;
			position.y += 1f;
			Req_Translate(position);
			m_servantCallback = true;
		}
	}

	private void RelashModel()
	{
		Vector3 value = PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetPos();
		value.z += 2f;
		value.y += 1f;
		base.Entity.ExtSetPos(value);
	}

	public void SetServantLeader(ServantLeaderCmpt leader)
	{
		if (!(m_Master != leader))
		{
			return;
		}
		if (m_Master != null && leader == null)
		{
			SendTalkMsg(ENpcTalkType.Dissolve, ENpcSpeakType.TopHead);
			RemoveServentSign();
			if (base.Entity.aliveEntity != null && base.Entity.aliveEntity.isDead)
			{
				Singleton<PeLogicGlobal>.Instance.ReviveEntity(base.Entity.aliveEntity, ReviveTime);
			}
		}
		m_Master = leader;
		UpdateType();
		UpdateBattle();
		if (m_Master != null && m_Behave != null)
		{
			m_Behave.Excute();
		}
	}

	public void ServentProcet()
	{
		if (m_Master != null)
		{
			Vector3 position = m_Master.Entity.position;
			Req_Translate(position);
		}
	}

	private void AddServentDieSign()
	{
		if (!(base.Entity == null) && IsServant)
		{
			ServantDeadLabel servantDeadLabel = null;
			servantDeadLabel = ((base.Entity.entityProto.proto != EEntityProto.RandomNpc) ? new ServantDeadLabel(43, NpcPostion, base.Entity.enityInfoCmpt.characterName.fullName, base.Entity.entityProto.protoId) : new ServantDeadLabel(43, NpcPostion, base.Entity.enityInfoCmpt.characterName.fullName, base.Entity.Id));
			PeSingleton<LabelMgr>.Instance.Add(servantDeadLabel);
		}
	}

	private void RemoveServentSign()
	{
		ILabel label = null;
		label = ((base.Entity.entityProto.proto != EEntityProto.RandomNpc) ? PeSingleton<LabelMgr>.Instance.Find((ILabel item) => item is ServantDeadLabel && ((ServantDeadLabel)item).servantId == base.Entity.entityProto.protoId) : PeSingleton<LabelMgr>.Instance.Find((ILabel item) => item is ServantDeadLabel && ((ServantDeadLabel)item).servantId == base.Entity.Id));
		PeSingleton<LabelMgr>.Instance.Remove(label);
	}

	public void SetCsBacking(bool value)
	{
		_IsCsBacking = value;
	}

	public bool IsIdle()
	{
		return !hasAnyRequest && m_Type == ENpcType.Field;
	}

	public bool CallBackProcess()
	{
		return !Equals(null) && NpcMgr.CallBackColonyNpcImmediately(base.Entity);
	}

	private void setIsTrainning(bool value)
	{
		m_IsTrainning = value;
	}

	private void MissionReady()
	{
		NpcMgr.NpcMissionReady(base.Entity);
	}

	private void MissionFinish()
	{
		NpcMgr.NpcMissionFinish(base.Entity);
	}

	public bool NpcNeedRest(float Percent = 0.1f)
	{
		if (base.Entity.aliveEntity == null)
		{
			return false;
		}
		float attribute = base.Entity.aliveEntity.GetAttribute(AttribType.Hunger);
		float attribute2 = base.Entity.aliveEntity.GetAttribute(AttribType.HungerMax);
		float attribute3 = base.Entity.aliveEntity.GetAttribute(AttribType.Comfort);
		float attribute4 = base.Entity.aliveEntity.GetAttribute(AttribType.ComfortMax);
		return attribute <= attribute2 * Percent || attribute3 <= attribute4 * Percent;
	}

	private void ThinkAdd(EThinkingType type)
	{
		if (ThinkAgent != null && NpcThinkDb.CanDoing(base.Entity, type))
		{
			ThinkAgent.AddThink(type);
		}
	}

	private void ThinkRemove(EThinkingType type)
	{
		if (ThinkAgent != null)
		{
			ThinkAgent.RemoveThink(type);
		}
	}

	public void AddTalkInfo(ENpcTalkType talkType, ENpcSpeakType spType, bool canLoop = false)
	{
		TalkAngent.AddAgentInfo(new NpaTalkAgent.AgentInfo(talkType, spType, canLoop));
	}

	public bool RmoveTalkInfo(ENpcTalkType type)
	{
		return TalkAngent.RemoveAgentInfo(type);
	}

	public bool SendTalkMsg(ENpcTalkType talkType, ENpcSpeakType spType, float loopTime = 0f)
	{
		return SendTalkMsg((int)talkType, loopTime, spType);
	}

	public bool SendTalkMsg(int caseid, float time = 0f, ENpcSpeakType speaker = ENpcSpeakType.TopHead)
	{
		int talkCase = NpcRandomTalkDb.GetTalkCase(caseid);
		if (talkCase < 0)
		{
			return false;
		}
		if (base.Entity == null || base.Entity.IsDead() || base.Entity.enityInfoCmpt == null || base.Entity.aliveEntity == null || base.Entity.aliveEntity.isDead)
		{
			return false;
		}
		base.Entity.enityInfoCmpt.NpcSayOneWord(talkCase, time, speaker);
		NpcRandomTalkAudio.PlaySound(base.Entity, caseid, talkCase);
		return true;
	}

	public void SetFixPos(Vector3 pos)
	{
		bool flag = pos.x < 0f && pos.z < 0f;
		if (!PeGameMgr.IsSingleStory || flag || WorldCollider.IsPointInWorld(pos))
		{
			mFixedPointPos = pos;
		}
	}

	private void UpdateAllys()
	{
		NpcCmpt[] servants = ServantLeaderCmpt.Instance.GetServants();
		List<NpcCmpt> mForcedFollowers = ServantLeaderCmpt.Instance.mForcedFollowers;
		List<NpcCmpt> list = new List<NpcCmpt>();
		if (MissionManager.Instance != null)
		{
			list = MissionManager.Instance.m_PlayerMission.followers;
		}
		NpcCmpt npcCmpt = null;
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			npcCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<NpcCmpt>();
		}
		if (m_Allys == null)
		{
			m_Allys = new List<NpcCmpt>();
		}
		m_Allys.Clear();
		if (npcCmpt != null)
		{
			m_Allys.Add(npcCmpt);
		}
		for (int i = 0; i < servants.Length; i++)
		{
			if (servants[i] != null)
			{
				m_Allys.Add(servants[i]);
			}
		}
		foreach (NpcCmpt item in mForcedFollowers)
		{
			if (!m_Allys.Contains(item))
			{
				m_Allys.Add(item);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (NpcCmpt item2 in list)
		{
			if (!m_Allys.Contains(item2) && item2 != this)
			{
				m_Allys.Add(item2);
			}
		}
	}

	private void UpdateType()
	{
		if (m_Master != null)
		{
			m_Type = ENpcType.Follower;
		}
		else if (Creater != null)
		{
			m_Type = ENpcType.Base;
		}
		else if (m_Camp != null)
		{
			m_Type = ENpcType.Campsite;
		}
		else
		{
			m_Type = ENpcType.Field;
		}
	}

	private void UpdateMount()
	{
		if (m_MountID != 0 && m_Mount == null && Req_GetRequest(EReqType.Idle) is RQIdle { state: "BeCarry" })
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_MountID);
			if (peEntity != null)
			{
				PeTrans component = peEntity.GetComponent<PeTrans>();
				Transform child = PEUtil.GetChild(component.existent, "CarryUp");
				if (child != null)
				{
					m_Mount = child;
					if (base.Entity.biologyViewCmpt != null)
					{
						base.Entity.biologyViewCmpt.ActivateCollider(value: false);
					}
					if (base.Entity.motionMgr != null)
					{
						base.Entity.motionMgr.FreezePhyState(typeof(StroyManager), v: true);
					}
					if (base.Entity.enityInfoCmpt != null)
					{
						base.Entity.enityInfoCmpt.ShowName(show: false);
						base.Entity.enityInfoCmpt.ShowMissionMark(show: false);
					}
				}
			}
		}
		if (m_Mount != null && !base.Entity.isRagdoll)
		{
			base.Entity.peTrans.position = m_Mount.position;
			base.Entity.peTrans.rotation = m_Mount.rotation;
		}
	}

	private void UpdateNpcControlInfo(int cmdid)
	{
		NpcTypeDb.Item item = NpcTypeDb.Get(cmdid);
		if (item != null)
		{
			if (base.Entity.target != null)
			{
				base.Entity.target.ClearEnemy();
				base.Entity.target.Scan = item.CanRun(ENpcControlType.AddHatred);
				base.Entity.target.IsAddHatred = item.CanRun(ENpcControlType.InjuredHatred);
				base.Entity.target.CanTransferHatred = item.CanRun(ENpcControlType.ReceiveHatred);
			}
			m_CanTalk = item.CanRun(ENpcControlType.CanTalk);
			m_CanHanded = item.CanRun(ENpcControlType.CanHanded);
		}
	}

	private void UpdateAbility()
	{
		if (m_AbilityIdes != null && m_AbilityIdes.GetDrity())
		{
			Npcskillcmpt.SetAblitiyIDs(m_AbilityIdes);
			m_AbilityIdes.SetDirty(bDirty: false);
		}
	}

	private void UpdateBattle()
	{
		switch (m_Battle)
		{
		case ENpcBattle.Attack:
			if (base.Entity.target != null)
			{
				base.Entity.target.Scan = true;
				base.Entity.target.IsAddHatred = true;
				base.Entity.target.CanActiveAttck = true;
				base.Entity.target.CanTransferHatred = true;
				m_FollowerSentry = false;
			}
			break;
		case ENpcBattle.Defence:
			if (base.Entity.target != null)
			{
				base.Entity.target.ClearEnemy();
				base.Entity.target.Scan = false;
				base.Entity.target.IsAddHatred = true;
				base.Entity.target.CanActiveAttck = true;
				base.Entity.target.CanTransferHatred = true;
				m_FollowerSentry = false;
			}
			break;
		case ENpcBattle.Passive:
			if (base.Entity.target != null)
			{
				base.Entity.target.ClearEnemy();
				base.Entity.target.Scan = false;
				base.Entity.target.IsAddHatred = false;
				base.Entity.target.CanTransferHatred = false;
				base.Entity.target.CanActiveAttck = false;
				m_FollowerSentry = false;
			}
			break;
		case ENpcBattle.Evasion:
			if (base.Entity.target != null)
			{
				base.Entity.target.ClearEnemy();
				base.Entity.target.Scan = true;
				base.Entity.target.IsAddHatred = false;
				base.Entity.target.CanTransferHatred = false;
				base.Entity.target.CanActiveAttck = false;
				m_FollowerSentry = false;
			}
			break;
		case ENpcBattle.Stay:
			if (base.Entity.target != null)
			{
				base.Entity.target.ClearEnemy();
				base.Entity.target.Scan = false;
				base.Entity.target.IsAddHatred = true;
				base.Entity.target.CanActiveAttck = true;
				base.Entity.target.CanTransferHatred = true;
				m_FollowerSentry = true;
			}
			break;
		}
	}

	private void UpdateNpcTalk()
	{
		if (base.Entity == null || base.Entity.aliveEntity == null || base.Entity.aliveEntity.isDead || base.Entity.commonCmpt == null || base.Entity.commonCmpt.IsPlayer)
		{
			return;
		}
		if (IsServant)
		{
			if (IsNeedMedicine)
			{
				AddTalkInfo(ENpcTalkType.NpcSick, ENpcSpeakType.TopHead, canLoop: true);
			}
			if (base.Entity.target != null && base.Entity.target.HasEnemy())
			{
				AddTalkInfo(ENpcTalkType.NpcCombat, ENpcSpeakType.TopHead, canLoop: true);
			}
			if (base.Entity.motionEquipment != null && base.Entity.motionEquipment.ActiveableEquipment != null && !CheckNpcEquipment_Durability())
			{
				AddTalkInfo(ENpcTalkType.Follower_LackDurability, ENpcSpeakType.TopHead, canLoop: true);
			}
			if (!CheckNpcEquipment_Ammunition())
			{
				AddTalkInfo(ENpcTalkType.Follower_LackAmmunition, ENpcSpeakType.TopHead, canLoop: true);
			}
			TalkAngent.RunAttrAgent(base.Entity);
		}
		if (Creater != null)
		{
			TalkAngent.RunAgent();
		}
	}

	private void UpdateNpcThink()
	{
		if (updateCnt % 10 != 0L || base.Entity == null || base.Entity.aliveEntity == null || base.Entity.aliveEntity.isDead || base.Entity.commonCmpt == null || base.Entity.commonCmpt.IsPlayer)
		{
			return;
		}
		if (Creater != null && Creater.Assembly != null)
		{
			if (IsNeedMedicine)
			{
				AddTalkInfo(ENpcTalkType.NpcSick, ENpcSpeakType.Both, canLoop: true);
				ThinkAdd(EThinkingType.Cure);
			}
			else
			{
				ThinkAgent.RemoveThink(EThinkingType.Cure);
			}
			if (m_IsUncomfortable)
			{
				ThinkAdd(EThinkingType.Sleep);
			}
			if (lineType == ELineType.TeamSleep)
			{
				ThinkAdd(EThinkingType.Sleep);
			}
			else
			{
				ThinkAgent.RemoveThink(EThinkingType.Sleep);
			}
			if (CSNpcTeam.checkTime != null && CSNpcTeam.checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay) && NpcEatDb.CanEatSthFromStorages(base.Entity, Creater.Assembly.Storages))
			{
				ThinkAdd(EThinkingType.Dining);
			}
			else
			{
				ThinkAgent.RemoveThink(EThinkingType.Dining);
			}
		}
		if (Creater == null)
		{
			if (m_IsUncomfortable || m_IsHunger || m_IsLowHp)
			{
				ThinkAdd(EThinkingType.Dining);
			}
			else
			{
				ThinkAgent.RemoveThink(EThinkingType.Dining);
			}
		}
		if (base.Entity.target != null && !Enemy.IsNullOrInvalid(base.Entity.attackEnemy))
		{
			ThinkAdd(EThinkingType.Combat);
		}
		else
		{
			ThinkAgent.RemoveThink(EThinkingType.Combat);
		}
		if (hasAnyRequest)
		{
			ThinkAdd(EThinkingType.Mission);
		}
		else
		{
			ThinkAgent.RemoveThink(EThinkingType.Mission);
		}
	}

	private void UpdateDisWithPlayer()
	{
		if (base.Entity.hasView)
		{
			if (PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer)
			{
				m_DistanceWithPlayer = Vector3.Distance(base.Entity.position, PeSingleton<PeCreature>.Instance.mainPlayer.position);
			}
			if (base.Entity.Id == 9007 && m_DistanceWithPlayer <= 3f && (bool)MissionManager.Instance && !MissionManager.Instance.HadCompleteMission(10047) && Time.realtimeSinceStartup - m_WaitPlayStartTime >= 18000f && null == m_ChenZhenMusicAduioCtrl && null != base.Entity.peTrans)
			{
				m_ChenZhenMusicAduioCtrl = AudioManager.instance.Create(base.Entity.position, 4513, base.Entity.peTrans.realTrans);
				AudioController chenZhenMusicAduioCtrl = m_ChenZhenMusicAduioCtrl;
				chenZhenMusicAduioCtrl.DestroyEvent = (Action<AudioController>)Delegate.Combine(chenZhenMusicAduioCtrl.DestroyEvent, new Action<AudioController>(ChenZhenMusicDeleteEvent));
			}
		}
	}

	private void ChenZhenMusicDeleteEvent(AudioController audio)
	{
		if (audio == m_ChenZhenMusicAduioCtrl)
		{
			AudioController chenZhenMusicAduioCtrl = m_ChenZhenMusicAduioCtrl;
			chenZhenMusicAduioCtrl.DestroyEvent = (Action<AudioController>)Delegate.Remove(chenZhenMusicAduioCtrl.DestroyEvent, new Action<AudioController>(ChenZhenMusicDeleteEvent));
			m_ChenZhenMusicAduioCtrl = null;
			m_WaitPlayStartTime = Time.realtimeSinceStartup;
		}
	}

	private void UpdateNpcMotionStyle()
	{
		ENpcMotionStyle motionStyle = m_MotionStyle;
		if (motionStyle == ENpcMotionStyle.InjuredSitEX)
		{
			base.Entity.motionMove.baseMoveStyle = MoveStyle.Abnormal;
		}
		else
		{
			base.Entity.motionMove.baseMoveStyle = MoveStyle.Normal;
		}
	}

	private void UpdateNpcAttr()
	{
		if (!(base.Entity == null) && !base.Entity.IsDeath())
		{
			float num = base.Entity.GetAttribute(AttribType.Hp) / base.Entity.GetAttribute(AttribType.HpMax);
			m_IsLowHp = num <= LOW_PERCENT;
			num = base.Entity.GetAttribute(AttribType.Comfort) / base.Entity.GetAttribute(AttribType.ComfortMax);
			m_IsUncomfortable = num <= LOW_PERCENT;
			num = base.Entity.GetAttribute(AttribType.Hunger) / base.Entity.GetAttribute(AttribType.HungerMax);
			m_IsHunger = num <= LOW_PERCENT;
		}
	}

	private void UpdateTime()
	{
		if (!(base.Entity == null))
		{
			if (Campsite != null || Creater != null)
			{
				m_IsInDinnerTime = npcCheck.IsEatTimeSlot((float)GameTime.Timer.HourInDay);
				m_IsInSleepTime = npcCheck.IsSleepTimeSlot((float)GameTime.Timer.HourInDay);
			}
			if (NpcPackage != null && NpcPackage.IsFull())
			{
				AddTalkInfo(ENpcTalkType.Follower_Pkg_full, ENpcSpeakType.TopHead, canLoop: true);
			}
			if (NpcPackage != null && !NpcPackage.IsFull())
			{
				RmoveTalkInfo(ENpcTalkType.Follower_Pkg_full);
			}
			if (Time.time - sleepStartTime >= sleepWaitTime)
			{
				NpcSleep();
			}
		}
	}

	private void CheckNearLeague()
	{
		int playerID = (int)base.Entity.GetAttribute(AttribType.DefaultPlayerID);
		m_HasNearleague = PeSingleton<EntityMgr>.Instance.NearEntityModel(base.Entity.peTrans.position, 0.3f, playerID, isDeath: false, base.Entity);
	}

	private bool CheckNpcEquipment_Ammunition()
	{
		if (base.Entity.motionEquipment == null)
		{
			return false;
		}
		return base.Entity.motionEquipment.CheckEquipmentAmmunition();
	}

	private bool CheckNpcEquipment_Durability()
	{
		if (base.Entity.motionEquipment == null)
		{
			return false;
		}
		return base.Entity.motionEquipment.CheckEquipmentDurability();
	}

	private void AttachEvent()
	{
		if (null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			Action_Fell action = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>().GetAction<Action_Fell>();
			if (action != null)
			{
				battachEvent = true;
				action.startFell += OnStartFell;
				action.endFell += OnEndFell;
			}
		}
	}

	private IEnumerator IgnorePlant(float radius)
	{
		m_IgnorePlantColliders = new List<Collider>();
		while (true)
		{
			if (m_Job == ENpcJob.Farmer)
			{
				Collider[] colliders = Physics.OverlapSphere(base.Entity.peTrans.position, radius, 1);
				Collider[] array = colliders;
				foreach (Collider collider in array)
				{
					if (!m_IgnorePlantColliders.Contains(collider))
					{
						base.Entity.biologyViewCmpt.IgnoreCollision(collider);
						m_IgnorePlantColliders.Add(collider);
					}
				}
			}
			else if (m_IgnorePlantColliders.Count > 0)
			{
				foreach (Collider collider2 in m_IgnorePlantColliders)
				{
					base.Entity.biologyViewCmpt.IgnoreCollision(collider2, isIgnore: false);
				}
				m_IgnorePlantColliders.Clear();
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator CalculateCamp()
	{
		while (true)
		{
			if (Creater == null)
			{
				if (!m_UpdateCampsite)
				{
					Campsite = null;
				}
				else if (Campsite == null)
				{
					Campsite = Camp.GetCamp(base.Entity.peTrans.position);
					if (Campsite != null)
					{
						Campsite.AddNpcIntoCamp(base.Entity.Id);
					}
				}
				else
				{
					Camp _tmpCampsite = Camp.GetCamp(base.Entity.peTrans.position);
					if (_tmpCampsite == null)
					{
						Campsite.RemoveFromCamp(base.Entity.Id);
					}
					if (_tmpCampsite != null && !_tmpCampsite.Equals(Campsite))
					{
						_tmpCampsite.AddNpcIntoCamp(base.Entity.Id);
						Campsite.RemoveFromCamp(base.Entity.Id);
					}
					Campsite = _tmpCampsite;
				}
			}
			if (Creater != null && Campsite != null)
			{
				Campsite = null;
			}
			yield return new WaitForSeconds(5f);
		}
	}

	private IEnumerator CheckMedicine()
	{
		yield return new WaitForSeconds(0.5f);
		while (true)
		{
			if (base.Entity != null && base.Entity.Alnormal != null)
			{
				if (m_illAbnormals == null)
				{
					m_illAbnormals = new List<PEAbnormalType>();
				}
				m_IsNeedMedicine = NeedToMedical();
				if (!m_IsNeedMedicine)
				{
					MedicalState = ENpcMedicalState.Cure;
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator ThinkingSth()
	{
		while (true)
		{
			UpdateNpcAttr();
			UpdateTime();
			yield return new WaitForSeconds(5f);
		}
	}

	private IEnumerator NpcTalk(float time)
	{
		while (true)
		{
			UpdateNpcTalk();
			if (base.Entity.hasView)
			{
				CheckNearLeague();
			}
			float wTime = UnityEngine.Random.Range(1f, time);
			yield return new WaitForSeconds(wTime);
		}
	}

	public bool Req_UseSkill()
	{
		if (Net != null && !Net.hasOwnerAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 11);
			return true;
		}
		if (m_Request != null && m_Request.Register(EReqType.UseSkill, null) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_Mount(int mountId)
	{
		if (Net != null && Net.hasOwnerAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_Mount, mountId);
			return true;
		}
		return false;
	}

	public bool Req_PauseAll()
	{
		if (m_Request != null && m_Request.Register(EReqType.PauseAll) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_Dialogue(params object[] objs)
	{
		if (m_Request != null && m_Request.Register(EReqType.Dialogue, objs) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_Translate(Vector3 position, bool adjust = true, bool lostController = true)
	{
		if (Net != null && !Net.hasAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 7, position, adjust, lostController);
			return true;
		}
		if ((base.Entity.peTrans == null || base.Entity.peTrans.position != position) && ((Net != null && Net.hasOwnerAuth) || Net == null) && m_Request != null && m_Request.Register(EReqType.Translate, position, adjust) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_Rotation(Quaternion rotation)
	{
		if ((base.Entity.peTrans == null || Quaternion.Angle(base.Entity.peTrans.rotation, rotation) > 1f) && m_Request != null && m_Request.Register(EReqType.Rotate, rotation) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_SetIdle(string name)
	{
		if (m_Request != null && m_Request.Register(EReqType.Idle, name) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_PlayAnimation(string name, float time, bool play = true)
	{
		if (Net != null && !Net.hasOwnerAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 1, name, time, play);
			return true;
		}
		if (m_Request != null && m_Request.Register(EReqType.Animation, name, time, play) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_MoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
	{
		if (Net != null && !Net.hasOwnerAuth && base.Entity != null && base.Entity.viewCmpt != null && base.Entity.viewCmpt.hasView)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 2, position, stopRadius, isForce, (int)state);
			return true;
		}
		if (Net != null && !Net.hasAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 2, position, stopRadius, isForce, (int)state);
			return true;
		}
		if (Req_GetRequest(EReqType.MoveToPoint) is RQMoveToPoint rQMoveToPoint && rQMoveToPoint.position == position && Net != null)
		{
			return false;
		}
		if (m_Request != null && m_Request.Register(EReqType.MoveToPoint, position, stopRadius, isForce, state) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_TalkMoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
	{
		if (Net != null && !Net.hasOwnerAuth && base.Entity != null && base.Entity.viewCmpt != null && base.Entity.viewCmpt.hasView)
		{
			return true;
		}
		if (Net != null && !Net.hasAuth)
		{
			return true;
		}
		if (m_Request != null && m_Request.Register(EReqType.TalkMove, position, stopRadius, isForce, state) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_FollowPath(Vector3[] path, bool isLoop, SpeedState state = SpeedState.Run, bool fromnet = false)
	{
		if (Net != null)
		{
			if (!fromnet)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 3, path, isLoop);
			}
			if (Req_GetRequest(EReqType.FollowPath) is RQFollowPath rQFollowPath && path.Length > 1)
			{
				if (!rQFollowPath.Equal(new Vector3[2]
				{
					path[0],
					path[path.Length - 1]
				}) && m_Request != null && m_Request.Register(EReqType.FollowPath, path, isLoop, state) != null)
				{
					return true;
				}
				return true;
			}
			if (m_Request != null && m_Request.Register(EReqType.FollowPath, path, isLoop, state) != null)
			{
				return true;
			}
			return false;
		}
		if (m_Request != null && m_Request.Register(EReqType.FollowPath, path, isLoop, state) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_FollowTarget(int targetId, Vector3 targetPos, int dirTargetID, float tRadius, bool bNet = false, bool send = true)
	{
		if (Net != null)
		{
			if (!bNet && send)
			{
				Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 4, targetId, targetPos, dirTargetID, tRadius);
			}
			if (Req_Contains(EReqType.FollowTarget))
			{
				return false;
			}
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(targetId);
			if (peEntity != null && m_Request != null && m_Request.Register(EReqType.FollowTarget, targetId, targetPos, dirTargetID, tRadius) != null)
			{
				return true;
			}
			return false;
		}
		PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(targetId);
		if (peEntity2 != null && m_Request != null && m_Request.Register(EReqType.FollowTarget, targetId, targetPos, dirTargetID, tRadius) != null)
		{
			return true;
		}
		return false;
	}

	public bool Req_Salvation(int id, bool carryUp)
	{
		GameUI.Instance.mShopWnd.Hide();
		NetworkInterface networkInterface = NetworkInterface.Get(id);
		if (Net != null && Net.hasOwnerAuth != networkInterface.hasOwnerAuth && Net.hasOwnerAuth)
		{
			Net.RPCServer(EPacketType.PT_NPC_RequestAiOp, 5, id, carryUp);
			return true;
		}
		if (Net != null && Net.hasOwnerAuth && networkInterface.hasOwnerAuth)
		{
			if (m_Request != null && m_Request.Register(EReqType.Salvation, id, carryUp) != null)
			{
				return true;
			}
			return false;
		}
		if (Net == null)
		{
			if (m_Request != null && m_Request.Register(EReqType.Salvation, id, carryUp) != null)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void Req_Remove(EReqType type)
	{
		if (m_Request != null)
		{
			m_Request.RemoveRequest(type);
		}
	}

	public void Req_Remove(Request request)
	{
		if (m_Request != null)
		{
			m_Request.RemoveRequest(request);
		}
	}

	public bool Req_Contains(EReqType type)
	{
		if (m_Request != null)
		{
			return m_Request.Contains(type);
		}
		return false;
	}

	public int GetFollowTargetId()
	{
		if (m_Request != null)
		{
			return m_Request.GetFollowID();
		}
		return -1;
	}

	public Request Req_GetRequest(EReqType type)
	{
		if (m_Request != null)
		{
			return m_Request.GetRequest(type);
		}
		return null;
	}

	public void AddServantDeathEvent(PeEntity servant)
	{
		if (this.OnServentDie != null)
		{
			this.OnServentDie(servant);
		}
	}

	public void AddServantReviveEvent(PeEntity servant)
	{
		if (this.OnServentRevive != null)
		{
			this.OnServentRevive(servant);
		}
	}

	private void OnDeath(SkEntity self, SkEntity caster)
	{
		State = ENpcState.Dead;
		CSMain.KickOutFromHospital(base.Entity);
		AddServentDieSign();
		if (IsServant)
		{
			Singleton<PeLogicGlobal>.Instance.ServantReviveAtuo(base.Entity, 5f);
		}
		if (!PeGameMgr.IsMultiStory)
		{
			return;
		}
		NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(base.Entity.Id);
		if (missionData == null || missionData.m_Rnpc_ID == -1)
		{
			return;
		}
		if (base.Entity.entityProto.proto == EEntityProto.Npc)
		{
			if (ReviveTime < 0)
			{
				PlayerNetwork.mainPlayer.CreateSceneItem("ash_ball", base.Entity.position, "1339,1", base.Entity.Id);
			}
		}
		else if (base.Entity.entityProto.proto == EEntityProto.RandomNpc)
		{
			RandomNpcDb.Item item = RandomNpcDb.Get(missionData.m_Rnpc_ID);
			if (item != null && item.reviveTime < 0)
			{
				PlayerNetwork.mainPlayer.CreateSceneItem("ash_ball", base.Entity.position, "1339,1", base.Entity.Id);
			}
		}
	}

	private void OnRevive(SkEntity entity)
	{
		State = ENpcState.UnKnown;
		RemoveServentSign();
		if (IsServant)
		{
			AddTalkInfo(ENpcTalkType.NpcResurgence, ENpcSpeakType.TopHead);
		}
	}

	private void OnAttack(SkEntity skEntity, float damage)
	{
		PeEntity component = skEntity.GetComponent<PeEntity>();
		if (component != null && component != base.Entity)
		{
			NpcHatreTargets.Instance.TryAddInTarget(base.Entity, component, damage);
		}
	}

	private void OnDamage(SkEntity entity, float damage)
	{
		if (!(null == Alive) && !(null == entity))
		{
			PeEntity component = entity.GetComponent<PeEntity>();
			if (!(component == base.Entity))
			{
				NpcHatreTargets.Instance.TryAddInTarget(base.Entity, component, damage, trans: true);
			}
		}
	}

	private void OnBeEnemyEnter(PeEntity attacker)
	{
		OnSkillTarget(attacker.skEntity);
		AddEnemyLocked(attacker);
	}

	private void OnBeEnemyExit(PeEntity enemyEntity)
	{
		RemoveEnemyLocked(enemyEntity);
	}

	private void OnEnemyAchieve(PeEntity enemyEntity)
	{
		if (base.Entity.aliveEntity != null && base.Entity.motionMgr != null)
		{
			base.Entity.motionMgr.EndAction(PEActionType.Sleep);
			NpcsleepBuffId = 0;
			NpcHasSleep = false;
		}
	}

	private void OnEnemyLost(PeEntity enemyEntity)
	{
	}

	private void OnSkillTarget(SkEntity caster)
	{
		if (null == Alive || null == caster)
		{
			return;
		}
		int playerID = (int)Alive.GetAttribute(91);
		PeEntity component = caster.GetComponent<PeEntity>();
		if (component == base.Entity)
		{
			return;
		}
		float radius = ((!component.IsBoss) ? 64f : 128f);
		bool flag = false;
		if (GameConfig.IsMultiClient)
		{
			if (Singleton<ForceSetting>.Instance.GetForceType(playerID) == EPlayerType.Human)
			{
				flag = true;
			}
		}
		else if (Singleton<ForceSetting>.Instance.GetForceID(playerID) == 1)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(base.Entity.peTrans.position, radius, playerID, isDeath: false, base.Entity);
		for (int i = 0; i < entities.Count; i++)
		{
			if (!(entities[i] == null) && !entities[i].Equals(base.Entity) && entities[i].target != null)
			{
				entities[i].target.OnTargetSkill(component.skEntity);
			}
		}
	}

	private void OnWeaponAttack(SkEntity caster)
	{
		OnSkillTarget(caster);
	}

	public void OnLeaderSleep(int buffid)
	{
		NpcsleepBuffId = buffid;
		sleepStartTime = Time.time;
	}

	private void NpcSleep()
	{
		if (base.Entity.aliveEntity != null && base.Entity.motionMgr != null && !NpcHasSleep && NpcsleepBuffId != 0)
		{
			PEActionParamVQNS param = PEActionParamVQNS.param;
			param.vec = base.Entity.peTrans.position;
			param.q = base.Entity.peTrans.rotation;
			param.n = NpcsleepBuffId;
			param.str = "Sleep";
			base.Entity.motionMgr.DoAction(PEActionType.Sleep, param);
			NpcHasSleep = true;
		}
	}

	public void OnLeaderEndSleep(int buffid)
	{
		if (this != null && !Equals(null) && (bool)base.gameObject && base.Entity.aliveEntity != null && base.Entity.motionMgr != null)
		{
			base.Entity.motionMgr.EndAction(PEActionType.Sleep);
			NpcsleepBuffId = 0;
			NpcHasSleep = false;
		}
	}

	private void OnStartFell(TreeInfo treeInfo)
	{
		if (IsServant)
		{
			m_FollowerCut = true;
		}
	}

	private void OnEndFell()
	{
	}

	public void RemoveSleepBuff()
	{
		if (base.Entity.aliveEntity != null && base.Entity.motionMgr != null)
		{
			base.Entity.motionMgr.EndAction(PEActionType.Sleep);
		}
	}

	public void OnBehaveStop(int behaveId)
	{
		if (!(this == null) && !Equals(null) && !(mFixedPointPos == Vector3.zero) && !(base.Entity == null) && !PeGameMgr.IsMulti && base.Entity.peTrans != null)
		{
			base.Entity.peTrans.position = mFixedPointPos;
		}
	}

	public void OnFastTravel(Vector3 pos)
	{
		if (!(this == null) && !Equals(null) && !PeGameMgr.IsMulti && (!(m_Request != null) || !m_Request.HasAnyRequest()) && (!(this != null) || (Type != ENpcType.Follower && Type != ENpcType.Base)) && !(mFixedPointPos == Vector3.zero) && !(base.Entity == null) && !PeGameMgr.IsMulti && base.Entity.peTrans != null)
		{
			base.Entity.peTrans.position = mFixedPointPos;
		}
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.Trans_Pos_set:
		{
			Vector3 pos = (Vector3)args[0];
			if (base.Entity.Id == NpcRobotDb.Instance.mFollowID)
			{
				if (m_RobotEntity == null)
				{
					PeEntityCreator.InitRobot();
				}
				if (m_RobotEntity != null)
				{
					m_RobotEntity.robotCmpt.Translate(pos);
				}
			}
			break;
		}
		case EMsg.View_Model_Build:
			if (base.Entity.Id == NpcRobotDb.Instance.mFollowID)
			{
				if (m_RobotEntity == null)
				{
					PeEntityCreator.InitRobot();
				}
				if (m_RobotEntity != null)
				{
					m_RobotEntity.robotCmpt.Translate(base.Entity.position);
				}
			}
			break;
		}
	}

	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
		m_UpdateCampsite = true;
		m_Request = GetComponent<RequestCmpt>();
		m_Behave = GetComponent<BehaveCmpt>();
		mNpcPackage = GetComponent<NpcPackageCmpt>();
		if (base.Entity != null && base.Entity.aliveEntity != null)
		{
			base.Entity.aliveEntity.deathEvent += OnDeath;
			base.Entity.aliveEntity.reviveEvent += OnRevive;
			base.Entity.aliveEntity.onHpReduce += OnDamage;
			base.Entity.aliveEntity.attackEvent += OnAttack;
			base.Entity.aliveEntity.onSkillEvent += OnSkillTarget;
			base.Entity.aliveEntity.onWeaponAttack += OnWeaponAttack;
			base.Entity.aliveEntity.OnBeEnemyEnter += OnBeEnemyEnter;
			base.Entity.aliveEntity.OnBeEnemyExit += OnBeEnemyExit;
			base.Entity.aliveEntity.OnEnemyAchieve += OnEnemyAchieve;
			base.Entity.aliveEntity.OnEnemyLost += OnEnemyLost;
		}
		if (m_Behave != null)
		{
			m_Behave.OnBehaveStop += OnBehaveStop;
		}
		PeSingleton<FastTravelMgr>.Instance.OnFastTravel += OnFastTravel;
		AttachEvent();
		StartCoroutine(IgnorePlant(10f));
		StartCoroutine(CalculateCamp());
		StartCoroutine(CheckMedicine());
		StartCoroutine(ThinkingSth());
		StartCoroutine(NpcTalk(5f));
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		updateCnt++;
		UpdateMount();
		if (!battachEvent)
		{
			AttachEvent();
		}
		UpdateAbility();
		UpdateNpcThink();
		UpdateDisWithPlayer();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (base.Entity.aliveEntity != null)
		{
			base.Entity.aliveEntity.deathEvent -= OnDeath;
		}
		if (m_RobotEntity != null)
		{
			m_RobotEntity.robotCmpt.OnDestroy();
		}
		if (PeSingleton<FastTravelMgr>.Instance != null)
		{
			PeSingleton<FastTravelMgr>.Instance.OnFastTravel -= OnFastTravel;
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(12);
		if (Master == null)
		{
			w.Write(0);
		}
		else
		{
			w.Write(Master.Entity.Id);
		}
		w.Write(m_ReviveTime);
		w.Write((int)MedicalState);
		if (m_AbilityIdes != null)
		{
			w.Write(m_AbilityIdes.Count);
			for (int i = 0; i < m_AbilityIdes.Count; i++)
			{
				w.Write(m_AbilityIdes[i]);
			}
		}
		else
		{
			w.Write(0);
		}
		w.Write(mAttributeUpTimes);
		w.Write(FixedPointPos.x);
		w.Write(FixedPointPos.y);
		w.Write(FixedPointPos.z);
		w.Write(mNpcControlCmdId);
		w.Write(m_FollowerCurReviveTime);
		w.Write((int)m_MotionStyle);
		w.Write(m_MountID);
		w.Write(m_NpcInAlert);
		w.Write(m_BaseNpcOutMission);
		w.Write(m_FollowerSentry);
		w.Write((int)m_Battle);
		w.Write(voiceType);
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		int num = r.ReadInt32();
		int entityId = r.ReadInt32();
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (peEntity != null)
		{
			ServantLeaderCmpt component = peEntity.GetComponent<ServantLeaderCmpt>();
			if (component != null)
			{
				component.AddServant(this);
			}
		}
		if (num >= 1)
		{
			m_ReviveTime = r.ReadInt32();
		}
		if (num >= 2)
		{
			MedicalState = (ENpcMedicalState)r.ReadInt32();
			int num2 = r.ReadInt32();
			if (num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					m_AbilityIdes.Add(r.ReadInt32());
				}
			}
		}
		if (num >= 3)
		{
			mAttributeUpTimes = r.ReadInt32();
		}
		if (num >= 4)
		{
			FixedPointPos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
		}
		if (num >= 5)
		{
			NpcControlCmdId = r.ReadInt32();
		}
		if (num >= 6)
		{
			FollowerCurReviveTime = r.ReadSingle();
			MotionStyle = (ENpcMotionStyle)r.ReadInt32();
		}
		if (num >= 7)
		{
			MountID = r.ReadInt32();
		}
		if (num >= 8)
		{
			m_NpcInAlert = r.ReadBoolean();
		}
		if (num >= 9)
		{
			m_BaseNpcOutMission = r.ReadBoolean();
			if (m_BaseNpcOutMission)
			{
				Invoke("MissionReady", 5f);
			}
		}
		if (num >= 10)
		{
			m_FollowerSentry = r.ReadBoolean();
		}
		if (num >= 11)
		{
			m_Battle = (ENpcBattle)r.ReadInt32();
		}
		if (num >= 12)
		{
			voiceType = r.ReadInt32();
		}
	}
}
