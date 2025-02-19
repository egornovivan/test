using System;
using ItemAsset;
using UnityEngine;
using WhiteCat;

namespace Pathea;

[Serializable]
public class Action_Drive : PEAction
{
	public SkillTreeUnitMgr skillTreeMgr;

	private CarrierController m_DrivingController;

	private VCPBaseSeat m_Seat;

	private string m_AnimName;

	private Transform m_LHand;

	private Transform m_RHand;

	public override PEActionType ActionType => PEActionType.Drive;

	public IKDrive ikDrive { get; set; }

	public VCPBaseSeat seat => m_Seat;

	public void SetSeat(string animName, VCPBaseSeat seat)
	{
		m_Seat = seat;
		m_AnimName = animName;
		if (null != base.anim && string.Empty != m_AnimName)
		{
			base.anim.SetBool(m_AnimName, value: true);
		}
	}

	public void SetHand(Transform lHand, Transform rHand)
	{
		m_LHand = lHand;
		m_RHand = rHand;
		if (null != ikDrive)
		{
			ikDrive.active = true;
			ikDrive.m_LHand = m_LHand;
			ikDrive.m_RHand = m_RHand;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (base.motionMgr.IsActionRunning(PEActionType.Build))
		{
			PeTipMsg.Register(PELocalization.GetString(9500246), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		PEActionParamDrive pEActionParamDrive = para as PEActionParamDrive;
		m_DrivingController = pEActionParamDrive.controller;
		if (m_DrivingController == null)
		{
			return false;
		}
		ItemObject itemObject = m_DrivingController.itemObject;
		if (itemObject != null && skillTreeMgr != null && RandomMapConfig.useSkillTree && !skillTreeMgr.CheckDriveEnable(itemObject.protoData.itemClassId, itemObject.protoData.level))
		{
			return false;
		}
		return true;
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.OnVehicle, state: true);
		PEActionParamDrive pEActionParamDrive = para as PEActionParamDrive;
		m_DrivingController = pEActionParamDrive.controller;
		int seatIndex = pEActionParamDrive.seatIndex;
		m_DrivingController.GetOn(base.motionMgr.Entity, seatIndex);
		base.motionMgr.FreezePhyState(GetType(), v: true);
		base.motionMgr.FreezeCol = true;
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = false;
		}
		if (null != base.equipCmpt)
		{
			base.equipCmpt.HideEquipmentByVehicle(hide: true);
		}
		if (null != base.motionMgr.Entity.biologyViewCmpt)
		{
			base.motionMgr.Entity.biologyViewCmpt.ActivateInjured(value: false);
		}
		base.motionMgr.Entity.SendMsg(EMsg.Action_GetOnVehicle, true, m_DrivingController);
	}

	public override void OnModelBuild()
	{
		if (null != base.anim && string.Empty != m_AnimName)
		{
			base.anim.SetBool(m_AnimName, value: true);
		}
		if (null != base.equipCmpt)
		{
			base.equipCmpt.HideEquipmentByVehicle(hide: true);
		}
		if (null != ikDrive && null != m_LHand && null != m_RHand)
		{
			ikDrive.active = true;
			ikDrive.m_LHand = m_LHand;
			ikDrive.m_RHand = m_RHand;
		}
	}

	public override void OnModelDestroy()
	{
	}

	public override bool Update()
	{
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.OnVehicle, state: false);
		if (null != base.anim)
		{
			base.anim.SetBool(m_AnimName, value: false);
		}
		if (null != m_Seat)
		{
			m_Seat.GetOff();
		}
		else if (null != base.entity && null != base.entity.passengerCmpt)
		{
			base.entity.passengerCmpt.GetOffCarrier(base.entity.position);
		}
		base.motionMgr.FreezePhyState(GetType(), v: false);
		base.motionMgr.FreezeCol = false;
		if (null != base.equipCmpt)
		{
			base.equipCmpt.HideEquipmentByVehicle(hide: false);
		}
		if (null != ikDrive)
		{
			ikDrive.active = false;
			ikDrive.m_LHand = null;
			ikDrive.m_RHand = null;
		}
		if (null != base.motionMgr.Entity.biologyViewCmpt)
		{
			base.motionMgr.Entity.biologyViewCmpt.ActivateInjured(value: true);
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.ikEnable = true;
		}
		m_Seat = null;
		m_AnimName = string.Empty;
		base.motionMgr.Entity.SendMsg(EMsg.Action_GetOnVehicle, false, m_DrivingController);
	}
}
