using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding;

public class GridNode : GraphNode
{
	private const int GridFlagsConnectionOffset = 0;

	private const int GridFlagsConnectionBit0 = 1;

	private const int GridFlagsConnectionMask = 255;

	private const int GridFlagsWalkableErosionOffset = 8;

	private const int GridFlagsWalkableErosionMask = 256;

	private const int GridFlagsWalkableTmpOffset = 9;

	private const int GridFlagsWalkableTmpMask = 512;

	private const int GridFlagsEdgeNodeOffset = 10;

	private const int GridFlagsEdgeNodeMask = 1024;

	private static GridGraph[] _gridGraphs = new GridGraph[0];

	protected int nodeInGridIndex;

	protected ushort gridFlags;

	public ushort clearance;

	internal ushort InternalGridFlags
	{
		get
		{
			return gridFlags;
		}
		set
		{
			gridFlags = value;
		}
	}

	public bool EdgeNode
	{
		get
		{
			return (gridFlags & 0x400) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & -1025) | (value ? 1024 : 0));
		}
	}

	public bool WalkableErosion
	{
		get
		{
			return (gridFlags & 0x100) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & -257) | (value ? 256 : 0));
		}
	}

	public bool TmpWalkable
	{
		get
		{
			return (gridFlags & 0x200) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & -513) | (value ? 512 : 0));
		}
	}

	public int NodeInGridIndex
	{
		get
		{
			return nodeInGridIndex;
		}
		set
		{
			nodeInGridIndex = value;
		}
	}

	public GridNode(AstarPath astar)
		: base(astar)
	{
	}

	public static GridGraph GetGridGraph(uint graphIndex)
	{
		return _gridGraphs[graphIndex];
	}

	public static void SetGridGraph(int graphIndex, GridGraph graph)
	{
		if (_gridGraphs.Length <= graphIndex)
		{
			GridGraph[] array = new GridGraph[graphIndex + 1];
			for (int i = 0; i < _gridGraphs.Length; i++)
			{
				array[i] = _gridGraphs[i];
			}
			_gridGraphs = array;
		}
		_gridGraphs[graphIndex] = graph;
	}

	[Obsolete("This method has been deprecated. Please use NodeInGridIndex instead.", true)]
	public int GetIndex()
	{
		return 0;
	}

	public bool GetConnectionInternal(int dir)
	{
		return ((gridFlags >> dir) & 1) != 0;
	}

	public void SetConnectionInternal(int dir, bool value)
	{
		gridFlags = (ushort)((gridFlags & ~(1 << dir)) | ((value ? 1 : 0) << dir));
	}

	public void ResetConnectionsInternal()
	{
		gridFlags = (ushort)(gridFlags & -256);
	}

	public bool IsValidConnectionY(GridNode n2, int iMaxClimb)
	{
		if (!n2.Walkable || Mathf.Abs(position.y - n2.position.y) > iMaxClimb)
		{
			return false;
		}
		return true;
	}

	public override void ClearConnections(bool alsoReverse)
	{
		if (alsoReverse)
		{
			GridGraph gridGraph = GetGridGraph(base.GraphIndex);
			for (int i = 0; i < 8; i++)
			{
				gridGraph.GetNodeConnection(this, i)?.SetConnectionInternal((i >= 4) ? 7 : ((i + 2) % 4), value: false);
			}
		}
		ResetConnectionsInternal();
	}

	public override void GetConnections(GraphNodeDelegate del)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNode[] nodes = gridGraph.nodes;
		for (int i = 0; i < 8; i++)
		{
			if (GetConnectionInternal(i))
			{
				GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
				if (gridNode != null)
				{
					del(gridNode);
				}
			}
		}
	}

	public override bool GetPortal(GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards)
	{
		if (backwards)
		{
			return true;
		}
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNode[] nodes = gridGraph.nodes;
		for (int i = 0; i < 4; i++)
		{
			if (GetConnectionInternal(i) && other == nodes[nodeInGridIndex + neighbourOffsets[i]])
			{
				Vector3 vector = (Vector3)(position + other.position) * 0.5f;
				Vector3 vector2 = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
				vector2.Normalize();
				vector2 *= gridGraph.nodeSize * 0.5f;
				left.Add(vector - vector2);
				right.Add(vector + vector2);
				return true;
			}
		}
		for (int j = 4; j < 8; j++)
		{
			if (!GetConnectionInternal(j) || other != nodes[nodeInGridIndex + neighbourOffsets[j]])
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			if (GetConnectionInternal(j - 4))
			{
				GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[j - 4]];
				if (gridNode.Walkable && gridNode.GetConnectionInternal((j - 4 + 1) % 4))
				{
					flag = true;
				}
			}
			if (GetConnectionInternal((j - 4 + 1) % 4))
			{
				GridNode gridNode2 = nodes[nodeInGridIndex + neighbourOffsets[(j - 4 + 1) % 4]];
				if (gridNode2.Walkable && gridNode2.GetConnectionInternal(j - 4))
				{
					flag2 = true;
				}
			}
			Vector3 vector3 = (Vector3)(position + other.position) * 0.5f;
			Vector3 vector4 = Vector3.Cross(gridGraph.collision.up, (Vector3)(other.position - position));
			vector4.Normalize();
			vector4 *= gridGraph.nodeSize * 1.4142f;
			left.Add(vector3 - ((!flag2) ? Vector3.zero : vector4));
			right.Add(vector3 + ((!flag) ? Vector3.zero : vector4));
			return true;
		}
		return false;
	}

	public override void FloodFill(Stack<GraphNode> stack, uint region)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNode[] nodes = gridGraph.nodes;
		for (int i = 0; i < 8; i++)
		{
			if (GetConnectionInternal(i))
			{
				GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
				if (gridNode != null && gridNode.Area != region)
				{
					gridNode.Area = region;
					stack.Push(gridNode);
				}
			}
		}
	}

	public override void AddConnection(GraphNode node, uint cost)
	{
		throw new NotImplementedException("GridNodes do not have support for adding manual connections");
	}

	public override void RemoveConnection(GraphNode node)
	{
		throw new NotImplementedException("GridNodes do not have support for adding manual connections");
	}

	public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		GridNode[] nodes = gridGraph.nodes;
		UpdateG(path, pathNode);
		handler.PushNode(pathNode);
		ushort pathID = handler.PathID;
		for (int i = 0; i < 8; i++)
		{
			if (GetConnectionInternal(i))
			{
				GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
				PathNode pathNode2 = handler.GetPathNode(gridNode);
				if (pathNode2.parent == pathNode && pathNode2.pathID == pathID)
				{
					gridNode.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}
	}

	public override void Open(Path path, PathNode pathNode, PathHandler handler)
	{
		GridGraph gridGraph = GetGridGraph(base.GraphIndex);
		ushort pathID = handler.PathID;
		int[] neighbourOffsets = gridGraph.neighbourOffsets;
		uint[] neighbourCosts = gridGraph.neighbourCosts;
		GridNode[] nodes = gridGraph.nodes;
		for (int i = 0; i < 8; i++)
		{
			if (!GetConnectionInternal(i))
			{
				continue;
			}
			GridNode gridNode = nodes[nodeInGridIndex + neighbourOffsets[i]];
			if (path.CanTraverse(gridNode) && path.width <= gridNode.clearance)
			{
				PathNode pathNode2 = handler.GetPathNode(gridNode);
				uint num = neighbourCosts[i];
				if (pathNode2.pathID != pathID)
				{
					pathNode2.parent = pathNode;
					pathNode2.pathID = pathID;
					pathNode2.cost = num;
					pathNode2.H = path.CalculateHScore(gridNode);
					gridNode.UpdateG(path, pathNode2);
					handler.PushNode(pathNode2);
				}
				else if (pathNode.G + num + path.GetTraversalCost(gridNode) < pathNode2.G)
				{
					pathNode2.cost = num;
					pathNode2.parent = pathNode;
					gridNode.UpdateRecursiveG(path, pathNode2, handler);
				}
				else if (pathNode2.G + num + path.GetTraversalCost(this) < pathNode.G)
				{
					pathNode.parent = pathNode2;
					pathNode.cost = num;
					UpdateRecursiveG(path, pathNode, handler);
				}
			}
		}
	}

	public override void SerializeNode(GraphSerializationContext ctx)
	{
		base.SerializeNode(ctx);
		ctx.writer.Write(position.x);
		ctx.writer.Write(position.y);
		ctx.writer.Write(position.z);
		ctx.writer.Write(gridFlags);
	}

	public override void DeserializeNode(GraphSerializationContext ctx)
	{
		base.DeserializeNode(ctx);
		position = new Int3(ctx.reader.ReadInt32(), ctx.reader.ReadInt32(), ctx.reader.ReadInt32());
		gridFlags = ctx.reader.ReadUInt16();
	}
}
