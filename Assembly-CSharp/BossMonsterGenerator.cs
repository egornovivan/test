using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class BossMonsterGenerator : MonoBehaviour
{
	private Vector3 pos => base.transform.position;

	public void GenBoss(List<IdWeight> landMonsterId, List<IdWeight> waterMonsterId, int buff, System.Random rand, bool isTaskDungeon)
	{
		List<IdWeight> list = new List<IdWeight>();
		if (VFVoxelWater.self != null)
		{
			if (VFVoxelWater.self.IsInWater(pos))
			{
				if (VFVoxelWater.self.IsInWater(pos + new Vector3(0f, 1f, 0f)))
				{
					if (!VFVoxelWater.self.IsInWater(pos + new Vector3(0f, 4f, 0f)))
					{
						return;
					}
					list = waterMonsterId;
				}
				else
				{
					list = landMonsterId;
				}
			}
			else
			{
				list = landMonsterId;
			}
		}
		if (list != null && list.Count != 0)
		{
			List<int> list2 = RandomDunGenUtil.PickIdFromWeightList(rand, list, 1);
			PeEntity peEntity = MonsterEntityCreator.CreateDungeonMonster(list2[0], pos, RandomDungenMgrData.DungeonId, buff);
		}
	}

	public static void GenAllBoss(List<BossMonsterGenerator> allBoss, List<IdWeight> landList, List<IdWeight> waterList, int buff, System.Random rand, bool isTaskDungeon)
	{
		foreach (BossMonsterGenerator item in allBoss)
		{
			item.GenBoss(landList, waterList, buff, rand, isTaskDungeon);
		}
	}
}
