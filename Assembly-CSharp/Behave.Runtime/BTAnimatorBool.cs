using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTAnimatorBool), "AnimatorBool")]
public class BTAnimatorBool : BTNormal
{
	private class Data
	{
		[Behave]
		public string anim = string.Empty;

		[Behave]
		public float time;

		[Behave]
		public bool value;

		public float m_StartTime;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartTime = Time.time;
		SetBool(m_Data.anim, m_Data.value);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime > m_Data.time)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}
}
