using System.Collections.Generic;
using ItemAsset.SlotListHelper;

namespace ItemAsset;

public class Bundle : Cmpt
{
	public IEnumerable<ItemObject> Extract()
	{
		ItemProto.Bundle bundle = base.protoData.bundle;
		if (bundle == null)
		{
			return null;
		}
		List<MaterialItem> items = bundle.GetItems();
		if (items == null || items.Count == 0)
		{
			return null;
		}
		SlotList slotList = new SlotList(items.Count);
		foreach (MaterialItem item in items)
		{
			if (!slotList.CanAdd(item.protoId, item.count))
			{
				slotList = SlotList.ResetCapacity(slotList, 2 * slotList.Count);
			}
			slotList.Add(item.protoId, item.count, isNew: true);
		}
		return slotList.ToList();
	}
}
