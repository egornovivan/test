using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSneakIdle), "SneakIdle")]
public class BTSneakIdle : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		public float m_Time;

		public float m_StartTime;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || !Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		StopMove();
		SetBool("Snake", value: true);
		m_Data.m_StartTime = Time.time;
		m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || !Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && GetBool("Snake"))
		{
			SetBool("Snake", value: false);
		}
	}
}
