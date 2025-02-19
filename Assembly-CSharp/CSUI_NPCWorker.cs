using System.Collections.Generic;
using UnityEngine;

public class CSUI_NPCWorker : MonoBehaviour
{
	public delegate void SelectItemDel(string item);

	[SerializeField]
	private UIGrid m_WorkRoomRootUI;

	[SerializeField]
	private UIPopupList m_ModeUI;

	[SerializeField]
	private UISprite m_NormalModeUI;

	[SerializeField]
	private UISprite m_WorkWhenNeedUI;

	[SerializeField]
	private UISprite m_WorkaholicUI;

	[SerializeField]
	private CSUI_WorkRoom WorkRoomUIPrefab;

	private List<CSUI_WorkRoom> m_WorkRooms = new List<CSUI_WorkRoom>();

	private CSPersonnel m_RefNpc;

	public SelectItemDel onSelectChange;

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
			UpdateWorkRoom();
			UpdateModeUI();
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
		for (int i = 0; i < m_WorkRooms.Count; i++)
		{
			m_WorkRooms[i].Activate(m_Active);
		}
	}

	public void Init()
	{
		CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);
	}

	private void OnEnable()
	{
		CSCreator creator = CSUI_MainWndCtrl.Instance.Creator;
		if (creator == null)
		{
			return;
		}
		Dictionary<int, CSCommon> commonEntities = creator.GetCommonEntities();
		foreach (KeyValuePair<int, CSCommon> item in commonEntities)
		{
			if (item.Value.Assembly != null && item.Value.WorkerMaxCount > 0 && item.Value is CSWorkerMachine)
			{
				CSUI_WorkRoom cSUI_WorkRoom = Object.Instantiate(WorkRoomUIPrefab);
				cSUI_WorkRoom.transform.parent = m_WorkRoomRootUI.transform;
				cSUI_WorkRoom.transform.localPosition = Vector3.zero;
				cSUI_WorkRoom.transform.localRotation = Quaternion.identity;
				cSUI_WorkRoom.transform.localScale = Vector3.one;
				cSUI_WorkRoom.m_RefCommon = item.Value;
				cSUI_WorkRoom.m_RefNpc = RefNpc;
				m_WorkRooms.Add(cSUI_WorkRoom);
			}
		}
		m_WorkRoomRootUI.repositionNow = true;
	}

	private void OnDisable()
	{
		foreach (CSUI_WorkRoom workRoom in m_WorkRooms)
		{
			Object.Destroy(workRoom.gameObject);
		}
		m_WorkRooms.Clear();
	}

	private void Start()
	{
		_activate();
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		CSPersonnel.UnregisterOccupaChangedListener(OnOccupationChange);
	}

	private void Update()
	{
	}

	private void UpdateWorkRoom()
	{
		foreach (CSUI_WorkRoom workRoom in m_WorkRooms)
		{
			workRoom.m_RefNpc = m_RefNpc;
		}
	}

	private void UpdateModeUI()
	{
		if (!m_Active)
		{
			_activate();
			return;
		}
		m_ModeUI.items.Clear();
		if (m_RefNpc != null)
		{
			if (m_RefNpc.m_Occupation == 1)
			{
				m_ModeUI.items.Add(CSUtils.GetWorkModeName(1));
				m_ModeUI.items.Add(CSUtils.GetWorkModeName(2));
				m_ModeUI.items.Add(CSUtils.GetWorkModeName(3));
			}
			ShowStatusTips = false;
			m_ModeUI.selection = CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode);
			ShowStatusTips = true;
		}
		else
		{
			m_ModeUI.items.Add("None");
		}
	}

	private void OnSelectionChange(string item)
	{
		if (item == CSUtils.GetWorkModeName(1))
		{
			m_NormalModeUI.enabled = true;
			m_WorkWhenNeedUI.enabled = false;
			m_WorkaholicUI.enabled = false;
			if (m_RefNpc != null)
			{
				m_RefNpc.m_WorkMode = 1;
			}
			if (ShowStatusTips)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mWorkerForNormal.GetString(), 6f);
			}
		}
		else if (item == CSUtils.GetWorkModeName(2))
		{
			m_NormalModeUI.enabled = false;
			m_WorkWhenNeedUI.enabled = true;
			m_WorkaholicUI.enabled = false;
			if (m_RefNpc != null)
			{
				m_RefNpc.m_WorkMode = 2;
			}
			if (ShowStatusTips)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mWorkerForWorkWhenNeed.GetString(), 6f);
			}
		}
		else if (item == CSUtils.GetWorkModeName(3))
		{
			m_NormalModeUI.enabled = false;
			m_WorkWhenNeedUI.enabled = false;
			m_WorkaholicUI.enabled = true;
			if (m_RefNpc != null)
			{
				m_RefNpc.m_WorkMode = 3;
			}
			if (ShowStatusTips)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mWorkerForWorkaholic.GetString(), 6f);
			}
		}
		if (onSelectChange != null)
		{
			onSelectChange(item);
		}
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
