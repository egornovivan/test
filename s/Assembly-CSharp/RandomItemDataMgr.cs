using System;
using System.Collections.Generic;
using RandomItem;

public class RandomItemDataMgr
{
	private const string testPath = "Prefab/Item/Scene/randomitem_test";

	public static int GetBoxAmount(int boxId)
	{
		return RandomItemBoxInfo.GetBoxInfoById(boxId)?.boxAmount ?? 0;
	}

	public static List<RandomItemBoxInfo> GetBoxIdByCondition(List<int> conditionList, int height)
	{
		return RandomItemBoxInfo.RandomBoxMatchCondition(conditionList, height);
	}

	public static List<ItemIdCount> GenItemDicByBoxId(int boxId, out string path, Random rand = null)
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		list.Add(new ItemIdCount(1, 1));
		path = "Prefab/Item/Scene/randomitem_test";
		RandomItemBoxInfo boxInfoById = RandomItemBoxInfo.GetBoxInfoById(boxId);
		if (boxInfoById == null)
		{
			return null;
		}
		path = boxInfoById.boxModelPath;
		RandomItemRulesInfo ruleInfoById = RandomItemRulesInfo.GetRuleInfoById(boxInfoById.rulesId);
		if (ruleInfoById == null)
		{
			return null;
		}
		if (rand == null)
		{
			rand = new Random((int)DateTime.UtcNow.Ticks);
		}
		int count = rand.Next(boxInfoById.boxItemAmountMin, boxInfoById.boxItemAmountMax + 1);
		return ruleInfoById.RandomItemDict(count, rand);
	}

	public static void LoadData()
	{
		RandomItemBoxInfo.LoadData();
		RandomItemRulesInfo.LoadData();
		RandomItemTypeInfo.LoadData();
	}
}
