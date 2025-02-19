using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_StorageMain : MonoBehaviour
{
	public enum EEventType
	{
		CantWork,
		PutItemInto,
		TakeAwayItem,
		ResortItem,
		SplitItem,
		DeleteItem
	}

	public delegate void OpStatusDel(EEventType type, object obj1, object obj2);

	[SerializeField]
	private UIGrid m_MainRoot;

	[SerializeField]
	private Grid_N m_GridPrefab;

	[SerializeField]
	private UILabel m_PageLb;

	[SerializeField]
	private GameObject m_SplitWnd;

	[SerializeField]
	private UILabel m_SplitNumLb;

	[SerializeField]
	private UIAtlas m_ButtonAtlas;

	public UIAtlas mNewUIAtlas;

	private List<Grid_N> m_Grids;

	public int m_GridsRow = 5;

	public int m_GridsCol = 8;

	private ItemPackage m_Package;

	[HideInInspector]
	public int m_Type;

	private int m_PageIndex;

	private int m_OpType;

	private Grid_N m_OpGird;

	private float m_SplitNumDur = 1f;

	private float m_LastOpTime;

	private float m_SplitOpStartTime;

	private SlotList m_CurPack;

	private bool m_IsWorking = true;

	public CSStorage m_storage;

	public int PageIndex => m_PageIndex;

	public ItemObject CurOperateItem => (!(m_OpGird == null)) ? m_OpGird.ItemObj : null;

	public event OpStatusDel OpStatusEvent;

	public void SetPackage(ItemPackage package, int type = 0, CSStorage storage = null)
	{
		m_Package = package;
		m_PageIndex = 0;
		m_OpType = 0;
		SetType(type);
		if (GameConfig.IsMultiMode)
		{
			m_storage = storage;
		}
	}

	public void SetType(int type, int pageIndex = 0)
	{
		if (type <= -1 || type >= 4)
		{
			Debug.LogWarning("The type you give is out of range!");
			return;
		}
		m_Type = type;
		m_PageIndex = pageIndex;
		RestItems();
	}

	public void RestItems()
	{
		if (m_Grids == null)
		{
			return;
		}
		if (m_Package == null)
		{
			Debug.LogWarning("It must need a package to Reset the items!");
			return;
		}
		m_CurPack = m_Package.GetSlotList((ItemPackage.ESlotType)m_Type);
		int num = m_GridsRow * m_GridsCol;
		if ((m_CurPack.Count - 1) / num < m_PageIndex)
		{
			m_PageIndex = (m_CurPack.Count - 1) / num;
		}
		int num2 = (((m_CurPack.Count - 1) / num != m_PageIndex) ? num : (m_CurPack.Count - m_PageIndex * num));
		for (int i = 0; i < num2; i++)
		{
			m_Grids[i].SetItem(m_CurPack[i + m_PageIndex * num]);
			if (m_storage != null)
			{
				m_Grids[i].SetItemPlace(ItemPlaceType.IPT_CSStorage, i + m_PageIndex * num);
			}
			else
			{
				m_Grids[i].SetItemPlace(ItemPlaceType.IPT_NPCStorage, i + m_PageIndex * num);
			}
			switch (m_Type)
			{
			case 0:
				m_Grids[i].SetGridMask(GridMask.GM_Item);
				break;
			case 1:
				m_Grids[i].SetGridMask(GridMask.GM_Equipment);
				break;
			case 2:
				m_Grids[i].SetGridMask(GridMask.GM_Resource);
				break;
			case 3:
				m_Grids[i].SetGridMask(GridMask.GM_Armor);
				break;
			}
		}
		m_PageLb.text = (m_PageIndex + 1).ToString() + "/" + ((m_CurPack.Count - 1) / num + 1);
	}

	public bool SetItemWithEmptyGrid(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		int num = m_GridsRow * m_GridsCol;
		int num2 = m_CurPack.VacancyIndex();
		if (num2 >= 0)
		{
			if (num2 / num == m_PageIndex)
			{
				m_CurPack[num2] = item;
				m_Grids[num2 % num].SetItem(item);
				return true;
			}
			m_CurPack[num2] = item;
			m_PageIndex = num2 / num;
			RestItems();
			return true;
		}
		return false;
	}

	public void SetWork(bool bWork)
	{
		if (bWork)
		{
			foreach (Grid_N grid in m_Grids)
			{
				grid.mSkillCooldown.fillAmount = 0f;
			}
		}
		else
		{
			foreach (Grid_N grid2 in m_Grids)
			{
				grid2.mSkillCooldown.fillAmount = 1f;
			}
		}
		m_IsWorking = bWork;
	}

	public bool EqualUsingItem(ItemSample item, bool showUsingTip = true)
	{
		if (item == null || CurOperateItem == null)
		{
			return false;
		}
		if (CurOperateItem == item)
		{
			if (showUsingTip)
			{
				PeTipMsg.Register(PELocalization.GetString(8000623), PeTipMsg.EMsgLevel.Error);
			}
			return true;
		}
		return false;
	}

	private void ResetPage()
	{
	}

	private void BtnLeftOnClick()
	{
		if (m_PageIndex > 0)
		{
			m_PageIndex--;
			RestItems();
			ResetPage();
		}
	}

	private void BtnLeftEndOnClick()
	{
		if (m_PageIndex > 0)
		{
			m_PageIndex = 0;
			RestItems();
			ResetPage();
		}
	}

	private void BtnRightOnClick()
	{
		int num = m_GridsRow * m_GridsCol;
		int num2 = (m_CurPack.Count - 1) / num + 1;
		if (m_PageIndex < num2)
		{
			m_PageIndex++;
			RestItems();
			ResetPage();
		}
	}

	private void BtnRightEndOnClick()
	{
		int num = m_GridsRow * m_GridsCol;
		int num2 = (m_CurPack.Count - 1) / num + 1;
		if (m_PageIndex < num2)
		{
			m_PageIndex = num2;
			RestItems();
			ResetPage();
		}
	}

	private void OnResort()
	{
		if (m_SplitWnd.activeInHierarchy)
		{
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			m_Package.Sort((ItemPackage.ESlotType)m_Type);
			RestItems();
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.ResortItem, null, null);
			}
		}
		else if (m_storage != null)
		{
			m_storage._ColonyObj._Network.STO_Sort(m_Type);
		}
		else
		{
			PlayerNetwork.mainPlayer.RequestPersonalStorageSort(m_Type);
		}
	}

	private void OnSplitBtn()
	{
		if (!m_SplitWnd.activeInHierarchy)
		{
			if (m_OpType == 1)
			{
				m_OpType = 0;
				UICursor.Clear();
			}
			else
			{
				m_OpType = 1;
				UICursor.Set(mNewUIAtlas, "icocai");
			}
			m_OpGird = null;
		}
	}

	private void OnDeleteBtn()
	{
		if (!m_SplitWnd.gameObject.activeInHierarchy)
		{
			if (m_OpType == 2)
			{
				m_OpType = 0;
				UICursor.Clear();
			}
			else
			{
				m_OpType = 2;
				UICursor.Set(mNewUIAtlas, "icodelete");
			}
			m_OpGird = null;
		}
	}

	private void OnSplitAddBtn()
	{
		if (Time.time - m_LastOpTime > 0.1f)
		{
			m_SplitOpStartTime = Time.time;
			m_SplitNumDur += 1f;
		}
		m_LastOpTime = Time.time;
		float num = Time.time - m_SplitOpStartTime;
		if (num < 0.5f)
		{
			m_SplitNumDur = m_SplitNumDur;
		}
		else if (num < 3f)
		{
			m_SplitNumDur += 3f * Time.deltaTime;
		}
		else if (num < 5f)
		{
			m_SplitNumDur += 6f * Time.deltaTime;
		}
		else if (num < 7f)
		{
			m_SplitNumDur += 9f * Time.deltaTime;
		}
		else
		{
			m_SplitNumDur += 12f * Time.deltaTime;
		}
		m_SplitNumDur = Mathf.Clamp(m_SplitNumDur, 1f, m_OpGird.Item.GetCount() - 1);
		m_SplitNumLb.text = ((int)m_SplitNumDur).ToString();
	}

	private void OnSplitSubstructBtn()
	{
		if (Time.time - m_LastOpTime > 0.1f)
		{
			m_SplitOpStartTime = Time.time;
			m_SplitNumDur -= 1f;
		}
		m_LastOpTime = Time.time;
		float num = Time.time - m_SplitOpStartTime;
		if (num < 0.5f)
		{
			m_SplitNumDur = m_SplitNumDur;
		}
		else if (num < 3f)
		{
			m_SplitNumDur -= 3f * Time.deltaTime;
		}
		else if (num < 5f)
		{
			m_SplitNumDur -= 6f * Time.deltaTime;
		}
		else if (num < 7f)
		{
			m_SplitNumDur -= 9f * Time.deltaTime;
		}
		else
		{
			m_SplitNumDur -= 12f * Time.deltaTime;
		}
		m_SplitNumDur = Mathf.Clamp(m_SplitNumDur, 1f, m_OpGird.Item.GetCount() - 1);
		m_SplitNumLb.text = ((int)m_SplitNumDur).ToString();
	}

	private void OnSplitOkBtn()
	{
		if (m_OpGird == null || m_OpGird.Item == null)
		{
			m_SplitWnd.SetActive(value: false);
			return;
		}
		m_SplitNumDur = Convert.ToInt32(m_SplitNumLb.text);
		if (m_SplitNumDur <= 0f)
		{
			return;
		}
		m_SplitNumDur = Mathf.Clamp(m_SplitNumDur, 1f, m_OpGird.Item.GetCount() - 1);
		if (!GameConfig.IsMultiMode)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(m_OpGird.Item.protoId);
			itemObject.IncreaseStackCount((int)m_SplitNumDur - 1);
			m_Package.AddItem(itemObject);
			m_OpGird.ItemObj.DecreaseStackCount((int)m_SplitNumDur);
			RestItems();
			m_OpGird = null;
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.SplitItem, itemObject.protoData.GetName(), m_SplitNumDur.ToString());
			}
		}
		else
		{
			if (m_storage != null)
			{
				m_storage._ColonyObj._Network.STO_Split(m_OpGird.ItemObj.instanceId, (int)m_SplitNumDur);
			}
			else
			{
				PlayerNetwork.mainPlayer.RequestPersonalStorageSplit(m_OpGird.ItemObj.instanceId, (int)m_SplitNumDur);
			}
			m_OpGird = null;
		}
		m_SplitWnd.SetActive(value: false);
	}

	private void OnSplitNoBtn()
	{
		m_OpGird = null;
		m_SplitWnd.SetActive(value: false);
	}

	private void OnDropItem(Grid_N grid)
	{
		if (CSUI_MainWndCtrl.Instance == null || grid == null || m_CurPack == null || !CSUI_MainWndCtrl.IsWorking())
		{
			return;
		}
		if (!m_IsWorking)
		{
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.CantWork, CSUtils.GetEntityName(2), null);
			}
			return;
		}
		if (SelectItem_N.Instance.ItemObj == null || SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (grid.ItemObj == null)
		{
			ItemPlaceType place = SelectItem_N.Instance.Place;
			if (GameConfig.IsMultiMode)
			{
				if (SelectItem_N.Instance.GridMask != GridMask.GM_Mission)
				{
					if (m_storage != null)
					{
						if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)
						{
							m_storage._ColonyObj._Network.STO_Store(grid.ItemIndex, SelectItem_N.Instance.ItemObj);
						}
						else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_CSStorage)
						{
							m_storage._ColonyObj._Network.STO_Exchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);
						}
						return;
					}
					if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)
					{
						PlayerNetwork.mainPlayer.RequestPersonalStorageStore(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
					}
					else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_NPCStorage)
					{
						PlayerNetwork.mainPlayer.RequestPersonalStorageExchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);
					}
					if (this.OpStatusEvent != null)
					{
						this.OpStatusEvent(EEventType.PutItemInto, SelectItem_N.Instance.ItemObj.protoData.GetName(), CSUtils.GetEntityName(2));
					}
				}
			}
			else if (SelectItem_N.Instance.GridMask != GridMask.GM_Mission)
			{
				SelectItem_N.Instance.RemoveOriginItem();
				grid.SetItem(SelectItem_N.Instance.ItemObj);
				m_CurPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
				if (this.OpStatusEvent != null)
				{
					this.OpStatusEvent(EEventType.PutItemInto, SelectItem_N.Instance.ItemObj.protoData.GetName(), CSUtils.GetEntityName(2));
				}
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (m_storage == null)
			{
				if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_NPCStorage)
				{
					PlayerNetwork.mainPlayer.RequestPersonalStorageExchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);
				}
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_CSStorage)
			{
				m_storage._ColonyObj._Network.STO_Exchange(SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Index, grid.ItemIndex);
			}
			return;
		}
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		ItemObject itemObj = SelectItem_N.Instance.ItemObj;
		ItemObject itemObj2 = grid.ItemObj;
		ItemPackage.ESlotType slotType = ItemPackage.GetSlotType(itemObj2.protoId);
		ItemPackage.ESlotType slotType2 = ItemPackage.GetSlotType(itemObj.protoId);
		if (slotType == slotType2 && null != SelectItem_N.Instance.Grid)
		{
			if (SelectItem_N.Instance.Grid.onGridsExchangeItem != null)
			{
				SelectItem_N.Instance.Grid.onGridsExchangeItem(SelectItem_N.Instance.Grid, itemObj2);
				grid.SetItem(itemObj);
				m_CurPack[grid.ItemIndex] = grid.ItemObj;
				SelectItem_N.Instance.SetItem(null);
			}
		}
		else if (cmpt.package.CanAdd(itemObj2))
		{
			cmpt.package.AddItem(itemObj2);
			grid.SetItem(itemObj);
			SelectItem_N.Instance.RemoveOriginItem();
			SelectItem_N.Instance.SetItem(null);
		}
	}

	private void OnGridsExchangeItems(Grid_N grid, ItemObject item)
	{
		grid.SetItem(item);
		m_CurPack[grid.ItemIndex] = grid.ItemObj;
	}

	private void RemoveOriginItem(Grid_N grid)
	{
		if (grid == null || grid.ItemIndex < -1 || m_CurPack.Count <= grid.ItemIndex)
		{
			Debug.LogWarning("The giving grid is error.");
			return;
		}
		if (this.OpStatusEvent != null)
		{
			this.OpStatusEvent(EEventType.TakeAwayItem, grid.Item.protoData.GetName(), CSUtils.GetEntityName(2));
		}
		m_CurPack[grid.ItemIndex] = null;
		grid.SetItem(null);
	}

	private void OnLeftMouseClicked(Grid_N grid)
	{
		if (!CSUI_MainWndCtrl.IsWorking())
		{
			return;
		}
		if (!m_IsWorking)
		{
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.CantWork, CSUtils.GetEntityName(2), null);
			}
		}
		else
		{
			if (grid.Item == null || EqualUsingItem(grid.Item, showUsingTip: false))
			{
				return;
			}
			switch (m_OpType)
			{
			case 0:
				SelectItem_N.Instance.SetItemGrid(grid);
				break;
			case 1:
				if (grid.Item.GetCount() > 1)
				{
					int num = -1;
					num = (GameConfig.IsMultiMode ? PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.GetVacancySlotIndex(ItemPackage.ESlotType.Item) : m_Package.GetVacancySlotIndex());
					if (num == -1)
					{
						MessageBox_N.ShowOkBox(PELocalization.GetString(8000102));
					}
					else if (m_OpGird == null)
					{
						m_SplitWnd.SetActive(value: true);
						m_OpGird = grid;
						m_SplitNumDur = 1f;
						m_SplitNumLb.text = "1";
					}
				}
				break;
			case 2:
				if (Input.GetMouseButtonDown(0))
				{
					m_OpGird = grid;
					if (m_OpGird.Item.protoId / 10000000 == 9)
					{
						MessageBox_N.ShowOkBox(PELocalization.GetString(8000054));
					}
					else
					{
						MessageBox_N.ShowYNBox(PELocalization.GetString(8000055), OnDeleteItem);
					}
				}
				break;
			}
		}
	}

	private void OnRightMouseClicked(Grid_N grid)
	{
		if (!CSUI_MainWndCtrl.IsWorking())
		{
			return;
		}
		if (!m_IsWorking)
		{
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.CantWork, CSUtils.GetEntityName(2), null);
			}
			return;
		}
		if (!GameUI.Instance.mItemPackageCtrl.IsOpen())
		{
			GameUI.Instance.mItemPackageCtrl.Show();
		}
		if (grid.ItemObj == null || EqualUsingItem(grid.Item, showUsingTip: false))
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (m_storage == null)
			{
				PlayerNetwork.mainPlayer.RequestPersonalStorageFetch(grid.ItemObj.instanceId, -1);
			}
			else
			{
				m_storage._ColonyObj._Network.STO_Fetch(grid.ItemObj.instanceId, -1);
			}
		}
		else if (PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>().Add(grid.ItemObj))
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			RemoveOriginItem(grid);
		}
		else
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
		}
	}

	private void OnPlayerPackageRightClicked(Grid_N grid)
	{
		if (!CSUI_MainWndCtrl.IsWorking(bShowMsg: false) || !m_IsWorking)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (m_storage == null)
			{
				PlayerNetwork.mainPlayer.RequestPersonalStorageStore(grid.ItemObj.instanceId, -1);
			}
			else
			{
				m_storage._ColonyObj._Network.STO_Store(-1, grid.ItemObj);
			}
		}
		else if (SetItemWithEmptyGrid(grid.ItemObj))
		{
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.PutItemInto, grid.Item.protoData.GetName(), CSUtils.GetEntityName(2));
			}
			PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>().Remove(grid.ItemObj);
			grid.SetItem(null);
		}
	}

	private void OnDeleteItem()
	{
		if (GameConfig.IsMultiMode)
		{
			if (m_storage == null)
			{
				PlayerNetwork.mainPlayer.RequestPersonalStorageDelete(m_OpGird.ItemObj.instanceId);
				m_OpGird = null;
			}
			else
			{
				m_storage._ColonyObj._Network.Delete(m_OpGird.ItemObj.instanceId);
			}
			return;
		}
		if (this.OpStatusEvent != null)
		{
			this.OpStatusEvent(EEventType.DeleteItem, m_OpGird.Item.protoData.GetName(), CSUtils.GetEntityName(2));
		}
		m_CurPack[m_OpGird.ItemIndex] = null;
		m_OpGird.SetItem(null);
	}

	private void OnDisable()
	{
		if (GameUI.Instance != null)
		{
			GameUI.Instance.mItemPackageCtrl.onRightMouseCliked -= OnPlayerPackageRightClicked;
		}
	}

	private void OnEnable()
	{
		if (GameUI.Instance != null)
		{
			GameUI.Instance.mItemPackageCtrl.onRightMouseCliked += OnPlayerPackageRightClicked;
		}
	}

	private void Start()
	{
		m_Grids = new List<Grid_N>();
		m_SplitWnd.SetActive(value: false);
		for (int i = 0; i < m_GridsRow; i++)
		{
			for (int j = 0; j < m_GridsCol; j++)
			{
				Grid_N grid_N = UnityEngine.Object.Instantiate(m_GridPrefab);
				grid_N.transform.parent = m_MainRoot.transform;
				grid_N.transform.localPosition = Vector3.zero;
				grid_N.transform.localRotation = Quaternion.identity;
				grid_N.transform.localScale = Vector3.one;
				grid_N.onDropItem = OnDropItem;
				grid_N.onRemoveOriginItem = RemoveOriginItem;
				grid_N.onLeftMouseClicked = OnLeftMouseClicked;
				grid_N.onRightMouseClicked = OnRightMouseClicked;
				grid_N.onGridsExchangeItem = OnGridsExchangeItems;
				m_Grids.Add(grid_N);
			}
		}
		m_MainRoot.repositionNow = true;
		RestItems();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.PageUp))
		{
			BtnLeftOnClick();
		}
		if (Input.GetKeyDown(KeyCode.PageDown))
		{
			BtnRightOnClick();
		}
		if ((PeInput.Get(PeInput.LogicFunction.OpenItemMenu) || Input.GetMouseButtonDown(1)) && !m_SplitWnd.activeSelf && m_OpGird == null)
		{
			m_OpType = 0;
			UICursor.Clear();
		}
		switch (m_OpType)
		{
		case 1:
			UICursor.Set(mNewUIAtlas, "icocai");
			break;
		case 2:
			UICursor.Set(mNewUIAtlas, "icodelete");
			break;
		}
	}

	public void SetPackageItemWithIndex(ItemPackage package, ItemObject item, int tabIndex, int index)
	{
		package.PutItem(item, index, (ItemPackage.ESlotType)tabIndex);
	}

	public void SetItemWithIndex(ItemObject item, int index)
	{
		int num = m_GridsRow * m_GridsCol;
		if (index / num == m_PageIndex)
		{
			m_Grids[index % num].SetItem(item);
		}
		else
		{
			RestItems();
		}
	}

	public void CSStoreResultStore(bool success, int index, int objId, CSStorage storage)
	{
		if (success && storage == m_storage)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.PutItemInto, itemObject.protoData.GetName(), CSUtils.GetEntityName(2));
			}
			if (m_Type == itemObject.protoData.tabIndex)
			{
				SetItemWithIndex(itemObject, index);
			}
		}
	}

	public void CSStoreResultFetch(bool success, int objId, CSStorage storage)
	{
		if (success && storage == m_storage)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.TakeAwayItem, itemObject.protoData.GetName(), CSUtils.GetEntityName(2));
			}
			if (m_Type == itemObject.protoData.tabIndex)
			{
				RestItems();
			}
		}
	}

	public void CSStoreResultExchange(bool success, int objId, int destIndex, int destId, int originIndex, CSStorage storage)
	{
		if (success && storage == m_storage)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
			int tabIndex = itemObject.protoData.tabIndex;
			ItemObject item = ((destId != -1) ? PeSingleton<ItemMgr>.Instance.Get(destId) : null);
			if (m_Type == tabIndex)
			{
				SetItemWithIndex(itemObject, destIndex);
				SetItemWithIndex(item, originIndex);
			}
		}
	}

	public void CSStoreSortSuccess(int tabIndex, int[] ids, CSStorage storage)
	{
		if (storage != m_storage || tabIndex != m_Type)
		{
			return;
		}
		int num = m_GridsRow * m_GridsCol;
		for (int i = 0; i < ids.Length; i++)
		{
			if (ids[i] == -1)
			{
				if (i / num == m_PageIndex)
				{
					m_Grids[i % num].SetItem(null);
				}
				continue;
			}
			ItemObject itemGrid = PeSingleton<ItemMgr>.Instance.Get(ids[i]);
			if (i / num == m_PageIndex)
			{
				m_Grids[i % num].SetItem(itemGrid);
			}
		}
	}

	public void CSStoreResultSplit(bool suc, int objId, int destIndex, CSStorage storage)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
		int tabIndex = itemObject.protoData.tabIndex;
		if (suc && storage == m_storage && tabIndex == m_Type)
		{
			SetItemWithIndex(itemObject, destIndex);
		}
	}

	public void CSStoreResultDelete(bool suc, int index, int objId, CSStorage storage)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
		int tabIndex = itemObject.protoData.tabIndex;
		if (suc && storage == m_storage)
		{
			if (this.OpStatusEvent != null)
			{
				this.OpStatusEvent(EEventType.DeleteItem, itemObject.protoData.GetName(), CSUtils.GetEntityName(2));
			}
			if (tabIndex == m_Type && index != -1)
			{
				SetItemWithIndex(null, index);
			}
		}
	}

	public void CSStorageResultSyncItemList(CSStorage storage)
	{
		if (storage == m_storage)
		{
			RestItems();
		}
	}
}
