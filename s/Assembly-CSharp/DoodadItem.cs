using System.Collections.Generic;
using ItemAsset;
using uLink;

public class DoodadItem : IDoodad
{
	public override void Create(MapObjNetwork net, NetworkMessageInfo info)
	{
		base.Create(net, info);
		LoadData();
		if (_hasRecord)
		{
			return;
		}
		_param = info.networkView.initialData.Read<string>(new object[0]);
		string[] array = _param.Split('|');
		if (array.Length != 2)
		{
			return;
		}
		List<ItemObject> list = new List<ItemObject>();
		_itemList = DoodadMgr.DescToItems(array[0], list);
		if (list != null && list.Count > 0)
		{
			foreach (ItemObject item in list)
			{
				if (item.GetCmpt<OwnerData>() != null)
				{
					item.GetCmpt<OwnerData>().npcID = _assetId;
					item.GetCmpt<OwnerData>().npcName = NpcManager.GetNpcNameById(_assetId);
				}
			}
			ChannelNetwork.SyncItemList(net.WorldId, list.ToArray());
		}
		SaveData();
	}

	public override void Insert(BitStream stream, NetworkMessageInfo info)
	{
	}
}
