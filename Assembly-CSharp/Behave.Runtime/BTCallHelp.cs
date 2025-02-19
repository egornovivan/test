using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCallHelp), "CallHelp")]
public class BTCallHelp : BTNormal
{
	private class Data
	{
		[Behave]
		public string anim = string.Empty;

		[Behave]
		public float hpPercent;

		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float radius;
	}

	private Data m_Data;

	private float m_LastCallTime;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.HpPercent > m_Data.hpPercent)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_LastCallTime < m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		m_LastCallTime = Time.time;
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		SetBool(m_Data.anim, value: true);
		CallHelp(m_Data.radius);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_LastCallTime < 0.25f)
		{
			return BehaveResult.Running;
		}
		if (GetBool("BehaveWaitting"))
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
