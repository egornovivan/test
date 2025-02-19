using UnityEngine;

public class CSUI_NPCFarmer : MonoBehaviour
{
	public delegate void SelectItemDel(string item);

	[SerializeField]
	private UIPopupList m_ModeUI;

	[SerializeField]
	private UISprite m_ManageUI;

	[SerializeField]
	private UISprite m_HarvestUI;

	[SerializeField]
	private UISprite m_PlantUI;

	[SerializeField]
	private UILabel m_FarmUI;

	[SerializeField]
	private UILabel m_TotalFamersUI;

	[SerializeField]
	private UILabel m_FullTipUI;

	[SerializeField]
	private UILabel m_MangeNumUI;

	[SerializeField]
	private UILabel m_HarvestNumUI;

	[SerializeField]
	private UILabel m_PlantNumUI;

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
	}

	public void Init()
	{
		CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);
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
		if (RefNpc == null)
		{
			return;
		}
		CSMgCreator cSMgCreator = RefNpc.m_Creator as CSMgCreator;
		if (cSMgCreator != null && cSMgCreator.Assembly != null)
		{
			int entityCnt = cSMgCreator.Assembly.GetEntityCnt(CSConst.ObjectType.Farm);
			int limitCnt = cSMgCreator.Assembly.GetLimitCnt(CSConst.ObjectType.Farm);
			m_FarmUI.text = "[" + entityCnt + "/" + limitCnt + "]";
			if (entityCnt != 0)
			{
				CSFarm cSFarm = cSMgCreator.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Farm][0] as CSFarm;
				int workerCount = cSFarm.WorkerCount;
				int workerMaxCount = cSFarm.WorkerMaxCount;
				m_TotalFamersUI.text = workerCount + "/" + workerMaxCount;
				if (workerCount >= workerMaxCount)
				{
					m_TotalFamersUI.color = Color.red;
				}
				else
				{
					m_TotalFamersUI.color = Color.white;
				}
			}
			else
			{
				m_TotalFamersUI.text = "0/0";
			}
			m_MangeNumUI.text = cSMgCreator.FarmMgNum.ToString();
			m_HarvestNumUI.text = cSMgCreator.FarmHarvestNum.ToString();
			m_PlantNumUI.text = cSMgCreator.FarmPlantNum.ToString();
		}
		m_FullTipUI.gameObject.SetActive(value: false);
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
			m_ModeUI.items.Add(CSUtils.GetWorkModeName(4));
			m_ModeUI.items.Add(CSUtils.GetWorkModeName(5));
			m_ModeUI.items.Add(CSUtils.GetWorkModeName(6));
			m_ModeUI.selection = CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode);
		}
		else
		{
			m_ModeUI.items.Add("None");
		}
	}

	private void OnSelectionChange(string item)
	{
		if (item == CSUtils.GetWorkModeName(4))
		{
			m_ManageUI.enabled = true;
			m_HarvestUI.enabled = false;
			m_PlantUI.enabled = false;
			if (m_RefNpc != null)
			{
				m_RefNpc.TrySetWorkMode(4);
			}
			if (ShowStatusTips)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mFarmerForManage.GetString(), 6f);
			}
		}
		else if (item == CSUtils.GetWorkModeName(5))
		{
			m_ManageUI.enabled = false;
			m_HarvestUI.enabled = true;
			m_PlantUI.enabled = false;
			if (m_RefNpc != null)
			{
				m_RefNpc.TrySetWorkMode(5);
			}
			if (ShowStatusTips)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mFarmerForHarvest.GetString(), 6f);
			}
		}
		else if (item == CSUtils.GetWorkModeName(6))
		{
			m_ManageUI.enabled = false;
			m_HarvestUI.enabled = true;
			m_PlantUI.enabled = false;
			if (m_RefNpc != null)
			{
				m_RefNpc.TrySetWorkMode(6);
			}
			if (ShowStatusTips)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mFarmerForPlant.GetString(), 6f);
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
