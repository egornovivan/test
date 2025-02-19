using System.Collections.Generic;
using ItemAsset;
using uLink;

public class DoodadBox : IDoodad
{
	public override void Create(MapObjNetwork net, NetworkMessageInfo info)
	{
		base.Create(net, info);
		_param = info.networkView.initialData.Read<string>(new object[0]);
		if (_itemList == null)
		{
			_itemList = new List<int>();
		}
		for (int i = 0; i < 120; i++)
		{
			if (_itemList.Count <= i)
			{
				_itemList.Add(-1);
			}
		}
		LoadData();
		if (_hasRecord)
		{
			return;
		}
		List<ItemObject> list = new List<ItemObject>();
		List<int> list2 = DoodadMgr.DescToItems(_param, list);
		if (list != null && list.Count > 0)
		{
			ChannelNetwork.SyncItemList(net.WorldId, list.ToArray());
		}
		if (list2 != null)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				if (_itemList.Count > j)
				{
					_itemList[j] = list2[j];
				}
			}
		}
		SaveData();
	}

	public override void Insert(BitStream stream, NetworkMessageInfo info)
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

	public override void AddToItemlist(int objID, int index)
	{
		if (index >= 0 && _itemList != null && index < _itemList.Count && _itemList[index] == -1)
		{
			_itemList[index] = objID;
		}
	}
}
