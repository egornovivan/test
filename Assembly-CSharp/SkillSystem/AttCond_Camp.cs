using System;

namespace SkillSystem;

public class AttCond_Camp : AttCond
{
	private int mType;

	private int mTargetAtt;

	private int mConAtt;

	private float mMin;

	private float mMax;

	public AttCond_Camp(SkEntity skEntity, string[] para)
		: base(skEntity)
	{
		mType = ((para.Length == 5) ? 1 : 0);
		mTargetAtt = Convert.ToInt32(para[1]);
		switch (mType)
		{
		case 0:
			mMin = Convert.ToSingle(para[2]);
			mMax = Convert.ToSingle(para[3]);
			break;
		case 1:
			mConAtt = Convert.ToInt32(para[2]);
			mMin = Convert.ToSingle(para[3]);
			mMax = Convert.ToSingle(para[4]);
			break;
		}
	}

	public override bool Check()
	{
		return mType switch
		{
			0 => mMin <= mSkEntity.attribs.sums[mTargetAtt] && mSkEntity.attribs.sums[mTargetAtt] <= mMax, 
			1 => mMin * mSkEntity.attribs.sums[mConAtt] <= mSkEntity.attribs.sums[mTargetAtt] && mSkEntity.attribs.sums[mTargetAtt] <= mMax * mSkEntity.attribs.sums[mConAtt], 
			_ => base.Check(), 
		};
	}
}
