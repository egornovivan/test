using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveArroundSuccess), "MoveArroundSuccess")]
public class BTMoveArroundSuccess : BTNormal
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

	private float m_ArroundTime;

	private float m_LastArroundTime;

	private Vector3 m_ArroundPosition;

	private Vector3 GetAroundPosition()
	{
		float num = base.radius + base.attackEnemy.radius;
		Vector2 vector = Random.insideUnitCircle.normalized * Random.Range(m_Data.minRange + num, m_Data.maxRange + num);
		return base.attackEnemy.position + new Vector3(vector.x, Random.Range(m_Data.minHeight, m_Data.maxHeight), vector.y);
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_LastArroundTime > m_ArroundTime)
		{
			m_LastArroundTime = Time.time;
			m_ArroundTime = Random.Range(m_Data.minTime, m_Data.maxTime);
			m_ArroundPosition = GetAroundPosition();
		}
		if (m_ArroundPosition != Vector3.zero)
		{
			float num = ((!(base.gravity > float.Epsilon)) ? PEUtil.SqrMagnitudeH(base.position, m_ArroundPosition) : PEUtil.SqrMagnitude(base.position, m_ArroundPosition));
			if (num > 0.25f)
			{
				MoveToPosition(m_ArroundPosition, SpeedState.Run);
			}
			else
			{
				MoveToPosition(Vector3.zero);
			}
			FaceDirection(base.attackEnemy.position - base.position);
		}
		return BehaveResult.Success;
	}
}
