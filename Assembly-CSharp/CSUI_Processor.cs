using UnityEngine;

public class CSUI_Processor : MonoBehaviour
{
	[SerializeField]
	private CSUI_ProcessorItem[] m_ProItems;

	[SerializeField]
	private CSUI_ProcessorItem m_ProItemsNull;

	private CSPersonnel m_RefNpc;

	private bool m_Active = true;

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
			if (m_RefNpc != null)
			{
				InitProcessChose(m_RefNpc.ProcessingIndex);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		UpdateTime();
		UpdateProcessingIndex();
	}

	public void Init()
	{
		InitEvent();
	}

	private void InitProcessChose(int index)
	{
		if (index == -1)
		{
			m_ProItemsNull.isChecked = true;
		}
		else
		{
			m_ProItems[index].isChecked = true;
		}
	}

	private void UpdateProcessingIndex()
	{
		if (m_RefNpc != null)
		{
			InitProcessChose(m_RefNpc.ProcessingIndex);
		}
	}

	private void OnDestroy()
	{
		CSPersonnel.UnregisterOccupaChangedListener(OnOccupationChange);
	}

	private void UpdateTime()
	{
		if (!(CSUI_MainWndCtrl.Instance == null) && !(CSUI_MainWndCtrl.Instance.CollectUI == null) && CSUI_MainWndCtrl.Instance.CollectUI.m_Processes != null)
		{
			for (int i = 0; i < m_ProItems.Length; i++)
			{
				m_ProItems[i].SetTime(CSUI_MainWndCtrl.Instance.CollectUI.m_Processes[i].Times);
			}
		}
	}

	private void InitEvent()
	{
		CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);
		for (int i = 0; i < m_ProItems.Length; i++)
		{
			m_ProItems[i].e_JionEvent += OnJionin;
			m_ProItems[i].e_DoubleClickEvent += OnDoubleClick;
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
		}
	}

	private void OnJionin(object sender, int index)
	{
		if (m_RefNpc != null)
		{
			m_RefNpc.TrySetProcessingIndex(index);
			if (m_RefNpc.CanProcess && !CheckProcessHasMaterial(index))
			{
				CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(82209012));
			}
		}
	}

	private void OnDoubleClick(GameObject go, int index)
	{
		if (m_RefNpc != null && m_RefNpc.CanProcess && (bool)CSUI_MainWndCtrl.Instance)
		{
			CSUI_MainWndCtrl.Instance.GoToCollectWnd(index);
		}
	}

	private bool CheckProcessHasMaterial(int index)
	{
		if ((bool)CSUI_MainWndCtrl.Instance && CSUI_MainWndCtrl.Instance.CollectUI.mProlists.ContainsKey(index))
		{
			return CSUI_MainWndCtrl.Instance.CollectUI.mProlists[index].Count > 0;
		}
		return false;
	}

	private void OnOccupationChange(CSPersonnel person, int prvState)
	{
		if (person == m_RefNpc && m_RefNpc != null)
		{
			InitProcessChose(m_RefNpc.ProcessingIndex);
		}
	}
}
