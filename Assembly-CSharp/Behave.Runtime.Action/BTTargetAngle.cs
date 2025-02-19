using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTargetAngle), "TargetAngle")]
public class BTTargetAngle : BTNormal
{
	private class Data
	{
		[Behave]
		public float minAngle;

		[Behave]
		public float maxAngle;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null || !PEUtil.IsScopeAngle(base.attackEnemy.position - base.position, base.transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
