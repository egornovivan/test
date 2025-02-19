using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTBlink), "Blink")]
public class BTBlink : BTAttackBase
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float radius;
	}

	private Data m_Data;

	private Vector3 GetRandomBlinkPos()
	{
		int layerMask = 81153;
		int layerMask2 = 79873;
		for (int i = 0; i < 5; i++)
		{
			Vector3 randomPosition = PEUtil.GetRandomPosition(base.position, 0f, m_Data.radius, is3D: true);
			if (!Physics.CheckSphere(randomPosition, base.radius, layerMask) && !Physics.Raycast(base.position, randomPosition - base.position, Vector3.Distance(randomPosition, base.position), layerMask2))
			{
				return randomPosition;
			}
		}
		return Vector3.zero;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		Vector3 randomBlinkPos = GetRandomBlinkPos();
		if (randomBlinkPos != Vector3.zero)
		{
			SetPosition(randomBlinkPos);
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
