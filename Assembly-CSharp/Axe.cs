using SkillAsset;

public class Axe : Equipment
{
	public override bool CostSkill(ISkillTarget target, int sex, bool buttonDown, bool buttonPressed)
	{
		if (buttonDown)
		{
			return base.CostSkill(target, sex, buttonDown, buttonPressed);
		}
		return false;
	}
}
