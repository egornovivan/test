using System;
using UnityEngine;

namespace Pathfinding;

public class BBTree
{
	private struct BBTreeBox
	{
		public Rect rect;

		public MeshNode node;

		public int left;

		public int right;

		public bool IsLeaf => node != null;

		public BBTreeBox(BBTree tree, MeshNode node)
		{
			this.node = node;
			Vector3 vector = (Vector3)node.GetVertex(0);
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			Vector2 vector3 = vector2;
			for (int i = 1; i < node.GetVertexCount(); i++)
			{
				Vector3 vector4 = (Vector3)node.GetVertex(i);
				vector2.x = Math.Min(vector2.x, vector4.x);
				vector2.y = Math.Min(vector2.y, vector4.z);
				vector3.x = Math.Max(vector3.x, vector4.x);
				vector3.y = Math.Max(vector3.y, vector4.z);
			}
			rect = Rect.MinMaxRect(vector2.x, vector2.y, vector3.x, vector3.y);
			left = (right = -1);
		}

		public bool Contains(Vector3 p)
		{
			return rect.Contains(new Vector2(p.x, p.z));
		}
	}

	private BBTreeBox[] arr = new BBTreeBox[6];

	private int count;

	public INavmeshHolder graph;

	public Rect Size => (count == 0) ? new Rect(0f, 0f, 0f, 0f) : arr[0].rect;

	public BBTree(INavmeshHolder graph)
	{
		this.graph = graph;
	}

	public void Clear()
	{
		count = 0;
	}

	private void EnsureCapacity(int c)
	{
		if (arr.Length < c)
		{
			BBTreeBox[] array = new BBTreeBox[Math.Max(c, (int)((float)arr.Length * 1.5f))];
			for (int i = 0; i < count; i++)
			{
				ref BBTreeBox reference = ref array[i];
				reference = arr[i];
			}
			arr = array;
		}
	}

	private int GetBox(MeshNode node)
	{
		if (count >= arr.Length)
		{
			EnsureCapacity(count + 1);
		}
		ref BBTreeBox reference = ref arr[count];
		reference = new BBTreeBox(this, node);
		count++;
		return count - 1;
	}

	public void Insert(MeshNode node)
	{
		int box = GetBox(node);
		if (box == 0)
		{
			return;
		}
		BBTreeBox bBTreeBox = arr[box];
		int num = 0;
		BBTreeBox bBTreeBox2;
		while (true)
		{
			bBTreeBox2 = arr[num];
			bBTreeBox2.rect = ExpandToContain(bBTreeBox2.rect, bBTreeBox.rect);
			if (bBTreeBox2.node != null)
			{
				break;
			}
			arr[num] = bBTreeBox2;
			float num2 = ExpansionRequired(arr[bBTreeBox2.left].rect, bBTreeBox.rect);
			float num3 = ExpansionRequired(arr[bBTreeBox2.right].rect, bBTreeBox.rect);
			num = ((!(num2 < num3)) ? ((!(num3 < num2)) ? ((!(RectArea(arr[bBTreeBox2.left].rect) < RectArea(arr[bBTreeBox2.right].rect))) ? bBTreeBox2.right : bBTreeBox2.left) : bBTreeBox2.right) : bBTreeBox2.left);
		}
		bBTreeBox2.left = box;
		int box2 = GetBox(bBTreeBox2.node);
		bBTreeBox2.right = box2;
		bBTreeBox2.node = null;
		arr[num] = bBTreeBox2;
	}

	public NNInfo Query(Vector3 p, NNConstraint constraint)
	{
		if (count == 0)
		{
			return new NNInfo(null);
		}
		NNInfo nnInfo = default(NNInfo);
		SearchBox(0, p, constraint, ref nnInfo);
		nnInfo.UpdateInfo();
		return nnInfo;
	}

	public NNInfo QueryCircle(Vector3 p, float radius, NNConstraint constraint)
	{
		if (count == 0)
		{
			return new NNInfo(null);
		}
		NNInfo nnInfo = new NNInfo(null);
		SearchBoxCircle(0, p, radius, constraint, ref nnInfo);
		nnInfo.UpdateInfo();
		return nnInfo;
	}

	public NNInfo QueryClosest(Vector3 p, NNConstraint constraint, out float distance)
	{
		distance = float.PositiveInfinity;
		return QueryClosest(p, constraint, ref distance, new NNInfo(null));
	}

	public NNInfo QueryClosestXZ(Vector3 p, NNConstraint constraint, ref float distance, NNInfo previous)
	{
		if (count == 0)
		{
			return previous;
		}
		SearchBoxClosestXZ(0, p, ref distance, constraint, ref previous);
		return previous;
	}

	private void SearchBoxClosestXZ(int boxi, Vector3 p, ref float closestDist, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			Vector3 constClampedPosition = bBTreeBox.node.ClosestPointOnNodeXZ(p);
			float num = (constClampedPosition.x - p.x) * (constClampedPosition.x - p.x) + (constClampedPosition.z - p.z) * (constClampedPosition.z - p.z);
			if (constraint == null || constraint.Suitable(bBTreeBox.node))
			{
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = constClampedPosition;
					closestDist = (float)Math.Sqrt(num);
				}
				else if (num < closestDist * closestDist)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = constClampedPosition;
					closestDist = (float)Math.Sqrt(num);
				}
			}
		}
		else
		{
			if (RectIntersectsCircle(arr[bBTreeBox.left].rect, p, closestDist))
			{
				SearchBoxClosestXZ(bBTreeBox.left, p, ref closestDist, constraint, ref nnInfo);
			}
			if (RectIntersectsCircle(arr[bBTreeBox.right].rect, p, closestDist))
			{
				SearchBoxClosestXZ(bBTreeBox.right, p, ref closestDist, constraint, ref nnInfo);
			}
		}
	}

	public NNInfo QueryClosest(Vector3 p, NNConstraint constraint, ref float distance, NNInfo previous)
	{
		if (count == 0)
		{
			return previous;
		}
		SearchBoxClosest(0, p, ref distance, constraint, ref previous);
		return previous;
	}

	private void SearchBoxClosest(int boxi, Vector3 p, ref float closestDist, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (!NodeIntersectsCircle(bBTreeBox.node, p, closestDist))
			{
				return;
			}
			Vector3 vector = bBTreeBox.node.ClosestPointOnNode(p);
			float sqrMagnitude = (vector - p).sqrMagnitude;
			if (constraint == null || constraint.Suitable(bBTreeBox.node))
			{
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
					closestDist = (float)Math.Sqrt(sqrMagnitude);
				}
				else if (sqrMagnitude < closestDist * closestDist)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
					closestDist = (float)Math.Sqrt(sqrMagnitude);
				}
			}
		}
		else
		{
			if (RectIntersectsCircle(arr[bBTreeBox.left].rect, p, closestDist))
			{
				SearchBoxClosest(bBTreeBox.left, p, ref closestDist, constraint, ref nnInfo);
			}
			if (RectIntersectsCircle(arr[bBTreeBox.right].rect, p, closestDist))
			{
				SearchBoxClosest(bBTreeBox.right, p, ref closestDist, constraint, ref nnInfo);
			}
		}
	}

	public MeshNode QueryInside(Vector3 p, NNConstraint constraint)
	{
		if (count == 0)
		{
			return null;
		}
		return SearchBoxInside(0, p, constraint);
	}

	private MeshNode SearchBoxInside(int boxi, Vector3 p, NNConstraint constraint)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (bBTreeBox.node.ContainsPoint((Int3)p) && (constraint == null || constraint.Suitable(bBTreeBox.node)))
			{
				return bBTreeBox.node;
			}
		}
		else
		{
			if (arr[bBTreeBox.left].rect.Contains(new Vector2(p.x, p.z)))
			{
				MeshNode meshNode = SearchBoxInside(bBTreeBox.left, p, constraint);
				if (meshNode != null)
				{
					return meshNode;
				}
			}
			if (arr[bBTreeBox.right].rect.Contains(new Vector2(p.x, p.z)))
			{
				MeshNode meshNode = SearchBoxInside(bBTreeBox.right, p, constraint);
				if (meshNode != null)
				{
					return meshNode;
				}
			}
		}
		return null;
	}

	private void SearchBoxCircle(int boxi, Vector3 p, float radius, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (!NodeIntersectsCircle(bBTreeBox.node, p, radius))
			{
				return;
			}
			Vector3 vector = bBTreeBox.node.ClosestPointOnNode(p);
			float sqrMagnitude = (vector - p).sqrMagnitude;
			if (nnInfo.node == null)
			{
				nnInfo.node = bBTreeBox.node;
				nnInfo.clampedPosition = vector;
			}
			else if (sqrMagnitude < (nnInfo.clampedPosition - p).sqrMagnitude)
			{
				nnInfo.node = bBTreeBox.node;
				nnInfo.clampedPosition = vector;
			}
			if (constraint == null || constraint.Suitable(bBTreeBox.node))
			{
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
				}
				else if (sqrMagnitude < (nnInfo.constClampedPosition - p).sqrMagnitude)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
				}
			}
		}
		else
		{
			if (RectIntersectsCircle(arr[bBTreeBox.left].rect, p, radius))
			{
				SearchBoxCircle(bBTreeBox.left, p, radius, constraint, ref nnInfo);
			}
			if (RectIntersectsCircle(arr[bBTreeBox.right].rect, p, radius))
			{
				SearchBoxCircle(bBTreeBox.right, p, radius, constraint, ref nnInfo);
			}
		}
	}

	private void SearchBox(int boxi, Vector3 p, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (!bBTreeBox.node.ContainsPoint((Int3)p))
			{
				return;
			}
			if (nnInfo.node == null)
			{
				nnInfo.node = bBTreeBox.node;
			}
			else if (Mathf.Abs(((Vector3)bBTreeBox.node.position).y - p.y) < Mathf.Abs(((Vector3)nnInfo.node.position).y - p.y))
			{
				nnInfo.node = bBTreeBox.node;
			}
			if (constraint.Suitable(bBTreeBox.node))
			{
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
				}
				else if (Mathf.Abs((float)bBTreeBox.node.position.y - p.y) < Mathf.Abs((float)nnInfo.constrainedNode.position.y - p.y))
				{
					nnInfo.constrainedNode = bBTreeBox.node;
				}
			}
		}
		else
		{
			if (RectContains(arr[bBTreeBox.left].rect, p))
			{
				SearchBox(bBTreeBox.left, p, constraint, ref nnInfo);
			}
			if (RectContains(arr[bBTreeBox.right].rect, p))
			{
				SearchBox(bBTreeBox.right, p, constraint, ref nnInfo);
			}
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
		if (count != 0)
		{
		}
	}

	private void OnDrawGizmos(int boxi, int depth)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		Vector3 vector = new Vector3(bBTreeBox.rect.xMin, 0f, bBTreeBox.rect.yMin);
		Vector3 vector2 = new Vector3(bBTreeBox.rect.xMax, 0f, bBTreeBox.rect.yMax);
		Vector3 vector3 = (vector + vector2) * 0.5f;
		Vector3 size = (vector2 - vector3) * 2f;
		vector3.y += (float)depth * 0.2f;
		Gizmos.color = AstarMath.IntToColor(depth, 0.05f);
		Gizmos.DrawCube(vector3, size);
		if (bBTreeBox.node == null)
		{
			OnDrawGizmos(bBTreeBox.left, depth + 1);
			OnDrawGizmos(bBTreeBox.right, depth + 1);
		}
	}

	private static bool NodeIntersectsCircle(MeshNode node, Vector3 p, float radius)
	{
		if (float.IsPositiveInfinity(radius))
		{
			return true;
		}
		return (p - node.ClosestPointOnNode(p)).sqrMagnitude < radius * radius;
	}

	private static bool RectIntersectsCircle(Rect r, Vector3 p, float radius)
	{
		if (float.IsPositiveInfinity(radius))
		{
			return true;
		}
		Vector3 vector = p;
		p.x = Math.Max(p.x, r.xMin);
		p.x = Math.Min(p.x, r.xMax);
		p.z = Math.Max(p.z, r.yMin);
		p.z = Math.Min(p.z, r.yMax);
		return (p.x - vector.x) * (p.x - vector.x) + (p.z - vector.z) * (p.z - vector.z) < radius * radius;
	}

	private static bool RectContains(Rect r, Vector3 p)
	{
		return p.x >= r.xMin && p.x <= r.xMax && p.z >= r.yMin && p.z <= r.yMax;
	}

	private static float ExpansionRequired(Rect r, Rect r2)
	{
		float num = Math.Min(r.xMin, r2.xMin);
		float num2 = Math.Max(r.xMax, r2.xMax);
		float num3 = Math.Min(r.yMin, r2.yMin);
		float num4 = Math.Max(r.yMax, r2.yMax);
		return (num2 - num) * (num4 - num3) - RectArea(r);
	}

	private static Rect ExpandToContain(Rect r, Rect r2)
	{
		float xmin = Math.Min(r.xMin, r2.xMin);
		float xmax = Math.Max(r.xMax, r2.xMax);
		float ymin = Math.Min(r.yMin, r2.yMin);
		float ymax = Math.Max(r.yMax, r2.yMax);
		return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
	}

	private static float RectArea(Rect r)
	{
		return r.width * r.height;
	}
}
