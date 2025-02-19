using UnityEngine;

public class PEAT_Time : PEAbnormalTrigger
{
	private float m_ElapseTime;

	public float interval { get; set; }

	public override bool Hit()
	{
		if (m_ElapseTime >= interval)
		{
			m_ElapseTime = 0f;
			return true;
		}
		return base.Hit();
	}

	public override void Update()
	{
		m_ElapseTime += Time.deltaTime;
	}

	public override void Clear()
	{
		m_ElapseTime = 0f;
		base.Clear();
	}
}
