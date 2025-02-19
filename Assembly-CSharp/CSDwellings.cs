using System.Collections.Generic;
using CSRecord;
using Pathea;
using Pathea.Operate;
using Pathea.PeEntityExt;
using UnityEngine;

public class CSDwellings : CSCommon
{
	private CSDwellingsData m_DData;

	public CSDwellingsInfo m_DInfo;

	public CSPersonnel[] m_NPCS;

	public PEBed m_Beds;

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
			m_Beds = gameLogic.GetComponentInParent<PEBed>();
			if (!(m_Beds != null))
			{
				return;
			}
			for (int i = 0; i < m_NPCS.Length; i++)
			{
				if (m_NPCS[i] != null)
				{
					m_NPCS[i].Bed = m_Beds;
				}
			}
		}
	}

	public CSDwellingsData Data
	{
		get
		{
			if (m_DData == null)
			{
				m_DData = m_Data as CSDwellingsData;
			}
			return m_DData;
		}
	}

	public CSDwellingsInfo Info
	{
		get
		{
			if (m_DInfo == null)
			{
				m_DInfo = m_Info as CSDwellingsInfo;
			}
			return m_DInfo;
		}
	}

	public CSDwellings()
	{
		m_Type = 21;
		m_NPCS = new CSPersonnel[4];
		m_Grade = 1;
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning;
	}

	public bool HasSpace()
	{
		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == null)
			{
				return true;
			}
		}
		return false;
	}

	public int GetEmptySpace()
	{
		int num = 0;
		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == null)
			{
				num++;
			}
		}
		return num;
	}

	public bool AddNpcs(CSPersonnel npc)
	{
		if (npc == null)
		{
			return false;
		}
		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == null)
			{
				m_NPCS[i] = npc;
				npc.Bed = m_Beds;
				npc.Dwellings = this;
				return true;
			}
		}
		return false;
	}

	public void RemoveNpc(CSPersonnel npc)
	{
		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == npc)
			{
				m_NPCS[i] = null;
				break;
			}
		}
	}

	public void RemoveNpc(int index)
	{
		if (index < m_NPCS.Length)
		{
			m_NPCS[index] = null;
		}
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		Dictionary<int, CSCommon> commonEntities = m_Creator.GetCommonEntities();
		List<CSDwellings> list = new List<CSDwellings>();
		foreach (CSCommon value in commonEntities.Values)
		{
			if (value != this && value is CSDwellings)
			{
				list.Add(value as CSDwellings);
			}
		}
		if (PeGameMgr.IsMulti)
		{
			return;
		}
		for (int i = 0; i < m_NPCS.Length; i++)
		{
			if (m_NPCS[i] == null)
			{
				continue;
			}
			foreach (CSDwellings item in list)
			{
				if (item.AddNpcs(m_NPCS[i]))
				{
					m_NPCS[i].ResetCmd();
					m_NPCS[i] = null;
				}
			}
		}
		CSPersonnel[] nPCS = m_NPCS;
		foreach (CSPersonnel cSPersonnel in nPCS)
		{
			if (cSPersonnel != null)
			{
				m_Creator.RemoveNpc(cSPersonnel.NPC);
				cSPersonnel.NPC.Dismiss();
			}
		}
	}

	public override void ChangeState()
	{
		base.ChangeState();
		if (!PeGameMgr.IsMulti)
		{
			if (!m_IsRunning)
			{
				CSMgCreator cSMgCreator = m_Creator as CSMgCreator;
				if (cSMgCreator != null)
				{
					Dictionary<int, CSCommon> commonEntities = cSMgCreator.GetCommonEntities();
					int i = 0;
					foreach (KeyValuePair<int, CSCommon> item in commonEntities)
					{
						if (item.Value.m_Type != 21 || !item.Value.IsRunning)
						{
							continue;
						}
						CSDwellings cSDwellings = item.Value as CSDwellings;
						for (; i < m_NPCS.Length; i++)
						{
							if (m_NPCS[i] != null)
							{
								if (!cSDwellings.AddNpcs(m_NPCS[i]))
								{
									break;
								}
								RemoveNpc(i);
							}
						}
						if (i != m_NPCS.Length)
						{
							continue;
						}
						break;
					}
				}
			}
			else
			{
				CSMgCreator cSMgCreator2 = m_Creator as CSMgCreator;
				if (cSMgCreator2 != null && HasSpace())
				{
					Dictionary<int, CSCommon> commonEntities2 = cSMgCreator2.GetCommonEntities();
					foreach (KeyValuePair<int, CSCommon> item2 in commonEntities2)
					{
						if (item2.Value.m_Type != 21 || item2.Value.IsRunning)
						{
							continue;
						}
						CSDwellings cSDwellings2 = item2.Value as CSDwellings;
						bool flag = false;
						for (int j = 0; j < cSDwellings2.m_NPCS.Length; j++)
						{
							if (cSDwellings2.m_NPCS[j] != null)
							{
								if (!AddNpcs(cSDwellings2.m_NPCS[j]))
								{
									flag = true;
									break;
								}
								cSDwellings2.RemoveNpc(j);
							}
						}
						if (!flag)
						{
							continue;
						}
						break;
					}
				}
			}
		}
		ExcuteEvent(4001, m_IsRunning);
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 21, ref refData) : MultiColonyManager.Instance.AssignData(ID, 21, ref refData, _ColonyObj));
		m_Data = refData as CSDwellingsData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		}
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
		if (m_CSRepair != null)
		{
			Object.Destroy(m_CSRepair);
		}
		if (m_CSDelete != null)
		{
			Object.Destroy(m_CSDelete);
		}
	}

	public override void Update()
	{
		base.Update();
	}
}
