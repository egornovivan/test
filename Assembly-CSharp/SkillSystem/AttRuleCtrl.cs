using System.Collections.Generic;

namespace SkillSystem;

public class AttRuleCtrl
{
	private SkEntity mSkEntity;

	private Dictionary<int, AttRule> mRuleDic;

	private List<int> mRemoveList;

	public AttRuleCtrl(SkEntity skEntity)
	{
		mSkEntity = skEntity;
		mRuleDic = new Dictionary<int, AttRule>();
		mRemoveList = new List<int>();
	}

	public void AddRule(int ID)
	{
		if (!mRuleDic.ContainsKey(ID))
		{
			AttRule attRule = AttRule.Creat(this, mSkEntity, ID);
			if (attRule != null)
			{
				mRuleDic[ID] = attRule;
			}
		}
	}

	public void RemoveRule(int ID)
	{
		if (mRuleDic.ContainsKey(ID))
		{
			mRemoveList.Add(ID);
		}
	}

	public void Update()
	{
		foreach (int mRemove in mRemoveList)
		{
			mRuleDic[mRemove].Destroy();
			mRuleDic.Remove(mRemove);
		}
		mRemoveList.Clear();
		foreach (AttRule value in mRuleDic.Values)
		{
			value.Update();
		}
	}
}
