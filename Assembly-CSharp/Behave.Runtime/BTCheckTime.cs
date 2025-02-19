using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCheckTime), "CheckTime")]
public class BTCheckTime : BTNormal
{
	private class Data
	{
		[Behave]
		public float checkTime;

		public float m_LastCheckTime;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_LastCheckTime >= m_Data.checkTime)
		{
			m_Data.m_LastCheckTime = Time.time;
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
