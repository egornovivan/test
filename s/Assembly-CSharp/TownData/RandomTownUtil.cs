using System;
using System.Collections.Generic;
using UnityEngine;

namespace TownData;

public class RandomTownUtil
{
	private static RandomTownUtil instance;

	public static RandomTownUtil Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new RandomTownUtil();
			}
			return instance;
		}
	}

	public IntVector3 CeilToIntVector3(Vector3 pos)
	{
		return new IntVector3(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
	}

	public Dictionary<IntVector3, B45Block> AdjustBuildingRotation(Dictionary<IntVector3, B45Block> rebuild, int rot)
	{
		return null;
	}

	public void shuffle(List<int> id, System.Random myRand)
	{
		int count = id.Count;
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			list.Add(id[i]);
		}
		int num = 0;
		while (list.Count > 0)
		{
			int index = myRand.Next(list.Count);
			id[num] = list[index];
			num++;
			list.RemoveAt(index);
		}
	}

	public TownInfo IsInTown(IntVector2 posXZ)
	{
		foreach (KeyValuePair<IntVector2, TownInfo> item in RandomTownManager.Instance.TownPosInfo)
		{
			IntVector2 posStart = item.Value.PosStart;
			IntVector2 posEnd = item.Value.PosEnd;
			if (posXZ.x >= posStart.x && posXZ.y >= posStart.y && posXZ.x <= posEnd.x && posXZ.y <= posEnd.y)
			{
				return item.Value;
			}
		}
		return null;
	}

	public TownInfo IsInTown(Vector2 posXZ)
	{
		foreach (KeyValuePair<IntVector2, TownInfo> item in RandomTownManager.Instance.TownPosInfo)
		{
			IntVector2 posStart = item.Value.PosStart;
			IntVector2 posEnd = item.Value.PosEnd;
			if (posXZ.x >= (float)posStart.x && posXZ.y >= (float)posStart.y && posXZ.x <= (float)posEnd.x && posXZ.y <= (float)posEnd.y)
			{
				return item.Value;
			}
		}
		return null;
	}
}
