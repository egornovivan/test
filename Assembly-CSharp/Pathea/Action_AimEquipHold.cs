using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_AimEquipHold : Action_HandChangeEquipHold
{
	public float m_RotateSpeed = 80f;

	public float m_StopRotAngle = 10f;

	public float m_MinAngle = 20f;

	public float m_MaxAngle = 45f;

	public float m_AimPointLerpSpeed = 5f;

	protected float m_CurAccuracy;

	protected Vector3 m_Phase;

	protected Vector3 m_TargetPhase;

	protected Vector3 m_PhaseT;

	protected float m_CenterHeight;

	private bool m_StartRot;

	public override PEActionType ActionType => PEActionType.AimEquipHold;

	public PEAimAbleEquip aimAbleEquip
	{
		get
		{
			return base.handChangeEquipment as PEAimAbleEquip;
		}
		set
		{
			base.handChangeEquipment = value;
		}
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == base.anim) && !(null == aimAbleEquip))
		{
			base.DoAction(para);
			InitAimPoint();
		}
	}

	public override void OnModelBuild()
	{
		base.OnModelBuild();
		if (m_EndAction)
		{
			return;
		}
		InitAimPoint();
		if (null != base.ikCmpt)
		{
			base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: true);
		}
		if (null != aimAbleEquip)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Battle_EnterShootMode, aimAbleEquip.m_AimAttr.m_AimPointType);
		}
		if (null != base.move && null != base.handChangeEquipment)
		{
			base.move.style = base.handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
		}
		if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
		{
			base.ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
			if (!base.pauseAction && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
			{
				base.ikCmpt.m_IKAimCtrl.SetActive(active: true);
			}
			if (!m_PutOnAnimEnd)
			{
				base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
			}
		}
	}

	public override void PauseAction()
	{
		if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
		{
			base.ikCmpt.m_IKAimCtrl.SetActive(active: false);
			base.ikCmpt.m_IKAimCtrl.SetAimTran(null);
		}
	}

	public override void ContinueAction()
	{
		if (base.motionMgr.GetMaskState(m_ActionMask) && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && !m_EndAction && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
		{
			base.ikCmpt.m_IKAimCtrl.SetActive(active: true);
			base.ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
		}
	}

	public override bool Update()
	{
		UpdateAimPoint();
		UpdateActiveMask();
		FixAnimError();
		return UpdateEndAction();
	}

	protected override void OnEndAction()
	{
		base.OnEndAction();
		base.motionMgr.Entity.SendMsg(EMsg.Battle_ExitShootMode);
		if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
		{
			base.ikCmpt.m_IKAimCtrl.m_TargetPosOffset = Vector3.zero;
			base.ikCmpt.m_IKAimCtrl.SetAimTran(null);
			base.ikCmpt.m_IKAimCtrl.SetActive(active: false);
		}
	}

	public override void EndAction()
	{
		base.EndAction();
		OnEndAction();
	}

	public override void EndImmediately()
	{
		base.EndImmediately();
		OnEndAction();
		if (null != base.anim)
		{
			base.anim.SetFloat("RotationAgr", 0f);
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		base.OnAnimEvent(eventParam);
		if (!(null != aimAbleEquip) || !base.motionMgr.IsActionRunning(ActionType))
		{
			return;
		}
		switch (eventParam)
		{
		case "StartUpbodyAnim":
			if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK && aimAbleEquip.m_AimAttr.m_SyncIKWhenAnim)
			{
				base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
			}
			break;
		case "PutOnAnimEnd":
		case "EndUpbodyAnim":
			if (!base.motionMgr.IsActionRunning(PEActionType.BowReload) && !base.motionMgr.IsActionRunning(PEActionType.GunReload) && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK && aimAbleEquip.m_AimAttr.m_SyncIKWhenAnim)
			{
				base.ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
			}
			break;
		case "EnterShootMode":
			if (m_EndAction)
			{
				break;
			}
			if (null != aimAbleEquip)
			{
				base.motionMgr.Entity.SendMsg(EMsg.Battle_EnterShootMode, aimAbleEquip.m_AimAttr.m_AimPointType);
			}
			if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
			{
				base.ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
				if (!base.pauseAction && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
				{
					base.ikCmpt.m_IKAimCtrl.SetActive(active: true);
				}
				if (!m_PutOnAnimEnd)
				{
					base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
				}
			}
			break;
		}
	}

	protected override void FixAnimError()
	{
		if (null == base.handChangeEquipment)
		{
			return;
		}
		if (m_EndAction)
		{
			if (!m_PutOffAnimEnd)
			{
				m_FixErrorTimer.Update(Time.deltaTime);
				if (m_FixErrorTimer.Second <= 0.0)
				{
					m_PutOffAnimEnd = true;
					ChangeHoldState(holdEquip: false);
					base.motionMgr.SetMaskState(m_ActionMask, state: false);
					base.entity.motionEquipment.SetWeapon(null);
					if (null != base.ikCmpt)
					{
						base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: false);
					}
				}
			}
			else if (null != base.handChangeEquipment.transform.parent && base.handChangeEquipment.transform.parent.name != base.handChangeEquipment.m_HandChangeAttr.m_PutOffBone)
			{
				base.viewCmpt.Reattach(base.handChangeEquipment.gameObject, base.handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
			}
		}
		else if (!m_PutOnAnimEnd)
		{
			m_FixErrorTimer.Update(Time.deltaTime);
			if (!(m_FixErrorTimer.Second <= 0.0))
			{
				return;
			}
			m_PutOnAnimEnd = true;
			ChangeHoldState(holdEquip: true);
			base.motionMgr.SetMaskState(m_ActionMask, state: true);
			base.entity.motionEquipment.SetWeapon(base.handChangeEquipment);
			if (null != base.ikCmpt)
			{
				base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: true);
			}
			if (null != aimAbleEquip)
			{
				base.motionMgr.Entity.SendMsg(EMsg.Battle_EnterShootMode, aimAbleEquip.m_AimAttr.m_AimPointType);
			}
			if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
			{
				base.ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
				if (!base.pauseAction && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
				{
					base.ikCmpt.m_IKAimCtrl.SetActive(active: true);
				}
				if (!m_PutOnAnimEnd)
				{
					base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
				}
			}
		}
		else if (null != base.handChangeEquipment.transform.parent && base.handChangeEquipment.transform.parent.name != base.handChangeEquipment.m_HandChangeAttr.m_PutOnBone)
		{
			base.viewCmpt.Reattach(base.handChangeEquipment.gameObject, base.handChangeEquipment.m_HandChangeAttr.m_PutOnBone);
		}
	}

	public void OnFire()
	{
		if (!(null == aimAbleEquip))
		{
			float num = (float)Math.PI * 2f;
			float num2 = 1f - aimAbleEquip.m_AimAttr.m_FireStability;
			m_TargetPhase.x += UnityEngine.Random.Range(0f, 0.5f * num2) * num;
			m_TargetPhase.y += UnityEngine.Random.Range(0f, 0.5f * num2) * num;
			m_TargetPhase.z += UnityEngine.Random.Range(0f, 0.5f * num2) * num;
			m_CurAccuracy = Mathf.Min(m_CurAccuracy + aimAbleEquip.m_AimAttr.m_AccuracyDiffusionRate, aimAbleEquip.m_AimAttr.m_AccuracyMax);
			m_CenterHeight = Mathf.Min(m_CenterHeight + aimAbleEquip.m_AimAttr.m_CenterUpDisPerShoot, aimAbleEquip.m_AimAttr.m_CenterUpDisMax);
		}
	}

	private void UpdateAimPoint()
	{
		if (!(null == aimAbleEquip) && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
		{
			float num = (float)Math.PI * 2f;
			m_CurAccuracy = Mathf.Max(m_CurAccuracy - aimAbleEquip.m_AimAttr.m_AccuracyShrinkSpeed * Time.deltaTime, aimAbleEquip.m_AimAttr.m_AccuracyMin);
			m_CenterHeight = Mathf.Max(m_CenterHeight - aimAbleEquip.m_AimAttr.m_CenterUpShrinkSpeed * Time.deltaTime, 0f);
			m_PhaseT.x = aimAbleEquip.m_AimAttr.m_AccuracyPeriod * UnityEngine.Random.Range(1f - aimAbleEquip.m_AimAttr.m_FireStability, 1f + aimAbleEquip.m_AimAttr.m_FireStability);
			m_PhaseT.y = aimAbleEquip.m_AimAttr.m_AccuracyPeriod * UnityEngine.Random.Range(1f - aimAbleEquip.m_AimAttr.m_FireStability, 1f + aimAbleEquip.m_AimAttr.m_FireStability);
			m_PhaseT.z = aimAbleEquip.m_AimAttr.m_AccuracyPeriod * UnityEngine.Random.Range(1f - aimAbleEquip.m_AimAttr.m_FireStability, 1f + aimAbleEquip.m_AimAttr.m_FireStability);
			m_TargetPhase.x += num * (Time.deltaTime / m_PhaseT.x);
			m_TargetPhase.y += num * (Time.deltaTime / m_PhaseT.y);
			m_TargetPhase.z += num * (Time.deltaTime / m_PhaseT.z);
			m_Phase = Vector3.Lerp(m_Phase, m_TargetPhase, m_AimPointLerpSpeed * Time.deltaTime);
			float magnitude = (base.ikCmpt.m_IKAimCtrl.targetPos - base.ikCmpt.m_IKAimCtrl.m_DetectorCenter.position).magnitude;
			float num2 = Mathf.Clamp01(magnitude / 100f);
			base.ikCmpt.m_IKAimCtrl.m_TargetPosOffset.x = num2 * m_CurAccuracy * Mathf.Sin(m_Phase.x);
			base.ikCmpt.m_IKAimCtrl.m_TargetPosOffset.y = num2 * m_CurAccuracy * Mathf.Sin(m_Phase.y) + m_CenterHeight;
			base.ikCmpt.m_IKAimCtrl.m_TargetPosOffset.z = num2 * m_CurAccuracy * Mathf.Sin(m_Phase.z);
		}
	}

	private void UpdateRotate()
	{
		if (base.pauseAction)
		{
			if (null != base.anim)
			{
				base.anim.SetFloat("RotationAgr", 0f);
			}
		}
		else
		{
			if ((null != aimAbleEquip && !base.motionMgr.GetMaskState(aimAbleEquip.m_HandChangeAttr.m_HoldActionMask)) || !(null != base.trans) || !(null != base.ikCmpt) || !(null != base.ikCmpt.m_IKAimCtrl))
			{
				return;
			}
			Vector3 vector = base.ikCmpt.m_IKAimCtrl.targetPos - base.trans.position;
			vector = Vector3.ProjectOnPlane(vector, base.trans.existent.up).normalized;
			float num = Vector3.Angle(vector, base.trans.existent.forward);
			if (m_StartRot || num > m_MinAngle)
			{
				m_StartRot = true;
				float num2 = Mathf.Clamp(m_RotateSpeed * Time.deltaTime, 0f, num);
				if (num - num2 > m_MaxAngle)
				{
					num2 = num - m_MaxAngle;
				}
				if (num - num2 < m_StopRotAngle)
				{
					m_StartRot = false;
				}
				float num3 = Vector3.Angle(vector, base.trans.existent.right);
				if (num3 > 90f)
				{
					num2 *= -1f;
				}
				base.trans.rotation = Quaternion.AngleAxis(num2, base.trans.existent.up) * base.trans.rotation;
			}
			else if (null != base.anim)
			{
				base.anim.SetFloat("RotationAgr", 0f);
			}
		}
	}

	private void InitAimPoint()
	{
		if (!(null == aimAbleEquip))
		{
			m_CurAccuracy = aimAbleEquip.m_AimAttr.m_AccuracyMin;
			m_CenterHeight = 0f;
			float num = (float)Math.PI * 2f;
			m_TargetPhase.x = UnityEngine.Random.Range(0f, 1f) * num;
			m_TargetPhase.y = UnityEngine.Random.Range(0f, 1f) * num;
			m_TargetPhase.z = UnityEngine.Random.Range(0f, 1f) * num;
			m_Phase = m_TargetPhase;
		}
	}

	public float GetAimPointScale()
	{
		if (null == aimAbleEquip)
		{
			return 0f;
		}
		if (base.motionMgr.IsActionRunning(PEActionType.Move) || base.motionMgr.IsActionRunning(PEActionType.Sprint) || base.motionMgr.IsActionRunning(PEActionType.Fall) || base.motionMgr.IsActionRunning(PEActionType.Jump))
		{
			return 1f;
		}
		if (null != aimAbleEquip)
		{
			return m_CurAccuracy / aimAbleEquip.m_AimAttr.m_AccuracyMax;
		}
		return 0f;
	}

	private bool UpdateEndAction()
	{
		if (m_EndAction && m_PutOffAnimEnd)
		{
			if (null != base.anim)
			{
				base.anim.SetFloat("RotationAgr", 0f);
			}
			OnEndAction();
			return true;
		}
		return false;
	}
}
