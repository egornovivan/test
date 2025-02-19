using UnityEngine;

namespace ItemAsset;

public class EquipDefence : Equip
{
	public override bool ExpendDefenceDurability(float hpChange)
	{
		float value = hpChange * base.durabilityFactor;
		value = Mathf.Clamp(value, 0f, float.PositiveInfinity);
		return ChangeDurability(0f - value);
	}
}
