using System;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class UIShopWnd : UIBaseWnd
{
	private int mRow = 5;

	private int mColumn = 10;

	private int mPageCount;

	public UILabel mPageCountText;

	public Grid_N mGridPrefab;

	public UIPanel mSellOpLayer;

	public UILabel mPriceLabel;

	public UIInput mOpNumLabel;

	public UILabel mTotalLabel;

	public UILabel mOKBtn;

	public Grid_N mOpItem;

	public UICheckbox mBtnAll;

	public UILabel mLbNpcMoney;

	public UILabel mLbNpcName;

	public UITexture mTxNpcIco;

	public UISprite mSpNpcIco;

	public Transform mItemGridsContent;

	public UISlicedSprite mMeatSprite;

	public UISlicedSprite mMoneySprite;

	private List<Grid_N> mItems;

	private int mPageIndex;

	private int mCurrentPickTab;

	private bool mIsBuy;

	private int MaxPage;

	private Grid_N mOpGrid;

	private int mPrice;

	private float mCurrentNum;

	private float mOpDurNum;

	private List<ItemObject> mBuyItemList;

	private List<ItemObject> mRepurchaseList;

	private List<ItemObject> mTypeOfBuyItemList;

	public ItemLabel.Root shopSelectItemType;

	private List<ItemObject> m_CurrentPack;

	private List<int> m_ShopIDList;

	private List<int> m_TypeShopIDList;

	public int m_CurNpcID;

	public PeEntity npc;

	private float mOpStarTime;

	private float mLastOpTime;

	private bool mAddBtnPress;

	private bool mSubBtnPress;

	private Vector3 prePos;

	public int CurrentTab => mCurrentPickTab;

	public bool isShopping { get; private set; }

	public List<ItemObject> RepurchaseList => mRepurchaseList;

	protected override void InitWindow()
	{
		base.InitWindow();
	}

	public override void OnCreate()
	{
		base.OnCreate();
		mItems = new List<Grid_N>();
		mPageCount = mRow * mColumn;
		for (int i = 0; i < mPageCount; i++)
		{
			mItems.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mItems[i].transform.parent = mItemGridsContent;
			mItems[i].transform.localPosition = new Vector3(-234 + i % mColumn * 52, 92 - i / mColumn * 54, -1f);
			mItems[i].transform.localRotation = Quaternion.identity;
			mItems[i].transform.localScale = Vector3.one;
			mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
			mItems[i].onRightMouseClicked = OnRightMouseCliked;
		}
		mCurrentPickTab = 0;
		mPageIndex = 0;
		mBuyItemList = new List<ItemObject>();
		mRepurchaseList = new List<ItemObject>();
		m_CurrentPack = new List<ItemObject>();
		mTypeOfBuyItemList = new List<ItemObject>();
		m_ShopIDList = new List<int>();
		m_TypeShopIDList = new List<int>();
		if (!Money.Digital)
		{
			mMeatSprite.gameObject.SetActive(value: true);
			mMoneySprite.gameObject.SetActive(value: false);
		}
		else
		{
			mMeatSprite.gameObject.SetActive(value: false);
			mMoneySprite.gameObject.SetActive(value: true);
		}
	}

	public override void Show()
	{
		mSellOpLayer.gameObject.SetActive(value: false);
		mOpItem.mShowNum = false;
		mBtnAll.isChecked = true;
		shopSelectItemType = ItemLabel.Root.all;
		mCurrentPickTab = 0;
		ResetItem(mCurrentPickTab, 0);
		npc = PeSingleton<EntityMgr>.Instance.Get(m_CurNpcID);
		EntityInfoCmpt cmpt = npc.GetCmpt<EntityInfoCmpt>();
		if (npc != null)
		{
			if (cmpt != null)
			{
				SetNpcName(cmpt.characterName.fullName);
			}
			NpcPackageCmpt cmpt2 = npc.GetCmpt<NpcPackageCmpt>();
			if (cmpt2 == null)
			{
				return;
			}
			SetNpcMoney(cmpt2.money.current);
			Texture texture = null;
			string npcICO = string.Empty;
			if (EntityCreateMgr.Instance.IsRandomNpc(npc))
			{
				texture = npc.ExtGetFaceTex();
				if (texture == null)
				{
					npcICO = npc.ExtGetFaceIcon();
				}
			}
			else
			{
				npcICO = npc.ExtGetFaceIcon();
			}
			if (texture != null)
			{
				SetNpcICO(texture);
			}
			else
			{
				SetNpcICO(npcICO);
			}
			StroyManager.Instance.SetTalking(PeSingleton<EntityMgr>.Instance.Get(m_CurNpcID), string.Empty);
			isShopping = true;
		}
		if (!GameUI.Instance.mItemPackageCtrl.isShow)
		{
			GameUI.Instance.mItemPackageCtrl.Show();
		}
		prePos = GameUI.Instance.mItemPackageCtrl.transform.localPosition;
		GameUI.Instance.mItemPackageCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_ItemPackge;
		base.transform.localPosition = UIDefaultPostion.Instance.pos_Shop;
		base.Show();
	}

	protected override void OnClose()
	{
		GameUI.Instance.mItemPackageCtrl.transform.localPosition = prePos;
		base.OnClose();
	}

	public void Sell(Grid_N grid, int num)
	{
		GameUI.Instance.mItemPackageCtrl.ResetCurOpItem();
		if (null == grid || grid.ItemObj == null)
		{
			Debug.LogError("Sell item is null");
			return;
		}
		int protoId = grid.ItemObj.protoId;
		int instanceId = grid.ItemObj.instanceId;
		int stackMax = grid.ItemObj.GetStackMax();
		int sellPrice = grid.ItemObj.GetSellPrice();
		if (npc == null)
		{
			return;
		}
		NpcPackageCmpt cmpt = npc.GetCmpt<NpcPackageCmpt>();
		if (cmpt == null)
		{
			return;
		}
		sellPrice *= num;
		if (sellPrice > cmpt.money.current)
		{
			string empty = string.Empty;
			EntityInfoCmpt cmpt2 = npc.GetCmpt<EntityInfoCmpt>();
			if (cmpt2 != null)
			{
				string empty2 = string.Empty;
				empty2 = cmpt2.characterName.fullName;
				empty = empty2 + " " + PELocalization.GetString(8000853);
			}
			else
			{
				empty = npc.name + " " + PELocalization.GetString(8000853);
			}
			new PeTipMsg(empty, PeTipMsg.EMsgLevel.Error);
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestSell(npc.Id, instanceId, num);
		}
		else
		{
			cmpt.money.current -= sellPrice;
			ItemObject item = ((stackMax != 1) ? PeSingleton<ItemMgr>.Instance.CreateItem(protoId) : grid.ItemObj);
			PlayerPackageCmpt cmpt3 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (null == cmpt3)
			{
				return;
			}
			if (stackMax == 1)
			{
				cmpt3.Remove(item);
			}
			else
			{
				cmpt3.DestroyItem(instanceId, num);
			}
			AddRepurchase(item, num);
			cmpt3.money.current += sellPrice;
			ResetItem();
		}
		mOpItem.SetItem(null);
		mOpGrid = null;
		mSellOpLayer.gameObject.SetActive(value: false);
	}

	public bool UpdataShop(StoreData npc)
	{
		if (!mInit)
		{
			InitWindow();
		}
		PeEntity curSelNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
		if (curSelNpc == null)
		{
			return false;
		}
		GameUI.Instance.mNpcWnd.Hide();
		mBuyItemList.Clear();
		if (npc == null)
		{
			return false;
		}
		int count = npc.itemList.Count;
		if (count < 1)
		{
			return false;
		}
		m_ShopIDList.Clear();
		m_CurNpcID = curSelNpc.Id;
		if (!StroyManager.Instance.m_BuyInfo.ContainsKey(m_CurNpcID))
		{
			StroyManager.Instance.InitBuyInfo(npc, m_CurNpcID);
		}
		Dictionary<int, stShopData> shopList = StroyManager.Instance.m_BuyInfo[m_CurNpcID].ShopList;
		bool flag = true;
		foreach (int key in shopList.Keys)
		{
			ShopData shopData = ShopRespository.GetShopData(key);
			if (shopData == null)
			{
				continue;
			}
			flag = true;
			for (int i = 0; i < shopData.m_LimitMisIDList.Count; i++)
			{
				if (shopData.m_LimitType == 1)
				{
					if (MissionManager.Instance.HadCompleteMission(shopData.m_LimitMisIDList[i]))
					{
						break;
					}
				}
				else if (!MissionManager.Instance.HadCompleteMission(shopData.m_LimitMisIDList[i]))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			if (GameTime.Timer.Second - shopList[key].CreateTime > (double)shopData.m_RefreshTime)
			{
				if (shopList[key].ItemObjID != 0)
				{
					PeSingleton<ItemMgr>.Instance.DestroyItem(shopList[key].ItemObjID);
				}
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(shopData.m_ItemID);
				itemObject.stackCount = shopData.m_LimitNum;
				shopList[key].ItemObjID = itemObject.instanceId;
				shopList[key].CreateTime = GameTime.Timer.Second;
				mBuyItemList.Add(itemObject);
				m_ShopIDList.Add(shopData.m_ID);
			}
			else if (shopList[key].ItemObjID != 0)
			{
				ItemObject itemObject2 = PeSingleton<ItemMgr>.Instance.Get(shopList[key].ItemObjID);
				if (itemObject2 != null)
				{
					shopList[key].ItemObjID = itemObject2.instanceId;
					shopList[key].CreateTime = GameTime.Timer.Second;
					mBuyItemList.Add(itemObject2);
					m_ShopIDList.Add(shopData.m_ID);
				}
			}
		}
		mRepurchaseList.Clear();
		if (StroyManager.Instance.m_SellInfo.ContainsKey(m_CurNpcID))
		{
			List<ItemObject> list = StroyManager.Instance.m_SellInfo[m_CurNpcID];
			for (int j = 0; j < list.Count; j++)
			{
				mRepurchaseList.Add(list[j]);
			}
		}
		else
		{
			StroyManager.Instance.m_SellInfo.Add(m_CurNpcID, new List<ItemObject>());
		}
		CSMain.AddTradeNpc(m_CurNpcID, shopList.Keys.ToList());
		ResetItem();
		return true;
	}

	public void AddRepurchase(ItemObject item, int num)
	{
		if (num > 1)
		{
			item.SetStackCount(num);
		}
		mRepurchaseList.Add(item);
	}

	public void RemoveBuyItem(int index, int num)
	{
		if (mBuyItemList[index].GetCount() < num)
		{
			Debug.LogError("Remove num is big than item you have.");
			return;
		}
		if (mBuyItemList[index].GetCount() > num)
		{
			mBuyItemList[index].DecreaseStackCount(num);
			return;
		}
		mBuyItemList.RemoveAt(index);
		if (index >= 0 && index < m_ShopIDList.Count)
		{
			m_ShopIDList.RemoveAt(index);
		}
	}

	public void RemoveRepurchase(int index, int num)
	{
		if (mRepurchaseList.Count <= index || mRepurchaseList[index].GetCount() < num)
		{
			Debug.LogError("Remove num is big than item you have.");
		}
		else if (mRepurchaseList[index].GetCount() > num)
		{
			mRepurchaseList[index].DecreaseStackCount(num);
		}
		else
		{
			mRepurchaseList.RemoveAt(index);
		}
	}

	public void ResetItem(int type, int pageIndex)
	{
		if (type == 0)
		{
			UpdateBuyItemList(shopSelectItemType);
			m_CurrentPack = mTypeOfBuyItemList;
		}
		else
		{
			m_CurrentPack = mRepurchaseList;
		}
		MaxPage = (m_CurrentPack.Count - 1) / mPageCount;
		if (MaxPage < 0)
		{
			MaxPage = 0;
		}
		if (MaxPage < pageIndex)
		{
			pageIndex = MaxPage;
		}
		mPageIndex = pageIndex;
		int num = ((MaxPage != mPageIndex) ? mPageCount : (m_CurrentPack.Count - pageIndex * mPageCount));
		for (int i = 0; i < mPageCount; i++)
		{
			if (i < num)
			{
				mItems[i].SetItem(m_CurrentPack[i + mPageCount * mPageIndex]);
			}
			else
			{
				mItems[i].SetItem(null);
			}
			mItems[i].SetItemPlace(ItemPlaceType.IPT_Shop, i + pageIndex * mPageCount);
			mItems[i].SetGridMask(GridMask.GM_Any);
		}
		mPageCountText.text = (mPageIndex + 1).ToString() + "/" + ((m_CurrentPack.Count - 1) / mPageCount + 1);
	}

	public void ResetItem()
	{
		ResetItem(mCurrentPickTab, mPageIndex);
		if (!(npc == null))
		{
			NpcPackageCmpt cmpt = npc.GetCmpt<NpcPackageCmpt>();
			if (!(cmpt == null))
			{
				SetNpcMoney(cmpt.money.current);
			}
		}
	}

	private void Buy(Grid_N grid, int num)
	{
		if (m_TypeShopIDList == null || m_ShopIDList == null)
		{
			return;
		}
		int num2 = 0;
		int num3 = 0;
		bool flag;
		if (mCurrentPickTab == 0)
		{
			flag = false;
			if (m_TypeShopIDList.Count <= grid.ItemIndex)
			{
				return;
			}
			num2 = m_TypeShopIDList[grid.ItemIndex];
			ShopData shopData = ShopRespository.GetShopData(num2);
			if (shopData != null)
			{
				num3 = shopData.m_Price;
			}
		}
		else
		{
			flag = true;
			if (grid.ItemObj.protoData != null)
			{
				num3 = grid.ItemObj.protoData.currency;
				mSellOpLayer.gameObject.SetActive(value: false);
			}
		}
		if (PeGameMgr.IsMulti)
		{
			mSellOpLayer.gameObject.SetActive(value: false);
			if (flag)
			{
				PlayerNetwork.mainPlayer.RequestRepurchase(npc.Id, grid.ItemObj.instanceId, num);
			}
			else
			{
				PlayerNetwork.mainPlayer.RequestBuy(npc.Id, grid.ItemObj.instanceId, num);
			}
		}
		else
		{
			if (!StroyManager.Instance.BuyItem(grid.ItemObj, num, num2, m_CurNpcID, !flag))
			{
				return;
			}
			num3 *= num;
			if (npc != null)
			{
				NpcPackageCmpt cmpt = npc.GetCmpt<NpcPackageCmpt>();
				if (cmpt == null)
				{
					return;
				}
				cmpt.money.current += num3;
			}
			if (flag)
			{
				RemoveRepurchase(grid.ItemIndex, num);
			}
			else
			{
				int num4 = -1;
				if (grid.ItemIndex < m_TypeShopIDList.Count)
				{
					for (int i = 0; i < m_ShopIDList.Count; i++)
					{
						if (m_ShopIDList[i] == m_TypeShopIDList[grid.ItemIndex])
						{
							num4 = i;
							break;
						}
					}
					mSellOpLayer.gameObject.SetActive(value: false);
					if (num4 != -1)
					{
						RemoveBuyItem(num4, num);
					}
				}
			}
			ResetItem();
		}
	}

	public void PreSell(Grid_N grid)
	{
		if (grid.ItemObj != null)
		{
			mSellOpLayer.gameObject.SetActive(value: true);
			mIsBuy = false;
			mOpGrid = grid;
			mOpItem.SetItem(grid.ItemObj);
			mCurrentNum = 1f;
			mPrice = grid.ItemObj.GetSellPrice();
			mOpNumLabel.text = mCurrentNum.ToString();
			mPriceLabel.text = mPrice.ToString();
			mTotalLabel.text = mPrice.ToString();
			mOKBtn.text = PELocalization.GetString(8000555);
			mOpItem.SetItem(grid.ItemObj);
		}
	}

	public void CloseSellWnd()
	{
		if (mSellOpLayer.gameObject.activeSelf && !mIsBuy)
		{
			mSellOpLayer.gameObject.SetActive(value: false);
		}
	}

	public void OnLeftMouseCliked(Grid_N grid)
	{
		if (grid.ItemObj == null)
		{
			return;
		}
		if (grid.ItemObj.GetCount() == 0)
		{
			new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		ActiveWnd();
		if (mSellOpLayer.gameObject.activeSelf)
		{
			return;
		}
		SelectItem_N.Instance.SetItem(null);
		mSellOpLayer.gameObject.SetActive(value: true);
		mIsBuy = true;
		mOpGrid = grid;
		mOpItem.SetItem(grid.ItemObj);
		mCurrentNum = 1f;
		if (mCurrentPickTab == 0)
		{
			if (m_TypeShopIDList.Count <= grid.ItemIndex)
			{
				return;
			}
			int id = m_TypeShopIDList[grid.ItemIndex];
			ShopData shopData = ShopRespository.GetShopData(id);
			if (shopData == null)
			{
				mPrice = 0;
			}
			else
			{
				mPrice = grid.ItemObj.GetBuyPrice();
			}
		}
		else
		{
			mPrice = grid.ItemObj.GetSellPrice();
		}
		mOpNumLabel.text = mCurrentNum.ToString();
		mPriceLabel.text = mPrice.ToString();
		mTotalLabel.text = mPrice.ToString();
		mOKBtn.text = PELocalization.GetString(8000556);
	}

	public void OnRightMouseCliked(Grid_N grid)
	{
		if (grid.ItemObj != null)
		{
			if (grid.ItemObj.GetCount() == 0)
			{
				new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
			}
			else if (!mSellOpLayer.gameObject.activeSelf)
			{
				ActiveWnd();
				SelectItem_N.Instance.SetItem(null);
				mOpGrid = grid;
				int count = mOpGrid.Item.GetCount();
				int num = ((!mRepurchaseList.Contains(mOpGrid.ItemObj)) ? mOpGrid.ItemObj.GetBuyPrice() : mOpGrid.ItemObj.GetSellPrice());
				string text = mOpGrid.Item.protoData.GetName();
				string text2 = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000077), text + " X " + count, PELocalization.GetString(8000253), count * num);
				MessageBox_N.ShowYNBox(text2, BuyAll);
			}
		}
	}

	public void BuyAll()
	{
		if (!(null == mOpGrid) && mOpGrid.ItemObj != null)
		{
			Buy(mOpGrid, mOpGrid.ItemObj.GetCount());
		}
	}

	private void OnBuyBtn()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void OnSellBtn()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			mCurrentPickTab = 1;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void OnOpOkBtn()
	{
		if (!(null == mOpGrid))
		{
			if (mIsBuy)
			{
				Buy(mOpGrid, (int)mCurrentNum);
			}
			else
			{
				Sell(mOpGrid, (int)mCurrentNum);
			}
		}
	}

	private void OnOpCancelBtn()
	{
		mSellOpLayer.gameObject.SetActive(value: false);
		GameUI.Instance.mItemPackageCtrl.ResetCurOpItem();
	}

	private void OnPageDown()
	{
		if (mPageIndex > 0)
		{
			mPageIndex--;
			ResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	private void OnPageUp()
	{
		if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
		{
			mPageIndex++;
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

	private void BtnLeftEndOnClick()
	{
		if (mPageIndex > 0)
		{
			mPageIndex = 0;
			ResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	private void OnAddBtnPress()
	{
		mAddBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0f;
	}

	private void OnAddBtnRelease()
	{
		mAddBtnPress = false;
		mCurrentNum = (int)(mCurrentNum + mOpDurNum);
		mOpDurNum = 0f;
		mOpNumLabel.text = ((int)mCurrentNum).ToString();
		mTotalLabel.text = (mPrice * (int)mCurrentNum).ToString();
	}

	private void OnSubstructBtnPress()
	{
		mSubBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0f;
	}

	private void OnSubstructBtnRelease()
	{
		mSubBtnPress = false;
		mCurrentNum = (int)(mCurrentNum + mOpDurNum);
		mOpDurNum = 0f;
		mOpNumLabel.text = ((int)mCurrentNum).ToString();
		mTotalLabel.text = (mPrice * (int)mCurrentNum).ToString();
	}

	protected override void OnHide()
	{
		if (mInit && isShow)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_CurNpcID);
			if (peEntity != null)
			{
				StroyManager.Instance.RemoveReq(peEntity, EReqType.Dialogue);
			}
			if (!PeGameMgr.IsMulti && StroyManager.Instance.m_SellInfo != null && StroyManager.Instance.m_SellInfo.ContainsKey(m_CurNpcID))
			{
				StroyManager.Instance.m_SellInfo[m_CurNpcID].Clear();
				for (int i = 0; i < mRepurchaseList.Count; i++)
				{
					StroyManager.Instance.m_SellInfo[m_CurNpcID].Add(mRepurchaseList[i]);
				}
			}
			m_CurNpcID = 0;
		}
		base.OnHide();
		isShopping = false;
	}

	public bool InitNpcShopWhenMultiMode(int npcid, int[] ids)
	{
		if (!mInit)
		{
			InitWindow();
		}
		GameUI.Instance.mNpcWnd.Hide();
		mBuyItemList.Clear();
		m_ShopIDList.Clear();
		foreach (int id in ids)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
			if (itemObject != null)
			{
				ShopData shopDataByItemId = ShopRespository.GetShopDataByItemId(itemObject.protoId);
				if (shopDataByItemId == null)
				{
					LogManager.Error("data is null! itemID = ", itemObject.protoId);
					mBuyItemList.Add(null);
					m_ShopIDList.Add(-1);
				}
				else
				{
					mBuyItemList.Add(itemObject);
					m_ShopIDList.Add(shopDataByItemId.m_ID);
				}
			}
			else
			{
				LogManager.Error("itemObj is null!");
				mBuyItemList.Add(null);
				m_ShopIDList.Add(-1);
			}
		}
		m_CurNpcID = npcid;
		ResetItem();
		return true;
	}

	public bool InitShopWhenMutipleMode(int[] objIDs)
	{
		if (!mInit)
		{
			InitWindow();
		}
		return UpdataShop(objIDs);
	}

	public bool UpdataShop(int[] objIDs)
	{
		mBuyItemList.Clear();
		m_ShopIDList.Clear();
		foreach (int num in objIDs)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(num);
			if (itemObject != null)
			{
				ShopData shopDataByItemId = ShopRespository.GetShopDataByItemId(itemObject.protoId);
				if (shopDataByItemId == null)
				{
					LogManager.Error("data is null! itemID = ", itemObject.protoId);
					mBuyItemList.Add(null);
					m_ShopIDList.Add(-1);
				}
				else
				{
					mBuyItemList.Add(itemObject);
					m_ShopIDList.Add(shopDataByItemId.m_ID);
				}
			}
			else
			{
				LogManager.Error("itemObj is null! ID = ", num);
				mBuyItemList.Add(null);
				m_ShopIDList.Add(-1);
			}
		}
		return true;
	}

	public bool AddNewItemOnSell(int objID, int index)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objID);
		if (itemObject != null)
		{
			LogManager.Info("An item is on sell!ID:", objID);
			ShopData shopDataByItemId = ShopRespository.GetShopDataByItemId(itemObject.protoId);
			itemObject.IncreaseStackCount(shopDataByItemId.m_LimitNum);
			mBuyItemList[index] = itemObject;
			m_ShopIDList[index] = shopDataByItemId.m_ID;
			return true;
		}
		LogManager.Error("itemObj is null! ID = ", objID);
		return false;
	}

	public bool AddNewRepurchaseItem(int objID)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objID);
		if (itemObject != null)
		{
			LogManager.Info("a item is on repurchase!ID:", objID);
			mRepurchaseList.Add(itemObject);
			return true;
		}
		LogManager.Error("itemObj is null! ID = ", objID);
		return false;
	}

	public bool RemoveRepurchaseItem(int objID)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objID);
		if (itemObject != null)
		{
			mRepurchaseList.Remove(itemObject);
			LogManager.Info("RemoveRepurchase: objID=" + objID);
			ResetItem();
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (!(null != mOpGrid) || mOpGrid.Item == null)
		{
			return;
		}
		if (mAddBtnPress)
		{
			float num = Time.time - mOpStarTime;
			if (num < 0.2f)
			{
				mOpDurNum = 1f;
			}
			else if (num < 1f)
			{
				mOpDurNum += 2f * Time.deltaTime;
			}
			else if (num < 2f)
			{
				mOpDurNum += 4f * Time.deltaTime;
			}
			else if (num < 3f)
			{
				mOpDurNum += 7f * Time.deltaTime;
			}
			else if (num < 4f)
			{
				mOpDurNum += 11f * Time.deltaTime;
			}
			else if (num < 5f)
			{
				mOpDurNum += 16f * Time.deltaTime;
			}
			else
			{
				mOpDurNum += 20f * Time.deltaTime;
			}
			mOpDurNum = Mathf.Clamp(mOpDurNum + mCurrentNum, 1f, mOpGrid.ItemObj.GetCount()) - mCurrentNum;
			mOpNumLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
			mTotalLabel.text = (mPrice * (int)(mOpDurNum + mCurrentNum)).ToString();
		}
		else if (mSubBtnPress)
		{
			float num2 = Time.time - mOpStarTime;
			if (num2 < 0.5f)
			{
				mOpDurNum = -1f;
			}
			else if (num2 < 1f)
			{
				mOpDurNum -= 2f * Time.deltaTime;
			}
			else if (num2 < 2f)
			{
				mOpDurNum -= 4f * Time.deltaTime;
			}
			else if (num2 < 3f)
			{
				mOpDurNum -= 7f * Time.deltaTime;
			}
			else if (num2 < 4f)
			{
				mOpDurNum -= 11f * Time.deltaTime;
			}
			else if (num2 < 5f)
			{
				mOpDurNum -= 16f * Time.deltaTime;
			}
			else
			{
				mOpDurNum -= 20f * Time.deltaTime;
			}
			mOpDurNum = Mathf.Clamp(mOpDurNum + mCurrentNum, 1f, mOpGrid.ItemObj.GetCount()) - mCurrentNum;
			mOpNumLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
			mTotalLabel.text = (mPrice * (int)(mOpDurNum + mCurrentNum)).ToString();
		}
		else
		{
			if (string.Empty == mOpNumLabel.text)
			{
				mCurrentNum = 1f;
			}
			else
			{
				mCurrentNum = Mathf.Clamp(Convert.ToInt32(mOpNumLabel.text), 1, mOpGrid.Item.GetCount());
			}
			if (!UICamera.inputHasFocus)
			{
				mOpNumLabel.text = mCurrentNum.ToString();
				mTotalLabel.text = (mPrice * (int)mCurrentNum).ToString();
			}
		}
	}

	private void UpdateBuyItemList(ItemLabel.Root _type)
	{
		if (mTypeOfBuyItemList == null || mBuyItemList == null || m_TypeShopIDList == null || m_ShopIDList == null)
		{
			return;
		}
		mTypeOfBuyItemList.Clear();
		for (int i = 0; i < mBuyItemList.Count; i++)
		{
			if (_type == ItemLabel.Root.all)
			{
				mTypeOfBuyItemList.Add(mBuyItemList[i]);
			}
			else if (mBuyItemList[i] != null && mBuyItemList[i].protoData != null && mBuyItemList[i].protoData != null)
			{
				ItemLabel.Root rootItemLabel = mBuyItemList[i].protoData.rootItemLabel;
				if (rootItemLabel == _type)
				{
					mTypeOfBuyItemList.Add(mBuyItemList[i]);
				}
			}
		}
		m_TypeShopIDList.Clear();
		for (int j = 0; j < m_ShopIDList.Count; j++)
		{
			if (_type == ItemLabel.Root.all)
			{
				m_TypeShopIDList.Add(m_ShopIDList[j]);
				continue;
			}
			ShopData shopData = ShopRespository.GetShopData(m_ShopIDList[j]);
			if (shopData == null)
			{
				continue;
			}
			ItemProto itemData = ItemProto.GetItemData(shopData.m_ItemID);
			if (itemData != null)
			{
				ItemLabel.Root rootItemLabel2 = itemData.rootItemLabel;
				if (rootItemLabel2 == _type)
				{
					m_TypeShopIDList.Add(m_ShopIDList[j]);
				}
			}
		}
	}

	private void SetNpcName(string strName)
	{
		mLbNpcName.text = strName;
	}

	private void SetNpcMoney(int npcMoney)
	{
		mLbNpcMoney.text = npcMoney.ToString();
	}

	private void SetNpcICO(string _sprName)
	{
		if (!(mSpNpcIco == null))
		{
			mSpNpcIco.spriteName = _sprName;
			mSpNpcIco.gameObject.SetActive(value: true);
			mTxNpcIco.gameObject.SetActive(value: false);
		}
	}

	private void SetNpcICO(Texture _contentTexture)
	{
		if (!(mSpNpcIco == null))
		{
			mTxNpcIco.mainTexture = _contentTexture;
			mTxNpcIco.gameObject.SetActive(value: true);
			mSpNpcIco.gameObject.SetActive(value: false);
		}
	}

	private void BtnAllOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.all;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnWeaponOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.weapon;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnEquipOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.equipment;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnToolOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.tool;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnTurretOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.turret;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnConsumOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.consumables;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnResoureOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.resoure;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnPartOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.part;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnDecorationOnClick()
	{
		if (!mSellOpLayer.gameObject.activeSelf)
		{
			shopSelectItemType = ItemLabel.Root.decoration;
			mCurrentPickTab = 0;
			ResetItem(mCurrentPickTab, 0);
		}
	}

	private void BtnRebuyOnClick()
	{
		OnSellBtn();
	}
}
