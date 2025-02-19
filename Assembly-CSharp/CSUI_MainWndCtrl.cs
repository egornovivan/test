using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class CSUI_MainWndCtrl : UIBaseWnd
{
	[Serializable]
	public class WindowPart
	{
		public CSUI_Assembly m_AssemblyUI;

		public CSUI_PPCoal m_PPCoalUI;

		public CSUI_Storage m_StorageUI;

		public CSUI_Engineering m_EngineeringUI;

		public CSUI_Dwellings m_DwellingsUI;

		public CSUI_Farm m_FarmUI;

		public CSUI_Factory m_FactoryUI;

		public CSUI_Personnel m_PersonnelUI;

		public CSUI_TradingPost m_TradingPostUI;

		public CSUI_CollectWnd m_CollectUI;

		public CSUI_TrainMgr m_TrainUI;

		public CSUI_Hospital m_HospitalUI;
	}

	[Serializable]
	public class MenuPart
	{
		public UIGrid mLeftMenuGrid;

		public CSUI_LeftMenuItem m_PersonnelMI;

		public CSUI_LeftMenuItem m_AssemblyMI;

		public CSUI_LeftMenuItem m_PPCoalMI;

		public CSUI_LeftMenuItem m_FarmMI;

		public CSUI_LeftMenuItem m_FactoryMI;

		public CSUI_LeftMenuItem m_StorageMI;

		public CSUI_LeftMenuItem m_EngineeringlMI;

		public CSUI_LeftMenuItem m_DwellingsMI;

		public CSUI_LeftMenuItem m_TransactionMI;

		public CSUI_LeftMenuItem m_CollectMI;

		public CSUI_LeftMenuItem m_HospitalMI;

		public CSUI_LeftMenuItem m_TrainingMI;
	}

	public enum EWorkType
	{
		Working,
		OutOfDistance,
		NoAssembly,
		UnKnown
	}

	private CSCreator m_Creator;

	private static CSUI_MainWndCtrl m_Instance;

	[SerializeField]
	private CSUI_PopupHint m_PopupHintPrefab;

	[SerializeField]
	private WindowPart m_Windows;

	public CSUI_Base m_OptionWnd;

	public CSUI_WorkWnd m_WorkWnd;

	[SerializeField]
	private CSUI_LeftMenuItem mSelectedMenuItem;

	[SerializeField]
	public MenuPart m_Menu;

	[SerializeField]
	private List<CSUI_LeftMenuItem> m_ActiveMenuList = new List<CSUI_LeftMenuItem>();

	[SerializeField]
	private GameObject mSkillLock;

	public float WorkDistance = 100f;

	private EWorkType m_WorkMode;

	[HideInInspector]
	public int mWndPartTag = -1;

	[HideInInspector]
	public GameObject m_ChildWindowOfBed;

	public CSCreator Creator
	{
		get
		{
			return m_Creator;
		}
		set
		{
			if (m_Creator != null)
			{
				m_Creator.UnregisterListener(OnCreatorEventListener);
				m_Creator.UnregisterPeronnelListener(OnCreatorEventListenerForPersonnel);
			}
			m_Creator = value;
			if (m_Creator != null)
			{
				RefreshMenu();
				if (isShow)
				{
					SelectBackupOrDefault();
				}
				m_Creator.RegisterListener(OnCreatorEventListener);
				m_Creator.RegisterPersonnelListener(OnCreatorEventListenerForPersonnel);
			}
		}
	}

	public static CSUI_MainWndCtrl Instance => m_Instance;

	public CSUI_Assembly AssemblyUI => m_Windows.m_AssemblyUI;

	public CSUI_PPCoal PPCoalUI => m_Windows.m_PPCoalUI;

	public CSUI_Storage StorageUI => m_Windows.m_StorageUI;

	public CSUI_Engineering EngineeringUI => m_Windows.m_EngineeringUI;

	public CSUI_Dwellings DwellingsUI => m_Windows.m_DwellingsUI;

	public CSUI_Farm FarmUI => m_Windows.m_FarmUI;

	public CSUI_Factory FactoryUI => m_Windows.m_FactoryUI;

	public CSUI_Personnel PersonnelUI => m_Windows.m_PersonnelUI;

	public CSUI_TradingPost TradingPostUI => m_Windows.m_TradingPostUI;

	public CSUI_CollectWnd CollectUI => m_Windows.m_CollectUI;

	public CSUI_TrainMgr TrainUI => m_Windows.m_TrainUI;

	public CSUI_Hospital HospitalUI => m_Windows.m_HospitalUI;

	public CSEntity mSelectedEnntity
	{
		set
		{
			m_OptionWnd.m_Entity = value;
			m_WorkWnd.m_Entity = value;
		}
	}

	public static EWorkType WorkType
	{
		get
		{
			if (m_Instance == null)
			{
				return EWorkType.UnKnown;
			}
			return m_Instance.m_WorkMode;
		}
	}

	private void OnCreatorEventListener(int event_type, CSEntity entity)
	{
		RefreshMenu();
		switch (event_type)
		{
		case 1001:
			if (entity.m_Type == 1)
			{
				CSAssembly cSAssembly = entity as CSAssembly;
				MapMaskData mapMaskData = new MapMaskData();
				mapMaskData.mDescription = "Colony";
				mapMaskData.mId = -1;
				mapMaskData.mIconId = 15;
				mapMaskData.mPosition = new Vector3(entity.Position.x, entity.Position.y + 4f, entity.Position.z);
				mapMaskData.mRadius = cSAssembly.Radius;
			}
			else if (entity.m_Type == 12)
			{
				m_Windows.m_HospitalUI.SetCheckIcon();
			}
			else if (entity.m_Type == 13)
			{
				m_Windows.m_HospitalUI.SetTreatIcon();
			}
			else if (entity.m_Type == 14)
			{
				m_Windows.m_HospitalUI.SetTentIcon();
			}
			if (isShow)
			{
				ShowWndPart(entity);
			}
			break;
		case 1002:
			if (entity.m_Type != 1)
			{
				if (entity.m_Type == 12)
				{
					m_Windows.m_HospitalUI.ClearCheckIcon();
				}
				else if (entity.m_Type == 13)
				{
					m_Windows.m_HospitalUI.ClearTreatIcon();
				}
				else if (entity.m_Type == 14)
				{
					m_Windows.m_HospitalUI.ClearTentIcon();
				}
			}
			if (isShow)
			{
				if ((entity.m_Type == 12 || entity.m_Type == 13 || entity.m_Type == 14) && m_Menu.m_HospitalMI.m_EntityList.Count > 0)
				{
					ShowWndPart(m_Menu.m_HospitalMI, m_Menu.m_HospitalMI.m_Type);
				}
				else if ((entity.m_Type == 4 || entity.m_Type == 5 || entity.m_Type == 6) && m_Menu.m_EngineeringlMI.m_EntityList.Count > 0)
				{
					ShowWndPart(m_Menu.m_EngineeringlMI, m_Menu.m_EngineeringlMI.m_Type);
				}
				else if ((entity.m_Type == 32 || entity.m_Type == 33 || entity.m_Type == 34 || entity.m_Type == 35) && m_Menu.m_PPCoalMI.m_EntityList.Count > 0)
				{
					ShowWndPart(m_Menu.m_PPCoalMI, m_Menu.m_PPCoalMI.m_Type);
				}
				else
				{
					ShowWndPart(m_Menu.m_PersonnelMI, m_Menu.m_PersonnelMI.m_Type);
				}
			}
			break;
		}
		m_Windows.m_PersonnelUI.m_NPCOccupaUI.UpdatePopupList();
	}

	private void OnCreatorEventListenerForPersonnel(int event_type, CSPersonnel p)
	{
		switch (event_type)
		{
		case 1003:
			m_Windows.m_PersonnelUI.OnCreatorAddPersennel(p);
			break;
		case 1004:
			m_Windows.m_PersonnelUI.OnCreatorRemovePersennel(p);
			break;
		}
	}

	public static bool IsWorking(bool bShowMsg = true)
	{
		if (m_Instance == null)
		{
			return false;
		}
		if (m_Instance.m_WorkMode == EWorkType.Working)
		{
			return true;
		}
		if (m_Instance.m_WorkMode == EWorkType.OutOfDistance)
		{
			if (bShowMsg)
			{
				ShowStatusBar(UIMsgBoxInfo.mNeedStaticField.GetString(), Color.red);
			}
			return false;
		}
		if (m_Instance.m_WorkMode == EWorkType.NoAssembly)
		{
			if (bShowMsg)
			{
				ShowStatusBar(UIMsgBoxInfo.mNeedAssembly.GetString(), Color.red);
			}
			return false;
		}
		return false;
	}

	private bool IsUnLock(int type)
	{
		if (GameUI.Instance == null || GameUI.Instance.mSkillWndCtrl == null || GameUI.Instance.mSkillWndCtrl._SkillMgr == null)
		{
			return true;
		}
		return GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockColony(type);
	}

	private void SelectBackupOrDefault()
	{
		if (null == mSelectedMenuItem || !m_ActiveMenuList.Contains(mSelectedMenuItem))
		{
			mSelectedMenuItem = m_Menu.m_PersonnelMI;
		}
		ShowWndPart(mSelectedMenuItem, mSelectedMenuItem.m_Type);
	}

	private void RefreshMenu()
	{
		HideAllMenu();
		m_ActiveMenuList.Clear();
		m_Menu.m_PersonnelMI.gameObject.SetActive(value: true);
		m_ActiveMenuList.Add(m_Menu.m_PersonnelMI);
		if (m_Creator.Assembly != null)
		{
			m_Menu.m_AssemblyMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_AssemblyMI);
		}
		m_Menu.m_AssemblyMI.m_EntityList.Clear();
		m_Menu.m_AssemblyMI.m_EntityList.Add(m_Creator.Assembly);
		m_Menu.m_PPCoalMI.m_EntityList.Clear();
		m_Menu.m_FarmMI.m_EntityList.Clear();
		m_Menu.m_FactoryMI.m_EntityList.Clear();
		m_Menu.m_StorageMI.m_EntityList.Clear();
		m_Menu.m_EngineeringlMI.m_EntityList.Clear();
		m_Menu.m_DwellingsMI.m_EntityList.Clear();
		m_Menu.m_HospitalMI.m_EntityList.Clear();
		m_Menu.m_TrainingMI.m_EntityList.Clear();
		m_Menu.m_TransactionMI.m_EntityList.Clear();
		m_Menu.m_CollectMI.m_EntityList.Clear();
		Dictionary<int, CSCommon> commonEntities = m_Creator.GetCommonEntities();
		foreach (CSCommon value in commonEntities.Values)
		{
			switch (value.m_Type)
			{
			case 32:
			case 33:
			case 34:
			case 35:
				m_Menu.m_PPCoalMI.m_EntityList.Add(value);
				break;
			case 7:
				m_Menu.m_FarmMI.m_EntityList.Add(value);
				break;
			case 8:
				m_Menu.m_FactoryMI.m_EntityList.Add(value);
				break;
			case 2:
				m_Menu.m_StorageMI.m_EntityList.Add(value);
				break;
			case 4:
			case 5:
			case 6:
				m_Menu.m_EngineeringlMI.m_EntityList.Add(value);
				break;
			case 21:
				m_Menu.m_DwellingsMI.m_EntityList.Add(value);
				break;
			case 10:
				m_Menu.m_TransactionMI.m_EntityList.Add(value);
				break;
			case 9:
				m_Menu.m_CollectMI.m_EntityList.Add(value);
				break;
			case 12:
			case 13:
			case 14:
				m_Menu.m_HospitalMI.m_EntityList.Add(value);
				break;
			case 11:
				m_Menu.m_TrainingMI.m_EntityList.Add(value);
				break;
			}
		}
		if (m_Menu.m_PPCoalMI.m_EntityList.Count > 0)
		{
			m_Menu.m_PPCoalMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_PPCoalMI);
		}
		if (m_Menu.m_FarmMI.m_EntityList.Count > 0)
		{
			m_Menu.m_FarmMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_FarmMI);
		}
		if (m_Menu.m_FactoryMI.m_EntityList.Count > 0)
		{
			m_Menu.m_FactoryMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_FactoryMI);
		}
		if (m_Menu.m_StorageMI.m_EntityList.Count > 0)
		{
			m_Menu.m_StorageMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_StorageMI);
		}
		if (m_Menu.m_EngineeringlMI.m_EntityList.Count > 0)
		{
			m_Menu.m_EngineeringlMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_EngineeringlMI);
		}
		if (m_Menu.m_DwellingsMI.m_EntityList.Count > 0)
		{
			m_Menu.m_DwellingsMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_DwellingsMI);
		}
		if (m_Menu.m_TransactionMI.m_EntityList.Count > 0)
		{
			m_Menu.m_TransactionMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_TransactionMI);
		}
		if (m_Menu.m_CollectMI.m_EntityList.Count > 0)
		{
			m_Menu.m_CollectMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_CollectMI);
		}
		if (m_Menu.m_HospitalMI.m_EntityList.Count > 0)
		{
			m_Menu.m_HospitalMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_HospitalMI);
		}
		if (m_Menu.m_TrainingMI.m_EntityList.Count > 0)
		{
			m_Menu.m_TrainingMI.gameObject.SetActive(value: true);
			m_ActiveMenuList.Add(m_Menu.m_TrainingMI);
		}
		m_Menu.mLeftMenuGrid.repositionNow = true;
	}

	public void HideWndByType(int mWndPartType)
	{
		GameObject gameObject = null;
		switch (mWndPartType)
		{
		case 1:
			gameObject = m_Windows.m_AssemblyUI.gameObject;
			break;
		case 2:
			gameObject = m_Windows.m_StorageUI.gameObject;
			break;
		case 33:
			gameObject = m_Windows.m_PPCoalUI.gameObject;
			break;
		case 21:
			gameObject = m_Windows.m_DwellingsUI.gameObject;
			break;
		case 3:
			gameObject = m_Windows.m_EngineeringUI.gameObject;
			break;
		case 7:
			gameObject = m_Windows.m_FarmUI.gameObject;
			break;
		case 8:
			gameObject = m_Windows.m_FactoryUI.gameObject;
			break;
		case 50:
			gameObject = m_Windows.m_PersonnelUI.gameObject;
			break;
		case 10:
			gameObject = m_Windows.m_TradingPostUI.gameObject;
			break;
		case 9:
			gameObject = m_Windows.m_CollectUI.gameObject;
			break;
		case 12:
		case 13:
		case 14:
			gameObject = m_Windows.m_HospitalUI.gameObject;
			break;
		case 11:
			gameObject = m_Windows.m_TrainUI.gameObject;
			break;
		}
		if (gameObject != null)
		{
			gameObject.SetActive(value: false);
		}
	}

	private void HideAllMenu()
	{
		m_Menu.m_PersonnelMI.IsSelected = false;
		m_Menu.m_PersonnelMI.gameObject.SetActive(value: false);
		m_Menu.m_AssemblyMI.IsSelected = false;
		m_Menu.m_AssemblyMI.gameObject.SetActive(value: false);
		m_Menu.m_PPCoalMI.IsSelected = false;
		m_Menu.m_PPCoalMI.gameObject.SetActive(value: false);
		m_Menu.m_FarmMI.IsSelected = false;
		m_Menu.m_FarmMI.gameObject.SetActive(value: false);
		m_Menu.m_FactoryMI.IsSelected = false;
		m_Menu.m_FactoryMI.gameObject.SetActive(value: false);
		m_Menu.m_StorageMI.IsSelected = false;
		m_Menu.m_StorageMI.gameObject.SetActive(value: false);
		m_Menu.m_EngineeringlMI.IsSelected = false;
		m_Menu.m_EngineeringlMI.gameObject.SetActive(value: false);
		m_Menu.m_DwellingsMI.IsSelected = false;
		m_Menu.m_DwellingsMI.gameObject.SetActive(value: false);
		m_Menu.m_TransactionMI.IsSelected = false;
		m_Menu.m_TransactionMI.gameObject.SetActive(value: false);
		m_Menu.m_CollectMI.IsSelected = false;
		m_Menu.m_CollectMI.gameObject.SetActive(value: false);
		m_Menu.m_HospitalMI.IsSelected = false;
		m_Menu.m_HospitalMI.gameObject.SetActive(value: false);
		m_Menu.m_TrainingMI.IsSelected = false;
		m_Menu.m_TrainingMI.gameObject.SetActive(value: false);
	}

	public void ShowWndPart(CSEntity entity)
	{
		if (!isShow)
		{
			base.Show();
			CheckFirstOpenColony();
		}
		CSUI_LeftMenuItem cSUI_LeftMenuItem;
		switch (entity.m_Type)
		{
		case 1:
			cSUI_LeftMenuItem = m_Menu.m_AssemblyMI;
			break;
		case 32:
		case 33:
		case 34:
		case 35:
			cSUI_LeftMenuItem = m_Menu.m_PPCoalMI;
			break;
		case 7:
			cSUI_LeftMenuItem = m_Menu.m_FarmMI;
			break;
		case 8:
			cSUI_LeftMenuItem = m_Menu.m_FactoryMI;
			break;
		case 2:
			cSUI_LeftMenuItem = m_Menu.m_StorageMI;
			break;
		case 4:
		case 5:
		case 6:
			cSUI_LeftMenuItem = m_Menu.m_EngineeringlMI;
			break;
		case 21:
			cSUI_LeftMenuItem = m_Menu.m_DwellingsMI;
			break;
		case 12:
		case 13:
		case 14:
			cSUI_LeftMenuItem = m_Menu.m_HospitalMI;
			break;
		case 9:
			cSUI_LeftMenuItem = m_Menu.m_CollectMI;
			break;
		case 10:
			cSUI_LeftMenuItem = m_Menu.m_TransactionMI;
			break;
		case 11:
			cSUI_LeftMenuItem = m_Menu.m_TrainingMI;
			break;
		default:
			cSUI_LeftMenuItem = null;
			break;
		}
		if (!(cSUI_LeftMenuItem == null))
		{
			ShowWndPart(cSUI_LeftMenuItem, cSUI_LeftMenuItem.m_Type, entity);
		}
	}

	public void ShowWndPart(CSUI_LeftMenuItem menuItem, int mWndPartType, CSEntity selectEntity = null)
	{
		if (!isShow)
		{
			base.Show();
			CheckFirstOpenColony();
		}
		if (null == menuItem || !m_ActiveMenuList.Contains(menuItem))
		{
			menuItem = m_Menu.m_PersonnelMI;
		}
		if (!menuItem.IsSelected)
		{
			menuItem.SelectSprite(isSelect: true);
		}
		if (mSelectedMenuItem != null && mSelectedMenuItem != menuItem)
		{
			mSelectedMenuItem.IsSelected = false;
		}
		mSelectedMenuItem = menuItem;
		if (menuItem.NotHaveAssembly || menuItem.NotHaveElectricity)
		{
			string text = CSUtils.GetEntityName(menuItem.m_Type);
			if (menuItem == m_Menu.m_HospitalMI && (m_Menu.m_HospitalMI.NotHaveAssembly || m_Menu.m_HospitalMI.NotHaveElectricity))
			{
				List<string> list = new List<string>();
				if (menuItem.AssemblyLevelInsufficient)
				{
					list = menuItem.GetNamesByAssemblyLevelInsufficient();
				}
				else if (menuItem.NotHaveAssembly)
				{
					list = menuItem.GetNamesByNotHaveAssembly();
				}
				else if (menuItem.NotHaveElectricity)
				{
					list = menuItem.GetNamesByNotHaveElectricity();
				}
				if (list != null && list.Count > 0)
				{
					text = string.Empty;
					for (int i = 0; i < list.Count; i++)
					{
						text += list[i];
						text += ",";
					}
					text = text.Substring(0, text.Length - 1);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (menuItem.AssemblyLevelInsufficient)
				{
					ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkAssemblyLevelInsufficient.GetString(), text), Color.red);
				}
				else if (menuItem.NotHaveAssembly)
				{
					ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutAssembly.GetString(), text), Color.red);
				}
				else if (menuItem.NotHaveElectricity)
				{
					ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), text), Color.red);
				}
			}
		}
		m_Windows.m_EngineeringUI.CloseLock();
		mWndPartTag = mWndPartType;
		mSkillLock.gameObject.SetActive(!IsUnLock(mWndPartType));
		bool flag = base.isActiveAndEnabled;
		switch (mWndPartType)
		{
		case 1:
			m_Windows.m_AssemblyUI.gameObject.SetActive(value: true);
			m_Windows.m_AssemblyUI.SetEntity(m_Creator.Assembly);
			mSelectedEnntity = m_Creator.Assembly;
			break;
		case 2:
			m_Windows.m_StorageUI.gameObject.SetActive(value: true);
			m_Windows.m_StorageUI.Replace(menuItem.m_EntityList);
			break;
		default:
			if (menuItem.m_Type == 33)
			{
				m_Windows.m_PPCoalUI.gameObject.SetActive(value: true);
				m_Windows.m_PPCoalUI.SetEntityList(menuItem.m_EntityList, selectEntity);
				break;
			}
			switch (mWndPartType)
			{
			case 21:
				m_Windows.m_DwellingsUI.gameObject.SetActive(flag);
				m_Windows.m_DwellingsUI.SetEntityList(menuItem.m_EntityList);
				break;
			case 3:
				m_Windows.m_EngineeringUI.Replace(menuItem.m_EntityList, selectEntity);
				m_Windows.m_EngineeringUI.gameObject.SetActive(flag);
				break;
			default:
				if (menuItem.m_Type == 7)
				{
					if (menuItem.m_EntityList.Count < 1)
					{
						break;
					}
					m_Windows.m_FarmUI.gameObject.SetActive(flag);
					CSFarm farm = menuItem.m_EntityList[0] as CSFarm;
					if (menuItem.m_EntityList.Count > 1)
					{
						foreach (CSEntity entity2 in menuItem.m_EntityList)
						{
							CSFarm cSFarm = entity2 as CSFarm;
							if (cSFarm.IsRunning)
							{
								farm = cSFarm;
								break;
							}
						}
					}
					m_Windows.m_FarmUI.SetFarm(farm);
					mSelectedEnntity = farm;
					break;
				}
				if (menuItem.m_Type == 8)
				{
					if (menuItem.m_EntityList.Count < 1)
					{
						break;
					}
					NGUITools.SetActive(m_Windows.m_FactoryUI.gameObject, flag);
					CSEntity entity = menuItem.m_EntityList[0];
					if (menuItem.m_EntityList.Count > 1)
					{
						foreach (CSEntity entity3 in menuItem.m_EntityList)
						{
							if (entity3.IsRunning)
							{
								entity = entity3;
								break;
							}
						}
					}
					m_Windows.m_FactoryUI.SetEntity(entity);
					break;
				}
				switch (mWndPartType)
				{
				case 50:
					m_Windows.m_PersonnelUI.gameObject.SetActive(value: true);
					break;
				case 10:
					m_Windows.m_TradingPostUI.gameObject.SetActive(value: true);
					m_Windows.m_TradingPostUI.SetMenu(menuItem);
					if (null != menuItem && menuItem.m_EntityList.Count > 0)
					{
						mSelectedEnntity = menuItem.m_EntityList[0];
					}
					break;
				case 9:
				{
					m_Windows.m_CollectUI.gameObject.SetActive(value: true);
					m_Windows.m_CollectUI.UpdateCollect();
					if (menuItem.m_EntityList.Count <= 0)
					{
						break;
					}
					CSEntity enity = menuItem.m_EntityList[0];
					if (menuItem.m_EntityList.Count > 1)
					{
						foreach (CSEntity entity4 in menuItem.m_EntityList)
						{
							if (entity4.IsRunning)
							{
								enity = entity4;
								break;
							}
						}
					}
					m_Windows.m_CollectUI.SetEnity(enity);
					break;
				}
				case 12:
				case 13:
				case 14:
					m_Windows.m_HospitalUI.gameObject.SetActive(value: true);
					m_Windows.m_HospitalUI.RefleshMechine(m_Windows.m_HospitalUI.m_CheckedPartType, menuItem.m_EntityList, selectEntity);
					break;
				case 11:
					m_Windows.m_TrainUI.gameObject.SetActive(value: true);
					if (null != menuItem && menuItem.m_EntityList.Count > 0)
					{
						mSelectedEnntity = menuItem.m_EntityList[0];
					}
					break;
				}
				break;
			}
			break;
		}
		DestroyChildWindowOfBed();
	}

	public static void ShowStatusBar(string text, float time = 4.5f)
	{
		if (!(m_Instance == null))
		{
			CSUI_StatusBar.ShowText(text, new Color(0f, 0.2f, 1f, 0f), time);
		}
	}

	public static void ShowStatusBar(string text, Color col, float time = 4.5f)
	{
		if (!(m_Instance == null))
		{
			CSUI_StatusBar.ShowText(text, col, time);
		}
	}

	public override void OnCreate()
	{
		m_Instance = this;
		base.OnCreate();
		if (Creator == null)
		{
			Creator = CSMain.GetCreator(0);
		}
		FactoryUI.Init();
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		m_Menu.m_PersonnelMI.m_Type = 50;
		m_Menu.m_AssemblyMI.m_Type = 1;
		m_Menu.m_PPCoalMI.m_Type = 33;
		m_Menu.m_FarmMI.m_Type = 7;
		m_Menu.m_FactoryMI.m_Type = 8;
		m_Menu.m_StorageMI.m_Type = 2;
		m_Menu.m_EngineeringlMI.m_Type = 3;
		m_Menu.m_DwellingsMI.m_Type = 21;
		m_Menu.m_TransactionMI.m_Type = 10;
		m_Menu.m_CollectMI.m_Type = 9;
		m_Menu.m_HospitalMI.m_Type = 12;
		m_Menu.m_TrainingMI.m_Type = 11;
		m_Windows.m_StorageUI.Init();
		m_Windows.m_FarmUI.Init();
		m_Windows.m_CollectUI.Init();
	}

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
		}
	}

	private void Update()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null || m_Creator == null)
		{
			m_WorkMode = EWorkType.UnKnown;
		}
		else if (m_Creator.Assembly == null)
		{
			m_WorkMode = EWorkType.NoAssembly;
		}
		else
		{
			Vector3 position = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			Vector3 position2 = m_Creator.Assembly.Position;
			if ((position - position2).sqrMagnitude < WorkDistance * WorkDistance)
			{
				m_WorkMode = EWorkType.Working;
			}
			else
			{
				m_WorkMode = EWorkType.OutOfDistance;
			}
		}
		UpdatePerson();
	}

	private void UpdatePerson()
	{
	}

	public static CSUI_PopupHint CreatePopupHint(Vector3 pos, Transform parent, Vector3 offset, string text, bool bGreen = true)
	{
		if (m_Instance == null)
		{
			return null;
		}
		if (m_Instance.m_PopupHintPrefab == null)
		{
			return null;
		}
		CSUI_PopupHint cSUI_PopupHint = UnityEngine.Object.Instantiate(m_Instance.m_PopupHintPrefab);
		cSUI_PopupHint.transform.parent = parent;
		cSUI_PopupHint.transform.position = pos;
		cSUI_PopupHint.transform.localScale = Vector3.one;
		cSUI_PopupHint.transform.localPosition = new Vector3(cSUI_PopupHint.transform.localPosition.x + offset.x, cSUI_PopupHint.transform.localPosition.y + offset.y, offset.z);
		cSUI_PopupHint.m_Pos = cSUI_PopupHint.transform.position;
		cSUI_PopupHint.Text = text;
		cSUI_PopupHint.bGreen = bGreen;
		cSUI_PopupHint.Tween();
		return cSUI_PopupHint;
	}

	private void DestroyChildWindowOfBed()
	{
		if (m_ChildWindowOfBed != null)
		{
			UnityEngine.Object.Destroy(m_ChildWindowOfBed);
			m_ChildWindowOfBed = null;
		}
	}

	public override void Show()
	{
		base.Show();
		RefreshMenu();
		SelectBackupOrDefault();
		CheckFirstOpenColony();
	}

	protected override void OnClose()
	{
		DestroyChildWindowOfBed();
		base.OnClose();
	}

	public void GoToPersonnelWorkWnd()
	{
		ShowWndPart(m_Menu.m_PersonnelMI, m_Menu.m_PersonnelMI.m_Type);
		m_Windows.m_PersonnelUI.m_NPCInfoUI.m_WorkCk.isChecked = true;
	}

	public void GoToCollectWnd(int processIndex)
	{
		if (m_Menu.m_CollectMI.m_EntityList.Count > 0)
		{
			ShowWndPart(m_Menu.m_CollectMI, m_Menu.m_CollectMI.m_Type);
			m_Windows.m_CollectUI.SelectProcessByIndex(processIndex);
		}
		else
		{
			ShowStatusBar(PELocalization.GetString(82201097));
		}
	}

	private void OnHelpBtnClick()
	{
		if (null != GameUI.Instance && null != GameUI.Instance.mPhoneWnd)
		{
			GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
		}
	}

	private void CheckFirstOpenColony()
	{
		TutorialData.AddActiveTutorialID(19, execEvent: false);
		TutorialData.AddActiveTutorialID(18, execEvent: false);
		TutorialData.AddActiveTutorialID(17, execEvent: false);
		TutorialData.AddActiveTutorialID(10, execEvent: false);
		TutorialData.AddActiveTutorialID(7, execEvent: false);
		TutorialData.AddActiveTutorialID(6, execEvent: false);
		TutorialData.AddActiveTutorialID(5, execEvent: false);
		TutorialData.AddActiveTutorialID(4, execEvent: false);
		TutorialData.AddActiveTutorialID(20);
	}
}
