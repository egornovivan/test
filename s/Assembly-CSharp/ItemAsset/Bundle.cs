using System.Collections.Generic;

namespace ItemAsset;

public class Bundle : Cmpt
{
	public List<ItemSample> Extract()
	{
		List<ItemSample> list = new List<ItemSample>();
		ItemProto.Bundle bundle = base.protoData.bundle;
		if (bundle == null)
		{
			return list;
		}
		List<MaterialItem> items = bundle.GetItems();
		if (items == null || items.Count == 0)
		{
			return list;
		}
		foreach (MaterialItem item in items)
		{
			list.Add(new ItemSample(item.protoId, item.count));
		}
		return list;
	}
}
