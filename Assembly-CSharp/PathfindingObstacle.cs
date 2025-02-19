using System;
using System.Collections;
using Pathfinding;
using PETools;
using UnityEngine;

public class PathfindingObstacle : MonoBehaviour
{
	public GameObject master;

	public float updateError = 1f;

	public float checkTime = 0.2f;

	private bool _colDirty;

	private int[] _colFlags;

	private Collider[] _colliders;

	private Bounds prevBounds;

	private bool isWaitingForUpdate;

	private void Start()
	{
		StartCoroutine(UpdateGraphs());
	}

	private bool CheckCollider()
	{
		Collider[] colliders = _colliders;
		foreach (Collider collider in colliders)
		{
			if (collider != null && !collider.isTrigger)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateColliderFlags()
	{
		int num = _colliders.Length;
		for (int i = 0; i < num; i++)
		{
			if (_colliders[i] == null)
			{
				if (_colFlags[i] != 0)
				{
					_colDirty = true;
					_colFlags[i] = 0;
				}
				continue;
			}
			int num2 = ((!_colliders[i].isTrigger) ? 1 : 2);
			if (_colFlags[i] != num2)
			{
				_colDirty = true;
				_colFlags[i] = num2;
			}
		}
	}

	private Bounds GetBounds()
	{
		Bounds result = default(Bounds);
		if (_colliders != null)
		{
			Collider[] colliders = _colliders;
			foreach (Collider collider in colliders)
			{
				if (collider != null && !collider.isTrigger)
				{
					if (result.size == Vector3.zero)
					{
						result = collider.bounds;
					}
					else
					{
						result.Encapsulate(collider.bounds);
					}
				}
			}
		}
		return result;
	}

	private IEnumerator UpdateGraphs()
	{
		if (AstarPath.active == null)
		{
			Debug.LogWarning("No collider is attached to the GameObject. Canceling check");
			yield break;
		}
		do
		{
			_colliders = ((!(master == null)) ? master.GetComponentsInChildren<Collider>() : GetComponentsInChildren<Collider>());
			yield return new WaitForSeconds(checkTime);
		}
		while (_colliders.Length == 0);
		_colFlags = new int[_colliders.Length];
		while (CheckCollider())
		{
			while (isWaitingForUpdate)
			{
				yield return new WaitForSeconds(checkTime);
			}
			UpdateColliderFlags();
			if (_colDirty)
			{
				_colDirty = false;
				Bounds newBounds = GetBounds();
				Bounds merged = newBounds;
				merged.Encapsulate(prevBounds);
				Vector3 minDiff = merged.min - newBounds.min;
				Vector3 maxDiff = merged.max - newBounds.max;
				if (Mathf.Abs(minDiff.x) > updateError || Mathf.Abs(minDiff.y) > updateError || Mathf.Abs(minDiff.z) > updateError || Mathf.Abs(maxDiff.x) > updateError || Mathf.Abs(maxDiff.y) > updateError || Mathf.Abs(maxDiff.z) > updateError)
				{
					isWaitingForUpdate = true;
					DoUpdateGraphs();
				}
			}
			yield return new WaitForSeconds(checkTime);
		}
		OnDestroy();
	}

	public void OnDestroy()
	{
		if (AstarPath.active != null)
		{
			GraphUpdateObject ob = new GraphUpdateObject(prevBounds);
			AstarPath.active.UpdateGraphs(ob);
		}
	}

	public void DoUpdateGraphs()
	{
		if (CheckCollider())
		{
			isWaitingForUpdate = false;
			Bounds bounds = GetBounds();
			Bounds bounds2 = bounds;
			bounds2.Encapsulate(prevBounds);
			if (BoundsVolume(bounds2) < BoundsVolume(bounds) + BoundsVolume(prevBounds))
			{
				AstarPath.active.UpdateGraphs(bounds2);
			}
			else
			{
				AstarPath.active.UpdateGraphs(prevBounds);
				AstarPath.active.UpdateGraphs(bounds);
			}
			prevBounds = bounds;
		}
	}

	private static float BoundsVolume(Bounds b)
	{
		return Math.Abs(b.size.x * b.size.y * b.size.z);
	}

	public void OnDrawGizmosSelected()
	{
		PEUtil.DrawBounds(PEUtil.GetWordColliderBoundsInChildren(base.gameObject), Color.yellow);
	}
}
