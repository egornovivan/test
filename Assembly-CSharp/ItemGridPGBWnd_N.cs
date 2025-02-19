using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ItemGridPGBWnd_N : PageGridBox
{
	public Grid_N mPrefab;

	private List<Grid_N> mGridList;

	private List<int> mItemList;

	private ItemPlaceType mItemPlace;

	public List<Grid_N> GridList => mGridList;

	public void Init()
	{
		mGridList = new List<Grid_N>();
		for (int i = 0; i < mPageNum; i++)
		{
			Grid_N grid_N = Object.Instantiate(mPrefab);
			grid_N.name = "Item" + i;
			grid_N.transform.parent = mGrid.transform;
			grid_N.transform.localScale = Vector3.one;
			grid_N.transform.localPosition = Vector3.back;
			mGridList.Add(grid_N);
		}
		mGrid.Reposition();
	}

	public void SetItemList(List<int> itemList, ItemPlaceType itemPlace)
	{
		mItemList = itemList;
		mItemPlace = itemPlace;
		mMaxPagIndex = (mItemList.Count - 1) / mPageNum;
		if (mPagIndex > mMaxPagIndex)
		{
			mPagIndex = mMaxPagIndex;
		}
		UpdateList();
	}

	protected override void UpdateList()
	{
		if (mItemList == null)
		{
			return;
		}
		base.UpdateList();
		int num = mPagIndex * mPageNum;
		for (int i = 0; i < mPageNum; i++)
		{
			if (i + num < mItemList.Count)
			{
				mGridList[i].gameObject.SetActive(value: true);
				mGridList[i].SetItem(PeSingleton<ItemMgr>.Instance.Get(mItemList[i + num]));
				mGridList[i].SetItemPlace(mItemPlace, i + num);
			}
			else
			{
				mGridList[i].SetItem(null);
				mGridList[i].gameObject.SetActive(value: false);
			}
		}
	}
}
