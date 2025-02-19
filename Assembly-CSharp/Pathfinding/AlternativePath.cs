using System;
using UnityEngine;

namespace Pathfinding;

[Serializable]
[AddComponentMenu("Pathfinding/Modifiers/Alternative Path")]
public class AlternativePath : MonoModifier
{
	public int penalty = 1000;

	public int randomStep = 10;

	private GraphNode[] prevNodes;

	private int prevSeed;

	private int prevPenalty;

	private bool waitingForApply;

	private object lockObject = new object();

	private System.Random rnd = new System.Random();

	private System.Random seedGenerator = new System.Random();

	private bool destroyed;

	private GraphNode[] toBeApplied;

	public override ModifierData input => ModifierData.Original;

	public override ModifierData output => ModifierData.All;

	public override void Apply(Path p, ModifierData source)
	{
		if (this == null)
		{
			return;
		}
		lock (lockObject)
		{
			toBeApplied = p.path.ToArray();
			if (!waitingForApply)
			{
				waitingForApply = true;
				AstarPath.OnPathPreSearch = (OnPathDelegate)Delegate.Combine(AstarPath.OnPathPreSearch, new OnPathDelegate(ApplyNow));
			}
		}
	}

	public new void OnDestroy()
	{
		destroyed = true;
		lock (lockObject)
		{
			if (!waitingForApply)
			{
				waitingForApply = true;
				AstarPath.OnPathPreSearch = (OnPathDelegate)Delegate.Combine(AstarPath.OnPathPreSearch, new OnPathDelegate(ClearOnDestroy));
			}
		}
		((MonoModifier)this).OnDestroy();
	}

	private void ClearOnDestroy(Path p)
	{
		lock (lockObject)
		{
			AstarPath.OnPathPreSearch = (OnPathDelegate)Delegate.Remove(AstarPath.OnPathPreSearch, new OnPathDelegate(ClearOnDestroy));
			waitingForApply = false;
			InversePrevious();
		}
	}

	private void InversePrevious()
	{
		int seed = prevSeed;
		rnd = new System.Random(seed);
		if (prevNodes == null)
		{
			return;
		}
		bool flag = false;
		int num = rnd.Next(randomStep);
		for (int i = num; i < prevNodes.Length; i += rnd.Next(1, randomStep))
		{
			if (prevNodes[i].Penalty < prevPenalty)
			{
				flag = true;
			}
			prevNodes[i].Penalty = (uint)(prevNodes[i].Penalty - prevPenalty);
		}
		if (flag)
		{
			Debug.LogWarning("Penalty for some nodes has been reset while this modifier was active. Penalties might not be correctly set.");
		}
	}

	private void ApplyNow(Path somePath)
	{
		lock (lockObject)
		{
			waitingForApply = false;
			AstarPath.OnPathPreSearch = (OnPathDelegate)Delegate.Remove(AstarPath.OnPathPreSearch, new OnPathDelegate(ApplyNow));
			InversePrevious();
			if (destroyed)
			{
				return;
			}
			int seed = seedGenerator.Next();
			rnd = new System.Random(seed);
			if (toBeApplied != null)
			{
				int num = rnd.Next(randomStep);
				for (int i = num; i < toBeApplied.Length; i += rnd.Next(1, randomStep))
				{
					toBeApplied[i].Penalty = (uint)(toBeApplied[i].Penalty + penalty);
				}
			}
			prevPenalty = penalty;
			prevSeed = seed;
			prevNodes = toBeApplied;
		}
	}
}
