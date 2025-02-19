using Pathea;
using Pathfinding;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTPatrolNPC), "PatrolNPC")]
public class BTPatrolNPC : BTNormal
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

	private bool GetCanWalkPos(out Vector3 walkPos)
	{
		Vector3 randomPositionOnGroundForWander = PEUtil.GetRandomPositionOnGroundForWander(base.FixedPointPostion, 0f, 64f);
		if (randomPositionOnGroundForWander != Vector3.zero && AiUtil.GetNearNodePosWalkable(randomPositionOnGroundForWander, out walkPos))
		{
			return true;
		}
		walkPos = Vector3.zero;
		return false;
	}

	private void OnPathComplete(Path path)
	{
		if (path != null && path.vectorPath.Count > 0)
		{
			Vector3 currentPatrolPosition = path.vectorPath[path.vectorPath.Count - 1];
			m_Data.m_CurrentPatrolPosition = currentPatrolPosition;
		}
	}

	private Vector3 GetPatrolPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
	{
		if (PEUtil.IsInAstarGrid(base.position))
		{
			RandomPath randomPath = RandomPath.Construct(base.position, (int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
			randomPath.spread = 40000;
			randomPath.aimStrength = 1f;
			randomPath.aim = PEUtil.GetRandomPosition(base.position, direction, minRadius, maxRadius, -75f, 75f);
			AstarPath.StartPath(randomPath);
			return Vector3.zero;
		}
		Vector3 walkPos = Vector3.zero;
		if (GetCanWalkPos(out walkPos))
		{
			return walkPos;
		}
		return Vector3.zero;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.hasAttackEnemy)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (m_Data.prob == 0f)
		{
			StopMove();
			return BehaveResult.Failure;
		}
		m_Data.m_CurrentPatrolPosition = GetPatrolPosition(base.entity.position, base.transform.forward, m_Data.minRadius, m_Data.maxRadius);
		m_Data.m_StartPatrolTime = Time.time;
		m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.FieldNpcIdle_Patrol);
		if (base.hasAttackEnemy)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (PEUtil.SqrMagnitudeH(base.position, m_Data.m_CurrentPatrolPosition) < 0.25f)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Success;
		}
		if (Time.time - m_Data.m_StartPatrolTime > m_Data.m_Time)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Failure;
		}
		MoveToPosition(m_Data.m_CurrentPatrolPosition);
		return BehaveResult.Running;
	}
}
