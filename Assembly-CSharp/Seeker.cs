using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[AddComponentMenu("Pathfinding/Seeker")]
public class Seeker : MonoBehaviour
{
	public enum ModifierPass
	{
		PreProcess,
		PostProcessOriginal,
		PostProcess
	}

	public int curSeekerSize;

	public int curGraphMask = -1;

	public bool drawGizmos = true;

	public bool detailedGizmos;

	[HideInInspector]
	public bool saveGetNearestHints = true;

	public StartEndModifier startEndModifier = new StartEndModifier();

	[HideInInspector]
	public TagMask traversableTags = new TagMask(-1, -1);

	[HideInInspector]
	public int[] tagPenalties = new int[32];

	public OnPathDelegate pathCallback;

	public OnPathDelegate preProcessPath;

	public OnPathDelegate postProcessOriginalPath;

	public OnPathDelegate postProcessPath;

	[NonSerialized]
	public List<Vector3> lastCompletedVectorPath;

	[NonSerialized]
	public List<GraphNode> lastCompletedNodePath;

	[NonSerialized]
	protected Path path;

	private Path prevPath;

	private GraphNode startHint;

	private GraphNode endHint;

	private OnPathDelegate onPathDelegate;

	private OnPathDelegate onPartialPathDelegate;

	private OnPathDelegate tmpPathCallback;

	protected uint lastPathID;

	private List<IPathModifier> modifiers = new List<IPathModifier>();

	public Path GetCurrentPath()
	{
		return path;
	}

	public void Awake()
	{
		onPathDelegate = OnPathComplete;
		onPartialPathDelegate = OnPartialPathComplete;
		startEndModifier.Awake(this);
	}

	public void OnDestroy()
	{
		ReleaseClaimedPath();
		startEndModifier.OnDestroy(this);
	}

	public void ReleaseClaimedPath()
	{
		if (prevPath != null)
		{
			prevPath.ReleaseSilent(this);
			prevPath = null;
		}
	}

	public void RegisterModifier(IPathModifier mod)
	{
		if (modifiers == null)
		{
			modifiers = new List<IPathModifier>(1);
		}
		modifiers.Add(mod);
	}

	public void DeregisterModifier(IPathModifier mod)
	{
		if (modifiers != null)
		{
			modifiers.Remove(mod);
		}
	}

	public void PostProcess(Path p)
	{
		RunModifiers(ModifierPass.PostProcess, p);
	}

	public void RunModifiers(ModifierPass pass, Path p)
	{
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int i = 0; i < modifiers.Count - 1; i++)
			{
				if (modifiers[i].Priority < modifiers[i + 1].Priority)
				{
					IPathModifier value = modifiers[i];
					modifiers[i] = modifiers[i + 1];
					modifiers[i + 1] = value;
					flag = true;
				}
			}
		}
		switch (pass)
		{
		case ModifierPass.PreProcess:
			if (preProcessPath != null)
			{
				preProcessPath(p);
			}
			break;
		case ModifierPass.PostProcessOriginal:
			if (postProcessOriginalPath != null)
			{
				postProcessOriginalPath(p);
			}
			break;
		case ModifierPass.PostProcess:
			if (postProcessPath != null)
			{
				postProcessPath(p);
			}
			break;
		}
		if (modifiers.Count == 0)
		{
			return;
		}
		ModifierData modifierData = ModifierData.All;
		IPathModifier pathModifier = modifiers[0];
		for (int j = 0; j < modifiers.Count; j++)
		{
			MonoModifier monoModifier = modifiers[j] as MonoModifier;
			if (monoModifier != null && !monoModifier.enabled)
			{
				continue;
			}
			switch (pass)
			{
			case ModifierPass.PreProcess:
				modifiers[j].PreProcess(p);
				break;
			case ModifierPass.PostProcessOriginal:
				modifiers[j].ApplyOriginal(p);
				break;
			case ModifierPass.PostProcess:
			{
				ModifierData modifierData2 = ModifierConverter.Convert(p, modifierData, modifiers[j].input);
				if (modifierData2 != 0)
				{
					modifiers[j].Apply(p, modifierData2);
					modifierData = modifiers[j].output;
				}
				else
				{
					Debug.Log("Error converting " + ((j <= 0) ? "original" : pathModifier.GetType().Name) + "'s output to " + modifiers[j].GetType().Name + "'s input.\nTry rearranging the modifier priorities on the Seeker.");
					modifierData = ModifierData.None;
				}
				pathModifier = modifiers[j];
				break;
			}
			}
			if (modifierData == ModifierData.None)
			{
				break;
			}
		}
	}

	public bool IsDone()
	{
		return path == null || path.GetState() >= PathState.Returned;
	}

	public void OnPathComplete(Path p)
	{
		OnPathComplete(p, runModifiers: true, sendCallbacks: true);
	}

	public void OnPathComplete(Path p, bool runModifiers, bool sendCallbacks)
	{
		if ((p != null && p != path && sendCallbacks) || this == null || p == null || p != path)
		{
			return;
		}
		if (!path.error && runModifiers)
		{
			RunModifiers(ModifierPass.PostProcessOriginal, path);
			RunModifiers(ModifierPass.PostProcess, path);
		}
		if (sendCallbacks)
		{
			p.Claim(this);
			lastCompletedNodePath = p.path;
			lastCompletedVectorPath = p.vectorPath;
			if (tmpPathCallback != null)
			{
				tmpPathCallback(p);
			}
			if (pathCallback != null)
			{
				pathCallback(p);
			}
			if (prevPath != null)
			{
				prevPath.ReleaseSilent(this);
			}
			prevPath = p;
			if (!drawGizmos)
			{
				ReleaseClaimedPath();
			}
		}
	}

	public void OnPartialPathComplete(Path p)
	{
		OnPathComplete(p, runModifiers: true, sendCallbacks: false);
	}

	public void OnMultiPathComplete(Path p)
	{
		OnPathComplete(p, runModifiers: false, sendCallbacks: true);
	}

	public ABPath GetNewPath(Vector3 start, Vector3 end)
	{
		return ABPath.Construct(start, end);
	}

	public Path StartPath(Vector3 start, Vector3 end)
	{
		return StartPath(start, end, null, curGraphMask);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback)
	{
		return StartPath(start, end, callback, curGraphMask);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask)
	{
		Path newPath = GetNewPath(start, end);
		newPath.width = curSeekerSize;
		return StartPath(newPath, callback, graphMask);
	}

	public Path StartPath(Path p, OnPathDelegate callback = null, int graphMask = -1)
	{
		p.enabledTags = traversableTags.tagsChange;
		p.tagPenalties = tagPenalties;
		if (path != null && path.GetState() <= PathState.Processing && lastPathID == path.pathID)
		{
			path.Error();
			path.LogError("Canceled path because a new one was requested.\nThis happens when a new path is requested from the seeker when one was already being calculated.\nFor example if a unit got a new order, you might request a new path directly instead of waiting for the now invalid path to be calculated. Which is probably what you want.\nIf you are getting this a lot, you might want to consider how you are scheduling path requests.");
		}
		path = p;
		Path obj = path;
		obj.callback = (OnPathDelegate)Delegate.Combine(obj.callback, onPathDelegate);
		path.nnConstraint.graphMask = graphMask;
		tmpPathCallback = callback;
		lastPathID = path.pathID;
		RunModifiers(ModifierPass.PreProcess, path);
		AstarPath.StartPath(path);
		return path;
	}

	public MultiTargetPath StartMultiTargetPath(Vector3 start, Vector3[] endPoints, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(start, endPoints, null);
		multiTargetPath.pathsForAll = pathsForAll;
		return StartMultiTargetPath(multiTargetPath, callback, graphMask);
	}

	public MultiTargetPath StartMultiTargetPath(Vector3[] startPoints, Vector3 end, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(startPoints, end, null);
		multiTargetPath.pathsForAll = pathsForAll;
		return StartMultiTargetPath(multiTargetPath, callback, graphMask);
	}

	public MultiTargetPath StartMultiTargetPath(MultiTargetPath p, OnPathDelegate callback = null, int graphMask = -1)
	{
		if (path != null && path.GetState() <= PathState.Processing && lastPathID == path.pathID)
		{
			path.ForceLogError("Canceled path because a new one was requested");
		}
		OnPathDelegate[] array = new OnPathDelegate[p.targetPoints.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = onPartialPathDelegate;
		}
		p.callbacks = array;
		p.callback = (OnPathDelegate)Delegate.Combine(p.callback, new OnPathDelegate(OnMultiPathComplete));
		p.nnConstraint.graphMask = graphMask;
		path = p;
		tmpPathCallback = callback;
		lastPathID = path.pathID;
		RunModifiers(ModifierPass.PreProcess, path);
		AstarPath.StartPath(path);
		return p;
	}

	public IEnumerator DelayPathStart(Path p)
	{
		yield return null;
		RunModifiers(ModifierPass.PreProcess, p);
		AstarPath.StartPath(p);
	}

	public void OnDrawGizmos()
	{
		if (lastCompletedNodePath == null || !drawGizmos)
		{
			return;
		}
		if (detailedGizmos)
		{
			Gizmos.color = new Color(0.7f, 0.5f, 0.1f, 0.5f);
			if (lastCompletedNodePath != null)
			{
				for (int i = 0; i < lastCompletedNodePath.Count - 1; i++)
				{
					Gizmos.DrawLine((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i + 1].position);
				}
			}
		}
		Gizmos.color = new Color(0f, 1f, 0f, 1f);
		if (lastCompletedVectorPath != null)
		{
			for (int j = 0; j < lastCompletedVectorPath.Count - 1; j++)
			{
				Gizmos.DrawLine(lastCompletedVectorPath[j], lastCompletedVectorPath[j + 1]);
			}
		}
	}
}
