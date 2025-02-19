using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UILobbyShopItemMgr
{
	public const int GAMEMODE_VS = 1;

	public const int GAMEMODE_COOPERATION = 2;

	public const int GAMEMODE_SURVIVE = 4;

	public static UILobbyShopItemMgr _self = new UILobbyShopItemMgr();

	private Dictionary<int, List<UILobbyShopItem>> _items = new Dictionary<int, List<UILobbyShopItem>>();

	public void Add(UILobbyShopItem item)
	{
		if (!_items.ContainsKey(item.GetTab()))
		{
			_items[item.GetTab()] = new List<UILobbyShopItem>();
		}
		_items[item.GetTab()].Add(item);
	}

	public void Remove(UILobbyShopItem item)
	{
		if (_items.ContainsKey(item.GetTab()))
		{
			_items[item.GetTab()].Remove(item);
		}
	}

	public void MallItemEvent(int starIndex, Mall_Tab tabIndex)
	{
		List<MallItemData> list = new List<MallItemData>();
		int num = 0;
		if (tabIndex < Mall_Tab.tab_Item)
		{
			if (!_items.ContainsKey((int)tabIndex) || _items[(int)tabIndex].Count < starIndex)
			{
				return;
			}
			for (int i = starIndex; i < starIndex + 12 && i < _items[(int)tabIndex].Count; i++)
			{
				list.Add(_items[(int)tabIndex][i]);
			}
			num = _items[(int)tabIndex].Count;
		}
		else
		{
			List<MallItemData> myShopItemsByTab = GetMyShopItemsByTab((int)tabIndex);
			num = myShopItemsByTab.Count;
			if (num < starIndex)
			{
				return;
			}
			for (int j = starIndex; j < starIndex + 12 && j < myShopItemsByTab.Count; j++)
			{
				list.Add(myShopItemsByTab[j]);
			}
		}
		UIMallWnd.Instance.SetPageInfo(num, list);
	}

	public void MallItemBuyEvent(Mall_Tab tabIndex, MallItemData mallItem)
	{
		if (_items.ContainsKey((int)tabIndex))
		{
			BuyItems(mallItem.GetID(), 1);
		}
	}

	public void MallItemExportEvent(Mall_Tab tabIndex, MallItemData mallItem)
	{
		if (mallItem == null)
		{
			Debug.LogError("mallItem is null");
		}
		else if (AccountItems.self.CheckCreateItems(mallItem.GetItemType(), 1))
		{
			CreateAccountItems(mallItem.GetItemType(), 1);
		}
	}

	public bool BuyItems(int id, int amount)
	{
		int price = LobbyShopMgr.GetPrice(id);
		if (price <= 0 || price * amount > (int)AccountItems.self.balance)
		{
			return false;
		}
		LobbyInterface.LobbyRPC(ELobbyMsgType.BuyItems, id, amount);
		return true;
	}

	public bool CreateAccountItems(int itemType, int amount)
	{
		int forbid = LobbyShopMgr.GetForbid(itemType);
		switch (PeGameMgr.gameType)
		{
		case PeGameMgr.EGameType.Cooperation:
			if ((forbid & 2) == 1)
			{
				return false;
			}
			break;
		case PeGameMgr.EGameType.VS:
			if ((forbid & 1) == 1)
			{
				return false;
			}
			break;
		case PeGameMgr.EGameType.Survive:
			if ((forbid & 4) == 1)
			{
				return false;
			}
			break;
		}
		PlayerNetwork.mainPlayer.CreateAccountItems(itemType, amount);
		return true;
	}

	public Dictionary<int, int> MyShopItems()
	{
		return AccountItems.self.MyShopItems;
	}

	public List<MallItemData> GetMyShopItemsByTab(int tab)
	{
		List<MallItemData> list = new List<MallItemData>();
		foreach (KeyValuePair<int, int> item in MyShopItems())
		{
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(item.Key);
			if (itemProto != null)
			{
				UIMyLobbyShopItem uIMyLobbyShopItem = new UIMyLobbyShopItem(itemProto, item.Value);
				if (uIMyLobbyShopItem.GetTab() == tab)
				{
					list.Add(uIMyLobbyShopItem);
				}
			}
		}
		return list;
	}
}
