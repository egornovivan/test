using CustomData;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UILobbyShopItem : MallItemData
{
	private LobbyShopData _data;

	private ItemProto _itemData;

	public UILobbyShopItem(LobbyShopData data)
	{
		_data = data;
		if (_data != null)
		{
			_itemData = PeSingleton<ItemProto.Mgr>.Instance.Get(_data.itemtype);
		}
		if (_itemData == null)
		{
			Debug.LogError("lobby shop _itemData is null itemtype = " + _data.itemtype);
		}
	}

	public int GetID()
	{
		return _data.id;
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
		return _data.price;
	}

	public int GetDiscount()
	{
		return _data.rebate;
	}

	public bool ShowDiscount()
	{
		if (_data.rebate == 100)
		{
			return false;
		}
		return true;
	}

	public int GetTab()
	{
		return _data.tab;
	}

	public int GetItemType()
	{
		return _data.itemtype;
	}

	public int GetCount()
	{
		return 1;
	}
}
