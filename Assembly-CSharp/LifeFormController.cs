using System.Collections.Generic;
using SkillAsset;
using UnityEngine;

public class LifeFormController : MonoBehaviour
{
	public List<int> mRules;

	private List<int> mActiveRules;

	private Dictionary<int, float> mRulesDecreaseStackCount;

	private List<int> mIndexList;

	private Dictionary<int, float> mPropertys;

	private Dictionary<int, float> mPropertyMaxs;

	private SkillRunner mSkillRunner;

	private void Awake()
	{
		mActiveRules = new List<int>();
		mRulesDecreaseStackCount = new Dictionary<int, float>();
		mIndexList = new List<int>();
		mPropertys = new Dictionary<int, float>();
		mPropertyMaxs = new Dictionary<int, float>();
		if (mRules == null)
		{
			mRules = new List<int>();
		}
		foreach (int mRule in mRules)
		{
			AddRule(mRule);
		}
		mSkillRunner = GetComponent<SkillRunner>();
	}

	private void Update()
	{
		foreach (int mIndex in mIndexList)
		{
			Dictionary<int, float> dictionary;
			Dictionary<int, float> dictionary2 = (dictionary = mRulesDecreaseStackCount);
			int key;
			int key2 = (key = mIndex);
			float num = dictionary[key];
			dictionary2[key2] = num - Time.deltaTime;
			if (mRulesDecreaseStackCount[mIndex] < 0f)
			{
				LifeFormRule rule = LifeFormRule.GetRule(mIndex);
				mRulesDecreaseStackCount[mIndex] = rule.mUpdateInterval;
				if ((rule.mConditionType == 0 && mPropertys[rule.mPropertyType] >= rule.mConditionMin && mPropertys[rule.mPropertyType] <= rule.mConditionMax) || (rule.mConditionType == 1 && mPropertys[rule.mPropertyType] / mPropertyMaxs[rule.mPropertyType] >= rule.mConditionMin && mPropertys[rule.mPropertyType] / mPropertyMaxs[rule.mPropertyType] <= rule.mConditionMax))
				{
					mSkillRunner.RunEff(rule.mCostSkillID, mSkillRunner);
				}
			}
		}
	}

	public void AddRule(int ruleID)
	{
		if (!mActiveRules.Contains(ruleID))
		{
			mActiveRules.Add(ruleID);
			LifeFormRule rule = LifeFormRule.GetRule(ruleID);
			mRulesDecreaseStackCount[rule.mID] = rule.mUpdateInterval;
			mIndexList.Add(rule.mID);
			mPropertyMaxs[rule.mPropertyType] = rule.mPropertyValueMax;
			mPropertys[rule.mPropertyType] = 0f;
		}
	}

	public void ApplyPropertyChange(Dictionary<int, float> propertyChanges)
	{
		foreach (int key in propertyChanges.Keys)
		{
			if (mPropertys.ContainsKey(key))
			{
				mPropertys[key] = Mathf.Clamp(mPropertys[key] + propertyChanges[key], 0f, mPropertyMaxs[key]);
				break;
			}
		}
	}
}
