using System;
using System.Text;
using UnityEngine;

namespace Pathfinding;

public class ABPath : Path
{
	public bool recalcStartEndCosts = true;

	public GraphNode startNode;

	public GraphNode endNode;

	public GraphNode startHint;

	public GraphNode endHint;

	public Vector3 originalStartPoint;

	public Vector3 originalEndPoint;

	public Vector3 startPoint;

	public Vector3 endPoint;

	protected bool hasEndPoint = true;

	public Int3 startIntPoint;

	public bool calculatePartial;

	protected PathNode partialBestTarget;

	protected int[] endNodeCosts;

	[Obsolete("Use PathPool<T>.GetPath instead")]
	public ABPath(Vector3 start, Vector3 end, OnPathDelegate callbackDelegate)
	{
		Reset();
		Setup(start, end, callbackDelegate);
	}

	public ABPath()
	{
	}

	public static ABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
	{
		ABPath aBPath = PathPool<ABPath>.GetPath();
		aBPath.Setup(start, end, callback);
		return aBPath;
	}

	protected void Setup(Vector3 start, Vector3 end, OnPathDelegate callbackDelegate)
	{
		callback = callbackDelegate;
		UpdateStartEnd(start, end);
	}

	protected void UpdateStartEnd(Vector3 start, Vector3 end)
	{
		originalStartPoint = start;
		originalEndPoint = end;
		startPoint = start;
		endPoint = end;
		startIntPoint = (Int3)start;
		hTarget = (Int3)end;
	}

	public override uint GetConnectionSpecialCost(GraphNode a, GraphNode b, uint currentCost)
	{
		if (startNode != null && endNode != null)
		{
			if (a == startNode)
			{
				return (uint)((double)(startIntPoint - ((b != endNode) ? b.position : hTarget)).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
			}
			if (b == startNode)
			{
				return (uint)((double)(startIntPoint - ((a != endNode) ? a.position : hTarget)).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
			}
			if (a == endNode)
			{
				return (uint)((double)(hTarget - b.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
			}
			if (b == endNode)
			{
				return (uint)((double)(hTarget - a.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
			}
		}
		else
		{
			if (a == startNode)
			{
				return (uint)((double)(startIntPoint - b.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
			}
			if (b == startNode)
			{
				return (uint)((double)(startIntPoint - a.position).costMagnitude * ((double)currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
			}
		}
		return currentCost;
	}

	public override void Reset()
	{
		base.Reset();
		startNode = null;
		endNode = null;
		startHint = null;
		endHint = null;
		originalStartPoint = Vector3.zero;
		originalEndPoint = Vector3.zero;
		startPoint = Vector3.zero;
		endPoint = Vector3.zero;
		calculatePartial = false;
		partialBestTarget = null;
		hasEndPoint = true;
		startIntPoint = default(Int3);
		hTarget = default(Int3);
		endNodeCosts = null;
	}

	public override void Prepare()
	{
		nnConstraint.tags = enabledTags;
		NNInfo nearest = AstarPath.active.GetNearest(startPoint, nnConstraint, startHint);
		if (nnConstraint is PathNNConstraint pathNNConstraint)
		{
			pathNNConstraint.SetStart(nearest.node);
		}
		startPoint = nearest.clampedPosition;
		startIntPoint = (Int3)startPoint;
		startNode = nearest.node;
		if (hasEndPoint)
		{
			NNInfo nearest2 = AstarPath.active.GetNearest(endPoint, nnConstraint, endHint);
			endPoint = nearest2.clampedPosition;
			hTarget = (Int3)endPoint;
			endNode = nearest2.node;
			hTargetNode = endNode;
		}
		if (startNode == null && hasEndPoint && endNode == null)
		{
			Error();
			LogError("Couldn't find close nodes to the start point or the end point");
		}
		else if (startNode == null)
		{
			Error();
			LogError("Couldn't find a close node to the start point");
		}
		else if (endNode == null && hasEndPoint)
		{
			Error();
			LogError("Couldn't find a close node to the end point");
		}
		else if (!startNode.Walkable)
		{
			Error();
			LogError("The node closest to the start point is not walkable");
		}
		else if (hasEndPoint && !endNode.Walkable)
		{
			Error();
			LogError("The node closest to the end point is not walkable");
		}
		else if (hasEndPoint && startNode.Area != endNode.Area)
		{
			Error();
			LogError("There is no valid path to the target (start area: " + startNode.Area + ", target area: " + endNode.Area + ")");
		}
	}

	public override void Initialize()
	{
		if (startNode != null)
		{
			pathHandler.GetPathNode(startNode).flag2 = true;
		}
		if (endNode != null)
		{
			pathHandler.GetPathNode(endNode).flag2 = true;
		}
		if (hasEndPoint && startNode == endNode)
		{
			PathNode pathNode = pathHandler.GetPathNode(endNode);
			pathNode.node = endNode;
			pathNode.parent = null;
			pathNode.H = 0u;
			pathNode.G = 0u;
			Trace(pathNode);
			base.CompleteState = PathCompleteState.Complete;
			return;
		}
		PathNode pathNode2 = pathHandler.GetPathNode(startNode);
		pathNode2.node = startNode;
		pathNode2.pathID = pathHandler.PathID;
		pathNode2.parent = null;
		pathNode2.cost = 0u;
		pathNode2.G = GetTraversalCost(startNode);
		pathNode2.H = CalculateHScore(startNode);
		startNode.Open(this, pathNode2, pathHandler);
		searchedNodes++;
		partialBestTarget = pathNode2;
		if (pathHandler.HeapEmpty())
		{
			if (!calculatePartial)
			{
				Error();
				LogError("No open points, the start node didn't open any nodes");
				return;
			}
			base.CompleteState = PathCompleteState.Partial;
			Trace(partialBestTarget);
		}
		currentR = pathHandler.PopNode();
	}

	public override void Cleanup()
	{
		if (startNode != null)
		{
			pathHandler.GetPathNode(startNode).flag2 = false;
		}
		if (endNode != null)
		{
			pathHandler.GetPathNode(endNode).flag2 = false;
		}
	}

	public override void CalculateStep(long targetTick)
	{
		int num = 0;
		while (base.CompleteState == PathCompleteState.NotCalculated)
		{
			searchedNodes++;
			if (currentR.node == endNode)
			{
				base.CompleteState = PathCompleteState.Complete;
				break;
			}
			if (currentR.H < partialBestTarget.H)
			{
				partialBestTarget = currentR;
			}
			currentR.node.Open(this, currentR, pathHandler);
			if (pathHandler.HeapEmpty())
			{
				Error();
				LogError("No open points, whole area searched");
				return;
			}
			currentR = pathHandler.PopNode();
			if (num > 500)
			{
				if (DateTime.UtcNow.Ticks >= targetTick)
				{
					return;
				}
				num = 0;
				if (searchedNodes > 1000000)
				{
					throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
				}
			}
			num++;
		}
		if (base.CompleteState == PathCompleteState.Complete)
		{
			Trace(currentR);
		}
		else if (calculatePartial && partialBestTarget != null)
		{
			base.CompleteState = PathCompleteState.Partial;
			Trace(partialBestTarget);
		}
	}

	public void ResetCosts(Path p)
	{
	}

	public override string DebugString(PathLog logMode)
	{
		if (logMode == PathLog.None || (!base.error && logMode == PathLog.OnlyErrors))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((!base.error) ? "Path Completed : " : "Path Failed : ");
		stringBuilder.Append("Computation Time ");
		stringBuilder.Append(duration.ToString((logMode != PathLog.Heavy) ? "0.00" : "0.000"));
		stringBuilder.Append(" ms Searched Nodes ");
		stringBuilder.Append(searchedNodes);
		if (!base.error)
		{
			stringBuilder.Append(" Path Length ");
			stringBuilder.Append((path != null) ? path.Count.ToString() : "Null");
			if (logMode == PathLog.Heavy)
			{
				stringBuilder.Append("\nSearch Iterations " + searchIterations);
				if (hasEndPoint && endNode != null)
				{
					PathNode pathNode = pathHandler.GetPathNode(endNode);
					stringBuilder.Append("\nEnd Node\n\tG: ");
					stringBuilder.Append(pathNode.G);
					stringBuilder.Append("\n\tH: ");
					stringBuilder.Append(pathNode.H);
					stringBuilder.Append("\n\tF: ");
					stringBuilder.Append(pathNode.F);
					stringBuilder.Append("\n\tPoint: ");
					stringBuilder.Append(endPoint.ToString());
					stringBuilder.Append("\n\tGraph: ");
					stringBuilder.Append(endNode.GraphIndex);
				}
				stringBuilder.Append("\nStart Node");
				stringBuilder.Append("\n\tPoint: ");
				stringBuilder.Append(startPoint.ToString());
				stringBuilder.Append("\n\tGraph: ");
				if (startNode != null)
				{
					stringBuilder.Append(startNode.GraphIndex);
				}
				else
				{
					stringBuilder.Append("< null startNode >");
				}
			}
		}
		if (base.error)
		{
			stringBuilder.Append("\nError: ");
			stringBuilder.Append(base.errorLog);
		}
		if (logMode == PathLog.Heavy && !AstarPath.IsUsingMultithreading)
		{
			stringBuilder.Append("\nCallback references ");
			if (callback != null)
			{
				stringBuilder.Append(callback.Target.GetType().FullName).AppendLine();
			}
			else
			{
				stringBuilder.AppendLine("NULL");
			}
		}
		stringBuilder.Append("\nPath Number ");
		stringBuilder.Append(pathID);
		return stringBuilder.ToString();
	}

	protected override void Recycle()
	{
		PathPool<ABPath>.Recycle(this);
	}

	public Vector3 GetMovementVector(Vector3 point)
	{
		if (vectorPath == null || vectorPath.Count == 0)
		{
			return Vector3.zero;
		}
		if (vectorPath.Count == 1)
		{
			return vectorPath[0] - point;
		}
		float num = float.PositiveInfinity;
		int num2 = 0;
		for (int i = 0; i < vectorPath.Count - 1; i++)
		{
			Vector3 vector = AstarMath.NearestPointStrict(vectorPath[i], vectorPath[i + 1], point);
			float sqrMagnitude = (vector - point).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				num2 = i;
			}
		}
		return vectorPath[num2 + 1] - point;
	}
}
