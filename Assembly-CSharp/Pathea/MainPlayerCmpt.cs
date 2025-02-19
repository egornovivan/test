using System;
using System.Collections;
using System.Collections.Generic;
using CustomData;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea;

public class MainPlayerCmpt : PeCmpt, IPeMsg
{
	private const float DeathDropRange = 10f;

	public const int StartInvincibleSkillID = 30100750;

	public const int RemoveInvincibleSkillID = 30100751;

	public static MainPlayerCmpt gMainPlayer;

	[HideInInspector]
	public Transform _camTarget;

	[HideInInspector]
	public Transform _bneckModel;

	[HideInInspector]
	public Transform _bneckRagdoll;

	private MotionMgrCmpt mMotionMgr;

	private Motion_Move_Human mMove;

	private Motion_Equip mEquip;

	private BiologyViewCmpt mView;

	private PeTrans mTrans;

	private IKCmpt mIK;

	private SkAliveEntity m_Skill;

	private PackageCmpt mPackage;

	private PassengerCmpt mPassenger;

	private HumanPhyCtrl m_PhyCtrl;

	private AbnormalConditionCmpt m_Abnormalcmpt;

	public LayerMask m_ShootLayer;

	public float m_ShootMinDis = 10f;

	private float m_DefautShootDis = 100f;

	public float m_DiveMinY = -0.2f;

	private bool m_InShootMode;

	private bool m_AutoRun;

	public bool m_MouseMoveMode;

	public float waterUpSpeed = 3f;

	public float waterJumpHeight = 1.5f;

	private bool m_MoveWalk;

	[Range(0f, 1f)]
	public float subTelescopeDamp = 0.8f;

	private bool m_ShowSubTelescope;

	private Vector3 m_SubTelescopePos;

	[Header("FallDamage")]
	public int MoveRecordCount = 10;

	public int CurrentSpeedFramCount = 4;

	public float FallDamageSpeedThreshold = 15f;

	public float FallDamageSpeedToDamage = 15f;

	private List<Vector3> m_MoveState;

	private List<Vector3> m_MoveRequest;

	private int MoveRequestCount = 5;

	private Vector3 m_MoveDir = Vector3.zero;

	private Vector3 m_MouseHitPos = Vector3.zero;

	[HideInInspector]
	public bool m_ActionEnable = true;

	private bool m_DisableActionByUI;

	public float m_FadeTime = 0.1f;

	private bool m_FirstPersonCtrl;

	[Header("MouseOperation")]
	private Action_Gather m_ActionGather;

	private Action_Fell m_Fell;

	public MouseOpMgr.MouseOpCursor actionOpCursor;

	private float m_UpdateMouseStateInterval = 0.2f;

	private float m_UpdateMouseStateTime;

	private PEAbnormalNotice[] m_AbnormalNotices;

	private Quaternion _cameraRotation;

	private Action_Hand m_Hand;

	public bool AutoRun => m_AutoRun;

	public static bool isCameraRollable
	{
		get
		{
			if (null != gMainPlayer)
			{
				return gMainPlayer.mMotionMgr.IsActionRunning(PEActionType.Glider);
			}
			return false;
		}
	}

	public bool firstPersonCtrl
	{
		get
		{
			return m_FirstPersonCtrl;
		}
		set
		{
			m_FirstPersonCtrl = value;
			PeCamera.is1stPerson = m_FirstPersonCtrl;
			base.Entity.SendMsg(EMsg.View_FirstPerson, m_FirstPersonCtrl, GetType());
		}
	}

	private Action_Gather actionGather
	{
		get
		{
			if (m_ActionGather == null)
			{
				m_ActionGather = mMotionMgr.GetAction<Action_Gather>();
			}
			return m_ActionGather;
		}
	}

	private Action_Fell actionFell
	{
		get
		{
			if (m_Fell == null)
			{
				m_Fell = mMotionMgr.GetAction<Action_Fell>();
			}
			if (m_Fell != null && null != m_Fell.m_Axe)
			{
				return m_Fell;
			}
			return null;
		}
	}

	public event Action<int> onEquipmentAttack;

	public event Action<bool> onBuildMode;

	public event Action onDurabilityDeficiency;

	public override void Start()
	{
		base.Start();
		m_MoveState = new List<Vector3>(MoveRecordCount);
		m_MoveRequest = new List<Vector3>(MoveRequestCount);
		gMainPlayer = this;
		base.gameObject.AddComponent<Scanner>();
		mMove = base.Entity.GetCmpt<Motion_Move_Human>();
		mEquip = base.Entity.motionEquipment;
		mView = base.Entity.biologyViewCmpt;
		mTrans = base.Entity.peTrans;
		mIK = base.Entity.GetCmpt<IKCmpt>();
		m_Skill = base.Entity.aliveEntity;
		mPackage = base.Entity.GetCmpt<PackageCmpt>();
		mPassenger = base.Entity.passengerCmpt;
		mMotionMgr = base.Entity.motionMgr;
		mMotionMgr.onActionStart += OnActionStart;
		mMotionMgr.onActionEnd += OnActionEnd;
		m_Abnormalcmpt = base.Entity.Alnormal;
		if (null != m_Abnormalcmpt)
		{
			m_Abnormalcmpt.evtStart += OnStartAbnormal;
			m_Abnormalcmpt.evtEnd += OnEndAbnormal;
		}
		if (null != m_Skill)
		{
			m_Skill.onHpReduce += OnDamage;
			m_Skill.attackEvent += OnAttack;
			m_Skill.deathEvent += OnDeath;
			m_Skill.onSkillEvent += OnSkillTarget;
			m_Skill.onWeaponAttack += OnWeaponAttack;
			m_Skill.OnBeEnemyEnter += OnBeEnemyEnter;
		}
		if (!PeGameMgr.IsTutorial)
		{
			StartCoroutine(UpdateAbnormalNotice());
		}
		Invoke("CheckAbnormalState", 5f);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		UpdateMouseOperation();
		UpdateInputState();
		UpdateUIState();
		UpdateImpactDamage();
		UpdateFirstPersonState();
		UpdateAnimatorSpeed();
	}

	private void UpdateAnimatorSpeed()
	{
	}

	private void InitReputationSystem()
	{
		if (!GameConfig.IsMultiMode && m_Skill != null)
		{
			PeSingleton<ReputationSystem>.Instance.AddPlayerID((int)m_Skill.GetAttribute(AttribType.DefaultPlayerID));
			if (PeGameMgr.IsAdventure)
			{
				PeSingleton<ReputationSystem>.Instance.ActiveReputation((int)m_Skill.GetAttribute(AttribType.DefaultPlayerID));
			}
		}
	}

	private void UpdateMoveState()
	{
		if (PeCamera.isFreeLook || (!PeGameMgr.IsMulti && PeGameMgr.gamePause))
		{
			mMove.Move(Vector3.zero);
			return;
		}
		m_MoveDir = Vector3.zero;
		Vector3 zero = Vector3.zero;
		zero = PeInput.GetAxisH() * Vector3.right + PeInput.GetAxisV() * Vector3.forward;
		if (m_AutoRun && zero == Vector3.zero && (!UIStateMgr.Instance.isTalking || GameUI.Instance.mNPCTalk.type != 0))
		{
			zero += Vector3.forward;
		}
		if (PeInput.Get(PeInput.LogicFunction.SwitchWalkRun) && !base.Entity.passengerCmpt.IsOnCarrier())
		{
			m_MoveWalk = !m_MoveWalk;
		}
		float magnitude = zero.magnitude;
		bool flag = magnitude > 0.1f && magnitude < 0.9f;
		if (PeInput.Get(PeInput.LogicFunction.AutoRunOnOff))
		{
			m_AutoRun = !m_AutoRun;
		}
		if (PeInput.GetAxisV() < -0.5f || base.Entity.passengerCmpt.IsOnCarrier())
		{
			m_AutoRun = false;
		}
		Vector3 zero2 = Vector3.zero;
		if (PeInput.Get(PeInput.LogicFunction.DodgeForward))
		{
			zero2 += Vector3.forward;
		}
		if (PeInput.Get(PeInput.LogicFunction.DodgeRight))
		{
			zero2 += Vector3.right;
		}
		if (PeInput.Get(PeInput.LogicFunction.DodgeBackward))
		{
			zero2 += Vector3.back;
		}
		if (PeInput.Get(PeInput.LogicFunction.DodgeLeft))
		{
			zero2 += Vector3.left;
		}
		if (!PeInput.Get(PeInput.LogicFunction.LiberatiePerspective))
		{
			_cameraRotation = PEUtil.MainCamTransform.rotation;
		}
		if (zero2 != Vector3.zero && (PeGameMgr.IsMulti || !PeGameMgr.gamePause))
		{
			zero2 = Vector3.ProjectOnPlane(_cameraRotation * zero2, Vector3.up).normalized;
			mMove.Dodge(zero2);
		}
		if (mMove.state == MovementState.Water)
		{
			m_MoveDir = _cameraRotation * zero;
		}
		else
		{
			m_MoveDir = Vector3.ProjectOnPlane(_cameraRotation * zero, Vector3.up);
		}
		if (null != m_PhyCtrl && m_PhyCtrl.spineInWater)
		{
			if (!m_PhyCtrl.headInWater)
			{
				if (m_MoveDir.y < 0f && m_MoveDir.y > m_DiveMinY)
				{
					m_MoveDir.y = 0f;
				}
				if (PeInput.Get(PeInput.LogicFunction.Jump) && !mView.IsRagdoll && !mMotionMgr.IsActionRunning(PEActionType.Dig) && !mMotionMgr.IsActionRunning(PEActionType.Gather))
				{
					m_PhyCtrl.ApplyImpact(Mathf.Sqrt(20f * waterJumpHeight) * Vector3.up);
				}
			}
			if (PeInput.Get(PeInput.LogicFunction.SwimUp) && !mView.IsRagdoll && !mMotionMgr.IsActionRunning(PEActionType.Dig) && !mMotionMgr.IsActionRunning(PEActionType.Gather))
			{
				m_PhyCtrl.ApplyMoveRequest(waterUpSpeed * Vector3.up);
			}
		}
		if (!m_MouseMoveMode)
		{
			if (mMove.autoRotate)
			{
				if (m_MoveRequest.Count == MoveRequestCount)
				{
					m_MoveRequest.RemoveAt(0);
				}
				for (int i = 0; i < m_MoveRequest.Count; i++)
				{
					if (Vector3.Angle(m_MoveRequest[i], zero) > 150f)
					{
						PEActionParamVBB param = PEActionParamVBB.param;
						param.vec = m_MoveDir.normalized;
						param.b1 = true;
						param.b2 = false;
						if (mMotionMgr.DoAction(PEActionType.Rotate, param))
						{
							m_MoveRequest.Clear();
						}
						break;
					}
				}
				m_MoveRequest.Add(zero);
			}
			if (mMotionMgr.IsActionRunning(PEActionType.Hand))
			{
				if (m_Hand == null)
				{
					m_Hand = mMotionMgr.GetAction<Action_Hand>();
				}
				if (m_Hand.moveable)
				{
					mMove.Move(m_MoveDir.normalized);
				}
			}
			else
			{
				SpeedState state = SpeedState.Run;
				if (PeInput.Get(PeInput.LogicFunction.Sprint))
				{
					state = SpeedState.Sprint;
				}
				else if (m_MoveWalk || flag)
				{
					state = SpeedState.Walk;
				}
				mMove.Move(m_MoveDir.normalized, state);
			}
			mMove.UpdateMoveDir(m_MoveDir, zero);
		}
		mEquip.UpdateMoveDir(m_MoveDir, zero);
		if (PeInput.Get(PeInput.LogicFunction.Jump))
		{
			mMove.Jump();
		}
	}

	private void UpdateAimTarget()
	{
		Ray ray = PeCamera.mouseRay;
		if (null != TestPEEntityCamCtrl.Instance)
		{
			Camera cam = TestPEEntityCamCtrl.Instance.GetCam();
			if (null != cam)
			{
				ray = cam.ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f));
			}
		}
		if (mMotionMgr.isInAimState)
		{
			ray = PeCamera.cursorRay;
		}
		RaycastHit[] array = Physics.RaycastAll(ray, m_DefautShootDis, m_ShootLayer.value, QueryTriggerInteraction.Ignore);
		float num = m_DefautShootDis;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].distance > 0f && array[i].distance < num && !array[i].collider.transform.IsChildOf(base.transform))
			{
				num = array[i].distance;
			}
		}
		num = Mathf.Clamp(num, m_ShootMinDis, m_DefautShootDis);
		m_MouseHitPos = ray.origin + ray.direction * num;
		if (!mMotionMgr.IsActionRunning(PEActionType.SwordAttack) && !mMotionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
		{
			mIK.aimTargetPos = m_MouseHitPos;
		}
		if (mMotionMgr.isInAimState && Vector3.zero != mIK.aimRay.direction)
		{
			float num2 = Vector3.Distance(m_MouseHitPos, mIK.aimRay.origin);
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(mIK.aimRay, out hitInfo, num2, m_ShootLayer.value, QueryTriggerInteraction.Ignore) && num2 > hitInfo.distance + 0.1f && !mMotionMgr.IsActionRunning(PEActionType.GunReload) && !mMotionMgr.IsActionRunning(PEActionType.BowReload);
			if (m_ShowSubTelescope != flag)
			{
				m_ShowSubTelescope = flag;
				UISightingTelescope.Instance.EnableOrthoAimPoint(m_ShowSubTelescope);
			}
			if (m_ShowSubTelescope && !mMotionMgr.IsActionRunning(PEActionType.GunFire) && !mMotionMgr.IsActionRunning(PEActionType.BowShoot))
			{
				Vector3 a = Camera.main.WorldToScreenPoint(hitInfo.point);
				a = (m_SubTelescopePos = Vector3.Lerp(a, m_SubTelescopePos, subTelescopeDamp));
				a.x = Mathf.RoundToInt(a.x);
				a.y = Mathf.RoundToInt(a.y);
				a.z = Mathf.RoundToInt(a.z);
				UISightingTelescope.Instance.SetOrthoAimPointPos(a);
			}
		}
	}

	private void UpdateOtherAction()
	{
		if (PeCamera.isFreeLook || !m_ActionEnable)
		{
			return;
		}
		if (PeInput.Get(PeInput.LogicFunction.Jet))
		{
			mMotionMgr.DoAction(PEActionType.JetPack);
		}
		else
		{
			mMotionMgr.EndImmediately(PEActionType.JetPack);
		}
		if (PeInput.Get(PeInput.LogicFunction.ClimbForwardLadderOnOff))
		{
			DragItemMousePickLadder dragItemMousePickLadder = PeSingleton<MousePicker>.Instance.curPickObj as DragItemMousePickLadder;
			if (null != dragItemMousePickLadder)
			{
				dragItemMousePickLadder.TryClimbLadder(this);
			}
		}
		mEquip.HoldSheild(PeInput.Get(PeInput.LogicFunction.Block));
		if (PeInput.Get(PeInput.LogicFunction.DrawWeapon))
		{
			if (m_DisableActionByUI && mEquip.ISAimWeapon)
			{
				m_DisableActionByUI = false;
			}
			mEquip.ActiveWeapon(active: true);
		}
		if (PeInput.Get(PeInput.LogicFunction.Attack))
		{
			if (SystemSettingData.Instance.AttackWhithMouseDir)
			{
				Vector3 mouseClickDir = GetMouseClickDir();
				mEquip.SwordAttack(mouseClickDir);
				mEquip.TwoHandWeaponAttack(mouseClickDir);
			}
			else
			{
				mEquip.SwordAttack(m_MoveDir.normalized);
				mEquip.TwoHandWeaponAttack(m_MoveDir.normalized);
			}
		}
		if (PeInput.Get(PeInput.LogicFunction.SheatheWeapon))
		{
			mEquip.ActiveWeapon(active: false);
		}
		if (PeInput.Get(PeInput.LogicFunction.GatherHerb))
		{
			mMotionMgr.DoAction(PEActionType.Gather);
		}
		if (PeInput.Get(PeInput.LogicFunction.DrawWater))
		{
			mMotionMgr.DoAction(PEActionType.Draw);
		}
		if (PeInput.Get(PeInput.LogicFunction.TakeForwardVehicleOnOff) && null != mPassenger)
		{
			if (mPassenger.IsOnVCCarrier)
			{
				mPassenger.GetOffCarrier();
			}
			else if (PeSingleton<MousePicker>.Instance.curPickObj != null)
			{
				DragItemMousePickCarrier dragItemMousePickCarrier = PeSingleton<MousePicker>.Instance.curPickObj as DragItemMousePickCarrier;
				if (null != dragItemMousePickCarrier)
				{
					CarrierController component = dragItemMousePickCarrier.GetComponent<CarrierController>();
					if (null != component)
					{
						int num = component.FindEmptySeatIndex();
						if (num > -2)
						{
							if (GameConfig.IsMultiMode)
							{
								PEActionParamDrive param = PEActionParamDrive.param;
								param.controller = component;
								param.seatIndex = num;
								if (mMotionMgr.CanDoAction(PEActionType.Drive, param))
								{
									CreationSkEntity component2 = component.GetComponent<CreationSkEntity>();
									if (component2 != null && component2._net != null)
									{
										if (!Singleton<ForceSetting>.Instance.Conflict(component2._net.TeamId, PlayerNetwork.mainPlayerId))
										{
											PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_GetOnVehicle, component2._net.Id);
										}
										else
										{
											new PeTipMsg(PELocalization.GetString(82209000), PeTipMsg.EMsgLevel.Warning);
										}
									}
								}
							}
							else
							{
								mPassenger.GetOn(component, num, checkState: true);
							}
						}
					}
				}
			}
		}
		if (PeInput.Get(PeInput.LogicFunction.Cut))
		{
			mMotionMgr.DoAction(PEActionType.Fell);
		}
		else
		{
			mMotionMgr.EndAction(PEActionType.Fell);
		}
		if (PeInput.Get(PeInput.LogicFunction.EndShooting))
		{
			mMotionMgr.EndAction(PEActionType.GunFire);
			mMotionMgr.EndAction(PEActionType.Pump);
		}
		else if (PeInput.Get(PeInput.LogicFunction.BegShooting))
		{
			PEActionParamB param2 = PEActionParamB.param;
			param2.b = false;
			mMotionMgr.DoAction(PEActionType.GunFire, param2);
			mMotionMgr.DoAction(PEActionType.BowShoot);
			mMotionMgr.DoAction(PEActionType.Throw);
			mMotionMgr.DoAction(PEActionType.Pump);
			mMotionMgr.DoAction(PEActionType.RopeGunShoot);
		}
		if (m_MouseMoveMode)
		{
			if (PeInput.Get(PeInput.LogicFunction.BegShooting))
			{
				mMove.MoveTo(m_MouseHitPos, SpeedState.Sprint);
			}
			if (Input.GetMouseButtonDown(1))
			{
				mMove.MoveTo(Vector3.zero, SpeedState.Sprint);
			}
		}
		if (PeInput.Get(PeInput.LogicFunction.EndDigging))
		{
			mMotionMgr.EndAction(PEActionType.Dig);
		}
		else if (PeInput.Get(PeInput.LogicFunction.BegDigging))
		{
			PEActionParamV param3 = PEActionParamV.param;
			param3.vec = Vector3.zero;
			mMotionMgr.DoAction(PEActionType.Dig, param3);
		}
		if (PeInput.Get(PeInput.LogicFunction.Reload))
		{
			mEquip.Reload();
		}
		if (PeInput.Get(PeInput.LogicFunction.BuildMode))
		{
			if (mMotionMgr.IsActionRunning(PEActionType.Build))
			{
				mMotionMgr.EndAction(PEActionType.Build);
			}
			else if (RandomDungenMgrData.InDungeon)
			{
				new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
			}
			else if (SingleGameStory.curType == SingleGameStory.StoryScene.MainLand || SingleGameStory.curType == SingleGameStory.StoryScene.TrainingShip)
			{
				mMotionMgr.DoAction(PEActionType.Build);
			}
			else
			{
				new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
			}
		}
	}

	private void UpdateInputState()
	{
		UpdateAimTarget();
		UpdateMoveState();
		UpdateOtherAction();
	}

	private void UpdateUIState()
	{
		if (m_InShootMode && null != UISightingTelescope.Instance)
		{
			UISightingTelescope.Instance.Scale = mEquip.GetAimPointScale();
		}
	}

	private void UpdateImpactDamage()
	{
		if ((!PeGameMgr.IsMulti && PeGameMgr.gamePause) || !(null != m_PhyCtrl))
		{
			return;
		}
		if (base.Entity.IsDeath() || mView.IsRagdoll || mMotionMgr.freezePhyState || mMotionMgr.IsActionRunning(PEActionType.Step) || mMotionMgr.IsActionRunning(PEActionType.RopeGunShoot) || base.Entity.passengerCmpt.IsOnCarrier())
		{
			m_MoveState.Clear();
			return;
		}
		if (m_MoveState.Count >= MoveRecordCount)
		{
			m_MoveState.RemoveAt(0);
		}
		m_MoveState.Add(m_PhyCtrl.velocity);
		if (m_MoveState.Count >= MoveRecordCount)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < MoveRecordCount - CurrentSpeedFramCount; i++)
			{
				zero += m_MoveState[i];
			}
			zero /= (float)(MoveRecordCount - CurrentSpeedFramCount);
			Vector3 zero2 = Vector3.zero;
			for (int j = 0; j < CurrentSpeedFramCount; j++)
			{
				zero2 += m_MoveState[MoveRecordCount - 1 - j];
			}
			zero2 /= (float)CurrentSpeedFramCount;
			float num = Vector3.Distance(zero2, zero);
			if (mMotionMgr.IsActionRunning(PEActionType.SwordAttack) || mMotionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
			{
				num = Mathf.Abs(zero2.y - zero.y);
			}
			if (num > FallDamageSpeedThreshold)
			{
				float attribute = base.Entity.GetAttribute(AttribType.Hp);
				attribute = Mathf.Clamp(attribute - (num - FallDamageSpeedThreshold) * FallDamageSpeedToDamage, 0f, attribute);
				base.Entity.SetAttribute(AttribType.Hp, attribute, offEvent: false);
				m_MoveState.Clear();
			}
		}
	}

	private void UpdateFirstPersonState()
	{
		bool flag = SystemSettingData.Instance.FirstPersonCtrl && !mMotionMgr.IsActionRunning(PEActionType.Build) && !PeCamera.isFreeLook && !mMotionMgr.IsActionRunning(PEActionType.Drive);
		if (firstPersonCtrl != flag)
		{
			firstPersonCtrl = flag;
		}
	}

	private void UpdateMouseOperation()
	{
		if (mMotionMgr.isInAimState)
		{
			actionOpCursor = MouseOpMgr.MouseOpCursor.Null;
		}
		else if (!(m_UpdateMouseStateTime > Time.time))
		{
			actionOpCursor = MouseOpMgr.MouseOpCursor.Null;
			m_UpdateMouseStateTime += m_UpdateMouseStateInterval;
			if (actionGather != null && actionGather.UpdateOPTreeInfo())
			{
				actionOpCursor = MouseOpMgr.MouseOpCursor.Gather;
			}
			if (actionFell != null && actionFell.UpdateOPTreeInfo() && actionOpCursor == MouseOpMgr.MouseOpCursor.Null && mMotionMgr.IsActionRunning(PEActionType.EquipmentHold))
			{
				actionOpCursor = MouseOpMgr.MouseOpCursor.Fell;
			}
		}
	}

	private Vector3 GetMouseClickDir()
	{
		Ray mouseRay = PeCamera.mouseRay;
		if (new Plane(mTrans.existent.up, mTrans.position + mTrans.existent.up).Raycast(mouseRay, out var enter))
		{
			return (mouseRay.GetPoint(enter) - (mTrans.position + mTrans.existent.up)).normalized;
		}
		return mTrans.existent.forward;
	}

	public void UpdateCamDirection(Vector3 camForward)
	{
		if (firstPersonCtrl || (m_InShootMode && mMotionMgr.isInAimState))
		{
			if (null != m_PhyCtrl && m_PhyCtrl.spineInWater)
			{
				mTrans.rotation = Quaternion.LookRotation(camForward, Vector3.up);
			}
			else
			{
				mTrans.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(camForward, Vector3.up).normalized, Vector3.up);
			}
		}
	}

	private void ResetFirstCtrl()
	{
		if (m_FirstPersonCtrl)
		{
			firstPersonCtrl = m_FirstPersonCtrl;
		}
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
		{
			BiologyViewCmpt biologyViewCmpt = args[0] as BiologyViewCmpt;
			m_PhyCtrl = biologyViewCmpt.monoPhyCtrl;
			if (biologyViewCmpt.monoModelCtrlr != null)
			{
				_camTarget = biologyViewCmpt.monoModelCtrlr.transform.Find("CamTarget");
			}
			if (biologyViewCmpt.monoModelCtrlr != null)
			{
				_bneckModel = biologyViewCmpt.monoModelCtrlr.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck");
			}
			if (biologyViewCmpt.monoRagdollCtrlr != null)
			{
				_bneckRagdoll = biologyViewCmpt.monoRagdollCtrlr.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck");
			}
			if (null != UILoadScenceEffect.Instance && UILoadScenceEffect.Instance.isInProgress)
			{
				StartInvincible();
			}
			InitReputationSystem();
			Invoke("ResetFirstCtrl", 1f);
			break;
		}
		case EMsg.Camera_ChangeMode:
			PeCamera.cameraModeData = args[0] as CameraModeData;
			break;
		case EMsg.Battle_EnterShootMode:
			m_InShootMode = true;
			if (null != TestPEEntityCamCtrl.Instance && null != mView.modelTrans)
			{
				TestPEEntityCamCtrl.Instance.SetCamMode(PEUtil.GetChild(mView.modelTrans, "CamTarget"), PEUtil.GetChild(mView.modelTrans, "Bip01 Neck"), "3rd Person Shoot");
			}
			if (null != UISightingTelescope.Instance)
			{
				UISightingTelescope.Instance.Show((UISightingTelescope.SightingType)(int)args[0]);
			}
			SystemSettingData.Instance.holdGun = true;
			break;
		case EMsg.Battle_ExitShootMode:
			m_InShootMode = false;
			if (null != TestPEEntityCamCtrl.Instance)
			{
				TestPEEntityCamCtrl.Instance.SetCamMode(PEUtil.GetChild(mView.modelTrans, "CamTarget"), PEUtil.GetChild(mView.modelTrans, "Bip01 Neck"), "Normal Mode F1");
			}
			if (null != UISightingTelescope.Instance)
			{
				UISightingTelescope.Instance.ExitShootMode();
			}
			SystemSettingData.Instance.holdGun = false;
			break;
		case EMsg.Battle_OnShoot:
			if (null != UISightingTelescope.Instance)
			{
				UISightingTelescope.Instance.OnShoot();
			}
			break;
		case EMsg.UI_ShowChange:
			m_DisableActionByUI = (bool)args[0];
			if (m_DisableActionByUI && mEquip.ISAimWeapon)
			{
				mEquip.ActiveWeapon(active: false);
			}
			break;
		case EMsg.Build_BuildMode:
		{
			bool flag = (bool)args[0];
			if (flag)
			{
				GameUIMode.Instance.GotoBuildMode();
			}
			else
			{
				GameUIMode.Instance.GotoBaseMode();
			}
			PeCamera.SetVar("Build Mode", flag);
			if (this.onBuildMode != null)
			{
				this.onBuildMode(flag);
			}
			break;
		}
		case EMsg.Battle_OnAttack:
			if (this.onEquipmentAttack != null)
			{
				this.onEquipmentAttack((int)args[2]);
			}
			Singleton<PeEventGlobal>.Instance.MainPlayerAttack.Invoke(base.Entity, (AttackMode)args[0]);
			break;
		case EMsg.Action_DurabilityDeficiency:
			if (this.onDurabilityDeficiency != null)
			{
				this.onDurabilityDeficiency();
			}
			break;
		}
	}

	private void OnDeath(SkEntity self, SkEntity caster)
	{
		ApplyDeathDropItem();
	}

	public void TransferHared(PeEntity targetentity, float damage)
	{
		float radius = ((!targetentity.IsBoss) ? 64f : 128f);
		int playerID = (int)base.Entity.GetAttribute(AttribType.DefaultPlayerID);
		List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(base.Entity.position, radius, playerID, isDeath: false, base.Entity);
		for (int i = 0; i < entities.Count; i++)
		{
			if (!entities[i].Equals(targetentity) && entities[i].target != null)
			{
				entities[i].target.TransferHatred(targetentity, damage);
			}
		}
	}

	private void OnAttack(SkEntity skEntity, float damage)
	{
		PeEntity component = skEntity.GetComponent<PeEntity>();
		TransferHared(component, damage);
	}

	private void OnDamage(SkEntity entity, float damage)
	{
		if (!(null == m_Skill) && !(null == entity))
		{
			PeEntity component = entity.GetComponent<PeEntity>();
			if (!(component == base.Entity))
			{
				TransferHared(component, damage);
			}
		}
	}

	private void OnSkillTarget(SkEntity caster)
	{
		if (null == m_Skill || null == caster)
		{
			return;
		}
		int playerID = (int)m_Skill.GetAttribute(91);
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
		List<PeEntity> entities = PeSingleton<EntityMgr>.Instance.GetEntities(mTrans.position, radius, playerID, isDeath: false, base.Entity);
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

	private void OnBeEnemyEnter(PeEntity attacker)
	{
		OnSkillTarget(attacker.skEntity);
	}

	private void ApplyDeathDropItem()
	{
		if (!GameConfig.IsMultiMode || null == mPackage)
		{
			return;
		}
		int num = 6;
		MapObj[] array = new MapObj[num];
		int num2 = 0;
		int num3 = 100;
		while (num2 < num)
		{
			Vector3 vector = new Vector3(UnityEngine.Random.Range(-10f, 10f), 5f, UnityEngine.Random.Range(-10f, 10f));
			if (Physics.Raycast(mTrans.position + vector, Vector3.down, out var hitInfo, 10f, 71680, QueryTriggerInteraction.Ignore) && hitInfo.distance < 10f)
			{
				array[num2] = new MapObj();
				array[num2].pos = hitInfo.point;
				array[num2].objID = 0;
				num2++;
			}
			if (num3-- <= 0)
			{
				return;
			}
		}
		PlayerNetwork.mainPlayer.CreateMapObj(2, array);
	}

	private void OnStartAbnormal(PEAbnormalType type)
	{
		if (null != UIMainMidCtrl.Instance)
		{
			AbnormalData data = AbnormalData.GetData(type);
			if (data != null && data.iconName != "0")
			{
				UIMainMidCtrl.Instance.AddBuffShow(data.iconName, data.description);
			}
		}
	}

	private void OnEndAbnormal(PEAbnormalType type)
	{
		if (null != UIMainMidCtrl.Instance)
		{
			AbnormalData data = AbnormalData.GetData(type);
			if (data != null && data.iconName != "0")
			{
				UIMainMidCtrl.Instance.DeleteBuffShow(data.iconName);
			}
		}
	}

	private void OnActionStart(PEActionType actionType)
	{
		if (actionType == PEActionType.Fell)
		{
			PeCamera.fpCameraCanRotate = false;
		}
	}

	private void OnActionEnd(PEActionType actionType)
	{
		if (actionType == PEActionType.Fell)
		{
			PeCamera.fpCameraCanRotate = true;
		}
	}

	private IEnumerator UpdateAbnormalNotice()
	{
		yield return new WaitForSeconds(5f);
		PEAbnormalNoticeData[] datas = PEAbnormalNoticeData.datas;
		m_AbnormalNotices = new PEAbnormalNotice[datas.Length];
		for (int i = 0; i < datas.Length; i++)
		{
			m_AbnormalNotices[i] = new PEAbnormalNotice();
			m_AbnormalNotices[i].Init(base.Entity, datas[i]);
		}
		while (true)
		{
			for (int j = 0; j < datas.Length; j++)
			{
				m_AbnormalNotices[j].Update();
			}
			yield return null;
		}
	}

	public void StartInvincible()
	{
		if (null != base.Entity.skEntity)
		{
			base.Entity.skEntity.StartSkill(base.Entity.skEntity, 30100750);
			StartCoroutine(EndInvincible());
		}
	}

	private IEnumerator EndInvincible()
	{
		while (null != UILoadScenceEffect.Instance && UILoadScenceEffect.Instance.isInProgress)
		{
			yield return null;
		}
		yield return new WaitForSeconds(3f);
		if (null != base.Entity.skEntity)
		{
			base.Entity.skEntity.StartSkill(base.Entity.skEntity, 30100751);
		}
	}
}
