using System;

namespace SkillSystem;

public class AttAction_Skill : AttAction
{
	private int mSkillID;

	public AttAction_Skill(SkEntity skEntity, string[] para)
		: base(skEntity)
	{
		mSkillID = Convert.ToInt32(para[1]);
	}

	public override void Do()
	{
		mSkEntity.StartSkill(mSkEntity, mSkillID);
	}
}
