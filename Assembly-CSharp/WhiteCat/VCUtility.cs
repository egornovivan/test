using UnityEngine;

namespace WhiteCat;

public struct VCUtility
{
	public static float GetSwordAnimSpeed(float weight)
	{
		return Mathf.Clamp(-4E-05f * weight * weight + 1.5f, 0.5f, 1.5f);
	}

	public static float GetAxeAnimSpeed(float weight)
	{
		return Mathf.Clamp(-4E-05f * weight * weight + 1.5f, 0.5f, 1.5f);
	}

	public static int GetSwordAtkSpeedTextID(float animSpeed)
	{
		float num = PEVCConfig.instance.swordAnimSpeedToASPD.Evaluate(animSpeed);
		if (num <= 0.95f)
		{
			return 8000637;
		}
		if (num >= 1.2f)
		{
			return 8000639;
		}
		return 8000638;
	}

	public static int GetAxeAtkSpeedTextID(float animSpeed)
	{
		float num = PEVCConfig.instance.axeAnimSpeedToASPD.Evaluate(animSpeed);
		if (num <= 1f)
		{
			return 8000637;
		}
		if (num >= 1.3f)
		{
			return 8000639;
		}
		return 8000638;
	}

	public static float GetArmorDefence(float durability)
	{
		float t = Mathf.Clamp01(durability / PEVCConfig.instance.maxArmorDurability);
		return Interpolation.EaseOut(t) * PEVCConfig.instance.maxArmorDefence;
	}
}
