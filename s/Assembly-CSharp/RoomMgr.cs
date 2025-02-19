using System.Collections.Generic;
using PETools;

public class RoomMgr
{
	private static Dictionary<int, Room> RoomList = new Dictionary<int, Room>(32);

	public static Room GetRoom(int index)
	{
		return GetRoom(index, bCreateNewIfNotExists: false);
	}

	public static Room GetRoom(int index, bool bCreateNewIfNotExists)
	{
		if (!RoomList.ContainsKey(index))
		{
			if (!bCreateNewIfNotExists)
			{
				return null;
			}
			RoomList[index] = new Room();
		}
		return RoomList[index];
	}

	public static void UpdateBroadcastSet(int index, Player player)
	{
		if (index == player.RoomIndex)
		{
			return;
		}
		GetRoom(player.RoomIndex)?.RemovePlayer(player);
		Room room = GetRoom(index, bCreateNewIfNotExists: true);
		room.AddPlayer(player);
		player.RoomIndex = index;
		List<int> neighborIndex = AreaHelper.GetNeighborIndex(index);
		foreach (int item in neighborIndex)
		{
			if (player.IsDirtyVoxelArea(item))
			{
				player.SendVoxelData(item);
			}
			if (player.IsDirtyBlockArea(item))
			{
				player.SendBlockData(item);
			}
		}
	}
}
