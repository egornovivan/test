using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using uLink;
using UnityEngine;

public class ChannelNetwork : NetInterface
{
	private static List<ChannelNetwork> Channels = new List<ChannelNetwork>();

	public int channelId;

	public static void InitChannel(int channel)
	{
		NetInterface.Instantiate(PrefabManager.Self.ChannelNetwork, Vector3.zero, Quaternion.identity, channel, channel);
	}

	public static void SyncChannel(int channelId, params object[] args)
	{
		ChannelNetwork channelNetwork = Channels.Find((ChannelNetwork iter) => iter.channelId == channelId);
		if (null != channelNetwork)
		{
			channelNetwork.RPCOthers(args);
		}
	}

	public static void SyncChannelPeer(uLink.NetworkPlayer peer, params object[] args)
	{
		ChannelNetwork channelNetwork = Channels.Find((ChannelNetwork iter) => iter.channelId == 101);
		if (null != channelNetwork)
		{
			channelNetwork.RPCPeer(peer, args);
		}
	}

	public static void SyncRoomChannel(params object[] args)
	{
		ChannelNetwork channelNetwork = Channels.Find((ChannelNetwork iter) => iter.channelId == 100);
		if (null != channelNetwork)
		{
			channelNetwork.RPCOthers(args);
		}
	}

	public static void SyncPlayerChannel(params object[] args)
	{
		ChannelNetwork channelNetwork = Channels.Find((ChannelNetwork iter) => iter.channelId == 101);
		if (null != channelNetwork)
		{
			channelNetwork.RPCOthers(args);
		}
	}

	public static void SyncTeamData(int channelId, int teamId, params object[] args)
	{
		ChannelNetwork channelNetwork = Channels.Find((ChannelNetwork iter) => iter.channelId == channelId);
		if (null != channelNetwork)
		{
			GroupNetwork.SyncGroup(channelNetwork, teamId, args);
		}
	}

	public static void SyncTeamData(int teamId, params object[] args)
	{
		ChannelNetwork channelNetwork = Channels.Find((ChannelNetwork iter) => iter.channelId == 101);
		if (null != channelNetwork)
		{
			GroupNetwork.SyncGroup(channelNetwork, teamId, args);
		}
	}

	public static void SyncItemList(int channelId, IEnumerable<ItemObject> items)
	{
		SyncItemList(channelId, items.ToArray());
	}

	public static void SyncItemList(int channelId, ItemObject[] items)
	{
		if (items.Length > 0)
		{
			SyncChannel(channelId, EPacketType.PT_InGame_ItemObjectList, items, false);
		}
	}

	public static void SyncItem(int channelId, ItemObject item)
	{
		SyncChannel(channelId, EPacketType.PT_InGame_ItemObject, item);
	}

	public static void SyncItemList(uLink.NetworkPlayer peer, ItemObject[] items)
	{
		if (items.Length > 0)
		{
			SyncChannelPeer(peer, EPacketType.PT_InGame_ItemObjectList, items, false);
		}
	}

	public static void SyncItem(uLink.NetworkPlayer peer, ItemObject item)
	{
		SyncChannelPeer(peer, EPacketType.PT_InGame_ItemObject, item);
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		channelId = info.networkView.initialData.Read<int>(new object[0]);
		Channels.Add(this);
	}

	protected override void OnPEDestroy()
	{
		Channels.Remove(this);
	}
}
