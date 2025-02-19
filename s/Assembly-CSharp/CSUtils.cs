using System.Collections.Generic;
using System.Linq;
using ItemAsset;

public class CSUtils
{
	public static void AddItemIdCount(List<ItemIdCount> list, int protoId, int count)
	{
		ItemIdCount itemIdCount = list.Find((ItemIdCount it) => it.protoId == protoId);
		if (itemIdCount == null)
		{
			list.Add(new ItemIdCount(protoId, count));
		}
		else
		{
			itemIdCount.count += count;
		}
	}

	public static void RemoveItemIdCount(List<ItemIdCount> list, int protoId, int count)
	{
		ItemIdCount itemIdCount = list.Find((ItemIdCount it) => it.protoId == protoId);
		if (itemIdCount != null)
		{
			if (itemIdCount.count <= count)
			{
				list.Remove(itemIdCount);
			}
			else
			{
				itemIdCount.count -= count;
			}
		}
	}

	public static List<ItemObject> GetItemListInStorage(int protoId, int teamId)
	{
		List<ItemObject> list = new List<ItemObject>();
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return list;
		}
		int num = 0;
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			ColonyStorage colonyStorage = item as ColonyStorage;
			list.AddRange(colonyStorage._Items.GetSameItems(protoId).ToList());
		}
		return list;
	}

	public static int GetItemCountFromAllStorage(int protoId, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			ColonyStorage colonyStorage = item as ColonyStorage;
			num += colonyStorage.GetItemCount(protoId);
		}
		return num;
	}

	public static int GetMaterialListCount(List<ItemIdCount> materialList, int teamId)
	{
		ItemIdCount itemIdCount = materialList[0];
		int num = GetItemCounFromFactoryAndAllStorage(itemIdCount.protoId, teamId) / itemIdCount.count;
		for (int i = 1; i < materialList.Count; i++)
		{
			int num2 = GetItemCounFromFactoryAndAllStorage(materialList[i].protoId, teamId) / materialList[i].count;
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static int GetItemCounFromFactoryAndAllStorage(int protoId, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1135);
		if (colonyItemsByItemId != null && colonyItemsByItemId.Count > 0)
		{
			return (colonyItemsByItemId[0] as ColonyFactory).GetCompoundEndItemCount(protoId) + GetItemCountFromAllStorage(protoId, teamId);
		}
		return GetItemCountFromAllStorage(protoId, teamId);
	}

	public static bool CountDownItemFromFacAndAllStorage(int protoId, int count, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1135);
		List<ColonyBase> colonyItemsByItemId2 = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if ((colonyItemsByItemId == null || colonyItemsByItemId.Count == 0) && (colonyItemsByItemId2 == null || colonyItemsByItemId2.Count == 0))
		{
			return false;
		}
		if (colonyItemsByItemId != null && colonyItemsByItemId.Count > 0)
		{
			ColonyFactory colonyFactory = colonyItemsByItemId[0] as ColonyFactory;
			int compoundEndItemCount = colonyFactory.GetCompoundEndItemCount(protoId);
			if (compoundEndItemCount > 0)
			{
				if (compoundEndItemCount >= count)
				{
					colonyFactory.CountDownItem(protoId, count);
					return true;
				}
				colonyFactory.CountDownItem(protoId, compoundEndItemCount);
				count -= compoundEndItemCount;
			}
		}
		if (colonyItemsByItemId2 != null && colonyItemsByItemId2.Count > 0)
		{
			foreach (ColonyBase item in colonyItemsByItemId2)
			{
				ColonyStorage colonyStorage = item as ColonyStorage;
				int itemCount = colonyStorage.GetItemCount(protoId);
				if (itemCount >= count)
				{
					colonyStorage.CountDownItem(protoId, count);
					return true;
				}
				colonyStorage.CountDownItem(protoId, itemCount);
				count -= itemCount;
			}
		}
		return false;
	}

	public static bool CountDownItemFromAllStorage(int protoId, int count, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return false;
		}
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			ColonyStorage colonyStorage = item as ColonyStorage;
			int itemCount = colonyStorage.GetItemCount(protoId);
			if (itemCount >= count)
			{
				colonyStorage.CountDownItem(protoId, count);
				return true;
			}
			colonyStorage.CountDownItem(protoId, itemCount);
			count -= itemCount;
		}
		return false;
	}

	public static bool CanAddToStorage(int protoId, int count, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return false;
		}
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			ColonyStorage colonyStorage = item as ColonyStorage;
			if (colonyStorage.CanAdd(protoId, count))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanAddListToStorage(List<ItemIdCount> iicList, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return false;
		}
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			ColonyStorage colonyStorage = item as ColonyStorage;
			if (colonyStorage.CanAdd(iicList))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AddToStorage(int protoId, int count, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return false;
		}
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			ColonyStorage colonyStorage = item as ColonyStorage;
			if (colonyStorage.CanAdd(protoId, count))
			{
				colonyStorage.Add(protoId, count);
				return true;
			}
		}
		return false;
	}

	public static bool AddItemListToStorage(List<ItemIdCount> iicList, int teamId)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(teamId, 1129);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return false;
		}
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			ColonyStorage colonyStorage = item as ColonyStorage;
			if (colonyStorage.CanAdd(iicList))
			{
				colonyStorage.Add(iicList);
				return true;
			}
		}
		return false;
	}

	public static List<ItemSample> ItemIdCountToSampleItems(List<ItemIdCount> itemList)
	{
		List<ItemSample> list = new List<ItemSample>();
		foreach (ItemIdCount item2 in itemList)
		{
			ItemSample item = new ItemSample(item2.protoId, item2.count);
			list.Add(item);
		}
		return list;
	}

	public static int[] ItemIdCountListToIntArray(List<ItemIdCount> list)
	{
		if (list == null)
		{
			return null;
		}
		int[] array = new int[list.Count * 2];
		int num = 0;
		foreach (ItemIdCount item in list)
		{
			array[num++] = item.protoId;
			array[num++] = item.count;
		}
		return array;
	}
}
