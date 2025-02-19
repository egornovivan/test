using System;
using ItemAsset;
using Pathea;
using UnityEngine;

public class SelectItem_N : MonoBehaviour
{
	private static SelectItem_N mInstance;

	public UIAtlas mIconAtlas;

	public UIAtlas mButtonUIAtlas;

	public UITexture mIcontex;

	private ItemPlaceType mItemPlace;

	private ItemSample mItemSample;

	private int mItemIndex;

	private Grid_N mGrid;

	private ItemSkillMar mItemSkillmar;

	private UISprite mItemSpr;

	private UITexture mItemTex;

	private bool mPutBackFlag;

	private bool mDiscardFlag;

	private bool mPutEnable;

	private bool mCreatEnable;

	private bool mHasCreated;

	private GridMask mGridMask = GridMask.GM_Any;

	public static SelectItem_N Instance => mInstance;

	public ItemSkillMar ItemSkillmar => mItemSkillmar;

	public Grid_N Grid => mGrid;

	public ItemPlaceType Place => mItemPlace;

	public ItemObject ItemObj => mItemSample as ItemObject;

	public ItemSample ItemSample => mItemSample;

	public int Index => mItemIndex;

	public GridMask GridMask => mGridMask;

	private void Awake()
	{
		mInstance = this;
		mItemPlace = ItemPlaceType.IPT_Null;
		mItemSample = null;
		mItemIndex = 0;
		mItemSkillmar = new ItemSkillMar();
		ItemMgr instance = PeSingleton<ItemMgr>.Instance;
		instance.DestoryItemEvent = (Action<int>)Delegate.Remove(instance.DestoryItemEvent, new Action<int>(DestoryItemEvent));
		ItemMgr instance2 = PeSingleton<ItemMgr>.Instance;
		instance2.DestoryItemEvent = (Action<int>)Delegate.Combine(instance2.DestoryItemEvent, new Action<int>(DestoryItemEvent));
	}

	private void Update()
	{
		CheckDrawItemState();
		UpdateObjectTransform();
		CheckIfCreat();
		CheckItemPutDownAction();
	}

	private void CheckDrawItemState()
	{
		if (mDiscardFlag)
		{
			mDiscardFlag = false;
			if (mItemSample != null && mItemPlace == ItemPlaceType.IPT_HotKeyBar)
			{
				GameUI.Instance.mUIMainMidCtrl.RemoveItemWithIndex(mItemIndex);
			}
		}
		if (mPutBackFlag)
		{
			mPutBackFlag = false;
			CancelDrop();
		}
		if (PeInput.Get(PeInput.LogicFunction.Item_CancelDrag) || Input.GetMouseButtonUp(0))
		{
			mPutBackFlag = true;
		}
		if (Input.GetMouseButtonUp(0) && !PeInput.Get(PeInput.LogicFunction.Item_CancelDrag))
		{
			mDiscardFlag = true;
		}
	}

	private void UpdateObjectTransform()
	{
		PeSingleton<DraggingMgr>.Instance.UpdateRay();
		mIcontex.transform.localPosition = base.transform.localPosition;
		mPutEnable = !(null != UICamera.hoveredObject);
		mCreatEnable = !(null != UICamera.hoveredObject);
		if (mPutEnable && mCreatEnable && PeInput.Get(PeInput.LogicFunction.Item_RotateItem))
		{
			PeSingleton<DraggingMgr>.Instance.Rotate();
		}
	}

	private void CheckItemPutDownAction()
	{
		if (PeInput.Get(PeInput.LogicFunction.Item_Drop))
		{
			if (null == UICamera.hoveredObject && mPutEnable)
			{
				PutItemDown();
			}
			else
			{
				CancelDrop();
			}
		}
	}

	private void CheckIfCreat()
	{
		if (PeInput.Get(PeInput.LogicFunction.Item_Drag) && null == UICamera.hoveredObject && mCreatEnable)
		{
			CreatTower();
		}
	}

	public void SetItemGrid(Grid_N grid)
	{
		SetItem(grid.Item, grid.ItemPlace, grid.ItemIndex, grid.ItemMask);
		mGrid = grid;
	}

	private void Clear()
	{
		Grid_N.SetActiveGrid(null);
		UICursor.Clear();
		mIcontex.enabled = false;
		mIcontex.mainTexture = null;
		mItemSample = null;
	}

	public void SetItem(ItemSample itemSample, ItemPlaceType place = ItemPlaceType.IPT_Null, int index = 0, GridMask gridMask = GridMask.GM_Any)
	{
		mGridMask = gridMask;
		mItemSample = itemSample;
		mItemPlace = place;
		mItemIndex = index;
		if (mPutBackFlag)
		{
			CancelDrop();
		}
		mPutBackFlag = false;
		mGrid = null;
		if (mItemSample == null)
		{
			Clear();
			return;
		}
		if (null != Grid_N.mActiveGrid)
		{
			Grid_N.mActiveGrid.mSkillCooldown.fillAmount = 0f;
			Grid_N.mActiveGrid = null;
		}
		GameUI.Instance.mItemPackageCtrl.RestItemState();
		SetIcon(mItemSample.iconTex, mItemSample.iconString0);
		mHasCreated = false;
	}

	private void CreatTower()
	{
		if (!mHasCreated && mItemSample != null)
		{
			ItemObject itemObject = mItemSample as ItemObject;
			Drag drag = null;
			if (itemObject != null)
			{
				drag = itemObject.GetCmpt<Drag>();
			}
			if (itemObject != null && drag != null && (mItemPlace == ItemPlaceType.IPT_Bag || mItemPlace == ItemPlaceType.IPT_HotKeyBar) && !GameUI.Instance.bMainPlayerIsDead)
			{
				ItemObjDragging dragable = new ItemObjDragging(drag);
				PeSingleton<DraggingMgr>.Instance.Begin(dragable);
			}
			mHasCreated = true;
		}
	}

	private void SetIcon(Texture iconTex, string iconString)
	{
		if (null != iconTex)
		{
			UICursor.Set(mIconAtlas, "Null");
			mIcontex.enabled = true;
			mIcontex.mainTexture = iconTex;
			mIcontex.transform.localScale = new Vector3(48f, 48f, 1f);
		}
		else
		{
			UICursor.Set(mIconAtlas, iconString);
			mIcontex.enabled = false;
		}
	}

	private void PutItemDown()
	{
		PeSingleton<DraggingMgr>.Instance.End();
	}

	private void CancelDrop()
	{
		PeSingleton<DraggingMgr>.Instance.Cancel();
		Clear();
	}

	public bool HaveOpItem()
	{
		return mItemSample != null;
	}

	public bool RemoveOriginItem()
	{
		bool result = false;
		switch (mItemPlace)
		{
		case ItemPlaceType.IPT_Bag:
			GameUI.Instance.mItemPackageCtrl.SetItemWithIndex(null, mItemIndex);
			result = true;
			break;
		case ItemPlaceType.IPT_Equipment:
			result = GameUI.Instance.mUIPlayerInfoCtrl.RemoveEquipmentByIndex(mItemIndex);
			break;
		case ItemPlaceType.IPT_Warehouse:
			result = GameUI.Instance.mWarehouse.SetItemWithIndex(null, mItemIndex);
			break;
		case ItemPlaceType.IPT_ServantInteraction:
			result = GameUI.Instance.mServantWndCtrl.SetItemWithIndex(null, mItemIndex);
			break;
		case ItemPlaceType.IPT_ServantInteraction2:
			result = GameUI.Instance.mServantWndCtrl.SetItemWithIndexWithPackage2(null, mItemIndex);
			break;
		case ItemPlaceType.IPT_ServantEqu:
			result = GameUI.Instance.mServantWndCtrl.RemoveEqByIndex(mItemIndex);
			break;
		case ItemPlaceType.IPT_ConolyServantInteractionTrain:
			result = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetItemWithIndex(null, mItemIndex);
			break;
		case ItemPlaceType.IPT_ConolyServantEquTrain:
			result = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetItemWithIndex(null, mItemIndex);
			break;
		case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
			result = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteractionItemWithIndex(null, mItemIndex);
			break;
		case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
			result = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteraction2ItemWithIndex(null, mItemIndex);
			break;
		case ItemPlaceType.IPT_ConolyServantEquPersonel:
			result = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.EquipRemoveOriginItem(mItemIndex);
			break;
		case ItemPlaceType.IPT_Repair:
			GameUI.Instance.mRepair.RemoveItem();
			result = true;
			break;
		case ItemPlaceType.IPT_Rail:
			PERailwayCtrl.RemoveTrain(ItemObj);
			result = true;
			break;
		}
		if (mGrid != null && mGrid.onRemoveOriginItem != null)
		{
			mGrid.onRemoveOriginItem(mGrid);
			result = true;
		}
		return result;
	}

	public void ExchangeItem(ItemObject io)
	{
		ItemPlaceType itemPlaceType = mItemPlace;
		if (itemPlaceType == ItemPlaceType.IPT_Bag)
		{
			GameUI.Instance.mItemPackageCtrl.ExchangeItem(io);
		}
	}

	private void DestoryItemEvent(int instanceId)
	{
		if (ItemObj != null && ItemObj.instanceId == instanceId)
		{
			SetItem(null);
		}
	}
}
