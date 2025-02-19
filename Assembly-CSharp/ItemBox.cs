using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ItemBox : MousePickableChildCollider
{
	[HideInInspector]
	public int mID;

	[HideInInspector]
	public MapObjNetwork mNetWork;

	private List<int> mItemList;

	public Vector3 mPos
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public List<int> ItemList => mItemList;

	private void Awake()
	{
		mItemList = new List<int>();
	}

	public void OnRequestItemList(List<int> itemlist, bool bShow = true)
	{
		if (null != GameUI.Instance.mItemBox && bShow)
		{
			GameUI.Instance.mItemBox.Show();
		}
		ResetItem(itemlist);
		if (null != GameUI.Instance.mItemBox)
		{
			GameUI.Instance.mItemBox.SetItemList(this, mItemList);
		}
	}

	public void ResetItem(List<int> itemlist)
	{
		mItemList.Clear();
		AddItems(itemlist);
	}

	public void InsertItem(List<int> itemList)
	{
		if (null != mNetWork)
		{
			mNetWork.InsertItemList(itemList.ToArray());
		}
	}

	public void AddItems(List<int> itemlist)
	{
		foreach (int item in itemlist)
		{
			if (!mItemList.Contains(item))
			{
				mItemList.Add(item);
			}
		}
		ResetUI();
	}

	public void AddItem(int objID)
	{
		if (!mItemList.Contains(objID))
		{
			mItemList.Add(objID);
			ResetUI();
		}
	}

	public void AddItem(ItemObject item)
	{
		AddItem(item.instanceId);
	}

	public void RemoveItem(int objID)
	{
		if (mItemList.Remove(objID))
		{
			ResetUI();
		}
	}

	private void ResetUI()
	{
		if (null != GameUI.Instance.mItemBox && GameUI.Instance.mItemBox.OpBox == this)
		{
			GameUI.Instance.mItemBox.SetItemList(this, mItemList);
		}
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (!PeInput.Get(PeInput.LogicFunction.OpenItemMenu) || !(null != GameUI.Instance.mMainPlayer) || GameUI.Instance.bMainPlayerIsDead)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (null != mNetWork)
			{
				mNetWork.RequestItemList();
			}
			GameUI.Instance.mItemBox.Show();
			mItemList.Clear();
			GameUI.Instance.mItemBox.SetItemList(this, mItemList);
		}
		else
		{
			GameUI.Instance.mItemBox.Show();
			GameUI.Instance.mItemBox.SetItemList(this, mItemList);
			ResetUI();
		}
	}

	public void SendItemToPlayer(ItemObject itemObj)
	{
	}

	public void SendAllItemToPlayer()
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != mNetWork)
			{
				mNetWork.GetAllItem();
			}
			return;
		}
		foreach (int mItem in mItemList)
		{
			ItemObject item = PeSingleton<ItemMgr>.Instance.Get(mItem);
			if (null != MainPlayerCmpt.gMainPlayer)
			{
				PlayerPackageCmpt component = MainPlayerCmpt.gMainPlayer.GetComponent<PlayerPackageCmpt>();
				component.Add(item);
			}
		}
		mItemList.Clear();
		CheckDestroy();
	}

	private void CheckDestroy()
	{
		if (mItemList.Count == 0)
		{
			ItemBoxMgr.Instance.Remove(this);
		}
	}
}
