using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIdleAnimation), "IdleAnimation")]
public class BTIdleAnimation : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public string[] relaxs = new string[0];
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAttackEnemy || base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		StopMove();
		SetBool(m_Data.relaxs[Random.Range(0, m_Data.relaxs.Length)], value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
