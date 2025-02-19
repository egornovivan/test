using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public abstract class Path
{
	public PathHandler pathHandler;

	public OnPathDelegate callback;

	public OnPathDelegate immediateCallback;

	private PathState state;

	private object stateLock = new object();

	private PathCompleteState pathCompleteState;

	private string _errorLog = string.Empty;

	public List<GraphNode> path;

	public List<Vector3> vectorPath;

	protected float maxFrameTime;

	protected PathNode currentR;

	public float duration;

	public int searchIterations;

	public int searchedNodes;

	public DateTime callTime;

	public bool recycled;

	protected bool hasBeenReset;

	public NNConstraint nnConstraint = PathNNConstraint.Default;

	public Path next;

	public int walkabilityMask = -1;

	public int height;

	public int turnRadius;

	public int speed;

	public Heuristic heuristic;

	public float heuristicScale = 1f;

	public ushort pathID;

	protected GraphNode hTargetNode;

	protected Int3 hTarget;

	public int enabledTags = -1;

	private static readonly int[] ZeroTagPenalties = new int[32];

	protected int[] internalTagPenalties;

	protected int[] manualTagPenalties;

	private List<object> claimed = new List<object>();

	private bool releasedNotSilent;

	public PathCompleteState CompleteState
	{
		get
		{
			return pathCompleteState;
		}
		protected set
		{
			pathCompleteState = value;
		}
	}

	public bool error => CompleteState == PathCompleteState.Error;

	public string errorLog => _errorLog;

	public int width
	{
		get
		{
			return nnConstraint.pathWidth;
		}
		set
		{
			nnConstraint.pathWidth = ((value < 1) ? 1 : value);
		}
	}

	public int[] tagPenalties
	{
		get
		{
			return manualTagPenalties;
		}
		set
		{
			if (value == null || value.Length != 32)
			{
				manualTagPenalties = null;
				internalTagPenalties = ZeroTagPenalties;
			}
			else
			{
				manualTagPenalties = value;
				internalTagPenalties = value;
			}
		}
	}

	public virtual bool FloodingPath => false;

	public float GetTotalLength()
	{
		if (vectorPath == null)
		{
			return float.PositiveInfinity;
		}
		float num = 0f;
		for (int i = 0; i < vectorPath.Count - 1; i++)
		{
			num += Vector3.Distance(vectorPath[i], vectorPath[i + 1]);
		}
		return num;
	}

	public IEnumerator WaitForPath()
	{
		if (GetState() == PathState.Created)
		{
			throw new InvalidOperationException("This path has not been started yet");
		}
		while (GetState() != PathState.Returned)
		{
			yield return null;
		}
	}

	public uint CalculateHScore(GraphNode node)
	{
		switch (heuristic)
		{
		case Heuristic.Euclidean:
		{
			uint val = (uint)((float)(GetHTarget() - node.position).costMagnitude * heuristicScale);
			uint val2 = ((hTargetNode != null) ? AstarPath.active.euclideanEmbedding.GetHeuristic(node.NodeIndex, hTargetNode.NodeIndex) : 0u);
			return Math.Max(val, val2);
		}
		case Heuristic.Manhattan:
		{
			Int3 position = node.position;
			uint val = (uint)((float)(Math.Abs(hTarget.x - position.x) + Math.Abs(hTarget.y - position.y) + Math.Abs(hTarget.z - position.z)) * heuristicScale);
			uint val2 = ((hTargetNode != null) ? AstarPath.active.euclideanEmbedding.GetHeuristic(node.NodeIndex, hTargetNode.NodeIndex) : 0u);
			return Math.Max(val, val2);
		}
		case Heuristic.DiagonalManhattan:
		{
			Int3 @int = GetHTarget() - node.position;
			@int.x = Math.Abs(@int.x);
			@int.y = Math.Abs(@int.y);
			@int.z = Math.Abs(@int.z);
			int num = Math.Min(@int.x, @int.z);
			int num2 = Math.Max(@int.x, @int.z);
			uint val = (uint)((float)(14 * num / 10 + (num2 - num) + @int.y) * heuristicScale);
			uint val2 = ((hTargetNode != null) ? AstarPath.active.euclideanEmbedding.GetHeuristic(node.NodeIndex, hTargetNode.NodeIndex) : 0u);
			return Math.Max(val, val2);
		}
		default:
			return 0u;
		}
	}

	public uint GetTagPenalty(int tag)
	{
		return (uint)internalTagPenalties[tag];
	}

	public Int3 GetHTarget()
	{
		return hTarget;
	}

	public bool CanTraverse(GraphNode node)
	{
		return node.Walkable && ((enabledTags >> (int)node.Tag) & 1) != 0;
	}

	public uint GetTraversalCost(GraphNode node)
	{
		return GetTagPenalty((int)node.Tag) + node.Penalty;
	}

	public virtual uint GetConnectionSpecialCost(GraphNode a, GraphNode b, uint currentCost)
	{
		return currentCost;
	}

	public bool IsDone()
	{
		return CompleteState != PathCompleteState.NotCalculated;
	}

	public void AdvanceState(PathState s)
	{
		lock (stateLock)
		{
			state = (PathState)Math.Max((int)state, (int)s);
		}
	}

	public PathState GetState()
	{
		return state;
	}

	public void LogError(string msg)
	{
		if (AstarPath.isEditor || AstarPath.active.logPathResults != 0)
		{
			_errorLog += msg;
		}
		if (AstarPath.active.logPathResults != 0 && AstarPath.active.logPathResults != PathLog.InGame)
		{
			Debug.LogWarning(msg);
		}
	}

	public void ForceLogError(string msg)
	{
		Error();
		_errorLog += msg;
		Debug.LogError(msg);
	}

	public void Log(string msg)
	{
		if (AstarPath.isEditor || AstarPath.active.logPathResults != 0)
		{
			_errorLog += msg;
		}
	}

	public void Error()
	{
		CompleteState = PathCompleteState.Error;
	}

	private void ErrorCheck()
	{
		if (!hasBeenReset)
		{
			throw new Exception("The path has never been reset. Use pooling API or call Reset() after creating the path with the default constructor.");
		}
		if (recycled)
		{
			throw new Exception("The path is currently in a path pool. Are you sending the path for calculation twice?");
		}
		if (pathHandler == null)
		{
			throw new Exception("Field pathHandler is not set. Please report this bug.");
		}
		if (GetState() > PathState.Processing)
		{
			throw new Exception("This path has already been processed. Do not request a path with the same path object twice.");
		}
	}

	public virtual void OnEnterPool()
	{
		if (vectorPath != null)
		{
			ListPool<Vector3>.Release(vectorPath);
		}
		if (path != null)
		{
			ListPool<GraphNode>.Release(path);
		}
		vectorPath = null;
		path = null;
	}

	public virtual void Reset()
	{
		if (object.ReferenceEquals(AstarPath.active, null))
		{
			throw new NullReferenceException("No AstarPath object found in the scene. Make sure there is one or do not create paths in Awake");
		}
		hasBeenReset = true;
		state = PathState.Created;
		releasedNotSilent = false;
		pathHandler = null;
		callback = null;
		_errorLog = string.Empty;
		pathCompleteState = PathCompleteState.NotCalculated;
		path = ListPool<GraphNode>.Claim();
		vectorPath = ListPool<Vector3>.Claim();
		currentR = null;
		duration = 0f;
		searchIterations = 0;
		searchedNodes = 0;
		nnConstraint = PathNNConstraint.Default;
		next = null;
		width = 1;
		walkabilityMask = -1;
		height = 0;
		turnRadius = 0;
		speed = 0;
		heuristic = AstarPath.active.heuristic;
		heuristicScale = AstarPath.active.heuristicScale;
		pathID = 0;
		enabledTags = -1;
		tagPenalties = null;
		callTime = DateTime.UtcNow;
		pathID = AstarPath.active.GetNextPathID();
		hTarget = Int3.zero;
		hTargetNode = null;
	}

	protected bool HasExceededTime(int searchedNodes, long targetTime)
	{
		return DateTime.UtcNow.Ticks >= targetTime;
	}

	protected abstract void Recycle();

	public void Claim(object o)
	{
		if (object.ReferenceEquals(o, null))
		{
			throw new ArgumentNullException("o");
		}
		for (int i = 0; i < claimed.Count; i++)
		{
			if (object.ReferenceEquals(claimed[i], o))
			{
				Debug.LogError("You have already claimed the path with that object (" + o.ToString() + "). Are you claiming the path with the same object twice?");
				return;
			}
		}
		claimed.Add(o);
	}

	public void ReleaseSilent(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		for (int i = 0; i < claimed.Count; i++)
		{
			if (object.ReferenceEquals(claimed[i], o))
			{
				claimed.RemoveAt(i);
				if (releasedNotSilent && claimed.Count == 0)
				{
					Recycle();
				}
				return;
			}
		}
		if (claimed.Count == 0)
		{
			Debug.LogError("[AStar]You are releasing a path which is not claimed at all (most likely it has been pooled already). Are you releasing the path with the same object (" + o.ToString() + ") twice?");
		}
		else
		{
			Debug.LogError("[AStar]You are releasing a path which has not been claimed with this object (" + o.ToString() + "). Are you releasing the path with the same object twice?");
		}
	}

	public void Release(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		for (int i = 0; i < claimed.Count; i++)
		{
			if (object.ReferenceEquals(claimed[i], o))
			{
				claimed.RemoveAt(i);
				releasedNotSilent = true;
				if (claimed.Count == 0)
				{
					Recycle();
				}
				break;
			}
		}
	}

	protected virtual void Trace(PathNode from)
	{
		int num = 0;
		PathNode pathNode = from;
		while (pathNode != null)
		{
			pathNode = pathNode.parent;
			num++;
			if (num > 1024)
			{
				Debug.LogWarning("Inifinity loop? >1024 node path. Remove this message if you really have that long paths (Path.cs, Trace function)");
				break;
			}
		}
		if (path.Capacity < num)
		{
			path.Capacity = num;
		}
		if (vectorPath.Capacity < num)
		{
			vectorPath.Capacity = num;
		}
		pathNode = from;
		for (int i = 0; i < num; i++)
		{
			path.Add(pathNode.node);
			pathNode = pathNode.parent;
		}
		int num2 = num / 2;
		for (int j = 0; j < num2; j++)
		{
			GraphNode value = path[j];
			path[j] = path[num - j - 1];
			path[num - j - 1] = value;
		}
		for (int k = 0; k < num; k++)
		{
			if (nnConstraint.pathWidth > 1 && path[k] is GridNode)
			{
				GridGraph gridGraph = GridNode.GetGridGraph(path[k].GraphIndex);
				float num3 = (float)(nnConstraint.pathWidth - 1) * gridGraph.nodeSize * 0.5f;
				vectorPath.Add((Vector3)path[k].position - new Vector3(num3, 0f, num3));
			}
			else
			{
				vectorPath.Add((Vector3)path[k].position);
			}
		}
	}

	public virtual string DebugString(PathLog logMode)
	{
		if (logMode == PathLog.None || (!error && logMode == PathLog.OnlyErrors))
		{
			return string.Empty;
		}
		StringBuilder debugStringBuilder = pathHandler.DebugStringBuilder;
		debugStringBuilder.Length = 0;
		debugStringBuilder.Append((!error) ? "Path Completed : " : "Path Failed : ");
		debugStringBuilder.Append("Computation Time ");
		debugStringBuilder.Append(duration.ToString((logMode != PathLog.Heavy) ? "0.00 ms " : "0.000 ms "));
		debugStringBuilder.Append("Searched Nodes ");
		debugStringBuilder.Append(searchedNodes);
		if (!error)
		{
			debugStringBuilder.Append(" Path Length ");
			debugStringBuilder.Append((path != null) ? path.Count.ToString() : "Null");
			if (logMode == PathLog.Heavy)
			{
				debugStringBuilder.Append("\nSearch Iterations " + searchIterations);
			}
		}
		if (error)
		{
			debugStringBuilder.Append("\nError: ");
			debugStringBuilder.Append(errorLog);
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

	public virtual void ReturnPath()
	{
		if (callback != null)
		{
			callback(this);
		}
	}

	public void PrepareBase(PathHandler pathHandler)
	{
		if (pathHandler.PathID > pathID)
		{
			pathHandler.ClearPathIDs();
		}
		this.pathHandler = pathHandler;
		pathHandler.InitializeForPath(this);
		if (internalTagPenalties == null || internalTagPenalties.Length != 32)
		{
			internalTagPenalties = ZeroTagPenalties;
		}
		try
		{
			ErrorCheck();
		}
		catch (Exception ex)
		{
			ForceLogError("Exception in path " + pathID + "\n" + ex.ToString());
		}
	}

	public abstract void Prepare();

	public virtual void Cleanup()
	{
	}

	public abstract void Initialize();

	public abstract void CalculateStep(long targetTick);
}
