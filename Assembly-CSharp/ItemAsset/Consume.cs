using SkillSystem;

namespace ItemAsset;

public class Consume : Cmpt
{
	public SkInst StartSkSkill(SkEntity skEntity)
	{
		return itemObj.GetCmpt<Property>()?.StartSkSkill(skEntity);
	}
}
