using System.Collections.Generic;
using CustomData;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UIDropItemWnd : UIBaseWnd
{
	public class DropItemInfo
	{
		public int mPickTab;

		public int mPagkageIndex;

		public Grid_N mPackageGrid;

		public ItemObject mDropItemObj;

		public void SetInfo(int pickTab, int index, Grid_N grid, ItemObject dropItemObj)
		{
			mPickTab = pickTab;
			mPagkageIndex = index;
			mPackageGrid = grid;
			mDropItemObj = dropItemObj;
		}
	}

	private const int PAGE_CNT = 24;

	[SerializeField]
	private Grid_N mGridPrefab;

	[SerializeField]
	private UIGrid mDropItemGrid;

	[SerializeField]
	private UIScrollBar mDropItemScrollBar;

	[SerializeField]
	private LayerMask mItemDroplayer;

	private List<Grid_N> mDropItemGrids;

	private List<DropItemInfo> mDropInfoList;

	private List<int> mDropReq = new List<int>();

	private UIItemPackageCtrl mItemPackage => GameUI.Instance.mItemPackageCtrl;

	public List<int> DropReqList => mDropReq;

	private void Update()
	{
		UpdatePos();
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		UpdatePos();
		mDropItemGrids = new List<Grid_N>();
		mDropInfoList = new List<DropItemInfo>();
		mItemPackage.e_OnResetItem += ResetPackageWnd;
		InitGrids();
	}

	public override void Show()
	{
		base.Show();
		ResetGridItems();
	}

	protected override void OnHide()
	{
		CancelDropItems();
		base.OnHide();
	}

	public void CancelDropItems()
	{
		ResetGridItems();
		if (null != mItemPackage)
		{
			mItemPackage.ResetItem();
		}
	}

	public void AddToDropList(int packTab, int Index, Grid_N grid, Grid_N curDropItem = null)
	{
		if (curDropItem == null)
		{
			curDropItem = GetEmptyGrid();
			if (null == curDropItem)
			{
				PeTipMsg.Register(PELocalization.GetString(82209001), PeTipMsg.EMsgLevel.Warning);
				return;
			}
		}
		RemoveFromDropList(curDropItem);
		curDropItem.SetItem(grid.Item);
		DropItemInfo dropItemInfo = new DropItemInfo();
		dropItemInfo.SetInfo(packTab, Index, grid, grid.ItemObj);
		mDropInfoList.Add(dropItemInfo);
		ResetPackageWnd(packTab, Index);
	}

	public void RemoveFromDropList(Grid_N grid)
	{
		if (grid.ItemObj != null)
		{
			int num = mDropInfoList.FindIndex((DropItemInfo itr) => itr.mDropItemObj == grid.ItemObj);
			if (num != -1)
			{
				mDropInfoList.RemoveAt(num);
				grid.SetItem(null);
				mItemPackage.ResetItem();
			}
		}
	}

	private Grid_N GetEmptyGrid()
	{
		return (mDropItemGrids != null) ? mDropItemGrids.Find((Grid_N it) => it.Item == null) : null;
	}

	private void OnMouseLeftClick(Grid_N grid)
	{
		if (null != grid && grid.ItemObj != null)
		{
			SelectItem_N.Instance.SetItemGrid(grid);
		}
	}

	private void OnDropItem(Grid_N grid)
	{
		if (null != mItemPackage && null != grid && null != SelectItem_N.Instance.Grid && SelectItem_N.Instance.Grid.ItemPlace == ItemPlaceType.IPT_Bag)
		{
			AddToDropList(mItemPackage.CurrentPickTab, mItemPackage.CurrentPageIndex, SelectItem_N.Instance.Grid, grid);
		}
	}

	private void ResetGridItems()
	{
		if (!mInit)
		{
			return;
		}
		foreach (Grid_N mDropItemGrid in mDropItemGrids)
		{
			mDropItemGrid.SetItem(null);
		}
		mDropInfoList.Clear();
		mDropItemScrollBar.scrollValue = 0f;
	}

	private Grid_N GetNewGrid()
	{
		Grid_N grid_N = Object.Instantiate(mGridPrefab);
		Object.Destroy(grid_N.transform.GetComponent<UIPanel>());
		grid_N.transform.parent = mDropItemGrid.transform;
		grid_N.transform.localPosition = Vector3.zero;
		grid_N.transform.localScale = Vector3.one;
		grid_N.SetItem(null);
		grid_N.SetItemPlace(ItemPlaceType.IPT_DropItem, mDropItemGrids.Count);
		grid_N.onLeftMouseClicked = OnMouseLeftClick;
		grid_N.onRightMouseClicked = RemoveFromDropList;
		grid_N.onDropItem = OnDropItem;
		return grid_N;
	}

	private void ResetPackageWnd(int packTab, int pageIndex)
	{
		if (!isShow)
		{
			return;
		}
		foreach (DropItemInfo mDropInfo in mDropInfoList)
		{
			if (mDropInfo.mPickTab == packTab && mDropInfo.mPagkageIndex == pageIndex)
			{
				mDropInfo.mPackageGrid.SetItem(null);
			}
		}
	}

	private void OnDropBtn()
	{
		if (null != GameUI.Instance.mMainPlayer && mDropInfoList.Count > 0)
		{
			ApplyPackDropItem(mDropInfoList);
		}
		base.OnHide();
	}

	private void OnDropClose()
	{
		OnHide();
	}

	private void ApplyPackDropItem(List<DropItemInfo> itemList)
	{
		if (itemList == null || null == GameUI.Instance || null == GameUI.Instance.mMainPlayer)
		{
			return;
		}
		SkAliveEntity cmpt = GameUI.Instance.mMainPlayer.GetCmpt<SkAliveEntity>();
		if (cmpt == null || null == cmpt.Entity)
		{
			return;
		}
		mDropReq.Clear();
		foreach (DropItemInfo item in itemList)
		{
			if (item != null && item.mDropItemObj != null)
			{
				mDropReq.Add(item.mDropItemObj.instanceId);
			}
		}
		MapObj[] array = new MapObj[1];
		int num = 0;
		while (num++ < 100)
		{
			if ((bool)cmpt.Entity)
			{
				Vector3 vector = new Vector3(Random.Range(-2f, 2f), 2f, Random.Range(-2f, 2f));
				if (Physics.Raycast(cmpt.Entity.position + vector, Vector3.down, out var hitInfo, 10f, mItemDroplayer) && !hitInfo.collider.isTrigger && hitInfo.distance < 10f)
				{
					array[0] = new MapObj();
					array[0].pos = hitInfo.point;
					array[0].objID = cmpt.GetId();
					break;
				}
			}
		}
		if (array[0] != null)
		{
			PlayerNetwork.mainPlayer.CreateMapObj(1, array);
		}
	}

	private void InitGrids()
	{
		if (mDropItemGrid.transform.childCount > 0)
		{
			for (int i = 0; i < mDropItemGrid.transform.childCount; i++)
			{
				Object.Destroy(mDropItemGrid.transform.GetChild(i).gameObject);
			}
		}
		for (int j = 0; j < 24; j++)
		{
			mDropItemGrids.Add(GetNewGrid());
		}
		mDropItemGrid.Reposition();
		mDropItemScrollBar.scrollValue = 0f;
	}

	private void UpdatePos()
	{
		Vector3 localPosition = mItemPackage.transform.localPosition;
		localPosition.x += 375f;
		base.transform.localPosition = localPosition;
	}
}
