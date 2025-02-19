using SkillSystem;

namespace ItemAsset;

public class EquipAttack : Equip
{
	public override bool ExpendAttackDurability(SkEntity skEntity)
	{
		return ChangeDurability(skEntity, 0f - base.durabilityFactor);
	}
}
