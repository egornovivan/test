using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCsMoveAway), "CsMoveAway")]
public class BTCsMoveAway : BTNormal
{
	private class Data
	{
		[Behave]
		public float awayTime;

		[Behave]
		public float minRadiu;

		[Behave]
		public float maxRadiu;

		public Vector3 mWanderWalkPos;

		public float startWanderTime;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.Creater == null || base.Creater.Assembly == null)
		{
			return BehaveResult.Failure;
		}
		if (!NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out m_Data.mWanderWalkPos))
		{
			return BehaveResult.Failure;
		}
		m_Data.startWanderTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.Creater == null || base.Creater.Assembly == null)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (PEUtil.IsForwardBlock(base.entity, base.entity.peTrans.forward, 2f) || Time.time - m_Data.startWanderTime > m_Data.awayTime)
		{
			StopMove();
			return BehaveResult.Success;
		}
		if (IsReached(base.position, m_Data.mWanderWalkPos, Is3D: true))
		{
			StopMove();
			return BehaveResult.Success;
		}
		if (PEUtil.IsUnderBlock(base.entity))
		{
			MoveDirection(m_Data.mWanderWalkPos - base.position, SpeedState.Run);
		}
		else
		{
			MoveToPosition(m_Data.mWanderWalkPos, SpeedState.Run);
		}
		return BehaveResult.Running;
	}
}
