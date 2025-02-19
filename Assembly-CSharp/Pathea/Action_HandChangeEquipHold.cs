using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_HandChangeEquipHold : PEAction, iEquipHideAbleAction
{
	private bool m_HideEquipInactive;

	private PEHoldAbleEquipment mHandChangeEquipment;

	protected bool m_EndAction;

	protected bool m_PutOnAnimEnd;

	protected bool m_PutOffAnimEnd;

	protected bool m_HoldState;

	protected UTimer m_FixErrorTimer;

	protected PEActionMask m_ActionMask = PEActionMask.EquipmentHold;

	public override PEActionType ActionType => PEActionType.EquipmentHold;

	public bool hideEquipInactive
	{
		get
		{
			return m_HideEquipInactive;
		}
		set
		{
			m_HideEquipInactive = value;
			if (null != mHandChangeEquipment)
			{
				SetHideState(!base.motionMgr.IsActionRunning(ActionType));
			}
		}
	}

	public PEHoldAbleEquipment handChangeEquipment
	{
		get
		{
			return mHandChangeEquipment;
		}
		set
		{
			if (null == value || (null != mHandChangeEquipment && mHandChangeEquipment != value))
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			mHandChangeEquipment = value;
			if (null != mHandChangeEquipment)
			{
				m_ActionMask = mHandChangeEquipment.m_HandChangeAttr.m_HoldActionMask;
				SetHideState(hide: true);
			}
			else
			{
				m_ActionMask = PEActionMask.EquipmentHold;
			}
		}
	}

	public event Action onActiveEvt;

	public event Action onDeactiveEvt;

	public Action_HandChangeEquipHold()
	{
		if (m_FixErrorTimer == null)
		{
			m_FixErrorTimer = new UTimer();
			m_FixErrorTimer.ElapseSpeed = -1f;
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (!base.motionMgr.IsActionRunning(ActionType))
		{
			return;
		}
		switch (eventParam)
		{
		case "PutOnAnimEnd":
			m_PutOnAnimEnd = true;
			break;
		case "PutOffAnimEnd":
			m_PutOffAnimEnd = true;
			if (null != base.move && m_EndAction)
			{
				base.move.style = ((!(null != handChangeEquipment)) ? base.move.baseMoveStyle : handChangeEquipment.m_HandChangeAttr.m_BaseMoveStyle);
			}
			break;
		case "PutOnEquipment":
			ChangeHoldState(holdEquip: true);
			break;
		case "PutOffEquipment":
			ChangeHoldState(holdEquip: false);
			break;
		case "ChangeCameraMode":
			if (null != handChangeEquipment)
			{
				base.motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
			}
			break;
		case "Active":
			if (null != handChangeEquipment)
			{
				handChangeEquipment.SetActiveState(active: true);
			}
			break;
		case "Deactive":
			if (null != handChangeEquipment)
			{
				handChangeEquipment.SetActiveState(active: false);
			}
			break;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		return null != base.anim && null != handChangeEquipment && handChangeEquipment.canHoldEquipment;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (this.onActiveEvt != null)
		{
			this.onActiveEvt();
		}
		m_EndAction = false;
		m_PutOnAnimEnd = false;
		m_PutOffAnimEnd = false;
		m_HoldState = false;
		m_FixErrorTimer.Second = 5.0;
		SetHideState(hide: false);
		if (null != base.anim && null != handChangeEquipment)
		{
			if (!string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim))
			{
				if (handChangeEquipment.m_HandChangeAttr.m_PutOnAnim != handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
				{
					base.anim.SetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim);
				}
				else
				{
					base.anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, value: true);
				}
			}
			else
			{
				ChangeHoldState(holdEquip: true);
				m_PutOnAnimEnd = true;
			}
		}
		if (null != base.move && null != handChangeEquipment)
		{
			base.move.style = handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: true);
			if (null != base.ikCmpt.m_IKAimCtrl && null != handChangeEquipment)
			{
				base.ikCmpt.m_IKAimCtrl.m_DetectorAngle = handChangeEquipment.m_HandChangeAttr.m_AimIKAngleRange;
			}
		}
	}

	public override void OnModelBuild()
	{
		if (m_EndAction)
		{
			return;
		}
		m_PutOnAnimEnd = true;
		ChangeHoldState(holdEquip: true, checkState: false, isReattach: false);
		if (null != handChangeEquipment)
		{
			base.motionMgr.SetMaskState(m_ActionMask, state: true);
			base.entity.motionEquipment.SetWeapon(handChangeEquipment);
			base.motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
			if (null != base.move)
			{
				base.move.style = handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
			}
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: true);
		}
		if (null != base.anim && null != handChangeEquipment && !string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim) && handChangeEquipment.m_HandChangeAttr.m_PutOnAnim == handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
		{
			base.anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, value: true);
		}
	}

	public override void ContinueAction()
	{
		if (m_EndAction)
		{
			return;
		}
		m_PutOnAnimEnd = true;
		ChangeHoldState(holdEquip: true, checkState: false, isReattach: false);
		if (null != handChangeEquipment)
		{
			base.motionMgr.SetMaskState(m_ActionMask, state: true);
			base.entity.motionEquipment.SetWeapon(handChangeEquipment);
			base.motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
			if (null != base.move)
			{
				base.move.style = handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
			}
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: true);
		}
		if (null != base.anim && null != handChangeEquipment && !string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim) && handChangeEquipment.m_HandChangeAttr.m_PutOnAnim == handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
		{
			base.anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, value: true);
		}
	}

	public override void OnModelDestroy()
	{
		if (m_EndAction)
		{
			base.OnModelDestroy();
		}
	}

	public override bool Update()
	{
		UpdateActiveMask();
		FixAnimError();
		if (m_EndAction && m_PutOffAnimEnd)
		{
			OnEndAction();
			return true;
		}
		return false;
	}

	public override void EndAction()
	{
		if (m_EndAction)
		{
			return;
		}
		m_EndAction = true;
		if (null != base.anim && null != handChangeEquipment)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, CameraModeData.DefaultCameraData);
			if (null != base.anim)
			{
				if (!string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim))
				{
					if (handChangeEquipment.m_HandChangeAttr.m_PutOnAnim != handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
					{
						base.anim.SetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim);
					}
					else
					{
						base.anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, value: false);
					}
				}
				else
				{
					m_PutOffAnimEnd = true;
				}
			}
			base.motionMgr.SetMaskState(m_ActionMask, state: false);
			base.entity.motionEquipment.SetWeapon(null);
			if (null != base.ikCmpt)
			{
				base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: false);
			}
		}
		else
		{
			m_PutOffAnimEnd = true;
		}
		if (null != base.move)
		{
			base.move.style = ((!(null != handChangeEquipment)) ? base.move.baseMoveStyle : handChangeEquipment.m_HandChangeAttr.m_BaseMoveStyle);
		}
		m_FixErrorTimer.Second = 5.0;
	}

	public override void EndImmediately()
	{
		m_EndAction = true;
		if (null != handChangeEquipment)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, CameraModeData.DefaultCameraData);
			base.viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
			if (null != base.ikCmpt)
			{
				base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: false);
			}
		}
		if (null != base.move)
		{
			base.move.style = ((!(null != handChangeEquipment)) ? base.move.baseMoveStyle : handChangeEquipment.m_HandChangeAttr.m_BaseMoveStyle);
		}
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetUpbody");
			if (null != handChangeEquipment)
			{
				base.anim.ResetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim);
				base.anim.ResetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim);
			}
		}
		OnEndAction();
	}

	protected virtual void OnEndAction()
	{
		SetHideState(hide: true);
		base.motionMgr.SetMaskState(m_ActionMask, state: false);
		if (this.onDeactiveEvt != null)
		{
			this.onDeactiveEvt();
		}
	}

	protected virtual void UpdateActiveMask()
	{
		if (!(null == handChangeEquipment) && !m_EndAction && !base.motionMgr.GetMaskState(m_ActionMask) && m_PutOnAnimEnd)
		{
			base.motionMgr.SetMaskState(m_ActionMask, state: true);
			base.entity.motionEquipment.SetWeapon(handChangeEquipment);
		}
	}

	protected virtual void FixAnimError()
	{
		if (null == handChangeEquipment)
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
			else if (null != handChangeEquipment.transform.parent && handChangeEquipment.transform.parent.name != handChangeEquipment.m_HandChangeAttr.m_PutOffBone)
			{
				base.viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
			}
		}
		else if (!m_PutOnAnimEnd)
		{
			m_FixErrorTimer.Update(Time.deltaTime);
			if (m_FixErrorTimer.Second <= 0.0)
			{
				m_PutOnAnimEnd = true;
				ChangeHoldState(holdEquip: true);
				base.motionMgr.SetMaskState(m_ActionMask, state: true);
				base.entity.motionEquipment.SetWeapon(handChangeEquipment);
				if (null != handChangeEquipment)
				{
					base.motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
				}
				if (null != base.ikCmpt)
				{
					base.ikCmpt.SetSpineEffectDeactiveState(GetType(), deactive: true);
				}
			}
		}
		else if (null != handChangeEquipment.transform.parent && handChangeEquipment.transform.parent.name != handChangeEquipment.m_HandChangeAttr.m_PutOnBone)
		{
			base.viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOnBone);
		}
	}

	public virtual void ChangeHoldState(bool holdEquip, bool checkState = true, bool isReattach = true)
	{
		if (null == handChangeEquipment || handChangeEquipment.m_HandChangeAttr.m_PutOffBone == handChangeEquipment.m_HandChangeAttr.m_PutOnBone)
		{
			return;
		}
		if (holdEquip)
		{
			if (m_HoldState && checkState)
			{
				return;
			}
			m_HoldState = true;
			if (null != base.viewCmpt)
			{
				if (isReattach)
				{
					base.viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOnBone);
				}
				else
				{
					base.viewCmpt.AttachObject(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOnBone);
				}
			}
		}
		else
		{
			if (!m_HoldState && checkState)
			{
				return;
			}
			m_HoldState = false;
			if (null != base.viewCmpt)
			{
				if (isReattach)
				{
					base.viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
				}
				else
				{
					base.viewCmpt.AttachObject(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
				}
			}
		}
	}

	private void SetHideState(bool hide)
	{
		if (hideEquipInactive && null != handChangeEquipment)
		{
			mHandChangeEquipment.HideEquipmentByFirstPerson(hide);
		}
	}
}
