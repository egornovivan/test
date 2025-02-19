using System.Collections.Generic;
using UnityEngine;

public class CSUI_NPCOccupation : MonoBehaviour
{
	public delegate void SelectItemDel(string item);

	[SerializeField]
	private UIPopupList m_OccupationUI;

	[SerializeField]
	private UISprite m_DwellerIconUI;

	[SerializeField]
	private UISprite m_WorkerIconUI;

	[SerializeField]
	private UISprite m_FarmerIconUI;

	[SerializeField]
	private UISprite m_SoldierIconUI;

	[SerializeField]
	private UISprite m_FollowerIcomUI;

	[SerializeField]
	private UISprite m_ProcessorIcomUI;

	[SerializeField]
	private UISprite m_DoctorIconUI;

	[SerializeField]
	private UISprite m_InstructorIconUI;

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
			UpdatePopupList();
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
			m_OccupationUI.items.Clear();
			if (m_RefNpc != null)
			{
				m_OccupationUI.items.Add(CSUtils.GetOccupaName(m_RefNpc.Occupation));
			}
			else
			{
				m_OccupationUI.items.Add(CSUtils.GetOccupaName(0));
			}
		}
		else
		{
			UpdatePopupList();
		}
	}

	private void Awake()
	{
		CSPersonnel.RegisterOccupaChangedListener(OnOccupationChanged);
	}

	private void OnDestroy()
	{
		CSPersonnel.UnRegisterStateChangedListener(OnOccupationChanged);
	}

	private void Start()
	{
		_activate();
	}

	private void Update()
	{
	}

	public void UpdatePopupList()
	{
		if (!m_Active)
		{
			_activate();
			return;
		}
		m_OccupationUI.items.Clear();
		if (m_RefNpc == null)
		{
			m_OccupationUI.items.Add("None");
			return;
		}
		List<CSEntity> protectedEntities = m_RefNpc.GetProtectedEntities();
		if (protectedEntities != null)
		{
			m_OccupationUI.items.Add(CSUtils.GetOccupaName(2));
		}
		CSMgCreator cSMgCreator = RefNpc.m_Creator as CSMgCreator;
		if (null != cSMgCreator && cSMgCreator.Assembly != null && cSMgCreator.Assembly.GetEntityCnt(CSConst.ObjectType.Farm) != 0)
		{
			m_OccupationUI.items.Add(CSUtils.GetOccupaName(3));
		}
		CSCreator creator = CSUI_MainWndCtrl.Instance.Creator;
		if (creator == null)
		{
			return;
		}
		Dictionary<int, CSCommon> commonEntities = creator.GetCommonEntities();
		foreach (KeyValuePair<int, CSCommon> item in commonEntities)
		{
			if (item.Value.Assembly != null && item.Value.WorkerMaxCount > 0 && item.Value is CSHealth)
			{
				m_OccupationUI.items.Add(CSUtils.GetOccupaName(6));
				break;
			}
		}
		foreach (KeyValuePair<int, CSCommon> item2 in commonEntities)
		{
			if (item2.Value.Assembly != null && item2.Value.WorkerMaxCount > 0 && item2.Value is CSWorkerMachine)
			{
				m_OccupationUI.items.Add(CSUtils.GetOccupaName(1));
				break;
			}
		}
		if (CSUI_MainWndCtrl.Instance.m_Menu.m_TrainingMI.IsShow)
		{
			m_OccupationUI.items.Add(CSUtils.GetOccupaName(7));
		}
		if (CSUI_MainWndCtrl.Instance.m_Menu.m_CollectMI.IsShow)
		{
			m_OccupationUI.items.Add(CSUtils.GetOccupaName(5));
		}
		m_OccupationUI.items.Add(CSUtils.GetOccupaName(0));
		if (m_RefNpc.IsRandomNpc)
		{
			m_OccupationUI.items.Add(CSUtils.GetOccupaName(4));
		}
		ShowStatusTips = false;
		m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
		ShowStatusTips = true;
	}

	private void OnSelectionChange(string item)
	{
		if (m_RefNpc == null || null == m_RefNpc.NPC)
		{
			return;
		}
		if (m_RefNpc.NPC.aliveEntity.isDead)
		{
			CSUI_StatusBar.ShowText(UIMsgBoxInfo.mNpdDeadNotChangeProfession.GetString(), Color.red, 5.5f);
			m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
			return;
		}
		HideAllProfessionalSpr();
		if (item == CSUtils.GetOccupaName(0))
		{
			if (m_RefNpc.TrySetOccupation(0))
			{
				m_DwellerIconUI.enabled = true;
				if (onSelectChange != null)
				{
					onSelectChange(item);
				}
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForDweller.GetString(), m_RefNpc.FullName), 5.5f);
				}
			}
			else
			{
				m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
				}
			}
		}
		else if (item == CSUtils.GetOccupaName(1))
		{
			if (m_RefNpc.TrySetOccupation(1))
			{
				m_WorkerIconUI.enabled = true;
				if (onSelectChange != null)
				{
					onSelectChange(item);
				}
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForWorker.GetString(), m_RefNpc.FullName), 5.5f);
				}
			}
			else
			{
				m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
				}
			}
		}
		else if (item == CSUtils.GetOccupaName(3))
		{
			if (CSMain.s_MgCreator.Farmers.Count >= 8)
			{
				m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(82201069), m_RefNpc.FullName), 5.5f);
				}
				return;
			}
			if (m_RefNpc != null)
			{
				if (m_RefNpc.TrySetOccupation(3))
				{
					m_FarmerIconUI.enabled = true;
					if (onSelectChange != null)
					{
						onSelectChange(item);
					}
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForFarmer.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
				else
				{
					m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
			}
		}
		else if (item == CSUtils.GetOccupaName(2))
		{
			if (m_RefNpc != null)
			{
				if (m_RefNpc.TrySetOccupation(2))
				{
					m_SoldierIconUI.enabled = true;
					if (onSelectChange != null)
					{
						onSelectChange(item);
					}
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForSolider.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
				else
				{
					m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
			}
		}
		else if (item == CSUtils.GetOccupaName(4))
		{
			if (m_RefNpc != null)
			{
				if (m_RefNpc.TrySetOccupation(4))
				{
					m_FollowerIcomUI.enabled = true;
					if (onSelectChange != null)
					{
						onSelectChange(item);
					}
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForFollower.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
				else
				{
					m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
			}
		}
		else if (item == CSUtils.GetOccupaName(5))
		{
			if (CSMain.s_MgCreator.Processors.Count >= 5)
			{
				m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(82201069), m_RefNpc.FullName), 5.5f);
				}
				return;
			}
			if (m_RefNpc != null)
			{
				if (m_RefNpc.TrySetOccupation(5))
				{
					m_ProcessorIcomUI.enabled = true;
					if (onSelectChange != null)
					{
						onSelectChange(item);
					}
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForProcessor.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
				else
				{
					m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
			}
		}
		else if (item == CSUtils.GetOccupaName(6))
		{
			if (m_RefNpc != null)
			{
				if (m_RefNpc.TrySetOccupation(6))
				{
					m_DoctorIconUI.enabled = true;
					if (onSelectChange != null)
					{
						onSelectChange(item);
					}
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForDoctor.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
				else
				{
					m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
					if (ShowStatusTips)
					{
						CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
					}
				}
			}
		}
		else if (item == CSUtils.GetOccupaName(7) && m_RefNpc != null)
		{
			if (m_RefNpc.TrySetOccupation(7))
			{
				m_InstructorIconUI.enabled = true;
				if (onSelectChange != null)
				{
					onSelectChange(item);
				}
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionForInstructor.GetString(), m_RefNpc.FullName), 5.5f);
				}
			}
			else
			{
				m_OccupationUI.selection = CSUtils.GetOccupaName(m_RefNpc.m_Occupation);
				if (ShowStatusTips)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mProfessionFailed.GetString(), m_RefNpc.FullName), 5.5f);
				}
			}
		}
		if (!GameConfig.IsMultiMode)
		{
		}
	}

	private void HideAllProfessionalSpr()
	{
		m_DwellerIconUI.enabled = false;
		m_WorkerIconUI.enabled = false;
		m_FarmerIconUI.enabled = false;
		m_SoldierIconUI.enabled = false;
		m_FollowerIcomUI.enabled = false;
		m_ProcessorIcomUI.enabled = false;
		m_DoctorIconUI.enabled = false;
		m_InstructorIconUI.enabled = false;
	}

	private void OnOccupationChanged(CSPersonnel person, int prvState)
	{
		if (person == m_RefNpc && person.IsRandomNpc && m_OccupationUI.selection != CSUtils.GetOccupaName(person.Occupation))
		{
			m_OccupationUI.selection = CSUtils.GetOccupaName(person.Occupation);
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
