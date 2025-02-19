using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

public class ProceduralGridMover : MonoBehaviour
{
	private const int c_maxNodesInUpdateGraphMin = 600;

	private const int c_maxNodesInUpdateGraphMax = 3000;

	public float updateDistance = 5f;

	public Transform target;

	public bool floodFill;

	private GridGraph graph;

	private GridNode[] tmp;

	public void Start()
	{
		if (AstarPath.active == null)
		{
			throw new Exception("There is no AstarPath object in the scene");
		}
		graph = AstarPath.active.astarData.gridGraph;
		if (graph == null)
		{
			throw new Exception("The AstarPath object has no GridGraph");
		}
		UpdateGraph();
	}

	private void Update()
	{
		if (target != null && AstarMath.SqrMagnitudeXZ(target.position, graph.center) > updateDistance * updateDistance)
		{
			UpdateGraph();
		}
	}

	public void UpdateGraph()
	{
		IEnumerator ie = UpdateGraphCoroutine();
		AstarPath.active.AddWorkItem(new AstarPath.AstarWorkItem(delegate(bool force)
		{
			if (force)
			{
				while (ie.MoveNext())
				{
				}
			}
			return !ie.MoveNext();
		}));
	}

	private IEnumerator UpdateGraphCoroutine()
	{
		if (graph == null || graph.nodes == null || target == null)
		{
			yield break;
		}
		Vector3 dir = target.position - graph.center;
		dir.x = Mathf.Round(dir.x / graph.nodeSize) * graph.nodeSize;
		dir.z = Mathf.Round(dir.z / graph.nodeSize) * graph.nodeSize;
		dir.y = 0f;
		if (dir == Vector3.zero)
		{
			yield break;
		}
		Int2 offset = new Int2(-Mathf.RoundToInt(dir.x / graph.nodeSize), -Mathf.RoundToInt(dir.z / graph.nodeSize));
		graph.center += dir;
		graph.GenerateMatrix();
		if (tmp == null || tmp.Length != graph.nodes.Length)
		{
			tmp = new GridNode[graph.nodes.Length];
		}
		int width = graph.width;
		int depth = graph.depth;
		GridNode[] nodes = graph.nodes;
		int absOfsX = Mathf.Abs(offset.x);
		int absOfsY = Mathf.Abs(offset.y);
		int nNodesUpdated = 0;
		int nMaxUpdateColPerFrame = absOfsX * depth + absOfsY * width >> 4;
		if (nMaxUpdateColPerFrame < 600)
		{
			nMaxUpdateColPerFrame = 600;
		}
		else if (nMaxUpdateColPerFrame > 3000)
		{
			nMaxUpdateColPerFrame = 3000;
		}
		int nMaxCalcConPerFrame = nMaxUpdateColPerFrame * 2;
		if (nMaxCalcConPerFrame > 3000)
		{
			nMaxCalcConPerFrame = 3000;
		}
		if (absOfsX <= width && absOfsY <= depth)
		{
			for (int z = 0; z < depth; z++)
			{
				int pz = z * width;
				int tz = (z + offset.y + depth) % depth * width;
				for (int x = 0; x < width; x++)
				{
					tmp[tz + (x + offset.x + width) % width] = nodes[pz + x];
				}
			}
			yield return null;
			for (int i = 0; i < depth; i++)
			{
				int pz2 = i * width;
				for (int j = 0; j < width; j++)
				{
					GridNode node = tmp[pz2 + j];
					node.NodeInGridIndex = pz2 + j;
					nodes[pz2 + j] = node;
				}
			}
			IntRect r = new IntRect(0, 0, offset.x, offset.y);
			int minz = r.ymax;
			int maxz = depth;
			if (r.xmin > r.xmax)
			{
				int tmp2 = r.xmax;
				r.xmax = width + r.xmin;
				r.xmin = width + tmp2;
			}
			if (r.ymin > r.ymax)
			{
				int tmp3 = r.ymax;
				r.ymax = depth + r.ymin;
				r.ymin = depth + tmp3;
				minz = 0;
				maxz = r.ymin;
			}
			r = r.Expand(graph.erodeIterations + 1);
			r = IntRect.Intersection(r, new IntRect(0, 0, width, depth));
			yield return null;
			nNodesUpdated = 0;
			for (int k = r.ymin; k < r.ymax; k++)
			{
				int x2 = 0;
				while (x2 < width)
				{
					graph.UpdateNodePositionCollision(nodes[k * width + x2], x2, k, resetPenalty: false);
					x2++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxUpdateColPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			for (int l = minz; l < maxz; l++)
			{
				int x3 = r.xmin;
				while (x3 < r.xmax)
				{
					graph.UpdateNodePositionCollision(nodes[l * width + x3], x3, l, resetPenalty: false);
					x3++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxUpdateColPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			yield return null;
			nNodesUpdated = 0;
			for (int m = r.ymin; m < r.ymax; m++)
			{
				int x4 = 0;
				while (x4 < width)
				{
					graph.CalculateConnections(nodes, x4, m, nodes[m * width + x4]);
					x4++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxCalcConPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			for (int n = minz; n < maxz; n++)
			{
				int x5 = r.xmin;
				while (x5 < r.xmax)
				{
					graph.CalculateConnections(nodes, x5, n, nodes[n * width + x5]);
					x5++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxCalcConPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			yield return null;
			graph.ErodeWalkableAreaIter1(0, r.ymin, width, r.ymax);
			graph.ErodeWalkableAreaIter1(r.xmin, r.xmax, minz, maxz);
			yield return null;
			nNodesUpdated = 0;
			for (int num = r.ymin; num < r.ymax; num++)
			{
				int x6 = 0;
				while (x6 < width)
				{
					graph.CalculateConnections(nodes, x6, num, nodes[num * width + x6]);
					x6++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxCalcConPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			for (int num2 = minz; num2 < maxz; num2++)
			{
				int x7 = r.xmin;
				while (x7 < r.xmax)
				{
					graph.CalculateConnections(nodes, x7, num2, nodes[num2 * width + x7]);
					x7++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxCalcConPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			yield return null;
			for (int num3 = 0; num3 < depth; num3++)
			{
				graph.CalculateConnections(nodes, 0, num3, nodes[num3 * width]);
				graph.CalculateConnections(nodes, width - 1, num3, nodes[num3 * width + width - 1]);
			}
			for (int num4 = 1; num4 < width - 1; num4++)
			{
				graph.CalculateConnections(nodes, num4, 0, nodes[num4]);
				graph.CalculateConnections(nodes, num4, depth - 1, nodes[(depth - 1) * width + num4]);
			}
		}
		else
		{
			nNodesUpdated = 0;
			for (int num5 = 0; num5 < depth; num5++)
			{
				int x8 = 0;
				while (x8 < width)
				{
					graph.UpdateNodePositionCollision(nodes[num5 * width + x8], x8, num5, resetPenalty: false);
					x8++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxUpdateColPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			nNodesUpdated = 0;
			for (int num6 = 0; num6 < depth; num6++)
			{
				int x9 = 0;
				while (x9 < width)
				{
					graph.CalculateConnections(nodes, x9, num6, nodes[num6 * width + x9]);
					x9++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxCalcConPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			graph.ErodeWalkableAreaIter1(0, 0, width, depth);
			yield return null;
			nNodesUpdated = 0;
			for (int num7 = 0; num7 < depth; num7++)
			{
				int x10 = 0;
				while (x10 < width)
				{
					graph.CalculateConnections(nodes, x10, num7, nodes[num7 * width + x10]);
					x10++;
					nNodesUpdated++;
				}
				if (nNodesUpdated > nMaxCalcConPerFrame)
				{
					nNodesUpdated = 0;
					yield return null;
				}
			}
			yield return null;
		}
		graph.UpdateClearance();
		if (floodFill)
		{
			yield return null;
			AstarPath.active.QueueWorkItemFloodFill();
		}
	}
}
