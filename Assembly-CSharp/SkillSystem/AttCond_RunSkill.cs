using System;

namespace SkillSystem;

public class AttCond_RunSkill : AttCond
{
	private int mSkillID;

	private bool mCheckRun;

	public AttCond_RunSkill(SkEntity skEntity, string[] para)
		: base(skEntity)
	{
		mSkillID = Convert.ToInt32(para[1]);
		mCheckRun = Convert.ToBoolean(para[2]);
	}

	public override bool Check()
	{
		return mSkEntity.IsSkillRunning(mSkillID) == mCheckRun;
	}
}
