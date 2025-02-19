using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding;

public class QuadtreeGraph : NavGraph
{
	public int editorWidthLog2 = 6;

	public int editorHeightLog2 = 6;

	public LayerMask layerMask = -1;

	public float nodeSize = 1f;

	public int minDepth = 3;

	private QuadtreeNodeHolder root;

	public Vector3 center;

	private BitArray map;

	public int Width { get; protected set; }

	public int Height { get; protected set; }

	public override void GetNodes(GraphNodeDelegateCancelable del)
	{
		if (root != null)
		{
			root.GetNodes(del);
		}
	}

	public bool CheckCollision(int x, int y)
	{
		Vector3 position = LocalToWorldPosition(x, y, 1);
		return !Physics.CheckSphere(position, nodeSize * 1.4142f, layerMask);
	}

	public int CheckNode(int xs, int ys, int width)
	{
		Debug.Log("Checking Node " + xs + " " + ys + " width: " + width);
		bool flag = map[xs + ys * Width];
		for (int i = xs; i < xs + width; i++)
		{
			for (int j = ys; j < ys + width; j++)
			{
				if (map[i + j * Width] != flag)
				{
					return -1;
				}
			}
		}
		return flag ? 1 : 0;
	}

	public override void ScanInternal(OnScanStatus statusCallback)
	{
		Width = 1 << editorWidthLog2;
		Height = 1 << editorHeightLog2;
		map = new BitArray(Width * Height);
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				map.Set(i + j * Width, CheckCollision(i, j));
			}
		}
		QuadtreeNodeHolder holder = new QuadtreeNodeHolder();
		CreateNodeRec(holder, 0, 0, 0);
		root = holder;
		RecalculateConnectionsRec(root, 0, 0, 0);
	}

	public void RecalculateConnectionsRec(QuadtreeNodeHolder holder, int depth, int x, int y)
	{
		if (holder.node != null)
		{
			RecalculateConnections(holder, depth, x, y);
			return;
		}
		int num = 1 << Math.Min(editorHeightLog2, editorWidthLog2) - depth;
		RecalculateConnectionsRec(holder.c0, depth + 1, x, y);
		RecalculateConnectionsRec(holder.c1, depth + 1, x + num / 2, y);
		RecalculateConnectionsRec(holder.c2, depth + 1, x + num / 2, y + num / 2);
		RecalculateConnectionsRec(holder.c3, depth + 1, x, y + num / 2);
	}

	public Vector3 LocalToWorldPosition(int x, int y, int width)
	{
		return new Vector3(((float)x + (float)width * 0.5f) * nodeSize, 0f, ((float)y + (float)width * 0.5f) * nodeSize);
	}

	public void CreateNodeRec(QuadtreeNodeHolder holder, int depth, int x, int y)
	{
		int num = 1 << Math.Min(editorHeightLog2, editorWidthLog2) - depth;
		int num2 = ((depth >= minDepth) ? CheckNode(x, y, num) : (-1));
		if (num2 == 1 || num2 == 0 || num == 1)
		{
			QuadtreeNode quadtreeNode = new QuadtreeNode(active);
			quadtreeNode.SetPosition((Int3)LocalToWorldPosition(x, y, num));
			quadtreeNode.Walkable = num2 == 1;
			holder.node = quadtreeNode;
			return;
		}
		holder.c0 = new QuadtreeNodeHolder();
		holder.c1 = new QuadtreeNodeHolder();
		holder.c2 = new QuadtreeNodeHolder();
		holder.c3 = new QuadtreeNodeHolder();
		CreateNodeRec(holder.c0, depth + 1, x, y);
		CreateNodeRec(holder.c1, depth + 1, x + num / 2, y);
		CreateNodeRec(holder.c2, depth + 1, x + num / 2, y + num / 2);
		CreateNodeRec(holder.c3, depth + 1, x, y + num / 2);
	}

	public void RecalculateConnections(QuadtreeNodeHolder holder, int depth, int x, int y)
	{
		if (root == null)
		{
			throw new InvalidOperationException("Graph contains no nodes");
		}
		if (holder.node == null)
		{
			throw new ArgumentException("No leaf node specified. Holder has no node.");
		}
		int num = 1 << Math.Min(editorHeightLog2, editorWidthLog2) - depth;
		List<QuadtreeNode> list = new List<QuadtreeNode>();
		AddNeighboursRec(list, root, 0, 0, 0, new IntRect(x, y, x + num, y + num).Expand(0), holder.node);
		holder.node.connections = list.ToArray();
		holder.node.connectionCosts = new uint[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			uint costMagnitude = (uint)(list[i].position - holder.node.position).costMagnitude;
			holder.node.connectionCosts[i] = costMagnitude;
		}
	}

	public void AddNeighboursRec(List<QuadtreeNode> arr, QuadtreeNodeHolder holder, int depth, int x, int y, IntRect bounds, QuadtreeNode dontInclude)
	{
		int num = 1 << Math.Min(editorHeightLog2, editorWidthLog2) - depth;
		IntRect a = new IntRect(x, y, x + num, y + num);
		if (!IntRect.Intersects(a, bounds))
		{
			return;
		}
		if (holder.node != null)
		{
			if (holder.node != dontInclude)
			{
				arr.Add(holder.node);
			}
		}
		else
		{
			AddNeighboursRec(arr, holder.c0, depth + 1, x, y, bounds, dontInclude);
			AddNeighboursRec(arr, holder.c1, depth + 1, x + num / 2, y, bounds, dontInclude);
			AddNeighboursRec(arr, holder.c2, depth + 1, x + num / 2, y + num / 2, bounds, dontInclude);
			AddNeighboursRec(arr, holder.c3, depth + 1, x, y + num / 2, bounds, dontInclude);
		}
	}

	public QuadtreeNode QueryPoint(int qx, int qy)
	{
		if (root == null)
		{
			return null;
		}
		QuadtreeNodeHolder quadtreeNodeHolder = root;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (quadtreeNodeHolder.node == null)
		{
			int num4 = 1 << Math.Min(editorHeightLog2, editorWidthLog2) - num;
			if (qx >= num2 + num4 / 2)
			{
				num2 += num4 / 2;
				if (qy >= num3 + num4 / 2)
				{
					num3 += num4 / 2;
					quadtreeNodeHolder = quadtreeNodeHolder.c2;
				}
				else
				{
					quadtreeNodeHolder = quadtreeNodeHolder.c1;
				}
			}
			else if (qy >= num3 + num4 / 2)
			{
				num3 += num4 / 2;
				quadtreeNodeHolder = quadtreeNodeHolder.c3;
			}
			else
			{
				quadtreeNodeHolder = quadtreeNodeHolder.c0;
			}
			num++;
		}
		return quadtreeNodeHolder.node;
	}

	public override void OnDrawGizmos(bool drawNodes)
	{
		base.OnDrawGizmos(drawNodes);
		if (drawNodes && root != null)
		{
			DrawRec(root, 0, 0, 0, Vector3.zero);
		}
	}

	public void DrawRec(QuadtreeNodeHolder h, int depth, int x, int y, Vector3 parentPos)
	{
		int num = 1 << Math.Min(editorHeightLog2, editorWidthLog2) - depth;
		Vector3 vector = LocalToWorldPosition(x, y, num);
		Debug.DrawLine(vector, parentPos, Color.red);
		if (h.node != null)
		{
			Debug.DrawRay(vector, Vector3.down, (!h.node.Walkable) ? Color.yellow : Color.green);
			return;
		}
		DrawRec(h.c0, depth + 1, x, y, vector);
		DrawRec(h.c1, depth + 1, x + num / 2, y, vector);
		DrawRec(h.c2, depth + 1, x + num / 2, y + num / 2, vector);
		DrawRec(h.c3, depth + 1, x, y + num / 2, vector);
	}
}
