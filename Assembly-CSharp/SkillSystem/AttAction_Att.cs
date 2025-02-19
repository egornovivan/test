using System;

namespace SkillSystem;

public class AttAction_Att : AttAction
{
	private int mType;

	private int mTargetAtt;

	private int mConAtt;

	private float mTargetValue;

	public AttAction_Att(SkEntity skEntity, string[] para)
		: base(skEntity)
	{
		mType = ((para.Length == 4) ? 1 : 0);
		mTargetAtt = Convert.ToInt32(para[1]);
		switch (mType)
		{
		case 0:
			mTargetValue = Convert.ToSingle(para[2]);
			break;
		case 1:
			mConAtt = Convert.ToInt32(para[2]);
			mTargetValue = Convert.ToSingle(para[3]);
			break;
		}
	}

	public override void Do()
	{
		switch (mType)
		{
		case 0:
			mSkEntity.attribs.sums[mTargetAtt] = mTargetValue;
			break;
		case 1:
			mSkEntity.attribs.sums[mTargetAtt] = mTargetValue * mSkEntity.attribs.sums[mConAtt];
			break;
		}
	}
}
