using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomDunGenMgrData
{
	public static Dictionary<Vector3, RandomDungeonData> allDungeons = new Dictionary<Vector3, RandomDungeonData>();

	public static Dictionary<int, RandomDungeonData> allIdDungeon = new Dictionary<int, RandomDungeonData>();

	public static Dictionary<IntVector2, RandomDungeonData> areaDungeons = new Dictionary<IntVector2, RandomDungeonData>();

	public static Dictionary<int, List<RandomItemObj>> rareRandomItem = new Dictionary<int, List<RandomItemObj>>();

	public static Dictionary<int, List<RandomItemObj>> finishedRareItems = new Dictionary<int, List<RandomItemObj>>();

	public static RandomDungeonData[] AllDungeons => allDungeons.Values.ToArray();

	public static void AddRareItem(int dungeonId, RandomItemObj rio)
	{
		if (!rareRandomItem.ContainsKey(dungeonId))
		{
			rareRandomItem[dungeonId] = new List<RandomItemObj>();
		}
		rareRandomItem[dungeonId].Add(rio);
	}

	public static List<RandomItemObj> GetRareItem(int dungeonId)
	{
		if (!rareRandomItem.ContainsKey(dungeonId))
		{
			return null;
		}
		return rareRandomItem[dungeonId];
	}

	public static void RemoveRareItemList(int dungeonId)
	{
		rareRandomItem.Remove(dungeonId);
	}

	public static RandomDungeonData GetDungeonData(Vector3 entrancePos)
	{
		if (allDungeons.ContainsKey(entrancePos))
		{
			return allDungeons[entrancePos];
		}
		return null;
	}

	public static void AddDungenData(Vector3 entrancePos, RandomDungeonData rdd)
	{
		allDungeons.Add(entrancePos, rdd);
		if (rdd.level < 100)
		{
			IntVector2 key = new IntVector2(Mathf.RoundToInt(entrancePos.x) >> 8, Mathf.RoundToInt(entrancePos.z) >> 8);
			areaDungeons.Add(key, rdd);
		}
		allIdDungeon.Add(rdd.id, rdd);
	}

	public static void CloseDungeon(RandomDungeonData rdd)
	{
		GameWorld gameWorld = GameWorld.GetGameWorld(RandomDunGenMgr.Instance.genWorld);
		List<Player> list = ObjNetInterface.Get<Player>();
		Player player = list.Find((Player it) => it.WorldId == RandomDunGenMgr.Instance.genWorld);
		List<AiAdNpcNetwork> list2 = ObjNetInterface.Get<AiAdNpcNetwork>();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			if (rdd.InDungeonPosY(list2[num].Pos.y))
			{
				list2[num].TransToSpawn();
			}
		}
		List<AiMonsterNetwork> list3 = ObjNetInterface.Get<AiMonsterNetwork>();
		for (int num2 = list3.Count - 1; num2 >= 0; num2--)
		{
			if (rdd.InDungeonPosY(list3[num2].Pos.y))
			{
				NetInterface.NetDestroy(list3[num2]);
			}
		}
		List<AiTowerNetwork> list4 = ObjNetInterface.Get<AiTowerNetwork>();
		for (int num3 = list4.Count - 1; num3 >= 0; num3--)
		{
			if (rdd.InDungeonPosY(list4[num3].Pos.y))
			{
				NetInterface.NetDestroy(list4[num3]);
			}
		}
		List<Vector3> list5 = new List<Vector3>();
		RandomItemObj[] allRandomItemObjs = RandomItemMgr.Instance.AllRandomItemObjs;
		RandomItemObj[] array = allRandomItemObjs;
		foreach (RandomItemObj randomItemObj in array)
		{
			if (rdd.InDungeonPosY(randomItemObj.Pos.y))
			{
				list5.Add(randomItemObj.Pos);
				RandomItemMgr.Instance.RemoveRandomItemObj(randomItemObj);
			}
		}
		if (player != null)
		{
			player.SyncDelRio(list5);
		}
		List<int> list6 = new List<int>();
		if (gameWorld != null)
		{
			SceneObject[] sceneObjs = gameWorld.GetSceneObjs();
			SceneObject[] array2 = sceneObjs;
			foreach (SceneObject sceneObject in array2)
			{
				if (rdd.InDungeonPosY(sceneObject.Pos.y) && sceneObject.WorldId == RandomDunGenMgr.Instance.genWorld)
				{
					list6.Add(sceneObject.Id);
					gameWorld.DelSceneObj(sceneObject);
				}
			}
		}
		if (player != null)
		{
			player.SyncDelSceneObjects(list6);
		}
		allDungeons.Remove(rdd.entrancePos);
		IntVector2 key = new IntVector2(Mathf.RoundToInt(rdd.entrancePos.x) >> 8, Mathf.RoundToInt(rdd.entrancePos.z) >> 8);
		areaDungeons.Remove(key);
		allIdDungeon.Remove(rdd.id);
	}

	public static void CloseDungeon(List<RandomDungeonData> rddList)
	{
		for (int i = 0; i < rddList.Count; i++)
		{
			CloseDungeon(rddList[i]);
		}
	}
}
