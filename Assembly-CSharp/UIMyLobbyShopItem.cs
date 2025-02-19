using ItemAsset;
using UnityEngine;

public class UIMyLobbyShopItem : MallItemData
{
	private ItemProto _itemData;

	private int _count;

	public int Count
	{
		get
		{
			return _count;
		}
		set
		{
			_count = value;
		}
	}

	public UIMyLobbyShopItem(ItemProto data, int count)
	{
		_itemData = data;
		_count = count;
		if (_itemData == null)
		{
			Debug.LogError("UIMyLobbyShopItem shop _itemData is null itemtype = " + data.id);
		}
	}

	public int GetID()
	{
		return -1;
	}

	public string GetSprName()
	{
		if (_itemData != null)
		{
			return _itemData.shopIcon;
		}
		return string.Empty;
	}

	public string GetName()
	{
		if (_itemData != null)
		{
			return _itemData.GetName();
		}
		return string.Empty;
	}

	public int GetPrice()
	{
		return -1;
	}

	public int GetDiscount()
	{
		return -1;
	}

	public bool ShowDiscount()
	{
		return false;
	}

	public int GetTab()
	{
		if (_itemData.equipType > EquipType.Null && _itemData.equipType <= EquipType.Trousers)
		{
			return 5;
		}
		return 4;
	}

	public int GetItemType()
	{
		return _itemData.id;
	}

	public int GetCount()
	{
		return _count;
	}
}
