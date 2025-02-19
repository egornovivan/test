public class Sickbed
{
	public int bedId;

	public ColonyTent tentBuilding;

	public ColonyNpc npc;

	public int npcId = -1;

	public float m_CurTime = -1f;

	public float m_Time = -1f;

	public bool isNpcReady;

	public bool occupied;

	public ColonyNpc Npc
	{
		get
		{
			return npc;
		}
		set
		{
			if (value != null)
			{
				npcId = value._npcID;
			}
			else
			{
				npcId = -1;
			}
			npc = value;
		}
	}

	public bool IsOccupied => occupied;

	public float TimeLeft
	{
		get
		{
			if (m_Time <= 0f)
			{
				return 0f;
			}
			return m_Time - m_CurTime;
		}
	}

	public Sickbed()
	{
	}

	public Sickbed(int bedId)
	{
		this.bedId = bedId;
	}

	public Sickbed(ColonyTent cst)
	{
		tentBuilding = cst;
	}

	public void OccupyMachine()
	{
		occupied = true;
	}

	public void ReleaseMachine()
	{
		occupied = false;
	}

	public bool IsPatientReady()
	{
		return Npc != null && isNpcReady;
	}

	private void StartCounter()
	{
		float finalTime = tentBuilding.CountFinalTime(npc);
		StartCounter(0f, finalTime);
	}

	public void StartCounter(float timeNeed)
	{
		StartCounter(0f, timeNeed);
	}

	public void StartCounterFromRecord()
	{
		StartCounter(m_CurTime, m_Time);
	}

	public void StartCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			m_Time = finalTime;
			m_CurTime = curTime;
		}
	}

	public void StopCounter()
	{
		m_Time = -1f;
		m_CurTime = -1f;
	}

	public void Update()
	{
		if (IsPatientReady())
		{
			if (m_Time == -1f)
			{
				StartCounter();
			}
			if (m_CurTime < m_Time)
			{
				m_CurTime += 1f;
			}
			if (m_CurTime >= m_Time)
			{
				OnTentFinish();
			}
		}
	}

	public void OnTentFinish()
	{
		tentBuilding.OnTentFinish(this);
	}
}
