using UnityEngine;

public class PEAH_RainTime : PEAH_TimeThreshold
{
	public override void Update()
	{
		m_ElapseTime += ((!PeEnv.isRain) ? (-1f) : 1f) * Time.deltaTime;
		m_ElapseTime = Mathf.Clamp(m_ElapseTime, 0f, 2f * base.time);
	}

	public override void Clear()
	{
		m_ElapseTime = 0f;
		base.Clear();
	}
}
