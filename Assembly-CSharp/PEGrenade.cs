using ItemAsset;
using Pathea;
using SkillSystem;
using UnityEngine;

public class PEGrenade : PEAimAbleEquip, IWeapon, IAimWeapon
{
	public AttackMode m_AttackRanges;

	public GameObject m_Model;

	public int m_SkillID = 20110033;

	public bool itemCost = true;

	public ItemObject ItemObj => m_ItemObj;

	public virtual bool HoldReady => m_MotionMgr.GetMaskState(m_HandChangeAttr.m_HoldActionMask);

	public virtual bool UnHoldReady => !m_MotionMgr.IsActionRunning(m_HandChangeAttr.m_ActiveActionType);

	public virtual bool Aimed
	{
		get
		{
			if (null != m_IKCmpt)
			{
				return m_IKCmpt.aimed;
			}
			return false;
		}
	}

	public string[] leisures => new string[0];

	public virtual void HoldWeapon(bool hold)
	{
		m_MotionEquip.ActiveWeapon(this, hold);
	}

	public AttackMode[] GetAttackMode()
	{
		return new AttackMode[1] { m_AttackRanges };
	}

	public bool CanAttack(int index = 0)
	{
		return m_MotionMgr.CanDoAction(PEActionType.Throw);
	}

	public void Attack(int index = 0, SkEntity targetEntity = null)
	{
		if (null != m_MotionEquip)
		{
			m_MotionEquip.SetTarget(targetEntity);
		}
		if (null != m_MotionMgr)
		{
			m_MotionMgr.DoAction(PEActionType.Throw);
		}
		m_AttackRanges.ResetCD();
	}

	public bool AttackEnd(int index = 0)
	{
		if (null != m_MotionMgr)
		{
			return !m_MotionMgr.IsActionRunning(PEActionType.Throw);
		}
		return true;
	}

	public virtual bool IsInCD(int index = 0)
	{
		return m_AttackRanges.IsInCD();
	}

	public virtual void SetAimState(bool aimState)
	{
	}

	public void SetTarget(Vector3 aimPos)
	{
		if (null != m_IKCmpt)
		{
			m_IKCmpt.aimTargetPos = aimPos;
		}
	}

	public void SetTarget(Transform trans)
	{
		if (null != m_IKCmpt)
		{
			m_IKCmpt.aimTargetTrans = trans;
		}
	}
}
