using Pathea;
using Pathea.Operate;
using UnityEngine;

public class Sickbed
{
	public int bedId;

	public CSMedicalTent tentBuilding;

	public PELay bedLay;

	public Transform bedTrans;

	public PeEntity npc;

	public int npcId = -1;

	public CounterScript cs;

	public float m_CurTime = -1f;

	public float m_Time = -1f;

	public bool isNpcReady;

	public bool occupied;

	public PeEntity Npc
	{
		get
		{
			return npc;
		}
		set
		{
			if (value != null)
			{
				npcId = value.Id;
			}
			else
			{
				npcId = -1;
			}
			npc = value;
		}
	}

	public bool IsOccupied => occupied;

	public CSPersonnel csp
	{
		get
		{
			if (Npc == null)
			{
				return null;
			}
			return tentBuilding.m_Creator.GetNpc(Npc.Id);
		}
	}

	public float TimeLeft
	{
		get
		{
			if (cs == null)
			{
				return 0f;
			}
			return cs.FinalCounter - cs.CurCounter;
		}
	}

	public Sickbed()
	{
	}

	public Sickbed(CSMedicalTent cst)
	{
		tentBuilding = cst;
	}

	public Sickbed(int bedId)
	{
		this.bedId = bedId;
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
		return Npc != null && npc.GetCmpt<NpcCmpt>().MedicalState == ENpcMedicalState.In_Hospital;
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
			if (cs == null)
			{
				cs = CSMain.Instance.CreateCounter("MedicalTent", curTime, finalTime);
			}
			else
			{
				cs.Init(curTime, finalTime);
			}
			cs.OnTimeUp = OnTentFinish;
		}
	}

	public void StopCounter()
	{
		m_CurTime = -1f;
		m_Time = -1f;
		CSMain.Instance.DestoryCounter(cs);
		cs = null;
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
			m_CurTime = -1f;
			m_Time = -1f;
		}
	}

	public void OnTentFinish()
	{
		tentBuilding.OnTentFinish(this);
	}
}
