using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DunGen.Adapters;
using DunGen.Graph;
using UnityEngine;

namespace DunGen;

public class Dungeon : MonoBehaviour
{
	public bool DebugRender;

	public PortalCullingAdapter Culling;

	public List<LockedDoor> LockedDoorList = new List<LockedDoor>();

	private readonly List<Tile> allTiles = new List<Tile>();

	private readonly List<Tile> mainPathTiles = new List<Tile>();

	private readonly List<Tile> branchPathTiles = new List<Tile>();

	private readonly List<GameObject> doors = new List<GameObject>();

	private readonly List<DoorwayConnection> connections = new List<DoorwayConnection>();

	public DungeonFlow DungeonFlow { get; protected set; }

	public ReadOnlyCollection<Tile> AllTiles { get; private set; }

	public ReadOnlyCollection<Tile> MainPathTiles { get; private set; }

	public ReadOnlyCollection<Tile> BranchPathTiles { get; private set; }

	public ReadOnlyCollection<GameObject> Doors { get; private set; }

	public ReadOnlyCollection<DoorwayConnection> Connections { get; private set; }

	public DungeonGraph ConnectionGraph { get; private set; }

	public Dungeon()
	{
		AllTiles = new ReadOnlyCollection<Tile>(allTiles);
		MainPathTiles = new ReadOnlyCollection<Tile>(mainPathTiles);
		BranchPathTiles = new ReadOnlyCollection<Tile>(branchPathTiles);
		Doors = new ReadOnlyCollection<GameObject>(doors);
		Connections = new ReadOnlyCollection<DoorwayConnection>(connections);
	}

	internal void PreGenerateDungeon(DungeonGenerator dungeonGenerator)
	{
		DungeonFlow = dungeonGenerator.DungeonFlow;
	}

	internal void PostGenerateDungeon(DungeonGenerator dungeonGenerator)
	{
		ConnectionGraph = new DungeonGraph(this);
	}

	public void Clear()
	{
		foreach (Tile allTile in allTiles)
		{
			UnityUtil.Destroy(allTile.gameObject);
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameObject obj = base.transform.GetChild(i).gameObject;
			UnityUtil.Destroy(obj);
		}
		allTiles.Clear();
		mainPathTiles.Clear();
		branchPathTiles.Clear();
		doors.Clear();
		connections.Clear();
	}

	public Doorway GetConnection(Doorway doorway)
	{
		foreach (DoorwayConnection connection in connections)
		{
			if (connection.A == doorway)
			{
				return connection.B;
			}
			if (connection.B == doorway)
			{
				return connection.A;
			}
		}
		return null;
	}

	internal void MakeConnection(Doorway a, Doorway b, System.Random randomStream)
	{
		bool flag = a.Dungeon != b.Dungeon;
		a.Tile.Placement.UnusedDoorways.Remove(a);
		a.Tile.Placement.UsedDoorways.Add(a);
		b.Tile.Placement.UnusedDoorways.Remove(b);
		b.Tile.Placement.UsedDoorways.Add(b);
		a.ConnectedDoorway = b;
		b.ConnectedDoorway = a;
		if (!flag)
		{
			DoorwayConnection item = new DoorwayConnection(a, b);
			connections.Add(item);
		}
		List<GameObject> list = ((a.DoorPrefabs.Count <= 0) ? b.DoorPrefabs : a.DoorPrefabs);
		if (list.Count > 0 && !a.HasDoorPrefab && !b.HasDoorPrefab)
		{
			GameObject gameObject = list[randomStream.Next(0, list.Count)];
			if (gameObject != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
				gameObject2.transform.parent = base.gameObject.transform;
				gameObject2.transform.position = a.transform.position;
				gameObject2.transform.rotation = a.transform.rotation;
				gameObject2.transform.localScale = a.transform.localScale;
				doors.Add(gameObject2);
				a.SetUsedPrefab(gameObject2);
				b.SetUsedPrefab(gameObject2);
				DungeonUtil.AddAndSetupDoorComponent(this, gameObject2, a);
			}
		}
	}

	internal void AddTile(Tile tile)
	{
		allTiles.Add(tile);
		if (tile.Placement.IsOnMainPath)
		{
			mainPathTiles.Add(tile);
		}
		else
		{
			branchPathTiles.Add(tile);
		}
	}

	internal void RemoveTile(Tile tile)
	{
		allTiles.Remove(tile);
		if (tile.Placement.IsOnMainPath)
		{
			mainPathTiles.Remove(tile);
		}
		else
		{
			branchPathTiles.Remove(tile);
		}
	}

	internal void RemoveLastConnection()
	{
		DoorwayConnection doorwayConnection = connections.Last();
		Doorway a = doorwayConnection.A;
		Doorway b = doorwayConnection.B;
		a.Tile.Placement.UnusedDoorways.Add(a);
		a.Tile.Placement.UsedDoorways.Remove(a);
		b.Tile.Placement.UnusedDoorways.Add(b);
		b.Tile.Placement.UsedDoorways.Remove(b);
		a.ConnectedDoorway = null;
		b.ConnectedDoorway = null;
		if (a.HasDoorPrefab)
		{
			doors.Remove(a.UsedDoorPrefab);
			a.RemoveUsedPrefab();
		}
		if (b.HasDoorPrefab)
		{
			doors.Remove(b.UsedDoorPrefab);
			b.RemoveUsedPrefab();
		}
		connections.Remove(doorwayConnection);
	}

	public void OnDrawGizmos()
	{
		if (DebugRender)
		{
			DebugDraw();
		}
	}

	public void DebugDraw()
	{
		Color red = Color.red;
		Color green = Color.green;
		Color blue = Color.blue;
		Color b = new Color(0.5f, 0f, 0.5f);
		float a = 0.75f;
		foreach (Tile allTile in allTiles)
		{
			Bounds bounds = allTile.Placement.Bounds;
			bounds.size *= 1.01f;
			Color color = ((!allTile.Placement.IsOnMainPath) ? Color.Lerp(blue, b, allTile.Placement.NormalizedDepth) : Color.Lerp(red, green, allTile.Placement.NormalizedDepth));
			color.a = a;
			Gizmos.color = color;
			Gizmos.DrawCube(bounds.center, bounds.size);
		}
	}
}
