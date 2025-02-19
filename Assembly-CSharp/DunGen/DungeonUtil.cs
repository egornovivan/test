using System;
using UnityEngine;

namespace DunGen;

public static class DungeonUtil
{
	public static void Append(Dungeon previousDungeon, Dungeon newDungeon, System.Random randomStream)
	{
		Doorway doorway = previousDungeon.MainPathTiles[previousDungeon.MainPathTiles.Count - 1].Placement.UnusedDoorways[0];
		Doorway doorway2 = newDungeon.MainPathTiles[0].Placement.UnusedDoorways[0];
		UnityUtil.PositionObjectBySocket(previousDungeon.gameObject, doorway.gameObject, doorway2.gameObject);
		newDungeon.MakeConnection(doorway, doorway2, randomStream);
		foreach (GameObject item in doorway.AddWhenNotInUse)
		{
			UnityUtil.Destroy(item);
		}
		foreach (GameObject item2 in doorway2.AddWhenNotInUse)
		{
			UnityUtil.Destroy(item2);
		}
	}

	public static void AddAndSetupDoorComponent(Dungeon dungeon, GameObject doorPrefab, Doorway doorway)
	{
		Door door = doorPrefab.GetComponent<Door>();
		if (door == null)
		{
			door = doorPrefab.AddComponent<Door>();
		}
		door.Dungeon = dungeon;
		door.DoorwayA = doorway;
		door.DoorwayB = doorway.ConnectedDoorway;
		door.TileA = doorway.Tile;
		door.TileB = doorway.ConnectedDoorway.Tile;
	}

	public static void AddLockedDoorToDungeon(Dungeon dungeon, GameObject doorPrefab, Doorway doorway)
	{
		dungeon.LockedDoorList.Add(doorPrefab.GetComponent<LockedDoor>());
	}
}
