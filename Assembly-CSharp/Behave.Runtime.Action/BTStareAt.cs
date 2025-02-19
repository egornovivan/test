using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTStareAt), "StareAt")]
public class BTStareAt : BTNormal
{
	private bool InStareRadiu(Vector3 pickPos, Transform target, out float weigtht)
	{
		float num = 3f;
		float num2 = 30f;
		float num3 = 10f;
		weigtht = 0f;
		float num4 = PEUtil.Magnitude(pickPos, target.position);
		if (num4 > num)
		{
			return false;
		}
		Vector3 from = target.position + Vector3.up;
		from.y = pickPos.y;
		Vector3 from2 = target.position + Vector3.up;
		from2.x = pickPos.x;
		Vector3 forward = target.forward;
		float f = Vector3.Angle(from2, forward);
		float f2 = Vector3.Angle(from, forward);
		if (Mathf.Abs(f) < num2 && Mathf.Abs(f2) < num3)
		{
			float num5 = (1f - Mathf.Abs(f) / num2) * 0.5f;
			float num6 = (1f - Mathf.Abs(f2) / num3) * 0.5f;
			weigtht = num5 + num6;
			return true;
		}
		return false;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity != null && base.entity.NpcCmpt != null && NpcThinkDb.CanDoing(base.entity, EThinkingType.Stroll))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
