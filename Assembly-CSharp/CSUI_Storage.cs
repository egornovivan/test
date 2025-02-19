using System;
using System.Collections.Generic;
using CustomData;
using Pathea;
using UnityEngine;

public class CSUI_Storage : MonoBehaviour
{
	[Serializable]
	public class StorageMenu
	{
		public UIGrid m_Root;

		public GameObject m_BtnPrefab;
	}

	[Serializable]
	public class StorageMain
	{
		public CSUI_StorageMain m_Main;

		public UICheckbox m_ItemCB;

		public UICheckbox m_EquipCB;

		public UICheckbox m_ResourceCB;

		public UICheckbox m_ArmorCB;
	}

	private CSStorage m_ActiveStorage;

	private List<CSStorage> m_Storages = new List<CSStorage>();

	[SerializeField]
	private StorageMenu m_StorageMenu;

	private List<GameObject> m_StorageMenuObjs;

	[SerializeField]
	private StorageMain m_StorageMain;

	[SerializeField]
	private UITable m_HistoryRootUI;

	[SerializeField]
	private UILabel m_HistoryPrefab;

	[SerializeField]
	private CSUI_SubStorageHistory m_SubHistoryPrefab;

	private List<UILabel> m_HistoryLbs = new List<UILabel>();

	private List<CSUI_SubStorageHistory> m_SubHistorys = new List<CSUI_SubStorageHistory>();

	private int m_CurrentPickTab;

	public CSStorage m_SetActiveStorage;

	public CSStorage ActiveStorage
	{
		get
		{
			return m_ActiveStorage;
		}
		set
		{
			if (m_ActiveStorage != value)
			{
				if (m_ActiveStorage != null)
				{
					m_ActiveStorage.RemoveEventListener(OnStorageEventHandler);
					HistoryStruct[] history = value.GetHistory();
					for (int i = 0; i < m_SubHistorys.Count; i++)
					{
						UnityEngine.Object.DestroyImmediate(m_SubHistorys[i].gameObject);
					}
					m_SubHistorys.Clear();
					for (int j = 0; j < history.Length; j++)
					{
						CSUI_SubStorageHistory cSUI_SubStorageHistory = _addHistoryItem(history[j]);
						cSUI_SubStorageHistory.AddHistory(history[j].m_Value);
					}
				}
				value?.AddEventListener(OnStorageEventHandler);
			}
			m_ActiveStorage = value;
		}
	}

	public CSUI_StorageMain StorageMainUI => m_StorageMain.m_Main;

	public bool IsEmpty()
	{
		return m_Storages.Count == 0;
	}

	public void AddStorage(CSStorage storage)
	{
		if (!m_Storages.Exists((CSStorage item0) => item0 == storage))
		{
			m_Storages.Add(storage);
			UpdateStorageMenu();
		}
		else
		{
			Debug.LogWarning("The storage that you want to add into UI is areadly exsts!");
		}
	}

	public void RemoveStorage(CSStorage storage)
	{
		if (m_Storages.Remove(storage))
		{
			UpdateStorageMenu();
			GameObject gameObject = m_StorageMenuObjs.Find((GameObject item0) => item0.activeSelf);
			if (gameObject != null)
			{
				gameObject.GetComponent<UICheckbox>().isChecked = true;
				OnStorageMenuSelect(gameObject);
			}
		}
	}

	public void RemoveAll()
	{
		m_Storages.Clear();
		UpdateStorageMenu();
	}

	public void Replace(List<CSEntity> entityList)
	{
		m_Storages.Clear();
		int num = ((entityList.Count <= 8) ? entityList.Count : 8);
		for (int i = 0; i < num; i++)
		{
			m_Storages.Add(entityList[i] as CSStorage);
		}
		UpdateStorageMenu();
	}

	public void Replace(List<CSStorage> storages)
	{
		m_Storages.Clear();
		int num = ((storages.Count <= 8) ? storages.Count : 8);
		for (int i = 0; i < num; i++)
		{
			m_Storages.Add(storages[i]);
		}
		UpdateStorageMenu();
	}

	private void UpdateStorageMenu()
	{
		if (m_StorageMenuObjs == null)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			CSUI_CommonIcon component = m_StorageMenuObjs[i].GetComponent<CSUI_CommonIcon>();
			if (i < m_Storages.Count)
			{
				m_StorageMenuObjs[i].SetActive(value: true);
				UIEventListener uIEventListener = UIEventListener.Get(m_StorageMenuObjs[i]);
				uIEventListener.onClick = OnStorageMenuSelect;
				if (m_StorageMenuObjs[i].GetComponent<UICheckbox>().isChecked)
				{
					OnStorageMenuSelect(m_StorageMenuObjs[i]);
				}
				component.Common = m_Storages[i];
			}
			else
			{
				m_StorageMenuObjs[i].SetActive(value: false);
				component.Common = null;
			}
		}
		m_StorageMenu.m_Root.repositionNow = true;
	}

	public void SetStorageType(int type, int pageIndex)
	{
		switch (type)
		{
		case 0:
			m_StorageMain.m_ItemCB.isChecked = true;
			break;
		case 1:
			m_StorageMain.m_EquipCB.isChecked = true;
			break;
		case 2:
			m_StorageMain.m_ResourceCB.isChecked = true;
			break;
		case 3:
			m_StorageMain.m_ArmorCB.isChecked = true;
			break;
		}
		m_CurrentPickTab = type;
		m_StorageMain.m_Main.SetType(m_CurrentPickTab, pageIndex);
	}

	private void SetActiveStorage()
	{
		if (m_SetActiveStorage != null)
		{
			int num = m_Storages.FindIndex((CSStorage item0) => item0 == m_SetActiveStorage);
			if (num != -1)
			{
				m_StorageMenuObjs[num].GetComponent<UICheckbox>().isChecked = true;
				OnStorageMenuSelect(m_StorageMenuObjs[num]);
			}
			m_SetActiveStorage = null;
		}
	}

	private CSUI_SubStorageHistory _addHistoryItem(HistoryStruct history)
	{
		if (m_SubHistorys.Count != 0)
		{
			CSUI_SubStorageHistory cSUI_SubStorageHistory = m_SubHistorys[m_SubHistorys.Count - 1];
			if (cSUI_SubStorageHistory.Day == history.m_Day)
			{
				return cSUI_SubStorageHistory;
			}
		}
		CSUI_SubStorageHistory cSUI_SubStorageHistory2 = UnityEngine.Object.Instantiate(m_SubHistoryPrefab);
		cSUI_SubStorageHistory2.transform.parent = m_HistoryRootUI.transform;
		cSUI_SubStorageHistory2.transform.localPosition = Vector3.zero;
		cSUI_SubStorageHistory2.transform.localRotation = Quaternion.identity;
		cSUI_SubStorageHistory2.transform.localScale = Vector3.one;
		cSUI_SubStorageHistory2.Day = history.m_Day;
		cSUI_SubStorageHistory2.onReposition = OnSubHistoryReposition;
		m_SubHistorys.Add(cSUI_SubStorageHistory2);
		return cSUI_SubStorageHistory2;
	}

	private void OnSubHistoryReposition()
	{
		m_HistoryRootUI.repositionNow = true;
	}

	private void OnStorageMenuSelect(GameObject go)
	{
		if (!Input.GetMouseButtonUp(1))
		{
			int index = m_StorageMenuObjs.FindIndex((GameObject item0) => item0 == go);
			ActiveStorage = m_Storages[index];
			CSUI_MainWndCtrl.Instance.mSelectedEnntity = ActiveStorage;
			m_StorageMain.m_ItemCB.isChecked = true;
			OnStorageTypeSelect(m_StorageMain.m_ItemCB.gameObject);
		}
	}

	private void OnStorageTypeSelect(GameObject go)
	{
		if (!Input.GetMouseButtonUp(1))
		{
			if (go == m_StorageMain.m_ItemCB.gameObject)
			{
				m_CurrentPickTab = 0;
			}
			else if (go == m_StorageMain.m_EquipCB.gameObject)
			{
				m_CurrentPickTab = 1;
			}
			else if (go == m_StorageMain.m_ResourceCB.gameObject)
			{
				m_CurrentPickTab = 2;
			}
			else if (go == m_StorageMain.m_ArmorCB.gameObject)
			{
				m_CurrentPickTab = 3;
			}
			if (PeGameMgr.IsMulti)
			{
				m_StorageMain.m_Main.SetPackage(ActiveStorage.m_Package, m_CurrentPickTab, ActiveStorage);
			}
			else
			{
				m_StorageMain.m_Main.SetPackage(ActiveStorage.m_Package, m_CurrentPickTab);
			}
			GameUI.Instance.mItemPackageCtrl.ResetItem(m_CurrentPickTab, m_StorageMain.m_Main.PageIndex);
			GameUI.Instance.mWarehouse.ResetItem(m_CurrentPickTab, m_StorageMain.m_Main.PageIndex);
		}
	}

	private void OnStorageMainOpStateEvent(CSUI_StorageMain.EEventType type, object obj1, object obj2)
	{
		switch (type)
		{
		case CSUI_StorageMain.EEventType.CantWork:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), (string)obj1), Color.red);
			break;
		case CSUI_StorageMain.EEventType.PutItemInto:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), (string)obj1, (string)obj2));
			break;
		case CSUI_StorageMain.EEventType.DeleteItem:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mDeleteItem.GetString(), (string)obj1, (string)obj2));
			break;
		case CSUI_StorageMain.EEventType.TakeAwayItem:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), (string)obj1, (string)obj2));
			break;
		case CSUI_StorageMain.EEventType.ResortItem:
			CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mResortTheItems.GetString());
			break;
		case CSUI_StorageMain.EEventType.SplitItem:
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mSplitItems.GetString(), (string)obj1, (string)obj2));
			break;
		}
	}

	private void OnStorageEventHandler(int event_id, CSEntity entity, object arg)
	{
		switch (event_id)
		{
		case 3002:
		{
			HistoryStruct historyStruct = arg as HistoryStruct;
			CSUI_SubStorageHistory cSUI_SubStorageHistory = _addHistoryItem(historyStruct);
			if (cSUI_SubStorageHistory != null)
			{
				cSUI_SubStorageHistory.AddHistory(historyStruct.m_Value);
			}
			break;
		}
		case 3001:
			if (m_SubHistorys.Count != 0)
			{
				m_SubHistorys[0].PopImmediate();
				if (m_SubHistorys[0].IsEmpty)
				{
					UnityEngine.Object.Destroy(m_SubHistorys[0].gameObject);
					m_SubHistorys.RemoveAt(0);
				}
			}
			break;
		case 3003:
			StorageMainUI.RestItems();
			break;
		}
	}

	public void Init()
	{
		m_StorageMenuObjs = new List<GameObject>();
		for (int i = 0; i < 8; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_StorageMenu.m_BtnPrefab);
			gameObject.transform.parent = m_StorageMenu.m_Root.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.name = i + " Storage Menu";
			UICheckbox component = gameObject.GetComponent<UICheckbox>();
			component.radioButtonRoot = m_StorageMenu.m_Root.transform;
			if (i == 0)
			{
				component.startsChecked = true;
				component.isChecked = true;
			}
			else
			{
				component.startsChecked = false;
				component.isChecked = false;
			}
			m_StorageMenuObjs.Add(gameObject);
		}
		UIEventListener uIEventListener = UIEventListener.Get(m_StorageMain.m_ItemCB.gameObject);
		uIEventListener.onClick = OnStorageTypeSelect;
		uIEventListener = UIEventListener.Get(m_StorageMain.m_EquipCB.gameObject);
		uIEventListener.onClick = OnStorageTypeSelect;
		uIEventListener = UIEventListener.Get(m_StorageMain.m_ResourceCB.gameObject);
		uIEventListener.onClick = OnStorageTypeSelect;
		uIEventListener = UIEventListener.Get(m_StorageMain.m_ArmorCB.gameObject);
		uIEventListener.onClick = OnStorageTypeSelect;
		StorageMainUI.OpStatusEvent += OnStorageMainOpStateEvent;
	}

	private void OnDestroy()
	{
		StorageMainUI.OpStatusEvent -= OnStorageMainOpStateEvent;
	}

	private void OnEnable()
	{
	}

	private void Start()
	{
		UpdateStorageMenu();
	}

	private void Update()
	{
		if (ActiveStorage == null && m_Storages.Count > 0)
		{
			ActiveStorage = m_Storages[0];
		}
		if (ActiveStorage != null)
		{
			m_StorageMain.m_Main.SetWork(ActiveStorage.IsRunning);
		}
		else
		{
			m_StorageMain.m_Main.SetWork(bWork: false);
		}
		Transform transform = m_HistoryRootUI.transform;
		transform.localPosition = new Vector3(transform.localPosition.x, 0f - m_HistoryRootUI.mVariableHeight, transform.localPosition.z);
		SetActiveStorage();
	}
}
