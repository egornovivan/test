using System.Collections.Generic;

namespace SkillAsset;

public class EffSkillBuffMultiply
{
	public float mAtkChangeP;

	public float mDefChangeP;

	public float mMaxHpChangeP;

	public float mFallInjurse;

	public EffSkillBuffMultiply()
	{
		Rest();
	}

	private void Rest()
	{
		mAtkChangeP = 1f;
		mDefChangeP = 1f;
		mMaxHpChangeP = 1f;
		mFallInjurse = 1f;
	}

	public void ResetBuffMultiply(List<EffSkillBuffInst> buffInstList)
	{
		Rest();
		foreach (EffSkillBuffInst buffInst in buffInstList)
		{
			mAtkChangeP *= 1f + buffInst.m_buff.m_atkChangeP;
			mDefChangeP *= 1f + buffInst.m_buff.m_defChangeP;
			mMaxHpChangeP *= 1f + buffInst.m_buff.m_hpMaxChangeP;
			mFallInjurse *= 1f + buffInst.m_buff.m_fallInjuries;
		}
	}
}
