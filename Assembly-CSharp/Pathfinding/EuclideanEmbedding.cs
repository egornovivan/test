using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public class EuclideanEmbedding
{
	public HeuristicOptimizationMode mode;

	public int seed;

	public Transform pivotPointRoot;

	public int spreadOutCount = 1;

	private uint[] costs = new uint[8];

	private int maxNodeIndex;

	private int pivotCount;

	[NonSerialized]
	public bool dirty;

	private GraphNode[] pivots;

	private uint ra = 12820163u;

	private uint rc = 1140671485u;

	private uint rval;

	private object lockObj = new object();

	public uint GetRandom()
	{
		rval = ra * rval + rc;
		return rval;
	}

	private void EnsureCapacity(int index)
	{
		if (index <= maxNodeIndex)
		{
			return;
		}
		lock (lockObj)
		{
			if (index <= maxNodeIndex)
			{
				return;
			}
			if (index >= costs.Length)
			{
				uint[] array = new uint[Math.Max(index * 2, pivots.Length * 2)];
				for (int i = 0; i < costs.Length; i++)
				{
					array[i] = costs[i];
				}
				costs = array;
			}
			maxNodeIndex = index;
		}
	}

	public uint GetHeuristic(int nodeIndex1, int nodeIndex2)
	{
		nodeIndex1 *= pivotCount;
		nodeIndex2 *= pivotCount;
		if (nodeIndex1 >= costs.Length || nodeIndex2 >= costs.Length)
		{
			EnsureCapacity((nodeIndex1 <= nodeIndex2) ? nodeIndex2 : nodeIndex1);
		}
		uint num = 0u;
		for (int i = 0; i < pivotCount; i++)
		{
			uint num2 = (uint)Math.Abs((int)(costs[nodeIndex1 + i] - costs[nodeIndex2 + i]));
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private void GetClosestWalkableNodesToChildrenRecursively(Transform tr, List<GraphNode> nodes)
	{
		foreach (Transform item in tr)
		{
			NNInfo nearest = AstarPath.active.GetNearest(item.position, NNConstraint.Default);
			if (nearest.node != null && nearest.node.Walkable)
			{
				nodes.Add(nearest.node);
			}
			GetClosestWalkableNodesToChildrenRecursively(tr, nodes);
		}
	}

	public void RecalculatePivots()
	{
		if (mode == HeuristicOptimizationMode.None)
		{
			pivotCount = 0;
			pivots = null;
			return;
		}
		rval = (uint)seed;
		NavGraph[] graphs = AstarPath.active.graphs;
		List<GraphNode> pivotList = ListPool<GraphNode>.Claim();
		if (mode == HeuristicOptimizationMode.Custom)
		{
			if (pivotPointRoot == null)
			{
				throw new Exception("Grid Graph -> heuristicOptimizationMode is HeuristicOptimizationMode.Custom, but no 'customHeuristicOptimizationPivotsRoot' is set");
			}
			GetClosestWalkableNodesToChildrenRecursively(pivotPointRoot, pivotList);
		}
		else if (mode == HeuristicOptimizationMode.Random)
		{
			int n = 0;
			for (int i = 0; i < graphs.Length; i++)
			{
				graphs[i].GetNodes(delegate(GraphNode node)
				{
					if (!node.Destroyed && node.Walkable)
					{
						n++;
						if (GetRandom() % n < spreadOutCount)
						{
							if (pivotList.Count < spreadOutCount)
							{
								pivotList.Add(node);
							}
							else
							{
								pivotList[(int)(GetRandom() % pivotList.Count)] = node;
							}
						}
					}
					return true;
				});
			}
		}
		else
		{
			if (mode != HeuristicOptimizationMode.RandomSpreadOut)
			{
				throw new Exception("Invalid HeuristicOptimizationMode: " + mode);
			}
			GraphNode first = null;
			if (pivotPointRoot != null)
			{
				GetClosestWalkableNodesToChildrenRecursively(pivotPointRoot, pivotList);
			}
			else
			{
				for (int j = 0; j < graphs.Length; j++)
				{
					graphs[j].GetNodes(delegate(GraphNode node)
					{
						if (node != null && node.Walkable)
						{
							first = node;
							return false;
						}
						return true;
					});
				}
				if (first == null)
				{
					Debug.LogError("Could not find any walkable node in any of the graphs.");
					ListPool<GraphNode>.Release(pivotList);
					return;
				}
				pivotList.Add(first);
			}
			for (int k = 0; k < spreadOutCount; k++)
			{
				pivotList.Add(null);
			}
		}
		pivots = pivotList.ToArray();
		ListPool<GraphNode>.Release(pivotList);
	}

	public void RecalculateCosts()
	{
		if (pivots == null)
		{
			RecalculatePivots();
		}
		if (mode == HeuristicOptimizationMode.None)
		{
			return;
		}
		pivotCount = 0;
		for (int i = 0; i < pivots.Length; i++)
		{
			if (pivots[i] != null && (pivots[i].Destroyed || !pivots[i].Walkable))
			{
				throw new Exception("Invalid pivot nodes (destroyed or unwalkable)");
			}
		}
		if (mode != HeuristicOptimizationMode.RandomSpreadOut)
		{
			for (int j = 0; j < pivots.Length; j++)
			{
				if (pivots[j] == null)
				{
					throw new Exception("Invalid pivot nodes (null)");
				}
			}
		}
		Debug.Log("Recalculating costs...");
		pivotCount = pivots.Length;
		Action<int> startCostCalculation = null;
		startCostCalculation = delegate(int k)
		{
			GraphNode pivot = pivots[k];
			FloodPath fp = null;
			fp = FloodPath.Construct(pivot);
			fp.immediateCallback = delegate(Path _p)
			{
				_p.Claim(this);
				MeshNode meshNode = pivot as MeshNode;
				uint costOffset = 0u;
				if (meshNode != null && meshNode.connectionCosts != null)
				{
					for (int l = 0; l < meshNode.connectionCosts.Length; l++)
					{
						costOffset = Math.Max(costOffset, meshNode.connectionCosts[l]);
					}
				}
				NavGraph[] graphs = AstarPath.active.graphs;
				for (int num = graphs.Length - 1; num >= 0; num--)
				{
					graphs[num].GetNodes(delegate(GraphNode node)
					{
						int num2 = node.NodeIndex * pivotCount + k;
						EnsureCapacity(num2);
						PathNode pathNode = fp.pathHandler.GetPathNode(node);
						if (costOffset != 0)
						{
							costs[num2] = ((pathNode.pathID == fp.pathID && pathNode.parent != null) ? Math.Max(pathNode.parent.G - costOffset, 0u) : 0u);
						}
						else
						{
							costs[num2] = ((pathNode.pathID == fp.pathID) ? pathNode.G : 0u);
						}
						return true;
					});
				}
				if (mode == HeuristicOptimizationMode.RandomSpreadOut && k < pivots.Length - 1)
				{
					int num3 = -1;
					uint num4 = 0u;
					int num5 = maxNodeIndex / pivotCount;
					for (int m = 1; m < num5; m++)
					{
						uint num6 = 1073741824u;
						for (int n = 0; n <= k; n++)
						{
							num6 = Math.Min(num6, costs[m * pivotCount + n]);
						}
						GraphNode node2 = fp.pathHandler.GetPathNode(m).node;
						if ((num6 > num4 || num3 == -1) && node2 != null && !node2.Destroyed && node2.Walkable)
						{
							num3 = m;
							num4 = num6;
						}
					}
					if (num3 == -1)
					{
						Debug.LogError("Failed generating random pivot points for heuristic optimizations");
						return;
					}
					pivots[k + 1] = fp.pathHandler.GetPathNode(num3).node;
					Debug.Log("Found node at " + (string)pivots[k + 1].position + " with score " + num4);
					startCostCalculation(k + 1);
				}
				_p.Release(this);
			};
			AstarPath.StartPath(fp, pushToFront: true);
		};
		if (mode != HeuristicOptimizationMode.RandomSpreadOut)
		{
			for (int num7 = 0; num7 < pivots.Length; num7++)
			{
				startCostCalculation(num7);
			}
		}
		else
		{
			startCostCalculation(0);
		}
		dirty = false;
	}

	public void OnDrawGizmos()
	{
		if (pivots == null)
		{
			return;
		}
		for (int i = 0; i < pivots.Length; i++)
		{
			Gizmos.color = new Color(53f / 85f, 0.36862746f, 0.7607843f, 0.8f);
			if (pivots[i] != null && !pivots[i].Destroyed)
			{
				Gizmos.DrawCube((Vector3)pivots[i].position, Vector3.one);
			}
		}
	}
}
