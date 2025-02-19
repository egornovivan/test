using System;

namespace SkillSystem;

public class AttCond_Compare : AttCond
{
	private int mType;

	private bool mLess;

	private int mTargetAtt;

	private int mConAtt;

	private float mTargetValue;

	public AttCond_Compare(SkEntity skEntity, string[] para)
		: base(skEntity)
	{
		mType = ((para.Length == 5) ? 1 : 0);
		mLess = para[1].ToLower() == "less";
		mTargetAtt = Convert.ToInt32(para[2]);
		switch (mType)
		{
		case 0:
			mTargetValue = Convert.ToSingle(para[3]);
			break;
		case 1:
			mConAtt = Convert.ToInt32(para[3]);
			mTargetValue = Convert.ToSingle(para[4]);
			break;
		}
	}

	public override bool Check()
	{
		if (mLess)
		{
			switch (mType)
			{
			case 0:
				return mSkEntity.attribs.sums[mTargetAtt] <= mTargetValue;
			case 1:
				return mSkEntity.attribs.sums[mTargetAtt] <= mTargetValue * mSkEntity.attribs.sums[mConAtt];
			}
		}
		else
		{
			switch (mType)
			{
			case 0:
				return mSkEntity.attribs.sums[mTargetAtt] >= mTargetValue;
			case 1:
				return mSkEntity.attribs.sums[mTargetAtt] >= mTargetValue * mSkEntity.attribs.sums[mConAtt];
			}
		}
		return base.Check();
	}
}
