using ItemAsset;
using SkillAsset;

public class Equipment_BuffBase : Equipment
{
	public int mBuffID;

	private EffSkillBuff mBuff;

	public override void InitEquipment(SkillRunner runner, ItemObject item)
	{
		base.InitEquipment(runner, item);
		if (mBuffID != 0)
		{
			mBuff = EffSkillBuff.s_tblEffSkillBuffs.Find((EffSkillBuff iter0) => EffSkillBuff.MatchId(iter0, mBuffID));
			if (mBuff != null)
			{
				mSkillRunner.m_effSkillBuffManager.AddBuff(mBuff);
			}
		}
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (mBuff != null)
		{
			mSkillRunner.m_effSkillBuffManager.RemoveBuff(mBuff);
		}
	}
}
