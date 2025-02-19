using System.Collections.Generic;

public class ProcessingTask
{
	public List<ItemIdCount> itemList;

	public List<ColonyNpc> npcList;

	public float m_CurTime = -1f;

	public float m_Time = -1f;

	public int runCount = 1;

	public ProcessingTask()
	{
		itemList = new List<ItemIdCount>();
		npcList = new List<ColonyNpc>();
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

	public void AddNpc(ColonyNpc npc)
	{
		if (!npcList.Contains(npc) && npcList.Count < 4)
		{
			if (m_Time < 0f)
			{
				npcList.Add(npc);
				npc.m_IsProcessing = false;
				return;
			}
			npcList.Add(npc);
			float num = m_CurTime / m_Time;
			m_Time = CountTime();
			m_CurTime = m_Time * num;
			npc.m_IsProcessing = true;
		}
	}

	public float GetFullWorkerParam(List<ColonyNpc> npcList)
	{
		float num = 1f;
		foreach (ColonyNpc npc in npcList)
		{
			num *= 1f + npc.GetProcessingTimeSkill;
		}
		return num / (float)npcList.Count;
	}

	public void InitNpc(ColonyNpc npc)
	{
		if (!npcList.Contains(npc))
		{
			npcList.Add(npc);
		}
	}

	public void RemoveNpc(ColonyNpc npc)
	{
		if (!npcList.Contains(npc))
		{
			return;
		}
		if (m_Time < 0f)
		{
			npcList.Remove(npc);
			npc.m_IsProcessing = false;
			return;
		}
		npcList.Remove(npc);
		if (npcList.Count == 0)
		{
			Stop();
		}
		else
		{
			float num = m_CurTime / m_Time;
			m_Time = CountTime();
			m_CurTime = m_Time * num;
		}
		npc.m_IsProcessing = false;
	}

	public bool AddItem(ItemIdCount po)
	{
		if (m_Time >= 0f)
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

	public bool RemoveItem(int protoId, out bool needRefresh)
	{
		needRefresh = false;
		if (m_Time >= 0f)
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
			needRefresh = true;
			return true;
		}
		needRefresh = true;
		return false;
	}

	public void Stop()
	{
		m_CurTime = -1f;
		m_Time = -1f;
		foreach (ColonyNpc npc in npcList)
		{
			npc.SetIsProcessing(isProcessing: false);
		}
	}

	public bool CanStart()
	{
		return itemList.Count > 0 && npcList.Count > 0 && m_Time < 0f && m_Time < 0f;
	}

	public void Start()
	{
		m_Time = CountTime();
		m_CurTime = 0f;
		foreach (ColonyNpc npc in npcList)
		{
			npc.SetIsProcessing(isProcessing: true);
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

	public void ClearItem()
	{
		itemList = new List<ItemIdCount>();
	}

	public void StopProcess()
	{
	}

	public bool IsWorking()
	{
		return m_Time > 0f && m_Time > m_CurTime;
	}
}
