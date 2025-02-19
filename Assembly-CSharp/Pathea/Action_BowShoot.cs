using System;
using SkillSystem;

namespace Pathea;

[Serializable]
public class Action_BowShoot : PEAction
{
	private PEBow m_Bow;

	public bool m_IgnoreItem;

	public override PEActionType ActionType => PEActionType.BowShoot;

	public IKAimCtrl ikAim { get; set; }

	public SkEntity targetEntity { get; set; }

	public PEBow bow
	{
		get
		{
			return m_Bow;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Bow = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null == base.skillCmpt || null == bow || base.skillCmpt.IsSkillRunning(bow.skillID))
		{
			return false;
		}
		if (bow.durability <= float.Epsilon)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
			return false;
		}
		return base.CanDoAction(para);
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null == base.skillCmpt || null == bow)
		{
			return;
		}
		ShootTargetPara shootTargetPara = new ShootTargetPara();
		if (null != ikAim)
		{
			shootTargetPara.m_TargetPos = ikAim.targetPos;
		}
		else
		{
			shootTargetPara.m_TargetPos = base.motionMgr.transform.forward;
		}
		if (base.skillCmpt.StartSkill(targetEntity, bow.skillID, shootTargetPara) == null)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(base.motionMgr.Entity.Id);
			if (null != networkInterface && networkInterface.hasOwnerAuth && !m_IgnoreItem)
			{
				PlayerNetwork.mainPlayer.RequestItemCost(base.motionMgr.Entity.Id, bow.curItemID, 1f);
			}
		}
		else if (null != base.packageCmpt && !m_IgnoreItem)
		{
			base.packageCmpt.Destory(bow.curItemID, 1);
		}
		bow.SetArrowShowState(show: false);
		bow.SetBowOpenState(openBow: false);
		base.motionMgr.Entity.SendMsg(EMsg.Battle_EquipAttack, bow.ItemObj);
		if (bow.m_AttackMode != null)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Battle_OnAttack, bow.m_AttackMode[0], bow.transform, bow.curItemID);
		}
	}

	public override bool Update()
	{
		if (null == base.skillCmpt || null == bow)
		{
			return true;
		}
		if (GameConfig.IsMultiMode)
		{
			NetworkInterface networkInterface = NetworkInterface.Get(base.motionMgr.Entity.Id);
			if (null != networkInterface && !networkInterface.hasOwnerAuth)
			{
				return true;
			}
		}
		if (!base.skillCmpt.IsSkillRunning(bow.skillID))
		{
			if (m_IgnoreItem || (null != base.packageCmpt && base.packageCmpt.GetItemCount(bow.curItemID) > 0))
			{
				PEActionParamN param = PEActionParamN.param;
				param.n = bow.curItemIndex;
				base.motionMgr.DoAction(PEActionType.BowReload, param);
			}
			else
			{
				base.motionMgr.DoAction(PEActionType.BowPutOff);
			}
			bow.OnShoot();
			return true;
		}
		return true;
	}
}
