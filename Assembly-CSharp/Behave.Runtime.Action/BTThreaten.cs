using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTThreaten), "Threaten")]
public class BTThreaten : BTNormal
{
	public class Data
	{
		[Behave]
		public float probability;

		[Behave]
		public string[] anims = new string[0];

		[Behave]
		public float[] times = new float[0];

		public float m_StartTime;

		public int index;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_Data.anims.Length <= 0 || m_Data.anims.Length != m_Data.times.Length)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.probability)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartTime = Time.time;
		m_Data.index = Random.Range(0, m_Data.anims.Length);
		SetBool(m_Data.anims[m_Data.index], value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime > m_Data.times[m_Data.index])
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}
}
