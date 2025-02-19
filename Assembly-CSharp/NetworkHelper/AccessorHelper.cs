using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

namespace NetworkHelper;

public static class AccessorHelper
{
	public static IEnumerable<ItemObject> GetItems(this ItemMgr mgr, int[] ids)
	{
		foreach (int id in ids)
		{
			ItemObject item = mgr.Get(id);
			if (item == null)
			{
				yield return item;
			}
		}
	}

	public static void ResetPackageItems(this ItemPackage package, int tab, int index, int id)
	{
		SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)tab);
		if (id == -1)
		{
			slotList[index] = null;
			return;
		}
		ItemObject value = PeSingleton<ItemMgr>.Instance.Get(id);
		slotList[index] = value;
	}

	public static void ResetPackageItems(this ItemPackage package, int tab, int[] ids)
	{
		SlotList slotList = package.GetSlotList((ItemPackage.ESlotType)tab);
		int num = Mathf.Min(slotList.Count, ids.Length);
		for (int i = 0; i < num; i++)
		{
			if (ids[i] == -1)
			{
				slotList[i] = null;
				continue;
			}
			ItemObject value = PeSingleton<ItemMgr>.Instance.Get(ids[i]);
			slotList[i] = value;
		}
	}
}
