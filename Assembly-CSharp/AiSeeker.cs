using System;
using System.Collections;
using UnityEngine;

public class AiSeeker : MonoBehaviour
{
	public Transform target;

	public float repathRate = 0.1f;

	public float pickNextWaypointDistance = 1f;

	public float targetReached = 0.2f;

	public bool drawGizmos;

	public bool canSearch = true;

	public bool canMove = true;

	protected Transform tr;

	protected float lastPathSearch = -9999f;

	protected int pathIndex;

	protected Vector3[] path;

	protected Vector3[] followPath;

	public bool followPathing => followPath != null;

	public Vector3 movement
	{
		get
		{
			if (path == null || pathIndex >= path.Length || pathIndex < 0 || !canMove)
			{
				return Vector3.zero;
			}
			Vector3 vector = path[pathIndex];
			vector.y = tr.position.y;
			while ((vector - tr.position).sqrMagnitude < pickNextWaypointDistance * pickNextWaypointDistance)
			{
				pathIndex++;
				if (pathIndex >= path.Length)
				{
					if ((vector - tr.position).sqrMagnitude < pickNextWaypointDistance * targetReached * (pickNextWaypointDistance * targetReached))
					{
						ReachedEndOfPath();
						return Vector3.zero;
					}
					pathIndex--;
					break;
				}
				vector = path[pathIndex];
				vector.y = tr.position.y;
			}
			return vector - tr.position;
		}
	}

	public void Start()
	{
		tr = base.transform;
	}

	public IEnumerator WaitToRepath()
	{
		float timeLeft = repathRate - (Time.time - lastPathSearch);
		yield return new WaitForSeconds(timeLeft);
		Repath();
	}

	public void Stop()
	{
		canMove = false;
		canSearch = false;
	}

	public void SetFollowPath(Vector3[] paths)
	{
		if (paths != null)
		{
			followPath = paths;
			path = paths;
		}
	}

	public void ClearFollowPath()
	{
		followPath = null;
	}

	public void ClearPath()
	{
		if (followPath == null)
		{
			path = null;
			pathIndex = 0;
		}
	}

	public void Resume()
	{
		canMove = true;
		canSearch = true;
	}

	public virtual void Repath()
	{
	}

	public void PathToTarget(Vector3 targetPoint)
	{
	}

	public virtual void ReachedEndOfPath()
	{
		ClearPath();
	}

	public void StartPath(Vector3 targetPoint)
	{
		if (targetPoint == Vector3.zero || followPath != null)
		{
			ClearPath();
		}
	}

	public void OnDrawGizmos()
	{
		if (drawGizmos && path != null && pathIndex < path.Length && pathIndex >= 0)
		{
			Vector3 vector = path[pathIndex];
			vector.y = tr.position.y;
			Debug.DrawLine(base.transform.position, vector, Color.blue);
			float num = pickNextWaypointDistance;
			if (pathIndex == path.Length - 1)
			{
				num *= targetReached;
			}
			Vector3 start = vector + num * new Vector3(1f, 0f, 0f);
			for (float num2 = 0f; (double)num2 < Math.PI * 2.0; num2 += 0.1f)
			{
				Vector3 vector2 = vector + new Vector3((float)Math.Cos(num2) * num, 0f, (float)Math.Sin(num2) * num);
				Debug.DrawLine(start, vector2, Color.yellow);
				start = vector2;
			}
			Debug.DrawLine(start, vector + num * new Vector3(1f, 0f, 0f), Color.yellow);
		}
	}
}
