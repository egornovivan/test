using System.Collections.Generic;
using UnityEngine;

public class CSUI_SoldierPatrol : MonoBehaviour
{
	[SerializeField]
	private UITable m_EntityRootUI;

	[SerializeField]
	private UIScrollBar m_ScrollBar;

	[SerializeField]
	private CSUI_EntityState m_EntityStatePrefab;

	private List<CSUI_EntityState> m_EntitesState = new List<CSUI_EntityState>();

	private CSPersonnel m_RefNpc;

	private CSPersonnel m_OldRefNpc;

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
		}
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		ClearEntites();
		m_OldRefNpc = null;
	}

	private void ClearEntites()
	{
		foreach (CSUI_EntityState item in m_EntitesState)
		{
			Object.Destroy(item.gameObject);
			item.m_RefCommon.RemoveEventListener(OnEntityEventListener);
		}
		m_EntitesState.Clear();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_OldRefNpc == m_RefNpc)
		{
			return;
		}
		ClearEntites();
		List<CSEntity> protectedEntities = m_RefNpc.GetProtectedEntities();
		m_RefNpc.GuardEntities = protectedEntities;
		if (protectedEntities != null)
		{
			foreach (CSEntity item in protectedEntities)
			{
				CSUI_EntityState cSUI_EntityState = Object.Instantiate(m_EntityStatePrefab);
				cSUI_EntityState.transform.parent = m_EntityRootUI.transform;
				cSUI_EntityState.transform.localPosition = Vector3.zero;
				cSUI_EntityState.transform.localRotation = Quaternion.identity;
				cSUI_EntityState.transform.localScale = Vector3.one;
				cSUI_EntityState.m_RefCommon = item;
				m_EntitesState.Add(cSUI_EntityState);
				item.AddEventListener(OnEntityEventListener);
			}
			m_EntityRootUI.repositionNow = true;
			m_ScrollBar.scrollValue = 0f;
		}
		m_OldRefNpc = m_RefNpc;
	}

	private void OnEntityEventListener(int event_id, CSEntity entity, object arg)
	{
		CSCommon csc = entity as CSCommon;
		if (csc != null && event_id == 1)
		{
			csc.RemoveEventListener(OnEntityEventListener);
			CSUI_EntityState cSUI_EntityState = m_EntitesState.Find((CSUI_EntityState item0) => item0.m_RefCommon == csc);
			if (cSUI_EntityState != null)
			{
				Object.Destroy(cSUI_EntityState.gameObject);
				m_EntitesState.Remove(cSUI_EntityState);
				m_EntityRootUI.repositionNow = true;
			}
		}
	}
}
