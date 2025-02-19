using System.Collections;
using System.Collections.Generic;
using CustomCharactor;
using Pathea;
using RandomItem;
using UnityEngine;

public static class AssetsPool
{
	public static readonly int s_Version;

	private static readonly string[] s_PooledPrefabs;

	private static Dictionary<string, Object> s_CacheMap;

	static AssetsPool()
	{
		s_Version = 108;
		s_PooledPrefabs = new string[22]
		{
			"Prefabs/CursorState", "Prefab/Particle/dengshuangzhu/Male_RunSwordAttack", "Prefab/Particle/dengshuangzhu/Female_RunSwordAttack", "Prefab/Particle/fx_grasslandAmbient01", "Prefab/Particle/FX_insects_01", "Prefab/Particle/FX_walkingOnDirt", "Prefab/Particle/FX_walkingOnGrass", "Prefab/Particle/FX_enemyFall", "Prefab/Particle/FX_enemyFall_large", "Prefab/Particle/FX_enemyHit_critical",
			"EntityPlayer", "EntityNpc", "EntityNpcNative", "EntityTower", "EntityDoodad", "EntityMonster", "EntityGroup", "Prefab/Npc/Component/OverHead", "Prefab/Item/Equip/Weapon/Other/Gloves", "AiPrefab/MonsterSpecialItem/Monster_feces01",
			"AiPrefab/MonsterSpecialItem/Monster_feces02", "AiPrefab/MonsterSpecialItem/Monster_feces03"
		};
		s_CacheMap = new Dictionary<string, Object>();
	}

	public static void PreLoad()
	{
		if (s_CacheMap.Count == 0)
		{
			int num = s_PooledPrefabs.Length;
			for (int i = 0; i < num; i++)
			{
				s_CacheMap[s_PooledPrefabs[i]] = Resources.Load(s_PooledPrefabs[i]);
			}
			AvatarCmpt.CachePrefab();
			NpcProtoDb.CachePrefab();
			CustomMetaData.CachePrefab();
			RandomItemBoxInfo.CachePrefab();
		}
	}

	public static void RegisterAsset(string pathName, Object obj)
	{
		if (!s_CacheMap.ContainsKey(pathName))
		{
			s_CacheMap.Add(pathName, obj);
		}
	}

	public static bool TryGetAsset(string pathName, out Object asset)
	{
		return s_CacheMap.TryGetValue(pathName, out asset);
	}

	public static void Clear()
	{
		s_CacheMap.Clear();
	}

	public static IEnumerator Cleanup()
	{
		while (true)
		{
			yield return new WaitForSeconds(180f);
		}
	}
}
