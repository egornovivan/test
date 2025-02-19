using System;
using System.Collections.Generic;
using UnityEngine;

public class CSUI_SubDwellings : CSUI_Base
{
	[Serializable]
	public class NPCPart
	{
		public UIGrid m_Root;

		public CSUI_NPCGrid m_Prefab;
	}

	public CSDwellings m_Dwellings;

	public CSUI_Dwellings m_DwelingsUI;

	[SerializeField]
	private NPCPart m_NpcPart;

	private List<CSUI_NPCGrid> m_NpcGirds = new List<CSUI_NPCGrid>();

	[SerializeField]
	private UILabel m_NameUI;

	public Transform m_IconRadioRoot;

	public string Description
	{
		get
		{
			return m_NameUI.text;
		}
		set
		{
			m_NameUI.text = value;
		}
	}

	private void Awake()
	{
	}

	private new void Start()
	{
		base.Start();
		for (int i = 0; i < 4; i++)
		{
			CSUI_NPCGrid cSUI_NPCGrid = UnityEngine.Object.Instantiate(m_NpcPart.m_Prefab);
			cSUI_NPCGrid.transform.parent = m_NpcPart.m_Root.transform;
			cSUI_NPCGrid.transform.localPosition = Vector3.zero;
			cSUI_NPCGrid.transform.localRotation = Quaternion.identity;
			cSUI_NPCGrid.transform.localScale = Vector3.one;
			cSUI_NPCGrid.NpcIconRadio = m_IconRadioRoot;
			cSUI_NPCGrid.m_UseDeletebutton = false;
			UIEventListener.Get(cSUI_NPCGrid.gameObject).onActivate = m_DwelingsUI.OnNPCGridClick;
			m_NpcGirds.Add(cSUI_NPCGrid);
		}
		m_NpcPart.m_Root.repositionNow = true;
		for (int j = 0; j < m_NpcGirds.Count; j++)
		{
			if (j < m_Dwellings.m_NPCS.Length)
			{
				m_NpcGirds[j].m_Npc = m_Dwellings.m_NPCS[j];
			}
			else
			{
				m_NpcGirds[j].m_Npc = null;
			}
		}
	}

	private new void Update()
	{
		base.Update();
		for (int i = 0; i < m_NpcGirds.Count; i++)
		{
			if (i < m_Dwellings.m_NPCS.Length)
			{
				m_NpcGirds[i].m_Npc = m_Dwellings.m_NPCS[i];
			}
			else
			{
				m_NpcGirds[i].m_Npc = null;
			}
		}
	}
}
