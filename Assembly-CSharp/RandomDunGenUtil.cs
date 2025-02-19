using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

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

	public static List<int> PickIdFromWeightList(System.Random rand, List<IdWeight> pool, int pickAmount)
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

	public static Vector3 GetPosOnGround(IntVector2 GenPos)
	{
		int posHeight = VFDataRTGen.GetPosHeight(GenPos, inWater: true);
		Vector3 vector = new Vector3(GenPos.x, posHeight + 4, GenPos.y);
		if (Physics.Raycast(vector, Vector3.down, out var hitInfo, 512f, 4096))
		{
			if (hitInfo.point.y > 0f)
			{
				vector.y = hitInfo.point.y;
			}
			else
			{
				vector.y = posHeight;
			}
		}
		else
		{
			vector.y = posHeight;
		}
		return vector;
	}

	public static bool GetAreaLowestPos(IntVector2 centerPos, int checkDistance, out Vector3 resultPos)
	{
		resultPos = Vector3.zero;
		IntVector2 genPos = centerPos + new IntVector2(-checkDistance, 0);
		IntVector2 genPos2 = centerPos + new IntVector2(checkDistance, 0);
		IntVector2 genPos3 = centerPos + new IntVector2(0, checkDistance);
		IntVector2 genPos4 = centerPos + new IntVector2(0, -checkDistance);
		Vector3 posOnGround = GetPosOnGround(genPos);
		Vector3 posOnGround2 = GetPosOnGround(genPos2);
		Vector3 posOnGround3 = GetPosOnGround(genPos3);
		Vector3 posOnGround4 = GetPosOnGround(genPos4);
		if (posOnGround.y < 0f || posOnGround2.y < 0f || posOnGround3.y < 0f || posOnGround4.y < 0f)
		{
			return false;
		}
		float num = Mathf.Max(posOnGround.y, posOnGround2.y, posOnGround3.y, posOnGround4.y);
		float num2 = Mathf.Min(posOnGround.y, posOnGround2.y, posOnGround3.y, posOnGround4.y);
		if (num > num2 + 4f)
		{
			return false;
		}
		List<Vector3> posList = new List<Vector3>();
		posList.Add(posOnGround);
		posList.Add(posOnGround2);
		posList.Add(posOnGround3);
		posList.Add(posOnGround4);
		Vector3 vector = posList.Find(delegate(Vector3 item)
		{
			foreach (Vector3 item in posList)
			{
				if (item.y > item.y)
				{
					return false;
				}
			}
			return true;
		});
		resultPos = vector;
		return true;
	}

	public static int GetEntranceLevel(Vector3 genPos)
	{
		System.Random random = new System.Random();
		float num = (float)(VATownGenerator.Instance.GetLevelByRealPos(new IntVector2(Mathf.RoundToInt(genPos.x), Mathf.RoundToInt(genPos.z))) + 1) / 5f;
		float num2 = num * 10f;
		if (random.NextDouble() < 0.8999999761581421)
		{
			return Mathf.Clamp(Mathf.RoundToInt(num2 + (float)(2.0 * random.NextDouble() - 1.0)), 1, 10);
		}
		return Mathf.Clamp(Mathf.FloorToInt((float)random.NextDouble() * 10f) + 1, 1, 10);
	}

	public static bool IsDungeonPosY(float posY)
	{
		return posY < -100f;
	}

	public static bool IsInDungeon(PeEntity entity)
	{
		return IsDungeonPosY(entity.position.y);
	}

	public static bool IsInIronDungeon()
	{
		if (RandomDungenMgrData.dungeonBaseData == null)
		{
			Debug.LogError("IsInIronDungeon:RandomDungenMgrData.dungeonBaseData==null");
			return false;
		}
		return RandomDungenMgrData.dungeonBaseData.IsIron;
	}

	public static DungeonType GetDungeonType()
	{
		if (RandomDungenMgrData.dungeonBaseData == null)
		{
			Debug.LogError("GetDungeonType:RandomDungenMgrData.dungeonBaseData==null");
			return DungeonType.Iron;
		}
		return RandomDungenMgrData.dungeonBaseData.Type;
	}
}
