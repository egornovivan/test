using System;
using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillAsset;

public class EffItemGot
{
	internal class RandGetItem
	{
		internal int m_id;

		internal float m_probablity;
	}

	internal class FixGetItem
	{
		internal int m_id;

		internal int m_num;
	}

	internal int m_numMax;

	internal int m_numMin;

	internal List<RandGetItem> randList = new List<RandGetItem>();

	internal List<FixGetItem> fixList = new List<FixGetItem>();

	internal static EffItemGot Create(SqliteDataReader reader)
	{
		string @string = reader.GetString(reader.GetOrdinal("_itemsGot"));
		string[] array = @string.Split(';');
		string[] array2 = array[0].Split(',');
		EffItemGot effItemGot = new EffItemGot();
		if (array2.Length >= 4 && array2.Length % 2 == 0)
		{
			effItemGot.m_numMin = Convert.ToInt32(array2[0]);
			effItemGot.m_numMax = Convert.ToInt32(array2[1]);
			int num = 2;
			while (num < array2.Length)
			{
				RandGetItem randGetItem = new RandGetItem();
				randGetItem.m_id = Convert.ToInt32(array2[num++]);
				randGetItem.m_probablity = Convert.ToSingle(array2[num++]);
				effItemGot.randList.Add(randGetItem);
			}
		}
		if (@string.Contains(";"))
		{
			string[] array3 = array[1].Split(',');
			if (array3.Length >= 2 && array3.Length % 2 == 0)
			{
				int num2 = 0;
				while (num2 < array3.Length)
				{
					FixGetItem fixGetItem = new FixGetItem();
					fixGetItem.m_id = Convert.ToInt32(array3[num2++]);
					fixGetItem.m_num = Convert.ToInt32(array3[num2++]);
					effItemGot.fixList.Add(fixGetItem);
				}
			}
		}
		return effItemGot;
	}

	public List<ItemSample> GetItemSample()
	{
		List<ItemSample> list = new List<ItemSample>(10);
		Dictionary<int, int> dictionary = new Dictionary<int, int>(10);
		int num = UnityEngine.Random.Range(m_numMin, m_numMax);
		for (int i = 0; i <= num; i++)
		{
			foreach (RandGetItem rand in randList)
			{
				if (UnityEngine.Random.value < rand.m_probablity)
				{
					if (dictionary.ContainsKey(rand.m_id))
					{
						Dictionary<int, int> dictionary2;
						Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
						int id;
						int key = (id = rand.m_id);
						id = dictionary2[id];
						dictionary3[key] = id + 1;
					}
					else
					{
						dictionary.Add(rand.m_id, 1);
					}
					break;
				}
			}
		}
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			list.Add(new ItemSample(item2.Key, item2.Value));
		}
		foreach (FixGetItem fix in fixList)
		{
			ItemSample item = new ItemSample(fix.m_id, fix.m_num);
			list.Add(item);
		}
		return list;
	}

	internal void PutIntoPack(ItemPackage pack)
	{
	}
}
