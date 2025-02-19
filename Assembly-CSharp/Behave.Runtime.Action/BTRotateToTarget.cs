using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTRotateToTarget), "RotateToTarget")]
public class BTRotateToTarget : BTNormal
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
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		Vector3 vector = base.attackEnemy.position - base.position;
		if (PEUtil.IsScopeAngle(vector, base.transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
		{
			FaceDirection(Vector3.zero);
			return BehaveResult.Success;
		}
		FaceDirection(vector);
		return BehaveResult.Running;
	}
}
