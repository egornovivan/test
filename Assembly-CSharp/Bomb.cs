using SkillAsset;
using WhiteCat;

public class Bomb : ShootEquipment
{
	public override bool CostSkill(ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if (buttonDown)
		{
			if (mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
			{
				return false;
			}
			EffSkillInstance effSkillInstance = null;
			switch (sex)
			{
			case 1:
				effSkillInstance = CostSkill(mSkillRunner, mSkillFemaleId[0], target);
				break;
			case 2:
				effSkillInstance = CostSkill(mSkillRunner, mSkillMaleId[0], target);
				break;
			}
			if (effSkillInstance != null)
			{
				mHuman.ApplyAmmoCost(EArmType.Bomb, mItemObj.instanceId);
			}
			return null != effSkillInstance;
		}
		return false;
	}
}
