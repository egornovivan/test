using System;
using System.Collections.Generic;
using System.Text;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public class MultiTargetPath : ABPath
{
	public enum HeuristicMode
	{
		None,
		Average,
		MovingAverage,
		Midpoint,
		MovingMidpoint,
		Sequential
	}

	public OnPathDelegate[] callbacks;

	public GraphNode[] targetNodes;

	protected int targetNodeCount;

	public bool[] targetsFound;

	public Vector3[] targetPoints;

	public Vector3[] originalTargetPoints;

	public List<Vector3>[] vectorPaths;

	public List<GraphNode>[] nodePaths;

	public int endsFound;

	public bool pathsForAll = true;

	public int chosenTarget = -1;

	public int sequentialTarget;

	public HeuristicMode heuristicMode = HeuristicMode.Sequential;

	public bool inverted = true;

	public MultiTargetPath()
	{
	}

	[Obsolete("Please use the Construct method instead")]
	public MultiTargetPath(Vector3[] startPoints, Vector3 target, OnPathDelegate[] callbackDelegates, OnPathDelegate callbackDelegate = null)
		: this(target, startPoints, callbackDelegates, callbackDelegate)
	{
		inverted = true;
	}

	[Obsolete("Please use the Construct method instead")]
	public MultiTargetPath(Vector3 start, Vector3[] targets, OnPathDelegate[] callbackDelegates, OnPathDelegate callbackDelegate = null)
	{
	}

	public static MultiTargetPath Construct(Vector3[] startPoints, Vector3 target, OnPathDelegate[] callbackDelegates, OnPathDelegate callback = null)
	{
		MultiTargetPath multiTargetPath = Construct(target, startPoints, callbackDelegates, callback);
		multiTargetPath.inverted = true;
		return multiTargetPath;
	}

	public static MultiTargetPath Construct(Vector3 start, Vector3[] targets, OnPathDelegate[] callbackDelegates, OnPathDelegate callback = null)
	{
		MultiTargetPath multiTargetPath = PathPool<MultiTargetPath>.GetPath();
		multiTargetPath.Setup(start, targets, callbackDelegates, callback);
		return multiTargetPath;
	}

	protected void Setup(Vector3 start, Vector3[] targets, OnPathDelegate[] callbackDelegates, OnPathDelegate callback)
	{
		inverted = false;
		base.callback = callback;
		callbacks = callbackDelegates;
		targetPoints = targets;
		originalStartPoint = start;
		startPoint = start;
		startIntPoint = (Int3)start;
		if (targets.Length == 0)
		{
			Error();
			LogError("No targets were assigned to the MultiTargetPath");
			return;
		}
		endPoint = targets[0];
		originalTargetPoints = new Vector3[targetPoints.Length];
		for (int i = 0; i < targetPoints.Length; i++)
		{
			ref Vector3 reference = ref originalTargetPoints[i];
			reference = targetPoints[i];
		}
	}

	protected override void Recycle()
	{
		PathPool<MultiTargetPath>.Recycle(this);
	}

	public override void OnEnterPool()
	{
		if (vectorPaths != null)
		{
			for (int i = 0; i < vectorPaths.Length; i++)
			{
				if (vectorPaths[i] != null)
				{
					ListPool<Vector3>.Release(vectorPaths[i]);
				}
			}
		}
		vectorPaths = null;
		vectorPath = null;
		if (nodePaths != null)
		{
			for (int j = 0; j < nodePaths.Length; j++)
			{
				if (nodePaths[j] != null)
				{
					ListPool<GraphNode>.Release(nodePaths[j]);
				}
			}
		}
		nodePaths = null;
		path = null;
		base.OnEnterPool();
	}

	public override void ReturnPath()
	{
		if (base.error)
		{
			if (callbacks != null)
			{
				for (int i = 0; i < callbacks.Length; i++)
				{
					if (callbacks[i] != null)
					{
						callbacks[i](this);
					}
				}
			}
			if (callback != null)
			{
				callback(this);
			}
			return;
		}
		bool flag = false;
		Vector3 vector = originalStartPoint;
		Vector3 vector2 = startPoint;
		GraphNode graphNode = startNode;
		for (int j = 0; j < nodePaths.Length; j++)
		{
			path = nodePaths[j];
			if (path != null)
			{
				base.CompleteState = PathCompleteState.Complete;
				flag = true;
			}
			else
			{
				base.CompleteState = PathCompleteState.Error;
			}
			if (callbacks != null && callbacks[j] != null)
			{
				vectorPath = vectorPaths[j];
				if (inverted)
				{
					endPoint = vector2;
					endNode = graphNode;
					startNode = targetNodes[j];
					startPoint = targetPoints[j];
					originalEndPoint = vector;
					originalStartPoint = originalTargetPoints[j];
				}
				else
				{
					endPoint = targetPoints[j];
					originalEndPoint = originalTargetPoints[j];
					endNode = targetNodes[j];
				}
				callbacks[j](this);
				vectorPaths[j] = vectorPath;
			}
		}
		if (flag)
		{
			base.CompleteState = PathCompleteState.Complete;
			if (!pathsForAll)
			{
				path = nodePaths[chosenTarget];
				vectorPath = vectorPaths[chosenTarget];
				if (inverted)
				{
					endPoint = vector2;
					endNode = graphNode;
					startNode = targetNodes[chosenTarget];
					startPoint = targetPoints[chosenTarget];
					originalEndPoint = vector;
					originalStartPoint = originalTargetPoints[chosenTarget];
				}
				else
				{
					endPoint = targetPoints[chosenTarget];
					originalEndPoint = originalTargetPoints[chosenTarget];
					endNode = targetNodes[chosenTarget];
				}
			}
		}
		else
		{
			base.CompleteState = PathCompleteState.Error;
		}
		if (callback != null)
		{
			callback(this);
		}
	}

	public void FoundTarget(PathNode nodeR, int i)
	{
		nodeR.flag1 = false;
		Trace(nodeR);
		vectorPaths[i] = vectorPath;
		nodePaths[i] = path;
		vectorPath = ListPool<Vector3>.Claim();
		path = ListPool<GraphNode>.Claim();
		targetsFound[i] = true;
		targetNodeCount--;
		if (!pathsForAll)
		{
			base.CompleteState = PathCompleteState.Complete;
			chosenTarget = i;
			targetNodeCount = 0;
		}
		else if (targetNodeCount <= 0)
		{
			base.CompleteState = PathCompleteState.Complete;
		}
		else if (heuristicMode == HeuristicMode.MovingAverage)
		{
			Vector3 zero = Vector3.zero;
			int num = 0;
			for (int j = 0; j < targetPoints.Length; j++)
			{
				if (!targetsFound[j])
				{
					zero += (Vector3)targetNodes[j].position;
					num++;
				}
			}
			if (num > 0)
			{
				zero /= (float)num;
			}
			hTarget = (Int3)zero;
			RebuildOpenList();
		}
		else if (heuristicMode == HeuristicMode.MovingMidpoint)
		{
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			bool flag = false;
			for (int k = 0; k < targetPoints.Length; k++)
			{
				if (!targetsFound[k])
				{
					if (!flag)
					{
						vector = (Vector3)targetNodes[k].position;
						vector2 = (Vector3)targetNodes[k].position;
						flag = true;
					}
					else
					{
						vector = Vector3.Min((Vector3)targetNodes[k].position, vector);
						vector2 = Vector3.Max((Vector3)targetNodes[k].position, vector2);
					}
				}
			}
			Int3 @int = (Int3)((vector + vector2) * 0.5f);
			hTarget = @int;
			RebuildOpenList();
		}
		else
		{
			if (heuristicMode != HeuristicMode.Sequential || sequentialTarget != i)
			{
				return;
			}
			float num2 = 0f;
			for (int l = 0; l < targetPoints.Length; l++)
			{
				if (!targetsFound[l])
				{
					float sqrMagnitude = (targetNodes[l].position - startNode.position).sqrMagnitude;
					if (sqrMagnitude > num2)
					{
						num2 = sqrMagnitude;
						hTarget = (Int3)targetPoints[l];
						sequentialTarget = l;
					}
				}
			}
			RebuildOpenList();
		}
	}

	protected void RebuildOpenList()
	{
		BinaryHeapM heap = pathHandler.GetHeap();
		for (int i = 0; i < heap.numberOfItems; i++)
		{
			PathNode node = heap.GetNode(i);
			node.H = CalculateHScore(node.node);
			heap.SetF(i, node.F);
		}
		pathHandler.RebuildHeap();
	}

	public override void Prepare()
	{
		nnConstraint.tags = enabledTags;
		NNInfo nearest = AstarPath.active.GetNearest(startPoint, nnConstraint, startHint);
		startNode = nearest.node;
		if (startNode == null)
		{
			LogError("Could not find start node for multi target path");
			Error();
			return;
		}
		if (!startNode.Walkable)
		{
			LogError("Nearest node to the start point is not walkable");
			Error();
			return;
		}
		if (nnConstraint is PathNNConstraint pathNNConstraint)
		{
			pathNNConstraint.SetStart(nearest.node);
		}
		vectorPaths = new List<Vector3>[targetPoints.Length];
		nodePaths = new List<GraphNode>[targetPoints.Length];
		targetNodes = new GraphNode[targetPoints.Length];
		targetsFound = new bool[targetPoints.Length];
		targetNodeCount = targetPoints.Length;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < targetPoints.Length; i++)
		{
			NNInfo nearest2 = AstarPath.active.GetNearest(targetPoints[i], nnConstraint);
			targetNodes[i] = nearest2.node;
			ref Vector3 reference = ref targetPoints[i];
			reference = nearest2.clampedPosition;
			if (targetNodes[i] != null)
			{
				flag3 = true;
				endNode = targetNodes[i];
			}
			bool flag4 = false;
			if (nearest2.node != null && nearest2.node.Walkable)
			{
				flag = true;
			}
			else
			{
				flag4 = true;
			}
			if (nearest2.node != null && nearest2.node.Area == startNode.Area)
			{
				flag2 = true;
			}
			else
			{
				flag4 = true;
			}
			if (flag4)
			{
				targetsFound[i] = true;
				targetNodeCount--;
			}
		}
		startPoint = nearest.clampedPosition;
		startIntPoint = (Int3)startPoint;
		if (startNode == null || !flag3)
		{
			LogError("Couldn't find close nodes to either the start or the end (start = " + ((startNode == null) ? "not found" : "found") + " end = " + ((!flag3) ? "none found" : "at least one found") + ")");
			Error();
		}
		else if (!startNode.Walkable)
		{
			LogError("The node closest to the start point is not walkable");
			Error();
		}
		else if (!flag)
		{
			LogError("No target nodes were walkable");
			Error();
		}
		else if (!flag2)
		{
			LogError("There are no valid paths to the targets");
			Error();
		}
		else if (pathsForAll)
		{
			if (heuristicMode == HeuristicMode.None)
			{
				heuristic = Heuristic.None;
				heuristicScale = 0f;
			}
			else if (heuristicMode == HeuristicMode.Average || heuristicMode == HeuristicMode.MovingAverage)
			{
				Vector3 zero = Vector3.zero;
				for (int j = 0; j < targetNodes.Length; j++)
				{
					zero += (Vector3)targetNodes[j].position;
				}
				zero /= (float)targetNodes.Length;
				hTarget = (Int3)zero;
			}
			else if (heuristicMode == HeuristicMode.Midpoint || heuristicMode == HeuristicMode.MovingMidpoint)
			{
				Vector3 vector = Vector3.zero;
				Vector3 vector2 = Vector3.zero;
				bool flag5 = false;
				for (int k = 0; k < targetPoints.Length; k++)
				{
					if (!targetsFound[k])
					{
						if (!flag5)
						{
							vector = (Vector3)targetNodes[k].position;
							vector2 = (Vector3)targetNodes[k].position;
							flag5 = true;
						}
						else
						{
							vector = Vector3.Min((Vector3)targetNodes[k].position, vector);
							vector2 = Vector3.Max((Vector3)targetNodes[k].position, vector2);
						}
					}
				}
				Vector3 vector3 = (vector + vector2) * 0.5f;
				hTarget = (Int3)vector3;
			}
			else
			{
				if (heuristicMode != HeuristicMode.Sequential)
				{
					return;
				}
				float num = 0f;
				for (int l = 0; l < targetNodes.Length; l++)
				{
					if (!targetsFound[l])
					{
						float sqrMagnitude = (targetNodes[l].position - startNode.position).sqrMagnitude;
						if (sqrMagnitude > num)
						{
							num = sqrMagnitude;
							hTarget = (Int3)targetPoints[l];
							sequentialTarget = l;
						}
					}
				}
			}
		}
		else
		{
			heuristic = Heuristic.None;
			heuristicScale = 0f;
		}
	}

	public override void Initialize()
	{
		for (int i = 0; i < targetNodes.Length; i++)
		{
			if (startNode == targetNodes[i])
			{
				PathNode pathNode = pathHandler.GetPathNode(startNode);
				FoundTarget(pathNode, i);
			}
			else if (targetNodes[i] != null)
			{
				pathHandler.GetPathNode(targetNodes[i]).flag1 = true;
			}
		}
		AstarPath.OnPathPostSearch = (OnPathDelegate)Delegate.Combine(AstarPath.OnPathPostSearch, new OnPathDelegate(ResetFlags));
		if (targetNodeCount <= 0)
		{
			base.CompleteState = PathCompleteState.Complete;
			return;
		}
		PathNode pathNode2 = pathHandler.GetPathNode(startNode);
		pathNode2.node = startNode;
		pathNode2.pathID = pathID;
		pathNode2.parent = null;
		pathNode2.cost = 0u;
		pathNode2.G = GetTraversalCost(startNode);
		pathNode2.H = CalculateHScore(startNode);
		startNode.Open(this, pathNode2, pathHandler);
		searchedNodes++;
		if (pathHandler.HeapEmpty())
		{
			LogError("No open points, the start node didn't open any nodes");
			Error();
		}
		else
		{
			currentR = pathHandler.PopNode();
		}
	}

	public void ResetFlags(Path p)
	{
		AstarPath.OnPathPostSearch = (OnPathDelegate)Delegate.Remove(AstarPath.OnPathPostSearch, new OnPathDelegate(ResetFlags));
		if (p != this)
		{
			Debug.LogError("This should have been cleared after it was called on 'this' path. Was it not called? Or did the delegate reset not work?");
		}
		for (int i = 0; i < targetNodes.Length; i++)
		{
			if (targetNodes[i] != null)
			{
				pathHandler.GetPathNode(targetNodes[i]).flag1 = false;
			}
		}
	}

	public override void CalculateStep(long targetTick)
	{
		int num = 0;
		while (base.CompleteState == PathCompleteState.NotCalculated)
		{
			searchedNodes++;
			if (currentR.flag1)
			{
				for (int i = 0; i < targetNodes.Length; i++)
				{
					if (!targetsFound[i] && currentR.node == targetNodes[i])
					{
						FoundTarget(currentR, i);
						if (base.CompleteState != 0)
						{
							break;
						}
					}
				}
				if (targetNodeCount <= 0)
				{
					base.CompleteState = PathCompleteState.Complete;
					break;
				}
			}
			currentR.node.Open(this, currentR, pathHandler);
			if (pathHandler.HeapEmpty())
			{
				base.CompleteState = PathCompleteState.Complete;
				break;
			}
			currentR = pathHandler.PopNode();
			if (num > 500)
			{
				if (DateTime.UtcNow.Ticks >= targetTick)
				{
					break;
				}
				num = 0;
			}
			num++;
		}
	}

	protected override void Trace(PathNode node)
	{
		base.Trace(node);
		if (inverted)
		{
			int num = path.Count / 2;
			for (int i = 0; i < num; i++)
			{
				GraphNode value = path[i];
				path[i] = path[path.Count - i - 1];
				path[path.Count - i - 1] = value;
			}
			for (int j = 0; j < num; j++)
			{
				Vector3 value2 = vectorPath[j];
				vectorPath[j] = vectorPath[vectorPath.Count - j - 1];
				vectorPath[vectorPath.Count - j - 1] = value2;
			}
		}
	}

	public override string DebugString(PathLog logMode)
	{
		if (logMode == PathLog.None || (!base.error && logMode == PathLog.OnlyErrors))
		{
			return string.Empty;
		}
		StringBuilder debugStringBuilder = pathHandler.DebugStringBuilder;
		debugStringBuilder.Length = 0;
		debugStringBuilder.Append((!base.error) ? "Path Completed : " : "Path Failed : ");
		debugStringBuilder.Append("Computation Time ");
		debugStringBuilder.Append(duration.ToString((logMode != PathLog.Heavy) ? "0.00" : "0.000"));
		debugStringBuilder.Append(" ms Searched Nodes ");
		debugStringBuilder.Append(searchedNodes);
		if (!base.error)
		{
			debugStringBuilder.Append("\nLast Found Path Length ");
			debugStringBuilder.Append((path != null) ? path.Count.ToString() : "Null");
			if (logMode == PathLog.Heavy)
			{
				debugStringBuilder.Append("\nSearch Iterations " + searchIterations);
				debugStringBuilder.Append("\nPaths (").Append(targetsFound.Length).Append("):");
				for (int i = 0; i < targetsFound.Length; i++)
				{
					debugStringBuilder.Append("\n\n\tPath " + i).Append(" Found: ").Append(targetsFound[i]);
					GraphNode graphNode = ((nodePaths[i] != null) ? nodePaths[i][nodePaths[i].Count - 1] : null);
					if (nodePaths[i] == null)
					{
						continue;
					}
					debugStringBuilder.Append("\n\t\tLength: ");
					debugStringBuilder.Append(nodePaths[i].Count);
					if (graphNode != null)
					{
						PathNode pathNode = pathHandler.GetPathNode(endNode);
						if (pathNode != null)
						{
							debugStringBuilder.Append("\n\t\tEnd Node");
							debugStringBuilder.Append("\n\t\t\tG: ");
							debugStringBuilder.Append(pathNode.G);
							debugStringBuilder.Append("\n\t\t\tH: ");
							debugStringBuilder.Append(pathNode.H);
							debugStringBuilder.Append("\n\t\t\tF: ");
							debugStringBuilder.Append(pathNode.F);
							debugStringBuilder.Append("\n\t\t\tPoint: ");
							debugStringBuilder.Append(endPoint.ToString());
							debugStringBuilder.Append("\n\t\t\tGraph: ");
							debugStringBuilder.Append(endNode.GraphIndex);
						}
						else
						{
							debugStringBuilder.Append("\n\t\tEnd Node: Null");
						}
					}
				}
				debugStringBuilder.Append("\nStart Node");
				debugStringBuilder.Append("\n\tPoint: ");
				debugStringBuilder.Append(endPoint.ToString());
				debugStringBuilder.Append("\n\tGraph: ");
				debugStringBuilder.Append(startNode.GraphIndex);
				debugStringBuilder.Append("\nBinary Heap size at completion: ");
				debugStringBuilder.AppendLine((pathHandler.GetHeap() != null) ? (pathHandler.GetHeap().numberOfItems - 2).ToString() : "Null");
			}
		}
		if (base.error)
		{
			debugStringBuilder.Append("\nError: ");
			debugStringBuilder.Append(base.errorLog);
			debugStringBuilder.AppendLine();
		}
		if (logMode == PathLog.Heavy && !AstarPath.IsUsingMultithreading)
		{
			debugStringBuilder.Append("\nCallback references ");
			if (callback != null)
			{
				debugStringBuilder.Append(callback.Target.GetType().FullName).AppendLine();
			}
			else
			{
				debugStringBuilder.AppendLine("NULL");
			}
		}
		debugStringBuilder.Append("\nPath Number ");
		debugStringBuilder.Append(pathID);
		return debugStringBuilder.ToString();
	}
}
