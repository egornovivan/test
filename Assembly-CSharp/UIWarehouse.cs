using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UIWarehouse : UIBaseWnd
{
	public static Action OnShow;

	private int mRow = 5;

	private int mColumn = 6;

	private int mPageCount;

	public UILabel mPageCountText;

	public Grid_N mGridPrefab;

	public UICheckbox mItemCheckbox;

	public UICheckbox mEquipCheckbox;

	public UICheckbox mResCheckbox;

	public UICheckbox mArmorCheckbox;

	public GameObject mGridsContent;

	private List<Grid_N> mItems;

	public int mCurrentPickTab;

	private int mPageIndex;

	private SlotList m_CurrentPack;

	private ItemPackage mItemPackage;

	private Transform mOpObject;

	private PlayerPackageCmpt playerPackage;

	private WareHouseObject _wareObj;

	private void Start()
	{
		Show();
	}

	public override void Show()
	{
		playerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		ResetItem();
		if (GameUI.Instance.mItemPackageCtrl != null)
		{
			if (!GameUI.Instance.mItemPackageCtrl.isShow)
			{
				GameUI.Instance.mItemPackageCtrl.Show();
			}
			else
			{
				mCurrentPickTab = GameUI.Instance.mItemPackageCtrl.CurrentPickTab;
				mPageIndex = 0;
			}
		}
		base.Show();
		if (OnShow != null)
		{
			OnShow();
		}
	}

	public void ResetItemPacket(ItemPackage itemPackage, Transform obj, WareHouseObject wareObj)
	{
		mItemPackage = itemPackage;
		mOpObject = obj;
		_wareObj = wareObj;
	}

	protected override void InitWindow()
	{
		base.InitWindow();
	}

	public override void OnCreate()
	{
		base.OnCreate();
		InitGrid();
		mCurrentPickTab = 0;
		mPageIndex = 0;
	}

	private void InitGrid()
	{
		mItems = new List<Grid_N>();
		mPageCount = mRow * mColumn;
		for (int i = 0; i < mPageCount; i++)
		{
			mItems.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mItems[i].gameObject.name = "WarehouseGrid" + i;
			mItems[i].transform.parent = mGridsContent.transform;
			mItems[i].transform.localPosition = new Vector3(i % mColumn * 55, -i / mColumn * 52, 0f);
			mItems[i].transform.localRotation = Quaternion.identity;
			mItems[i].transform.localScale = Vector3.one;
			mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
			mItems[i].onRightMouseClicked = OnRightMouseCliked;
			mItems[i].onDropItem = OnDropItem;
		}
	}

	private void OnItemBtn()
	{
		mCurrentPickTab = 0;
		mPageIndex = 0;
		ResetItem();
		ChangeItemPackageTab();
	}

	private void OnEquipmentBtn()
	{
		mCurrentPickTab = 1;
		mPageIndex = 0;
		ResetItem();
		ChangeItemPackageTab();
	}

	private void OnResourceBtn()
	{
		mCurrentPickTab = 2;
		mPageIndex = 0;
		ResetItem();
		ChangeItemPackageTab();
	}

	private void OnArmorBtn()
	{
		mCurrentPickTab = 3;
		mPageIndex = 0;
		ResetItem();
		ChangeItemPackageTab();
	}

	private void ChangeItemPackageTab()
	{
		if (GameUI.Instance.mItemPackageCtrl != null && GameUI.Instance.mItemPackageCtrl.isShow)
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	private void BtnLeftOnClick()
	{
		if (mPageIndex > 0)
		{
			mPageIndex--;
			ResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	private void BtnRightOnClick()
	{
		if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
		{
			mPageIndex++;
			ResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	private void BtnLeftEndOnClick()
	{
		if (mPageIndex > 0)
		{
			mPageIndex = 0;
			ResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	private void BtnRightEndOnClick()
	{
		if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
		{
			mPageIndex = (m_CurrentPack.Count - 1) / mPageCount;
			ResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	public void ResetItem(int type, int pageIndex)
	{
		if (mItemPackage == null)
		{
			return;
		}
		mCurrentPickTab = type;
		switch (type)
		{
		case 0:
			mItemCheckbox.isChecked = true;
			break;
		case 1:
			mEquipCheckbox.isChecked = true;
			break;
		case 2:
			mResCheckbox.isChecked = true;
			break;
		case 3:
			mArmorCheckbox.isChecked = true;
			break;
		}
		m_CurrentPack = mItemPackage.GetSlotList((ItemPackage.ESlotType)type);
		if ((m_CurrentPack.Count - 1) / mPageCount < pageIndex)
		{
			pageIndex = (m_CurrentPack.Count - 1) / mPageCount;
		}
		mPageIndex = pageIndex;
		int num = (((m_CurrentPack.Count - 1) / mPageCount != mPageIndex) ? mPageCount : (m_CurrentPack.Count - pageIndex * mPageCount));
		for (int i = 0; i < num; i++)
		{
			mItems[i].SetItem(m_CurrentPack[i + pageIndex * mPageCount]);
			mItems[i].SetItemPlace(ItemPlaceType.IPT_Warehouse, i + pageIndex * mPageCount);
			switch (mCurrentPickTab)
			{
			case 0:
				mItems[i].SetGridMask(GridMask.GM_Item);
				break;
			case 1:
				mItems[i].SetGridMask(GridMask.GM_Equipment);
				break;
			case 2:
				mItems[i].SetGridMask(GridMask.GM_Resource);
				break;
			case 3:
				mItems[i].SetGridMask(GridMask.GM_Armor);
				break;
			}
		}
		mPageCountText.text = (mPageIndex + 1).ToString() + "/" + ((m_CurrentPack.Count - 1) / mPageCount + 1);
	}

	public void ResetItem()
	{
		ResetItem(mCurrentPickTab, mPageIndex);
	}

	public bool SetItemWithIndex(ItemObject itemObj, int index = -1)
	{
		bool result = false;
		if (index == -1)
		{
			result = m_CurrentPack.Add(itemObj);
		}
		else
		{
			if (index < 0 || index > m_CurrentPack.Count)
			{
				result = false;
			}
			if (m_CurrentPack != null)
			{
				m_CurrentPack[index] = itemObj;
				result = true;
			}
		}
		ResetItem();
		return result;
	}

	private void OnLeftMouseCliked(Grid_N grid)
	{
		ActiveWnd();
		SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
	}

	private void OnRightMouseCliked(Grid_N grid)
	{
		ActiveWnd();
		GameUI.Instance.mItemPackageCtrl.Show();
		if (grid.ItemObj == null || !(playerPackage != null))
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			if (_wareObj != null)
			{
				_wareObj._objNet.GetItem(grid.ItemObj.instanceId);
			}
		}
		else if (playerPackage.Add(grid.ItemObj))
		{
			SetItemWithIndex(null, grid.ItemIndex);
		}
		else
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
		}
	}

	private void OnDropItem(Grid_N grid)
	{
		if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
		{
			return;
		}
		ActiveWnd();
		if (PeGameMgr.IsMulti)
		{
			if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag && _wareObj != null)
			{
				_wareObj._objNet.InsertItemList(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex + mCurrentPickTab * mPageCount);
			}
			return;
		}
		if (grid.ItemObj == null)
		{
			ItemPlaceType place = SelectItem_N.Instance.Place;
			if (place == ItemPlaceType.IPT_HotKeyBar)
			{
				SelectItem_N.Instance.SetItem(null);
				return;
			}
			if (SelectItem_N.Instance.GridMask != GridMask.GM_Mission)
			{
				SelectItem_N.Instance.RemoveOriginItem();
				grid.SetItem(SelectItem_N.Instance.ItemObj);
				m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Warehouse:
		{
			ItemObject itemObj = SelectItem_N.Instance.ItemObj;
			m_CurrentPack[SelectItem_N.Instance.Index] = grid.ItemObj;
			m_CurrentPack[grid.ItemIndex] = itemObj;
			SelectItem_N.Instance.SetItem(null);
			ResetItem();
			break;
		}
		case ItemPlaceType.IPT_Bag:
			if (SelectItem_N.Instance.RemoveOriginItem())
			{
				GameUI.Instance.mItemPackageCtrl.SetItemWithIndex(grid.ItemObj, SelectItem_N.Instance.Index);
				m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
				SelectItem_N.Instance.SetItem(null);
				ResetItem();
			}
			break;
		default:
			SelectItem_N.Instance.SetItem(null);
			break;
		}
	}

	private void Update()
	{
		if (!(GameUI.Instance == null) && null != GameUI.Instance.mMainPlayer && null != mOpObject && Vector3.Distance(GameUI.Instance.mMainPlayer.position, mOpObject.position) > 10f)
		{
			OnClose();
		}
	}
}
