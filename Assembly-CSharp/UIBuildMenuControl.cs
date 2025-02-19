using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UIBuildMenuControl : UIStaticWnd
{
	public delegate void OnClickFun();

	public delegate bool EventCanClick();

	public delegate void EventFuncItem(int index);

	public delegate void EventFuncCkBtn(bool isChecked);

	public delegate void ToolToipFunc(bool isShow, UIBuildWndItem.ItemType _ItemType, int _Index);

	public delegate void EventBuildItemFunc(UIBuildWndItem item);

	public delegate void BrushItemClickEvent(UIBrushMenuItem.BrushType type);

	public delegate void EventFunc();

	private const int c_MaxMenuItems = 10;

	private Camera uiCamera;

	public UIGridBoxBars mBoxBars;

	public GameObject mWndMenuItem;

	public GameObject mDragItemContent;

	public GameObject mMenuItemRoot;

	public UIBuildSaveWndCtrl mSaveWndCtrl;

	public UIBuildWndContol mBulidWnd;

	public UIImageButton mBtnSave;

	public GameObject mPointBrushTypeList;

	public GameObject mDiagonalTypeList;

	public GameObject mBoxTypeList;

	public GameObject mSelectBrushTypeList;

	public UIMenuBrushBtn mPointBtn;

	public UIMenuBrushBtn mBoxBtn;

	public UIMenuBrushBtn mDiagonalBtn;

	public UIMenuBrushBtn mSelectBtn;

	public UIMenuBrushBtn mSelectBlockBtn;

	public UIMenuBrushBtn mSaveBtn;

	public UIMenuBrushBtn mDeleteBtn;

	public UIBuildSelectStateItem mTypeSelectedItem;

	public UIBuildSelectStateItem mShapeSelectedItem;

	public UICheckbox mBlockSelectCB;

	public UICheckbox mVoxelSelectCB;

	public List<UIBuildWndItem> mMenuList = new List<UIBuildWndItem>();

	public List<UIBuildMenuItemData> m_InitQuickBarData;

	private bool m_Start;

	public GameObject DragObejct;

	public UIBuildWndItem DragItem;

	private bool _firstClickMenuBtn = true;

	public event OnClickFun BtnSave;

	public event OnClickFun BtnDle;

	public event OnClickFun BtnB;

	public event OnClickFun BtnMenu;

	public event OnClickFun BtnSelectType;

	public event OnClickFun BtnSelectShape;

	public event OnClickFun BtnBlockSelect;

	public event OnClickFun BtnVoxelSelect;

	public event EventCanClick onCanClickSaveBtn;

	public event EventFuncItem MenuItemClick;

	public event EventFuncItem MenuItemGetDrag;

	public event ToolToipFunc ToolTip;

	public event EventBuildItemFunc onQuickBarFunc;

	public event EventBuildItemFunc onDropItem;

	public event BrushItemClickEvent onBrushItemClick;

	public event EventFunc onIntiMenuList;

	public event Action onMenuBtn;

	public void ResetMenuButtonClickEvent(bool started)
	{
		UICheckbox[] componentsInChildren = mMenuItemRoot.GetComponentsInChildren<UICheckbox>(includeInactive: true);
		if (started)
		{
			UICheckbox[] array = componentsInChildren;
			foreach (UICheckbox uICheckbox in array)
			{
				if (uICheckbox.startsChecked)
				{
					UIButtonMessage component = uICheckbox.gameObject.GetComponent<UIButtonMessage>();
					Invoke(component.functionName, 0f);
					break;
				}
			}
			return;
		}
		UICheckbox[] array2 = componentsInChildren;
		foreach (UICheckbox uICheckbox2 in array2)
		{
			if (uICheckbox2.isChecked)
			{
				UIButtonMessage component2 = uICheckbox2.gameObject.GetComponent<UIButtonMessage>();
				Invoke(component2.functionName, 0f);
				break;
			}
		}
	}

	public void ManualEnbleBtn(UIMenuBrushBtn btn)
	{
		btn.checkBox.isChecked = true;
		UIButtonMessage component = btn.gameObject.GetComponent<UIButtonMessage>();
		if (component != null)
		{
			Invoke(component.functionName, 0f);
		}
	}

	protected override void InitWindow()
	{
		base.InitWindow();
	}

	private void Awake()
	{
		if (mWndMenuItem == null)
		{
			return;
		}
		mBoxBars.Init(mWndMenuItem, 10);
		mBoxBars.Reposition();
		mBoxBars.e_PageIndexChange += OnPageIndexChange;
		for (int i = 0; i < mBoxBars.Items.Count; i++)
		{
			UIBuildWndItem component = mBoxBars.Items[i].GetComponent<UIBuildWndItem>();
			component.InitItem(UIBuildWndItem.ItemType.mMenu, i);
			int num = i + 1;
			if (num == 10)
			{
				num = 0;
			}
			component.SetSpriteIndex(num.ToString());
			component.ClickItem += MenuItemOnClick;
			component.BeginDrag += OnBeginDragMeaterItem;
			component.Drag += OnDragMeaterItem;
			component.Drop += OnDropMeaterItem;
			component.OnGetDrag += OnGetDrag;
			component.ToolTip += OnToolTip;
			mMenuList.Add(component);
		}
		if (!PeSingleton<UIBlockSaver>.Instance.First)
		{
			return;
		}
		foreach (UIBuildMenuItemData initQuickBarDatum in m_InitQuickBarData)
		{
			PeSingleton<UIBlockSaver>.Instance.AddData(initQuickBarDatum);
			PeSingleton<UIBlockSaver>.Instance.First = false;
		}
	}

	private void OnDestory()
	{
		for (int i = 0; i < mBoxBars.Items.Count; i++)
		{
			UnityEngine.Object.Destroy(mBoxBars.Items[i]);
		}
		mBoxBars.Items.Clear();
		mBoxBars.e_PageIndexChange -= OnPageIndexChange;
	}

	private void OnGetDrag(UIBuildWndItem.ItemType mItemType, int mIndex)
	{
		if (mItemType == UIBuildWndItem.ItemType.mMenu && this.MenuItemGetDrag != null)
		{
			mIndex += (mBoxBars.PageIndex - 1) * mBoxBars.ItemCount;
			this.MenuItemGetDrag(mIndex);
		}
	}

	private void Start()
	{
		m_Start = true;
		ResetMenuButtonClickEvent(started: true);
		UpdateMenuItems(mBoxBars.PageIndex);
		if (this.onIntiMenuList != null)
		{
			this.onIntiMenuList();
		}
	}

	private void Update()
	{
		if (DragObejct != null && Input.GetMouseButtonUp(0))
		{
			UnityEngine.Object.Destroy(DragObejct);
			DragObejct = null;
			if (DragItem != null && DragItem.mItemType == UIBuildWndItem.ItemType.mMenu)
			{
				PeSingleton<UIBlockSaver>.Instance.RemoveData((mBoxBars.PageIndex - 1) * 10 + DragItem.mIndex);
				UpdateMenuItems(mBoxBars.PageIndex);
				DragItem = null;
			}
		}
		if (PeInput.Get(PeInput.LogicFunction.QuickBar1))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[0]);
			}
			mMenuList[0].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar2))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[1]);
			}
			mMenuList[1].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar3))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[2]);
			}
			mMenuList[2].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar4))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[3]);
			}
			mMenuList[3].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar5))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[4]);
			}
			mMenuList[4].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar6))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[5]);
			}
			mMenuList[5].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar7))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[6]);
			}
			mMenuList[6].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar8))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[7]);
			}
			mMenuList[7].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar9))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[8]);
			}
			mMenuList[8].PlayGridEffect();
		}
		else if (PeInput.Get(PeInput.LogicFunction.QuickBar10))
		{
			if (this.onQuickBarFunc != null)
			{
				this.onQuickBarFunc(mMenuList[9]);
			}
			mMenuList[9].PlayGridEffect();
		}
		if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut1))
		{
			if (!mPointBtn.checkBox.isChecked)
			{
				mPointBtn.checkBox.isChecked = true;
				BtnBrush1_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut2))
		{
			if (!mBoxBtn.checkBox.isChecked)
			{
				mBoxBtn.checkBox.isChecked = true;
				BtnBrush2_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut3))
		{
			if (!mDiagonalBtn.checkBox.isChecked)
			{
				UISkillWndCtrl mSkillWndCtrl = GameUI.Instance.mSkillWndCtrl;
				if (mSkillWndCtrl._SkillMgr != null && !mSkillWndCtrl._SkillMgr.CheckUnlockBuildBlockBevel())
				{
					return;
				}
				mDiagonalBtn.checkBox.isChecked = true;
				BtnBrush3_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut4))
		{
			if (!mSelectBtn.checkBox.isChecked)
			{
				mSelectBtn.checkBox.isChecked = true;
				BtnBrush4_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut5))
		{
			if (!mSelectBlockBtn.checkBox.isChecked)
			{
				mSelectBlockBtn.checkBox.isChecked = true;
				BtnBrush5_OnClick();
			}
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut6))
		{
			BtnSaveOnClick();
		}
		else if (PeInput.Get(PeInput.LogicFunction.Build_Shortcut7))
		{
			BtnMenuOnClick();
		}
		if (PeInput.Get(PeInput.LogicFunction.PrevQuickBar))
		{
			mBoxBars.BtnPageUpOnClick();
		}
		if (PeInput.Get(PeInput.LogicFunction.NextQuickBar))
		{
			mBoxBars.BtnPageDnOnClick();
		}
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer != null))
		{
			return;
		}
		PackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
		foreach (UIBuildWndItem mMenu in mMenuList)
		{
			if (mMenu.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
			{
				mMenu.SetNumber(cmpt.GetItemCount(mMenu.ItemId).ToString());
			}
			else if (mMenu.mTargetItemType == UIBuildWndItem.ItemType.mVoxelType)
			{
				int voxelItemProtoID = PEBuildingMan.GetVoxelItemProtoID((byte)mMenu.ItemId);
				mMenu.SetNumber(cmpt.GetItemCount(voxelItemProtoID).ToString());
			}
			else if (mMenu.mTargetItemType == UIBuildWndItem.ItemType.mBlockPattern)
			{
				mMenu.SetNumber(mBulidWnd.mBlockPatternList[mMenu.mTargetIndex].mNumber.text);
			}
			else
			{
				mMenu.SetNumber(string.Empty);
			}
		}
	}

	private void LateUpdate()
	{
		if (Input.GetMouseButtonDown(0) && !UIBrushMenuItem.MouseOnHover)
		{
			mPointBrushTypeList.gameObject.SetActive(value: false);
			mDiagonalTypeList.gameObject.SetActive(value: false);
			mBoxTypeList.gameObject.SetActive(value: false);
			mSelectBrushTypeList.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (m_Start)
		{
			ResetMenuButtonClickEvent(started: false);
			UpdateMenuItems(mBoxBars.PageIndex);
		}
	}

	private void OnDisable()
	{
	}

	private void OnBeginDragMeaterItem(UIBuildWndItem.ItemType mItemType, int mIndex)
	{
		GameObject gameObject = null;
		if (mItemType == UIBuildWndItem.ItemType.mMenu)
		{
			if (mIndex >= mMenuList.Count && mIndex < 0)
			{
				return;
			}
			gameObject = ((!mMenuList[mIndex].mContentSprite.gameObject.activeSelf) ? mMenuList[mIndex].mContentTexture.gameObject : mMenuList[mIndex].mContentSprite.gameObject);
			DragItem = mMenuList[mIndex];
		}
		if (gameObject != null)
		{
			DragObejct = UnityEngine.Object.Instantiate(gameObject);
			DragObejct.transform.parent = mDragItemContent.transform;
			DragObejct.transform.localScale = new Vector3(48f, 48f, 1f);
			DragObejct.transform.position = Input.mousePosition;
		}
	}

	private void OnDragMeaterItem(UIBuildWndItem.ItemType mItemType, int mIndex)
	{
		if (DragObejct != null)
		{
			Vector3 mousePosition = Input.mousePosition;
			DragObejct.transform.localPosition = new Vector3(mousePosition.x, mousePosition.y, -15f);
		}
	}

	private void OnDropMeaterItem(UIBuildWndItem.ItemType mItemType, int mIndex)
	{
		if (DragObejct == null)
		{
			return;
		}
		UIBuildWndItem uIBuildWndItem = QueryGetDragItem();
		if (uIBuildWndItem != null)
		{
			UISprite component = DragObejct.GetComponent<UISprite>();
			if (component != null)
			{
				uIBuildWndItem.GetDrag(DragItem.mTargetItemType, DragItem.mIndex, component.spriteName, DragItem.atlas);
			}
			else
			{
				uIBuildWndItem.GetDrag(DragItem.mTargetItemType, DragItem.mTargetIndex, DragItem.mContentTexture.mainTexture);
			}
			if (DragItem.mItemType == UIBuildWndItem.ItemType.mMenu)
			{
				uIBuildWndItem.mTargetItemType = DragItem.mTargetItemType;
				uIBuildWndItem.SetItemID(DragItem.ItemId);
				uIBuildWndItem.mTargetIndex = DragItem.mTargetIndex;
			}
			else
			{
				uIBuildWndItem.mTargetItemType = DragItem.mItemType;
				uIBuildWndItem.SetItemID(DragItem.ItemId);
				uIBuildWndItem.mTargetIndex = DragItem.mIndex;
			}
			PeSingleton<UIBlockSaver>.Instance.SetData((mBoxBars.PageIndex - 1) * 10 + uIBuildWndItem.mIndex, uIBuildWndItem);
		}
		if (DragItem.mItemType == UIBuildWndItem.ItemType.mMenu && uIBuildWndItem != DragItem)
		{
			DragItem.SetNullContent();
			PeSingleton<UIBlockSaver>.Instance.RemoveData((mBoxBars.PageIndex - 1) * 10 + DragItem.mIndex);
		}
		if (this.onDropItem != null)
		{
			this.onDropItem(uIBuildWndItem);
		}
	}

	private UIBuildWndItem QueryGetDragItem()
	{
		if (uiCamera == null)
		{
			uiCamera = GameUI.Instance.mUICamera;
		}
		Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
		int num = -1;
		for (int i = 0; i < mMenuList.Count; i++)
		{
			BoxCollider component = mMenuList[i].gameObject.GetComponent<BoxCollider>();
			if (component.Raycast(ray, out var _, 1000f))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return null;
		}
		return mMenuList[num];
	}

	private void OnPageIndexChange(int page_index)
	{
		if (page_index >= 0)
		{
			UpdateMenuItems(page_index);
		}
	}

	private void UpdateMenuItems(int page_index)
	{
		List<UIBuildMenuItemData> pageItemDatas = PeSingleton<UIBlockSaver>.Instance.GetPageItemDatas(page_index - 1, 10);
		UIBuildWndItem item1;
		foreach (UIBuildWndItem mMenu in mMenuList)
		{
			item1 = mMenu;
			int num = pageItemDatas.FindIndex((UIBuildMenuItemData item0) => item1.mIndex == item0.m_Index);
			if (num == -1)
			{
				item1.SetNullContent();
				continue;
			}
			item1.mTargetItemType = (UIBuildWndItem.ItemType)pageItemDatas[num].m_Type;
			item1.mTargetIndex = pageItemDatas[num].m_TargetIndex;
			item1.InitItem(UIBuildWndItem.ItemType.mMenu, pageItemDatas[num].m_IconName, "Icon", pageItemDatas[num].m_Index);
			item1.SetItemID(pageItemDatas[num].m_ItemId);
			item1.mSubsetIndex = pageItemDatas[num].m_SubsetIndex;
		}
	}

	private void BtnMenuOnClick()
	{
		if (this.BtnMenu != null)
		{
			this.BtnMenu();
		}
		mBulidWnd.Show();
		mSaveWndCtrl.Hide();
		if (_firstClickMenuBtn)
		{
			mBulidWnd.ChangeCkMenu(0);
			_firstClickMenuBtn = false;
		}
		if (this.onMenuBtn != null)
		{
			this.onMenuBtn();
		}
	}

	private void MenuItemOnClick(UIBuildWndItem.ItemType _itemType, int index)
	{
		if (_itemType == UIBuildWndItem.ItemType.mMenu && this.MenuItemClick != null)
		{
			this.MenuItemClick(index);
		}
	}

	private void BtnBrush1_OnClick()
	{
		if (!(UIBuildBlock.Instance == null))
		{
			UIBuildBlock.Instance.CreateBrush(UIBuildBlock.BrushType.bt_point);
			mSaveWndCtrl.Hide();
		}
	}

	private void BtnBrush2_OnClick()
	{
		UIBuildBlock.Instance.CreateBrush(UIBuildBlock.BrushType.bt_box);
		mSaveWndCtrl.Hide();
	}

	private void BtnBrush3_OnClick()
	{
		UISkillWndCtrl mSkillWndCtrl = GameUI.Instance.mSkillWndCtrl;
		if (mSkillWndCtrl._SkillMgr != null && !mSkillWndCtrl._SkillMgr.CheckUnlockBuildBlockBevel())
		{
			mDiagonalBtn.checkBox.isChecked = false;
			return;
		}
		mDiagonalBtn.checkBox.isChecked = true;
		UIBuildBlock.Instance.CreateBrush(UIBuildBlock.BrushType.bt_inclined);
		mSaveWndCtrl.Hide();
	}

	private void BtnBrush4_OnClick()
	{
		UIBuildBlock.Instance.CreateBrush(UIBuildBlock.BrushType.bt_selectAll);
		mSaveWndCtrl.Hide();
	}

	private void BtnBrush5_OnClick()
	{
		UIBuildBlock.Instance.CreateBrush(UIBuildBlock.BrushType.bt_selectBlock);
		mSaveWndCtrl.Hide();
	}

	private void BtnBrush6_OnClick()
	{
		UIBuildBlock.Instance.CreateBrush(UIBuildBlock.BrushType.bt_selectInclined);
		mSaveWndCtrl.Hide();
	}

	private void BtnSaveOnClick()
	{
		if (this.onCanClickSaveBtn != null)
		{
			if (this.onCanClickSaveBtn())
			{
				mSaveWndCtrl.Show();
				mBulidWnd.Hide();
			}
		}
		else
		{
			mSaveWndCtrl.Show();
			mBulidWnd.Hide();
		}
		if (this.BtnSave != null)
		{
			this.BtnSave();
		}
	}

	private void BtnDelOnClick()
	{
		if (this.BtnDle != null)
		{
			this.BtnDle();
		}
	}

	private void BtnBOnClick()
	{
		if (this.BtnB != null)
		{
			this.BtnB();
		}
	}

	private void OnSelectTypeItemClick()
	{
		if (this.BtnSelectType != null)
		{
			this.BtnSelectType();
		}
	}

	private void OnSelectShapeItemClick()
	{
		if (this.BtnSelectShape != null)
		{
			this.BtnSelectShape();
		}
	}

	public void SetBrushItemSprite(UIBrushMenuItem.BrushType type, Color checkColor)
	{
		switch (type)
		{
		case UIBrushMenuItem.BrushType.pointAdd:
			mPointBtn.bgSprite.spriteName = "build_point";
			mPointBtn.checkedSprite.spriteName = "build_point";
			mPointBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.pointRemove:
			mPointBtn.bgSprite.spriteName = "build_point_down";
			mPointBtn.checkedSprite.spriteName = "build_point_down";
			mPointBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.boxAdd:
			mBoxBtn.bgSprite.spriteName = "build_area";
			mBoxBtn.checkedSprite.spriteName = "build_area";
			mBoxBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.boxRemove:
			mBoxBtn.bgSprite.spriteName = "build_area_down";
			mBoxBtn.checkedSprite.spriteName = "build_area_down";
			mBoxBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.diagonalXPos:
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie1";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie1";
			mDiagonalBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.diagonalXNeg:
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie";
			mDiagonalBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.diagonalZPos:
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie2";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie2";
			mDiagonalBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.diagonalZNeg:
			mDiagonalBtn.bgSprite.spriteName = "build_gjxie3";
			mDiagonalBtn.checkedSprite.spriteName = "build_gjxie3";
			mDiagonalBtn.checkedSprite.color = checkColor;
			break;
		case UIBrushMenuItem.BrushType.SelectAll:
			mSelectBtn.bgSprite.spriteName = "build_vx_all";
			mSelectBtn.checkedSprite.spriteName = "build_vx_all";
			mSelectBtn.checkBox.isChecked = true;
			mSelectBtn.checkedSprite.color = checkColor;
			BtnBrush4_OnClick();
			break;
		case UIBrushMenuItem.BrushType.SelectDetail:
			mSelectBtn.bgSprite.spriteName = "build_vx_point";
			mSelectBtn.checkedSprite.spriteName = "build_vx_point";
			mSelectBtn.checkBox.isChecked = true;
			mSelectBtn.checkedSprite.color = checkColor;
			BtnBrush4_OnClick();
			break;
		}
	}

	public void OnBrushItemClick(UIBrushMenuItem.BrushType type)
	{
		if (this.onBrushItemClick != null)
		{
			this.onBrushItemClick(type);
		}
	}

	private void OnToolTip(bool isShow, UIBuildWndItem.ItemType _ItemType, int _Index)
	{
		if (this.ToolTip != null)
		{
			this.ToolTip(isShow, _ItemType, _Index);
		}
	}

	private void OnVoxelSelect()
	{
		if (this.BtnVoxelSelect != null)
		{
			this.BtnVoxelSelect();
		}
	}

	private void OnBlockSelect()
	{
		if (this.BtnBlockSelect != null)
		{
			this.BtnBlockSelect();
		}
	}
}
