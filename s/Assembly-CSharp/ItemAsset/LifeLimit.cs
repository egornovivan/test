using System.Collections.Generic;
using Pathea;

namespace ItemAsset;

public class LifeLimit : OneFloat
{
	public PeFloatRangeNum lifePoint => base.floatValue;

	public override float GetRawMax()
	{
		return itemObj.protoData.propertyList.GetProperty(AttribType.HpMax);
	}

	public bool ExpendLife(float deltaPoint)
	{
		return lifePoint.Change(0f - deltaPoint);
	}

	public virtual void Revive()
	{
		lifePoint.SetToMax();
	}

	public List<MaterialItem> GetRepairRequirements()
	{
		if (itemObj.protoId < CreationData.ObjectStartID)
		{
			return null;
		}
		return base.protoData.repairMaterialList;
	}
}
