using System;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public class StartEndModifier : PathModifier
{
	public enum Exactness
	{
		SnapToNode,
		Original,
		Interpolate,
		ClosestOnNode
	}

	public bool addPoints;

	public Exactness exactStartPoint = Exactness.ClosestOnNode;

	public Exactness exactEndPoint = Exactness.ClosestOnNode;

	public bool useRaycasting;

	public LayerMask mask = -1;

	public bool useGraphRaycasting;

	public override ModifierData input => ModifierData.Vector;

	public override ModifierData output => (ModifierData)(((!addPoints) ? 4 : 0) | 8);

	public override void Apply(Path _p, ModifierData source)
	{
		if (!(_p is ABPath { width: <=1 } aBPath) || aBPath.vectorPath.Count == 0)
		{
			return;
		}
		if (aBPath.vectorPath.Count < 2 && !addPoints)
		{
			aBPath.vectorPath.Add(aBPath.vectorPath[0]);
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (exactStartPoint == Exactness.Original)
		{
			zero = GetClampedPoint((Vector3)aBPath.path[0].position, aBPath.originalStartPoint, aBPath.path[0]);
		}
		else if (exactStartPoint == Exactness.ClosestOnNode)
		{
			zero = GetClampedPoint((Vector3)aBPath.path[0].position, aBPath.startPoint, aBPath.path[0]);
		}
		else if (exactStartPoint == Exactness.Interpolate)
		{
			zero = GetClampedPoint((Vector3)aBPath.path[0].position, aBPath.originalStartPoint, aBPath.path[0]);
			zero = AstarMath.NearestPointStrict((Vector3)aBPath.path[0].position, (Vector3)aBPath.path[(1 < aBPath.path.Count) ? 1 : 0].position, zero);
		}
		else
		{
			zero = (Vector3)aBPath.path[0].position;
		}
		if (exactEndPoint == Exactness.Original)
		{
			zero2 = GetClampedPoint((Vector3)aBPath.path[aBPath.path.Count - 1].position, aBPath.originalEndPoint, aBPath.path[aBPath.path.Count - 1]);
		}
		else if (exactEndPoint == Exactness.ClosestOnNode)
		{
			zero2 = GetClampedPoint((Vector3)aBPath.path[aBPath.path.Count - 1].position, aBPath.endPoint, aBPath.path[aBPath.path.Count - 1]);
		}
		else if (exactEndPoint == Exactness.Interpolate)
		{
			zero2 = GetClampedPoint((Vector3)aBPath.path[aBPath.path.Count - 1].position, aBPath.originalEndPoint, aBPath.path[aBPath.path.Count - 1]);
			zero2 = AstarMath.NearestPointStrict((Vector3)aBPath.path[aBPath.path.Count - 1].position, (Vector3)aBPath.path[(aBPath.path.Count - 2 >= 0) ? (aBPath.path.Count - 2) : 0].position, zero2);
		}
		else
		{
			zero2 = (Vector3)aBPath.path[aBPath.path.Count - 1].position;
		}
		if (!addPoints)
		{
			aBPath.vectorPath[0] = zero;
			aBPath.vectorPath[aBPath.vectorPath.Count - 1] = zero2;
			return;
		}
		if (exactStartPoint != 0)
		{
			aBPath.vectorPath.Insert(0, zero);
		}
		if (exactEndPoint != 0)
		{
			aBPath.vectorPath.Add(zero2);
		}
	}

	public Vector3 GetClampedPoint(Vector3 from, Vector3 to, GraphNode hint)
	{
		Vector3 vector = to;
		if (useRaycasting && Physics.Linecast(from, to, out var hitInfo, mask))
		{
			vector = hitInfo.point;
		}
		if (useGraphRaycasting && hint != null)
		{
			NavGraph graph = AstarData.GetGraph(hint);
			if (graph != null && graph is IRaycastableGraph raycastableGraph && raycastableGraph.Linecast(from, vector, hint, out var hit))
			{
				vector = hit.point;
			}
		}
		return vector;
	}
}
