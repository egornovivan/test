using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ColonyProcessing : ColonyBase
{
	private CSProcessingData _MyData;

	public override int MaxWorkerCount => 5;

	public bool IsAuto
	{
		get
		{
			return _MyData.isAuto;
		}
		set
		{
			_MyData.isAuto = value;
		}
	}

	public ProcessingTask[] mTaskTable
	{
		get
		{
			return _MyData.mTaskTable;
		}
		set
		{
			_MyData.mTaskTable = value;
		}
	}

	public ColonyProcessing(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSProcessingData();
		_MyData = (CSProcessingData)_RecordData;
		LoadData();
	}

	public static bool CanProcessItem(int protoId)
	{
		return ProcessingObjInfo.GetPobTime(protoId) > 0f;
	}

	public override void InitMyData()
	{
	}

	public override void InitNpc()
	{
		List<ColonyNpc> teamNpcs = ColonyNpcMgr.GetTeamNpcs(base.TeamId);
		if (teamNpcs == null || teamNpcs.Count <= 0)
		{
			return;
		}
		foreach (ColonyNpc item in teamNpcs)
		{
			if (item.m_Occupation != 5 || !AddWorker(item))
			{
				continue;
			}
			item.m_WorkRoomID = base.Id;
			item.Save();
			if (item.m_ProcessingIndex >= 0)
			{
				if (mTaskTable[item.m_ProcessingIndex] == null)
				{
					mTaskTable[item.m_ProcessingIndex] = new ProcessingTask();
				}
				mTaskTable[item.m_ProcessingIndex].AddNpc(item);
			}
		}
		SyncSave();
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HaveCore(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public override void MyUpdate()
	{
		if (!IsWorking())
		{
			return;
		}
		int num = _MyData.mTaskTable.Count();
		for (int i = 0; i < num; i++)
		{
			ProcessingTask processingTask = _MyData.mTaskTable[i];
			if (processingTask == null || processingTask.itemList.Count <= 0 || !(processingTask.m_Time >= 0f) || !(processingTask.m_CurTime >= 0f))
			{
				continue;
			}
			if (processingTask.m_CurTime < processingTask.m_Time)
			{
				if (ColonyMgr._Instance.InTest)
				{
					processingTask.m_CurTime += 30f;
				}
				else
				{
					processingTask.m_CurTime += 1f;
				}
			}
			if (processingTask.m_CurTime >= processingTask.m_Time)
			{
				TaskAccomplished(i, 1f);
			}
		}
		if (num != 0)
		{
			UpdateTimeTick(Time.time);
		}
		if (IsAuto)
		{
			DoAuto();
		}
	}

	protected override void UpdateTimeTick(float curTime)
	{
		if ((int)curTime % 5 == 0)
		{
			SyncAllCounter();
		}
	}

	public List<ColonyNpc> GetFreeWorkers()
	{
		List<ColonyNpc> list = new List<ColonyNpc>();
		foreach (ColonyNpc value in _worker.Values)
		{
			if (!value.m_IsProcessing)
			{
				list.Add(value);
			}
		}
		return list;
	}

	public void DoAuto()
	{
		List<ColonyNpc> freeWorkers = GetFreeWorkers();
		foreach (ColonyNpc item in freeWorkers)
		{
			if (item != null && (item.m_ProcessingIndex < 0 || !mTaskTable[item.m_ProcessingIndex].CanStart()))
			{
				int num = FindTaskNotEmptyIndex();
				if (num >= 0)
				{
					TrySetNpcProcessingIndex(item, num);
				}
			}
		}
		for (int i = 0; i < mTaskTable.Count(); i++)
		{
			ProcessingTask processingTask = mTaskTable[i];
			if (processingTask != null && processingTask.CanStart())
			{
				processingTask.Start();
				SyncSave();
				_Network.RPCOthers(EPacketType.PT_CL_PRC_Start, i);
			}
		}
	}

	public int FindTaskNotEmptyIndex()
	{
		for (int i = 0; i < mTaskTable.Length; i++)
		{
			if (mTaskTable[i] != null && mTaskTable[i].HasItem())
			{
				return i;
			}
		}
		return -1;
	}

	public void TaskAccomplished(int index, float percent)
	{
		ProcessingTask processingTask = mTaskTable[index];
		GetTaskResult(percent, processingTask.itemList);
		if (percent < 1f || processingTask.runCount == 1)
		{
			processingTask.itemList = new List<ItemIdCount>();
			processingTask.m_CurTime = -1f;
			processingTask.m_Time = -1f;
			foreach (ColonyNpc npc in processingTask.npcList)
			{
				npc.m_IsProcessing = false;
			}
			_Network.RPCOthers(EPacketType.PT_CL_PRC_Stop, index);
			return;
		}
		processingTask.runCount--;
		processingTask.m_CurTime = -1f;
		processingTask.m_Time = -1f;
		if (processingTask != null && processingTask.CanStart())
		{
			processingTask.Start();
			SyncSave();
			_Network.RPCOthers(EPacketType.PT_CL_PRC_Start, index, processingTask.runCount);
		}
	}

	public void GetTaskResult(float percent, List<ItemIdCount> itemList)
	{
		System.Random random = new System.Random();
		foreach (ItemIdCount item in itemList)
		{
			item.count = Mathf.FloorToInt((float)item.count * percent);
		}
		List<ItemIdCount> list = itemList.FindAll((ItemIdCount it) => it.count > 0);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		if (CSUtils.CanAddListToStorage(list, base.TeamId))
		{
			CSUtils.AddItemListToStorage(list, base.TeamId);
			_Network.RPCOthers(EPacketType.PT_CL_PRC_FinishToStorage);
			return;
		}
		int[] array = new int[list.Count * 2];
		int num = 0;
		foreach (ItemIdCount item2 in list)
		{
			array[num++] = item2.protoId;
			array[num++] = item2.count;
		}
		Vector3 vector = _Network.transform.position + new Vector3(0f, 0.66f, 0f);
		Quaternion quaternion = Quaternion.Euler(0f, random.Next(360), 0f);
		RandomItemObj randomItemObj = new RandomItemObj(vector + new Vector3((float)random.NextDouble() * 0.75f, (float)random.NextDouble() * 0.1f, (float)random.NextDouble() * 0.75f), quaternion, array);
		RandomItemMgr.Instance.AddItemForProcessing(randomItemObj);
		_Network.RPCOthers(EPacketType.PT_CL_PRC_GenResult, randomItemObj.position, quaternion, randomItemObj.items);
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, IsAuto);
		int taskCount = _MyData.TaskCount;
		BufferHelper.Serialize(writer, taskCount);
		for (int i = 0; i < _MyData.mTaskTable.Count(); i++)
		{
			if (_MyData.mTaskTable[i] == null || _MyData.mTaskTable[i].itemList.Count <= 0)
			{
				continue;
			}
			BufferHelper.Serialize(writer, i);
			int count = _MyData.mTaskTable[i].itemList.Count;
			BufferHelper.Serialize(writer, count);
			for (int j = 0; j < count; j++)
			{
				if (_MyData.mTaskTable[i].itemList[j] != null)
				{
					BufferHelper.Serialize(writer, _MyData.mTaskTable[i].itemList[j].protoId);
					BufferHelper.Serialize(writer, _MyData.mTaskTable[i].itemList[j].count);
				}
			}
			BufferHelper.Serialize(writer, _MyData.mTaskTable[i].runCount);
			BufferHelper.Serialize(writer, _MyData.mTaskTable[i].m_CurTime);
			BufferHelper.Serialize(writer, _MyData.mTaskTable[i].m_Time);
		}
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		IsAuto = BufferHelper.ReadBoolean(reader);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			int num2 = BufferHelper.ReadInt32(reader);
			ProcessingTask processingTask = new ProcessingTask();
			int num3 = BufferHelper.ReadInt32(reader);
			for (int j = 0; j < num3; j++)
			{
				ItemIdCount itemIdCount = new ItemIdCount();
				itemIdCount.protoId = BufferHelper.ReadInt32(reader);
				itemIdCount.count = BufferHelper.ReadInt32(reader);
				processingTask.itemList.Add(itemIdCount);
			}
			if (VersionMgr.colonyRecordVersion >= 2016071500)
			{
				processingTask.runCount = BufferHelper.ReadInt32(reader);
			}
			processingTask.m_CurTime = BufferHelper.ReadSingle(reader);
			processingTask.m_Time = BufferHelper.ReadSingle(reader);
			_MyData.mTaskTable[num2] = processingTask;
		}
	}

	public void SetRunCount(int taskIndex, int runCount)
	{
		if (mTaskTable[taskIndex] == null)
		{
			mTaskTable[taskIndex] = new ProcessingTask();
		}
		mTaskTable[taskIndex].runCount = runCount;
	}

	public bool StartProcessing(int index)
	{
		if (CanStartTask(index))
		{
			ProcessingTask processingTask = _MyData.mTaskTable[index];
			processingTask.Start();
			return true;
		}
		return false;
	}

	public void StopProcessing(int index)
	{
		if (mTaskTable[index] != null)
		{
			ProcessingTask processingTask = _MyData.mTaskTable[index];
			TaskAccomplished(index, processingTask.m_CurTime / processingTask.m_Time);
			processingTask.Stop();
		}
	}

	public bool CanStartTask(int index)
	{
		return _MyData.mTaskTable[index]?.CanStart() ?? false;
	}

	public bool AddItemToTask(int protoType, int count, int taskIndex)
	{
		if (mTaskTable[taskIndex] == null)
		{
			mTaskTable[taskIndex] = new ProcessingTask();
		}
		if (mTaskTable[taskIndex].m_Time < 0f && mTaskTable[taskIndex].AddItem(new ItemIdCount(protoType, count)))
		{
			return true;
		}
		return false;
	}

	public List<ItemIdCount> GetItemList(int taskIndex)
	{
		if (mTaskTable[taskIndex] == null)
		{
			return new List<ItemIdCount>();
		}
		return mTaskTable[taskIndex].itemList;
	}

	public void RemoveItemFromTask(int taskIndex, int protoId, out bool needRefresh)
	{
		if (mTaskTable[taskIndex] == null)
		{
			mTaskTable[taskIndex] = new ProcessingTask();
			needRefresh = true;
		}
		else
		{
			mTaskTable[taskIndex].RemoveItem(protoId, out needRefresh);
		}
	}

	public void AddNpcToTask(ColonyNpc npc, int index)
	{
		if (mTaskTable[index] == null)
		{
			mTaskTable[index] = new ProcessingTask();
		}
		mTaskTable[index].AddNpc(npc);
	}

	public void RemoveNpcFromTask(ColonyNpc npc, int index)
	{
		if (index >= 0 && index < mTaskTable.Length && mTaskTable[index] != null)
		{
			if (mTaskTable[index].npcList.Count == 1 && mTaskTable[index].npcList.Contains(npc) && mTaskTable[index].IsWorking())
			{
				TaskAccomplished(index, mTaskTable[index].m_CurTime / mTaskTable[index].m_Time);
			}
			mTaskTable[index].RemoveNpc(npc);
		}
	}

	public void NpcLineChange(ColonyNpc npc, int oldIndex, int newIndex)
	{
		RemoveNpcFromTask(npc, oldIndex);
		AddNpcToTask(npc, newIndex);
	}

	public void InitNpcData(ColonyNpc npc)
	{
		if (npc.m_ProcessingIndex >= 0)
		{
			if (mTaskTable[npc.m_ProcessingIndex] == null)
			{
				mTaskTable[npc.m_ProcessingIndex] = new ProcessingTask();
			}
			mTaskTable[npc.m_ProcessingIndex].InitNpc(npc);
		}
	}

	public override bool RemoveWorker(ColonyNpc npc)
	{
		bool result = base.RemoveWorker(npc);
		if (npc.m_ProcessingIndex >= 0)
		{
			RemoveNpcFromTask(npc, npc.m_ProcessingIndex);
			npc.SetProcessingIndex(-1);
		}
		return result;
	}

	public void SetNpcProcessingIndex(ColonyNpc npc, int old, int index)
	{
		if (old != index)
		{
			if (old == -1 && index != -1)
			{
				AddNpcToTask(npc, index);
			}
			else if (old != -1 && index == -1)
			{
				RemoveNpcFromTask(npc, index);
			}
			else if (old != -1 && index != -1)
			{
				NpcLineChange(npc, old, index);
			}
			SyncSave();
		}
	}

	public void TrySetNpcProcessingIndex(ColonyNpc worker, int taskIndex)
	{
		int processingIndex = worker.m_ProcessingIndex;
		worker.m_ProcessingIndex = taskIndex;
		SetNpcProcessingIndex(worker, processingIndex, taskIndex);
		worker.Save();
		worker._refNpc.RPCOthers(EPacketType.PT_CL_CLN_SetProcessingIndex, taskIndex);
	}

	public byte[] TaskTableToByte()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		CombomData(writer);
		return memoryStream.ToArray();
	}

	public void SyncAllCounter()
	{
		List<float> list = new List<float>();
		List<float> list2 = new List<float>();
		List<int> list3 = new List<int>();
		for (int i = 0; i < _MyData.mTaskTable.Count(); i++)
		{
			ProcessingTask processingTask = _MyData.mTaskTable[i];
			if (processingTask == null || processingTask.itemList.Count == 0 || processingTask.m_Time < 0f || processingTask.m_CurTime < 0f)
			{
				list.Add(-1f);
				list2.Add(-1f);
				list3.Add(-1);
			}
			else
			{
				list.Add(processingTask.m_CurTime);
				list2.Add(processingTask.m_Time);
				list3.Add(processingTask.runCount);
			}
		}
		_Network.RPCOthers(EPacketType.PT_CL_PRC_SyncAllCounter, list.ToArray(), list2.ToArray(), list3.ToArray());
	}

	public List<ItemIdCount> GetItemsInProcessing()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		for (int i = 0; i < mTaskTable.Length; i++)
		{
			if (mTaskTable[i] == null || !mTaskTable[i].IsWorking())
			{
				continue;
			}
			ProcessingTask processingTask = mTaskTable[i];
			if (processingTask.itemList.Count <= 0)
			{
				continue;
			}
			foreach (ItemIdCount item in processingTask.itemList)
			{
				CSUtils.AddItemIdCount(list, item.protoId, item.count);
			}
		}
		return list;
	}

	public void CreateNewTaskWithItems(List<ItemIdCount> allItems)
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		List<List<ItemIdCount>> list2 = new List<List<ItemIdCount>>();
		list2.Add(new List<ItemIdCount>());
		int num = 0;
		foreach (ItemIdCount allItem in allItems)
		{
			int pobMax = ProcessingObjInfo.GetPobMax(allItem.protoId);
			while (list2[num].Count >= 12)
			{
				num++;
				if (list2.Count <= num)
				{
					list2.Add(new List<ItemIdCount>());
				}
			}
			int num2 = num;
			while (allItem.count > pobMax)
			{
				if (list2.Count <= num2)
				{
					list2.Add(new List<ItemIdCount>());
				}
				if (list2[num2].Count < 12)
				{
					list2[num2].Add(new ItemIdCount(allItem.protoId, pobMax));
					allItem.count -= pobMax;
				}
				num2++;
			}
			if (list2.Count <= num2)
			{
				list2.Add(new List<ItemIdCount>());
			}
			list2[num2].Add(allItem);
		}
		List<ColonyNpc> freeWorkers = GetFreeWorkers();
		if (freeWorkers.Count == 0)
		{
			return;
		}
		List<int> list3 = new List<int>();
		for (int i = 0; i < 4; i++)
		{
			if (mTaskTable[i] == null || mTaskTable[i].itemList.Count == 0)
			{
				list3.Add(i);
			}
		}
		if (list3.Count == 0)
		{
			return;
		}
		int count = list2.Count;
		int count2 = freeWorkers.Count;
		int count3 = list3.Count;
		int num3 = Mathf.Min(count, count2, count3);
		int num4 = count2 / num3;
		if (num3 == 1 && count2 > 1)
		{
			num4 = count2 - 1;
		}
		for (int j = 0; j < num3; j++)
		{
			int num5 = list3[j];
			if (mTaskTable[num5] == null)
			{
				mTaskTable[num5] = new ProcessingTask();
			}
			for (int k = 0; k < num4; k++)
			{
				TrySetNpcProcessingIndex(freeWorkers[k], num5);
			}
			freeWorkers.RemoveRange(0, num4);
			mTaskTable[num5].itemList = list2[j];
			_Network.RPCOthers(EPacketType.PT_CL_PRC_AddItem, num5, list2[j].ToArray());
		}
		List<int> range = list3.GetRange(0, num3);
		foreach (ColonyNpc item in freeWorkers)
		{
			if (range.Contains(item.m_ProcessingIndex))
			{
				int num6 = FindNextTaskNoStart(range);
				if (num6 >= 0)
				{
					TrySetNpcProcessingIndex(item, num6);
				}
			}
		}
		for (int l = 0; l < num3; l++)
		{
			int num7 = list3[l];
			StartProcessing(num7);
			_Network.RPCOthers(EPacketType.PT_CL_PRC_Start, num7, 1);
		}
		if (num3 > 0)
		{
			ColonyMgr._Instance.GetColonyAssembly(base.TeamId)?.ShowProcessFor(list2[0]);
			SyncSave();
		}
	}

	private int FindNextTaskNoStart(List<int> runRange)
	{
		for (int i = 0; i < 4; i++)
		{
			if (!runRange.Contains(i) && (mTaskTable[i] == null || !mTaskTable[i].IsWorking()))
			{
				return i;
			}
		}
		return -1;
	}
}
