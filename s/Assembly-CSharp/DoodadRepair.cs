using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using uLink;
using UnityEngine;

public class DoodadRepair : IDoodad
{
	private float _totalTime;

	private float _curTime;

	public override void Create(MapObjNetwork net, uLink.NetworkMessageInfo info)
	{
		base.Create(net, info);
		LoadData();
	}

	public override void Insert(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		if (array == null || array.Length <= 0)
		{
			return;
		}
		Player player = Player.GetPlayer(info.sender);
		if (null != player)
		{
			if (_itemList.Count != 0)
			{
				ItemObject itemByID = ItemManager.GetItemByID(_itemList[0]);
				if (itemByID != null)
				{
					if (player.Package.GetEmptyGridCount(itemByID.protoData) <= 0)
					{
						return;
					}
					player.Package.AddItem(itemByID);
					player.SyncPackageIndex();
					_itemList.Clear();
				}
			}
			ItemObject[] items = player.PutItemIntoMapObj(array, _net).ToArray();
			ChannelNetwork.SyncItemList(_net.WorldId, items);
			_net.RPCOthers(EPacketType.PT_MO_ModifyItemList, _itemList.ToArray());
		}
		SaveData();
	}

	public Dictionary<int, int> GetCostsItems(ItemObject itemObj)
	{
		Repair cmpt = itemObj.GetCmpt<Repair>();
		if (cmpt == null || cmpt.GetValue().IsCurrentMax())
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (MaterialItem repairMaterial in cmpt.protoData.repairMaterialList)
		{
			dictionary[repairMaterial.protoId] = Mathf.CeilToInt((float)repairMaterial.count * (1f - cmpt.GetValue().percent));
		}
		return dictionary;
	}

	public void StartRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objectId = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (player == null)
		{
			return;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objectId);
		if (itemByID == null)
		{
			return;
		}
		Dictionary<int, int> costsItems = GetCostsItems(itemByID);
		if (costsItems == null || costsItems.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, int> item in costsItems)
		{
			if (player.GetItemNum(item.Key) < item.Value)
			{
				return;
			}
		}
		IEnumerable<ItemSample> items = costsItems.Select((KeyValuePair<int, int> iter) => new ItemSample(iter.Key, iter.Value));
		ItemObject[] items2 = player.Package.RemoveItem(items);
		ChannelNetwork.SyncItemList(_net.WorldId, items2);
		player.SyncPackageIndex();
		_totalTime = CSRepairInfo.m_BaseTime * 1f;
		_curTime = 0f;
		CommonManager._self.RegisterEvent(Repair);
		SyncRepairTime();
	}

	public void StopRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		_curTime = 0f;
		_totalTime = 0f;
		_net.RPCOthers(EPacketType.PT_MO_StopRepair, num);
	}

	public void SyncRepairTime()
	{
		_net.RPCOthers(EPacketType.PT_MO_SyncRepairTime, _curTime, _totalTime);
	}

	private void Repair()
	{
		if (_totalTime == 0f)
		{
			return;
		}
		_curTime += 2f;
		if (!(_curTime >= _totalTime))
		{
			return;
		}
		if (_itemList.Count != 0)
		{
			ItemObject itemByID = ItemManager.GetItemByID(_itemList[0]);
			if (itemByID.instanceId >= 100000000)
			{
				CreationOriginData creationData = SteamWorks.GetCreationData(itemByID.instanceId);
				creationData.HP = creationData.MaxHP;
				_net.RPCOthers(EPacketType.PT_CL_SyncCreationHP, itemByID.instanceId, creationData.HP);
			}
			else
			{
				itemByID.GetCmpt<Repair>().Do();
				ChannelNetwork.SyncItem(_net.WorldId, itemByID);
			}
			_net.RPCOthers(EPacketType.PT_MO_StartRepair);
		}
		_curTime = 0f;
		_totalTime = 0f;
	}
}
