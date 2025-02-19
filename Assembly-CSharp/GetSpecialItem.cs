using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
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

	public static void ClearLootSpecialItemRecord()
	{
		targetPlant_ItemIDMaxNumChance.Clear();
		targetMonster_ItemIDMaxNumChance.Clear();
	}

	public static void AddLootSpecialItem(int targetID)
	{
		TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(targetID);
		if (typeCollectData != null)
		{
			SpecialItemData value = default(SpecialItemData);
			value.itemID = typeCollectData.ItemID;
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
			targetMonster_ItemIDMaxNumChance.Remove(targetID);
		}
	}

	public static bool ExistSpecialItem(PeEntity entity)
	{
		if (entity.proto == EEntityProto.Monster)
		{
			foreach (KeyValuePair<int, SpecialItemData> item in targetMonster_ItemIDMaxNumChance)
			{
				if (item.Value.targetItemID == entity.ProtoID || item.Value.targetItemID == -999)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<ItemSample> MonsterItemAdd(int monsterProtoID)
	{
		List<ItemSample> list = new List<ItemSample>();
		System.Random random = new System.Random();
		foreach (KeyValuePair<int, SpecialItemData> item in targetMonster_ItemIDMaxNumChance)
		{
			if ((item.Value.targetItemID == monsterProtoID || item.Value.targetItemID == -999) && (!(item.Value.pos != Vector3.zero) || !(Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<PeTrans>().position, item.Value.pos) > (float)item.Value.radius)) && random.Next(100) < item.Value.chance)
			{
				list.Add(new ItemSample(item.Value.itemID, random.Next(1, item.Value.maxNum + 1)));
			}
		}
		return list;
	}

	public static void PlantItemAdd(ref List<int> plantList)
	{
		System.Random random = new System.Random();
		foreach (KeyValuePair<int, SpecialItemData> item in targetPlant_ItemIDMaxNumChance)
		{
			if ((!(item.Value.pos != Vector3.zero) || !(Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<PeTrans>().position, item.Value.pos) > (float)item.Value.radius)) && random.Next(100) < item.Value.chance)
			{
				plantList.Add(item.Value.itemID);
				plantList.Add(random.Next(1, item.Value.maxNum + 1));
			}
		}
	}
}
