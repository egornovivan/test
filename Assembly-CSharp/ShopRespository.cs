using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class ShopRespository
{
	public static Dictionary<int, ShopData> m_ShopMap = new Dictionary<int, ShopData>();

	public static Dictionary<int, int> m_ShopRefresh = new Dictionary<int, int>();

	public static ShopData GetShopData(int id)
	{
		return (!m_ShopMap.ContainsKey(id)) ? null : m_ShopMap[id];
	}

	public static ShopData GetShopDataByItemId(int itemId)
	{
		foreach (ShopData value in m_ShopMap.Values)
		{
			if (value.m_ItemID == itemId)
			{
				return value;
			}
		}
		return null;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("store");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			ShopData shopData = new ShopData();
			shopData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("store_id")));
			shopData.m_ItemID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("item_id")));
			shopData.m_Price1 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sale_price")));
			shopData.m_Price2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sale_price2")));
			shopData.m_ExtDemand = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("extra_demand")));
			shopData.m_LimitNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sale_limit")));
			shopData.m_RefreshTime = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("refresh_time")));
			if (shopData.m_LimitNum > 0)
			{
				m_ShopRefresh.Add(shopData.m_ID, shopData.m_RefreshTime);
			}
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PreQuestLimit"));
			string[] array = @string.Split(':');
			if (array.Length == 2)
			{
				shopData.m_LimitType = Convert.ToInt32(array[0]);
				string[] array2 = array[1].Split(',');
				for (int i = 0; i < array2.Length; i++)
				{
					int num = Convert.ToInt32(array2[i]);
					if (num != 0)
					{
						shopData.m_LimitMisIDList.Add(num);
					}
				}
			}
			m_ShopMap.Add(shopData.m_ID, shopData);
		}
	}

	public static int GetPriceBuyItemId(int itemId)
	{
		foreach (int key in m_ShopMap.Keys)
		{
			if (m_ShopMap[key].m_ItemID == itemId)
			{
				return m_ShopMap[key].m_Price;
			}
		}
		return 0;
	}

	public static int GetLimitNum(int id)
	{
		if (!m_ShopMap.ContainsKey(id))
		{
			return -1;
		}
		return m_ShopMap[id].m_LimitNum;
	}

	public static List<int> GetAllIdOfSameItem(int id)
	{
		if (!m_ShopMap.ContainsKey(id))
		{
			return null;
		}
		int itemID = m_ShopMap[id].m_ItemID;
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, ShopData> item in m_ShopMap)
		{
			if (item.Value.m_ItemID == itemID)
			{
				list.Add(item.Key);
			}
		}
		return list;
	}
}
