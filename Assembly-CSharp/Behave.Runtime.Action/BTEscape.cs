using Pathea;
using Pathfinding;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTEscape), "Escape")]
public class BTEscape : BTNormal
{
	private class Data
	{
		[Behave]
		public float cdTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float maxDistance;

		[Behave]
		public float interval;

		[Behave]
		public float prob;

		[Behave]
		public float clearRadius = 40f;

		[Behave]
		public string anim = string.Empty;

		public float m_StartTime;

		public float m_FollowTime;

		public float m_LastProbTime;

		public float m_LastEscapeTime;

		public float m_LastFollowTime;

		public float m_LastRandomTime;

		public Vector3 m_StartPoint;

		public Vector3 m_EscapePosition;
	}

	private Data m_Data;

	private bool IsClearEscape()
	{
		if (base.escapeEnemy == null || m_Data == null)
		{
			return false;
		}
		if (PEUtil.SqrMagnitudeH(base.position, base.escapeEnemy.position) <= m_Data.clearRadius * m_Data.clearRadius)
		{
			return true;
		}
		return false;
	}

	private void OnPathComplete(Path path)
	{
		if (path == null)
		{
			return;
		}
		path.Claim(path);
		if (path.vectorPath != null && path.vectorPath.Count > 0 && !Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			Vector3 vector = path.vectorPath[path.vectorPath.Count - 1];
			if (EvadePolarShield(vector) && !PEUtil.CheckPositionUnderWater(vector - Vector3.up * 0.6f) && Vector3.Angle(base.entity.position - base.escapeEnemy.position, vector - base.escapeEnemy.position) < 75f)
			{
				m_Data.m_EscapePosition = vector;
			}
		}
		path.Release(path);
	}

	private Vector3 GetEscapePosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minAngle, float maxAngle)
	{
		if (base.field == MovementField.Sky)
		{
			if (IsFly())
			{
				return PEUtil.GetRandomPositionInSky(center, direction, minRadius, maxRadius, 10f, 25f, minAngle, maxAngle);
			}
			return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, isResult: false);
		}
		if (base.field == MovementField.water)
		{
			return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 5f, 25f, minAngle, maxAngle, isResult: false);
		}
		return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, isResult: false);
	}

	private Vector3 GetEscapePosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
	{
		if (base.entity.Field == MovementField.Land && EvadePolarShield(base.position) && PEUtil.IsInAstarGrid(base.position))
		{
			FleePath fleePath = FleePath.Construct(base.position, direction, (int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
			fleePath.spread = 40000;
			fleePath.aimStrength = 1f;
			fleePath.aim = base.position + direction.normalized * Random.Range(minRadius, maxRadius);
			AstarPath.StartPath(fleePath);
		}
		for (int i = 0; i < 5; i++)
		{
			Vector3 escapePosition = GetEscapePosition(center, direction, minRadius, maxRadius, -60f, 60f);
			if (escapePosition != Vector3.zero && EvadePolarShield(escapePosition))
			{
				return escapePosition;
			}
		}
		for (int j = 0; j < 5; j++)
		{
			Vector3 escapePosition2 = GetEscapePosition(center, direction, minRadius, maxRadius, -135f, 135f);
			if (escapePosition2 != Vector3.zero && EvadePolarShield(escapePosition2))
			{
				return escapePosition2;
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
		if (base.escapeEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.IsInjury)
		{
			SetBool("Injured_Escape", value: true);
		}
		else
		{
			SetBool(m_Data.anim, value: true);
		}
		m_Data.m_StartTime = Time.time;
		m_Data.m_LastProbTime = Time.time;
		m_Data.m_StartPoint = base.position;
		MoveToPosition(Vector3.zero);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.escapeEnemy == null || Stucking(3f))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime > m_Data.maxTime)
		{
			return BehaveResult.Success;
		}
		if (Time.time - m_Data.m_LastProbTime > m_Data.interval)
		{
			m_Data.m_LastProbTime = Time.time;
			if (Random.value < m_Data.prob)
			{
				return BehaveResult.Success;
			}
		}
		if (base.entity.Leader == null || base.entity.IsLeader)
		{
			if (Stucking() || PEUtil.SqrMagnitude(base.position, m_Data.m_EscapePosition, base.entity.gravity < float.Epsilon) < 4f)
			{
				m_Data.m_EscapePosition = Vector3.zero;
			}
			if (base.escapeEnemy == null)
			{
				return BehaveResult.Failure;
			}
			if (m_Data.m_EscapePosition == Vector3.zero)
			{
				m_Data.m_EscapePosition = GetEscapePosition(base.position, base.position - base.escapeEnemy.position, 25f + base.radius, 35f + base.radius);
			}
		}
		else if (base.entity.Leader.Stucking(2f) || Stucking())
		{
			base.entity.Group.RemoveMember(base.entity);
			if (base.escapeEnemy == null)
			{
				return BehaveResult.Failure;
			}
			m_Data.m_EscapePosition = GetEscapePosition(base.position, base.position - base.escapeEnemy.position, 25f + base.radius, 35f + base.radius);
		}
		else if (Time.time - m_Data.m_LastFollowTime > m_Data.m_FollowTime)
		{
			m_Data.m_EscapePosition = base.entity.Group.FollowLeader(base.entity);
			m_Data.m_LastFollowTime = Time.time;
			m_Data.m_FollowTime = Random.Range(1f, 3f);
		}
		float attribute = base.entity.GetAttribute(AttribType.RunSpeed);
		SetSpeed(Mathf.Lerp(0.5f, 1f, base.HpPercent) * attribute);
		MoveToPosition(m_Data.m_EscapePosition, SpeedState.Run);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			SetBool("Injured_Escape", value: false);
			SetBool(m_Data.anim, value: false);
			m_Data.m_StartTime = 0f;
			m_Data.m_LastFollowTime = 0f;
			m_Data.m_StartPoint = Vector3.zero;
			m_Data.m_EscapePosition = Vector3.zero;
			m_Data.m_LastEscapeTime = Time.time;
			SetSpeed(0f);
			ClearEscape();
		}
	}
}
