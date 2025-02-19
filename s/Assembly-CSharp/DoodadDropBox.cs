using System.Linq;
using ItemAsset;
using uLink;

public class DoodadDropBox : IDoodad
{
	public override void Create(MapObjNetwork net, NetworkMessageInfo info)
	{
		base.Create(net, info);
		_net.DestroyByTimeOut();
	}

	public override void Insert(BitStream stream, NetworkMessageInfo info)
	{
		_itemList.Clear();
		int[] array = stream.Read<int[]>(new object[0]);
		if (array != null && array.Length > 0)
		{
			Player player = Player.GetPlayer(info.sender);
			if (null != player)
			{
				ItemObject[] items = player.PutItemIntoMapObj(array, _net).ToArray();
				ChannelNetwork.SyncItemList(_net.WorldId, items);
			}
		}
	}
}
