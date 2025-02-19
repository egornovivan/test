using System.Collections.Generic;

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
		if (itemObj.protoId >= CreationData.ObjectStartID)
		{
			return null;
		}
		return base.protoData.repairMaterialList;
	}
}
