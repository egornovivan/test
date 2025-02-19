using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using UnityEngine;

public class ColonyTrade : ColonyBase
{
	public const int MAX_WORKER_COUNT = 4;

	private CSTradeData _MyData;

	public Dictionary<int, List<ItemObject>> _repurchaseItems = new Dictionary<int, List<ItemObject>>();

	public override int MaxWorkerCount => 4;

	public Dictionary<int, Record.stShopData> ShopList => _MyData.mShopList;

	private int ColonyMoney
	{
		get
		{
			return ColonyMgr.GetColonyMoney(base.TeamId);
		}
		set
		{
			ColonyMgr.SetColonyMoney(value, base.TeamId);
		}
	}

	public ColonyTrade(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSTradeData();
		_MyData = (CSTradeData)_RecordData;
		LoadData();
		CreateData();
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public override void MyUpdate()
	{
		if (!IsWorking())
		{
		}
	}

	public override void InitMyData()
	{
		UpdateShopList();
	}

	public new void CreateData()
	{
		UpdateShopList();
	}

	public void UpdateShopList()
	{
		bool flag = false;
		if (ColonyMgr.addedStoreId.ContainsKey(base.TeamId))
		{
			foreach (int item in ColonyMgr.addedStoreId[base.TeamId])
			{
				if (!ShopList.ContainsKey(item))
				{
					ShopList[item] = new Record.stShopData(-1, -1.0);
					flag = true;
				}
			}
			List<int> list = new List<int>();
			foreach (int key in ShopList.Keys)
			{
				if (!ColonyMgr.addedStoreId[base.TeamId].Contains(key))
				{
					list.Add(key);
					flag = true;
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				ShopList.Remove(list[i]);
			}
		}
		if (flag)
		{
			SyncSave();
			RefreshShop();
		}
	}

	public void AddShopList(List<int> removeList, List<int> storeIdList)
	{
		foreach (int remove in removeList)
		{
			if (ShopList.ContainsKey(remove))
			{
				ShopList.Remove(remove);
			}
		}
		foreach (int storeId in storeIdList)
		{
			if (!ShopList.ContainsKey(storeId))
			{
				ShopList[storeId] = new Record.stShopData(-1, -1.0);
			}
		}
		SyncSave();
	}

	public void RefreshShop()
	{
		int[] array = new int[0];
		List<ItemObject> list = ShopManager.RefreshShop(ShopList);
		ChannelNetwork.SyncItemList(_Network.WorldId, list);
		array = list.Select((ItemObject it) => it.instanceId).ToArray();
		GroupNetwork.SyncGroup(_Network, EPacketType.PT_CL_TRD_UpdateBuyItem, array);
	}

	public void RefreshMoney(int money)
	{
		GroupNetwork.SyncGroup(_Network, EPacketType.PT_CL_TRD_UpdateMoney, money);
	}

	public void UpdateShop(Player opPlayer)
	{
		int[] array = new int[0];
		int[] array2 = new int[0];
		List<ItemObject> list = ShopManager.RefreshShop(ShopList);
		opPlayer.SyncItemList(list);
		array = list.Select((ItemObject it) => it.instanceId).ToArray();
		if (_repurchaseItems.ContainsKey(opPlayer.Id))
		{
			array2 = _repurchaseItems[opPlayer.Id].Select((ItemObject iter) => iter.instanceId).ToArray();
		}
		GroupNetwork.SyncGroup(_Network, EPacketType.PT_CL_TRD_UpdateBuyItem, array);
		_Network.RPCPeer(Player.GetPlayerPeer(opPlayer.Id), EPacketType.PT_CL_TRD_UpdateRepurchaseItem, array2);
		_Network.RPCPeer(Player.GetPlayerPeer(opPlayer.Id), EPacketType.PT_CL_TRD_UpdateMoney, ColonyMoney);
	}

	public void BuyItem(int instanceId, int count, Player buyer)
	{
		if (count <= 0)
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(instanceId);
		if (itemByID == null)
		{
			return;
		}
		if (itemByID.stackCount <= 0)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Sold out.");
			}
			buyer.SyncErrorMsg("Sold out.");
			return;
		}
		if (itemByID.stackCount < count)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Count Not Enough out.");
			}
			buyer.SyncErrorMsg("Count not enough.");
			return;
		}
		int num = -1;
		Record.stShopData stShopData = null;
		foreach (KeyValuePair<int, Record.stShopData> mShop in _MyData.mShopList)
		{
			if (mShop.Value.ItemObjID == instanceId)
			{
				num = mShop.Key;
				stShopData = mShop.Value;
			}
		}
		if (stShopData == null || num < 0)
		{
			return;
		}
		if (!buyer.Package.CanAdd(itemByID.protoId, itemByID.stackCount))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Package is full");
			}
			buyer.SyncErrorMsg("Package is full.");
			return;
		}
		ShopData shopData = ShopRespository.GetShopData(num);
		if (shopData == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("item Error");
			}
			buyer.SyncErrorMsg("item Error");
			return;
		}
		int num2 = Mathf.RoundToInt((float)shopData.m_Price * 1.15f) * count;
		if (buyer.GameMoney < num2)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough money.");
			}
			buyer.SyncErrorMsg("Not enough money.");
			return;
		}
		List<ItemObject> effItems = new List<ItemObject>();
		ItemManager.CreateFromItemList(itemByID.protoId, count, itemByID, ref effItems);
		if (effItems.Count >= 1)
		{
			buyer.Package.AddItemList(effItems);
		}
		int num3 = 0;
		if (count < itemByID.stackCount)
		{
			itemByID.CountDown(count);
			ChannelNetwork.SyncItem(_Network.WorldId, itemByID);
			num3 = itemByID.stackCount;
		}
		else
		{
			ItemManager.RemoveItem(itemByID.instanceId);
			stShopData.ItemObjID = -1;
			num3 = -1;
		}
		buyer.SyncItemList(effItems);
		ColonyMoney += num2;
		buyer.SyncPackageIndex();
		buyer.SyncNewItem(new ItemSample[1]
		{
			new ItemSample(itemByID.protoId, count)
		});
		buyer.ReduceMoney(num2);
		SyncSave();
		if (num3 < 0)
		{
			GroupNetwork.SyncGroup(_Network, EPacketType.PT_CL_TRD_BuyItem, instanceId);
		}
		GroupNetwork.SyncGroup(_Network, EPacketType.PT_CL_TRD_UpdateMoney, ColonyMoney);
	}

	public void SellItem(int instanceId, int count, Player seller)
	{
		ItemObject itemById = seller.Package.GetItemById(instanceId);
		if (itemById == null)
		{
			return;
		}
		int instanceId2 = itemById.instanceId;
		if (itemById.stackCount < count)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough count.");
			}
			seller.SyncErrorMsg("Not enough count.");
			return;
		}
		int num = count * itemById.GetSellPrice();
		if (ColonyMoney < num)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Trade Post does not have enough money.");
			}
			seller.SyncErrorMsg("Trade Post does not have enough money.");
			return;
		}
		if (!_repurchaseItems.ContainsKey(seller.Id))
		{
			_repurchaseItems.Add(seller.Id, new List<ItemObject>());
		}
		if (itemById.stackCount <= 1 || itemById.stackCount == count)
		{
			_repurchaseItems[seller.Id].Add(itemById);
			seller.Package.RemoveItem(itemById);
		}
		else
		{
			ItemObject itemObject = ItemManager.CreateFromItem(itemById.protoId, count, itemById);
			_repurchaseItems[seller.Id].Add(itemObject);
			itemById.CountDown(count);
			seller.SyncItemList(new ItemObject[2] { itemObject, itemById });
			instanceId2 = itemObject.instanceId;
		}
		ColonyMoney -= num;
		seller.AddMoney(num);
		seller.SyncPackageIndex();
		SyncSave();
		GroupNetwork.SyncGroup(_Network, EPacketType.PT_CL_TRD_UpdateMoney, ColonyMoney);
		_Network.RPCPeer(Player.GetPlayerPeer(seller.Id), EPacketType.PT_CL_TRD_SellItem, instanceId2);
	}

	public void RepurchaseItem(int instanceId, int count, Player buyer)
	{
		ItemObject itemByID = ItemManager.GetItemByID(instanceId);
		if (itemByID == null || itemByID.protoData == null || !_repurchaseItems.ContainsKey(buyer.Id) || _repurchaseItems.Count((KeyValuePair<int, List<ItemObject>> iter) => iter.Value.Exists((ItemObject o) => o.instanceId == instanceId)) <= 0)
		{
			return;
		}
		int num = itemByID.GetSellPrice() * count;
		if (buyer.GameMoney < num)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough money.");
			}
			buyer.SyncErrorMsg("Not enough money.");
			return;
		}
		if (!buyer.Package.CanAdd(itemByID.protoId, count))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Not enough space.");
			}
			buyer.SyncErrorMsg("Not enough space.");
			return;
		}
		ColonyMoney += num;
		buyer.ReduceMoney(num);
		if (itemByID.MaxStackNum == 1)
		{
			buyer.Package.AddItem(itemByID);
		}
		else
		{
			List<ItemObject> effItems = new List<ItemObject>();
			buyer.Package.AddSameItems(itemByID.protoId, count, ref effItems);
			buyer.SyncItemList(effItems);
		}
		buyer.SyncNewItem(new ItemSample[1]
		{
			new ItemSample(itemByID.protoId, count)
		});
		buyer.SyncPackageIndex();
		bool flag = false;
		if (count == itemByID.stackCount)
		{
			_repurchaseItems[buyer.Id].Remove(itemByID);
			flag = true;
		}
		else
		{
			itemByID.CountDown(count);
			buyer.SyncItem(itemByID);
		}
		SyncSave();
		GroupNetwork.SyncGroup(_Network, EPacketType.PT_CL_TRD_UpdateMoney, ColonyMoney);
		if (flag)
		{
			_Network.RPCPeer(Player.GetPlayerPeer(buyer.Id), EPacketType.PT_CL_TRD_RepurchaseItem, instanceId);
		}
	}

	public byte[] PackData()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		CombomData(writer);
		return memoryStream.ToArray();
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.mShopList.Count);
		foreach (KeyValuePair<int, Record.stShopData> mShop in _MyData.mShopList)
		{
			BufferHelper.Serialize(writer, mShop.Key);
			BufferHelper.Serialize(writer, mShop.Value.ItemObjID);
			BufferHelper.Serialize(writer, mShop.Value.CreateTime);
		}
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		if (ver >= 2016102100)
		{
			int num = BufferHelper.ReadInt32(reader);
			for (int i = 0; i < num; i++)
			{
				int key = BufferHelper.ReadInt32(reader);
				int itemObjId = BufferHelper.ReadInt32(reader);
				double createTime = BufferHelper.ReadDouble(reader);
				ShopList.Add(key, new Record.stShopData(itemObjId, createTime));
			}
		}
	}
}
