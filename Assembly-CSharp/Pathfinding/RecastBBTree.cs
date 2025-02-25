using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding;

public class RecastBBTree
{
	public RecastBBTreeBox root;

	public void QueryInBounds(Rect bounds, List<RecastMeshObj> buffer)
	{
		RecastBBTreeBox recastBBTreeBox = root;
		if (recastBBTreeBox != null)
		{
			QueryBoxInBounds(recastBBTreeBox, bounds, buffer);
		}
	}

	public void QueryBoxInBounds(RecastBBTreeBox box, Rect bounds, List<RecastMeshObj> boxes)
	{
		if (box.mesh != null)
		{
			if (RectIntersectsRect(box.rect, bounds))
			{
				boxes.Add(box.mesh);
			}
			return;
		}
		if (RectIntersectsRect(box.c1.rect, bounds))
		{
			QueryBoxInBounds(box.c1, bounds, boxes);
		}
		if (RectIntersectsRect(box.c2.rect, bounds))
		{
			QueryBoxInBounds(box.c2, bounds, boxes);
		}
	}

	public bool Remove(RecastMeshObj mesh)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (root == null)
		{
			return false;
		}
		bool found = false;
		Bounds bounds = mesh.GetBounds();
		Rect bounds2 = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
		root = RemoveBox(root, mesh, bounds2, ref found);
		return found;
	}

	private RecastBBTreeBox RemoveBox(RecastBBTreeBox c, RecastMeshObj mesh, Rect bounds, ref bool found)
	{
		if (!RectIntersectsRect(c.rect, bounds))
		{
			return c;
		}
		if (c.mesh == mesh)
		{
			found = true;
			return null;
		}
		if (c.mesh == null && !found)
		{
			c.c1 = RemoveBox(c.c1, mesh, bounds, ref found);
			if (c.c1 == null)
			{
				return c.c2;
			}
			if (!found)
			{
				c.c2 = RemoveBox(c.c2, mesh, bounds, ref found);
				if (c.c2 == null)
				{
					return c.c1;
				}
			}
			if (found)
			{
				c.rect = ExpandToContain(c.c1.rect, c.c2.rect);
			}
		}
		return c;
	}

	public void Insert(RecastMeshObj mesh)
	{
		RecastBBTreeBox recastBBTreeBox = new RecastBBTreeBox(this, mesh);
		if (root == null)
		{
			root = recastBBTreeBox;
			return;
		}
		RecastBBTreeBox recastBBTreeBox2 = root;
		while (true)
		{
			recastBBTreeBox2.rect = ExpandToContain(recastBBTreeBox2.rect, recastBBTreeBox.rect);
			if (recastBBTreeBox2.mesh != null)
			{
				break;
			}
			float num = ExpansionRequired(recastBBTreeBox2.c1.rect, recastBBTreeBox.rect);
			float num2 = ExpansionRequired(recastBBTreeBox2.c2.rect, recastBBTreeBox.rect);
			recastBBTreeBox2 = ((!(num < num2)) ? ((!(num2 < num)) ? ((!(RectArea(recastBBTreeBox2.c1.rect) < RectArea(recastBBTreeBox2.c2.rect))) ? recastBBTreeBox2.c2 : recastBBTreeBox2.c1) : recastBBTreeBox2.c2) : recastBBTreeBox2.c1);
		}
		recastBBTreeBox2.c1 = recastBBTreeBox;
		RecastBBTreeBox c = new RecastBBTreeBox(this, recastBBTreeBox2.mesh);
		recastBBTreeBox2.c2 = c;
		recastBBTreeBox2.mesh = null;
	}

	public void OnDrawGizmos()
	{
	}

	public void OnDrawGizmos(RecastBBTreeBox box)
	{
		if (box != null)
		{
			Vector3 vector = new Vector3(box.rect.xMin, 0f, box.rect.yMin);
			Vector3 vector2 = new Vector3(box.rect.xMax, 0f, box.rect.yMax);
			Vector3 vector3 = (vector + vector2) * 0.5f;
			Vector3 size = (vector2 - vector3) * 2f;
			Gizmos.DrawCube(vector3, size);
			OnDrawGizmos(box.c1);
			OnDrawGizmos(box.c2);
		}
	}

	public void TestIntersections(Vector3 p, float radius)
	{
		RecastBBTreeBox box = root;
		TestIntersections(box, p, radius);
	}

	public void TestIntersections(RecastBBTreeBox box, Vector3 p, float radius)
	{
		if (box != null)
		{
			RectIntersectsCircle(box.rect, p, radius);
			TestIntersections(box.c1, p, radius);
			TestIntersections(box.c2, p, radius);
		}
	}

	public bool RectIntersectsRect(Rect r, Rect r2)
	{
		return r.xMax > r2.xMin && r.yMax > r2.yMin && r2.xMax > r.xMin && r2.yMax > r.yMin;
	}

	public bool RectIntersectsCircle(Rect r, Vector3 p, float radius)
	{
		if (float.IsPositiveInfinity(radius))
		{
			return true;
		}
		if (RectContains(r, p))
		{
			return true;
		}
		return XIntersectsCircle(r.xMin, r.xMax, r.yMin, p, radius) || XIntersectsCircle(r.xMin, r.xMax, r.yMax, p, radius) || ZIntersectsCircle(r.yMin, r.yMax, r.xMin, p, radius) || ZIntersectsCircle(r.yMin, r.yMax, r.xMax, p, radius);
	}

	public bool RectContains(Rect r, Vector3 p)
	{
		return p.x >= r.xMin && p.x <= r.xMax && p.z >= r.yMin && p.z <= r.yMax;
	}

	public bool ZIntersectsCircle(float z1, float z2, float xpos, Vector3 circle, float radius)
	{
		double num = Math.Abs(xpos - circle.x) / radius;
		if (num > 1.0 || num < -1.0)
		{
			return false;
		}
		float num2 = (float)Math.Sqrt(1.0 - num * num) * radius;
		float val = circle.z - num2;
		num2 += circle.z;
		float b = Math.Min(num2, val);
		float b2 = Math.Max(num2, val);
		b = Mathf.Max(z1, b);
		b2 = Mathf.Min(z2, b2);
		return b2 > b;
	}

	public bool XIntersectsCircle(float x1, float x2, float zpos, Vector3 circle, float radius)
	{
		double num = Math.Abs(zpos - circle.z) / radius;
		if (num > 1.0 || num < -1.0)
		{
			return false;
		}
		float num2 = (float)Math.Sqrt(1.0 - num * num) * radius;
		float val = circle.x - num2;
		num2 += circle.x;
		float b = Math.Min(num2, val);
		float b2 = Math.Max(num2, val);
		b = Mathf.Max(x1, b);
		b2 = Mathf.Min(x2, b2);
		return b2 > b;
	}

	public float ExpansionRequired(Rect r, Rect r2)
	{
		float num = Mathf.Min(r.xMin, r2.xMin);
		float num2 = Mathf.Max(r.xMax, r2.xMax);
		float num3 = Mathf.Min(r.yMin, r2.yMin);
		float num4 = Mathf.Max(r.yMax, r2.yMax);
		return (num2 - num) * (num4 - num3) - RectArea(r);
	}

	public Rect ExpandToContain(Rect r, Rect r2)
	{
		float xmin = Mathf.Min(r.xMin, r2.xMin);
		float xmax = Mathf.Max(r.xMax, r2.xMax);
		float ymin = Mathf.Min(r.yMin, r2.yMin);
		float ymax = Mathf.Max(r.yMax, r2.yMax);
		return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
	}

	public float RectArea(Rect r)
	{
		return r.width * r.height;
	}

	public new void ToString()
	{
		RecastBBTreeBox recastBBTreeBox = root;
		Stack<RecastBBTreeBox> stack = new Stack<RecastBBTreeBox>();
		stack.Push(recastBBTreeBox);
		recastBBTreeBox.WriteChildren(0);
	}
}
