namespace SkillSystem;

public class AttRule
{
	private AttFilterCtrl mFilter;

	private AttCondCtrl mCond;

	private AttActionCtrl mAction;

	public void Update()
	{
		if (mFilter.CheckFilter())
		{
			CheckAction();
		}
	}

	public void CheckAction()
	{
		if (mCond.Check())
		{
			mAction.DoAction();
		}
	}

	public void Destroy()
	{
		mFilter.Destroy();
	}

	public static AttRule Creat(AttRuleCtrl ctrl, SkEntity skEntity, int ruleID)
	{
		AttRuleData ruleData = AttRuleData.GetRuleData(ruleID);
		if (ruleData != null)
		{
			AttRule attRule = new AttRule();
			attRule.mFilter = new AttFilterCtrl(skEntity, ruleData.mFilter, attRule.CheckAction);
			attRule.mCond = new AttCondCtrl(skEntity, ruleData.mCond);
			attRule.mAction = new AttActionCtrl(ctrl, skEntity, ruleData.mAction);
			return attRule;
		}
		return null;
	}
}
