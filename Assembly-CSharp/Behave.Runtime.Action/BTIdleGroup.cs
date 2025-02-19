using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIdleGroup), "IdleGroup")]
public class BTIdleGroup : BTNormalGroup
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		public float m_StartIdleTime;

		public float m_CurrentIdleTime;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartIdleTime = Time.time;
		m_Data.m_CurrentIdleTime = Random.Range(m_Data.minTime, m_Data.maxTime);
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
		if (Time.time - m_Data.m_StartIdleTime > m_Data.m_CurrentIdleTime)
		{
			return BehaveResult.Success;
		}
		behaveGroup.PauseMemberBehave(value: false);
		return BehaveResult.Running;
	}
}
