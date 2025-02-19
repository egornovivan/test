using UnityEngine;

public class CSUI_NPCSoldier : MonoBehaviour
{
	[SerializeField]
	private CSUI_SoldierPatrol m_PatrolInfoUI;

	[SerializeField]
	private UIPopupList m_ModeUI;

	[SerializeField]
	private UISprite m_PatrolModeUI;

	[SerializeField]
	private CSUI_EntityState m_EntityStatePrefab;

	private CSPersonnel m_RefNpc;

	private bool m_Active = true;

	private bool ShowStatusTips = true;

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
			UpdateModeUI();
			m_PatrolInfoUI.RefNpc = value;
		}
	}

	public void Activate(bool active)
	{
		if (m_Active != active)
		{
			m_Active = active;
			_activate();
		}
		else
		{
			m_Active = active;
		}
	}

	private void _activate()
	{
		if (!m_Active)
		{
			m_ModeUI.items.Clear();
			if (m_RefNpc != null)
			{
				m_ModeUI.items.Add(CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode));
			}
			else
			{
				m_ModeUI.items.Add("None");
			}
		}
		else
		{
			UpdateModeUI();
		}
	}

	public void Init()
	{
		CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);
	}

	private void OnEnable()
	{
		if (m_RefNpc != null && (m_RefNpc.m_WorkMode == 8 || m_RefNpc.m_WorkMode == 7))
		{
			m_PatrolInfoUI.gameObject.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		CSPersonnel.UnregisterOccupaChangedListener(OnOccupationChange);
	}

	private void Start()
	{
		_activate();
	}

	private void Update()
	{
	}

	private void UpdateModeUI()
	{
		if (m_Active)
		{
			m_ModeUI.items.Clear();
			if (m_RefNpc != null)
			{
				m_ModeUI.items.Add(CSUtils.GetWorkModeName(7));
				m_ModeUI.items.Add(CSUtils.GetWorkModeName(8));
				ShowStatusTips = false;
				m_ModeUI.selection = CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode);
				ShowStatusTips = true;
			}
			else
			{
				m_ModeUI.items.Add("None");
			}
		}
	}

	private void OnSelectionChange(string item)
	{
		if (item == CSUtils.GetWorkModeName(7))
		{
			if (m_RefNpc != null)
			{
				m_RefNpc.m_WorkMode = 7;
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mSoldierForPatrol.GetString(), 6f);
				}
			}
		}
		else if (item == CSUtils.GetWorkModeName(8) && m_RefNpc != null)
		{
			m_RefNpc.m_WorkMode = 8;
			if (ShowStatusTips)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mSoldierForGuard.GetString(), 6f);
			}
		}
		m_PatrolInfoUI.gameObject.SetActive(value: true);
	}

	private void OnOccupationChange(CSPersonnel person, int prvState)
	{
		if (person == m_RefNpc)
		{
			UpdateModeUI();
		}
	}

	private void OnPopupListClick()
	{
		if (!m_Active)
		{
			CSUI_StatusBar.ShowText(UIMsgBoxInfo.mCantHandlePersonnel.GetString(), Color.red, 5.5f);
		}
	}
}
