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
		if (itemObj.protoId < 100000000)
		{
			return null;
		}
		return base.protoData.repairMaterialList;
	}

	public override string ProcessTooltip(string text)
	{
		string text2 = base.ProcessTooltip(text);
		text2 = text2.Replace("$" + 1 + "$", ((int)lifePoint.current).ToString());
		return text2.Replace("$" + 0 + "$", ((int)base.valueMax).ToString());
	}
}
