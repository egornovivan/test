using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSkillAround), "SkillAround")]
public class BTSkillAround : BTNormal
{
	private PeEntity Target;

	private int AlreadySkill;

	private Vector3 TargetPos;

	private RQUseSkill m_UseSkill;

	private bool ConditionDecide(Vector3 targetPos, int skillId)
	{
		float num = Vector3.Distance(base.CostPos, targetPos);
		float skillRange = GetSkillRange(skillId);
		if (skillRange == -1f)
		{
			return false;
		}
		return num < skillRange && GetHpJudge(skillId);
	}

	private BehaveResult Init(Tree sender)
	{
		m_UseSkill = GetRequest(EReqType.UseSkill) as RQUseSkill;
		if (m_UseSkill == null)
		{
			return BehaveResult.Failure;
		}
		Target = base.SkillTarget;
		if (Target == null)
		{
			return BehaveResult.Failure;
		}
		if (Target.IsDeath())
		{
			SkillOver();
			return BehaveResult.Failure;
		}
		TargetPos = base.AllyTargetPos;
		AlreadySkill = GetAreadySkill();
		if (AlreadySkill == -1)
		{
			SkillOver();
			return BehaveResult.Failure;
		}
		if (!ConditionDecide(TargetPos, AlreadySkill) || IsSkillRunning(AlreadySkill))
		{
			SkillOver();
			return BehaveResult.Failure;
		}
		StartSkill(base.SkillTarget, AlreadySkill);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.SkillAround);
		if (!base.IsSkillCast || Target == null)
		{
			return BehaveResult.Failure;
		}
		if (Target.IsDeath())
		{
			SkillOver();
			return BehaveResult.Failure;
		}
		if (IsSkillRunning(AlreadySkill))
		{
			return BehaveResult.Running;
		}
		if (SkillOver())
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		SkillOver();
	}
}
