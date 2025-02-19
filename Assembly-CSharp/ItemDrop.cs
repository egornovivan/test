using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ItemDrop : MonoBehaviour, IDroppableItemList, IItemDrop
{
	private const float PickDistance = 8f;

	public Action fetchItem;

	private MapObjNetwork _net;

	protected List<ItemSample> _itemList = new List<ItemSample>(1);

	protected PlayerPackageCmpt mPlayerPkg;

	protected PlayerPackageCmpt playerPkg
	{
		get
		{
			if (null == mPlayerPkg)
			{
				mPlayerPkg = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			}
			return mPlayerPkg;
		}
	}

	public int DroppableItemCount => _itemList.Count;

	public void SetNet(MapObjNetwork net)
	{
		_net = net;
	}

	public int GetCountByProtoId(int protoId)
	{
		int num = 0;
		foreach (ItemSample item in _itemList)
		{
			if (item.protoId == protoId)
			{
				num += item.stackCount;
			}
		}
		return num;
	}

	public void AddItem(int itemId, int count)
	{
		ItemSample item = new ItemSample(itemId, count);
		AddItem(item);
	}

	public void AddItem(ItemSample item)
	{
		_itemList.Add(item);
	}

	protected bool CanFetch(int index)
	{
		if (index < 0 || index >= ((IItemDrop)this).GetCount())
		{
			return false;
		}
		return playerPkg.package.CanAdd(((IItemDrop)this).Get(index));
	}

	protected bool CanFetchAll()
	{
		int count = GetCount();
		MaterialItem[] array = new MaterialItem[count];
		for (int i = 0; i < count; i++)
		{
			ItemSample itemSample = Get(i);
			array[i] = new MaterialItem
			{
				protoId = itemSample.protoId,
				count = itemSample.stackCount
			};
		}
		return playerPkg.package.CanAdd(array);
	}

	public virtual ItemSample Get(int index)
	{
		return _itemList[index];
	}

	public virtual int GetCount()
	{
		return _itemList.Count;
	}

	public virtual void Fetch(int index)
	{
		if (!CanFetch(index))
		{
			return;
		}
		if (_net != null)
		{
			_net.GetItem(((ItemObject)_itemList[index]).instanceId);
			return;
		}
		ItemSample itemSample = _itemList[index];
		playerPkg.Add(itemSample.protoId, itemSample.stackCount);
		_itemList.Remove(itemSample);
		if (fetchItem != null)
		{
			fetchItem();
		}
	}

	public virtual void FetchAll()
	{
		if (!CanFetchAll())
		{
			return;
		}
		if (_net != null)
		{
			_net.GetAllItem();
			return;
		}
		foreach (ItemSample item in _itemList)
		{
			playerPkg.Add(item.protoId, item.stackCount);
		}
		_itemList.Clear();
		if (fetchItem != null)
		{
			fetchItem();
		}
	}

	public ItemSample GetDroppableItemAt(int idx)
	{
		return _itemList[idx];
	}

	public void AddDroppableItem(ItemSample item)
	{
		_itemList.Add(item);
	}

	public void RemoveDroppableItem(ItemSample item)
	{
		_itemList.Remove(item);
	}

	public void RemoveDroppableItemAll()
	{
		_itemList.Clear();
	}

	private bool CheckDistance(Vector3 pos)
	{
		PeTrans cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PeTrans>();
		if (cmpt == null)
		{
			return true;
		}
		return Vector3.Distance(cmpt.position, pos) < 8f;
	}

	public bool CheckDistance()
	{
		MousePickable component = GetComponent<MousePickable>();
		if (component != null)
		{
			PeTrans component2 = component.GetComponent<PeTrans>();
			if (component2 != null)
			{
				return CheckDistance(component2.position);
			}
		}
		return false;
	}

	protected void OpenGui(Vector3 pos)
	{
		if (CheckDistance(pos) && GameUI.Instance != null)
		{
			GameUI.Instance.mItemGet.Show();
			GameUI.Instance.mItemGet.SetItemDrop(this);
		}
	}
}
