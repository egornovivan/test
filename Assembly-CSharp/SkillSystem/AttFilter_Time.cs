using UnityEngine;

namespace SkillSystem;

public class AttFilter_Time : AttFilter
{
	private float mIntervalTime;

	private UTimer mTimer;

	public AttFilter_Time(float intervalTime)
	{
		mIntervalTime = intervalTime;
		mTimer = new UTimer();
		mTimer.ElapseSpeed = -1f;
		mTimer.Second = mIntervalTime;
	}

	public override bool CheckFilter()
	{
		mTimer.Update(Time.deltaTime);
		if (mTimer.Second <= 0.0)
		{
			mTimer.Second = mIntervalTime;
			return true;
		}
		return base.CheckFilter();
	}
}
