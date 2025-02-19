using System;
using UnityEngine;

namespace Pathfinding;

public class ConvexMeshNode : MeshNode
{
	private int[] indices;

	protected static INavmeshHolder[] navmeshHolders;

	public ConvexMeshNode(AstarPath astar)
		: base(astar)
	{
		indices = new int[0];
	}

	static ConvexMeshNode()
	{
		navmeshHolders = new INavmeshHolder[0];
	}

	protected static INavmeshHolder GetNavmeshHolder(uint graphIndex)
	{
		return navmeshHolders[graphIndex];
	}

	public void SetPosition(Int3 p)
	{
		position = p;
	}

	public int GetVertexIndex(int i)
	{
		return indices[i];
	}

	public override Int3 GetVertex(int i)
	{
		return GetNavmeshHolder(base.GraphIndex).GetVertex(GetVertexIndex(i));
	}

	public override int GetVertexCount()
	{
		return indices.Length;
	}

	public override Vector3 ClosestPointOnNode(Vector3 p)
	{
		throw new NotImplementedException();
	}

	public override Vector3 ClosestPointOnNodeXZ(Vector3 p)
	{
		throw new NotImplementedException();
	}

	public override void GetConnections(GraphNodeDelegate del)
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				del(connections[i]);
			}
		}
	}

	public override void Open(Path path, PathNode pathNode, PathHandler handler)
	{
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			GraphNode graphNode = connections[i];
			if (!path.CanTraverse(graphNode))
			{
				continue;
			}
			PathNode pathNode2 = handler.GetPathNode(graphNode);
			if (pathNode2.pathID != handler.PathID)
			{
				pathNode2.parent = pathNode;
				pathNode2.pathID = handler.PathID;
				pathNode2.cost = connectionCosts[i];
				pathNode2.H = path.CalculateHScore(graphNode);
				graphNode.UpdateG(path, pathNode2);
				handler.PushNode(pathNode2);
				continue;
			}
			uint num = connectionCosts[i];
			if (pathNode.G + num + path.GetTraversalCost(graphNode) < pathNode2.G)
			{
				pathNode2.cost = num;
				pathNode2.parent = pathNode;
				graphNode.UpdateRecursiveG(path, pathNode2, handler);
			}
			else if (pathNode2.G + num + path.GetTraversalCost(this) < pathNode.G && graphNode.ContainsConnection(this))
			{
				pathNode.parent = pathNode2;
				pathNode.cost = num;
				UpdateRecursiveG(path, pathNode, handler);
			}
		}
	}
}
