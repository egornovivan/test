using System;
using System.Collections.Generic;

public class RandomDunGenUtil
{
	public static IdWeight GetIdWeightFromStr(string str)
	{
		string[] array = str.Split(',');
		int idP = Convert.ToInt32(array[0]);
		int weightP = Convert.ToInt32(array[1]);
		return new IdWeight(idP, weightP);
	}

	public static List<IdWeight> GetIdWeightList(string str)
	{
		List<IdWeight> list = new List<IdWeight>();
		if (str == "0")
		{
			return list;
		}
		string[] array = str.Split(';');
		string[] array2 = array;
		foreach (string str2 in array2)
		{
			IdWeight idWeightFromStr = GetIdWeightFromStr(str2);
			list.Add(idWeightFromStr);
		}
		return list;
	}

	public static IdWeight GetChanceWeightFromStr(string str)
	{
		string[] array = str.Split(',');
		int idP = Convert.ToInt32(array[0]);
		int weightP = Convert.ToInt32(array[1]);
		return new IdWeight(idP, weightP);
	}

	public static List<int> PickIdFromWeightList(Random rand, List<IdWeight> pool, int pickAmount)
	{
		WeightPool weightPool = new WeightPool();
		foreach (IdWeight item in pool)
		{
			weightPool.Add(item.weight, item.id);
		}
		return weightPool.PickSomeId(rand, pickAmount);
	}

	public static List<IdWeight> GenIdWeight(List<int> weightList, List<int> idList = null)
	{
		List<IdWeight> list = new List<IdWeight>();
		if (idList == null)
		{
			for (int i = 0; i < weightList.Count; i++)
			{
				list.Add(new IdWeight(i, weightList[i]));
			}
		}
		else
		{
			for (int j = 0; j < weightList.Count; j++)
			{
				list.Add(new IdWeight(idList[j], weightList[j]));
			}
		}
		return list;
	}
}
