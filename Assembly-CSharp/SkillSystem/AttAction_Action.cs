using System;

namespace SkillSystem;

public class AttAction_Action : AttAction
{
	private AttRuleCtrl mRuleCtrl;

	private bool mIsAdd;

	private int mRuleID;

	public AttAction_Action(SkEntity skEntity, string[] para, AttRuleCtrl ruleCtrl)
		: base(skEntity)
	{
		mIsAdd = para[1].ToLower() == "add";
		mRuleID = Convert.ToInt32(para[2]);
		mRuleCtrl = ruleCtrl;
	}

	public override void Do()
	{
		if (mIsAdd)
		{
			mRuleCtrl.AddRule(mRuleID);
		}
		else
		{
			mRuleCtrl.RemoveRule(mRuleID);
		}
	}
}
