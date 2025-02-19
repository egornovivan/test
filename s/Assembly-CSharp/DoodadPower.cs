using ItemAsset;
using uLink;
using UnityEngine;

public class DoodadPower : IDoodad
{
	private int _chargePro;

	public override void Create(MapObjNetwork net, uLink.NetworkMessageInfo info)
	{
		base.Create(net, info);
		LoadData();
		for (int i = 0; i < 12; i++)
		{
			if (_itemList.Count <= i)
			{
				_itemList.Add(-1);
			}
		}
		CommonManager._self.RegisterEvent(ChargingItem);
	}

	public override void Insert(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		if (ItemManager.GetItemByID(num) == null || num2 < 0 || _itemList == null || num2 >= _itemList.Count)
		{
			return;
		}
		Player player = Player.GetPlayer(info.sender);
		if (null != player)
		{
			ItemObject itemByID = ItemManager.GetItemByID(_itemList[num2]);
			if (itemByID != null)
			{
				if (player.Package.GetEmptyGridCount(itemByID.protoData) <= 0)
				{
					return;
				}
				player.Package.AddItem(itemByID);
				player.SyncPackageIndex();
				_itemList[num2] = -1;
			}
			player.PutItemIntoMapObj(num, num2, _net);
			ChannelNetwork.SyncItem(_net.WorldId, ItemManager.GetItemByID(num));
			_net.RPCOthers(EPacketType.PT_MO_ModifyItemList, _itemList.ToArray());
		}
		SaveData();
	}

	public override void GetItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (_itemList == null)
		{
			return;
		}
		int itemID = stream.Read<int>(new object[0]);
		int num = _itemList.FindIndex((int iter) => iter == itemID);
		if (num < 0)
		{
			return;
		}
		Player player = Player.GetPlayer(info.sender);
		if (!(null != player) || itemID == 0)
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(itemID);
		if (itemByID != null)
		{
			if (player.Package.GetEmptyGridCount(itemByID.protoData) == 0)
			{
				player.SyncErrorMsg("Package is full.");
				return;
			}
			player.Package.AddItem(itemByID);
			_itemList[num] = -1;
			player.SyncItem(itemByID);
			player.SyncPackageIndex();
			SaveData();
			_net.RPCOthers(EPacketType.PT_MO_RemoveItem, itemID);
			TryToClearItemlist();
		}
	}

	public override void AddToItemlist(int objID, int index)
	{
		if (index >= 0 && _itemList != null && index < _itemList.Count && _itemList[index] == -1)
		{
			_itemList[index] = objID;
		}
	}

	protected void ChargingItem()
	{
		float num = Mathf.Clamp(GameTime.Timer.ElapseSpeed, 0f, 50f);
		_chargePro++;
		foreach (int item in _itemList)
		{
			ItemObject itemByID = ItemManager.GetItemByID(item);
			if (itemByID == null)
			{
				continue;
			}
			if (itemByID.instanceId < 100000000)
			{
				Energy cmpt = itemByID.GetCmpt<Energy>();
				if (cmpt != null)
				{
					cmpt.energy.Change(num * CSPPCoalInfo.ppCoalInfo.m_ChargingRate * 10000f * 1.5f);
					if (_chargePro % 5 == 0 || cmpt.energy.current == cmpt.GetRawMax())
					{
						ChannelNetwork.SyncItem(_net.WorldId, itemByID);
					}
				}
				continue;
			}
			CreationOriginData creationData = SteamWorks.GetCreationData(itemByID.instanceId);
			if (creationData != null && creationData.Fuel < creationData.MaxFuel)
			{
				creationData.Fuel = Mathf.Min(creationData.MaxFuel, Mathf.Abs(creationData.Fuel + CSPPCoalInfo.ppCoalInfo.m_ChargingRate * 10000f * num));
				if (_chargePro % 5 == 0 || creationData.Fuel == creationData.MaxFuel)
				{
					_net.RPCOthers(EPacketType.PT_CL_SyncCreationFuel, item, creationData.Fuel);
				}
			}
		}
	}
}
