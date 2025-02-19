using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMallWnd : UIBaseWnd
{
	public delegate void MallItemEvent(int starIndex, Mall_Tab tabIndex);

	public delegate void MallItemBuyEvent(Mall_Tab tabIndex, MallItemData mallItem);

	public const int GridCount = 12;

	private static UIMallWnd mInstance;

	[SerializeField]
	private GameObject mMailItemPrefab;

	[SerializeField]
	private UIGrid mGrid;

	[SerializeField]
	private UILabel mLbPageText;

	[SerializeField]
	private UILabel mLbMyPrice;

	private List<UIMallItem> mMallItemList;

	private int pageIndex;

	private int maxPageIndex = 1;

	private int maxItemCount;

	private int myPrice;

	public Mall_Tab mCurrentTab;

	public static UIMallWnd Instance => mInstance;

	public event MallItemEvent e_Reflash;

	public event MallItemBuyEvent e_OnBuyItemClick;

	public event MallItemBuyEvent e_ItemOnSelected;

	public event MallItemBuyEvent e_ItemExportClick;

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
		mMallItemList = new List<UIMallItem>();
	}

	private void Start()
	{
		InitGrid();
		ClearPageInfo();
		this.e_Reflash = (MallItemEvent)Delegate.Combine(this.e_Reflash, new MallItemEvent(UILobbyShopItemMgr._self.MallItemEvent));
		this.e_OnBuyItemClick = (MallItemBuyEvent)Delegate.Combine(this.e_OnBuyItemClick, new MallItemBuyEvent(UILobbyShopItemMgr._self.MallItemBuyEvent));
		this.e_ItemExportClick = (MallItemBuyEvent)Delegate.Combine(this.e_ItemExportClick, new MallItemBuyEvent(UILobbyShopItemMgr._self.MallItemExportEvent));
		OnCreate();
		SetMyBalance((int)AccountItems.self.balance);
	}

	private void InitGrid()
	{
		for (int i = 0; i < 12; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mMailItemPrefab);
			gameObject.transform.parent = mGrid.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			UIMallItem component = gameObject.GetComponent<UIMallItem>();
			component.mCheckBox.radioButtonRoot = base.gameObject.transform;
			component.e_ItemBuy += MallItemBuy_OnClick;
			component.e_OnClick += MallItemSelect;
			component.e_ItemExport += MallItemExport_OnClick;
			mMallItemList.Add(component);
		}
		mGrid.repositionNow = true;
	}

	private void BtnLeftOnClick()
	{
		if (pageIndex > 0)
		{
			pageIndex--;
			Reflash();
		}
	}

	private void BtnRightOnClick()
	{
		if (pageIndex < maxPageIndex - 1)
		{
			pageIndex++;
			Reflash();
		}
	}

	private void Reflash()
	{
		ClearPageInfo();
		int starIndex = pageIndex * 12;
		if (this.e_Reflash != null)
		{
			this.e_Reflash(starIndex, mCurrentTab);
		}
	}

	private void Update()
	{
		maxPageIndex = ((maxItemCount % 12 != 0) ? 2 : (maxItemCount / 12));
		if (maxPageIndex == 0)
		{
			maxPageIndex = 1;
		}
		mLbPageText.text = pageIndex + 1 + "/" + maxPageIndex;
	}

	private void ClearPageInfo()
	{
		for (int i = 0; i < 12; i++)
		{
			mMallItemList[i].ClearInfo();
		}
	}

	private void MallItemBuy_OnClick(int index, UIMallItem item)
	{
		if (this.e_OnBuyItemClick != null)
		{
			this.e_OnBuyItemClick(mCurrentTab, item.mData);
		}
	}

	private void MallItemExport_OnClick(int index, UIMallItem item)
	{
		if (this.e_ItemExportClick != null)
		{
			this.e_ItemExportClick(mCurrentTab, item.mData);
		}
	}

	private void MallItemSelect(int index, UIMallItem item)
	{
		if (this.e_ItemOnSelected != null)
		{
			this.e_ItemOnSelected(mCurrentTab, item.mData);
		}
	}

	private void BtnHot_OnClick()
	{
		if (mCurrentTab != 0)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Hot;
			Reflash();
		}
	}

	private void BtnTools_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Tools)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Tools;
			Reflash();
		}
	}

	private void BtnClothes_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Clothes)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Clothes;
			Reflash();
		}
	}

	private void BtnFace_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Face)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Face;
			Reflash();
		}
	}

	private void BtnItem_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Item)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Item;
			Reflash();
		}
	}

	private void BtnEquip_OnClick()
	{
		if (mCurrentTab != Mall_Tab.tab_Equip)
		{
			pageIndex = 0;
			mCurrentTab = Mall_Tab.tab_Equip;
			Reflash();
		}
	}

	public void SetMyBalance(int price)
	{
		mLbMyPrice.text = price + " [C8C800]PP[-]";
	}

	public void SetPageInfo(int _maxItemCount, List<MallItemData> itemDataList)
	{
		for (int i = 0; i < 12; i++)
		{
			if (i < itemDataList.Count)
			{
				mMallItemList[i].SetInfo(itemDataList[i], i);
			}
			else
			{
				mMallItemList[i].ClearInfo();
			}
		}
		maxItemCount = _maxItemCount;
	}
}
