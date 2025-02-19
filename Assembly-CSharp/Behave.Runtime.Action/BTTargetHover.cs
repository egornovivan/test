using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTargetHover), "TargetHover")]
public class BTTargetHover : BTNormal
{
	private class Data
	{
		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minHeight;

		[Behave]
		public float maxHeight;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;
	}

	private Data m_Data;

	private Vector3 m_HoverPosition;

	private float m_Time;

	private float m_StartTime;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		m_HoverPosition = PEUtil.GetRandomPosition(base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight);
		if (m_HoverPosition == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		m_StartTime = Time.time;
		m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		MoveToPosition(m_HoverPosition, SpeedState.Run);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_StartTime > m_Time)
		{
			return BehaveResult.Success;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Vector3.Distance(base.position, m_HoverPosition) < 1f)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}
}
