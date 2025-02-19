using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarPathUpdate : MonoBehaviour
{
	public static AstarPathUpdate instance;

	private List<IntVector4> terrainColliders = new List<IntVector4>();

	private List<IntVector4> caveTerrain = new List<IntVector4>();

	private List<Bounds> b45Bound = new List<Bounds>();

	private List<Bounds> bounds = new List<Bounds>();

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		StartCoroutine(WaitForUpdateBounds());
		StartCoroutine(UpdateAstarPathBlock45());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void Start()
	{
		VFVoxelChunkGo.CreateChunkColliderEvent += OnChunkColliderCreated;
		VFVoxelChunkGo.RebuildChunkColliderEvent += OnChunkColliderRebuild;
		Block45Man.self.AttachEvents(OnBlock45BuildingColliderRebuild);
		if (LODOctreeMan.self != null)
		{
			LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
		}
	}

	private void Update()
	{
		if (!Application.isEditor)
		{
		}
	}

	private void OnDestroy()
	{
		VFVoxelChunkGo.CreateChunkColliderEvent -= OnChunkColliderCreated;
		VFVoxelChunkGo.RebuildChunkColliderEvent -= OnChunkColliderRebuild;
		if (Block45Man.self != null)
		{
			Block45Man.self.DetachEvents(OnBlock45BuildingColliderRebuild);
		}
		if (LODOctreeMan.self != null)
		{
			LODOctreeMan.self.DetachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
		}
	}

	public void UpdateGraphs(Bounds argBounds)
	{
	}

	private void UpdateGraphsFromBounds(Bounds argBounds)
	{
	}

	private void OnBlock45BuildingColliderRebuild(Block45ChunkGo vfGo)
	{
		if (vfGo != null && vfGo._mc != null)
		{
			b45Bound.Add(vfGo._mc.bounds);
		}
	}

	private void OnChunkColliderCreated(VFVoxelChunkGo chunk)
	{
		Vector3 position = chunk.transform.position;
		Vector3 max = chunk.transform.position + Vector3.one * 32f;
		Bounds argBounds = default(Bounds);
		argBounds.SetMinMax(position, max);
		UpdateGraphs(argBounds);
	}

	private void OnChunkColliderRebuild(VFVoxelChunkGo chunk)
	{
		Vector3 position = chunk.transform.position;
		Vector3 max = chunk.transform.position + Vector3.one * 32f;
		Bounds argBounds = default(Bounds);
		argBounds.SetMinMax(position, max);
		UpdateGraphs(argBounds);
	}

	private bool MatchCave(IntVector4 node1, IntVector4 node2)
	{
		IntVector3 intVector = new IntVector3(node1.x, node1.z, node1.w);
		IntVector3 obj = new IntVector3(node2.x, node2.z, node2.w);
		return intVector.Equals(obj);
	}

	private void OnMainPlayerCaveEnter()
	{
		StartCoroutine(UpdateAstarPathGraphs());
	}

	private void OnMainPlayerCaveExit()
	{
		StartCoroutine(UpdateAstarPathGraphs());
	}

	private void OnTerrainColliderCreated(IntVector4 node)
	{
		RegisterTerrainCollider(node);
	}

	private void OnTerrainColliderDestroy(IntVector4 node)
	{
		RemoveTerrainCollider(node);
	}

	private void RegisterTerrainCollider(IntVector4 node)
	{
		if (terrainColliders.Contains(node))
		{
			return;
		}
		IntVector4 intVector = terrainColliders.Find((IntVector4 ret) => MatchCave(ret, node));
		if (intVector != null)
		{
			IntVector4 intVector2 = caveTerrain.Find((IntVector4 ret) => MatchCave(ret, node));
			if (intVector2 == null)
			{
				caveTerrain.Add(intVector);
			}
		}
		terrainColliders.Add(node);
	}

	private void RemoveTerrainCollider(IntVector4 node)
	{
		if (!terrainColliders.Contains(node))
		{
			return;
		}
		terrainColliders.Remove(node);
		IntVector4 intVector = terrainColliders.Find((IntVector4 ret) => MatchCave(ret, node));
		if (intVector != null)
		{
			IntVector4 intVector2 = caveTerrain.Find((IntVector4 ret) => MatchCave(ret, node));
			if (intVector2 != null)
			{
				caveTerrain.Add(intVector2);
			}
		}
	}

	private IEnumerator WaitForUpdateBounds()
	{
		yield break;
	}

	private IEnumerator UpdateAstarPathBlock45()
	{
		while (true)
		{
			if (Block45Man.self != null && !Block45Man.self.isColliderBuilding)
			{
				Bounds graphBound = default(Bounds);
				for (int i = b45Bound.Count - 1; i >= 0; i--)
				{
					if (graphBound.size == Vector3.zero)
					{
						graphBound = b45Bound[i];
					}
					Bounds tmpBound = graphBound;
					tmpBound.Expand(4f);
					if (tmpBound.Intersects(b45Bound[i]))
					{
						graphBound.Encapsulate(b45Bound[i].min);
						graphBound.Encapsulate(b45Bound[i].max);
						b45Bound.RemoveAt(i);
					}
				}
				if (graphBound.size != Vector3.zero)
				{
					graphBound.Expand(2f);
					UpdateGraphs(graphBound);
					bounds.Add(graphBound);
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private IEnumerator UpdateAstarPathGraphs()
	{
		yield break;
	}
}
