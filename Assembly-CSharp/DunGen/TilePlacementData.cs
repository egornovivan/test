using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DunGen;

[Serializable]
public sealed class TilePlacementData
{
	public List<Doorway> UsedDoorways = new List<Doorway>();

	public List<Doorway> UnusedDoorways = new List<Doorway>();

	public List<Doorway> AllDoorways = new List<Doorway>();

	[SerializeField]
	private int pathDepth;

	[SerializeField]
	private float normalizedPathDepth;

	[SerializeField]
	private int branchDepth;

	[SerializeField]
	private float normalizedBranchDepth;

	[SerializeField]
	private bool isOnMainPath;

	[SerializeField]
	private Bounds bounds;

	[SerializeField]
	private GameObject root;

	[SerializeField]
	private Tile tile;

	public GameObject Root => root;

	public Tile Tile => tile;

	public int PathDepth
	{
		get
		{
			return pathDepth;
		}
		internal set
		{
			pathDepth = value;
		}
	}

	public float NormalizedPathDepth
	{
		get
		{
			return normalizedPathDepth;
		}
		internal set
		{
			normalizedPathDepth = value;
		}
	}

	public int BranchDepth
	{
		get
		{
			return branchDepth;
		}
		internal set
		{
			branchDepth = value;
		}
	}

	public float NormalizedBranchDepth
	{
		get
		{
			return normalizedBranchDepth;
		}
		internal set
		{
			normalizedBranchDepth = value;
		}
	}

	public bool IsOnMainPath
	{
		get
		{
			return isOnMainPath;
		}
		internal set
		{
			isOnMainPath = value;
		}
	}

	public Bounds Bounds
	{
		get
		{
			return bounds;
		}
		internal set
		{
			bounds = value;
		}
	}

	public int Depth => (!isOnMainPath) ? branchDepth : pathDepth;

	public float NormalizedDepth => (!isOnMainPath) ? normalizedBranchDepth : normalizedPathDepth;

	internal TilePlacementData(PreProcessTileData preProcessData, bool isOnMainPath, DungeonArchetype archetype, TileSet tileSet, Dungeon dungeon)
	{
		root = UnityEngine.Object.Instantiate(preProcessData.Prefab);
		Bounds = preProcessData.Proxy.GetComponent<Collider>().bounds;
		IsOnMainPath = isOnMainPath;
		tile = Root.GetComponent<Tile>();
		if (tile == null)
		{
			tile = Root.AddComponent<Tile>();
		}
		tile.Placement = this;
		tile.Archetype = archetype;
		tile.TileSet = tileSet;
		tile.Dungeon = dungeon;
		Doorway[] componentsInChildren = Root.GetComponentsInChildren<Doorway>(includeInactive: true);
		foreach (Doorway doorway in componentsInChildren)
		{
			doorway.Dungeon = dungeon;
			doorway.Tile = tile;
			AllDoorways.Add(doorway);
		}
		UnusedDoorways.AddRange(AllDoorways);
		root.SetActive(value: false);
	}

	public void ProcessDoorways(System.Random randomStream)
	{
		foreach (Doorway allDoorway in AllDoorways)
		{
			allDoorway.placedByGenerator = true;
			allDoorway.HideConditionalObjects = false;
		}
		foreach (Doorway usedDoorway in UsedDoorways)
		{
			foreach (GameObject item in usedDoorway.AddWhenNotInUse)
			{
				if (item != null)
				{
					UnityUtil.Destroy(item);
				}
			}
		}
		foreach (Doorway unusedDoorway in UnusedDoorways)
		{
			foreach (GameObject item2 in unusedDoorway.AddWhenInUse)
			{
				if (item2 != null)
				{
					UnityUtil.Destroy(item2);
				}
			}
			IEnumerable<GameObject> enumerable = unusedDoorway.BlockerPrefabs.Where((GameObject x) => x != null);
			if (enumerable != null && enumerable.Count() > 0)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(enumerable.ElementAt(randomStream.Next(0, enumerable.Count())));
				gameObject.transform.parent = unusedDoorway.gameObject.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
			}
		}
	}

	public void RecalculateBounds(bool ignoreSpriteRenderers, Vector3 upVector)
	{
		Bounds = UnityUtil.CalculateObjectBounds(Root, includeInactive: true, ignoreSpriteRenderers);
		Bounds = UnityUtil.CondenseBounds(Bounds, AllDoorways);
	}

	public Doorway PickRandomDoorway(System.Random randomStream, bool mustBeAvailable, DungeonArchetype archetype)
	{
		float num = ((!(archetype == null)) ? archetype.StraightenChance : 0f);
		double num2 = randomStream.NextDouble();
		if (isOnMainPath && UsedDoorways.Count == 1 && num2 < (double)num)
		{
			foreach (Doorway unusedDoorway in UnusedDoorways)
			{
				if (UsedDoorways[0].transform.forward == -unusedDoorway.transform.forward)
				{
					return unusedDoorway;
				}
			}
		}
		int num3 = PickRandomDoorwayIndex(randomStream, mustBeAvailable);
		return (num3 != -1) ? AllDoorways[num3] : null;
	}

	public int PickRandomDoorwayIndex(System.Random randomStream, bool mustBeAvailable)
	{
		List<Doorway> list = ((!mustBeAvailable) ? AllDoorways : UnusedDoorways);
		if (list.Count == 0)
		{
			return -1;
		}
		return AllDoorways.IndexOf(list[randomStream.Next(0, list.Count)]);
	}
}
