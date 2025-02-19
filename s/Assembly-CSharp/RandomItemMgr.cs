using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomItemMgr
{
	public const float PASSTIME_T = 30f;

	public const float PASSDIST_T = 30f;

	public const float GEN_T = 2f;

	public const int DISTANCE_MAX = 10;

	public const int INDEX256MAX = 16;

	public const int f_INDEX256MAX = 6;

	private static RandomItemMgr mInstance;

	public Dictionary<int, List<RandomItemObj>> allRandomItems = new Dictionary<int, List<RandomItemObj>>();

	public Dictionary<IntVector2, List<RandomItemObj>> index256Items = new Dictionary<IntVector2, List<RandomItemObj>>();

	public Dictionary<Vector3, RandomItemObj> mRandomItemsDic = new Dictionary<Vector3, RandomItemObj>();

	public bool generateSwitch;

	public Vector3 born_pos;

	public Vector3 start_pos;

	public Vector3 last_pos;

	public float timeCounter;

	public float last_time;

	public float timePassed;

	public float distancePassed;

	public int counter;

	public Dictionary<IntVector2, List<RandomItemObj>> index256Feces = new Dictionary<IntVector2, List<RandomItemObj>>();

	public static RandomItemMgr Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new RandomItemMgr();
			}
			return mInstance;
		}
	}

	public RandomItemObj[] AllRandomItemObjs => mRandomItemsDic.Values.ToArray();

	public bool ContainsPos(Vector3 pos)
	{
		return mRandomItemsDic.ContainsKey(pos);
	}

	public RandomItemObj GetRandomItemObj(Vector3 pos)
	{
		if (mRandomItemsDic.ContainsKey(pos))
		{
			return mRandomItemsDic[pos];
		}
		return null;
	}

	public void RemoveRandomItemObj(RandomItemObj riObj)
	{
		if (riObj.Pos.y >= 0f)
		{
			if (allRandomItems.ContainsKey(riObj.boxId))
			{
				allRandomItems[riObj.boxId].Remove(riObj);
			}
			IntVector2 key = new IntVector2(Mathf.RoundToInt(riObj.Pos.x) >> 8, Mathf.RoundToInt(riObj.Pos.z) >> 8);
			if (index256Items.ContainsKey(key))
			{
				index256Items[key].Remove(riObj);
			}
			if (index256Feces.ContainsKey(key))
			{
				index256Feces[key].Remove(riObj);
			}
		}
		mRandomItemsDic.Remove(riObj.Pos);
	}

	public RandomItemObj AddRandomItem(int templateId, Vector3 pos, Quaternion rot, string modelPath, List<int> itemIdCount)
	{
		RandomItemObj randomItemObj = new RandomItemObj(templateId, pos, rot, itemIdCount, modelPath);
		if (pos.y >= 0f)
		{
			AddToAllItems(randomItemObj);
			AddToIndex256(randomItemObj);
		}
		if (!mRandomItemsDic.ContainsKey(pos))
		{
			mRandomItemsDic.Add(pos, randomItemObj);
		}
		return randomItemObj;
	}

	public bool IsBoxNumAvailable(int boxId)
	{
		int boxAmount = RandomItemDataMgr.GetBoxAmount(boxId);
		if (!allRandomItems.ContainsKey(boxId))
		{
			return 0 < boxAmount;
		}
		return allRandomItems[boxId].Count < boxAmount;
	}

	public bool IsBoxNumAvailable(int boxId, int boxAmount)
	{
		if (!allRandomItems.ContainsKey(boxId))
		{
			return 0 < boxAmount;
		}
		return allRandomItems[boxId].Count < boxAmount;
	}

	public bool IsAreaAvalable(Vector2 pos)
	{
		IntVector2 key = new IntVector2(Mathf.RoundToInt(pos.x) >> 8, Mathf.RoundToInt(pos.y) >> 8);
		if (index256Items.ContainsKey(key))
		{
			return index256Items[key].Count < 16;
		}
		return true;
	}

	public void AddToIndex256(RandomItemObj rio)
	{
		IntVector2 key = new IntVector2(Mathf.RoundToInt(rio.position.x) >> 8, Mathf.RoundToInt(rio.position.z) >> 8);
		if (!index256Items.ContainsKey(key))
		{
			index256Items.Add(key, new List<RandomItemObj> { rio });
		}
		else
		{
			index256Items[key].Add(rio);
		}
	}

	public void AddToAllItems(RandomItemObj rio)
	{
		if (!allRandomItems.ContainsKey(rio.boxId))
		{
			allRandomItems.Add(rio.boxId, new List<RandomItemObj> { rio });
		}
		else
		{
			allRandomItems[rio.boxId].Add(rio);
		}
	}

	public void RandomTheItems()
	{
	}

	public bool CheckGenerate(float passedTime, float passedDistance)
	{
		bool result = false;
		if (passedTime < 30f)
		{
			return result;
		}
		if (passedDistance < 30f)
		{
			return result;
		}
		if (passedTime * passedDistance / 30f / 30f > 2f)
		{
			result = true;
		}
		return result;
	}

	public List<int> RandomWeightIndex(List<int> weightList, int objCount, int pickNum)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < objCount; i++)
		{
			list.Add(i);
		}
		if (pickNum < objCount)
		{
			System.Random random = new System.Random();
			List<int> list2 = new List<int>(weightList);
			int num = 0;
			foreach (int item in list2)
			{
				num += item;
			}
			for (int j = 0; j < pickNum; j++)
			{
				int num2 = random.Next(num);
				for (int k = 0; k < list2.Count; k++)
				{
					num2 -= list2[k];
					if (num2 < 0)
					{
						list.Add(k);
						num -= list2[k];
						list2[k] = 0;
						break;
					}
				}
			}
		}
		return list;
	}

	public bool IsAreaAvalableForFeces(Vector3 pos)
	{
		IntVector2 key = new IntVector2(Mathf.RoundToInt(pos.x) >> 8, Mathf.RoundToInt(pos.y) >> 8);
		if (index256Feces.ContainsKey(key))
		{
			return index256Feces[key].Count < 6;
		}
		return true;
	}

	public bool AddFeces(Vector3 pos, Quaternion rot, int[] itemIdCount)
	{
		if (mRandomItemsDic.ContainsKey(pos))
		{
			return false;
		}
		RandomItemObj randomItemObj = new RandomItemObj(itemIdCount, pos, rot);
		mRandomItemsDic.Add(randomItemObj.position, randomItemObj);
		return true;
	}

	public void AddItemForProcessing(RandomItemObj rio)
	{
		while (mRandomItemsDic.ContainsKey(rio.position))
		{
			rio.position.y += 0.01f;
		}
		mRandomItemsDic.Add(rio.position, rio);
	}

	public void AddItemForFactoryCancel(RandomItemObj rio)
	{
		while (mRandomItemsDic.ContainsKey(rio.position))
		{
			rio.position.y += 0.01f;
		}
		mRandomItemsDic.Add(rio.position, rio);
	}
}
