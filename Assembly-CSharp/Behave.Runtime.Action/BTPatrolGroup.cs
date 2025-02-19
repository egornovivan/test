using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTPatrolGroup), "PatrolGroup")]
public class BTPatrolGroup : BTNormalGroup
{
	private class Data
	{
		[Behave]
		public int field;

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

		public float m_SwitchTime;

		public float m_LastSwitchTime;

		public float m_StartPatrolTime;

		public Vector3 m_CurrentPatrolPosition = Vector3.zero;
	}

	private Data m_Data;

	private Enemy m_Escape;

	private Enemy m_Threat;

	private Vector3 GetPatrolPosition(Tree sender)
	{
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return Vector3.zero;
		}
		PeTrans component = behaveGroup.Leader.GetComponent<PeTrans>();
		Vector3 position = component.position;
		if (m_Data.field == 2)
		{
			return PEUtil.GetRandomPositionInWater(position, component.trans.forward, m_Data.minRadius, m_Data.maxRadius, m_Data.minHeight, m_Data.maxHeight, -135f, 135f);
		}
		if (m_Data.field == 3)
		{
			if (behaveGroup.Leader.IsFly)
			{
				return PEUtil.GetRandomPositionInSky(position, component.trans.forward, m_Data.minRadius, m_Data.maxRadius, m_Data.minHeight, m_Data.maxHeight, -135f, 135f);
			}
			return PEUtil.GetRandomPositionOnGround(position, component.trans.forward, m_Data.minRadius, m_Data.maxRadius, -135f, 135f);
		}
		return PEUtil.GetRandomPositionOnGround(position, component.trans.forward, m_Data.minRadius, m_Data.maxRadius, -135f, 135f);
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartPatrolTime = Time.time;
		m_Data.m_LastSwitchTime = 0f;
		m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		m_Data.m_SwitchTime = Random.Range(5f, 10f);
		behaveGroup.PauseMemberBehave(value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (behaveGroup.HasAttackEnemy() || behaveGroup.HasEscapeEnemy())
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartPatrolTime > m_Data.m_Time)
		{
			return BehaveResult.Success;
		}
		if (Time.time - m_Data.m_LastSwitchTime > m_Data.m_SwitchTime)
		{
			m_Data.m_LastSwitchTime = Time.time;
			m_Data.m_SwitchTime = Random.Range(5f, 10f);
			m_Data.m_CurrentPatrolPosition = GetPatrolPosition(sender);
			if (m_Data.m_CurrentPatrolPosition != Vector3.zero)
			{
				behaveGroup.MoveToPosition(m_Data.m_CurrentPatrolPosition);
			}
		}
		return BehaveResult.Running;
	}
}
