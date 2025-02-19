using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class LootItemDropPeEntity
{
	private static List<PeEntity> DeathLists = new List<PeEntity>();

	public static bool HasLootEntity()
	{
		return DeathLists != null && DeathLists.Count > 0;
	}

	public static void AddPeEntity(PeEntity entity)
	{
		if (DeathLists == null)
		{
			DeathLists = new List<PeEntity>();
		}
		if (!(entity.monster == null) && !DeathLists.Contains(entity))
		{
			DeathLists.Add(entity);
		}
	}

	public static bool RemovePeEntity(PeEntity entity)
	{
		return DeathLists.Remove(entity);
	}

	public static bool ContainsPeEnity(PeEntity entity)
	{
		return DeathLists.Contains(entity);
	}

	public static List<PeEntity> GetEntities(Vector3 pos, float radiu)
	{
		List<PeEntity> list = new List<PeEntity>();
		for (int i = 0; i < DeathLists.Count; i++)
		{
			if (Match(DeathLists[i], pos, radiu))
			{
				list.Add(DeathLists[i]);
			}
		}
		return list;
	}

	public static PeEntity GetLootEntity(Vector3 pos, float radiu)
	{
		List<PeEntity> entities = GetEntities(pos, radiu);
		if (entities == null || entities.Count <= 0)
		{
			return null;
		}
		return entities[Random.Range(0, entities.Count)];
	}

	private static bool Match(PeEntity entity, Vector3 pos, float radius)
	{
		if (entity != null)
		{
			return PEUtil.SqrMagnitudeH(entity.position, pos) <= (radius + entity.maxRadius) * (radius + entity.maxRadius);
		}
		return false;
	}
}
