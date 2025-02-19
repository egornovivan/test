using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSRecord;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSProcessing : CSCommon
{
	public delegate void TaskEvent();

	public CSUI_CollectWnd uiObj;

	private bool uiInit;

	private ProcessingTask selectProcess;

	public CSProcessingInfo m_PInfo;

	private CSProcessingData m_PData;

	public List<ItemIdCount> mProcessingObjData;

	public TaskEvent taskComplished;

	public override bool IsDoingJobOn
	{
		get
		{
			if (!base.IsRunning)
			{
				return false;
			}
			if (mTaskTable != null)
			{
				ProcessingTask[] array = mTaskTable;
				foreach (ProcessingTask processingTask in array)
				{
					if (processingTask != null && processingTask.IsWorkingOn)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool IsAuto
	{
		get
		{
			return Data.isAuto;
		}
		set
		{
			Data.isAuto = value;
		}
	}

	public override GameObject gameLogic
	{
		get
		{
			return base.gameLogic;
		}
		set
		{
			base.gameLogic = value;
			if (!(gameLogic != null))
			{
				return;
			}
			PEMachine component = gameLogic.GetComponent<PEMachine>();
			if (component != null)
			{
				for (int i = 0; i < m_WorkSpaces.Length; i++)
				{
					m_WorkSpaces[i].WorkMachine = component;
				}
			}
			if (BuildingLogic != null)
			{
				workTrans = BuildingLogic.m_WorkTrans;
			}
		}
	}

	public CSBuildingLogic BuildingLogic => (!(gameLogic == null)) ? gameLogic.GetComponent<CSBuildingLogic>() : null;

	public CSProcessingInfo Info
	{
		get
		{
			if (m_PInfo == null)
			{
				m_PInfo = m_Info as CSProcessingInfo;
			}
			return m_PInfo;
		}
	}

	public CSProcessingData Data
	{
		get
		{
			if (m_PData == null)
			{
				m_PData = m_Data as CSProcessingData;
			}
			return m_PData;
		}
	}

	public ProcessingTask[] mTaskTable => Data.mTaskTable;

	public List<CSPersonnel> allProcessor => m_Workers.ToList();

	public CSProcessing(CSCreator creator)
	{
		m_Creator = creator;
		m_Type = 9;
		m_Workers = new CSPersonnel[5];
		m_WorkSpaces = new PersonnelSpace[5];
		for (int i = 0; i < m_WorkSpaces.Length; i++)
		{
			m_WorkSpaces[i] = new PersonnelSpace(this);
		}
		m_Grade = 3;
		if (base.IsMine)
		{
			BindEvent();
		}
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning;
	}

	public override bool AddWorker(CSPersonnel csp)
	{
		if (base.AddWorker(csp))
		{
			return true;
		}
		return false;
	}

	private void InitNPC()
	{
		CSMgCreator cSMgCreator = m_Creator as CSMgCreator;
		if (!(cSMgCreator != null))
		{
			return;
		}
		foreach (CSPersonnel processor in cSMgCreator.Processors)
		{
			if (AddWorker(processor))
			{
				processor.WorkRoom = this;
				InitNpcProcessingIndex(processor);
			}
		}
	}

	private void InitNpcProcessingIndex(CSPersonnel csp)
	{
		if (csp.ProcessingIndex >= 0 && csp.ProcessingIndex < mTaskTable.Length)
		{
			if (mTaskTable[csp.ProcessingIndex] == null)
			{
				mTaskTable[csp.ProcessingIndex] = new ProcessingTask();
			}
			mTaskTable[csp.ProcessingIndex].InitNpc(csp);
		}
	}

	private void CheckAllProcessor()
	{
		for (int i = 0; i < mTaskTable.Length; i++)
		{
			if (mTaskTable[i] == null)
			{
				continue;
			}
			for (int num = mTaskTable[i].npcList.Count - 1; num >= 0; num--)
			{
				CSPersonnel cSPersonnel = mTaskTable[i].npcList[num];
				if (!cSPersonnel.CanProcess && !cSPersonnel.IsProcessing)
				{
					cSPersonnel.StopWork();
				}
				else if (cSPersonnel.IsProcessing && cSPersonnel.ShouldStopProcessing)
				{
					cSPersonnel.StopWork();
				}
			}
		}
	}

	private void BindEvent()
	{
		CSPersonnel.RegisterProcessingIndexChangedListener(SetNpcProcessingIndex);
		CSPersonnel.RegisterProcessingIndexInitListener(InitNpcProcessingIndex);
		if (CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.CollectUI != null)
		{
			uiObj = CSUI_MainWndCtrl.Instance.CollectUI;
			uiObj.e_InitCollectEvent += InitUI;
			uiObj.e_UpdateCollect += UpdateDataToUi;
			uiObj.e_AddtoClick += OnAddClick;
			uiObj.e_RemoveEvent += OnRemoveClick;
			uiObj.e_StartClick += TryStartProcessing;
			uiObj.e_ProcessChoseEvent += OnSelectProcess;
			uiObj.e_StopClick += TryStop;
			uiObj.e_AutoClick += SetAuto;
			uiObj.e_SetRunCountEvent += OnSetRunCount;
		}
	}

	private void UnbindEvent()
	{
		CSPersonnel.UnRegisterProcessingIndexChangedListener(SetNpcProcessingIndex);
		CSPersonnel.UnRegisterProcessingIndexInitListener(InitNpcProcessingIndex);
		if (CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.CollectUI != null)
		{
			uiObj.e_InitCollectEvent -= InitUI;
			uiObj.e_UpdateCollect -= UpdateDataToUi;
			uiObj.e_AddtoClick -= OnAddClick;
			uiObj.e_RemoveEvent -= OnRemoveClick;
			uiObj.e_StartClick -= TryStartProcessing;
			uiObj.e_ProcessChoseEvent -= OnSelectProcess;
			uiObj.e_StopClick -= TryStop;
			uiObj.e_AutoClick -= SetAuto;
			uiObj.e_SetRunCountEvent -= OnSetRunCount;
		}
	}

	private void InitUI(object obj)
	{
		SetSelect();
	}

	private void UpdateDataToUi(object obj)
	{
		if (!base.IsRunning)
		{
			return;
		}
		for (int i = 0; i < mTaskTable.Length; i++)
		{
			if (mTaskTable[i] != null)
			{
				(obj as CSUI_CollectWnd).UpdateProcess(i, mTaskTable[i].itemList, mTaskTable[i].npcList.Count, mTaskTable[i].CountTime(), mTaskTable[i].runCount);
			}
		}
	}

	private void OnAddClick(object sender, int index, int protoId, int curCout)
	{
		if (!base.IsRunning)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_AddItem, protoId, curCout, index);
			return;
		}
		if (mTaskTable[index] == null)
		{
			mTaskTable[index] = new ProcessingTask();
		}
		ProcessingTask processingTask = mTaskTable[index];
		if (processingTask.AddItem(new ItemIdCount(protoId, curCout)))
		{
			UpdateLineToUI(index);
		}
	}

	private void OnSetRunCount(object sender, int index, int runCount)
	{
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_SetRound, index, runCount);
			return;
		}
		if (mTaskTable[index] == null)
		{
			mTaskTable[index] = new ProcessingTask();
		}
		ProcessingTask processingTask = mTaskTable[index];
		processingTask.SetRunCount(runCount);
		UpdateLineToUI(index);
	}

	public void OnRemoveClick(object sender, int index, int protoId)
	{
		if (base.IsRunning)
		{
			if (PeGameMgr.IsMulti)
			{
				_ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_RemoveItem, index, protoId);
			}
			else if (mTaskTable[index] != null)
			{
				ProcessingTask processingTask = mTaskTable[index];
				processingTask.RemoveItem(protoId);
				UpdateLineToUI(index);
			}
		}
	}

	public void TryStartProcessing(object sender, int index)
	{
		if (!base.IsRunning)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_Start, index);
		}
		else if (mTaskTable[index] != null)
		{
			if (mTaskTable[index].CanStart(out var errorCode))
			{
				StartProcessing(index);
			}
			else if (errorCode > 0)
			{
				CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(errorCode));
				CSUI_MainWndCtrl.Instance.GoToPersonnelWorkWnd();
			}
			UpdateSelectedTask();
		}
	}

	private void OnSelectProcess(object sender, int index)
	{
		selectProcess = mTaskTable[index];
		UpdateSelectedTask();
	}

	public void TryStop(object sender, int index)
	{
		if (base.IsRunning)
		{
			if (PeGameMgr.IsMulti)
			{
				_ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_Stop, index);
			}
			else
			{
				Stop(index);
				UpdateSelectedTask();
			}
		}
	}

	private void SetAuto(object sender, bool flag)
	{
		if (base.IsRunning)
		{
			if (PeGameMgr.IsMulti)
			{
				BuildingLogic.network.RPCServer(EPacketType.PT_CL_PRC_SetAuto, flag);
			}
			IsAuto = flag;
		}
	}

	private void UpdateLineToUI(int index)
	{
		if (index >= 0 && index < mTaskTable.Length && mTaskTable[index] != null && uiObj != null)
		{
			uiObj.Init();
			uiObj.UpdateProcess(index, mTaskTable[index].itemList, mTaskTable[index].npcList.Count, mTaskTable[index].CountTime(), mTaskTable[index].runCount);
		}
	}

	private void UpdateSelectedTask()
	{
		if (selectProcess != null)
		{
			if (selectProcess.cs != null)
			{
				if (uiObj != null)
				{
					uiObj.ShowStartBtn(show: false);
				}
			}
			else if (uiObj != null)
			{
				uiObj.ShowStartBtn(show: true);
			}
		}
		else if (uiObj != null)
		{
			uiObj.ShowStartBtn(show: true);
		}
	}

	public void SetSelect()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		List<ItemIdCount> list2 = new List<ItemIdCount>();
		List<ItemIdCount> list3 = new List<ItemIdCount>();
		foreach (ProcessingObjInfo item in ProcessingObjInfo.GetAllInfo())
		{
			switch (item.tab)
			{
			case 0:
				list.Add(new ItemIdCount(item.protoId, item.max));
				break;
			case 1:
				list2.Add(new ItemIdCount(item.protoId, item.max));
				break;
			default:
				list3.Add(new ItemIdCount(item.protoId, item.max));
				break;
			}
		}
		uiObj.ClearPlan();
		uiObj.AddOreList(list);
		uiObj.AddHerbList(list2);
		uiObj.AddOtherList(list3);
		uiObj.ClearProcess();
	}

	public override void Update()
	{
		base.Update();
		CheckAllProcessor();
		if (!base.IsRunning)
		{
			for (int i = 0; i < mTaskTable.Length; i++)
			{
				if (mTaskTable[i] != null && mTaskTable[i].cs != null)
				{
					mTaskTable[i].cs.enabled = false;
				}
			}
			return;
		}
		for (int j = 0; j < mTaskTable.Length; j++)
		{
			if (mTaskTable[j] != null && mTaskTable[j].cs != null)
			{
				mTaskTable[j].cs.enabled = true;
			}
		}
		for (int k = 0; k < mTaskTable.Length; k++)
		{
			if (mTaskTable[k] != null)
			{
				mTaskTable[k].Update();
				if (mTaskTable[k].cs != null && uiObj != null)
				{
					uiObj.UpdateTimes(k, mTaskTable[k].TimeLeft);
				}
			}
			if (!PeGameMgr.IsMulti && IsAuto)
			{
				DoAuto();
			}
		}
	}

	public void LoadData()
	{
	}

	public void AddItemToTask(int protoId, int count, int taskIndex)
	{
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_AddItem, protoId, count, taskIndex);
			return;
		}
		if (mTaskTable[taskIndex] == null)
		{
			mTaskTable[taskIndex] = new ProcessingTask();
		}
		if (!mTaskTable[taskIndex].IsWorking())
		{
			mTaskTable[taskIndex].itemList.Add(new ItemIdCount(protoId, count));
		}
	}

	public void AddNpcToTask(CSPersonnel npc, int index)
	{
		if (mTaskTable[index] == null)
		{
			mTaskTable[index] = new ProcessingTask();
		}
		mTaskTable[index].AddNpc(npc);
		UpdateLineToUI(index);
	}

	public override void RemoveWorker(CSPersonnel npc)
	{
		base.RemoveWorker(npc);
		if (npc.ProcessingIndex >= 0)
		{
			npc.ProcessingIndex = -1;
		}
	}

	public void RemoveNpcFromTask(CSPersonnel npc, int index)
	{
		if (mTaskTable[index] != null)
		{
			mTaskTable[index].RemoveNpc(npc);
			UpdateLineToUI(index);
		}
	}

	public void SetNpcProcessingIndex(CSPersonnel npc, int old, int index)
	{
		if (AddWorker(npc))
		{
			if (old == -1 && index != -1)
			{
				AddNpcToTask(npc, index);
			}
			else if (old != -1 && index == -1)
			{
				RemoveNpcFromTask(npc, old);
			}
			else if (old != -1 && index != -1)
			{
				NpcLineChange(npc, old, index);
			}
			UpdateSelectedTask();
		}
	}

	public void StartProcessing(int index)
	{
		if (Application.isEditor && base.InTest)
		{
			mTaskTable[index].Start(5f);
		}
		else
		{
			mTaskTable[index].Start();
		}
		mTaskTable[index].taskAccomplished = TaskAccomplished;
		UpdateLineToUI(index);
		if (uiObj != null)
		{
			uiObj.ShowStartBtn(show: false);
		}
	}

	public void Stop(int index)
	{
		if (mTaskTable[index] != null && (!PeGameMgr.IsSingle || !(mTaskTable[index].cs == null)))
		{
			mTaskTable[index].StopCounter();
			if (uiObj != null)
			{
				uiObj.ShowStartBtn(show: true);
			}
		}
	}

	public void SyncStop(int index)
	{
		if (mTaskTable[index] != null)
		{
			mTaskTable[index].SyncStopCounter();
			if (uiObj != null)
			{
				uiObj.ShowStartBtn(show: true);
			}
		}
	}

	public void TaskAccomplished(ProcessingTask pTask, float percent = 1f)
	{
		GetTaskResult(percent, pTask.itemList);
		int index = mTaskTable.ToList().FindIndex((ProcessingTask it) => it == pTask);
		if (percent < 1f)
		{
			pTask.ClearItem();
			UpdateLineToUI(index);
			if (pTask == selectProcess && uiObj != null)
			{
				uiObj.ShowStartBtn(show: true);
			}
			pTask.SetRunCount(0);
		}
		else if (pTask.runCount <= 1)
		{
			pTask.ClearItem();
			UpdateLineToUI(index);
			if (pTask == selectProcess && uiObj != null)
			{
				uiObj.ShowStartBtn(show: true);
			}
			pTask.SetRunCount(0);
		}
		else
		{
			pTask.CountDownRepeat();
			StartProcessing(index);
		}
	}

	public RandomItemObj GetTaskResult(float percent, List<ItemIdCount> itemList)
	{
		System.Random random = new System.Random();
		List<ItemIdCount> list = new List<ItemIdCount>();
		foreach (ItemIdCount item in itemList)
		{
			int count = Mathf.FloorToInt((float)item.count * percent);
			list.Add(new ItemIdCount(item.protoId, count));
		}
		List<ItemIdCount> list2 = list.FindAll((ItemIdCount it) => it.count > 0);
		if (list2.Count <= 0)
		{
			return null;
		}
		bool flag = false;
		if (base.Assembly != null && base.Assembly.Storages != null)
		{
			List<MaterialItem> list3 = CSUtils.ItemIdCountToMaterialItem(list2);
			foreach (CSCommon storage in base.Assembly.Storages)
			{
				CSStorage cSStorage = storage as CSStorage;
				if (cSStorage.m_Package.CanAdd(list3))
				{
					cSStorage.m_Package.Add(list3);
					flag = true;
					CSUtils.ShowTips(82201067);
					break;
				}
				if (CSAutocycleMgr.Instance != null)
				{
					CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
				}
			}
		}
		if (flag)
		{
			return null;
		}
		int[] items = CSUtils.ItemIdCountListToIntArray(list2);
		Vector3 vector = base.Position + new Vector3(0f, 0.72f, 0f);
		if (BuildingLogic != null && BuildingLogic.m_ResultTrans.Length > 0)
		{
			Transform transform = BuildingLogic.m_ResultTrans[random.Next(BuildingLogic.m_ResultTrans.Count())];
			if (transform != null)
			{
				vector = transform.position;
			}
		}
		for (; RandomItemMgr.Instance.ContainsPos(vector); vector += new Vector3(0f, 0.01f, 0f))
		{
		}
		CSUtils.ShowTips(82201068);
		return RandomItemMgr.Instance.GenProcessingItem(vector + new Vector3((float)random.NextDouble() * 0.15f, 0f, (float)random.NextDouble() * 0.15f), items);
	}

	public void DoAuto()
	{
		List<CSPersonnel> freeWorkers = GetFreeWorkers();
		if (freeWorkers.Count == 0)
		{
			return;
		}
		foreach (CSPersonnel item in freeWorkers)
		{
			if (item != null && (item.ProcessingIndex < 0 || !mTaskTable[item.ProcessingIndex].CanStart()))
			{
				int num = FindTaskNotEmptyIndex();
				if (num >= 0)
				{
					item.ProcessingIndex = num;
				}
			}
		}
		ProcessingTask[] array = mTaskTable;
		foreach (ProcessingTask processingTask in array)
		{
			if (processingTask != null && processingTask.CanStart())
			{
				if (Application.isEditor && base.InTest)
				{
					processingTask.Start(5f);
				}
				else
				{
					processingTask.Start();
				}
				processingTask.taskAccomplished = TaskAccomplished;
			}
		}
	}

	public void NpcLineChange(CSPersonnel npc, int oldIndex, int newIndex)
	{
		RemoveNpcFromTask(npc, oldIndex);
		AddNpcToTask(npc, newIndex);
		UpdateLineToUI(oldIndex);
		UpdateLineToUI(newIndex);
		UpdateSelectedTask();
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!PeGameMgr.IsMulti) ? m_Creator.m_DataInst.AssignData(ID, 9, ref refData) : MultiColonyManager.Instance.AssignData(ID, 9, ref refData, _ColonyObj));
		m_Data = refData as CSProcessingData;
		InitNPC();
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			return;
		}
		StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
		StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		for (int i = 0; i < Data.mTaskTable.Length; i++)
		{
			if (Data.mTaskTable[i] != null)
			{
				Data.mTaskTable[i].StartCounterFromRecord();
				Data.mTaskTable[i].taskAccomplished = TaskAccomplished;
			}
		}
	}

	public bool HasLine(int index)
	{
		if (mTaskTable[index] == null)
		{
			return false;
		}
		if (mTaskTable[index].itemList == null)
		{
			return false;
		}
		if (mTaskTable[index].itemList.Find((ItemIdCount item) => item != null) == null)
		{
			return false;
		}
		return true;
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
	}

	private void DestroyCounter()
	{
		for (int i = 0; i < 4; i++)
		{
			if (mTaskTable[i] != null && mTaskTable[i].cs != null)
			{
				mTaskTable[i].StopCounter();
			}
		}
	}

	public override void DestroySelf()
	{
		DestroyCounter();
		base.DestroySelf();
		if (base.IsMine)
		{
			UnbindEvent();
		}
	}

	public override void StopWorking(int npcId)
	{
		CSPersonnel npc = base.m_MgCreator.GetNpc(npcId);
		if (npc != null && m_Workers.Contains(npc) && npc.ProcessingIndex >= 0)
		{
			npc.ProcessingIndex = -1;
		}
	}

	public override void UpdateDataToUI()
	{
		for (int i = 0; i < 4; i++)
		{
			UpdateLineToUI(i);
		}
	}

	public void AddItemResult(int taskIndex)
	{
		UpdateLineToUI(taskIndex);
	}

	public void RemoveItemResult(int taskIndex)
	{
		UpdateLineToUI(taskIndex);
	}

	public void AddNpcResult()
	{
	}

	public void RemoveNpcResult()
	{
	}

	public void SetRoundResult(int taskIndex)
	{
		UpdateLineToUI(taskIndex);
	}

	public void SetAutoResult()
	{
	}

	public void StartResult(int taskIndex)
	{
	}

	public void StopResult(int taskIndex)
	{
		UpdateLineToUI(taskIndex);
	}

	public void SetCounter(int index, float cur, float final, int runCount)
	{
		if (mTaskTable[index] == null)
		{
			mTaskTable[index] = new ProcessingTask();
		}
		mTaskTable[index].StartCounterFromNet(cur, final, runCount);
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

	public List<CSPersonnel> GetFreeWorkers()
	{
		List<CSPersonnel> list = new List<CSPersonnel>();
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null && !cSPersonnel.IsProcessing && cSPersonnel.CanProcess)
			{
				list.Add(cSPersonnel);
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
			if (pobMax < 0)
			{
				continue;
			}
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
		List<CSPersonnel> freeWorkers = GetFreeWorkers();
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
				freeWorkers[k].TrySetProcessingIndex(num5);
			}
			freeWorkers.RemoveRange(0, num4);
			mTaskTable[num5].itemList = list2[j];
		}
		List<int> range = list3.GetRange(0, num3);
		foreach (CSPersonnel item in freeWorkers)
		{
			if (range.Contains(item.ProcessingIndex))
			{
				int num6 = FindNextTaskNoStart(range);
				if (num6 >= 0)
				{
					item.TrySetProcessingIndex(num6);
				}
			}
		}
		for (int l = 0; l < num3; l++)
		{
			int index = list3[l];
			StartProcessing(index);
		}
		if (num3 > 0)
		{
			CSAutocycleMgr.Instance.ShowProcessFor(list2[0]);
		}
		UpdateDataToUI();
	}

	private int FindNextTaskNoStart(List<int> runRange)
	{
		for (int i = 0; i < 4; i++)
		{
			if (!runRange.Contains(i) && (mTaskTable[i] == null || !(mTaskTable[i].cs != null)))
			{
				return i;
			}
		}
		return -1;
	}

	public static void ParseData(byte[] data, CSProcessingData recordData)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		recordData.isAuto = BufferHelper.ReadBoolean(reader);
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
			processingTask.runCount = BufferHelper.ReadInt32(reader);
			processingTask.m_CurTime = BufferHelper.ReadSingle(reader);
			processingTask.m_Time = BufferHelper.ReadSingle(reader);
			recordData.mTaskTable[num2] = processingTask;
		}
	}

	public static bool CanProcessItem(int protoId)
	{
		return ProcessingObjInfo.GetPobTime(protoId) > 0f;
	}
}
