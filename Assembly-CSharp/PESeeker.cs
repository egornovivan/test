using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
public class PESeeker : MonoBehaviour
{
	public bool canSearch = true;

	public bool closestOnPathCheck = true;

	public float repathRate = 0.5f;

	public float pickNextWaypointDist = 2f;

	public float endReachedDistance = 0.2f;

	public float forwardLook = 1f;

	private Path path;

	private float lastRepath = -9999f;

	private float lastFoundWaypointTime = -9999f;

	private int currentWaypointIndex;

	private bool targetReached;

	private bool canSearchAgain = true;

	private bool startHasRun;

	private Vector3 lastFoundWaypointPosition;

	private Vector3 targetPosition;

	private Seeker seeker;

	public Vector3 target
	{
		set
		{
			targetPosition = value;
		}
	}

	public Vector3 CalculateVelocity()
	{
		return CalculateVelocity(base.transform.position);
	}

	private void Awake()
	{
		seeker = GetComponent<Seeker>();
	}

	private void Start()
	{
		startHasRun = true;
		OnEnable();
	}

	private void OnEnable()
	{
		lastRepath = -9999f;
		canSearchAgain = true;
		lastFoundWaypointPosition = GetFeetPosition();
		if (startHasRun)
		{
			Seeker obj = seeker;
			obj.pathCallback = (OnPathDelegate)Delegate.Combine(obj.pathCallback, new OnPathDelegate(OnPathComplete));
			StartCoroutine(RepeatTrySearchPath());
		}
	}

	private void OnDisable()
	{
		if (seeker != null && !seeker.IsDone())
		{
			seeker.GetCurrentPath().Error();
		}
		if (path != null)
		{
			path.Release(this);
		}
		path = null;
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Remove(obj.pathCallback, new OnPathDelegate(OnPathComplete));
	}

	private IEnumerator RepeatTrySearchPath()
	{
		while (true)
		{
			float v = TrySearchPath();
			yield return new WaitForSeconds(v);
		}
	}

	private float TrySearchPath()
	{
		if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch && targetPosition != Vector3.zero)
		{
			SearchPath();
			return repathRate;
		}
		float num = repathRate - (Time.time - lastRepath);
		return (!(num < 0f)) ? num : 0f;
	}

	private void SearchPath()
	{
		if (targetPosition == Vector3.zero)
		{
			throw new InvalidOperationException("Target is null");
		}
		lastRepath = Time.time;
		canSearchAgain = false;
		seeker.StartPath(GetFeetPosition(), targetPosition);
	}

	private void OnTargetReached()
	{
	}

	private void OnPathComplete(Path _p)
	{
		if (!(_p is ABPath aBPath))
		{
			throw new Exception("This function only handles ABPaths, do not use special path types");
		}
		canSearchAgain = true;
		aBPath.Claim(this);
		if (aBPath.error)
		{
			aBPath.Release(this);
			return;
		}
		if (path != null)
		{
			path.Release(this);
		}
		path = aBPath;
		currentWaypointIndex = 0;
		targetReached = false;
		if (closestOnPathCheck)
		{
			Vector3 vector = ((!(Time.time - lastFoundWaypointTime < 0.3f)) ? aBPath.originalStartPoint : lastFoundWaypointPosition);
			Vector3 feetPosition = GetFeetPosition();
			Vector3 vector2 = feetPosition - vector;
			float magnitude = vector2.magnitude;
			vector2 /= magnitude;
			int num = (int)(magnitude / pickNextWaypointDist);
			for (int i = 0; i <= num; i++)
			{
				CalculateVelocity(vector);
				vector += vector2;
			}
		}
	}

	private Vector3 GetFeetPosition()
	{
		return base.transform.position;
	}

	private float XZSqrMagnitude(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	private Vector3 CalculateVelocity(Vector3 currentPosition)
	{
		if (path == null || path.vectorPath == null || path.vectorPath.Count == 0)
		{
			return Vector3.zero;
		}
		List<Vector3> vectorPath = path.vectorPath;
		if (vectorPath.Count == 1)
		{
			vectorPath.Insert(0, currentPosition);
		}
		if (currentWaypointIndex >= vectorPath.Count)
		{
			currentWaypointIndex = vectorPath.Count - 1;
		}
		if (currentWaypointIndex <= 1)
		{
			currentWaypointIndex = 1;
		}
		while (currentWaypointIndex < vectorPath.Count - 1)
		{
			float num = XZSqrMagnitude(vectorPath[currentWaypointIndex], currentPosition);
			if (num < pickNextWaypointDist * pickNextWaypointDist)
			{
				lastFoundWaypointPosition = currentPosition;
				lastFoundWaypointTime = Time.time;
				currentWaypointIndex++;
				continue;
			}
			break;
		}
		Vector3 vector = vectorPath[currentWaypointIndex] - vectorPath[currentWaypointIndex - 1];
		Vector3 vector2 = CalculateTargetPoint(currentPosition, vectorPath[currentWaypointIndex - 1], vectorPath[currentWaypointIndex]);
		vector = vector2 - currentPosition;
		vector.y = 0f;
		float magnitude = vector.magnitude;
		if (currentWaypointIndex == vectorPath.Count - 1 && magnitude <= endReachedDistance)
		{
			if (!targetReached)
			{
				targetReached = true;
				OnTargetReached();
			}
			return Vector3.zero;
		}
		return vector.normalized;
	}

	private Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b)
	{
		a.y = p.y;
		b.y = p.y;
		float magnitude = (a - b).magnitude;
		if (magnitude == 0f)
		{
			return a;
		}
		float num = AstarMath.Clamp01(AstarMath.NearestPointFactor(a, b, p));
		Vector3 vector = (b - a) * num + a;
		float magnitude2 = (vector - p).magnitude;
		float num2 = Mathf.Clamp(forwardLook - magnitude2, 0f, forwardLook);
		float num3 = num2 / magnitude;
		num3 = Mathf.Clamp(num3 + num, 0f, 1f);
		return (b - a) * num3 + a;
	}
}
