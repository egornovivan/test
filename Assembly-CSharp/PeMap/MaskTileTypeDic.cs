using System.Collections.Generic;

namespace PeMap;

public class MaskTileTypeDic
{
	private Dictionary<int, int> countDic = new Dictionary<int, int>();

	public void Reset()
	{
		for (int i = 0; i < 10; i++)
		{
			countDic[i] = 0;
		}
	}

	public void CountType(MaskTileType type)
	{
		Dictionary<int, int> dictionary;
		Dictionary<int, int> dictionary2 = (dictionary = countDic);
		int key;
		int key2 = (key = (int)type);
		key = dictionary[key];
		dictionary2[key2] = key + 1;
	}

	public MaskTileType GetMostType()
	{
		MaskTileType result = MaskTileType.GrassLand;
		int num = 0;
		foreach (KeyValuePair<int, int> item in countDic)
		{
			if (item.Value > num)
			{
				result = (MaskTileType)item.Key;
				num = item.Value;
			}
		}
		return result;
	}
}
