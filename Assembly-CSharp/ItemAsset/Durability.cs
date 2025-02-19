using System.Collections.Generic;
using UnityEngine;

namespace ItemAsset;

public class Durability : OneFloat
{
	public PeFloatRangeNum value => base.floatValue;

	public override float GetRawMax()
	{
		return base.protoData.durabilityMax;
	}

	public bool Expend(float deltaTime)
	{
		return base.floatValue.Change(0f - deltaTime);
	}

	public List<MaterialItem> GetRepairRequirements()
	{
		return base.protoData.repairMaterialList;
	}

	public override string ProcessTooltip(string text)
	{
		string text2 = base.ProcessTooltip(text);
		text2 = text2.Replace("$100000000$", Mathf.CeilToInt(value.current / 100f).ToString());
		return text2.Replace("$durabilityMax$", Mathf.CeilToInt(base.valueMax / 100f).ToString());
	}
}
