using System;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class UIItemPackageCtrl : UIBaseWnd
{
	public delegate void OnResetItem(int packTab, int pageIndex);

	public delegate void OnOpenPackage();

	public Grid_N mGridPrefab;

	public GameObject ItemsContent;

	public UICheckbox mItemCheckbox;

	public UICheckbox mEquipCheckbox;

	public UICheckbox mResCheckbox;

	public UICheckbox mArmorCheckbox;

	public UICheckbox mMissionCheckbox;

	public Transform mSplitOpWnd;

	public UIInput mSplitNumlabel;

	public UILabel mPageCountText;

	public GameObject mDropItemBtn;

	public GameObject DropItemWndPrefab;

	public UIAtlas mNewUIAtlas;

	public Action<GridMask> SelectPageEvent;

	public UILabel mMoneyCurrent;

	public GameObject nMoneyRoot;

	[HideInInspector]
	public UIDropItemWnd mDropItemWnd;

	private ItemPackage mItemPackage;

	private Grid_N m_CurOpItem;

	private int mRow = 7;

	private int mColumn = 6;

	private int mPageCount;

	private int mPageIndex;

	private int mCurrentPickTab;

	private int mOpType;

	private float mSplitNumDur = 1f;

	private float mOpDurNum;

	private bool mAddBtnPress;

	private bool mSubBtnPress;

	private float mOpStarTime;

	private int mOpBagID = -1;

	private List<Grid_N> mItems;

	private SlotList m_CurrentPack;

	public bool isMission;

	[SerializeField]
	private ItemPackageGridTutorial_N m_GridTutorialPrefab;

	private List<int> m_NeedTutorialItemID = new List<int>();

	private List<ItemPackageGridTutorial_N> m_CurPageGridTutorials = new List<ItemPackageGridTutorial_N>();

	public int CurrentPickTab => mCurrentPickTab;

	public int CurrentPageIndex => mPageIndex;

	public ItemSample CurOperateItem => (!(null == m_CurOpItem) && m_CurOpItem.Item != null) ? m_CurOpItem.Item : null;

	public ItemPackage ItemPackage => mItemPackage;

	public event OnResetItem e_OnResetItem;

	public event OnOpenPackage e_OnOpenPackage;

	public event Action<Grid_N> onRightMouseCliked;

	public event Action<ItemObject> onItemSelected;

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
		if ((PeInput.Get(PeInput.LogicFunction.OpenItemMenu) || Input.GetMouseButtonDown(1)) && !mSplitOpWnd.gameObject.activeSelf && m_CurOpItem == null)
		{
			mOpType = 0;
			UICursor.Clear();
		}
		if (mOpType != 0)
		{
			if (IsMouseHovered())
			{
				switch (mOpType)
				{
				case 1:
					UICursor.Set(mNewUIAtlas, "icocai");
					break;
				case 2:
					UICursor.Set(mNewUIAtlas, "icodelete");
					break;
				}
			}
			else
			{
				UICursor.Clear();
			}
		}
		if (null != m_CurOpItem && m_CurOpItem.Item != null && mSplitOpWnd.gameObject.activeSelf)
		{
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
				mOpDurNum = Mathf.Clamp(mOpDurNum + mSplitNumDur, 1f, m_CurOpItem.Item.GetCount() - 1) - mSplitNumDur;
				mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
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
				mOpDurNum = Mathf.Clamp(mOpDurNum + mSplitNumDur, 1f, m_CurOpItem.Item.GetCount() - 1) - mSplitNumDur;
				mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
			}
			else
			{
				if (string.Empty == mSplitNumlabel.text)
				{
					mSplitNumDur = 1f;
				}
				else
				{
					mSplitNumDur = Mathf.Clamp(Convert.ToInt32(mSplitNumlabel.text), 1, m_CurOpItem.Item.GetCount() - 1);
				}
				if (!UICamera.inputHasFocus)
				{
					mSplitNumlabel.text = mSplitNumDur.ToString();
				}
			}
		}
		mMoneyCurrent.text = GetcurrentMoney();
	}

	protected override void OnHide()
	{
		SelectItem_N.Instance.SetItem(null);
		RestItemState();
		if (null != mDropItemWnd && mDropItemWnd.isShow)
		{
			mDropItemWnd.Hide();
		}
		DeleteCurPageTutorialEffect();
		base.OnHide();
	}

	public override void OnCreate()
	{
		base.OnCreate();
		InitGrid();
		CreatDropWnd();
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		base.SelfWndType = UIEnum.WndType.ItemPackage;
		mSplitOpWnd.gameObject.SetActive(value: false);
		mCurrentPickTab = 0;
		mPageIndex = 0;
		mDropItemBtn.SetActive(GameConfig.IsMultiMode);
		PlayerPackageCmpt cmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		cmpt.package._playerPak.changeEventor.Subscribe(ResetItem);
		PlayerPackage._missionPak.changeEventor.Subscribe(ResetItem);
		if (!Money.Digital)
		{
			nMoneyRoot.SetActive(value: false);
		}
		else
		{
			nMoneyRoot.SetActive(value: true);
		}
	}

	private void ResetItem(object sender, ItemPackage.EventArg arg)
	{
		if (!isMission)
		{
			ResetItem();
		}
		else
		{
			ResetMissionItem();
		}
	}

	private void CreatDropWnd()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(DropItemWndPrefab);
		gameObject.transform.parent = base.transform.parent;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		mDropItemWnd = gameObject.GetComponent<UIDropItemWnd>();
		mDropItemWnd.Hide();
	}

	private bool IsMouseHovered()
	{
		if (UICamera.hoveredObject == null)
		{
			return false;
		}
		if (UICamera.hoveredObject == base.gameObject || UICamera.hoveredObject.transform.IsChildOf(base.transform))
		{
			return true;
		}
		return false;
	}

	private void ExecSelectPageEvent(GridMask type)
	{
		CloseOpWnd();
		if (SelectPageEvent != null)
		{
			SelectPageEvent(type);
		}
	}

	private void CheckMission(int itemid)
	{
		if (null != GameUI.Instance && null != MissionManager.Instance && null != GameUI.Instance.mMainPlayer && MissionManager.Instance.m_PlayerMission != null)
		{
			MissionManager.Instance.ProcessCollectMissionByID(itemid);
		}
	}

	private void InitGrid()
	{
		mItems = new List<Grid_N>();
		mPageCount = mRow * mColumn;
		for (int i = 0; i < mPageCount; i++)
		{
			mItems.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mItems[i].gameObject.name = "ItemPack" + i;
			mItems[i].transform.parent = ItemsContent.transform;
			mItems[i].transform.localPosition = new Vector3(i % mColumn * 55, -i / mColumn * 52, 0f);
			mItems[i].transform.localRotation = Quaternion.identity;
			mItems[i].transform.localScale = Vector3.one;
			mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
			mItems[i].onRightMouseClicked = OnRightMouseCliked;
			mItems[i].onDropItem = OnDropItem;
			mItems[i].onGridsExchangeItem = OnGridsExchangeItems;
		}
	}

	private void OnGridsExchangeItems(Grid_N grid, ItemObject item)
	{
		grid.SetItem(item);
		m_CurrentPack[grid.ItemIndex] = item;
	}

	private void ResetPage()
	{
		if (!isMission)
		{
			GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
		}
	}

	private void OnItemBtn()
	{
		isMission = false;
		mCurrentPickTab = 0;
		ExecSelectPageEvent(GridMask.GM_Item);
		mPageIndex = 0;
		mOpType = 0;
		ResetItem();
		ResetPage();
		GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
		if (UINpcStorageCtrl.Instance != null)
		{
			UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
		}
	}

	private void OnEquipmentBtn()
	{
		isMission = false;
		mCurrentPickTab = 1;
		ExecSelectPageEvent(GridMask.GM_Equipment);
		mPageIndex = 0;
		mOpType = 0;
		ResetItem();
		ResetPage();
		GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
		if (UINpcStorageCtrl.Instance != null)
		{
			UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
		}
	}

	private void OnResourceBtn()
	{
		isMission = false;
		mCurrentPickTab = 2;
		ExecSelectPageEvent(GridMask.GM_Resource);
		mPageIndex = 0;
		mOpType = 0;
		ResetItem();
		ResetPage();
		GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
		if (UINpcStorageCtrl.Instance != null)
		{
			UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
		}
	}

	private void OnArmorBtn()
	{
		isMission = false;
		mCurrentPickTab = 3;
		ExecSelectPageEvent(GridMask.GM_Armor);
		mPageIndex = 0;
		mOpType = 0;
		ResetItem();
		ResetPage();
		GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
		if (UINpcStorageCtrl.Instance != null)
		{
			UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
		}
	}

	private void OnMissionBtn()
	{
		isMission = true;
		mPageIndex = 0;
		mOpType = 0;
		ResetMissionItem();
		ExecSelectPageEvent(GridMask.GM_Mission);
	}

	private void BtnLeftOnClick()
	{
		if (mPageIndex > 0)
		{
			mPageIndex--;
			if (!isMission)
			{
				ResetItem(mCurrentPickTab, mPageIndex);
			}
			else
			{
				ResetMissionItem(mPageIndex);
			}
		}
	}

	private void BtnRightOnClick()
	{
		if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
		{
			mPageIndex++;
			if (!isMission)
			{
				ResetItem(mCurrentPickTab, mPageIndex);
			}
			else
			{
				ResetMissionItem(mPageIndex);
			}
		}
	}

	private void BtnLeftEndOnClick()
	{
		if (mPageIndex > 0)
		{
			mPageIndex = 0;
			if (!isMission)
			{
				ResetItem(mCurrentPickTab, mPageIndex);
			}
			else
			{
				ResetMissionItem(mPageIndex);
			}
		}
	}

	private void BtnRightEndOnClick()
	{
		if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
		{
			mPageIndex = (m_CurrentPack.Count - 1) / mPageCount;
			if (!isMission)
			{
				ResetItem(mCurrentPickTab, mPageIndex);
			}
			else
			{
				ResetMissionItem(mPageIndex);
			}
		}
	}

	private void OnResort()
	{
		if (mSplitOpWnd.gameObject.activeSelf)
		{
			return;
		}
		if (null != mDropItemWnd && mDropItemWnd.isShow)
		{
			mDropItemWnd.CancelDropItems();
		}
		if (!GameConfig.IsMultiMode)
		{
			if (!isMission)
			{
				mItemPackage.Sort((ItemPackage.ESlotType)mCurrentPickTab);
				ResetItem();
			}
			else
			{
				mItemPackage.Sort(ItemPackage.ESlotType.Item);
				ResetMissionItem();
			}
		}
		else if (!isMission)
		{
			PlayerNetwork.mainPlayer.RequestSortPackage(mCurrentPickTab);
		}
		else
		{
			PlayerNetwork.mainPlayer.RequestSortPackage(-1);
		}
	}

	private void OnSplitBtn()
	{
		if (!mSplitOpWnd.gameObject.activeSelf)
		{
			if (mOpType == 1)
			{
				mOpType = 0;
				UICursor.Clear();
			}
			else
			{
				mOpType = 1;
				UICursor.Set(mNewUIAtlas, "icocai");
			}
			ResetCurOpItem();
		}
	}

	private void OnDeleteBtn()
	{
		if (!mSplitOpWnd.gameObject.activeSelf)
		{
			if (mOpType == 2)
			{
				mOpType = 0;
				UICursor.Clear();
			}
			else
			{
				mOpType = 2;
				UICursor.Set(mNewUIAtlas, "icodelete");
			}
			ResetCurOpItem();
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
		mSplitNumDur += mOpDurNum;
		mOpDurNum = 0f;
		mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
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
		mSplitNumDur += mOpDurNum;
		mOpDurNum = 0f;
		mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
	}

	private void OnSplitOkBtn()
	{
		if (m_CurOpItem == null || m_CurOpItem.ItemObj == null)
		{
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			if (!isMission)
			{
				ItemPackage package = mItemPackage;
				package.Split(m_CurOpItem.ItemObj.instanceId, (int)mSplitNumDur);
				ResetItem();
				ResetCurOpItem();
				mOpBagID = -1;
			}
			else
			{
				ItemPackage package2 = mItemPackage;
				package2.Split(m_CurOpItem.ItemObj.instanceId, (int)mSplitNumDur);
				ResetMissionItem();
				ResetCurOpItem();
				mOpBagID = -1;
			}
		}
		else
		{
			PlayerNetwork.mainPlayer.RequestSplitItem(m_CurOpItem.ItemObj.instanceId, (int)mSplitNumDur);
			ResetCurOpItem();
			mOpBagID = -1;
			mSplitNumDur = 0f;
		}
		mSplitOpWnd.gameObject.SetActive(value: false);
	}

	private void OnSplitNoBtn()
	{
		ResetCurOpItem();
		mOpBagID = -1;
		mSplitOpWnd.gameObject.SetActive(value: false);
	}

	private void OnOpenDropWnd()
	{
		if (!(mDropItemWnd == null))
		{
			if (mDropItemWnd.isShow)
			{
				mDropItemWnd.Hide();
			}
			else
			{
				mDropItemWnd.Show();
			}
		}
	}

	private string GetcurrentMoney()
	{
		if (PeSingleton<PeCreature>.Instance == null)
		{
			return string.Empty;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return string.Empty;
		}
		PackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
		if (cmpt != null)
		{
			Money money = cmpt.money;
			if (money != null)
			{
				return money.current.ToString();
			}
		}
		return string.Empty;
	}

	private void CloseOpWnd()
	{
		mSplitOpWnd.gameObject.SetActive(value: false);
		if (GameUI.Instance.mShopWnd.isShow)
		{
			GameUI.Instance.mShopWnd.CloseSellWnd();
		}
		ResetCurOpItem();
	}

	public void SetItempackage(ItemPackage itempackage)
	{
		mItemPackage = itempackage;
	}

	public void ResetItem()
	{
		if ((bool)base.gameObject && base.gameObject.activeSelf)
		{
			if (isMission)
			{
				ResetMissionItem();
			}
			else
			{
				ResetItem(mCurrentPickTab, mPageIndex);
			}
		}
	}

	public void ResetMissionItem()
	{
		if (base.gameObject.activeSelf)
		{
			ResetMissionItem(mPageIndex);
		}
	}

	public override void Show()
	{
		if (!isMission)
		{
			ResetItem(mCurrentPickTab, mPageIndex);
		}
		else
		{
			ResetMissionItem(mPageIndex);
		}
		base.Show();
		if (this.e_OnOpenPackage != null)
		{
			this.e_OnOpenPackage();
		}
	}

	public void ResetItem(int type, int pageIndex)
	{
		isMission = false;
		PlayerPackageCmpt playerPackageCmpt = ((PeSingleton<PeCreature>.Instance != null && !(null == PeSingleton<PeCreature>.Instance.mainPlayer)) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>() : null);
		if (playerPackageCmpt == null)
		{
			return;
		}
		mItemPackage = playerPackageCmpt.package._playerPak;
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
		if (m_CurrentPack == null)
		{
			return;
		}
		if ((m_CurrentPack.Count - 1) / mPageCount < pageIndex)
		{
			pageIndex = (m_CurrentPack.Count - 1) / mPageCount;
		}
		mPageIndex = pageIndex;
		int num = (((m_CurrentPack.Count - 1) / mPageCount != mPageIndex) ? mPageCount : (m_CurrentPack.Count - pageIndex * mPageCount));
		DeleteCurPageTutorialEffect();
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (!(mItems[i] == null))
			{
				num2 = i + pageIndex * mPageCount;
				ItemObject itemObject = m_CurrentPack[num2];
				mItems[i].SetItem(itemObject, m_CurrentPack.newFlagMgr.IsNew(num2));
				mItems[i].SetItemPlace(ItemPlaceType.IPT_Bag, num2);
				if (itemObject != null)
				{
					CheckAddTutorialEffect(itemObject.protoId, mItems[i].transform);
				}
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
		}
		mPageCountText.text = (mPageIndex + 1).ToString() + "/" + ((m_CurrentPack.Count - 1) / mPageCount + 1);
		if (this.e_OnResetItem != null)
		{
			this.e_OnResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	public void ResetMissionItem(int pageIndex)
	{
		mItemPackage = PlayerPackage._missionPak;
		mMissionCheckbox.isChecked = true;
		m_CurrentPack = mItemPackage.GetSlotList();
		if ((m_CurrentPack.Count - 1) / mPageCount < pageIndex)
		{
			pageIndex = (m_CurrentPack.Count - 1) / mPageCount;
		}
		mPageIndex = pageIndex;
		int num = mPageCount;
		for (int i = 0; i < num; i++)
		{
			mItems[i].SetItem(m_CurrentPack[i + pageIndex * mPageCount], m_CurrentPack.newFlagMgr.IsNew(i + pageIndex * mPageCount));
			mItems[i].SetItemPlace(ItemPlaceType.IPT_Bag, i + pageIndex * mPageCount);
			mItems[i].SetGridMask(GridMask.GM_Mission);
		}
		mPageCountText.text = (mPageIndex + 1).ToString() + "/" + ((m_CurrentPack.Count - 1) / mPageCount + 1);
		if (this.e_OnResetItem != null)
		{
			this.e_OnResetItem(mCurrentPickTab, mPageIndex);
		}
	}

	public void SetItemWithIndex(ItemObject itemGrid, int Index)
	{
		if (m_CurrentPack != null)
		{
			m_CurrentPack[Index] = itemGrid;
		}
		if (!isMission)
		{
			ResetItem();
		}
		else
		{
			ResetMissionItem();
		}
	}

	public bool RemoveItemPackgeByIndex(int Index)
	{
		if (m_CurrentPack != null)
		{
			m_CurrentPack[Index] = null;
			return true;
		}
		return false;
	}

	public void ExchangeItem(ItemObject newItem)
	{
		mItemPackage.AddItem(newItem);
		if (!isMission)
		{
			ResetItem();
		}
		else
		{
			ResetMissionItem();
		}
	}

	public void Sell()
	{
		if (null == m_CurOpItem || m_CurOpItem.ItemObj == null)
		{
			return;
		}
		if (m_CurOpItem.ItemObj.protoData.category == "Quest Item")
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(82209003));
			return;
		}
		int protoId = m_CurOpItem.ItemObj.protoId;
		if (GameUI.Instance.mShopWnd.IsOpen())
		{
			GameUI.Instance.mShopWnd.Sell(m_CurOpItem, m_CurOpItem.Item.GetCount());
		}
		else
		{
			GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.SellAllItemByPackage(m_CurOpItem.ItemObj);
		}
		CheckMission(protoId);
		ResetItem();
		ResetCurOpItem();
	}

	public void DeleteSelectedItem()
	{
		if (null == m_CurOpItem || m_CurOpItem.ItemObj == null)
		{
			return;
		}
		if (m_CurOpItem.ItemObj.protoData.category == "Quest Item")
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(82209003));
			return;
		}
		int protoId = m_CurOpItem.ItemObj.protoId;
		if (!GameConfig.IsMultiMode)
		{
			PeSingleton<ItemMgr>.Instance.DestroyItem(m_CurOpItem.ItemObj.instanceId);
			CheckMission(m_CurOpItem.ItemObj.protoId);
			m_CurrentPack[m_CurOpItem.ItemIndex] = null;
			ResetCurOpItem();
			mOpBagID = -1;
			ResetItem();
		}
		else
		{
			if (!isMission)
			{
				PlayerNetwork.mainPlayer.RequestDeleteItem(m_CurOpItem.ItemObj.instanceId, mCurrentPickTab, mOpBagID);
			}
			mItemPackage.RemoveItem(m_CurOpItem.ItemObj);
			ResetCurOpItem();
			mOpBagID = -1;
		}
		CheckMission(protoId);
	}

	public void ResetCurOpItem()
	{
		m_CurOpItem = null;
	}

	public void OnLeftMouseCliked(Grid_N grid)
	{
		if (null == GameUI.Instance || GameUI.Instance.bMainPlayerIsDead || null == grid || grid.Item == null || EqualUsingItem(grid.Item, showUsingTip: false))
		{
			return;
		}
		if (this.onItemSelected != null)
		{
			this.onItemSelected(grid.Item as ItemObject);
		}
		ActiveWnd();
		CheckRemoveTutorialItemProtoID(grid.Item.protoId);
		switch (mOpType)
		{
		case 0:
			if (!isMission)
			{
				if (GameUI.Instance.mShopWnd.isShow)
				{
					m_CurOpItem = grid;
					GameUI.Instance.mShopWnd.PreSell(grid);
					return;
				}
				if (null != GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI && GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.IsShowAndCanUse)
				{
					m_CurOpItem = grid;
					GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.SellItemByPakcage(m_CurOpItem.ItemObj);
					return;
				}
			}
			SelectItem_N.Instance.SetItemGrid(grid);
			break;
		case 1:
			if (Input.GetMouseButtonDown(0) && grid.Item.GetCount() > 1)
			{
				int vacancySlotIndex = mItemPackage.GetVacancySlotIndex(grid.Item.protoData.tabIndex);
				if (vacancySlotIndex == -1)
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000053));
				}
				else if (m_CurOpItem == null)
				{
					mSplitOpWnd.gameObject.SetActive(value: true);
					m_CurOpItem = grid;
					mSplitNumDur = 1f;
					mSplitNumlabel.text = "1";
					mOpBagID = grid.ItemIndex;
				}
			}
			break;
		case 2:
			if (Input.GetMouseButtonDown(0))
			{
				m_CurOpItem = grid;
				mOpBagID = grid.ItemIndex;
				if (m_CurOpItem.Item.protoId / 10000000 == 9)
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000054));
				}
				else
				{
					MessageBox_N.ShowYNBox(PELocalization.GetString(8000055), DeleteSelectedItem, ResetCurOpItem);
				}
			}
			break;
		}
		if (!isMission)
		{
			ResetItem();
		}
		else
		{
			ResetMissionItem();
		}
	}

	public void OnRightMouseCliked(Grid_N grid)
	{
		if (PeSingleton<PeCreature>.Instance == null || null == PeSingleton<PeCreature>.Instance.mainPlayer || PeSingleton<PeCreature>.Instance.mainPlayer.IsDeath() || null == GameUI.Instance || grid.Item == null || EqualUsingItem(grid.Item, showUsingTip: false))
		{
			return;
		}
		CheckRemoveTutorialItemProtoID(grid.Item.protoId);
		UseItemCmpt useItemCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
		if (null == useItemCmpt)
		{
			useItemCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.Add<UseItemCmpt>();
		}
		ItemObject itemObj = grid.ItemObj;
		if (GameConfig.IsMultiMode)
		{
			if (mOpType == 0 && mDropItemWnd.isShow && !isMission)
			{
				mDropItemWnd.AddToDropList(CurrentPickTab, mPageIndex, grid);
			}
			else if (mOpType == 0 && (GameUI.Instance.mShopWnd.isShow || GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.IsShowAndCanUse) && !isMission)
			{
				m_CurOpItem = grid;
				int count = m_CurOpItem.Item.GetCount();
				int sellPrice = m_CurOpItem.ItemObj.GetSellPrice();
				string text = m_CurOpItem.Item.protoData.GetName();
				string text2 = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000056), text + " X " + count, PELocalization.GetString(8000253), count * sellPrice);
				MessageBox_N.ShowYNBox(text2, Sell, ResetCurOpItem);
			}
			else if (this.onRightMouseCliked != null && !isMission)
			{
				this.onRightMouseCliked(grid);
			}
			else if (CurrentPickTab == 3)
			{
				useItemCmpt.RightMouseClickArmorItem(itemObj);
			}
			else
			{
				useItemCmpt.Request(grid.ItemObj);
			}
		}
		else if (mOpType == 0 && (GameUI.Instance.mShopWnd.IsOpen() || GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.IsShowAndCanUse) && !isMission)
		{
			m_CurOpItem = grid;
			int count2 = m_CurOpItem.Item.GetCount();
			int sellPrice2 = m_CurOpItem.ItemObj.GetSellPrice();
			string text3 = m_CurOpItem.Item.protoData.GetName();
			string text4 = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000056), text3 + " X " + count2, PELocalization.GetString(8000253), count2 * sellPrice2);
			MessageBox_N.ShowYNBox(text4, Sell, ResetCurOpItem);
		}
		else if (GameUI.Instance.mWarehouse.IsOpen() && !isMission)
		{
			if (GameUI.Instance.mWarehouse.SetItemWithIndex(grid.ItemObj))
			{
				m_CurrentPack[grid.ItemIndex] = null;
				grid.SetItem(null);
			}
		}
		else if (this.onRightMouseCliked != null && !isMission)
		{
			this.onRightMouseCliked(grid);
			m_CurrentPack[grid.ItemIndex] = grid.ItemObj;
		}
		else if (GameUI.Instance.mServantWndCtrl.isShow && GameUI.Instance.mServantWndCtrl.ServantIsNotNull)
		{
			if (!GameUI.Instance.mServantWndCtrl.EquipItem(grid.ItemObj))
			{
				if (GameUI.Instance.mServantWndCtrl.SetItemWithIndex(grid.ItemObj))
				{
					m_CurrentPack[grid.ItemIndex] = null;
					if (!isMission)
					{
						ResetItem();
					}
					else
					{
						ResetMissionItem();
					}
				}
			}
			else
			{
				m_CurrentPack[grid.ItemIndex] = null;
				if (!isMission)
				{
					ResetItem();
				}
				else
				{
					ResetMissionItem();
				}
			}
		}
		else if (GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.IsShow && GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc != null && GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.IsRandomNpc())
		{
			if (!GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.EquipItem(grid.ItemObj))
			{
				if (GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteractionItemWithIndex(grid.ItemObj) || GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteraction2ItemWithIndex(grid.ItemObj))
				{
					m_CurrentPack[grid.ItemIndex] = null;
					if (!isMission)
					{
						ResetItem();
					}
					else
					{
						ResetMissionItem();
					}
				}
			}
			else
			{
				m_CurrentPack[grid.ItemIndex] = null;
				if (!isMission)
				{
					ResetItem();
				}
				else
				{
					ResetMissionItem();
				}
			}
		}
		else if (CurrentPickTab == 3)
		{
			useItemCmpt.RightMouseClickArmorItem(itemObj);
		}
		else
		{
			useItemCmpt.Request(grid.ItemObj);
		}
	}

	public void OnDropItem(Grid_N grid)
	{
		if (SelectItem_N.Instance.ItemObj == null || GameUI.Instance.bMainPlayerIsDead || SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar || (null != mDropItemWnd && mDropItemWnd.isShow && SelectItem_N.Instance.Place != ItemPlaceType.IPT_DropItem))
		{
			return;
		}
		if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Hospital)
		{
			if (grid.ItemObj != null)
			{
				SelectItem_N.Instance.SetItem(null);
				return;
			}
			if (SelectItem_N.Instance.ItemObj.protoData.category == "Quest Item")
			{
				if (isMission)
				{
					CSUI_Hospital.Instance.mMedicineRealOp(mItemPackage, isMission, 0, grid.ItemIndex, SelectItem_N.Instance.ItemObj.instanceId, _inorout: false);
				}
			}
			else if (!isMission)
			{
				CSUI_Hospital.Instance.mMedicineRealOp(mItemPackage, isMission, mCurrentPickTab, grid.ItemIndex, SelectItem_N.Instance.ItemObj.instanceId, _inorout: false);
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (SelectItem_N.Instance.ItemObj != null && SelectItem_N.Instance.Place == ItemPlaceType.IPT_DropItem && mDropItemWnd.isShow)
		{
			mDropItemWnd.RemoveFromDropList(SelectItem_N.Instance.Grid);
		}
		if (GameConfig.IsMultiMode)
		{
			if (SelectItem_N.Instance.Index != grid.ItemIndex && SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)
			{
				PlayerNetwork.mainPlayer.RequestExchangeItem(SelectItem_N.Instance.ItemObj, SelectItem_N.Instance.Index, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Equipment)
			{
				if (SelectItem_N.Instance.RemoveOriginItem())
				{
					PlayerNetwork.mainPlayer.RequestTakeOffEquipment(SelectItem_N.Instance.ItemObj);
				}
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_PublicInventory)
			{
				PlayerNetwork.mainPlayer.RequestPublicStorageFetch(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ServantEqu)
			{
				if (SelectItem_N.Instance.RemoveOriginItem())
				{
					int getCurServantId = GameUI.Instance.mServantWndCtrl.GetCurServantId;
					if (getCurServantId == -1)
					{
						return;
					}
					PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(getCurServantId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
					GameUI.Instance.PlayTakeOffEquipAudio();
				}
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ServantInteraction)
			{
				int getCurServantId2 = GameUI.Instance.mServantWndCtrl.GetCurServantId;
				if (getCurServantId2 == -1)
				{
					return;
				}
				PlayerNetwork.mainPlayer.RequestGetItemFromNpc(0, getCurServantId2, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ServantInteraction2)
			{
				int getCurServantId3 = GameUI.Instance.mServantWndCtrl.GetCurServantId;
				if (getCurServantId3 == -1)
				{
					return;
				}
				PlayerNetwork.mainPlayer.RequestGetItemFromNpc(1, getCurServantId3, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ConolyServantEquPersonel)
			{
				if (SelectItem_N.Instance.RemoveOriginItem())
				{
					int id = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.Id;
					if (id == -1)
					{
						return;
					}
					PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(id, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
					GameUI.Instance.PlayTakeOffEquipAudio();
				}
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ColonyServantInteractionPersonel)
			{
				int id2 = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.Id;
				if (id2 == -1)
				{
					return;
				}
				PlayerNetwork.mainPlayer.RequestGetItemFromNpc(0, id2, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ColonyServantInteraction2Personel)
			{
				int id3 = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.Id;
				if (id3 == -1)
				{
					return;
				}
				PlayerNetwork.mainPlayer.RequestGetItemFromNpc(1, id3, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ConolyServantEquTrain)
			{
				int id4 = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.m_TrainNpcInfCtrl.Npc.m_Npc.Id;
				if (id4 == -1)
				{
					return;
				}
				PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(id4, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
				GameUI.Instance.PlayTakeOffEquipAudio();
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ConolyServantInteractionTrain)
			{
				int id5 = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.m_TrainNpcInfCtrl.Npc.m_Npc.Id;
				if (id5 == -1)
				{
					return;
				}
				PlayerNetwork.mainPlayer.RequestGetItemFromNpc(0, id5, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_CSStorage)
			{
				PlayerNetwork.mainPlayer.RequestPersonalStorageFetch(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
			}
			else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Rail)
			{
				PERailwayCtrl.Instance.RemoveTrain(grid.ItemIndex);
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (grid.ItemObj == null)
		{
			switch (SelectItem_N.Instance.Place)
			{
			case ItemPlaceType.IPT_HotKeyBar:
				SelectItem_N.Instance.SetItem(null);
				return;
			case ItemPlaceType.IPT_NPCStorage:
				if (!isMission)
				{
					grid.SetItem(SelectItem_N.Instance.ItemObj);
					m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
					SelectItem_N.Instance.RemoveOriginItem();
				}
				SelectItem_N.Instance.SetItem(null);
				return;
			}
			if (!isMission)
			{
				if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != mCurrentPickTab)
				{
					return;
				}
			}
			else if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != 0 || SelectItem_N.Instance.GridMask != GridMask.GM_Mission)
			{
				return;
			}
			if (SelectItem_N.Instance.RemoveOriginItem())
			{
				grid.SetItem(SelectItem_N.Instance.ItemObj);
				m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (!isMission)
		{
			if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != mCurrentPickTab)
			{
				return;
			}
		}
		else if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != 0)
		{
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Bag:
		{
			ItemObject itemObj = SelectItem_N.Instance.ItemObj;
			m_CurrentPack[SelectItem_N.Instance.Index] = grid.ItemObj;
			m_CurrentPack[grid.ItemIndex] = itemObj;
			SelectItem_N.Instance.SetItem(null);
			if (!isMission)
			{
				ResetItem();
			}
			else
			{
				ResetMissionItem();
			}
			break;
		}
		case ItemPlaceType.IPT_Equipment:
			SelectItem_N.Instance.SetItem(null);
			break;
		default:
			SelectItem_N.Instance.SetItem(null);
			break;
		}
	}

	public void RestItemState()
	{
		mOpType = 0;
		ResetCurOpItem();
		mSplitOpWnd.gameObject.SetActive(value: false);
		UICursor.Clear();
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

	public void AddTutorialItemProtoID(int protoID)
	{
		if (!PeGameMgr.IsTutorial || m_NeedTutorialItemID.Contains(protoID))
		{
			return;
		}
		m_NeedTutorialItemID.Add(protoID);
		if (!base.isShow || mItems == null || mItems.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < mItems.Count; i++)
		{
			if (!(null == mItems[i]) && mItems[i].Item != null)
			{
				CheckAddTutorialEffect(mItems[i].Item.protoId, mItems[i].transform);
			}
		}
	}

	private void CheckRemoveTutorialItemProtoID(int protoID)
	{
		if (!m_NeedTutorialItemID.Contains(protoID))
		{
			return;
		}
		m_NeedTutorialItemID.Remove(protoID);
		List<ItemPackageGridTutorial_N> list = m_CurPageGridTutorials.FindAll((ItemPackageGridTutorial_N a) => a.ProtoID == protoID);
		if (list != null && list.Count > 0)
		{
			ItemPackageGridTutorial_N itemPackageGridTutorial_N = null;
			for (int i = 0; i < list.Count; i++)
			{
				itemPackageGridTutorial_N = list[i];
				itemPackageGridTutorial_N.gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(itemPackageGridTutorial_N.gameObject);
				m_CurPageGridTutorials.Remove(itemPackageGridTutorial_N);
			}
		}
	}

	private void CheckAddTutorialEffect(int protoID, Transform trans)
	{
		if (PeGameMgr.IsTutorial && m_NeedTutorialItemID.Count > 0 && m_NeedTutorialItemID.Contains(protoID))
		{
			ItemPackageGridTutorial_N component = UnityEngine.Object.Instantiate(m_GridTutorialPrefab.gameObject).GetComponent<ItemPackageGridTutorial_N>();
			component.gameObject.SetActive(value: false);
			component.SetProtoID(protoID);
			component.transform.parent = trans.parent;
			component.transform.localPosition = trans.localPosition;
			component.transform.localScale = Vector3.one;
			component.gameObject.SetActive(value: true);
			m_CurPageGridTutorials.Add(component);
		}
	}

	private void DeleteCurPageTutorialEffect()
	{
		for (int i = 0; i < m_CurPageGridTutorials.Count; i++)
		{
			m_CurPageGridTutorials[i].gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(m_CurPageGridTutorials[i].gameObject);
		}
		m_CurPageGridTutorials.Clear();
	}
}
