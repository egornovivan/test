using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTEnergyShield), "EnergyShield")]
public class BTEnergyShield : BTNormal
{
	private class Data
	{
		[Behave]
		public float hpPercent;

		[Behave]
		public float time;

		[Behave]
		public string animName = string.Empty;
	}

	private Data m_Data;

	private float m_StartTime;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_StartTime < float.Epsilon)
		{
			if (base.HpPercent <= m_Data.hpPercent)
			{
				m_StartTime = Time.time;
				ActivateEnergyShield(value: true);
				SetBool(m_Data.animName, value: true);
			}
		}
		else if (Time.time - m_StartTime > m_Data.time)
		{
			ActivateEnergyShield(value: false);
		}
		return BehaveResult.Failure;
	}
}
