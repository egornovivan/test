using System.Collections.Generic;
using UnityEngine;

public class EatDesc
{
	public int assesID;

	public List<TimeSlot> Eattimes;

	public CircleTrans mEatCircleTrans;

	public EatDesc()
	{
		Eattimes = new List<TimeSlot>();
	}

	public bool InTheRange(float slot)
	{
		if (Eattimes == null || Eattimes.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < Eattimes.Count; i++)
		{
			if (Eattimes[i].InTheRange(slot))
			{
				return true;
			}
		}
		return false;
	}

	public void SetCircleTrans(Vector3 center, float radiu = 3f)
	{
		mEatCircleTrans = new CircleTrans(center, radiu);
		if (mEatCircleTrans.mCircleTransList == null || mEatCircleTrans.mCircleTransList.Count <= 0)
		{
			mEatCircleTrans.DisCircletarns(4, radiu);
		}
	}

	public Vector3 GetEmptyPostion(int _enityId)
	{
		if (mEatCircleTrans != null)
		{
			return mEatCircleTrans.CalculateEmptyPostion(_enityId);
		}
		return Vector3.zero;
	}
}
