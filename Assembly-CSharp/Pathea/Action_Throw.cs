using System;
using ItemAsset;
using SkillSystem;

namespace Pathea;

[Serializable]
public class Action_Throw : PEAction
{
	private PEGrenade m_Grenade;

	private bool m_StartThrow;

	private bool m_AnimEnd;

	private bool m_Throwed;

	public override PEActionType ActionType => PEActionType.Throw;

	public SkEntity targetEntity { get; set; }

	public PEGrenade grenade
	{
		get
		{
			return m_Grenade;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Grenade = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		return null != grenade;
	}

	public override void DoAction(PEActionParam para = null)
	{
		m_AnimEnd = false;
		if (null != base.anim)
		{
			base.anim.ResetTrigger("ResetUpbody");
			base.anim.SetTrigger("ThrowGrenade");
		}
		m_StartThrow = false;
		m_Throwed = false;
		if (null != grenade && grenade.m_AttackRanges != null)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Battle_OnAttack, grenade.m_AttackRanges, grenade.transform, grenade.ItemObj.protoId);
		}
	}

	public override void ResetAction(PEActionParam para = null)
	{
		base.ResetAction(para);
	}

	public override bool Update()
	{
		if (null == base.anim || null == base.viewCmpt || !base.viewCmpt.hasView || null == grenade)
		{
			return true;
		}
		if (m_StartThrow)
		{
			m_StartThrow = false;
			ThrowGrenade();
		}
		if (m_AnimEnd)
		{
			base.motionMgr.SetMaskState(PEActionMask.Throw, state: false);
			if (null != grenade.m_Model)
			{
				grenade.m_Model.SetActive(value: true);
			}
			if (null != grenade && grenade.m_ItemObj.stackCount == 0 && null != base.equipCmpt)
			{
				int instanceId = grenade.m_ItemObj.instanceId;
				base.equipCmpt.TakeOffEquipment(grenade.m_ItemObj, addToReceiver: false);
				PeSingleton<ItemMgr>.Instance.DestroyItem(instanceId);
			}
			return true;
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Throw, state: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetUpbody");
			base.anim.ResetTrigger("ThrowGrenade");
		}
	}

	private void ThrowGrenade()
	{
		if (null != grenade && null != base.skillCmpt)
		{
			int num = (grenade.itemCost ? 1 : 0);
			if (GameConfig.IsMultiMode)
			{
				PlayerNetwork.mainPlayer.RequestThrow(base.motionMgr.Entity.Id, grenade.m_ItemObj.instanceId, num);
			}
			else
			{
				grenade.m_ItemObj.stackCount -= num;
			}
			ShootTargetPara shootTargetPara = new ShootTargetPara();
			if (null != base.ikCmpt)
			{
				shootTargetPara.m_TargetPos = base.ikCmpt.aimTargetPos;
			}
			else
			{
				shootTargetPara.m_TargetPos = base.motionMgr.Entity.position + base.motionMgr.Entity.forward;
			}
			base.skillCmpt.StartSkill(targetEntity, grenade.m_SkillID, shootTargetPara);
			if (null != grenade.m_Model)
			{
				grenade.m_Model.SetActive(value: false);
			}
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType))
		{
			if (!m_Throwed && "ThrowGrenade" == eventParam)
			{
				m_StartThrow = true;
				m_Throwed = true;
			}
			else if ("ThrowAnimEnd" == eventParam)
			{
				m_AnimEnd = true;
			}
		}
	}
}
