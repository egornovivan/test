using System.Collections.Generic;
using UnityEngine;

namespace ItemAsset;

public class Repair : Cmpt
{
	private LifeLimit mLifeLimit;

	private Durability mDurability;

	public override void Init()
	{
		base.Init();
		mLifeLimit = itemObj.GetCmpt<LifeLimit>();
		mDurability = itemObj.GetCmpt<Durability>();
		itemObj.SetPriceFactor(GetValue());
	}

	private static void AddRequirements(ref List<MaterialItem> retRequirements, List<MaterialItem> baseRequirements, float factor)
	{
		if (factor - 1f > float.Epsilon || factor < float.Epsilon)
		{
			return;
		}
		if (baseRequirements == null)
		{
			return;
		}
		foreach (MaterialItem baseRequirement in baseRequirements)
		{
			retRequirements.Add(new MaterialItem
			{
				protoId = baseRequirement.protoId,
				count = Mathf.CeilToInt((float)baseRequirement.count * factor)
			});
		}
		retRequirements.RemoveAll((MaterialItem item) => item.count <= 0);
	}

	public List<MaterialItem> GetRequirements()
	{
		List<MaterialItem> retRequirements = new List<MaterialItem>(10);
		if (mLifeLimit != null)
		{
			AddRequirements(ref retRequirements, mLifeLimit.GetRepairRequirements(), 1f - mLifeLimit.lifePoint.percent);
		}
		if (mDurability != null)
		{
			AddRequirements(ref retRequirements, mDurability.GetRepairRequirements(), 1f - mDurability.value.percent);
		}
		return retRequirements;
	}

	public PeFloatRangeNum GetValue()
	{
		if (mDurability != null)
		{
			return mDurability.value;
		}
		if (mLifeLimit != null)
		{
			return mLifeLimit.lifePoint;
		}
		return null;
	}

	public void Do()
	{
		if (mLifeLimit != null)
		{
			mLifeLimit.lifePoint.SetToMax();
		}
		if (mDurability != null)
		{
			mDurability.value.SetToMax();
		}
	}
}
