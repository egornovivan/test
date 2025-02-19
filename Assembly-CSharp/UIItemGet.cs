using System.Collections.Generic;
using ItemAsset;
using UnityEngine;

public class UIItemGet : UIBaseWnd
{
	public UIGrid mGrid;

	public ItemGetItem_N mPrefab;

	public UIScrollBar mSlider;

	private List<ItemGetItem_N> mItemList = new List<ItemGetItem_N>();

	private IItemDrop mItemDrop;

	private Vector3 mPostionShow = Vector3.zero;

	public override void Show()
	{
		mSlider.scrollValue = 0f;
		mPostionShow = GameUI.Instance.mMainPlayer.position;
		base.Show();
	}

	protected override void OnHide()
	{
		mItemDrop = null;
		Clear();
		base.OnHide();
	}

	private void Clear()
	{
		for (int i = 0; i < mItemList.Count; i++)
		{
			Object.Destroy(mItemList[i].gameObject);
			mItemList[i].gameObject.transform.parent = null;
		}
		mItemList.Clear();
	}

	public void SetItemDrop(IItemDrop itemDrop)
	{
		mItemDrop = itemDrop;
		Reflash();
	}

	public void Reflash()
	{
		if (mItemDrop == null)
		{
			return;
		}
		int i;
		for (i = 0; i < mItemDrop.GetCount(); i++)
		{
			if (mItemList.Count <= i)
			{
				AddItem(mItemDrop.Get(i));
				mGrid.repositionNow = true;
			}
			else
			{
				SetItem(i, mItemDrop.Get(i));
			}
		}
		int num = i;
		while (num < mItemList.Count)
		{
			RemoveItem(mItemList[num]);
		}
		if (mItemList.Count == 0)
		{
			Hide();
		}
	}

	private void AddItem(ItemSample itemGrid)
	{
		if (itemGrid != null)
		{
			ItemGetItem_N itemGetItem_N = Object.Instantiate(mPrefab);
			itemGetItem_N.transform.parent = mGrid.transform;
			itemGetItem_N.transform.localPosition = new Vector3(0f, 0f, -1f);
			itemGetItem_N.transform.localRotation = Quaternion.identity;
			itemGetItem_N.transform.localScale = Vector3.one;
			itemGetItem_N.SetItem(itemGrid, mItemList.Count);
			itemGetItem_N.e_GetItem += GetItem;
			mItemList.Add(itemGetItem_N);
		}
	}

	private void SetItem(int index, ItemSample item)
	{
		if (item != null && index > -1 && index < mItemList.Count)
		{
			mItemList[index].SetItem(item, index);
			mItemList[index].e_GetItem -= GetItem;
			mItemList[index].e_GetItem += GetItem;
		}
	}

	private void RemoveItem(ItemGetItem_N item)
	{
		Object.Destroy(item.gameObject);
		item.gameObject.transform.parent = null;
		mItemList.Remove(item);
	}

	private void Update()
	{
		float num = Vector3.Distance(mPostionShow, GameUI.Instance.mMainPlayer.position);
		if (num > 10f || mItemDrop == null || mItemDrop.Equals(null))
		{
			Hide();
		}
	}

	private void BtnGetAll_OnClick()
	{
		if (mItemDrop != null && !mItemDrop.Equals(null))
		{
			mItemDrop.FetchAll();
		}
		Hide();
	}

	private void GetItem(ItemGetItem_N item)
	{
		if (!mItemList.Contains(item))
		{
			return;
		}
		int num = mItemList.FindIndex((ItemGetItem_N itr) => itr == item);
		if (num != -1 && mItemDrop != null && !mItemDrop.Equals(null))
		{
			mItemDrop.Fetch(num);
			if (!GameConfig.IsMultiClient)
			{
				Reflash();
			}
		}
		if (mItemList.Count == 0)
		{
			Hide();
		}
	}
}
