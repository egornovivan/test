using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateHolder
{
	private Animator mAnimator;

	private Dictionary<string, bool> mBoolValue = new Dictionary<string, bool>(5);

	private Dictionary<string, float> mFloatValue = new Dictionary<string, float>(5);

	public AnimatorStateHolder(Animator animator)
	{
		mAnimator = animator;
	}

	public void SetBool(string name, bool value)
	{
		mBoolValue[name] = value;
	}

	public void SetFloat(string name, float value)
	{
		mFloatValue[name] = value;
	}

	public void RestoreAnimator()
	{
		if (null == mAnimator)
		{
			Debug.LogError("Animator is null.");
			return;
		}
		foreach (KeyValuePair<string, bool> item in mBoolValue)
		{
			mAnimator.SetBool(item.Key, item.Value);
		}
		foreach (KeyValuePair<string, float> item2 in mFloatValue)
		{
			mAnimator.SetFloat(item2.Key, item2.Value);
		}
	}
}
