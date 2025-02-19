using System.Collections.Generic;
using UnityEngine;

public class LifeFormController : MonoBehaviour
{
	public List<int> mRules;

	private List<int> mActiveRules;

	private Dictionary<int, float> mRulesCountDown;

	private List<int> mIndexList;

	private Dictionary<int, float> mPropertys;

	private Dictionary<int, float> mPropertyMaxs;

	private void Awake()
	{
		mActiveRules = new List<int>();
		mRulesCountDown = new Dictionary<int, float>();
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
	}

	private void Update()
	{
	}

	public void AddRule(int ruleID)
	{
		if (!mActiveRules.Contains(ruleID))
		{
			mActiveRules.Add(ruleID);
			LifeFormRule rule = LifeFormRule.GetRule(ruleID);
			mRulesCountDown[rule.mID] = rule.mUpdateInterval;
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
