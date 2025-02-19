using Pathea;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTargetRangeRunning), "TargetRangeRunning")]
public class BTTargetRangeRunning : BTNormal
{
	private class Data
	{
		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;
	}

	private Data m_Data;

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
		float sqrDistanceLogic = base.attackEnemy.SqrDistanceLogic;
		if (sqrDistanceLogic < m_Data.minRange * m_Data.minRange)
		{
			return BehaveResult.Failure;
		}
		if (sqrDistanceLogic > m_Data.maxRange * m_Data.maxRange)
		{
			MoveToPosition(base.attackEnemy.position, SpeedState.Run);
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
