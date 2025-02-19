using System.Collections.Generic;
using UnityEngine;

namespace ItemAsset;

public abstract class Recycle : Cmpt
{
	private LifeLimit mLifeLimit;

	private Durability mDurability;

	public override void Init()
	{
		base.Init();
		mLifeLimit = itemObj.GetCmpt<LifeLimit>();
		mDurability = itemObj.GetCmpt<Durability>();
	}

	public PeFloatRangeNum GetCurrent()
	{
		if (mDurability != null)
		{
			return mDurability.floatValue;
		}
		if (mLifeLimit != null)
		{
			return mLifeLimit.floatValue;
		}
		return null;
	}

	private float GetFactor()
	{
		return GetCurrent()?.percent ?? 1f;
	}

	public abstract List<MaterialItem> GetResItemList();

	public virtual MaterialItem[] GetRecycleItems()
	{
		List<MaterialItem> resItemList = GetResItemList();
		if (resItemList == null)
		{
			return null;
		}
		List<MaterialItem> list = new List<MaterialItem>(10);
		foreach (MaterialItem item in resItemList)
		{
			list.Add(new MaterialItem
			{
				protoId = item.protoId,
				count = Mathf.CeilToInt((float)item.count * GetFactor())
			});
		}
		list.RemoveAll((MaterialItem item) => item.count <= 0);
		return list.ToArray();
	}
}
