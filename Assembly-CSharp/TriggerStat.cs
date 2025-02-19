public class TriggerStat : Istat
{
	public override bool IsAccomplish()
	{
		return m_StatValue >= m_Goal;
	}

	public override void IncreaseStat()
	{
		m_StatValue++;
	}

	public override void DropStat()
	{
		if (m_StatValue > 0)
		{
			m_StatValue--;
		}
	}

	public override void ResetValue()
	{
		m_StatValue = 0;
	}

	public override void SetGoal(object obj)
	{
		m_Goal = (int)obj;
	}
}
