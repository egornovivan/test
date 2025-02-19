using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using RandomItem;
using UnityEngine;

public class RandomItemMgr : MonoBehaviour
{
	private const float s_maxHeight = 999f;

	private const float a_maxHeight = 512f;

	public const float PASSTIME_T = 30f;

	public const float PASSDIST_T = 30f;

	public const float GEN_T = 2f;

	public const int DISTANCE_MAX = 10;

	public const int INDEX256MAX = 16;

	public const int GEN_RADIUS_MIN = 50;

	public const int GEN_RADIUS_MAX = 100;

	public const float f_PASSTIME_T = 60f;

	public const float f_PASSDIST_T = 60f;

	public const float f_GEN_T = 2f;

	public const int f_DISTANCE_MAX = 10;

	public const int f_INDEX256MAX = 6;

	public const int f_GEN_RADIUS_MIN = 50;

	public const int f_GEN_RADIUS_MAX = 100;

	private static RandomItemMgr mInstance;

	public Dictionary<int, List<RandomItemObj>> allRandomItems = new Dictionary<int, List<RandomItemObj>>();

	public Dictionary<IntVector2, List<RandomItemObj>> index256Items = new Dictionary<IntVector2, List<RandomItemObj>>();

	public Dictionary<Vector3, RandomItemObj> mRandomItemsDic = new Dictionary<Vector3, RandomItemObj>();

	public bool generateSwitch;

	public PeTrans playerTrans;

	public Vector3 born_pos;

	public Vector3 start_pos;

	public Vector3 last_pos;

	public float timeCounter;

	public float last_time;

	public float timePassed;

	public float distancePassed;

	public int counter;

	public Dictionary<IntVector2, List<RandomItemObj>> index256Feces = new Dictionary<IntVector2, List<RandomItemObj>>();

	public bool f_generateSwitch;

	public Vector3 f_start_pos;

	public Vector3 f_last_pos;

	public float f_timeCounter;

	public float f_last_time;

	public float f_timePassed;

	public float f_distancePassed;

	public int f_counter;

	public static RandomItemMgr Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void Update()
	{
		if ((PeGameMgr.IsAdventure && PeGameMgr.yirdName != AdventureScene.Dungen.ToString()) || PeGameMgr.IsMultiAdventure || PeGameMgr.IsMultiBuild)
		{
			counter++;
			timeCounter += Time.deltaTime;
			if (counter > 240)
			{
				if (playerTrans == null)
				{
					if (PeSingleton<PeCreature>.Instance.mainPlayer != null && PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position != Vector3.zero)
					{
						playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
						born_pos = playerTrans.position;
						if (Application.isEditor)
						{
							Debug.Log(string.Concat("<color=yellow>born_pos", born_pos, "</color>"));
						}
						start_pos = born_pos;
						last_pos = start_pos;
						last_time = timeCounter;
					}
					counter = 0;
					return;
				}
				if (PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position.y < -100f)
				{
					return;
				}
				if (!generateSwitch)
				{
					timePassed += timeCounter - last_time;
					float num = Vector2.Distance(new Vector2(playerTrans.position.x, playerTrans.position.z), new Vector2(last_pos.x, last_pos.z));
					if (num > 10f)
					{
						num = 10f;
					}
					distancePassed += num;
					last_pos = playerTrans.position;
					last_time = timeCounter;
					if (!CheckGenerate(timePassed, distancePassed))
					{
						counter = 0;
						return;
					}
					generateSwitch = true;
					timePassed = 0f;
					distancePassed = 0f;
				}
				if (generateSwitch)
				{
					System.Random random = new System.Random((int)DateTime.UtcNow.Ticks);
					int num2 = 50;
					int num3 = random.Next(num2 * 2) - num2;
					int num4 = random.Next(num2 * 2) - num2;
					num3 = ((num3 < 0) ? (num3 - 50 + 1) : (num3 + 50));
					num4 = ((num4 < 0) ? (num4 - 50 + 1) : (num4 + 50));
					IntVector2 intVector = new IntVector2((int)playerTrans.position.x + num3, (int)playerTrans.position.z + num4);
					if (IsAreaAvalable(intVector) && IsTerrainAvailable(intVector, out var pos, out var boxId))
					{
						TryGenItem(pos, boxId);
						generateSwitch = false;
					}
				}
				counter = 0;
			}
		}
		if ((!PeGameMgr.IsAdventure || !(PeGameMgr.yirdName != AdventureScene.Dungen.ToString())) && !PeGameMgr.IsStory)
		{
			return;
		}
		f_counter++;
		f_timeCounter += Time.deltaTime;
		if (f_counter <= 60)
		{
			return;
		}
		if (playerTrans == null)
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer != null && PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position != Vector3.zero)
			{
				playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
				born_pos = playerTrans.position;
				if (Application.isEditor)
				{
					Debug.Log(string.Concat("<color=yellow>fecesborn_pos", born_pos, "</color>"));
				}
				f_start_pos = born_pos;
				f_last_pos = f_start_pos;
				f_last_time = f_timeCounter;
			}
			f_counter = 0;
		}
		else
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position.y < -100f)
			{
				return;
			}
			if (!f_generateSwitch)
			{
				f_timePassed += f_timeCounter - f_last_time;
				float num5 = Vector2.Distance(new Vector2(playerTrans.position.x, playerTrans.position.z), new Vector2(f_last_pos.x, f_last_pos.z));
				if (num5 > 10f)
				{
					num5 = 10f;
				}
				f_distancePassed += num5;
				f_last_pos = playerTrans.position;
				f_last_time = f_timeCounter;
				if (!CheckGenerateForFeces(f_timePassed, f_distancePassed))
				{
					f_counter = 0;
					return;
				}
				f_generateSwitch = true;
				f_timePassed = 0f;
				f_distancePassed = 0f;
			}
			if (f_generateSwitch)
			{
				System.Random random2 = new System.Random((int)DateTime.UtcNow.Ticks);
				int num6 = 50;
				int num7 = random2.Next(num6 * 2) - num6;
				int num8 = random2.Next(num6 * 2) - num6;
				num7 = ((num7 < 0) ? (num7 - 50 + 1) : (num7 + 50));
				num8 = ((num8 < 0) ? (num8 - 50 + 1) : (num8 + 50));
				IntVector2 intVector2 = new IntVector2((int)playerTrans.position.x + num7, (int)playerTrans.position.z + num8);
				if (IsAreaAvalableForFeces(intVector2) && IsTerrainAvailableForFeces(intVector2, out var pos2))
				{
					TryGenFeces(pos2);
					f_generateSwitch = false;
				}
			}
			f_counter = 0;
		}
	}

	public void TryGenItem(Vector3 pos, int boxId, System.Random rand = null)
	{
		if (ContainsPos(pos))
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItem, pos, boxId);
			return;
		}
		string path;
		List<ItemIdCount> list = RandomItemDataMgr.GenItemDicByBoxId(boxId, out path, rand);
		if (list == null)
		{
			Debug.LogError("boxId error: " + boxId);
			return;
		}
		List<ItemIdCount> list2 = list.FindAll((ItemIdCount it) => it.protoId <= 0 || it.count <= 0 || PeSingleton<ItemProto.Mgr>.Instance.Get(it.protoId) == null);
		if (list2 != null && list2.Count > 0)
		{
			foreach (ItemIdCount item in list2)
			{
				Debug.LogError("randomItem error:" + item.protoId + " " + item.count);
				list.Remove(item);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError("empty boxId:" + boxId);
			return;
		}
		int[] array = new int[list.Count * 2];
		int num = 0;
		foreach (ItemIdCount item2 in list)
		{
			array[num++] = item2.protoId;
			array[num++] = item2.count;
		}
		RandomItemObj randomItemObj = new RandomItemObj(boxId, pos, array, path);
		if (pos.y >= 0f)
		{
			AddToAllItems(randomItemObj);
			AddToIndex256(randomItemObj);
		}
		mRandomItemsDic.Add(pos, randomItemObj);
	}

	public bool IsTerrainAvailable(IntVector2 GenPos, out Vector3 pos, out int boxId)
	{
		int posHeight = VFDataRTGen.GetPosHeight(GenPos.x, GenPos.y, inWater: true);
		bool flag = VFDataRTGen.IsSea(posHeight);
		bool flag2 = false;
		pos = new Vector3(GenPos.x, posHeight - 2, GenPos.y);
		if (Physics.Raycast(pos, Vector3.down, out var _, 512f, 4096))
		{
			flag2 = true;
		}
		RandomMapType xZMapType = VFDataRTGen.GetXZMapType(GenPos.x, GenPos.x);
		IntVector2 intVector = new IntVector2(GenPos.x >> 5, GenPos.y >> 5);
		VArtifactTown vArtifactTown = null;
		if (VArtifactTownManager.Instance != null)
		{
			for (int i = -4; i < 5; i++)
			{
				for (int j = -4; j < 5; j++)
				{
					vArtifactTown = VArtifactTownManager.Instance.GetTileTown(new IntVector2(intVector.x + i, intVector.y + j));
					if (vArtifactTown != null)
					{
						break;
					}
				}
				if (vArtifactTown != null)
				{
					break;
				}
			}
		}
		List<int> list = new List<int>();
		if (vArtifactTown != null)
		{
			if (vArtifactTown.type == VArtifactType.NpcTown)
			{
				list.Add(1);
			}
			else
			{
				list.Add(2);
			}
		}
		switch (xZMapType)
		{
		case RandomMapType.Desert:
			list.Add(4);
			break;
		case RandomMapType.Redstone:
			list.Add(5);
			break;
		default:
			list.Add(3);
			break;
		}
		if (flag)
		{
			list.Add(6);
		}
		if (flag2)
		{
			list.Add(7);
		}
		boxId = -1;
		List<RandomItemBoxInfo> boxIdByCondition = RandomItemDataMgr.GetBoxIdByCondition(list, posHeight);
		if (boxIdByCondition == null || boxIdByCondition.Count == 0)
		{
			return false;
		}
		List<RandomItemBoxInfo> list2 = new List<RandomItemBoxInfo>();
		foreach (RandomItemBoxInfo item in boxIdByCondition)
		{
			if (IsBoxNumAvailable(item.boxId, item.boxAmount))
			{
				list2.Add(item);
			}
		}
		if (list2.Count == 0)
		{
			return false;
		}
		RandomItemBoxInfo randomItemBoxInfo = list2[new System.Random((int)DateTime.UtcNow.Ticks).Next(list2.Count)];
		boxId = randomItemBoxInfo.boxId;
		float num = randomItemBoxInfo.boxDepth;
		if (num <= 0f)
		{
			num = -3f;
		}
		else if (num < 2f)
		{
			num = 2f;
		}
		pos = new Vector3(GenPos.x, (float)posHeight - num, GenPos.y);
		RaycastHit hitInfo2;
		if (pos.y < 0f)
		{
			pos.y = 0f;
		}
		else if (Physics.Raycast(pos, Vector3.down, out hitInfo2, 512f, 4096))
		{
			pos.y = hitInfo2.point.y - randomItemBoxInfo.boxDepth;
		}
		if (pos.y < 0f)
		{
			return false;
		}
		return true;
	}

	public bool IsBoxNumAvailable(int boxId)
	{
		int boxAmount = RandomItemDataMgr.GetBoxAmount(boxId);
		if (!allRandomItems.ContainsKey(boxId))
		{
			return 0 < boxAmount;
		}
		return allRandomItems[boxId].Count < boxAmount;
	}

	public bool IsBoxNumAvailable(int boxId, int boxAmount)
	{
		if (!allRandomItems.ContainsKey(boxId))
		{
			return 0 < boxAmount;
		}
		return allRandomItems[boxId].Count < boxAmount;
	}

	public bool IsAreaAvalable(Vector2 pos)
	{
		IntVector2 key = new IntVector2(Mathf.RoundToInt(pos.x) >> 8, Mathf.RoundToInt(pos.y) >> 8);
		if (index256Items.ContainsKey(key))
		{
			return index256Items[key].Count < 16;
		}
		return true;
	}

	public bool ContainsPos(Vector3 pos)
	{
		return mRandomItemsDic.ContainsKey(pos);
	}

	public RandomItemObj GetRandomItemObj(Vector3 pos)
	{
		if (mRandomItemsDic.ContainsKey(pos))
		{
			return mRandomItemsDic[pos];
		}
		return null;
	}

	public void RemoveRandomItemObj(RandomItemObj riObj)
	{
		if (allRandomItems.ContainsKey(riObj.boxId))
		{
			allRandomItems[riObj.boxId].Remove(riObj);
		}
		IntVector2 key = new IntVector2(Mathf.RoundToInt(riObj.Pos.x) >> 8, Mathf.RoundToInt(riObj.Pos.z) >> 8);
		if (index256Items.ContainsKey(key))
		{
			index256Items[key].Remove(riObj);
		}
		if (index256Feces.ContainsKey(key))
		{
			index256Feces[key].Remove(riObj);
		}
		mRandomItemsDic.Remove(riObj.Pos);
	}

	public void AddToIndex256(RandomItemObj rio)
	{
		IntVector2 key = new IntVector2(Mathf.RoundToInt(rio.position.x) >> 8, Mathf.RoundToInt(rio.position.z) >> 8);
		if (!index256Items.ContainsKey(key))
		{
			index256Items.Add(key, new List<RandomItemObj> { rio });
		}
		else
		{
			index256Items[key].Add(rio);
		}
	}

	public void AddToAllItems(RandomItemObj rio)
	{
		if (!allRandomItems.ContainsKey(rio.boxId))
		{
			allRandomItems.Add(rio.boxId, new List<RandomItemObj> { rio });
		}
		else
		{
			allRandomItems[rio.boxId].Add(rio);
		}
	}

	public void RandomTheItems()
	{
	}

	public bool CheckGenerate(float passedTime, float passedDistance)
	{
		bool result = false;
		if (passedTime < 30f)
		{
			return result;
		}
		if (passedDistance < 30f)
		{
			return result;
		}
		if (passedTime * passedDistance / 30f / 30f > 2f)
		{
			result = true;
		}
		return result;
	}

	public List<int> RandomWeightIndex(List<int> weightList, int objCount, int pickNum)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < objCount; i++)
		{
			list.Add(i);
		}
		if (pickNum < objCount)
		{
			System.Random random = new System.Random((int)DateTime.UtcNow.Ticks);
			List<int> list2 = new List<int>(weightList);
			int num = 0;
			foreach (int item in list2)
			{
				num += item;
			}
			for (int j = 0; j < pickNum; j++)
			{
				int num2 = random.Next(num);
				for (int k = 0; k < list2.Count; k++)
				{
					num2 -= list2[k];
					if (num2 < 0)
					{
						list.Add(k);
						num -= list2[k];
						list2[k] = 0;
						break;
					}
				}
			}
		}
		return list;
	}

	public void AddItmeResult(Vector3 pos, Quaternion rot, int templateId, int[] itemIdNum, string path)
	{
		RandomItemObj randomItemObj = new RandomItemObj(templateId, pos, rot, itemIdNum, path);
		if (randomItemObj.Pos.y > 0f)
		{
			AddToAllItems(randomItemObj);
			AddToIndex256(randomItemObj);
		}
		mRandomItemsDic[randomItemObj.position] = randomItemObj;
	}

	public void AddRareItmeResult(Vector3 pos, Quaternion rot, int templateId, int[] itemIdNum, string path)
	{
		RandomItemObj randomItemObj = new RandomItemObj(templateId, pos, rot, itemIdNum, path);
		randomItemObj.AddRareProto(281, 1);
		mRandomItemsDic[randomItemObj.position] = randomItemObj;
		if (Application.isEditor)
		{
			Debug.LogError(string.Concat("<color=yellow>A Rare RandomItem is Added!", pos, " </color>"));
		}
	}

	public void TryGenRareItem(Vector3 pos, int boxId, System.Random rand = null, List<ItemIdCount> specifiedItems = null)
	{
		if (ContainsPos(pos))
		{
			return;
		}
		string path;
		List<ItemIdCount> list = RandomItemDataMgr.GenItemDicByBoxId(boxId, out path, rand);
		if (list == null)
		{
			Debug.LogError("boxId error: " + boxId);
			list.Add(new ItemIdCount(1, 1));
		}
		if (specifiedItems != null)
		{
			list.AddRange(specifiedItems);
		}
		List<ItemIdCount> list2 = list.FindAll((ItemIdCount it) => it.protoId <= 0 || it.count <= 0 || PeSingleton<ItemProto.Mgr>.Instance.Get(it.protoId) == null);
		if (list2 != null && list2.Count > 0)
		{
			foreach (ItemIdCount item in list2)
			{
				Debug.LogError("randomItem error:" + item.protoId + " " + item.count);
				list.Remove(item);
			}
		}
		int[] array = new int[list.Count * 2];
		int num = 0;
		foreach (ItemIdCount item2 in list)
		{
			array[num++] = item2.protoId;
			array[num++] = item2.count;
		}
		RandomItemObj randomItemObj = new RandomItemObj(boxId, pos, array, path);
		if (pos.y >= 0f)
		{
			AddToAllItems(randomItemObj);
			AddToIndex256(randomItemObj);
		}
		randomItemObj.AddRareProto(281, 5);
		mRandomItemsDic.Add(pos, randomItemObj);
		RandomDungenMgrData.AddRareItem(randomItemObj);
	}

	public ProcessingResultObj GenProcessingItem(Vector3 pos, int[] items)
	{
		ProcessingResultObj processingResultObj = new ProcessingResultObj(pos, items);
		processingResultObj.TryGenObject();
		return processingResultObj;
	}

	public ProcessingResultObj GenProcessingItem(Vector3 pos, Quaternion rot, int[] items)
	{
		ProcessingResultObj processingResultObj = new ProcessingResultObj(pos, rot, items);
		processingResultObj.TryGenObject();
		return processingResultObj;
	}

	public void AddItemToManager(RandomItemObj rio)
	{
		if (!mRandomItemsDic.ContainsKey(rio.position))
		{
			mRandomItemsDic.Add(rio.position, rio);
		}
	}

	public bool IsTerrainAvailableForFeces(IntVector2 GenPos, out Vector3 pos)
	{
		pos = Vector3.zero;
		if (AIErodeMap.IsInErodeArea2D(new Vector3(GenPos.x, 0f, GenPos.y)) != null)
		{
			return false;
		}
		if (PeGameMgr.IsStory)
		{
			Vector3 origin = new Vector3(GenPos.x, 999f, GenPos.y);
			if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 999f, 4096))
			{
				pos = hitInfo.point + new Vector3(0f, 1f, 0f);
				if (!VFVoxelWater.self.IsInWater(pos) && !mRandomItemsDic.ContainsKey(pos))
				{
					return true;
				}
			}
			return false;
		}
		if (PeGameMgr.IsAdventure)
		{
			int posHeight = VFDataRTGen.GetPosHeight(GenPos.x, GenPos.y, inWater: true);
			pos = new Vector3(GenPos.x, posHeight, GenPos.y);
			if (!VFDataRTGen.IsSea(posHeight))
			{
				if (Physics.Raycast(pos + new Vector3(0f, 2f, 0f), Vector3.down, out var hitInfo2, 512f, 4096))
				{
					pos = hitInfo2.point;
				}
				pos.y += 1f;
				if (!mRandomItemsDic.ContainsKey(pos))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAreaAvalableForFeces(Vector2 pos)
	{
		IntVector2 key = new IntVector2(Mathf.RoundToInt(pos.x) >> 8, Mathf.RoundToInt(pos.y) >> 8);
		if (index256Feces.ContainsKey(key))
		{
			return index256Feces[key].Count < 6;
		}
		return true;
	}

	public bool CheckGenerateForFeces(float passedTime, float passedDistance)
	{
		bool result = false;
		if (passedTime < 60f)
		{
			return result;
		}
		if (passedDistance < 60f)
		{
			return result;
		}
		if (passedTime * passedDistance / 60f / 60f > 2f)
		{
			result = true;
		}
		return result;
	}

	public void TryGenFeces(Vector3 pos)
	{
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomFeces, pos);
			return;
		}
		string modelPath;
		int[] itemIdNum = RandomFecesDataMgr.GenFecesItemIdCount(out modelPath);
		new RandomItemObj("feces", pos, itemIdNum, Quaternion.Euler(0f, new System.Random((int)DateTime.UtcNow.Ticks).Next(360), 0f), modelPath);
		if (Application.isEditor)
		{
			Debug.Log(string.Concat("<color=brown>A RandomFeces is Added!", pos, " </color>"));
		}
	}

	public void AddFeces(RandomItemObj rio)
	{
		if (!mRandomItemsDic.ContainsKey(rio.position))
		{
			mRandomItemsDic.Add(rio.position, rio);
			IntVector2 key = new IntVector2(Mathf.RoundToInt(rio.position.x) >> 8, Mathf.RoundToInt(rio.position.z) >> 8);
			if (!index256Feces.ContainsKey(key))
			{
				index256Feces[key] = new List<RandomItemObj>();
			}
			index256Feces[key].Add(rio);
		}
	}

	public void AddFecesResult(Vector3 pos, Quaternion rot, int[] itemIdNum)
	{
		string modelPath = FecesData.GetModelPath((int)(pos.x + pos.y + pos.z));
		new RandomItemObj("feces", pos, itemIdNum, rot, modelPath);
	}

	public void GenFactoryCancel(Vector3 pos, int[] items)
	{
		ProcessingResultObj processingResultObj = new ProcessingResultObj(pos, items);
		processingResultObj.TryGenObject();
	}

	public void GenFactoryCancel(Vector3 pos, Quaternion rot, int[] items)
	{
		ProcessingResultObj processingResultObj = new ProcessingResultObj(pos, rot, items);
		processingResultObj.TryGenObject();
	}
}
