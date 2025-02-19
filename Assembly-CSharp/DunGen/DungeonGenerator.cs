using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DunGen.Adapters;
using DunGen.Analysis;
using DunGen.Graph;
using UnityEngine;

namespace DunGen;

[Serializable]
public class DungeonGenerator
{
	public int Seed;

	public bool ShouldRandomizeSeed = true;

	public int MaxAttemptCount = 10;

	public bool IgnoreSpriteBounds = true;

	public Vector3 UpVector = Vector3.up;

	public bool OverrideAllowImmediateRepeats;

	public bool AllowImmediateRepeats;

	public bool OverrideAllowTileRotation;

	public bool AllowTileRotation;

	public bool DebugRender;

	public bool AllowBacktracking = true;

	public float LengthMultiplier = 1f;

	public bool UseLegacyWeightCombineMethod;

	public GameObject Root;

	public DungeonFlow DungeonFlow;

	protected int retryCount;

	protected int roomRetryCount;

	protected Dungeon currentDungeon;

	protected readonly List<PreProcessTileData> preProcessData = new List<PreProcessTileData>();

	protected readonly List<GameObject> useableTiles = new List<GameObject>();

	protected int targetLength;

	protected List<InjectedTile> tilesPendingInjection;

	private int nextNodeIndex;

	private DungeonArchetype currentArchetype;

	private GraphLine previousLineSegment;

	private bool isAnalysis;

	private GameObject lastTilePrefabUsed;

	private Dungeon appendedToDungeon;

	private bool allowAppendedDungeonIntersection;

	private Doorway appendedToDoorway;

	public SerializableType PortalCullingAdapterClass = new SerializableType();

	public PortalCullingAdapter Culling;

	public bool IsPortalCullingEnabled = true;

	public Light DirShadowCaster;

	public float ExtraBounds = 0.01f;

	public bool CullEachChild;

	public System.Random RandomStream { get; protected set; }

	public GenerationStatus Status { get; private set; }

	public GenerationStats GenerationStats { get; private set; }

	public int ChosenSeed { get; protected set; }

	public Dungeon CurrentDungeon => currentDungeon;

	public event GenerationStatusDelegate OnGenerationStatusChanged;

	public event TileInjectionDelegate TileInjectionMethods;

	public DungeonGenerator()
	{
		GenerationStats = new GenerationStats();
	}

	public DungeonGenerator(string flowPath)
	{
		GenerationStats = new GenerationStats();
		DungeonFlow = Resources.Load(flowPath) as DungeonFlow;
		if (DungeonFlow == null)
		{
			UnityEngine.Debug.LogError("flow null: " + flowPath);
		}
	}

	public DungeonGenerator(GameObject root)
		: this()
	{
		Root = root;
	}

	protected bool OuterGenerate(int? seed)
	{
		ShouldRandomizeSeed = !seed.HasValue;
		if (seed.HasValue)
		{
			Seed = seed.Value;
		}
		return Generate();
	}

	protected bool OuterGenerate(int? seed, GameObject manager)
	{
		ShouldRandomizeSeed = !seed.HasValue;
		if (seed.HasValue)
		{
			Seed = seed.Value;
		}
		return Generate(manager);
	}

	public bool GenerateAppended(Dungeon appendTo, bool allowIntersection)
	{
		if (appendTo == currentDungeon)
		{
			DetachDungeon();
		}
		appendedToDungeon = appendTo;
		allowAppendedDungeonIntersection = allowIntersection;
		bool flag = Generate();
		if (flag)
		{
			foreach (GameObject item in appendedToDoorway.AddWhenNotInUse)
			{
				if (item != null)
				{
					UnityUtil.Destroy(item);
				}
			}
		}
		return flag;
	}

	public bool GenerateWithSeed(GameObject manager, int seed)
	{
		isAnalysis = false;
		return OuterGenerate(seed, manager);
	}

	public bool Generate()
	{
		isAnalysis = false;
		return OuterGenerate();
	}

	public bool Generate(GameObject manager)
	{
		isAnalysis = false;
		return OuterGenerate(manager);
	}

	public Dungeon DetachDungeon()
	{
		if (currentDungeon == null)
		{
			return null;
		}
		Dungeon result = currentDungeon;
		currentDungeon = null;
		Root = null;
		Clear();
		return result;
	}

	protected virtual bool OuterGenerate(GameObject manager)
	{
		Clear();
		Status = GenerationStatus.NotStarted;
		DungeonArchetypeValidator dungeonArchetypeValidator = new DungeonArchetypeValidator(DungeonFlow);
		if (!dungeonArchetypeValidator.IsValid())
		{
			ChangeStatus(GenerationStatus.Failed);
			return false;
		}
		ChosenSeed = ((!ShouldRandomizeSeed) ? Seed : new System.Random().Next());
		RandomStream = new System.Random(ChosenSeed);
		if (Root == null)
		{
			Root = new GameObject("Dungeon");
		}
		Root.transform.SetParent(manager.transform);
		Root.transform.position = Root.transform.parent.position;
		bool flag = InnerGenerate(isRetry: false);
		if (!flag)
		{
			Clear();
		}
		return flag;
	}

	protected virtual bool OuterGenerate()
	{
		Clear();
		Status = GenerationStatus.NotStarted;
		DungeonArchetypeValidator dungeonArchetypeValidator = new DungeonArchetypeValidator(DungeonFlow);
		if (!dungeonArchetypeValidator.IsValid())
		{
			ChangeStatus(GenerationStatus.Failed);
			return false;
		}
		ChosenSeed = ((!ShouldRandomizeSeed) ? Seed : new System.Random().Next());
		RandomStream = new System.Random(ChosenSeed);
		if (Root == null)
		{
			Root = new GameObject("Dungeon");
		}
		bool flag = InnerGenerate(isRetry: false);
		if (!flag)
		{
			Clear();
		}
		return flag;
	}

	public GenerationAnalysis RunAnalysis(int iterations, float maximumAnalysisTime)
	{
		DungeonArchetypeValidator dungeonArchetypeValidator = new DungeonArchetypeValidator(DungeonFlow);
		if (Application.isEditor && !dungeonArchetypeValidator.IsValid())
		{
			ChangeStatus(GenerationStatus.Failed);
			return null;
		}
		bool shouldRandomizeSeed = ShouldRandomizeSeed;
		isAnalysis = true;
		ShouldRandomizeSeed = true;
		GenerationAnalysis generationAnalysis = new GenerationAnalysis(iterations);
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < iterations; i++)
		{
			if (maximumAnalysisTime > 0f && stopwatch.Elapsed.TotalMilliseconds >= (double)maximumAnalysisTime)
			{
				break;
			}
			if (OuterGenerate())
			{
				generationAnalysis.IncrementSuccessCount();
				generationAnalysis.Add(GenerationStats);
			}
		}
		Clear();
		generationAnalysis.Analyze();
		ShouldRandomizeSeed = shouldRandomizeSeed;
		return generationAnalysis;
	}

	public void RandomizeSeed()
	{
		Seed = new System.Random().Next();
	}

	protected virtual bool InnerGenerate(bool isRetry)
	{
		if (isRetry)
		{
			if (retryCount >= MaxAttemptCount && Application.isEditor)
			{
				UnityEngine.Debug.LogError($"Failed to generate the dungeon {MaxAttemptCount} times. This could indicate a problem with the way the tiles are set up. Try to make sure most rooms have more than one doorway and that all doorways are easily accessible.");
				ChangeStatus(GenerationStatus.Failed);
				return false;
			}
			retryCount++;
			GenerationStats.IncrementRetryCount();
		}
		else
		{
			retryCount = 0;
			GenerationStats.Clear();
		}
		currentDungeon = Root.GetComponent<Dungeon>();
		if (currentDungeon == null)
		{
			currentDungeon = Root.AddComponent<Dungeon>();
		}
		currentDungeon.DebugRender = DebugRender;
		currentDungeon.PreGenerateDungeon(this);
		Clear();
		targetLength = Mathf.RoundToInt((float)DungeonFlow.Length.GetRandom(RandomStream) * LengthMultiplier);
		targetLength = Mathf.Max(targetLength, 2);
		GenerationStats.BeginTime(GenerationStatus.TileInjection);
		if (tilesPendingInjection == null)
		{
			tilesPendingInjection = new List<InjectedTile>();
		}
		else
		{
			tilesPendingInjection.Clear();
		}
		GatherTilesToInject();
		GenerationStats.BeginTime(GenerationStatus.PreProcessing);
		PreProcess();
		GenerationStats.BeginTime(GenerationStatus.MainPath);
		if (!GenerateMainPath())
		{
			ChosenSeed = RandomStream.Next();
			RandomStream = new System.Random(ChosenSeed);
			return InnerGenerate(isRetry: true);
		}
		GenerationStats.BeginTime(GenerationStatus.Branching);
		GenerateBranchPaths();
		foreach (InjectedTile item in tilesPendingInjection)
		{
			if (item.IsRequired)
			{
				ChosenSeed = RandomStream.Next();
				RandomStream = new System.Random(ChosenSeed);
				return InnerGenerate(isRetry: true);
			}
		}
		GenerationStats.BeginTime(GenerationStatus.PostProcessing);
		PostProcess();
		GenerationStats.EndTime();
		foreach (GameObject door in currentDungeon.Doors)
		{
			if (door != null)
			{
				door.SetActive(value: true);
			}
		}
		CurrentDungeon.StartCoroutine(NotifyGenerationComplete());
		return true;
	}

	private IEnumerator NotifyGenerationComplete()
	{
		yield return null;
		ChangeStatus(GenerationStatus.Complete);
		DungenCharacter[] array = UnityEngine.Object.FindObjectsOfType<DungenCharacter>();
		foreach (DungenCharacter character in array)
		{
			character.ForceRecheckTile();
		}
	}

	public virtual void Clear()
	{
		if (currentDungeon != null)
		{
			currentDungeon.Clear();
		}
		foreach (PreProcessTileData preProcessDatum in preProcessData)
		{
			UnityUtil.Destroy(preProcessDatum.Proxy);
		}
		useableTiles.Clear();
		preProcessData.Clear();
		if (Culling != null)
		{
			Culling.Clear();
		}
	}

	private void ChangeStatus(GenerationStatus status)
	{
		GenerationStatus status2 = Status;
		Status = status;
		if (status2 != status && this.OnGenerationStatusChanged != null)
		{
			this.OnGenerationStatusChanged(this, status);
		}
	}

	protected virtual void PreProcess()
	{
		if (preProcessData.Count > 0)
		{
			return;
		}
		ChangeStatus(GenerationStatus.PreProcessing);
		PreProcessTileData.FindProBuilderObjectType();
		IEnumerable<TileSet> enumerable = DungeonFlow.GetUsedTileSets().Concat(tilesPendingInjection.Select((InjectedTile x) => x.TileSet)).Distinct();
		foreach (TileSet item in enumerable)
		{
			foreach (GameObjectChance weight in item.TileWeights.Weights)
			{
				if (weight.Value != null)
				{
					useableTiles.Add(weight.Value);
					weight.TileSet = item;
				}
			}
		}
		if (Culling == null && PortalCullingAdapterClass.Type != null)
		{
			if (!PortalCullingAdapterClass.Type.IsValidSubtypeOf(typeof(PortalCullingAdapter)))
			{
				UnityEngine.Debug.LogError("[DunGen] Chosen portal culling adapter class (" + PortalCullingAdapterClass.Type.FullName + ") does not derive from PortalCullingAdapter");
			}
			else
			{
				Culling = Activator.CreateInstance(PortalCullingAdapterClass.Type) as PortalCullingAdapter;
			}
		}
	}

	protected virtual void GatherTilesToInject()
	{
		System.Random random = new System.Random(ChosenSeed);
		foreach (TileInjectionRule tileInjectionRule in DungeonFlow.TileInjectionRules)
		{
			if (!(tileInjectionRule.TileSet == null) && (tileInjectionRule.CanAppearOnMainPath || tileInjectionRule.CanAppearOnBranchPath))
			{
				bool isOnMainPath = !tileInjectionRule.CanAppearOnBranchPath || (tileInjectionRule.CanAppearOnMainPath && random.NextDouble() > 0.5);
				InjectedTile item = new InjectedTile(tileInjectionRule.TileSet, isOnMainPath, tileInjectionRule.NormalizedPathDepth.GetRandom(random), tileInjectionRule.NormalizedBranchDepth.GetRandom(random), tileInjectionRule.IsRequired);
				tilesPendingInjection.Add(item);
			}
		}
		if (this.TileInjectionMethods != null)
		{
			this.TileInjectionMethods(random, ref tilesPendingInjection);
		}
	}

	protected virtual bool GenerateMainPath()
	{
		ChangeStatus(GenerationStatus.MainPath);
		nextNodeIndex = 0;
		List<GraphNode> list = new List<GraphNode>(DungeonFlow.Nodes.Count);
		bool flag = false;
		int num = 0;
		List<List<TileSet>> list2 = new List<List<TileSet>>(targetLength);
		List<DungeonArchetype> list3 = new List<DungeonArchetype>(targetLength);
		List<GraphNode> list4 = new List<GraphNode>(targetLength);
		List<GraphLine> list5 = new List<GraphLine>(targetLength);
		while (!flag)
		{
			float num2 = Mathf.Clamp((float)num / (float)(targetLength - 1), 0f, 1f);
			GraphLine lineAtDepth = DungeonFlow.GetLineAtDepth(num2);
			if (lineAtDepth == null)
			{
				return false;
			}
			if (lineAtDepth != previousLineSegment)
			{
				currentArchetype = lineAtDepth.DungeonArchetypes[RandomStream.Next(0, lineAtDepth.DungeonArchetypes.Count)];
				previousLineSegment = lineAtDepth;
			}
			List<TileSet> list6 = null;
			GraphNode graphNode = null;
			GraphNode[] array = DungeonFlow.Nodes.OrderBy((GraphNode x) => x.Position).ToArray();
			GraphNode[] array2 = array;
			foreach (GraphNode graphNode2 in array2)
			{
				if (num2 >= graphNode2.Position && !list.Contains(graphNode2))
				{
					graphNode = graphNode2;
					list.Add(graphNode2);
					break;
				}
			}
			if (graphNode != null)
			{
				list6 = graphNode.TileSets;
				nextNodeIndex = ((nextNodeIndex < array.Length - 1) ? (nextNodeIndex + 1) : (-1));
				list3.Add(null);
				list5.Add(null);
				list4.Add(graphNode);
				if (graphNode == array[array.Length - 1])
				{
					flag = true;
				}
			}
			else
			{
				list6 = currentArchetype.TileSets;
				list3.Add(currentArchetype);
				list5.Add(lineAtDepth);
				list4.Add(null);
			}
			list2.Add(list6);
			num++;
		}
		if (AllowBacktracking)
		{
			int num3 = 0;
			int num4 = 0;
			for (int j = 0; j < list2.Count; j++)
			{
				Tile tile = AddTile((j != 0) ? currentDungeon.MainPathTiles[j - 1] : null, list2[j], (float)j / (float)(list2.Count - 1), list3[j]);
				if (j > 5 && tile == null && num3 < 5 && num4 < 20)
				{
					Tile tile2 = currentDungeon.MainPathTiles[j - 1];
					currentDungeon.RemoveLastConnection();
					currentDungeon.RemoveTile(tile2);
					UnityUtil.Destroy(tile2.gameObject);
					j -= 2;
					num3++;
					num4++;
				}
				else
				{
					if (tile == null)
					{
						return false;
					}
					tile.Node = list4[j];
					tile.Line = list5[j];
					num3 = 0;
				}
			}
		}
		else
		{
			for (int k = 0; k < list2.Count; k++)
			{
				Tile tile3 = AddTile((k != 0) ? currentDungeon.MainPathTiles[k - 1] : null, list2[k], (float)k / (float)(list2.Count - 1), list3[k]);
				if (tile3 == null)
				{
					return false;
				}
				tile3.Node = list4[k];
				tile3.Line = list5[k];
			}
		}
		return true;
	}

	protected virtual void GenerateBranchPaths()
	{
		ChangeStatus(GenerationStatus.Branching);
		foreach (Tile mainPathTile in currentDungeon.MainPathTiles)
		{
			if (mainPathTile.Archetype == null)
			{
				continue;
			}
			int random = mainPathTile.Archetype.BranchCount.GetRandom(RandomStream);
			random = Mathf.Min(random, mainPathTile.Placement.UnusedDoorways.Count);
			if (random == 0)
			{
				continue;
			}
			for (int i = 0; i < random; i++)
			{
				Tile tile = mainPathTile;
				int random2 = mainPathTile.Archetype.BranchingDepth.GetRandom(RandomStream);
				for (int j = 0; j < random2; j++)
				{
					List<TileSet> useableTileSets = ((j != random2 - 1 || !mainPathTile.Archetype.GetHasValidBranchCapTiles()) ? mainPathTile.Archetype.TileSets : ((mainPathTile.Archetype.BranchCapType != 0) ? mainPathTile.Archetype.TileSets.Concat(mainPathTile.Archetype.BranchCapTileSets).ToList() : mainPathTile.Archetype.BranchCapTileSets));
					float num = ((random2 > 1) ? ((float)j / (float)(random2 - 1)) : 1f);
					Tile tile2 = AddTile(tile, useableTileSets, num, mainPathTile.Archetype);
					if (!(tile2 == null))
					{
						tile2.Placement.BranchDepth = j;
						tile2.Placement.NormalizedBranchDepth = num;
						tile2.Node = tile.Node;
						tile2.Line = tile.Line;
						tile = tile2;
					}
				}
			}
		}
	}

	protected virtual Tile AddTile(Tile attachTo, IList<TileSet> useableTileSets, float normalizedDepth, DungeonArchetype archetype, bool isRetry = false)
	{
		if (!isRetry)
		{
			roomRetryCount = 0;
		}
		bool flag = Status == GenerationStatus.MainPath;
		if (attachTo == null && appendedToDungeon != null)
		{
			attachTo = appendedToDungeon.MainPathTiles[appendedToDungeon.MainPathTiles.Count - 1];
		}
		InjectedTile injectedTile = null;
		int index = -1;
		bool flag2 = flag && archetype == null;
		if (tilesPendingInjection != null && !flag2)
		{
			float pathDepth = ((!flag) ? ((float)attachTo.Placement.PathDepth / (float)(targetLength - 1)) : normalizedDepth);
			float branchDepth = ((!flag) ? normalizedDepth : 0f);
			for (int i = 0; i < tilesPendingInjection.Count; i++)
			{
				InjectedTile injectedTile2 = tilesPendingInjection[i];
				if (injectedTile2.ShouldInjectTileAtPoint(flag, pathDepth, branchDepth))
				{
					injectedTile = injectedTile2;
					index = i;
					break;
				}
			}
		}
		Doorway doorway = ((!(attachTo == null)) ? attachTo.Placement.PickRandomDoorway(RandomStream, mustBeAvailable: true, archetype) : null);
		if (attachTo != null && doorway == null)
		{
			return null;
		}
		if (appendedToDungeon != null && appendedToDoorway == null)
		{
			appendedToDoorway = doorway;
		}
		GameObjectChanceTable gameObjectChanceTable = null;
		if (injectedTile != null)
		{
			gameObjectChanceTable = injectedTile.TileSet.TileWeights.Clone();
		}
		else if (UseLegacyWeightCombineMethod)
		{
			TileSet tileSet = useableTileSets[RandomStream.Next(0, useableTileSets.Count)];
			gameObjectChanceTable = tileSet.TileWeights.Clone();
		}
		else
		{
			GameObjectChanceTable[] tables = useableTileSets.Select((TileSet x) => x.TileWeights).ToArray();
			gameObjectChanceTable = GameObjectChanceTable.Combine(tables);
		}
		if (attachTo != null)
		{
			for (int num = gameObjectChanceTable.Weights.Count - 1; num >= 0; num--)
			{
				GameObjectChance gameObjectChance = gameObjectChanceTable.Weights[num];
				PreProcessTileData tileTemplate = GetTileTemplate(gameObjectChance.Value);
				if (tileTemplate == null || !tileTemplate.DoorwaySockets.Contains(doorway.SocketGroup))
				{
					gameObjectChanceTable.Weights.RemoveAt(num);
				}
			}
		}
		if (gameObjectChanceTable.Weights.Count == 0)
		{
			roomRetryCount++;
			if (roomRetryCount > MaxAttemptCount)
			{
				return null;
			}
			return AddTile(attachTo, useableTileSets, normalizedDepth, archetype, isRetry: true);
		}
		bool allowImmediateRepeats = !(attachTo != null) || attachTo.AllowImmediateRepeats;
		if (OverrideAllowImmediateRepeats)
		{
			allowImmediateRepeats = AllowImmediateRepeats;
		}
		GameObjectChance random = gameObjectChanceTable.GetRandom(RandomStream, flag, normalizedDepth, lastTilePrefabUsed, allowImmediateRepeats);
		if (random == null)
		{
			return null;
		}
		GameObject value = random.Value;
		PreProcessTileData tileTemplate2 = GetTileTemplate(value);
		if (tileTemplate2 == null)
		{
			return null;
		}
		int doorwayIndex = 0;
		Doorway doorway2 = null;
		if (doorway != null)
		{
			Tile component = tileTemplate2.Prefab.GetComponent<Tile>();
			bool flag3 = component == null || component.AllowRotation;
			if (OverrideAllowTileRotation)
			{
				flag3 = AllowTileRotation;
			}
			if (!tileTemplate2.ChooseRandomDoorway(allowedDirection: (!(component == null) && !flag3) ? new Vector3?(-doorway.transform.forward) : ((doorway.transform.forward == UpVector) ? new Vector3?(-UpVector) : ((!(doorway.transform.forward == -UpVector)) ? ((Vector3?)null) : new Vector3?(UpVector))), random: RandomStream, socketGroupFilter: doorway.SocketGroup, doorwayIndex: out doorwayIndex, doorway: out doorway2))
			{
				return null;
			}
			GameObject socketA = tileTemplate2.ProxySockets[doorwayIndex];
			UnityUtil.PositionObjectBySocket(tileTemplate2.Proxy, socketA, doorway.gameObject);
			if (IsCollidingWithAnyTile(tileTemplate2.Proxy))
			{
				roomRetryCount++;
				if (roomRetryCount > MaxAttemptCount)
				{
					return null;
				}
				return AddTile(attachTo, useableTileSets, normalizedDepth, archetype, isRetry: true);
			}
		}
		TilePlacementData tilePlacementData = new TilePlacementData(tileTemplate2, Status == GenerationStatus.MainPath, archetype, random.TileSet, currentDungeon);
		if (tilePlacementData == null)
		{
			return null;
		}
		if (tilePlacementData.IsOnMainPath)
		{
			if (attachTo != null)
			{
				tilePlacementData.PathDepth = attachTo.Placement.PathDepth + 1;
			}
		}
		else
		{
			tilePlacementData.PathDepth = attachTo.Placement.PathDepth;
			tilePlacementData.BranchDepth = ((!attachTo.Placement.IsOnMainPath) ? (attachTo.Placement.BranchDepth + 1) : 0);
		}
		if (doorway != null)
		{
			if (!Application.isPlaying)
			{
				tilePlacementData.Root.SetActive(value: false);
			}
			tilePlacementData.Root.transform.parent = Root.transform;
			doorway2 = tilePlacementData.AllDoorways[doorwayIndex];
			UnityUtil.PositionObjectBySocket(tilePlacementData.Root, doorway2.gameObject, doorway.gameObject);
			if (!Application.isPlaying)
			{
				tilePlacementData.Root.SetActive(value: true);
			}
			currentDungeon.MakeConnection(doorway, doorway2, RandomStream);
		}
		else
		{
			tilePlacementData.Root.transform.parent = Root.transform;
			tilePlacementData.Root.transform.localPosition = Vector3.zero;
		}
		if (injectedTile != null)
		{
			tilesPendingInjection.RemoveAt(index);
			if (flag)
			{
				targetLength++;
			}
		}
		currentDungeon.AddTile(tilePlacementData.Tile);
		tilePlacementData.RecalculateBounds(IgnoreSpriteBounds, UpVector);
		lastTilePrefabUsed = value;
		tilePlacementData.Tile.AddTriggerVolume();
		return tilePlacementData.Tile;
	}

	protected PreProcessTileData GetTileTemplate(GameObject prefab)
	{
		PreProcessTileData preProcessTileData = preProcessData.Where((PreProcessTileData x) => x.Prefab == prefab).FirstOrDefault();
		if (preProcessTileData == null)
		{
			preProcessTileData = new PreProcessTileData(prefab, IgnoreSpriteBounds, UpVector);
			preProcessData.Add(preProcessTileData);
		}
		return preProcessTileData;
	}

	protected PreProcessTileData PickRandomTemplate(DoorwaySocketType? socketGroupFilter)
	{
		GameObject prefab = useableTiles[RandomStream.Next(0, useableTiles.Count)];
		PreProcessTileData tileTemplate = GetTileTemplate(prefab);
		if (socketGroupFilter.HasValue && !tileTemplate.DoorwaySockets.Contains(socketGroupFilter.Value))
		{
			return PickRandomTemplate(socketGroupFilter);
		}
		return tileTemplate;
	}

	protected int NormalizedDepthToIndex(float normalizedDepth)
	{
		return Mathf.RoundToInt(normalizedDepth * (float)(targetLength - 1));
	}

	protected float IndexToNormalizedDepth(int index)
	{
		return (float)index / (float)targetLength;
	}

	protected bool IsCollidingWithAnyTile(GameObject proxy)
	{
		foreach (Tile allTile in currentDungeon.AllTiles)
		{
			if (allTile.Placement.Bounds.Intersects(proxy.GetComponent<Collider>().bounds))
			{
				return true;
			}
		}
		if (appendedToDungeon != null && !allowAppendedDungeonIntersection)
		{
			foreach (Tile allTile2 in appendedToDungeon.AllTiles)
			{
				if (allTile2.Placement.Bounds.Intersects(proxy.GetComponent<Collider>().bounds))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected void ClearPreProcessData()
	{
		foreach (PreProcessTileData preProcessDatum in preProcessData)
		{
			UnityUtil.Destroy(preProcessDatum.Proxy);
		}
		preProcessData.Clear();
	}

	protected virtual void ConnectOverlappingDoorways(float percentageChance)
	{
		if (percentageChance <= 0f)
		{
			return;
		}
		Doorway[] componentsInChildren = Root.GetComponentsInChildren<Doorway>();
		List<Doorway> list = new List<Doorway>(componentsInChildren.Length);
		Doorway[] array = componentsInChildren;
		foreach (Doorway doorway in array)
		{
			Doorway[] array2 = componentsInChildren;
			foreach (Doorway doorway2 in array2)
			{
				if (!(doorway == doorway2) && !(doorway.Tile == doorway2.Tile) && doorway.SocketGroup == doorway2.SocketGroup && !list.Contains(doorway2))
				{
					float sqrMagnitude = (doorway.transform.position - doorway2.transform.position).sqrMagnitude;
					if (sqrMagnitude < 1E-05f && RandomStream.NextDouble() < (double)percentageChance)
					{
						currentDungeon.MakeConnection(doorway, doorway2, RandomStream);
					}
				}
			}
			list.Add(doorway);
		}
	}

	protected virtual void PostProcess()
	{
		ChangeStatus(GenerationStatus.PostProcessing);
		foreach (Tile allTile in currentDungeon.AllTiles)
		{
			allTile.gameObject.SetActive(value: true);
		}
		int count = currentDungeon.MainPathTiles.Count;
		int maxBranchDepth = 0;
		if (currentDungeon.BranchPathTiles.Count > 0)
		{
			List<Tile> list = currentDungeon.BranchPathTiles.ToList();
			list.Sort((Tile a, Tile b) => b.Placement.BranchDepth.CompareTo(a.Placement.BranchDepth));
			maxBranchDepth = list[0].Placement.BranchDepth;
		}
		if (!isAnalysis)
		{
			ConnectOverlappingDoorways(DungeonFlow.DoorwayConnectionChance);
			foreach (Tile allTile2 in currentDungeon.AllTiles)
			{
				allTile2.Placement.NormalizedPathDepth = (float)allTile2.Placement.PathDepth / (float)(count - 1);
				allTile2.Placement.ProcessDoorways(RandomStream);
			}
			currentDungeon.PostGenerateDungeon(this);
			if (DungeonFlow.KeyManager != null)
			{
				PlaceLocksAndKeys();
			}
			foreach (Tile allTile3 in currentDungeon.AllTiles)
			{
				RandomProp[] componentsInChildren = allTile3.GetComponentsInChildren<RandomProp>();
				foreach (RandomProp randomProp in componentsInChildren)
				{
					randomProp.Process(RandomStream, allTile3);
				}
			}
			ProcessGlobalProps();
		}
		GenerationStats.SetRoomStatistics(currentDungeon.MainPathTiles.Count, currentDungeon.BranchPathTiles.Count, maxBranchDepth);
		ClearPreProcessData();
		if (Culling != null && IsPortalCullingEnabled && !isAnalysis)
		{
			Culling.Clear();
			Culling.PrepareForCulling(this, currentDungeon);
			CurrentDungeon.Culling = Culling.Clone();
		}
		Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
		foreach (Door door in array)
		{
			door.IsOpen = door.IsOpen;
		}
	}

	protected virtual void ProcessGlobalProps()
	{
		Dictionary<int, GameObjectChanceTable> dictionary = new Dictionary<int, GameObjectChanceTable>();
		foreach (Tile allTile in currentDungeon.AllTiles)
		{
			GlobalProp[] componentsInChildren = allTile.GetComponentsInChildren<GlobalProp>();
			foreach (GlobalProp globalProp in componentsInChildren)
			{
				GameObjectChanceTable value = null;
				if (!dictionary.TryGetValue(globalProp.PropGroupID, out value))
				{
					value = new GameObjectChanceTable();
					dictionary[globalProp.PropGroupID] = value;
				}
				float num = ((!allTile.Placement.IsOnMainPath) ? globalProp.BranchPathWeight : globalProp.MainPathWeight);
				num *= globalProp.DepthWeightScale.Evaluate(allTile.Placement.NormalizedDepth);
				value.Weights.Add(new GameObjectChance(globalProp.gameObject, num, 0f, null));
			}
		}
		IEnumerable<GameObject> enumerable = dictionary.SelectMany((KeyValuePair<int, GameObjectChanceTable> x) => x.Value.Weights.Select((GameObjectChance y) => y.Value));
		foreach (GameObject item in enumerable)
		{
			item.SetActive(value: false);
		}
		List<int> list = new List<int>(dictionary.Count);
		foreach (KeyValuePair<int, GameObjectChanceTable> item2 in dictionary)
		{
			if (list.Contains(item2.Key))
			{
				UnityEngine.Debug.LogWarning("Dungeon Flow contains multiple entries for the global prop group ID: " + item2.Key + ". Only the first entry will be used.");
				continue;
			}
			int num2 = DungeonFlow.GlobalPropGroupIDs.IndexOf(item2.Key);
			if (num2 == -1)
			{
				continue;
			}
			IntRange intRange = DungeonFlow.GlobalPropRanges[num2];
			GameObjectChanceTable gameObjectChanceTable = item2.Value.Clone();
			int random = intRange.GetRandom(RandomStream);
			random = Mathf.Clamp(random, 0, gameObjectChanceTable.Weights.Count);
			for (int j = 0; j < random; j++)
			{
				GameObjectChance random2 = gameObjectChanceTable.GetRandom(RandomStream, isOnMainPath: true, 0f, null, allowImmediateRepeats: true, removeFromTable: true);
				if (random2 != null && random2.Value != null)
				{
					random2.Value.SetActive(value: true);
				}
			}
			list.Add(item2.Key);
		}
	}

	protected virtual void PlaceLocksAndKeys()
	{
		GraphNode[] array = (from x in currentDungeon.ConnectionGraph.Nodes
			select x.Tile.Node into x
			where x != null
			select x).Distinct().ToArray();
		GraphLine[] array2 = (from x in currentDungeon.ConnectionGraph.Nodes
			select x.Tile.Line into x
			where x != null
			select x).Distinct().ToArray();
		Dictionary<Doorway, Key> lockedDoorways = new Dictionary<Doorway, Key>();
		GraphNode[] array3 = array;
		foreach (GraphNode node in array3)
		{
			foreach (KeyLockPlacement @lock in node.Locks)
			{
				Tile tile = currentDungeon.AllTiles.Where((Tile x) => x.Node == node).FirstOrDefault();
				List<DungeonGraphConnection> connections = currentDungeon.ConnectionGraph.Nodes.Where((DungeonGraphNode x) => x.Tile == tile).FirstOrDefault().Connections;
				Doorway doorway = null;
				Doorway doorway2 = null;
				foreach (DungeonGraphConnection item in connections)
				{
					if (item.DoorwayA.Tile == tile)
					{
						doorway2 = item.DoorwayA;
					}
					else if (item.DoorwayB.Tile == tile)
					{
						doorway = item.DoorwayB;
					}
				}
				Key keyByID = node.Graph.KeyManager.GetKeyByID(@lock.ID);
				if (keyByID.Prefab != null)
				{
					if (doorway != null && (node.LockPlacement & NodeLockPlacement.Entrance) == NodeLockPlacement.Entrance)
					{
						lockedDoorways.Add(doorway, keyByID);
					}
					if (doorway2 != null && (node.LockPlacement & NodeLockPlacement.Exit) == NodeLockPlacement.Exit)
					{
						lockedDoorways.Add(doorway2, keyByID);
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Key with ID " + @lock.ID + " does not have a prefab to place");
				}
			}
		}
		GraphLine[] array4 = array2;
		foreach (GraphLine line in array4)
		{
			List<Doorway> list = (from x in currentDungeon.ConnectionGraph.Connections.Where(delegate(DungeonGraphConnection x)
				{
					bool flag = lockedDoorways.ContainsKey(x.DoorwayA) || lockedDoorways.ContainsKey(x.DoorwayB);
					bool flag2 = x.DoorwayA.Tile.TileSet.LockPrefabs.Count > 0;
					return x.DoorwayA.Tile.Line == line && x.DoorwayB.Tile.Line == line && !flag && flag2;
				})
				select x.DoorwayA).ToList();
			if (list.Count == 0)
			{
				continue;
			}
			foreach (KeyLockPlacement lock2 in line.Locks)
			{
				int random = lock2.Range.GetRandom(RandomStream);
				random = Mathf.Clamp(random, 0, list.Count);
				for (int k = 0; k < random; k++)
				{
					Doorway doorway3 = list[RandomStream.Next(0, list.Count)];
					list.Remove(doorway3);
					Key keyByID2 = line.Graph.KeyManager.GetKeyByID(lock2.ID);
					lockedDoorways.Add(doorway3, keyByID2);
				}
			}
		}
		List<Doorway> list2 = new List<Doorway>();
		foreach (KeyValuePair<Doorway, Key> item2 in lockedDoorways)
		{
			Doorway key = item2.Key;
			Key key2 = item2.Value;
			List<Tile> list3 = new List<Tile>();
			foreach (Tile allTile in currentDungeon.AllTiles)
			{
				if (!(allTile.Placement.NormalizedPathDepth >= key.Tile.Placement.NormalizedPathDepth))
				{
					bool flag3 = false;
					if (allTile.Node != null && allTile.Node.Keys.Where((KeyLockPlacement x) => x.ID == key2.ID).Count() > 0)
					{
						flag3 = true;
					}
					else if (allTile.Line != null && allTile.Line.Keys.Where((KeyLockPlacement x) => x.ID == key2.ID).Count() > 0)
					{
						flag3 = true;
					}
					if (flag3)
					{
						list3.Add(allTile);
					}
				}
			}
			List<IKeySpawnable> list4 = list3.SelectMany((Tile x) => x.GetComponentsInChildren<Component>().OfType<IKeySpawnable>()).ToList();
			if (list4.Count == 0)
			{
				continue;
			}
			int random2 = key2.KeysPerLock.GetRandom(RandomStream);
			random2 = Math.Min(random2, list4.Count);
			for (int l = 0; l < random2; l++)
			{
				int index = RandomStream.Next(0, list4.Count);
				IKeySpawnable keySpawnable = list4[index];
				keySpawnable.SpawnKey(key2, DungeonFlow.KeyManager);
				foreach (IKeyLock item3 in (keySpawnable as Component).GetComponentsInChildren<Component>().OfType<IKeyLock>())
				{
					item3.OnKeyAssigned(key2, DungeonFlow.KeyManager);
				}
				list4.RemoveAt(index);
			}
		}
		foreach (Doorway item4 in list2)
		{
			lockedDoorways.Remove(item4);
		}
		foreach (KeyValuePair<Doorway, Key> item5 in lockedDoorways)
		{
			item5.Key.RemoveUsedPrefab();
			LockDoorway(item5.Key, item5.Value, DungeonFlow.KeyManager);
		}
	}

	protected virtual void LockDoorway(Doorway doorway, Key key, KeyManager keyManager)
	{
		TilePlacementData placement = doorway.Tile.Placement;
		GameObjectChanceTable[] array = (from x in doorway.Tile.TileSet.LockPrefabs
			where x.SocketGroup == doorway.SocketGroup
			select x.LockPrefabs).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		GameObjectChance random = array[RandomStream.Next(0, array.Length)].GetRandom(RandomStream, placement.IsOnMainPath, placement.NormalizedDepth, null, allowImmediateRepeats: true);
		GameObject value = random.Value;
		GameObject gameObject = UnityEngine.Object.Instantiate(value);
		gameObject.transform.parent = Root.transform;
		gameObject.transform.position = doorway.transform.position;
		gameObject.transform.rotation = doorway.transform.rotation;
		doorway.SetUsedPrefab(gameObject);
		doorway.ConnectedDoorway.SetUsedPrefab(gameObject);
		DungeonUtil.AddLockedDoorToDungeon(CurrentDungeon, gameObject, doorway);
		DungeonUtil.AddAndSetupDoorComponent(CurrentDungeon, gameObject, doorway);
		foreach (IKeyLock item in gameObject.GetComponentsInChildren<Component>().OfType<IKeyLock>())
		{
			item.OnKeyAssigned(key, keyManager);
		}
	}
}
