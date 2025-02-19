using System;
using System.Collections;
using System.Collections.Generic;
using PETools;
using RootMotion.FinalIK;
using UnityEngine;

namespace Pathea;

public class Motion_Move_Human : Motion_Move, IPeMsg
{
	public const float AnimLerpSpeed = 5f;

	private const float NetMoveMin = 0.01f;

	private const int UnSafePosCountNum = 5;

	private HumanPhyCtrl m_PhyCtrl;

	private PEPathfinder m_Pathfinder;

	private SkAliveEntity m_SkEntity;

	public LayerMask m_TerrainLayer;

	public float m_CheckForwardDis = 1.5f;

	public float m_CheckDisMin = 0.1f;

	public float m_CheckUpDis = 1f;

	public float m_CheckDisInterval = 0.3f;

	public float m_AngleAnimLerpF = 1f;

	public float m_WaterStateLerpF = 5f;

	public float m_DefaultRunSpeed = 5f;

	public float m_DefaultWalkSpeed = 2f;

	public float m_DefaultSprintSpeed = 8f;

	public float m_MaxNetDelay = 4f;

	public int m_NetSpeedDownCount = 3;

	public float m_NetSpeedUpDelay = 0.5f;

	public float m_NetMoveMinSqrDis = 0.25f;

	public float m_NetLerpSpeed = 5f;

	public float m_NetSpeedScaleF = 0.5f;

	private Vector3 m_NetMovePos = Vector3.zero;

	private bool m_NetMove;

	private float m_LastMoveTime;

	private float m_StuckSpeed = 1f;

	private Vector3 m_CurMovement = Vector3.zero;

	private Vector3 m_MoveRequest = Vector3.zero;

	private Vector3 m_CurAvoidDirection = Vector3.zero;

	public float m_FallStartTime = 0.3f;

	public float m_FallHeight = 3f;

	private float m_LastOnGroundTime;

	private float m_LastOnGroundHeight;

	private Vector3 m_MoveDestination;

	private float m_InWaterLevel;

	private bool m_HeadInWater = true;

	private AudioController m_SwimmingSound;

	private static readonly int[] SwimmingID = new int[4] { 941, 942, 951, 952 };

	public AnimationCurve m_ForwardAngleFootRotate;

	private GrounderFBBIK m_GroundIk;

	private MoveParam m_Param;

	public Action_Move m_Move;

	public Action_Sprint m_Sprint;

	public Action_Rotate m_Rotate;

	public Action_Step m_Step;

	public Action_Jump m_Jump;

	public Action_Fall m_Fall;

	public Action_Climb m_ClimbLadder;

	public Action_Drive m_Drive;

	private Action_Train m_Train;

	private Action_Halt m_Halt;

	private Action_Ride m_Ride;

	private bool m_MoveToModel;

	private bool m_Avoid;

	private Vector3 m_LastSafePos;

	private List<Vector3> m_UnSafePos = new List<Vector3>(5);

	private float changeToRunTime;

	[SerializeField]
	private float m_MinRunTime = 0.2f;

	private bool m_FirstPersonCtrl;

	private double mNetJumpTime = -1.0;

	private static readonly int layer = 2173184;

	private static readonly int layer1 = 2171136;

	public Vector3 NetMovePos
	{
		set
		{
			m_NetMovePos = value;
		}
	}

	private bool firstPersonCtrl
	{
		get
		{
			return m_FirstPersonCtrl;
		}
		set
		{
			m_FirstPersonCtrl = value;
			UpdateMoveSubState();
		}
	}

	private bool isMainPlayer => PeSingleton<MainPlayer>.Instance.entity == base.Entity;

	public bool autoRotate => m_Move.m_AutoRotate;

	public override SpeedState speed
	{
		set
		{
			if (value != m_SpeedState)
			{
				m_SpeedState = value;
				if (m_SpeedState == SpeedState.None)
				{
					m_SpeedState = SpeedState.Walk;
				}
				switch (m_SpeedState)
				{
				case SpeedState.Walk:
					m_Move.SetWalkState(walk: true);
					break;
				case SpeedState.Run:
					m_Move.SetWalkState(walk: false);
					break;
				}
			}
		}
	}

	public override MoveStyle baseMoveStyle
	{
		set
		{
			if (style == base.baseMoveStyle)
			{
				style = value;
			}
			base.baseMoveStyle = value;
		}
	}

	public override MoveStyle style
	{
		get
		{
			return m_Style;
		}
		set
		{
			m_Style = value;
			UpdateMoveSubState();
			MoveSpeed moveSpeed = null;
			if (null != m_Param)
			{
				moveSpeed = m_Param.m_MoveSpeedList.Find((MoveSpeed itr) => itr.m_Style == m_Style);
			}
			if (moveSpeed != null)
			{
				m_Move.runSpeed = moveSpeed.m_RunSpeed;
				m_Move.walkSpeed = moveSpeed.m_WalkSpeed;
				m_Sprint.m_MoveSpeed = moveSpeed.m_SprintSpeed;
			}
			else
			{
				m_Move.runSpeed = m_DefaultRunSpeed;
				m_Move.walkSpeed = m_DefaultWalkSpeed;
				m_Sprint.m_MoveSpeed = m_DefaultSprintSpeed;
			}
		}
	}

	public override MoveMode mode
	{
		set
		{
			m_Mode = value;
			UpdateMoveSubState();
		}
	}

	public override Vector3 velocity
	{
		get
		{
			if (null != m_PhyCtrl)
			{
				return m_PhyCtrl.velocity;
			}
			return base.velocity;
		}
		set
		{
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.velocity = value;
			}
		}
	}

	public override Vector3 movement => m_MoveRequest;

	public override float gravity => Mathf.Abs(Physics.gravity.y);

	public override bool grounded => m_PhyCtrl != null && m_PhyCtrl.grounded;

	public override void Start()
	{
		base.Start();
		m_SkEntity = base.Entity.aliveEntity;
		m_NetMove = false;
		m_MoveToModel = false;
		m_Avoid = false;
		mode = MoveMode.ForwardOnly;
		style = MoveStyle.Normal;
		baseMoveStyle = style;
		state = MovementState.Air;
		speed = SpeedState.Run;
		base.Entity.motionMgr.AddAction(m_Move);
		m_Sprint.m_UseStamina = isMainPlayer;
		base.Entity.motionMgr.AddAction(m_Sprint);
		base.Entity.animCmpt.AnimEvtString += m_Rotate.AnimEvent;
		base.Entity.motionMgr.AddAction(m_Rotate);
		m_Step.m_UseStamina = isMainPlayer;
		base.Entity.motionMgr.AddAction(m_Step);
		base.Entity.motionMgr.AddAction(m_Jump);
		base.Entity.motionMgr.AddAction(m_Fall);
		base.Entity.motionMgr.AddAction(m_ClimbLadder);
		m_Drive.skillTreeMgr = base.Entity.GetCmpt<SkillTreeUnitMgr>();
		base.Entity.motionMgr.AddAction(m_Drive);
		m_Train = new Action_Train();
		base.Entity.motionMgr.AddAction(m_Train);
		m_Halt = new Action_Halt();
		base.Entity.motionMgr.AddAction(m_Halt);
		m_Ride = new Action_Ride();
		base.Entity.motionMgr.AddAction(m_Ride);
		m_LastOnGroundTime = Time.time;
		m_LastOnGroundHeight = base.Entity.position.y;
	}

	public override void OnUpdate()
	{
		UpdateLocation();
		UpdateAnimState();
		UpdatePathfinding();
		CheckFall();
		UpdateStuck();
		UpdateNetMove();
		UpdateContrlerPhy();
		UpdateSafePlace();
	}

	public override void Move(Vector3 dir, SpeedState state = SpeedState.Walk)
	{
		m_MoveToModel = false;
		if (state == SpeedState.Retreat)
		{
			state = SpeedState.Run;
		}
		MoveDir(dir, state, rotateImmediately: false);
	}

	private void MoveDir(Vector3 dir, SpeedState state, bool rotateImmediately)
	{
		if (state == SpeedState.Sprint)
		{
			if (Vector3.Angle(dir, base.Entity.forward) > 90f || Vector3.Project(dir, base.Entity.forward).sqrMagnitude < 0.01f)
			{
				state = SpeedState.Run;
				changeToRunTime = Time.time;
			}
			else if (base.Entity.motionMgr.IsActionRunning(PEActionType.Sprint))
			{
				if (base.Entity.GetAttribute(AttribType.Stamina) <= 0f)
				{
					state = SpeedState.Run;
				}
			}
			else
			{
				PEActionParamNVB param = PEActionParamNVB.param;
				param.n = 0;
				param.vec = dir;
				param.b = rotateImmediately;
				if (!base.Entity.motionMgr.CanDoAction(PEActionType.Sprint, param))
				{
					state = SpeedState.Run;
				}
			}
			if (Time.time - changeToRunTime < m_MinRunTime)
			{
				state = SpeedState.Run;
			}
		}
		speed = state;
		m_MoveRequest = dir;
		switch (speed)
		{
		case SpeedState.Sprint:
			if (dir != Vector3.zero)
			{
				PEActionParamNVB param4 = PEActionParamNVB.param;
				param4.n = 0;
				param4.vec = dir;
				param4.b = rotateImmediately;
				base.Entity.motionMgr.DoAction(PEActionType.Sprint, param4);
			}
			else
			{
				base.Entity.motionMgr.EndAction(PEActionType.Move);
				base.Entity.motionMgr.EndAction(PEActionType.Sprint);
			}
			break;
		case SpeedState.Walk:
			m_Move.SetWalkState(walk: true);
			if (dir != Vector3.zero)
			{
				PEActionParamNV param3 = PEActionParamNV.param;
				param3.n = 0;
				param3.vec = dir;
				base.Entity.motionMgr.DoAction(PEActionType.Move, param3);
			}
			else
			{
				base.Entity.motionMgr.EndAction(PEActionType.Move);
				base.Entity.motionMgr.EndAction(PEActionType.Sprint);
			}
			break;
		case SpeedState.Run:
			m_Move.SetWalkState(walk: false);
			if (dir != Vector3.zero)
			{
				PEActionParamNV param2 = PEActionParamNV.param;
				param2.n = 0;
				param2.vec = dir;
				base.Entity.motionMgr.DoAction(PEActionType.Move, param2);
			}
			else
			{
				base.Entity.motionMgr.EndAction(PEActionType.Move);
				base.Entity.motionMgr.EndAction(PEActionType.Sprint);
			}
			break;
		}
	}

	public override void SetSpeed(float Speed)
	{
	}

	public void UpdateMoveDir(Vector3 moveDirWorld, Vector3 moveDirLocal)
	{
		m_ClimbLadder.SetMoveDir(moveDirLocal.z, checkLadder: true);
		m_Jump.SetMoveDir(moveDirWorld);
		m_Fall.SetMoveDir(moveDirWorld);
	}

	private void UpdateMoveSubState()
	{
		switch (m_Style)
		{
		case MoveStyle.Rifle:
		case MoveStyle.HandGun:
		case MoveStyle.Bow:
		case MoveStyle.Shotgun:
		case MoveStyle.Carry:
		case MoveStyle.BeCarry:
		case MoveStyle.Grenade:
		case MoveStyle.Drill:
			m_Sprint.m_ApplyStopIK = (m_Move.m_ApplyStopIK = false);
			m_Jump.m_AutoRotate = (m_Move.m_AutoRotate = false);
			return;
		}
		if (m_Style == MoveStyle.Abnormal && base.Entity.motionMgr.IsActionRunning(PEActionType.Handed))
		{
			m_Sprint.m_ApplyStopIK = (m_Move.m_ApplyStopIK = false);
			m_Jump.m_AutoRotate = (m_Move.m_AutoRotate = false);
		}
		else
		{
			m_Sprint.m_ApplyStopIK = (m_Move.m_ApplyStopIK = !firstPersonCtrl);
			m_Jump.m_AutoRotate = (m_Move.m_AutoRotate = !firstPersonCtrl);
		}
	}

	public override void MoveTo(Vector3 targetPos, SpeedState state = SpeedState.Walk, bool avoid = true)
	{
		m_Avoid = avoid;
		m_MoveToModel = true;
		m_MoveDestination = targetPos;
		speed = state;
	}

	public override void NetMoveTo(Vector3 position, Vector3 moveVelocity, bool immediately = false)
	{
		m_LastSafePos = position + 0.5f * Vector3.up;
		if (!immediately && !m_SkEntity.IsController())
		{
			if (base.Entity.peTrans == null)
			{
				return;
			}
			if (!m_NetMove)
			{
				ResetNetMoveState(netMove: true);
			}
			float num = Vector3.Distance(m_NetMovePos, position);
			if (num < 0.01f)
			{
				return;
			}
			m_NetMovePos = position;
			if (null != base.Entity.viewCmpt && !base.Entity.viewCmpt.hasView)
			{
				base.Entity.position = position;
				SceneMan.SetDirty(base.Entity.lodCmpt);
			}
		}
		else
		{
			m_NetMovePos = position;
			base.Entity.position = position;
			SceneMan.SetDirty(base.Entity.lodCmpt);
			Stop();
			for (int i = 0; i < mNetTransInfos.Count; i++)
			{
				RecycleNetTranInfo(mNetTransInfos[i]);
			}
			mNetTransInfos.Clear();
		}
		NetTranInfo netTransInfo = GetNetTransInfo();
		netTransInfo.pos = position;
		netTransInfo.rot = base.Entity.rotation.eulerAngles;
		netTransInfo.speed = speed;
		netTransInfo.contrllerTime = GameTime.Timer.Second;
		mNetTransInfos.Add(netTransInfo);
	}

	public void NetJump(double time)
	{
		mNetJumpTime = time;
	}

	public override void NetRotateTo(Vector3 eulerAngle)
	{
		NetTranInfo netTransInfo = GetNetTransInfo();
		netTransInfo.pos = base.Entity.position;
		netTransInfo.rot = eulerAngle;
		netTransInfo.speed = speed;
		netTransInfo.contrllerTime = GameTime.Timer.Second;
		mNetTransInfos.Add(netTransInfo);
	}

	public override void RotateTo(Vector3 targetDir)
	{
		if (!(base.Entity.motionMgr == null))
		{
			if (state != MovementState.Water && !base.Entity.motionMgr.IsActionRunning(PEActionType.Glider))
			{
				targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up);
			}
			if (base.Entity.motionMgr.IsActionRunning(PEActionType.Move))
			{
				m_Move.SetLookDir(targetDir);
				return;
			}
			if (base.Entity.motionMgr.IsActionRunning(PEActionType.Jump))
			{
				m_Jump.SetLookDir(targetDir);
				return;
			}
			PEActionParamVBB param = PEActionParamVBB.param;
			param.vec = targetDir;
			param.b1 = false;
			param.b2 = true;
			base.Entity.motionMgr.DoAction(PEActionType.Rotate, param);
		}
	}

	public override void Jump()
	{
		base.Entity.motionMgr.DoAction(PEActionType.Jump);
	}

	public override void Dodge(Vector3 dir)
	{
		PEActionParamV param = PEActionParamV.param;
		param.vec = dir;
		base.Entity.motionMgr.DoAction(PEActionType.Step, param);
	}

	public override void ApplyForce(Vector3 power, ForceMode mode)
	{
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.GetComponent<Rigidbody>().AddForce(power, mode);
		}
	}

	public override bool Stucking(float time)
	{
		return Time.time - m_LastMoveTime > time || (base.Entity.viewCmpt != null && !base.Entity.viewCmpt.hasView);
	}

	public override void Stop()
	{
		base.Stop();
		m_MoveDestination = Vector3.zero;
		if (null != base.Entity.motionMgr)
		{
			base.Entity.motionMgr.EndImmediately(PEActionType.Move);
			base.Entity.motionMgr.EndImmediately(PEActionType.Sprint);
		}
	}

	private void UpdateLocation()
	{
		if (!(null != m_PhyCtrl))
		{
			return;
		}
		if (m_PhyCtrl.feetInWater)
		{
			state = MovementState.Water;
		}
		else if (m_PhyCtrl.enabled && m_PhyCtrl.grounded)
		{
			state = MovementState.Ground;
		}
		else
		{
			state = MovementState.Air;
		}
		if (base.Entity.motionMgr.GetMaskState(PEActionMask.Ground) != m_PhyCtrl.grounded)
		{
			base.Entity.motionMgr.SetMaskState(PEActionMask.Ground, m_PhyCtrl.grounded);
		}
		if (base.Entity.motionMgr.GetMaskState(PEActionMask.InWater) != m_PhyCtrl.spineInWater)
		{
			base.Entity.motionMgr.SetMaskState(PEActionMask.InWater, m_PhyCtrl.spineInWater);
		}
		if (!m_NetMove)
		{
			if (base.Entity.motionMgr.GetMaskState(PEActionMask.InAir) != (state == MovementState.Air))
			{
				base.Entity.motionMgr.SetMaskState(PEActionMask.InAir, state == MovementState.Air);
			}
		}
		else
		{
			base.Entity.motionMgr.SetMaskState(PEActionMask.InAir, state: false);
		}
		bool flag = m_PhyCtrl.headInWater && (null == base.Entity.passengerCmpt || !base.Entity.passengerCmpt.IsOnCarrier());
		if (m_HeadInWater != flag)
		{
			m_HeadInWater = flag;
			base.Entity.SendMsg(EMsg.state_Water, m_HeadInWater);
		}
		if (m_PhyCtrl.spineInWater)
		{
			base.Entity.motionMgr.EndImmediately(PEActionType.AimEquipHold);
			base.Entity.motionMgr.EndImmediately(PEActionType.BowHold);
		}
		if (!base.Entity.motionMgr.IsActionRunning(PEActionType.Move) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Sprint) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Glider) && !base.Entity.motionMgr.isInAimState)
		{
			base.Entity.peTrans.rotation = Quaternion.Lerp(base.Entity.peTrans.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(base.Entity.forward, Vector3.up), Vector3.up), m_Param.m_MoveRotateSpeed * Time.deltaTime);
		}
		if (m_PhyCtrl.spineInWater && !m_PhyCtrl.headInWater && (base.Entity.motionMgr.IsActionRunning(PEActionType.Move) || base.Entity.motionMgr.IsActionRunning(PEActionType.Sprint)))
		{
			if (null != m_SwimmingSound && !m_SwimmingSound.isPlaying)
			{
				m_SwimmingSound.Delete();
				m_SwimmingSound = null;
			}
			if (null == m_SwimmingSound)
			{
				m_SwimmingSound = AudioManager.instance.Create(base.Entity.position, SwimmingID[UnityEngine.Random.Range(0, SwimmingID.Length)], base.Entity.tr, isPlay: false, isDelete: false);
				m_SwimmingSound.PlayAudio(0.3f);
			}
		}
		else if (null != m_SwimmingSound && m_SwimmingSound.isPlaying)
		{
			m_SwimmingSound.StopAudio(0.5f);
			m_SwimmingSound = null;
		}
	}

	private void CheckFall()
	{
		if (null != m_PhyCtrl && !m_NetMove)
		{
			if (m_PhyCtrl.grounded || base.Entity.motionMgr.IsActionRunning(PEActionType.Drive) || base.Entity.motionMgr.IsActionRunning(PEActionType.GetOnTrain) || base.Entity.motionMgr.IsActionRunning(PEActionType.Glider))
			{
				m_LastOnGroundTime = Time.time;
				m_LastOnGroundHeight = base.Entity.position.y;
			}
			else if ((Time.time - m_LastOnGroundTime > m_FallStartTime || m_LastOnGroundHeight - base.Entity.position.y < 0f - m_FallHeight) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Fall))
			{
				base.Entity.motionMgr.DoAction(PEActionType.Fall);
			}
		}
	}

	private void UpdateStuck()
	{
		if (m_MoveRequest == Vector3.zero || velocity.sqrMagnitude > m_StuckSpeed * m_StuckSpeed || base.Entity.isRagdoll)
		{
			m_LastMoveTime = Time.time;
		}
	}

	private void UpdateNetMove()
	{
		if (base.Entity.motionMgr.IsActionRunning(PEActionType.GetOnTrain) || base.Entity.motionMgr.IsActionRunning(PEActionType.Drive) || base.Entity.HasMount || !m_NetMove)
		{
			return;
		}
		if (!base.Entity.viewCmpt.hasView)
		{
			if (mNetTransInfos.Count > 0)
			{
				m_NetMovePos = mNetTransInfos[mNetTransInfos.Count - 1].pos;
			}
			if (mNetTransInfos.Count > 1)
			{
				for (int num = mNetTransInfos.Count - 2; num >= 0; num--)
				{
					mNetTransInfos.RemoveAt(num);
				}
			}
			if (Vector3.SqrMagnitude(base.Entity.peTrans.position - m_NetMovePos) > 0.01f)
			{
				if (null != base.Entity.netCmpt && base.Entity.netCmpt.IsPlayer)
				{
					SceneMan.SetDirty(base.Entity.lodCmpt);
				}
				base.Entity.peTrans.position = m_NetMovePos;
			}
			return;
		}
		if (Vector3.zero == m_NetMovePos)
		{
			m_NetMovePos = base.Entity.position;
		}
		if (mNetTransInfos.Count > 0 && Vector3.Distance(base.Entity.position, mNetTransInfos[0].pos) > 32f)
		{
			base.Entity.position = mNetTransInfos[0].pos;
		}
		if (mNetTransInfos.Count > 1)
		{
			int num2 = 0;
			int num3 = 0;
			while (num3 < mNetTransInfos.Count - 1)
			{
				if (!(GameTime.Timer.Second > mNetTransInfos[num3].contrllerTime + (double)(m_MaxNetDelay * GameTime.Timer.ElapseSpeed)))
				{
					Vector3 vector = mNetTransInfos[num3 + 1].pos - mNetTransInfos[num3].pos;
					if (!(vector.sqrMagnitude < float.Epsilon) && !(Vector3.SqrMagnitude(mNetTransInfos[num3 + 1].pos - base.Entity.position) < m_NetMoveMinSqrDis))
					{
						Vector3 vector2 = base.Entity.position - mNetTransInfos[num3].pos;
						Vector3 from = Vector3.Project(vector2, vector);
						if (from.sqrMagnitude < float.Epsilon || Vector3.Angle(from, vector) > 90f || from.sqrMagnitude < vector.sqrMagnitude)
						{
							break;
						}
					}
				}
				num3++;
				num2++;
			}
			for (int i = 0; i < num2; i++)
			{
				RecycleNetTranInfo(mNetTransInfos[0]);
				mNetTransInfos.RemoveAt(0);
			}
			if (mNetTransInfos.Count > 1)
			{
				if (mNetTransInfos.Count > 2)
				{
					m_NetMovePos = 0.5f * (mNetTransInfos[1].pos + mNetTransInfos[2].pos);
				}
				else
				{
					m_NetMovePos = mNetTransInfos[1].pos;
				}
				for (int j = 0; j < mNetTransInfos.Count - 1; j++)
				{
					Debug.DrawLine(mNetTransInfos[j].pos, mNetTransInfos[j + 1].pos, Color.yellow);
					Debug.DrawLine(mNetTransInfos[j].pos, mNetTransInfos[j].pos + 0.1f * Vector3.up, Color.green);
				}
				Debug.DrawLine(m_NetMovePos + 0.5f * Vector3.left, m_NetMovePos - 0.5f * Vector3.left, Color.red);
				Debug.DrawLine(m_NetMovePos + 0.5f * Vector3.up, m_NetMovePos - 0.5f * Vector3.up, Color.red);
			}
			else if (mNetTransInfos.Count > 0)
			{
				m_NetMovePos = mNetTransInfos[0].pos;
			}
			speed = mNetTransInfos[0].speed;
			if (mNetJumpTime > 0.0 && mNetJumpTime <= mNetTransInfos[0].contrllerTime)
			{
				if (null != m_PhyCtrl)
				{
					m_PhyCtrl.ApplyImpact(Vector3.up);
				}
				if (null != base.Entity && null != base.Entity.animCmpt)
				{
					base.Entity.animCmpt.SetTrigger("Jump");
				}
				mNetJumpTime = -1.0;
			}
			SceneMan.SetDirty(base.Entity.lodCmpt);
		}
		else if (mNetTransInfos.Count > 0)
		{
			m_NetMovePos = mNetTransInfos[0].pos;
		}
		if (null != m_PhyCtrl)
		{
			double num4 = GameTime.Timer.Second - ((mNetTransInfos.Count <= 1) ? GameTime.Timer.Second : mNetTransInfos[1].contrllerTime);
			if (GameTime.Timer.ElapseSpeed > float.Epsilon)
			{
				num4 /= (double)GameTime.Timer.ElapseSpeed;
			}
			if (mNetTransInfos.Count <= m_NetSpeedDownCount)
			{
				m_PhyCtrl.netMoveSpeedScale = Mathf.Lerp(m_PhyCtrl.netMoveSpeedScale, 1f - m_NetSpeedScaleF, m_NetLerpSpeed * Time.deltaTime);
			}
			else if (num4 <= (double)m_NetSpeedUpDelay)
			{
				m_PhyCtrl.netMoveSpeedScale = Mathf.Lerp(m_PhyCtrl.netMoveSpeedScale, 1f, m_NetLerpSpeed * Time.deltaTime);
			}
			else
			{
				m_PhyCtrl.netMoveSpeedScale = Mathf.Lerp(m_PhyCtrl.netMoveSpeedScale, 1f + m_NetSpeedScaleF, m_NetLerpSpeed * Time.deltaTime);
			}
		}
		if (null != base.Entity.biologyViewCmpt && base.Entity.biologyViewCmpt.IsRagdoll)
		{
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.desiredMovementDirection = Vector3.zero;
				m_PhyCtrl.CancelMoveRequest();
			}
			base.Entity.motionMgr.EndImmediately(PEActionType.Move);
			base.Entity.motionMgr.EndImmediately(PEActionType.Sprint);
			return;
		}
		if (base.Entity.motionMgr.IsActionRunning(PEActionType.Climb))
		{
			float num5 = m_NetMovePos.y - base.Entity.position.y;
			if (Mathf.Abs(num5) > 0.5f)
			{
				m_ClimbLadder.SetMoveDir(num5);
			}
			else
			{
				m_ClimbLadder.SetMoveDir(0f);
			}
			if (mNetTransInfos.Count > 0)
			{
				base.Entity.rotation = Quaternion.LookRotation(Quaternion.Euler(mNetTransInfos[0].rot) * Vector3.forward);
			}
			return;
		}
		if (base.Entity.motionMgr.IsActionRunning(PEActionType.Glider) || base.Entity.motionMgr.IsActionRunning(PEActionType.Parachute) || base.Entity.motionMgr.IsActionRunning(PEActionType.Fall) || base.Entity.motionMgr.IsActionRunning(PEActionType.JetPack) || base.Entity.motionMgr.IsActionRunning(PEActionType.Sit) || base.Entity.motionMgr.IsActionRunning(PEActionType.Repulsed) || base.Entity.motionMgr.IsActionRunning(PEActionType.SwordAttack))
		{
			base.Entity.position = Vector3.Lerp(base.Entity.position, mNetTransInfos[(mNetTransInfos.Count > 1) ? 1 : 0].pos, m_NetLerpSpeed * Time.deltaTime);
			if (mNetTransInfos.Count > 0)
			{
				base.Entity.rotation = Quaternion.LookRotation(Quaternion.Euler(mNetTransInfos[0].rot) * Vector3.forward);
			}
			return;
		}
		if (mNetTransInfos.Count > 0)
		{
			RotateTo(Quaternion.Euler(mNetTransInfos[0].rot) * Vector3.forward);
		}
		switch (speed)
		{
		case SpeedState.Walk:
		case SpeedState.Run:
		{
			PEActionParamNV param2 = PEActionParamNV.param;
			param2.n = 1;
			param2.vec = m_NetMovePos;
			base.Entity.motionMgr.DoAction(PEActionType.Move, param2);
			break;
		}
		case SpeedState.Sprint:
		{
			PEActionParamNVB param = PEActionParamNVB.param;
			param.n = 1;
			param.vec = m_NetMovePos;
			param.b = true;
			base.Entity.motionMgr.DoAction(PEActionType.Sprint, param);
			break;
		}
		}
	}

	private Vector3 needAvoid()
	{
		float num = 0f;
		if (speed == SpeedState.Walk)
		{
			num = m_DefaultWalkSpeed;
		}
		else if (speed == SpeedState.Run)
		{
			num = m_DefaultRunSpeed;
		}
		else if (speed == SpeedState.Sprint)
		{
			num = m_DefaultSprintSpeed;
		}
		Vector3 zero = Vector3.zero;
		float num2 = Mathf.Max(1.5f, num * Time.deltaTime * 5f);
		Collider[] array = Physics.OverlapSphere(base.Entity.peTrans.position, base.Entity.peTrans.radius + num2, layer);
		foreach (Collider collider in array)
		{
			if (!collider.transform.IsChildOf(base.transform) && (!(base.Entity.target != null) || base.Entity.target.GetAttackEnemy() == null || !(base.Entity.target.GetAttackEnemy().trans != null) || !collider.transform.IsChildOf(base.Entity.target.GetAttackEnemy().trans)) && (!(base.Entity.target != null) || !(base.Entity.target.Treat != null) || !collider.transform.IsChildOf(base.Entity.target.Treat.transform)))
			{
				zero += Vector3.ProjectOnPlane(base.Entity.peTrans.position - collider.transform.position, Vector3.up).normalized;
			}
		}
		return zero;
	}

	private void HumanCalculateAvoid(Vector3 movement)
	{
		Vector3 zero = Vector3.zero;
		bool flag = true;
		if (base.Entity.NpcCmpt != null)
		{
			Collider[] array = Physics.OverlapSphere(m_MoveDestination, 5f, layer);
			flag = array == null || array.Length <= 0;
		}
		if (!flag || !m_Avoid || movement == Vector3.zero)
		{
			m_CurAvoidDirection = Vector3.zero;
			return;
		}
		float num = m_DefaultRunSpeed * Time.deltaTime * 5f;
		Vector3 position = base.Entity.position;
		Vector3 point = base.Entity.position + base.Entity.bounds.size.y * Vector3.up;
		float radius = base.Entity.bounds.extents.x + 0.1f;
		float maxDistance = base.Entity.bounds.extents.z + num;
		int layermask = ((base.Entity.proto != EEntityProto.Monster) ? layer : layer1);
		Vector3 zero2 = Vector3.zero;
		RaycastHit[] array2 = Physics.CapsuleCastAll(position, point, radius, movement, maxDistance, layermask);
		if (array2 == null)
		{
			return;
		}
		for (int i = 0; i < array2.Length; i++)
		{
			Collider collider = array2[i].collider;
			if (null == collider || collider.transform.IsChildOf(base.transform))
			{
				continue;
			}
			PeEntity componentInParent = collider.GetComponentInParent<PeEntity>();
			if (componentInParent != null && m_MoveDestination != Vector3.zero)
			{
				Bounds bounds = new Bounds(componentInParent.position, componentInParent.bounds.size);
				Vector3 moveDestination = m_MoveDestination;
				moveDestination.y = bounds.center.y;
				if (bounds.Contains(moveDestination))
				{
					continue;
				}
			}
			if (!(base.Entity.target != null) || base.Entity.target.GetAttackEnemy() == null || !(base.Entity.target.GetAttackEnemy().trans != null) || !collider.transform.IsChildOf(base.Entity.target.GetAttackEnemy().trans))
			{
				zero2 = base.Entity.position - collider.transform.position;
				zero2.y = 0f;
				zero += zero2.normalized;
			}
		}
		if (zero != Vector3.zero)
		{
			if (m_CurAvoidDirection == Vector3.zero)
			{
				m_CurAvoidDirection = zero.normalized;
			}
			else
			{
				m_CurAvoidDirection = Util.ConstantSlerp(m_CurAvoidDirection, zero, Time.deltaTime * 120f);
			}
		}
		else if (m_CurAvoidDirection != Vector3.zero)
		{
			m_CurAvoidDirection = Util.ConstantSlerp(m_CurAvoidDirection, movement, Time.deltaTime * 120f);
			if (Vector3.Angle(m_CurAvoidDirection, movement) < 5f)
			{
				m_CurAvoidDirection = Vector3.zero;
			}
		}
	}

	private void UpdatePathfinding()
	{
		m_MoveRequest = Vector3.zero;
		if (!base.Entity.viewCmpt.hasView || m_NetMove)
		{
			m_MoveDestination = Vector3.zero;
		}
		else
		{
			if (!m_MoveToModel)
			{
				return;
			}
			Vector3 vector = m_MoveDestination;
			if (vector != Vector3.zero && m_PhyCtrl != null)
			{
				if (state == MovementState.Ground)
				{
					if (PEUtil.SqrMagnitudeH(m_PhyCtrl.transform.position, vector) < 0.0625f)
					{
						vector = Vector3.zero;
					}
				}
				else if (PEUtil.SqrMagnitude(m_PhyCtrl.transform.position, vector) < 4f)
				{
					vector = Vector3.zero;
				}
			}
			if (m_Pathfinder != null)
			{
				m_Pathfinder.SetTargetposition(vector);
			}
			if (vector != Vector3.zero)
			{
				Vector3 moveRequest = vector - base.Entity.position;
				if (m_Param == null || moveRequest.sqrMagnitude >= MoveParam.AutoMoveStopSqrDis * MoveParam.AutoMoveStopSqrDis)
				{
					if (m_Pathfinder != null && AstarPath.active != null && state != MovementState.Water)
					{
						m_MoveRequest = m_Pathfinder.CalculateVelocity(base.Entity.position);
					}
					if (m_MoveRequest == Vector3.zero)
					{
						m_MoveRequest = moveRequest;
					}
				}
				if (m_MoveRequest != Vector3.zero)
				{
					if (Vector3.Angle(m_MoveRequest, Vector3.up) < 30f)
					{
						m_MoveRequest = Vector3.Slerp(Vector3.up, base.Entity.forward, 0.333f);
					}
					if (state != MovementState.Water && !base.Entity.motionMgr.IsActionRunning(PEActionType.Glider))
					{
						m_MoveRequest = Vector3.ProjectOnPlane(m_MoveRequest, Vector3.up);
					}
					HumanCalculateAvoid(m_MoveRequest);
					m_MoveRequest = m_MoveRequest.normalized + m_CurAvoidDirection.normalized;
					if (m_MoveRequest == Vector3.zero)
					{
						m_CurMovement = Vector3.zero;
					}
					else if (m_CurMovement == Vector3.zero)
					{
						m_CurMovement = base.Entity.forward;
					}
					else
					{
						m_CurMovement = Util.ConstantSlerp(m_CurMovement, m_MoveRequest, 180f * Time.deltaTime);
					}
					Debug.DrawRay(base.Entity.position + Vector3.up, m_MoveRequest * 10f, Color.blue);
					Debug.DrawRay(base.Entity.position + Vector3.up, m_CurMovement * 10f, Color.red);
					Debug.DrawLine(base.Entity.position, vector, Color.yellow);
					MoveDir(m_MoveRequest, speed, rotateImmediately: true);
				}
				else
				{
					m_CurAvoidDirection = Vector3.zero;
					base.Entity.motionMgr.EndImmediately(PEActionType.Sprint);
					base.Entity.motionMgr.EndImmediately(PEActionType.Move);
				}
			}
			else
			{
				m_CurAvoidDirection = Vector3.zero;
				base.Entity.motionMgr.EndImmediately(PEActionType.Sprint);
				base.Entity.motionMgr.EndImmediately(PEActionType.Move);
			}
		}
	}

	private void UpdateAnimState()
	{
		if (!(null == base.Entity.animCmpt) && !(null == base.Entity.animCmpt.animator) && base.Entity.hasView)
		{
			Animator animator = base.Entity.animCmpt.animator;
			animator.SetFloat("FirstPerson", (!firstPersonCtrl) ? 0f : 1f);
			animator.SetBool("OnGround", (!(null != m_PhyCtrl)) ? (state == MovementState.Ground) : m_PhyCtrl.grounded);
			animator.SetFloat("InWater", Mathf.Lerp(animator.GetFloat("InWater"), (!(null != m_PhyCtrl) || !m_PhyCtrl.spineInWater) ? 0f : 1f, m_WaterStateLerpF * Time.deltaTime));
			float num = (float)Math.PI / 180f * (float)style * 20f;
			Vector3 zero = Vector3.zero;
			zero.x = animator.GetFloat("MoveStyleH");
			zero.y = animator.GetFloat("MoveStyleV");
			Vector3 zero2 = Vector3.zero;
			if (num > 0f)
			{
				zero2.x = Mathf.Cos(num);
				zero2.y = Mathf.Sin(num);
			}
			zero2 = Vector3.Lerp(zero, zero2, 5f * Time.deltaTime);
			animator.SetFloat("MoveStyleH", zero2.x);
			animator.SetFloat("MoveStyleV", zero2.y);
			animator.SetFloat("MoveStyle", Mathf.Lerp(animator.GetFloat("MoveStyle"), (float)style, 5f * Time.deltaTime));
			float num2 = ((!(null != m_PhyCtrl)) ? 0f : m_PhyCtrl.forwardGroundAngle);
			if (null != m_GroundIk)
			{
				m_GroundIk.solver.maxFootRotationAngle = m_ForwardAngleFootRotate.Evaluate(num2);
			}
			animator.SetFloat("ForwardAngle", Mathf.Lerp(animator.GetFloat("ForwardAngle"), num2, m_AngleAnimLerpF * Time.deltaTime));
			if (null != m_PhyCtrl && null != base.Entity.peTrans && !base.Entity.motionMgr.IsActionRunning(PEActionType.Move) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Sprint) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Repulsed) && !base.Entity.motionMgr.IsActionRunning(PEActionType.JetPack) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Jump) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Parachute) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Fall) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Glider) && !base.Entity.motionMgr.IsActionRunning(PEActionType.Step) && !base.Entity.motionMgr.IsActionRunning(PEActionType.SwordAttack))
			{
				Vector3 vector = Quaternion.Inverse(base.Entity.peTrans.rotation) * m_PhyCtrl.currentDesiredMovementDirection;
				animator.SetFloat("ForwardSpeed", Mathf.Lerp(animator.GetFloat("ForwardSpeed"), vector.z, (!base.Entity.motionMgr.isInAimState) ? (5f * Time.deltaTime) : 1f));
				animator.SetFloat("RightSpeed", Mathf.Lerp(animator.GetFloat("RightSpeed"), vector.x, (!base.Entity.motionMgr.isInAimState) ? (5f * Time.deltaTime) : 1f));
			}
		}
	}

	private void UpdateContrlerPhy()
	{
		if (PeGameMgr.IsMulti && null != m_PhyCtrl && null != m_SkEntity)
		{
			m_PhyCtrl.gravity = ((!m_SkEntity.IsController()) ? 0f : Physics.gravity.y);
			m_PhyCtrl.m_IsContrler = m_SkEntity.IsController();
		}
	}

	private void UpdateSafePlace()
	{
		if (!base.Entity.viewCmpt.hasView || (PeGameMgr.IsMulti && (!m_SkEntity.IsController() || (PlayerNetwork.mainPlayer._curSceneId != 0 && PlayerNetwork.mainPlayer._curSceneId != -1))) || (base.Entity.position - m_LastSafePos).sqrMagnitude < 1f)
		{
			return;
		}
		if (CheckPosSafe(base.Entity.position))
		{
			m_LastSafePos = base.Entity.position + 0.5f * Vector3.up;
			m_UnSafePos.Clear();
			return;
		}
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (CheckPosSafe(base.Entity.position + new Vector3(i, 0f, j)))
				{
					return;
				}
			}
		}
		for (int k = 0; k < m_UnSafePos.Count; k++)
		{
			if ((base.Entity.position - m_UnSafePos[k]).sqrMagnitude < 1f)
			{
				return;
			}
		}
		if (m_UnSafePos.Count < 5)
		{
			m_UnSafePos.Add(base.Entity.position);
			return;
		}
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.velocity = Vector3.zero;
		}
		base.Entity.position = m_LastSafePos;
		if (m_LastSafePos.y < 1500f)
		{
			m_LastSafePos.y += 1f;
		}
		m_UnSafePos.Clear();
	}

	private bool CheckPosSafe(Vector3 pos)
	{
		if (PEUtil.CheckPositionUnderWater(pos))
		{
			return true;
		}
		if (null != VFVoxelTerrain.self)
		{
			return VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 0.5f), Mathf.RoundToInt(pos.z)).Volume < 128 && VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 1.5f), Mathf.RoundToInt(pos.z)).Volume < 128;
		}
		return false;
	}

	public void ResetNetMoveState(bool netMove)
	{
		m_NetMove = netMove;
		if (netMove)
		{
			if (null != base.Entity.motionMgr)
			{
				base.Entity.motionMgr.FreezePhyByNet(v: true);
				base.Entity.motionMgr.EndImmediately(PEActionType.Move);
				base.Entity.motionMgr.EndImmediately(PEActionType.Sprint);
			}
			m_Move.m_AutoRotate = false;
			m_Move.m_ApplyStopIK = false;
			m_Sprint.m_ApplyStopIK = false;
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.desiredMovementDirection = Vector3.zero;
				m_PhyCtrl.CancelMoveRequest();
			}
			if (null != base.Entity.netCmpt && null != base.Entity.netCmpt.transform)
			{
				for (int i = 0; i < mNetTransInfos.Count; i++)
				{
					RecycleNetTranInfo(mNetTransInfos[i]);
				}
				mNetTransInfos.Clear();
				NetTranInfo netTransInfo = GetNetTransInfo();
				netTransInfo.pos = base.Entity.netCmpt.network.transform.position;
				netTransInfo.rot = base.Entity.netCmpt.network.transform.eulerAngles;
				netTransInfo.speed = speed;
				netTransInfo.contrllerTime = GameTime.Timer.Second;
				mNetTransInfos.Add(netTransInfo);
			}
		}
		else
		{
			if (null != base.Entity.motionMgr)
			{
				base.Entity.motionMgr.FreezePhyByNet(v: false);
			}
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.netMoveSpeedScale = 1f;
			}
			UpdateMoveSubState();
		}
	}

	private IEnumerator UpdateSafePos()
	{
		if (PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != 0 && PlayerNetwork.mainPlayer._curSceneId != -1)
		{
			yield return new WaitForSeconds(1f);
		}
		int count = 0;
		while (null == base.Entity.viewCmpt || null == base.Entity.peTrans || null == base.Entity.motionMgr)
		{
			yield return new WaitForSeconds(1f);
		}
		while (base.Entity.viewCmpt.hasView && count < 10)
		{
			if (PE.FindHumanSafePos(base.Entity.position + (float)count * 10f * Vector3.up, out var safePos))
			{
				base.Entity.position = safePos;
				base.Entity.motionMgr.FreezePhySteateForSystem(v: false);
				break;
			}
			base.Entity.motionMgr.FreezePhySteateForSystem(v: true);
			yield return new WaitForSeconds(1f);
			count++;
		}
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
		{
			BiologyViewRoot biologyViewRoot = args[1] as BiologyViewRoot;
			m_Pathfinder = biologyViewRoot.pathFinder;
			m_PhyCtrl = biologyViewRoot.humanPhyCtrl;
			m_Drive.ikDrive = biologyViewRoot.ikDrive;
			m_Halt.animEffect = biologyViewRoot.ikAnimEffectCtrl;
			m_Jump.fBBIK = biologyViewRoot.fbbik;
			m_GroundIk = biologyViewRoot.grounderFBBIK;
			m_Param = biologyViewRoot.moveParam;
			m_LastSafePos = base.Entity.peTrans.position;
			if (null != m_PhyCtrl)
			{
				m_PhyCtrl.gravity = Physics.gravity.y;
				m_PhyCtrl.m_IsContrler = true;
				m_Move.phyCtrl = m_PhyCtrl;
				m_Sprint.phyCtrl = m_PhyCtrl;
				m_Rotate.phyMotor = m_PhyCtrl;
				m_Jump.phyMotor = m_PhyCtrl;
				m_Step.phyMotor = m_PhyCtrl;
				m_Fall.phyMotor = m_PhyCtrl;
				m_Halt.phyMotor = m_PhyCtrl;
				m_ClimbLadder.m_PhyCtrl = m_PhyCtrl;
			}
			m_Move.m_Param = m_Param;
			m_Sprint.m_Param = m_Param;
			break;
		}
		case EMsg.Net_Controller:
			if (null != base.Entity.peTrans && m_NetMove && m_NetMovePos != Vector3.zero)
			{
				base.Entity.position = m_NetMovePos;
				SceneMan.SetDirty(base.Entity.lodCmpt);
			}
			ResetNetMoveState(netMove: false);
			break;
		case EMsg.Net_Proxy:
			ResetNetMoveState(netMove: true);
			break;
		case EMsg.View_FirstPerson:
			firstPersonCtrl = (bool)args[0];
			break;
		}
	}

	public void SetIsKinematic(bool value)
	{
		m_PhyCtrl._rigidbody.isKinematic = value;
	}
}
