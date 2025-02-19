using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTRadiusStep), "RadiusStep")]
public class BTRadiusStep : BTNormal
{
	private class Data
	{
		[Behave]
		public float minRadius;
	}

	private Data m_Data;

	private void DoStep()
	{
		Vector3 vec = base.position - base.selectattackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		DoAction(PEActionType.Step, param);
	}

	private bool InRadiu(Vector3 self, Vector3 target, float radiu)
	{
		float num = PEUtil.SqrMagnitudeH(self, target);
		return num < radiu * radiu;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.selectattackEnemy.entityTarget.Field != MovementField.Sky && InRadiu(base.position, base.selectattackEnemy.position, m_Data.minRadius))
		{
			DoStep();
		}
		return BehaveResult.Success;
	}
}
