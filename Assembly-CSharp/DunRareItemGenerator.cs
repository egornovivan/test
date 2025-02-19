using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class DunRareItemGenerator : MonoBehaviour
{
	public List<string> boxIdList = new List<string>();

	private Vector3 pos => base.transform.position;

	private void Awake()
	{
	}

	private void Start()
	{
	}

	public void GenItem(List<IdWeight> idWeightList, System.Random rand, List<ItemIdCount> specifiedItems = null)
	{
		List<int> list = RandomDunGenUtil.PickIdFromWeightList(rand, idWeightList, 1);
		RandomItemMgr.Instance.TryGenRareItem(pos, list[0], rand, specifiedItems);
	}

	public void GenItem(int id, System.Random rand, List<ItemIdCount> specifiedItem)
	{
		RandomItemMgr.Instance.TryGenRareItem(pos, id, rand, specifiedItem);
	}

	public void RandomId(out int id, List<IdWeight> idWeightList, System.Random rand)
	{
		id = -1;
		List<int> list = RandomDunGenUtil.PickIdFromWeightList(rand, idWeightList, 1);
		id = list[0];
	}

	public static void GenAllItem(List<DunRareItemGenerator> drigList, List<IdWeight> idWeightList, float chance, List<IdWeight> rareItemTags, System.Random rand, List<ItemIdCount> specifiedItems = null)
	{
		if (idWeightList == null || idWeightList.Count == 0)
		{
			return;
		}
		if (PeGameMgr.IsSingle)
		{
			int num = 0;
			foreach (DunRareItemGenerator drig in drigList)
			{
				drig.GenItem(idWeightList, rand, specifiedItems);
				specifiedItems = null;
				if (rand.NextDouble() < (double)chance)
				{
					num++;
				}
			}
			string randomIsoTag = RandomDungeonDataBase.GetRandomIsoTag(rand, rareItemTags);
			if (SteamWorkShop.Instance != null)
			{
				SteamWorkShop.Instance.GetRandIsos(RandomDungenMgrData.DungeonId, num, randomIsoTag);
			}
			return;
		}
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		int num2 = 0;
		foreach (DunRareItemGenerator drig2 in drigList)
		{
			Vector3 item = drig2.pos;
			drig2.RandomId(out var id, idWeightList, rand);
			list.Add(item);
			list2.Add(id);
			if (rand.NextDouble() < (double)chance)
			{
				num2++;
			}
		}
		List<ItemIdCount> list3 = new List<ItemIdCount>();
		if (specifiedItems != null && specifiedItems.Count > 0)
		{
			list3 = specifiedItems;
		}
		else
		{
			list3.Add(new ItemIdCount(-1, 0));
		}
		string randomIsoTag2 = RandomDungeonDataBase.GetRandomIsoTag(rand, rareItemTags);
		PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemRareAry, RandomDungenMgrData.entrancePos, num2, randomIsoTag2, list.ToArray(), list2.ToArray(), list3.ToArray());
	}
}
