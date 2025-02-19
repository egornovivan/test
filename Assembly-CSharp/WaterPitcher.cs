using SkillAsset;

public class WaterPitcher : Equipment
{
	public override bool CostSkill(ISkillTarget target, int sex, bool buttonDown, bool buttonPressed)
	{
		return false;
	}
}
