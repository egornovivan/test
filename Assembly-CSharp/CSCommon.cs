using Pathea.Operate;
using UnityEngine;

public abstract class CSCommon : CSEntity
{
	public const float MAXDistance = 2f;

	public const float SqrMaxDistance = 4f;

	private CSAssembly m_Assembly;

	public float m_Power;

	protected CSPersonnel[] m_Workers;

	protected PersonnelSpace[] m_WorkSpaces;

	public PEMachine WorkPoints;

	public CSAssembly Assembly
	{
		get
		{
			return m_Assembly;
		}
		set
		{
			if (m_Assembly != value)
			{
				m_Assembly = value;
				ExcuteEvent(5001, value);
			}
			else
			{
				m_Assembly = value;
			}
		}
	}

	public PersonnelSpace[] WorkSpaces => m_WorkSpaces;

	public int WorkerCount
	{
		get
		{
			int num = 0;
			if (m_Workers != null)
			{
				CSPersonnel[] workers = m_Workers;
				foreach (CSPersonnel cSPersonnel in workers)
				{
					if (cSPersonnel != null)
					{
						num++;
					}
				}
			}
			return num;
		}
	}

	public int WorkerMaxCount
	{
		get
		{
			if (m_Workers == null)
			{
				return 0;
			}
			return m_Workers.Length;
		}
	}

	public CSPersonnel Worker(int index)
	{
		return m_Workers[index];
	}

	public virtual float GetWorkerParam()
	{
		return GetWorkingCount();
	}

	public int GetWorkingCount()
	{
		int num = 0;
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null)
			{
				num++;
			}
		}
		return num;
	}

	public virtual bool AddWorker(CSPersonnel npc)
	{
		if (m_Workers == null)
		{
			Debug.LogWarning("There are not workers in this common entity.");
			return false;
		}
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null && cSPersonnel == npc)
			{
				return true;
			}
		}
		for (int j = 0; j < m_Workers.Length; j++)
		{
			if (m_Workers[j] == null)
			{
				m_Workers[j] = npc;
				return true;
			}
		}
		return false;
	}

	public virtual void RemoveWorker(CSPersonnel npc)
	{
		if (m_Workers == null)
		{
			Debug.LogWarning("There are not workers in this common entity.");
			return;
		}
		for (int i = 0; i < m_Workers.Length; i++)
		{
			if (m_Workers[i] != npc)
			{
				continue;
			}
			for (int j = 0; j < m_WorkSpaces.Length; j++)
			{
				if (m_WorkSpaces[j].m_Person == m_Workers[i])
				{
					m_WorkSpaces[j].m_Person = null;
					break;
				}
			}
			m_Workers[i] = null;
			break;
		}
	}

	public void ClearWorkers()
	{
		if (m_Workers == null)
		{
			Debug.LogWarning("There are not workers in this common entity.");
			return;
		}
		for (int i = 0; i < m_Workers.Length; i++)
		{
			if (m_Workers[i] != null)
			{
				m_Workers[i].WorkRoom = null;
			}
		}
	}

	public void AutoSettleWorkers()
	{
		if (m_Creator == null)
		{
			Debug.LogError("The creator is null.");
			return;
		}
		CSPersonnel[] npcs = m_Creator.GetNpcs();
		if (npcs == null)
		{
			return;
		}
		CSPersonnel[] array = npcs;
		foreach (CSPersonnel cSPersonnel in array)
		{
			int num = WorkerMaxCount - WorkerCount;
			if (num > 0)
			{
				if (cSPersonnel.WorkRoom == null)
				{
					cSPersonnel.WorkRoom = this;
				}
				continue;
			}
			break;
		}
	}

	public virtual bool NeedWorkers()
	{
		return false;
	}

	public override void ChangeState()
	{
		bool isRunning = m_IsRunning;
		if (m_Assembly != null && m_Assembly.IsRunning)
		{
			m_IsRunning = true;
		}
		else
		{
			m_IsRunning = false;
		}
		if (isRunning && !m_IsRunning)
		{
			DestroySomeData();
		}
		else if (!isRunning && m_IsRunning)
		{
			UpdateDataToUI();
		}
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		if (m_Assembly != null)
		{
			m_Assembly.DetachCommonEntity(this);
		}
		if (m_Workers == null)
		{
			return;
		}
		for (int i = 0; i < m_Workers.Length; i++)
		{
			if (m_Workers[i] != null)
			{
				m_Workers[i].ClearWorkRoom();
			}
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public PersonnelSpace FindEmptySpace(PersonnelBase person)
	{
		for (int i = 0; i < m_WorkSpaces.Length; i++)
		{
			if (m_WorkSpaces[i].m_Person == null || m_WorkSpaces[i].m_Person == person)
			{
				return m_WorkSpaces[i];
			}
		}
		return null;
	}
}
