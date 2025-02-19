using System;
using System.Collections.Generic;
using UnityEngine;

public class CSUI_Dwellings : MonoBehaviour
{
	[Serializable]
	public class SubDwellingsPart
	{
		public UIGrid m_Root;

		public CSUI_SubDwellings m_Prefab;

		public Transform m_IconRadio;
	}

	[Serializable]
	public class PagePart
	{
		public UILabel m_ValueLb;

		public UIButton m_UpBtn;

		public UIButton m_DownBtn;

		public int m_PageCount;

		[HideInInspector]
		public int m_Index;

		[HideInInspector]
		public int m_MaxPageCount;
	}

	[Serializable]
	public class NPCHandleBtns
	{
		public N_ImageButton m_Rest;

		public N_ImageButton m_Work;

		public N_ImageButton m_FollowMe;

		public N_ImageButton m_Idle;
	}

	[SerializeField]
	private Transform m_QuaryMatWndRoot;

	[SerializeField]
	private SubDwellingsPart m_SubDwellings;

	private List<CSUI_SubDwellings> m_SubDwellingsList = new List<CSUI_SubDwellings>();

	[SerializeField]
	private PagePart m_Page;

	[SerializeField]
	private NPCHandleBtns m_NPCHandleBtns;

	private CSPersonnel m_ActiveNpc;

	public void PageTurning(int pageIndex)
	{
		if (m_Page.m_MaxPageCount < pageIndex && pageIndex >= 0)
		{
			return;
		}
		m_Page.m_Index = pageIndex;
		int num = m_Page.m_PageCount * pageIndex;
		int num2 = num + m_Page.m_PageCount;
		for (int i = 0; i < m_SubDwellingsList.Count; i++)
		{
			if (i < num || i >= num2)
			{
				m_SubDwellingsList[i].gameObject.SetActive(value: false);
			}
			else
			{
				m_SubDwellingsList[i].gameObject.SetActive(value: true);
			}
		}
		m_SubDwellings.m_Root.repositionNow = true;
	}

	public bool IsEmpty()
	{
		return m_SubDwellingsList.Count == 0;
	}

	public void SetEntityList(List<CSEntity> entityList)
	{
		for (int i = 0; i < entityList.Count; i++)
		{
			if (!m_SubDwellingsList.Exists((CSUI_SubDwellings item0) => item0.m_Entity == entityList[i]))
			{
				AddDwellings(entityList[i] as CSDwellings);
			}
		}
		int j;
		for (j = 0; j < m_SubDwellingsList.Count; j++)
		{
			if (!entityList.Exists((CSEntity item0) => item0 == m_SubDwellingsList[j].m_Entity))
			{
				RomveDwellings(m_SubDwellingsList[j].m_Dwellings);
			}
		}
	}

	public void AddDwellings(CSDwellings dwellings)
	{
		if (!m_SubDwellingsList.Exists((CSUI_SubDwellings item0) => item0.m_Dwellings == dwellings))
		{
			CSUI_SubDwellings cSUI_SubDwellings = UnityEngine.Object.Instantiate(m_SubDwellings.m_Prefab);
			cSUI_SubDwellings.transform.parent = m_SubDwellings.m_Root.transform;
			cSUI_SubDwellings.transform.localPosition = Vector3.zero;
			cSUI_SubDwellings.transform.localRotation = Quaternion.identity;
			cSUI_SubDwellings.transform.localScale = Vector3.one;
			cSUI_SubDwellings.m_IconRadioRoot = m_SubDwellings.m_IconRadio;
			cSUI_SubDwellings.m_QueryMatRoot = m_QuaryMatWndRoot;
			cSUI_SubDwellings.m_Entity = dwellings;
			cSUI_SubDwellings.m_Dwellings = dwellings;
			cSUI_SubDwellings.gameObject.name = m_SubDwellingsList.Count.ToString();
			cSUI_SubDwellings.m_DwelingsUI = this;
			cSUI_SubDwellings.gameObject.SetActive(value: true);
			m_SubDwellingsList.Add(cSUI_SubDwellings);
			int num = m_SubDwellingsList.Count / m_Page.m_PageCount;
			m_Page.m_MaxPageCount = ((m_SubDwellingsList.Count % m_Page.m_PageCount != 0) ? (num + 1) : num);
			if (m_Page.m_Index + m_Page.m_PageCount < m_SubDwellingsList.Count)
			{
				cSUI_SubDwellings.gameObject.SetActive(value: false);
			}
			else
			{
				m_SubDwellings.m_Root.repositionNow = true;
			}
		}
		else
		{
			Debug.LogWarning("The Dwellings that you want to add into UI is areadly exsts!");
		}
	}

	public void RomveDwellings(CSDwellings dwellings)
	{
		int num = m_SubDwellingsList.FindIndex((CSUI_SubDwellings item0) => item0.m_Dwellings == dwellings);
		if (num != -1)
		{
			CSUI_SubDwellings cSUI_SubDwellings = m_SubDwellingsList[num];
			m_SubDwellingsList.RemoveAt(num);
			UnityEngine.Object.DestroyImmediate(cSUI_SubDwellings.gameObject);
			for (int i = num; i < m_SubDwellingsList.Count; i++)
			{
				m_SubDwellingsList[i].gameObject.name = i.ToString();
			}
			int num2 = m_SubDwellingsList.Count / m_Page.m_PageCount;
			m_Page.m_MaxPageCount = ((m_SubDwellingsList.Count % m_Page.m_PageCount != 0) ? (num2 + 1) : num2);
			PageTurning(m_Page.m_Index);
		}
		else
		{
			Debug.LogWarning("The Dwellings that you want to remove is not exsist!");
		}
	}

	private void OnPageUpBtn()
	{
		PageTurning(m_Page.m_Index - 1);
	}

	private void OnPageDownBtn()
	{
		PageTurning(m_Page.m_Index + 1);
	}

	public void OnNPCGridClick(GameObject go, bool active)
	{
		if (active)
		{
			CSUI_NPCGrid component = go.GetComponent<CSUI_NPCGrid>();
			m_ActiveNpc = component.m_Npc;
		}
	}

	private void OnFollowMeClick()
	{
		if (m_ActiveNpc != null)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000096), SetActiveNpcFollow);
		}
		else
		{
			Debug.LogWarning("The Active Npc is not exist!");
		}
	}

	private void SetActiveNpcFollow()
	{
		m_ActiveNpc.FollowMe(follow: true);
	}

	private void OnCallClick()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_Page.m_Index == 0)
		{
			m_Page.m_UpBtn.gameObject.SetActive(value: false);
		}
		else
		{
			m_Page.m_UpBtn.gameObject.SetActive(value: true);
		}
		if (m_Page.m_Index == m_Page.m_MaxPageCount - 1)
		{
			m_Page.m_DownBtn.gameObject.SetActive(value: false);
		}
		else
		{
			m_Page.m_DownBtn.gameObject.SetActive(value: true);
		}
		m_Page.m_ValueLb.text = m_Page.m_Index + 1 + " / " + m_Page.m_MaxPageCount;
		if (m_ActiveNpc != null)
		{
			m_NPCHandleBtns.m_Rest.isEnabled = true;
			m_NPCHandleBtns.m_FollowMe.isEnabled = true;
			m_NPCHandleBtns.m_Work.isEnabled = true;
			m_NPCHandleBtns.m_Idle.isEnabled = true;
		}
		else
		{
			m_NPCHandleBtns.m_FollowMe.isEnabled = false;
			m_NPCHandleBtns.m_Rest.isEnabled = false;
			m_NPCHandleBtns.m_Work.isEnabled = false;
		}
	}
}
