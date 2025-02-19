using System.Collections.Generic;
using Pathea;

public class ProcessingTask
{
	public delegate void TaskEvent(ProcessingTask task, float percent);

	public List<ItemIdCount> itemList;

	public List<CSPersonnel> npcList;

	public int runCount;

	public CounterScript cs;

	public float m_CurTime;

	public float m_Time;

	public TaskEvent taskAccomplished;

	public float TimeLeft
	{
		get
		{
			if (cs != null)
			{
				return cs.FinalCounter - cs.CurCounter;
			}
			return CountTime();
		}
	}

	public bool IsWorkingOn => cs != null && cs.enabled;

	public ProcessingTask()
	{
		itemList = new List<ItemIdCount>();
		npcList = new List<CSPersonnel>();
		runCount = 1;
		cs = null;
	}

	public void SetRunCount(int count)
	{
		runCount = count;
		if (cs != null)
		{
			cs.SetRunCount(count);
		}
	}

	public void CountDownRepeat()
	{
		if (runCount > 0)
		{
			runCount--;
		}
	}

	public bool NeedRepeat()
	{
		return runCount >= 1;
	}

	public bool CanStart(out int errorCode)
	{
		errorCode = -1;
		if (npcList.Count <= 0)
		{
			errorCode = 82201059;
			return false;
		}
		if (itemList.Count <= 0)
		{
			return false;
		}
		if (cs != null)
		{
			return false;
		}
		return true;
	}

	public bool CanStart()
	{
		if (npcList.Count <= 0)
		{
			return false;
		}
		if (itemList.Count <= 0)
		{
			return false;
		}
		if (cs != null)
		{
			return false;
		}
		return true;
	}

	public void Start()
	{
		if (npcList.Count == 0)
		{
			return;
		}
		float timeNeed = CountTime();
		StartCounter(timeNeed);
		foreach (CSPersonnel npc in npcList)
		{
			npc.resultItems = null;
			npc.IsProcessing = true;
		}
	}

	public void Start(float time)
	{
		if (npcList.Count == 0)
		{
			return;
		}
		StartCounter(time);
		foreach (CSPersonnel npc in npcList)
		{
			npc.resultItems = null;
			npc.IsProcessing = true;
		}
	}

	public void StartCounter(float timeNeed)
	{
		StartCounter(0f, timeNeed);
	}

	public void StartCounterFromRecord()
	{
		StartCounter(m_CurTime, m_Time);
		if (!(m_Time > 0f))
		{
			return;
		}
		foreach (CSPersonnel npc in npcList)
		{
			npc.resultItems = null;
			npc.IsProcessing = true;
		}
	}

	public void StartCounterFromNet(float cur, float final, int runCount)
	{
		StartCounter(cur, final, runCount);
		if (!(m_Time > 0f))
		{
			return;
		}
		foreach (CSPersonnel npc in npcList)
		{
			npc.resultItems = null;
			npc.IsProcessing = true;
		}
	}

	public void StartCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			if (cs == null)
			{
				cs = CSMain.Instance.CreateCounter("Processing", curTime, finalTime);
				cs.SetRunCount(runCount);
			}
			else
			{
				cs.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				cs.OnTimeUp = Accomplished;
			}
		}
	}

	public void StartCounter(float curTime, float finalTime, int runCount)
	{
		if (!(finalTime < 0f))
		{
			this.runCount = runCount;
			if (cs == null)
			{
				cs = CSMain.Instance.CreateCounter("Processing", curTime, finalTime);
				cs.SetRunCount(runCount);
			}
			else
			{
				cs.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				cs.OnTimeUp = Accomplished;
			}
		}
	}

	public void StopCounter()
	{
		if (!PeGameMgr.IsMulti)
		{
			taskAccomplished(this, cs.CurCounter / cs.FinalCounter);
		}
		else
		{
			ClearItem();
		}
		CSMain.Instance.DestoryCounter(cs);
		cs = null;
		foreach (CSPersonnel npc in npcList)
		{
			npc.IsProcessing = false;
		}
	}

	public void SyncStopCounter()
	{
		if (cs != null)
		{
			CSMain.Instance.DestoryCounter(cs);
			cs = null;
		}
		foreach (CSPersonnel npc in npcList)
		{
			npc.IsProcessing = false;
		}
	}

	public float CountTime()
	{
		if (npcList.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (ItemIdCount item in itemList)
		{
			if (item != null)
			{
				num += ProcessingObjInfo.GetPobTime(item.protoId) * (float)item.count;
			}
		}
		return num * GetFullWorkerParam(npcList);
	}

	public float GetFullWorkerParam(List<CSPersonnel> npcList)
	{
		float num = 1f;
		foreach (CSPersonnel npc in npcList)
		{
			num *= 1f + npc.GetProcessingTimeSkill;
		}
		return num / (float)npcList.Count;
	}

	private List<CSPersonnel> GenNewNpcList(List<CSPersonnel> originalNpcList, CSPersonnel changeNpc, bool isAdd)
	{
		List<CSPersonnel> list = new List<CSPersonnel>();
		list.AddRange(originalNpcList);
		if (isAdd)
		{
			list.Add(changeNpc);
		}
		else
		{
			list.Remove(changeNpc);
		}
		return list;
	}

	private void Accomplished()
	{
		taskAccomplished(this, 1f);
		if (runCount > 0)
		{
			return;
		}
		foreach (CSPersonnel npc in npcList)
		{
			npc.IsProcessing = false;
		}
	}

	public void AddNpc(CSPersonnel npc)
	{
		if (!npcList.Contains(npc))
		{
			if (cs == null)
			{
				npcList.Add(npc);
				npc.IsProcessing = false;
				return;
			}
			npcList.Add(npc);
			float num = cs.CurCounter / cs.FinalCounter;
			float num2 = CountTime();
			float curCounter = num2 * num;
			cs.Init(curCounter, num2);
			npc.IsProcessing = true;
		}
	}

	public void InitNpc(CSPersonnel npc)
	{
		if (!npcList.Contains(npc))
		{
			npcList.Add(npc);
			if (cs == null)
			{
				npc.IsProcessing = false;
			}
			else
			{
				npc.IsProcessing = true;
			}
		}
	}

	public void RemoveNpc(CSPersonnel npc)
	{
		if (!npcList.Contains(npc))
		{
			return;
		}
		if (cs == null)
		{
			npcList.Remove(npc);
		}
		else
		{
			npcList.Remove(npc);
			if (npcList.Count == 0)
			{
				StopCounter();
			}
			else
			{
				float num = cs.CurCounter / cs.FinalCounter;
				float num2 = CountTime();
				float curCounter = num2 * num;
				cs.Init(curCounter, num2);
			}
		}
		npc.IsProcessing = false;
	}

	public bool AddItem(ItemIdCount po)
	{
		if (cs != null)
		{
			return false;
		}
		bool flag = false;
		ItemIdCount itemIdCount = null;
		foreach (ItemIdCount item in itemList)
		{
			if (item.protoId == po.protoId)
			{
				itemIdCount = item;
				flag = true;
			}
		}
		if (flag)
		{
			if (itemIdCount.count >= ProcessingObjInfo.GetPobMax(itemIdCount.protoId))
			{
				return false;
			}
			itemIdCount.count += po.count;
			if (itemIdCount.count > ProcessingObjInfo.GetPobMax(itemIdCount.protoId))
			{
				itemIdCount.count = ProcessingObjInfo.GetPobMax(itemIdCount.protoId);
			}
			return true;
		}
		if (itemList.Count >= 12)
		{
			return false;
		}
		if (po.count > ProcessingObjInfo.GetPobMax(po.protoId))
		{
			po.count = ProcessingObjInfo.GetPobMax(po.protoId);
		}
		itemList.Add(po);
		return true;
	}

	public bool RemoveItem(int protoId)
	{
		if (cs != null)
		{
			return false;
		}
		bool flag = false;
		ItemIdCount item = null;
		foreach (ItemIdCount item2 in itemList)
		{
			if (item2.protoId == protoId)
			{
				item = item2;
				flag = true;
			}
		}
		if (flag)
		{
			itemList.Remove(item);
			return true;
		}
		return false;
	}

	public void ClearItem()
	{
		itemList = new List<ItemIdCount>(12);
	}

	public bool IsWorking()
	{
		return cs != null;
	}

	public void Update()
	{
		if (cs != null)
		{
			m_CurTime = cs.CurCounter;
			m_Time = cs.FinalCounter;
		}
		else
		{
			m_CurTime = 0f;
			m_Time = -1f;
		}
	}

	public bool HasItem()
	{
		if (itemList.Count > 0)
		{
			return true;
		}
		return false;
	}

	public void CallBackAllNpc()
	{
		foreach (CSPersonnel npc in npcList)
		{
			npc.IsProcessing = false;
		}
	}
}
