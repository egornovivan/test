using System;
using SkillSystem;

namespace Pathea;

[Serializable]
public class Action_GunMelee : PEAction
{
	private PEGun m_Gun;

	private SkInst m_SkillInst;

	private int m_ModeIndex;

	public override PEActionType ActionType => PEActionType.GunMelee;

	public SkEntity targetEntity { get; set; }

	public PEGun gun
	{
		get
		{
			return m_Gun;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Gun = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		return null != gun;
	}

	public override void DoAction(PEActionParam para = null)
	{
		PEActionParamN pEActionParamN = para as PEActionParamN;
		m_ModeIndex = pEActionParamN.n;
		if (null != gun && gun.m_SkillIDList != null && gun.m_SkillIDList.Length > m_ModeIndex)
		{
			m_SkillInst = base.skillCmpt.StartSkill(targetEntity, gun.m_SkillIDList[m_ModeIndex]);
		}
		if (null != base.entity && gun.m_AttackMode != null && gun.m_AttackMode.Length > m_ModeIndex)
		{
			base.entity.SendMsg(EMsg.Battle_OnAttack, gun.m_AttackMode[m_ModeIndex], gun.transform, gun.curItemID);
		}
	}

	public override bool Update()
	{
		if (null == base.anim)
		{
			return true;
		}
		if (null != gun && gun.m_SkillIDList != null && gun.m_SkillIDList.Length > m_ModeIndex)
		{
			return !base.skillCmpt.IsSkillRunning(gun.m_SkillIDList[m_ModeIndex]);
		}
		return true;
	}

	public override void EndImmediately()
	{
		if (null != gun && gun.m_SkillIDList != null && gun.m_SkillIDList.Length > m_ModeIndex)
		{
			base.skillCmpt.CancelSkillById(gun.m_SkillIDList[m_ModeIndex]);
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (null != gun && base.motionMgr.IsActionRunning(ActionType))
		{
			if (eventParam == "EndAction")
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			if (eventParam == "MonsterEndAttack" && m_SkillInst != null)
			{
				m_SkillInst.SkipWaitAll = true;
			}
		}
	}
}
