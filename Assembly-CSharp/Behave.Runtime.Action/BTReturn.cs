using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTReturn), "Return")]
public class BTReturn : BTNormal
{
	private class Data
	{
		[Behave]
		public float arriveRadius;

		[Behave]
		public float returnRadius;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy != null)
		{
			return BehaveResult.Failure;
		}
		if (PEUtil.SqrMagnitudeH(base.position, base.entity.spawnPos) < m_Data.returnRadius * m_Data.returnRadius)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy != null)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Failure;
		}
		if (PEUtil.SqrMagnitudeH(base.position, base.entity.spawnPos) < m_Data.arriveRadius * m_Data.arriveRadius)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Failure;
		}
		MoveToPosition(base.entity.spawnPos);
		return BehaveResult.Running;
	}
}
