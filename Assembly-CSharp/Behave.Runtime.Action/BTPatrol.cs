using Pathea;
using Pathfinding;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTPatrol), "Patrol")]
public class BTPatrol : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float minRadius;

		[Behave]
		public float maxRadius;

		[Behave]
		public float minHeight;

		[Behave]
		public float maxHeight;

		[Behave]
		public bool spawnCenter;

		public float m_Time;

		public float m_StartPatrolTime;

		public Vector3 m_CurrentPatrolPosition = Vector3.zero;
	}

	private Data m_Data;

	private Enemy m_Escape;

	private Enemy m_Threat;

	private Vector3 m_PatrolPosition;

	private float m_LastFollowTime;

	private float m_FollowTime;

	private bool m_Falied;

	private Vector3 GetPatrolCenter()
	{
		return (base.behave.PatrolMode != BHPatrolMode.SpawnCenter) ? base.position : base.entity.spawnPos;
	}

	private float GetMinPatrolRadius()
	{
		return (!(base.behave.MinPatrolRadius > float.Epsilon)) ? m_Data.minRadius : base.behave.MinPatrolRadius;
	}

	private float GetMaxPatrolRadius()
	{
		return (!(base.behave.MaxPatrolRadius > float.Epsilon)) ? m_Data.maxRadius : base.behave.MaxPatrolRadius;
	}

	private void OnPathComplete(Path path)
	{
		if (path != null)
		{
			path.Claim(path);
			if (path.vectorPath != null && path.vectorPath.Count > 0)
			{
				Vector3 vector = path.vectorPath[path.vectorPath.Count - 1];
				if (EvadePolarShield(vector))
				{
					if (!PEUtil.CheckPositionUnderWater(vector - Vector3.up * 0.6f))
					{
						m_PatrolPosition = vector;
					}
					else
					{
						m_Falied = true;
					}
				}
			}
			path.Release(path);
		}
		else
		{
			m_Falied = true;
		}
	}

	private Vector3 GetPatrolePosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minAngle, float maxAngle)
	{
		if (base.field == MovementField.Sky)
		{
			if (IsFly())
			{
				return PEUtil.GetRandomPositionInSky(center, direction, minRadius, maxRadius, m_Data.minHeight, m_Data.maxHeight, minAngle, maxAngle);
			}
			return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, isResult: false);
		}
		if (base.field == MovementField.water)
		{
			if (base.entity.monster != null && base.entity.monster.WaterSurface)
			{
				return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 0f, base.entity.maxHeight * 0.5f, minAngle, maxAngle, isResult: false);
			}
			return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, m_Data.minHeight, m_Data.maxHeight, minAngle, maxAngle, isResult: false);
		}
		return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, isResult: false);
	}

	private Vector3 GetPatrolPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
	{
		if (!m_Falied && base.entity.Field == MovementField.Land && EvadePolarShield(base.position) && PEUtil.IsInAstarGrid(base.position) && base.behave.PatrolMode == BHPatrolMode.CurrentCenter)
		{
			RandomPath randomPath = RandomPath.Construct(base.position, (int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
			randomPath.spread = 40000;
			randomPath.aimStrength = 1f;
			randomPath.aim = PEUtil.GetRandomPosition(base.position, direction, minRadius, maxRadius, -75f, 75f);
			AstarPath.StartPath(randomPath);
			return Vector3.zero;
		}
		for (int i = 0; i < 5; i++)
		{
			Vector3 patrolePosition = GetPatrolePosition(center, direction, minRadius, maxRadius, -75f, 75f);
			if (patrolePosition != Vector3.zero && EvadePolarShield(patrolePosition))
			{
				return patrolePosition;
			}
		}
		for (int j = 0; j < 5; j++)
		{
			Vector3 patrolePosition2 = GetPatrolePosition(center, direction, minRadius, maxRadius, 135f, -135f);
			if (patrolePosition2 != Vector3.zero && EvadePolarShield(patrolePosition2))
			{
				return patrolePosition2;
			}
		}
		for (int k = 0; k < 5; k++)
		{
			Vector3 zero = Vector3.zero;
			zero = ((base.field != MovementField.Land && (base.field != MovementField.Sky || IsFly())) ? PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75f, 75f, -5f, 5f) : PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75f, 75f));
			if (zero != Vector3.zero && (EvadePolarShield(zero) || !EvadePolarShield(base.position)))
			{
				return zero;
			}
		}
		return GetEvadePolarShieldPosition(base.position);
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.behave.PatrolMode == BHPatrolMode.None)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.entity.Food != null || base.entity.IsDarkInDaytime || base.entity.Chat != null)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Treat != null && !base.entity.Treat.IsDeath() && base.entity.Treat.hasView)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartPatrolTime = Time.time;
		m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		m_Falied = false;
		m_PatrolPosition = Vector3.zero;
		MoveToPosition(Vector3.zero);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.behave.PatrolMode == BHPatrolMode.None)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.entity.Food != null || base.entity.IsDarkInDaytime || base.entity.Chat != null)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Treat != null && !base.entity.Treat.IsDeath() && base.entity.Treat.hasView)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartPatrolTime > m_Data.m_Time || Stucking(5f))
		{
			return BehaveResult.Failure;
		}
		SpeedState state = SpeedState.Walk;
		if (base.entity.Leader == null || base.entity.IsLeader || base.entity.Leader.Stucking(2f) || Stucking())
		{
			if (Stucking() || PEUtil.SqrMagnitude(base.position, m_PatrolPosition, base.entity.gravity < float.Epsilon) < 4f)
			{
				m_PatrolPosition = Vector3.zero;
			}
			if (m_PatrolPosition == Vector3.zero)
			{
				m_PatrolPosition = GetPatrolPosition(GetPatrolCenter(), base.transform.forward, GetMinPatrolRadius(), GetMaxPatrolRadius());
			}
		}
		else
		{
			if (Time.time - m_LastFollowTime > m_FollowTime)
			{
				m_PatrolPosition = Vector3.zero;
				m_LastFollowTime = Time.time;
				m_FollowTime = Random.Range(3f, 6f);
			}
			if (PEUtil.SqrMagnitude(base.position, m_PatrolPosition, base.entity.gravity < float.Epsilon) < 4f)
			{
				m_PatrolPosition = Vector3.zero;
			}
			if (m_PatrolPosition == Vector3.zero)
			{
				m_PatrolPosition = base.entity.Group.FollowLeader(base.entity);
			}
			float num = Mathf.Max(0f, PEUtil.Magnitude(base.position, base.entity.Leader.position, is3D: false) - base.radius - base.entity.Leader.maxRadius);
			if (num > m_Data.maxRadius)
			{
				state = SpeedState.Run;
			}
		}
		if (m_PatrolPosition != Vector3.zero)
		{
			m_Falied = false;
		}
		MoveToPosition(m_PatrolPosition, state);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_PatrolPosition != Vector3.zero)
		{
			MoveToPosition(Vector3.zero);
			m_PatrolPosition = Vector3.zero;
		}
	}
}
