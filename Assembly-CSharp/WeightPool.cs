using System;
using System.Collections.Generic;

public class WeightPool
{
	public int maxValue;

	private List<int> weightTree = new List<int>();

	private List<int> idTree = new List<int>();

	public int count;

	public void Add(int weight, int id)
	{
		maxValue += weight;
		weightTree.Add(maxValue);
		idTree.Add(id);
		count++;
	}

	public void Clear()
	{
		maxValue = 0;
		weightTree = new List<int>();
		idTree = new List<int>();
		count = 0;
	}

	public int GetRandID(Random randSeed)
	{
		if (maxValue == 0)
		{
			return -1;
		}
		int num = randSeed.Next(maxValue);
		for (int i = 0; i < count; i++)
		{
			if (num < weightTree[i])
			{
				return idTree[i];
			}
		}
		return -1;
	}

	public List<int> PickSomeId(Random randSeed, int pickAmount)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>(weightTree);
		List<int> list3 = new List<int>(idTree);
		int num = maxValue;
		int num2 = count;
		for (int i = 0; i < pickAmount; i++)
		{
			int num3 = randSeed.Next(num);
			for (int j = 0; j < num2; j++)
			{
				if (num3 < list2[j])
				{
					list.Add(list3[j]);
					int num4 = ((j != 0) ? (list2[j] - list2[j - 1]) : list2[0]);
					for (int k = j + 1; k < num2; k++)
					{
						List<int> list4;
						List<int> list5 = (list4 = list2);
						int index;
						int index2 = (index = k);
						index = list4[index];
						list5[index2] = index - num4;
					}
					list2.RemoveAt(j);
					list3.RemoveAt(j);
					num -= num4;
					num2--;
					break;
				}
			}
		}
		return list;
	}
}
