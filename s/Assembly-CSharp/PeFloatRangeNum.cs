using UnityEngine;

public class PeFloatRangeNum
{
	private float mMin;

	private float mMax;

	private float mCurrent;

	public float percent => (mCurrent - mMin) / (mMax - mMin);

	public float current
	{
		get
		{
			return mCurrent;
		}
		set
		{
			mCurrent = Mathf.Clamp(value, mMin, mMax);
		}
	}

	public float ExpendValue => mMax - current;

	public PeFloatRangeNum(float cur, float min, float max)
	{
		mCurrent = cur;
		mMin = min;
		mMax = max;
	}

	public bool Change(float delta)
	{
		current += delta;
		if (current < mMin)
		{
			current = mMin;
			return false;
		}
		if (current > mMax)
		{
			current = mMax;
			return false;
		}
		return true;
	}

	public bool ChangePercent(float percent)
	{
		current += (mMax - mMin) * percent;
		if (current < mMin)
		{
			current = mMin;
			return false;
		}
		if (current > mMax)
		{
			current = mMax;
			return false;
		}
		return true;
	}

	public void SetToMax()
	{
		current = mMax;
	}

	public void SetToMin()
	{
		current = mMin;
	}

	public bool IsCurrentMax()
	{
		return Mathf.Abs(current - mMax) < Mathf.Epsilon;
	}

	public bool IsCurrentMin()
	{
		return Mathf.Abs(current) < Mathf.Epsilon;
	}
}
