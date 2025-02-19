using System.Collections.Generic;
using CustomData;

public class LobbyShopMgr
{
	private static List<LobbyShopData> shopData = new List<LobbyShopData>();

	public static List<LobbyShopData> ShopData => shopData;

	public static void Add(LobbyShopData data)
	{
		if (!shopData.Contains(data))
		{
			shopData.Add(data);
			UILobbyShopItem item = new UILobbyShopItem(data);
			UILobbyShopItemMgr._self.Add(item);
		}
	}

	public static void Remove(LobbyShopData data)
	{
		shopData.Remove(data);
	}

	public static void Add(LobbyShopData[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			if (!shopData.Contains(data[i]))
			{
				Add(data[i]);
			}
		}
	}

	public static void AddAll(LobbyShopData[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			LobbyShopData lobbyShopData = shopData.Find((LobbyShopData iter) => iter.id == data[i].id);
			if (lobbyShopData == null)
			{
				Add(data[i]);
			}
		}
		UILobbyShopItemMgr._self.MallItemEvent(0, Mall_Tab.tab_Hot);
	}

	public static void AddRange(LobbyShopData[] data, int startIndex, int tabIndex)
	{
		for (int i = 0; i < data.Length; i++)
		{
			if (!shopData.Contains(data[i]))
			{
				Add(data[i]);
			}
		}
		UILobbyShopItemMgr._self.MallItemEvent(startIndex, (Mall_Tab)tabIndex);
	}

	public static int GetPrice(int id)
	{
		LobbyShopData lobbyShopData = shopData.Find((LobbyShopData iter) => iter.id == id);
		if (lobbyShopData != null)
		{
			return lobbyShopData.price * lobbyShopData.rebate / 100;
		}
		return -1;
	}

	public static int GetTab(int id)
	{
		return shopData.Find((LobbyShopData iter) => iter.id == id)?.tab ?? (-1);
	}

	public static int GetForbid(int itemType)
	{
		return shopData.Find((LobbyShopData iter) => iter.itemtype == itemType)?.forbid ?? (-1);
	}
}
