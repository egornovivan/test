using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSneak), "Sneak")]
public class BTSneak : BTNormal
{
	private float m_LookAroundTime;

	private float m_LastLookAroundTime;

	private BehaveResult Tick(Tree sender)
	{
		if (GetBool("BehaveWaiting"))
		{
			return BehaveResult.Running;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || !Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			SetBool("Sneak", value: false);
		}
		else
		{
			SetBool("Sneak", value: true);
			if (Time.time - m_LastLookAroundTime > m_LookAroundTime)
			{
				m_LookAroundTime = Random.Range(5f, 10f);
				m_LastLookAroundTime = Time.time;
				SetBool("scout_lookaround", value: true);
			}
		}
		return BehaveResult.Failure;
	}
}
