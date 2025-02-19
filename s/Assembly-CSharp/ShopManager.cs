using System.Collections.Generic;
using ItemAsset;

public class ShopManager
{
	private static Dictionary<int, Record.stShopInfo> _npcShops = new Dictionary<int, Record.stShopInfo>();

	public static bool HasNpcShop(int npcId)
	{
		return _npcShops.ContainsKey(npcId);
	}

	private static void InitNpcShopData(int npcId, int templateId)
	{
		StoreData npcStoreData = StoreRepository.GetNpcStoreData(templateId);
		if (npcStoreData == null)
		{
			return;
		}
		if (!_npcShops.ContainsKey(npcId))
		{
			_npcShops[npcId] = new Record.stShopInfo();
		}
		if (ServerConfig.IsStory)
		{
			foreach (int item in npcStoreData.itemListstory)
			{
				ShopData shopData = ShopRespository.GetShopData(item);
				if (shopData != null)
				{
					ItemObject itemObject = ItemManager.CreateItem(shopData.m_ItemID, shopData.m_LimitNum);
					if (itemObject != null)
					{
						_npcShops[npcId].ShopList[item] = new Record.stShopData(itemObject.instanceId, GameTime.Timer.Second);
					}
				}
			}
			return;
		}
		foreach (int item2 in npcStoreData.ItemList)
		{
			ShopData shopData2 = ShopRespository.GetShopData(item2);
			if (shopData2 != null)
			{
				ItemObject itemObject2 = ItemManager.CreateItem(shopData2.m_ItemID, shopData2.m_LimitNum);
				if (itemObject2 != null)
				{
					_npcShops[npcId].ShopList[item2] = new Record.stShopData(itemObject2.instanceId, GameTime.Timer.Second);
				}
			}
		}
	}

	public static ItemObject[] GetNpcShopData(int npcId, int templateId)
	{
		if (!_npcShops.ContainsKey(npcId))
		{
			InitNpcShopData(npcId, templateId);
		}
		List<ItemObject> list = RefreshShop(_npcShops[npcId].ShopList);
		return list.ToArray();
	}

	public static Record.stShopData GetNpcShopItem(int npcId, int instanceId, out int shopId)
	{
		shopId = -1;
		if (!_npcShops.ContainsKey(npcId))
		{
			return null;
		}
		if (_npcShops[npcId].ShopList == null)
		{
			return null;
		}
		foreach (KeyValuePair<int, Record.stShopData> shop in _npcShops[npcId].ShopList)
		{
			if (shop.Value.ItemObjID == instanceId)
			{
				shopId = shop.Key;
				return shop.Value;
			}
		}
		return null;
	}

	public static List<int> GetNpcStoreId(int npcId, int templateId)
	{
		List<int> list = new List<int>();
		StoreData npcStoreData = StoreRepository.GetNpcStoreData(templateId);
		if (npcStoreData == null)
		{
			return list;
		}
		if (ServerConfig.IsStory)
		{
			foreach (int item in npcStoreData.itemListstory)
			{
				ShopData shopData = ShopRespository.GetShopData(item);
				if (shopData != null)
				{
					list.Add(item);
				}
			}
		}
		else
		{
			foreach (int item2 in npcStoreData.ItemList)
			{
				ShopData shopData2 = ShopRespository.GetShopData(item2);
				if (shopData2 != null)
				{
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public static List<ItemObject> RefreshNpcShop(int npcId)
	{
		List<ItemObject> result = new List<ItemObject>();
		if (!_npcShops.ContainsKey(npcId))
		{
			return result;
		}
		return RefreshShop(_npcShops[npcId].ShopList);
	}

	public static List<ItemObject> RefreshShop(Dictionary<int, Record.stShopData> shop)
	{
		List<ItemObject> list = new List<ItemObject>();
		bool flag = true;
		foreach (KeyValuePair<int, Record.stShopData> item in shop)
		{
			ShopData shopData = ShopRespository.GetShopData(item.Key);
			if (shopData == null)
			{
				continue;
			}
			flag = true;
			if (ServerConfig.IsStory)
			{
				for (int i = 0; i < shopData.m_LimitMisIDList.Count; i++)
				{
					if (shopData.m_LimitType == 1)
					{
						if (MissionManager.Manager.GetTeamPlayerMission(1).HadCompleteMission(shopData.m_LimitMisIDList[i], null))
						{
							break;
						}
					}
					else if (!MissionManager.Manager.GetTeamPlayerMission(1).HadCompleteMission(shopData.m_LimitMisIDList[i], null))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			ItemObject itemObject = ItemManager.GetItemByID(item.Value.ItemObjID);
			if (item.Value.CreateTime < 0.0)
			{
				itemObject = ItemManager.CreateItem(shopData.m_ItemID, 1);
				itemObject.SetStackCount(shopData.m_LimitNum);
				item.Value.ItemObjID = itemObject.instanceId;
				item.Value.CreateTime = GameTime.Timer.Second;
			}
			else if (GameTime.Timer.Second - item.Value.CreateTime > (double)shopData.m_RefreshTime)
			{
				if (itemObject == null)
				{
					itemObject = ItemManager.CreateItem(shopData.m_ItemID, 1);
				}
				itemObject.SetStackCount(shopData.m_LimitNum);
				item.Value.ItemObjID = itemObject.instanceId;
				item.Value.CreateTime = GameTime.Timer.Second;
			}
			else if (itemObject == null)
			{
				continue;
			}
			list.Add(itemObject);
		}
		return list;
	}
}
