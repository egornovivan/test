using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTMoveDirection), "MoveDirection")]
public class BTMoveDirection : BTNormal
{
	private class Data
	{
		[Behave]
		public float time;

		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float speed;

		[Behave]
		public Vector3 anchor = Vector3.forward;

		public float m_StartTime;

		public float m_LastCooldownTime;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_LastCooldownTime <= m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartTime = Time.time;
		MoveDirection(base.transform.TransformDirection(m_Data.anchor), SpeedState.Run);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime < m_Data.time)
		{
			return BehaveResult.Running;
		}
		m_Data.m_LastCooldownTime = Time.time;
		MoveDirection(Vector3.zero);
		return BehaveResult.Success;
	}

	private void Reset(Tree sender)
	{
		SetSpeed(0f);
		MoveDirection(Vector3.zero);
	}
}
