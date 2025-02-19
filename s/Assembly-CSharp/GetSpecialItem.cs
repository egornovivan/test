using System;
using System.Collections.Generic;
using ItemAsset;
using UnityEngine;

public class GetSpecialItem
{
	private struct SpecialItemData
	{
		public int itemID;

		public int targetItemID;

		public int maxNum;

		public int chance;

		public Vector3 pos;

		public int radius;
	}

	private static Dictionary<int, SpecialItemData> targetPlant_ItemIDMaxNumChance = new Dictionary<int, SpecialItemData>();

	private static Dictionary<int, SpecialItemData> targetMonster_ItemIDMaxNumChance = new Dictionary<int, SpecialItemData>();

	public static void AddLootSpecialItem(int targetID)
	{
		if (!MissionRepository.m_TypeCollect.ContainsKey(targetID))
		{
			return;
		}
		TypeCollectData typeCollectData = MissionRepository.m_TypeCollect[targetID];
		if (typeCollectData != null && ((typeCollectData.m_Type != 2 && typeCollectData.m_Type != 3) || !targetPlant_ItemIDMaxNumChance.ContainsKey(targetID)) && ((typeCollectData.m_Type != 1 && typeCollectData.m_Type != 3) || !targetMonster_ItemIDMaxNumChance.ContainsKey(targetID)) && typeCollectData.m_Type != 0)
		{
			SpecialItemData value = default(SpecialItemData);
			value.itemID = typeCollectData.m_ItemID;
			value.targetItemID = typeCollectData.m_TargetItemID;
			value.maxNum = typeCollectData.m_MaxNum;
			value.chance = typeCollectData.m_Chance;
			value.pos = typeCollectData.m_TargetPos;
			value.radius = typeCollectData.m_TargetRadius;
			if ((typeCollectData.m_Type == 2 || typeCollectData.m_Type == 3) && !targetPlant_ItemIDMaxNumChance.ContainsKey(targetID))
			{
				targetPlant_ItemIDMaxNumChance.Add(targetID, value);
			}
			if ((typeCollectData.m_Type == 1 || typeCollectData.m_Type == 3) && !targetMonster_ItemIDMaxNumChance.ContainsKey(targetID))
			{
				targetMonster_ItemIDMaxNumChance.Add(targetID, value);
			}
		}
	}

	public static void RemoveLootSpecialItem(int targetID)
	{
		if (targetPlant_ItemIDMaxNumChance.ContainsKey(targetID))
		{
			targetPlant_ItemIDMaxNumChance.Remove(targetID);
		}
		if (targetMonster_ItemIDMaxNumChance.ContainsKey(targetID))
		{
			targetMonster_ItemIDMaxNumChance.ContainsKey(targetID);
		}
	}

	public static bool ExistSpecialItem(AiObject entity)
	{
		if (entity is AiMonsterNetwork)
		{
			foreach (KeyValuePair<int, SpecialItemData> item in targetMonster_ItemIDMaxNumChance)
			{
				if (item.Value.targetItemID == entity.ExternId || item.Value.targetItemID == -999)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<ItemSample> MonsterItemAdd(int monsterProtoID, NetInterface caster)
	{
		if (caster == null)
		{
			return null;
		}
		List<ItemSample> list = new List<ItemSample>();
		System.Random random = new System.Random();
		foreach (KeyValuePair<int, SpecialItemData> item in targetMonster_ItemIDMaxNumChance)
		{
			if ((item.Value.targetItemID == monsterProtoID || item.Value.targetItemID == -999) && (!(item.Value.pos != Vector3.zero) || !(Vector3.Distance(caster.transform.position, item.Value.pos) > (float)item.Value.radius)) && random.Next(100) < item.Value.chance)
			{
				list.Add(new ItemSample(item.Value.itemID, random.Next(1, item.Value.maxNum + 1)));
			}
		}
		return list;
	}

	public static void PlantItemAdd(ref Dictionary<int, int> plantList, NetInterface caster)
	{
		if (caster == null)
		{
			return;
		}
		System.Random random = new System.Random();
		foreach (KeyValuePair<int, SpecialItemData> item in targetPlant_ItemIDMaxNumChance)
		{
			if ((!(item.Value.pos != Vector3.zero) || !(Vector3.Distance(caster.transform.position, item.Value.pos) > (float)item.Value.radius)) && random.Next(100) < item.Value.chance)
			{
				if (plantList.ContainsKey(item.Value.itemID))
				{
					Dictionary<int, int> dictionary;
					Dictionary<int, int> dictionary2 = (dictionary = plantList);
					int itemID;
					int key = (itemID = item.Value.itemID);
					itemID = dictionary[itemID];
					dictionary2[key] = itemID + random.Next(1, item.Value.maxNum + 1);
				}
				else
				{
					plantList[item.Value.itemID] = random.Next(1, item.Value.maxNum + 1);
				}
			}
		}
	}
}
