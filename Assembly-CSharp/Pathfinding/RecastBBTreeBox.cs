using System;
using UnityEngine;

namespace Pathfinding;

public class RecastBBTreeBox
{
	public Rect rect;

	public RecastMeshObj mesh;

	public RecastBBTreeBox c1;

	public RecastBBTreeBox c2;

	public RecastBBTreeBox(RecastBBTree tree, RecastMeshObj mesh)
	{
		this.mesh = mesh;
		Vector3 min = mesh.bounds.min;
		Vector3 max = mesh.bounds.max;
		rect = Rect.MinMaxRect(min.x, min.z, max.x, max.z);
	}

	public bool Contains(Vector3 p)
	{
		return rect.Contains(p);
	}

	public void WriteChildren(int level)
	{
		for (int i = 0; i < level; i++)
		{
			Console.Write("  ");
		}
		if (mesh != null)
		{
			Console.WriteLine("Leaf ");
			return;
		}
		Console.WriteLine("Box ");
		c1.WriteChildren(level + 1);
		c2.WriteChildren(level + 1);
	}
}
