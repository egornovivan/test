using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UIBuildWndContol : UIBaseWnd
{
	public delegate void OnClickFun();

	public delegate void OnPageClickFunc(int index);

	public delegate void ItemClickFunc(int mIndex);

	public delegate void ToolToipFunc(bool isShow, UIBuildWndItem.ItemType _ItemType, int _Index);

	[HideInInspector]
	public Camera uiCamera;

	public UIBuildMenuControl mMeunControl;

	public GameObject mPageBlack;

	public GameObject mPageTexture;

	public GameObject mPageSave;

	public UICheckbox mCbBlock;

	public UICheckbox mCbTexture;

	public UICheckbox mCbSave;

	public UICheckbox mCbControl2;

	public GameObject mDefoultWndItem;

	public GameObject mIsoWndItem;

	public GameObject mCostItem;

	public GameObject mBlockMatGrid;

	public GameObject mBlockPatternGrid;

	public GameObject mVoxelMatGrid;

	public GameObject mVoxelTypeGrid;

	public GameObject mIsoGrid;

	public GameObject mCostGrid;

	public GameObject mDragItemContent;

	public UILabel mLbTitle;

	public List<UIBuildWndItem> mBlockMatList = new List<UIBuildWndItem>();

	public List<UIBuildWndItem> mBlockPatternList = new List<UIBuildWndItem>();

	public List<UIBuildWndItem> mIsoList = new List<UIBuildWndItem>();

	public List<UIBuildWndItem> mCostList = new List<UIBuildWndItem>();

	public List<UIBuildWndItem> mVoxelMatList = new List<UIBuildWndItem>();

	public List<UIBuildWndItem> mTypeList = new List<UIBuildWndItem>();

	private int mBlockMatListSelectIndex;

	private int mBlockPatternListSelectIndex;

	private int mVoxelMatListSelectIndex = -1;

	private int mIsoListSelectIndex = -1;

	private int mCostSelectIndex = -1;

	private int mTypeSelectIndex = -1;

	public int BlockMatSelectIndex
	{
		get
		{
			return mBlockMatListSelectIndex;
		}
		set
		{
			if (mBlockMatListSelectIndex != -1 && mBlockMatListSelectIndex < mBlockMatList.Count)
			{
				mBlockMatList[mBlockMatListSelectIndex].SetSelect(_IsSelect: false);
			}
			if (value != -1 && value < mBlockMatList.Count)
			{
				mBlockMatList[value].SetSelect(_IsSelect: true);
				mBlockMatListSelectIndex = value;
			}
			else
			{
				mBlockMatListSelectIndex = -1;
			}
		}
	}

	public int BlockPatternSelectIndex
	{
		get
		{
			return mBlockPatternListSelectIndex;
		}
		set
		{
			if (mBlockPatternListSelectIndex != -1 && mBlockPatternListSelectIndex < mBlockPatternList.Count)
			{
				mBlockPatternList[mBlockPatternListSelectIndex].SetSelect(_IsSelect: false);
			}
			if (value != -1 && value < mBlockPatternList.Count)
			{
				mBlockPatternList[value].SetSelect(_IsSelect: true);
				mBlockPatternListSelectIndex = value;
			}
			else
			{
				mBlockPatternListSelectIndex = -1;
			}
		}
	}

	public int TextureListSelectIndex
	{
		get
		{
			return mVoxelMatListSelectIndex;
		}
		set
		{
			if (mVoxelMatListSelectIndex != -1 && mVoxelMatListSelectIndex < mVoxelMatList.Count)
			{
				mVoxelMatList[mVoxelMatListSelectIndex].SetSelect(_IsSelect: false);
			}
			if (value != -1 && value < mVoxelMatList.Count)
			{
				mVoxelMatList[value].SetSelect(_IsSelect: true);
				mVoxelMatListSelectIndex = value;
			}
			else
			{
				mVoxelMatListSelectIndex = -1;
			}
		}
	}

	public int IsoListSelectIndex
	{
		get
		{
			return mIsoListSelectIndex;
		}
		set
		{
			if (mIsoListSelectIndex != -1 && mIsoListSelectIndex < mIsoList.Count)
			{
				mIsoList[mIsoListSelectIndex].SetSelect(_IsSelect: false);
			}
			if (value != -1 && value < mIsoList.Count)
			{
				mIsoListSelectIndex = value;
				mIsoList[value].SetSelect(_IsSelect: true);
			}
			else
			{
				mIsoListSelectIndex = -1;
			}
		}
	}

	public int CostSelectIndex
	{
		get
		{
			return mCostSelectIndex;
		}
		set
		{
			if (mCostSelectIndex != -1 && mCostSelectIndex < mCostList.Count)
			{
				mCostList[mCostSelectIndex].SetSelect(_IsSelect: false);
			}
			if (value != -1 && value < mCostList.Count)
			{
				mCostList[value].SetSelect(_IsSelect: true);
				mCostSelectIndex = value;
			}
			else
			{
				mCostSelectIndex = -1;
			}
		}
	}

	public int TypeSelectIndex
	{
		get
		{
			return mTypeSelectIndex;
		}
		set
		{
			if (mTypeSelectIndex != -1 && mTypeSelectIndex < mTypeList.Count)
			{
				mTypeList[mTypeSelectIndex].SetSelect(_IsSelect: false);
			}
			if (value != -1 && value < mTypeList.Count)
			{
				mTypeList[value].SetSelect(_IsSelect: true);
				mTypeSelectIndex = value;
			}
			else
			{
				mTypeSelectIndex = -1;
			}
		}
	}

	public event OnClickFun BtnClose;

	public event OnClickFun CkControl2;

	public event OnPageClickFunc onPageClick;

	public event ItemClickFunc BlockItemOnClick;

	public event ItemClickFunc BlockPatternOnClick;

	public event ItemClickFunc TextureItemOnClick;

	public event ItemClickFunc IsoItemOnClick;

	public event ItemClickFunc VoxelTypeOnClick;

	public event ItemClickFunc BtnDelete;

	public event ItemClickFunc BtnExport;

	public event ToolToipFunc ToolTip;

	public override void Show()
	{
		base.Show();
		ResetBlockPostion();
		ResetVoxelPostion();
	}

	public bool GetControl2isChecked()
	{
		return mCbControl2.isChecked;
	}

	protected override void InitWindow()
	{
		if (GameUI.Instance != null)
		{
			uiCamera = GameUI.Instance.mUICamera;
		}
		base.InitWindow();
	}

	public int AddBlockListItem(Texture _contentTexture)
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mDefoultWndItem, mBlockMatGrid.transform);
		int count = mBlockMatList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mBlockMat, _contentTexture, count);
		uIBuildWndItem.ClickItem += WndItemOnClick;
		uIBuildWndItem.BeginDrag += OnBeginDragMeaterItem;
		uIBuildWndItem.Drag += OnDragMeaterItem;
		uIBuildWndItem.Drop += OnDropMeaterItem;
		uIBuildWndItem.ToolTip += OnToolTip;
		mBlockMatList.Add(uIBuildWndItem);
		return count;
	}

	public int AddBlockListItem(string _spriteName, string _AtlasName = "Button")
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mDefoultWndItem, mBlockMatGrid.transform);
		int count = mBlockMatList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mBlockMat, _spriteName, _AtlasName, count);
		uIBuildWndItem.ClickItem += WndItemOnClick;
		uIBuildWndItem.BeginDrag += OnBeginDragMeaterItem;
		uIBuildWndItem.Drag += OnDragMeaterItem;
		uIBuildWndItem.Drop += OnDropMeaterItem;
		uIBuildWndItem.ToolTip += OnToolTip;
		mBlockMatList.Add(uIBuildWndItem);
		return count;
	}

	public int AddBlockPatternItem(string _spriteName, string _AtlasName = "Button")
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mDefoultWndItem, mBlockPatternGrid.transform);
		int count = mBlockPatternList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mBlockPattern, _spriteName, _AtlasName, count);
		uIBuildWndItem.ClickItem += WndItemOnClick;
		uIBuildWndItem.BeginDrag += OnBeginDragMeaterItem;
		uIBuildWndItem.Drag += OnDragMeaterItem;
		uIBuildWndItem.Drop += OnDropMeaterItem;
		uIBuildWndItem.ToolTip += OnToolTip;
		mBlockPatternList.Add(uIBuildWndItem);
		return count;
	}

	public int AddVoxelMatListItem(Texture _contentTexture)
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mDefoultWndItem, mVoxelMatGrid.transform);
		int count = mVoxelMatList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mVoxelMat, _contentTexture, count);
		uIBuildWndItem.ClickItem += WndItemOnClick;
		uIBuildWndItem.ToolTip += OnToolTip;
		mVoxelMatList.Add(uIBuildWndItem);
		return count;
	}

	public int AddVoxelMatListItem(string _spriteName, string _AtlasName = "Button")
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mDefoultWndItem, mVoxelMatGrid.transform);
		int count = mVoxelMatList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mVoxelMat, _spriteName, _AtlasName, count);
		uIBuildWndItem.ClickItem += WndItemOnClick;
		uIBuildWndItem.ToolTip += OnToolTip;
		mVoxelMatList.Add(uIBuildWndItem);
		return count;
	}

	public void RefreshTypeLisItem(string[] _spriteName, string _AtlasName = "Button")
	{
		if (_spriteName.Length > mTypeList.Count)
		{
			int num = _spriteName.Length - mTypeList.Count;
			for (int i = 0; i < num; i++)
			{
				UIBuildWndItem item = AddBuildItem(mDefoultWndItem, mVoxelTypeGrid.transform);
				mTypeList.Add(item);
			}
		}
		else if (_spriteName.Length < mTypeList.Count)
		{
			for (int num2 = mTypeList.Count - 1; num2 >= _spriteName.Length; num2--)
			{
				mTypeList[num2].transform.parent = null;
				Object.Destroy(mTypeList[num2].gameObject);
				mTypeList.RemoveAt(num2);
			}
		}
		for (int j = 0; j < mTypeList.Count; j++)
		{
			mTypeList[j].InitItem(UIBuildWndItem.ItemType.mVoxelType, _spriteName[j], _AtlasName, j);
			mTypeList[j].ClickItem += WndItemOnClick;
			mTypeList[j].BeginDrag += OnBeginDragMeaterItem;
			mTypeList[j].Drag += OnDragMeaterItem;
			mTypeList[j].Drop += OnDropMeaterItem;
			mTypeList[j].ToolTip += OnToolTip;
		}
		UIGrid component = mVoxelTypeGrid.GetComponent<UIGrid>();
		component.repositionNow = true;
	}

	public int AddIsoListItem(string _itemName, Texture _contentTexture)
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mIsoWndItem, mIsoGrid.transform);
		int count = mIsoList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mIso, _contentTexture, count);
		uIBuildWndItem.SetText(_itemName);
		uIBuildWndItem.ClickItem += WndItemOnClick;
		uIBuildWndItem.ToolTip += OnToolTip;
		mIsoList.Add(uIBuildWndItem);
		return count;
	}

	public int AddIsoListItem(string _itemName, string _spriteName, string _AtlasName = "Button")
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mIsoWndItem, mIsoGrid.transform);
		int count = mIsoList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mIso, _spriteName, _AtlasName, count);
		uIBuildWndItem.SetText(_itemName);
		uIBuildWndItem.ClickItem += WndItemOnClick;
		uIBuildWndItem.ToolTip += OnToolTip;
		mIsoList.Add(uIBuildWndItem);
		return count;
	}

	public int AddCostListItem(string _itemText, string _itemNumber, Texture _contentTexture)
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mCostItem, mCostGrid.transform);
		int count = mCostList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mCost, _contentTexture, count);
		uIBuildWndItem.SetText(_itemText);
		uIBuildWndItem.SetNumber(_itemNumber);
		mCostList.Add(uIBuildWndItem);
		return count;
	}

	public int AddCostListItem(string _itemText, string _itemNumber, string _spriteName, string _AtlasName = "Button")
	{
		UIBuildWndItem uIBuildWndItem = AddBuildItem(mCostItem, mCostGrid.transform);
		int count = mCostList.Count;
		uIBuildWndItem.InitItem(UIBuildWndItem.ItemType.mCost, _spriteName, _AtlasName, count);
		uIBuildWndItem.SetText(_itemText);
		uIBuildWndItem.SetNumber(_itemNumber);
		mCostList.Add(uIBuildWndItem);
		return count;
	}

	private UIBuildWndItem AddBuildItem(GameObject _ItemPrefab, Transform _parent)
	{
		GameObject gameObject = Object.Instantiate(_ItemPrefab);
		gameObject.transform.parent = _parent;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.SetActive(value: true);
		return gameObject.GetComponent<UIBuildWndItem>();
	}

	public void RemoveIsoItem(int index)
	{
		if (index >= -1)
		{
			Object.Destroy(mIsoList[index].gameObject);
			mIsoList[index].transform.parent = null;
			mIsoList.RemoveAt(index);
			for (int i = 0; i < mIsoList.Count; i++)
			{
				mIsoList[i].mIndex = i;
			}
		}
	}

	public void ClearBlockList()
	{
		for (int i = 0; i < mBlockMatList.Count; i++)
		{
			mBlockMatList[i].transform.parent = null;
			Object.Destroy(mBlockMatList[i].gameObject);
		}
		mBlockMatList.Clear();
	}

	public void ClearTextureList()
	{
		for (int i = 0; i < mVoxelMatList.Count; i++)
		{
			mVoxelMatList[i].transform.parent = null;
			Object.Destroy(mVoxelMatList[i].gameObject);
		}
		mVoxelMatList.Clear();
	}

	public void ClearIsoList()
	{
		for (int i = 0; i < mIsoList.Count; i++)
		{
			mIsoList[i].transform.parent = null;
			Object.Destroy(mIsoList[i].gameObject);
		}
		mIsoList.Clear();
	}

	public void ClearCostList()
	{
		for (int i = 0; i < mCostList.Count; i++)
		{
			mCostList[i].transform.parent = null;
			Object.Destroy(mCostList[i].gameObject);
		}
		mCostList.Clear();
	}

	public void ResetBlockPostion()
	{
		UIGrid component = mBlockMatGrid.GetComponent<UIGrid>();
		component.repositionNow = true;
		component = mBlockPatternGrid.GetComponent<UIGrid>();
		component.repositionNow = true;
	}

	public void ResetVoxelPostion()
	{
		UIGrid component = mVoxelMatGrid.GetComponent<UIGrid>();
		component.repositionNow = true;
	}

	public void ResetIsoPostion()
	{
		UIGrid component = mIsoGrid.GetComponent<UIGrid>();
		component.repositionNow = true;
	}

	public void ResetCostPostion()
	{
		UIGrid component = mCostGrid.GetComponent<UIGrid>();
		component.repositionNow = true;
	}

	public void DisselectVoxel()
	{
		foreach (UIBuildWndItem mVoxelMat in mVoxelMatList)
		{
			mVoxelMat.SetSelect(_IsSelect: false);
		}
		mVoxelMatListSelectIndex = -1;
		string[] spriteName = new string[0];
		RefreshTypeLisItem(spriteName, "Icon");
		mTypeSelectIndex = -1;
	}

	public void DisselectBlock()
	{
		foreach (UIBuildWndItem mBlockMat in mBlockMatList)
		{
			mBlockMat.SetSelect(_IsSelect: false);
		}
		mBlockMatListSelectIndex = -1;
	}

	public void WndItemOnClick(UIBuildWndItem.ItemType _itemType, int _index)
	{
		switch (_itemType)
		{
		case UIBuildWndItem.ItemType.mBlockMat:
			if (mBlockMatList[_index].IsActive)
			{
				if (mBlockMatListSelectIndex > -1)
				{
					mBlockMatList[mBlockMatListSelectIndex].SetSelect(_IsSelect: false);
				}
				mBlockMatListSelectIndex = _index;
				mBlockMatList[mBlockMatListSelectIndex].SetSelect(_IsSelect: true);
				if (this.BlockItemOnClick != null)
				{
					this.BlockItemOnClick(_index);
				}
			}
			break;
		case UIBuildWndItem.ItemType.mBlockPattern:
			if (mBlockPatternList[_index].IsActive)
			{
				if (mBlockPatternListSelectIndex > -1)
				{
					mBlockPatternList[mBlockPatternListSelectIndex].SetSelect(_IsSelect: false);
				}
				mBlockPatternListSelectIndex = _index;
				mBlockPatternList[mBlockPatternListSelectIndex].SetSelect(_IsSelect: true);
				if (this.BlockPatternOnClick != null)
				{
					this.BlockPatternOnClick(_index);
				}
			}
			break;
		case UIBuildWndItem.ItemType.mVoxelMat:
			if (mVoxelMatList[_index].IsActive)
			{
				if (mVoxelMatListSelectIndex > -1)
				{
					mVoxelMatList[mVoxelMatListSelectIndex].SetSelect(_IsSelect: false);
				}
				mVoxelMatListSelectIndex = _index;
				mVoxelMatList[mVoxelMatListSelectIndex].SetSelect(_IsSelect: true);
				if (this.TextureItemOnClick != null)
				{
					this.TextureItemOnClick(_index);
				}
			}
			break;
		case UIBuildWndItem.ItemType.mIso:
			if (mIsoList[_index].IsActive)
			{
				if (mIsoListSelectIndex > -1)
				{
					mIsoList[mIsoListSelectIndex].SetSelect(_IsSelect: false);
				}
				mIsoListSelectIndex = _index;
				mIsoList[mIsoListSelectIndex].SetSelect(_IsSelect: true);
				if (this.IsoItemOnClick != null)
				{
					this.IsoItemOnClick(_index);
				}
			}
			break;
		case UIBuildWndItem.ItemType.mVoxelType:
			if (mTypeList[_index].IsActive)
			{
				if (mTypeSelectIndex > -1 && mTypeSelectIndex < mTypeList.Count)
				{
					mTypeList[mTypeSelectIndex].SetSelect(_IsSelect: false);
				}
				mTypeSelectIndex = _index;
				mTypeList[mTypeSelectIndex].SetSelect(_IsSelect: true);
				if (this.VoxelTypeOnClick != null)
				{
					this.VoxelTypeOnClick(_index);
				}
			}
			break;
		}
	}

	public void ChangeCkMenu(int index)
	{
		if (index == 0)
		{
			mCbTexture.isChecked = true;
		}
		if (index == 1)
		{
			mCbBlock.isChecked = true;
		}
		if (index == 2)
		{
			mCbSave.isChecked = true;
		}
		ChangePage(index);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		PackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
		if (mCbTexture.isChecked)
		{
			foreach (UIBuildWndItem mVoxelMat in mVoxelMatList)
			{
				mVoxelMat.SetNumber(cmpt.GetItemCount(mVoxelMat.ItemId).ToString());
			}
			return;
		}
		if (!mCbBlock.isChecked)
		{
			return;
		}
		foreach (UIBuildWndItem mBlockMat in mBlockMatList)
		{
			mBlockMat.SetNumber(cmpt.GetItemCount(mBlockMat.ItemId).ToString());
		}
	}

	private void ChangePage(int index)
	{
		switch (index)
		{
		case 0:
			mPageBlack.SetActive(value: false);
			mPageTexture.SetActive(value: true);
			mPageSave.SetActive(value: false);
			break;
		case 1:
			mPageBlack.SetActive(value: true);
			mPageTexture.SetActive(value: false);
			mPageSave.SetActive(value: false);
			break;
		case 2:
			mPageBlack.SetActive(value: false);
			mPageTexture.SetActive(value: false);
			mPageSave.SetActive(value: true);
			break;
		}
	}

	private void OnBeginDragMeaterItem(UIBuildWndItem.ItemType mItemType, int mIndex)
	{
		GameObject gameObject = null;
		switch (mItemType)
		{
		case UIBuildWndItem.ItemType.mBlockMat:
			if (mIndex >= mBlockMatList.Count && mIndex < 0)
			{
				return;
			}
			gameObject = ((!mBlockMatList[mIndex].mContentSprite.gameObject.activeSelf) ? mBlockMatList[mIndex].mContentTexture.gameObject : mBlockMatList[mIndex].mContentSprite.gameObject);
			mMeunControl.DragItem = mBlockMatList[mIndex];
			break;
		case UIBuildWndItem.ItemType.mBlockPattern:
			if (mIndex >= mBlockPatternList.Count && mIndex < 0)
			{
				return;
			}
			gameObject = ((!mBlockPatternList[mIndex].mContentSprite.gameObject.activeSelf) ? mBlockPatternList[mIndex].mContentTexture.gameObject : mBlockPatternList[mIndex].mContentSprite.gameObject);
			mMeunControl.DragItem = mBlockPatternList[mIndex];
			break;
		case UIBuildWndItem.ItemType.mVoxelMat:
			if (mIndex >= mVoxelMatList.Count && mIndex < 0)
			{
				return;
			}
			gameObject = ((!mVoxelMatList[mIndex].mContentSprite.gameObject.activeSelf) ? mVoxelMatList[mIndex].mContentTexture.gameObject : mVoxelMatList[mIndex].mContentSprite.gameObject);
			mMeunControl.DragItem = mVoxelMatList[mIndex];
			break;
		case UIBuildWndItem.ItemType.mVoxelType:
			if (mIndex >= mTypeList.Count && mIndex < 0)
			{
				return;
			}
			gameObject = ((!mTypeList[mIndex].mContentSprite.gameObject.activeSelf) ? mTypeList[mIndex].mContentTexture.gameObject : mTypeList[mIndex].mContentSprite.gameObject);
			mMeunControl.DragItem = mTypeList[mIndex];
			break;
		}
		if (gameObject != null)
		{
			if (mMeunControl.DragObejct != null)
			{
				Object.Destroy(mMeunControl.DragObejct);
				mMeunControl.DragObejct = null;
			}
			mMeunControl.DragObejct = Object.Instantiate(gameObject);
			mMeunControl.DragObejct.transform.parent = mDragItemContent.transform;
			mMeunControl.DragObejct.transform.localScale = new Vector3(48f, 48f, 1f);
			mMeunControl.DragObejct.transform.localPosition = Vector3.zero;
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = mMeunControl.DragObejct.transform.position.z;
			mMeunControl.DragObejct.transform.position = mousePosition;
		}
	}

	private void OnDragMeaterItem(UIBuildWndItem.ItemType mItemType, int mIndex)
	{
		if (mMeunControl.DragObejct != null)
		{
			Vector3 mousePosition = Input.mousePosition;
			mMeunControl.DragObejct.transform.localPosition = new Vector3(mousePosition.x, mousePosition.y, -15f);
		}
	}

	private void OnDropMeaterItem(UIBuildWndItem.ItemType mItemType, int mIndex)
	{
	}

	private UIBuildWndItem QueryGetDragItem()
	{
		if (uiCamera == null)
		{
			return null;
		}
		Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
		int num = -1;
		for (int i = 0; i < mMeunControl.mMenuList.Count; i++)
		{
			BoxCollider component = mMeunControl.mMenuList[i].gameObject.GetComponent<BoxCollider>();
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
		return mMeunControl.mMenuList[num];
	}

	private void BtnTex_OnClick()
	{
		ChangeCkMenu(0);
		if (this.onPageClick != null)
		{
			this.onPageClick(0);
		}
	}

	private void BtnBlock_OnClick()
	{
		ChangeCkMenu(1);
		if (this.onPageClick != null)
		{
			this.onPageClick(1);
		}
	}

	private void BtnISO_OnClick()
	{
		ChangeCkMenu(2);
		if (this.onPageClick != null)
		{
			this.onPageClick(2);
		}
	}

	private void BtnCloseOnClick()
	{
		Hide();
		if (this.BtnClose != null)
		{
			this.BtnClose();
		}
	}

	private void BtnDeleteOnClick()
	{
		if (mIsoListSelectIndex != -1 && this.BtnDelete != null)
		{
			this.BtnDelete(mIsoListSelectIndex);
		}
	}

	private void BtnExportOnClick()
	{
		if (mIsoListSelectIndex != -1 && this.BtnExport != null)
		{
			this.BtnExport(mIsoListSelectIndex);
		}
	}

	private void CkControl2OnClick()
	{
		if (this.CkControl2 != null)
		{
			this.CkControl2();
		}
	}

	private void OnToolTip(bool isShow, UIBuildWndItem.ItemType _ItemType, int _Index)
	{
		if (this.ToolTip != null)
		{
			this.ToolTip(isShow, _ItemType, _Index);
		}
	}
}
