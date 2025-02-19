using UnityEngine;

public class PEAH_TimeThreshold : PEAbnormalHit
{
	protected float m_ElapseTime;

	public float time { get; set; }

	public override float HitRate()
	{
		return (!(m_ElapseTime >= time)) ? 0f : 1f;
	}

	public override void Update()
	{
		m_ElapseTime += ((!base.preHit) ? (-1f) : 1f) * Time.deltaTime;
		m_ElapseTime = Mathf.Clamp(m_ElapseTime, 0f, 2f * time);
	}

	public override void Clear()
	{
		m_ElapseTime = 0f;
		base.Clear();
	}
}
