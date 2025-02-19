using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathfinding;
using Pathfinding.RVO;
using PETools;
using UnityEngine;

[RequireComponent(typeof(SimpleSmoothModifier))]
[RequireComponent(typeof(Seeker))]
public class PEPathfinder : MonoBehaviour
{
	private static float MaxAngle = 30f;

	public Transform master;

	public float repathRate = 0.5f;

	public Transform target;

	public Vector3 targetPosition;

	public bool canSearch = true;

	public float pickNextWaypointDist = 2f;

	public float forwardLook = 1f;

	public float endReachedDistance = 0.2f;

	public bool closestOnPathCheck = true;

	protected float minMoveScale = 0.05f;

	protected Seeker seeker;

	protected float lastRepath = -9999f;

	protected Path path;

	protected CharacterController controller;

	protected NavmeshController navController;

	protected RVOController rvoController;

	protected Rigidbody rigid;

	protected int currentWaypointIndex;

	protected bool targetReached;

	protected bool canSearchAgain = true;

	protected bool isPathError;

	protected Vector3 lastFoundWaypointPosition;

	protected float lastFoundWaypointTime = -9999f;

	private float lastClearTime;

	private PeEntity entity;

	private Vector3 searchPosition;

	private int layer;

	private int terLayer;

	private int allLayer;

	private bool initSeekerSize;

	private bool startHasRun;

	protected Vector3 targetPoint;

	protected Vector3 targetDirection;

	public bool TargetReached => targetReached;

	protected virtual void Awake()
	{
		seeker = GetComponent<Seeker>();
	}

	protected virtual void Start()
	{
		startHasRun = true;
		OnEnable();
		layer = 2172928;
		terLayer = 4096;
		allLayer = layer | terLayer;
		entity = GetComponentInParent<PeEntity>();
		seeker.startEndModifier.exactStartPoint = StartEndModifier.Exactness.Original;
		seeker.startEndModifier.exactEndPoint = StartEndModifier.Exactness.Original;
	}

	protected virtual void OnEnable()
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

	public void OnDisable()
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

	protected IEnumerator RepeatTrySearchPath()
	{
		while (AstarPath.active != null)
		{
			float v = TrySearchPath();
			yield return new WaitForSeconds(v);
		}
	}

	private bool CanSearch()
	{
		if (target == null && searchPosition == Vector3.zero)
		{
			return false;
		}
		return true;
	}

	public float TrySearchPath()
	{
		if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch)
		{
			if (CanSearch())
			{
				SearchPath();
			}
			return repathRate;
		}
		float num = repathRate - (Time.time - lastRepath);
		return (!(num < 0f)) ? num : 0f;
	}

	public virtual void SearchPath()
	{
		if (!initSeekerSize && entity != null && entity.bounds.size != Vector3.zero)
		{
			initSeekerSize = true;
			seeker.curSeekerSize = Mathf.RoundToInt(entity.bounds.size.x);
		}
		targetPosition = searchPosition;
		if (target == null && targetPosition == Vector3.zero)
		{
			throw new InvalidOperationException("Target is null");
		}
		lastRepath = Time.time;
		Vector3 vector = ((!(target != null)) ? targetPosition : target.position);
		canSearchAgain = false;
		if (PEUtil.IsInAstarGrid(vector))
		{
			seeker.StartPath(GetFeetPosition(), vector);
		}
	}

	public virtual void OnTargetReached()
	{
		ClearPath();
	}

	public virtual void OnPathComplete(Path _p)
	{
		if (!(_p is ABPath aBPath))
		{
			throw new Exception("This function only handles ABPaths, do not use special path types");
		}
		isPathError = false;
		canSearchAgain = true;
		aBPath.Claim(this);
		if (aBPath.error)
		{
			ClearPath();
			isPathError = true;
			aBPath.Release(this);
			return;
		}
		if (CanClearPath(aBPath))
		{
			ClearPath();
			isPathError = true;
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
			Debug.DrawLine(vector, feetPosition, Color.red, 1f);
			for (int i = 0; i <= num; i++)
			{
				CalculateVelocity(vector);
				vector += vector2;
			}
		}
	}

	public virtual Vector3 GetFeetPosition()
	{
		if (master != null)
		{
			return master.position;
		}
		Debug.LogError("NO master!!");
		return base.transform.position;
	}

	private bool CanClearPath(Path argPath)
	{
		if (argPath.vectorPath.Count == 0 || Vector3.Distance(GetFeetPosition(), argPath.vectorPath[0]) > 2f)
		{
			return true;
		}
		if (entity != null && PEUtil.IsInAstarGrid(searchPosition))
		{
			if (entity.movement != Vector3.zero)
			{
				Ray ray = new Ray(entity.centerPos, entity.movement);
				Vector3 position = entity.position;
				Vector3 point = entity.position + entity.bounds.size.y * Vector3.up;
				float radius = entity.bounds.extents.x + 0.5f;
				float maxDistance = entity.bounds.extents.z + 1f;
				if (Physics.CapsuleCast(position, point, radius, entity.movement, maxDistance, layer))
				{
					return false;
				}
			}
			Vector3 vector = searchPosition - entity.position;
			Vector3 from = Vector3.ProjectOnPlane(vector, Vector3.up);
			for (int i = 1; i < argPath.vectorPath.Count; i++)
			{
				Vector3 to = Vector3.ProjectOnPlane(argPath.vectorPath[i] - entity.position, Vector3.up);
				if (Vector3.Angle(from, to) > MaxAngle)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void ClearPath()
	{
		if (path != null)
		{
			path.Release(this);
			path = null;
		}
		if (seeker != null)
		{
			seeker.lastCompletedVectorPath = null;
		}
		currentWaypointIndex = 0;
		targetReached = false;
	}

	public void SetTargetposition(Vector3 position)
	{
		searchPosition = position;
		if (searchPosition == Vector3.zero && AstarPath.active != null)
		{
			targetPosition = searchPosition;
			ClearPath();
		}
	}

	protected float XZSqrMagnitude(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	public Vector3 CalculateVelocity(Vector3 currentPosition)
	{
		if (path == null || isPathError || path.vectorPath == null || path.vectorPath.Count == 0)
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
			if (entity != null && entity.MoveState == MovementState.Water)
			{
				num = PEUtil.SqrMagnitude(vectorPath[currentWaypointIndex], currentPosition);
			}
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
		float magnitude = vector.magnitude;
		targetDirection = vector;
		targetPoint = vector2;
		if (currentWaypointIndex == vectorPath.Count - 1 && magnitude <= endReachedDistance)
		{
			if (!targetReached)
			{
				targetReached = true;
				OnTargetReached();
			}
			return Vector3.zero;
		}
		return targetDirection;
	}

	protected Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b)
	{
		if (entity == null || entity.MoveState != MovementState.Water)
		{
			a.y = p.y;
			b.y = p.y;
		}
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
