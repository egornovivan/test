using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTEscapeGroup), "EscapeGroup")]
public class BTEscapeGroup : BTNormalGroup
{
	private float m_StartTime;

	private float m_EscapeTime;

	private float m_LastRandomTime;

	private Vector3 m_CurEscapeposition;

	private Vector3 GetEscapePosition(BehaveGroup group, Vector3 center, Vector3 direction, float minRadius, float maxRadius)
	{
		if (group.Leader.Field == MovementField.Sky)
		{
			if (group.Leader.IsFly)
			{
				return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, 25f, 50f, -135f, 135f);
			}
			return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, -135f, 135f);
		}
		if (group.Leader.Field == MovementField.water)
		{
			return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 5f, 25f, -135f, 135f);
		}
		return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, -135f, 135f);
	}

	private BehaveResult Init(Tree sender)
	{
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (!behaveGroup.HasEscapeEnemy())
		{
			return BehaveResult.Failure;
		}
		m_StartTime = Time.time;
		m_LastRandomTime = 0f;
		behaveGroup.PauseMemberBehave(value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (behaveGroup.EscapeEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (m_CurEscapeposition == Vector3.zero || PEUtil.SqrMagnitudeH(behaveGroup.Leader.position, m_CurEscapeposition) < 1f || Time.time - m_LastRandomTime > 10f)
		{
			m_LastRandomTime = Time.time;
			PeTrans component = behaveGroup.Leader.GetComponent<PeTrans>();
			m_CurEscapeposition = GetEscapePosition(behaveGroup, component.position, component.position - behaveGroup.EscapeEnemy.position, 25f, 35f);
			if (m_CurEscapeposition == Vector3.zero)
			{
				return BehaveResult.Failure;
			}
			behaveGroup.MoveToPosition(m_CurEscapeposition, SpeedState.Run);
		}
		return BehaveResult.Running;
	}
}
