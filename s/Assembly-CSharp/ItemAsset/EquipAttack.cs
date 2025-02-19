namespace ItemAsset;

public class EquipAttack : Equip
{
	public override bool ExpendAttackDurability()
	{
		return ChangeDurability(0f - base.durabilityFactor);
	}
}
